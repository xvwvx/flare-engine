// 对应 C++ 源：MenuTouchControls.h + MenuTouchControls.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// MenuTouchControls
    ///
    /// 触摸屏虚拟摇杆与动作按钮 overlay：根据触摸位置模拟方向键与 Main2 输入，
    /// 并在屏幕上绘制触控区域指示椭圆。
    /// 析构函数体为空，无额外托管资源；基类 <see cref="Menu"/> 背景精灵仍由 <see cref="Menu.Dispose"/> 释放。
    /// </summary>
    public class MenuTouchControls : Menu
    {
        private int _moveRadius;
        private Int2 _moveCenter;
        private Int2 _moveCenterBase;
        private int _moveAlign;
        private int _moveDeadzone;

        private int _main1Radius;
        private Int2 _main1Center;
        private Int2 _main1CenterBase;
        private int _main1Align;

        private int _main2Radius;
        private Int2 _main2Center;
        private Int2 _main2CenterBase;
        private int _main2Align;

        private int _radiusPadding;

        private float _prevTouchScale;

        public MenuTouchControls()
        {
            _moveCenter = new Int2(0, 0);
            _moveAlign = Utils.AlignBottomLeft;
            _main1Center = new Int2(0, 0);
            _main1Align = Utils.AlignBottomRight;
            _main2Center = new Int2(0, 0);
            _main2Align = Utils.AlignBottomRight;
            _prevTouchScale = SharedResources.Settings!.TouchScale;

            Visible = true;
            Align();
        }

        /// <summary>
        /// 对应 C++ 的 <c>~MenuTouchControls()</c>：析构函数体为空，随后隐式调用基类 <see cref="Menu.Dispose"/>。
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
        }

        private void AlignInput(ref Int2 center, Int2 centerBase, int radius, int align)
        {
            Rectangle inputRect = default;
            inputRect.X = centerBase.X - radius;
            inputRect.Y = centerBase.Y - radius;
            inputRect.Width = radius;
            inputRect.Height = inputRect.Width;

            Utils.AlignToScreenEdge(align, ref inputRect);
            center.X = inputRect.X + radius;
            center.Y = inputRect.Y + radius;
        }

        public override void Align()
        {
            Settings settings = SharedResources.Settings!;

            _moveRadius = (int)(settings.ViewH / 4 * settings.TouchScale);
            _moveCenterBase.X = _moveRadius;
            _moveCenterBase.Y = -(_moveRadius / 2);
            _moveDeadzone = (int)(settings.ViewH / 20 * settings.TouchScale);

            _main1Radius = (int)(settings.ViewH / 6 * settings.TouchScale);
            _main1CenterBase.X = -_main1Radius - (int)((settings.ViewH / 4) * settings.TouchScale);
            _main1CenterBase.Y = (int)((settings.ViewH / 8) * settings.TouchScale * -1);

            _main2Radius = (int)(settings.ViewH / 6 * settings.TouchScale);
            _main2CenterBase.X = 0;
            _main2CenterBase.Y = (int)((settings.ViewH / 6) * settings.TouchScale * -1);

            _radiusPadding = (int)(settings.ViewH / 20 * settings.TouchScale);

            AlignInput(ref _moveCenter, _moveCenterBase, _moveRadius, _moveAlign);
            AlignInput(ref _main1Center, _main1CenterBase, _main1Radius, _main1Align);
            AlignInput(ref _main2Center, _main2CenterBase, _main2Radius, _main2Align);
        }

        public void Logic()
        {
            Settings settings = SharedResources.Settings!;
            InputState inpt = SharedResources.Inpt!;

            if (!Visible || !settings.Touchscreen || settings.MouseMove)
                return;

            // update scaling from settings
            if (settings.TouchScale != _prevTouchScale)
            {
                Align();
                _prevTouchScale = settings.TouchScale;
            }

            inpt.Pressing[Input.Left] = inpt.Pressing[Input.Right] = inpt.Pressing[Input.Up] = inpt.Pressing[Input.Down] = false;
            inpt.Pressing[Input.Main2] = false;

            Vector2 mvCenter = new Vector2((float)_moveCenter.X, (float)_moveCenter.Y);
            Vector2 m2Center = new Vector2((float)_main2Center.X, (float)_main2Center.Y);

            Vector2 mouse = new Vector2((float)inpt.Mouse.X, (float)inpt.Mouse.Y);

            if (inpt.Pressing[Input.Main1] && Utils.IsWithinRadius(mvCenter, (float)_moveRadius, mouse))
            {
                if (inpt.Mouse.X < _moveCenter.X - _moveDeadzone)
                    inpt.Pressing[Input.Left] = true;
                if (inpt.Mouse.X > _moveCenter.X + _moveDeadzone)
                    inpt.Pressing[Input.Right] = true;
                if (inpt.Mouse.Y < _moveCenter.Y - _moveDeadzone)
                    inpt.Pressing[Input.Up] = true;
                if (inpt.Mouse.Y > _moveCenter.Y + _moveDeadzone)
                    inpt.Pressing[Input.Down] = true;
            }

            // checking for MAIN1 is redundant, as the touch event itself triggers that

            if (inpt.Pressing[Input.Main1] && Utils.IsWithinRadius(m2Center, (float)_main2Radius, mouse))
            {
                inpt.Pressing[Input.Main2] = true;
            }
        }

        public bool CheckAllowMain1()
        {
            Settings settings = SharedResources.Settings!;
            InputState inpt = SharedResources.Inpt!;

            if (!Visible || !settings.Touchscreen || settings.MouseMove)
                return false;

            Vector2 m1Center = new Vector2((float)_main1Center.X, (float)_main1Center.Y);
            Vector2 mouse = new Vector2((float)inpt.Mouse.X, (float)inpt.Mouse.Y);

            return Utils.IsWithinRadius(m1Center, (float)_main1Radius, mouse);
        }

        private void RenderInput(Int2 center, int radius, Color color)
        {
            SharedResources.RenderDevice!.DrawEllipse(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius, color, 15);
        }

        public override void Render()
        {
            Settings settings = SharedResources.Settings!;

            if (!Visible || !settings.Touchscreen || settings.MouseMove)
                return;

            Color colorNormal = new Color(255, 255, 255, 255);
            Color colorDeadzone = new Color(127, 127, 127, 255);

            RenderInput(_moveCenter, _moveRadius - _radiusPadding, colorNormal);
            if (_moveDeadzone > 0)
            {
                RenderInput(_moveCenter, _moveDeadzone, colorDeadzone);
            }
            RenderInput(_main1Center, _main1Radius - _radiusPadding, colorNormal);
            RenderInput(_main2Center, _main2Radius - _radiusPadding, colorNormal);
        }
    }
}
