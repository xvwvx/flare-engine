// 对应 C++ 源文件：MenuPowers.h + MenuPowers.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using System.Text;
using Stride.Core.Mathematics;

namespace FlareEngine
{
    public class MenuPowersTab
    {
        public string Title = "";
        public string Background = "";
        public bool BackgroundIsMenuSize = true;

        public MenuPowersTab()
        {
            Title = "";
            Background = "";
            BackgroundIsMenuSize = true;
        }
    }

    public class MenuPowersCell
    {
        public PowerID Id;
        public bool RequiresPoint;

        public int RequiresLevel;
        public List<int> RequiresPrimary;
        public List<PowerID> RequiresPower;
        public List<StatusID> RequiresStatus;
        public List<StatusID> RequiresNotStatus;

        public bool Visible;
        public bool VisibleCheckLocked;
        public bool VisibleCheckStatus;

        public int UpgradeLevel;
        public bool PassiveOn;
        public bool IsUnlocked;

        public int Group;
        public MenuPowersCell? Next;

        public MenuPowersCell()
        {
            Id = 0;
            RequiresPoint = false;
            RequiresLevel = 0;
            RequiresPrimary = new List<int>(new int[SharedResources.Eset!.PrimaryStats.Stats.Count]);
            RequiresPower = new List<PowerID>();
            RequiresStatus = new List<StatusID>();
            RequiresNotStatus = new List<StatusID>();
            Visible = true;
            VisibleCheckLocked = false;
            VisibleCheckStatus = false;
            UpgradeLevel = 0;
            PassiveOn = false;
            IsUnlocked = false;
            Group = 0;
            Next = null;
        }
    }

    public class MenuPowersCellGroup
    {
        public int Tab;
        public Int2 Pos;

        public int CurrentCell;
        public List<MenuPowersCell> Cells = new List<MenuPowersCell>();

        public WidgetButton? UpgradeButton;

        public List<(int First, int Second)> BonusLevels = new List<(int, int)>();

        public MenuPowersCellGroup()
        {
            Tab = 0;
            Pos = default;
            CurrentCell = 0;
            Cells = new List<MenuPowersCell>();
            UpgradeButton = null;
        }

        public MenuPowersCell GetCurrent()
        {
            return Cells[CurrentCell];
        }

        public MenuPowersCell GetBonusCurrent(MenuPowersCell pcell)
        {
            if (BonusLevels.Count == 0)
                return pcell;

            int current = CurrentCell;

            for (int i = 0; i < Cells.Count; ++i)
            {
                if (ReferenceEquals(pcell, Cells[i]))
                {
                    current = i;
                    break;
                }
            }

            int currentBonusLevels = GetBonusLevels();
            int bonusCell = current + currentBonusLevels;

            if (bonusCell >= Cells.Count)
                return Cells[Cells.Count - 1];

            return Cells[bonusCell];
        }

        public int GetBonusLevels()
        {
            int blevel = 0;
            for (int i = 0; i < BonusLevels.Count; ++i)
            {
                if (CurrentCell >= BonusLevels[i].First)
                    blevel += BonusLevels[i].Second;
            }
            return blevel;
        }
    }

    public class MenuPowersClick
    {
        public PowerID Drag;
        public PowerID Unlock;

        public MenuPowersClick()
        {
            Drag = 0;
            Unlock = 0;
        }
    }

    /// <summary>
    /// MenuPowers — 技能树菜单，负责解析 power tree 配置、解锁/升级逻辑、工具提示与渲染。
    /// </summary>
    public class MenuPowers : Menu
    {
        private const bool UpgradePowerAllTabs = true;
        private const bool TooltipShowActivateHint = true;

        public const int TooltipShort = 0;
        public const int TooltipLongMenu = 1;
        public const int TooltipLongAll = 2;

        public List<WidgetSlot?> Slots = new List<WidgetSlot?>();
        public bool NewPowerNotification;
        public List<TabList> TablistPow = new List<TabList>();

        private readonly List<MenuPowersCellGroup> _powerCell = new List<MenuPowersCellGroup>();
        private bool _skipSection;

        private readonly List<Sprite?> _treeSurf = new List<Sprite?>();
        private WidgetButton? _closeButton;

        private Int2 _closePos;
        private Rectangle _tabArea;

        private int _pointsLeft;
        private readonly List<MenuPowersTab> _tabs = new List<MenuPowersTab>();
        private string _defaultBackground = "";

        private readonly WidgetLabel _labelPowers = new WidgetLabel();
        private readonly WidgetLabel _labelUnspent = new WidgetLabel();
        private WidgetTabControl? _tabControl;

        private bool _treeLoaded;

        private int _defaultPowerTab;

        private Int2 _upgradeButtonOffset;

        private readonly List<MenuPowersCell?> _recentlyLockedCells = new List<MenuPowersCell?>();

        private string _tooltipTextShield;
        private string _tooltipTextHeal;

        public MenuPowers()
        {
            _skipSection = false;
            _pointsLeft = 0;
            _defaultBackground = "";
            _tabControl = null;
            _treeLoaded = false;
            _defaultPowerTab = -1;
            _upgradeButtonOffset = new Int2(SharedResources.Eset!.Resolutions.IconSize, 0);
            _tooltipTextShield = SharedResources.Msg!.Get("Magical Shield");
            _tooltipTextHeal = SharedResources.Msg.Get("Healing");
            NewPowerNotification = false;

            _closeButton = new WidgetButton(WidgetButton.CloseFile);

            FileParser infile = new FileParser();
            // @CLASS MenuPowers: Menu layout|Description of menus/powers.txt
            if (infile.Open("menus/powers.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (ParseMenuKey(infile.Key, infile.Val))
                        continue;

                    // @ATTR label_title|label|Position of the "Powers" text.
                    if (infile.Key == "label_title")
                        _labelPowers.SetFromLabelInfo(Parse.PopLabelInfo(infile.Val));

                    // @ATTR unspent_points|label|Position of the text that displays the amount of unused power points.
                    else if (infile.Key == "unspent_points")
                        _labelUnspent.SetFromLabelInfo(Parse.PopLabelInfo(infile.Val));

                    // @ATTR close|point|Position of the close button.
                    else if (infile.Key == "close")
                        _closePos = Parse.ToPoint(infile.Val);

                    // @ATTR tab_area|rectangle|Position and dimensions of the tree pages.
                    else if (infile.Key == "tab_area")
                        _tabArea = Parse.ToRect(infile.Val);

                    // @ATTR upgrade_button_offset|point|X/Y offset of the upgrade button relative to each power icon. Defaults to (ICON_SIZE, 0)
                    else if (infile.Key == "upgrade_button_offset")
                        _upgradeButtonOffset = Parse.ToPoint(infile.Val);

                    // @ATTR tooltip_text_shield|string|Text to use in place of "Magical Shield".
                    else if (infile.Key == "tooltip_text_shield")
                        _tooltipTextShield = SharedResources.Msg.Get(infile.Val);

                    // @ATTR tooltip_text_heal|string|Text to use in place of "Healing".
                    else if (infile.Key == "tooltip_text_heal")
                        _tooltipTextHeal = SharedResources.Msg.Get(infile.Val);

                    else infile.Error("MenuPowers: '%s' is not a valid key.", infile.Key);
                }
                infile.Close();
            }

            _labelPowers.SetText(SharedResources.Msg.Get("Powers"));
            _labelPowers.SetColor(SharedResources.Font!.GetColor(FontEngine.ColorMenuNormal));

            _labelUnspent.SetColor(SharedResources.Font.GetColor(FontEngine.ColorMenuBonus));

            LoadGraphics();

            SharedGameResources.MenuPowers = this;

            Align();
        }

        public override void Dispose()
        {
            for (int i = 0; i < _treeSurf.Count; i++)
            {
                if (_treeSurf[i] != null)
                {
                    _treeSurf[i]!.Dispose();
                    _treeSurf[i] = null;
                }
            }
            for (int i = 0; i < Slots.Count; i++)
            {
                Slots[i]?.Dispose();
            }
            Slots.Clear();

            for (int i = 0; i < _powerCell.Count; i++)
            {
                _powerCell[i].UpgradeButton?.Dispose();
                _powerCell[i].UpgradeButton = null;
            }

            _closeButton?.Dispose();
            _closeButton = null;
            _tabControl?.Dispose();
            _tabControl = null;
            SharedGameResources.MenuPowers = null;

            _labelPowers.Dispose();
            _labelUnspent.Dispose();

            base.Dispose();
        }

        public override void Align()
        {
            base.Align();

            _labelPowers.SetPos(WindowArea.X, WindowArea.Y);
            _labelUnspent.SetPos(WindowArea.X, WindowArea.Y);

            _closeButton!.Pos.X = WindowArea.X + _closePos.X;
            _closeButton.Pos.Y = WindowArea.Y + _closePos.Y;

            if (_tabControl != null)
            {
                _tabControl.SetMainArea(WindowArea.X + _tabArea.X, WindowArea.Y + _tabArea.Y, _tabArea.Width);
            }

            for (int i = 0; i < Slots.Count; i++)
            {
                if (Slots[i] == null) continue;

                Slots[i]!.SetPos(WindowArea.X, WindowArea.Y);

                if (_powerCell[i].UpgradeButton != null)
                {
                    _powerCell[i].UpgradeButton!.SetPos(WindowArea.X, WindowArea.Y);
                }
            }
        }

        private void LoadGraphics()
        {
            if (_background == null)
                SetBackground("images/menus/powers.png");
        }

        public void LoadPowerTree(string filename)
        {
            if (_treeLoaded) return;

            List<MenuPowersCell> powerCellUpgrade = new List<MenuPowersCell>();

            FileParser infile = new FileParser();
            // @CLASS MenuPowers: Power tree layout|Description of powers/trees/
            if (infile.Open(filename, FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (infile.NewSection)
                    {
                        if (infile.Section == "power")
                        {
                            MenuPowersCellGroup cellGroup = new MenuPowersCellGroup();
                            MenuPowersCell baseLevel = new MenuPowersCell();

                            baseLevel.Group = _powerCell.Count;
                            cellGroup.Cells.Add(baseLevel);

                            _powerCell.Add(cellGroup);

                            Slots.Add(null);
                        }
                        else if (infile.Section == "upgrade")
                            powerCellUpgrade.Add(new MenuPowersCell());
                        else if (infile.Section == "tab")
                            _tabs.Add(new MenuPowersTab());
                    }

                    if (infile.Section == "")
                    {
                        // @ATTR background|filename|Filename of the default background image
                        if (infile.Key == "background") _defaultBackground = infile.Val;
                    }
                    else if (infile.Section == "tab")
                        LoadTab(infile);
                    else if (infile.Section == "power")
                        LoadPower(infile);
                    else if (infile.Section == "upgrade")
                        LoadUpgrade(infile, powerCellUpgrade);
                }
                infile.Close();
            }

            for (int i = 0; i < _powerCell.Count; ++i)
            {
                for (int j = 1; j < _powerCell[i].Cells.Count; ++j)
                {
                    for (int k = 0; k < powerCellUpgrade.Count; ++k)
                    {
                        if (powerCellUpgrade[k].Id == _powerCell[i].Cells[j].Id)
                        {
                            _powerCell[i].Cells[j] = powerCellUpgrade[k];
                            _powerCell[i].Cells[j].UpgradeLevel = j + 1;
                            _powerCell[i].Cells[j].Group = i;
                            _powerCell[i].Cells[j - 1].Next = _powerCell[i].Cells[j];
                        }
                    }
                }
            }

            Image? graphics;
            if (_tabs.Count == 0 && _defaultBackground != "")
            {
                graphics = SharedResources.RenderDevice!.LoadImage(_defaultBackground, RenderDevice.ErrorNormal);
                if (graphics != null)
                {
                    _treeSurf.Add(graphics.CreateSprite());
                    graphics.Unref();
                }
            }
            else
            {
                for (int i = 0; i < _tabs.Count; ++i)
                {
                    if (_tabs[i].Background == "")
                        _tabs[i].Background = _defaultBackground;

                    if (_tabs[i].Background == "")
                    {
                        _treeSurf.Add(null);
                        continue;
                    }

                    graphics = SharedResources.RenderDevice!.LoadImage(_tabs[i].Background, RenderDevice.ErrorNormal);
                    if (graphics != null)
                    {
                        Sprite? treeSprite = graphics.CreateSprite();

                        if (_background != null && (_background.GetGraphicsWidth() != treeSprite!.GetGraphicsWidth() || _background.GetGraphicsHeight() != treeSprite.GetGraphicsHeight()))
                        {
                            _tabs[i].BackgroundIsMenuSize = false;
                        }

                        _treeSurf.Add(treeSprite);
                        graphics.Unref();
                    }
                    else
                    {
                        _treeSurf.Add(null);
                    }
                }
            }

            if (_tabs.Count > 0)
            {
                TablistPow = new List<TabList>(_tabs.Count);
                for (int ti = 0; ti < _tabs.Count; ++ti)
                    TablistPow.Add(new TabList());

                _tabControl = new WidgetTabControl();

                if (_tabControl != null)
                {
                    for (int i = 0; i < _tabs.Count; i++)
                    {
                        _tabControl.SetupTab((uint)i, SharedResources.Msg!.Get(_tabs[i].Title), TablistPow[i]);
                    }

                    _tabControl.SetMainArea(WindowArea.X + _tabArea.X, WindowArea.Y + _tabArea.Y, _tabArea.Width);
                }
            }

            for (int i = 0; i < Slots.Count; i++)
            {
                if (_powerCell[i].Cells.Count > 0 && SharedGameResources.Powers!.IsValid(_powerCell[i].Cells[0].Id))
                {
                    Slots[i] = new WidgetSlot(SharedGameResources.Powers.Powers[_powerCell[i].Cells[0].Id]!.Icon, WidgetSlot.HighlightPowerMenu);
                    Slots[i]!.SetBasePos(_powerCell[i].Pos.X, _powerCell[i].Pos.Y, Utils.AlignTopLeft);

                    if (TablistPow.Count > 0)
                    {
                        TablistPow[_powerCell[i].Tab].Add(Slots[i]!);
                        TablistPow[_powerCell[i].Tab].SetPrevTabList(Tablist);
                        TablistPow[_powerCell[i].Tab].Lock();
                    }
                    else
                    {
                        Tablist.Add(Slots[i]!);
                    }

                    if (_powerCell[i].UpgradeButton != null)
                    {
                        _powerCell[i].UpgradeButton!.SetBasePos(_powerCell[i].Pos.X + _upgradeButtonOffset.X, _powerCell[i].Pos.Y + _upgradeButtonOffset.Y, Utils.AlignTopLeft);
                    }
                }
            }

            SetUnlockedPowers();

            EngineSettings.HeroClassesSettings.HeroClass? pcClass;
            pcClass = SharedResources.Eset!.HeroClasses.GetByName(SharedGameResources.Pc!.Stats.CharacterClass);
            if (pcClass != null)
            {
                _defaultPowerTab = pcClass.DefaultPowerTab;
            }

            _treeLoaded = true;

            Align();
        }

        private void LoadTab(FileParser infile)
        {
            // @ATTR tab.title|string|The name of this power tree tab
            if (infile.Key == "title") _tabs[^1].Title = infile.Val;
            // @ATTR tab.background|filename|Filename of the background image for this tab's power tree
            else if (infile.Key == "background") _tabs[^1].Background = infile.Val;
        }

        private void LoadPower(FileParser infile)
        {
            MenuPowersCellGroup cellGroup = _powerCell[^1];

            if (cellGroup.Cells.Count == 0)
                return;

            // @ATTR power.id|power_id|A power id from powers/powers.txt for this slot.
            if (infile.Key == "id")
            {
                string val = infile.Val;
                PowerID id = SharedGameResources.Powers!.VerifyID(Parse.ToPowerID(Parse.PopFirstString(ref val)), infile, !PowerManager.AllowZeroId);
                if (id > 0)
                {
                    _skipSection = false;
                    cellGroup.Cells[0].Id = id;
                }
                return;
            }

            if (cellGroup.Cells[0].Id == 0)
            {
                _skipSection = true;
                _powerCell.RemoveAt(_powerCell.Count - 1);
                Slots.RemoveAt(Slots.Count - 1);
                infile.Error("MenuPowers: Power without ID as first attribute. Skipping section.");
            }

            if (_skipSection)
                return;

            if (infile.Key == "tab")
            {
                // @ATTR power.tab|int|Tab index to place this power on, starting from 0.
                cellGroup.Tab = Parse.ToInt(infile.Val);
            }
            else if (infile.Key == "position")
            {
                // @ATTR power.position|point|Position of this power icon; relative to MenuPowers "pos".
                cellGroup.Pos = Parse.ToPoint(infile.Val);
            }

            else if (infile.Key == "requires_point")
            {
                // @ATTR power.requires_point|bool|Power requires a power point to unlock.
                cellGroup.Cells[0].RequiresPoint = Parse.ToBool(infile.Val);
            }
            else if (infile.Key == "requires_primary")
            {
                // @ATTR power.requires_primary|predefined_string, int : Primary stat name, Required value|Power requires this primary stat to be at least the specificed value.
                string primVal = infile.Val;
                string primStat = Parse.PopFirstString(ref primVal);
                int primStatIndex = SharedResources.Eset!.PrimaryStats.GetIndexByID(primStat);

                if (primStatIndex != SharedResources.Eset.PrimaryStats.Stats.Count)
                {
                    cellGroup.Cells[0].RequiresPrimary[primStatIndex] = Parse.ToInt(primVal);
                }
                else
                {
                    infile.Error("MenuPowers: '%s' is not a valid primary stat.", primStat);
                }
            }
            else if (infile.Key == "requires_level")
            {
                // @ATTR power.requires_level|int|Power requires at least this level for the hero.
                cellGroup.Cells[0].RequiresLevel = Parse.ToInt(infile.Val);
            }
            else if (infile.Key == "requires_power")
            {
                // @ATTR power.requires_power|power_id|Power requires another power id.
                PowerID powerId = SharedGameResources.Powers!.VerifyID(Parse.ToPowerID(infile.Val), infile, !PowerManager.AllowZeroId);
                if (powerId != 0)
                    cellGroup.Cells[0].RequiresPower.Add(powerId);
            }
            else if (infile.Key == "requires_status")
            {
                // @ATTR power.requires_status|repeatable(string)|Power requires this campaign status.
                cellGroup.Cells[0].RequiresStatus.Add(SharedGameResources.Camp!.RegisterStatus(infile.Val));
            }
            else if (infile.Key == "requires_not_status")
            {
                // @ATTR power.requires_not_status|repeatable(string)|Power requires not having this campaign status.
                cellGroup.Cells[0].RequiresNotStatus.Add(SharedGameResources.Camp!.RegisterStatus(infile.Val));
            }
            else if (infile.Key == "visible_requires_status")
            {
                // @ATTR power.visible_requires_status|repeatable(string)|(Deprecated as of v1.11.75) Hide the power if we don't have this campaign status.
                infile.Error("MenuPowers: visible_requires_status is deprecated. Use requires_status and visible_check_status=true instead.");
                cellGroup.Cells[0].RequiresStatus.Add(SharedGameResources.Camp!.RegisterStatus(infile.Val));
                cellGroup.Cells[0].VisibleCheckStatus = true;
            }
            else if (infile.Key == "visible_requires_not_status")
            {
                // @ATTR power.visible_requires_not_status|repeatable(string)|(Deprecated as of v1.11.75) Hide the power if we have this campaign status.
                infile.Error("MenuPowers: visible_requires_not_status is deprecated. Use requires_not_status and visible_check_status=true instead.");
                cellGroup.Cells[0].RequiresNotStatus.Add(SharedGameResources.Camp!.RegisterStatus(infile.Val));
                cellGroup.Cells[0].VisibleCheckStatus = true;
            }
            else if (infile.Key == "upgrades")
            {
                // @ATTR power.upgrades|list(power_id)|A list of upgrade power ids that this power slot can upgrade to. Each of these powers should have a matching upgrade section.
                string upgradeVal = infile.Val;
                string repeatVal = Parse.PopFirstString(ref upgradeVal);
                while (repeatVal != "")
                {
                    PowerID testId = SharedGameResources.Powers!.VerifyID(Parse.ToPowerID(repeatVal), infile, !PowerManager.AllowZeroId);
                    if (testId != 0)
                    {
                        if (testId == cellGroup.Cells[0].Id)
                        {
                            infile.Error("MenuPowers: Upgrade ID '%d' is the same as the base ID. Ignoring.", testId);
                        }
                        else
                        {
                            MenuPowersCell upgradeCell = new MenuPowersCell();
                            upgradeCell.Id = testId;
                            cellGroup.Cells.Add(upgradeCell);
                        }
                    }
                    repeatVal = Parse.PopFirstString(ref upgradeVal);
                }

                if (cellGroup.Cells.Count > 1)
                {
                    cellGroup.Cells[0].UpgradeLevel = 1;
                    if (cellGroup.UpgradeButton == null)
                        cellGroup.UpgradeButton = new WidgetButton(WidgetButton.UpgradePowerFile);
                }
            }
            else if (infile.Key == "visible")
            {
                // @ATTR power.visible|bool|Controls whether or not a power is visible or hidden regardless of unlocked state. Defaults to true.
                cellGroup.Cells[0].Visible = Parse.ToBool(infile.Val);
            }
            else if (infile.Key == "visible_check_locked")
            {
                // @ATTR power.visible_check_locked|bool|When set to true, the power will be hidden if it is locked. Defaults to false.
                cellGroup.Cells[0].VisibleCheckLocked = Parse.ToBool(infile.Val);
            }
            else if (infile.Key == "visible_check_status")
            {
                // @ATTR power.visible_check_status|bool|When set to true, the power will be hidden if its status requirements are not met. Defaults to false.
                cellGroup.Cells[0].VisibleCheckStatus = Parse.ToBool(infile.Val);
            }
            else
            {
                infile.Error("MenuPowers: '%s' is not a valid key.", infile.Key);
            }
        }

        private void LoadUpgrade(FileParser infile, List<MenuPowersCell> powerCellUpgrade)
        {
            if (powerCellUpgrade.Count == 0)
                return;

            MenuPowersCell cell = powerCellUpgrade[^1];

            // @ATTR upgrade.id|power_id|A power id from powers/powers.txt for this upgrade.
            if (infile.Key == "id")
            {
                string val = infile.Val;
                PowerID id = SharedGameResources.Powers!.VerifyID(Parse.ToPowerID(Parse.PopFirstString(ref val)), infile, !PowerManager.AllowZeroId);
                if (id > 0)
                {
                    _skipSection = false;
                    cell.Id = id;
                }
                return;
            }

            if (cell.Id == 0)
            {
                _skipSection = true;
                powerCellUpgrade.RemoveAt(powerCellUpgrade.Count - 1);
                infile.Error("MenuPowers: Upgrade without ID as first attribute. Skipping section.");
            }

            if (_skipSection)
                return;

            // @ATTR upgrade.requires_primary|predefined_string, int : Primary stat name, Required value|Upgrade requires this primary stat to be at least the specificed value.
            if (infile.Key == "requires_primary")
            {
                string primVal = infile.Val;
                string primStat = Parse.PopFirstString(ref primVal);
                int primStatIndex = SharedResources.Eset!.PrimaryStats.GetIndexByID(primStat);

                if (primStatIndex != SharedResources.Eset.PrimaryStats.Stats.Count)
                {
                    cell.RequiresPrimary[primStatIndex] = Parse.ToInt(primVal);
                }
                else
                {
                    infile.Error("MenuPowers: '%s' is not a valid primary stat.", primStat);
                }
            }
            else if (infile.Key == "requires_point")
            {
                // @ATTR upgrade.requires_point|bool|Upgrade requires a power point to unlock.
                cell.RequiresPoint = Parse.ToBool(infile.Val);
            }
            else if (infile.Key == "requires_level")
            {
                // @ATTR upgrade.requires_level|int|Upgrade requires at least this level for the hero.
                cell.RequiresLevel = Parse.ToInt(infile.Val);
            }
            else if (infile.Key == "requires_power")
            {
                // @ATTR upgrade.requires_power|int|Upgrade requires another power id.
                PowerID powerId = SharedGameResources.Powers!.VerifyID(Parse.ToPowerID(infile.Val), infile, !PowerManager.AllowZeroId);
                if (powerId != 0)
                    cell.RequiresPower.Add(powerId);
            }
            else if (infile.Key == "requires_status")
            {
                // @ATTR upgrade.requires_status|repeatable(string)|Upgrade requires this campaign status.
                cell.RequiresStatus.Add(SharedGameResources.Camp!.RegisterStatus(infile.Val));
            }
            else if (infile.Key == "requires_not_status")
            {
                // @ATTR upgrade.requires_not_status|repeatable(string)|Upgrade requires not having this campaign status.
                cell.RequiresNotStatus.Add(SharedGameResources.Camp!.RegisterStatus(infile.Val));
            }
            else if (infile.Key == "visible_requires_status")
            {
                // @ATTR upgrade.visible_requires_status|repeatable(string)|(Deprecated as of v1.11.75) Hide the upgrade if we don't have this campaign status.
                infile.Error("MenuPowers: visible_requires_status is deprecated. Use requires_status and visible_check_status=true instead.");
                cell.RequiresStatus.Add(SharedGameResources.Camp!.RegisterStatus(infile.Val));
                cell.VisibleCheckStatus = true;
            }
            else if (infile.Key == "visible_requires_not_status")
            {
                // @ATTR upgrade.visible_requires_not_status|repeatable(string)|(Deprecated as of v1.11.75) Hide the upgrade if we have this campaign status.
                infile.Error("MenuPowers: visible_requires_not_status is deprecated. Use requires_not_status and visible_check_status=true instead.");
                cell.RequiresNotStatus.Add(SharedGameResources.Camp!.RegisterStatus(infile.Val));
                cell.VisibleCheckStatus = true;
            }
            else if (infile.Key == "visible")
            {
                // @ATTR upgrade.visible|bool|Controls whether or not a power is visible or hidden regardless of unlocked state. Defaults to true.
                cell.Visible = Parse.ToBool(infile.Val);
            }
            else if (infile.Key == "visible_check_locked")
            {
                // @ATTR upgrade.visible_check_locked|bool|When set to true, the power will be hidden if it is locked. Defaults to false.
                cell.VisibleCheckLocked = Parse.ToBool(infile.Val);
            }
            else if (infile.Key == "visible_check_status")
            {
                // @ATTR upgrade.visible_check_status|bool|When set to true, the power will be hidden if its status requirements are not met. Defaults to false.
                cell.VisibleCheckStatus = Parse.ToBool(infile.Val);
            }
            else
            {
                infile.Error("MenuPowers: '%s' is not a valid key.", infile.Key);
            }
        }

        private bool CheckRequirements(MenuPowersCell? pcell)
        {
            if (pcell == null)
                return false;

            if (SharedGameResources.Pc!.Stats.Level < pcell.RequiresLevel)
                return false;

            for (int i = 0; i < SharedResources.Eset!.PrimaryStats.Stats.Count; ++i)
            {
                if (SharedGameResources.Pc.Stats.GetPrimary(i) < pcell.RequiresPrimary[i])
                    return false;
            }

            for (int i = 0; i < pcell.RequiresStatus.Count; ++i)
                if (!SharedGameResources.Camp!.CheckStatus(pcell.RequiresStatus[i]))
                    return false;

            for (int i = 0; i < pcell.RequiresNotStatus.Count; ++i)
                if (SharedGameResources.Camp.CheckStatus(pcell.RequiresNotStatus[i]))
                    return false;

            for (int i = 0; i < pcell.RequiresPower.Count; ++i)
            {
                if (!CheckUnlocked(GetCellByPowerIndex(pcell.RequiresPower[i])))
                    return false;
            }

            if (SharedGameResources.Powers!.IsValid(pcell.Id) && SharedGameResources.Powers.Powers[pcell.Id]!.Passive && SharedGameResources.Pc.Stats.Hp > 0)
            {
                if (!SharedGameResources.Pc.Stats.CanUsePower(pcell.Id, StatBlock.CanUsePassive))
                    return false;
            }

            return true;
        }

        private bool CheckRequirementStatus(MenuPowersCell? pcell)
        {
            if (pcell == null)
                return false;

            for (int i = 0; i < pcell.RequiresStatus.Count; ++i)
                if (!SharedGameResources.Camp!.CheckStatus(pcell.RequiresStatus[i]))
                    return false;

            for (int i = 0; i < pcell.RequiresNotStatus.Count; ++i)
                if (SharedGameResources.Camp.CheckStatus(pcell.RequiresNotStatus[i]))
                    return false;

            return true;
        }

        private bool CheckUnlocked(MenuPowersCell? pcell)
        {
            if (pcell == null)
                return true;

            if (pcell.IsUnlocked)
                return true;
            if (SharedGameResources.Pc!.Stats.PowersList.Contains(pcell.Id))
                return true;

            if (!pcell.RequiresPoint && pcell.UpgradeLevel <= 1 && CheckRequirements(pcell))
                return true;

            return false;
        }

        private bool CheckUnlock(MenuPowersCell? pcell)
        {
            if (pcell == null)
                return true;

            if (CheckUnlocked(pcell))
                return false;

            if (CheckRequirements(pcell))
                return true;

            return false;
        }

        private bool CheckUpgrade(MenuPowersCell? pcell)
        {
            if (!CheckUnlocked(pcell))
                return false;

            if (pcell!.Next == null || (pcell.Next.RequiresPoint && _pointsLeft < 1))
                return false;

            if (!CheckUnlock(pcell.Next))
                return false;

            return true;
        }

        private void LockCell(MenuPowersCell pcell)
        {
            pcell.IsUnlocked = false;

            if (SharedGameResources.Powers!.Powers[pcell.Id]!.Passive && pcell.PassiveOn)
            {
                int passiveIt = SharedGameResources.Pc!.Stats.PowersPassive.IndexOf(pcell.Id);
                if (passiveIt != -1)
                    SharedGameResources.Pc.Stats.PowersPassive.RemoveAt(passiveIt);

                if (!SharedGameResources.Powers.Powers[pcell.Id]!.PassiveEffectsPersist)
                {
                    SharedGameResources.Pc.Stats.Effects.RemoveEffectPassive(pcell.Id);
                }
                pcell.PassiveOn = false;
                SharedGameResources.Pc.Stats.RefreshStats = true;
            }

            int it = SharedGameResources.Pc.Stats.PowersList.IndexOf(pcell.Id);
            if (it != -1)
                SharedGameResources.Pc.Stats.PowersList.RemoveAt(it);

            SharedGameResources.Menu!.Act!.AddPower(0, pcell.Id);

            if (pcell.Next != null)
            {
                LockCell(pcell.Next);
            }
        }

        private bool IsBonusCell(MenuPowersCell? pcell)
        {
            if (pcell == null)
                return false;

            if (_powerCell[pcell.Group].GetBonusLevels() <= 0)
                return false;

            return ReferenceEquals(pcell, _powerCell[pcell.Group].GetBonusCurrent(_powerCell[pcell.Group].GetCurrent()));
        }

        private bool IsCellVisible(MenuPowersCell? pcell)
        {
            if (pcell == null)
                return false;

            if (!pcell.Visible)
                return false;

            if (pcell.VisibleCheckStatus && !CheckRequirementStatus(pcell))
                return false;
            else if (pcell.VisibleCheckLocked && !CheckUnlocked(pcell))
                return false;

            return true;
        }

        private MenuPowersCell? GetCellByPowerIndex(PowerID powerIndex)
        {
            if (powerIndex == 0)
                return null;

            for (int i = 0; i < _powerCell.Count; ++i)
            {
                for (int j = 0; j < _powerCell[i].Cells.Count; ++j)
                {
                    if (_powerCell[i].Cells[j].Id == powerIndex)
                        return _powerCell[i].Cells[j];
                }
            }

            return null;
        }

        private void UpgradePower(MenuPowersCell? pcell, bool ignoreTab)
        {
            if (pcell == null || pcell.Next == null)
                return;

            if (_tabControl == null || ignoreTab || _tabControl.GetActiveTab() == _powerCell[pcell.Group].Tab)
            {
                pcell.Next.IsUnlocked = true;
                SharedGameResources.Pc!.Stats.PowersList.Add(pcell.Next.Id);
                SharedGameResources.Pc.Stats.CheckTitle = true;
            }
            SetUnlockedPowers();
        }

        public void SetUnlockedPowers()
        {
            bool didCellLock = false;

            ClearActionBarBonusLevels();

            for (int i = 0; i < _powerCell.Count; ++i)
            {
                for (int j = 0; j < _powerCell[i].Cells.Count; ++j)
                {
                    if (!_recentlyLockedCells.Contains(_powerCell[i].Cells[j]))
                    {
                        if (SharedGameResources.Pc!.Stats.PowersList.Contains(_powerCell[i].Cells[j].Id))
                        {
                            _powerCell[i].Cells[j].IsUnlocked = true;
                        }
                        else
                        {
                            if (CheckUnlocked(_powerCell[i].Cells[j]))
                            {
                                SharedGameResources.Pc.Stats.PowersList.Add(_powerCell[i].Cells[j].Id);
                                _powerCell[i].Cells[j].IsUnlocked = true;
                            }
                        }
                    }

                    if (_powerCell[i].Cells[j].IsUnlocked)
                    {
                        if (!CheckRequirements(_powerCell[i].Cells[j]))
                        {
                            LockCell(_powerCell[i].Cells[j]);
                            didCellLock = true;

                            if (_powerCell[i].CurrentCell > 1)
                                _powerCell[i].CurrentCell = j;

                            _recentlyLockedCells.Add(_powerCell[i].Cells[j]);
                        }
                        else
                        {
                            if (_powerCell[i].CurrentCell != j)
                                SharedGameResources.Menu!.Act!.AddPower(_powerCell[i].Cells[j].Id, _powerCell[i].GetCurrent().Id);

                            _powerCell[i].CurrentCell = j;
                            if (Slots[i] != null)
                                Slots[i]!.SetIcon(SharedGameResources.Powers!.Powers[_powerCell[i].Cells[j].Id]!.Icon, WidgetSlot.NoOverlay);
                        }
                    }
                }
            }

            if (didCellLock)
            {
                SetUnlockedPowers();
                return;
            }

            _recentlyLockedCells.Clear();

            for (int i = 0; i < _powerCell.Count; ++i)
            {
                MenuPowersCell currentPcell = _powerCell[i].GetCurrent();

                if (currentPcell.IsUnlocked)
                {
                    MenuPowersCell bonusPcell = _powerCell[i].GetBonusCurrent(currentPcell);

                    for (int j = 0; j < _powerCell[i].Cells.Count; ++j)
                    {
                        MenuPowersCell pcell = _powerCell[i].Cells[j];

                        if (!ReferenceEquals(pcell, bonusPcell) || (pcell.PassiveOn && SharedGameResources.Powers!.Powers[pcell.Id]!.Passive && (!CheckRequirements(currentPcell) || (!pcell.IsUnlocked && !IsBonusCell(pcell)))))
                        {
                            int passiveIt = SharedGameResources.Pc!.Stats.PowersPassive.IndexOf(pcell.Id);
                            if (passiveIt != -1)
                            {
                                SharedGameResources.Pc.Stats.PowersPassive.RemoveAt(passiveIt);

                                if (!SharedGameResources.Powers.Powers[pcell.Id]!.PassiveEffectsPersist)
                                {
                                    SharedGameResources.Pc.Stats.Effects.RemoveEffectPassive(pcell.Id);
                                }
                                pcell.PassiveOn = false;
                                SharedGameResources.Pc.Stats.RefreshStats = true;

                                SharedGameResources.Menu!.Inv!.ApplyEquipment();
                            }
                        }
                        else if (ReferenceEquals(pcell, bonusPcell) && !pcell.PassiveOn && SharedGameResources.Powers!.Powers[pcell.Id]!.Passive && CheckRequirements(currentPcell))
                        {
                            int passiveIt = SharedGameResources.Pc!.Stats.PowersPassive.IndexOf(pcell.Id);
                            if (passiveIt == -1)
                            {
                                SharedGameResources.Pc.Stats.PowersPassive.Add(pcell.Id);

                                pcell.PassiveOn = true;
                                if (SharedGameResources.Pc.Stats.Effects.TriggeredOthers)
                                    SharedGameResources.Powers.ActivateSinglePassive(SharedGameResources.Pc.Stats, pcell.Id);

                                SharedGameResources.Menu!.Inv!.ApplyEquipment();
                            }
                        }

                        if (!SharedGameResources.Powers!.Powers[pcell.Id]!.SpawnType.Equals(""))
                        {
                            SharedGameResources.Pc!.Stats.UpdateSummonPowerIDs(pcell.Id, bonusPcell.Id);
                        }
                    }

                    if (!ReferenceEquals(currentPcell, bonusPcell))
                    {
                        SharedGameResources.Menu!.Act!.AddPower(bonusPcell.Id, currentPcell.Id);
                    }
                }
                else
                {
                    for (int j = 0; j < _powerCell[i].Cells.Count; ++j)
                    {
                        MenuPowersCell pcell = _powerCell[i].Cells[j];

                        if (!SharedGameResources.Powers!.Powers[pcell.Id]!.SpawnType.Equals(""))
                        {
                            SharedGameResources.Pc!.Stats.UpdateSummonPowerIDs(pcell.Id, 0);
                        }
                    }
                }
            }
        }

        private int GetPointsUsed()
        {
            int used = 0;

            for (int i = 0; i < SharedGameResources.Pc!.Stats.PowersList.Count; ++i)
            {
                MenuPowersCell? pcell = GetCellByPowerIndex(SharedGameResources.Pc.Stats.PowersList[i]);
                if (pcell != null && pcell.RequiresPoint)
                    used++;
            }

            return used;
        }

        public void CreateTooltipFromActionBar(TooltipData tipData, uint slot, int tooltipLength)
        {
            if (slot >= SharedGameResources.Menu!.Act!.Hotkeys.Count || slot >= SharedGameResources.Menu.Act.HotkeysMod.Count)
                return;

            PowerID powerIndex = SharedGameResources.Menu.Act.Hotkeys[(int)slot];
            PowerID modPowerIndex = SharedGameResources.Menu.Act.HotkeysMod[(int)slot];

            PowerID pindex = modPowerIndex;
            MenuPowersCell? pcell = GetCellByPowerIndex(pindex);

            if (powerIndex != modPowerIndex && pcell == null)
            {
                PowerID testPindex = powerIndex;
                MenuPowersCell? testPcell = GetCellByPowerIndex(testPindex);

                if (testPcell != null && SharedGameResources.Powers!.Powers[testPindex]!.MetaPowerProvidesTooltip)
                {
                    pindex = testPindex;
                    pcell = testPcell;
                }
            }

            MenuPowersCell? pcellBase = null;
            if (pcell != null)
            {
                pcellBase = _powerCell[pcell.Group].GetCurrent();
            }

            CreateTooltip(tipData, pcellBase, pindex, false, tooltipLength);
            CreateTooltipInputHint(tipData, TooltipShowActivateHint);
        }

        private void CreateTooltip(TooltipData tipData, MenuPowersCell? pcell, PowerID powerIndex, bool showUnlockPrompt, int tooltipLength)
        {
            MenuPowersCell? pcellBonus = null;
            if (pcell != null)
            {
                pcellBonus = _powerCell[pcell.Group].GetBonusCurrent(pcell);
            }
            Power pwr = pcellBonus != null ? SharedGameResources.Powers!.Powers[pcellBonus.Id]! : SharedGameResources.Powers!.Powers[powerIndex]!;

            {
                StringBuilder ss = new StringBuilder();
                ss.Append(pwr.Name);
                if (pcell != null && pcell.UpgradeLevel > 0)
                {
                    ss.Append(" (");
                    ss.Append(SharedResources.Msg!.GetV("Level %d", pcell.UpgradeLevel));
                    int bonusLevels = _powerCell[pcell.Group].GetBonusLevels();
                    if (bonusLevels > 0)
                        ss.Append(", +").Append(bonusLevels);
                    ss.Append(')');
                }
                tipData.AddText(ss.ToString());
            }

            if (tooltipLength == TooltipShort || (pcell == null && tooltipLength != TooltipLongAll))
                return;

            if (pwr.Passive) tipData.AddText(SharedResources.Msg!.Get("Passive"));
            if (pwr.Description != "")
            {
                tipData.AddColoredText(Utils.SubstituteVarsInString(pwr.Description, SharedGameResources.Pc), SharedResources.Font!.GetColor(FontEngine.ColorItemFlavor));
            }

            if (pwr.RequiresMp > 0)
            {
                tipData.AddText(SharedResources.Msg!.GetV("Costs %s MP", Utils.FloatToString(pwr.RequiresMp, SharedResources.Eset!.NumberFormat.PowerTooltips)));
            }
            if (pwr.RequiresHp > 0)
            {
                tipData.AddText(SharedResources.Msg!.GetV("Costs %s HP", Utils.FloatToString(pwr.RequiresHp, SharedResources.Eset!.NumberFormat.PowerTooltips)));
            }
            for (int i = 0; i < pwr.RequiresResourceStat.Count; ++i)
            {
                if (pwr.RequiresResourceStat[i] > 0 && !SharedResources.Eset!.ResourceStats.Stats[i].TextTooltipCost.Equals(""))
                {
                    tipData.AddText(SharedResources.Eset.ResourceStats.Stats[i].TextTooltipCost + ": " + Utils.FloatToString(pwr.RequiresResourceStat[i], SharedResources.Eset.NumberFormat.PowerTooltips));
                }
            }
            if (pwr.Cooldown > 0)
            {
                tipData.AddText(SharedResources.Msg!.Get("Cooldown:") + " " + Utils.GetDurationString(pwr.Cooldown, SharedResources.Eset!.NumberFormat.Durations));
            }

            if (pwr.UseHazard || pwr.Type == Power.TypeRepeater)
            {
                StringBuilder ss = new StringBuilder();

                if (pwr.ModDamageMode > -1)
                {
                    if (pwr.ModDamageMode == Power.StatModifierModeAdd && pwr.ModDamageValueMin > 0)
                        ss.Append('+');

                    if (pwr.ModDamageValueMax == 0 || pwr.ModDamageValueMin == pwr.ModDamageValueMax)
                    {
                        ss.Append(Utils.FloatToString(pwr.ModDamageValueMin, SharedResources.Eset!.NumberFormat.PowerTooltips));
                    }
                    else
                    {
                        ss.Append(Utils.FloatToString(pwr.ModDamageValueMin, SharedResources.Eset!.NumberFormat.PowerTooltips)).Append('-').Append(Utils.FloatToString(pwr.ModDamageValueMax, SharedResources.Eset.NumberFormat.PowerTooltips));
                    }

                    if (pwr.ModDamageMode == Power.StatModifierModeMultiply)
                    {
                        ss.Append('%');
                    }
                    ss.Append(' ');

                    if (pwr.BaseDamage != SharedResources.Eset!.DamageTypes.Types.Count)
                    {
                        ss.Append(SharedResources.Eset.DamageTypes.Types[pwr.BaseDamage].Name);
                    }

                    if (pwr.Count > 1 && pwr.Type != Power.TypeRepeater)
                        ss.Append(" (x").Append(pwr.Count).Append(')');

                    if (ss.Length > 0)
                        tipData.AddColoredText(ss.ToString(), SharedResources.Font!.GetColor(FontEngine.ColorMenuBonus));
                }
                else
                {
                    if (pwr.BaseDamage != SharedResources.Eset!.DamageTypes.Types.Count)
                    {
                        tipData.AddText(SharedResources.Eset.DamageTypes.Types[pwr.BaseDamage].Name);
                    }
                }
            }

            for (int i = 0; i < pwr.PostEffects.Count; ++i)
            {
                StringBuilder ss = new StringBuilder();

                EffectDef? effectPtr = pwr.PostEffects[i].EffectPtr;
                int effectType = pwr.PostEffects[i].EffectType;

                if (Effect.TypeIsStat(effectType) ||
                    Effect.TypeIsDmgMin(effectType) ||
                    Effect.TypeIsDmgMax(effectType) ||
                    Effect.TypeIsResist(effectType) ||
                    Effect.TypeIsPrimary(effectType) ||
                    Effect.TypeIsResourceStat(effectType))
                {
                    if (pwr.PostEffects[i].IsMultiplier)
                        ss.Append(Utils.FloatToString(pwr.PostEffects[i].Magnitude, SharedResources.Eset!.NumberFormat.PowerTooltips + 2)).Append('×');
                    else if (pwr.PostEffects[i].Magnitude > 0)
                        ss.Append('+').Append(Utils.FloatToString(pwr.PostEffects[i].Magnitude, SharedResources.Eset!.NumberFormat.PowerTooltips));
                    else
                        ss.Append(Utils.FloatToString(pwr.PostEffects[i].Magnitude, SharedResources.Eset!.NumberFormat.PowerTooltips));
                }

                if (Effect.TypeIsStat(effectType))
                {
                    int index = Effect.GetStatFromType(effectType);
                    if (Stats.Percent[index] && !pwr.PostEffects[i].IsMultiplier)
                    {
                        ss.Append('%');
                    }
                    ss.Append(' ').Append(Stats.Name[index]);
                }
                else if (Effect.TypeIsDmgMin(effectType))
                {
                    int index = Effect.GetDmgFromType(effectType);
                    ss.Append(' ').Append(SharedResources.Eset!.DamageTypes.Types[index].NameMin);
                }
                else if (Effect.TypeIsDmgMax(effectType))
                {
                    int index = Effect.GetDmgFromType(effectType);
                    ss.Append(' ').Append(SharedResources.Eset!.DamageTypes.Types[index].NameMax);
                }
                else if (Effect.TypeIsResist(effectType))
                {
                    int index = Effect.GetDmgFromType(effectType);
                    if (!pwr.PostEffects[i].IsMultiplier)
                    {
                        ss.Append('%');
                    }
                    ss.Append(' ').Append(SharedResources.Eset!.DamageTypes.Types[index].NameResist);
                }
                else if (Effect.TypeIsPrimary(effectType))
                {
                    int index = Effect.GetPrimaryFromType(effectType);
                    ss.Append(' ').Append(SharedResources.Eset!.PrimaryStats.Stats[index].Name);
                }
                else if (Effect.TypeIsResourceStat(effectType))
                {
                    int index = Effect.GetResourceStatFromType(effectType);
                    int subIndex = Effect.GetResourceStatSubIndexFromType(effectType);

                    if (!pwr.PostEffects[i].IsMultiplier && (subIndex == EngineSettings.ResourceStatsSettings.StatSteal || subIndex == EngineSettings.ResourceStatsSettings.StatResistSteal))
                    {
                        ss.Append('%');
                    }
                    ss.Append(' ').Append(SharedResources.Eset!.ResourceStats.Stats[index].Text[subIndex]);
                }
                else if (Effect.TypeIsResourceEffect(effectType))
                {
                    int index = Effect.GetResourceStatFromType(effectType);
                    int subIndex = Effect.GetResourceStatSubIndexFromType(effectType);

                    if (subIndex == EngineSettings.ResourceStatsSettings.StatHeal)
                    {
                        ss.Append(Utils.FloatToString(pwr.PostEffects[i].Magnitude, SharedResources.Eset!.NumberFormat.PowerTooltips)).Append(' ').Append(SharedResources.Eset.ResourceStats.Stats[index].TextTooltipHeal);
                    }
                    else if (subIndex == EngineSettings.ResourceStatsSettings.StatHealPercent)
                    {
                        ss.Append(Utils.FloatToString(pwr.PostEffects[i].Magnitude, SharedResources.Eset!.NumberFormat.PowerTooltips)).Append("% ").Append(SharedResources.Eset.ResourceStats.Stats[index].TextTooltipHeal);
                    }
                }
                else if (effectType == Effect.Damage)
                {
                    ss.Append(Utils.FloatToString(pwr.PostEffects[i].Magnitude, SharedResources.Eset!.NumberFormat.PowerTooltips)).Append(' ').Append(SharedResources.Msg!.Get("Damage per second"));
                }
                else if (effectType == Effect.DamagePercent)
                {
                    ss.Append(Utils.FloatToString(pwr.PostEffects[i].Magnitude, SharedResources.Eset!.NumberFormat.PowerTooltips)).Append("% ").Append(SharedResources.Msg!.Get("Damage per second"));
                }
                else if (effectType == Effect.Hpot)
                {
                    ss.Append(Utils.FloatToString(pwr.PostEffects[i].Magnitude, SharedResources.Eset!.NumberFormat.PowerTooltips)).Append(' ').Append(SharedResources.Msg!.Get("HP per second"));
                }
                else if (effectType == Effect.HpotPercent)
                {
                    ss.Append(Utils.FloatToString(pwr.PostEffects[i].Magnitude, SharedResources.Eset!.NumberFormat.PowerTooltips)).Append("% ").Append(SharedResources.Msg!.Get("HP per second"));
                }
                else if (effectType == Effect.Mpot)
                {
                    ss.Append(Utils.FloatToString(pwr.PostEffects[i].Magnitude, SharedResources.Eset!.NumberFormat.PowerTooltips)).Append(' ').Append(SharedResources.Msg!.Get("MP per second"));
                }
                else if (effectType == Effect.MpotPercent)
                {
                    ss.Append(Utils.FloatToString(pwr.PostEffects[i].Magnitude, SharedResources.Eset!.NumberFormat.PowerTooltips)).Append("% ").Append(SharedResources.Msg!.Get("MP per second"));
                }
                else if (effectType == Effect.Speed)
                {
                    if (pwr.PostEffects[i].Magnitude == 0)
                        ss.Append(SharedResources.Msg!.Get("Immobilize"));
                    else
                        ss.Append(SharedResources.Msg!.GetV("%s%% Speed", Utils.FloatToString(pwr.PostEffects[i].Magnitude, SharedResources.Eset!.NumberFormat.PowerTooltips)));
                }
                else if (effectType == Effect.AttackSpeed)
                {
                    ss.Append(SharedResources.Msg!.GetV("%s%% Attack Speed", Utils.FloatToString(pwr.PostEffects[i].Magnitude, SharedResources.Eset!.NumberFormat.PowerTooltips)));
                }
                else if (effectType == Effect.ResistAll)
                {
                    ss.Append('+').Append(pwr.PostEffects[i].Magnitude).Append("% ").Append(SharedResources.Msg!.Get("Resist All Negative Effects"));
                }

                else if (effectType == Effect.Stun)
                {
                    ss.Append(SharedResources.Msg!.Get("Stun"));
                }
                else if (effectType == Effect.Revive)
                {
                    ss.Append(SharedResources.Msg!.Get("Automatic revive on death"));
                }
                else if (effectType == Effect.Convert)
                {
                    ss.Append(SharedResources.Msg!.Get("Convert"));
                }
                else if (effectType == Effect.Fear)
                {
                    ss.Append(SharedResources.Msg!.Get("Fear"));
                }
                else if (effectType == Effect.DeathSentence)
                {
                    ss.Append(SharedResources.Msg!.Get("Lifespan"));
                }
                else if (effectType == Effect.Shield)
                {
                    if (pwr.BaseDamage == SharedResources.Eset!.DamageTypes.Types.Count)
                        continue;

                    if (pwr.ModDamageMode == Power.StatModifierModeMultiply)
                    {
                        float magnitude = SharedResources.Eset.Combat.ResourceRound(SharedGameResources.Pc!.Stats.GetDamageMax(pwr.BaseDamage) * pwr.ModDamageValueMin / 100);
                        ss.Append(Utils.FloatToString(magnitude, SharedResources.Eset.NumberFormat.PowerTooltips));
                    }
                    else if (pwr.ModDamageMode == Power.StatModifierModeAdd)
                    {
                        float magnitude = SharedResources.Eset.Combat.ResourceRound(SharedGameResources.Pc!.Stats.GetDamageMax(pwr.BaseDamage) + pwr.ModDamageValueMin);
                        ss.Append(Utils.FloatToString(magnitude, SharedResources.Eset.NumberFormat.PowerTooltips));
                    }
                    else if (pwr.ModDamageMode == Power.StatModifierModeAbsolute)
                    {
                        if (pwr.ModDamageValueMax == 0 || pwr.ModDamageValueMin == pwr.ModDamageValueMax)
                            ss.Append(Utils.FloatToString(SharedResources.Eset.Combat.ResourceRound(pwr.ModDamageValueMin), SharedResources.Eset.NumberFormat.PowerTooltips));
                        else
                            ss.Append(Utils.FloatToString(SharedResources.Eset.Combat.ResourceRound(pwr.ModDamageValueMin), SharedResources.Eset.NumberFormat.PowerTooltips)).Append('-').Append(Utils.FloatToString(SharedResources.Eset.Combat.ResourceRound(pwr.ModDamageValueMax), SharedResources.Eset.NumberFormat.PowerTooltips));
                    }
                    else
                    {
                        ss.Append(Utils.FloatToString(SharedResources.Eset.Combat.ResourceRound(SharedGameResources.Pc!.Stats.GetDamageMax(pwr.BaseDamage)), SharedResources.Eset.NumberFormat.PowerTooltips));
                    }

                    ss.Append(' ').Append(_tooltipTextShield);
                }
                else if (effectType == Effect.Heal)
                {
                    if (pwr.BaseDamage == SharedResources.Eset!.DamageTypes.Types.Count)
                        continue;

                    float magMin = SharedGameResources.Pc!.Stats.GetDamageMin(pwr.BaseDamage);
                    float magMax = SharedGameResources.Pc.Stats.GetDamageMax(pwr.BaseDamage);

                    if (pwr.ModDamageMode == Power.StatModifierModeMultiply)
                    {
                        magMin = magMin * pwr.ModDamageValueMin / 100;
                        magMax = magMax * pwr.ModDamageValueMin / 100;
                    }
                    else if (pwr.ModDamageMode == Power.StatModifierModeAdd)
                    {
                        magMin = magMin + pwr.ModDamageValueMin;
                        magMax = magMax + pwr.ModDamageValueMin;
                    }
                    else if (pwr.ModDamageMode == Power.StatModifierModeAbsolute)
                    {
                        if (pwr.ModDamageValueMax == 0 || pwr.ModDamageValueMin == pwr.ModDamageValueMax)
                            magMin = magMax = pwr.ModDamageValueMin;
                        else
                        {
                            magMin = pwr.ModDamageValueMin;
                            magMax = pwr.ModDamageValueMax;
                        }
                    }

                    magMax = Math.Max(magMin, magMax);

                    ss.Append(Utils.FloatToString(magMin, SharedResources.Eset!.NumberFormat.PowerTooltips));
                    if (magMin != magMax)
                        ss.Append('-').Append(Utils.FloatToString(magMax, SharedResources.Eset.NumberFormat.PowerTooltips));

                    ss.Append(' ').Append(_tooltipTextHeal);
                }
                else if (effectType == Effect.Knockback)
                {
                    ss.Append(Utils.FloatToString(pwr.PostEffects[i].Magnitude, SharedResources.Eset!.NumberFormat.PowerTooltips)).Append(' ').Append(SharedResources.Msg!.Get("Knockback"));
                }
                else if (effectPtr != null && !effectPtr.Name.Equals("") && pwr.PostEffects[i].Magnitude > 0)
                {
                    if (effectPtr.CanStack)
                        ss.Append('+');
                    ss.Append(Utils.FloatToString(pwr.PostEffects[i].Magnitude, SharedResources.Eset!.NumberFormat.PowerTooltips)).Append(' ').Append(SharedResources.Msg!.Get(effectPtr.Name));
                }
                else if (pwr.PostEffects[i].Magnitude == 0)
                {
                    // nothing
                }

                if (ss.Length > 0)
                {
                    if (pwr.PostEffects[i].Duration > 0)
                    {
                        if (effectType == Effect.DeathSentence)
                        {
                            ss.Append(": ").Append(Utils.GetDurationString(pwr.PostEffects[i].Duration, SharedResources.Eset!.NumberFormat.Durations));
                        }
                        else
                        {
                            ss.Append(" (").Append(Utils.GetDurationString(pwr.PostEffects[i].Duration, SharedResources.Eset!.NumberFormat.Durations)).Append(')');
                        }

                        if (pwr.PostEffects[i].Chance != 100)
                            ss.Append(' ');
                    }
                    if (pwr.PostEffects[i].Chance != 100)
                    {
                        ss.Append('(').Append(SharedResources.Msg!.GetV("%s%% chance", Utils.FloatToString(pwr.PostEffects[i].Chance, SharedResources.Eset!.NumberFormat.PowerTooltips))).Append(')');
                    }

                    tipData.AddColoredText(ss.ToString(), SharedResources.Font!.GetColor(FontEngine.ColorMenuBonus));
                }
            }

            if (pwr.UseHazard || pwr.Type == Power.TypeRepeater)
            {
                StringBuilder ss = new StringBuilder();

                if (pwr.ModAccuracyMode > -1)
                {
                    ss.Clear();

                    if (pwr.ModAccuracyMode == Power.StatModifierModeAdd && pwr.ModAccuracyValue > 0)
                        ss.Append('+');

                    ss.Append(Utils.FloatToString(pwr.ModAccuracyValue, SharedResources.Eset!.NumberFormat.PowerTooltips));

                    if (pwr.ModAccuracyMode == Power.StatModifierModeMultiply)
                    {
                        ss.Append('%');
                    }
                    ss.Append(' ');

                    ss.Append(SharedResources.Msg!.Get("Base Accuracy"));

                    if (ss.Length > 0)
                        tipData.AddColoredText(ss.ToString(), SharedResources.Font!.GetColor(FontEngine.ColorMenuBonus));
                }

                if (pwr.ModCritMode > -1)
                {
                    ss.Clear();

                    if (pwr.ModCritMode == Power.StatModifierModeAdd && pwr.ModCritValue > 0)
                        ss.Append('+');

                    ss.Append(Utils.FloatToString(pwr.ModCritValue, SharedResources.Eset!.NumberFormat.PowerTooltips));

                    if (pwr.ModCritMode == Power.StatModifierModeMultiply)
                    {
                        ss.Append('%');
                    }
                    ss.Append(' ');

                    ss.Append(SharedResources.Msg!.Get("Base Critical Chance"));

                    if (ss.Length > 0)
                        tipData.AddColoredText(ss.ToString(), SharedResources.Font!.GetColor(FontEngine.ColorMenuBonus));
                }

                if (pwr.TraitArmorPenetration)
                {
                    ss.Clear();
                    ss.Append(SharedResources.Msg!.Get("Ignores Absorption"));
                    tipData.AddColoredText(ss.ToString(), SharedResources.Font!.GetColor(FontEngine.ColorMenuBonus));
                }
                if (pwr.TraitAvoidanceIgnore)
                {
                    ss.Clear();
                    ss.Append(SharedResources.Msg!.Get("Ignores Avoidance"));
                    tipData.AddColoredText(ss.ToString(), SharedResources.Font!.GetColor(FontEngine.ColorMenuBonus));
                }
                if (pwr.TraitCritsImpaired > 0)
                {
                    ss.Clear();
                    ss.Append(SharedResources.Msg!.GetV("%s%% Chance to crit slowed targets", Utils.FloatToString(pwr.TraitCritsImpaired, SharedResources.Eset!.NumberFormat.PowerTooltips)));
                    tipData.AddColoredText(ss.ToString(), SharedResources.Font!.GetColor(FontEngine.ColorMenuBonus));
                }
                if (pwr.ConvertedDamage < SharedResources.Eset!.DamageTypes.Types.Count)
                {
                    ss.Clear();
                    ss.Append(SharedResources.Msg!.GetV("Damage dealt as: %s", SharedResources.Eset.DamageTypes.Types[pwr.ConvertedDamage].Name));
                    tipData.AddColoredText(ss.ToString(), SharedResources.Font!.GetColor(FontEngine.ColorMenuBonus));
                }
            }

            if (pwr.SpawnLimitCount > 0 && !pwr.SpawnType.Equals(""))
            {
                int spawnLimit = (int)pwr.SpawnLimitCount;
                if (pwr.SpawnLimitMode == Power.SpawnLimitModeStat)
                {
                    if (pwr.SpawnLimitStat < SharedResources.Eset!.PrimaryStats.Stats.Count)
                    {
                        spawnLimit = (int)(pwr.SpawnLimitCount * ((float)SharedGameResources.Pc!.Stats.GetPrimary(pwr.SpawnLimitStat) / pwr.SpawnLimitRatio));
                    }
                }
                tipData.AddColoredText(SharedResources.Msg!.GetV("Spawn limit: %d", spawnLimit), SharedResources.Font!.GetColor(FontEngine.ColorMenuBonus));
            }

            foreach (int flagIndex in pwr.RequiresFlags.OrderBy(x => x))
            {
                string requiredFlag = SharedResources.Eset!.EquipFlags.Flags[flagIndex].Name;
                if (!requiredFlag.Equals(""))
                    tipData.AddText(SharedResources.Msg!.GetV("Requires a %s", SharedResources.Msg.Get(requiredFlag)));
            }

            if (pcell != null)
            {
                for (int i = 0; i < SharedResources.Eset!.PrimaryStats.Stats.Count; ++i)
                {
                    if (pcell.RequiresPrimary[i] > 0)
                    {
                        if (SharedGameResources.Pc!.Stats.GetPrimary(i) < pcell.RequiresPrimary[i])
                            tipData.AddColoredText(SharedResources.Msg!.GetV("Requires %s %d", SharedResources.Eset.PrimaryStats.Stats[i].Name, pcell.RequiresPrimary[i]), SharedResources.Font!.GetColor(FontEngine.ColorMenuPenalty));
                        else
                            tipData.AddText(SharedResources.Msg!.GetV("Requires %s %d", SharedResources.Eset.PrimaryStats.Stats[i].Name, pcell.RequiresPrimary[i]));
                    }
                }

                if ((pcell.RequiresLevel > 0) && SharedGameResources.Pc!.Stats.Level < pcell.RequiresLevel)
                {
                    tipData.AddColoredText(SharedResources.Msg!.GetV("Requires Level %d", pcell.RequiresLevel), SharedResources.Font!.GetColor(FontEngine.ColorMenuPenalty));
                }
                else if ((pcell.RequiresLevel > 0) && SharedGameResources.Pc.Stats.Level >= pcell.RequiresLevel)
                {
                    tipData.AddText(SharedResources.Msg!.GetV("Requires Level %d", pcell.RequiresLevel));
                }

                for (int j = 0; j < pcell.RequiresPower.Count; ++j)
                {
                    MenuPowersCell? reqCell = GetCellByPowerIndex(pcell.RequiresPower[j]);
                    if (reqCell == null)
                        continue;

                    string reqPowerName;
                    if (reqCell.UpgradeLevel > 0)
                        reqPowerName = SharedGameResources.Powers!.Powers[reqCell.Id]!.Name + " (" + SharedResources.Msg!.GetV("Level %d", reqCell.UpgradeLevel) + ")";
                    else
                        reqPowerName = SharedGameResources.Powers!.Powers[reqCell.Id]!.Name;

                    if (!CheckUnlocked(reqCell))
                    {
                        tipData.AddColoredText(SharedResources.Msg!.GetV("Requires Power: %s", reqPowerName), SharedResources.Font!.GetColor(FontEngine.ColorMenuPenalty));
                    }
                    else
                    {
                        tipData.AddText(SharedResources.Msg!.GetV("Requires Power: %s", reqPowerName));
                    }
                }

                if (pcell.RequiresPoint && !SharedGameResources.Pc!.Stats.PowersList.Contains(pcell.Id))
                {
                    MenuPowersCell? unlockCell = GetCellByPowerIndex(pcell.Id);
                    if (showUnlockPrompt && pcell.UpgradeLevel <= 1 && _pointsLeft > 0 && SharedResources.Inpt!.UsingMouse() && CheckUnlock(unlockCell))
                    {
                        tipData.AddColoredText(SharedResources.Msg!.Get("Click to Unlock (uses 1 Skill Point)"), SharedResources.Font!.GetColor(FontEngine.ColorMenuBonus));
                    }
                    else
                    {
                        if (pcell.RequiresPoint && _pointsLeft < 1)
                            tipData.AddColoredText(SharedResources.Msg!.Get("Requires 1 Skill Point"), SharedResources.Font!.GetColor(FontEngine.ColorMenuPenalty));
                        else
                            tipData.AddText(SharedResources.Msg!.Get("Requires 1 Skill Point"));
                    }
                }
            }
        }

        private void CreateTooltipInputHint(TooltipData tipData, bool enableActivateMsg)
        {
            bool showActivateMsg = false;
            string activateBindStr = "";

            bool showMoreMsg = false;
            string moreBindStr = "";

            if (SharedResources.Inpt!.Mode == InputState.ModeTouchscreen)
            {
                tipData.AddColoredText('\n' + SharedResources.Msg!.Get("Tap icon again for more options"), SharedResources.Font!.GetColor(FontEngine.ColorItemBonus));
            }
            else if (SharedResources.Inpt.Mode == InputState.ModeJoystick)
            {
                if (enableActivateMsg)
                {
                    showActivateMsg = true;
                    activateBindStr = SharedResources.Inpt.GetGamepadBindingString(Input.MenuActivate);
                }

                showMoreMsg = true;
                moreBindStr = SharedResources.Inpt.GetGamepadBindingString(Input.Accept);
            }
            else if (!SharedResources.Inpt.UsingMouse())
            {
                if (enableActivateMsg)
                {
                    showActivateMsg = true;
                    activateBindStr = SharedResources.Inpt.GetBindingString(Input.MenuActivate);
                }

                showMoreMsg = true;
                moreBindStr = SharedResources.Inpt.GetBindingString(Input.Accept);
            }
            else
            {
                showActivateMsg = enableActivateMsg;
                activateBindStr = SharedResources.Inpt.GetBindingString(Input.Main2);
            }

            if (showActivateMsg || showMoreMsg)
            {
                tipData.AddText("");
            }

            if (showActivateMsg)
            {
                tipData.AddColoredText(SharedResources.Msg!.GetV("Press [%s] to use", activateBindStr), SharedResources.Font!.GetColor(FontEngine.ColorItemBonus));
            }

            if (showMoreMsg)
            {
                tipData.AddColoredText(SharedResources.Msg!.GetV("Press [%s] for more options", moreBindStr), SharedResources.Font!.GetColor(FontEngine.ColorItemBonus));
            }
        }

        private void RenderPowers(int tabNum)
        {
            Rectangle disabledSrc;
            disabledSrc.X = disabledSrc.Y = 0;
            disabledSrc.Width = disabledSrc.Height = SharedResources.Eset!.Resolutions.IconSize;

            for (int i = 0; i < _powerCell.Count; i++)
            {
                if (_powerCell[i].Tab != tabNum) continue;

                MenuPowersCell slotCell = _powerCell[i].GetCurrent();
                if (!IsCellVisible(slotCell))
                    continue;

                if (Slots[i] != null)
                {
                    Slots[i]!.Enabled = true;

                    if (CheckUnlocked(slotCell))
                    {
                        int selectedSlot = -1;
                        if (IsTabListSelected())
                        {
                            selectedSlot = GetSelectedCellIndex();
                        }

                        if (selectedSlot != i)
                            Slots[i]!.Highlight = true;
                    }
                    else
                    {
                        Slots[i]!.Highlight = false;
                        Slots[i]!.Enabled = false;
                    }

                    Slots[i]!.Render();
                }

                if (_powerCell[i].UpgradeButton != null)
                    _powerCell[i].UpgradeButton!.Render();
            }
        }

        public void Logic()
        {
            if (!Visible && _tabControl != null && _defaultPowerTab > -1)
            {
                _tabControl.SetActiveTab((uint)_defaultPowerTab);
                Tablist.SetNextTabList(TablistPow[_defaultPowerTab]);
            }

            SetUnlockedPowers();

            _pointsLeft = (SharedGameResources.Pc!.Stats.Level * SharedGameResources.Pc.Stats.PowerPointsPerLevel) - GetPointsUsed();
            if (_pointsLeft > 0)
            {
                NewPowerNotification = true;
            }

            for (int i = 0; i < _powerCell.Count; i++)
            {
                if (Visible && Slots[i] != null)
                    Slots[i]!.EnableTablistNav = IsCellVisible(_powerCell[i].GetCurrent());

                if (_powerCell[i].UpgradeButton != null)
                {
                    _powerCell[i].UpgradeButton!.Enabled = false;
                }

                MenuPowersCell pcell = _powerCell[i].GetCurrent();
                while (CheckUpgrade(pcell))
                {
                    if (pcell.Next != null && !pcell.Next.RequiresPoint)
                    {
                        UpgradePower(pcell, UpgradePowerAllTabs);
                        pcell = _powerCell[i].GetCurrent();
                        if (_powerCell[i].UpgradeButton != null)
                            _powerCell[i].UpgradeButton!.Enabled = (SharedGameResources.Pc!.Stats.Hp > 0 && IsCellVisible(pcell) && CheckUpgrade(pcell));
                    }
                    else
                    {
                        if (_powerCell[i].UpgradeButton != null)
                            _powerCell[i].UpgradeButton!.Enabled = (SharedGameResources.Pc!.Stats.Hp > 0 && IsCellVisible(pcell));
                        break;
                    }
                }

                if (Visible && SharedGameResources.Pc!.Stats.Hp > 0 && _powerCell[i].UpgradeButton != null)
                {
                    if ((_tabControl == null || _powerCell[i].Tab == _tabControl.GetActiveTab()) && _powerCell[i].UpgradeButton!.CheckClick())
                    {
                        UpgradePower(_powerCell[i].GetCurrent(), !UpgradePowerAllTabs);
                    }
                }
            }

            if (!Visible) return;

            Tablist.Logic();
            if (_tabs.Count > 0)
            {
                for (int i = 0; i < _tabs.Count; i++)
                {
                    if (_tabControl != null && _tabControl.GetActiveTab() == i)
                    {
                        Tablist.SetNextTabList(TablistPow[i]);
                    }
                    TablistPow[i].Logic();
                }
            }

            if (_closeButton!.CheckClick())
            {
                Visible = false;
                SharedResources.Snd!.Play(SfxClose, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);
            }

            if (_tabControl != null)
            {
                for (int i = 0; i < Slots.Count; i++)
                {
                    if (_powerCell[i].Tab == _tabControl.GetActiveTab())
                        continue;

                    if (Slots[i] != null && Slots[i]!.InFocus)
                        Slots[i]!.Defocus();
                }

                _tabControl.Logic();
            }
        }

        public override void Render()
        {
            if (!Visible) return;

            Rectangle src;
            Rectangle dest, tabDest;

            dest = WindowArea;
            src.X = 0;
            src.Y = 0;
            src.Width = WindowArea.Width;
            src.Height = WindowArea.Height;

            tabDest = WindowArea;
            tabDest.X += _tabArea.X;
            tabDest.Y += _tabArea.Y;
            if (_tabControl != null)
                tabDest.Y += _tabControl.GetTabHeight();

            SetBackgroundClip(src);
            SetBackgroundDest(dest);
            base.Render();

            if (_tabControl != null)
            {
                _tabControl.Render();
                int activeTab = _tabControl.GetActiveTab();
                for (int i = 0; i < _tabs.Count; i++)
                {
                    if (activeTab == i)
                    {
                        Sprite? r = _treeSurf[i];
                        if (r != null)
                        {
                            if (_tabs[i].BackgroundIsMenuSize)
                            {
                                r.SetClipFromRect(src);
                                r.SetDestFromRect(dest);
                            }
                            else
                            {
                                r.SetDestFromRect(tabDest);
                            }
                            SharedResources.RenderDevice!.Render(r);
                        }

                        RenderPowers(activeTab);
                    }
                }
            }
            else if (_treeSurf.Count > 0)
            {
                Sprite? r = _treeSurf[0];
                if (r != null)
                {
                    r.SetClipFromRect(src);
                    r.SetDestFromRect(dest);
                    SharedResources.RenderDevice!.Render(r);
                }
                RenderPowers(0);
            }
            else
            {
                RenderPowers(0);
            }

            _closeButton!.Render();

            _labelPowers.Render();

            if (!_labelUnspent.IsHidden())
            {
                if (_pointsLeft >= 1)
                {
                    _labelUnspent.SetText(SharedResources.Msg!.GetV("Available skill points: %d", _pointsLeft));
                }
                else
                {
                    _labelUnspent.SetText("");
                }
                _labelUnspent.Render();
            }
        }

        public void RenderTooltips(Int2 position)
        {
            if (!Visible || !Utils.IsWithinRect(WindowArea, position))
                return;

            TooltipData tipData = new TooltipData();

            for (int i = 0; i < _powerCell.Count; i++)
            {
                if (_tabControl != null && (_tabControl.GetActiveTab() != _powerCell[i].Tab))
                    continue;

                MenuPowersCell tipCell = _powerCell[i].GetCurrent();
                if (!IsCellVisible(tipCell))
                    continue;

                if (Slots[i] != null && Utils.IsWithinRect(Slots[i]!.Pos, position))
                {
                    bool baseUnlocked = CheckUnlocked(tipCell);

                    CreateTooltip(tipData, tipCell, tipCell.Id, !baseUnlocked, TooltipLongMenu);
                    if (baseUnlocked && tipCell.Next != null)
                    {
                        tipData.AddText("\n" + SharedResources.Msg!.Get("Next Level:"));
                        CreateTooltip(tipData, tipCell.Next, tipCell.Next.Id, baseUnlocked, TooltipLongMenu);
                    }
                    CreateTooltipInputHint(tipData, !TooltipShowActivateHint);

                    SharedResources.Tooltipm!.Push(tipData, position, TooltipData.StyleFloat);

                    break;
                }
                else if (_powerCell[i].UpgradeButton != null)
                {
                    if (_powerCell[i].UpgradeButton!.Enabled && tipCell.Next != null)
                        _powerCell[i].UpgradeButton!.Tooltip = SharedResources.Msg!.GetV("Upgrade to: %s (Level %d)\nUses 1 Skill Point", SharedGameResources.Powers!.Powers[tipCell.Next.Id]!.Name, tipCell.Next.UpgradeLevel);
                    else
                        _powerCell[i].UpgradeButton!.Tooltip = "";
                }
            }
        }

        public MenuPowersClick Click(Int2 mouse)
        {
            MenuPowersClick result = new MenuPowersClick();

            int activeTab = (_tabControl != null) ? _tabControl.GetActiveTab() : 0;

            for (int i = 0; i < _powerCell.Count; i++)
            {
                if (Slots[i] != null && Utils.IsWithinRect(Slots[i]!.Pos, mouse) && (_powerCell[i].Tab == activeTab))
                {
                    if (SharedResources.Inpt!.Mode == InputState.ModeTouchscreen)
                    {
                        bool slotHadFocus = Slots[i]!.InFocus;

                        if (_tabs.Count > 0)
                        {
                            TablistPow[activeTab].SetCurrent(Slots[i]);
                        }
                        else
                        {
                            Tablist.SetCurrent(Slots[i]);
                        }

                        if (!slotHadFocus)
                        {
                            return result;
                        }
                    }

                    MenuPowersCell? pcell = _powerCell[i].GetCurrent();
                    if (pcell == null || !IsCellVisible(pcell))
                        return result;

                    if (CheckUnlock(pcell) && _pointsLeft > 0 && pcell.RequiresPoint)
                    {
                        result.Unlock = pcell.Id;
                    }
                    else if (pcell.Next != null && CheckUpgrade(pcell))
                    {
                        result.Unlock = pcell.Id;
                    }

                    if (CheckUnlocked(pcell) && !SharedGameResources.Powers!.Powers[pcell.Id]!.Passive)
                    {
                        if (SharedResources.Inpt.UsingMouse() && SharedResources.Inpt.Mode != InputState.ModeTouchscreen)
                        {
                            Slots[i]!.Defocus();
                            if (_tabs.Count > 0)
                            {
                                TablistPow[activeTab].SetCurrent(null);
                            }
                            else
                            {
                                Tablist.SetCurrent(null);
                            }
                        }
                        result.Drag = _powerCell[i].GetBonusCurrent(pcell).Id;
                    }

                    return result;
                }
            }

            DefocusTabLists();

            return result;
        }

        public void ClickUnlock(PowerID powerIndex)
        {
            MenuPowersCell? pcell = GetCellByPowerIndex(powerIndex);
            if (pcell == null)
                return;

            if (!CheckUnlocked(pcell))
            {
                SharedGameResources.Pc!.Stats.PowersList.Add(powerIndex);
                SharedGameResources.Pc.Stats.CheckTitle = true;
                SetUnlockedPowers();
                SharedGameResources.Menu!.Act!.AddPower(powerIndex, 0);
            }
            else
            {
                UpgradePower(pcell, !UpgradePowerAllTabs);
            }
        }

        public void ResetToBasePowers()
        {
            for (int i = 0; i < _powerCell.Count; ++i)
            {
                _powerCell[i].CurrentCell = 0;
                for (int j = 0; j < _powerCell[i].Cells.Count; ++j)
                {
                    _powerCell[i].Cells[j].IsUnlocked = false;
                    _powerCell[i].Cells[j].PassiveOn = false;
                }
            }

            SetUnlockedPowers();
        }

        public bool MeetsUsageStats(PowerID powerIndex)
        {
            MenuPowersCell? pcell = GetCellByPowerIndex(powerIndex);

            if (pcell == null)
                return true;

            MenuPowersCell basePcell = _powerCell[pcell.Group].GetCurrent();

            if (SharedGameResources.Pc!.Stats.Level < basePcell.RequiresLevel)
                return false;

            for (int i = 0; i < SharedResources.Eset!.PrimaryStats.Stats.Count; ++i)
            {
                if (SharedGameResources.Pc.Stats.GetPrimary(i) < basePcell.RequiresPrimary[i])
                    return false;
            }

            return true;
        }

        public void ClearActionBarBonusLevels()
        {
            for (int i = 0; i < _powerCell.Count; ++i)
            {
                if (_powerCell[i].GetBonusLevels() > 0)
                {
                    MenuPowersCell pcell = _powerCell[i].GetCurrent();
                    SharedGameResources.Menu!.Act!.AddPower(pcell.Id, _powerCell[i].GetBonusCurrent(pcell).Id);
                }
            }
        }

        public void ClearBonusLevels()
        {
            ClearActionBarBonusLevels();

            for (int i = 0; i < _powerCell.Count; ++i)
            {
                _powerCell[i].BonusLevels.Clear();
            }
        }

        public void AddBonusLevels(PowerID powerIndex, int bonusLevels)
        {
            MenuPowersCell? pcell = GetCellByPowerIndex(powerIndex);

            if (pcell == null)
                return;

            MenuPowersCellGroup pgroup = _powerCell[pcell.Group];

            int minLevel = pgroup.Cells.Count - 1;
            for (int i = 0; i < pgroup.Cells.Count; ++i)
            {
                if (ReferenceEquals(pcell, pgroup.Cells[i]))
                {
                    minLevel = i;
                    break;
                }
            }

            (int, int) bonus = (minLevel, bonusLevels);
            pgroup.BonusLevels.Add(bonus);
        }

        public string GetItemBonusPowerReqString(PowerID powerIndex)
        {
            MenuPowersCell? pcell = GetCellByPowerIndex(powerIndex);

            if (pcell == null)
                return "";

            string output = SharedGameResources.Powers!.Powers[powerIndex]!.Name;
            if (pcell.UpgradeLevel > 0)
            {
                output += " (" + SharedResources.Msg!.GetV("Level %d", pcell.UpgradeLevel) + ")";
            }

            return output;
        }

        public bool IsTabListSelected()
        {
            return (GetCurrentTabList() != null && (_tabs.Count == 0 || (_tabs.Count > 0 && GetCurrentTabList() != Tablist)));
        }

        public int GetSelectedCellIndex()
        {
            TabList? curTablist = GetCurrentTabList();
            int current = curTablist!.GetCurrent();

            if (_tabs.Count == 0)
            {
                return current;
            }
            else
            {
                WidgetSlot? curSlot = (WidgetSlot?)curTablist.GetWidgetByIndex(current);

                for (int i = 0; i < Slots.Count; ++i)
                {
                    if (ReferenceEquals(Slots[i], curSlot))
                        return i;
                }

                return 0;
            }
        }

        public void SetNextTabList(TabList? tl)
        {
            if (_tabs.Count > 0)
            {
                for (int i = 0; i < _tabs.Count; ++i)
                {
                    TablistPow[i].SetNextTabList(tl);
                }
            }
        }

        public override TabList? GetCurrentTabList()
        {
            if (Tablist.GetCurrent() != -1)
            {
                return Tablist;
            }
            else if (_tabs.Count > 0)
            {
                for (int i = 0; i < _tabs.Count; ++i)
                {
                    if (TablistPow[i].GetCurrent() != -1)
                        return TablistPow[i];
                }
            }

            return null;
        }

        public override void DefocusTabLists()
        {
            Tablist.Defocus();

            if (_tabs.Count > 0)
            {
                for (int i = 0; i < _tabs.Count; ++i)
                {
                    TablistPow[i].Defocus();
                }
            }
        }
    }
}
