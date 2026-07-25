// 对应 C++ 源：MenuStash.h + MenuStash.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using System.Text;
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// 单个仓库标签页数据，对应 C++ <c>class MenuStashTab</c>。
    /// </summary>
    public class MenuStashTab
    {
        /// <summary>对应 C++ <c>static const bool IS_PRIVATE = true;</c>，作构造默认参数用。</summary>
        public const bool IsPrivateConst = true;

        public string Id = "";
        public string Name = "";
        public string Filename = "";
        public bool IsPrivate;
        public bool IsLegacy;
        public bool Updated;
        public MenuItemStorage Stock;
        public TabList Tablist;

        public MenuStashTab(string id, string name, string filename, bool isPrivate)
        {
            Id = id;
            Name = name;
            Filename = filename;
            IsPrivate = isPrivate;
            IsLegacy = false;
            Updated = false;
            Stock = new MenuItemStorage();
            Tablist = new TabList();
        }
    }

    /// <summary>
    /// 仓库菜单，对应 C++ <c>class MenuStash : public Menu</c>。
    /// 持有的 <see cref="WidgetButton"/>、<see cref="WidgetTabControl"/> 与
    /// 各标签页 <see cref="MenuItemStorage"/> 通过 <see cref="IDisposable"/> 显式释放，
    /// 释放顺序与原始析构函数 <c>~MenuStash()</c> 一致。
    /// </summary>
    public class MenuStash : Menu, IDisposable
    {
        public const int NoSlot = -1;
        public const bool AddPlaySound = true;

        public Rectangle SlotsArea;
        public List<MenuStashTab> Tabs = new List<MenuStashTab>();
        public Queue<ItemStack> DropStack = new Queue<ItemStack>();
        public bool TabControlLocked;

        private WidgetButton? _buttonClose;
        private WidgetButton? _buttonSort;
        private WidgetTabControl? _tabControl;
        private readonly WidgetLabel _labelTitle = new WidgetLabel();
        private readonly WidgetLabel _labelCurrency = new WidgetLabel();

        private int _activeTab;
        private bool _sortEnabled;
        private Int2 _sortPos;

        public MenuStash()
        {
            MessageEngine msg = SharedResources.Msg!;
            EngineSettings eset = SharedResources.Eset!;
            FontEngine font = SharedResources.Font!;

            _buttonClose = new WidgetButton(WidgetButton.CloseFile);
            _buttonSort = null;
            _tabControl = new WidgetTabControl();
            _activeTab = 0;
            _sortEnabled = false;
            TabControlLocked = false;

            int slotsCols = 8; // default if menus/stash.txt::stash_cols not set
            int slotsRows = 8; // default if menus/stash.txt::slots_rows not set

            // Load config settings
            FileParser infile = new FileParser();
            // @CLASS MenuStash|Description of menus/stash.txt
            if (infile.Open("menus/stash.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (infile.NewSection)
                    {
                        if (infile.Section == "tab")
                        {
                            StringBuilder idStr = new StringBuilder();
                            StringBuilder nameStr = new StringBuilder();
                            StringBuilder filenameStr = new StringBuilder();

                            // default tab settings
                            idStr.Append("Stash ").Append(Tabs.Count);
                            nameStr.Append(msg.Get("Stash")).Append(" ").Append(Tabs.Count);
                            filenameStr.Append("stash_tab_").Append(Tabs.Count).Append(".txt");

                            Tabs.Add(new MenuStashTab(idStr.ToString(), nameStr.ToString(), filenameStr.ToString(), MenuStashTab.IsPrivateConst));
                        }
                    }

                    if (infile.Section == "")
                    {
                        if (ParseMenuKey(infile.Key, infile.Val))
                            continue;

                        // @ATTR close|point|Position of the close button.
                        if (infile.Key == "close")
                        {
                            Int2 pos = Parse.ToPoint(infile.Val);
                            _buttonClose!.SetBasePos(pos.X, pos.Y, Utils.AlignTopLeft);
                        }
                        // @ATTR slots_area|point|Position of the top-left slot.
                        else if (infile.Key == "slots_area")
                        {
                            string val = infile.Val;
                            SlotsArea.X = Parse.PopFirstInt(ref val);
                            SlotsArea.Y = Parse.PopFirstInt(ref val);
                        }
                        // @ATTR stash_cols|int|The number of columns for the grid of slots.
                        else if (infile.Key == "stash_cols")
                        {
                            slotsCols = Math.Max(1, Parse.ToInt(infile.Val));
                        }
                        // @ATTR stash_rows|int|The number of rows for the grid of slots.
                        else if (infile.Key == "stash_rows")
                        {
                            slotsRows = Math.Max(1, Parse.ToInt(infile.Val));
                        }
                        // @ATTR label_title|label|Position of the "Stash" label.
                        else if (infile.Key == "label_title")
                        {
                            _labelTitle.SetFromLabelInfo(Parse.PopLabelInfo(infile.Val));
                        }
                        // @ATTR currency|label|Position of the label displaying the amount of currency stored in the stash.
                        else if (infile.Key == "currency")
                        {
                            _labelCurrency.SetFromLabelInfo(Parse.PopLabelInfo(infile.Val));
                        }
                        // @ATTR sort_enabled|bool|Enables the button for sorting items in the active stash tab. Defaults to false. The button image is expected to be located at 'images/menus/buttons/button_sort.png'.
                        else if (infile.Key == "sort_enabled")
                        {
                            _sortEnabled = Parse.ToBool(infile.Val);
                        }
                        // @ATTR sort_pos|point|Position of the sort button.
                        else if (infile.Key == "sort_pos")
                        {
                            _sortPos = Parse.ToPoint(infile.Val);
                        }
                        else
                        {
                            infile.Error("MenuStash: '%s' is not a valid key.", infile.Key);
                        }
                    }
                    else if (infile.Section == "tab")
                    {
                        // don't load any settings for the "Private" and "Shared" tabs
                        if (Tabs[^1].IsLegacy)
                            continue;

                        if (infile.Key == "name")
                        {
                            // @ATTR tab.name|["Private", "Shared", string]|The displayed name of this tab. It is also used to determine the filename of the stash file that the engine will create. 'Private' and 'Shared' will use their legacy filenames for compatibility.
                            string nameStr = infile.Val;
                            Tabs[^1].Id = nameStr;
                            Tabs[^1].Name = msg.Get(nameStr);

                            if (nameStr == "Private")
                            {
                                Tabs[^1].Filename = "stash_HC.txt";
                                Tabs[^1].IsPrivate = true;
                                Tabs[^1].IsLegacy = true;
                            }
                            else if (nameStr == "Shared")
                            {
                                Tabs[^1].Filename = "stash.txt";
                                Tabs[^1].IsPrivate = false;
                                Tabs[^1].IsLegacy = true;
                            }
                            else
                            {
                                // generate a filename
                                StringBuilder ss = new StringBuilder();
                                ss.Append("stash_").Append(Utils.HashString(nameStr).ToString("x")).Append(".txt");
                                Tabs[^1].Filename = ss.ToString();
                            }
                        }
                        else if (infile.Key == "is_private")
                        {
                            // @ATTR tab.is_private|bool|If true, this stash will not be shared across other saves.
                            Tabs[^1].IsPrivate = Parse.ToBool(infile.Val);
                        }
                        else
                        {
                            infile.Error("MenuStash: '%s' is not a valid key.", infile.Key);
                        }
                    }
                }
                infile.Close();
            }

            _labelTitle.SetText(msg.Get("Stash"));
            _labelTitle.SetColor(font.GetColor(FontEngine.ColorMenuNormal));

            _labelCurrency.SetColor(font.GetColor(FontEngine.ColorMenuNormal));

            int stashSlots = slotsCols * slotsRows;
            SlotsArea.Width = slotsCols * eset.Resolutions.IconSize;
            SlotsArea.Height = slotsRows * eset.Resolutions.IconSize;

            // default tabs if none are defined in the config file
            if (Tabs.Count == 0)
            {
                Tabs.Add(new MenuStashTab("Private", msg.Get("Private"), "stash_HC.txt", MenuStashTab.IsPrivateConst));
                Tabs.Add(new MenuStashTab("Shared", msg.Get("Shared"), "stash.txt", !MenuStashTab.IsPrivateConst));
            }

            // ensure that characters have access to at least one private stash
            // this needs to be done because permadeath characters ONLY have access to private stashes
            bool privateExists = false;
            for (int i = 0; i < Tabs.Count; ++i)
            {
                if (Tabs[i].IsPrivate)
                {
                    privateExists = true;
                    break;
                }
            }
            if (!privateExists)
            {
                Tabs.Add(new MenuStashTab("Private", msg.Get("Private"), "stash_HC.txt", MenuStashTab.IsPrivateConst));
            }

            for (int i = 0; i < Tabs.Count; ++i)
            {
                Tabs[i].Stock.InitGrid(stashSlots, SlotsArea, slotsCols);
                _tabControl!.SetupTab((uint)i, Tabs[i].Name, Tabs[i].Tablist);

                Tabs[i].Tablist.Lock();
                for (int j = 0; j < stashSlots; ++j)
                {
                    Tabs[i].Tablist.Add(Tabs[i].Stock.Slots[j]);
                }
            }

            if (_background == null)
                SetBackground("images/menus/stash.png");

            if (_sortEnabled)
            {
                _buttonSort = new WidgetButton(WidgetButton.SortItemsFile);

                if (_buttonSort != null)
                {
                    _buttonSort.SetBasePos(_sortPos.X, _sortPos.Y, Utils.AlignTopLeft);
                    for (int i = 0; i < Tabs.Count; ++i)
                    {
                        Tabs[i].Stock.SortTooltip = text => _buttonSort!.Tooltip = text;
                    }
                    if (Tabs.Count > 0)
                    {
                        Tabs[0].Stock.RefreshSortTooltip();
                    }
                }
            }

            Align();
        }

        public override void Align()
        {
            base.Align();

            Rectangle tabsArea = SlotsArea;
            tabsArea.X += WindowArea.X;
            tabsArea.Y += WindowArea.Y;

            _tabControl!.SetMainArea(tabsArea.X, tabsArea.Y - _tabControl.GetTabHeight(), tabsArea.Width);

            _buttonClose!.SetPos(WindowArea.X, WindowArea.Y);

            for (int i = 0; i < Tabs.Count; ++i)
            {
                Tabs[i].Stock.SetPos(WindowArea.X, WindowArea.Y);
            }

            _labelTitle.SetPos(WindowArea.X, WindowArea.Y);
            _labelCurrency.SetPos(WindowArea.X, WindowArea.Y);

            if (_buttonSort != null)
            {
                _buttonSort.SetPos(WindowArea.X, WindowArea.Y);
            }
        }

        public void Logic()
        {
            if (!Visible) return;

            InputState inpt = SharedResources.Inpt!;
            SoundManager snd = SharedResources.Snd!;

            int prevtab = _tabControl!.GetActiveTab();

            Tablist.Logic();
            for (int i = 0; i < Tabs.Count; ++i)
            {
                Tabs[i].Tablist.Logic();
            }

            // disable tab control if we're dragging something
            if (!TabControlLocked)
                _tabControl.Logic();

            if (inpt.UsingTouchscreen() && _activeTab != _tabControl.GetActiveTab())
            {
                for (int i = 0; i < Tabs.Count; ++i)
                {
                    Tabs[i].Tablist.Defocus();
                }
            }
            _activeTab = _tabControl.GetActiveTab();

            Tablist.SetNextTabList(Tabs[_activeTab].Tablist);

            if (inpt.UsingTouchscreen())
            {
                if (Tabs[_activeTab].Tablist.GetCurrent() == -1)
                    Tabs[_activeTab].Stock.CurrentSlot = null;
            }

            if (_buttonClose!.CheckClick())
            {
                Visible = false;
                snd.Play(SfxClose, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);
            }

            if (_buttonSort != null)
            {
                if (prevtab != _activeTab)
                {
                    Tabs[_activeTab].Stock.RefreshSortTooltip();
                }

                if (_buttonSort.CheckClick())
                {
                    Tabs[_activeTab].Stock.SortNext();
                    Tabs[_activeTab].Updated = true;
                }
            }
        }

        public override void Render()
        {
            if (!Visible) return;

            MessageEngine msg = SharedResources.Msg!;
            EngineSettings eset = SharedResources.Eset!;

            // background
            base.Render();

            // close button
            _buttonClose!.Render();

            // text overlay
            _labelTitle.Render();
            if (!_labelCurrency.IsHidden())
            {
                _labelCurrency.SetText(msg.GetV("%d %s", Tabs[_activeTab].Stock.Count(eset.Misc.CurrencyId), eset.Loot.Currency));
                _labelCurrency.Render();
            }

            _tabControl!.Render();

            // show stock
            Tabs[_activeTab].Stock.Render();

            if (_buttonSort != null)
            {
                _buttonSort.Render();
            }
        }

        /// <summary>
        /// Dragging and dropping an item can be used to rearrange the stash
        /// </summary>
        public bool Drop(Int2 position, ItemStack stack)
        {
            if (stack.Empty())
            {
                return true;
            }

            ItemManager items = SharedGameResources.Items!;

            int slot;
            int dragPrevSlot;
            bool success = true;

            items.PlaySound(stack.Item);

            slot = Tabs[_activeTab].Stock.SlotOver(position);
            dragPrevSlot = Tabs[_activeTab].Stock.DragPrevSlot;

            if (slot == -1)
            {
                success = Add(stack, slot, !AddPlaySound);
            }
            else if (dragPrevSlot != -1)
            {
                if (Tabs[_activeTab].Stock[slot].Item == stack.Item || Tabs[_activeTab].Stock[slot].Empty())
                {
                    // Drop the stack, merging if needed
                    success = Add(stack, slot, !AddPlaySound);
                }
                else if (Tabs[_activeTab].Stock[dragPrevSlot].Empty())
                {
                    // Check if the previous slot is free (could still be used if SHIFT was used).
                    // Swap the two stacks
                    ItemReturn(Tabs[_activeTab].Stock[slot]);
                    Tabs[_activeTab].Stock[slot] = stack.Clone();
                    Tabs[_activeTab].Updated = true;
                }
                else
                {
                    ItemReturn(stack);
                    Tabs[_activeTab].Updated = true;
                }
            }
            else
            {
                success = Add(stack, slot, !AddPlaySound);
            }

            return success;
        }

        public bool Add(ItemStack stack, int slot, bool playSound)
        {
            if (stack.Empty())
            {
                return true;
            }

            ItemManager items = SharedGameResources.Items!;
            Avatar pc = SharedGameResources.Pc!;
            MessageEngine msg = SharedResources.Msg!;

            if (!items.IsValid(stack.Item))
            {
                return false;
            }

            if (playSound)
            {
                items.PlaySound(stack.Item);
            }

            if (items.Items[stack.Item]!.NoStash == Item.NoStashAll)
            {
                pc.LogMsg(msg.Get("This item can not be stored in the stash."), Avatar.MsgNormal);
                DropStack.Enqueue(stack.Clone());
                return false;
            }
            else if (Tabs[_activeTab].IsPrivate && items.Items[stack.Item]!.NoStash == Item.NoStashPrivate)
            {
                pc.LogMsg(msg.Get("This item can not be stored in the private stash."), Avatar.MsgNormal);
                DropStack.Enqueue(stack.Clone());
                return false;
            }
            else if (!Tabs[_activeTab].IsPrivate && items.Items[stack.Item]!.NoStash == Item.NoStashShared)
            {
                pc.LogMsg(msg.Get("This item can not be stored in the shared stash."), Avatar.MsgNormal);
                DropStack.Enqueue(stack.Clone());
                return false;
            }

            ItemStack leftover = Tabs[_activeTab].Stock.Add(stack, slot);
            if (!leftover.Empty())
            {
                if (leftover.Quantity != stack.Quantity)
                {
                    Tabs[_activeTab].Updated = true;
                }
                pc.LogMsg(msg.Get("Stash is full."), Avatar.MsgNormal);
                DropStack.Enqueue(leftover.Clone());
                return false;
            }
            else
            {
                Tabs[_activeTab].Updated = true;
            }

            return true;
        }

        /// <summary>
        /// Start dragging a vendor item
        /// Players can drag an item to their inventory.
        /// </summary>
        public ItemStack Click(Int2 position)
        {
            InputState inpt = SharedResources.Inpt!;

            ItemStack stack = Tabs[_activeTab].Stock.Click(position);
            if (inpt.UsingTouchscreen())
            {
                Tabs[_activeTab].Tablist.SetCurrent(Tabs[_activeTab].Stock.CurrentSlot);
            }
            return stack;
        }

        /// <summary>
        /// Cancel the dragging initiated by the click()
        /// </summary>
        public void ItemReturn(ItemStack stack)
        {
            Tabs[_activeTab].Stock.ItemReturn(stack);
        }

        public void RenderTooltips(Int2 position)
        {
            if (!Visible || !Utils.IsWithinRect(WindowArea, position))
                return;

            Avatar pc = SharedGameResources.Pc!;
            TooltipManager tooltipm = SharedResources.Tooltipm!;

            TooltipData tipData = Tabs[_activeTab].Stock.CheckTooltip(position, pc.Stats, ItemManager.PlayerInv, ItemManager.TooltipInputHint);
            tooltipm.Push(tipData, position, TooltipData.StyleFloat);
        }

        public void RemoveFromPrevSlot(int quantity)
        {
            int dragPrevSlot = Tabs[_activeTab].Stock.DragPrevSlot;
            if (dragPrevSlot > -1)
            {
                Tabs[_activeTab].Stock.Subtract(dragPrevSlot, quantity);
            }
        }

        public void Validate(Queue<ItemStack> globalDropStack)
        {
            ItemManager items = SharedGameResources.Items!;
            Avatar pc = SharedGameResources.Pc!;
            MessageEngine msg = SharedResources.Msg!;

            for (int tab = 0; tab < Tabs.Count; ++tab)
            {
                for (int i = 0; i < Tabs[_activeTab].Stock.GetSlotNumber(); ++i)
                {
                    if (Tabs[tab].Stock[i].Empty())
                        continue;

                    ItemStack stack = Tabs[tab].Stock[i].Clone();  // Clone: reference-type fix

                    int noStash = 0;
                    if (items.IsValid(stack.Item))
                        noStash = items.Items[stack.Item]!.NoStash;

                    if (noStash == Item.NoStashAll || (Tabs[tab].IsPrivate && noStash == Item.NoStashPrivate) || (!Tabs[tab].IsPrivate && noStash == Item.NoStashShared))
                    {
                        pc.LogMsg(msg.GetV("Can not store item in stash: %s", items.GetItemName(stack.Item)), Avatar.MsgNormal);
                        globalDropStack.Enqueue(stack);
                        Tabs[tab].Stock[i].Clear();
                        Tabs[tab].Updated = true;
                    }
                }
            }
        }

        public bool CheckUpdates()
        {
            EngineSettings eset = SharedResources.Eset!;

            bool updated = false;

            for (int i = 0; i < Tabs.Count; ++i)
            {
                if (Tabs[i].Updated)
                {
                    Tabs[i].Updated = false;

                    if (eset.Misc.SaveOnstash == EngineSettings.MiscSettings.SaveOnstashAll)
                        updated = true;
                    else if (Tabs[i].IsPrivate && eset.Misc.SaveOnstash == EngineSettings.MiscSettings.SaveOnstashPrivate)
                        updated = true;
                    else if (!Tabs[i].IsPrivate && eset.Misc.SaveOnstash == EngineSettings.MiscSettings.SaveOnstashShared)
                        updated = true;
                }
            }

            return updated;
        }

        public void EnableSharedTab(bool permadeath)
        {
            for (int i = 0; i < Tabs.Count; ++i)
            {
                _tabControl!.SetEnabled((uint)i, !permadeath || Tabs[i].IsPrivate);
            }
        }

        public void SetTab(int tab)
        {
            InputState inpt = SharedResources.Inpt!;

            if (inpt.UsingTouchscreen() && _activeTab != tab)
            {
                for (int i = 0; i < Tabs.Count; ++i)
                {
                    Tabs[i].Tablist.Defocus();
                }
            }
            if (tab >= Tabs.Count)
            {
                _tabControl!.SetActiveTab(0);
                _activeTab = 0;
            }
            else
            {
                _tabControl!.SetActiveTab((uint)tab);
                _activeTab = tab;
            }
        }

        public int GetTab()
        {
            return _activeTab;
        }

        public void LockTabControl()
        {
            for (int i = 0; i < Tabs.Count; ++i)
            {
                Tabs[i].Tablist.SetPrevTabList(null);
            }
        }

        public void UnlockTabControl()
        {
            for (int i = 0; i < Tabs.Count; ++i)
            {
                Tabs[i].Tablist.SetPrevTabList(Tablist);
            }
        }

        public override TabList? GetCurrentTabList()
        {
            if (Tablist.GetCurrent() != -1)
            {
                return Tablist;
            }
            else
            {
                for (int i = 0; i < Tabs.Count; ++i)
                {
                    if (Tabs[i].Tablist.GetCurrent() != -1)
                        return Tabs[i].Tablist;
                }
            }

            return null;
        }

        public override void DefocusTabLists()
        {
            Tablist.Defocus();
            for (int i = 0; i < Tabs.Count; ++i)
            {
                Tabs[i].Tablist.Defocus();
            }
        }

        public override void Dispose()
        {
            _buttonClose?.Dispose();
            _buttonClose = null;
            _buttonSort?.Dispose();
            _buttonSort = null;
            _tabControl?.Dispose();
            _tabControl = null;
            for (int i = 0; i < Tabs.Count; ++i)
            {
                Tabs[i].Stock.Dispose();
            }
            _labelTitle.Dispose();
            _labelCurrency.Dispose();

            base.Dispose();
        }
    }
}
