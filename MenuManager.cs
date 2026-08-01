// <自动生成> 对应 C++ 源文件：MenuManager.h + MenuManager.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks ???
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// MenuManager
    ///
    /// ??? UI已全局导入，此处省略。    /// ??/??/已全局导入，此处省略。    /// <see cref="WidgetSlot"/> 已全局导入，此处省略。
    /// <see cref="IDisposable"/> 已全局导入，此处省略。    /// </summary>
    public class MenuManager : IDisposable
    {
        private const int DragSrcNone = 0;
        private const int DragSrcPowers = 1;
        private const int DragSrcInventory = 2;
        private const int DragSrcActionbar = 3;
        private const int DragSrcVendor = 4;
        private const int DragSrcStash = 5;

        private const int ActionSrcNone = 0;
        private const int ActionSrcPowers = 1;
        private const int ActionSrcInventory = 2;
        private const int ActionSrcActionbar = 3;
        private const int ActionSrcVendor = 4;
        private const int ActionSrcStash = 5;

        private const int ActionPickerActionbarSelect = 0;
        private const int ActionPickerActionbarClear = 1;
        private const int ActionPickerActionbarUse = 2;

        private const int ActionPickerPowersSelect = 0;
        private const int ActionPickerPowersUpgrade = 1;

        private const int ActionPickerInventorySelect = 0;
        private const int ActionPickerInventoryActivate = 1;
        private const int ActionPickerInventoryActionbar = 2;
        private const int ActionPickerInventoryDrop = 3;
        private const int ActionPickerInventorySell = 4;
        private const int ActionPickerInventoryStash = 5;

        private const int ActionPickerStashSelect = 0;
        private const int ActionPickerStashTransfer = 1;

        private const int ActionPickerVendorBuy = 0;

        private const int DragPostActionNone = 0;
        private const int DragPostActionDrop = 1;
        private const int DragPostActionBuy = 2;
        private const int DragPostActionSell = 3;
        private const int DragPostActionStash = 4;

        private bool _keyLock;
        private bool _mouseDragging;
        private bool _keyboardDragging;
        private bool _stickyDragging;
        private ItemStack _dragStack = new ItemStack();
        private PowerID _dragPower;
        private int _dragSrc;
        private WidgetSlot? _dragIcon;
        private bool _done;
        private bool _actDragHover;
        private Int2 _keydragPos;
        private int _actionSrc;
        private Int2 _actionPickerTarget;
        private Dictionary<int, uint> _actionPickerMap = new Dictionary<int, uint>();
        private int _dragPostAction;

        public List<Menu?> Menus = new List<Menu?>();
        public MenuInventory? Inv;
        public MenuPowers? Pow;
        public MenuCharacter? Chr;
        public MenuLog? Questlog;
        public MenuHUDLog? Hudlog;
        public MenuActionBar? Act;
        public MenuBook? Book;
        public MenuStatBar? Hp;
        public MenuStatBar? Mp;
        public MenuStatBar? Xp;
        public List<MenuStatBar?> ResourceStatbars = new List<MenuStatBar?>();
        public MenuMiniMap? Mini;
        public MenuNumPicker? NumPicker;
        public MenuEnemy? Enemy;
        public MenuVendor? Vendor;
        public MenuTalker? Talker;
        public MenuExit? Exit;
        public MenuActiveEffects? Effects;
        public MenuStash? Stash;
        public MenuGameOver? GameOver;
        public MenuConfirm? ActionPicker;
        public MenuRegionTitle? RegionTitle;
        public MenuDevConsole? Devconsole;
        public MenuTouchControls? TouchControls;
        public Subtitles? Subtitles;

        public bool Pause;
        public bool MenusOpen;
        public Queue<ItemStack> DropStack = new Queue<ItemStack>();

        public bool RequestingExit => _done;

        public MenuManager()
        {
            var eset = SharedResources.Eset!;
            var settings = SharedResources.Settings!;
            var msg = SharedResources.Msg!;

            _keyLock = false;
            _mouseDragging = false;
            _keyboardDragging = false;
            _stickyDragging = false;
            _dragStack = new ItemStack();
            _dragPower = 0;
            _dragSrc = DragSrcNone;
            _dragIcon = new WidgetSlot(WidgetSlot.NoIcon, WidgetSlot.HighlightNormal);
            _done = false;
            _actDragHover = false;
            _keydragPos = default;
            _actionSrc = ActionSrcNone;
            _dragPostAction = DragPostActionNone;
            Inv = null;
            Pow = null;
            Chr = null;
            Questlog = null;
            Hudlog = null;
            Act = null;
            Book = null;
            Hp = null;
            Mp = null;
            Xp = null;
            Mini = null;
            NumPicker = null;
            Enemy = null;
            Vendor = null;
            Talker = null;
            Exit = null;
            Effects = null;
            Stash = null;
            GameOver = null;
            ActionPicker = null;
            RegionTitle = null;
            Devconsole = null;
            TouchControls = null;
            Subtitles = null;
            Pause = false;
            MenusOpen = false;

            Hp = new MenuStatBar(MenuStatBar.TypeHp, 0);
            Mp = new MenuStatBar(MenuStatBar.TypeMp, 0);
            Xp = new MenuStatBar(MenuStatBar.TypeXp, 0);
            Effects = new MenuActiveEffects();
            Hudlog = new MenuHUDLog();
            Act = new MenuActionBar();
            Enemy = new MenuEnemy();
            Vendor = new MenuVendor();
            Talker = new MenuTalker();
            Exit = new MenuExit();
            Mini = new MenuMiniMap();
            Chr = new MenuCharacter();
            Inv = new MenuInventory();
            Pow = new MenuPowers();
            Questlog = new MenuLog();
            Stash = new MenuStash();
            Book = new MenuBook();
            NumPicker = new MenuNumPicker();
            GameOver = new MenuGameOver();
            ActionPicker = new MenuConfirm();
            RegionTitle = new MenuRegionTitle();

            ResourceStatbars.Capacity = eset.ResourceStats.Stats.Count;
            while (ResourceStatbars.Count < eset.ResourceStats.Stats.Count)
                ResourceStatbars.Add(null!);
            for (int i = 0; i < ResourceStatbars.Count; ++i)
            {
                ResourceStatbars[i] = new MenuStatBar(MenuStatBar.TypeResourceStat, i);
            }

            Menus.Add(Hp);
            Menus.Add(Mp);
            Menus.Add(Xp);
            for (int i = 0; i < ResourceStatbars.Count; ++i)
            {
                Menus.Add(ResourceStatbars[i]);
            }
            Menus.Add(Effects);
            Menus.Add(Hudlog);
            Menus.Add(Act);
            Menus.Add(Enemy);
            Menus.Add(Mini);
            Menus.Add(RegionTitle);
            Menus.Add(Chr);
            Menus.Add(Inv);
            Menus.Add(Pow);
            Menus.Add(Questlog);
            Menus.Add(Stash);
            Menus.Add(Vendor);
            Menus.Add(Talker);
            Menus.Add(Book);
            Menus.Add(NumPicker);
            Menus.Add(GameOver);
            Menus.Add(ActionPicker);
            Menus.Add(Exit);

            if (settings.DevMode)
            {
                Devconsole = new MenuDevConsole();
            }

            TouchControls = new MenuTouchControls();

            Subtitles = new Subtitles();

            CloseAll(); // make sure all togglable Menus start closed

            settings.ShowHud = true;

            _dragIcon!.Enabled = false;
            _dragIcon!.ShowDisabledOverlay = false;

            ActionPicker!.SetTitle(msg.Get("Choose an action:"));

            // enabled menu buttons on actionbar
            Act!.SetupMenuButtons(Chr, Inv, Pow, Questlog);
        }
        
        public void AlignAll() {
            for (int i=0; i<Menus.Count; i++) {
                Menus[i]!.Align();
            }
        
            if (SharedResources.Settings!.DevMode) {
                Devconsole!.Align();
            }
        
            TouchControls!.Align();
        }
        
        private void RenderIcon(int x, int y) {
            if (_dragIcon!.GetIcon() != WidgetSlot.NoIcon) {
                _dragIcon!.SetPos(x,y);
                _dragIcon!.Render();
            }
        }
        
        private void SetDragIcon(int iconId, int overlayId) {
            _dragIcon!.SetIcon(iconId, overlayId);
            _dragIcon!.SetAmount(0, 0);
        }
        
        private void SetDragIconItem(ItemStack stack) {
            var items = SharedGameResources.Items!;

            if (stack.Empty() || !items.IsValid(stack.Item)) {
                _dragIcon!.SetIcon(WidgetSlot.NoIcon, WidgetSlot.NoOverlay);
                _dragIcon!.SetAmount(0, 0);
            }
            else {
                _dragIcon!.SetIcon(items.Items[(int)stack.Item]!.Icon, items.GetItemIconOverlay(stack.Item));
                _dragIcon!.SetAmount(stack.Quantity, items.Items[(int)stack.Item]!.MaxQuantity);
            }
        }
        
        private void HandleKeyboardNavigation() {
        
            for (int i = 0; i < Stash!.Tabs.Count; ++i) {
                Stash!.Tabs[i].Tablist.SetNextTabList(null);
            }
            for (int i = 0; i < MenuVendor.TabCount; ++i) {
                Vendor!.TablistTabs[i].SetNextTabList(null);
            }
            Chr!.Tablist.SetNextTabList(null);
            Questlog!.SetNextTabList(null);
            Inv!.Tablist.SetPrevTabList(null);
            Pow!.SetNextTabList(null);
        
            // unlock Menus if only one side is showing
            if (!Inv!.Visible && !Pow!.Visible) {
                Stash!.Tablist.Unlock();
                Vendor!.Tablist.Unlock();
                Chr!.Tablist.Unlock();
                if (!Questlog!.GetCurrentTabList())
                    Questlog!.Tablist.Unlock();
        
            }
            else if (!Vendor!.Visible && !Stash!.Visible && !Chr!.Visible && !Questlog!.Visible) {
                if (Inv!.Visible)
                    Inv!.Tablist.Unlock();
                else if (Pow!.Visible && !Pow!.GetCurrentTabList())
                    Pow!.Tablist.Unlock();
            }
        
            if (_dragSrc == DragSrcNone) {
                if (Inv!.Visible) {
                    for (int i = 0; i < Stash!.Tabs.Count; ++i) {
                        Stash!.Tabs[i].Tablist.SetNextTabList(Inv!.Tablist);
                    }
                    for (int i = 0; i < MenuVendor.TabCount; ++i) {
                        Vendor!.TablistTabs[i].SetNextTabList(Inv!.Tablist);
                    }
                    Chr!.Tablist.SetNextTabList(Inv!.Tablist);
                    Questlog!.SetNextTabList(Inv!.Tablist);
        
                    if (Stash!.Visible) {
                        Inv!.Tablist.SetPrevTabList(Stash!.Tablist);
                    }
                    else if (Vendor!.Visible) {
                        Inv!.Tablist.SetPrevTabList(Vendor!.Tablist);
                    }
                    else if (Chr!.Visible) {
                        Inv!.Tablist.SetPrevTabList(Chr!.Tablist);
                    }
                    else if (Questlog!.Visible) {
                        Inv!.Tablist.SetPrevTabList(Questlog!.GetVisibleChildTabList());
                    }
                }
                else if (Pow!.Visible) {
                    for (int i = 0; i < Stash!.Tabs.Count; ++i) {
                        Stash!.Tabs[i].Tablist.SetNextTabList(Pow!.Tablist);
                    }
                    for (int i = 0; i < MenuVendor.TabCount; ++i) {
                        Vendor!.TablistTabs[i].SetNextTabList(Pow!.Tablist);
                    }
                    Chr!.Tablist.SetNextTabList(Pow!.Tablist);
                    Questlog!.SetNextTabList(Pow!.Tablist);
        
                    // NOTE Stash and Vendor are only visible with inventory, so we don't need to handle them here
                    if (Chr!.Visible) {
                        Pow!.Tablist.SetPrevTabList(Chr!.Tablist);
                    }
                    else if (Questlog!.Visible) {
                        Pow!.Tablist.SetPrevTabList(Questlog!.GetVisibleChildTabList());
                    }
                }
            }
        
            // Stash and Vendor always start locked
            if (!Stash!.Visible) {
                Stash!.Tablist.Lock();
                for (int i = 0; i < Stash!.Tabs.Count; ++i) {
                    Stash!.Tabs[i].Tablist.Lock();
                }
            }
            if (!Vendor!.Visible) {
                Vendor!.Tablist.Lock();
                for (int i = 0; i < MenuVendor.TabCount; ++i) {
                    Vendor!.TablistTabs[i].Lock();
                }
            }
        
            // inventory always starts unlocked
            if (!Inv!.Visible) Inv!.Tablist.Unlock();
        
            if (Act!.GetCurrentTabList()) {
                Inv!.Tablist.Lock();
            }
            else {
                Act!.Tablist.Lock();
            }
        }
        
        public void Logic() {
            var inpt = SharedResources.Inpt!;
            var snd = SharedResources.Snd!;
            var pc = SharedGameResources.Pc!;
            var settings = SharedResources.Settings!;
            var eset = SharedResources.Eset!;
            var msg = SharedResources.Msg!;
            var items = SharedGameResources.Items!;

            ItemStack stack;
        
            Subtitles!.Logic(snd.GetLastPlayedSID());
        
            // refresh statbar values from player statblock
            Hp!.Update();
            Mp!.Update();
            Xp!.Update();
            for (int i = 0; i < ResourceStatbars.Count; ++i) {
                ResourceStatbars[i]!.Update();
            }
        
            // close Talker/Vendor menu if player is attacked
            if (pc.Stats.AbortNpcInteract && eset.Misc.CombatAbortsNpcInteract) {
                Talker!.SetNPC(null);
                Vendor!.SetNPC(null);
            }
            pc.Stats.AbortNpcInteract = false;
        
            if (Act!.Tablist.GetCurrent() != -1) {
                // books/npcs can be activated by powers in the actionbar, so we need to defocus the bar if one of those is opened
                if (Talker!.Visible || Book!.Visible) {
                    Act!.Tablist.Defocus();
                }
        
                // Vendor/Stash items can't be placed on the action bar.
                // This restriction is actually handled during the drop, but clearing the drag contents is a quicker indication to the player
                if (_dragSrc == DragSrcVendor || _dragSrc == DragSrcStash) {
                    ResetDrag();
                }
            }
        
            // when selecting item quantities, don't process other Menus
            if (NumPicker!.Visible) {
                NumPicker!.Logic();
        
                if (NumPicker!.ConfirmClicked) {
                    // start dragging items
                    // removes the desired quantity from the source stack
        
                    if (_dragSrc == DragSrcInventory) {
                        _dragStack.Quantity = NumPicker!.GetValue();
                        Inv!.RemoveFromPrevSlot(_dragStack.Quantity);
                    }
                    else if (_dragSrc == DragSrcVendor) {
                        _dragStack.Quantity = NumPicker!.GetValue();
                        Vendor!.RemoveFromPrevSlot(_dragStack.Quantity);
                    }
                    else if (_dragSrc == DragSrcStash) {
                        _dragStack.Quantity = NumPicker!.GetValue();
                        Stash!.RemoveFromPrevSlot(_dragStack.Quantity);
                    }
        
                    NumPicker!.ConfirmClicked = false;
                    NumPicker!.Visible = false;
                    if (inpt.UsingMouse()) {
                        _stickyDragging = true;
                    }
                    NumPicker!.Tablist.Defocus();
                }
                else if (NumPicker!.CancelClicked) {
                    // cancel item dragging
                    _dragStack.Clear();
                    NumPicker!.CloseWindow();
                    ResetDrag();
                }
                else {
                    Pause = true;
                    return;
                }
            }
            else if (ActionPicker!.Visible) {
                ActionPicker!.Logic();
                if (ActionPicker!.ClickedConfirm) {
                    uint action = _actionPickerMap[(int)ActionPicker!.ActionList!.GetSelected()];
        
                    if (_actionSrc == ActionSrcInventory) {
                        if (action == ActionPickerInventorySelect || action == ActionPickerInventoryDrop || action == ActionPickerInventorySell || action == ActionPickerInventoryStash || action == ActionPickerInventoryActionbar) {
                            _dragStack = Inv!.Click(_actionPickerTarget);
                            if (!_dragStack.Empty()) {
                                ActionPickerStartDrag();
                                _dragSrc = DragSrcInventory;
                            }
                            if (_dragStack.Quantity > 1) {
                                NumPicker!.SetValueBounds(1, _dragStack.Quantity);
                                NumPicker!.Visible = true;
                            }
        
                            if (action == ActionPickerInventoryDrop)
                                _dragPostAction = DragPostActionDrop;
                            else if (action == ActionPickerInventorySell)
                                _dragPostAction = DragPostActionSell;
                            else if (action == ActionPickerInventoryStash)
                                _dragPostAction = DragPostActionStash;
                            else if (action == ActionPickerInventoryActionbar) {
                                if (inpt.Mode != InputState.ModeTouchscreen) {
                                    // move the cursor to the actionbar
                                    Act!.Tablist.Unlock();
                                    Act!.Tablist.GetNext(!TabList.GetInner, TabList.WidgetSelectAuto);
                                    DefocusLeft();
                                    DefocusRight();
                                    Inv!.Visible = false;
                                }
                            }
                        }
                        else if (action == ActionPickerInventoryActivate) {
                            Inv!.Activate(_actionPickerTarget);
                        }
                    }
                    else if (_actionSrc == ActionSrcStash) {
                        if (action == ActionPickerStashSelect || action == ActionPickerStashTransfer) {
                            _dragStack = Stash!.Click(_actionPickerTarget);
                            if (!_dragStack.Empty()) {
                                ActionPickerStartDrag();
                                _dragSrc = DragSrcStash;
                            }
                            if (_dragStack.Quantity > 1) {
                                NumPicker!.SetValueBounds(1, _dragStack.Quantity);
                                NumPicker!.Visible = true;
                            }
        
                            if (action == ActionPickerStashTransfer)
                                _dragPostAction = DragPostActionStash;
                        }
                    }
                    else if (_actionSrc == ActionSrcVendor) {
                        if (action == ActionPickerVendorBuy) {
                            _dragStack = Vendor!.Click(_actionPickerTarget);
                            if (!_dragStack.Empty()) {
                                ActionPickerStartDrag();
                                _dragSrc = DragSrcVendor;
                                Vendor!.LockTabControl();
                            }
                            if (_dragStack.Quantity > 1 || Vendor!.GetTab() == ItemManager.VendorCraft) {
                                int maxQuantity = Inv!.GetMaxPurchasable(_dragStack, Vendor!.GetTab());
                                if (Vendor!.GetTab() != ItemManager.VendorCraft)
                                    maxQuantity = Math.Min(maxQuantity, _dragStack.Quantity);
        
                                if (maxQuantity >= 1) {
                                    NumPicker!.SetValueBounds(1, maxQuantity);
                                    NumPicker!.Visible = true;
                                }
                                else {
                                    Vendor!.RemoveFromPrevSlot(_dragStack.Quantity);
                                    // _dragStack.Clear();
                                    // ResetDrag();
                                }
                            }
        
                            if (!_dragStack.Empty()) {
                                _dragPostAction = DragPostActionBuy;
                            }
                        }
                    }
                    else if (_actionSrc == ActionSrcPowers) {
                        if (action == ActionPickerPowersSelect) {
                            MenuPowersClick powClick = Pow!.Click(_actionPickerTarget);
                            _dragPower = powClick.Drag;
                            if (_dragPower > 0) {
                                ActionPickerStartDrag();
                                _dragSrc = DragSrcPowers;
        
                                if (inpt.Mode != InputState.ModeTouchscreen) {
                                    // move the cursor to the actionbar
                                    Act!.Tablist.Unlock();
                                    Act!.Tablist.GetNext(!TabList.GetInner, TabList.WidgetSelectAuto);
                                    DefocusLeft();
                                    DefocusRight();
                                    Pow!.Visible = false;
                                }
                            }
                        }
                        else if (action == ActionPickerPowersUpgrade) {
                            MenuPowersClick powClick = Pow!.Click(_actionPickerTarget);
                            if (powClick.Unlock > 0) {
                                Pow!.ClickUnlock(powClick.Unlock);
                            }
                        }
                    }
                    else if (_actionSrc == ActionSrcActionbar) {
                        if (action == ActionPickerActionbarSelect) {
                            _dragPower = Act!.CheckDrag(_actionPickerTarget);
                            if (_dragPower > 0) {
                                ActionPickerStartDrag();
                                _dragSrc = DragSrcActionbar;
                            }
                        }
                        else if (action == ActionPickerActionbarClear) {
                            Act!.Remove(_actionPickerTarget);
                        }
                        else if (action == ActionPickerActionbarUse) {
                            WidgetSlot? actSlot = Act!.GetSlotFromPosition(_actionPickerTarget);
                            if (actSlot != null) {
                                Act!.TouchSlot = actSlot;
                            }
                        }
                    }
        
                    ActionPicker!.Visible = false;
                    ActionPicker!.Tablist.Defocus();
                }
                else if (ActionPicker!.ClickedCancel) {
                    ActionPicker!.Tablist.Defocus();
                }
                else {
                    Pause = true;
                    return;
                }
            }
            else {
                // Sometimes, an action from ActionPicker needs to show NumPicker first.
                // NumPicker will only start dragging the item stack, so we need to complete the selected action here
        
                // when using a touchscreen, MenuItemStorage.Click() will highlight the selected slot. We can clear the highlight here
                if (_dragPostAction != DragPostActionNone && inpt.Mode == InputState.ModeTouchscreen) {
                    DefocusLeft();
                    DefocusRight();
                }
        
                if (_dragPostAction == DragPostActionDrop) {
                    if (_dragSrc == DragSrcInventory) {
                        // quest items cannot be dropped
                        bool itemIsValid = items.IsValid(_dragStack.Item);
                        if (!itemIsValid || (itemIsValid && !items.Items[(int)_dragStack.Item]!.QuestItem)) {
                            DropStack.Enqueue(_dragStack.Clone());
                        }
                        else {
                            pc.LogMsg(msg.Get("This item can not be dropped."), Avatar.MsgNormal);
                            items.PlaySound(_dragStack.Item);
        
                            Inv!.ItemReturn(_dragStack);
                        }
                        if (inpt.Mode == InputState.ModeTouchscreen)
                            Inv!.DefocusTabLists();
                    }
                    else if (_dragSrc == DragSrcStash) {
                        DropStack.Enqueue(_dragStack.Clone());
                        Stash!.Tabs[Stash!.GetTab()].Updated = true;
                    }
                    _dragSrc = DragSrcNone;
                    ResetDrag();
                }
                else if (_dragPostAction == DragPostActionBuy) {
                    if (!Inv!.Buy(_dragStack, Vendor!.GetTab(), !MenuInventory.IsDragging)) {
                        Vendor!.ItemReturn(Inv!.DropStack.Peek());
                        Inv!.DropStack.Dequeue();
                    }
                    _dragSrc = DragSrcNone;
                    ResetDrag();
                    Vendor!.ResetDrag();
                }
                else if (_dragPostAction == DragPostActionSell) {
                    if (Vendor!.SellEnabled && Inv!.Sell(_dragStack)) {
                        if (Vendor!.Visible) {
                            Vendor!.SetTab(ItemManager.VendorSell);
                            Vendor!.Add(_dragStack);
                        }
                    }
                    else {
                        Inv!.ItemReturn(_dragStack);
                    }
                    _dragSrc = DragSrcNone;
                    ResetDrag();
                }
                else if (_dragPostAction == DragPostActionStash) {
                    if (_dragSrc == DragSrcInventory) {
                        if (!Stash!.Add(_dragStack, MenuStash.NoSlot, MenuStash.AddPlaySound)) {
                            Inv!.ItemReturn(Stash!.DropStack.Peek());
                            Stash!.DropStack.Dequeue();
                        }
                    }
                    else if (_dragSrc == DragSrcStash) {
                        if (!Inv!.Add(_dragStack, MenuInventory.Carried, ItemStorage.NoSlot, MenuInventory.AddPlaySound, MenuInventory.AddAutoEquip)) {
                            Stash!.ItemReturn(Inv!.DropStack.Peek());
                            Inv!.DropStack.Dequeue();
                        }
                        Stash!.Tabs[Stash!.GetTab()].Updated = true;
                    }
                    _dragSrc = DragSrcNone;
                    ResetDrag();
                }
        
                _dragPostAction = DragPostActionNone;
            }
        
            if (!inpt.UsingMouse())
                HandleKeyboardNavigation();
        
            // Check if the mouse is within any of the visible windows. Excludes the minimap and the Exit/Pause menu
            bool isWithinMenus = ((Book!.Visible && Utils.IsWithinRect(Book!.WindowArea, inpt.Mouse)) ||
            (Chr!.Visible && Utils.IsWithinRect(Chr!.WindowArea, inpt.Mouse)) ||
            (Inv!.Visible && Utils.IsWithinRect(Inv!.WindowArea, inpt.Mouse)) ||
            (Vendor!.Visible && Utils.IsWithinRect(Vendor!.WindowArea, inpt.Mouse)) ||
            (Pow!.Visible && Utils.IsWithinRect(Pow!.WindowArea, inpt.Mouse)) ||
            (Questlog!.Visible && Utils.IsWithinRect(Questlog!.WindowArea, inpt.Mouse)) ||
            (Talker!.Visible && Utils.IsWithinRect(Talker!.WindowArea, inpt.Mouse)) ||
            (Stash!.Visible && Utils.IsWithinRect(Stash!.WindowArea, inpt.Mouse)) ||
            (settings.DevMode && Devconsole!.Visible && Utils.IsWithinRect(Devconsole!.WindowArea, inpt.Mouse)));
        
            // Stop attacking if the cursor is inside an interactable menu
            if ((pc.UsingMain1 || pc.UsingMain2) && isWithinMenus) {
                inpt.Pressing[Input.Main1] = false;
                inpt.Pressing[Input.Main2] = false;
            }
        
            Stash!.TabControlLocked = (_mouseDragging || _keyboardDragging);
        
            if (settings.DevMode) {
                Devconsole!.Logic();
            }
        
            if (!Exit!.Visible && !isWithinMenus)
                Mini!.Logic();
        
            Book!.Logic();
            Effects!.Logic();
        
            if (!Exit!.Visible && !GameOver!.Visible)
                Act!.Logic();
        
            Hudlog!.Logic();
            Enemy!.Logic();
            Chr!.Logic();
            Inv!.Logic();
            Vendor!.Logic();
            Pow!.Logic();
            Questlog!.Logic();
            Talker!.Logic();
            Stash!.Logic();
            GameOver!.Logic();
        
            if (!Pause)
                RegionTitle!.Logic();
        
            TouchControls!.Logic();
        
            if (Chr!.CheckUpgrade() || pc.Stats.LevelUp) {
                // apply equipment and max Hp/Mp
                Inv!.ApplyEquipment();
                pc.Stats.Hp = pc.Stats.Get(global::FlareEngine.Stats.HpMax);
                pc.Stats.Mp = pc.Stats.Get(global::FlareEngine.Stats.MpMax);
                pc.Stats.LevelUp = false;
            }
        
            // only allow the Vendor window to be open if the inventory is open
            if (Vendor!.Visible && !(Inv!.Visible)) {
                snd.Play(Vendor!.SfxClose, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);
                CloseAll();
            }
        
            // context-sensistive help tooltip in inventory menu
            if (Inv!.Visible && Vendor!.Visible) {
                Inv!.InvCtrl = MenuInventory.CtrlVendor;
            }
            else if (Inv!.Visible && Stash!.Visible) {
                Inv!.InvCtrl = MenuInventory.CtrlStash;
            }
            else {
                Inv!.InvCtrl = MenuInventory.CtrlNone;
            }
        
            if (!inpt.Pressing[Input.Inventory] && !inpt.Pressing[Input.Powers] && !inpt.Pressing[Input.Character] && !inpt.Pressing[Input.Log])
                _keyLock = false;
        
            if (settings.DevMode && Devconsole!.InputFocus())
                _keyLock = true;
        
            // stop dragging with cancel key
            if (!_keyLock && inpt.Pressing[Input.Cancel] && !inpt.Lock[Input.Cancel] && !pc.Stats.Corpse) {
                if (_keyboardDragging || _mouseDragging) {
                    inpt.Lock[Input.Cancel] = true;
                    ResetDrag();
                }
            }
        
            // Exit menu toggle
            if (!_keyLock && !_mouseDragging && !_keyboardDragging) {
                if (inpt.Pressing[Input.Cancel] && !inpt.Lock[Input.Cancel]) {
                    _keyLock = true;
                    if (Act!.TwostepSlot != -1) {
                        inpt.Lock[Input.Cancel] = true;
                        Act!.TwostepSlot = -1;
                    }
                    else if (settings.DevMode && Devconsole!.Visible) {
                        inpt.Lock[Input.Cancel] = true;
                        Devconsole!.CloseWindow();
                    }
                    else if (MenusOpen) {
                        inpt.Lock[Input.Cancel] = true;
        
                        // play *one* close sound effect
                        if (Chr!.Visible) {
                            snd.Play(Chr!.SfxClose, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);
                        }
                        else if (Questlog!.Visible) {
                            snd.Play(Questlog!.SfxClose, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);
                        }
                        else {
                            if (Inv!.Visible) {
                                snd.Play(Inv!.SfxClose, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);
                            }
                            else if (Pow!.Visible) {
                                snd.Play(Pow!.SfxClose, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);
                            }
                        }
                        CloseAll();
                    }
                    else if (!GameOver!.Visible && (inpt.Mode != InputState.ModeJoystick || Exit!.Visible)) {
                        // CANCEL (which is usually mapped to keyboard Escape) can open the Pause menu if there are no other actions
                        // we make an exception for opening the Pause menu with joysticks, which usually have CANCEL mapped to a face button with a separate mapping for PAUSE (i.e. "Start")
                        // closing the menu can always be _done with either mapping
                        inpt.Lock[Input.Cancel] = true;
                        Act!.DefocusTabLists();
                        Exit!.HandleCancel();
                    }
                }
        
                // handle opening the Pause menu with the dedicated binding
                if (inpt.Pressing[Input.Pause] && !inpt.Lock[Input.Pause] && !inpt.Lock[Input.Cancel]) {
                    inpt.Lock[Input.Pause] = true;
        
                    // perform all "cancel" actions
                    ResetDrag();
                    Act!.TwostepSlot = -1;
                    if (MenusOpen) {
                        CloseAll();
                    }
        
                    if (!GameOver!.Visible) {
                        Act!.DefocusTabLists();
                        Exit!.HandleCancel();
                    }
                }
            }
            if (Mini!.ClickedConfig) {
                Mini!.ClickedConfig = false;
                ShowExitMenu();
            }
        
            if (Exit!.Visible) {
                Exit!.Logic();
                if (Exit!.IsExitRequested()) {
                    _done = true;
                }
                // if dpi scaling is changed, we need to realign the Menus
                if (inpt.WindowResized) {
                    AlignAll();
                }
            }
            else if (GameOver!.Visible) {
                if (GameOver!.ExitClicked) {
                    GameOver!.Close();
                    _done = true;
                }
            }
            else {
                if (!settings.DevMode || !Devconsole!.Visible) {
                    bool clickingCharacter = false;
                    bool clickingInventory = false;
                    bool clickingPowers = false;
                    bool clickingLog = false;
        
                    // check if mouse-clicking a menu button
                    Act!.CheckMenu(ref clickingCharacter, ref clickingInventory, ref clickingPowers, ref clickingLog);
        
                    // inventory menu toggle
                    if (Inv!.Enabled && ((inpt.Pressing[Input.Inventory] && !_keyLock && !_mouseDragging && !_keyboardDragging) || clickingInventory)) {
                        _keyLock = true;
                        if (Inv!.Visible) {
                            snd.Play(Inv!.SfxClose, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);
                            CloseRight();
                        }
                        else {
                            CloseRight();
                            DefocusLeft();
                            Act!.RequiresAttention[MenuActionBar.MenuInventory] = false;
                            Inv!.Visible = true;
                            snd.Play(Inv!.SfxOpen, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);
                        }
        
                    }
        
                    // powers menu toggle
                    if (Pow!.Enabled && (((inpt.Pressing[Input.Powers] && !_keyLock && !_mouseDragging && !_keyboardDragging) || clickingPowers) && !pc.Stats.Transformed)) {
                        _keyLock = true;
                        if (Pow!.Visible) {
                            snd.Play(Pow!.SfxClose, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);
                            CloseRight();
                        }
                        else {
                            CloseRight();
                            DefocusLeft();
                            Act!.RequiresAttention[MenuActionBar.MenuPowers] = false;
                            Pow!.Visible = true;
                            snd.Play(Pow!.SfxOpen, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);
                        }
                    }
        
                    // character menu toggleggle
                    if (Chr!.Enabled && ((inpt.Pressing[Input.Character] && !_keyLock && !_mouseDragging && !_keyboardDragging) || clickingCharacter)) {
                        _keyLock = true;
                        if (Chr!.Visible) {
                            snd.Play(Chr!.SfxClose, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);
                            CloseLeft();
                        }
                        else {
                            CloseLeft();
                            DefocusRight();
                            Act!.RequiresAttention[MenuActionBar.MenuCharacter] = false;
                            Chr!.Visible = true;
                            snd.Play(Chr!.SfxOpen, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);
                            // Make sure the stat list isn't scrolled when we open the character menu
                            inpt.ResetScroll();
                        }
                    }
        
                    // log menu toggle
                    if (Questlog!.Enabled && ((inpt.Pressing[Input.Log] && !_keyLock && !_mouseDragging && !_keyboardDragging) || clickingLog)) {
                        _keyLock = true;
                        if (Questlog!.Visible) {
                            snd.Play(Questlog!.SfxClose, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);
                            CloseLeft();
                        }
                        else {
                            CloseLeft();
                            DefocusRight();
                            Act!.RequiresAttention[MenuActionBar.MenuLog] = false;
                            Questlog!.Visible = true;
                            snd.Play(Questlog!.SfxOpen, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);
                            // Make sure the log isn't scrolled when we open the log menu
                            inpt.ResetScroll();
                        }
                    }
        
                    if (inpt.Pressing[Input.CycleMenus] && !inpt.Lock[Input.CycleMenus] && !_mouseDragging && !_keyboardDragging) {
                        inpt.Lock[Input.CycleMenus] = true;
        
                        if (Chr!.Enabled && !Chr!.Visible && Inv!.Enabled && !Inv!.Visible && !Pow!.Visible && !Questlog!.Visible) {
                            CloseLeft();
                            DefocusRight();
                            Act!.RequiresAttention[MenuActionBar.MenuCharacter] = false;
                            Chr!.Visible = true;
                            snd.Play(Chr!.SfxOpen, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);
                            // Make sure the stat list isn't scrolled when we open the character menu
                            inpt.ResetScroll();
        
                            CloseRight();
                            DefocusLeft();
                            Act!.RequiresAttention[MenuActionBar.MenuInventory] = false;
                            Inv!.Visible = true;
                            // SharedResources.Snd!.Play(Inv!.SfxOpen, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);
                        }
                        else if (Pow!.Enabled && !Pow!.Visible && Questlog!.Enabled && !Questlog!.Visible && (Chr!.Visible || !Chr!.Enabled) && (Inv!.Visible || !Inv!.Enabled)) {
                            CloseLeft();
                            DefocusRight();
                            Act!.RequiresAttention[MenuActionBar.MenuLog] = false;
                            Questlog!.Visible = true;
                            snd.Play(Questlog!.SfxOpen, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);
                            // Make sure the log isn't scrolled when we open the log menu
                            inpt.ResetScroll();
        
                            CloseRight();
                            DefocusLeft();
                            Act!.RequiresAttention[MenuActionBar.MenuPowers] = false;
                            Pow!.Visible = true;
                            // SharedResources.Snd!.Play(Pow!.SfxOpen, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);
                        }
                        else {
                            CloseAll();
                        }
                    }
                }
        
                //developer console
                if (settings.DevMode && inpt.Pressing[Input.DeveloperMenu] && !inpt.Lock[Input.DeveloperMenu] && !_mouseDragging && !_keyboardDragging) {
                    inpt.Lock[Input.DeveloperMenu] = true;
                    if (Devconsole!.Visible) {
                        CloseAll();
                        _keyLock = false;
                    }
                    else {
                        CloseAll();
                        Devconsole!.Visible = true;
                    }
                }
            }
        
            bool consoleOpen = settings.DevMode && Devconsole!.Visible;
            MenusOpen = (Inv!.Visible || Pow!.Visible || Chr!.Visible || Questlog!.Visible || Vendor!.Visible || Talker!.Visible || Book!.Visible || consoleOpen);
            Pause = (eset.Misc.MenusPause && MenusOpen) || Exit!.Visible || consoleOpen || Book!.Visible;
        
            TouchControls!.Visible = !MenusOpen && !Exit!.Visible && inpt.UsingTouchscreen();
        
            if (pc.Stats.Alive && !inpt.UsingMouse()) {
                if (MenusOpen) {
                    if (inpt.Pressing[Input.Main1]) inpt.Lock[Input.Main1] = true;
                    if (inpt.Pressing[Input.Main2]) inpt.Lock[Input.Main2] = true;
                }
        
                DragAndDropWithKeyboard();
            }
            else if (pc.Stats.Alive) {
                // handle right-click
                if (!_mouseDragging && inpt.Pressing[Input.Main2]) {
                    // Exit menu
                    if (Exit!.Visible && Utils.IsWithinRect(Exit!.WindowArea, inpt.Mouse)) {
                        inpt.Lock[Input.Main2] = true;
                        inpt.Pressing[Input.Main2] = false;
                    }
        
                    // Book menu
                    else if (Book!.Visible && Utils.IsWithinRect(Book!.WindowArea, inpt.Mouse)) {
                        inpt.Lock[Input.Main2] = true;
                        inpt.Pressing[Input.Main2] = false;
                    }
        
                    // inventory
                    else if (Inv!.Visible && Utils.IsWithinRect(Inv!.WindowArea, inpt.Mouse)) {
                        if (!inpt.Lock[Input.Main2] && Utils.IsWithinRect(Inv!.CarriedArea, inpt.Mouse)) {
                            // activate inventory item
                            Inv!.Activate(inpt.Mouse);
                        }
                        inpt.Lock[Input.Main2] = true;
                        inpt.Pressing[Input.Main2] = false;
                    }
        
                    // other Menus
                    else if (Talker!.Visible && Utils.IsWithinRect(Talker!.WindowArea, inpt.Mouse)) {
                        inpt.Lock[Input.Main2] = true;
                        inpt.Pressing[Input.Main2] = false;
                    }
                    else if (Pow!.Visible && Utils.IsWithinRect(Pow!.WindowArea, inpt.Mouse)) {
                        inpt.Lock[Input.Main2] = true;
                        inpt.Pressing[Input.Main2] = false;
                    }
                    else if (Chr!.Visible && Utils.IsWithinRect(Chr!.WindowArea, inpt.Mouse)) {
                        inpt.Lock[Input.Main2] = true;
                        inpt.Pressing[Input.Main2] = false;
                    }
                    else if (Questlog!.Visible && Utils.IsWithinRect(Questlog!.WindowArea, inpt.Mouse)) {
                        inpt.Lock[Input.Main2] = true;
                        inpt.Pressing[Input.Main2] = false;
                    }
                    else if (Vendor!.Visible && Utils.IsWithinRect(Vendor!.WindowArea, inpt.Mouse)) {
                        inpt.Lock[Input.Main2] = true;
                        inpt.Pressing[Input.Main2] = false;
                    }
                    else if (Stash!.Visible && Utils.IsWithinRect(Stash!.WindowArea, inpt.Mouse)) {
                        inpt.Lock[Input.Main2] = true;
                        inpt.Pressing[Input.Main2] = false;
                    }
                }
        
                // handle left-click for Book menu first
                if (!_mouseDragging && inpt.Pressing[Input.Main1] && !inpt.Lock[Input.Main1]) {
                    if (Book!.Visible && Utils.IsWithinRect(Book!.WindowArea, inpt.Mouse)) {
                        inpt.Lock[Input.Main1] = true;
                    }
                }
        
                // handle left-click
                if (!_mouseDragging && inpt.Pressing[Input.Main1] && !inpt.Lock[Input.Main1]) {
                    ResetDrag();
        
                    for (int i=0; i<Menus.Count; ++i) {
                        if (Menus[i] == Act)
                            continue;
                        if (!Menus[i]!.Visible || !Utils.IsWithinRect(Menus[i]!.WindowArea, inpt.Mouse)) {
                            Menus[i]!.DefocusTabLists();
                        }
                    }
        
                    // Exit menu
                    if (Exit!.Visible && Utils.IsWithinRect(Exit!.WindowArea, inpt.Mouse)) {
                        inpt.Lock[Input.Main1] = true;
                    }
        
        
                    if (Chr!.Visible && Utils.IsWithinRect(Chr!.WindowArea, inpt.Mouse)) {
                        inpt.Lock[Input.Main1] = true;
                    }
        
                    if (Vendor!.Visible && Utils.IsWithinRect(Vendor!.WindowArea, inpt.Mouse)) {
                        inpt.Lock[Input.Main1] = true;
                        if (inpt.Pressing[Input.Ctrl] && Vendor!.GetTab() != ItemManager.VendorCraft) {
                            // buy item from a Vendor
                            stack = Vendor!.Click(inpt.Mouse);
                            if (!Inv!.Buy(stack, Vendor!.GetTab(), !MenuInventory.IsDragging)) {
                                Vendor!.ItemReturn(Inv!.DropStack.Peek());
                                Inv!.DropStack.Dequeue();
                            }
                        }
                        else {
                            if (inpt.TouchLocked) {
                                ShowActionPicker(Vendor, inpt.Mouse);
                            }
                            else {
                                // start dragging a Vendor item
                                _dragStack = Vendor!.Click(inpt.Mouse);
                                if (!_dragStack.Empty()) {
                                    _mouseDragging = true;
                                    _dragSrc = DragSrcVendor;
                                }
                                if ((_dragStack.Quantity > 1 && inpt.Pressing[Input.Shift]) || Vendor!.GetTab() == ItemManager.VendorCraft) {
                                    int maxQuantity = Inv!.GetMaxPurchasable(_dragStack, Vendor!.GetTab());
                                    if (Vendor!.GetTab() != ItemManager.VendorCraft)
                                        maxQuantity = Math.Min(maxQuantity, _dragStack.Quantity);
        
                                    if (maxQuantity >= 1) {
                                        NumPicker!.SetValueBounds(1, maxQuantity);
                                        NumPicker!.Visible = true;
                                    }
                                    else {
                                        _dragStack.Clear();
                                        ResetDrag();
                                    }
                                }
                            }
                        }
                    }
        
                    if (Stash!.Visible && Utils.IsWithinRect(Stash!.WindowArea, inpt.Mouse)) {
                        inpt.Lock[Input.Main1] = true;
                        if (inpt.Pressing[Input.Ctrl]) {
                            // take an item from the Stash
                            stack = Stash!.Click(inpt.Mouse);
                            if (!Inv!.Add(stack, MenuInventory.Carried, ItemStorage.NoSlot, MenuInventory.AddPlaySound, MenuInventory.AddAutoEquip)) {
                                Stash!.ItemReturn(Inv!.DropStack.Peek());
                                Inv!.DropStack.Dequeue();
                            }
                            Stash!.Tabs[Stash!.GetTab()].Updated = true;
                        }
                        else {
                            if (inpt.TouchLocked) {
                                ShowActionPicker(Stash, inpt.Mouse);
                            }
                            else {
                                // start dragging a Stash item
                                _dragStack = Stash!.Click(inpt.Mouse);
                                if (!_dragStack.Empty()) {
                                    _mouseDragging = true;
                                    _dragSrc = DragSrcStash;
                                }
                                if (_dragStack.Quantity > 1 && inpt.Pressing[Input.Shift]) {
                                    NumPicker!.SetValueBounds(1, _dragStack.Quantity);
                                    NumPicker!.Visible = true;
                                }
                            }
                        }
                    }
        
                    if (Questlog!.Visible && Utils.IsWithinRect(Questlog!.WindowArea, inpt.Mouse)) {
                        inpt.Lock[Input.Main1] = true;
                    }
        
                    // pick up an inventory item
                    if (Inv!.Visible && Utils.IsWithinRect(Inv!.WindowArea, inpt.Mouse)) {
                        if (inpt.Pressing[Input.Ctrl]) {
                            inpt.Lock[Input.Main1] = true;
                            stack = Inv!.Click(inpt.Mouse);
                            if (Stash!.Visible) {
                                if (!Stash!.Add(stack, MenuStash.NoSlot, MenuStash.AddPlaySound)) {
                                    Inv!.ItemReturn(Stash!.DropStack.Peek());
                                    Stash!.DropStack.Dequeue();
                                }
                            }
                            else {
                                // The Vendor could have a limited amount of currency in the future. It will be tested here.
                                if ((eset.Misc.SellWithoutVendor || (Vendor!.Visible && Vendor!.SellEnabled)) && Inv!.Sell(stack)) {
                                    if (Vendor!.Visible) {
                                        Vendor!.SetTab(ItemManager.VendorSell);
                                        Vendor!.Add(stack);
                                    }
                                }
                                else {
                                    Inv!.ItemReturn(stack);
                                }
                            }
                        }
                        else {
                            inpt.Lock[Input.Main1] = true;
        
                            if (inpt.TouchLocked) {
                                ShowActionPicker(Inv, inpt.Mouse);
                            }
                            else {
                                _dragStack = Inv!.Click(inpt.Mouse);
                                if (!_dragStack.Empty()) {
                                    _mouseDragging = true;
                                    _dragSrc = DragSrcInventory;
                                }
                                if (_dragStack.Quantity > 1 && inpt.Pressing[Input.Shift]) {
                                    NumPicker!.SetValueBounds(1, _dragStack.Quantity);
                                    NumPicker!.Visible = true;
                                }
                            }
                        }
                    }
                    // pick up a power
                    if (Pow!.Visible && Utils.IsWithinRect(Pow!.WindowArea, inpt.Mouse)) {
                        inpt.Lock[Input.Main1] = true;
        
                        if (inpt.TouchLocked) {
                            ShowActionPicker(Pow, inpt.Mouse);
                        }
                        else {
                            // check for unlock/dragging
                            MenuPowersClick powClick = Pow!.Click(inpt.Mouse);
                            _dragPower = powClick.Drag;
                            if (_dragPower > 0) {
                                _mouseDragging = true;
                                _keyboardDragging = false;
                                _dragSrc = DragSrcPowers;
                            }
                            else if (powClick.Unlock > 0) {
                                Pow!.ClickUnlock(powClick.Unlock);
                            }
                        }
                    }
                    // action bar
                    if (!Exit!.Visible && (Act!.IsWithinSlots(inpt.Mouse) || Act!.IsWithinMenus(inpt.Mouse)) && !pc.UsingMain1 && !pc.UsingMain2) {
                        inpt.Lock[Input.Main1] = true;
        
                        // ctrl-click action bar to clear that slot
                        if (inpt.Pressing[Input.Ctrl]) {
                            Act!.Remove(inpt.Mouse);
                        }
                        // allow drag-to-rearrange action bar
                        else if (!Act!.IsWithinMenus(inpt.Mouse)) {
                            if (inpt.TouchLocked) {
                                WidgetSlot? actSlot = Act!.GetSlotFromPosition(inpt.Mouse);
                                if (actSlot != null) {
                                    Act!.Tablist.SetCurrent(actSlot);
                                    _keydragPos = new Int2(actSlot.Pos.X, actSlot.Pos.Y);
                                }
                                ShowActionPicker(Act, inpt.Mouse);
                            }
                            else {
                                _dragPower = Act!.CheckDrag(inpt.Mouse);
                                if (_dragPower > 0) {
                                    _mouseDragging = true;
                                    _dragSrc = DragSrcActionbar;
                                }
                            }
                        }
        
                        // else, clicking action bar to use a power?
                        // this check is _done by GameEngine when calling Avatar.Logic()
        
        
                    }
                }
        
                // highlight matching inventory slots based on what we're dragging
                if (Inv!.Visible && (_mouseDragging || _keyboardDragging)) {
                    Inv!.Inventory[MenuInventory.Equipment].HighlightMatching(_dragStack.Item);
                }
        
                // handle dropping
                if (_mouseDragging && ((_stickyDragging && inpt.Pressing[Input.Main1] && !inpt.Lock[Input.Main1]) || (!_stickyDragging && !inpt.Pressing[Input.Main1]))) {
                    if (_stickyDragging) {
                        inpt.Lock[Input.Main1] = true;
                        _stickyDragging = false;
                    }
        
                    // putting a power on the Action Bar
                    if (_dragSrc == DragSrcPowers) {
                        if (Act!.IsWithinSlots(inpt.Mouse)) {
                            Act!.Drop(inpt.Mouse, _dragPower, !MenuActionBar.Reorder);
                        }
                    }
        
                    // rearranging the action bar
                    else if (_dragSrc == DragSrcActionbar) {
                        if (Act!.IsWithinSlots(inpt.Mouse)) {
                            Act!.Drop(inpt.Mouse, _dragPower, MenuActionBar.Reorder);
                            // for locked slots forbid power dropping
                        }
                        else if (Act!.Locked[Act!.DragPrevSlot]) {
                            Act!.Hotkeys[Act!.DragPrevSlot] = _dragPower;
                        }
                        _dragPower = 0;
                    }
        
                    // rearranging inventory or dropping items
                    else if (_dragSrc == DragSrcInventory) {
        
                        if (Inv!.Visible && Utils.IsWithinRect(Inv!.WindowArea, inpt.Mouse)) {
                            Inv!.Drop(inpt.Mouse, _dragStack);
                        }
                        else if (Act!.IsWithinSlots(inpt.Mouse)) {
                            // The action bar is not storage!
                            Inv!.ItemReturn(_dragStack);
                            Inv!.ApplyEquipment();
        
                            // put an item with a power on the action bar
                            if (items.IsValid(_dragStack.Item) && items.Items[(int)_dragStack.Item]!.Power != 0) {
                                Act!.Drop(inpt.Mouse, items.Items[(int)_dragStack.Item]!.Power, !MenuActionBar.Reorder);
                            }
                        }
                        else if (Vendor!.Visible && Utils.IsWithinRect(Vendor!.WindowArea, inpt.Mouse)) {
                            if (Vendor!.SellEnabled && Inv!.Sell( _dragStack)) {
                                Vendor!.SetTab(ItemManager.VendorSell);
                                Vendor!.Add( _dragStack);
                            }
                            else {
                                Inv!.ItemReturn(_dragStack);
                            }
                        }
                        else if (Stash!.Visible && Utils.IsWithinRect(Stash!.WindowArea, inpt.Mouse)) {
                            for (int i = 0; i < Stash!.Tabs.Count; ++i) {
                                Stash!.Tabs[i].Stock.DragPrevSlot = -1;
                            }
                            if (!Stash!.Drop(inpt.Mouse, _dragStack)) {
                                Inv!.ItemReturn(Stash!.DropStack.Peek());
                                Stash!.DropStack.Dequeue();
                            }
                        }
                        else {
                            // if dragging and the source was inventory, drop item to the floor
        
                            // quest items cannot be dropped
                            bool itemIsValid = items.IsValid(_dragStack.Item);
                            if (!itemIsValid || (itemIsValid && !items.Items[(int)_dragStack.Item]!.QuestItem)) {
                                DropStack.Enqueue(_dragStack.Clone());
                            }
                            else {
                                pc.LogMsg(msg.Get("This item can not be dropped."), Avatar.MsgNormal);
                                items.PlaySound(_dragStack.Item);
        
                                Inv!.ItemReturn(_dragStack);
                            }
                        }
                        Inv!.ClearHighlight();
                    }
        
                    else if (_dragSrc == DragSrcVendor) {
        
                        // dropping an item from Vendor (we only allow to drop into the carried area)
                        if (Inv!.Visible && Utils.IsWithinRect(Inv!.WindowArea, inpt.Mouse)) {
                            if (!Inv!.Buy(_dragStack, Vendor!.GetTab(), MenuInventory.IsDragging)) {
                                Vendor!.ItemReturn(Inv!.DropStack.Peek());
                                Inv!.DropStack.Dequeue();
                            }
                        }
                        else {
                            items.PlaySound(_dragStack.Item);
                            Vendor!.ItemReturn(_dragStack);
                        }
                    }
        
                    else if (_dragSrc == DragSrcStash) {
        
                        // dropping an item from Stash (we only allow to drop into the carried area)
                        if (Inv!.Visible && Utils.IsWithinRect(Inv!.WindowArea, inpt.Mouse)) {
                            if (!Inv!.Drop(inpt.Mouse, _dragStack)) {
                                Stash!.ItemReturn(Inv!.DropStack.Peek());
                                Inv!.DropStack.Dequeue();
                            }
                            Stash!.Tabs[Stash!.GetTab()].Updated = true;
                        }
                        else if (Stash!.Visible && Utils.IsWithinRect(Stash!.WindowArea, inpt.Mouse)) {
                            if (!Stash!.Drop(inpt.Mouse,_dragStack)) {
                                DropStack.Enqueue(Stash!.DropStack.Peek());
                                Stash!.DropStack.Dequeue();
                            }
                        }
                        else {
                            DropStack.Enqueue(_dragStack.Clone());
                            Stash!.Tabs[Stash!.GetTab()].Updated = true;
                        }
                    }
        
                    _dragStack.Clear();
                    _dragPower = 0;
                    _dragSrc = DragSrcNone;
                    _mouseDragging = false;
                }
            }
            else {
                if (_mouseDragging || _keyboardDragging) {
                    ResetDrag();
                }
            }
        
            // return items that are currently begin dragged when returning to title screen or exiting game
            if (_done || inpt.Done) {
                ResetDrag();
            }
        
            // handle equipment changes affecting hero stats
            if (Inv!.ChangedEquipment) {
                Inv!.ApplyEquipment();
                // the equipment flags get reset in GameStatePlay
            }
        
            if (!(_mouseDragging || _keyboardDragging)) {
                SetDragIcon(WidgetSlot.NoIcon, WidgetSlot.NoOverlay);
            }
        
            // auto-select tablists when using keyboard/gamepad
            if (!inpt.UsingMouse()) {
                if (GameOver!.Visible && !GameOver!.Tablist.IsLocked() && GameOver!.Tablist.GetCurrent() == -1) {
                    GameOver!.Tablist.GetNext(!TabList.GetInner, TabList.WidgetSelectAuto);
                }
                else if (NumPicker!.Visible && !NumPicker!.Tablist.IsLocked() && NumPicker!.Tablist.GetCurrent() == -1) {
                    NumPicker!.Tablist.GetNext(!TabList.GetInner, TabList.WidgetSelectAuto);
                }
                else if (ActionPicker!.Visible && !ActionPicker!.Tablist.IsLocked() && ActionPicker!.Tablist.GetCurrent() == -1) {
                    ActionPicker!.Tablist.GetNext(!TabList.GetInner, TabList.WidgetSelectAuto);
                }
                else if (Book!.Visible && !Book!.Tablist.IsLocked() && Book!.Tablist.GetCurrent() == -1) {
                    Book!.Tablist.GetNext(!TabList.GetInner, TabList.WidgetSelectAuto);
                }
                else if (!IsTabListSelected()) {
                    Inv!.Tablist.Lock();
                    Pow!.Tablist.Lock();
                    Chr!.Tablist.Lock();
                    Questlog!.Tablist.Lock();
                    Stash!.Tablist.Lock();
                    Vendor!.Tablist.Lock();
        
                    if (Inv!.Visible) {
                        Inv!.Tablist.Unlock();
                        Inv!.Tablist.GetNext(!TabList.GetInner, TabList.WidgetSelectAuto);
                    }
                    else if (Pow!.Visible) {
                        Pow!.Tablist.Unlock();
                        Pow!.Tablist.GetNext(!TabList.GetInner, TabList.WidgetSelectAuto);
                    }
                    else if (Chr!.Visible) {
                        Chr!.Tablist.Unlock();
                        Chr!.Tablist.GetNext(!TabList.GetInner, TabList.WidgetSelectAuto);
                    }
                    else if (Questlog!.Visible) {
                        Questlog!.Tablist.Unlock();
                        Questlog!.Tablist.GetNext(!TabList.GetInner, TabList.WidgetSelectAuto);
                    }
                }
            }
        }
        
        private void DragAndDropWithKeyboard() {
            var items = SharedGameResources.Items!;

            // inventory menu
        
            if (Inv!.Visible && Inv!.GetCurrentTabList() && _dragSrc != DragSrcActionbar) {
                int slotIndex = Inv!.GetCurrentTabList().GetCurrent();
                Int2 srcSlot;
                WidgetSlot? invSlot;
        
                if (slotIndex < Inv!.GetEquippedCount())
                    invSlot = Inv!.Inventory[MenuInventory.Equipment].Slots[slotIndex];
                else if (slotIndex < Inv!.GetTotalSlotCount())
                    invSlot = Inv!.Inventory[MenuInventory.Carried].Slots[slotIndex - Inv!.GetEquippedCount()];
                else
                    invSlot = null;
        
                if (invSlot != null) {
                    srcSlot.X = invSlot.Pos.X;
                    srcSlot.Y = invSlot.Pos.Y;
        
                    int slotClick = invSlot.CheckClick();
        
                    // pick up item
                    if (slotClick == WidgetSlot.Drag && _dragStack.Empty()) {
                        ShowActionPicker(Inv, srcSlot);
                    }
                    // rearrange item
                    else if (slotClick == WidgetSlot.Drag && !_dragStack.Empty()) {
                        Inv!.Drop(srcSlot, _dragStack);
                        _dragSrc = DragSrcNone;
                        _dragStack.Clear();
                        _keyboardDragging = false;
                        _stickyDragging = false;
                    }
                    else if (slotClick == WidgetSlot.ActivateResult && _dragStack.Empty()) {
                        Inv!.Activate(srcSlot);
                    }
                }
            }
        
            // Vendor menu
            if (Vendor!.Visible && Vendor!.GetCurrentTabList() && Vendor!.GetCurrentTabList() != (Vendor!.Tablist) && _dragSrc != DragSrcActionbar) {
                int slotIndex = Vendor!.GetCurrentTabList().GetCurrent();
                Int2 srcSlot;
                WidgetSlot? vendorSlot;
        
                vendorSlot = Vendor!.Stock[Vendor!.GetTab()].Slots[slotIndex];
        
                srcSlot.X = vendorSlot.Pos.X;
                srcSlot.Y = vendorSlot.Pos.Y;
        
                int slotClick = vendorSlot.CheckClick();
        
                // pick up item
                if (slotClick == WidgetSlot.Drag && _dragStack.Empty()) {
                    ShowActionPicker(Vendor, srcSlot);
                }
            }
        
            // Stash menu
            if (Stash!.Visible && Stash!.GetCurrentTabList() && _dragSrc != DragSrcActionbar) {
                int slotIndex = Stash!.GetCurrentTabList().GetCurrent();
                int tab = Stash!.GetTab();
                int slotClick = Stash!.Tabs[tab].Stock.Slots[slotIndex].CheckClick();
                Int2 srcSlot = new Int2(Stash!.Tabs[tab].Stock.Slots[slotIndex]!.Pos.X, Stash!.Tabs[tab].Stock.Slots[slotIndex]!.Pos.Y);
        
                // pick up item
                if (slotClick == WidgetSlot.Drag && _dragStack.Empty()) {
                    ShowActionPicker(Stash, srcSlot);
                }
                // rearrange item
                else if (slotClick == WidgetSlot.Drag && !_dragStack.Empty()) {
                    if (!Stash!.Drop(srcSlot, _dragStack)) {
                        DropStack.Enqueue(Stash!.DropStack.Peek());
                        Stash!.DropStack.Dequeue();
                    }
                    Inv!.ClearHighlight();
                    _dragSrc = DragSrcNone;
                    _dragStack.Clear();
                    _keyboardDragging = false;
                    _stickyDragging = false;
                }
            }
        
            // powers menu
            if (Pow!.Visible && Pow!.IsTabListSelected() && _dragSrc != DragSrcActionbar) {
                int slotIndex = Pow!.GetSelectedCellIndex();
                Int2 srcSlot = new Int2(Pow!.Slots[slotIndex]!.Pos.X, Pow!.Slots[slotIndex]!.Pos.Y);
        
                // save slot enable state
                bool slotEnabled = Pow!.Slots[slotIndex].Enabled;
        
                // temporarily enable slot if the power can be upgraded
                if (Pow!.Click(srcSlot).Unlock > 0) {
                    Pow!.Slots[slotIndex].Enabled = true;
                }
        
                int slotClick = Pow!.Slots[slotIndex].CheckClick();
        
                // restore slot enable state
                Pow!.Slots[slotIndex].Enabled = slotEnabled;
        
                if (slotClick == WidgetSlot.Drag) {
                    ShowActionPicker(Pow, srcSlot);
                }
            }
        
            // actionbar
            if (Act!.GetCurrentTabList() && (uint)Act!.GetCurrentTabList()!.GetCurrent() < Act!.Slots.Count) {
                int slotIndex = Act!.GetCurrentSlotIndexFromTablist();
                if (slotIndex < Act!.Slots.Count && Act!.Slots[slotIndex] != null) {
                    // temporarily enable slot so that it can be "clicked"
                    bool slotEnabled = Act!.Slots[slotIndex].Enabled;
                    Act!.Slots[slotIndex].Enabled = true;
        
                    int slotClick = Act!.Slots[slotIndex].CheckClick();
        
                    Act!.Slots[slotIndex].Enabled = slotEnabled;
        
                    Int2 destSlot = Act!.GetSlotPos(slotIndex);
        
                    // pick up power
                    if (slotClick == WidgetSlot.Drag && _dragStack.Empty() && _dragPower == 0) {
                        ShowActionPicker(Act, destSlot);
                    }
                    // drop power/item from other menu
                    else if (slotClick == WidgetSlot.Drag && _dragSrc != DragSrcActionbar && (!_dragStack.Empty() || _dragPower > 0)) {
                        if (_dragSrc == DragSrcPowers) {
                            Act!.Drop(destSlot, _dragPower, !MenuActionBar.Reorder);
                        }
                        else if (_dragSrc == DragSrcInventory) {
                            if (items.IsValid(_dragStack.Item) && items.Items[(int)_dragStack.Item]!.Power != 0) {
                                Act!.Drop(destSlot, items.Items[(int)_dragStack.Item]!.Power, !MenuActionBar.Reorder);
                            }
                        }
                        ResetDrag();
                        Inv!.ApplyEquipment();
                    }
                    // rearrange actionbar
                    else if (slotClick == WidgetSlot.Drag && _dragSrc == DragSrcActionbar && _dragPower > 0) {
                        Act!.Drop(destSlot, _dragPower, MenuActionBar.Reorder);
                        _dragSrc = DragSrcNone;
                        _dragPower = 0;
                        _keyboardDragging = false;
                    }
                }
            }
        }
        
        public void ResetDrag() {
            if (_dragSrc == DragSrcVendor) {
                Vendor!.ItemReturn(_dragStack);
                Vendor!.UnlockTabControl();
                Inv!.ClearHighlight();
            }
            else if (_dragSrc == DragSrcStash) {
                Stash!.ItemReturn(_dragStack);
                Inv!.ClearHighlight();
            }
            else if (_dragSrc == DragSrcInventory) {
                Inv!.ItemReturn(_dragStack);
                Inv!.ClearHighlight();
            }
            else if (_dragSrc == DragSrcActionbar) {
                Act!.ActionReturn(_dragPower);
            }
        
            _dragSrc = DragSrcNone;
            _dragStack.Clear();
            _dragPower = 0;
        
            SetDragIcon(WidgetSlot.NoIcon, WidgetSlot.NoOverlay);
        
            Vendor!.Stock[ItemManager.VendorBuy].DragPrevSlot = -1;
            Vendor!.Stock[ItemManager.VendorSell].DragPrevSlot = -1;
            for (int i = 0; i < Stash!.Tabs.Count; ++i) {
                Stash!.Tabs[i].Stock.DragPrevSlot = -1;
            }
            Inv!.DragPrevSrc = -1;
            Inv!.Inventory[MenuInventory.Equipment].DragPrevSlot = -1;
            Inv!.Inventory[MenuInventory.Carried].DragPrevSlot = -1;
        
            _keyboardDragging = false;
            _mouseDragging = false;
            _stickyDragging = false;
        }
        
        public void Render() {
            var settings = SharedResources.Settings!;
            var inpt = SharedResources.Inpt!;
            var powers = SharedGameResources.Powers!;
            var eset = SharedResources.Eset!;

            if (!settings.ShowHud) {
                // if the hud is disabled, only show a few necessary Menus
        
                // Exit menu
                Exit!.Render();
        
                // dev console
                if (settings.DevMode)
                    Devconsole!.Render();
        
                return;
            }
        
            bool hudlogOverlapped = false;
            if (Chr!.Visible && Utils.RectsOverlap(Hudlog!.WindowArea, Chr!.WindowArea)) {
                hudlogOverlapped = true;
            }
            if (Questlog!.Visible && Utils.RectsOverlap(Hudlog!.WindowArea, Questlog!.WindowArea)) {
                hudlogOverlapped = true;
            }
            if (Inv!.Visible && Utils.RectsOverlap(Hudlog!.WindowArea, Inv!.WindowArea)) {
                hudlogOverlapped = true;
            }
            if (Pow!.Visible && Utils.RectsOverlap(Hudlog!.WindowArea, Pow!.WindowArea)) {
                hudlogOverlapped = true;
            }
            if (Vendor!.Visible && Utils.RectsOverlap(Hudlog!.WindowArea, Vendor!.WindowArea)) {
                hudlogOverlapped = true;
            }
            if (Stash!.Visible && Utils.RectsOverlap(Hudlog!.WindowArea, Stash!.WindowArea)) {
                hudlogOverlapped = true;
            }
            if (Talker!.Visible && Utils.RectsOverlap(Hudlog!.WindowArea, Talker!.WindowArea)) {
                hudlogOverlapped = true;
            }
        
            for (int i=0; i<Menus.Count; i++) {
                if (Menus[i] == Hudlog && hudlogOverlapped && !Hudlog!.HideOverlay) {
                    continue;
                }
        
                Menus[i]!.Render();
            }
        
            if (hudlogOverlapped && !Hudlog!.HideOverlay) {
            Hudlog!.RenderOverlay();
            }
        
            Subtitles!.Render();
        
            TouchControls!.Render();
        
            if (!NumPicker!.Visible && !ActionPicker!.Visible && !_mouseDragging && !_stickyDragging) {
                if (!inpt.UsingMouse() || inpt.UsingTouchscreen())
                    HandleKeyboardTooltips();
                else {
                    // Find tooltips depending on mouse position
                    if (!Book!.Visible) {
                        PushMatchingItemsOf(inpt.Mouse);
        
                        Chr!.RenderTooltips(inpt.Mouse);
                        Vendor!.RenderTooltips(inpt.Mouse);
                        Stash!.RenderTooltips(inpt.Mouse);
                        Pow!.RenderTooltips(inpt.Mouse);
                        Inv!.RenderTooltips(inpt.Mouse);
                    }
                    if (!Exit!.Visible) {
                        Effects!.RenderTooltips(inpt.Mouse);
                        Act!.RenderTooltips(inpt.Mouse);
                    }
                }
            }
        
            // draw icon under cursor if dragging
            if (_mouseDragging && !NumPicker!.Visible) {
                if (_dragSrc == DragSrcInventory || _dragSrc == DragSrcVendor || _dragSrc == DragSrcStash)
                    SetDragIconItem(_dragStack);
                else if (_dragSrc == DragSrcPowers || _dragSrc == DragSrcActionbar)
                    SetDragIcon(powers.Powers[_dragPower].Icon, -1);
        
                if (inpt.UsingTouchscreen() && _stickyDragging)
                    RenderIcon(_keydragPos.X - eset.Resolutions.IconSize/2, _keydragPos.Y - eset.Resolutions.IconSize/2);
                else
                    RenderIcon(inpt.Mouse.X - eset.Resolutions.IconSize/2, inpt.Mouse.Y - eset.Resolutions.IconSize/2);
            }
            else if (_keyboardDragging && !NumPicker!.Visible) {
                if (_dragSrc == DragSrcInventory || _dragSrc == DragSrcVendor || _dragSrc == DragSrcStash)
                    SetDragIconItem(_dragStack);
                else if (_dragSrc == DragSrcPowers || _dragSrc == DragSrcActionbar)
                    SetDragIcon(powers.Powers[_dragPower].Icon, -1);
        
                RenderIcon(_keydragPos.X - eset.Resolutions.IconSize/2, _keydragPos.Y - eset.Resolutions.IconSize/2);
            }
        
            // render the dev console above everything else
            if (settings.DevMode) {
                Devconsole!.Render();
            }
        }
        
        private void HandleKeyboardTooltips() {
            if (Book!.Visible)
                return;

            var eset = SharedResources.Eset!;
        
            if (Vendor!.Visible) {
                TabList? curTablist = Vendor!.GetCurrentTabList();
                if (curTablist != null && curTablist != Vendor!.Tablist) {
                    int slotIndex = curTablist.GetCurrent();
        
                    _keydragPos.X = Vendor!.Stock[Vendor!.GetTab()].Slots[slotIndex].Pos.X;
                    _keydragPos.Y = Vendor!.Stock[Vendor!.GetTab()].Slots[slotIndex].Pos.Y;
        
                    Int2 tooltipPos = _keydragPos;
                    tooltipPos.X += eset.Resolutions.IconSize / 2;
                    tooltipPos.Y += eset.Resolutions.IconSize / 2;
        
                    PushMatchingItemsOf(tooltipPos);
                    Vendor!.RenderTooltips(tooltipPos);
                }
            }
        
            if (Stash!.Visible) {
                TabList? curTablist = Stash!.GetCurrentTabList();
                if (curTablist != null && curTablist != Stash!.Tablist) {
                    int slotIndex = Stash!.GetCurrentTabList().GetCurrent();
                    int tab = Stash!.GetTab();
        
                    _keydragPos.X = Stash!.Tabs[tab].Stock.Slots[slotIndex].Pos.X;
                    _keydragPos.Y = Stash!.Tabs[tab].Stock.Slots[slotIndex].Pos.Y;
        
                    Int2 tooltipPos = _keydragPos;
                    tooltipPos.X += eset.Resolutions.IconSize / 2;
                    tooltipPos.Y += eset.Resolutions.IconSize / 2;
        
                    PushMatchingItemsOf(tooltipPos);
                    Stash!.RenderTooltips(tooltipPos);
                }
            }
        
            if (Pow!.Visible && Pow!.IsTabListSelected()) {
                int slotIndex = Pow!.GetSelectedCellIndex();
        
                _keydragPos.X = Pow!.Slots[slotIndex].Pos.X;
                _keydragPos.Y = Pow!.Slots[slotIndex].Pos.Y;
        
                Int2 tooltipPos = _keydragPos;
                tooltipPos.X += eset.Resolutions.IconSize / 2;
                tooltipPos.Y += eset.Resolutions.IconSize / 2;
        
                Pow!.RenderTooltips(tooltipPos);
            }
        
            if (Inv!.Visible && Inv!.GetCurrentTabList()) {
                int slotIndex = Inv!.GetCurrentTabList().GetCurrent();
        
                if (slotIndex < Inv!.GetEquippedCount()) {
                    _keydragPos.X = Inv!.Inventory[MenuInventory.Equipment].Slots[slotIndex].Pos.X;
                    _keydragPos.Y = Inv!.Inventory[MenuInventory.Equipment].Slots[slotIndex].Pos.Y;
                }
                else if (slotIndex < Inv!.GetTotalSlotCount()) {
                    _keydragPos.X = Inv!.Inventory[MenuInventory.Carried].Slots[slotIndex - Inv!.GetEquippedCount()].Pos.X;
                    _keydragPos.Y = Inv!.Inventory[MenuInventory.Carried].Slots[slotIndex - Inv!.GetEquippedCount()].Pos.Y;
                }
                else {
                    Widget?tempWidget = Inv!.GetCurrentTabList().GetWidgetByIndex(slotIndex);
                    _keydragPos.X = tempWidget.Pos.X;
                    _keydragPos.Y = tempWidget.Pos.Y;
                }
        
                Int2 tooltipPos = _keydragPos;
                tooltipPos.X += eset.Resolutions.IconSize / 2;
                tooltipPos.Y += eset.Resolutions.IconSize / 2;
        
                PushMatchingItemsOf(tooltipPos);
                Inv!.RenderTooltips(tooltipPos);
            }
        
            if (Act!.GetCurrentTabList()) {
                int slotIndex = Act!.GetCurrentSlotIndexFromTablist();
                if (slotIndex < Act!.Slots.Count + MenuActionBar.MenuCount) {
                    _keydragPos = Act!.GetSlotPos(slotIndex);
                    // since the actionbar tablist is always active when using a controller, don't draw tooltips. It'd be too distracting
                }
            }
        }
        
        public void CloseAll() {
            CloseLeft();
            CloseRight();
        }
        
        public void CloseLeft() {
            if (NumPicker!.Visible) {
                _dragStack.Clear();
                NumPicker!.CloseWindow();
            }
        
            ResetDrag();
            Chr!.Visible = false;
            Questlog!.Visible = false;
            Exit!.Visible = false;
            Stash!.Visible = false;
            Book!.SetBookFilename("");
        
            Talker!.SetNPC(null);
            Vendor!.SetNPC(null);
        
            if (SharedResources.Settings!.DevMode && Devconsole!.Visible) {
                Devconsole!.CloseWindow();
            }
        
            DefocusLeft();
        }
        
        public void CloseRight() {
            if (NumPicker!.Visible) {
                _dragStack.Clear();
                NumPicker!.CloseWindow();
            }
        
            ResetDrag();
            Inv!.Visible = false;
            Pow!.Visible = false;
            Exit!.Visible = false;
            Book!.SetBookFilename("");
        
            Talker!.SetNPC(null);
        
            if (SharedResources.Settings!.DevMode && Devconsole!.Visible) {
                Devconsole!.CloseWindow();
            }
        
            DefocusRight();
        }
        
        public bool IsDragging()
        {
            return _dragSrc != DragSrcNone;
        }

        public bool IsNPCMenuVisible()
        {
            return Talker!.Visible || Vendor!.Visible;
        }
        
        public void ShowExitMenu() {
            if (GameOver!.Visible)
                return;
        
            Pause = true;
            CloseAll();
            if (Exit != null) {
                // handleCancel() will show the menu, but only if it is already hidden
                // we use handleCancel() here because it will reset the menu to the "Exit" tab
                Exit!.Visible = false;
                Exit!.HandleCancel();
            }
        }
        
        private void PushMatchingItemsOf(Int2 hovPos) {
            var settings = SharedResources.Settings!;
            var items = SharedGameResources.Items!;
            var eset = SharedResources.Eset!;
            var msg = SharedResources.Msg!;
            var font = SharedResources.Font!;
            var tooltipm = SharedResources.Tooltipm!;
            var pc = SharedGameResources.Pc!;

            if (!settings.ItemCompareTips)
                return;
        
            int area = -1;
            ItemStack hovStack = default;
            bool haveHovStack = false;

            if (Inv!.Visible && Utils.IsWithinRect(Inv!.WindowArea, hovPos)) {
                area = Inv!.AreaOver(hovPos);
                if (area == MenuInventory.Carried) {
                    hovStack = Inv!.Inventory[area].GetItemStackAtPos(hovPos);
                    haveHovStack = true;
                }
            }
            else if (Vendor!.Visible && Utils.IsWithinRect(Vendor!.WindowArea, hovPos)) {
                area = Vendor!.GetTab();
                if (area >= 0) {
                    hovStack = Vendor!.Stock[area].GetItemStackAtPos(hovPos);
                    haveHovStack = true;
                }
            }
            else if (Stash!.Visible && Utils.IsWithinRect(Stash!.WindowArea, hovPos)) {
                area = (int)(Stash!.GetTab());
                if (area >= 0) {
                    hovStack = Stash!.Tabs[area].Stock.GetItemStackAtPos(hovPos);
                    haveHovStack = true;
                }
            }

            // we assume that a non-empty item type means that there is a primary tooltip
            if (!haveHovStack || !items.IsValid(hovStack.Item) || items.GetItemType(items.Items[(int)hovStack.Item]!.Type).Name.Length == 0)
                return;

            {
                int tipIndex = 1;
                List<ItemID> matchingIds = new List<ItemID>();
        
                for (int i = 0; i < Inv!.EquippedArea.Count; i++) {
                    if (Inv!.IsEquipSlotActive(i) && !Inv!.Inventory[MenuInventory.Equipment].Storage[i].Empty() && Inv!.SlotType[i] == items.Items[(int)hovStack.Item]!.Type) {
                        matchingIds.Add(Inv!.Inventory[MenuInventory.Equipment].Storage[i].Item);
                    }
                }
        
                if (matchingIds.Count + 1 > eset.Tooltips.VisibleMax) {
                    TooltipData match = new TooltipData();
                    match.AddColoredText(msg.Get("Equipped") + ": " + items.GetItemType(items.Items[(int)hovStack.Item]!.Type).Name, font.GetColor(FontEngine.ColorItemFlavor));
                    for (int i = 0; i < matchingIds.Count; ++i) {
                        match.AddText(items.GetItemName(matchingIds[i]));
                    }
        
                    tooltipm.Push(match, hovPos, TooltipData.StyleFloat, tipIndex);
                }
                else {
                    //get equipped items of the same type
                    for (int i = 0; i < Inv!.EquippedArea.Count; i++) {
                        if (tipIndex >= eset.Tooltips.VisibleMax)
                            break; // can't show any more tooltips
        
                        if (Inv!.IsEquipSlotActive(i) && !Inv!.Inventory[MenuInventory.Equipment].Storage[i].Empty() && Inv!.SlotType[i] == items.Items[(int)hovStack.Item]!.Type) {
                            Int2 matchPos = new Int2(Inv!.EquippedArea[i].X, Inv!.EquippedArea[i].Y);
        
                            TooltipData match = Inv!.Inventory[MenuInventory.Equipment].CheckTooltip(matchPos, pc.Stats, ItemManager.PlayerInv, !ItemManager.TooltipInputHint);
                            match.AddColoredText(msg.Get("Equipped"), font.GetColor(FontEngine.ColorItemFlavor));
        
                            tooltipm.Push(match, hovPos, TooltipData.StyleFloat, tipIndex);
                            tipIndex++;
                        }
                    }
                }
            }
        }
        
        public void DefocusLeft() {
            Chr!.DefocusTabLists();
            Questlog!.DefocusTabLists();
            Stash!.DefocusTabLists();
            Vendor!.DefocusTabLists();
        }
        
        public void DefocusRight() {
            Inv!.DefocusTabLists();
            Pow!.DefocusTabLists();
        }
        
        private bool IsTabListSelected()
        {
            for (int i = 0; i < Menus.Count; ++i)
            {
                if (Menus[i]!.GetCurrentTabList())
                    return true;
            }

            if (Devconsole != null && Devconsole!.Tablist.GetCurrent() != -1)
                return true;

            return false;
        }
        
        private void ShowActionPicker(Menu? srcMenu, Int2 target) {
            var inpt = SharedResources.Inpt!;
            var msg = SharedResources.Msg!;
            var eset = SharedResources.Eset!;

            ActionPicker!.ActionList!.Clear();
            _actionPickerMap.Clear();
        
            if (srcMenu == Act) {
                PowerID slotPower = Act!.CheckDrag(target);
                Act!.ActionReturn(slotPower);
        
                if (slotPower > 0) {
                    _actionSrc = ActionSrcActionbar;
                    _actionPickerTarget = target;
        
                    if (inpt.Mode == InputState.ModeTouchscreen) {
                        ActionPicker!.ActionList!.Append(msg.Get("Use"), "");
                        _actionPickerMap[_actionPickerMap.Count] = ActionPickerActionbarUse;
                    }
        
                    ActionPicker!.ActionList!.Append(msg.Get("Move"), "");
                    _actionPickerMap[_actionPickerMap.Count] = ActionPickerActionbarSelect;
        
                    ActionPicker!.ActionList!.Append(msg.Get("Clear"), "");
                    _actionPickerMap[_actionPickerMap.Count] = ActionPickerActionbarClear;
                }
            }
            else if (srcMenu == Pow) {
                MenuPowersClick powClick = Pow!.Click(target);
        
                if (powClick.Drag > 0 || powClick.Unlock > 0) {
                    _actionSrc = ActionSrcPowers;
                    _actionPickerTarget = target;
                }
        
                if (powClick.Drag > 0 && (inpt.UsingMouse() || Act!.EnableGamepadNav)) {
                    ActionPicker!.ActionList!.Append(msg.Get("Equip"), "");
                    _actionPickerMap[0] = ActionPickerPowersSelect;
                }
                if (powClick.Unlock > 0) {
                    ActionPicker!.ActionList!.Append(msg.Get("Upgrade"), "");
                    _actionPickerMap[_actionPickerMap.Count] = ActionPickerPowersUpgrade;
                }
            }
            else if (srcMenu == Inv) {
                ItemStack invClick = Inv!.Click(target);
                if (invClick.Quantity == 1) {
                    Inv!.ItemReturn(invClick);
                }
        
                if (!invClick.Empty()) {
                    _actionSrc = ActionSrcInventory;
                    _actionPickerTarget = target;
        
                    if (Vendor!.Visible) {
                        ActionPicker!.ActionList!.Append(msg.Get("Sell"), "");
                        _actionPickerMap[_actionPickerMap.Count] = ActionPickerInventorySell;
                    }
                    else if (Stash!.Visible) {
                        ActionPicker!.ActionList!.Append(msg.Get("Transfer"), "");
                        _actionPickerMap[_actionPickerMap.Count] = ActionPickerInventoryStash;
                    }
        
                    ActionPicker!.ActionList!.Append(msg.Get("Move"), "");
                    _actionPickerMap[_actionPickerMap.Count] = ActionPickerInventorySelect;
        
                    if (Inv!.CanUseItem(target)) {
                        ActionPicker!.ActionList!.Append(msg.Get("Use"), "");
                        _actionPickerMap[_actionPickerMap.Count] = ActionPickerInventoryActivate;
                    }
                    else if (Inv!.CanEquipItem(target)) {
                        ActionPicker!.ActionList!.Append(msg.Get("Equip"), "");
                        _actionPickerMap[_actionPickerMap.Count] = ActionPickerInventoryActivate;
                    }
        
                    if (!inpt.UsingMouse() && Act!.EnableGamepadNav) {
                        if (Inv!.CanPlaceItemOnActionbar(target)) {
                            ActionPicker!.ActionList!.Append(msg.Get("Add to bar"), "");
                            _actionPickerMap[_actionPickerMap.Count] = ActionPickerInventoryActionbar;
                        }
                    }
        
                    ActionPicker!.ActionList!.Append(msg.Get("Drop"), "");
                    _actionPickerMap[_actionPickerMap.Count] = ActionPickerInventoryDrop;
        
                    if (!Vendor!.Visible && eset.Misc.SellWithoutVendor) {
                        ActionPicker!.ActionList!.Append(msg.Get("Sell"), "");
                        _actionPickerMap[_actionPickerMap.Count] = ActionPickerInventorySell;
                    }
                }
            }
            else if (srcMenu == Stash) {
                ItemStack stashClick = Stash!.Click(target);
                if (stashClick.Quantity == 1) {
                    Stash!.ItemReturn(stashClick);
                }
        
                if (!stashClick.Empty()) {
                    _actionSrc = ActionSrcStash;
                    _actionPickerTarget = target;
        
                    ActionPicker!.ActionList!.Append(msg.Get("Move"), "");
                    _actionPickerMap[_actionPickerMap.Count] = ActionPickerStashSelect;
        
                    ActionPicker!.ActionList!.Append(msg.Get("Transfer"), "");
                    _actionPickerMap[_actionPickerMap.Count] = ActionPickerStashTransfer;
                }
            }
            else if (srcMenu == Vendor) {
                ItemStack vendorClick = Vendor!.Click(target);
                if (vendorClick.Quantity == 1) {
                    Vendor!.ItemReturn(vendorClick);
                }
        
                if (!vendorClick.Empty()) {
                    _actionSrc = ActionSrcVendor;
                    _actionPickerTarget = target;
        
                    ActionPicker!.ActionList!.Append(msg.Get("Buy"), "");
                    _actionPickerMap[_actionPickerMap.Count] = ActionPickerVendorBuy;
                }
            }
        
            if (_actionPickerMap.Count > 0)
                ActionPicker!.Show();
        }
        
        private void ActionPickerStartDrag() {
            if (SharedResources.Inpt!.UsingMouse()) {
                _mouseDragging = true;
                _stickyDragging = true;
            }
            else {
                _keyboardDragging = true;
            }
        }
        
        public void Dispose()
        {
            Utils.LogInfo("Cleaning up: MenuManager");
        
            for (int i = 0; i < ResourceStatbars.Count; ++i)
            {
                ResourceStatbars[i]?.Dispose();
            }
        
            Hp?.Dispose();
            Mp?.Dispose();
            Xp?.Dispose();
            Mini?.Dispose();
            Inv?.Dispose();
            Pow?.Dispose();
            Chr?.Dispose();
            Hudlog?.Dispose();
            Questlog?.Dispose();
            Act?.Dispose();
            Vendor?.Dispose();
            Talker?.Dispose();
            Exit?.Dispose();
            Enemy?.Dispose();
            Effects?.Dispose();
            Stash?.Dispose();
            Book?.Dispose();
            NumPicker?.Dispose();
            GameOver?.Dispose();
            ActionPicker?.Dispose();
            RegionTitle?.Dispose();
        
            if (SharedResources.Settings!.DevMode) {
                Devconsole?.Dispose();
            }
        
            TouchControls?.Dispose();
        
            Subtitles?.Dispose();
        
            _dragIcon?.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
