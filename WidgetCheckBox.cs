// <自动生成> 对应 C++ 源文件：WidgetCheckBox.h + WidgetCheckBox.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// WidgetCheckBox
    ///
    /// 菜单中使用的复选框控件：支持鼠标/键盘点击切换选中状态，
    /// 并通过 Sprite 上下半区 clip 切换 checked/unchecked 视觉。
    /// 持有的 Sprite 缓存资源通过 IDisposable 显式释放，对应原始析构函数中的 delete cb。
    /// </summary>
    public class WidgetCheckBox : Widget, IDisposable
    {
        public const string DefaultFile = "images/menus/buttons/checkbox_default.png";

        public bool Enabled;
        public string Tooltip;

        /// <summary>
        /// 对应 C++ 的 <c>bool isChecked() const</c>：无额外逻辑的只读访问器，
        /// 按规则转为自动属性；写入只能通过 <see cref="SetChecked"/> 内部完成。
        /// </summary>
        public bool IsChecked { get; private set; }

        private Sprite? _cb;
        private bool _pressed;
        private bool _activated;

        private SoundID _soundActivate;

        public WidgetCheckBox(string fname)
        {
            Enabled = true;
            Tooltip = "";
            _cb = null;
            IsChecked = false;
            _pressed = false;
            _activated = false;
            _soundActivate = 0;

            var renderDevice = SharedResources.RenderDevice!;
            var eset = SharedResources.Eset!;
            var snd = SharedResources.Snd!;

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
                _cb = graphics.CreateSprite();
                Pos.Width = _cb.GetGraphicsWidth();
                Pos.Height = _cb.GetGraphicsHeight() / 2;
                _cb.SetClip(0, 0, Pos.Width, Pos.Height);
                graphics.Unref();
            }

            if (eset.Widgets.SoundActivate.Length != 0)
                _soundActivate = snd.Load(eset.Widgets.SoundActivate, "Widget activate");
        }

        /// <summary>
        /// 对应 C++ 的 <c>~WidgetCheckBox()</c>，释放缓存的 Sprite 并卸载音效，
        /// 释放顺序与原始析构函数一致（先 Sprite，后音效）。
        /// </summary>
        public void Dispose()
        {
            _cb?.Dispose();
            _cb = null;
            SharedResources.Snd!.Unload(_soundActivate);
            GC.SuppressFinalize(this);
        }

        public override void Activate()
        {
            _pressed = true;
            _activated = true;
        }

        public void SetChecked(bool status)
        {
            IsChecked = status;
            if (_cb != null)
            {
                _cb.SetClip(0, (IsChecked ? Pos.Height : 0), Pos.Width, Pos.Height);
            }
        }

        public bool CheckClick()
        {
            InputState inpt = SharedResources.Inpt!;
            return CheckClickAt(inpt.Mouse.X, inpt.Mouse.Y);
        }

        public bool CheckClickAt(int x, int y)
        {
            EnableTablistNav = Enabled;

            if (!Enabled) return false;

            Int2 mouse = new Int2(x, y);

            CheckTooltip(mouse);

            // main button already in use, new click not allowed
            var inpt = SharedResources.Inpt!;
            var snd = SharedResources.Snd!;

            if (inpt.Lock[Input.Main1]) return false;
            if (!inpt.UsingMouse() && inpt.Lock[Input.Accept]) return false;

            if (_pressed && !inpt.Lock[Input.Main1] && (!inpt.Lock[Input.Accept] || inpt.UsingMouse()) && (Utils.IsWithinRect(Pos, mouse) || _activated)) // this is a button release
            {
                _activated = false;
                _pressed = false;
                SetChecked(!IsChecked);
                snd.Play(_soundActivate, "widget_activate", SoundManager.NoPos, !SoundManager.Loop);
                return true;
            }

            _pressed = false;

            if (inpt.Pressing[Input.Main1])
            {
                if (Utils.IsWithinRect(Pos, mouse))
                {
                    _pressed = true;
                    inpt.Lock[Input.Main1] = true;
                }
            }
            return false;
        }

        public override void Render()
        {
            var renderDevice = SharedResources.RenderDevice!;
            var eset = SharedResources.Eset!;

            if (_cb != null)
            {
                _cb.LocalFrame = LocalFrame;
                _cb.SetOffset(LocalOffset);
                _cb.SetDestFromRect(Pos);
                renderDevice.Render(_cb);
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
        }

        private void CheckTooltip(Int2 mouse)
        {
            var inpt = SharedResources.Inpt!;
            var tooltipm = SharedResources.Tooltipm!;

            TooltipData tipData = new TooltipData();
            if (inpt.UsingMouse() && Utils.IsWithinRect(Pos, mouse) && Tooltip != "")
            {
                tipData.AddText(Tooltip);
            }

            if (!tipData.IsEmpty())
            {
                Int2 newMouse = new Int2(mouse.X + LocalFrame.X - LocalOffset.X, mouse.Y + LocalFrame.Y - LocalOffset.Y);
                tooltipm.Push(tipData, newMouse, TooltipData.StyleFloat);
            }
        }
    }
}
