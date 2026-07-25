// 对应 C++ 源文件：MenuActiveEffects.h + MenuActiveEffects.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using System.Text;
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// EffectIcon
    ///
    /// 单个激活效果图标的运行时显示状态（对应原始 <c>class EffectIcon</c>）。
    /// </summary>
    public class EffectIcon
    {
        public int Icon;
        public int Type;
        public int Current;
        public int Max;
        public int Stacks;
        public int TimerCurrent;
        public int TimerMax;
        public Rectangle Pos;
        public Rectangle Overlay;
        public string Name = "";
        public WidgetLabel? StacksLabel;

        public EffectIcon()
        {
            Icon = -1;
            Type = 0;
            Current = 0;
            Max = 0;
            Stacks = 0;
            TimerCurrent = 0;
            TimerMax = 0;
            StacksLabel = null;
        }
    }

    /// <summary>
    /// MenuActiveEffects
    ///
    /// 处理激活效果（增益/减益）图标的显示、堆叠与工具提示。
    /// 持有的 <see cref="_timer"/> 与各 <see cref="EffectIcon.StacksLabel"/> 通过
    /// <see cref="IDisposable"/> 显式释放；<see cref="_timer"/> 的释放顺序与原始析构函数
    /// <c>~MenuActiveEffects()</c> 一致（仅 delete timer；StacksLabel 在 <see cref="Logic"/> 每帧清理）。
    /// </summary>
    public class MenuActiveEffects : Menu, IDisposable
    {
        private Sprite? _timer;
        private bool _isVertical;
        private bool _wrapBefore;
        private List<EffectIcon> _effectIcons = new List<EffectIcon>();

        public MenuActiveEffects()
        {
            _timer = null;
            _isVertical = false;

            // Load config settings
            using FileParser infile = new FileParser();
            // @CLASS MenuActiveEffects|Description of menus/activeeffects.txt
            if (infile.Open("menus/activeeffects.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (ParseMenuKey(infile.Key, infile.Val))
                        continue;

                    // @ATTR vertical|bool|True is vertical orientation; False is horizontal orientation.
                    if (infile.Key == "vertical")
                    {
                        _isVertical = Parse.ToBool(infile.Val);
                    }
                    // @ATTR wrap_before|bool|Determines where to place icons when they need to wrap to fit on the screen. The default is false, which will place icons from top to bottom (or left to right if using the vertical orientation).
                    else if (infile.Key == "wrap_before")
                    {
                        _wrapBefore = Parse.ToBool(infile.Val);
                    }
                    else
                    {
                        infile.Error("MenuActiveEffects: '%s' is not a valid key.", infile.Key);
                    }
                }
                infile.Close();
            }

            LoadGraphics();
            Align();
        }

        public void LoadGraphics()
        {
            Image? graphics;
            graphics = SharedResources.RenderDevice!.LoadImage("images/menus/disabled.png", RenderDevice.ErrorNormal);
            if (graphics != null)
            {
                _timer = graphics.CreateSprite();
                graphics.Unref();
            }
        }

        public void Logic()
        {
            for (int i = 0; i < _effectIcons.Count; ++i)
            {
                if (_effectIcons[i].StacksLabel != null)
                {
                    _effectIcons[i].StacksLabel!.Dispose();
                }
            }

            _effectIcons.Clear();

            int iconsPerRow = 1;
            if (!_isVertical)
            {
                iconsPerRow = (SharedResources.Settings!.ViewW - WindowArea.X) / SharedResources.Eset!.Resolutions.IconSize;
            }
            else
            {
                iconsPerRow = (SharedResources.Settings!.ViewH - WindowArea.Y) / SharedResources.Eset!.Resolutions.IconSize;
            }
            if (iconsPerRow < 1)
            {
                iconsPerRow = 1;
            }
            int wrapDir = (_wrapBefore ? -1 : 1);

            Avatar pc = SharedGameResources.Pc!;
            for (int i = 0; i < pc.Stats.Effects.EffectList.Count; ++i)
            {
                Effect ed = pc.Stats.Effects.EffectList[i];

                if (ed.Icon == -1)
                    continue;

                int mostRecentId = _effectIcons.Count - 1;

                if (ed.GroupStack && _effectIcons.Count > 0)
                {
                    EffectIcon eicon = _effectIcons[mostRecentId];
                    if (eicon.Type == ed.Type && eicon.Name == ed.Name && eicon.Icon == ed.Icon)
                    {
                        eicon.Stacks++;

                        eicon.TimerCurrent = (int)ed.EffectTimer.Current;
                        eicon.TimerMax = (int)ed.EffectTimer.Duration;

                        if (ed.Type == Effect.Shield)
                        {
                            //Shields stacks in momment of addition, we never have to reach that
                        }
                        else if (ed.Type == Effect.Heal)
                        {
                            //No special behavior
                        }
                        else
                        {
                            if (ed.EffectTimer.Current < (uint)eicon.Current)
                            {
                                if (ed.EffectTimer.Duration > 0)
                                    eicon.Overlay.Y = (SharedResources.Eset!.Resolutions.IconSize * (int)ed.EffectTimer.Current) / (int)ed.EffectTimer.Duration;
                                else
                                    eicon.Overlay.Y = SharedResources.Eset!.Resolutions.IconSize;
                                eicon.Current = eicon.TimerCurrent;
                                eicon.Max = eicon.TimerMax;
                            }
                        }

                        if (eicon.StacksLabel == null)
                        {
                            eicon.StacksLabel = new WidgetLabel();
                            eicon.StacksLabel.SetPos(eicon.Pos.X, eicon.Pos.Y);
                            eicon.StacksLabel.SetMaxWidth(SharedResources.Eset!.Resolutions.IconSize);
                        }

                        StringBuilder ss = new StringBuilder();
                        ss.Append('×').Append(eicon.Stacks);
                        eicon.StacksLabel.SetText(ss.ToString());

                        continue;
                    }
                }

                EffectIcon ei = new EffectIcon();
                ei.Icon = ed.Icon;
                ei.Name = ed.Name;
                ei.Type = ed.Type;
                ei.Stacks = 1;

                // icon position
                if (!_isVertical)
                {
                    ei.Pos.X = WindowArea.X + (_effectIcons.Count % iconsPerRow * SharedResources.Eset!.Resolutions.IconSize);
                    ei.Pos.Y = WindowArea.Y + (wrapDir * (_effectIcons.Count / iconsPerRow * SharedResources.Eset!.Resolutions.IconSize));
                }
                else
                {
                    ei.Pos.X = WindowArea.X + (wrapDir * (_effectIcons.Count / iconsPerRow * SharedResources.Eset!.Resolutions.IconSize));
                    ei.Pos.Y = WindowArea.Y + (_effectIcons.Count % iconsPerRow * SharedResources.Eset!.Resolutions.IconSize);
                }
                ei.Pos.Width = ei.Pos.Height = SharedResources.Eset!.Resolutions.IconSize;

                // timer overlay
                ei.Overlay.X = 0;
                ei.Overlay.Width = SharedResources.Eset!.Resolutions.IconSize;

                ei.TimerCurrent = (int)ed.EffectTimer.Current;
                ei.TimerMax = (int)ed.EffectTimer.Duration;

                if (ed.Type == Effect.Shield)
                {
                    ei.Overlay.Y = (int)((SharedResources.Eset!.Resolutions.IconSize * ed.Magnitude) / ed.MagnitudeMax);
                    ei.Current = (int)ed.Magnitude;
                    ei.Max = (int)ed.MagnitudeMax;
                }
                else if (ed.Type == Effect.Heal)
                {
                    ei.Overlay.Y = SharedResources.Eset!.Resolutions.IconSize;
                    // current and max are ignored
                }
                else
                {
                    if (ed.EffectTimer.Duration > 0)
                        ei.Overlay.Y = (SharedResources.Eset!.Resolutions.IconSize * (int)ed.EffectTimer.Current) / (int)ed.EffectTimer.Duration;
                    else
                        ei.Overlay.Y = SharedResources.Eset!.Resolutions.IconSize;
                    ei.Current = ei.TimerCurrent;
                    ei.Max = ei.TimerMax;
                }
                ei.Overlay.Height = SharedResources.Eset!.Resolutions.IconSize - ei.Overlay.Y;

                _effectIcons.Add(ei);
            }

            if (!_isVertical)
            {
                WindowArea.Width = _effectIcons.Count * SharedResources.Eset!.Resolutions.IconSize;
                WindowArea.Height = SharedResources.Eset!.Resolutions.IconSize;
            }
            else
            {
                WindowArea.Width = SharedResources.Eset!.Resolutions.IconSize;
                WindowArea.Height = _effectIcons.Count * SharedResources.Eset!.Resolutions.IconSize;
            }
            Align();
        }

        public override void Render()
        {
            IconManager icons = SharedResources.Icons!;
            for (int i = 0; i < _effectIcons.Count; ++i)
            {
                Int2 iconPos = new Int2(_effectIcons[i].Pos.X, _effectIcons[i].Pos.Y);
                icons.SetIcon(_effectIcons[i].Icon, iconPos);
                icons.Render();

                if (_timer != null)
                {
                    _timer.SetClipFromRect(_effectIcons[i].Overlay);
                    _timer.SetDestFromRect(_effectIcons[i].Pos);
                    SharedResources.RenderDevice!.Render(_timer);
                }

                if (_effectIcons[i].StacksLabel != null)
                {
                    _effectIcons[i].StacksLabel!.Render();
                }
            }
        }

        public void RenderTooltips(Int2 position)
        {
            TooltipData tipData = new TooltipData();

            for (int i = 0; i < _effectIcons.Count; ++i)
            {
                if (Utils.IsWithinRect(_effectIcons[i].Pos, position))
                {
                    StringBuilder ss = new StringBuilder();
                    if (_effectIcons[i].Name.Length != 0)
                    {
                        ss.Append(SharedResources.Msg!.Get(_effectIcons[i].Name));
                        if (_effectIcons[i].Type != Effect.Shield && _effectIcons[i].Stacks > 1)
                        {
                            ss.Append(' ').Append("(×").Append(_effectIcons[i].Stacks).Append(')');

                        }
                        tipData.AddText(ss.ToString());
                    }

                    if (_effectIcons[i].Type == Effect.Heal)
                        continue;

                    if (_effectIcons[i].Type == Effect.Shield)
                    {
                        ss.Clear();
                        ss.Append('(').Append(_effectIcons[i].Current).Append('/').Append(_effectIcons[i].Max).Append(')');
                        tipData.AddText(ss.ToString());
                    }
                    if (_effectIcons[i].Max > 0 && _effectIcons[i].TimerMax > 0)
                    {
                        ss.Clear();
                        ss.Append(SharedResources.Msg!.Get("Remaining:")).Append(' ').Append(Utils.GetDurationString(_effectIcons[i].TimerCurrent, SharedResources.Eset!.NumberFormat.Durations));
                        tipData.AddText(ss.ToString());
                    }

                    break;
                }
            }

            SharedResources.Tooltipm!.Push(tipData, position, TooltipData.StyleFloat);
        }

        /// <summary>
        /// 对应 C++ 的 <c>~MenuActiveEffects()</c>：析构函数体中仅释放 <c>timer</c>，
        /// 随后隐式调用基类 <c>~Menu()</c>。
        /// </summary>
        public override void Dispose()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }

            base.Dispose();
        }
    }
}
