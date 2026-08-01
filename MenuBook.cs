// 对应 C++ 源文件：MenuBook.h + MenuBook.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// MenuBook
    ///
    /// 动态加载并显示 books/ 目录下配置的书籍界面（图片、文本、按钮与开关事件）。
    /// 持有的 <see cref="WidgetButton"/>、<see cref="Sprite"/>、<see cref="Event"/> 等资源
    /// 通过 <see cref="IDisposable"/> 显式释放，释放顺序与原始析构函数 <c>~MenuBook()</c> 一致
    /// （先 <see cref="_closeButton"/>，再 <see cref="CloseWindow"/>，最后基类 <see cref="Menu"/>）。
    /// </summary>
    public class MenuBook : Menu, IDisposable
    {
        private class BookImage
        {
            public Sprite? Image;
            public int Icon;
            public Int2 Dest;
            public List<StatusID> RequiresStatus = new List<StatusID>();
            public List<StatusID> RequiresNotStatus = new List<StatusID>();

            public BookImage()
            {
                Image = null;
                Icon = -1;
            }
        }

        private class BookText
        {
            public Sprite? Sprite;
            public string Text = "";
            public string TextRaw = "";
            public string Font = "";
            public Color Color;
            public Rectangle Size;
            public int Justify;
            public bool Shadow;
            public List<StatusID> RequiresStatus = new List<StatusID>();
            public List<StatusID> RequiresNotStatus = new List<StatusID>();

            public BookText()
            {
                Sprite = null;
                Justify = 0;
                Shadow = false;
            }
        }

        private class BookButton
        {
            public WidgetButton? Button;
            public Int2 Dest;
            public string Image = "";
            public string Label = "";
            public Event Event = new Event();
            public bool EnableNavLeft;
            public bool EnableNavRight;

            public BookButton()
            {
                Button = null;
                EnableNavLeft = false;
                EnableNavRight = false;
            }
        }

        private string _bookName = "";
        private string _lastBookName = "";
        private bool _bookLoaded;

        private WidgetButton? _closeButton;
        private readonly List<BookImage> _images = new List<BookImage>();
        private readonly List<BookText> _text = new List<BookText>();
        private readonly List<BookButton> _buttons = new List<BookButton>();
        private Event? _eventOpen;
        private Event? _eventClose;

        public MenuBook()
        {
            _bookName = "";
            _lastBookName = "";
            _bookLoaded = false;
            _closeButton = new WidgetButton(WidgetButton.CloseFile);
            _eventOpen = null;
            _eventClose = null;
            Tablist = new TabList();
        }

        /// <summary>
        /// 对应 C++ 的 <c>~MenuBook()</c>：先释放 closeButton，再调用 CloseWindow 清理书籍内容，
        /// 最后调用基类 <see cref="Menu.Dispose"/>。
        /// </summary>
        public override void Dispose()
        {
            _closeButton?.Dispose();
            _closeButton = null;
            CloseWindow();
            base.Dispose();
            GC.SuppressFinalize(this);
        }

        public void SetBookFilename(string filename)
        {
            if (Visible && (filename == "" || filename == "close"))
            {
                CloseWindow();
                SharedResources.Snd!.Play(SfxClose, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);
            }
            else
            {
                _bookName = filename;
            }
        }

        private void LoadBook()
        {
            var msg = SharedResources.Msg!;
            var snd = SharedResources.Snd!;
            var eventm = SharedGameResources.Eventm!;

            if (_lastBookName != _bookName)
            {
                _lastBookName = "";
                _bookLoaded = false;
                ClearBook();
            }

            if (_bookLoaded)
                return;

            Tablist.Add(_closeButton!);

            // Read data from config file
            using FileParser infile = new FileParser();

            // @CLASS MenuBook|Description of books in books/
            if (infile.Open(_bookName, FileParser.ModFile, FileParser.ErrorNormal))
            {
                _lastBookName = _bookName;

                while (infile.Next())
                {
                    if (infile.Section == "" && ParseMenuKey(infile.Key, infile.Val))
                        continue;

                    infile.Val = infile.Val + ",";

                    // @ATTR close|point|Position of the close button.
                    if (infile.Key == "close")
                    {
                        int x = Parse.PopFirstInt(ref infile.Val);
                        int y = Parse.PopFirstInt(ref infile.Val);
                        _closeButton!.SetBasePos(x, y, Utils.AlignTopLeft);
                    }
                    else if (infile.Section == "")
                    {
                        infile.Error("MenuBook: '%s' is not a valid key.", infile.Key);
                    }

                    if (infile.NewSection)
                    {
                        // for sections that are stored in collections, add a new object here
                        if (infile.Section == "text")
                        {
                            _text.Add(new BookText());
                        }
                        else if (infile.Section == "image")
                        {
                            _images.Add(new BookImage());
                        }
                        else if (infile.Section == "button")
                        {
                            _buttons.Add(new BookButton());
                        }
                        else if (infile.Section == "event_open" && _eventOpen == null)
                        {
                            _eventOpen = new Event();
                        }
                        else if (infile.Section == "event_close" && _eventClose == null)
                        {
                            _eventClose = new Event();
                        }
                    }
                    if (infile.Section == "text" && _text.Count != 0)
                        LoadText(infile, _text[^1]);
                    else if (infile.Section == "image" && _images.Count != 0)
                        LoadImage(infile, _images[^1]);
                    else if (infile.Section == "button" && _buttons.Count != 0)
                        LoadButton(infile, _buttons[^1]);
                    else if (infile.Section == "event_open")
                        LoadBookEvent(infile, _eventOpen!);
                    else if (infile.Section == "event_close")
                        LoadBookEvent(infile, _eventClose!);
                }

                infile.Close();
            }
            else
            {
                CloseWindow();
                return;
            }

            RefreshText();

            for (int i = _images.Count; i > 0;)
            {
                i--;

                if (_images[i].Image != null)
                    _images[i].Image!.SetDestFromPoint(_images[i].Dest);
                else if (_images[i].Icon == -1)
                    _images.RemoveAt(i);
            }

            BookButton? buttonLeft = null;
            BookButton? buttonRight = null;
            bool buttonLeftEnable = true;
            bool buttonRightEnable = true;

            for (int i = 0; i < _buttons.Count; ++i)
            {
                if (_buttons[i].Image == "")
                    _buttons[i].Button = new WidgetButton(WidgetButton.DefaultFile);
                else
                    _buttons[i].Button = new WidgetButton(_buttons[i].Image);

                // we want left/right inputs to map to left/right buttons
                // this should only happen when buttons have an event that loads another book (i.e. multi-page book)
                // if there's multiple of the same button, we disable this behavior, since we don't know which one to use
                if (_buttons[i].Image == WidgetButton.DirLeftFile && buttonLeftEnable)
                {
                    if (buttonLeft != null)
                    {
                        buttonLeft = null;
                        buttonLeftEnable = false;
                    }
                    else if (_buttons[i].Event.GetComponent(EventComponent.Book) != null)
                    {
                        buttonLeft = _buttons[i];
                    }
                }
                if (_buttons[i].Image == WidgetButton.DirRightFile && buttonRightEnable)
                {
                    if (buttonRight != null)
                    {
                        buttonRight = null;
                        buttonRightEnable = false;
                    }
                    else if (_buttons[i].Event.GetComponent(EventComponent.Book) != null)
                    {
                        buttonRight = _buttons[i];
                    }
                }

                _buttons[i].Button!.SetBasePos(_buttons[i].Dest.X, _buttons[i].Dest.Y, Utils.AlignTopLeft);
                _buttons[i].Button!.SetLabel(msg.Get(_buttons[i].Label));
                _buttons[i].Button!.Refresh();

                Tablist.Add(_buttons[i].Button!);
            }

            if (buttonLeft != null)
            {
                buttonLeft.EnableNavLeft = true;
            }
            if (buttonRight != null)
            {
                buttonRight.EnableNavRight = true;
            }

            Align();

            snd.Play(SfxOpen, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);

            if (_eventOpen != null && eventm.IsActive(_eventOpen))
            {
                eventm.ExecuteEvent(_eventOpen);
            }

            _bookLoaded = true;
        }

        private void LoadImage(FileParser infile, BookImage bimage)
        {
            var camp = SharedGameResources.Camp!;

            // @ATTR image.image_pos|point|Position of the image.
            if (infile.Key == "image_pos")
            {
                bimage.Dest = Parse.ToPoint(infile.Val);
            }
            // @ATTR image.image|filename|Filename of the image.
            else if (infile.Key == "image")
            {
                if (bimage.Image != null)
                {
                    bimage.Image.Dispose();
                    bimage.Image = null;
                }

                Image? graphics;
                graphics = SharedResources.RenderDevice!.LoadImage(Parse.PopFirstString(ref infile.Val), RenderDevice.ErrorNormal);
                if (graphics != null)
                {
                    bimage.Image = graphics.CreateSprite();
                    graphics.Unref();
                }
            }
            // @ATTR image.image_icon|icon_id|Use an icon as the image instead of a file.
            else if (infile.Key == "image_icon")
            {
                if (bimage.Image != null)
                {
                    bimage.Image.Dispose();
                    bimage.Image = null;
                }

                bimage.Icon = Parse.ToInt(infile.Val);
            }
            // @ATTR image.requires_status|list(string)|Image requires these campaign statuses in order to be visible.
            else if (infile.Key == "requires_status")
            {
                string temp = Parse.PopFirstString(ref infile.Val);
                while (temp != "")
                {
                    bimage.RequiresStatus.Add(camp.RegisterStatus(temp));
                    temp = Parse.PopFirstString(ref infile.Val);
                }
            }
            // @ATTR image.requires_not_status|list(string)|Image must not have any of these campaign statuses in order to be visible.
            else if (infile.Key == "requires_not_status")
            {
                string temp = Parse.PopFirstString(ref infile.Val);
                while (temp != "")
                {
                    bimage.RequiresNotStatus.Add(camp.RegisterStatus(temp));
                    temp = Parse.PopFirstString(ref infile.Val);
                }
            }
            else
            {
                infile.Error("MenuBook: '%s' is not a valid key.", infile.Key);
            }
        }

        private void LoadText(FileParser infile, BookText btext)
        {
            var camp = SharedGameResources.Camp!;

            // @ATTR text.text_pos|int, int, int, ["left", "center", "right"] : X, Y, Width, Text justify|Position of the text.
            if (infile.Key == "text_pos")
            {
                btext.Size.X = Parse.PopFirstInt(ref infile.Val);
                btext.Size.Y = Parse.PopFirstInt(ref infile.Val);
                btext.Size.Width = Parse.PopFirstInt(ref infile.Val);
                string justify = Parse.PopFirstString(ref infile.Val);

                if (justify == "left")
                    btext.Justify = FontEngine.JustifyLeft;
                else if (justify == "center")
                    btext.Justify = FontEngine.JustifyCenter;
                else if (justify == "right")
                    btext.Justify = FontEngine.JustifyRight;
            }
            // @ATTR text.text_font|color, string : Font color, Font style|Font color and style.
            else if (infile.Key == "text_font")
            {
                btext.Color.R = (byte)Parse.PopFirstInt(ref infile.Val);
                btext.Color.G = (byte)Parse.PopFirstInt(ref infile.Val);
                btext.Color.B = (byte)Parse.PopFirstInt(ref infile.Val);
                btext.Font = Parse.PopFirstString(ref infile.Val);
            }
            // @ATTR text.text_shadow|bool|If true, the text will have a black shadow like the text labels in various menus.
            else if (infile.Key == "text_shadow")
            {
                btext.Shadow = Parse.ToBool(Parse.PopFirstString(ref infile.Val));
            }
            // @ATTR text.text|string|The text to be displayed.
            else if (infile.Key == "text")
            {
                // we use substr here to remove the trailing comma that was added in loadBook()
                btext.TextRaw = infile.Val.Substring(0, infile.Val.Length - 1);
            }
            // @ATTR text.requires_status|list(string)|Text requires these campaign statuses in order to be visible.
            else if (infile.Key == "requires_status")
            {
                string temp = Parse.PopFirstString(ref infile.Val);
                while (temp != "")
                {
                    btext.RequiresStatus.Add(camp.RegisterStatus(temp));
                    temp = Parse.PopFirstString(ref infile.Val);
                }
            }
            // @ATTR text.requires_not_status|list(string)|Text must not have any of these campaign statuses in order to be visible.
            else if (infile.Key == "requires_not_status")
            {
                string temp = Parse.PopFirstString(ref infile.Val);
                while (temp != "")
                {
                    btext.RequiresNotStatus.Add(camp.RegisterStatus(temp));
                    temp = Parse.PopFirstString(ref infile.Val);
                }
            }
            else
            {
                infile.Error("MenuBook: '%s' is not a valid key.", infile.Key);
            }
        }

        private void LoadButton(FileParser infile, BookButton bbutton)
        {
            // @ATTR button.button_pos|point|Position of the button.
            if (infile.Key == "button_pos")
            {
                bbutton.Dest.X = Parse.PopFirstInt(ref infile.Val);
                bbutton.Dest.Y = Parse.PopFirstInt(ref infile.Val);
            }
            // @ATTR button.button_image|filename|Image file to use for this button. Default is the normal menu button.
            else if (infile.Key == "button_image")
            {
                bbutton.Image = Parse.PopFirstString(ref infile.Val);
            }
            // @ATTR button.text|string|Optional text label for the button.
            else if (infile.Key == "text")
            {
                bbutton.Label = Parse.PopFirstString(ref infile.Val);
            }
            else
            {
                // @ATTR button.${EVENT_COMPONENT}|Event components to execute when the button is clicked. See the definitions in EventManager for possible attributes.
                LoadBookEvent(infile, bbutton.Event);
            }
        }

        private void LoadBookEvent(FileParser infile, Event ev)
        {
            // we use substr here to remove the trailing comma that was added in loadBook()
            string trimmed = infile.Val.Substring(0, infile.Val.Length - 1);

            // @ATTR event_open.${EVENT_COMPONENT}|Event components to execute when the book is opened. See the definitions in EventManager for possible attributes.
            // @ATTR event_close.${EVENT_COMPONENT}|Event components to execute when the book is closed. See the definitions in EventManager for possible attributes.
            if (!SharedGameResources.Eventm!.LoadEventComponentString(infile.Key, ref trimmed, ev, null))
            {
                infile.Error("MenuBook: '%s' is not a valid key.", infile.Key);
            }
        }

        public override void Align()
        {
            base.Align();

            _closeButton!.SetPos(WindowArea.X, WindowArea.Y);

            for (int i = 0; i < _text.Count; i++)
            {
                if (_text[i].Sprite != null)
                    _text[i].Sprite!.SetDest(_text[i].Size.X + WindowArea.X, _text[i].Size.Y + WindowArea.Y);
            }
            for (int i = 0; i < _images.Count; ++i)
            {
                if (_images[i].Image != null)
                    _images[i].Image!.SetDest(_images[i].Dest.X + WindowArea.X, _images[i].Dest.Y + WindowArea.Y);
            }
            for (int i = 0; i < _buttons.Count; ++i)
            {
                _buttons[i].Button!.SetPos(WindowArea.X, WindowArea.Y);
            }
        }

        private void ClearBook()
        {
            for (int i = 0; i < _text.Count; ++i)
            {
                _text[i].Sprite?.Dispose();
            }
            _text.Clear();

            for (int i = 0; i < _images.Count; ++i)
            {
                _images[i].Image?.Dispose();
            }
            _images.Clear();

            for (int i = 0; i < _buttons.Count; ++i)
            {
                _buttons[i].Button?.Dispose();
            }
            _buttons.Clear();

            Tablist.Clear();
            Tablist.Defocus();

            _eventOpen = null;
            _eventClose = null;

            if (_background != null)
            {
                _background.Dispose();
                _background = null;
            }
        }

        public void CloseWindow()
        {
            var eventm = SharedGameResources.Eventm!;

            if (_eventClose != null && eventm.IsActive(_eventClose))
            {
                eventm.ExecuteEvent(_eventClose);
            }

            ClearBook();

            Visible = false;
            _bookName = "";
            _lastBookName = "";
            _bookLoaded = false;
        }

        private void RefreshText()
        {
            var msg = SharedResources.Msg!;
            var pc = SharedGameResources.Pc!;
            var font = SharedResources.Font!;
            var renderDevice = SharedResources.RenderDevice!;

            for (int i = _text.Count; i > 0;)
            {
                i--;

                string textNew = Utils.SubstituteVarsInString(msg.Get(_text[i].TextRaw), pc);
                if (_text[i].Text == textNew)
                    continue;

                _text[i].Text = textNew;

                // render text to surface
                font.SetFont(_text[i].Font);
                Int2 pSize = font.CalcSizeWrapped(_text[i].Text, _text[i].Size.Width);
                Image? graphics = null;
                if (_text[i].Shadow)
                {
                    graphics = renderDevice.CreateImage(_text[i].Size.Width + 1, pSize.Y + 1);
                }
                else
                {
                    graphics = renderDevice.CreateImage(_text[i].Size.Width, pSize.Y);
                }

                if (graphics != null)
                {
                    int xOffset = 0;
                    if (_text[i].Justify == FontEngine.JustifyCenter)
                        xOffset = _text[i].Size.Width / 2;
                    else if (_text[i].Justify == FontEngine.JustifyRight)
                        xOffset = _text[i].Size.Width;

                    if (_text[i].Shadow)
                    {
                        font.Render(_text[i].Text, xOffset, 0, _text[i].Justify, graphics, _text[i].Size.Width, font.GetColor(FontEngine.ColorBlack), FontEngine.ShadowOffset);
                    }
                    font.Render(_text[i].Text, xOffset, 0, _text[i].Justify, graphics, _text[i].Size.Width, _text[i].Color, !FontEngine.ShadowOffset);
                    _text[i].Sprite = graphics.CreateSprite();
                    graphics.Unref();
                }

                if (graphics == null || _text[i].Sprite == null)
                    _text.RemoveAt(i);
            }
        }

        public void Logic()
        {
            var snd = SharedResources.Snd!;
            var inpt = SharedResources.Inpt!;
            var eventm = SharedGameResources.Eventm!;

            if (_bookName == "")
                return;
            else
            {
                LoadBook();
                Visible = _bookLoaded;
            }

            if (!Visible)
                return;

            Tablist.Logic();

            if (_closeButton!.CheckClick())
            {
                CloseWindow();
                snd.Play(SfxClose, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);
            }
            else if (inpt.Pressing[Input.Accept] && !inpt.Lock[Input.Accept] && Tablist.GetCurrent() == -1)
            {
                inpt.Lock[Input.Accept] = true;
                CloseWindow();
                snd.Play(SfxClose, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);
            }
            else if (inpt.Pressing[Input.Cancel] && !inpt.Lock[Input.Cancel])
            {
                inpt.Lock[Input.Cancel] = true;
                CloseWindow();
                snd.Play(SfxClose, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);
            }

            RefreshText();

            for (int i = 0; i < _buttons.Count; ++i)
            {
                if (_buttons[i].Event.Components.Count == 0 || !eventm.IsActive(_buttons[i].Event))
                {
                    _buttons[i].Button!.Enabled = false;
                    _buttons[i].Button!.Refresh();

                    // defocus the disabled button. MenuManager will auto-select the next enabled one
                    if (!inpt.UsingMouse() && _buttons[i].Button == Tablist.GetWidgetByIndex(Tablist.GetCurrent()))
                    {
                        Tablist.Defocus();
                    }
                }
                else
                {
                    _buttons[i].Button!.Enabled = true;
                    _buttons[i].Button!.Refresh();
                }

                if (_buttons[i].Button!.CheckClick())
                {
                    if (eventm.ExecuteEvent(_buttons[i].Event))
                    {
                        _buttons[i].Event = new Event();
                    }
                }
                else if (inpt.UsingMouse() && _buttons[i].Button!.Enabled && _buttons[i].EnableNavLeft && inpt.Pressing[Input.Left] && !inpt.Lock[Input.Left])
                {
                    inpt.Lock[Input.Left] = true;
                    if (eventm.ExecuteEvent(_buttons[i].Event))
                    {
                        _buttons[i].Event = new Event();
                    }
                }
                else if (inpt.UsingMouse() && _buttons[i].Button!.Enabled && _buttons[i].EnableNavRight && inpt.Pressing[Input.Right] && !inpt.Lock[Input.Right])
                {
                    inpt.Lock[Input.Right] = true;
                    if (eventm.ExecuteEvent(_buttons[i].Event))
                    {
                        _buttons[i].Event = new Event();
                    }
                }
            }
        }

        public override void Render()
        {
            var camp = SharedGameResources.Camp!;
            var renderDevice = SharedResources.RenderDevice!;
            var icons = SharedResources.Icons!;

            if (!Visible)
                return;

            base.Render();

            _closeButton!.Render();
            for (int i = 0; i < _text.Count; i++)
            {
                bool skip = false;

                for (int j = 0; j < _text[i].RequiresStatus.Count; ++j)
                {
                    if (!camp.CheckStatus(_text[i].RequiresStatus[j]))
                        skip = true;
                }

                for (int j = 0; j < _text[i].RequiresNotStatus.Count; ++j)
                {
                    if (camp.CheckStatus(_text[i].RequiresNotStatus[j]))
                        skip = true;
                }

                if (skip)
                    continue;

                renderDevice.Render(_text[i].Sprite!);
            }
            for (int i = 0; i < _images.Count; ++i)
            {
                bool skip = false;

                for (int j = 0; j < _images[i].RequiresStatus.Count; ++j)
                {
                    if (!camp.CheckStatus(_images[i].RequiresStatus[j]))
                        skip = true;
                }

                for (int j = 0; j < _images[i].RequiresNotStatus.Count; ++j)
                {
                    if (camp.CheckStatus(_images[i].RequiresNotStatus[j]))
                        skip = true;
                }

                if (skip)
                    continue;

                if (_images[i].Image != null)
                {
                    renderDevice.Render(_images[i].Image!);
                }
                else if (_images[i].Icon != -1)
                {
                    icons.SetIcon(_images[i].Icon, new Int2(_images[i].Dest.X + WindowArea.X, _images[i].Dest.Y + WindowArea.Y));
                    icons.Render();
                }
            }
            for (int i = 0; i < _buttons.Count; ++i)
            {
                _buttons[i].Button!.Render();
            }
        }
    }
}
