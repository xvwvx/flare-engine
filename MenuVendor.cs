// <自动生成> 对应 C++ 源文件：MenuVendor.h + MenuVendor.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// MenuVendor
    ///
    /// NPC 商人菜单：展示购买、回购与合成三个标签页中的物品库存。
    /// 持有的 <see cref="WidgetButton"/>、<see cref="WidgetTabControl"/>、
    /// <see cref="WidgetTooltip"/> 与 <see cref="MenuItemStorage"/> 实例通过
    /// <see cref="IDisposable"/> 显式释放，释放顺序与原始析构函数 <c>~MenuVendor()</c> 一致
    /// （先 <see cref="_closeButton"/>、<see cref="_tabControl"/>、<see cref="_tip"/>，
    /// 再各 <see cref="Stock"/> 槽位容器与 <see cref="_labelVendor"/>，最后基类 <see cref="Menu"/>）。
    /// </summary>
    public class MenuVendor : Menu, IDisposable
    {
        public const int TabCount = 3;

        private WidgetButton? _closeButton;
        private WidgetTabControl? _tabControl;
        private readonly WidgetLabel _labelVendor = new WidgetLabel();

        private int _vendorSlots;

        private int _slotsCols;
        private int _slotsRows;
        private int _activetab;
        private int _sortStockBuy;
        private int _sortStockCraft;

        private WidgetTooltip? _tip;

        /// <summary>对应 C++ 原始公有字段 <c>npc</c>。</summary>
        public NPC? Npc;

        /// <summary>对应 C++ 原始公有字段 <c>buyback_stock</c>。</summary>
        public Dictionary<string, ItemStorage> BuybackStock = new Dictionary<string, ItemStorage>();

        /// <summary>对应 C++ 原始公有字段 <c>stock[TAB_COUNT]</c>。</summary>
        public MenuItemStorage[] Stock = new MenuItemStorage[TabCount];

        /// <summary>对应 C++ 原始公有字段 <c>slots_area</c>。</summary>
        public Rectangle SlotsArea;

        /// <summary>对应 C++ 原始公有字段 <c>tablist_tabs[TAB_COUNT]</c>。</summary>
        public TabList[] TablistTabs = new TabList[TabCount];

        /// <summary>对应 C++ 原始公有字段 <c>sell_enabled</c>。</summary>
        public bool SellEnabled;

        /// <summary>对应 C++ 内联方法 <c>getTab()</c>。</summary>
        public int Tab
        {
            get { return _activetab; }
        }

        public int GetTab() => _activetab;

        public MenuVendor()
        {
            _closeButton = new WidgetButton(WidgetButton.CloseFile);
            _tabControl = new WidgetTabControl();
            _slotsCols = 1;
            _slotsRows = 1;
            _activetab = ItemManager.VendorBuy;
            _sortStockBuy = ItemStorage.SortNone;
            _sortStockCraft = ItemStorage.SortNone;
            _tip = new WidgetTooltip();
            Npc = null;
            SellEnabled = true;

            for (int i = 0; i < TabCount; ++i)
            {
                Stock[i] = new MenuItemStorage();
                TablistTabs[i] = new TabList();
            }

            MessageEngine msg = SharedResources.Msg!;
            _tabControl.SetupTab((uint)ItemManager.VendorBuy, msg.Get("Inventory"), TablistTabs[ItemManager.VendorBuy]);
            _tabControl.SetupTab((uint)ItemManager.VendorSell, msg.Get("Buyback"), TablistTabs[ItemManager.VendorSell]);
            _tabControl.SetupTab((uint)ItemManager.VendorCraft, msg.Get("Craft"), TablistTabs[ItemManager.VendorCraft]);

            // Load config settings
            using FileParser infile = new FileParser();
            // @CLASS MenuVendor|Description of menus/vendor.txt
            if (infile.Open("menus/vendor.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (ParseMenuKey(infile.Key, infile.Val))
                        continue;

                    // @ATTR close|point|Position of the close button.
                    if (infile.Key == "close")
                    {
                        Int2 pos = Parse.ToPoint(infile.Val);
                        _closeButton.SetBasePos(pos.X, pos.Y, Utils.AlignTopLeft);
                    }
                    // @ATTR slots_area|point|Position of the top-left slot.
                    else if (infile.Key == "slots_area")
                    {
                        string val = infile.Val;
                        SlotsArea.X = Parse.PopFirstInt(ref val);
                        SlotsArea.Y = Parse.PopFirstInt(ref val);
                    }
                    // @ATTR vendor_cols|int|The number of columns in the grid of slots.
                    else if (infile.Key == "vendor_cols")
                    {
                        _slotsCols = Math.Max(1, Parse.ToInt(infile.Val));
                    }
                    // @ATTR vendor_rows|int|The number of rows in the grid of slots.
                    else if (infile.Key == "vendor_rows")
                    {
                        _slotsRows = Math.Max(1, Parse.ToInt(infile.Val));
                    }
                    // @ATTR label_title|label|The position of the text that displays the NPC's name.
                    else if (infile.Key == "label_title")
                    {
                        _labelVendor.SetFromLabelInfo(Parse.PopLabelInfo(infile.Val));
                    }
                    // @ATTR sort_stock_buy|["none", "type", "quality", "level", "price", "id"]|Sorts all vendors' "Buy" stock by the given metric.
                    else if (infile.Key == "sort_stock_buy")
                    {
                        if (infile.Val == "none")
                            _sortStockBuy = ItemStorage.SortNone;
                        else if (infile.Val == "type")
                            _sortStockBuy = ItemStorage.SortType;
                        else if (infile.Val == "quality")
                            _sortStockBuy = ItemStorage.SortQuality;
                        else if (infile.Val == "level")
                            _sortStockBuy = ItemStorage.SortLevel;
                        else if (infile.Val == "price")
                            _sortStockBuy = ItemStorage.SortBuyPrice;
                        else if (infile.Val == "id")
                            _sortStockBuy = ItemStorage.SortId;
                    }
                    // @ATTR sort_stock_craft|["none", "type", "quality", "level", "price", "id"]|Sorts all vendors' "Craft" stock by the given metric.
                    else if (infile.Key == "sort_stock_craft")
                    {
                        if (infile.Val == "none")
                            _sortStockCraft = ItemStorage.SortNone;
                        else if (infile.Val == "type")
                            _sortStockCraft = ItemStorage.SortType;
                        else if (infile.Val == "quality")
                            _sortStockCraft = ItemStorage.SortQuality;
                        else if (infile.Val == "level")
                            _sortStockCraft = ItemStorage.SortLevel;
                        else if (infile.Val == "price")
                            _sortStockCraft = ItemStorage.SortBuyPrice;
                        else if (infile.Val == "id")
                            _sortStockCraft = ItemStorage.SortId;
                    }
                    else
                    {
                        infile.Error("MenuVendor: '%s' is not a valid key.", infile.Key);
                    }
                }
                infile.Close();
            }

            
            var eset = SharedResources.Eset!;
            var font = SharedResources.Font!;

            _labelVendor.SetColor(font.GetColor(FontEngine.ColorMenuNormal));

            _vendorSlots = _slotsCols * _slotsRows;
            SlotsArea.Width = _slotsCols * eset.Resolutions.IconSize;
            SlotsArea.Height = _slotsRows * eset.Resolutions.IconSize;

            for (int i = 0; i < TabCount; ++i)
            {
                Stock[i].InitGrid(_vendorSlots, SlotsArea, _slotsCols);
                TablistTabs[i].Lock();

                for (int j = 0; j < _vendorSlots; ++j)
                {
                    TablistTabs[i].Add(Stock[i].Slots[j]);
                }
            }

            // since we never subtract items from the craft tab, we set the max quantity to 1 in order to hide the quantity display on the icons
            Stock[ItemManager.VendorCraft].MaxQuantityIsOne = true;

            // we need to behave like we're holding Shift without actually doing so
            Stock[ItemManager.VendorCraft].ClickSubtractsItem = false;

            Tablist.SetNextTabList(TablistTabs[ItemManager.VendorBuy]);

            if (_background == null)
                SetBackground("images/menus/vendor.png");

            Align();
        }

        /// <summary>
        /// 对应 C++ 的 <c>~MenuVendor()</c>：先释放 closeButton、tabControl、tip，
        /// 再释放各 Stock 容器与 label_vendor，最后调用基类 <see cref="Menu.Dispose"/>。
        /// </summary>
        public override void Dispose()
        {
            _closeButton?.Dispose();
            _tabControl?.Dispose();
            _tip?.Dispose();

            for (int i = 0; i < TabCount; ++i)
            {
                Stock[i]?.Dispose();
            }

            _labelVendor.Dispose();

            base.Dispose();
        }

        public override void Align()
        {
            base.Align();

            _labelVendor.SetPos(WindowArea.X, WindowArea.Y);

            Rectangle tabsArea = SlotsArea;
            tabsArea.X += WindowArea.X;
            tabsArea.Y += WindowArea.Y;

            _tabControl!.SetMainArea(tabsArea.X, tabsArea.Y - _tabControl.GetTabHeight(), tabsArea.Width);

            _closeButton!.SetPos(WindowArea.X, WindowArea.Y);

            Stock[ItemManager.VendorBuy].SetPos(WindowArea.X, WindowArea.Y);
            Stock[ItemManager.VendorSell].SetPos(WindowArea.X, WindowArea.Y);
            Stock[ItemManager.VendorCraft].SetPos(WindowArea.X, WindowArea.Y);
        }

        public void Logic()
        {
            var inpt = SharedResources.Inpt!;
            var snd = SharedResources.Snd!;

            if (!Visible) return;

            Tablist.Logic();
            for (int i = 0; i < TabCount; ++i)
            {
                TablistTabs[i].Logic();
            }

            if (Stock[ItemManager.VendorBuy].DragPrevSlot == -1 && Stock[ItemManager.VendorSell].DragPrevSlot == -1 && Stock[ItemManager.VendorCraft].DragPrevSlot == -1)
                _tabControl!.Logic();

            if (inpt.UsingTouchscreen() && _activetab != _tabControl!.GetActiveTab())
            {
                for (int i = 0; i < TabCount; ++i)
                {
                    TablistTabs[i].Defocus();
                }
            }
            _activetab = _tabControl!.GetActiveTab();

            for (int i = 0; i < TabCount; ++i)
            {
                if (_activetab == i)
                {
                    TablistTabs[i].Unlock();
                    Tablist.SetNextTabList(TablistTabs[i]);
                }
                else
                {
                    TablistTabs[i].Lock();
                }
            }

            if (inpt.UsingTouchscreen())
            {
                for (int i = 0; i < TabCount; ++i)
                {
                    if (_activetab == i && TablistTabs[i].GetCurrent() == -1)
                    {
                        Stock[i].CurrentSlot = null;
                    }
                }
            }

            if (_closeButton!.CheckClick())
            {
                SetNPC(null);
                snd.Play(SfxClose, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);
            }
        }

        public void SetTab(int tab)
        {
            if (SharedResources.Inpt!.UsingTouchscreen() && _activetab != tab)
            {
                for (int i = 0; i < TabCount; ++i)
                {
                    TablistTabs[i].Defocus();
                }
            }
            _tabControl!.SetActiveTab((uint)tab);
            _activetab = tab;
        }

        public override void Render()
        {
            if (!Visible) return;

            // background
            base.Render();

            // close button
            _closeButton!.Render();

            // text overlay
            _labelVendor.Render();

            // render tabs
            _tabControl!.Render();

            // show stock
            Stock[_activetab].Render();
        }

        /// <summary>
        /// Start dragging a vendor item
        /// Players can drag an item to their inventory to purchase.
        /// </summary>
        public ItemStack Click(Int2 position)
        {
            ItemStack stack = Stock[_activetab].Click(position);
            SaveInventory();
            if (SharedResources.Inpt!.UsingTouchscreen())
            {
                for (int i = 0; i < TabCount; ++i)
                {
                    if (_activetab == i)
                    {
                        TablistTabs[i].SetCurrent(Stock[i].CurrentSlot);
                    }
                }
            }
            return stack;
        }

        /// <summary>
        /// Cancel the dragging initiated by the clic()
        /// </summary>
        public void ItemReturn(ItemStack stack)
        {
            if (_activetab == ItemManager.VendorCraft)
            {
                ResetDrag();
                return;
            }

            Stock[_activetab].ItemReturn(stack);
            SaveInventory();
        }

        public void Add(ItemStack stack)
        {
            stack.CanBuyback = true;

            // Remove the first item stack to make room
            if (Stock[ItemManager.VendorSell].Full(stack))
            {
                Stock[ItemManager.VendorSell][0].Clear();
                MoveEmptySlotsToEnd(ItemManager.VendorSell);
            }
            SharedGameResources.Items!.PlaySound(stack.Item);
            Stock[ItemManager.VendorSell].Add(stack, ItemStorage.NoSlot);
            SaveInventory();
        }

        public void RenderTooltips(Int2 position)
        {
            var pc = SharedGameResources.Pc!;
            var tooltipm = SharedResources.Tooltipm!;

            if (!Visible || !Utils.IsWithinRect(WindowArea, position))
                return;

            TooltipData tipData = Stock[_activetab].CheckTooltip(position, pc.Stats, _activetab, ItemManager.TooltipInputHint);
            tooltipm.Push(tipData, position, TooltipData.StyleFloat);
        }

        /// <summary>
        /// Save changes to the inventory back to the NPC
        /// For persistent stock amounts and buyback (at least until
        /// the player leaves this map)
        /// </summary>
        public void SaveInventory()
        {
            if (Npc != null)
            {
                for (int i = 0; i < _vendorSlots; i++)
                {
                    // Clone() needed: C# ItemStack is a class. Without Clone(),
                    // NPC stock and vendor stock would share the same object,
                    // causing mutations to one to affect the other.
                    Npc.Stock[i] = Stock[ItemManager.VendorBuy][i].Clone();
                    if (!BuybackStock.ContainsKey(Npc.Filename))
                        BuybackStock[Npc.Filename] = new ItemStorage();
                    BuybackStock[Npc.Filename][i] = Stock[ItemManager.VendorSell][i].Clone();
                    Npc.CraftStock[i] = Stock[ItemManager.VendorCraft][i].Clone();
                }
            }
            else
            {
                Utils.LogError("MenuVendor: saveInventory() failed, unknown NPC.");
            }
        }

        public void MoveEmptySlotsToEnd(int type)
        {
            for (int i = 0; i < _vendorSlots; i++)
            {
                // Clone() needed: C# ItemStack is a class. Without Clone(),
                // Clear() below would also empty temp via shared reference.
                ItemStack temp = Stock[type][i].Clone();
                Stock[type][i].Clear();
                if (!temp.Empty())
                    Stock[type].Add(temp, ItemStorage.NoSlot);
            }
        }

        public void SetNPC(NPC? npc)
        {
            var msg = SharedResources.Msg!;
            var eset = SharedResources.Eset!;
            var snd = SharedResources.Snd!;

            Npc = npc;

            if (npc == null)
            {
                Visible = false;
                return;
            }

            _labelVendor.SetText(msg.Get("Vendor") + " - " + npc.Name);

            if (!BuybackStock.ContainsKey(npc.Filename))
                BuybackStock[npc.Filename] = new ItemStorage();
            BuybackStock[npc.Filename].Init(NPC.VendorMaxStock);

            for (int i = 0; i < _vendorSlots; i++)
            {
                // Clone() needed: C# ItemStack is a class. Without Clone(),
                // mutations to vendor stock would propagate to NPC stock.
                Stock[ItemManager.VendorBuy][i] = npc.Stock[i].Clone();
                if (npc.ResetBuyback)
                {
                    // this occurs on the first interaction with an NPC after map load
                    if (eset.Misc.KeepBuybackOnMapChange)
                        BuybackStock[npc.Filename][i].CanBuyback = false;
                    else
                        BuybackStock[npc.Filename][i].Clear();
                }
                Stock[ItemManager.VendorSell][i] = BuybackStock[npc.Filename][i].Clone();

                Stock[ItemManager.VendorCraft][i] = npc.CraftStock[i].Clone();
                Stock[ItemManager.VendorCraft][i].Quantity = 1;
            }
            npc.ResetBuyback = false;

            for (int i = 0; i < TabCount; ++i)
            {
                MoveEmptySlotsToEnd(i);
                _tabControl!.SetEnabled((uint)i, npc.VendorTabEnabled[i]);
            }

            SellEnabled = npc.VendorTabEnabled[ItemManager.VendorSell];

            for (int i = 0; i < TabCount; ++i)
            {
                if (npc.VendorTabEnabled[i])
                {
                    SetTab(i);
                    break;
                }
            }

            Stock[ItemManager.VendorBuy].Sort(_sortStockBuy);
            Stock[ItemManager.VendorCraft].Sort(_sortStockCraft);

            if (!Visible)
            {
                Visible = true;
                snd.Play(SfxOpen, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);
                npc.PlaySoundIntro();
            }

            Align();
        }

        public void RemoveFromPrevSlot(int quantity)
        {
            if (_activetab == ItemManager.VendorCraft)
            {
                ResetDrag();
                return;
            }

            int dragPrevSlot = Stock[_activetab].DragPrevSlot;
            if (dragPrevSlot > -1)
            {
                Stock[_activetab].Subtract(dragPrevSlot, quantity);
                SaveInventory();
            }
        }

        public void LockTabControl()
        {
            for (int i = 0; i < TabCount; ++i)
            {
                TablistTabs[i].SetPrevTabList(null);
            }
        }

        public void UnlockTabControl()
        {
            for (int i = 0; i < TabCount; ++i)
            {
                TablistTabs[i].SetPrevTabList(Tablist);
            }
        }

        public override TabList? GetCurrentTabList()
        {
            if (Tablist.GetCurrent() != -1)
                return Tablist;

            for (int i = 0; i < TabCount; ++i)
            {
                if (TablistTabs[i].GetCurrent() != -1)
                {
                    return TablistTabs[i];
                }
            }

            return null;
        }

        public override void DefocusTabLists()
        {
            Tablist.Defocus();
            for (int i = 0; i < TabCount; ++i)
            {
                TablistTabs[i].Defocus();
            }
        }

        public void ResetDrag()
        {
            for (int i = 0; i < TabCount; ++i)
            {
                Stock[i].DragPrevSlot = -1;
            }
        }
    }
}
