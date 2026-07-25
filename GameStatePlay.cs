// <自动生成> 对应 C++ 源文件：GameStatePlay.h + GameStatePlay.cpp
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// 玩家称号定义，对应 C++ GameStatePlay 中的 <c>class Title</c>。
    /// </summary>
    public class PlayTitle
    {
        /// <summary>对应 C++ <c>std::string title</c>。/summary>
        public string Title = "";
        public int Level;
        public PowerID Power;
        public List<StatusID> RequiresStatus = [];
        public List<StatusID> RequiresNotStatus = [];
        public string PrimaryStat1 = "";
        public string PrimaryStat2 = "";

        public PlayTitle()
        {
            Title = "";
            Level = 0;
            Power = 0;
            RequiresStatus = [];
            RequiresNotStatus = [];
            PrimaryStat1 = "";
            PrimaryStat2 = "";
        }
    }

    /// <summary>
    /// GameStatePlay - 游戏主循环状态，对应 C++ <c>class GameStatePlay : public GameState</c>。
    /// 持有所有游戏运行时资源（物品/任务/事件/地图/玩家/敌人/NPC 等），通过 SharedGameResources 访问。
    /// </summary>
    public class GameStatePlay : GameState
    {
        private Entity? _enemy;
        private QuestLog? _quests;
        private int _npcId;
        private List<PlayTitle> _titles = [];
        private Timer _secondTimer = new Timer();
        private bool _isFirstMapLoad;

        private const uint UpdateActionbarAll = 0;

        public GameStatePlay()
        {
            _enemy = null;
            _npcId = -1;
            _isFirstMapLoad = true;

            _secondTimer.Duration = SharedResources.Settings!.MaxFramesPerSec;

            HasMusic = true;
            HasBackground = false;

            if (SharedGameResources.Items == null)
                SharedGameResources.Items = new ItemManager();

            SharedGameResources.Camp = new CampaignManager();
            SharedGameResources.Eventm = new EventManager();
            SharedGameResources.Loot = new LootManager();
            SharedGameResources.Powers = new PowerManager();
            SharedGameResources.Fow = new FogOfWar();
            SharedGameResources.Mapr = new MapRenderer();
            SharedGameResources.Pc = new Avatar();
            SharedGameResources.Entitym = new EntityManager();
            SharedGameResources.Enemyg = new EnemyGroupManager();
            SharedGameResources.Hazards = new HazardManager();
            SharedGameResources.Menu = new MenuManager();
            SharedGameResources.Npcs = new NPCManager();
            _quests = new QuestLog(SharedGameResources.Menu!.Questlog!);
            SharedGameResources.XpScaling = new XPScaling();

            LoadTitles();

            RefreshWidgets();
        }

        public override void RefreshWidgets()
        {
            SharedGameResources.Menu!.AlignAll();
        }

        /// <summary>
        /// 已全局导入，此处省略。
        /// </summary>
        public void ResetGame()
        {
            SharedGameResources.Camp!.ResetAllStatuses();
            SharedGameResources.Pc!.Init();
            SharedGameResources.Pc!.Stats.Currency = 0;
            SharedGameResources.Menu!.Act!.Clear(!MenuActionBar.ClearSkipItems);
            SharedGameResources.Menu!.Inv!.Inventory[MenuInventory.Equipment].Clear();
            SharedGameResources.Menu!.Inv!.Inventory[MenuInventory.Carried].Clear();
            SharedGameResources.Menu!.Inv!.ChangedEquipment = true;
            SharedGameResources.Menu!.Inv!.Currency = 0;
            SharedGameResources.Menu!.Questlog!.ClearAll();
            _quests!.CreateQuestList();
            SharedGameResources.Menu!.Hudlog!.Clear();

            SharedGameResources.Menu!.Talker!.SetHero(SharedGameResources.Pc!.Stats);
            SharedGameResources.Pc!.LoadSounds();

            SharedGameResources.Mapr!.Teleportation = true;
            SharedGameResources.Mapr!.TeleportMapname = "maps/spawn.txt";
        }

        /// <summary>
        /// <see cref="_enemy"/> 已全局导入，此处省略。
        ///  MenuEnemy ???
        /// </summary>
        private void CheckEnemyFocus()
        {
            Avatar pc = SharedGameResources.Pc!;
            EntityManager entitym = SharedGameResources.Entitym!;
            HazardManager hazards = SharedGameResources.Hazards!;
            MenuManager menu = SharedGameResources.Menu!;
            MapRenderer mapr = SharedGameResources.Mapr!;
            InputState inpt = SharedResources.Inpt!;
            CursorManager curs = SharedResources.Curs!;
            EngineSettings eset = SharedResources.Eset!;

            pc.Stats.TargetCorpse = null;
            pc.Stats.TargetNearest = null;
            pc.Stats.TargetNearestCorpse = null;
            pc.Stats.TargetNearestDist = 0;
            pc.Stats.TargetNearestCorpseDist = 0;

            Vector2 srcPos = pc.Stats.Pos;

            if (!inpt.UsingMouse())
            {
                if (hazards.LastEnemy != null)
                {
                    if (_enemy == hazards.LastEnemy)
                    {
                        if (!menu.Enemy!.Timeout.IsEnd() && hazards.LastEnemy.Stats.Hp > 0)
                            return;
                        else
                            hazards.LastEnemy = null;
                    }
                    _enemy = hazards.LastEnemy;
                }
                else
                {
                    float? savedDistance = null;
                    _enemy = entitym.GetNearestEntity(pc.Stats.Pos, !EntityManager.GetCorpse, ref savedDistance, eset.Misc.InteractRange);
                }
            }
            else
            {
                if (hazards.LastEnemy != null)
                {
                    _enemy = hazards.LastEnemy;
                    hazards.LastEnemy = null;
                }
                else
                {
                    _enemy = entitym.EntityFocus(inpt.Mouse, mapr.Cam.Pos, EntityManager.IsAlive);
                    if (_enemy != null)
                    {
                        curs.SetCursor(CursorManager.CursorAttack);
                    }
                    srcPos = Utils.ScreenToMap(inpt.Mouse.X, inpt.Mouse.Y, mapr.Cam.Pos.X, mapr.Cam.Pos.Y);
                }
            }

            if (_enemy != null)
            {
                if (!_enemy.Stats.SuppressHp)
                {
                    menu.Enemy!.Enemy = _enemy;
                    menu.Enemy!.Timeout.Reset(Timer.Begin);
                }
            }
            else if (inpt.UsingMouse())
            {
                Entity? tempEnemy = entitym.EntityFocus(inpt.Mouse, mapr.Cam.Pos, !EntityManager.IsAlive);
                if (tempEnemy != null && !tempEnemy.Stats.SuppressHp)
                {
                    pc.Stats.TargetCorpse = tempEnemy.Stats;
                    menu.Enemy!.Enemy = tempEnemy;
                    menu.Enemy!.Timeout.Reset(Timer.Begin);
                }
            }

            if (_enemy != null)
            {
                pc.CursorEnemy = _enemy;
            }
            else
            {
                pc.CursorEnemy = null;
            }

            float? nearestDist = pc.Stats.TargetNearestDist;
            Entity? nearest = entitym.GetNearestEntity(srcPos, !EntityManager.GetCorpse, ref nearestDist, eset.Misc.InteractRange);
            pc.Stats.TargetNearestDist = nearestDist ?? 0;
            if (nearest != null)
                pc.Stats.TargetNearest = nearest.Stats;

            float? nearestCorpseDist = pc.Stats.TargetNearestCorpseDist;
            Entity? nearestCorpse = entitym.GetNearestEntity(srcPos, EntityManager.GetCorpse, ref nearestCorpseDist, eset.Misc.InteractRange);
            pc.Stats.TargetNearestCorpseDist = nearestCorpseDist ?? 0;
            if (nearestCorpse != null)
                pc.Stats.TargetNearestCorpse = nearestCorpse.Stats;
        }

        /// <summary>
        /// <see cref="CheckEnemyFocus"/> NPCManager??
        /// </summary>
        private void CheckNPCFocus()
        {
            Entity? focusNpc;
            Avatar pc = SharedGameResources.Pc!;
            NPCManager npcs = SharedGameResources.Npcs!;
            MenuManager menu = SharedGameResources.Menu!;
            MapRenderer mapr = SharedGameResources.Mapr!;
            InputState inpt = SharedResources.Inpt!;

            if (!inpt.UsingMouse() && (menu.Enemy!.Enemy == null || menu.Enemy!.Enemy!.Stats.HeroAlly))
            {
                focusNpc = npcs.GetNearestNpc(pc.Stats.Pos);
            }
            else
            {
                focusNpc = npcs.NpcFocus(inpt.Mouse, mapr.Cam.Pos, true);
            }

            if (focusNpc != null)
            {
                if (!focusNpc.Stats.SuppressHp)
                {
                    menu.Enemy!.Enemy = focusNpc;
                    menu.Enemy!.Timeout.Reset(Timer.Begin);
                }
            }
            else if (inpt.UsingMouse())
            {
                Entity? tempNpc = npcs.NpcFocus(inpt.Mouse, mapr.Cam.Pos, false);
                if (tempNpc != null)
                {
                    menu.Enemy!.Enemy = tempNpc;
                    menu.Enemy!.Timeout.Reset(Timer.Begin);
                }
            }
        }

        /// <summary>
        /// 已全局导入，此处省略。
        /// </summary>
        private void CheckLoot()
        {
            Avatar pc = SharedGameResources.Pc!;
            MenuManager menu = SharedGameResources.Menu!;
            LootManager loot = SharedGameResources.Loot!;
            ItemManager items = SharedGameResources.Items!;
            CampaignManager camp = SharedGameResources.Camp!;
            MapRenderer mapr = SharedGameResources.Mapr!;
            InputState inpt = SharedResources.Inpt!;

            if (!pc.Stats.Alive)
                return;

            if (menu.IsDragging())
                return;

            ItemStack pickup = new ItemStack();

            pickup = loot.CheckAutoPickup(pc.Stats.Pos);

            if (pickup.Empty() && !pc.UsingMain1)
            {
                pickup = loot.CheckPickup(inpt.Mouse, mapr.Cam.Pos, pc.Stats.Pos);
            }

            if (!pickup.Empty())
            {
                menu.Inv!.Add(pickup, MenuInventory.Carried, ItemStorage.NoSlot, MenuInventory.AddPlaySound, MenuInventory.AddAutoEquip);
                if (items.IsValid(pickup.Item))
                {
                    StatusID pickupStatus = camp.RegisterStatus(items.Items[(int)pickup.Item]!.PickupStatus);
                    camp.SetStatus(pickupStatus);
                }
                pickup.Clear();
            }
        }

        private void CheckTeleport()
        {
            Avatar pc = SharedGameResources.Pc!;
            MapRenderer mapr = SharedGameResources.Mapr!;
            EntityManager entitym = SharedGameResources.Entitym!;
            NPCManager npcs = SharedGameResources.Npcs!;
            MenuManager menu = SharedGameResources.Menu!;
            HazardManager hazards = SharedGameResources.Hazards!;
            LootManager loot = SharedGameResources.Loot!;
            PowerManager powers = SharedGameResources.Powers!;
            FogOfWar fow = SharedGameResources.Fow!;
            SaveLoad saveLoad = SharedResources.SaveLoad!;
            InputState inpt = SharedResources.Inpt!;
            Settings settings = SharedResources.Settings!;
            EngineSettings eset = SharedResources.Eset!;
            SoundManager snd = SharedResources.Snd!;
            RenderDevice renderDevice = SharedResources.RenderDevice!;

            bool onLoadTeleport = false;

            if (mapr.Teleportation || pc.Stats.Teleportation)
            {
                if (mapr.Fogofwar != 0)
                    if (fow.FogLayerId != 0)
                        fow.HandleIntramapTeleport();

                mapr.Collider.Unblock(pc.Stats.Pos.X, pc.Stats.Pos.Y);

                if (mapr.Teleportation)
                {
                    pc.Stats.Pos.X = mapr.TeleportDestination.X;
                    pc.Stats.Pos.Y = mapr.TeleportDestination.Y;
                    pc.TeleportCameraLock = true;
                }
                else
                {
                    pc.Stats.Pos.X = pc.Stats.TeleportDestination.X;
                    pc.Stats.Pos.Y = pc.Stats.TeleportDestination.Y;
                }

                if (mapr.TeleportMapname == "")
                {
                    Int2 target = new Int2((int)pc.Stats.Pos.X, (int)pc.Stats.Pos.Y);
                    Vector2 spawnPos = mapr.Collider.GetRandomNeighbor(target, 1, MapCollision.MoveNormal, MapCollision.CollideTypeAllEntities);
                    for (uint i = 0; i < entitym.Entities.Count; i++)
                    {
                        if (entitym.Entities[(int)i].Stats.HeroAlly && entitym.Entities[(int)i].Stats.Alive && entitym.Entities[(int)i].Stats.Speed > 0)
                        {
                            mapr.Collider.Unblock(entitym.Entities[(int)i].Stats.Pos.X, entitym.Entities[(int)i].Stats.Pos.Y);
                            entitym.Entities[(int)i].Stats.Pos = spawnPos;
                            mapr.Collider.Block(entitym.Entities[(int)i].Stats.Pos.X, entitym.Entities[(int)i].Stats.Pos.Y, MapCollision.IsAlly);
                        }
                    }
                }

                if (mapr.Teleportation && mapr.TeleportMapname != "")
                {
                    mapr.Cam.WarpTo(pc.Stats.Pos);
                    string teleportMapname = mapr.TeleportMapname;
                    mapr.TeleportMapname = "";
                    inpt.LockAll = (teleportMapname == "maps/spawn.txt");
                    mapr.ExecuteOnMapExitEvents();
                    ShowLoading();
                    renderDevice.CleanupQueuedImages();
                    saveLoad.SaveFOW();
                    mapr.Load(teleportMapname);
                    SetLoadingFrame();

                    if (mapr.ForceSpawnPos || (mapr.TeleportDestination.X == -1 && mapr.TeleportDestination.Y == -1))
                    {
                        pc.Stats.Pos.X = mapr.HeroPos.X;
                        pc.Stats.Pos.Y = mapr.HeroPos.Y;

                        if (mapr.TeleportDestinationId > 0)
                        {
                            for (int i = 0; i < mapr.Events.Count; ++i)
                            {
                                EventComponent? ecHeroPos = mapr.Events[i].GetComponent(EventComponent.IntermapID);
                                if (ecHeroPos != null && ecHeroPos.Data[0].Int == mapr.TeleportDestinationId)
                                {
                                    pc.Stats.Pos.X = (float)mapr.Events[i].Location.X + 0.5f;
                                    pc.Stats.Pos.Y = (float)mapr.Events[i].Location.Y + 0.5f;
                                    break;
                                }
                            }
                        }
                        mapr.Cam.WarpTo(pc.Stats.Pos);
                    }

                    if (mapr.Collider.IsValidPosition(pc.Stats.Pos.X, pc.Stats.Pos.Y, MapCollision.MoveNormal, MapCollision.CollideTypeHero))
                    {
                        mapr.RespawnMap = teleportMapname;
                        mapr.RespawnPoint = pc.Stats.Pos;
                    }
                    else
                    {
                        Utils.LogError("GameStatePlay: Spawn position (%d, %d) is blocked.", (int)pc.Stats.Pos.X, (int)pc.Stats.Pos.Y);
                    }

                    pc.HandleNewMap();
                    hazards.HandleNewMap();
                    loot.HandleNewMap();
                    powers.HandleNewMap(mapr.Collider);
                    menu.Enemy!.HandleNewMap();
                    menu.Stash!.Visible = false;

                    mapr.Teleportation = false;

                    mapr.ExecuteOnLoadEvents();
                    if (mapr.Teleportation)
                        onLoadTeleport = true;

                    entitym.HandleNewMap();
                    npcs.HandleNewMap();
                    ResetNPC();

                    menu.Mini!.Prerender(mapr.Collider, mapr.W, mapr.H);

                    if (pc.Stats.Permadeath && pc.Stats.CurState == StatBlock.EntityDead)
                    {
                        snd.StopMusic();
                        ShowLoading();
                        SetRequestedGameState(new GameStateTitle());
                    }
                    else if (eset.Misc.SaveOnload)
                    {
                        if (!_isFirstMapLoad)
                            saveLoad.SaveGame();
                        else
                            _isFirstMapLoad = false;
                    }
                }

                if (mapr.Collider.IsOutsideMap(pc.Stats.Pos.X, pc.Stats.Pos.Y))
                {
                    Utils.LogError("GameStatePlay: Teleport position is outside of map bounds.");
                    pc.Stats.Pos.X = 0.5f;
                    pc.Stats.Pos.Y = 0.5f;
                }

                mapr.Collider.Block(pc.Stats.Pos.X, pc.Stats.Pos.Y, !MapCollision.IsAlly);

                pc.Stats.Teleportation = false;

                if (settings.MouseMove)
                {
                    pc.MmTargetObject = Avatar.MmTargetNone;
                    Vector2 mmTarget = pc.Stats.Pos;
                    pc.SetDesiredMMTarget(ref mmTarget);
                }
            }

            if (!onLoadTeleport && mapr.TeleportMapname == "")
                mapr.Teleportation = false;
        }

        /// <summary>
        /// 已全局导入，此处省略。已全局导入，此处省略。
        /// </summary>
        private void CheckCancel()
        {
            Avatar pc = SharedGameResources.Pc!;
            MapRenderer mapr = SharedGameResources.Mapr!;
            MenuManager menu = SharedGameResources.Menu!;
            SaveLoad saveLoad = SharedResources.SaveLoad!;
            Settings settings = SharedResources.Settings!;
            InputState inpt = SharedResources.Inpt!;
            EngineSettings eset = SharedResources.Eset!;
            SoundManager snd = SharedResources.Snd!;

            bool saveOnExit = eset.Misc.SaveOnexit && !(pc.Stats.Permadeath && pc.Stats.CurState == StatBlock.EntityDead);

            if (saveOnExit && eset.Misc.SavePosOnexit)
            {
                mapr.RespawnPoint = pc.Stats.Pos;
            }

            if (menu.RequestingExit)
            {
                menu.CloseAll();

                if (saveOnExit)
                    saveLoad.SaveGame();

                settings.SaveSettings();
                inpt.SaveKeyBindings();

                snd.StopMusic();
                ShowLoading();
                SetRequestedGameState(new GameStateTitle());

                saveLoad.GameSlot = 0;
            }

            if (inpt.Done)
            {
                menu.CloseAll();

                if (saveOnExit)
                    saveLoad.SaveGame();

                settings.SaveSettings();
                inpt.SaveKeyBindings();

                snd.StopMusic();
                ExitRequested = true;
            }
        }

        /// <summary>
        /// 已全局导入，此处省略。
        /// </summary>
        private void CheckLog()
        {
            Avatar pc = SharedGameResources.Pc!;
            MenuManager menu = SharedGameResources.Menu!;

            if (pc.Respawn)
            {
                menu.Hudlog!.Clear();
            }

            while (pc.LogMsgQueue.Count > 0)
            {
                (string str, int msgType) = pc.LogMsgQueue.Peek();

                menu.Questlog!.Add(str, MenuLog.TypeMessages, msgType);
                menu.Hudlog!.Add(str, msgType);

                pc.LogMsgQueue.Dequeue();
            }
        }

        /// <summary>
        /// 已全局导入，此处省略。
        /// </summary>
        private void CheckBook()
        {
            MapRenderer mapr = SharedGameResources.Mapr!;
            MenuManager menu = SharedGameResources.Menu!;

            if (mapr.ShowBook != "")
            {
                menu.Book!.SetBookFilename(mapr.ShowBook);
                mapr.ShowBook = "";
            }

            if (menu.Inv!.ShowBook != "")
            {
                menu.Book!.SetBookFilename(menu.Inv!.ShowBook);
                menu.Inv!.ShowBook = "";
            }
        }

        private void LoadTitles()
        {
            using FileParser infile = new FileParser();
            if (infile.Open("engine/titles.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (infile.NewSection && infile.Section == "title")
                    {
                        PlayTitle t = new PlayTitle();
                        _titles.Add(t);
                    }

                    if (_titles.Count == 0) continue;

                    PlayTitle title = _titles[^1];

                    if (infile.Key == "title")
                    {
                        title.Title = infile.Val;
                    }
                    else if (infile.Key == "level")
                    {
                        title.Level = Parse.ToInt(infile.Val);
                    }
                    else if (infile.Key == "power")
                    {
                        title.Power = SharedGameResources.Powers!.VerifyID(Parse.ToPowerID(infile.Val), infile, !PowerManager.AllowZeroId);
                    }
                    else if (infile.Key == "requires_status")
                    {
                        string repeatVal = Parse.PopFirstString(ref infile.Val);
                        while (repeatVal != "")
                        {
                            title.RequiresStatus.Add(SharedGameResources.Camp!.RegisterStatus(repeatVal));
                            repeatVal = Parse.PopFirstString(ref infile.Val);
                        }
                    }
                    else if (infile.Key == "requires_not_status")
                    {
                        string repeatVal = Parse.PopFirstString(ref infile.Val);
                        while (repeatVal != "")
                        {
                            title.RequiresNotStatus.Add(SharedGameResources.Camp!.RegisterStatus(repeatVal));
                            repeatVal = Parse.PopFirstString(ref infile.Val);
                        }
                    }
                    else if (infile.Key == "primary_stat")
                    {
                        title.PrimaryStat1 = Parse.PopFirstString(ref infile.Val);
                        title.PrimaryStat2 = Parse.PopFirstString(ref infile.Val);
                    }
                    else infile.Error("GameStatePlay: '%s' is not a valid key.", infile.Key);
                }
                infile.Close();
            }
        }

        private void CheckTitle()
        {
            Avatar pc = SharedGameResources.Pc!;
            CampaignManager camp = SharedGameResources.Camp!;

            if (!pc.Stats.CheckTitle || _titles.Count == 0)
                return;

            int titleId = -1;

            for (uint i = 0; i < _titles.Count; i++)
            {
                if (_titles[(int)i].Title == "")
                    continue;

                if (_titles[(int)i].Level > 0 && pc.Stats.Level < _titles[(int)i].Level)
                    continue;
                if (_titles[(int)i].Power > 0 && !pc.Stats.PowersList.Contains(_titles[(int)i].Power))
                    continue;
                if (_titles[(int)i].PrimaryStat1 != "" && !CheckPrimaryStat(_titles[(int)i].PrimaryStat1, _titles[(int)i].PrimaryStat2))
                    continue;

                bool statusFailed = false;
                for (int j = 0; j < _titles[(int)i].RequiresStatus.Count; ++j)
                {
                    if (!camp.CheckStatus(_titles[(int)i].RequiresStatus[j]))
                    {
                        statusFailed = true;
                        break;
                    }
                }
                for (int j = 0; j < _titles[(int)i].RequiresNotStatus.Count; ++j)
                {
                    if (camp.CheckStatus(_titles[(int)i].RequiresNotStatus[j]))
                    {
                        statusFailed = true;
                        break;
                    }
                }

                if (statusFailed)
                    continue;

                titleId = (int)i;
                break;
            }

            if (titleId != -1) pc.Stats.CharacterSubclass = _titles[titleId].Title;
            pc.Stats.CheckTitle = false;
            pc.Stats.RefreshStats = true;
        }

        private void CheckEquipmentChange()
        {
            Avatar pc = SharedGameResources.Pc!;
            MenuManager menu = SharedGameResources.Menu!;
            ItemManager items = SharedGameResources.Items!;

            if (menu.Inv!.ChangedEquipment)
            {
                menu.Act!.Updated = true;

                pc.LoadAnimations();

                if (pc.FeetIndex != -1)
                {
                    ItemID feetId = menu.Inv!.Inventory[MenuInventory.Equipment][pc.FeetIndex].Item;
                    if (items.IsValid(feetId))
                        pc.LoadStepFX(items.Items[(int)feetId]!.Stepfx);
                }
            }

            menu.Inv!.ChangedEquipment = false;
        }

        private void CheckLootDrop()
        {
            MenuManager menu = SharedGameResources.Menu!;
            CampaignManager camp = SharedGameResources.Camp!;
            LootManager loot = SharedGameResources.Loot!;
            Avatar pc = SharedGameResources.Pc!;

            while (menu.DropStack.Count > 0)
            {
                if (!menu.DropStack.Peek().Empty())
                {
                    loot.AddLoot(menu.DropStack.Peek(), pc.Stats.Pos, LootManager.DroppedByHero);
                }
                menu.DropStack.Dequeue();
            }

            while (camp.DropStack.Count > 0)
            {
                if (!camp.DropStack.Peek().Empty())
                {
                    loot.AddLoot(camp.DropStack.Peek(), pc.Stats.Pos, LootManager.DroppedByHero);
                }
                camp.DropStack.Dequeue();
            }

            while (menu.Inv!.DropStack.Count > 0)
            {
                if (!menu.Inv!.DropStack.Peek().Empty())
                {
                    loot.AddLoot(menu.Inv!.DropStack.Peek(), pc.Stats.Pos, LootManager.DroppedByHero);
                }
                menu.Inv!.DropStack.Dequeue();
            }
        }

        /// <summary>
        /// ???
        /// </summary>
        private void CheckUsedItems()
        {
            MenuManager menu = SharedGameResources.Menu!;
            PowerManager powers = SharedGameResources.Powers!;

            for (uint i = 0; i < powers.UsedItems.Count; i++)
            {
                menu.Inv!.Remove(powers.UsedItems[(int)i], 1);
            }
            for (uint i = 0; i < powers.UsedEquippedItems.Count; i++)
            {
                menu.Inv!.Inventory[MenuInventory.Equipment].Remove(powers.UsedEquippedItems[(int)i], 1);
                menu.Inv!.ApplyEquipment();
            }
            powers.UsedItems.Clear();
            powers.UsedEquippedItems.Clear();
        }

        /// <summary>
        /// 已全局导入，此处省略。
        /// </summary>
        private void CheckNotifications()
        {
            Avatar pc = SharedGameResources.Pc!;
            MenuManager menu = SharedGameResources.Menu!;

            if (pc.NewLevelNotification || menu.Chr!.GetUnspent() > 0)
            {
                pc.NewLevelNotification = false;
                menu.Act!.RequiresAttention[MenuActionBar.MenuCharacter] = !menu.Chr!.Visible;
            }
            if (menu.Pow!.NewPowerNotification)
            {
                menu.Pow!.NewPowerNotification = false;
                menu.Act!.RequiresAttention[MenuActionBar.MenuPowers] = !menu.Pow!.Visible;
            }
            if (_quests!.NewQuestNotification)
            {
                _quests!.NewQuestNotification = false;
                menu.Act!.RequiresAttention[MenuActionBar.MenuLog] = !menu.Questlog!.Visible && !pc.QuestlogDismissed;
                pc.QuestlogDismissed = false;
            }

            if (pc.Stats.Transformed)
            {
                menu.Act!.RequiresAttention[MenuActionBar.MenuPowers] = false;
            }
        }

        /// <summary>
        /// 检测玩家是否与附近 NPC 交互，触发对话或交易。
        /// </summary>
        private void CheckNPCInteraction()
        {
            Avatar pc = SharedGameResources.Pc!;
            MapRenderer mapr = SharedGameResources.Mapr!;
            NPCManager npcs = SharedGameResources.Npcs!;
            MenuManager menu = SharedGameResources.Menu!;
            InputState inpt = SharedResources.Inpt!;
            EngineSettings eset = SharedResources.Eset!;

            if (pc.UsingMain1 || !pc.Stats.Humanoid)
                return;

            if (!menu.Talker!.Visible)
            {
                pc.AllowMovement = true;
            }

            if (_npcId != -1 && !menu.IsNPCMenuVisible())
            {
                ResetNPC();
            }

            if (mapr.EventNpc != "")
            {
                if (_npcId != -1)
                {
                    ResetNPC();
                }
                _npcId = mapr.NpcId = npcs.GetId(mapr.EventNpc);
                menu.Talker!.NpcFromMap = false;
            }
            else if (mapr.NpcId != -1)
            {
                _npcId = mapr.NpcId;
                menu.Talker!.NpcFromMap = true;
            }
            mapr.EventNpc = "";
            mapr.NpcId = -1;

            if (_npcId != -1)
            {
                bool interactWithNpc = false;
                if (menu.Talker!.NpcFromMap)
                {
                    float interactDistance = Utils.CalcDist(pc.Stats.Pos, npcs.Npcs[_npcId].Stats.Pos);
                    bool npcIsAlive = !npcs.Npcs[_npcId].Stats.HeroAlly || npcs.Npcs[_npcId].Stats.Hp > 0;

                    if (interactDistance < eset.Misc.InteractRange && npcIsAlive)
                    {
                        interactWithNpc = true;
                    }
                    else
                    {
                        ResetNPC();
                    }
                }
                else
                {
                    interactWithNpc = true;

                    pc.AllowMovement = false;
                }

                if (interactWithNpc)
                {
                    if (!menu.IsNPCMenuVisible())
                    {
                        if (inpt.Pressing[Input.Main1] && inpt.UsingMouse()) inpt.Lock[Input.Main1] = true;
                        if (inpt.Pressing[Input.Accept]) inpt.Lock[Input.Accept] = true;

                        menu.CloseAll();
                        menu.Talker!.SetNPC(npcs.Npcs[_npcId]);
                        menu.Talker!.ChooseDialogNode(-1);
                    }
                }
            }
        }

        private void CheckStash()
        {
            Avatar pc = SharedGameResources.Pc!;
            MapRenderer mapr = SharedGameResources.Mapr!;
            MenuManager menu = SharedGameResources.Menu!;
            SaveLoad saveLoad = SharedResources.SaveLoad!;
            EngineSettings eset = SharedResources.Eset!;

            if (mapr.Stash)
            {
                menu.CloseAll();
                menu.Inv!.Visible = true;
                menu.Stash!.Visible = true;
                mapr.Stash = false;
                menu.Stash!.Validate(menu.DropStack);
            }
            else if (menu.Stash!.Visible)
            {
                if (!menu.Inv!.Visible)
                {
                    menu.ResetDrag();
                    menu.Stash!.Visible = false;
                }

                float interactDistance = Utils.CalcDist(pc.Stats.Pos, mapr.StashPos);
                if (interactDistance > eset.Misc.InteractRange || !pc.Stats.Alive)
                {
                    menu.ResetDrag();
                    menu.Stash!.Visible = false;
                }
            }

            if (menu.Stash!.CheckUpdates())
            {
                saveLoad.SaveGame();
            }
        }

        private void CheckCutscene()
        {
            MapRenderer mapr = SharedGameResources.Mapr!;
            Avatar pc = SharedGameResources.Pc!;
            MenuManager menu = SharedGameResources.Menu!;
            SaveLoad saveLoad = SharedResources.SaveLoad!;
            EngineSettings eset = SharedResources.Eset!;

            if (!mapr.Cutscene)
                return;

            ShowLoading();
            GameStateCutscene cutscene = new GameStateCutscene(null);

            if (!cutscene.Load(mapr.CutsceneFile))
            {
                cutscene.Dispose();
                mapr.Cutscene = false;
                return;
            }

            cutscene.GameSlot = saveLoad.GameSlot;

            if (mapr.Teleportation)
            {
                if (mapr.TeleportMapname != "")
                    mapr.RespawnMap = mapr.TeleportMapname;

                mapr.RespawnPoint = mapr.TeleportDestination;
            }
            else
            {
                mapr.RespawnPoint = pc.Stats.Pos;
            }

            if (eset.Misc.SaveOncutscene)
                saveLoad.SaveGame();

            menu.CloseAll();

            SetRequestedGameState(cutscene);
        }

        private void CheckSaveEvent()
        {
            Avatar pc = SharedGameResources.Pc!;
            MapRenderer mapr = SharedGameResources.Mapr!;
            SaveLoad saveLoad = SharedResources.SaveLoad!;

            if (mapr.SaveGame)
            {
                mapr.RespawnPoint = pc.Stats.Pos;
                saveLoad.SaveGame();
                mapr.SaveGame = false;
            }
        }

        /// <summary>
        /// 已全局导入，此处省略。
        /// </summary>
        private void UpdateActionBar(uint index)
        {
            MenuManager menu = SharedGameResources.Menu!;
            ItemManager items = SharedGameResources.Items!;

            if (menu.Act!.SlotsCount == 0 || index > menu.Act!.SlotsCount - 1) return;

            if (items.Items.Count == 0) return;

            for (uint i = index; i < menu.Act!.SlotsCount; i++)
            {
                if (menu.Act!.Hotkeys[(int)i] == 0) continue;

                PowerID id = menu.Inv!.GetPowerMod(menu.Act!.HotkeysMod[(int)i]);
                if (id > 0)
                {
                    menu.Act!.HotkeysMod[(int)i] = id;
                    UpdateActionBar(i);
                    return;
                }
            }
        }

        /// <summary>
        /// 已全局导入，此处省略。
        /// </summary>
        public override void Logic()
        {
            Avatar pc = SharedGameResources.Pc!;
            MenuManager menu = SharedGameResources.Menu!;
            MapRenderer mapr = SharedGameResources.Mapr!;
            EntityManager entitym = SharedGameResources.Entitym!;
            HazardManager hazards = SharedGameResources.Hazards!;
            LootManager loot = SharedGameResources.Loot!;
            NPCManager npcs = SharedGameResources.Npcs!;
            PowerManager powers = SharedGameResources.Powers!;
            InputState inpt = SharedResources.Inpt!;
            CursorManager curs = SharedResources.Curs!;
            CombatText comb = SharedResources.Comb!;

            if (inpt.WindowResized)
                RefreshWidgets();

            curs.LowHp = pc.IsLowHpCursorEnabled() && pc.IsLowHp();

            CheckCutscene();

            menu.Logic();

            if (!IsPaused())
            {
                if (!_secondTimer.IsEnd())
                    _secondTimer.Tick();
                else
                {
                    pc.TimePlayed++;
                    _secondTimer.Reset(Timer.Begin);
                }

                if (pc.Stats.Alive) CheckLoot();
                CheckEnemyFocus();
                CheckNPCFocus();
                if (pc.Stats.Alive)
                {
                    mapr.CheckHotspots();
                    mapr.CheckNearestEvent();
                    CheckNPCInteraction();
                }
                CheckTitle();

                menu.Act!.CheckAction(pc.ActionQueue);
                pc.Logic();

                if (pc.Stats.Get(Stats.Stealth) > 100) entitym.HeroStealth = 100;
                else entitym.HeroStealth = pc.Stats.Get(Stats.Stealth);

                entitym.Logic();
                hazards.Logic();
                loot.Logic();
                npcs.Logic();

                comb.Logic(mapr.Cam.Pos);
            }

            if (pc.CloseMenus)
            {
                pc.CloseMenus = false;
                menu.CloseAll();
            }

            CheckTeleport();
            CheckLootDrop();
            CheckLog();
            CheckBook();
            CheckEquipmentChange();
            CheckUsedItems();
            CheckStash();
            CheckSaveEvent();
            CheckNotifications();
            CheckCancel();

            mapr.Logic(IsPaused());
            mapr.EnemiesCleared = entitym.IsCleared();
            _quests!.Logic();

            pc.CheckTransform();

            if (pc.SetPowers)
            {
                pc.SetPowers = false;
                if (!pc.Stats.Humanoid && menu.Pow!.Visible) menu.CloseRight();
                for (int i = 0; i < MenuActionBar.SlotMax; i++)
                {
                    menu.Act!.HotkeysTemp[i] = menu.Act!.Hotkeys[i];
                    menu.Act!.Hotkeys[i] = 0;
                }
                int count = MenuActionBar.SlotMain1;
                for (int i = 0; i < pc.CharmedStats!.PowersAi.Count; i++)
                {
                    if (powers.IsValid(pc.CharmedStats!.PowersAi[i].Id) && powers.Powers[pc.CharmedStats!.PowersAi[i].Id]!.Beacon != true)
                    {
                        menu.Act!.Hotkeys[count] = pc.CharmedStats!.PowersAi[i].Id;
                        menu.Act!.Locked[count] = true;
                        count++;
                        if (count == MenuActionBar.SlotMax)
                            count = 0;
                        else if (count == MenuActionBar.SlotMain1)
                            break;
                    }
                }
                if (pc.Stats.ManualUntransform && powers.IsValid(pc.UntransformPower))
                {
                    menu.Act!.Hotkeys[count] = pc.UntransformPower;
                    menu.Act!.Locked[count] = true;
                }
                else if (pc.Stats.ManualUntransform && pc.UntransformPower == 0)
                    Utils.LogError("GameStatePlay: Untransform power not found, you can't untransform manually");

                menu.Act!.Updated = true;

                if (pc.Stats.TransformWithEquipment)
                    menu.Inv!.ApplyEquipment();
            }
            if (pc.RevertPowers)
            {
                pc.RevertPowers = false;

                for (int i = 0; i < MenuActionBar.SlotMax; i++)
                {
                    menu.Act!.Hotkeys[i] = menu.Act!.HotkeysTemp[i];
                    menu.Act!.Locked[i] = false;
                }

                menu.Act!.Updated = true;

                menu.Inv!.ApplyEquipment();
            }

            if (pc.Respawn)
            {
                pc.Stats.Alive = true;
                pc.Stats.Corpse = false;
                pc.Stats.CurState = StatBlock.EntityStance;
                menu.Inv!.ApplyEquipment();
                menu.Inv!.ChangedEquipment = true;
                CheckEquipmentChange();
                pc.Stats.Hp = pc.Stats.Get(global::FlareEngine.Stats.HpMax);
                pc.Stats.Logic();
                pc.Stats.Recalc();
                menu.Pow!.ResetToBasePowers();
                menu.Pow!.SetUnlockedPowers();
                powers.ActivatePassives(pc.Stats);
                pc.Respawn = false;
            }

            if (menu.MenusOpen)
            {
                curs.SetCursor(CursorManager.CursorNormal);
            }

            if (menu.Act!.Updated)
            {
                menu.Act!.Updated = false;

                for (uint i = 0; i < menu.Act!.SlotsCount; i++)
                {
                    menu.Act!.HotkeysMod[(int)i] = menu.Act!.Hotkeys[(int)i];
                }

                UpdateActionBar(UpdateActionbarAll);
            }

            if (menu.Exit!.ReloadMusic)
            {
                mapr.LoadMusic();
                menu.Exit!.ReloadMusic = false;
            }
        }

        /// <summary>
        /// ??
        /// </summary>
        public override void Render()
        {
            Avatar pc = SharedGameResources.Pc!;
            MapRenderer mapr = SharedGameResources.Mapr!;
            EntityManager entitym = SharedGameResources.Entitym!;
            NPCManager npcs = SharedGameResources.Npcs!;
            LootManager loot = SharedGameResources.Loot!;
            HazardManager hazards = SharedGameResources.Hazards!;
            MenuManager menu = SharedGameResources.Menu!;
            CombatText comb = SharedResources.Comb!;

            if (mapr.IsSpawnMap)
                return;

            List<Renderable> rens = new List<Renderable>();
            List<Renderable> rensDead = new List<Renderable>();

            pc.AddRenders(rens);

            entitym.AddRenders(rens, rensDead);

            npcs.AddRenders(rens);

            loot.AddRenders(rens, rensDead);

            hazards.AddRenders(rens, rensDead);

            mapr.Render(rens, rensDead);

            loot.RenderTooltips(mapr.Cam.Pos);

            if (mapr.MapChange)
            {
                menu.Mini!.Prerender(mapr.Collider, mapr.W, mapr.H);
                mapr.MapChange = false;
            }
            menu.Mini!.SetMapTitle(mapr.Title);
            menu.Mini!.Render(pc.Stats.Pos);
            menu.RegionTitle!.SetTitle(mapr.Title);
            menu.Render();

            if (!IsPaused())
                comb.Render();
        }

        public override bool IsPaused()
        {
            return SharedGameResources.Menu!.Pause;
        }

        private void ResetNPC()
        {
            MenuManager menu = SharedGameResources.Menu!;

            _npcId = -1;
            menu.Talker!.NpcFromMap = true;
            menu.ResetDrag();
            menu.Vendor!.SetNPC(null);
            menu.Talker!.SetNPC(null);
        }

        private bool CheckPrimaryStat(string first, string second)
        {
            Avatar pc = SharedGameResources.Pc!;
            EngineSettings eset = SharedResources.Eset!;

            int high = 0;
            int highIndex = eset.PrimaryStats.Stats.Count;
            int lowIndex = eset.PrimaryStats.Stats.Count;

            for (int i = 0; i < eset.PrimaryStats.Stats.Count; ++i)
            {
                int stat = pc.Stats.GetPrimary(i);
                if (stat > high)
                {
                    if (highIndex != eset.PrimaryStats.Stats.Count)
                    {
                        lowIndex = highIndex;
                    }
                    high = stat;
                    highIndex = i;
                }
                else if (stat == high && lowIndex == eset.PrimaryStats.Stats.Count)
                {
                    lowIndex = i;
                }
                else if (lowIndex == eset.PrimaryStats.Stats.Count || (lowIndex < eset.PrimaryStats.Stats.Count && stat > pc.Stats.GetPrimary(lowIndex)))
                {
                    lowIndex = i;
                }
            }

            if (highIndex != eset.PrimaryStats.Stats.Count && first != eset.PrimaryStats.Stats[highIndex].Id)
                return false;

            if (second != "")
            {
                if (lowIndex != eset.PrimaryStats.Stats.Count && second != eset.PrimaryStats.Stats[lowIndex].Id)
                    return false;
            }
            else if (pc.Stats.GetPrimary(highIndex) == pc.Stats.GetPrimary(lowIndex))
            {
                return false;
            }

            return true;
        }

        public override void Dispose()
        {
            SharedResources.Curs!.LowHp = false;

            _quests = null;

            SharedGameResources.Npcs?.Dispose();
            SharedGameResources.Hazards?.Dispose();
            SharedGameResources.Entitym?.Dispose();
            SharedGameResources.Pc?.Dispose();
            SharedGameResources.Mapr?.Dispose();
            SharedGameResources.Menu?.Dispose();
            SharedGameResources.Loot?.Dispose();
            SharedGameResources.Camp?.Dispose();
            SharedGameResources.Items?.Dispose();
            SharedGameResources.Powers?.Dispose();
            SharedGameResources.Fow?.Dispose();
            SharedGameResources.XpScaling?.Dispose();
            SharedGameResources.Enemyg?.Dispose();
            SharedGameResources.Eventm?.Dispose();

            SharedGameResources.Pc = null;
            SharedGameResources.Menu = null;
            SharedGameResources.Camp = null;
            SharedGameResources.Enemyg = null;
            SharedGameResources.Entitym = null;
            SharedGameResources.Eventm = null;
            SharedGameResources.Items = null;
            SharedGameResources.Loot = null;
            SharedGameResources.Mapr = null;
            SharedGameResources.MenuAct = null;
            SharedGameResources.MenuPowers = null;
            SharedGameResources.Powers = null;
            SharedGameResources.Fow = null;
            SharedGameResources.XpScaling = null;
            SharedGameResources.Npcs = null;
            SharedGameResources.Hazards = null;

            base.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
