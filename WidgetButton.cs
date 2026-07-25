// <自动生成> 对应 C++ 源文件：WidgetButton.h + WidgetButton.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// WidgetButton
    ///
    /// 菜单中使用的按钮控件：支持鼠标/键盘点击、悬停与禁用状态，
    /// 并通过 Sprite 四行 clip 切换 normal/pressed/hover/disabled 视觉。
    /// 持有的 <see cref="Sprite"/> 缓存资源通过 <see cref="IDisposable"/> 显式释放，
    /// 对应原始析构函数中的 delete buttons 与 snd->unload。
    /// </summary>
    public class WidgetButton : Widget, IDisposable
    {
        public const string DefaultFile = "images/menus/buttons/button_default.png";
        public const string NoFile = "_NO_FILE_";
        public const string CloseFile = "images/menus/buttons/button_x.png";
        public const string DirLeftFile = "images/menus/buttons/left.png";
        public const string DirRightFile = "images/menus/buttons/right.png";
        public const string DirUpFile = "images/menus/buttons/up.png";
        public const string DirDownFile = "images/menus/buttons/down.png";
        public const string UpgradeStatFile = "images/menus/buttons/upgrade.png";
        public const string UpgradePowerFile = "images/menus/buttons/button_plus.png";
        public const string ConfigMenuFile = "images/menus/buttons/button_config.png";
        public const string SortItemsFile = "images/menus/buttons/button_sort.png";

        public const int ButtonNormal = 0;
        public const int ButtonPressed = 1;
        public const int ButtonHover = 2;
        public const int ButtonDisabled = 3;

        public string Tooltip;
        public bool Enabled;
        public bool Pressed;
        public bool Hover;

        private readonly string _fileName;
        private Sprite? _buttons;
        private readonly WidgetLabel _wlabel;
        private bool _activated;
        private string _label;
        private Color _textColorNormal;
        private Color _textColorPressed;
        private Color _textColorHover;
        private Color _textColorDisabled;
        private SoundID _soundActivate;

        public WidgetButton(string fileName)
        {
            _fileName = fileName;
            _buttons = null;
            _wlabel = new WidgetLabel();
            _activated = false;
            _label = "";
            _soundActivate = 0;
            Tooltip = "";
            Enabled = true;
            Pressed = false;
            Hover = false;

            LoadArt();

            _textColorNormal = SharedResources.Font!.GetColor(FontEngine.ColorWidgetNormal);
            _textColorPressed = _textColorNormal;
            _textColorHover = _textColorNormal;
            _textColorDisabled = SharedResources.Font.GetColor(FontEngine.ColorWidgetDisabled);

            if (SharedResources.Eset!.Widgets.SoundActivate.Length != 0)
                _soundActivate = SharedResources.Snd!.Load(SharedResources.Eset.Widgets.SoundActivate, "Widget activate");
        }

        /// <summary>
        /// 对应 C++ 的 <c>~WidgetButton()</c>，释放缓存的 Sprite 并卸载音效，
        /// 释放顺序与原始析构函数一致（先 Sprite，后音效；随后成员 wlabel 析构）。
        /// </summary>
        public void Dispose()
        {
            _buttons?.Dispose();
            _buttons = null;
            SharedResources.Snd!.Unload(_soundActivate);
            _wlabel.Dispose();
            GC.SuppressFinalize(this);
        }

        public override void Activate()
        {
            Pressed = true;
            _activated = true;
        }

        public override void SetPos(int offsetX, int offsetY)
        {
            base.SetPos(offsetX, offsetY);
            Refresh();
        }

        public void SetLabel(string s)
        {
            _label = s;
            Refresh();

            if (_buttons == null)
            {
                Pos.Width = _wlabel.GetBounds().Width;
                Pos.Height = _wlabel.GetBounds().Height;
                Refresh();
                Pos = _wlabel.GetBounds();
            }
        }

        public void SetTextColor(int state, Color c)
        {
            if (state == ButtonNormal)
                _textColorNormal = c;
            else if (state == ButtonPressed)
                _textColorPressed = c;
            else if (state == ButtonHover)
                _textColorHover = c;
            else if (state == ButtonDisabled)
                _textColorDisabled = c;
        }

        public void SetTextFont(string font)
        {
            _wlabel.SetFont(font);
        }

        private void LoadArt()
        {
            if (_fileName == NoFile)
                return;

            // load button images
            Image? graphics = null;
            if (_fileName != DefaultFile)
            {
                graphics = SharedResources.RenderDevice!.LoadImage(_fileName, RenderDevice.ErrorNormal);
            }
            if (graphics == null)
            {
                graphics = SharedResources.RenderDevice!.LoadImage(DefaultFile, RenderDevice.ErrorExit);
            }
            if (graphics != null)
            {
                _buttons = graphics.CreateSprite();
                Pos.Width = _buttons.GetGraphicsWidth();
                Pos.Height = _buttons.GetGraphicsHeight() / 4; // height of one button
                _buttons.SetClip(0, 0, Pos.Width, Pos.Height);
                graphics.Unref();
            }
        }

        public bool CheckClick()
        {
            InputState inpt = SharedResources.Inpt!;
            return CheckClickAt(inpt.Mouse.X, inpt.Mouse.Y);
        }

        /// <summary>
        /// 设置并释放按钮的"按下"视觉状态。
        /// 若完成按下并释放，则激活（返回 true）。
        /// </summary>
        public bool CheckClickAt(int x, int y)
        {
            EnableTablistNav = Enabled;

            Int2 mouse = new Int2(x, y);

            CheckTooltip(mouse);

            // Change the hover state
            Hover = Utils.IsWithinRect(Pos, mouse) && SharedResources.Inpt!.UsingMouse();

            // disabled buttons can't be clicked;
            if (!Enabled) return false;

            InputState inpt = SharedResources.Inpt!;

            // main button already in use, new click not allowed
            if (inpt.Lock[Input.Main1]) return false;
            if (!inpt.UsingMouse() && inpt.Lock[Input.Accept]) return false;

            // main click released, so the button state goes back to unpressed
            if (Pressed && !inpt.Lock[Input.Main1] && (!inpt.Lock[Input.Accept] || inpt.UsingMouse()) && (Utils.IsWithinRect(Pos, mouse) || _activated))
            {
                _activated = false;
                Pressed = false;
                SharedResources.Snd!.Play(_soundActivate, "widget_activate", SoundManager.NoPos, !SoundManager.Loop);
                return true;
            }

            Pressed = false;

            // detect new click
            if (inpt.Pressing[Input.Main1])
            {
                if (Utils.IsWithinRect(Pos, mouse))
                {
                    inpt.Lock[Input.Main1] = true;
                    Pressed = true;
                }
            }
            return false;
        }

        public override void Render()
        {
            // the "button" surface contains button variations.
            // choose which variation to display.
            int y;
            if (!Enabled)
            {
                y = ButtonDisabled * Pos.Height;
                _wlabel.SetColor(_textColorDisabled);
            }
            else if (Pressed)
            {
                y = ButtonPressed * Pos.Height;
                _wlabel.SetColor(_textColorPressed);
            }
            else if (Hover || InFocus)
            {
                y = ButtonHover * Pos.Height;
                _wlabel.SetColor(_textColorHover);
            }
            else
            {
                y = ButtonNormal * Pos.Height;
                _wlabel.SetColor(_textColorNormal);
            }

            if (_buttons != null)
            {
                _buttons.LocalFrame = LocalFrame;
                _buttons.SetOffset(LocalOffset);

                Rectangle clip = _buttons.GetClip();
                clip.Y = y;

                _buttons.SetClipFromRect(clip);
                _buttons.SetDestFromRect(Pos);

                SharedResources.RenderDevice!.Render(_buttons);
            }

            // render label
            _wlabel.LocalFrame = LocalFrame;
            _wlabel.LocalOffset = LocalOffset;
            _wlabel.Render();
        }

        /// <summary>
        /// 创建文本缓冲。
        /// </summary>
        public void Refresh()
        {
            if (_label != "")
            {
                if (_buttons != null)
                {
                    _wlabel.SetPos(Pos.X + (Pos.Width / 2), Pos.Y + (Pos.Height / 2));
                    _wlabel.SetJustify(FontEngine.JustifyCenter);
                    _wlabel.SetVAlign(LabelInfo.ValignCenter);
                }
                else
                {
                    _wlabel.SetPos(Pos.X, Pos.Y);
                    _wlabel.SetJustify(FontEngine.JustifyLeft);
                    _wlabel.SetVAlign(LabelInfo.ValignTop);
                }
                _wlabel.SetText(_label);

                if (Enabled)
                    _wlabel.SetColor(SharedResources.Font!.GetColor(FontEngine.ColorWidgetNormal));
                else
                    _wlabel.SetColor(SharedResources.Font!.GetColor(FontEngine.ColorWidgetDisabled));
            }
        }

        private void CheckTooltip(Int2 mouse)
        {
            InputState inpt = SharedResources.Inpt!;
            if (inpt.UsingMouse() && Utils.IsWithinRect(Pos, mouse) && Tooltip != "")
            {
                TooltipData tipData = new TooltipData();
                tipData.AddText(Tooltip);
                Int2 newMouse = new Int2(mouse.X + LocalFrame.X - LocalOffset.X, mouse.Y + LocalFrame.Y - LocalOffset.Y);
                SharedResources.Tooltipm!.Push(tipData, newMouse, TooltipData.StyleFloat);
            }
        }
    }
}
