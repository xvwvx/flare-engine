// <自动生成> 对应 C++ 源文件：MenuStatBar.h + MenuStatBar.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using System.Text;
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// MenuStatBar - HP/MP/XP 状态条控件，对应 C++ <c>class MenuStatBar : public Menu</c>。
    /// 管理血条 Sprite 和数值标签 WidgetLabel，通过 IDisposable 释放资源。
    /// </summary>
    public class MenuStatBar : Menu, IDisposable
    {
        private const int Horizontal = 0;
        private const int Vertical = 1;

        /// <summary>?? C++ <c>MenuStatBarValue</c>。XP  UnsignedFloat</summary>
        private struct MenuStatBarValue
        {
            public float Float;
            public ulong Unsigned;
        }

        public const int TypeHp = 0;
        public const int TypeMp = 1;
        public const int TypeXp = 2;
        public const int TypeResourceStat = 3;

        private Sprite? _bar;
        private WidgetLabel _label;
        private MenuStatBarValue _statMin;
        private MenuStatBarValue _statCur;
        private MenuStatBarValue _statCurPrev;
        private MenuStatBarValue _statMax;
        private Rectangle _barPos;
        private LabelInfo _textPos;
        private bool _enabled;
        private bool _orientation;
        private bool _customTextPos;
        private string _customString;
        private string _barGfx;
        private string _barGfxBackground;
        private short _type;
        private int _resourceStatIndex;
        private Timer _timeout;
        private Int2 _barFillOffset;
        private Int2 _barFillSize;

        public MenuStatBar(short type, int resourceStatIndex)
        {
            _bar = null;
            _label = new WidgetLabel();
            _enabled = true;
            _orientation = false;
            _customTextPos = false;
            _customString = "";
            _barGfx = "";
            _barGfxBackground = "";
            _type = type;
            _resourceStatIndex = resourceStatIndex;
            _barFillOffset = new Int2();
            _barFillSize = new Int2(-1, -1);
            _timeout = new Timer();

            string typeFilename = "";
            if (_type == TypeHp)
                typeFilename = "menus/hp.txt";
            else if (_type == TypeMp)
                typeFilename = "menus/mp.txt";
            else if (_type == TypeXp)
                typeFilename = "menus/xp.txt";
            else if (_type == TypeResourceStat)
                typeFilename = SharedResources.Eset!.ResourceStats.Stats[_resourceStatIndex].MenuFilename;

            if (_type == TypeXp)
            {
                _statMin.Unsigned = 0;
                _statCur.Unsigned = 0;
                _statCurPrev.Unsigned = 0;
                _statMax.Unsigned = 0;
            }
            else
            {
                _statMin.Float = 0;
                _statCur.Float = 0;
                _statCurPrev.Float = 0;
                _statMax.Float = 0;
            }

            // Load config settings
            using FileParser infile = new FileParser();
            // @CLASS MenuStatBar|Description of menus/hp.txt, menus/mp.txt, menus/xp.txt
            if (typeFilename != "" && infile.Open(typeFilename, FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (ParseMenuKey(infile.Key, infile.Val))
                        continue;

                    // @ATTR bar_pos|rectangle|Position and dimensions of the bar graphics.
                    if (infile.Key == "bar_pos")
                    {
                        _barPos = Parse.ToRect(infile.Val);
                    }
                    // @ATTR text_pos|label|Position of the text displaying the current value of the relevant stat.
                    else if (infile.Key == "text_pos")
                    {
                        _customTextPos = true;
                        _textPos = Parse.PopLabelInfo(infile.Val);
                    }
                    // @ATTR orientation|bool|True is vertical orientation; false is horizontal.
                    else if (infile.Key == "orientation")
                    {
                        _orientation = Parse.ToBool(infile.Val);
                    }
                    // @ATTR bar_gfx|filename|Filename of the image to use for the "fill" of the bar.
                    else if (infile.Key == "bar_gfx")
                    {
                        _barGfx = infile.Val;
                    }
                    // @ATTR bar_gfx_background|filename|Filename of the image to use for the base of the bar.
                    else if (infile.Key == "bar_gfx_background")
                    {
                        _barGfxBackground = infile.Val;
                    }
                    // @ATTR hide_timeout|duration|Hide HP and MP bar if full mana or health, after given amount of seconds; Hide XP bar if no changes in XP points for given amount of seconds. 0 disable hiding.
                    else if (infile.Key == "hide_timeout")
                    {
                        _timeout.Duration = (uint)Parse.ToDuration(infile.Val);
                    }
                    // @ATTR bar_fill_offset|point|Offset of the bar's fill graphics relative to the bar_pos X/Y.
                    else if (infile.Key == "bar_fill_offset")
                    {
                        _barFillOffset = Parse.ToPoint(infile.Val);
                    }
                    // @ATTR bar_fill_size|int, int : Width, Height|Size of the bar's fill graphics. If not defined, the width/height of bar_pos is used.
                    else if (infile.Key == "bar_fill_size")
                    {
                        _barFillSize = Parse.ToPoint(infile.Val);
                    }
                    // @ATTR enabled|bool|Determines if the bar will be rendered. Disable the bar completely by setting this to false.
                    else if (infile.Key == "enabled")
                    {
                        _enabled = Parse.ToBool(infile.Val);
                    }
                    else
                    {
                        infile.Error("MenuStatBar: '%s' is not a valid key.", infile.Key);
                    }
                }
                infile.Close();
            }

            // default to bar_pos size if bar_fill_size is undefined
            if (_barFillSize.X == -1 || _barFillSize.Y == -1)
            {
                _barFillSize.X = _barPos.Width;
                _barFillSize.Y = _barPos.Height;
            }

            LoadGraphics();

            Align();
        }

        public void LoadGraphics()
        {
            if (!_enabled)
                return;

            Image? graphics;

            if (_barGfxBackground != "")
            {
                SetBackground(_barGfxBackground);
            }

            if (_barGfx != "")
            {
                graphics = SharedResources.RenderDevice!.LoadImage(_barGfx, RenderDevice.ErrorNormal);
                if (graphics != null)
                {
                    _bar = graphics.CreateSprite();
                    graphics.Unref();
                }
            }
        }

        public void Update()
        {
            var pc = SharedGameResources.Pc!;
            var eset = SharedResources.Eset!;
            var msg = SharedResources.Msg!;

            if (!_enabled)
                return;

            if (_type == TypeXp)
            {
                _statCurPrev.Unsigned = _statCur.Unsigned;
                _statMin.Unsigned = 0;
                _statCur.Unsigned = pc.Stats.Xp - eset.Xp.GetLevelXP(pc.Stats.Level);
                _statMax.Unsigned = eset.Xp.GetLevelXP(pc.Stats.Level + 1) - eset.Xp.GetLevelXP(pc.Stats.Level);

                if (pc.Stats.Level == eset.Xp.GetMaxLevel())
                {
                    _customString = msg.GetV("XP: %lu", pc.Stats.Xp);
                }
                else
                {
                    _customString = msg.GetV("XP: %lu/%lu", _statCur.Unsigned, _statMax.Unsigned);
                }
            }
            else if (_type == TypeHp)
            {
                _statCurPrev.Float = _statCur.Float;
                _statMin.Float = 0;
                _statCur.Float = pc.Stats.Hp;
                _statMax.Float = pc.Stats.Get(global::FlareEngine.Stats.HpMax);
            }
            else if (_type == TypeMp)
            {
                _statCurPrev.Float = _statCur.Float;
                _statMin.Float = 0;
                _statCur.Float = pc.Stats.Mp;
                _statMax.Float = pc.Stats.Get(global::FlareEngine.Stats.MpMax);
            }
            else if (_type == TypeResourceStat)
            {
                _statCurPrev.Float = _statCur.Float;
                _statMin.Float = 0;
                _statCur.Float = pc.Stats.ResourceStats[_resourceStatIndex];
                _statMax.Float = pc.Stats.GetResourceStat(_resourceStatIndex, EngineSettings.ResourceStatsSettings.StatBase);
            }
        }

        private bool Disappear()
        {
            if (!_enabled)
                return true;

            if (_timeout.Duration > 0 && SharedResources.Settings!.StatbarAutohide)
            {
                if (_type == TypeXp)
                {
                    if (_statCurPrev.Unsigned != _statCur.Unsigned)
                    {
                        _timeout.Reset(Timer.Begin);
                    }
                }
                else
                {
                    if (_statCur.Float != _statMax.Float)
                    {
                        _timeout.Reset(Timer.Begin);
                    }
                }

                _timeout.Tick();
                if (_timeout.IsEnd())
                    return true;
            }
            return false;
        }

        public override void Render()
        {
            var renderDevice = SharedResources.RenderDevice!;
            var settings = SharedResources.Settings!;
            var inpt = SharedResources.Inpt!;
            var menu = SharedGameResources.Menu!;
            var eset = SharedResources.Eset!;

            if (Disappear()) return;

            Rectangle src;
            Rectangle dest = default;

            Rectangle barDest = _barPos;
            barDest.X = _barPos.X + WindowArea.X;
            barDest.Y = _barPos.Y + WindowArea.Y;

            dest.X = barDest.X;
            dest.Y = barDest.Y;
            src.X = 0;
            src.Y = 0;
            src.Width = _barPos.Width;
            src.Height = _barPos.Height;
            SetBackgroundClip(src);
            SetBackgroundDest(dest);
            base.Render();

            int barLength = 0;

            if (_type == TypeXp)
            {
                ulong statCurClamped = Math.Min(_statCur.Unsigned, _statMax.Unsigned);
                ulong normalizedCur = statCurClamped - Math.Min(statCurClamped, _statMin.Unsigned);
                ulong normalizedMax = _statMax.Unsigned - Math.Min(_statMax.Unsigned, _statMin.Unsigned);

                ulong barFillSizeOriented = 0;
                if (!_orientation)
                    barFillSizeOriented = (ulong)_barFillSize.X;
                else
                    barFillSizeOriented = (ulong)_barFillSize.Y;

                barLength = (int)((normalizedMax == 0) ? 0 : (normalizedCur * barFillSizeOriented) / normalizedMax);
                if (barLength == 0 && normalizedCur > 0)
                    barLength = 1;
            }
            else
            {
                float statCurClamped = Math.Min(_statCur.Float, _statMax.Float);
                float normalizedCur = statCurClamped - Math.Min(statCurClamped, _statMin.Float);
                float normalizedMax = _statMax.Float - Math.Min(_statMax.Float, _statMin.Float);

                float barFillSizeOriented = 0;
                if (!_orientation)
                    barFillSizeOriented = (float)_barFillSize.X;
                else
                    barFillSizeOriented = (float)_barFillSize.Y;

                barLength = (int)((normalizedMax == 0) ? 0 : (normalizedCur * barFillSizeOriented) / normalizedMax);
                if (barLength == 0 && normalizedCur > 0)
                    barLength = 1;
            }

            if (!_orientation)
            {
                src.X = 0;
                src.Y = 0;
                src.Width = barLength;
                src.Height = _barFillSize.Y;
                dest.X = barDest.X + _barFillOffset.X;
                dest.Y = barDest.Y + _barFillOffset.Y;
            }
            else
            {
                src.X = 0;
                src.Y = _barFillSize.Y - barLength;
                src.Width = _barFillSize.X;
                src.Height = barLength;
                dest.X = barDest.X + _barFillOffset.X;
                dest.Y = barDest.Y + _barFillOffset.Y + src.Y;
            }

            if (_bar != null)
            {
                _bar.SetClipFromRect(src);
                _bar.SetDestFromRect(dest);
                renderDevice.Render(_bar);
            }

            if (_textPos == null || !_textPos.Hidden)
            {
                if (settings.StatbarLabels || (inpt.UsingMouse() && Utils.IsWithinRect(barDest, inpt.Mouse) && !menu.Exit!.Visible))
                {
                    StringBuilder ss = new StringBuilder();
                    if (_customString != "")
                        ss.Append(_customString);
                    else if (_type == TypeXp)
                        ss.Append(_statCur.Unsigned).Append('/').Append(_statMax.Unsigned);
                    else
                        ss.Append(Utils.FloatToString(_statCur.Float, eset.NumberFormat.PlayerStatbar)).Append('/').Append(Utils.FloatToString(_statMax.Float, eset.NumberFormat.PlayerStatbar));

                    _label.SetText(ss.ToString());
                    _label.SetColor(SharedResources.Font!.GetColor(FontEngine.ColorMenuNormal));

                    if (_customTextPos)
                    {
                        _label.SetPos(barDest.X + _textPos.X, barDest.Y + _textPos.Y);
                        _label.SetJustify(_textPos.Justify);
                        _label.SetVAlign(_textPos.Valign);
                        _label.SetFont(_textPos.FontStyle);
                    }
                    else
                    {
                        if (_bar != null)
                        {
                            _label.SetPos(dest.X + _barFillSize.X / 2, dest.Y + _barFillSize.Y / 2);
                        }
                        else
                        {
                            _label.SetPos(barDest.X + _barPos.Width / 2, barDest.Y + _barPos.Height / 2);
                        }
                        _label.SetJustify(FontEngine.JustifyCenter);
                        _label.SetVAlign(LabelInfo.ValignCenter);
                    }
                    _label.Render();
                }
            }
        }

        /// <summary>
        /// 对应 C++<c>~MenuStatBar()</c>bar label<see cref="Menu.Dispose"/>??
        /// </summary>
        public override void Dispose()
        {
            if (_bar != null)
            {
                _bar.Dispose();
                _bar = null;
            }

            _label.Dispose();

            base.Dispose();
        }
    }
}
