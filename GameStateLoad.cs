// 对应 C++ 源：GameStateLoad.h + GameStateLoad.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using System.Text;
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// 存档槽位数据：对应 C++ 的 <c>class GameSlot</c>。
    /// 嵌入的 <see cref="WidgetLabel"/>、<see cref="GameSlotPreview"/>、<see cref="StatBlock"/>
    /// 在 C++ 中由成员析构函数自动释放；C# 通过 <see cref="Dispose"/> 复现相同释放顺序。
    /// </summary>
    public class GameSlot : IDisposable
    {
        public int Id;

        public StatBlock Stats = new StatBlock();
        public string CurrentMap = "";
        public ulong TimePlayed;

        public List<ItemID> Equipped = new List<ItemID>();
        public List<ItemID> ExtendedItems = new List<ItemID>();
        public int ActiveEquipmentSet;
        public GameSlotPreview Preview = new GameSlotPreview();
        public Timer PreviewTurnTimer;

        public WidgetLabel LabelName = new WidgetLabel();
        public WidgetLabel LabelLevel = new WidgetLabel();
        public WidgetLabel LabelClass = new WidgetLabel();
        public WidgetLabel LabelMap = new WidgetLabel();
        public WidgetLabel LabelSlotNumber = new WidgetLabel();

        public GameSlot()
        {
            Id = 0;
            TimePlayed = 0;
            ActiveEquipmentSet = 0;
            PreviewTurnTimer = new Timer((uint)(SharedResources.Settings!.MaxFramesPerSec / 2));
            PreviewTurnTimer.Reset(Timer.Begin);
        }

        /// <summary>对应 C++ 空析构函数；成员子对象析构由 <see cref="Dispose"/> 显式触发。</summary>
        public void Dispose()
        {
            Preview.Dispose();
            LabelName.Dispose();
            LabelLevel.Dispose();
            LabelClass.Dispose();
            LabelMap.Dispose();
            LabelSlotNumber.Dispose();
            Stats.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// GameStateLoad
    ///
    /// 显示当前存档槽、允许继续/新建/删除存档。
    /// 前向引用类型（尚未转换）：<see cref="GameStateNew"/>、<see cref="GameStateTitle"/>、
    /// <see cref="GameStatePlay"/>、<see cref="MenuStashTab"/>。
    /// </summary>
    public class GameStateLoad : GameState
    {
        private const int DeleteConfirmOptionNo = 0;
        private const int DeleteConfirmOptionYes = 1;

        private const int NavModeDefault = 0;
        private const int NavModeLoad = 1;
        private const int NavModeDelete = 2;

        private TabList _tablist;

        private WidgetButton? _buttonExit;
        private WidgetButton? _buttonNew;
        private WidgetButton? _buttonLoad;
        private WidgetButton? _buttonDelete;
        private WidgetLabel? _labelLoading;
        private WidgetScrollBar? _scrollbar;

        private MenuConfirm? _confirm;

        private Sprite? _slotsBackground;
        private Sprite? _slotsSelection;
        private Sprite? _portraitBorder;
        private Sprite? _portrait;
        private List<Rectangle> _slotPos = new List<Rectangle>();

        private List<GameSlot?> _gameSlots = new List<GameSlot?>();

        private bool _loadingRequested;
        private bool _loading;
        private bool _loaded;
        private bool _deleteItems;

        private LabelInfo _namePos;
        private LabelInfo _levelPos;
        private LabelInfo _classPos;
        private LabelInfo _mapPos;
        private LabelInfo _slotNumberPos;
        private Int2 _spritesPos;

        private Rectangle _portraitDest;

        private Rectangle _gameslotPos;

        private int _selectedSlot;
        private int _lastSelectedSlot;
        private int _visibleSlots;
        private int _scrollOffset;
        private bool _hasScrollBar;
        private int _gameSlotMax;
        private int _textTrimBoundary;
        private int _portraitAlign;
        private int _gameslotAlign;
        private int _navMode;

        private List<int> _equipSets = new List<int>();

        private List<MenuStashTab> _stashTabs = new List<MenuStashTab>();

        public GameStateLoad()
        {
            Settings settings = SharedResources.Settings!;
            MessageEngine msg = SharedResources.Msg!;
            EngineSettings eset = SharedResources.Eset!;
            RenderDevice renderDevice = SharedResources.RenderDevice!;
            ModManager mods = SharedResources.Mods!;
            ItemManager items = SharedGameResources.Items!;
            SaveLoad saveLoad = SharedResources.SaveLoad!;

            _slotsBackground = null;
            _slotsSelection = null;
            _portraitBorder = null;
            _portrait = null;
            _loadingRequested = false;
            _loading = false;
            _loaded = false;
            _deleteItems = true;
            _selectedSlot = -1;
            _lastSelectedSlot = -1;
            _visibleSlots = 0;
            _scrollOffset = 0;
            _hasScrollBar = false;
            _gameSlotMax = 4;
            _textTrimBoundary = 0;
            _portraitAlign = Utils.AlignFrameTopLeft;
            _gameslotAlign = Utils.AlignFrameTopLeft;
            _navMode = NavModeDefault;

            if (SharedGameResources.Items == null)
                SharedGameResources.Items = new ItemManager();
            items = SharedGameResources.Items!;

            _labelLoading = new WidgetLabel();

            // Confirmation box to confirm deleting
            _confirm = new MenuConfirm();
            _confirm.SetTitle(msg.Get("Delete this save?"));
            _confirm.ActionList!.Append(msg.Get("No"), "");
            _confirm.ActionList.Append(msg.Get("Yes"), "");

            _buttonExit = new WidgetButton(WidgetButton.DefaultFile);
            _buttonExit.SetLabel(msg.Get("Exit to Title"));

            _buttonNew = new WidgetButton(WidgetButton.DefaultFile);
            _buttonNew.SetLabel(msg.Get("New Game"));
            _buttonNew.Enabled = true;

            _buttonLoad = new WidgetButton(WidgetButton.DefaultFile);
            _buttonLoad.SetLabel(msg.Get("Choose a Slot"));
            _buttonLoad.Enabled = false;

            _buttonDelete = new WidgetButton(WidgetButton.DefaultFile);
            _buttonDelete.SetLabel(msg.Get("Delete Save"));
            _buttonDelete.Enabled = false;

            _scrollbar = new WidgetScrollBar(WidgetScrollBar.DefaultFile);

            // Set up tab list
            _tablist = new TabList();
            _tablist.Add(_buttonExit);
            _tablist.Add(_buttonNew);

            // Some widgets default to being aligned to the menu frame
            _buttonNew.Alignment = Utils.AlignFrameTopLeft;
            _buttonLoad!.Alignment = Utils.AlignFrameTopLeft;
            _buttonDelete!.Alignment = Utils.AlignFrameTopLeft;

            // Read positions from config file
            FileParser infile = new FileParser();

            // @CLASS GameStateLoad|Description of menus/gameload.txt
            if (infile.Open("menus/gameload.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    // @ATTR button_new|int, int, alignment : X, Y, Alignment|Position of the "New Game" button.
                    if (infile.Key == "button_new")
                    {
                        int x = Parse.PopFirstInt(ref infile.Val);
                        int y = Parse.PopFirstInt(ref infile.Val);
                        int a = Parse.ToAlignment(Parse.PopFirstString(ref infile.Val), Utils.AlignFrameTopLeft);
                        _buttonNew!.SetBasePos(x, y, a);
                    }
                    // @ATTR button_load|int, int, alignment : X, Y, Alignment|Position of the "Load Game" button.
                    else if (infile.Key == "button_load")
                    {
                        int x = Parse.PopFirstInt(ref infile.Val);
                        int y = Parse.PopFirstInt(ref infile.Val);
                        int a = Parse.ToAlignment(Parse.PopFirstString(ref infile.Val), Utils.AlignFrameTopLeft);
                        _buttonLoad!.SetBasePos(x, y, a);
                    }
                    // @ATTR button_delete|int, int, alignment : X, Y, Alignment|Position of the "Delete Save" button.
                    else if (infile.Key == "button_delete")
                    {
                        int x = Parse.PopFirstInt(ref infile.Val);
                        int y = Parse.PopFirstInt(ref infile.Val);
                        int a = Parse.ToAlignment(Parse.PopFirstString(ref infile.Val), Utils.AlignFrameTopLeft);
                        _buttonDelete!.SetBasePos(x, y, a);
                    }
                    // @ATTR button_exit|int, int, alignment : X, Y, Alignment|Position of the "Exit to Title" button.
                    else if (infile.Key == "button_exit")
                    {
                        int x = Parse.PopFirstInt(ref infile.Val);
                        int y = Parse.PopFirstInt(ref infile.Val);
                        int a = Parse.ToAlignment(Parse.PopFirstString(ref infile.Val));
                        _buttonExit!.SetBasePos(x, y, a);
                    }
                    // @ATTR portrait|rectangle|Position and dimensions of the portrait image.
                    else if (infile.Key == "portrait")
                    {
                        _portraitDest = Parse.ToRect(infile.Val);
                    }
                    // @ATTR gameslot|rectangle|Position and dimensions of the first game slot.
                    else if (infile.Key == "gameslot")
                    {
                        _gameslotPos = Parse.ToRect(infile.Val);
                    }
                    // @ATTR name|label|The label for the hero's name. Position is relative to game slot position.
                    else if (infile.Key == "name")
                    {
                        _namePos = Parse.PopLabelInfo(infile.Val);
                    }
                    // @ATTR level|label|The label for the hero's level. Position is relative to game slot position.
                    else if (infile.Key == "level")
                    {
                        _levelPos = Parse.PopLabelInfo(infile.Val);
                    }
                    // @ATTR class|label|The label for the hero's class. Position is relative to game slot position.
                    else if (infile.Key == "class")
                    {
                        _classPos = Parse.PopLabelInfo(infile.Val);
                    }
                    // @ATTR map|label|The label for the hero's current location. Position is relative to game slot position.
                    else if (infile.Key == "map")
                    {
                        _mapPos = Parse.PopLabelInfo(infile.Val);
                    }
                    // @ATTR slot_number|label|The label for the save slot index. Position is relative to game slot position.
                    else if (infile.Key == "slot_number")
                    {
                        _slotNumberPos = Parse.PopLabelInfo(infile.Val);
                    }
                    // @ATTR loading_label|label|The label for the "Entering game world..."/"Loading saved game..." text.
                    else if (infile.Key == "loading_label")
                    {
                        _labelLoading!.SetFromLabelInfo(Parse.PopLabelInfo(infile.Val));
                    }
                    // @ATTR sprite|point|Position for the avatar preview image in each slot
                    else if (infile.Key == "sprite")
                    {
                        _spritesPos = Parse.ToPoint(infile.Val);
                    }
                    // @ATTR visible_slots|int|The maximum numbers of visible save slots.
                    else if (infile.Key == "visible_slots")
                    {
                        _gameSlotMax = Parse.ToInt(infile.Val);

                        // can't have less than 1 game slot visible
                        _gameSlotMax = Math.Max(_gameSlotMax, 1);
                    }
                    // @ATTR text_trim_boundary|int|The position of the right-side boundary where text will be shortened with an ellipsis. Position is relative to game slot position.
                    else if (infile.Key == "text_trim_boundary")
                    {
                        _textTrimBoundary = Parse.ToInt(infile.Val);
                    }
                    // @ATTR show_frame_background|bool|If true, the frame background image is drawn behind the menu.
                    else if (infile.Key == "show_frame_background")
                    {
                        HasFrameBackground = Parse.ToBool(infile.Val);
                    }
                    else
                    {
                        infile.Error("GameStateLoad: '%s' is not a valid key.", infile.Key);
                    }
                }
                infile.Close();
            }

            // We need to read the inventory menu config to properly handle equipment sets
            if (infile.Open("menus/inventory.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (infile.Key == "equipment_slot")
                    {
                        // we don't need the first 3 parameters, so pop them off
                        Parse.PopFirstInt(ref infile.Val);
                        Parse.PopFirstInt(ref infile.Val);
                        Parse.PopFirstString(ref infile.Val);

                        int eqSet = Parse.PopFirstInt(ref infile.Val);
                        _equipSets.Add(eqSet);
                    }
                }
                infile.Close();
            }

            // We need to read the stash menu config in order to get the paths to private stashes
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
                            idStr.Append("Stash ").Append(_stashTabs.Count);
                            nameStr.Append(msg.Get("Stash")).Append(" ").Append(_stashTabs.Count);
                            filenameStr.Append("stash_tab_").Append(_stashTabs.Count).Append(".txt");

                            _stashTabs.Add(new MenuStashTab(idStr.ToString(), nameStr.ToString(), filenameStr.ToString(), MenuStashTab.IsPrivateConst));
                        }
                    }

                    if (infile.Section == "tab")
                    {
                        // don't load any settings for the "Private" and "Shared" tabs
                        if (_stashTabs[^1].IsLegacy)
                            continue;

                        if (infile.Key == "name")
                        {
                            string nameStr = infile.Val;
                            _stashTabs[^1].Id = nameStr;
                            _stashTabs[^1].Name = msg.Get(nameStr);

                            if (nameStr == "Private")
                            {
                                _stashTabs[^1].Filename = "stash_HC.txt";
                                _stashTabs[^1].IsPrivate = true;
                                _stashTabs[^1].IsLegacy = true;
                            }
                            else if (nameStr == "Shared")
                            {
                                _stashTabs[^1].Filename = "stash.txt";
                                _stashTabs[^1].IsPrivate = false;
                                _stashTabs[^1].IsLegacy = true;
                            }
                            else
                            {
                                // generate a filename
                                StringBuilder ss = new StringBuilder();
                                ss.Append("stash_").Append(Utils.HashString(nameStr).ToString("x")).Append(".txt");
                                _stashTabs[^1].Filename = ss.ToString();
                            }
                        }
                        else if (infile.Key == "is_private")
                        {
                            _stashTabs[^1].IsPrivate = Parse.ToBool(infile.Val);
                        }
                    }
                }
                infile.Close();
            }

            if (_stashTabs.Count == 0)
            {
                _stashTabs.Add(new MenuStashTab("Private", msg.Get("Private"), "stash_HC.txt", MenuStashTab.IsPrivateConst));
                // no need to add "Shared" tab, since we're only checking private stashes for extended items
            }

            // prevent text from overflowing on the right edge of game slots
            if (_textTrimBoundary == 0 || _textTrimBoundary > _gameslotPos.Width)
                _textTrimBoundary = _gameslotPos.Width;

            _buttonNew!.Refresh();
            _buttonLoad!.Refresh();
            _buttonDelete!.Refresh();

            LoadGraphics();
            ReadGameSlots();
            RefreshSavePaths();

            RefreshWidgets();
            UpdateButtons();

            // if we specified a slot to load at launch, load it now
            if (settings.LoadSlot.Length != 0)
            {
                int loadSlotId = Parse.ToInt(settings.LoadSlot) - 1;
                settings.LoadSlot = "";

                if (loadSlotId < _gameSlots.Count)
                {
                    SetSelectedSlot(loadSlotId);
                    _loadingRequested = true;
                }
            }
            else if (settings.PrevSaveSlot >= 0 && settings.PrevSaveSlot < _gameSlots.Count)
            {
                SetSelectedSlot(settings.PrevSaveSlot);
                ScrollToSelected();
                UpdateButtons();
            }
            else if (_gameSlots.Count != 0)
            {
                SetSelectedSlot(0);
                ScrollToSelected();
                UpdateButtons();
            }

            renderDevice.SetBackgroundColor(new Color(0, 0, 0, 0));
        }

        private void LoadGraphics()
        {
            RenderDevice renderDevice = SharedResources.RenderDevice!;
            Image? graphics;

            graphics = renderDevice.LoadImage("images/menus/game_slots.png", RenderDevice.ErrorNormal);
            if (graphics != null)
            {
                _slotsBackground = graphics.CreateSprite();
                graphics.Unref();
            }

            graphics = renderDevice.LoadImage("images/menus/game_slot_select.png", RenderDevice.ErrorNormal);
            if (graphics != null)
            {
                _slotsSelection = graphics.CreateSprite();
                graphics.Unref();
            }

            graphics = renderDevice.LoadImage("images/menus/portrait_border.png", RenderDevice.ErrorNormal);
            if (graphics != null)
            {
                _portraitBorder = graphics.CreateSprite();
                _portraitBorder.SetClip(0, 0, _portraitDest.Width, _portraitDest.Height);
                graphics.Unref();
            }
        }

        private void LoadPortrait(int slot)
        {
            RenderDevice renderDevice = SharedResources.RenderDevice!;
            Image? graphics;

            if (_portrait != null)
            {
                _portrait.Dispose();
                _portrait = null;
            }

            if (slot < 0 || slot >= _gameSlots.Count || _gameSlots[slot] == null)
                return;

            if (_gameSlots[slot]!.Stats.Name == "")
                return;

            graphics = renderDevice.LoadImage(_gameSlots[slot]!.Stats.GfxPortrait, RenderDevice.ErrorNormal);
            if (graphics != null)
            {
                _portrait = graphics.CreateSprite();
                _portrait.SetClip(0, 0, _portraitDest.Width, _portraitDest.Height);
                graphics.Unref();
            }
        }

        private void ReadGameSlots()
        {
            FileParser infile = new FileParser();
            Settings settings = SharedResources.Settings!;
            EngineSettings eset = SharedResources.Eset!;
            ItemManager items = SharedGameResources.Items!;
            
            string saveRoot = settings.PathUser + "saves/" + eset.Misc.SavePrefix + "/";
            List<string> saveDirs = new List<string>();

            Filesystem.GetDirList(settings.PathUser + "saves/" + eset.Misc.SavePrefix, saveDirs);
            saveDirs.Sort(GameStateLoadCompare.CompareSaveDirs);

            // save dirs can only be >= 1
            for (int i = saveDirs.Count; i > 0; --i)
            {
                if (Parse.ToInt(saveDirs[i - 1]) < 1)
                    saveDirs.RemoveAt(i - 1);
            }

            _gameSlots.Clear();
            for (int i = 0; i < saveDirs.Count; ++i)
                _gameSlots.Add(null);

            _visibleSlots = (_gameSlotMax > _gameSlots.Count ? _gameSlots.Count : _gameSlotMax);

            for (int i = 0; i < saveDirs.Count; ++i)
            {
                // save data is stored in slot#/avatar.txt
                string filename = saveRoot + saveDirs[i] + "/avatar.txt";

                if (!infile.Open(filename, !FileParser.ModFile, FileParser.ErrorNormal))
                    continue;

                _gameSlots[i] = new GameSlot();
                _gameSlots[i]!.Id = Parse.ToInt(saveDirs[i]);
                _gameSlots[i]!.Stats.Hero = true;
                _gameSlots[i]!.LabelName.SetFromLabelInfo(_namePos);
                _gameSlots[i]!.LabelLevel.SetFromLabelInfo(_levelPos);
                _gameSlots[i]!.LabelClass.SetFromLabelInfo(_classPos);
                _gameSlots[i]!.LabelMap.SetFromLabelInfo(_mapPos);
                _gameSlots[i]!.LabelSlotNumber.SetFromLabelInfo(_slotNumberPos);

                while (infile.Next())
                {
                    // load (key=value) pairs
                    if (infile.Key == "name")
                        _gameSlots[i]!.Stats.Name = infile.Val;
                    else if (infile.Key == "class")
                    {
                        _gameSlots[i]!.Stats.CharacterClass = Parse.PopFirstString(ref infile.Val);
                        _gameSlots[i]!.Stats.CharacterSubclass = Parse.PopFirstString(ref infile.Val);
                    }
                    else if (infile.Key == "xp")
                        _gameSlots[i]!.Stats.Xp = (ulong)Parse.ToInt(infile.Val);
                    else if (infile.Key == "build")
                    {
                        for (int j = 0; j < eset.PrimaryStats.Stats.Count; ++j)
                        {
                            _gameSlots[i]!.Stats.Primary[j] = Parse.PopFirstInt(ref infile.Val);
                        }
                    }
                    else if (infile.Key == "equipped")
                    {
                        string repeatVal = Parse.PopFirstString(ref infile.Val);
                        while (repeatVal != "")
                        {
                            ItemID itemId = items.VerifyID(Parse.ToItemID(repeatVal), infile, ItemManager.VerifyAllowZero, ItemManager.VerifyAllocate);
                            _gameSlots[i]!.Equipped.Add(itemId);
                            if (itemId != 0 && items.Items[(int)itemId]!.Parent != 0)
                            {
                                _gameSlots[i]!.ExtendedItems.Add(itemId);
                            }
                            repeatVal = Parse.PopFirstString(ref infile.Val);
                        }
                    }
                    else if (infile.Key == "carried")
                    {
                        string repeatVal = Parse.PopFirstString(ref infile.Val);
                        while (repeatVal != "")
                        {
                            ItemID itemId = items.VerifyID(Parse.ToItemID(repeatVal), infile, ItemManager.VerifyAllowZero, ItemManager.VerifyAllocate);
                            if (itemId != 0 && items.Items[(int)itemId]!.Parent != 0)
                            {
                                _gameSlots[i]!.ExtendedItems.Add(itemId);
                            }
                            repeatVal = Parse.PopFirstString(ref infile.Val);
                        }
                    }
                    else if (infile.Key == "active_equipment_set")
                    {
                        _gameSlots[i]!.ActiveEquipmentSet = Parse.ToInt(infile.Val);

                        if (_gameSlots[i]!.ActiveEquipmentSet > 0 && !_equipSets.Contains(_gameSlots[i]!.ActiveEquipmentSet))
                        {
                            Utils.LogError("GameStateLoad: Save slot {0} has an invalid active equipment set. Resetting to 0.", _gameSlots[i]!.Id);
                            _gameSlots[i]!.ActiveEquipmentSet = 0;
                        }
                    }
                    else if (infile.Key == "option")
                    {
                        _gameSlots[i]!.Stats.GfxBase = Parse.PopFirstString(ref infile.Val);
                        _gameSlots[i]!.Stats.GfxHead = Parse.PopFirstString(ref infile.Val);
                        _gameSlots[i]!.Stats.GfxPortrait = Parse.PopFirstString(ref infile.Val);

                        _gameSlots[i]!.Stats.CheckGfxPaths();
                    }
                    else if (infile.Key == "spawn")
                    {
                        _gameSlots[i]!.CurrentMap = GetMapName(Parse.PopFirstString(ref infile.Val));
                    }
                    else if (infile.Key == "permadeath")
                    {
                        _gameSlots[i]!.Stats.Permadeath = Parse.ToBool(infile.Val);
                    }
                    else if (infile.Key == "time_played")
                    {
                        _gameSlots[i]!.TimePlayed = Parse.ToUnsignedLong(infile.Val);
                    }
                }
                infile.Close();

                // search private stashes for extended items
                for (int j = 0; j < _stashTabs.Count; ++j)
                {
                    if (!_stashTabs[j].IsPrivate)
                        continue;

                    string stashFilename = saveRoot + saveDirs[i] + "/" + _stashTabs[j].Filename;

                    if (!infile.Open(stashFilename, !FileParser.ModFile, FileParser.ErrorNormal))
                        continue;

                    while (infile.Next())
                    {
                        if (infile.Key == "item")
                        {
                            string repeatVal = Parse.PopFirstString(ref infile.Val);
                            while (repeatVal != "")
                            {
                                ItemID itemId = items.VerifyID(Parse.ToItemID(repeatVal), infile, ItemManager.VerifyAllowZero, ItemManager.VerifyAllocate);
                                if (itemId != 0 && items.Items[(int)itemId]!.Parent != 0)
                                {
                                    _gameSlots[i]!.ExtendedItems.Add(itemId);
                                }
                                repeatVal = Parse.PopFirstString(ref infile.Val);
                            }
                        }
                    }
                    infile.Close();
                }

                _gameSlots[i]!.Stats.Recalc();
                _gameSlots[i]!.Stats.Direction = 6;
                _gameSlots[i]!.Preview.SetStatBlock(_gameSlots[i]!.Stats);

                LoadPreview(_gameSlots[i]!);
            }
        }

        private string GetMapName(string mapFilename)
        {
            FileParser infile = new FileParser();
            MessageEngine msg = SharedResources.Msg!;
            if (!infile.Open(mapFilename, FileParser.ModFile, FileParser.ErrorNone))
                return "";

            string mapName = "";

            while (mapName == "" && infile.Next())
            {
                if (infile.Key == "title")
                    mapName = msg.Get(infile.Val);
            }

            infile.Close();
            return mapName;
        }

        private void LoadPreview(GameSlot slot)
        {
            if (slot == null)
                return;

            ItemManager items = SharedGameResources.Items!;
            List<string> imgGfx = new List<string>();
            List<string> previewLayer = slot.Preview.LayerReferenceOrder;

            // fall back to default if it exists
            for (int i = 0; i < previewLayer.Count; i++)
            {
                bool exists = Filesystem.FileExists(SharedResources.Mods!.Locate("animations/avatar/" + slot.Stats.GfxBase + "/default_" + previewLayer[i] + ".txt"));
                if (exists)
                {
                    imgGfx.Add("default_" + previewLayer[i]);
                }
                else if (previewLayer[i] == "head")
                {
                    imgGfx.Add(slot.Stats.GfxHead);
                }
                else
                {
                    imgGfx.Add("");
                }
            }

            for (int i = 0; i < slot.Equipped.Count; i++)
            {
                if (slot.Equipped[i] <= 0)
                    continue;

                if (i >= _equipSets.Count)
                {
                    Utils.LogError("GameStateLoad: Item in save slot {0} with id={1} has an invalid position. Your savegame is broken or you might be using an incompatible savegame/mod", slot.Id, slot.Equipped[i]);
                    break;
                }

                if (!items.IsValid(slot.Equipped[i]) || !items.Items[(int)slot.Equipped[i]]!.HasName)
                {
                    Utils.LogError("GameStateLoad: Item in save slot {0} with id={1} is unknown. Your savegame is broken or you might be using an incompatible savegame/mod", slot.Id, slot.Equipped[i]);
                    continue;
                }

                if ((slot.ActiveEquipmentSet == 0 || _equipSets[i] == 0 || slot.ActiveEquipmentSet == _equipSets[i]) && previewLayer.Count != 0)
                {
                    int found = previewLayer.IndexOf(items.GetItemType(items.Items[(int)slot.Equipped[i]]!.Type).Id);
                    if (found != -1)
                        imgGfx[found] = items.Items[(int)slot.Equipped[i]]!.Gfx;
                }
            }

            slot.Preview.LoadGraphics(imgGfx);
        }

        public override void Logic()
        {
            InputState inpt = SharedResources.Inpt!;
            MessageEngine msg = SharedResources.Msg!;
            Settings settings = SharedResources.Settings!;
            ItemManager items = SharedGameResources.Items!;
            SaveLoad saveLoad = SharedResources.SaveLoad!;

            if (inpt.WindowResized)
                RefreshWidgets();

            for (int i = 0; i < _gameSlots.Count; ++i)
            {
                if (_gameSlots[i] == null)
                    continue;

                if (i == _selectedSlot)
                {
                    _gameSlots[i]!.PreviewTurnTimer.Tick();

                    if (_gameSlots[i]!.PreviewTurnTimer.IsEnd())
                    {
                        _gameSlots[i]!.PreviewTurnTimer.Reset(Timer.Begin);

                        _gameSlots[i]!.Stats.Direction++;
                        if (_gameSlots[i]!.Stats.Direction > 7)
                            _gameSlots[i]!.Stats.Direction = 0;
                    }
                }
                _gameSlots[i]!.Preview.Logic();
            }

            if (inpt.UsingMouse() && _navMode != NavModeDefault)
            {
                _navMode = NavModeDefault;
                UpdateButtons();
            }

            if (_selectedSlot == -1 || _selectedSlot >= _gameSlots.Count)
            {
                if (_lastSelectedSlot != -1)
                    SetSelectedSlot(_lastSelectedSlot);
                else
                    SetSelectedSlot(0);

                UpdateButtons();
            }

            if (_confirm!.Visible)
            {
                _confirm.Logic();
                if (_confirm.ClickedConfirm)
                {
                    if (_confirm.ActionList!.GetSelected() == DeleteConfirmOptionYes)
                    {
                        if (_selectedSlot != -1 && _selectedSlot < _gameSlots.Count)
                        {
                            // mark extended items associated with *only* this save as not foreign
                            // then save the extended items data for *only* foreign items
                            // this will effectively clean up extended item IDs that were being used by this save file
                            for (int i = 0; i < _gameSlots[_selectedSlot]!.ExtendedItems.Count; ++i)
                            {
                                ItemID itemId = _gameSlots[_selectedSlot]!.ExtendedItems[i];
                                items.Items[(int)itemId]!.IsForeign = false;
                            }
                            saveLoad.SaveExtendedItems(!SaveLoad.SaveStorageItems);

                            Utils.RemoveSaveDir(_gameSlots[_selectedSlot]!.Id);

                            _gameSlots[_selectedSlot]!.Dispose();
                            _gameSlots.RemoveAt(_selectedSlot);

                            _visibleSlots = (_gameSlotMax > _gameSlots.Count ? _gameSlots.Count : _gameSlotMax);
                            if (_gameSlots.Count != 0)
                            {
                                if (_selectedSlot > 0)
                                    SetSelectedSlot(_selectedSlot - 1);
                                else
                                    SetSelectedSlot(0);
                            }
                            else
                            {
                                SetSelectedSlot(-1);
                            }

                            while (_scrollOffset + _visibleSlots > _gameSlots.Count)
                            {
                                _scrollOffset--;
                            }

                            ScrollToSelected();
                            UpdateButtons();

                            RefreshSavePaths();
                            settings.PrevSaveSlot = -1;
                        }
                        _tablist.Defocus();
                    }
                    else if (_confirm.ActionList!.GetSelected() == DeleteConfirmOptionNo)
                    {
                        // do nothing, since the window will be closed anyway
                    }
                    else
                    {
                        // We shouldn't end up here!
                        // But if we do, log an error and try to reset the UI

                        Utils.LogError("GameStateLoad: Can't delete save. Index at {0} is out-of-bounds.", _selectedSlot);

                        if (_gameSlots.Count != 0)
                            SetSelectedSlot(0);
                        else
                            SetSelectedSlot(-1);

                        ScrollToSelected();
                        UpdateButtons();
                        settings.PrevSaveSlot = -1;
                        _tablist.Defocus();
                    }

                    // both yes and no close the dialog
                    _confirm.Visible = false;
                    _confirm.ClickedConfirm = false;
                }
            }
            else
            {
                if (_navMode == NavModeDefault)
                {
                    if (!inpt.UsingMouse() && !_loadingRequested)
                        SetSelectedSlot(-1);

                    _tablist.Logic();

                    if (!inpt.UsingMouse() && _tablist.GetCurrent() == -1)
                    {
                        if (_buttonLoad!.Enabled)
                            _tablist.SetCurrent(_buttonLoad);
                        else if (_buttonNew!.Enabled)
                            _tablist.SetCurrent(_buttonNew);
                        else
                            _tablist.SetCurrent(_buttonExit);
                    }

                    if (_buttonExit!.CheckClick() || (inpt.Pressing[Input.Cancel] && !inpt.Lock[Input.Cancel]))
                    {
                        inpt.Lock[Input.Cancel] = true;
                        ShowLoading();
                        SetRequestedGameState(new GameStateTitle());
                    }
                }

                if (_loadingRequested)
                {
                    _loading = true;
                    _loadingRequested = false;
                    LogicLoading();
                }

                if (_buttonNew!.CheckClick())
                {
                    // create a new game
                    ShowLoading();
                    GameStateNew newgame = new GameStateNew();
                    newgame.GameSlot = (_gameSlots.Count == 0 ? 1 : _gameSlots[^1]!.Id + 1);
                    _deleteItems = false;
                    SetRequestedGameState(newgame);
                }
                else if (_buttonLoad!.CheckClick())
                {
                    if (!inpt.UsingMouse())
                    {
                        _navMode = NavModeLoad;
                        SetSelectedSlot(_lastSelectedSlot);
                        UpdateButtons();
                    }
                    else
                    {
                        _loadingRequested = true;
                    }
                }
                else if (_buttonDelete!.CheckClick())
                {
                    if (!inpt.UsingMouse())
                    {
                        _navMode = NavModeDelete;
                        SetSelectedSlot(_lastSelectedSlot);
                        UpdateButtons();
                    }
                    else
                    {
                        // Display pop-up to make sure save should be deleted
                        _confirm.Show();
                    }
                }
                else if (_gameSlots.Count > 0)
                {
                    Rectangle scrollArea = _slotPos[0];
                    scrollArea.Height = _slotPos[0].Height * _gameSlotMax;

                    if (Utils.IsWithinRect(scrollArea, inpt.Mouse))
                    {
                        if (inpt.Pressing[Input.Main1] && !inpt.Lock[Input.Main1])
                        {
                            for (int i = 0; i < _visibleSlots; ++i)
                            {
                                if (Utils.IsWithinRect(_slotPos[i], inpt.Mouse))
                                {
                                    inpt.Lock[Input.Main1] = true;
                                    SetSelectedSlot(i + _scrollOffset);
                                    UpdateButtons();
                                    break;
                                }
                            }
                        }
                        else if (inpt.ScrollUp)
                        {
                            ScrollUp();
                        }
                        else if (inpt.ScrollDown)
                        {
                            ScrollDown();
                        }
                    }
                    else if (_hasScrollBar)
                    {
                        switch (_scrollbar!.CheckClick())
                        {
                            case WidgetScrollBar.ClickUp:
                                ScrollUp();
                                break;
                            case WidgetScrollBar.ClickDown:
                                ScrollDown();
                                break;
                            case WidgetScrollBar.ClickKnob:
                                _scrollOffset = _scrollbar.Value;
                                if (_scrollOffset >= _gameSlots.Count - _visibleSlots)
                                {
                                    _scrollOffset = _gameSlots.Count - _visibleSlots;
                                }
                                break;
                            default:
                                break;
                        }
                    }

                    if (inpt.UsingMouse() || _navMode != NavModeDefault)
                    {
                        // Allow characters to be navigateable via up/down keys
                        if (inpt.Pressing[Input.Up] && !inpt.Lock[Input.Up] && _selectedSlot > 0)
                        {
                            inpt.Lock[Input.Up] = true;
                            SetSelectedSlot(_selectedSlot - 1);
                            ScrollToSelected();
                            UpdateButtons();
                        }
                        else if (inpt.Pressing[Input.Down] && !inpt.Lock[Input.Down] && _selectedSlot < _gameSlots.Count - 1)
                        {
                            inpt.Lock[Input.Down] = true;
                            SetSelectedSlot(_selectedSlot + 1);
                            ScrollToSelected();
                            UpdateButtons();
                        }
                    }

                    if (_navMode != NavModeDefault)
                    {
                        if (inpt.Pressing[Input.Accept] && !inpt.Lock[Input.Accept])
                        {
                            inpt.Pressing[Input.Accept] = true;

                            if (_navMode == NavModeLoad)
                                _loadingRequested = true;
                            else if (_navMode == NavModeDelete)
                                _confirm.Show();

                            _navMode = NavModeDefault;
                            UpdateButtons();
                        }
                        else if (inpt.Pressing[Input.Cancel] && !inpt.Lock[Input.Cancel])
                        {
                            inpt.Lock[Input.Cancel] = true;

                            _navMode = NavModeDefault;
                            UpdateButtons();
                        }
                    }
                }
            }
        }

        private void LogicLoading()
        {
            InputState inpt = SharedResources.Inpt!;
            SaveLoad saveLoad = SharedResources.SaveLoad!;

            // load an existing game
            inpt.LockAll = true;
            _deleteItems = false;
            ShowLoading();
            GameStatePlay play = new GameStatePlay();
            play.ResetGame();
            saveLoad.GameSlot = _gameSlots[_selectedSlot]!.Id;
            saveLoad.LoadGame();
            _loaded = true;
            _loading = false;
            SetRequestedGameState(play);
        }

        private void UpdateButtons()
        {
            MessageEngine msg = SharedResources.Msg!;
            ModManager mods = SharedResources.Mods!;

            LoadPortrait(_selectedSlot);

            if (_navMode == NavModeDefault)
            {
                _buttonNew!.Enabled = true;
                _buttonExit!.Enabled = true;
            }

            // check status of New Game button
            if (!Filesystem.FileExists(mods.Locate("maps/spawn.txt")))
            {
                _buttonNew!.Enabled = false;
                _tablist.Remove(_buttonNew);
                _buttonNew.Tooltip = msg.Get("Enable a story mod to continue");
            }

            if (_selectedSlot >= 0 && _selectedSlot < _gameSlots.Count && _gameSlots[_selectedSlot] != null)
            {
                // slot selected: we can load/delete
                if (_buttonLoad!.Enabled == false)
                {
                    _buttonLoad.Enabled = true;
                    _tablist.Add(_buttonLoad);
                }
                _buttonLoad.Tooltip = "";

                if (_buttonDelete!.Enabled == false)
                {
                    _buttonDelete.Enabled = true;
                    _tablist.Add(_buttonDelete);
                }

                _buttonLoad.SetLabel(msg.Get("Load Game"));
                if (_gameSlots[_selectedSlot]!.CurrentMap == "")
                {
                    if (!Filesystem.FileExists(mods.Locate("maps/spawn.txt")))
                    {
                        _buttonLoad.Enabled = false;
                        _tablist.Remove(_buttonLoad);
                        _buttonLoad.Tooltip = msg.Get("Enable a story mod to continue");
                    }
                }
            }
            else
            {
                // no slot selected: can't load/delete
                _buttonLoad!.SetLabel(msg.Get("Choose a Slot"));
                _buttonLoad.Enabled = false;
                _tablist.Remove(_buttonLoad);

                _buttonDelete!.Enabled = false;
                _tablist.Remove(_buttonDelete);
            }

            if (_navMode == NavModeLoad)
            {
                _tablist.Defocus();
                _buttonDelete!.Enabled = false;
                _buttonNew!.Enabled = false;
                _buttonExit!.Enabled = false;
            }
            else if (_navMode == NavModeDelete)
            {
                _tablist.Defocus();
                _buttonLoad!.Enabled = false;
                _buttonNew!.Enabled = false;
                _buttonExit!.Enabled = false;
            }

            _buttonNew!.Refresh();
            _buttonLoad!.Refresh();
            _buttonDelete!.Refresh();
            _buttonExit!.Refresh();

            RefreshWidgets();
        }

        private void RefreshWidgets()
        {
            _buttonExit!.SetPos(0, 0);
            _buttonNew!.SetPos(0, 0);
            _buttonLoad!.SetPos(0, 0);
            _buttonDelete!.SetPos(0, 0);

            if (_portrait != null)
            {
                Rectangle portraitRect = _portraitDest;
                Utils.AlignToScreenEdge(_portraitAlign, ref portraitRect);
                _portrait.SetDest(portraitRect.X, portraitRect.Y);
            }

            _slotPos.Clear();
            for (int i = 0; i < _visibleSlots; i++)
            {
                Rectangle slotRect = new Rectangle();
                slotRect.X = _gameslotPos.X;
                slotRect.Height = _gameslotPos.Height;
                slotRect.Y = _gameslotPos.Y + (i * _gameslotPos.Height);
                slotRect.Width = _gameslotPos.Width;
                Utils.AlignToScreenEdge(_gameslotAlign, ref slotRect);
                _slotPos.Add(slotRect);
            }

            RefreshScrollBar();
            _confirm!.Align();
        }

        private void ScrollUp()
        {
            if (_scrollOffset > 0)
                _scrollOffset--;

            RefreshScrollBar();
        }

        private void ScrollDown()
        {
            if (_scrollOffset < _gameSlots.Count - _visibleSlots)
                _scrollOffset++;

            RefreshScrollBar();
        }

        private void ScrollToSelected()
        {
            if (_visibleSlots == 0)
                return;

            _scrollOffset = _selectedSlot - (_selectedSlot % _visibleSlots);

            if (_scrollOffset < 0)
                _scrollOffset = 0;
            else if (_scrollOffset > _gameSlots.Count - _visibleSlots)
                _scrollOffset = _selectedSlot - _visibleSlots + 1;
        }

        private void RefreshScrollBar()
        {
            _hasScrollBar = (_gameSlots.Count > _gameSlotMax);

            if (_hasScrollBar)
            {
                Rectangle scrollPos = new Rectangle();
                scrollPos.X = _slotPos[0].X + _slotPos[0].Width;
                scrollPos.Y = _slotPos[0].Y;
                scrollPos.Height = (_slotPos[0].Height * _gameSlotMax);
                _scrollbar!.Refresh(scrollPos.X, scrollPos.Y, scrollPos.Height, _scrollOffset, _gameSlots.Count - _visibleSlots);
            }
        }

        public override void Render()
        {
            RenderDevice renderDevice = SharedResources.RenderDevice!;
            MessageEngine msg = SharedResources.Msg!;
            FontEngine font = SharedResources.Font!;
            Settings settings = SharedResources.Settings!;
            EngineSettings eset = SharedResources.Eset!;

            Rectangle src = new Rectangle();
            Rectangle dest = new Rectangle();

            // portrait
            if (_selectedSlot >= 0 && _portrait != null && _portraitBorder != null)
            {
                renderDevice.Render(_portrait);
                dest.X = _portrait.GetDest().X;
                dest.Y = _portrait.GetDest().Y;
                _portraitBorder.SetDestFromRect(dest);
                renderDevice.Render(_portraitBorder);
            }

            StringBuilder ss = new StringBuilder();

            if (_loadingRequested || _loading || _loaded)
            {
                if (_loaded)
                {
                    _labelLoading!.SetText(msg.Get("Entering game world..."));
                }
                else
                {
                    _labelLoading!.SetText(msg.Get("Loading saved game..."));
                }

                _labelLoading.SetPos((settings.ViewW - eset.Resolutions.FrameW) / 2, (settings.ViewH - eset.Resolutions.FrameH) / 2);
                _labelLoading.SetColor(font.GetColor(FontEngine.ColorMenuNormal));
                _labelLoading.Render();
            }

            // display text
            for (int slot = 0; slot < _visibleSlots; slot++)
            {
                int offSlot = slot + _scrollOffset;

                // slot background
                Int2 slotDest = new Int2();
                if (_slotsBackground != null)
                {
                    src.X = 0;
                    src.Y = (offSlot % 4) * _gameslotPos.Height;
                    dest.Width = dest.Height = 0;

                    src.Width = _gameslotPos.Width;
                    src.Height = _gameslotPos.Height;
                    dest.X = _slotPos[slot].X;
                    dest.Y = _slotPos[slot].Y;

                    _slotsBackground.SetClipFromRect(src);
                    _slotsBackground.SetDestFromRect(dest);
                    renderDevice.Render(_slotsBackground);

                    slotDest = _slotsBackground.GetDest();
                }

                if (_gameSlots[offSlot] == null)
                {
                    WidgetLabel slotError = new WidgetLabel();
                    slotError.SetFromLabelInfo(_namePos);
                    slotError.SetPos(_slotPos[slot].X, _slotPos[slot].Y);
                    slotError.SetText(msg.Get("Invalid save"));
                    slotError.SetColor(font.GetColor(FontEngine.ColorWidgetDisabled));
                    slotError.Render();
                    slotError.Dispose();
                    continue;
                }

                Color nameColor;
                if (_gameSlots[offSlot]!.Stats.Permadeath)
                    nameColor = font.GetColor(FontEngine.ColorHardcoreName);
                else
                    nameColor = font.GetColor(FontEngine.ColorMenuNormal);

                // name
                _gameSlots[offSlot]!.LabelName.SetPos(_slotPos[slot].X, _slotPos[slot].Y);
                _gameSlots[offSlot]!.LabelName.SetText(_gameSlots[offSlot]!.Stats.Name);
                _gameSlots[offSlot]!.LabelName.SetColor(nameColor);

                if (_textTrimBoundary > 0 && _gameSlots[offSlot]!.LabelName.GetBounds().X + _gameSlots[offSlot]!.LabelName.GetBounds().Width >= _textTrimBoundary + slotDest.X)
                    _gameSlots[offSlot]!.LabelName.SetMaxWidth(_textTrimBoundary - (_gameSlots[offSlot]!.LabelName.GetBounds().X - slotDest.X));

                _gameSlots[offSlot]!.LabelName.Render();

                // level
                ss.Clear();
                ss.Append(msg.GetV("Level %d", _gameSlots[offSlot]!.Stats.Level));
                ss.Append(" / ").Append(Utils.GetTimeString(_gameSlots[offSlot]!.TimePlayed));
                if (_gameSlots[offSlot]!.Stats.Permadeath)
                    ss.Append(" / +");

                _gameSlots[offSlot]!.LabelLevel.SetPos(_slotPos[slot].X, _slotPos[slot].Y);
                _gameSlots[offSlot]!.LabelLevel.SetText(ss.ToString());
                _gameSlots[offSlot]!.LabelLevel.SetColor(font.GetColor(FontEngine.ColorMenuNormal));

                if (_textTrimBoundary > 0 && _gameSlots[offSlot]!.LabelLevel.GetBounds().X + _gameSlots[offSlot]!.LabelLevel.GetBounds().Width >= _textTrimBoundary + slotDest.X)
                    _gameSlots[offSlot]!.LabelLevel.SetMaxWidth(_textTrimBoundary - (_gameSlots[offSlot]!.LabelLevel.GetBounds().X - slotDest.X));

                _gameSlots[offSlot]!.LabelLevel.Render();

                // class
                _gameSlots[offSlot]!.LabelClass.SetPos(_slotPos[slot].X, _slotPos[slot].Y);
                _gameSlots[offSlot]!.LabelClass.SetText(_gameSlots[offSlot]!.Stats.GetLongClass());
                _gameSlots[offSlot]!.LabelClass.SetColor(font.GetColor(FontEngine.ColorMenuNormal));

                if (_textTrimBoundary > 0 && _gameSlots[offSlot]!.LabelClass.GetBounds().X + _gameSlots[offSlot]!.LabelClass.GetBounds().Width >= _textTrimBoundary + slotDest.X)
                    _gameSlots[offSlot]!.LabelClass.SetMaxWidth(_textTrimBoundary - (_gameSlots[offSlot]!.LabelClass.GetBounds().X - slotDest.X));

                _gameSlots[offSlot]!.LabelClass.Render();

                // map
                _gameSlots[offSlot]!.LabelMap.SetPos(_slotPos[slot].X, _slotPos[slot].Y);
                _gameSlots[offSlot]!.LabelMap.SetText(_gameSlots[offSlot]!.CurrentMap);
                _gameSlots[offSlot]!.LabelMap.SetColor(font.GetColor(FontEngine.ColorMenuNormal));

                if (_textTrimBoundary > 0 && _gameSlots[offSlot]!.LabelMap.GetBounds().X + _gameSlots[offSlot]!.LabelMap.GetBounds().Width >= _textTrimBoundary + slotDest.X)
                    _gameSlots[offSlot]!.LabelMap.SetMaxWidth(_textTrimBoundary - (_gameSlots[offSlot]!.LabelMap.GetBounds().X - slotDest.X));

                _gameSlots[offSlot]!.LabelMap.Render();

                // render character preview
                dest.X = _slotPos[slot].X + _spritesPos.X;
                dest.Y = _slotPos[slot].Y + _spritesPos.Y;
                _gameSlots[offSlot]!.Preview.SetPos(new Int2(dest.X, dest.Y));
                _gameSlots[offSlot]!.Preview.Render();

                // slot number
                ss.Clear();
                ss.Append(msg.Get("#")).Append(offSlot + 1);

                _gameSlots[offSlot]!.LabelSlotNumber.SetPos(_slotPos[slot].X, _slotPos[slot].Y);
                _gameSlots[offSlot]!.LabelSlotNumber.SetText(ss.ToString());
                _gameSlots[offSlot]!.LabelSlotNumber.SetColor(font.GetColor(FontEngine.ColorMenuNormal));

                if (_textTrimBoundary > 0 && _gameSlots[offSlot]!.LabelSlotNumber.GetBounds().X + _gameSlots[offSlot]!.LabelSlotNumber.GetBounds().Width >= _textTrimBoundary + slotDest.X)
                    _gameSlots[offSlot]!.LabelSlotNumber.SetMaxWidth(_textTrimBoundary - (_gameSlots[offSlot]!.LabelSlotNumber.GetBounds().X - slotDest.X));

                _gameSlots[offSlot]!.LabelSlotNumber.Render();
            }

            // display selection
            if (_selectedSlot >= _scrollOffset && _selectedSlot < _visibleSlots + _scrollOffset && _slotsSelection != null)
            {
                _slotsSelection.SetDestFromRect(_slotPos[_selectedSlot - _scrollOffset]);
                renderDevice.Render(_slotsSelection);
            }

            if (_hasScrollBar)
                _scrollbar!.Render();

            // display buttons
            _buttonExit!.Render();
            _buttonNew!.Render();
            _buttonLoad!.Render();
            _buttonDelete!.Render();

            // display warnings
            if (_confirm!.Visible)
                _confirm.Render();
        }

        private void SetSelectedSlot(int slot)
        {
            if (_selectedSlot != -1)
                _lastSelectedSlot = _selectedSlot;

            if (_selectedSlot != -1 && _selectedSlot < _gameSlots.Count && _gameSlots[_selectedSlot] != null)
            {
                _gameSlots[_selectedSlot]!.Stats.Direction = 6;
                _gameSlots[_selectedSlot]!.PreviewTurnTimer.Reset(Timer.Begin);
                _gameSlots[_selectedSlot]!.Preview.SetAnimation("stance");
            }

            if (slot != -1 && slot < _gameSlots.Count && _gameSlots[slot] != null)
            {
                _gameSlots[slot]!.Stats.Direction = 6;
                _gameSlots[slot]!.PreviewTurnTimer.Reset(Timer.Begin);
                _gameSlots[slot]!.Preview.SetAnimation("run");
            }

            _selectedSlot = slot;
        }

        private void RefreshSavePaths()
        {
            Settings settings = SharedResources.Settings!;
            EngineSettings eset = SharedResources.Eset!;

            for (int i = 0; i < _gameSlots.Count; ++i)
            {
                if (_gameSlots[i] != null && _gameSlots[i]!.Id != i + 1)
                {
                    StringBuilder oldpath = new StringBuilder();
                    StringBuilder newpath = new StringBuilder();
                    oldpath.Append(settings.PathUser).Append("saves/").Append(eset.Misc.SavePrefix).Append("/").Append(_gameSlots[i]!.Id);
                    newpath.Append(settings.PathUser).Append("saves/").Append(eset.Misc.SavePrefix).Append("/").Append(i + 1);
                    if (Filesystem.RenameFile(oldpath.ToString(), newpath.ToString()))
                    {
                        _gameSlots[i]!.Id = i + 1;
                    }
                }
            }
        }

        /// <summary>
        /// 对应 C++ 的 <c>~GameStateLoad()</c>：按原始析构函数逐行顺序释放资源，最后调用基类 <see cref="GameState.Dispose"/>。
        /// </summary>
        public override void Dispose()
        {
            if (_slotsBackground != null)
                _slotsBackground.Dispose();
            if (_slotsSelection != null)
                _slotsSelection.Dispose();
            if (_portraitBorder != null)
                _portraitBorder.Dispose();
            if (_portrait != null)
                _portrait.Dispose();

            _buttonExit?.Dispose();
            _buttonNew?.Dispose();
            _buttonLoad?.Dispose();
            _buttonDelete?.Dispose();

            if (_deleteItems)
            {
                SharedGameResources.Items?.Dispose();
                SharedGameResources.Items = null;
            }

            for (int i = 0; i < _gameSlots.Count; ++i)
            {
                _gameSlots[i]?.Dispose();
            }
            _gameSlots.Clear();

            _labelLoading?.Dispose();
            _scrollbar?.Dispose();
            _confirm?.Dispose();

            base.Dispose();
        }
    }

    /// <summary>对应 C++ 全局函数 <c>compareSaveDirs</c>。</summary>
    internal static class GameStateLoadCompare
    {
        public static int CompareSaveDirs(string dir1, string dir2)
        {
            int first = Parse.ToInt(dir1);
            int second = Parse.ToInt(dir2);

            if (first < second)
                return -1;
            if (first > second)
                return 1;
            return 0;
        }
    }
}
