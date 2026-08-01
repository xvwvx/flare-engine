// 对应 C++ 源文件：Subtitles.h + Subtitles.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// Subtitles
    ///
    /// 加载并显示与音效文件关联的字幕文本。
    /// 持有的 <see cref="Sprite"/> 背景精灵通过 <see cref="IDisposable"/> 显式释放，
    /// 释放顺序与原始析构函数 <c>~Subtitles()</c> 一致（先 <c>background</c>，再值成员
    /// <see cref="WidgetLabel"/> 的析构）。
    /// </summary>
    public class Subtitles : IDisposable
    {
        /// <summary>对应 C++ 内部类 <c>Subtitle</c>。</summary>
        public class Subtitle
        {
            public ulong Filename;
            public string Text;

            public Subtitle()
            {
                Filename = 0;
                Text = "";
            }
        }

        private readonly List<Subtitle> _subtitles = new List<Subtitle>();
        private readonly WidgetLabel _label = new WidgetLabel();
        private Int2 _labelPos;
        private int _labelAlignment;
        private ulong _currentId;
        private string _currentText = "";
        private bool _visible;
        private Sprite? _background;
        private Rectangle _backgroundRect;
        private Color _backgroundColor;
        private readonly Timer _visibleTimer = new Timer();

        public Subtitles()
        {
            var mods = SharedResources.Mods!;
            var msg = SharedResources.Msg!;
            var font = SharedResources.Font!;

            _currentId = unchecked((ulong)-1);
            _currentText = "";
            _visible = false;
            _background = null;
            _backgroundColor = new Color(0, 0, 0, 200);

            using FileParser infile = new FileParser();

            Subtitle temp = new Subtitle();
            Subtitle current = temp;

            // @CLASS Subtitles|Description of soundfx/subtitles.txt
            if (infile.Open("soundfx/subtitles.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (infile.NewSection && infile.Section == "subtitle")
                    {
                        temp = new Subtitle();
                        current = temp;
                    }

                    if (infile.Section == "style")
                    {
                        if (infile.Key == "text_pos")
                        {
                            // @ATTR style.text_pos|label|Position and style of the subtitle text.
                            _label.SetFromLabelInfo(Parse.PopLabelInfo(infile.Val));
                        }
                        else if (infile.Key == "pos")
                        {
                            // @ATTR style.pos|point|Position of the subtitle text relative to alignment.
                            _labelPos = Parse.ToPoint(infile.Val);
                        }
                        else if (infile.Key == "align")
                        {
                            // @ATTR style.align|alignment|Alignment of the subtitle text.
                            _labelAlignment = Parse.ToAlignment(infile.Val);
                        }
                        else if (infile.Key == "background_color")
                        {
                            // @ATTR style.background_color|color, int : Color, Alpha|Color and alpha of the subtitle background rectangle.
                            _backgroundColor = Parse.ToRGBA(infile.Val);
                        }
                    }
                    else if (infile.Section == "subtitle")
                    {
                        // if we want to replace a list item by ID, the ID needs to be parsed first
                        // but it is not essential if we're just adding to the list, so this is simply a warning
                        if (infile.Key != "id" && current.Filename == 0)
                        {
                            infile.Error("Subtitles: Expected 'id', but found '%s'.", infile.Key);
                        }

                        if (infile.Key == "id")
                        {
                            // @ATTR subtitle.id|filename|Filename of the sound file that will trigger this subtitle.
                            ulong filenameHash = (ulong)Utils.HashString(mods.Locate(infile.Val));

                            bool foundId = false;
                            for (int i = 0; i < _subtitles.Count; ++i)
                            {
                                if (_subtitles[i].Filename == filenameHash)
                                {
                                    current = _subtitles[i];
                                    foundId = true;
                                }
                            }
                            if (!foundId)
                            {
                                _subtitles.Add(temp);
                                current = _subtitles[_subtitles.Count - 1];
                                current.Filename = filenameHash;
                            }
                        }
                        else if (infile.Key == "text")
                        {
                            // @ATTR subtitle.text|string|The subtitle text that will be displayed.
                            current.Text = msg.Get(infile.Val);
                        }
                    }
                }
            }

            _label.SetColor(font.GetColor(FontEngine.ColorMenuNormal));
        }

        /// <summary>对应 C++ 的 <c>~Subtitles()</c>。</summary>
        public void Dispose()
        {
            if (_background != null)
            {
                _background.Dispose();
                _background = null;
            }

            _label.Dispose();

            GC.SuppressFinalize(this);
        }

        private void SetTextByID(SoundID id)
        {
            if (id == -1 && _visibleTimer.IsEnd())
            {
                _currentId = unchecked((ulong)-1);
                _currentText = "";

                if (_background != null)
                {
                    _background.Dispose();
                    _background = null;
                }

                return;
            }

            var settings = SharedResources.Settings!;
            for (int i = 0; i < _subtitles.Count; ++i)
            {
                if (_subtitles[i].Filename == (ulong)id)
                {
                    _currentId = (ulong)id;
                    _currentText = _subtitles[i].Text;
                    UpdateLabelAndBackground();

                    // 1 second per 10 letters
                    _visibleTimer.Duration = (uint)(_currentText.Length * (settings.MaxFramesPerSec / 10));

                    return;
                }
            }
        }

        public void Logic(SoundID id)
        {
            SetTextByID(id);

            _visibleTimer.Tick();

            if (string.IsNullOrEmpty(_currentText))
            {
                _visible = false;
                return;
            }
            else
            {
                _visible = true;
            }

            UpdateLabelAndBackground();
        }

        public void Render()
        {
            if (!_visible || !SharedResources.Settings!.Subtitles)
                return;

            if (_background != null)
            {
                SharedResources.RenderDevice!.Render(_background);
            }

            _label.Render();
        }

        private void UpdateLabelAndBackground()
        {
            // position subtitle
            Rectangle r = default;
            r.X = _labelPos.X;
            r.Y = _labelPos.Y;
            Utils.AlignToScreenEdge(_labelAlignment, ref r);
            _label.SetPos(r.X, r.Y);
            _label.SetText(_currentText);

            // background is transparent, no need to create a background surface
            if (_backgroundColor.A == 0)
                return;

            var font = SharedResources.Font!;
            var renderDevice = SharedResources.RenderDevice!;

            // create padded background rectangle
            Rectangle oldBackgroundRect = _backgroundRect;
            _backgroundRect = _label.GetBounds();
            int padding = font.GetLineHeight() / 4;
            _backgroundRect.X -= padding;
            _backgroundRect.Y -= padding;
            _backgroundRect.Width += padding * 2;
            _backgroundRect.Height += padding * 2;

            // update our background surface if needed
            if (_background == null || oldBackgroundRect.Width != _backgroundRect.Width || oldBackgroundRect.Height != _backgroundRect.Height)
            {
                if (_background != null)
                {
                    _background.Dispose();
                    _background = null;
                }

                // fill the background rectangle
                Image? temp = renderDevice.CreateImage(_backgroundRect.Width, _backgroundRect.Height);
                if (temp != null)
                {
                    // translucent black background
                    temp.FillWithColor(_backgroundColor);
                    _background = temp.CreateSprite();
                    temp.Unref();
                }
            }

            if (_background != null)
            {
                _background.SetDest(_backgroundRect.X, _backgroundRect.Y);
            }
        }
    }
}
