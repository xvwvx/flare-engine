// <自动生成> 对应 C++ 源文件：WidgetHorizontalList.h + WidgetHorizontalList.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// WidgetHorizontalList
    ///
    /// 菜单中使用的水平列表控件：左右箭头切换当前选中项，可显示文本标签或一组动作按钮。
    /// 持有的 WidgetButton 实例通过 <see cref="IDisposable"/> 显式释放，
    /// 对应原始析构函数中的 clear() + delete button_left/button_right。
    /// </summary>
    public class WidgetHorizontalList : Widget, IDisposable
    {
        private const string DefaultFileLeft = "images/menus/buttons/left.png";
        private const string DefaultFileRight = "images/menus/buttons/right.png";

        /// <summary>
        /// HListItem：水平列表中的单个条目（对应 C++ 嵌套类 <c>WidgetHorizontalList::HListItem</c>）。
        /// </summary>
        private class HListItem
        {
            public string Value = "";
            public string Tooltip = "";
            public WidgetButton? Button;

            public HListItem()
            {
                Button = null;
            }
        }

        private readonly WidgetLabel _label = new WidgetLabel();
        private WidgetButton? _buttonLeft;
        private WidgetButton? _buttonRight;

        private uint _cursor;
        private bool _changedWithoutMouse;
        private bool _actionTriggered;
        private bool _activated;
        private List<HListItem> _listItems = new List<HListItem>();
        private Rectangle _tooltipArea;
        private Int2 _actionButtonSize;

        public bool Enabled;
        public bool HasAction;
        public int MaxVisibleActions;

        public WidgetHorizontalList()
        {
            _buttonLeft = new WidgetButton(DefaultFileLeft);
            _buttonRight = new WidgetButton(DefaultFileRight);
            _cursor = 0;
            _changedWithoutMouse = false;
            _actionTriggered = false;
            _activated = false;
            Enabled = true;
            HasAction = false;
            MaxVisibleActions = 2;

            // we want the dimensions of a regular button, so load a temporary one here
            WidgetButton? temp = new WidgetButton(WidgetButton.DefaultFile);
            if (temp != null)
            {
                _actionButtonSize.X = temp.Pos.Width;
                _actionButtonSize.Y = temp.Pos.Height;
                temp.Dispose();
            }

            Refresh();
        }

        /// <summary>
        /// 对应 C++ 的 <c>~WidgetHorizontalList()</c>：先 clear() 释放列表项按钮，
        /// 再 delete button_left / button_right，顺序与原始析构函数一致。
        /// </summary>
        public void Dispose()
        {
            Clear();
            _buttonLeft?.Dispose();
            _buttonLeft = null;
            _buttonRight?.Dispose();
            _buttonRight = null;
            GC.SuppressFinalize(this);
        }

        public override void SetPos(int offsetX, int offsetY)
        {
            base.SetPos(offsetX, offsetY);
            Refresh();
        }

        public bool CheckClick()
        {
            InputState inpt = SharedResources.Inpt!;
            return CheckClickAt(inpt.Mouse.X, inpt.Mouse.Y);
        }

        public bool CheckClickAt(int x, int y)
        {
            // enable_tablist_nav = enabled;

            Int2 mouse = new Int2(x, y);

            CheckTooltip(mouse);

            if (_buttonLeft!.CheckClickAt(mouse.X, mouse.Y))
            {
                ScrollLeft();
                return true;
            }
            else if (_buttonRight!.CheckClickAt(mouse.X, mouse.Y))
            {
                ScrollRight();
                return true;
            }
            else if (_changedWithoutMouse)
            {
                // getNext() or getPrev() was used to change the slider, so treat it as a "click"
                _changedWithoutMouse = false;
                return true;
            }
            else if (HasAction && _activated)
            {
                _activated = false;
                _listItems[(int)_cursor].Button!.Activate();
            }
            else if (HasAction)
            {
                int start;
                int end;
                GetVisibleButtonRange(out start, out end);
                for (int i = start; i < end; ++i)
                {
                    if (_listItems[i].Button!.CheckClickAt(mouse.X, mouse.Y))
                    {
                        _cursor = (uint)i;
                        _actionTriggered = true;
                        return true;
                    }
                }
            }

            return false;
        }

        public bool CheckAction()
        {
            if (_actionTriggered)
            {
                _actionTriggered = false;
                return true;
            }
            return false;
        }

        public override void Activate()
        {
            _activated = true;
        }

        public override void Render()
        {
            var renderDevice = SharedResources.RenderDevice!;
            var eset = SharedResources.Eset!;

            _buttonLeft!.LocalFrame = LocalFrame;
            _buttonLeft.LocalOffset = LocalOffset;

            _buttonRight!.LocalFrame = LocalFrame;
            _buttonRight.LocalOffset = LocalOffset;

            if (!MultipleActionsVisible())
            {
                _buttonLeft.Render();
                _buttonRight.Render();
            }

            if (HasAction)
            {
                int start;
                int end;
                GetVisibleButtonRange(out start, out end);
                for (int i = start; i < end; ++i)
                {
                    _listItems[i].Button!.LocalFrame = LocalFrame;
                    _listItems[i].Button.LocalOffset = LocalOffset;
                    _listItems[i].Button.Render();
                }
            }
            else
            {
                // render label
                _label.LocalFrame = LocalFrame;
                _label.LocalOffset = LocalOffset;
                _label.Render();
            }

            if (InFocus && (!HasAction || !Enabled))
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
            else if (InFocus && HasAction && Enabled)
            {
                for (int i = 0; i < _listItems.Count; ++i)
                {
                    _listItems[i].Button!.InFocus = (_cursor == (uint)i ? true : false);
                }
            }
            else
            {
                if (HasAction)
                {
                    for (int i = 0; i < _listItems.Count; ++i)
                    {
                        _listItems[i].Button!.InFocus = false;
                    }
                }
            }
        }

        public void Refresh()
        {
            var eset = SharedResources.Eset!;
            var font = SharedResources.Font!;

            ScrollType = (byte)(MultipleActionsVisible() ? ScrollVertical : ScrollHorizontal);

            int contentWidth = eset.Widgets.HorizontalListTextWidth;
            bool isEnabled = !IsEmpty() && Enabled;

            _buttonLeft!.Enabled = isEnabled && !MultipleActionsVisible();
            _buttonRight!.Enabled = isEnabled && !MultipleActionsVisible();

            _buttonLeft.SetPos(Pos.X, Pos.Y);

            if (HasAction)
            {
                contentWidth = _actionButtonSize.X;

                for (int i = 0; i < _listItems.Count; ++i)
                {
                    _listItems[i].Button!.Enabled = isEnabled;

                    int yOffset = Pos.Y + (_buttonLeft.Pos.Height / 2);
                    if (MultipleActionsVisible())
                    {
                        int actionCount = Math.Min(MaxVisibleActions, _listItems.Count);
                        yOffset += (-_actionButtonSize.Y * actionCount / 2) + _actionButtonSize.Y * i;
                    }
                    else
                    {
                        yOffset += (-_actionButtonSize.Y / 2);
                    }

                    _listItems[i].Button.SetPos(Pos.X + _buttonLeft.Pos.Width, yOffset);

                    _listItems[i].Button.SetLabel(_listItems[i].Value);
                    if (_cursor < _listItems.Count)
                    {
                        _listItems[i].Button.Tooltip = _listItems[i].Tooltip;
                    }

                    Pos.Height = Math.Max(_buttonLeft.Pos.Height, _listItems[i].Button.Pos.Height);
                }
            }
            else
            {
                _label.SetText(GetValue());
                _label.SetPos(Pos.X + _buttonLeft.Pos.Width + contentWidth / 2, Pos.Y + _buttonLeft.Pos.Height / 2);
                _label.SetMaxWidth(contentWidth);
                _label.SetJustify(FontEngine.JustifyCenter);
                _label.SetVAlign(LabelInfo.ValignCenter);
                _label.SetColor(isEnabled ? font.GetColor(FontEngine.ColorWidgetNormal) : font.GetColor(FontEngine.ColorWidgetDisabled));

                Pos.Height = Math.Max(_buttonLeft.Pos.Height, _label.GetBounds().Height);

                _tooltipArea.X = Pos.X + _buttonLeft.Pos.Width;
                _tooltipArea.Y = Math.Min(Pos.Y, _label.GetBounds().Y);
                _tooltipArea.Width = contentWidth;
                _tooltipArea.Height = Math.Max(_buttonLeft.Pos.Height, _label.GetBounds().Height);
            }

            _buttonRight.SetPos(Pos.X + _buttonLeft.Pos.Width + contentWidth, Pos.Y);
            Pos.Width = _buttonLeft.Pos.Width + _buttonRight.Pos.Width + contentWidth;
        }

        private void CheckTooltip(Int2 mouse)
        {
            // WidgetButton will handle the tooltip if the action button is enabled
            if (HasAction)
                return;

            if (IsEmpty())
                return;

            var inpt = SharedResources.Inpt!;
            var tooltipm = SharedResources.Tooltipm!;

            if (inpt.UsingMouse() && Utils.IsWithinRect(_tooltipArea, mouse) && _listItems[(int)_cursor].Tooltip.Length != 0)
            {
                TooltipData tipData = new TooltipData();
                tipData.AddText(_listItems[(int)_cursor].Tooltip);
                Int2 newMouse = new Int2(mouse.X + LocalFrame.X - LocalOffset.X, mouse.Y + LocalFrame.Y - LocalOffset.Y);
                tooltipm.Push(tipData, newMouse, TooltipData.StyleFloat);
            }
        }

        public void Append(string value, string tooltip)
        {
            HListItem hli = new HListItem();
            hli.Value = value;
            hli.Tooltip = tooltip;

            // these get deleted when calling clear()
            hli.Button = new WidgetButton(WidgetButton.DefaultFile);

            _listItems.Add(hli);
        }

        public void Clear()
        {
            for (int i = 0; i < _listItems.Count; ++i)
            {
                _listItems[i].Button?.Dispose();
            }
            _listItems.Clear();
            _cursor = 0;
        }

        public string GetValue()
        {
            if (_cursor < GetSize())
            {
                return _listItems[(int)_cursor].Value;
            }

            return "";
        }

        public void SetValue(uint index, string value)
        {
            if (index < GetSize())
            {
                _listItems[(int)index].Value = value;
            }
        }

        public uint GetSelected()
        {
            if (_cursor < GetSize())
                return _cursor;
            else
                return GetSize();
        }

        public uint GetSize()
        {
            return (uint)_listItems.Count;
        }

        public bool IsEmpty()
        {
            return _listItems.Count == 0;
        }

        public void ScrollLeft()
        {
            if (IsEmpty())
                return;

            if (_cursor == 0)
                _cursor = GetSize() - 1;
            else
                _cursor--;

            Refresh();
        }

        public void Select(uint index)
        {
            if (IsEmpty())
                return;

            if (index < GetSize())
                _cursor = index;

            Refresh();
        }

        public void ScrollRight()
        {
            if (IsEmpty())
                return;

            if (_cursor + 1 >= GetSize())
                _cursor = 0;
            else
                _cursor++;

            Refresh();
        }

        public override bool GetPrev()
        {
            if (!IsEmpty() && Enabled)
            {
                ScrollLeft();
                _changedWithoutMouse = true;
            }
            return true;
        }

        public override bool GetNext()
        {
            if (!IsEmpty() && Enabled)
            {
                ScrollRight();
                _changedWithoutMouse = true;
            }
            return true;
        }

        private bool MultipleActionsVisible()
        {
            return HasAction && MaxVisibleActions > 1 && MaxVisibleActions >= _listItems.Count;
        }

        private void GetVisibleButtonRange(out int start, out int end)
        {
            if (!MultipleActionsVisible())
            {
                start = (int)_cursor;
                end = (int)_cursor + 1;
            }
            else
            {
                start = 0;
                end = _listItems.Count;
            }
        }
    }
}
