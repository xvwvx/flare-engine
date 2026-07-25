// 对应 C++ 源文件：MenuRegionTitle.h + MenuRegionTitle.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// MenuRegionTitle
    ///
    /// 区域名称标题菜单：在玩家进入新区域时以淡入/停留/淡出动画显示地图标题文本。
    /// 持有的 <see cref="WidgetLabel"/> 通过 <see cref="IDisposable"/> 显式释放，
    /// 释放顺序与原始析构函数 <c>~MenuRegionTitle()</c> 一致（先 <see cref="_label"/>，再基类 <see cref="Menu"/>）。
    /// </summary>
    public class MenuRegionTitle : Menu, IDisposable
    {
        private string _title;
        private readonly Timer _timer;
        private readonly Timer _fadeInTimer;
        private readonly Timer _fadeOutTimer;
        private readonly WidgetLabel _label;

        public MenuRegionTitle()
        {
            Settings settings = SharedResources.Settings!;
            FontEngine font = SharedResources.Font!;

            _title = "";
            _timer = new Timer();
            _fadeInTimer = new Timer();
            _fadeOutTimer = new Timer();
            _label = new WidgetLabel();

            // default layout
            // default window width and height are 0, since we just need an anchor point for the label
            SetWindowPos(0, 64);
            Alignment = Utils.AlignTop;
            _label.SetJustify(FontEngine.JustifyCenter);
            _label.SetBasePos(WindowArea.Width / 2, WindowArea.Height / 2, Utils.AlignTopLeft);
            _label.SetFont("font_region_title");

            _timer.Duration = (uint)(settings.MaxFramesPerSec * 3);
            _fadeInTimer.Duration = (uint)settings.MaxFramesPerSec;
            _fadeOutTimer.Duration = (uint)settings.MaxFramesPerSec;

            // Load config settings
            using FileParser infile = new FileParser();
            // @CLASS MenuRegionTitle|Description of menus/region_title.txt
            if (infile.Open("menus/region_title.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (ParseMenuKey(infile.Key, infile.Val))
                        continue;
                    else if (infile.Key == "label")
                    {
                        // @ATTR label_title|label|Position of the text displaying the map name.
                        _label.SetFromLabelInfo(Parse.PopLabelInfo(infile.Val));
                    }
                    else if (infile.Key == "hold_duration")
                    {
                        // @ATTR hold_duration|duration|Duration that the text is shown in between the fade animations.
                        _timer.Duration = (uint)Parse.ToDuration(infile.Val);
                    }
                    else if (infile.Key == "fade_in_duration")
                    {
                        // @ATTR fade_in_duration|duration|Duration of the "fade in" animation.
                        _fadeInTimer.Duration = (uint)Parse.ToDuration(infile.Val);
                    }
                    else if (infile.Key == "fade_out_duration")
                    {
                        // @ATTR fade_out_duration|duration|Duration of the "fade out" animation.
                        _fadeOutTimer.Duration = (uint)Parse.ToDuration(infile.Val);
                    }
                }
                infile.Close();
            }

            _label.SetText("");
            _label.SetColor(font.GetColor(FontEngine.ColorMenuNormal));

            if (_background == null)
                SetBackground("images/menus/game_over.png");

            Align();

            Visible = false;
        }

        /// <summary>
        /// 对应 C++ 的 <c>~MenuRegionTitle()</c>：析构函数体为空，随后释放嵌入的 label，最后调用基类 <see cref="Menu.Dispose"/>。
        /// </summary>
        public override void Dispose()
        {
            _label.Dispose();

            base.Dispose();
        }

        public override void Align()
        {
            base.Align();

            _label.SetPos(WindowArea.X, WindowArea.Y);
        }

        public void Logic()
        {
            if (!Visible || !Enabled)
                return;

            if (_timer.IsEnd() && _fadeOutTimer.IsEnd())
            {
                Visible = false;
            }
            else
            {
                if (_timer.IsBegin() && !_fadeInTimer.IsEnd())
                {
                    float normalizedTime = (float)_fadeInTimer.Current / (float)_fadeInTimer.Duration;
                    _label.SetAlpha((byte)(255f * (1f - normalizedTime)));
                    _fadeInTimer.Tick();
                }
                if (!_timer.IsEnd() && _fadeInTimer.IsEnd())
                {
                    _label.SetAlpha(255);
                    _timer.Tick();
                }
                if (_timer.IsEnd() && !_fadeOutTimer.IsEnd())
                {
                    float normalizedTime = (float)_fadeOutTimer.Current / (float)_fadeOutTimer.Duration;
                    _label.SetAlpha((byte)(255f * normalizedTime));
                    _fadeOutTimer.Tick();
                }
            }
        }

        public void SetTitle(string newTitle)
        {
            if (!Enabled)
                return;

            if (newTitle != _title)
            {
                _title = newTitle;
                _label.SetText(_title);
                _label.SetAlpha(0);
                _timer.Reset(Timer.Begin);
                _fadeInTimer.Reset(Timer.Begin);
                _fadeOutTimer.Reset(Timer.Begin);
                Visible = true;
            }
        }

        public override void Render()
        {
            if (!Visible || !Enabled)
                return;

            // background
            base.Render();

            _label.Render();
        }
    }
}
