// <自动生成> 对应 C++ 源文件：WidgetListBox.h + WidgetListBox.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// WidgetListBox
    ///
    /// 菜单中使用的列表框控件：支持多行可见项、滚动条、单选/多选、键盘导航与 tooltip。
    /// 持有的 Sprite 与 WidgetScrollBar 资源通过 IDisposable 显式释放，
    /// 对应原始析构函数中的 delete listboxs / delete scrollbar / snd->unload。
    /// </summary>
    public class WidgetListBox : Widget, IDisposable
    {
        /// <summary>
        /// ListBoxItem：列表框中的单个条目（对应 C++ 嵌套类 <c>WidgetListBox::ListBoxItem</c>）。
        /// C++ 以值语义存储于 <c>std::vector</c> 中；C# 以引用类型承载，
        /// 赋值/交换时通过 <see cref="CopyFrom"/> 复现原始拷贝构造/赋值运算符的深拷贝语义。
        /// </summary>
        private class ListBoxItem : IComparable<ListBoxItem>
        {
            public string Value = "";
            public string Tooltip = "";
            public WidgetLabel Label = new WidgetLabel();
            public bool Selected;
            public bool Highlight;

            /// <summary>
            /// 对应 C++ 的 <c>bool operator&lt;(const ListBoxItem&amp; other) const</c>，
            /// 供 <see cref="Sort"/> 调用 <c>std::sort</c> 时使用。
            /// </summary>
            public int CompareTo(ListBoxItem? other)
            {
                if (other == null)
                    return 1;
                return string.Compare(Value, other.Value, StringComparison.Ordinal);
            }

            /// <summary>
            /// 对应 C++ 编译器生成的 <c>ListBoxItem&amp; operator=(const ListBoxItem&amp;)</c>：
            /// 逐成员深拷贝，其中 <see cref="WidgetLabel"/> 通过其 CopyFrom 完成。
            /// </summary>
            public void CopyFrom(ListBoxItem other)
            {
                Value = other.Value;
                Tooltip = other.Tooltip;
                Selected = other.Selected;
                Highlight = other.Highlight;
                Label.CopyFrom(other.Label);
            }
        }

        public const string DefaultFile = "images/menus/buttons/listbox_default.png";
        public const string CharMenuFile = "images/menus/buttons/listbox_char.png";

        public Rectangle PosScroll;
        public bool Pressed;
        public bool MultiSelect;
        public bool CanDeselect;
        public bool CanSelect;
        public int ScrollbarOffset;
        public bool DisableTextTrim;

        private string _fileName;
        private Sprite? _listboxs;
        private int _cursor;
        private bool _hasScrollBar;
        private bool _anySelected;
        private bool _showTooltipForSelected;
        private bool _clicked;
        private List<ListBoxItem> _items = new List<ListBoxItem>();
        private List<Rectangle> _rows = new List<Rectangle>();
        private WidgetScrollBar? _scrollbar;
        private SoundID _soundActivate;

        public WidgetListBox(int height, string fileName)
        {
            _fileName = fileName;
            _listboxs = null;
            _cursor = 0;
            _hasScrollBar = false;
            _anySelected = false;
            _showTooltipForSelected = false;
            _clicked = false;
            _rows = new List<Rectangle>(height);
            for (int i = 0; i < height; i++)
            {
                _rows.Add(default);
            }
            _scrollbar = new WidgetScrollBar(WidgetScrollBar.DefaultFile);
            _soundActivate = 0;
            PosScroll = default;
            Pressed = false;
            MultiSelect = false;
            CanDeselect = true;
            CanSelect = true;
            ScrollbarOffset = 0;
            DisableTextTrim = false;

            // load ListBox images
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
                _listboxs = graphics.CreateSprite();
                Pos.Width = _listboxs.GetGraphicsWidth();
                Pos.Height = _listboxs.GetGraphicsHeight() / 3; // height of one item
                graphics.Unref();
            }

            ScrollType = ScrollVertical;

            if (SharedResources.Eset!.Widgets.SoundActivate.Length != 0)
                _soundActivate = SharedResources.Snd!.Load(SharedResources.Eset.Widgets.SoundActivate, "Widget activate");
        }

        /// <summary>
        /// 对应 C++ 的 <c>~WidgetListBox()</c>，释放 Sprite、ScrollBar 并卸载音效，
        /// 释放顺序与原始析构函数一致（先 listboxs，再 scrollbar，后音效）。
        /// </summary>
        public void Dispose()
        {
            _listboxs?.Dispose();
            _listboxs = null;
            _scrollbar?.Dispose();
            _scrollbar = null;
            SharedResources.Snd!.Unload(_soundActivate);
            GC.SuppressFinalize(this);
        }

        public bool CheckClick()
        {
            InputState inpt = SharedResources.Inpt!;
            return CheckClickAt(inpt.Mouse.X, inpt.Mouse.Y);
        }

        public override void SetPos(int offsetX, int offsetY)
        {
            base.SetPos(offsetX, offsetY);
            Refresh();
        }

        /// <summary>
        /// Sets and releases the "pressed" visual state of the ListBox
        /// If press and release, activate (return true)
        /// </summary>
        public bool CheckClickAt(int x, int y)
        {
            Int2 mouse = new Int2(x, y);

            Refresh();

            InputState inpt = SharedResources.Inpt!;
            if (inpt.UsingMouse())
            {
                CheckTooltip(mouse);
            }
            else if (!inpt.UsingMouse() && _showTooltipForSelected)
            {
                int sel = GetSelected();
                if (sel != -1)
                {
                    int row = sel - _cursor;
                    Int2 tipPos = new Int2();
                    tipPos.X = _rows[row].X + _rows[row].Width / 2;
                    tipPos.Y = _rows[row].Y + _rows[row].Height / 2;
                    CheckTooltip(tipPos);
                }
            }

            // check scroll wheel
            Rectangle scrollArea = new Rectangle();
            scrollArea.X = _rows[0].X;
            scrollArea.Y = _rows[0].Y;
            scrollArea.Width = _rows[0].Width;
            scrollArea.Height = _rows[0].Height * _rows.Count;

            if (Utils.IsWithinRect(scrollArea, mouse))
            {
                inpt.LockScroll = true;
                if (inpt.ScrollUp) ScrollUp();
                if (inpt.ScrollDown) ScrollDown();
            }
            else
            {
                inpt.LockScroll = false;
            }

            // check ScrollBar clicks
            if (_hasScrollBar)
            {
                switch (_scrollbar!.CheckClickAt(mouse.X, mouse.Y))
                {
                    case WidgetScrollBar.ClickUp:
                        ScrollUp();
                        break;
                    case WidgetScrollBar.ClickDown:
                        ScrollDown();
                        break;
                    case WidgetScrollBar.ClickKnob:
                        _cursor = _scrollbar.Value;
                        Refresh();
                        break;
                    default:
                        break;
                }
            }

            if (inpt.UsingMouse())
            {
                if (CanSelect && inpt.Pressing[Input.Main1] && !inpt.Lock[Input.Main1])
                {
                    for (int i = 0; i < _rows.Count; i++)
                    {
                        if (Utils.IsWithinRect(_rows[i], mouse) && i + _cursor < _items.Count && _items[i + _cursor].Value.Length != 0)
                        {
                            inpt.Lock[Input.Main1] = true;

                            SharedResources.Snd!.Play(_soundActivate, "widget_activate", SoundManager.NoPos, !SoundManager.Loop);

                            // deselect other options if multi-select is disabled
                            if (!MultiSelect)
                            {
                                for (int j = 0; j < _items.Count; j++)
                                {
                                    if (j != i + _cursor)
                                        _items[j].Selected = false;
                                }
                            }
                            // toggle selection
                            if (_items[i + _cursor].Selected)
                            {
                                if (CanDeselect)
                                    _items[i + _cursor].Selected = false;
                            }
                            else
                            {
                                _items[i + _cursor].Selected = true;
                            }
                            Refresh();
                            return true;
                        }
                    }
                }
            }

            if (_clicked)
            {
                _clicked = false;
                return true;
            }

            return false;
        }

        private void CheckTooltip(Int2 mouse)
        {
            TooltipData tipData = new TooltipData();
            for (int i = 0; i < _rows.Count; i++)
            {
                if (Utils.IsWithinRect(_rows[i], mouse) && i + _cursor < _items.Count && _items[i + _cursor].Tooltip.Length != 0)
                {
                    tipData.AddText(_items[i + _cursor].Tooltip);
                    break;
                }
            }

            if (!tipData.IsEmpty())
            {
                Int2 newMouse = new Int2(mouse.X + LocalFrame.X - LocalOffset.X, mouse.Y + LocalFrame.Y - LocalOffset.Y);
                SharedResources.Tooltipm!.Push(tipData, newMouse, TooltipData.StyleFloat);
            }
        }

        /// <summary>
        /// Add a new value (with tooltip) to the list
        /// </summary>
        public void Append(string value, string tooltip)
        {
            _items.Add(new ListBoxItem());
            _items[_items.Count - 1].Value = value;
            _items[_items.Count - 1].Tooltip = tooltip;
            Refresh();
        }

        /// <summary>
        /// Set a value (with tooltip) at a specific index
        /// </summary>
        public void Set(uint index, string value, string tooltip)
        {
            if (index >= _items.Count)
            {
                Append(value, tooltip);
                return;
            }

            _items[(int)index].Value = value;
            _items[(int)index].Tooltip = tooltip;
            Refresh();
        }

        /// <summary>
        /// Remove a value from the list
        /// </summary>
        public void Remove(int index)
        {
            _items.RemoveAt(index);
            ScrollUp();
            Refresh();
        }

        /// <summary>
        /// Clear the list
        /// </summary>
        public void Clear()
        {
            _items.Clear();
            _cursor = 0;
            Refresh();
        }

        /// <summary>
        /// Move an item up on the list
        /// </summary>
        public void ShiftUp()
        {
            _anySelected = false;
            if (_items.Count != 0 && !_items[0].Selected)
            {
                for (int i = 1; i < _items.Count; i++)
                {
                    if (_items[i].Selected)
                    {
                        _anySelected = true;
                        ListBoxItem tmpItem = new ListBoxItem();
                        tmpItem.CopyFrom(_items[i]);

                        _items[i].CopyFrom(_items[i - 1]);
                        _items[i - 1].CopyFrom(tmpItem);
                    }
                }
                if (_anySelected)
                {
                    ScrollUp();
                }
            }
        }

        /// <summary>
        /// Move an item down on the list
        /// </summary>
        public void ShiftDown()
        {
            _anySelected = false;
            if (_items.Count != 0 && !_items[_items.Count - 1].Selected)
            {
                for (int i = _items.Count - 2; i >= 0; i--)
                {
                    if (_items[i].Selected)
                    {
                        _anySelected = true;
                        ListBoxItem tmpItem = new ListBoxItem();
                        tmpItem.CopyFrom(_items[i]);

                        _items[i].CopyFrom(_items[i + 1]);
                        _items[i + 1].CopyFrom(tmpItem);
                    }
                }
                if (_anySelected)
                {
                    ScrollDown();
                }
            }
        }

        public int GetSelected()
        {
            // return the first selected value
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].Selected) return i;
            }
            return -1; // nothing is selected
        }

        public string GetValue()
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].Selected) return _items[i].Value;
            }
            return "";
        }

        /// <summary>
        /// Get the item name at a specific index
        /// </summary>
        public string GetValue(int index)
        {
            if (_items.Count == 0)
                return "";

            return _items[index].Value;
        }

        /// <summary>
        /// Get the item tooltip at a specific index
        /// </summary>
        public string GetTooltip(int index)
        {
            if (_items.Count == 0)
                return "";

            return _items[index].Tooltip;
        }

        /// <summary>
        /// Get the amount of ListBox items
        /// </summary>
        public int Size => _items.Count;

        /// <summary>
        /// Shift the viewing area up
        /// </summary>
        public void ScrollUp()
        {
            if (_cursor > 0)
                _cursor -= 1;
            Refresh();
        }

        /// <summary>
        /// Shift the viewing area down
        /// </summary>
        public void ScrollDown()
        {
            if (_cursor + _rows.Count < _items.Count)
                _cursor += 1;
            Refresh();
        }

        public override void Render()
        {
            Rectangle src = new Rectangle();
            src.X = 0;
            src.Width = Pos.Width;
            src.Height = Pos.Height;

            if (_listboxs != null)
            {
                _listboxs.LocalFrame = LocalFrame;
                _listboxs.SetOffset(LocalOffset);
            }

            for (int i = 0; i < _rows.Count; i++)
            {
                if (i == 0)
                    src.Y = 0;
                else if (i == _rows.Count - 1)
                    src.Y = Pos.Height * 2;
                else
                    src.Y = Pos.Height;

                if (_listboxs != null)
                {
                    _listboxs.SetClipFromRect(src);
                    _listboxs.SetDestFromRect(_rows[i]);
                    SharedResources.RenderDevice!.Render(_listboxs);
                }

                if (i + _cursor < _items.Count)
                {
                    _items[i + _cursor].Label.LocalFrame = LocalFrame;
                    _items[i + _cursor].Label.LocalOffset = LocalOffset;
                    _items[i + _cursor].Label.Render();
                }
            }

            if (InFocus)
            {
                Int2 topLeft = new Int2();
                Int2 bottomRight = new Int2();

                if (_rows.Count != 0)
                {
                    topLeft.X = _rows[0].X + LocalFrame.X - LocalOffset.X;
                    topLeft.Y = _rows[0].Y + LocalFrame.Y - LocalOffset.Y;
                    bottomRight.X = _rows[_rows.Count - 1].X + Pos.Width + LocalFrame.X - LocalOffset.X;
                    bottomRight.Y = _rows[_rows.Count - 1].Y + Pos.Height + LocalFrame.Y - LocalOffset.Y;
                }

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
                    SharedResources.RenderDevice!.DrawRectangleCorners(SharedResources.Eset!.Widgets.SelectionRectCornerSize, topLeft, bottomRight, SharedResources.Eset.Widgets.SelectionRectColor);
                }
            }

            if (_hasScrollBar)
            {
                _scrollbar!.LocalFrame = LocalFrame;
                _scrollbar.LocalOffset = LocalOffset;
                _scrollbar.Render();
            }
        }

        /// <summary>
        /// Create the text buffer
        /// Also, toggle the scrollbar based on the size of the list
        /// </summary>
        public void Refresh()
        {
            int rightMargin = 0;
            int padding = SharedResources.Font!.GetFontHeight();

            // Update the scrollbar
            if (_items.Count > _rows.Count)
            {
                _hasScrollBar = true;

                PosScroll.Width = _scrollbar!.GetBounds().Width;
                PosScroll.Height = (Pos.Height * _rows.Count) - (ScrollbarOffset * 2);

                PosScroll.X = Pos.X + Pos.Width - PosScroll.Width - ScrollbarOffset;
                PosScroll.Y = Pos.Y + ScrollbarOffset;

                _scrollbar.Refresh(PosScroll.X, PosScroll.Y, PosScroll.Height, _cursor, _items.Count - _rows.Count);

                rightMargin = PosScroll.Width + SharedResources.Eset!.Widgets.ListboxTextMargin.Y;
            }
            else
            {
                _hasScrollBar = false;
                rightMargin = SharedResources.Eset!.Widgets.ListboxTextMargin.Y;
            }

            // cache all item text
            for (int i = 0; i < _items.Count; ++i)
            {
                _items[i].Label.SetVAlign(LabelInfo.ValignCenter);
                if (DisableTextTrim)
                    _items[i].Label.SetText(_items[i].Value);
                else
                    _items[i].Label.SetText(SharedResources.Font!.TrimTextToWidth(_items[i].Value, Pos.Width - rightMargin - padding, FontEngine.UseEllipsis, 0));

                _items[i].Label.SetHidden(i < _cursor || i >= _cursor + _rows.Count);
            }

            // Update each row's hitbox and label
            for (int i = 0; i < _rows.Count; i++)
            {
                Rectangle row = _rows[i];
                row.X = Pos.X;
                row.Y = (Pos.Height * i) + Pos.Y;
                if (_hasScrollBar)
                {
                    row.Width = Pos.Width - PosScroll.Width;
                }
                else
                {
                    row.Width = Pos.Width;
                }
                row.Height = Pos.Height;
                _rows[i] = row;


                if (i + _cursor < _items.Count)
                {
                    _items[i + _cursor].Label.SetPos(_rows[i].X + SharedResources.Eset!.Widgets.ListboxTextMargin.X, _rows[i].Y + (_rows[i].Height / 2));
                    if (CanSelect)
                    {
                        if (_items[i + _cursor].Selected)
                        {
                            _items[i + _cursor].Label.SetColor(SharedResources.Font!.GetColor(FontEngine.ColorWidgetNormal));
                        }
                        else
                        {
                            _items[i + _cursor].Label.SetColor(SharedResources.Font!.GetColor(FontEngine.ColorWidgetDisabled));
                        }
                    }
                    else
                    {
                        if (_items[i + _cursor].Highlight)
                        {
                            _items[i + _cursor].Label.SetColor(SharedResources.Font!.GetColor(FontEngine.ColorWidgetNormal));
                        }
                        else
                        {
                            _items[i + _cursor].Label.SetColor(SharedResources.Font!.GetColor(FontEngine.ColorWidgetDisabled));
                        }
                    }
                }
            }
        }

        public override bool GetNext()
        {
            if (CanSelect)
            {
                if (_items.Count < 1)
                    return false;

                int sel = GetSelected();
                if (sel == -1)
                    sel = _cursor - 1;

                for (int i = 0; i < _items.Count; ++i)
                {
                    _items[i].Selected = false;
                }

                if (sel == _items.Count - 1)
                {
                    _items[0].Selected = true;
                    while (GetSelected() < _cursor)
                    {
                        ScrollUp();
                    }
                }
                else
                {
                    _items[sel + 1].Selected = true;
                    while (GetSelected() > _cursor + _rows.Count - 1)
                    {
                        ScrollDown();
                    }
                }

                _clicked = true;
            }
            else
            {
                ScrollDown();
            }

            return true;
        }

        public override bool GetPrev()
        {
            if (CanSelect)
            {
                if (_items.Count < 1)
                    return false;

                int sel = GetSelected();
                if (sel == -1)
                    sel = _cursor;

                for (int i = 0; i < _items.Count; ++i)
                {
                    _items[i].Selected = false;
                }

                if (sel == 0)
                {
                    _items[_items.Count - 1].Selected = true;
                    while (GetSelected() > _cursor + _rows.Count - 1)
                    {
                        ScrollDown();
                    }
                }
                else
                {
                    _items[sel - 1].Selected = true;
                    while (GetSelected() < _cursor)
                    {
                        ScrollUp();
                    }
                }

                _clicked = true;
            }
            else
            {
                ScrollUp();
            }

            return true;
        }

        public override void Defocus()
        {
            base.Defocus();

            int sel = GetSelected();
            if (!CanSelect && sel != -1)
            {
                _items[sel].Selected = false;
            }
        }

        public override void Activate()
        {
            InputState inpt = SharedResources.Inpt!;
            if (!inpt.UsingMouse())
            {
                _showTooltipForSelected = !_showTooltipForSelected;
            }
        }

        public void Select(int index)
        {
            if (_items.Count == 0)
                return;

            int sel = GetSelected();
            if (!MultiSelect && sel != -1)
            {
                _items[sel].Selected = false;
            }
            _items[index].Selected = true;
        }

        public bool IsSelected(int index)
        {
            if (_items.Count == 0)
                return false;

            return _items[index].Selected;
        }

        /// <summary>
        /// Change the number of visible rows
        /// </summary>
        public void SetHeight(int newSize)
        {
            if (newSize < 2)
                newSize = 2;

            _rows.Clear();

            for (int i = 0; i < newSize; i++)
            {
                _rows.Add(default);
            }

            Refresh();
        }

        public void SetRowHighlight(uint index, bool highlight)
        {
            if (CanSelect || index >= _items.Count)
                return;

            _items[(int)index].Highlight = highlight;
        }

        public void Sort()
        {
            _items.Sort();
        }
    }
}
