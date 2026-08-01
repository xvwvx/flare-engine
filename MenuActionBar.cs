// 对应 C++ 源文件：MenuActionBar.h + MenuActionBar.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// MenuActionBar
    ///
    /// 处理 0-9 快捷键、鼠标按键与菜单按钮的配置、显示与使用。
    /// 持有的 <see cref="_spriteEmptyslot"/> 与各 <see cref="WidgetSlot"/> 通过
    /// <see cref="IDisposable"/> 显式释放，释放顺序与原始析构函数 <c>~MenuActionBar()</c> 一致。
    /// </summary>
    public class MenuActionBar : Menu, IDisposable
    {
        private const bool IsEquipped = true;

        /// <summary>对应匿名枚举 <c>MENU_CHARACTER</c>。</summary>
        public const int MenuCharacter = 0;
        /// <summary>对应匿名枚举 <c>MENU_INVENTORY</c>。</summary>
        public const int MenuInventory = 1;
        /// <summary>对应匿名枚举 <c>MENU_POWERS</c>。</summary>
        public const int MenuPowers = 2;
        /// <summary>对应匿名枚举 <c>MENU_LOG</c>。</summary>
        public const int MenuLog = 3;
        /// <summary>对应 <c>static const unsigned MENU_COUNT = 4</c>。</summary>
        public const int MenuCount = 4;

        public const int SlotMain1 = 10;
        public const int SlotMain2 = 11;
        public const int SlotMax = 12;

        public const int UseEmptySlot = 0;

        public const bool Reorder = true;
        public const bool ClearSkipItems = true;
        public const bool SetSkipEmpty = true;

        private void AddSlot(uint index, int x, int y, bool isLocked)
        {
            if (index >= Slots.Count)
            {
                while (Labels.Count < (int)index + 1)
                    Labels.Add("");
                while (Slots.Count < (int)index + 1)
                    Slots.Add(null);
            }

            Slots[(int)index] = new WidgetSlot(WidgetSlot.NoIcon, WidgetSlot.HighlightNormal);
            Slots[(int)index]!.SetBasePos(x, y, Utils.AlignTopLeft);
            Slots[(int)index]!.Pos.Width = Slots[(int)index]!.Pos.Height = SharedResources.Eset!.Resolutions.IconSize;
            Slots[(int)index]!.Continuous = true;

            if (index < 10)
                Slots[(int)index]!.SetHotkey(Input.Bar1 + (int)index);
            else if (index < 12)
                Slots[(int)index]!.SetHotkey(Input.Main1 + (int)index - 10);

            while (PreventChanging.Count < Slots.Count)
                PreventChanging.Add(false);
            PreventChanging[(int)index] = isLocked;

            Tablist.Add(Slots[(int)index]);
        }

        private void SetItemCount(uint index, int count, bool isEquipped)
        {
            if (index >= SlotsCount || Slots[(int)index] == null) return;

            SlotItemCount[(int)index] = count;
            if (count == 0)
            {
                if (SlotActivated[(int)index])
                    Slots[(int)index]!.Deactivate();

                Slots[(int)index]!.Enabled = false;
            }

            if (isEquipped)
                // we don't care how many of an equipped item we're carrying
                Slots[(int)index]!.SetAmount(count, 0);
            else if (count >= 0)
                // we can always carry more than 1 of any item, so always display non-equipped item count
                Slots[(int)index]!.SetAmount(count, 2);
            else
                // slot contains a regular power, so ignore item count
                Slots[(int)index]!.SetAmount(0, 0);
        }

        private Sprite? _spriteEmptyslot;

        private Rectangle _src;

        private List<string> Labels = new List<string>();
        private List<string> MenuLabels = new List<string>();

        private Int2 LastMouse;

        private List<int> SlotFailCooldown = new List<int>();

        private SoundID SfxUnableToCast;

        private int TooltipLength;
        private bool PowersOverlapSlots;

        private int TablistCursor;

        private WidgetSlot?[] _menus = new WidgetSlot?[MenuCount];
        private string[] MenuTitles = new string[MenuCount];

        public uint SlotsCount;
        public List<PowerID> Hotkeys = new List<PowerID>();
        public List<PowerID> HotkeysTemp = new List<PowerID>();
        public List<PowerID> HotkeysMod = new List<PowerID>();
        public List<bool> Locked = new List<bool>();
        public List<bool> PreventChanging = new List<bool>();
        public List<WidgetSlot?> Slots = new List<WidgetSlot?>();
        public bool[] RequiresAttention = new bool[MenuCount];
        public List<int> SlotItemCount = new List<int>();
        public List<bool> SlotActivated = new List<bool>();

        public int DragPrevSlot;
        public bool Updated;
        public int TwostepSlot;

        public WidgetSlot? TouchSlot;

        public bool EnableGamepadNav;

        public MenuActionBar()
        {
            _spriteEmptyslot = null;
            SfxUnableToCast = 0;
            TooltipLength = global::FlareEngine.MenuPowers.TooltipLongMenu;
            PowersOverlapSlots = false;
            TablistCursor = -1;
            SlotsCount = 0;
            DragPrevSlot = -1;
            Updated = false;
            TwostepSlot = -1;
            TouchSlot = null;
            EnableGamepadNav = true;

            while (MenuLabels.Count < MenuCount)
                MenuLabels.Add("");

            Tablist = new TabList();
            Tablist.SetScrollType(Widget.ScrollHorizontal);
            Tablist.SetHorizontalKeys(Input.MenuPagePrev, Input.MenuPageNext);
            Tablist.Lock();

            for (uint i = 0; i < MenuCount; i++)
            {
                _menus[i] = new WidgetSlot(WidgetSlot.NoIcon, WidgetSlot.HighlightNormal);
                _menus[i]!.SetHotkey(Input.Character + (int)i);
                _menus[i]!.ShowColorblindHighlight = true;
                _menus[i]!.Enabled = false;

                // NOTE: This prevents these buttons from being clickable unless they get defined in the config file.
                // However, it doesn't prevent them from being added to the tablist, so they can still be activated there despite being invisible
                // Mods should be expected to define these slot positions. This just keeps things from getting *too* broken if they're not defined.
                _menus[i]!.Pos.Width = 0;
                _menus[i]!.Pos.Height = 0;
            }

            MenuTitles[MenuCharacter] = SharedResources.Msg!.Get("Character");
            MenuTitles[MenuInventory] = SharedResources.Msg.Get("Inventory");
            MenuTitles[MenuPowers] = SharedResources.Msg.Get("Powers");
            MenuTitles[MenuLog] = SharedResources.Msg.Get("Log");

            // Read data from config file
            using FileParser infile = new FileParser();

            // @CLASS MenuActionBar|Description of menus/actionbar.txt
            if (infile.Open("menus/actionbar.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (ParseMenuKey(infile.Key, infile.Val))
                        continue;

                    // @ATTR slot|repeatable(int, int, int, bool) : Index, X, Y, Locked|Index (max 10) and position for power slot. If a slot is locked, its Power can't be changed by the player.
                    if (infile.Key == "slot")
                    {
                        string slotVal = infile.Val;
                        uint index = (uint)Parse.PopFirstInt(ref slotVal);
                        if (index == 0 || index > 10)
                        {
                            infile.Error("MenuActionBar: Slot index must be in range 1-10.");
                        }
                        else
                        {
                            int x = Parse.PopFirstInt(ref slotVal);
                            int y = Parse.PopFirstInt(ref slotVal);
                            string val = Parse.PopFirstString(ref slotVal);
                            bool isLocked = (val == "" ? false : Parse.ToBool(val));
                            AddSlot(index - 1, x, y, isLocked);
                        }
                    }
                    // @ATTR slot_M1|point, bool : Position, Locked|Position for the primary action slot. If the slot is locked, its Power can't be changed by the player.
                    else if (infile.Key == "slot_M1")
                    {
                        string slotVal = infile.Val;
                        int x = Parse.PopFirstInt(ref slotVal);
                        int y = Parse.PopFirstInt(ref slotVal);
                        string val = Parse.PopFirstString(ref slotVal);
                        bool isLocked = (val == "" ? false : Parse.ToBool(val));
                        AddSlot(10, x, y, isLocked);
                    }
                    // @ATTR slot_M2|point, bool : Position Locked|Position for the secondary action slot. If the slot is locked, its Power can't be changed by the player.
                    else if (infile.Key == "slot_M2")
                    {
                        string slotVal = infile.Val;
                        int x = Parse.PopFirstInt(ref slotVal);
                        int y = Parse.PopFirstInt(ref slotVal);
                        string val = Parse.PopFirstString(ref slotVal);
                        bool isLocked = (val == "" ? false : Parse.ToBool(val));
                        AddSlot(11, x, y, isLocked);
                    }

                    // @ATTR char_menu|point|Position for the Character menu button.
                    else if (infile.Key == "char_menu")
                    {
                        string slotVal = infile.Val;
                        int x = Parse.PopFirstInt(ref slotVal);
                        int y = Parse.PopFirstInt(ref slotVal);
                        _menus[MenuCharacter]!.SetBasePos(x, y, Utils.AlignTopLeft);
                        _menus[MenuCharacter]!.Pos.Width = _menus[MenuCharacter]!.Pos.Height = SharedResources.Eset!.Resolutions.IconSize;
                    }
                    // @ATTR inv_menu|point|Position for the Inventory menu button.
                    else if (infile.Key == "inv_menu")
                    {
                        string slotVal = infile.Val;
                        int x = Parse.PopFirstInt(ref slotVal);
                        int y = Parse.PopFirstInt(ref slotVal);
                        _menus[MenuInventory]!.SetBasePos(x, y, Utils.AlignTopLeft);
                        _menus[MenuInventory]!.Pos.Width = _menus[MenuInventory]!.Pos.Height = SharedResources.Eset!.Resolutions.IconSize;
                    }
                    // @ATTR powers_menu|point|Position for the Powers menu button.
                    else if (infile.Key == "powers_menu")
                    {
                        string slotVal = infile.Val;
                        int x = Parse.PopFirstInt(ref slotVal);
                        int y = Parse.PopFirstInt(ref slotVal);
                        _menus[MenuPowers]!.SetBasePos(x, y, Utils.AlignTopLeft);
                        _menus[MenuPowers]!.Pos.Width = _menus[MenuPowers]!.Pos.Height = SharedResources.Eset!.Resolutions.IconSize;
                    }
                    // @ATTR log_menu|point|Position for the Log menu button.
                    else if (infile.Key == "log_menu")
                    {
                        string slotVal = infile.Val;
                        int x = Parse.PopFirstInt(ref slotVal);
                        int y = Parse.PopFirstInt(ref slotVal);
                        _menus[MenuLog]!.SetBasePos(x, y, Utils.AlignTopLeft);
                        _menus[MenuLog]!.Pos.Width = _menus[MenuLog]!.Pos.Height = SharedResources.Eset!.Resolutions.IconSize;
                    }
                    // @ATTR tooltip_length|["short", "long_menu", "long_all"]|The length of power descriptions in tooltips. 'short' will display only the power name. 'long_menu' (the default setting) will display full tooltips, but only for powers that are in the Powers menu. 'long_all' will display full tooltips for all powers.
                    else if (infile.Key == "tooltip_length")
                    {
                        if (infile.Val == "short")
                            TooltipLength = global::FlareEngine.MenuPowers.TooltipShort;
                        else if (infile.Val == "long_menu")
                            TooltipLength = global::FlareEngine.MenuPowers.TooltipLongMenu;
                        else if (infile.Val == "long_all")
                            TooltipLength = global::FlareEngine.MenuPowers.TooltipLongAll;
                        else
                            infile.Error("MenuActionBar: '%s' is not a valid tooltip_length setting.", infile.Val);
                    }
                    // @ATTR powers_overlap_slots|bool|When true, the power icon is drawn on top of the empty slot graphic for any given slot. If false, the empty slot graphic will only be drawn if there's not a power in the slot. The default value is false.
                    else if (infile.Key == "powers_overlap_slots")
                    {
                        PowersOverlapSlots = Parse.ToBool(infile.Val);
                    }
                    // @ATTR enable_gamepad_nav|bool|When true, the actionbar can be interacted with via the next/prev/activate menu bindings. Defaults to true.
                    else if (infile.Key == "enable_gamepad_nav")
                    {
                        EnableGamepadNav = Parse.ToBool(infile.Val);
                    }

                    else infile.Error("MenuActionBar: '%s' is not a valid key.", infile.Key);
                }
                infile.Close();
            }

            // menus are added to tablist with setupMenuButtons()

            SlotsCount = (uint)Slots.Count;

            while (Hotkeys.Count < (int)SlotsCount)
                Hotkeys.Add(0);
            while (HotkeysTemp.Count < (int)SlotsCount)
                HotkeysTemp.Add(0);
            while (HotkeysMod.Count < (int)SlotsCount)
                HotkeysMod.Add(0);
            while (Locked.Count < (int)SlotsCount)
                Locked.Add(false);
            while (SlotItemCount.Count < (int)SlotsCount)
                SlotItemCount.Add(0);
            while (SlotActivated.Count < (int)SlotsCount)
                SlotActivated.Add(false);
            while (SlotFailCooldown.Count < (int)SlotsCount)
                SlotFailCooldown.Add(0);

            Clear(!ClearSkipItems);

            LoadGraphics();

            if (SharedResources.Eset!.Misc.SfxUnableToCast != "")
                SfxUnableToCast = SharedResources.Snd!.Load(SharedResources.Eset.Misc.SfxUnableToCast, "MenuActionBar unable to cast");

            Align();

            SharedGameResources.MenuAct = this;
        }

        /// <summary>
        /// 对应 C++ 的 <c>~MenuActionBar()</c>：释放精灵、槽位与菜单按钮，卸载音效，最后调用基类 <see cref="Menu.Dispose"/>。
        /// </summary>
        public override void Dispose()
        {
            SharedGameResources.MenuAct = null;

            _spriteEmptyslot?.Dispose();
            _spriteEmptyslot = null;

            Labels.Clear();
            MenuLabels.Clear();

            for (uint i = 0; i < SlotsCount; i++)
            {
                Slots[(int)i]?.Dispose();
                Slots[(int)i] = null;
            }

            for (uint i = 0; i < MenuCount; i++)
            {
                _menus[i]?.Dispose();
                _menus[i] = null;
            }

            SharedResources.Snd!.Unload(SfxUnableToCast);

            base.Dispose();
        }

        public override void Align()
        {
            base.Align();

            for (uint i = 0; i < SlotsCount; i++)
            {
                if (Slots[(int)i] != null)
                {
                    Slots[(int)i]!.SetPos(WindowArea.X, WindowArea.Y);
                }
            }
            for (uint i = 0; i < MenuCount; i++)
            {
                _menus[i]!.SetPos(WindowArea.X, WindowArea.Y);
            }

            // set keybinding labels
            for (uint i = 0; i < (uint)SlotMain1; i++)
            {
                if (i < Slots.Count && Slots[(int)i] != null)
                {
                    Labels[(int)i] = SharedResources.Msg!.GetV("Hotkey: %s", SharedResources.Inpt!.GetBindingString((int)i + Input.Bar1));
                }
            }

            for (uint i = (uint)SlotMain1; i < (uint)SlotMax; i++)
            {
                if (i < Slots.Count && Slots[(int)i] != null)
                {
                    Settings settings = SharedResources.Settings!;
                    if (settings.MouseMove && ((i == SlotMain2 && settings.MouseMoveSwap) || (i == SlotMain1 && !settings.MouseMoveSwap)))
                    {
                        Labels[(int)i] = SharedResources.Msg!.GetV("Hotkey: %s", SharedResources.Inpt!.GetBindingString(Input.Shift) + " + " + SharedResources.Inpt!.GetBindingString((int)i - SlotMain1 + Input.Main1));
                    }
                    else
                    {
                        Labels[(int)i] = SharedResources.Msg!.GetV("Hotkey: %s", SharedResources.Inpt!.GetBindingString((int)i - SlotMain1 + Input.Main1));
                    }
                }
            }
            for (uint i = 0; i < MenuLabels.Count; i++)
            {
                _menus[i]!.SetPos(WindowArea.X, WindowArea.Y);
                MenuLabels[(int)i] = SharedResources.Msg!.GetV("Hotkey: %s", SharedResources.Inpt!.GetBindingString((int)i + Input.Character));
            }
        }

        public void ClearSlot(int slot)
        {
            Hotkeys[slot] = 0;
            HotkeysTemp[slot] = 0;
            HotkeysMod[slot] = 0;
            SlotItemCount[slot] = -1;
            Locked[slot] = false;
            SlotActivated[slot] = false;
            SlotFailCooldown[slot] = 0;

            if (Slots[slot] != null)
            {
                Slots[slot]!.Enabled = true;
                Slots[slot]!.SetIcon(WidgetSlot.NoIcon, WidgetSlot.NoOverlay);
            }
        }

        public void Clear(bool skipItems)
        {
            // clear action bar
            for (uint i = 0; i < SlotsCount; i++)
            {
                if (skipItems && SharedGameResources.Powers!.IsValid(HotkeysMod[(int)i]))
                {
                    if (SharedGameResources.Powers!.Powers[HotkeysMod[(int)i]]!.RequiredItems.Count != 0)
                    {
                        continue;
                    }
                }

                ClearSlot((int)i);
            }

            // clear menu notifications
            for (uint i = 0; i < MenuCount; i++)
                RequiresAttention[i] = false;

            TwostepSlot = -1;
        }

        public void LoadGraphics()
        {
            Image? graphics;

            if (_background == null)
                SetBackground("images/menus/actionbar_trim.png");

            Rectangle iconClip = default;
            iconClip.Width = iconClip.Height = SharedResources.Eset!.Resolutions.IconSize;

            graphics = SharedResources.RenderDevice!.LoadImage("images/menus/slot_empty.png", RenderDevice.ErrorNormal);
            if (graphics != null)
            {
                _spriteEmptyslot = graphics.CreateSprite();
                _spriteEmptyslot.SetClipFromRect(iconClip);
                graphics.Unref();
            }
        }

        public void Logic()
        {
            Tablist.Logic();
            if (Tablist.GetCurrent() != -1)
            {
                TablistCursor = Tablist.GetCurrent();
            }
            if (!SharedResources.Inpt!.UsingMouse() && EnableGamepadNav)
            {
                MenuManager menu = SharedGameResources.Menu!;
                if (!menu.MenusOpen && Tablist.GetCurrent() == -1)
                {
                    Tablist.Unlock();
                    if (menu.IsDragging())
                    {
                        Tablist.GetNext(!TabList.GetInner, TabList.WidgetSelectAuto);
                    }
                    else
                    {
                        if (TablistCursor == -1)
                        {
                            // try to start the cursor on a menu button
                            for (int i = 0; i < MenuCount; ++i)
                            {
                                if (_menus[i]!.Enabled)
                                {
                                    Tablist.SetCurrent(_menus[i]);
                                    break;
                                }
                            }

                            // couldn't find a menu, try a regular slot
                            if (Tablist.GetCurrent() == -1)
                                Tablist.GetNext(!TabList.GetInner, TabList.WidgetSelectAuto);

                            if (Tablist.GetCurrent() == -1)
                                TablistCursor = Tablist.GetCurrent();
                        }
                        else
                        {
                            Tablist.SetCurrent(Tablist.GetWidgetByIndex(TablistCursor));
                        }
                    }
                    menu.DefocusLeft();
                    menu.DefocusRight();
                }
                else if (menu.MenusOpen)
                {
                    Tablist.Defocus();
                }

                if (menu.IsDragging())
                {
                    Tablist.SetActivateKey(Input.Accept);
                }
                else
                {
                    Tablist.SetActivateKey(Input.Actionbar);
                }
            }
            if (Tablist.GetCurrent() == -1)
            {
                Tablist.Lock();
            }

            // hero has no powers
            Avatar pc = SharedGameResources.Pc!;
            if (pc.PowerCastTimers.Count == 0)
                return;

            PowerManager powers = SharedGameResources.Powers!;
            MenuManager menuMgr = SharedGameResources.Menu!;
            MenuInventory inv = menuMgr.Inv!;

            for (uint i = 0; i < SlotsCount; i++)
            {
                if (Slots[(int)i] == null) continue;

                if (powers.IsValid(HotkeysMod[(int)i]))
                {
                    Power power = powers.Powers[HotkeysMod[(int)i]]!;

                    if (power.RequiredItems.Count == 0)
                    {
                        SetItemCount(i, -1, !IsEquipped);
                    }
                    else
                    {
                        for (int j = 0; j < power.RequiredItems.Count; ++j)
                        {
                            if (power.RequiredItems[j].Equipped)
                            {
                                if (!inv.EquipmentContain(power.RequiredItems[j].Id, 1))
                                    SetItemCount(i, 0, IsEquipped);
                                else
                                    SetItemCount(i, 1, IsEquipped);
                            }
                            else
                            {
                                if (power.RequiredItems[j].Quantity == 0)
                                {
                                    if (!inv.Inventory[global::FlareEngine.MenuInventory.Carried].Contain(power.RequiredItems[j].Id, 1))
                                        SetItemCount(i, 0, IsEquipped);
                                    else
                                        SetItemCount(i, 1, IsEquipped);
                                }
                                else
                                {
                                    SetItemCount(i, inv.Inventory[global::FlareEngine.MenuInventory.Carried].Count(power.RequiredItems[j].Id), !IsEquipped);
                                }
                            }

                            if (power.RequiredItems[j].Quantity > 0)
                                break;
                        }
                    }

                    //see if the slot should be greyed out
                    var canUsePower = pc.Stats.CanUsePower(HotkeysMod[(int)i], !StatBlock.CanUsePassive);
                    Slots[(int)i]!.Enabled = pc.PowerCooldownTimers[HotkeysMod[(int)i]]!.IsEnd()
                                          && pc.PowerCastTimers[HotkeysMod[(int)i]]!.IsEnd()
                                          && canUsePower
                                          && (TwostepSlot == -1 || TwostepSlot == i);

                    Slots[(int)i]!.SetIcon(power.Icon, WidgetSlot.NoOverlay);

                    if (canUsePower)
                    {
                        Slots[(int)i]!.Cooldown = 1;
                    }
                    else if (!pc.PowerCooldownTimers[HotkeysMod[(int)i]]!.IsEnd() && pc.PowerCooldownTimers[HotkeysMod[(int)i]]!.Duration > 0)
                    {
                        Slots[(int)i]!.Cooldown = (float)pc.PowerCooldownTimers[HotkeysMod[(int)i]]!.Current / (float)pc.PowerCooldownTimers[HotkeysMod[(int)i]]!.Duration;
                    }
                    else
                    {
                        Slots[(int)i]!.Cooldown = 1;
                    }
                }
                else
                {
                    // no valid power, so treat the slot as empty
                    Slots[(int)i]!.Enabled = true;
                    Slots[(int)i]!.Cooldown = 0;
                    Slots[(int)i]!.SetIcon(WidgetSlot.NoIcon, WidgetSlot.NoOverlay);
                }

                if (SlotFailCooldown[(int)i] > 0)
                    SlotFailCooldown[(int)i]--;
            }

        }

        public override void Render()
        {

            base.Render();

            // draw hotkeyed icons
            for (uint i = 0; i < SlotsCount; i++)
            {
                if (Slots[(int)i] == null) continue;

                Slots[(int)i]!.ShowDisabledOverlay = (Hotkeys[(int)i] != 0);

                if (Hotkeys[(int)i] == 0 || PowersOverlapSlots)
                {
                    // TODO move this to WidgetSlot?
                    if (_spriteEmptyslot != null)
                    {
                        _spriteEmptyslot.SetDestFromRect(Slots[(int)i]!.Pos);
                        SharedResources.RenderDevice!.Render(_spriteEmptyslot);
                    }
                }
                Slots[(int)i]!.Render();
            }

            // render primary menu buttons
            for (uint i = 0; i < MenuCount; i++)
            {
                if (_menus[i]!.Enabled)
                {
                    _menus[i]!.Highlight = (RequiresAttention[i] && _menus[i]!.Enabled && !_menus[i]!.InFocus);
                    _menus[i]!.Render();
                }
            }
        }

        /// <summary>
        /// On mouseover, show tooltip for buttons
        /// </summary>
        public void RenderTooltips(Int2 position)
        {
            Avatar pc = SharedGameResources.Pc!;
            if (SharedResources.Inpt!.UsingMouse() && (pc.UsingMain1 || pc.UsingMain2))
                return;

            TooltipData tipData = new TooltipData();

            // menus
            for (uint i = 0; i < MenuCount; ++i)
            {
                if (_menus[i]!.Enabled && Utils.IsWithinRect(_menus[i]!.Pos, position))
                {
                    if (SharedResources.Settings!.Colorblind && RequiresAttention[i])
                        tipData.AddText(MenuTitles[i] + " (*)");
                    else
                        tipData.AddText(MenuTitles[i]);

                    if (MenuLabels[(int)i] != "")
                    {
                        tipData.AddText(MenuLabels[(int)i]);
                    }

                    SharedResources.Tooltipm!.Push(tipData, position, TooltipData.StyleFloat);
                    break;
                }
            }
            tipData.Clear();

            for (uint i = 0; i < SlotsCount; i++)
            {
                if (Slots[(int)i] != null && Utils.IsWithinRect(Slots[(int)i]!.Pos, position))
                {
                    if (HotkeysMod[(int)i] != 0)
                    {
                        SharedGameResources.Menu!.Pow!.CreateTooltipFromActionBar(tipData, i, TooltipLength);
                    }
                    tipData.AddText(Labels[(int)i]);
                }
            }

            SharedResources.Tooltipm!.Push(tipData, position, TooltipData.StyleFloat);
        }

        /// <summary>
        /// After dragging a power or item onto the action bar, set as new hotkey
        /// </summary>
        public void Drop(Int2 mouse, PowerID powerIndex, bool rearranging)
        {
            PowerManager powers = SharedGameResources.Powers!;
            if (!powers.IsValid(powerIndex) || powers.Powers[powerIndex]!.NoActionbar)
                return;

            for (uint i = 0; i < SlotsCount; i++)
            {
                if (Slots[(int)i] != null && Utils.IsWithinRect(Slots[(int)i]!.Pos, mouse))
                {
                    if (rearranging)
                    {
                        if (PreventChanging[(int)i])
                        {
                            ActionReturn(powerIndex);
                            return;
                        }
                        if ((Locked[(int)i] && !Locked[DragPrevSlot]) || (!Locked[(int)i] && Locked[DragPrevSlot]))
                        {
                            Locked[(int)i] = !Locked[(int)i];
                            Locked[DragPrevSlot] = !Locked[DragPrevSlot];
                        }
                        Hotkeys[DragPrevSlot] = Hotkeys[(int)i];
                    }
                    else if (Locked[(int)i] || PreventChanging[(int)i]) return;
                    Hotkeys[(int)i] = powerIndex;

                    // we need to set the icon here instead of depending on logic() to do it, since MenuManager's action picker may be blocking it
                    Slots[(int)i]!.SetIcon(powers.Powers[powerIndex]!.Icon, WidgetSlot.NoOverlay);

                    Updated = true;
                    return;
                }
            }
        }

        /// <summary>
        /// Return the power to the last clicked on slot
        /// </summary>
        public void ActionReturn(PowerID powerIndex)
        {
            Drop(LastMouse, powerIndex, !Reorder);
        }

        /// <summary>
        /// CTRL-click a hotkey to clear it
        /// </summary>
        public void Remove(Int2 mouse)
        {
            for (uint i = 0; i < SlotsCount; i++)
            {
                if (Slots[(int)i] != null && Utils.IsWithinRect(Slots[(int)i]!.Pos, mouse))
                {
                    if (!Locked[(int)i])
                    {
                        ClearSlot((int)i);
                        Updated = true;
                    }
                    return;
                }
            }
        }

        /// <summary>
        /// If pressing an action key (keyboard or mouseclick) and the power can be used,
        /// add that power to the action queue
        /// </summary>
        public void CheckAction(List<ActionData> actionQueue)
        {
            Settings settings = SharedResources.Settings!;
            InputState inpt = SharedResources.Inpt!;
            Avatar pc = SharedGameResources.Pc!;
            PowerManager powers = SharedGameResources.Powers!;
            MapRenderer mapr = SharedGameResources.Mapr!;
            MenuManager menu = SharedGameResources.Menu!;

            bool enableMmAttack = (!settings.MouseMove || inpt.Pressing[Input.Shift] || inpt.UsingTouchscreen());
            bool enableMain1 = (!inpt.UsingTouchscreen() || (!menu.MenusOpen && menu.TouchControls!.CheckAllowMain1())) && (settings.MouseMoveSwap || enableMmAttack);
            bool enableMain2 = !settings.MouseMoveSwap || enableMmAttack;

            uint mmSlot = settings.MouseMoveSwap ? (uint)11 : (uint)10;
            bool mouseMoveTarget = false;
            if (settings.MouseMove)
            {
                mouseMoveTarget = pc.MmTargetObject == Avatar.MmTargetEntity &&
                                    powers.CheckCombatRange(powers.CheckReplaceByEffect(HotkeysMod[(int)mmSlot], pc.Stats), pc.Stats, pc.MmTargetObjectPos) &&
                                    mapr.Collider.LineOfSight(pc.Stats.Pos.X, pc.Stats.Pos.Y, pc.MmTargetObjectPos.X, pc.MmTargetObjectPos.Y);

                if (mouseMoveTarget && pc.Stats.CurState == StatBlock.EntityMove)
                {
                    pc.Stats.CurState = StatBlock.EntityStance;
                }
                else if (!mouseMoveTarget && pc.MmTargetObject == Avatar.MmTargetEntity && pc.Stats.CurState == StatBlock.EntityStance)
                {
                    pc.Stats.CurState = StatBlock.EntityMove;
                }
            }

            // check click and hotkey actions
            for (uint i = 0; i < SlotsCount; i++)
            {
                ActionData action = new ActionData();
                action.Hotkey = i;
                bool haveAim = false;
                SlotActivated[(int)i] = false;

                if (Slots[(int)i] == null) continue;

                if (i == mmSlot && mouseMoveTarget)
                {
                    action.Power = HotkeysMod[(int)i];
                    haveAim = true;
                }
                // part two of two step activation
                else if (TwostepSlot == i && ((inpt.Pressing[Input.Main1] && !inpt.Lock[Input.Main1]) || (inpt.Pressing[Input.Main2] && !inpt.Lock[Input.Main2])))
                {
                    haveAim = true;
                    action.Power = HotkeysMod[(int)i];
                    TwostepSlot = -1;
                    if (inpt.Pressing[Input.Main1]) inpt.Lock[Input.Main1] = true;
                    if (inpt.Pressing[Input.Main2]) inpt.Lock[Input.Main2] = true;
                }

                // mouse/touch click
                else if ((inpt.Mode == InputState.ModeTouchscreen && TouchSlot == Slots[(int)i]) || (inpt.Mode != InputState.ModeTouchscreen && inpt.UsingMouse() && !pc.UsingMain1 && !pc.UsingMain2 && Slots[(int)i]!.CheckClick() == WidgetSlot.ActivateResult))
                {
                    TouchSlot = null;
                    haveAim = false;
                    SlotActivated[(int)i] = true;
                    action.Power = HotkeysMod[(int)i];

                    // if a power requires a fixed target (like teleportation), break up activation into two parts
                    // the first step is to mark the slot that was clicked on
                    // NOTE only works for enabled slots
                    if (powers.IsValid(action.Power))
                    {
                        Power power = powers.Powers[action.Power]!;
                        if (power.StartingPos == Power.StartingPosTarget || power.BuffTeleport)
                        {
                            if (Slots[(int)i]!.Enabled)
                            {
                                TwostepSlot = (int)i;
                                action.Power = 0;
                            }
                            else
                            {
                                TwostepSlot = -1;
                                action.Power = 0;
                            }
                        }
                        else
                        {
                            TwostepSlot = -1;
                        }
                    }
                }

                // joystick/keyboard action button
                else if (!inpt.UsingMouse() && Slots[(int)i]!.CheckClick() == WidgetSlot.ActivateResult)
                {
                    haveAim = false;
                    SlotActivated[(int)i] = true;
                    action.Power = HotkeysMod[(int)i];
                    TwostepSlot = -1;
                }

                // pressing hotkey
                else if (i < 10 && inpt.Pressing[(int)i + Input.Bar1])
                {
                    haveAim = inpt.UsingMouse();
                    action.Power = HotkeysMod[(int)i];
                    TwostepSlot = -1;
                }
                else if (i == 10 && inpt.Pressing[Input.Main1] && !inpt.Lock[Input.Main1] && enableMain1 && TwostepSlot == -1)
                {
                    haveAim = inpt.UsingMouse();
                    action.Power = HotkeysMod[10];
                    TwostepSlot = -1;
                }
                else if (i == 11 && inpt.Pressing[Input.Main2] && !inpt.Lock[Input.Main2] && enableMain2 && TwostepSlot == -1)
                {
                    haveAim = inpt.UsingMouse();
                    action.Power = HotkeysMod[11];
                    TwostepSlot = -1;
                }

                // a power slot was activated
                if (powers.IsValid(action.Power))
                {
                    Power power = powers.Powers[action.Power]!;

                    bool notEnoughResources = false;
                    if (pc.Stats.Mp < power.RequiresMp && SlotFailCooldown[(int)i] == 0)
                    {
                        pc.LogMsg(SharedResources.Msg!.Get("Not enough MP."), Avatar.MsgNormal);
                        notEnoughResources = true;
                    }
                    for (int j = 0; j < SharedResources.Eset!.ResourceStats.Stats.Count; ++j)
                    {
                        if (pc.Stats.ResourceStats[j] < power.RequiresResourceStat[j] && SlotFailCooldown[(int)i] == 0)
                        {
                            pc.LogMsg(SharedResources.Eset.ResourceStats.Stats[j].TextLogLow, Avatar.MsgNormal);
                            notEnoughResources = true;
                        }
                    }

                    if (notEnoughResources)
                    {
                        SlotFailCooldown[(int)i] = SharedResources.Settings!.MaxFramesPerSec;
                        SharedResources.Snd!.Play(SfxUnableToCast, "ACT_NO_MP", SoundManager.NoPos, !SoundManager.Loop);
                        continue;
                    }

                    SlotFailCooldown[(int)i] = (int)pc.PowerCastTimers[action.Power]!.Duration;

                    action.InstantItem = false;
                    if (power.NewState == Power.StateInstant)
                    {
                        for (int j = 0; j < power.RequiredItems.Count; ++j)
                        {
                            if (power.RequiredItems[j].Id > 0 && !power.RequiredItems[j].Equipped)
                            {
                                action.InstantItem = true;
                                break;
                            }
                        }
                    }

                    // set the target depending on how the power was triggered
                    if (haveAim && settings.MouseAim && (settings.MouseMove || !inpt.UsingTouchscreen()))
                    {
                        action.Target = pc.Stats.Pos;

                        if (power.TargetNearest > 0)
                        {
                            if (!power.RequiresCorpse && powers.CheckNearestTargeting(power, pc.Stats, false))
                            {
                                action.Target = pc.Stats.TargetNearest!.Pos;
                            }
                            else if (power.RequiresCorpse && powers.CheckNearestTargeting(power, pc.Stats, true))
                            {
                                action.Target = pc.Stats.TargetNearestCorpse!.Pos;
                            }
                        }
                        else if (mouseMoveTarget)
                        {
                            action.Target = pc.MmTargetObjectPos;
                        }
                        else
                        {
                            if (power.AimAssist)
                                action.Target = Utils.ScreenToMap(inpt.Mouse.X, inpt.Mouse.Y + SharedResources.Eset.Misc.AimAssist, mapr.Cam.Pos.X, mapr.Cam.Pos.Y);
                            else
                                action.Target = Utils.ScreenToMap(inpt.Mouse.X, inpt.Mouse.Y, mapr.Cam.Pos.X, mapr.Cam.Pos.Y);
                        }
                    }
                    else
                    {
                        action.Target = Utils.CalcVector(pc.Stats.Pos, pc.Stats.Direction, pc.Stats.MeleeRange);
                    }

                    bool canUsePower = Slots[(int)i]!.Enabled &&
                        (power.NewState == Power.StateInstant || (pc.Stats.Cooldown.IsEnd() && pc.Stats.CurState != StatBlock.EntityPower && pc.Stats.CurState != StatBlock.EntityHit)) &&
                        powers.HasValidTarget(action.Power, pc.Stats, action.Target);

                    // add it to the queue
                    if (canUsePower)
                    {
                        if (i != mmSlot && !action.InstantItem)
                        {
                            pc.MmTargetObject = Avatar.MmTargetNone;
                        }

                        actionQueue.Add(action);
                    }
                }
                else
                {
                    // if we're not triggering an action that is currently in the queue,
                    // remove it from the queue
                    for (int j = actionQueue.Count; j > 0; --j)
                    {
                        if (!actionQueue[j - 1].ActivatedFromInventory && actionQueue[j - 1].Hotkey == i)
                            actionQueue.RemoveAt(j - 1);
                    }
                }
            }
        }

        /// <summary>
        /// If clicking while a menu is open, assume the player wants to rearrange the action bar
        /// </summary>
        public PowerID CheckDrag(Int2 mouse)
        {
            PowerID powerIndex = 0;

            for (uint i = 0; i < SlotsCount; i++)
            {
                if (Slots[(int)i] != null && Utils.IsWithinRect(Slots[(int)i]!.Pos, mouse))
                {
                    if (PreventChanging[(int)i])
                        return 0;

                    DragPrevSlot = (int)i;
                    powerIndex = Hotkeys[(int)i];
                    ClearSlot((int)i);
                    LastMouse = mouse;
                    Updated = true;
                    TwostepSlot = -1;
                    return powerIndex;
                }
            }

            return 0;
        }

        /// <summary>
        /// if clicking a menu, act as if the player pressed that menu's hotkey
        /// </summary>
        public void CheckMenu(ref bool menuC, ref bool menuI, ref bool menuP, ref bool menuL)
        {
            Avatar pc = SharedGameResources.Pc!;
            if (SharedResources.Inpt!.UsingMouse() && (pc.UsingMain1 || pc.UsingMain2))
                return;

            if (_menus[MenuCharacter]!.Enabled && _menus[MenuCharacter]!.CheckClick() != WidgetSlot.NoClick)
            {
                menuC = true;
                _menus[MenuCharacter]!.Deactivate();
                DefocusTabLists();
            }
            else if (_menus[MenuInventory]!.Enabled && _menus[MenuInventory]!.CheckClick() != WidgetSlot.NoClick)
            {
                menuI = true;
                _menus[MenuInventory]!.Deactivate();
                DefocusTabLists();
            }
            else if (_menus[MenuPowers]!.Enabled && _menus[MenuPowers]!.CheckClick() != WidgetSlot.NoClick)
            {
                menuP = true;
                _menus[MenuPowers]!.Deactivate();
                DefocusTabLists();
            }
            else if (_menus[MenuLog]!.Enabled && _menus[MenuLog]!.CheckClick() != WidgetSlot.NoClick)
            {
                menuL = true;
                _menus[MenuLog]!.Deactivate();
                DefocusTabLists();
            }
        }

        /// <summary>
        /// Set all hotkeys at once e.g. when loading a game
        /// </summary>
        public void Set(List<PowerID> powerId, bool skipEmpty)
        {
            PowerManager powers = SharedGameResources.Powers!;
            for (uint i = 0; i < SlotsCount; i++)
            {
                if (!powers.IsValid(powerId[(int)i]))
                    continue;

                if (powers.Powers[powerId[(int)i]] == null || powers.Powers[powerId[(int)i]]!.NoActionbar)
                    continue;

                if (!skipEmpty || Hotkeys[(int)i] == 0)
                    Hotkeys[(int)i] = powerId[(int)i];
            }
            Updated = true;
        }

        public bool IsWithinSlots(Int2 mouse)
        {
            for (uint i = 0; i < SlotsCount; i++)
            {
                if (Slots[(int)i] != null && Utils.IsWithinRect(Slots[(int)i]!.Pos, mouse))
                    return true;
            }
            return false;
        }

        public bool IsWithinMenus(Int2 mouse)
        {
            for (uint i = 0; i < MenuCount; i++)
            {
                if (_menus[i]!.Enabled && Utils.IsWithinRect(_menus[i]!.Pos, mouse))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Replaces the power(s) in slots that match the target_id with the power of id
        /// So a target_id of 0 will place the power in an empty slot, if available
        /// </summary>
        public void AddPower(PowerID id, PowerID targetId)
        {
            if (id == 0 && targetId != 0)
            {
                // clear all slots that match target_id
                for (uint i = 0; i < 12; ++i)
                {
                    if (Hotkeys[(int)i] == targetId && !PreventChanging[(int)i])
                    {
                        ClearSlot((int)i);
                        Updated = true;
                    }
                }
            }

            PowerManager powers = SharedGameResources.Powers!;
            if (!powers.IsValid(id))
                return;

            // some powers are explicitly prevented from being placed on the actionbar
            if (powers.Powers[id]!.NoActionbar)
                return;

            // can't put passive powers on the action bar
            if (powers.Powers[id]!.Passive)
                return;

            // if we're not replacing an existing power, avoid placing duplicate powers
            if (targetId == 0)
            {
                for (uint i = 0; i < (uint)SlotMax; ++i)
                {
                    if (Hotkeys[(int)i] == id)
                        return;
                }
            }

            // MAIN slots have priority
            for (uint i = 10; i < 12; ++i)
            {
                if (Hotkeys[(int)i] == targetId)
                {
                    if (targetId == 0 && PreventChanging[(int)i])
                    {
                        continue;
                    }
                    Hotkeys[(int)i] = id;
                    Updated = true;
                    if (targetId == 0)
                        return;
                }
            }

            // now try 0-9 slots
            for (uint i = 0; i < 10; ++i)
            {
                if (Hotkeys[(int)i] == targetId)
                {
                    if (targetId == 0 && PreventChanging[(int)i])
                    {
                        continue;
                    }
                    Hotkeys[(int)i] = id;
                    Updated = true;
                    if (targetId == 0)
                        return;
                }
            }
        }

        public Int2 GetSlotPos(int slot)
        {
            if (slot < Slots.Count)
            {
                return new Int2(Slots[slot]!.Pos.X, Slots[slot]!.Pos.Y);
            }
            else if (slot < Slots.Count + MenuCount)
            {
                int menuIndex = slot - Slots.Count;
                return new Int2(_menus[menuIndex]!.Pos.X, _menus[menuIndex]!.Pos.Y);
            }
            return default;
        }

        public WidgetSlot? GetSlotFromPosition(Int2 position)
        {
            for (int i = 0; i < Slots.Count; ++i)
            {
                if (Slots[i] != null && Utils.IsWithinRect(Slots[i]!.Pos, position))
                    return Slots[i];
            }
            return null;
        }

        public int GetCurrentSlotIndexFromTablist()
        {
            int tablistIndex = GetCurrentTabList()!.GetCurrent();
            Widget? currentSlot = GetCurrentTabList()!.GetWidgetByIndex(tablistIndex);
            if (currentSlot == null)
                return Slots.Count + MenuCount;

            for (int i = 0; i < Slots.Count; ++i)
            {
                if (Slots[i] == currentSlot)
                    return i;
            }
            for (int i = 0; i < MenuCount; ++i)
            {
                if (_menus[i] == currentSlot)
                    return i + Slots.Count;
            }

            return Slots.Count + MenuCount;
        }

        public void SetupMenuButtons(MenuCharacter chr, MenuInventory inv, MenuPowers pow, MenuLog questlog)
        {
            _menus[MenuCharacter]!.Enabled = chr.Enabled;
            _menus[MenuInventory]!.Enabled = inv.Enabled;
            _menus[MenuPowers]!.Enabled = pow.Enabled;
            _menus[MenuLog]!.Enabled = questlog.Enabled;

            if (chr.Enabled)
                Tablist.Add(_menus[MenuCharacter]);
            if (inv.Enabled)
                Tablist.Add(_menus[MenuInventory]);
            if (pow.Enabled)
                Tablist.Add(_menus[MenuPowers]);
            if (questlog.Enabled)
                Tablist.Add(_menus[MenuLog]);
        }
    }
}
