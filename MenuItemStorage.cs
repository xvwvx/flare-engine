// <自动生成> 对应 C++ 源文件：MenuItemStorage.h + MenuItemStorage.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// MenuItemStorage：带 WidgetSlot 网格/列表布局的菜单物品容器，继承 <see cref="ItemStorage"/>。
    /// 对应 C++ 的 <c>class MenuItemStorage : public ItemStorage</c>。
    /// </summary>
    public class MenuItemStorage : ItemStorage
    {
        /// <summary>对应 C++ protected 字段 <c>Rect grid_area;</c>。</summary>
        protected Rectangle _gridArea;
        /// <summary>对应 C++ protected 字段 <c>Point grid_pos;</c>。</summary>
        protected Int2 _gridPos;
        /// <summary>对应 C++ protected 字段 <c>int nb_cols;</c>。</summary>
        protected int _nbCols;

        /// <summary>对应 C++ 公有字段 <c>std::vector&lt;size_t&gt; slot_type;</c>。</summary>
        public List<int> SlotType;
        /// <summary>对应 C++ 公有字段 <c>int drag_prev_slot;</c>。</summary>
        public int DragPrevSlot;
        /// <summary>对应 C++ 公有字段 <c>std::vector&lt;WidgetSlot*&gt; slots;</c>。</summary>
        public List<WidgetSlot?> Slots;
        /// <summary>对应 C++ 公有字段 <c>WidgetSlot *current_slot;</c>。</summary>
        public WidgetSlot? CurrentSlot;
        /// <summary>对应 C++ 公有字段 <c>bool max_quantity_is_one;</c>。</summary>
        public bool MaxQuantityIsOne;
        /// <summary>对应 C++ 公有字段 <c>bool click_subtracts_item;</c>。</summary>
        public bool ClickSubtractsItem;

        /// <summary>
        /// 对应 C++ 构造函数初始化列表：<c>grid_area(), nb_cols(0), slot_type(), drag_prev_slot(-1),
        /// slots(), current_slot(NULL), max_quantity_is_one(false), click_subtracts_item(true)</c>。
        /// </summary>
        public MenuItemStorage()
        {
            _gridArea = default;
            _nbCols = 0;
            SlotType = [];
            DragPrevSlot = -1;
            Slots = [];
            CurrentSlot = null;
            MaxQuantityIsOne = false;
            ClickSubtractsItem = true;
        }

        /// <summary>
        /// 对应 C++ 析构函数 <c>~MenuItemStorage()</c>：逐槽释放 WidgetSlot，再释放基类 storage。
        /// 基类 <see cref="ItemStorage.Dispose"/> 仅清空 <see cref="ItemStorage.Storage"/> 引用。
        /// </summary>
        public new void Dispose()
        {
            for (int i = 0; i < Slots.Count; ++i)
            {
                Slots[i]?.Dispose();
            }
            Slots.Clear();
            Storage = null;
            GC.SuppressFinalize(this);
        }

        public void InitGrid(int slotNumber, Rectangle area, int nbCols)
        {
            Init(slotNumber);
            _gridArea = area;
            _gridPos.X = area.X;
            _gridPos.Y = area.Y;
            for (int i = 0; i < slotNumber; i++)
            {
                WidgetSlot slot = new WidgetSlot(WidgetSlot.NoIcon, WidgetSlot.HighlightNormal);
                Slots.Add(slot);
            }
            _nbCols = nbCols;
            EngineSettings eset = SharedResources.Eset!;
            for (int i = 0; i < slotNumber; i++)
            {
                Slots[i]!.Pos.X = _gridArea.X + (i % _nbCols * eset.Resolutions.IconSize);
                Slots[i]!.Pos.Y = _gridArea.Y + (i / _nbCols * eset.Resolutions.IconSize);
                Slots[i]!.Pos.Height = Slots[i]!.Pos.Width = eset.Resolutions.IconSize;
                Slots[i]!.SetBasePos(Slots[i]!.Pos.X, Slots[i]!.Pos.Y, Utils.AlignTopLeft);
            }
        }

        public void InitFromList(int slotNumber, List<Rectangle> area, List<int> slotType)
        {
            Init(slotNumber);
            for (int i = 0; i < slotNumber; i++)
            {
                WidgetSlot slot = new WidgetSlot(WidgetSlot.NoIcon, WidgetSlot.HighlightNormal);
                slot.Pos = area[i];
                slot.SetBasePos(slot.Pos.X, slot.Pos.Y, Utils.AlignTopLeft);
                Slots.Add(slot);
            }
            _nbCols = 0;
            SlotType = [.. slotType];
        }

        public void SetPos(int x, int y)
        {
            for (int i = 0; i < Slots.Count; ++i)
            {
                Slots[i]!.SetPos(x, y);
            }
            if (_nbCols > 0)
            {
                _gridArea.X = _gridPos.X + x;
                _gridArea.Y = _gridPos.Y + y;
            }
        }

        public void Render()
        {
            ItemManager items = SharedGameResources.Items!;
            for (int i = 0; i < _slotNumber; i++)
            {
                if (items.IsValid(Storage![i].Item))
                {
                    Slots[i]!.SetIcon(items.Items[Storage[i].Item]!.Icon, items.GetItemIconOverlay(Storage[i].Item));
                    if (MaxQuantityIsOne)
                        Slots[i]!.SetAmount(1, 1);
                    else
                        Slots[i]!.SetAmount(Storage[i].Quantity, items.Items[Storage[i].Item]!.MaxQuantity);
                }
                else
                {
                    Slots[i]!.SetIcon(WidgetSlot.NoIcon, WidgetSlot.NoOverlay);
                }
                Slots[i]!.Render();
            }
        }

        public int SlotOver(Int2 position)
        {
            if (Utils.IsWithinRect(_gridArea, position) && _nbCols > 0)
            {
                return (position.X - _gridArea.X) / Slots[0]!.Pos.Width + (position.Y - _gridArea.Y) / Slots[0]!.Pos.Width * _nbCols;
            }
            else if (_nbCols == 0)
            {
                for (int i = 0; i < Slots.Count; i++)
                {
                    if (Slots[i]!.Visible)
                        if (Utils.IsWithinRect(Slots[i]!.Pos, position)) return i;
                }
            }
            return -1;
        }

        public TooltipData CheckTooltip(Int2 position, StatBlock? stats, int context, bool inputHint)
        {
            TooltipData tip = new TooltipData();
            int slot = SlotOver(position);

            if (slot > -1 && Storage![slot].Item > 0)
            {
                ItemManager items = SharedGameResources.Items!;
                return items.GetTooltip(Storage[slot], stats, context, inputHint);
            }
            return tip;
        }

        public ItemStack Click(Int2 position)
        {
            ItemStack item = new ItemStack();

            DragPrevSlot = SlotOver(position);

            // no selection, so defocus everything
            if (DragPrevSlot == -1)
            {
                for (int i = 0; i < Slots.Count; i++)
                {
                    if (Slots[i]!.InFocus)
                    {
                        Slots[i]!.Defocus();
                    }

                    if (Slots[i] == CurrentSlot)
                    {
                        CurrentSlot = null;
                    }
                }
            }

            if (DragPrevSlot > -1)
            {
                // Clone() needed: C# ItemStack is a class (reference type).
                // C++ value copy means Click returns independent data; C# must explicitly clone.
                item = Storage![DragPrevSlot].Clone();
                InputState inpt = SharedResources.Inpt!;
                if (inpt.Mode == InputState.ModeTouchscreen)
                {
                    if (!Slots[DragPrevSlot]!.InFocus && !item.Empty())
                    {
                        Slots[DragPrevSlot]!.InFocus = true;
                        CurrentSlot = Slots[DragPrevSlot];
                        item.Clear();
                        DragPrevSlot = -1;
                        return item;
                    }
                    else if (item.Empty())
                    {
                        Slots[DragPrevSlot]!.Defocus();
                        CurrentSlot = null;
                    }
                }
                if (!item.Empty())
                {
                    if (item.Quantity > 1 && (!inpt.Pressing[Input.Ctrl] || !inpt.UsingMouse()) && (inpt.Pressing[Input.Shift] || !inpt.UsingMouse() || inpt.Mode == InputState.ModeTouchscreen))
                    {
                        // we use an external menu to let the player pick the desired quantity
                        // we will subtract from this stack after they've made their decision
                        return item;
                    }
                    else if (!ClickSubtractsItem)
                    {
                        return item;
                    }

                    Subtract(DragPrevSlot, item.Quantity);
                }
                // item will be cleared if item.empty() == true
                return item;
            }
            else
            {
                CurrentSlot = null;
                item.Clear();
                return item;
            }
        }

        public void ItemReturn(ItemStack stack)
        {
            Add(stack, DragPrevSlot);
            DragPrevSlot = -1;
        }

        public void HighlightMatching(ItemID itemId)
        {
            ItemManager items = SharedGameResources.Items!;
            for (int i = 0; i < _slotNumber; i++)
            {
                if (Slots[i]!.Visible && items.IsValid(itemId) && SlotType[i] == items.Items[itemId]!.Type)
                    Slots[i]!.Highlight = true;
            }
        }

        public void HighlightClear()
        {
            for (int i = 0; i < _slotNumber; i++)
            {
                Slots[i]!.Highlight = false;
            }
        }

        public ItemStack GetItemStackAtPos(Int2 position)
        {
            int slotOver = SlotOver(position);
            if (slotOver > -1)
            {
                return Storage![slotOver];
            }
            return new ItemStack();
        }
    }
}
