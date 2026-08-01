// 对应 C++ 源：MenuInventory.h + MenuInventory.cpp
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// MenuInventory
    ///
    /// 玩家背包与装备栏菜单，对应 C++ <c>class MenuInventory : public Menu</c>。
    /// 持有的 <see cref="WidgetButton"/>、<see cref="GameSlotPreview"/> 与
    /// <see cref="MenuItemStorage"/> 实例通过 <see cref="IDisposable"/> 显式释放，
    /// 释放顺序与原始析构函数 <c>~MenuInventory()</c> 一致。
    /// </summary>
    public class MenuInventory : Menu, IDisposable
    {
        public const int CtrlNone = 0;
        public const int CtrlVendor = 1;
        public const int CtrlStash = 2;

        public const int NoArea = -1;
        public const int Equipment = 0;
        public const int Carried = 1;

        public const bool AddPlaySound = true;
        public const bool AddAutoEquip = true;
        public const bool IsDragging = true;

        public const bool OnlyEmptySlots = true;

        public Rectangle CarriedArea;
        public List<Rectangle> EquippedArea = new List<Rectangle>();
        public List<int> SlotType = new List<int>();
        public List<uint> EquipmentSet = new List<uint>();

        public MenuItemStorage[] Inventory = new MenuItemStorage[2];
        public uint ActiveEquipmentSet;
        public uint MaxEquipmentSet;
        public int Currency;
        public int DragPrevSrc;
        public bool ChangedEquipment;
        public short InvCtrl;
        public string ShowBook = "";
        public Queue<ItemStack> DropStack = new Queue<ItemStack>();

        private readonly WidgetLabel _labelInventory = new WidgetLabel();
        private readonly WidgetLabel _labelCurrency = new WidgetLabel();
        private WidgetButton? _buttonClose;
        private WidgetButton? _buttonSort;

        private readonly List<WidgetButton?> _equipmentSetButton = new List<WidgetButton?>();
        private WidgetButton? _equipmentSetPrevious;
        private WidgetButton? _equipmentSetNext;
        private WidgetLabel? _equipmentSetLabel;

        private int _maxEquipped;
        private int _maxCarried;

        private Rectangle _helpPos;
        private int _carriedCols;
        private int _carriedRows;
        private readonly List<Int2> _equippedPos = new List<Int2>();
        private Int2 _carriedPos;

        private readonly Timer _tapToActivateTimer;

        private int _activatedSlot;
        private ItemID _activatedItem;

        private GameSlotPreview? _preview;
        private bool _previewEnabled;
        private Int2 _previewPos;

        private bool _sortEnabled;
        private Int2 _sortPos;

        public MenuInventory()
        {
            Settings settings = SharedResources.Settings!;
            EngineSettings eset = SharedResources.Eset!;
            MessageEngine msg = SharedResources.Msg!;
            FontEngine font = SharedResources.Font!;
            ItemManager items = SharedGameResources.Items!;

            _buttonClose = new WidgetButton(WidgetButton.CloseFile);
            _buttonSort = null;
            _equipmentSetPrevious = null;
            _equipmentSetNext = null;
            _equipmentSetLabel = null;
            _maxEquipped = 4;
            _maxCarried = 64;
            _carriedCols = 4;
            _carriedRows = 4;
            _tapToActivateTimer = new Timer((uint)(settings.MaxFramesPerSec / 3));
            _activatedSlot = -1;
            _activatedItem = 0;
            _preview = null;
            _previewEnabled = false;
            _sortEnabled = false;
            ActiveEquipmentSet = 0;
            MaxEquipmentSet = 0;
            Currency = 0;
            DragPrevSrc = -1;
            ChangedEquipment = true;
            InvCtrl = CtrlNone;
            ShowBook = "";

            Inventory[Equipment] = new MenuItemStorage();
            Inventory[Carried] = new MenuItemStorage();

            Visible = false;

            Dictionary<uint, string> rawSetButton = new Dictionary<uint, string>();
            string rawPrevious = "";
            string rawNext = "";
            string rawLabel = "";

            using FileParser infile = new FileParser();
            if (infile.Open("menus/inventory.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (ParseMenuKey(infile.Key, infile.Val))
                        continue;

                    if (infile.Key == "close")
                    {
                        Int2 pos = Parse.ToPoint(infile.Val);
                        _buttonClose!.SetBasePos(pos.X, pos.Y, Utils.AlignTopLeft);
                    }
                    else if (infile.Key == "set_button")
                    {
                        uint id = (uint)Parse.PopFirstInt(ref infile.Val);
                        rawSetButton[id] = infile.Val;
                    }
                    else if (infile.Key == "set_previous")
                    {
                        rawPrevious = infile.Val;
                    }
                    else if (infile.Key == "set_next")
                    {
                        rawNext = infile.Val;
                    }
                    else if (infile.Key == "label_equipment_set")
                    {
                        rawLabel = infile.Val;
                    }
                    else if (infile.Key == "equipment_slot")
                    {
                        Rectangle area = default;
                        Int2 pos = default;
                        int sltType;
                        int eqSet;

                        string val = infile.Val;
                        pos.X = area.X = Parse.PopFirstInt(ref val);
                        pos.Y = area.Y = Parse.PopFirstInt(ref val);
                        sltType = items.GetItemTypeIndexByString(Parse.PopFirstString(ref val));
                        eqSet = Parse.PopFirstInt(ref val);
                        area.Width = area.Height = eset.Resolutions.IconSize;

                        EquippedArea.Add(area);
                        _equippedPos.Add(pos);
                        SlotType.Add(sltType);
                        EquipmentSet.Add((uint)eqSet);
                    }
                    else if (infile.Key == "carried_area")
                    {
                        string val = infile.Val;
                        _carriedPos.X = CarriedArea.X = Parse.PopFirstInt(ref val);
                        _carriedPos.Y = CarriedArea.Y = Parse.PopFirstInt(ref val);
                    }
                    else if (infile.Key == "carried_cols")
                        _carriedCols = Math.Max(1, Parse.ToInt(infile.Val));
                    else if (infile.Key == "carried_rows")
                        _carriedRows = Math.Max(1, Parse.ToInt(infile.Val));
                    else if (infile.Key == "label_title")
                    {
                        _labelInventory.SetFromLabelInfo(Parse.PopLabelInfo(infile.Val));
                    }
                    else if (infile.Key == "currency")
                    {
                        _labelCurrency.SetFromLabelInfo(Parse.PopLabelInfo(infile.Val));
                    }
                    else if (infile.Key == "help")
                        _helpPos = Parse.ToRect(infile.Val);
                    else if (infile.Key == "preview_enabled")
                        _previewEnabled = Parse.ToBool(infile.Val);
                    else if (infile.Key == "preview_pos")
                        _previewPos = Parse.ToPoint(infile.Val);
                    else if (infile.Key == "sort_enabled")
                        _sortEnabled = Parse.ToBool(infile.Val);
                    else if (infile.Key == "sort_pos")
                        _sortPos = Parse.ToPoint(infile.Val);
                    else
                        infile.Error("MenuInventory: '%s' is not a valid key.", infile.Key);
                }
                infile.Close();
            }

            _maxEquipped = EquippedArea.Count;
            _maxCarried = _carriedCols * _carriedRows;

            CarriedArea.Width = _carriedCols * eset.Resolutions.IconSize;
            CarriedArea.Height = _carriedRows * eset.Resolutions.IconSize;

            _labelInventory.SetText(msg.Get("Inventory"));
            _labelInventory.SetColor(font.GetColor(FontEngine.ColorMenuNormal));

            _labelCurrency.SetColor(font.GetColor(FontEngine.ColorMenuNormal));

            Inventory[Equipment].InitFromList(_maxEquipped, EquippedArea, SlotType);
            Inventory[Carried].InitGrid(_maxCarried, CarriedArea, _carriedCols);

            for (int i = 0; i < _maxEquipped; i++)
            {
                Tablist.Add(Inventory[Equipment].Slots[i]!);
            }
            for (int i = 0; i < _maxCarried; i++)
            {
                Tablist.Add(Inventory[Carried].Slots[i]!);
            }

            for (int i = 0; i < EquipmentSet.Count; i++)
            {
                if (EquipmentSet[i] > MaxEquipmentSet)
                {
                    MaxEquipmentSet = EquipmentSet[i];
                }
            }

            List<uint> setButtonKeys = new List<uint>(rawSetButton.Keys);
            setButtonKeys.Sort();
            for (int keyIndex = 0; keyIndex < setButtonKeys.Count; ++keyIndex)
            {
                uint key = setButtonKeys[keyIndex];
                string buttonVal = rawSetButton[key];
                int px = Parse.PopFirstInt(ref buttonVal);
                int py = Parse.PopFirstInt(ref buttonVal);
                string icon = Parse.PopFirstString(ref buttonVal);
                _equipmentSetButton.Add(new WidgetButton(icon));
                _equipmentSetButton[^1]!.SetBasePos(px, py, Utils.AlignTopLeft);
                Tablist.Add(_equipmentSetButton[^1]!);
            }
            rawSetButton.Clear();

            if (rawPrevious != "")
            {
                int px = Parse.PopFirstInt(ref rawPrevious);
                int py = Parse.PopFirstInt(ref rawPrevious);
                string icon = Parse.PopFirstString(ref rawPrevious);
                _equipmentSetPrevious = new WidgetButton(icon);
                _equipmentSetPrevious.SetBasePos(px, py, Utils.AlignTopLeft);
                Tablist.Add(_equipmentSetPrevious);
            }

            if (rawNext != "")
            {
                int px = Parse.PopFirstInt(ref rawNext);
                int py = Parse.PopFirstInt(ref rawNext);
                string icon = Parse.PopFirstString(ref rawNext);
                _equipmentSetNext = new WidgetButton(icon);
                _equipmentSetNext.SetBasePos(px, py, Utils.AlignTopLeft);
                Tablist.Add(_equipmentSetNext);
            }

            if (rawLabel != "")
            {
                _equipmentSetLabel = new WidgetLabel();
                _equipmentSetLabel.SetFromLabelInfo(Parse.PopLabelInfo(rawLabel));

                string label = ActiveEquipmentSet + "/" + MaxEquipmentSet;
                _equipmentSetLabel.SetText(label);
                _equipmentSetLabel.SetColor(font.GetColor(FontEngine.ColorMenuNormal));
            }

            if (MaxEquipmentSet > 0)
            {
                ApplyEquipmentSet(1);
            }

            if (_background == null)
                SetBackground("images/menus/inventory.png");

            if (_previewEnabled)
            {
                _preview = new GameSlotPreview();
                _preview.SetStatBlock(SharedGameResources.Pc!.Stats);
                _preview.SetDirection(6);
                _preview.LoadDefaultGraphics();
                _preview.LoadGraphicsFromInventory(this);
            }

            if (_sortEnabled)
            {
                _buttonSort = new WidgetButton(WidgetButton.SortItemsFile);

                if (_buttonSort != null)
                {
                    _buttonSort.SetBasePos(_sortPos.X, _sortPos.Y, Utils.AlignTopLeft);
                    Tablist.Add(_buttonSort);

                    Inventory[Carried].SortTooltip = text => _buttonSort.Tooltip = text;
                    Inventory[Carried].RefreshSortTooltip();
                }
            }

            Align();
        }

        public override void Dispose()
        {
            _buttonClose?.Dispose();
            _buttonClose = null;
            _buttonSort?.Dispose();
            _buttonSort = null;
            for (int i = 0; i < _equipmentSetButton.Count; i++)
            {
                _equipmentSetButton[i]?.Dispose();
                _equipmentSetButton[i] = null;
            }
            _equipmentSetNext?.Dispose();
            _equipmentSetNext = null;
            _equipmentSetPrevious?.Dispose();
            _equipmentSetPrevious = null;
            _equipmentSetLabel?.Dispose();
            _equipmentSetLabel = null;
            _preview?.Dispose();
            _preview = null;
            Inventory[Equipment]?.Dispose();
            Inventory[Carried]?.Dispose();
            _labelInventory.Dispose();
            _labelCurrency.Dispose();

            base.Dispose();
        }

        public override void Align()
        {
            base.Align();

            for (int i = 0; i < _maxEquipped; i++)
            {
                Rectangle equipped = EquippedArea[i];
                equipped.X = _equippedPos[i].X + WindowArea.X;
                equipped.Y = _equippedPos[i].Y + WindowArea.Y;
                EquippedArea[i] = equipped;
            }

            CarriedArea.X = _carriedPos.X + WindowArea.X;
            CarriedArea.Y = _carriedPos.Y + WindowArea.Y;

            Inventory[Equipment].SetPos(WindowArea.X, WindowArea.Y);
            Inventory[Carried].SetPos(WindowArea.X, WindowArea.Y);

            _buttonClose!.SetPos(WindowArea.X, WindowArea.Y);

            if (_equipmentSetButton.Count > 0)
            {
                for (int i = 0; i < _equipmentSetButton.Count; i++)
                {
                    _equipmentSetButton[i]!.SetPos(WindowArea.X, WindowArea.Y);
                }
            }

            if (_equipmentSetPrevious != null) _equipmentSetPrevious.SetPos(WindowArea.X, WindowArea.Y);
            if (_equipmentSetNext != null) _equipmentSetNext.SetPos(WindowArea.X, WindowArea.Y);
            if (_equipmentSetLabel != null) _equipmentSetLabel.SetPos(WindowArea.X, WindowArea.Y);

            _labelInventory.SetPos(WindowArea.X, WindowArea.Y);
            _labelCurrency.SetPos(WindowArea.X, WindowArea.Y);

            if (_preview != null)
                _preview.SetPos(new Int2(WindowArea.X + _previewPos.X, WindowArea.Y + _previewPos.Y));

            if (_buttonSort != null)
                _buttonSort.SetPos(WindowArea.X, WindowArea.Y);
        }

        public void Logic()
        {
            Avatar pc = SharedGameResources.Pc!;
            EngineSettings eset = SharedResources.Eset!;
            MessageEngine msg = SharedResources.Msg!;
            ItemManager items = SharedGameResources.Items!;
            InputState inpt = SharedResources.Inpt!;
            SoundManager snd = SharedResources.Snd!;
            MenuManager menu = SharedGameResources.Menu!;

            if (pc.Stats.DeathPenalty && eset.DeathPenalty.Enabled)
            {
                string deathMessage = "";

                if (eset.DeathPenalty.Currency > 0)
                {
                    if (Currency > 0)
                        RemoveCurrency((int)((float)Currency * eset.DeathPenalty.Currency / 100f));
                    deathMessage += msg.GetV("Lost %s%% of %s.", Utils.FloatToString(eset.DeathPenalty.Currency, eset.NumberFormat.DeathPenalty), eset.Loot.Currency) + ' ';
                }

                if (eset.DeathPenalty.Xp > 0)
                {
                    if (pc.Stats.Xp > 0)
                        pc.Stats.Xp -= (ulong)((float)pc.Stats.Xp * eset.DeathPenalty.Xp / 100f);
                    deathMessage += msg.GetV("Lost %s%% of total XP.", Utils.FloatToString(eset.DeathPenalty.Xp, eset.NumberFormat.DeathPenalty)) + ' ';
                }
                else if (eset.DeathPenalty.XpCurrent > 0)
                {
                    if (pc.Stats.Xp - eset.Xp.GetLevelXP(pc.Stats.Level) > 0)
                        pc.Stats.Xp -= (ulong)((float)(pc.Stats.Xp - eset.Xp.GetLevelXP(pc.Stats.Level)) * eset.DeathPenalty.XpCurrent / 100f);
                    deathMessage += msg.GetV("Lost %s%% of current level XP.", Utils.FloatToString(eset.DeathPenalty.XpCurrent, eset.NumberFormat.DeathPenalty)) + ' ';
                }

                if (pc.Stats.Xp < eset.Xp.GetLevelXP(pc.Stats.Level))
                    pc.Stats.Xp = eset.Xp.GetLevelXP(pc.Stats.Level);

                if (eset.DeathPenalty.Item)
                {
                    List<ItemID> removableItems = new List<ItemID>();
                    removableItems.Clear();
                    for (int i = 0; i < _maxEquipped; i++)
                    {
                        if (!Inventory[Equipment][i].Empty() && items.IsValid(Inventory[Equipment][i].Item))
                        {
                            if (!items.Items[(int)Inventory[Equipment][i].Item]!.QuestItem)
                                removableItems.Add(Inventory[Equipment][i].Item);
                        }
                    }
                    for (int i = 0; i < _maxCarried; i++)
                    {
                        if (!Inventory[Carried][i].Empty() && items.IsValid(Inventory[Carried][i].Item))
                        {
                            if (!items.Items[(int)Inventory[Carried][i].Item]!.QuestItem)
                                removableItems.Add(Inventory[Carried][i].Item);
                        }
                    }
                    if (removableItems.Count > 0)
                    {
                        int randomItem = Program.Rng.Next(removableItems.Count);
                        Remove(removableItems[randomItem], 1);
                        deathMessage += msg.GetV("Lost %s.", items.GetItemName(removableItems[randomItem]));
                    }
                }

                pc.LogMsg(deathMessage, Avatar.MsgNormal);

                pc.Stats.DeathPenalty = false;
            }

            pc.Stats.Currency = Currency = Inventory[Carried].Count(eset.Misc.CurrencyId);

            if (Visible)
            {
                Tablist.Logic();

                if (_buttonClose!.CheckClick())
                {
                    Visible = false;
                    snd.Play(SfxClose, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);
                }

                if (DragPrevSrc == -1)
                {
                    ClearHighlight();
                }

                if (_equipmentSetButton.Count > 0)
                {
                    for (int i = 0; i < _equipmentSetButton.Count; i++)
                    {
                        if (_equipmentSetButton[i]!.CheckClick())
                        {
                            ApplyEquipmentSet((uint)i + 1);
                            ApplyEquipment();
                        }
                    }
                }

                if (_equipmentSetNext != null)
                {
                    if (_equipmentSetNext.CheckClick())
                    {
                        ApplyNextEquipmentSet();
                        ApplyEquipment();
                    }
                }

                if (_equipmentSetPrevious != null)
                {
                    if (_equipmentSetPrevious.CheckClick())
                    {
                        ApplyPreviousEquipmentSet();
                        ApplyEquipment();
                    }
                }

                if (_buttonSort != null && _buttonSort.CheckClick())
                {
                    Inventory[Carried].SortNext();
                }
            }

            if (MaxEquipmentSet > 0 && !menu.Pause)
            {
                if (inpt.Pressing[Input.EquipmentSwap] && !inpt.Lock[Input.EquipmentSwap])
                {
                    inpt.Lock[Input.EquipmentSwap] = true;
                    ApplyNextEquipmentSet();
                    ApplyEquipment();
                    ClearHighlight();
                }
                else if (inpt.Pressing[Input.EquipmentSwapPrev] && !inpt.Lock[Input.EquipmentSwapPrev])
                {
                    inpt.Lock[Input.EquipmentSwapPrev] = true;
                    ApplyPreviousEquipmentSet();
                    ApplyEquipment();
                    ClearHighlight();
                }
            }

            _tapToActivateTimer.Tick();

            if (_preview != null)
                _preview.Logic();
        }

        public override void Render()
        {
            var msg = SharedResources.Msg!;
            var eset = SharedResources.Eset!;

            if (!Visible) return;

            base.Render();

            _buttonClose!.Render();

            if (_equipmentSetButton.Count > 0)
            {
                for (int i = 0; i < _equipmentSetButton.Count; i++)
                {
                    _equipmentSetButton[i]!.Render();
                }
            }
            if (_equipmentSetPrevious != null) _equipmentSetPrevious.Render();
            if (_equipmentSetNext != null) _equipmentSetNext.Render();
            if (_equipmentSetLabel != null) _equipmentSetLabel.Render();

            _labelInventory.Render();

            if (!_labelCurrency.IsHidden())
            {
                _labelCurrency.SetText(msg.GetV("%d %s", Currency, eset.Loot.Currency));
                _labelCurrency.Render();
            }

            Inventory[Equipment].Render();
            Inventory[Carried].Render();

            if (_preview != null)
                _preview.Render();

            if (_buttonSort != null)
                _buttonSort.Render();
        }

        public int AreaOver(Int2 position)
        {
            if (Utils.IsWithinRect(CarriedArea, position))
            {
                return Carried;
            }
            else
            {
                for (int i = 0; i < EquippedArea.Count; i++)
                {
                    if (Utils.IsWithinRect(EquippedArea[i], position))
                    {
                        return Equipment;
                    }
                }
            }

            if (Utils.IsWithinRect(WindowArea, position))
            {
                return NoArea;
            }

            return -2;
        }

        public void RenderTooltips(Int2 position)
        {
            if (!Visible || !Utils.IsWithinRect(WindowArea, position))
                return;

            Avatar pc = SharedGameResources.Pc!;
            ItemManager items = SharedGameResources.Items!;
            InputState inpt = SharedResources.Inpt!;
            EngineSettings eset = SharedResources.Eset!;
            MessageEngine msg = SharedResources.Msg!;
            TooltipManager tooltipm = SharedResources.Tooltipm!;

            int area = AreaOver(position);
            int slot = -1;
            TooltipData tipData = new TooltipData();

            if (area < 0)
            {
                if (position.X >= WindowArea.X + _helpPos.X && position.Y >= WindowArea.Y + _helpPos.Y && position.X < WindowArea.X + _helpPos.X + _helpPos.Width && position.Y < WindowArea.Y + _helpPos.Y + _helpPos.Height)
                {
                    tipData.AddText(msg.Get("Pick up item(s):") + " " + inpt.GetBindingString(Input.Main1));
                    tipData.AddText(msg.Get("Use or equip item:") + " " + inpt.GetBindingString(Input.Main2) + "\n");
                    tipData.AddText(msg.GetV("%s modifiers", inpt.GetBindingString(Input.Main1)));
                    tipData.AddText(msg.Get("Select a quantity of item:") + " " + inpt.GetBindingString(Input.Shift));

                    if (InvCtrl == CtrlStash)
                        tipData.AddText(msg.Get("Stash item stack:") + " " + inpt.GetBindingString(Input.Ctrl));
                    else if (InvCtrl == CtrlVendor || eset.Misc.SellWithoutVendor)
                        tipData.AddText(msg.Get("Sell item stack:") + " " + inpt.GetBindingString(Input.Ctrl));
                }
                tooltipm.Push(tipData, position, TooltipData.StyleFloat);
            }
            else
            {
                slot = Inventory[area].SlotOver(position);
            }

            if (slot == -1)
                return;

            tipData.Clear();

            if (area == Equipment)
                if (!IsEquipSlotActive(slot))
                    return;

            if (Inventory[area][slot].Item > 0)
            {
                tipData = Inventory[area].CheckTooltip(position, pc.Stats, ItemManager.PlayerInv, ItemManager.TooltipInputHint);
            }
            else if (area == Equipment && Inventory[area][slot].Empty())
            {
                tipData.AddText(msg.Get(items.GetItemType(SlotType[slot]).Name));
            }

            tooltipm.Push(tipData, position, TooltipData.StyleFloat);
        }

        public ItemStack Click(Int2 position)
        {
            ItemStack item = new ItemStack();
            InputState inpt = SharedResources.Inpt!;

            DragPrevSrc = AreaOver(position);
            if (DragPrevSrc > -1)
            {
                item = Inventory[DragPrevSrc].Click(position);

                if (inpt.UsingTouchscreen())
                {
                    Tablist.SetCurrent(Inventory[DragPrevSrc].CurrentSlot);
                    _tapToActivateTimer.Reset(Timer.Begin);
                }

                if (item.Empty())
                {
                    DragPrevSrc = -1;
                    return item;
                }

                if (DragPrevSrc == Equipment)
                {
                    Avatar pc = SharedGameResources.Pc!;
                    if (pc.Stats.Humanoid)
                    {
                        UpdateEquipment(Inventory[Equipment].DragPrevSlot);
                    }
                    else
                    {
                        ItemReturn(item);
                        item.Clear();
                    }
                }
            }

            return item;
        }

        public void ItemReturn(ItemStack stack)
        {
            if (DragPrevSrc == -1)
            {
                Add(stack, Carried, ItemStorage.NoSlot, !AddPlaySound, !AddAutoEquip);
            }
            else
            {
                int prevSlot = Inventory[DragPrevSrc].DragPrevSlot;
                Inventory[DragPrevSrc].ItemReturn(stack);
                if (DragPrevSrc == Equipment)
                {
                    UpdateEquipment(prevSlot);
                }
            }
            DragPrevSrc = -1;
        }

        public bool Drop(Int2 position, ItemStack stack)
        {
            ItemManager items = SharedGameResources.Items!;
            items.PlaySound(stack.Item);

            bool success = true;

            int area = AreaOver(position);
            if (area < 0)
            {
                if (DragPrevSrc == -1)
                {
                    success = Add(stack, Carried, ItemStorage.NoSlot, !AddPlaySound, AddAutoEquip);
                }
                else
                {
                    ItemReturn(stack);
                }
                return success;
            }

            int slot = Inventory[area].SlotOver(position);
            if (slot == -1)
            {
                if (DragPrevSrc == -1)
                {
                    success = Add(stack, Carried, ItemStorage.NoSlot, !AddPlaySound, AddAutoEquip);
                }
                else
                {
                    ItemReturn(stack);
                }
                return success;
            }

            int dragPrevSlot = -1;
            if (DragPrevSrc != -1)
                dragPrevSlot = Inventory[DragPrevSrc].DragPrevSlot;

            Avatar pc = SharedGameResources.Pc!;

            if (area == Equipment)
            {
                if (items.IsValid(stack.Item) && SlotType[slot] == items.Items[(int)stack.Item]!.Type && items.RequirementsMet(pc.Stats, stack.Item) && pc.Stats.Humanoid && Inventory[Equipment].Slots[slot]!.Enabled)
                {
                    if (Inventory[area][slot].Item == stack.Item)
                    {
                        success = Add(stack, area, slot, !AddPlaySound, !AddAutoEquip);
                    }
                    else
                    {
                        if (!Inventory[area][slot].Empty())
                            ItemReturn(Inventory[area][slot]);
                        Inventory[area][slot] = stack.Clone();
                        UpdateEquipment(slot);
                        ApplyEquipment();

                        if (items.Items[(int)stack.Item]!.Power > 0)
                        {
                            SharedGameResources.MenuAct!.AddPower(items.Items[(int)stack.Item]!.Power, 0);
                        }
                    }
                }
                else
                {
                    ItemReturn(stack);
                    UpdateEquipment(slot);
                    ApplyEquipment();
                }
            }
            else if (area == Carried)
            {
                if (DragPrevSrc == Carried)
                {
                    if (slot != dragPrevSlot)
                    {
                        if (Inventory[area][slot].Item == stack.Item)
                        {
                            success = Add(stack, area, slot, !AddPlaySound, !AddAutoEquip);
                        }
                        else if (Inventory[area][slot].Empty())
                        {
                            Inventory[area][slot] = stack.Clone();
                        }
                        else if (dragPrevSlot != -1 && Inventory[DragPrevSrc][dragPrevSlot].Empty())
                        {
                            ItemReturn(Inventory[area][slot]);
                            Inventory[area][slot] = stack.Clone();
                        }
                        else
                        {
                            ItemReturn(stack);
                        }
                    }
                    else
                    {
                        ItemReturn(stack);

                        InputState inpt = SharedResources.Inpt!;
                        if (inpt.UsingTouchscreen() && !_tapToActivateTimer.IsEnd() && stack.Quantity == 1 && items.IsValid(stack.Item) && items.Items[(int)stack.Item]!.Book != "")
                        {
                            Activate(position);
                        }
                    }
                }
                else
                {
                    if (Inventory[area][slot].Item == stack.Item || DragPrevSrc == -1)
                    {
                        success = Add(stack, area, slot, !AddPlaySound, !AddAutoEquip);
                    }
                    else if (Inventory[area][slot].Empty())
                    {
                        Inventory[area][slot] = stack.Clone();
                    }
                    else if (
                        Inventory[Equipment][dragPrevSlot].Empty()
                        && Inventory[Carried][slot].Item != stack.Item
                        && items.IsValid(Inventory[Carried][slot].Item)
                        && items.Items[(int)Inventory[Carried][slot].Item]!.Type == SlotType[dragPrevSlot]
                        && items.RequirementsMet(pc.Stats, Inventory[Carried][slot].Item)
                    )
                    {
                        ItemReturn(Inventory[area][slot]);
                        UpdateEquipment(dragPrevSlot);

                        if (items.Items[(int)Inventory[Equipment][dragPrevSlot].Item]!.Power > 0)
                        {
                            SharedGameResources.MenuAct!.AddPower(items.Items[(int)Inventory[Equipment][dragPrevSlot].Item]!.Power, 0);
                        }

                        Inventory[area][slot] = stack.Clone();

                        ApplyEquipment();
                    }
                    else
                    {
                        ItemReturn(stack);
                    }
                }
            }

            DragPrevSrc = -1;

            return success;
        }

        public void Activate(Int2 position)
        {
            Vector2 nullpt = default;
            nullpt.X = nullpt.Y = 0;

            Avatar pc = SharedGameResources.Pc!;
            ItemManager items = SharedGameResources.Items!;
            PowerManager powers = SharedGameResources.Powers!;
            EventManager eventm = SharedGameResources.Eventm!;
            MessageEngine msg = SharedResources.Msg!;

            int slot = Inventory[Carried].SlotOver(position);
            if (slot == -1)
                return;

            ItemStack stack = Inventory[Carried][slot];

            if (stack.Empty() || !items.IsValid(stack.Item))
                return;

            if (items.Items[(int)stack.Item]!.Script != "")
            {
                eventm.ExecuteScript(items.Items[(int)stack.Item]!.Script, pc.Stats.Pos.X, pc.Stats.Pos.Y);
            }
            else if (items.Items[(int)stack.Item]!.Book != "")
            {
                ShowBook = items.Items[(int)stack.Item]!.Book;
            }
            else if (powers.IsValid(items.Items[(int)stack.Item]!.Power) && GetEquipSlotFromItem(stack.Item, !OnlyEmptySlots) == -1)
            {
                PowerID powerId = items.Items[(int)stack.Item]!.Power;
                Power itemPower = powers.Powers[(int)powerId]!;

                for (int i = 0; i < Inventory[Equipment].GetSlotNumber(); ++i)
                {
                    ItemID id = Inventory[Equipment][i].Item;
                    if (id == 0 || items.Items[(int)id] == null)
                        continue;

                    for (int j = 0; j < items.Items[(int)id]!.ReplacePower.Count; ++j)
                    {
                        if (powerId == items.Items[(int)id]!.ReplacePower[j].Item1)
                        {
                            powerId = items.Items[(int)id]!.ReplacePower[j].Item2;
                            break;
                        }
                    }
                }

                for (int i = 0; i < itemPower.RequiredItems.Count; ++i)
                {
                    if (itemPower.RequiredItems[i].Id > 0 &&
                        itemPower.RequiredItems[i].Quantity > Inventory[Carried].Count(itemPower.RequiredItems[i].Id))
                    {
                        pc.LogMsg(msg.Get("You don't have enough of the required item."), Avatar.MsgNormal);
                        return;
                    }

                    if (itemPower.RequiredItems[i].Id == stack.Item)
                    {
                        _activatedSlot = slot;
                        _activatedItem = stack.Item;
                    }
                }

                if (!pc.Stats.CanUsePower(powerId, !StatBlock.CanUsePassive) || !pc.PowerCooldownTimers[(int)powerId]!.IsEnd())
                {
                    pc.LogMsg(msg.Get("You can't use this item right now."), Avatar.MsgNormal);
                    return;
                }

                if (!itemPower.RequiresTargeting)
                {
                    ActionData actionData = new ActionData();
                    actionData.Power = powerId;
                    actionData.ActivatedFromInventory = true;

                    actionData.Target = Utils.CalcVector(pc.Stats.Pos, pc.Stats.Direction, pc.Stats.MeleeRange);

                    if (itemPower.NewState == Power.StateInstant)
                    {
                        for (int j = 0; j < itemPower.RequiredItems.Count; ++j)
                        {
                            if (itemPower.RequiredItems[j].Id > 0 && !itemPower.RequiredItems[j].Equipped)
                            {
                                actionData.InstantItem = true;
                                break;
                            }
                        }
                    }

                    pc.ActionQueue.Add(actionData);
                }
                else
                {
                    pc.LogMsg(msg.Get("This item can only be used from the action bar."), Avatar.MsgNormal);
                }
            }
            else if (pc.Stats.Humanoid && items.GetItemType(items.Items[(int)stack.Item]!.Type).Name != "")
            {
                int equipSlot = GetEquipSlotFromItem(Inventory[Carried].Storage![slot].Item, !OnlyEmptySlots);

                if (equipSlot >= 0)
                {
                    ItemStack activeStack = Click(position);

                    if (Inventory[Equipment][equipSlot].Item == activeStack.Item)
                    {
                        Add(activeStack, Equipment, equipSlot, !AddPlaySound, !AddAutoEquip);
                    }
                    else if (Inventory[Equipment][equipSlot].Empty())
                    {
                        Inventory[Equipment][equipSlot] = activeStack;
                    }
                    else
                    {
                        if (stack.Empty())
                        {
                            ItemReturn(Inventory[Equipment][equipSlot]);
                        }
                        else
                        {
                            Add(Inventory[Equipment][equipSlot], Carried, ItemStorage.NoSlot, AddPlaySound, !AddAutoEquip);
                        }
                        Inventory[Equipment][equipSlot] = activeStack;
                    }

                    UpdateEquipment(equipSlot);
                    items.PlaySound(Inventory[Equipment][equipSlot].Item);

                    if (items.Items[(int)activeStack.Item]!.Power > 0)
                    {
                        SharedGameResources.MenuAct!.AddPower(items.Items[(int)activeStack.Item]!.Power, 0);
                    }

                    ApplyEquipment();
                }
                else if (equipSlot == -1)
                {
                    Utils.LogError("MenuInventory: Can't find equip slot, corresponding to type %s", items.GetItemType(items.Items[(int)stack.Item]!.Type).Id);
                }
            }

            DragPrevSrc = -1;
        }

        public bool Add(ItemStack stack, int area, int slot, bool playSound, bool autoEquip)
        {
            if (stack.Empty())
                return true;

            ItemManager items = SharedGameResources.Items!;
            Settings settings = SharedResources.Settings!;
            Avatar pc = SharedGameResources.Pc!;

            if (!items.IsValid(stack.Item))
                return false;

            bool success = true;

            if (playSound)
                items.PlaySound(stack.Item);

            if (autoEquip && settings.AutoEquip)
            {
                int equipSlot = GetEquipSlotFromItem(stack.Item, OnlyEmptySlots);
                bool disabledSlotsEmpty = true;

                for (int i = 0; i < items.Items[(int)stack.Item]!.DisableSlots.Count; ++i)
                {
                    for (int j = 0; j < _maxEquipped; ++j)
                    {
                        if (!Inventory[Equipment].Storage![j].Empty() && SlotType[j] == items.Items[(int)stack.Item]!.DisableSlots[i])
                        {
                            disabledSlotsEmpty = false;
                        }
                    }
                }

                if (equipSlot >= 0 && Inventory[Equipment].Slots[equipSlot]!.Enabled && disabledSlotsEmpty)
                {
                    area = Equipment;
                    slot = equipSlot;
                }
            }

            if (area == Carried)
            {
                ItemStack leftover = Inventory[Carried].Add(stack, slot);
                if (!leftover.Empty())
                {
                    if (items.Items[(int)stack.Item]!.QuestItem)
                    {
                        int maxQ = items.Items[(int)stack.Item]!.MaxQuantity;
                        int slotsToClear = 1;
                        if (maxQ > 0)
                            slotsToClear = leftover.Quantity + (leftover.Quantity % maxQ) / maxQ;

                        for (int i = _maxCarried - 1; i >= 0; --i)
                        {
                            if (items.Items[(int)Inventory[Carried].Storage![i].Item]!.QuestItem)
                                continue;

                            DropStack.Enqueue(Inventory[Carried].Storage[i].Clone());  // Clone: reference-type fix
                            Inventory[Carried].Storage[i].Clear();

                            slotsToClear--;
                            if (slotsToClear <= 0)
                                break;
                        }

                        if (slotsToClear > 0)
                        {
                            DropStack.Enqueue(leftover.Clone());
                        }
                        else
                        {
                            Add(leftover, Carried, slot, !AddPlaySound, !AddAutoEquip);
                        }
                    }
                    else
                    {
                        DropStack.Enqueue(leftover.Clone());
                    }
                    pc.LogMsg(SharedResources.Msg!.Get("Inventory is full."), Avatar.MsgNormal);
                    success = false;
                }
            }
            else if (area == Equipment)
            {
                ref ItemStack dest = ref Inventory[Equipment].Storage![slot];
                ItemStack leftover = new ItemStack();
                leftover.Item = stack.Item;

                if (!dest.Empty() && dest.Item != stack.Item)
                {
                    leftover.Quantity = stack.Quantity;
                }
                else if (dest.Quantity + stack.Quantity > items.Items[(int)stack.Item]!.MaxQuantity)
                {
                    leftover.Quantity = dest.Quantity + stack.Quantity - items.Items[(int)stack.Item]!.MaxQuantity;
                    stack.Quantity = items.Items[(int)stack.Item]!.MaxQuantity - dest.Quantity;
                    if (stack.Quantity > 0)
                    {
                        Add(stack, Equipment, slot, !AddPlaySound, !AddAutoEquip);
                    }
                }
                else
                {
                    Inventory[Equipment].Add(stack, slot);
                    UpdateEquipment(slot);
                    leftover.Clear();
                }

                if (!leftover.Empty())
                {
                    Add(leftover, Carried, ItemStorage.NoSlot, !AddPlaySound, !AddAutoEquip);
                }

                ApplyEquipment();
            }

            if (success && items.GetItemType(items.Items[(int)stack.Item]!.Type).AutoActionbar && items.Items[(int)stack.Item]!.Power > 0)
            {
                SharedGameResources.MenuAct!.AddPower(items.Items[(int)stack.Item]!.Power, 0);
            }

            DragPrevSrc = -1;

            return success;
        }

        public bool Remove(ItemID item, int quantity)
        {
            if (_activatedItem != 0 && _activatedSlot != -1 && item == _activatedItem)
            {
                Inventory[Carried].Subtract(_activatedSlot, 1);
                _activatedItem = 0;
                _activatedSlot = -1;
            }
            else if (!Inventory[Carried].Remove(item, quantity))
            {
                if (!Inventory[Equipment].Remove(item, quantity))
                {
                    return false;
                }
                else
                {
                    ApplyEquipment();
                }
            }

            return true;
        }

        public void RemoveFromPrevSlot(int quantity)
        {
            if (DragPrevSrc > -1 && Inventory[DragPrevSrc].DragPrevSlot > -1)
            {
                int dragPrevSlot = Inventory[DragPrevSrc].DragPrevSlot;
                Inventory[DragPrevSrc].Subtract(dragPrevSlot, quantity);
                if (Inventory[DragPrevSrc].Storage![dragPrevSlot].Empty())
                {
                    if (DragPrevSrc == Equipment)
                        UpdateEquipment(Inventory[Equipment].DragPrevSlot);
                }
            }
        }

        public void AddCurrency(int count)
        {
            if (count > 0)
            {
                ItemStack stack = new ItemStack();
                stack.Item = SharedResources.Eset!.Misc.CurrencyId;
                stack.Quantity = count;
                Add(stack, Carried, ItemStorage.NoSlot, !AddPlaySound, !AddAutoEquip);
            }
        }

        public void RemoveCurrency(int count)
        {
            Inventory[Carried].Remove(SharedResources.Eset!.Misc.CurrencyId, count);
        }

        public bool Buy(ItemStack stack, int tab, bool dragging)
        {
            // Clone: C# ItemStack is a class. Without clone, mutations to stack
            // (CanBuyback, Add->Quantity) would propagate to the source (e.g. vendor storage).
            stack = stack.Clone();

            if (stack.Empty())
            {
                return true;
            }

            ItemManager items = SharedGameResources.Items!;
            Avatar pc = SharedGameResources.Pc!;
            MessageEngine msg = SharedResources.Msg!;
            InputState inpt = SharedResources.Inpt!;
            EngineSettings eset = SharedResources.Eset!;

            if (!items.IsValid(stack.Item))
                return false;

            Item item = items.Items[(int)stack.Item]!;

            bool canAfford = false;
            int count = 0;

            if (tab == ItemManager.VendorCraft)
            {
                count = item.GetCraftCount();
                canAfford = (count > 0);
            }
            else
            {
                int valueEach = 0;
                if (tab == ItemManager.VendorBuy)
                    valueEach = item.GetPrice(ItemManager.UseVendorRatio);
                else if (tab == ItemManager.VendorSell)
                    valueEach = item.GetSellPrice(stack.CanBuyback);

                count = valueEach * stack.Quantity;
                canAfford = (Inventory[Carried].Count(eset.Misc.CurrencyId) >= count);
            }

            if (canAfford)
            {
                stack.CanBuyback = false;

                if (dragging)
                {
                    Drop(inpt.Mouse, stack);
                }
                else
                {
                    Add(stack, Carried, ItemStorage.NoSlot, AddPlaySound, AddAutoEquip);
                }

                if (tab == ItemManager.VendorCraft)
                {
                    for (int i = 0; i < item.CraftingItems.Count; ++i)
                    {
                        Remove(item.CraftingItems[i].Item, item.CraftingItems[i].Quantity * stack.Quantity);
                    }
                }
                else
                {
                    RemoveCurrency(count);
                    items.PlaySound(eset.Misc.CurrencyId);
                }

                return true;
            }
            else
            {
                if (tab == ItemManager.VendorCraft)
                    pc.LogMsg(msg.Get("You do not have the required items to craft."), Avatar.MsgNormal);
                else
                    pc.LogMsg(msg.GetV("Not enough %s.", eset.Loot.Currency), Avatar.MsgNormal);

                DropStack.Enqueue(stack);
                return false;
            }
        }

        public bool Sell(ItemStack stack)
        {
            ItemManager items = SharedGameResources.Items!;
            Avatar pc = SharedGameResources.Pc!;
            MessageEngine msg = SharedResources.Msg!;
            EngineSettings eset = SharedResources.Eset!;

            if (stack.Empty() || !items.IsValid(stack.Item))
            {
                return false;
            }

            if (stack.Item == eset.Misc.CurrencyId) return false;

            if (items.Items[(int)stack.Item]!.GetPrice(ItemManager.UseVendorRatio) == 0)
            {
                items.PlaySound(stack.Item);
                pc.LogMsg(msg.Get("This item can not be sold."), Avatar.MsgNormal);
                return false;
            }

            if (items.Items[(int)stack.Item]!.QuestItem)
            {
                items.PlaySound(stack.Item);
                pc.LogMsg(msg.Get("This item can not be sold."), Avatar.MsgNormal);
                return false;
            }

            int valueEach = items.Items[(int)stack.Item]!.GetSellPrice(ItemManager.DefaultSellPrice);
            int value = valueEach * stack.Quantity;
            AddCurrency(value);
            items.PlaySound(eset.Misc.CurrencyId);
            DragPrevSrc = -1;
            return true;
        }

        private void UpdateEquipment(int slot)
        {
            if (slot == -1)
            {
                return;
            }
            else
            {
                ChangedEquipment = true;
            }
        }

        public void ApplyEquipment()
        {
            ItemManager items = SharedGameResources.Items!;
            Avatar pc = SharedGameResources.Pc!;
            PowerManager powers = SharedGameResources.Powers!;
            MenuManager menu = SharedGameResources.Menu!;
            EngineSettings eset = SharedResources.Eset!;

            if (items.Items.Count == 0)
                return;

            ItemID itemId;
            List<ItemSetID> activeSets = new List<ItemSetID>();
            List<int> activeSetQuantities = new List<int>();

            bool checkRequired = true;
            while (checkRequired)
            {
                checkRequired = false;
                activeSets.Clear();
                activeSetQuantities.Clear();

                for (int j = 0; j < eset.PrimaryStats.Stats.Count; ++j)
                {
                    pc.Stats.PrimaryAdditional[j] = 0;
                }

                for (int i = 0; i < _maxEquipped; i++)
                {
                    if (IsEquipSlotActive(i))
                    {
                        itemId = Inventory[Equipment].Storage![i].Item;
                        if (!items.IsValid(itemId))
                            continue;

                        Item item = items.Items[(int)itemId]!;
                        int bonusCounter = 0;
                        while (bonusCounter < item.Bonus.Count)
                        {
                            for (int j = 0; j < eset.PrimaryStats.Stats.Count; ++j)
                            {
                                if (item.Bonus[bonusCounter].Type == BonusData.PrimaryStat && item.Bonus[bonusCounter].Index == j)
                                    pc.Stats.PrimaryAdditional[j] += (int)item.Bonus[bonusCounter].Value.Get();
                            }

                            bonusCounter++;
                        }
                    }
                }

                for (int i = 0; i < _maxEquipped; i++)
                {
                    ItemStack stack = Inventory[Equipment].Storage![i];

                    if (items.IsValid(stack.Item) && IsEquipSlotActive(i) && items.Items[(int)stack.Item]!.Set > 0)
                    {
                        int setIndex = activeSets.IndexOf(items.Items[(int)stack.Item]!.Set);
                        if (setIndex != -1)
                        {
                            activeSetQuantities[setIndex] += 1;
                        }
                        else
                        {
                            activeSets.Add(items.Items[(int)stack.Item]!.Set);
                            activeSetQuantities.Add(1);
                        }
                    }
                }

                for (int k = 0; k < activeSets.Count; ++k)
                {
                    if (!items.IsValidSet(activeSets[k]))
                        continue;

                    ItemSet itemSet = items.ItemSets[(int)activeSets[k]]!;
                    for (int bonusCounter = 0; bonusCounter < itemSet.Bonus.Count; ++bonusCounter)
                    {
                        if (itemSet.Bonus[bonusCounter].Requirement != activeSetQuantities[k])
                            continue;

                        for (int j = 0; j < eset.PrimaryStats.Stats.Count; ++j)
                        {
                            if (itemSet.Bonus[bonusCounter].Type == BonusData.PrimaryStat && itemSet.Bonus[bonusCounter].Index == j)
                                pc.Stats.PrimaryAdditional[j] += (int)itemSet.Bonus[bonusCounter].Value.Get();
                        }
                    }
                }

                for (int i = 0; i < _maxEquipped; i++)
                {
                    ItemStack stack = Inventory[Equipment].Storage![i];

                    if (items.IsValid(stack.Item))
                    {
                        if ((IsEquipSlotActive(i) && !items.RequirementsMet(pc.Stats, stack.Item)) || (!stack.Empty() && SlotType[i] != items.Items[(int)stack.Item]!.Type))
                        {
                            Add(stack, Carried, ItemStorage.NoSlot, AddPlaySound, !AddAutoEquip);
                            stack.Clear();
                            checkRequired = true;
                        }
                    }
                }
            }

            for (int i = 0; i < pc.Stats.PowersListItems.Count; ++i)
            {
                PowerID id = pc.Stats.PowersListItems[i];
                if (powers.Powers[(int)id]!.Passive && pc.Stats.Hp > 0 && !powers.Powers[(int)id]!.PassiveEffectsPersist)
                {
                    pc.Stats.Effects.RemoveEffectPassive(id);
                }
            }
            pc.Stats.PowersListItems.Clear();

            pc.Stats.EquipFlags.Clear();

            pc.Stats.Effects.ClearItemEffects();

            menu.Pow!.ClearBonusLevels();

            ApplyItemStats();
            ApplyItemSetBonuses(activeSets, activeSetQuantities);

            for (int i = 0; i < _maxEquipped; ++i)
            {
                Inventory[Equipment].Slots[i]!.Enabled = true;
            }
            for (int i = 0; i < _maxEquipped; ++i)
            {
                itemId = Inventory[Equipment][i].Item;

                if (items.IsValid(itemId) && IsEquipSlotActive(i))
                {
                    for (int j = 0; j < items.Items[(int)itemId]!.DisableSlots.Count; ++j)
                    {
                        DisableEquipmentSlot(items.Items[(int)itemId]!.DisableSlots[j]);
                    }
                }
            }

            for (int i = 0; i < pc.Stats.PowersPassive.Count; ++i)
            {
                PowerID id = pc.Stats.PowersPassive[i];
                if (!powers.Powers[(int)id]!.Passive)
                    continue;

                for (int j = 0; j < powers.Powers[(int)id]!.DisableEquipSlots.Count; ++j)
                {
                    DisableEquipmentSlot(powers.Powers[(int)id]!.DisableEquipSlots[j]);
                }
            }
            for (int i = 0; i < pc.Stats.PowersListItems.Count; ++i)
            {
                PowerID id = pc.Stats.PowersListItems[i];
                if (!powers.Powers[(int)id]!.Passive)
                    continue;

                for (int j = 0; j < powers.Powers[(int)id]!.DisableEquipSlots.Count; ++j)
                {
                    DisableEquipmentSlot(powers.Powers[(int)id]!.DisableEquipSlots[j]);
                }
            }

            pc.Stats.RefreshStats = true;

            if (_preview != null)
                _preview.LoadGraphicsFromInventory(this);

            if (pc.Stats.CurState == StatBlock.EntityPower)
            {
                pc.Stats.CurState = StatBlock.EntityStance;
            }
        }

        public void ApplyItemStats()
        {
            ItemManager items = SharedGameResources.Items!;
            Avatar pc = SharedGameResources.Pc!;
            PowerManager powers = SharedGameResources.Powers!;
            EngineSettings eset = SharedResources.Eset!;

            if (items.Items.Count == 0)
                return;

            for (int i = 0; i < eset.DamageTypes.Types.Count; ++i)
            {
                pc.Stats.ItemBaseDmg[i].Min = pc.Stats.ItemBaseDmg[i].Max = 0;
            }
            pc.Stats.ItemBaseAbs.Min = pc.Stats.ItemBaseAbs.Max = 0;

            for (int i = 0; i < _maxEquipped; i++)
            {
                if (IsEquipSlotActive(i))
                {
                    ItemID itemId = Inventory[Equipment].Storage![i].Item;
                    if (!items.IsValid(itemId))
                        continue;

                    Item item = items.Items[(int)itemId]!;

                    for (int j = 0; j < eset.DamageTypes.Types.Count; ++j)
                    {
                        pc.Stats.ItemBaseDmg[j].Min += item.BaseDmg[j].Min.Get();
                        pc.Stats.ItemBaseDmg[j].Max += item.BaseDmg[j].Max.Get();
                    }

                    for (int j = 0; j < item.EquipFlags.Count; ++j)
                    {
                        pc.Stats.EquipFlags.Add(item.EquipFlags[j]);
                    }

                    pc.Stats.ItemBaseAbs.Min += item.BaseAbs.Min.Get();
                    pc.Stats.ItemBaseAbs.Max += item.BaseAbs.Max.Get();

                    int bonusCounter = 0;
                    while (bonusCounter < item.Bonus.Count)
                    {
                        ApplyBonus(item.Bonus[bonusCounter]);
                        bonusCounter++;
                    }

                    if (item.Power > 0)
                    {
                        pc.Stats.PowersListItems.Add(item.Power);
                        if (pc.Stats.Effects.TriggeredOthers)
                            powers.ActivateSinglePassive(pc.Stats, item.Power);
                    }
                }
            }
        }

        public void ApplyItemSetBonuses(List<ItemSetID> activeSets, List<int> activeSetQuantities)
        {
            ItemManager items = SharedGameResources.Items!;

            for (int i = 0; i < activeSets.Count; ++i)
            {
                if (!items.IsValidSet(activeSets[i]))
                    continue;

                ItemSet itemSet = items.ItemSets[(int)activeSets[i]]!;

                for (int j = 0; j < itemSet.Bonus.Count; ++j)
                {
                    if (itemSet.Bonus[j].Requirement > activeSetQuantities[i])
                        continue;

                    ApplyBonus(itemSet.Bonus[j]);
                }
            }
        }

        public void ApplyBonus(BonusData bdata)
        {
            Avatar pc = SharedGameResources.Pc!;
            MenuManager menu = SharedGameResources.Menu!;
            EngineSettings eset = SharedResources.Eset!;

            EffectDef ed = new EffectDef();

            if (bdata.Type == BonusData.Speed)
            {
                ed.Id = "speed";
            }
            else if (bdata.Type == BonusData.AttackSpeed)
            {
                ed.Id = "attack_speed";
            }
            else if (bdata.Type == BonusData.Stat)
            {
                ed.Id = Stats.Key[bdata.Index];
            }
            else if (bdata.Type == BonusData.DamageMin)
            {
                ed.Id = eset.DamageTypes.Types[bdata.Index].Min;
            }
            else if (bdata.Type == BonusData.DamageMax)
            {
                ed.Id = eset.DamageTypes.Types[bdata.Index].Max;
            }
            else if (bdata.Type == BonusData.ResistElement)
            {
                ed.Id = eset.DamageTypes.Types[bdata.Index].Resist;
            }
            else if (bdata.Type == BonusData.PrimaryStat)
            {
                ed.Id = eset.PrimaryStats.Stats[bdata.Index].Id;
            }
            else if (bdata.PowerId > 0)
            {
                menu.Pow!.AddBonusLevels(bdata.PowerId, (int)bdata.Value.Get());
                return;
            }
            else if (bdata.Type == BonusData.ResourceStat)
            {
                ed.Id = eset.ResourceStats.Stats[bdata.Index].Ids[bdata.SubIndex];
            }

            ed.Type = Effect.GetTypeFromString(ed.Id);

            EffectParams ep = new EffectParams();
            ep.Magnitude = bdata.Value.Get();
            ep.IsMultiplier = bdata.IsMultiplier;
            ep.SourceType = Power.SourceTypeHero;
            ep.IsFromItem = true;

            pc.Stats.Effects.AddEffect(pc.Stats, ed, ep);
        }

        public void ApplyEquipmentSet(uint set)
        {
            uint prevEquipmentSet = ActiveEquipmentSet;

            if (set > 0 && set <= MaxEquipmentSet)
            {
                ActiveEquipmentSet = set;
                UpdateEquipmentSetWidgets();
            }

            if (ActiveEquipmentSet > 0 && prevEquipmentSet != ActiveEquipmentSet)
            {
                MenuManager menu = SharedGameResources.Menu!;
                if (!Visible && menu != null && menu.Hudlog != null)
                {
                    menu.Hudlog.Add(SharedResources.Msg!.GetV("Equipped set %d.", (int)ActiveEquipmentSet), MenuHUDLog.MsgNormal);
                }
            }
        }

        public void ApplyNextEquipmentSet()
        {
            if (ActiveEquipmentSet < MaxEquipmentSet)
            {
                ApplyEquipmentSet(ActiveEquipmentSet + 1);
            }
            else
            {
                ApplyEquipmentSet(1);
            }
        }

        public void ApplyPreviousEquipmentSet()
        {
            if (ActiveEquipmentSet > 1)
            {
                ApplyEquipmentSet(ActiveEquipmentSet - 1);
            }
            else
            {
                ApplyEquipmentSet(MaxEquipmentSet);
            }
        }

        private void UpdateEquipmentSetWidgets()
        {
            Widget? firstActiveSlot = null;
            Widget? currentTablistWidget = Tablist.GetWidgetByIndex(Tablist.GetCurrent());
            bool resetTablistCursor = false;

            for (int i = 0; i < _maxEquipped; i++)
            {
                if (IsEquipSlotActive(i))
                {
                    if (firstActiveSlot == null)
                    {
                        firstActiveSlot = Inventory[Equipment].Slots[i];
                    }
                    Inventory[Equipment].Slots[i]!.Visible = true;
                    Inventory[Equipment].Slots[i]!.EnableTablistNav = true;
                }
                else
                {
                    Inventory[Equipment].Slots[i]!.Visible = false;
                    Inventory[Equipment].Slots[i]!.EnableTablistNav = false;
                    if (currentTablistWidget != null && Inventory[Equipment].Slots[i] == currentTablistWidget)
                    {
                        resetTablistCursor = true;
                    }
                }
            }

            if (_equipmentSetButton.Count > 0)
            {
                for (int i = 0; i < _equipmentSetButton.Count; i++)
                {
                    if (ActiveEquipmentSet > 0)
                    {
                        if (i == ActiveEquipmentSet - 1)
                        {
                            _equipmentSetButton[i]!.Enabled = false;
                            if (currentTablistWidget != null && _equipmentSetButton[i] == currentTablistWidget)
                            {
                                resetTablistCursor = true;
                            }
                        }
                        else
                        {
                            _equipmentSetButton[i]!.Enabled = true;
                        }
                    }
                }
            }

            if (resetTablistCursor && firstActiveSlot != null)
            {
                Tablist.SetCurrent(firstActiveSlot);
            }

            if (_equipmentSetLabel != null)
            {
                string label = ActiveEquipmentSet + "/" + MaxEquipmentSet;
                _equipmentSetLabel.SetText(label);
                _equipmentSetLabel.SetColor(SharedResources.Font!.GetColor(FontEngine.ColorMenuNormal));
            }

            ChangedEquipment = true;
        }

        public bool IsEquipSlotActive(int equipped)
        {
            if (EquipmentSet[equipped] == 0 || EquipmentSet[equipped] == ActiveEquipmentSet)
            {
                return true;
            }

            return false;
        }

        public int GetEquippedCount()
        {
            return EquippedArea.Count;
        }

        public int GetTotalSlotCount()
        {
            return _maxCarried + _maxEquipped;
        }

        public void ClearHighlight()
        {
            Inventory[Equipment].HighlightClear();
            Inventory[Carried].HighlightClear();
        }

        public void FillEquipmentSlots()
        {
            ItemManager items = SharedGameResources.Items!;

            ItemStack[] equipStack = new ItemStack[_maxEquipped];

            for (int i = 0; i < _maxEquipped; ++i)
            {
                // Clone() needed: C# ItemStack is a class (reference type).
                // C++ equivalent is value copy; without Clone(), Clear()
                // below would also empty equipStack[i] via shared reference.
                equipStack[i] = Inventory[Equipment].Storage![i].Clone();
                if (equipStack[i].Item > 0)
                    equipStack[i].Quantity = Math.Max(1, equipStack[i].Quantity);
                else
                    equipStack[i].Clear();

                Inventory[Equipment].Storage![i].Clear();

                if (!equipStack[i].Empty() && Inventory[Equipment].Storage![i].Empty() && items.IsValid(equipStack[i].Item) && items.Items[(int)equipStack[i].Item]!.Type == SlotType[i])
                {
                    Inventory[Equipment].Storage![i] = equipStack[i].Clone();
                    equipStack[i].Clear();
                }
            }

            for (int i = 0; i < _maxEquipped; ++i)
            {
                if (equipStack[i].Empty() || !items.IsValid(equipStack[i].Item))
                    continue;

                bool foundSlot = false;
                for (int j = 0; j < _maxEquipped; ++j)
                {
                    if (Inventory[Equipment].Storage![j].Empty())
                    {
                        if (items.Items[(int)equipStack[i].Item]!.Type == SlotType[j])
                        {
                            Inventory[Equipment].Storage[j] = equipStack[i];
                            foundSlot = true;
                            break;
                        }
                    }
                }

                if (!foundSlot)
                {
                    Add(equipStack[i], Carried, ItemStorage.NoSlot, !AddPlaySound, !AddAutoEquip);
                }
            }
        }

        public int GetMaxPurchasable(ItemStack item, int vendorTab)
        {
            ItemManager items = SharedGameResources.Items!;

            if (!items.IsValid(item.Item))
                return 0;

            if (vendorTab == ItemManager.VendorBuy)
                return Currency / items.Items[(int)item.Item]!.GetPrice(ItemManager.UseVendorRatio);
            else if (vendorTab == ItemManager.VendorSell)
                return Currency / items.Items[(int)item.Item]!.GetSellPrice(item.CanBuyback);
            else if (vendorTab == ItemManager.VendorCraft)
                return items.Items[(int)item.Item]!.GetCraftCount();
            else
                return 0;
        }

        public int GetEquipSlotFromItem(ItemID item, bool onlyEmptySlots)
        {
            ItemManager items = SharedGameResources.Items!;
            Avatar pc = SharedGameResources.Pc!;

            if (!items.IsValid(item) || !items.RequirementsMet(pc.Stats, item))
                return -2;

            int equipSlot = -1;

            for (int i = 0; i < _maxEquipped; i++)
            {
                if (!IsEquipSlotActive(i))
                    continue;

                if (SlotType[i] == items.Items[(int)item]!.Type)
                {
                    if (Inventory[Equipment].Storage![i].Empty())
                    {
                        equipSlot = i;
                        break;
                    }
                    else if (!onlyEmptySlots && equipSlot == -1)
                    {
                        equipSlot = i;
                    }
                }
            }

            return equipSlot;
        }

        public PowerID GetPowerMod(PowerID metaPower)
        {
            ItemManager items = SharedGameResources.Items!;

            for (int i = 0; i < Inventory[Equipment].GetSlotNumber(); ++i)
            {
                if (!IsEquipSlotActive(i))
                    continue;

                ItemID id = Inventory[Equipment][i].Item;
                if (!items.IsValid(id))
                    continue;

                for (int j = 0; j < items.Items[(int)id]!.ReplacePower.Count; j++)
                {
                    if (items.Items[(int)id]!.ReplacePower[j].Item1 == metaPower && items.Items[(int)id]!.ReplacePower[j].Item2 != metaPower)
                    {
                        return items.Items[(int)id]!.ReplacePower[j].Item2;
                    }
                }
            }

            return 0;
        }

        public void DisableEquipmentSlot(int disableSlotType)
        {
            for (int i = 0; i < _maxEquipped; ++i)
            {
                if (IsEquipSlotActive(i) && SlotType[i] == disableSlotType)
                {
                    if (!Inventory[Equipment].Storage![i].Empty())
                    {
                        Add(Inventory[Equipment].Storage[i], Carried, ItemStorage.NoSlot, AddPlaySound, !AddAutoEquip);
                        Inventory[Equipment].Storage[i].Clear();
                        UpdateEquipment(i);
                        ApplyEquipment();
                    }
                    Inventory[Equipment].Slots[i]!.Enabled = false;
                }
            }
        }

        public bool CanActivateItem(ItemID item)
        {
            ItemManager items = SharedGameResources.Items!;

            if (!items.IsValid(item))
                return false;

            if (items.Items[(int)item]!.Script != "")
                return true;
            if (items.Items[(int)item]!.Book != "")
                return true;
            if (items.Items[(int)item]!.Power > 0 && GetEquipSlotFromItem(item, !OnlyEmptySlots) == -1)
                return true;

            return false;
        }

        public int GetEquippedSetCount(int setId)
        {
            ItemManager items = SharedGameResources.Items!;
            int quantity = 0;
            for (int i = 0; i < _maxEquipped; i++)
            {
                ItemID itemId = Inventory[Equipment].Storage![i].Item;
                if (items.IsValid(itemId) && IsEquipSlotActive(i))
                {
                    if (items.Items[(int)itemId]!.Set == setId)
                    {
                        quantity++;
                    }
                }
            }
            return quantity;
        }

        public bool CanEquipItem(Int2 position)
        {
            ItemManager items = SharedGameResources.Items!;
            Avatar pc = SharedGameResources.Pc!;

            int slot = Inventory[Carried].SlotOver(position);
            if (slot == -1)
                return false;

            ItemID itemId = Inventory[Carried][slot].Item;

            if (Inventory[Carried][slot].Empty() || !items.IsValid(itemId))
                return false;

            return (pc.Stats.Humanoid && items.GetItemType(items.Items[(int)itemId]!.Type).Name != "" && GetEquipSlotFromItem(itemId, !OnlyEmptySlots) >= 0);
        }

        public bool CanUseItem(Int2 position)
        {
            int slot = Inventory[Carried].SlotOver(position);
            if (slot == -1)
                return false;

            if (Inventory[Carried][slot].Empty())
                return false;

            ItemID itemId = Inventory[Carried][slot].Item;

            return CanActivateItem(itemId);
        }

        public bool CanPlaceItemOnActionbar(Int2 position)
        {
            ItemManager items = SharedGameResources.Items!;
            Avatar pc = SharedGameResources.Pc!;

            int area = AreaOver(position);
            if (area == -1)
                return false;

            int slot = Inventory[area].SlotOver(position);
            if (slot == -1)
                return false;

            ItemID itemId = Inventory[area][slot].Item;

            if (Inventory[area][slot].Empty() || !items.IsValid(itemId))
                return false;

            return (pc.Stats.Humanoid && items.Items[(int)itemId]!.Power > 0);
        }

        public bool EquipmentContain(ItemID item, int quantity)
        {
            int totalQuantity = 0;
            for (int i = 0; i < _maxEquipped; ++i)
            {
                if (!IsEquipSlotActive(i))
                    continue;

                if (Inventory[Equipment][i].Item == item)
                    totalQuantity += Inventory[Equipment][i].Quantity;

                if (totalQuantity >= quantity)
                    return true;
            }
            return false;
        }
    }
}
