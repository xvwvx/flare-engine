// <自动生成> 对应 C++ 源文件：WidgetScrollBar.h + WidgetScrollBar.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// WidgetScrollBar
    ///
    /// 菜单中使用的垂直滚动条控件：支持点击上/下按钮步进，以及拖拽滑块（knob）调整数值。
    /// 持有的 Sprite 缓存资源通过 IDisposable 显式释放，对应原始析构函数中的 delete scrollbars/bg。
    /// </summary>
    public class WidgetScrollBar : Widget, IDisposable
    {
        public const string DefaultFile = "images/menus/buttons/scrollbar_default.png";

        public const int ClickNone = 0;
        public const int ClickUp = 1;
        public const int ClickDown = 2;
        public const int ClickKnob = 3;

        private const int GfxPrev = 0;
        private const int GfxPrevPress = 1;
        private const int GfxNext = 2;
        private const int GfxNextPress = 3;
        private const int GfxKnob = 4;
        private const int GfxTotal = 5;

        /// <summary>
        /// 对应 C++ 的 <c>int getValue()</c>：无额外逻辑的只读访问器，
        /// 按规则转为自动属性；写入只能通过 Refresh()/SetKnobPos()/CheckClickAt() 内部完成。
        /// </summary>
        public int Value { get; private set; }

        private string _fileName; // the path to the ScrollBar's atlas

        private Sprite? _scrollbars;

        private int _barHeight;
        private int _maximum;
        private bool _lockMain1;
        private bool _dragging;

        private Sprite? _bg;

        private Rectangle _upToKnob;
        private Rectangle _knobToDown;

        private Rectangle _posUp;
        private Rectangle _posDown;
        private Rectangle _posKnob;
        private bool _pressedUp;
        private bool _pressedDown;
        private bool _pressedKnob;

        private SoundID _soundActivate;

        public WidgetScrollBar(string fileName)
        {
            _fileName = fileName;
            _scrollbars = null;
            Value = 0;
            _barHeight = 0;
            _maximum = 0;
            _lockMain1 = false;
            _dragging = false;
            _bg = null;
            _pressedUp = false;
            _pressedDown = false;
            _pressedKnob = false;
            _soundActivate = 0;

            var renderDevice = SharedResources.RenderDevice!;
            var eset = SharedResources.Eset!;
            var snd = SharedResources.Snd!;

            Image? graphics = null;
            if (_fileName != DefaultFile)
            {
                graphics = renderDevice.LoadImage(_fileName, RenderDevice.ErrorNormal);
            }
            if (graphics == null)
            {
                graphics = renderDevice.LoadImage(DefaultFile, RenderDevice.ErrorExit);
            }
            if (graphics != null)
            {
                _scrollbars = graphics.CreateSprite();
                graphics.Unref();
            }

            if (_scrollbars != null)
            {
                _posUp.Width = _posDown.Width = _posKnob.Width = _scrollbars.GetGraphicsWidth();
                _posUp.Height = _posDown.Height = _posKnob.Height = (_scrollbars.GetGraphicsHeight() / GfxTotal); // height of one button; all buttons are the same size
            }

            if (eset.Widgets.SoundActivate.Length != 0)
                _soundActivate = snd.Load(eset.Widgets.SoundActivate, "Widget activate");
        }

        /// <summary>
        /// 对应 C++ 的 <c>~WidgetScrollBar()</c>，释放缓存的 Sprite 并卸载音效，
        /// 释放顺序与原始析构函数一致（先 scrollbars，再 bg，后音效）。
        /// </summary>
        public void Dispose()
        {
            _scrollbars?.Dispose();
            _scrollbars = null;
            _bg?.Dispose();
            _bg = null;
            SharedResources.Snd!.Unload(_soundActivate);
            GC.SuppressFinalize(this);
        }

        public int CheckClick()
        {
            InputState inpt = SharedResources.Inpt!;
            return CheckClickAt(inpt.Mouse.X, inpt.Mouse.Y);
        }

        /// <summary>
        /// Sets and releases the "pressed" visual state of the ScrollBar
        /// If press and release, activate and return click state
        /// </summary>
        public int CheckClickAt(int x, int y)
        {
            Int2 mouse = new Int2(x, y);
            var inpt = SharedResources.Inpt!;
            var snd = SharedResources.Snd!;

            bool inBounds = Utils.IsWithinRect(GetBounds(), mouse);
            bool inUp = Utils.IsWithinRect(_posUp, mouse) || Utils.IsWithinRect(_upToKnob, mouse);
            bool inDown = Utils.IsWithinRect(_posDown, mouse) || Utils.IsWithinRect(_knobToDown, mouse);
            bool inKnob = Utils.IsWithinRect(_posKnob, mouse);

            // detect new click
            if (inBounds && (!_lockMain1 || _dragging))
            {
                _lockMain1 = false;
                _dragging = false;

                if (inpt.Pressing[Input.Main1])
                {
                    inpt.Lock[Input.Main1] = true;

                    if (inUp && !_pressedKnob)
                    {
                        _pressedUp = true;
                    }
                    else if (inDown && !_pressedKnob)
                    {
                        _pressedDown = true;
                    }
                    else if (inKnob && !_pressedUp && !_pressedDown)
                    {
                        if (!_pressedKnob)
                            snd.Play(_soundActivate, "widget_activate", SoundManager.NoPos, !SoundManager.Loop);

                        _pressedKnob = true;
                        _dragging = true;
                    }
                    else if (_pressedKnob)
                    {
                        _dragging = true;
                    }
                }
            }
            else
            {
                _lockMain1 = inpt.Pressing[Input.Main1];
            }

            int ret = ClickNone;
            // main click released, so the ScrollBar state goes back to unpressed
            if (_pressedUp && !inpt.Pressing[Input.Main1])
            {
                _pressedUp = false;
                if (inUp)
                {
                    // activate upon release
                    ret = ClickUp;
                    snd.Play(_soundActivate, "widget_activate", SoundManager.NoPos, !SoundManager.Loop);
                }
            }
            else if (_pressedDown && !inpt.Pressing[Input.Main1])
            {
                _pressedDown = false;
                if (inDown)
                {
                    // activate upon release
                    ret = ClickDown;
                    snd.Play(_soundActivate, "widget_activate", SoundManager.NoPos, !SoundManager.Loop);
                }
            }
            else if (_pressedKnob && _dragging)
            {
                int tmp = mouse.Y - _posUp.Y - _posUp.Height;

                if (_barHeight < 1) _barHeight = 1;
                Value = (tmp * _maximum) / _barHeight;
                SetKnobPos();

                ret = ClickKnob;
            }

            if (!inpt.Pressing[Input.Main1])
            {
                _dragging = false;
                _pressedKnob = false;
                _pressedUp = false;
                _pressedDown = false;
            }

            return ret;
        }

        private void SetKnobPos()
        {
            if (_maximum < 1) _maximum = 1;
            Value = Math.Max(0, Math.Min(_maximum, Value));
            _posKnob.Y = _posUp.Y + _posUp.Height + (Value * (_barHeight - _posUp.Height) / _maximum);

            _upToKnob.X = _knobToDown.X = _posKnob.X;
            _upToKnob.Width = _knobToDown.Width = _posKnob.Width;
            _upToKnob.Y = _posUp.Y + _posUp.Height;
            _upToKnob.Height = _posKnob.Y - _upToKnob.Y;
            _knobToDown.Y = _posKnob.Y + _posKnob.Height;
            _knobToDown.Height = _posDown.Y - _knobToDown.Y;
        }

        public Rectangle GetBounds()
        {
            Rectangle r = new Rectangle();
            r.X = _posUp.X;
            r.Y = _posUp.Y;
            r.Width = _posUp.Width;
            r.Height = (_posUp.Height * 2) + _barHeight;

            return r;
        }

        public override void Render()
        {
            var renderDevice = SharedResources.RenderDevice!;

            Rectangle srcUp = new Rectangle();
            Rectangle srcDown = new Rectangle();
            Rectangle srcKnob = new Rectangle();

            int gfxSize = _posUp.Height; // all buttons are the same height

            srcUp.X = 0;
            srcUp.Y = (_pressedUp ? gfxSize * GfxPrevPress : gfxSize * GfxPrev);
            srcUp.Width = _posUp.Width;
            srcUp.Height = _posUp.Height;

            srcDown.X = 0;
            srcDown.Y = (_pressedDown ? gfxSize * GfxNextPress : gfxSize * GfxNext);
            srcDown.Width = _posDown.Width;
            srcDown.Height = _posDown.Height;

            srcKnob.X = 0;
            srcKnob.Y = gfxSize * GfxKnob;
            srcKnob.Width = _posKnob.Width;
            srcKnob.Height = _posKnob.Height;

            if (_bg != null)
            {
                _bg.LocalFrame = LocalFrame;
                _bg.SetOffset(LocalOffset);
                _bg.SetDestFromRect(_posUp);
                renderDevice.Render(_bg);
            }
            if (_scrollbars != null)
            {
                _scrollbars.LocalFrame = LocalFrame;
                _scrollbars.SetOffset(LocalOffset);

                _scrollbars.SetClipFromRect(srcUp);
                _scrollbars.SetDestFromRect(_posUp);
                renderDevice.Render(_scrollbars);

                _scrollbars.SetClipFromRect(srcDown);
                _scrollbars.SetDestFromRect(_posDown);
                renderDevice.Render(_scrollbars);

                _scrollbars.SetClipFromRect(srcKnob);
                _scrollbars.SetDestFromRect(_posKnob);
                renderDevice.Render(_scrollbars);
            }
        }

        /// <summary>
        /// Updates the scrollbar's location
        /// </summary>
        public void Refresh(int x, int y, int h, int val, int max)
        {
            Rectangle before = GetBounds();
            _maximum = max;
            Value = val;
            _posUp.X = _posDown.X = _posKnob.X = x;
            _posUp.Y = y;
            _posDown.Y = y + h - _posDown.Height;
            _barHeight = _posDown.Y - (_posUp.Y + _posUp.Height);
            SetKnobPos();

            Rectangle after = GetBounds();
            if (before.Height != after.Height)
            {
                var renderDevice = SharedResources.RenderDevice!;
                var eset = SharedResources.Eset!;

                // create background surface
                if (_bg != null)
                {
                    _bg.Dispose();
                    _bg = null;
                }
                Image? graphics;
                graphics = renderDevice.CreateImage(after.Width, after.Height);
                if (graphics != null)
                {
                    _bg = graphics.CreateSprite();
                    graphics.Unref();
                }

                if (_bg != null)
                {
                    _bg.GetGraphics()!.FillWithColor(eset.Widgets.ScrollbarBgColor);
                }
            }
        }
    }
}
