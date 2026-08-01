// <自动生成> 对应 C++ 源文件：SaveLoad.h + SaveLoad.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略�?
using System.Globalization;
using System.Text;

namespace FlareEngine
{
    /// <summary>
    /// SaveLoad
    ///
    /// GameStatePlay 的存�?读档功能。原始实现将其单独放在一�?cpp 文件中，只是为了�?
    /// GameStatePlay.cpp 保持精简，专注于自身核心逻辑�?
    ///
    /// 前向引用说明：本单元依赖的以下类型尚未在本次批次中转换，按照迁移指南"阶段 B"的规则，
    /// 直接使用预期�?PascalCase 类型名进行前向引用（不要求这些类型已存在于代码库中）�?
    /// 具体名称与成员见 SaveLoad.report.txt�?前向引用的未转换类型与假�?API" 一节：
    /// Avatar / StatBlock（通过 Avatar.Stats 暴露�? CampaignManager / FogOfWar / Item /
    /// ItemManager / ItemStack / ItemStorage / MenuActionBar / MenuCharacter / MenuHUDLog /
    /// MenuInventory / MenuItemStorage / MenuLog / MenuManager / MenuPowers / MenuStash /
    /// MenuStashTab / MenuTalker / MenuVendor / MapRenderer / NPC / PowerManager /
    /// SharedGameResources / Stats / Version / VersionInfo / WidgetLog / BonusData /
    /// LevelScaledValue / LevelScaledMinMax / EngineSettings.HeroClassesSettings.HeroClass（后�?
    /// 已存在于 EngineSettings.cs，直接复用其已确定的成员）�?
    ///
    /// 全局单例访问方式：与 SharedResources.cs 建立的约定一致——引擎级单例通过已存在的
    /// `SharedResources.Xxx` 访问；游戏专属的全局单例（对�?C++ SharedGameResources.h 中的裸指�?
    /// 全局变量 pc/camp/mapr/menu/powers/items/fow）则通过一个同样风格、尚待转换的
    /// `SharedGameResources` 静态类�?`SharedGameResources.Xxx`（Xxx 为对应全局变量名的
    /// PascalCase 形式）方式访问。为了让方法体尽量贴近原�?C++ 代码的书写方式，
    /// 各方法内部会先把用到的全局单例缓存到与原始全局变量同名（camelCase）的局部变量中
    /// （例�?`Avatar pc = SharedGameResources.Pc!;`），后续代码即可像原始代码一样使�?
    /// `pc.Stats.Xxx`、`menu.Inv` 等写法�?
    /// </summary>
    public class SaveLoad : IDisposable
    {
        public const bool SaveStorageItems = true;

        /// <summary>对应 getGameSlot()/setGameSlot()：无附加逻辑的简单读写，转为自动属性�</summary>
        public int GameSlot { get; set; }

        public SaveLoad()
        {
            GameSlot = 0;
        }

        /// <summary>
        /// 对应 C++ 析构函数：仅记录一条清理日志，不持有需要释放的非托管资源�?
        /// 采用�?EngineSettings/FontEngine 一致的 IDisposable 模式，表�?生命周期�?main 管理"的语义�?
        /// </summary>
        public void Dispose()
        {
            Utils.LogInfo("Cleaning up: SaveLoad");
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 退出游戏前，将当前状态写入存档文件�?
        /// </summary>
        public void SaveGame()
        {
            if (GameSlot <= 0) return;

            Settings settings = SharedResources.Settings!;
            EngineSettings eset = SharedResources.Eset!;
            Avatar pc = SharedGameResources.Pc!;
            MenuManager menu = SharedGameResources.Menu!;
            MapRenderer mapr = SharedGameResources.Mapr!;
            CampaignManager camp = SharedGameResources.Camp!;
            MenuInventory inv = menu.Inv!;
            MenuActionBar act = menu.Act!;
            MenuPowers pow = menu.Pow!;
            MenuStash stash = menu.Stash!;
            MenuLog questlog = menu.Questlog!;
            MenuHUDLog hudlog = menu.Hudlog!;
            MessageEngine msg = SharedResources.Msg!;

            // if needed, create the save file structure
            Utils.CreateSaveDir(GameSlot);

            // remove items with zero quantity from inventory
            inv.Inventory[MenuInventory.Equipment].Clean();
            inv.Inventory[MenuInventory.Carried].Clean();

            StringBuilder ss = new StringBuilder();
            ss.Append(settings.PathUser).Append("saves/").Append(eset.Misc.SavePrefix).Append('/').Append(GameSlot).Append("/avatar.txt");

            // 语言差异适配：C++ �?std::ofstream::open 打开失败时不会抛异常，通过 is_open()/bad()
            // 判断状态；C# �?StreamWriter 构�?写入失败会抛�?IOException。这里用 try/catch
            // 包裹打开与写入过程，日志文案与原�?bad() 分支保持一致，并保证无论成功与否都会继�?
            // 执行后续�?stash 保存、SaveFOW、SaveExtendedItems 等逻辑（与原始控制流一致）�?
            StreamWriter? outfile;
            try
            {
                outfile = new StreamWriter(Filesystem.ConvertSlashes(ss.ToString()), append: false);
            }
            catch (IOException)
            {
                outfile = null;
            }

            if (outfile != null)
            {
                try
                {
                    // comment
                    outfile.Write("## flare-engine save file ##" + "\n");

                    // hero name
                    outfile.Write("name=" + pc.Stats.Name + "\n");

                    // permadeath
                    outfile.Write("permadeath=" + (pc.Stats.Permadeath ? "1" : "0") + "\n");

                    // hero visual option
                    outfile.Write("option=");

                    if (!string.IsNullOrEmpty(pc.Stats.GfxBaseOriginal))
                        outfile.Write(pc.Stats.GfxBaseOriginal);
                    else
                        outfile.Write(pc.Stats.GfxBase);

                    outfile.Write(",");

                    if (!string.IsNullOrEmpty(pc.Stats.GfxHeadOriginal))
                        outfile.Write(pc.Stats.GfxHeadOriginal);
                    else
                        outfile.Write(pc.Stats.GfxHead);

                    outfile.Write("," + pc.Stats.GfxPortrait + "\n");

                    // hero class
                    outfile.Write("class=" + pc.Stats.CharacterClass + "," + pc.Stats.CharacterSubclass + "\n");

                    // current experience
                    outfile.Write("xp=" + pc.Stats.Xp.ToString(CultureInfo.InvariantCulture) + "\n");

                    // hp and mp
                    if (eset.Misc.SaveHpmp) outfile.Write("hpmp=" + pc.Stats.Hp.ToString("G6", CultureInfo.InvariantCulture) + "," + pc.Stats.Mp.ToString("G6", CultureInfo.InvariantCulture) + "\n");

                    // stat spec
                    outfile.Write("build=");
                    for (int i = 0; i < eset.PrimaryStats.Stats.Count; ++i)
                    {
                        outfile.Write(pc.Stats.Primary[i].ToString(CultureInfo.InvariantCulture));
                        if (i < eset.PrimaryStats.Stats.Count - 1)
                            outfile.Write(",");
                    }
                    outfile.Write("\n");

                    // equipped gear
                    outfile.Write("equipped_quantity=" + inv.Inventory[MenuInventory.Equipment].GetQuantities() + "\n");
                    outfile.Write("equipped=" + inv.Inventory[MenuInventory.Equipment].GetItems() + "\n");

                    // active equipped set
                    outfile.Write("active_equipment_set=" + inv.ActiveEquipmentSet.ToString(CultureInfo.InvariantCulture) + "\n");

                    // carried items
                    outfile.Write("carried_quantity=" + inv.Inventory[MenuInventory.Carried].GetQuantities() + "\n");
                    outfile.Write("carried=" + inv.Inventory[MenuInventory.Carried].GetItems() + "\n");

                    // spawn point
                    outfile.Write("spawn=" + mapr.RespawnMap + "," + ((int)mapr.RespawnPoint.X).ToString(CultureInfo.InvariantCulture) + "," + ((int)mapr.RespawnPoint.Y).ToString(CultureInfo.InvariantCulture) + "\n");

                    // action bar
                    // NOTE we need to reset any bonus-modified powers in the action bar before writing
                    // we use menu->pow->setUnlockedPowers() after to restore the action bar state
                    pow.ClearActionBarBonusLevels();
                    outfile.Write("actionbar=");
                    for (uint i = 0; i < (uint)MenuActionBar.SlotMax; i++)
                    {
                        if (i < act.SlotsCount)
                        {
                            if (pc.Stats.Transformed) outfile.Write(act.HotkeysTemp[(int)i].ToString(CultureInfo.InvariantCulture));
                            else outfile.Write(act.Hotkeys[(int)i].ToString(CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            outfile.Write("0");
                        }
                        if (i < MenuActionBar.SlotMax - 1) outfile.Write(",");
                    }
                    outfile.Write("\n");
                    pow.SetUnlockedPowers();

                    //shapeshifter value
                    if (pc.Stats.TransformType == "untransform" || pc.Stats.TransformDuration != -1) outfile.Write("transformed=" + "\n");
                    else outfile.Write("transformed=" + pc.Stats.TransformType + "," + (pc.Stats.ManualUntransform ? "1" : "0") + "\n");

                    // restore hero powers
                    if (pc.Stats.Transformed && pc.HeroStats != null)
                    {
                        pc.Stats.PowersList = pc.HeroStats.PowersList;
                    }

                    // enabled powers
                    outfile.Write("powers=");
                    for (uint i = 0; i < pc.Stats.PowersList.Count; i++)
                    {
                        if (i < pc.Stats.PowersList.Count - 1)
                        {
                            if (pc.Stats.PowersList[(int)i] > 0)
                                outfile.Write(pc.Stats.PowersList[(int)i].ToString(CultureInfo.InvariantCulture) + ",");
                        }
                        else
                        {
                            if (pc.Stats.PowersList[(int)i] > 0)
                                outfile.Write(pc.Stats.PowersList[(int)i].ToString(CultureInfo.InvariantCulture));
                        }
                    }
                    outfile.Write("\n");

                    // restore transformed powers
                    if (pc.Stats.Transformed && pc.CharmedStats != null)
                    {
                        pc.Stats.PowersList = pc.CharmedStats.PowersList;
                    }

                    // campaign data
                    outfile.Write("campaign=" + camp.GetAll() + "\n");

                    outfile.Write("time_played=" + pc.TimePlayed.ToString(CultureInfo.InvariantCulture) + "\n");

                    // save the engine version for troubleshooting purposes
                    outfile.Write("engine_version=" + VersionInfo.Engine.GetString() + "\n");

                    // save the vendor buyback
                    if (eset.Misc.SaveBuyback)
                    {
                        MenuVendor vendor = menu.Vendor!;

                        foreach (KeyValuePair<string, ItemStorage> it in vendor.BuybackStock)
                        {
                            if (it.Value.Empty())
                                continue;

                            outfile.Write("buyback_item=" + it.Key + ";" + it.Value.GetItems() + "\n");
                            outfile.Write("buyback_quantity=" + it.Key + ";" + it.Value.GetQuantities() + "\n");
                        }
                    }

                    outfile.Write("questlog_dismissed=" + (!act.RequiresAttention[MenuActionBar.MenuLog] ? "1" : "0") + "\n");

                    outfile.Write("stash_tab=" + stash.GetTab().ToString(CultureInfo.InvariantCulture));

                    outfile.Write("\n");
                }
                catch (IOException)
                {
                    Utils.LogError("SaveLoad: Unable to save the game. No write access or disk is full!");
                }
                finally
                {
                    outfile.Dispose();
                }

                Platform.Instance.FsCommit();
            }

            // Save stashes
            for (int i = 0; i < stash.Tabs.Count; ++i)
            {
                // shared stashes are not saved for permadeath characters
                if (pc.Stats.Permadeath && !stash.Tabs[i].IsPrivate)
                    continue;

                ss.Clear();
                ss.Append(settings.PathUser).Append("saves/").Append(eset.Misc.SavePrefix);
                if (stash.Tabs[i].IsPrivate)
                    ss.Append('/').Append(GameSlot);
                ss.Append('/').Append(stash.Tabs[i].Filename);

                StreamWriter? tabOutfile;
                try
                {
                    tabOutfile = new StreamWriter(Filesystem.ConvertSlashes(ss.ToString()), append: false);
                }
                catch (IOException)
                {
                    tabOutfile = null;
                }

                if (tabOutfile != null)
                {
                    try
                    {
                        // comment
                        tabOutfile.Write("# flare-engine stash file: \"" + stash.Tabs[i].Id + "\"\n");

                        tabOutfile.Write("quantity=" + stash.Tabs[i].Stock.GetQuantities() + "\n");
                        tabOutfile.Write("item=" + stash.Tabs[i].Stock.GetItems() + "\n");

                        tabOutfile.Write("\n");
                    }
                    catch (IOException)
                    {
                        Utils.LogError("SaveLoad: Unable to save stash. No write access or disk is full!");
                    }
                    finally
                    {
                        tabOutfile.Dispose();
                    }

                    Platform.Instance.FsCommit();
                }
            }

            // save fog-of-war layers
            SaveFOW();

            SaveExtendedItems(SaveStorageItems);
            settings.PrevSaveSlot = GameSlot - 1;

            // display a log message saying that we saved the game
            questlog.Add(msg.Get("Game saved."), MenuLog.TypeMessages, WidgetLog.MsgNormal);
            hudlog.Add(msg.Get("Game saved."), MenuHUDLog.MsgNormal);
        }

        /// <summary>保存扩展物品（extended_items.txt）�</summary>
        public void SaveExtendedItems(bool saveStorageItems)
        {
            Settings settings = SharedResources.Settings!;
            EngineSettings eset = SharedResources.Eset!;
            ItemManager items = SharedGameResources.Items!;
            MenuManager? menu = SharedGameResources.Menu;

            // Save extended Items
            StringBuilder ss = new StringBuilder();
            ss.Append(settings.PathUser).Append("saves/").Append(eset.Misc.SavePrefix).Append("/extended_items.txt");

            StreamWriter? outfile;
            try
            {
                outfile = new StreamWriter(Filesystem.ConvertSlashes(ss.ToString()), append: false);
            }
            catch (IOException)
            {
                outfile = null;
            }

            if (outfile != null)
            {
                try
                {
                    for (int i = eset.Loot.ExtendedItemsOffset; i < items.Items.Count; ++i)
                    {
                        Item? item = items.Items[i];

                        if (item == null || item.Parent == 0)
                            continue;

                        bool itemInStorage = false;
                        if (saveStorageItems && menu != null)
                        {
                            if (menu.Inv != null && menu.Inv.Inventory[MenuInventory.Equipment].Contain(i, 1))
                            {
                                itemInStorage = true;
                            }
                            else if (menu.Inv != null && menu.Inv.Inventory[MenuInventory.Carried].Contain(i, 1))
                            {
                                itemInStorage = true;
                            }
                            else if (menu.Stash != null)
                            {
                                for (int j = 0; j < menu.Stash.Tabs.Count; ++j)
                                {
                                    if (menu.Stash.Tabs[j].Stock.Contain(i, 1))
                                    {
                                        itemInStorage = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (!itemInStorage && !item.IsForeign)
                            continue;

                        outfile.Write("[item]" + "\n");
                        outfile.Write("id=" + i.ToString(CultureInfo.InvariantCulture) + "," + item.Parent.ToString(CultureInfo.InvariantCulture) + "\n");
                        outfile.Write("level=" + item.Level.ToString(CultureInfo.InvariantCulture) + "\n");

                        if (item.Quality < items.ItemQualities.Count && !string.IsNullOrEmpty(items.ItemQualities[item.Quality].Name))
                        {
                            outfile.Write("quality=" + items.ItemQualities[item.Quality].Id + "\n");
                        }

                        if (item.RequiresLevel.Randomized)
                        {
                            outfile.Write("requires_level=" + item.RequiresLevel.Serialize(false) + "\n");
                        }

                        for (int j = 0; j < eset.PrimaryStats.Stats.Count; ++j)
                        {
                            if (item.RequiresStat[j].Randomized)
                            {
                                outfile.Write("requires_stat=" + eset.PrimaryStats.Stats[j].Id + "," + item.RequiresStat[j].Serialize(false) + "\n");
                            }
                        }

                        if (item.Price.Randomized)
                        {
                            outfile.Write("price=" + item.Price.Serialize(false) + "\n");
                        }

                        if (item.PriceSell.Randomized)
                        {
                            outfile.Write("price=" + item.PriceSell.Serialize(false) + "\n");
                        }

                        if (item.BaseAbs.Min.Randomized)
                        {
                            outfile.Write("abs_min=" + item.BaseAbs.Min.Serialize(false) + "\n");
                        }

                        if (item.BaseAbs.Max.Randomized)
                        {
                            outfile.Write("abs_max=" + item.BaseAbs.Max.Serialize(false) + "\n");
                        }

                        for (int j = 0; j < eset.DamageTypes.Types.Count; ++j)
                        {
                            if (item.BaseDmg[j].Min.Randomized)
                            {
                                outfile.Write("dmg_min=" + eset.DamageTypes.Types[j].Id + "," + item.BaseDmg[j].Min.Serialize(false) + "\n");
                            }
                            if (item.BaseDmg[j].Max.Randomized)
                            {
                                outfile.Write("dmg_max=" + eset.DamageTypes.Types[j].Id + "," + item.BaseDmg[j].Max.Serialize(false) + "\n");
                            }
                        }

                        for (int j = 0; j < item.Bonus.Count; ++j)
                        {
                            BonusData bonus = item.Bonus[j];

                            if (!bonus.IsExtended)
                                continue;

                            if (bonus.PowerId > 0)
                                outfile.Write("bonus_power_level=");
                            else
                                outfile.Write("bonus=");

                            if (bonus.Type == BonusData.Speed)
                                outfile.Write("speed");
                            else if (bonus.Type == BonusData.AttackSpeed)
                                outfile.Write("attack_speed");
                            else if (bonus.Type == BonusData.Stat)
                                outfile.Write(Stats.Key[bonus.Index]);
                            else if (bonus.Type == BonusData.DamageMin)
                                outfile.Write(eset.DamageTypes.Types[bonus.Index].Min);
                            else if (bonus.Type == BonusData.DamageMax)
                                outfile.Write(eset.DamageTypes.Types[bonus.Index].Max);
                            else if (bonus.Type == BonusData.ResistElement)
                                outfile.Write(eset.DamageTypes.Types[bonus.Index].Resist);
                            else if (bonus.Type == BonusData.PrimaryStat)
                                outfile.Write(eset.PrimaryStats.Stats[bonus.Index].Id);
                            else if (bonus.Type == BonusData.ResourceStat)
                                outfile.Write(eset.ResourceStats.Stats[bonus.Index].Ids[bonus.SubIndex]);
                            else if (bonus.Type == BonusData.PowerLevel)
                                outfile.Write(bonus.PowerId.ToString(CultureInfo.InvariantCulture));
                            else
                                continue;

                            outfile.Write("," + bonus.Value.Serialize(bonus.IsMultiplier));

                            outfile.Write("\n");
                        }
                        outfile.Write("\n");
                    }
                }
                catch (IOException)
                {
                    // 对应原始代码从未检查过 outfile.bad()：写入失败时静默忽略，不记录日志�?
                    // 也不改变调用方（SaveGame）后续继续执行的行为�?
                }
                finally
                {
                    outfile.Dispose();
                }
            }
        }

        /// <summary>
        /// 加载游戏时，如果可能则从文件中读取存档�?
        /// </summary>
        public void LoadGame()
        {
            if (GameSlot <= 0) return;

            Settings settings = SharedResources.Settings!;
            EngineSettings eset = SharedResources.Eset!;
            Avatar pc = SharedGameResources.Pc!;
            MenuManager menu = SharedGameResources.Menu!;
            MapRenderer mapr = SharedGameResources.Mapr!;
            CampaignManager camp = SharedGameResources.Camp!;
            PowerManager powers = SharedGameResources.Powers!;
            ModManager mods = SharedResources.Mods!;
            MenuInventory inv = menu.Inv!;

            // ensure that the save folder has all its sub-folders
            Utils.CreateSaveDir(GameSlot);

            float savedHp = 0;
            float savedMp = 0;
            int currency = 0;
            int stashTab = 0;
            Version saveVersion = new Version(VersionInfo.Min.X, VersionInfo.Min.Y, VersionInfo.Min.Z);

            using FileParser infile = new FileParser();
            List<PowerID> hotkeys = new List<PowerID>(Enumerable.Repeat(-1, MenuActionBar.SlotMax));

            StringBuilder ss = new StringBuilder();
            ss.Append(settings.PathUser).Append("saves/").Append(eset.Misc.SavePrefix).Append('/').Append(GameSlot).Append("/avatar.txt");

            if (infile.Open(ss.ToString(), !FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (infile.Key == "name") pc.Stats.Name = infile.Val;
                    else if (infile.Key == "permadeath")
                    {
                        pc.Stats.Permadeath = Parse.ToBool(infile.Val);
                    }
                    else if (infile.Key == "option")
                    {
                        pc.Stats.GfxBase = Parse.PopFirstString(ref infile.Val);
                        pc.Stats.GfxHead = Parse.PopFirstString(ref infile.Val);
                        pc.Stats.GfxPortrait = Parse.PopFirstString(ref infile.Val);

                        pc.Stats.CheckGfxPaths();
                    }
                    else if (infile.Key == "class")
                    {
                        pc.Stats.CharacterClass = Parse.PopFirstString(ref infile.Val);
                        pc.Stats.CharacterSubclass = Parse.PopFirstString(ref infile.Val);
                    }
                    else if (infile.Key == "xp")
                    {
                        pc.Stats.Xp = Parse.ToUnsignedLong(infile.Val);
                    }
                    else if (infile.Key == "hpmp")
                    {
                        savedHp = Parse.PopFirstFloat(ref infile.Val);
                        savedMp = Parse.PopFirstFloat(ref infile.Val);
                    }
                    else if (infile.Key == "build")
                    {
                        for (int i = 0; i < eset.PrimaryStats.Stats.Count; ++i)
                        {
                            pc.Stats.Primary[i] = Parse.PopFirstInt(ref infile.Val);
                            if (pc.Stats.Primary[i] < 0 || pc.Stats.Primary[i] > pc.Stats.MaxPointsPerStat)
                            {
                                Utils.LogInfo("SaveLoad: Primary stat value for '%s' is out of bounds, setting to zero.", eset.PrimaryStats.Stats[i].Id);
                                pc.Stats.Primary[i] = 0;
                            }
                        }
                    }
                    else if (infile.Key == "currency")
                    {
                        currency = Parse.ToInt(infile.Val);
                    }
                    else if (infile.Key == "equipped")
                    {
                        inv.Inventory[MenuInventory.Equipment].SetItems(infile.Val);
                        inv.Inventory[MenuInventory.Equipment].SetForeign(false);
                    }
                    else if (infile.Key == "equipped_quantity")
                    {
                        inv.Inventory[MenuInventory.Equipment].SetQuantities(infile.Val);
                    }
                    else if (infile.Key == "active_equipment_set")
                    {
                        inv.ApplyEquipmentSet((uint)Parse.ToInt(infile.Val));
                    }
                    else if (infile.Key == "carried")
                    {
                        inv.Inventory[MenuInventory.Carried].SetItems(infile.Val);
                        inv.Inventory[MenuInventory.Carried].SetForeign(false);
                    }
                    else if (infile.Key == "carried_quantity")
                    {
                        inv.Inventory[MenuInventory.Carried].SetQuantities(infile.Val);
                    }
                    else if (infile.Key == "spawn")
                    {
                        mapr.TeleportMapname = Parse.PopFirstString(ref infile.Val);
                        if (mapr.TeleportMapname != "" && Filesystem.FileExists(mods.Locate(mapr.TeleportMapname)))
                        {
                            mapr.TeleportDestination.X = (float)Parse.PopFirstInt(ref infile.Val) + 0.5f;
                            mapr.TeleportDestination.Y = (float)Parse.PopFirstInt(ref infile.Val) + 0.5f;
                            mapr.Teleportation = true;
                            // prevent spawn.txt from putting us on the starting map
                            mapr.ClearEvents();
                        }
                        else
                        {
                            Utils.LogError("SaveLoad: Unable to find %s, loading maps/spawn.txt", mapr.TeleportMapname);
                            mapr.TeleportMapname = "maps/spawn.txt";
                            mapr.TeleportDestination.X = 0.5f;
                            mapr.TeleportDestination.Y = 0.5f;
                            mapr.Teleportation = true;
                        }
                    }
                    else if (infile.Key == "actionbar")
                    {
                        for (int i = 0; i < MenuActionBar.SlotMax; i++)
                        {
                            hotkeys[i] = powers.VerifyID(Parse.PopFirstInt(ref infile.Val), infile, PowerManager.AllowZeroId);
                        }
                        menu.Act!.Set(hotkeys, !MenuActionBar.SetSkipEmpty);
                    }
                    else if (infile.Key == "transformed")
                    {
                        pc.Stats.TransformType = Parse.PopFirstString(ref infile.Val);
                        if (pc.Stats.TransformType != "")
                        {
                            pc.Stats.TransformDuration = -1;
                            pc.Stats.ManualUntransform = Parse.ToBool(Parse.PopFirstString(ref infile.Val));
                        }
                    }
                    else if (infile.Key == "powers")
                    {
                        string power;
                        while ((power = Parse.PopFirstString(ref infile.Val)) != "")
                        {
                            PowerID powerId = powers.VerifyID(Parse.ToInt(power), infile, !PowerManager.AllowZeroId);
                            if (powerId > 0)
                                pc.Stats.PowersList.Add(powerId);
                        }
                    }
                    else if (infile.Key == "campaign") camp.SetAll(infile.Val);
                    else if (infile.Key == "time_played") pc.TimePlayed = Parse.ToUnsignedLong(infile.Val);
                    else if (infile.Key == "engine_version") saveVersion.SetFromString(infile.Val);
                    else if (eset.Misc.SaveBuyback && infile.Key == "buyback_item")
                    {
                        string npcFilename = Parse.PopFirstString(ref infile.Val, ';');
                        if (!string.IsNullOrEmpty(npcFilename))
                        {
                            if (!menu.Vendor!.BuybackStock.TryGetValue(npcFilename, out ItemStorage? buybackEntry))
                            {
                                buybackEntry = new ItemStorage();
                                menu.Vendor.BuybackStock[npcFilename] = buybackEntry;
                            }
                            buybackEntry.Init(NPC.VendorMaxStock);
                            buybackEntry.SetItems(infile.Val);
                        }
                    }
                    else if (eset.Misc.SaveBuyback && infile.Key == "buyback_quantity")
                    {
                        string npcFilename = Parse.PopFirstString(ref infile.Val, ';');
                        if (!string.IsNullOrEmpty(npcFilename))
                        {
                            if (!menu.Vendor!.BuybackStock.TryGetValue(npcFilename, out ItemStorage? buybackEntry))
                            {
                                buybackEntry = new ItemStorage();
                                menu.Vendor.BuybackStock[npcFilename] = buybackEntry;
                            }
                            buybackEntry.Init(NPC.VendorMaxStock);
                            buybackEntry.SetQuantities(infile.Val);
                        }
                    }
                    else if (infile.Key == "questlog_dismissed") pc.QuestlogDismissed = Parse.ToBool(infile.Val);
                    else if (infile.Key == "stash_tab") stashTab = Parse.ToInt(infile.Val);
                }

                infile.Close();
            }
            else Utils.LogError("SaveLoad: Unable to open %s!", ss.ToString());

            // set starting values for primary stats based on class
            EngineSettings.HeroClassesSettings.HeroClass? pcClass;
            pcClass = eset.HeroClasses.GetByName(pc.Stats.CharacterClass);
            if (pcClass != null)
            {
                for (int i = 0; i < eset.PrimaryStats.Stats.Count; ++i)
                {
                    pc.Stats.PrimaryStarting[i] = pcClass.Primary[i] + 1;
                }
            }

            // add legacy currency to inventory
            inv.AddCurrency(currency);

            // apply stats, inventory, and powers
            ApplyPlayerData();

            // trigger passive effects here? Saved HP/MP values might depend on passively boosted HP/MP
            // powers->activatePassives(pc->stats);
            if (eset.Misc.SaveHpmp && savedHp != 0)
            {
                if (savedHp < 0 || savedHp > pc.Stats.Get(global::FlareEngine.Stats.HpMax))
                {
                    Utils.LogError("SaveLoad: HP value is out of bounds, setting to maximum");
                    pc.Stats.Hp = pc.Stats.Get(global::FlareEngine.Stats.HpMax);
                }
                else pc.Stats.Hp = savedHp;

                if (savedMp < 0 || savedMp > pc.Stats.Get(global::FlareEngine.Stats.MpMax))
                {
                    Utils.LogError("SaveLoad: MP value is out of bounds, setting to maximum");
                    pc.Stats.Mp = pc.Stats.Get(global::FlareEngine.Stats.MpMax);
                }
                else pc.Stats.Mp = savedMp;
            }
            else
            {
                pc.Stats.Hp = pc.Stats.Get(global::FlareEngine.Stats.HpMax);
                pc.Stats.Mp = pc.Stats.Get(global::FlareEngine.Stats.MpMax);
            }

            if (saveVersion != VersionInfo.Engine)
                Utils.LogInfo("SaveLoad: Warning! Engine version of save file (%s) does not match current engine version (%s). Be on the lookout for bugs.", saveVersion.GetString(), VersionInfo.Engine.GetString());

            // reset character menu
            menu.Chr!.RefreshStats();

            LoadPowerTree();

            // disable the shared stash for permadeath characters
            menu.Stash!.EnableSharedTab(pc.Stats.Permadeath);

            menu.Stash.SetTab(stashTab);

            pc.LoadAnimations();
        }

        /// <summary>
        /// 加载一个职业定义，index 为职业索引�?
        /// </summary>
        public void LoadClass(int index)
        {
            if (GameSlot <= 0) return;

            EngineSettings eset = SharedResources.Eset!;
            Avatar pc = SharedGameResources.Pc!;
            MenuManager menu = SharedGameResources.Menu!;
            PowerManager powers = SharedGameResources.Powers!;
            CampaignManager camp = SharedGameResources.Camp!;
            MenuInventory inv = menu.Inv!;

            if (index < 0 || (uint)index >= eset.HeroClasses.Classes.Count)
            {
                Utils.LogError("SaveLoad: Class index out of bounds.");
                return;
            }

            EngineSettings.HeroClassesSettings.HeroClass heroClass = eset.HeroClasses.Classes[index];

            // name
            pc.Stats.CharacterClass = heroClass.Name;

            // stat points
            for (int i = 0; i < eset.PrimaryStats.Stats.Count; ++i)
            {
                // Avatar::init() sets primary stats to 1, so we add to that here
                pc.Stats.Primary[i] += heroClass.Primary[i];
                pc.Stats.PrimaryStarting[i] = pc.Stats.Primary[i];
            }

            // inventory
            inv.AddCurrency(heroClass.Currency);

            ItemStack stack = new ItemStack();

            string equipment = heroClass.Equipment;
            while (!string.IsNullOrEmpty(equipment))
            {
                stack = Parse.ToItemQuantityPair(Parse.PopFirstString(ref equipment));
                int equipSlot = inv.GetEquipSlotFromItem(stack.Item, MenuInventory.OnlyEmptySlots);
                inv.Add(stack, MenuInventory.Equipment, equipSlot, !MenuInventory.AddPlaySound, !MenuInventory.AddAutoEquip);
            }

            for (int i = 0; i < heroClass.EquipmentSets.Count; ++i)
            {
                inv.ApplyEquipmentSet(heroClass.EquipmentSets[i].SetId);
                string equipmentSet = heroClass.EquipmentSets[i].Items;
                while (!string.IsNullOrEmpty(equipmentSet))
                {
                    stack = Parse.ToItemQuantityPair(Parse.PopFirstString(ref equipmentSet));
                    int equipSlot = inv.GetEquipSlotFromItem(stack.Item, MenuInventory.OnlyEmptySlots);
                    inv.Add(stack, MenuInventory.Equipment, equipSlot, !MenuInventory.AddPlaySound, !MenuInventory.AddAutoEquip);
                }

            }
            inv.ApplyEquipmentSet(1);

            string carried = heroClass.Carried;
            while (!string.IsNullOrEmpty(carried))
            {
                stack = Parse.ToItemQuantityPair(Parse.PopFirstString(ref carried));
                inv.Add(stack, MenuInventory.Carried, ItemStorage.NoSlot, !MenuInventory.AddPlaySound, !MenuInventory.AddAutoEquip);
            }

            // powers & action bar
            for (int i = 0; i < heroClass.Powers.Count; ++i)
            {
                PowerID powerId = powers.VerifyID(heroClass.Powers[i], null, !PowerManager.AllowZeroId);
                heroClass.Powers[i] = powerId;
                if (powerId > 0)
                    pc.Stats.PowersList.Add(powerId);
            }
            for (int i = 0; i < heroClass.Hotkeys.Count; ++i)
            {
                heroClass.Hotkeys[i] = powers.VerifyID(heroClass.Hotkeys[i], null, PowerManager.AllowZeroId);
            }

            menu.Act!.Set(heroClass.Hotkeys, !MenuActionBar.SetSkipEmpty);

            // campaign statuses
            for (int i = 0; i < heroClass.Statuses.Count; ++i)
            {
                StatusID classStatus = camp.RegisterStatus(heroClass.Statuses[i]);
                camp.SetStatus(classStatus);
            }

            // apply stats, inventory, and powers
            ApplyPlayerData();

            // reset character menu
            menu.Chr!.RefreshStats();

            LoadPowerTree();
        }

        /// <summary>
        /// 用于在开始新游戏时加载仓库�?
        /// </summary>
        public void LoadStash()
        {
            // Load stash
            Settings settings = SharedResources.Settings!;
            EngineSettings eset = SharedResources.Eset!;
            Avatar pc = SharedGameResources.Pc!;
            MenuManager menu = SharedGameResources.Menu!;
            MenuStash stash = menu.Stash!;

            using FileParser infile = new FileParser();
            StringBuilder ss = new StringBuilder();

            for (int i = 0; i < stash.Tabs.Count; ++i)
            {
                // shared stashes are not loaded for permadeath characters
                if (pc.Stats.Permadeath && !stash.Tabs[i].IsPrivate)
                    continue;

                ss.Clear();
                ss.Append(settings.PathUser).Append("saves/").Append(eset.Misc.SavePrefix);
                if (stash.Tabs[i].IsPrivate)
                    ss.Append('/').Append(GameSlot);
                ss.Append('/').Append(stash.Tabs[i].Filename);

                if (infile.Open(ss.ToString(), !FileParser.ModFile, FileParser.ErrorNone))
                {
                    while (infile.Next())
                    {
                        if (infile.Key == "item")
                        {
                            stash.Tabs[i].Stock.SetItems(infile.Val);
                            if (stash.Tabs[i].IsPrivate)
                            {
                                stash.Tabs[i].Stock.SetForeign(false);
                            }
                        }
                        else if (infile.Key == "quantity")
                        {
                            stash.Tabs[i].Stock.SetQuantities(infile.Val);
                        }
                    }
                    infile.Close();
                }
                else Utils.LogInfo("SaveLoad: Could not open stash file '%s'. This may be because it hasn't been created yet.", ss.ToString());

                stash.Tabs[i].Stock.Clean();
            }
        }

        /// <summary>
        /// 保存雾区（战争迷雾）图层�?
        /// </summary>
        public void SaveFOW()
        {
            MapRenderer mapr = SharedGameResources.Mapr!;
            FogOfWar fow = SharedGameResources.Fow!;

            // Save fow dark layer
            if (mapr.Fogofwar != 0 && mapr.SaveFogofwar && !string.IsNullOrEmpty(mapr.Filename) && fow.DarkLayerId < mapr.Layernames.Count)
            {
                string fowFilename = mapr.GetFOWFilename();

                StreamWriter? outfile;
                try
                {
                    outfile = new StreamWriter(Filesystem.ConvertSlashes(fowFilename), append: false);
                }
                catch (IOException)
                {
                    outfile = null;
                }

                if (outfile != null)
                {
                    try
                    {
                        outfile.Write("# " + mapr.Filename + "\n");
                        outfile.Write("[layer]" + "\n");
                        outfile.Write("type=" + mapr.Layernames[fow.DarkLayerId] + "\n");
                        outfile.Write("data=" + "\n");

                        string layer = "";
                        for (int line = 0; line < mapr.H; line++)
                        {
                            StringBuilder mapRow = new StringBuilder();
                            for (int tile = 0; tile < mapr.W; tile++)
                            {
                                ushort val = mapr.Layers[fow.DarkLayerId][tile][line];
                                mapRow.Append(val.ToString(CultureInfo.InvariantCulture)).Append(',');
                            }
                            layer += mapRow.ToString();
                            layer += '\n';
                        }
                        layer = layer.Substring(0, layer.Length - 2);
                        layer += '\n';
                        outfile.Write(layer + "\n");

                        // 对应原始代码中的 outfile.bad() 检查：通过 catch (IOException) 实现�?
                    }
                    catch (IOException)
                    {
                        Utils.LogError("SaveLoad: Unable to save map data. No write access or disk is full!");
                    }
                    finally
                    {
                        outfile.Dispose();
                    }

                    Platform.Instance.FsCommit();
                }
            }
        }

        /// <summary>
        /// 读档或选择新职业后，进行最终计算�?
        /// </summary>
        private void ApplyPlayerData()
        {
            Avatar pc = SharedGameResources.Pc!;
            MenuManager menu = SharedGameResources.Menu!;
            MenuInventory inv = menu.Inv!;

            inv.FillEquipmentSlots();

            // remove items with zero quantity from inventory
            inv.Inventory[MenuInventory.Equipment].Clean();
            inv.Inventory[MenuInventory.Carried].Clean();

            // Load stash
            LoadStash();

            // initialize vars
            pc.Stats.Recalc();
            pc.Stats.LoadHeroSfx();
            inv.ApplyEquipment();
            pc.Stats.Logic(); // run stat logic once to apply items bonuses

            // just for aesthetics, turn the hero to face the camera
            pc.Stats.Direction = 6;

            // set up MenuTalker for this hero
            menu.Talker!.SetHero(pc.Stats);

            // load sounds (gender specific)
            pc.LoadSounds();

            // apply power upgrades
            menu.Pow!.SetUnlockedPowers();
        }

        private void LoadPowerTree()
        {
            EngineSettings eset = SharedResources.Eset!;
            Avatar pc = SharedGameResources.Pc!;
            MenuManager menu = SharedGameResources.Menu!;

            EngineSettings.HeroClassesSettings.HeroClass? pcClass;
            pcClass = eset.HeroClasses.GetByName(pc.Stats.CharacterClass);
            if (pcClass != null && !string.IsNullOrEmpty(pcClass.PowerTree))
            {
                menu.Pow!.LoadPowerTree(pcClass.PowerTree);
                return;
            }

            // fall back to the default power tree
            menu.Pow!.LoadPowerTree("powers/trees/default.txt");
        }
    }
}
