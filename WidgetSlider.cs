// <自动生成> 对应 C++ 源文件：WidgetSlider.h + WidgetSlider.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using System.Diagnostics;
using System.Globalization;
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// WidgetSlider
    ///
    /// 菜单中使用的滑块控件：在一个整数区间 [minimum, maximum] 内选取 value，
    /// 既支持鼠标拖拽滑块把手（knob），也支持通过 GetPrev()/GetNext()（键盘/手柄步进）调整数值。
    /// 持有的 Sprite 缓存资源通过 IDisposable 显式释放，对应原始析构函数中的 delete sl。
    /// </summary>
    public class WidgetSlider : Widget, IDisposable
    {
        public const string DefaultFile = "images/menus/buttons/slider_default.png";

        // This is the position of the slider's knob within the screen
        public Rectangle PosKnob;

        public bool Enabled;

        /// <summary>
        /// 对应 C++ 的 <c>int getValue() const</c>：无额外逻辑的只读访问器，
        /// 按规则转为自动属性；写入只能通过 Set()/GetPrev()/GetNext() 内部完成，
        /// 与原始"仅 set() 系列方法可修改 value"的封装保持一致。
        /// </summary>
        public int Value { get; private set; }

        private Sprite? _sl;
        private bool _pressed;
        private bool _changedWithoutMouse;
        private int _minimum;
        private int _maximum;

        private SoundID _soundActivate;

        public WidgetSlider(string fname)
        {
            var renderDevice = SharedResources.RenderDevice!;
            var eset = SharedResources.Eset!;
            var snd = SharedResources.Snd!;

            Enabled = true;
            _sl = null;
            _pressed = false;
            _changedWithoutMouse = false;
            _minimum = 0;
            _maximum = 0;
            Value = 0;
            _soundActivate = 0;

            Image? graphics = null;
            if (fname != DefaultFile)
            {
                graphics = renderDevice.LoadImage(fname, RenderDevice.ErrorNormal);
            }
            if (graphics == null)
            {
                graphics = renderDevice.LoadImage(DefaultFile, RenderDevice.ErrorExit);
            }
            if (graphics != null)
            {
                _sl = graphics.CreateSprite();
                Pos.Width = _sl.GetGraphicsWidth();
                Pos.Height = _sl.GetGraphicsHeight() / 2;
                PosKnob.Width = _sl.GetGraphicsWidth() / 8;
                PosKnob.Height = _sl.GetGraphicsHeight() / 2;
                graphics.Unref();
            }

            ScrollType = ScrollHorizontal;

            if (eset.Widgets.SoundActivate.Length != 0)
                _soundActivate = snd.Load(eset.Widgets.SoundActivate, "Widget activate");
        }

        /// <summary>
        /// 对应 C++ 的 <c>~WidgetSlider()</c>，释放缓存的 Sprite 并卸载音效，
        /// 释放顺序与原始析构函数一致（先 Sprite，后音效）。
        /// </summary>
        public void Dispose()
        {
            _sl?.Dispose();
            _sl = null;
            SharedResources.Snd!.Unload(_soundActivate);
            GC.SuppressFinalize(this);
        }

        public override void SetPos(int offsetX, int offsetY)
        {
            base.SetPos(offsetX, offsetY);
            // Widget.SetPos(offsetX - (PosKnob.Width / 2), offsetY);
            Set(_minimum, _maximum, Value);
        }

        public bool CheckClick()
        {
            InputState inpt = SharedResources.Inpt!;
            return CheckClickAt(inpt.Mouse.X, inpt.Mouse.Y);
        }

        public bool CheckClickAt(int x, int y)
        {
            InputState inpt = SharedResources.Inpt!;
            var snd = SharedResources.Snd!;

            EnableTablistNav = Enabled;

            if (!Enabled) return false;
            Int2 mouse = new Int2(x, y);
            //	We are just grabbing the knob
            if (!_pressed && inpt.Pressing[Input.Main1] && !inpt.Lock[Input.Main1])
            {
                if (Utils.IsWithinRect(PosKnob, mouse))
                {
                    _pressed = true;
                    inpt.Lock[Input.Main1] = true;
                    snd.Play(_soundActivate, "widget_activate", SoundManager.NoPos, !SoundManager.Loop);
                    return true;
                }
                return false;
            }

            // getNext() or getPrev() was used to change the slider, so treat it as a "click"
            if (_changedWithoutMouse)
            {
                _changedWithoutMouse = false;
                return true;
            }

            // buttons already in use, new click not allowed
            if (inpt.Lock[Input.Up]) return false;
            if (inpt.Lock[Input.Down]) return false;

            if (_pressed)
            {
                // The knob has been released
                if (!inpt.Lock[Input.Main1])
                {
                    _pressed = false;
                }

                // set the value of the slider
                int baseWidth = Pos.Width - PosKnob.Width;
                int knobOffset = PosKnob.Width / 2;
                int tmp = Math.Max(knobOffset, Math.Min(mouse.X - Pos.X, baseWidth + knobOffset));

                PosKnob.X = Pos.X + tmp - knobOffset;
                Debug.Assert(baseWidth != 0);
                Value = _minimum + ((tmp - knobOffset) * (_maximum - _minimum)) / baseWidth;

                // "snap" to a whole value
                Set(_minimum, _maximum, Value);

                return true;
            }
            return false;
        }

        public void Set(int min, int max, int val)
        {
            _minimum = min;
            _maximum = max;
            Value = val;

            if (max - min != 0)
            {
                PosKnob.X = Pos.X + ((val - min) * (Pos.Width - PosKnob.Width)) / (max - min);
                PosKnob.Y = Pos.Y;
            }
        }

        public override void Render()
        {
            Rectangle baseRect = new Rectangle();
            baseRect.X = 0;
            baseRect.Y = 0;
            baseRect.Height = Pos.Height;
            baseRect.Width = Pos.Width;

            Rectangle knobRect = new Rectangle();
            knobRect.X = 0;
            knobRect.Y = Pos.Height;
            knobRect.Height = PosKnob.Height;
            knobRect.Width = PosKnob.Width;

            var renderDevice = SharedResources.RenderDevice!;
            var eset = SharedResources.Eset!;
            var tooltipm = SharedResources.Tooltipm!;

            if (_sl != null)
            {
                _sl.LocalFrame = LocalFrame;
                _sl.SetOffset(LocalOffset);
                _sl.SetClipFromRect(baseRect);
                _sl.SetDestFromRect(Pos);
                renderDevice.Render(_sl);
                _sl.SetClipFromRect(knobRect);
                _sl.SetDestFromRect(PosKnob);
                renderDevice.Render(_sl);
            }

            if (InFocus)
            {
                Int2 topLeft = new Int2();
                Int2 bottomRight = new Int2();

                topLeft.X = Pos.X + LocalFrame.X - LocalOffset.X;
                topLeft.Y = Pos.Y + LocalFrame.Y - LocalOffset.Y;
                bottomRight.X = topLeft.X + Pos.Width;
                bottomRight.Y = topLeft.Y + Pos.Height;

                // Only draw rectangle if it fits in local frame
                bool draw = true;
                if (LocalFrame.Width != 0 &&
                        (topLeft.X < LocalFrame.X || bottomRight.X > (LocalFrame.X + LocalFrame.Width)))
                {
                    draw = false;
                }
                if (LocalFrame.Height != 0 &&
                        (topLeft.Y < LocalFrame.Y || bottomRight.Y > (LocalFrame.Y + LocalFrame.Height)))
                {
                    draw = false;
                }
                if (draw)
                {
                    renderDevice.DrawRectangleCorners(eset.Widgets.SelectionRectCornerSize, topLeft, bottomRight, eset.Widgets.SelectionRectColor);
                }
            }

            if (_pressed || InFocus)
            {
                TooltipData tipData = new TooltipData();
                tipData.AddText(Value.ToString(CultureInfo.InvariantCulture));

                Int2 newMouse = new Int2();
                newMouse.X = PosKnob.X + (PosKnob.Width * 2) + LocalFrame.X - LocalOffset.X;
                newMouse.Y = PosKnob.Y + (PosKnob.Height / 2) + LocalFrame.Y - LocalOffset.Y;
                tooltipm.Push(tipData, newMouse, TooltipData.StyleFloat);
            }
        }

        public override bool GetPrev()
        {
            if (!Enabled) return false;

            Value -= (_maximum - _minimum) / 10;
            if (Value < _minimum)
                Value = _minimum;

            PosKnob.X = Pos.X + ((Value - _minimum) * Pos.Width) / (_maximum - _minimum) - (PosKnob.Width / 2);
            PosKnob.Y = Pos.Y;

            _changedWithoutMouse = true;

            return true;
        }

        public override bool GetNext()
        {
            if (!Enabled) return false;

            Value += (_maximum - _minimum) / 10;
            if (Value > _maximum)
                Value = _maximum;

            PosKnob.X = Pos.X + ((Value - _minimum) * Pos.Width) / (_maximum - _minimum) - (PosKnob.Width / 2);
            PosKnob.Y = Pos.Y;

            _changedWithoutMouse = true;

            return true;
        }
    }
}
