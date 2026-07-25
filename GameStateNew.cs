// 对应 C++ 源：GameStateNew.h + GameStateNew.cpp
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// HeroOption：预设英雄外观选项（对应 C++ <c>class HeroOption</c>）。
    /// </summary>
    public class HeroOption
    {
        public string Base = "";
        public string Head = "";
        public string Portrait = "";
        public string Name = "";
    }

    /// <summary>
    /// GameStateNew
    ///
    /// 处理开始新游戏时的玩家选择（例如角色外观、名称、职业等）。
    /// 持有的 Sprite、Widget 控件通过 <see cref="Dispose"/> 显式释放，
    /// 释放顺序与原始析构函数 <c>~GameStateNew()</c> 一致。
    /// </summary>
    public class GameStateNew : GameState
    {
        private const int OptionCurrent = 0;
        private const int OptionPrev = 1;
        private const int OptionNext = 2;
        private const int OptionRandom = 3;

        private readonly List<HeroOption> _heroOptions = new List<HeroOption>();
        private int _currentOption;

        private Sprite? _portraitImage;
        private Sprite? _portraitBorder;
        private WidgetButton? _buttonExit;
        private WidgetButton? _buttonCreate;
        private WidgetButton? _buttonNext;
        private WidgetButton? _buttonPrev;
        private WidgetButton? _buttonRandomize;
        private WidgetLabel? _labelPortrait;
        private WidgetLabel? _labelName;
        private WidgetInput? _inputName;
        private WidgetCheckBox? _buttonPermadeath;
        private WidgetLabel? _labelPermadeath;
        private WidgetLabel? _labelClasslist;
        private WidgetListBox? _classList;
        private WidgetTooltip? _classTip;

        private readonly TabList _tablist = new TabList();

        private Rectangle _portraitPos;
        private Int2 _classTipPos;
        private int _classTipAlign;
        private bool _showClasslist;
        private bool _showClassTip;
        private bool _showRandomize;
        private bool _showPermadeath;
        private bool _modifiedName;
        private bool _deleteItems;
        private bool _randomOption;
        private bool _randomClass;

        private readonly TooltipData _classTipData = new TooltipData();

        private readonly List<int> _allOptions = new List<int>();

        /// <summary>对应 C++ 公有字段 <c>int game_slot;</c>。</summary>
        public int GameSlot;

        public GameStateNew()
        {
            _currentOption = 0;
            _portraitImage = null;
            _portraitBorder = null;
            _classTipAlign = Utils.AlignFrameTopLeft;
            _showClasslist = true;
            _showClassTip = false;
            _showRandomize = true;
            _showPermadeath = true;
            _modifiedName = false;
            _deleteItems = true;
            _randomOption = false;
            _randomClass = false;
            GameSlot = 0;

            // set up buttons
            _buttonExit = new WidgetButton(WidgetButton.DefaultFile);
            _buttonExit.SetLabel(SharedResources.Msg!.Get("Cancel"));

            _buttonCreate = new WidgetButton(WidgetButton.DefaultFile);
            _buttonCreate.SetLabel(SharedResources.Msg.Get("Create"));
            _buttonCreate.Enabled = false;
            _buttonCreate.Refresh();

            _buttonPrev = new WidgetButton(WidgetButton.DirLeftFile);
            _buttonNext = new WidgetButton(WidgetButton.DirRightFile);

            _buttonRandomize = new WidgetButton(WidgetButton.DefaultFile);
            _buttonRandomize.SetLabel(SharedResources.Msg.Get("Randomize"));

            _inputName = new WidgetInput(WidgetInput.DefaultFile);
            _inputName.MaxLength = 20;

            _buttonPermadeath = new WidgetCheckBox(WidgetCheckBox.DefaultFile);
            if (SharedResources.Eset!.DeathPenalty.Permadeath)
            {
                _buttonPermadeath.Enabled = false;
                _buttonPermadeath.SetChecked(true);
            }

            _classList = new WidgetListBox(12, WidgetListBox.DefaultFile);
            _classList.CanDeselect = false;

            _classTip = new WidgetTooltip();

            // set up labels
            _labelPortrait = new WidgetLabel();
            _labelPortrait.SetText(SharedResources.Msg.Get("Choose a Portrait"));
            _labelPortrait.SetColor(SharedResources.Font!.GetColor(FontEngine.ColorMenuNormal));

            _labelName = new WidgetLabel();
            _labelName.SetText(SharedResources.Msg.Get("Choose a Name"));
            _labelName.SetColor(SharedResources.Font.GetColor(FontEngine.ColorMenuNormal));

            _labelPermadeath = new WidgetLabel();
            _labelPermadeath.SetText(SharedResources.Msg.Get("Permadeath?"));
            _labelPermadeath.SetColor(SharedResources.Font.GetColor(FontEngine.ColorMenuNormal));

            _labelClasslist = new WidgetLabel();
            _labelClasslist.SetText(SharedResources.Msg.Get("Choose a Class"));
            _labelClasslist.SetColor(SharedResources.Font.GetColor(FontEngine.ColorMenuNormal));

            // Some widgets default to being aligned to the menu frame
            _buttonPrev.Alignment = Utils.AlignFrameTopLeft;
            _buttonNext.Alignment = Utils.AlignFrameTopLeft;
            _buttonPermadeath.Alignment = Utils.AlignFrameTopLeft;
            _buttonRandomize.Alignment = Utils.AlignFrameTopLeft;
            _inputName.Alignment = Utils.AlignFrameTopLeft;
            _classList.Alignment = Utils.AlignFrameTopLeft;

            // Read positions from config file
            using FileParser infile = new FileParser();

            // @CLASS GameStateNew: Layout|Description of menus/gamenew.txt
            if (infile.Open("menus/gamenew.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    // @ATTR button_prev|int, int, alignment : X, Y, Alignment|Position of button to choose the previous preset hero.
                    if (infile.Key == "button_prev")
                    {
                        int x = Parse.PopFirstInt(ref infile.Val);
                        int y = Parse.PopFirstInt(ref infile.Val);
                        int a = Parse.ToAlignment(Parse.PopFirstString(ref infile.Val), Utils.AlignFrameTopLeft);
                        _buttonPrev.SetBasePos(x, y, a);
                    }
                    // @ATTR button_next|int, int, alignment : X, Y, Alignment|Position of button to choose the next preset hero.
                    else if (infile.Key == "button_next")
                    {
                        int x = Parse.PopFirstInt(ref infile.Val);
                        int y = Parse.PopFirstInt(ref infile.Val);
                        int a = Parse.ToAlignment(Parse.PopFirstString(ref infile.Val), Utils.AlignFrameTopLeft);
                        _buttonNext.SetBasePos(x, y, a);
                    }
                    // @ATTR button_exit|int, int, alignment : X, Y, Alignment|Position of "Cancel" button.
                    else if (infile.Key == "button_exit")
                    {
                        int x = Parse.PopFirstInt(ref infile.Val);
                        int y = Parse.PopFirstInt(ref infile.Val);
                        int a = Parse.ToAlignment(Parse.PopFirstString(ref infile.Val));
                        _buttonExit.SetBasePos(x, y, a);
                    }
                    // @ATTR button_create|int, int, alignment : X, Y, Alignment|Position of "Create" button.
                    else if (infile.Key == "button_create")
                    {
                        int x = Parse.PopFirstInt(ref infile.Val);
                        int y = Parse.PopFirstInt(ref infile.Val);
                        int a = Parse.ToAlignment(Parse.PopFirstString(ref infile.Val));
                        _buttonCreate.SetBasePos(x, y, a);
                    }
                    // @ATTR button_permadeath|int, int, alignment : X, Y, Alignment|Position of checkbox for toggling permadeath.
                    else if (infile.Key == "button_permadeath")
                    {
                        int x = Parse.PopFirstInt(ref infile.Val);
                        int y = Parse.PopFirstInt(ref infile.Val);
                        int a = Parse.ToAlignment(Parse.PopFirstString(ref infile.Val), Utils.AlignFrameTopLeft);
                        _buttonPermadeath.SetBasePos(x, y, a);
                    }
                    // @ATTR button_randomize|int, int, alignment : X, Y, Alignment|Position of the "Randomize" button.
                    else if (infile.Key == "button_randomize")
                    {
                        int x = Parse.PopFirstInt(ref infile.Val);
                        int y = Parse.PopFirstInt(ref infile.Val);
                        int a = Parse.ToAlignment(Parse.PopFirstString(ref infile.Val), Utils.AlignFrameTopLeft);
                        _buttonRandomize.SetBasePos(x, y, a);
                    }
                    // @ATTR name_input|int, int, alignment : X, Y, Alignment|Position of the hero name textbox.
                    else if (infile.Key == "name_input")
                    {
                        int x = Parse.PopFirstInt(ref infile.Val);
                        int y = Parse.PopFirstInt(ref infile.Val);
                        int a = Parse.ToAlignment(Parse.PopFirstString(ref infile.Val), Utils.AlignFrameTopLeft);
                        _inputName.SetBasePos(x, y, a);
                    }
                    // @ATTR portrait_label|label|Label for the "Choose a Portrait" text.
                    else if (infile.Key == "portrait_label")
                    {
                        _labelPortrait.SetFromLabelInfo(Parse.PopLabelInfo(infile.Val));
                    }
                    // @ATTR name_label|label|Label for the "Choose a Name" text.
                    else if (infile.Key == "name_label")
                    {
                        _labelName.SetFromLabelInfo(Parse.PopLabelInfo(infile.Val));
                    }
                    // @ATTR permadeath_label|label|Label for the "Permadeath?" text.
                    else if (infile.Key == "permadeath_label")
                    {
                        _labelPermadeath.SetFromLabelInfo(Parse.PopLabelInfo(infile.Val));
                    }
                    // @ATTR classlist_label|label|Label for the "Choose a Class" text.
                    else if (infile.Key == "classlist_label")
                    {
                        _labelClasslist.SetFromLabelInfo(Parse.PopLabelInfo(infile.Val));
                    }
                    // @ATTR classlist_height|int|Number of visible rows for the class list widget.
                    else if (infile.Key == "classlist_height")
                    {
                        _classList.SetHeight(Parse.PopFirstInt(ref infile.Val));
                    }
                    // @ATTR portrait|rectangle|Position and dimensions of the portrait image.
                    else if (infile.Key == "portrait")
                    {
                        _portraitPos = Parse.ToRect(infile.Val);
                    }
                    // @ATTR class_list|int, int, alignment : X, Y, Alignment|Position of the class list.
                    else if (infile.Key == "class_list")
                    {
                        int x = Parse.PopFirstInt(ref infile.Val);
                        int y = Parse.PopFirstInt(ref infile.Val);
                        int a = Parse.ToAlignment(Parse.PopFirstString(ref infile.Val), Utils.AlignFrameTopLeft);
                        _classList.SetBasePos(x, y, a);
                    }
                    // @ATTR show_classlist|bool|Allows hiding the class list.
                    else if (infile.Key == "show_classlist")
                    {
                        _showClasslist = Parse.ToBool(infile.Val);
                    }
                    // @ATTR class_tip|int, int, alignment : X, Y, Alignment|Position of the class description tooltip.
                    else if (infile.Key == "class_tip")
                    {
                        _classTipPos.X = Parse.PopFirstInt(ref infile.Val);
                        _classTipPos.Y = Parse.PopFirstInt(ref infile.Val);
                        _classTipAlign = Parse.ToAlignment(Parse.PopFirstString(ref infile.Val), Utils.AlignFrameTopLeft);
                    }
                    // @ATTR show_class_tip|bool|When true, shows a persistent tooltip with the description of the selected class. Defaults to false.
                    else if (infile.Key == "show_class_tip")
                    {
                        _showClassTip = Parse.ToBool(infile.Val);
                    }
                    // @ATTR show_randomize|bool|Toggles the visibility of the "Randomize" button.
                    else if (infile.Key == "show_randomize")
                    {
                        _showRandomize = Parse.ToBool(infile.Val);
                    }
                    // @ATTR show_permadeath|bool|Toggles the visibility of the "Permadeath?" checkbox.
                    else if (infile.Key == "show_permadeath")
                    {
                        _showPermadeath = Parse.ToBool(infile.Val);
                    }
                    // @ATTR random_option|bool|Initially picks a random character option (aka portrait/name).
                    else if (infile.Key == "random_option")
                    {
                        _randomOption = Parse.ToBool(infile.Val);
                    }
                    // @ATTR random_class|bool|Initially picks a random character class.
                    else if (infile.Key == "random_class")
                    {
                        _randomClass = Parse.ToBool(infile.Val);
                    }
                    // @ATTR show_frame_background|bool|If true, the frame background image is drawn behind the menu.
                    else if (infile.Key == "show_frame_background")
                    {
                        HasFrameBackground = Parse.ToBool(infile.Val);
                    }
                    else
                    {
                        infile.Error("GameStateNew: '%s' is not a valid key.", infile.Key);
                    }
                }
                infile.Close();
            }

            // set up class list
            for (int i = 0; i < SharedResources.Eset.HeroClasses.Classes.Count; i++)
            {
                if (_showClassTip)
                    _classList.Append(SharedResources.Msg.Get(SharedResources.Eset.HeroClasses.Classes[i].Name), "");
                else
                    _classList.Append(SharedResources.Msg.Get(SharedResources.Eset.HeroClasses.Classes[i].Name), GetClassTooltip(i));
            }

            if (SharedResources.Eset.HeroClasses.Classes.Count != 0)
            {
                int classIndex = 0;
                if (_randomClass)
                    classIndex = Program.Rng.Next() % SharedResources.Eset.HeroClasses.Classes.Count;

                _classList.Select(classIndex);

                if (_showClassTip)
                {
                    _classTipData.Clear();
                    _classTipData.AddText(GetClassTooltip(classIndex));
                }
            }

            LoadGraphics();
            LoadOptions("hero_options.txt");

            if (_randomOption)
                SetHeroOption(OptionRandom);
            else
                SetHeroOption(OptionCurrent);

            // Set up tab list
            _tablist.Add(_buttonExit);
            _tablist.Add(_buttonCreate);
            _tablist.Add(_inputName);

            if (_showPermadeath)
            {
                _tablist.Add(_buttonPermadeath);
            }

            if (_showRandomize)
            {
                _tablist.Add(_buttonRandomize);
            }

            _tablist.Add(_buttonPrev);
            _tablist.Add(_buttonNext);
            if (_showClasslist)
            {
                _tablist.Add(_classList);
            }

            RefreshWidgets();

            SharedResources.RenderDevice!.SetBackgroundColor(new Color(0, 0, 0, 0));
        }

        private void LoadGraphics()
        {
            Image? graphics;

            graphics = SharedResources.RenderDevice!.LoadImage("images/menus/portrait_border.png", RenderDevice.ErrorNormal);
            if (graphics != null)
            {
                _portraitBorder = graphics.CreateSprite();
                graphics.Unref();
            }
        }

        private void LoadPortrait(string portraitFilename)
        {
            Image? graphics;

            if (_portraitImage != null)
                _portraitImage.Dispose();

            _portraitImage = null;
            graphics = SharedResources.RenderDevice!.LoadImage(portraitFilename, RenderDevice.ErrorNormal);
            if (graphics != null)
            {
                _portraitImage = graphics.CreateSprite();
                _portraitImage.SetDestFromRect(_portraitPos);
                graphics.Unref();
            }
        }

        /// <summary>
        /// Load body type "base" and portrait/head "portrait" options from a config file
        /// </summary>
        /// <param name="filename">File containing entries for option=base,look</param>
        private void LoadOptions(string filename)
        {
            using FileParser fin = new FileParser();
            // @CLASS GameStateNew: Hero options|Description of engine/hero_options.txt
            if (!fin.Open("engine/" + filename, FileParser.ModFile, FileParser.ErrorNormal)) return;

            int curIndex;
            while (fin.Next())
            {
                // @ATTR option|int, string, string, filename, string : Index, Base, Head, Portrait, Name|A default body, head, portrait, and name for a hero.
                if (fin.Key == "option")
                {
                    curIndex = Math.Max(0, Parse.PopFirstInt(ref fin.Val));

                    if (curIndex + 1 > _heroOptions.Count)
                    {
                        while (_heroOptions.Count < curIndex + 1)
                            _heroOptions.Add(new HeroOption());
                        _allOptions.Add(curIndex);
                    }

                    _heroOptions[curIndex].Base = Parse.PopFirstString(ref fin.Val);
                    _heroOptions[curIndex].Head = Parse.PopFirstString(ref fin.Val);
                    _heroOptions[curIndex].Portrait = Parse.PopFirstString(ref fin.Val);
                    _heroOptions[curIndex].Name = SharedResources.Msg!.Get(Parse.PopFirstString(ref fin.Val));
                }
            }
            fin.Close();

            if (_heroOptions.Count == 0)
            {
                _heroOptions.Add(new HeroOption());
            }

            _allOptions.Sort();
        }

        /// <summary>
        /// If the name text box is empty or hasn't been user-modified, set the name
        /// </summary>
        /// <param name="defaultName">The name we want to use for the hero</param>
        private void SetName(string defaultName)
        {
            if (_inputName!.GetText() == "" || !_modifiedName)
            {
                _inputName.SetText(defaultName);
                _modifiedName = false;
            }
        }

        private void SetHeroOption(int dir)
        {
            List<int> availableOptions = _allOptions;

            // get the available options from the currently selected class
            int classIndex;
            if ((classIndex = _classList!.GetSelected()) != -1)
            {
                if ((uint)classIndex < SharedResources.Eset!.HeroClasses.Classes.Count && SharedResources.Eset.HeroClasses.Classes[classIndex].Options.Count != 0)
                {
                    availableOptions = SharedResources.Eset.HeroClasses.Classes[classIndex].Options;
                }
            }

            if (dir == OptionCurrent)
            {
                // don't change current_option unless required
                if (!availableOptions.Contains(_currentOption))
                {
                    if (_randomOption && !ReferenceEquals(availableOptions, _allOptions))
                    {
                        int randIndex = Program.Rng.Next() % availableOptions.Count;
                        _currentOption = availableOptions[randIndex];
                    }
                    else
                    {
                        _currentOption = availableOptions[0];
                    }
                }
            }
            else if (dir == OptionNext)
            {
                // increment current_option
                int it = availableOptions.IndexOf(_currentOption);
                if (it == -1)
                {
                    _currentOption = availableOptions[0];
                }
                else
                {
                    ++it;
                    if (it != availableOptions.Count)
                        _currentOption = availableOptions[it];
                    else
                        _currentOption = availableOptions[0];
                }
            }
            else if (dir == OptionPrev)
            {
                // decrement current_option
                int it = availableOptions.IndexOf(_currentOption);
                if (it == 0)
                {
                    _currentOption = availableOptions[^1];
                }
                else
                {
                    --it;
                    _currentOption = availableOptions[it];
                }
            }
            else if (dir == OptionRandom && availableOptions.Count != 0)
            {
                int randIndex = Program.Rng.Next() % availableOptions.Count;
                _currentOption = availableOptions[randIndex];
            }

            LoadPortrait(_heroOptions[_currentOption].Portrait);
            SetName(_heroOptions[_currentOption].Name);
        }

        public override void Logic()
        {
            InputState inpt = SharedResources.Inpt!;

            if (inpt.WindowResized)
                RefreshWidgets();

            if (!_inputName!.EditMode)
                _tablist.Logic();

            _inputName.Logic();

            if (_showPermadeath)
            {
                _buttonPermadeath!.CheckClick();
            }

            if (_showClasslist && _classList!.CheckClick())
            {
                SetHeroOption(OptionCurrent);

                if (_showClassTip)
                {
                    _classTipData.Clear();
                    _classTipData.AddText(GetClassTooltip(_classList.GetSelected()));
                }
            }

            // require character name
            if (_inputName.GetText() == "")
            {
                if (_buttonCreate!.Enabled)
                {
                    _buttonCreate.Enabled = false;
                    _buttonCreate.Refresh();
                }
            }
            else
            {
                if (!_buttonCreate!.Enabled)
                {
                    _buttonCreate.Enabled = true;
                    _buttonCreate.Refresh();
                }
            }

            if (!_inputName.EditMode && !inpt.UsingMouse() && _tablist.GetCurrent() == -1)
            {
                if (_buttonCreate!.Enabled)
                    _tablist.SetCurrent(_buttonCreate);
                else
                    _tablist.SetCurrent(_buttonExit);
            }

            if ((inpt.Pressing[Input.Cancel] && !inpt.Lock[Input.Cancel]) || _buttonExit!.CheckClick())
            {
                if (inpt.Pressing[Input.Cancel])
                    inpt.Lock[Input.Cancel] = true;
                _deleteItems = false;
                ShowLoading();
                SetRequestedGameState(new GameStateLoad());
            }

            if (_buttonCreate!.CheckClick())
            {
                // start the new game
                inpt.LockAll = true;
                _deleteItems = false;
                ShowLoading();
                GameStatePlay play = new GameStatePlay();
                Avatar avatar = SharedGameResources.Pc!;
                avatar.Stats.GfxBase = _heroOptions[_currentOption].Base;
                avatar.Stats.GfxHead = _heroOptions[_currentOption].Head;
                avatar.Stats.GfxPortrait = _heroOptions[_currentOption].Portrait;
                avatar.Stats.CheckGfxPaths();
                avatar.Stats.Name = _inputName.GetText();
                avatar.Stats.Permadeath = _buttonPermadeath!.IsChecked;
                SharedResources.SaveLoad!.GameSlot = GameSlot;
                play.ResetGame();
                SharedResources.SaveLoad.LoadClass(_classList!.GetSelected());
                SetRequestedGameState(play);
            }

            // scroll through portrait options
            if (_buttonNext!.CheckClick())
            {
                SetHeroOption(OptionNext);
            }
            else if (_buttonPrev!.CheckClick())
            {
                SetHeroOption(OptionPrev);
            }

            if (_showRandomize && _buttonRandomize!.CheckClick())
            {
                if (SharedResources.Eset!.HeroClasses.Classes.Count != 0)
                {
                    int classIndex = Program.Rng.Next() % SharedResources.Eset.HeroClasses.Classes.Count;
                    _classList!.Select(classIndex);

                    if (_showClassTip)
                    {
                        _classTipData.Clear();
                        _classTipData.AddText(GetClassTooltip(classIndex));
                    }
                }
                SetHeroOption(OptionRandom);
            }

            if (_inputName.GetText() != _heroOptions[_currentOption].Name)
                _modifiedName = true;
        }

        public override void RefreshWidgets()
        {
            _buttonExit!.SetPos(0, 0);
            _buttonCreate!.SetPos(0, 0);

            _buttonPrev!.SetPos(0, 0);
            _buttonNext!.SetPos(0, 0);
            _buttonPermadeath!.SetPos(0, 0);
            _buttonRandomize!.SetPos(0, 0);
            _classList!.SetPos(0, 0);

            int frameOffsetX = (SharedResources.Settings!.ViewW - SharedResources.Eset!.Resolutions.FrameW) / 2;
            int frameOffsetY = (SharedResources.Settings.ViewH - SharedResources.Eset.Resolutions.FrameH) / 2;

            _labelPortrait!.SetPos(frameOffsetX, frameOffsetY);
            _labelName!.SetPos(frameOffsetX, frameOffsetY);
            _labelPermadeath!.SetPos(frameOffsetX, frameOffsetY);
            _labelClasslist!.SetPos(frameOffsetX, frameOffsetY);

            _inputName!.SetPos(0, 0);
        }

        public override void Render()
        {
            // display buttons
            _buttonExit!.Render();
            _buttonCreate!.Render();
            _buttonPrev!.Render();
            _buttonNext!.Render();
            _inputName!.Render();

            if (_showPermadeath)
                _buttonPermadeath!.Render();

            if (_showRandomize)
            {
                _buttonRandomize!.Render();
            }

            // display portrait option
            Rectangle src;
            Rectangle dest;

            src.Width = dest.Width = _portraitPos.Width;
            src.Height = dest.Height = _portraitPos.Height;
            src.X = src.Y = 0;
            dest.X = _portraitPos.X + (SharedResources.Settings!.ViewW - SharedResources.Eset!.Resolutions.FrameW) / 2;
            dest.Y = _portraitPos.Y + (SharedResources.Settings.ViewH - SharedResources.Eset.Resolutions.FrameH) / 2;

            if (_portraitImage != null)
            {
                _portraitImage.SetClipFromRect(src);
                _portraitImage.SetDestFromRect(dest);
                SharedResources.RenderDevice!.Render(_portraitImage);
                _portraitBorder!.SetClipFromRect(src);
                _portraitBorder.SetDestFromRect(dest);
                SharedResources.RenderDevice.Render(_portraitBorder);
            }

            // display labels
            _labelPortrait!.Render();
            _labelName!.Render();

            if (_showPermadeath)
                _labelPermadeath!.Render();

            // display class list
            if (_showClasslist)
            {
                _labelClasslist!.Render();
                _classList!.Render();

                if (_showClassTip && !_classTipData.IsEmpty())
                {
                    _classTip!.Prerender(_classTipData, new Int2(_classTipPos.X, _classTipPos.Y), TooltipData.StyleAbsolute);
                    Rectangle temp = _classTip.Bounds;
                    Utils.AlignToScreenEdge(Utils.AlignFrameTopLeft, ref temp);
                    _classTip.Render(_classTipData, new Int2(temp.X, temp.Y), TooltipData.StyleAbsolute);
                }
            }
        }

        private string GetClassTooltip(int index)
        {
            if ((uint)index >= SharedResources.Eset!.HeroClasses.Classes.Count)
                return "";

            string tooltip = "";
            if (SharedResources.Eset.HeroClasses.Classes[index].Description != "") tooltip += SharedResources.Msg!.Get(SharedResources.Eset.HeroClasses.Classes[index].Description);
            return tooltip;
        }

        /// <summary>
        /// 对应 C++ 析构函数 <c>~GameStateNew()</c>，释放顺序与原始 delete 顺序一致，
        /// 随后调用基类 <see cref="GameState.Dispose"/>。
        /// </summary>
        public override void Dispose()
        {
            if (_portraitImage != null)
                _portraitImage.Dispose();

            if (_portraitBorder != null)
                _portraitBorder.Dispose();

            if (_deleteItems)
            {
                SharedGameResources.Items?.Dispose();
                SharedGameResources.Items = null;
            }

            _buttonExit?.Dispose();
            _buttonCreate?.Dispose();
            _buttonNext?.Dispose();
            _buttonPrev?.Dispose();
            _buttonRandomize?.Dispose();
            _labelPortrait?.Dispose();
            _labelName?.Dispose();
            _inputName?.Dispose();
            _buttonPermadeath?.Dispose();
            _labelPermadeath?.Dispose();
            _labelClasslist?.Dispose();
            _classList?.Dispose();
            _classTip?.Dispose();

            base.Dispose();
        }
    }
}
