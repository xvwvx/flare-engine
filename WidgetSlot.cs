// <自动生成> 对应 C++ 源文件：WidgetSlot.h + WidgetSlot.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// WidgetSlot
    ///
    /// 菜单中用于显示图标、数量、快捷键提示及高亮/禁用状态的槽位控件。
    /// 持有的 Sprite 缓存资源通过 IDisposable 显式释放，对应原始析构函数中的 delete 调用。
    /// </summary>
    public class WidgetSlot : Widget, IDisposable
    {
        public const int NoClick = 0;
        public const int Drag = 1;
        public const int ActivateResult = 2;

        public const int HighlightNormal = 0;
        public const int HighlightPowerMenu = 1;

        public const int NoIcon = -1;
        public const int NoOverlay = -1;

        /// <summary>对应 C++ 公有字段 <c>bool enabled;</c>。</summary>
        public bool Enabled;
        /// <summary>对应 C++ 公有字段 <c>bool continuous;</c>。</summary>
        public bool Continuous;
        /// <summary>对应 C++ 公有字段 <c>bool visible;</c>。</summary>
        public bool Visible;
        /// <summary>对应 C++ 公有字段 <c>float cooldown;</c>（归一化冷却进度）。</summary>
        public float Cooldown;
        /// <summary>对应 C++ 公有字段 <c>bool highlight;</c>。</summary>
        public bool Highlight;
        /// <summary>对应 C++ 公有字段 <c>bool show_disabled_overlay;</c>。</summary>
        public bool ShowDisabledOverlay;
        /// <summary>对应 C++ 公有字段 <c>bool show_colorblind_highlight;</c>。</summary>
        public bool ShowColorblindHighlight;

        private Sprite? _slotSelected;
        private Sprite? _slotHighlight;
        private Sprite? _slotDisabled;
        private Sprite? _labelAmountBg;
        private Sprite? _labelHotkeyBg;

        private WidgetLabel _labelAmount;
        private WidgetLabel _labelHotkey;
        private WidgetLabel _labelColorblindHighlight;

        private int _iconId;
        private int _overlayId;
        private int _amount;
        private int _maxAmount;
        private string _amountStr;
        private int _hotkey;
        private bool _activated;

        public WidgetSlot(int iconId, int highlightType)
        {
            _slotSelected = null;
            _slotHighlight = null;
            _slotDisabled = null;
            _labelAmountBg = null;
            _labelHotkeyBg = null;
            _iconId = iconId;
            _overlayId = NoIcon;
            _amount = 1;
            _maxAmount = 1;
            _amountStr = "";
            _hotkey = -1;
            _activated = false;
            Enabled = true;
            Continuous = false;
            Visible = true;
            Cooldown = 1;
            Highlight = false;
            ShowDisabledOverlay = true;
            ShowColorblindHighlight = false;

            _labelAmount = new WidgetLabel();
            _labelHotkey = new WidgetLabel();
            _labelColorblindHighlight = new WidgetLabel();

            var eset = SharedResources.Eset!;
            var font = SharedResources.Font!;

            _labelAmount.SetFromLabelInfo(eset.Widgets.SlotQuantityLabel);
            _labelAmount.SetColor(eset.Widgets.SlotQuantityColor);
            _labelHotkey.SetFromLabelInfo(eset.Widgets.SlotHotkeyLabel);
            _labelHotkey.SetColor(eset.Widgets.SlotHotkeyColor);

            _labelColorblindHighlight.SetText("*");
            _labelColorblindHighlight.SetColor(font.GetColor(FontEngine.ColorMenuNormal));

            // in case the hotkey string is long (we only have a fixed set of short keynames), keep the label width to the icon size
            _labelHotkey.SetMaxWidth(eset.Resolutions.IconSize);

            Pos.X = Pos.Y = 0;

            Rectangle src = default;
            src.X = src.Y = 0;

            Pos.Width = eset.Resolutions.IconSize;
            Pos.Height = eset.Resolutions.IconSize;
            src.Width = src.Height = eset.Resolutions.IconSize;

            RenderDevice renderDevice = SharedResources.RenderDevice!;
            Image? graphics;
            graphics = renderDevice.LoadImage("images/menus/slot_selected.png", RenderDevice.ErrorNormal);
            if (graphics != null)
            {
                _slotSelected = graphics.CreateSprite();
                _slotSelected.SetClipFromRect(src);
                graphics.Unref();
            }

            if (highlightType == HighlightPowerMenu)
                graphics = renderDevice.LoadImage("images/menus/powers_unlock.png", RenderDevice.ErrorNormal);
            else // HIGHLIGHT_NORMAL
                graphics = renderDevice.LoadImage("images/menus/attention_glow.png", RenderDevice.ErrorNormal);

            if (graphics != null)
            {
                _slotHighlight = graphics.CreateSprite();
                graphics.Unref();
            }

            graphics = renderDevice.LoadImage("images/menus/disabled.png", RenderDevice.ErrorNormal);
            if (graphics != null)
            {
                _slotDisabled = graphics.CreateSprite();
                graphics.Unref();
            }
        }

        /// <summary>
        /// 对应 C++ 析构函数 <c>~WidgetSlot()</c>：释放全部 Sprite 与 WidgetLabel 资源，
        /// 释放顺序与原始析构函数一致（先显式 delete 的 Sprite，后成员 WidgetLabel 析构）。
        /// </summary>
        public void Dispose()
        {
            _slotSelected?.Dispose();
            _slotSelected = null;
            _slotHighlight?.Dispose();
            _slotHighlight = null;
            _slotDisabled?.Dispose();
            _slotDisabled = null;
            _labelAmountBg?.Dispose();
            _labelAmountBg = null;
            _labelHotkeyBg?.Dispose();
            _labelHotkeyBg = null;
            _labelAmount.Dispose();
            _labelHotkey.Dispose();
            _labelColorblindHighlight.Dispose();
            GC.SuppressFinalize(this);
        }

        public override void SetPos(int offsetX, int offsetY)
        {
            base.SetPos(offsetX, offsetY);

            IconManager icons = SharedResources.Icons!;
            _labelAmount.SetPos(Pos.X + icons.TextOffset.X, Pos.Y + icons.TextOffset.Y);
            _labelHotkey.SetPos(Pos.X + icons.TextOffset.X, Pos.Y + icons.TextOffset.Y);

            if (_labelAmountBg != null)
            {
                ref Rectangle r = ref _labelAmount.GetBounds();
                _labelAmountBg.SetDest(r.X, r.Y);
            }

            if (_labelHotkeyBg != null)
            {
                ref Rectangle r = ref _labelHotkey.GetBounds();
                _labelHotkeyBg.SetDest(r.X, r.Y);
            }
        }

        public override void Activate()
        {
            _activated = true;
        }

        public override void Defocus()
        {
            InFocus = false;
        }

        public override bool GetNext()
        {
            return false;
        }

        public override bool GetPrev()
        {
            return false;
        }

        public int CheckClick()
        {
            InputState inpt = SharedResources.Inpt!;
            return CheckClick(inpt.Mouse.X, inpt.Mouse.Y);
        }

        public int CheckClick(int x, int y)
        {
            InputState inpt = SharedResources.Inpt!;

            if (!Enabled)
            {
                _activated = false;
                return NoClick;
            }

            Int2 mouse = new Int2(x, y);
            bool mouseInRect = Utils.IsWithinRect(Pos, mouse);

            if (mouseInRect && inpt.Pressing[Input.Main1] && !inpt.Lock[Input.Main1])
            {
                inpt.Lock[Input.Main1] = true;
                return Drag;
            }
            else if (mouseInRect && inpt.Pressing[Input.Main2] && !inpt.Lock[Input.Main2])
            {
                inpt.Lock[Input.Main2] = true;
                return ActivateResult;
            }
            else if (InFocus && inpt.Pressing[Input.MenuActivate] && !inpt.Lock[Input.MenuActivate])
            {
                inpt.Lock[Input.MenuActivate] = true;
                return ActivateResult;
            }
            else if (_activated)
            {
                // activate() was called
                _activated = false;
                return Drag;
            }

            if (Continuous)
            {
                bool continuousMouse = mouseInRect && (inpt.Lock[Input.Main2] || inpt.TouchLocked);
                bool continuousButton = InFocus && inpt.Lock[Input.MenuActivate];

                if (continuousMouse || continuousButton)
                {
                    return ActivateResult;
                }
            }

            return NoClick;
        }

        public int GetIcon()
        {
            return _iconId;
        }

        public void SetIcon(int iconId, int overlayId)
        {
            _iconId = iconId;
            _overlayId = overlayId;
        }

        public void SetAmount(int amount, int maxAmount)
        {
            _amount = amount;
            _maxAmount = maxAmount;

            _amountStr = Utils.AbbreviateKilo(_amount);

            var eset = SharedResources.Eset!;
            var icons = SharedResources.Icons!;
            var renderDevice = SharedResources.RenderDevice!;

            if ((_amount > 1 || _maxAmount > 1) && !eset.Widgets.SlotQuantityLabel.Hidden)
            {
                _labelAmount.SetPos(Pos.X + icons.TextOffset.X, Pos.Y + icons.TextOffset.Y);
                _labelAmount.SetText(_amountStr);
                _labelAmount.LocalFrame = LocalFrame;
                _labelAmount.LocalOffset = LocalOffset;

                ref Rectangle r = ref _labelAmount.GetBounds();
                if (_labelAmountBg == null || _labelAmountBg.GetGraphicsWidth() != r.Width || _labelAmountBg.GetGraphicsHeight() != r.Height)
                {
                    if (_labelAmountBg != null)
                    {
                        _labelAmountBg.Dispose();
                        _labelAmountBg = null;
                    }

                    if (eset.Widgets.SlotQuantityBgColor.A != 0)
                    {
                        Image? temp = renderDevice.CreateImage(r.Width, r.Height);
                        if (temp != null)
                        {
                            temp.FillWithColor(eset.Widgets.SlotQuantityBgColor);
                            _labelAmountBg = temp.CreateSprite();
                            temp.Unref();
                        }
                    }

                    if (_labelAmountBg != null)
                    {
                        _labelAmountBg.SetDest(r.X, r.Y);
                    }
                }
            }
        }

        public void SetHotkey(int key)
        {
            _hotkey = key;

            var eset = SharedResources.Eset!;
            var icons = SharedResources.Icons!;
            var inpt = SharedResources.Inpt!;
            var renderDevice = SharedResources.RenderDevice!;

            if (_hotkey != -1 && !eset.Widgets.SlotHotkeyLabel.Hidden)
            {
                _labelHotkey.SetPos(Pos.X + icons.TextOffset.X, Pos.Y + icons.TextOffset.Y);
                _labelHotkey.SetText(inpt.GetBindingString(_hotkey, InputState.GetShortString));
                _labelHotkey.LocalFrame = LocalFrame;
                _labelHotkey.LocalOffset = LocalOffset;

                ref Rectangle r = ref _labelHotkey.GetBounds();
                if (_labelHotkeyBg == null || _labelHotkeyBg.GetGraphicsWidth() != r.Width || _labelHotkeyBg.GetGraphicsHeight() != r.Height)
                {
                    if (_labelHotkeyBg != null)
                    {
                        _labelHotkeyBg.Dispose();
                        _labelHotkeyBg = null;
                    }

                    if (eset.Widgets.SlotHotkeyBgColor.A != 0)
                    {
                        Image? temp = renderDevice.CreateImage(r.Width, r.Height);
                        if (temp != null)
                        {
                            temp.FillWithColor(eset.Widgets.SlotHotkeyBgColor);
                            _labelHotkeyBg = temp.CreateSprite();
                            temp.Unref();
                        }
                    }

                    if (_labelHotkeyBg != null)
                    {
                        _labelHotkeyBg.SetDest(r.X, r.Y);
                    }
                }
            }
        }

        public override void Render()
        {
            if (!Visible) return;

            Rectangle src = default;

            EngineSettings eset = SharedResources.Eset!;
            IconManager? icons = SharedResources.Icons;
            InputState inpt = SharedResources.Inpt!;
            RenderDevice renderDevice = SharedResources.RenderDevice!;
            Settings settings = SharedResources.Settings!;

            // icon/overlay/quantity
            if (_iconId != -1 && icons != null)
            {
                icons.SetIcon(_iconId, new Int2(Pos.X, Pos.Y));
                icons.Render();

                if (_overlayId != -1)
                {
                    icons.SetIcon(_overlayId, new Int2(Pos.X, Pos.Y));
                    icons.Render();
                }

                if (_amount > 1 || _maxAmount > 1)
                {
                    if (_labelAmountBg != null)
                        renderDevice.Render(_labelAmountBg);
                    _labelAmount.Render();
                }
            }

            // hotkey hint
            if (_hotkey != -1)
            {
                // reload the hotkey label if keybindings have changed
                if (inpt.RefreshHotkeys)
                    SetHotkey(_hotkey);

                if (_labelHotkeyBg != null)
                    renderDevice.Render(_labelHotkeyBg);
                _labelHotkey.Render();
            }

            // disabled/cooldown tint
            if (ShowDisabledOverlay && (!Enabled || Cooldown < 1))
            {
                Rectangle clip = default;
                clip.X = clip.Y = 0;
                clip.Width = clip.Height = eset.Resolutions.IconSize;

                // Wipe from bottom to top
                if (Cooldown > 0)
                {
                    clip.Height = (int)((float)eset.Resolutions.IconSize * Cooldown);
                }

                if (_slotDisabled != null && clip.Height > 0)
                {
                    _slotDisabled.SetClipFromRect(clip);
                    _slotDisabled.SetDestFromRect(Pos);
                    renderDevice.Render(_slotDisabled);
                }
            }

            // matching/attention highlight
            if (Highlight)
            {
                if (_slotHighlight != null)
                {
                    _slotHighlight.SetDestFromRect(Pos);
                    renderDevice.Render(_slotHighlight);
                }

                // put an asterisk on this icon if in colorblind mode
                if (ShowColorblindHighlight && settings.Colorblind)
                {
                    _labelColorblindHighlight.SetPos(Pos.X + eset.Widgets.ColorblindHighlightOffset.X, Pos.Y + eset.Widgets.ColorblindHighlightOffset.Y);
                    _labelColorblindHighlight.Render();
                }
            }

            // no-mouse navigation highlight
            if (InFocus && _slotSelected != null)
            {
                _slotSelected.SetDestFromRect(Pos);
                renderDevice.Render(_slotSelected);
            }
        }
    }
}
