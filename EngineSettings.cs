// <自动生成> 对应 C++ 源文件：EngineSettings.h + EngineSettings.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;
using Utils = FlareEngine.Utils;

namespace FlareEngine
{
    /// <summary>
    /// EngineSettings
    ///
    /// 引擎级别的可配置数据（游戏规则、UI 数值、职业/伤害类型定义等），通过一系列嵌套的
    /// "分组"类型进行组织，每个分组都从 engine/*.txt 中加载数据。
    ///
    /// 命名说明：C++ 原始代码中，EngineSettings 的每个嵌套类型（如 <c>class Misc</c>）都与一个
    /// 同名的实例成员（如 <c>Misc misc;</c>）共存，这在大小写不敏感的语境下没有问题，但转换为
    /// C# 的帕斯卡命名后会产生"属性名与其自身类型名相同"的合法但极易混淆的写法。为了让代码更清晰、
    /// 并统一采用本次迁移中"类型名与实例属性名不同"的约定，所有嵌套类型都添加了 "Settings" 后缀
    /// （例如 Misc -> MiscSettings，Tileset -> TilesetSettings，与 Utils.cs/Settings.cs 中已经
    /// 使用的 <c>EngineSettings.TilesetSettings.TilesetIsometric</c> 保持一致）。
    /// 类似地，各分组内部的 <c>std::vector&lt;T&gt; list;</c> 成员如果直接命名为 "List"，会与
    /// C# 的 <c>System.Collections.Generic.List&lt;T&gt;</c> 类型名冲突，因此按各自的语义分别
    /// 重命名为 Flags/Stats/Classes/Types 等（详见各分组的说明）。
    /// </summary>
    public class EngineSettings : IDisposable
    {
        public EngineSettings()
        {
        }

        /// <summary>
        /// 对应 C++ 的 <c>~EngineSettings()</c>。采用与 FontEngine 一致的 IDisposable 模式
        /// （而非终结器），以保持确定性的清理时机；调用方需要在原始 `delete eset;` 位置改为
        /// 调用 `eset.Dispose();`（将在转换到 main.cpp 对应单元时接入）。
        /// </summary>
        public void Dispose()
        {
            Utils.LogInfo("Cleaning up: EngineSettings");
            GC.SuppressFinalize(this);
        }

        public void Load()
        {
            Misc.Load();
            Resolutions.Load();
            Gameplay.Load();
            Combat.Load();
            EquipFlags.Load();
            PrimaryStats.Load();
            HeroClasses.Load(); // depends on primary_stats
            DamageTypes.Load();
            DeathPenalty.Load();
            Tooltips.Load();
            Loot.Load(); // depends on misc
            Tileset.Load();
            Widgets.Load();
            Xp.Load();
            NumberFormat.Load();
            ResourceStats.Load();
        }

        public MiscSettings Misc { get; } = new MiscSettings();
        public ResolutionsSettings Resolutions { get; } = new ResolutionsSettings();
        public GameplaySettings Gameplay { get; } = new GameplaySettings();
        public CombatSettings Combat { get; } = new CombatSettings();
        public EquipFlagsSettings EquipFlags { get; } = new EquipFlagsSettings();
        public PrimaryStatsSettings PrimaryStats { get; } = new PrimaryStatsSettings();
        public HeroClassesSettings HeroClasses { get; } = new HeroClassesSettings();
        public DamageTypesSettings DamageTypes { get; } = new DamageTypesSettings();
        public DeathPenaltySettings DeathPenalty { get; } = new DeathPenaltySettings();
        public TooltipsSettings Tooltips { get; } = new TooltipsSettings();
        public LootSettings Loot { get; } = new LootSettings();
        public TilesetSettings Tileset { get; } = new TilesetSettings();
        public WidgetsSettings Widgets { get; } = new WidgetsSettings();
        public XpTableSettings Xp { get; } = new XpTableSettings();
        public NumberFormatSettings NumberFormat { get; } = new NumberFormatSettings();
        public ResourceStatsSettings ResourceStats { get; } = new ResourceStatsSettings();

        public class MiscSettings
        {
            public const int SaveOnstashNone = 0;
            public const int SaveOnstashPrivate = 1;
            public const int SaveOnstashShared = 2;
            public const int SaveOnstashAll = 3;

            public bool SaveHpmp;
            public int CorpseTimeout;
            public bool CorpseTimeoutEnabled;
            public bool SellWithoutVendor;
            public int AimAssist;
            public string WindowTitle = "";
            public string SavePrefix = "";
            public int SoundFalloff;
            public float PartyExpPercentage;
            public bool EnableAllyCollision;
            public bool EnableAllyCollisionAi;
            public ItemID CurrencyId;
            public float InteractRange;
            public bool MenusPause;
            public bool SaveOnload;
            public bool SaveOnexit;
            public bool SavePosOnexit;
            public bool SaveOncutscene;
            public int SaveOnstash;
            public bool SaveAnywhere;
            public float CameraSpeed;
            public bool SaveBuyback;
            public bool KeepBuybackOnMapChange;
            public string SfxUnableToCast = "";
            public bool CombatAbortsNpcInteract;
            public ushort Fogofwar;
            public bool SaveFogofwar;
            public bool MouseMoveEnabled;
            public float MouseMoveDeadzoneMoving;
            public float MouseMoveDeadzoneNotMoving;
            public bool PassiveTriggerEffectStacking;
            public byte FadeWallAlpha;
            public float RaycastResolution;

            public void Load()
            {
                Settings settings = SharedResources.Settings!;

                // reset to defaults
                SaveHpmp = false;
                CorpseTimeout = 60 * settings.MaxFramesPerSec;
                CorpseTimeoutEnabled = true;
                SellWithoutVendor = true;
                AimAssist = 0;
                WindowTitle = "Flare";
                SavePrefix = "";
                SoundFalloff = 15;
                PartyExpPercentage = 100;
                EnableAllyCollision = true;
                EnableAllyCollisionAi = true;
                CurrencyId = 1;
                InteractRange = 3;
                MenusPause = false;
                SaveOnload = true;
                SaveOnexit = true;
                SavePosOnexit = false;
                SaveOncutscene = true;
                SaveOnstash = SaveOnstashAll;
                SaveAnywhere = false;
                CameraSpeed = 10f * ((float)settings.MaxFramesPerSec / Settings.LogicFps);
                SaveBuyback = true;
                KeepBuybackOnMapChange = true;
                SfxUnableToCast = "";
                CombatAbortsNpcInteract = true;
                Fogofwar = 0;
                SaveFogofwar = false;
                MouseMoveEnabled = true;
                MouseMoveDeadzoneMoving = 0.25f;
                MouseMoveDeadzoneNotMoving = 0.75f;
                PassiveTriggerEffectStacking = false;
                FadeWallAlpha = 63;
                RaycastResolution = 0.1f;

                using FileParser infile = new FileParser();
                // @CLASS EngineSettings: Misc|Description of engine/misc.txt
                if (infile.Open("engine/misc.txt", FileParser.ModFile, FileParser.ErrorNormal))
                {
                    while (infile.Next())
                    {
                        if (infile.Key == "save_hpmp")
                            SaveHpmp = Parse.ToBool(infile.Val);
                        else if (infile.Key == "corpse_timeout")
                            CorpseTimeout = Parse.ToDuration(infile.Val);
                        else if (infile.Key == "sell_without_vendor")
                            SellWithoutVendor = Parse.ToBool(infile.Val);
                        else if (infile.Key == "aim_assist")
                            AimAssist = Parse.ToInt(infile.Val);
                        else if (infile.Key == "window_title")
                            WindowTitle = infile.Val;
                        else if (infile.Key == "save_prefix")
                            SavePrefix = infile.Val;
                        else if (infile.Key == "sound_falloff")
                            SoundFalloff = Parse.ToInt(infile.Val);
                        else if (infile.Key == "party_exp_percentage")
                            PartyExpPercentage = Parse.ToFloat(infile.Val);
                        else if (infile.Key == "enable_ally_collision")
                            EnableAllyCollision = Parse.ToBool(infile.Val);
                        else if (infile.Key == "enable_ally_collision_ai")
                            EnableAllyCollisionAi = Parse.ToBool(infile.Val);
                        else if (infile.Key == "currency_id")
                        {
                            CurrencyId = Parse.ToItemID(infile.Val);
                            if (CurrencyId < 1)
                            {
                                CurrencyId = 1;
                                Utils.LogError("EngineSettings: Currency ID below the minimum allowed value. Resetting it to %d", CurrencyId);
                            }
                        }
                        else if (infile.Key == "interact_range")
                            InteractRange = Parse.ToFloat(infile.Val);
                        else if (infile.Key == "menus_pause")
                            MenusPause = Parse.ToBool(infile.Val);
                        else if (infile.Key == "save_onload")
                            SaveOnload = Parse.ToBool(infile.Val);
                        else if (infile.Key == "save_onexit")
                            SaveOnexit = Parse.ToBool(infile.Val);
                        else if (infile.Key == "save_pos_onexit")
                            SavePosOnexit = Parse.ToBool(infile.Val);
                        else if (infile.Key == "save_oncutscene")
                            SaveOncutscene = Parse.ToBool(infile.Val);
                        else if (infile.Key == "save_onstash")
                        {
                            if (infile.Val == "private")
                                SaveOnstash = SaveOnstashPrivate;
                            else if (infile.Val == "shared")
                                SaveOnstash = SaveOnstashShared;
                            else
                            {
                                if (Parse.ToBool(infile.Val))
                                    SaveOnstash = SaveOnstashAll;
                                else
                                    SaveOnstash = SaveOnstashNone;
                            }
                        }
                        else if (infile.Key == "save_anywhere")
                            SaveAnywhere = Parse.ToBool(infile.Val);
                        else if (infile.Key == "camera_speed")
                        {
                            CameraSpeed = Parse.ToFloat(infile.Val);
                            if (CameraSpeed <= 0)
                                CameraSpeed = 1;
                        }
                        else if (infile.Key == "save_buyback")
                            SaveBuyback = Parse.ToBool(infile.Val);
                        else if (infile.Key == "keep_buyback_on_map_change")
                            KeepBuybackOnMapChange = Parse.ToBool(infile.Val);
                        else if (infile.Key == "sfx_unable_to_cast")
                            SfxUnableToCast = infile.Val;
                        else if (infile.Key == "combat_aborts_npc_interact")
                            CombatAbortsNpcInteract = Parse.ToBool(infile.Val);
                        else if (infile.Key == "fogofwar")
                            Fogofwar = (ushort)Parse.ToInt(infile.Val);
                        else if (infile.Key == "save_fogofwar")
                            SaveFogofwar = Parse.ToBool(infile.Val);
                        else if (infile.Key == "mouse_move_enabled")
                        {
                            MouseMoveEnabled = Parse.ToBool(infile.Val);
                        }
                        else if (infile.Key == "mouse_move_deadzone")
                        {
                            MouseMoveDeadzoneMoving = Parse.PopFirstFloat(ref infile.Val);
                            MouseMoveDeadzoneNotMoving = Parse.PopFirstFloat(ref infile.Val);
                        }
                        else if (infile.Key == "passive_trigger_effect_stacking")
                        {
                            PassiveTriggerEffectStacking = Parse.ToBool(infile.Val);
                        }
                        else if (infile.Key == "fade_wall_alpha")
                        {
                            FadeWallAlpha = (byte)Parse.PopFirstInt(ref infile.Val);
                        }
                        else if (infile.Key == "raycast_resolution")
                        {
                            RaycastResolution = Parse.ToFloat(infile.Val);
                            if (RaycastResolution <= 0)
                            {
                                RaycastResolution = 0.1f;
                                infile.Error("EngineSettings: raycast_resolution must be greater than 0.");
                            }
                        }
                        else infile.Error("EngineSettings: '%s' is not a valid key.", infile.Key);
                    }
                    infile.Close();
                }

                if (SavePrefix == "")
                {
                    Utils.LogError("EngineSettings: save_prefix not found in engine/misc.txt, setting to 'default'. This may cause save file conflicts between games that have no save_prefix.");
                    SavePrefix = "default";
                }

                if (SaveBuyback && !KeepBuybackOnMapChange)
                {
                    Utils.LogError("EngineSettings: Warning, save_buyback=true is ignored when keep_buyback_on_map_change=false.");
                    SaveBuyback = false;
                }

                if (CorpseTimeout <= 0)
                {
                    CorpseTimeoutEnabled = false;
                    CorpseTimeout = settings.MaxFramesPerSec + 1;
                }
            }
        }

        public class ResolutionsSettings
        {
            public ushort FrameW;
            public ushort FrameH;
            public ushort IconSize;
            public ushort MinScreenW;
            public ushort MinScreenH;
            public List<ushort> VirtualHeights = new List<ushort>();
            public float VirtualDpi;
            public bool IgnoreTextureFilter;

            public void Load()
            {
                Settings settings = SharedResources.Settings!;

                FrameW = 0;
                FrameH = 0;
                IconSize = 0;
                MinScreenW = 640;
                MinScreenH = 480;
                VirtualHeights.Clear();
                VirtualDpi = 0;
                IgnoreTextureFilter = false;

                using FileParser infile = new FileParser();
                // @CLASS EngineSettings: Resolution|Description of engine/resolutions.txt
                if (infile.Open("engine/resolutions.txt", FileParser.ModFile, FileParser.ErrorNormal))
                {
                    while (infile.Next())
                    {
                        if (infile.Key == "menu_frame_width")
                            FrameW = (ushort)Parse.ToInt(infile.Val);
                        else if (infile.Key == "menu_frame_height")
                            FrameH = (ushort)Parse.ToInt(infile.Val);
                        else if (infile.Key == "icon_size")
                            IconSize = (ushort)Parse.ToInt(infile.Val);
                        else if (infile.Key == "required_width")
                        {
                            MinScreenW = (ushort)Parse.ToInt(infile.Val);
                        }
                        else if (infile.Key == "required_height")
                        {
                            MinScreenH = (ushort)Parse.ToInt(infile.Val);
                        }
                        else if (infile.Key == "virtual_height")
                        {
                            VirtualHeights.Clear();
                            string vHeight = Parse.PopFirstString(ref infile.Val);
                            while (!string.IsNullOrEmpty(vHeight))
                            {
                                int testVHeight = Parse.ToInt(vHeight);
                                if (testVHeight <= 0)
                                {
                                    Utils.LogError("EngineSettings: virtual_height must be greater than zero.");
                                }
                                else
                                {
                                    VirtualHeights.Add((ushort)testVHeight);
                                }
                                vHeight = Parse.PopFirstString(ref infile.Val);
                            }

                            VirtualHeights.Sort();

                            if (VirtualHeights.Count > 0)
                            {
                                settings.ViewH = VirtualHeights[^1];
                            }

                            settings.ViewHHalf = (ushort)(settings.ViewH / 2);
                        }
                        else if (infile.Key == "virtual_dpi")
                        {
                            VirtualDpi = Parse.ToFloat(infile.Val);
                        }
                        else if (infile.Key == "ignore_texture_filter")
                        {
                            IgnoreTextureFilter = Parse.ToBool(infile.Val);
                        }
                        else infile.Error("EngineSettings: '%s' is not a valid key.", infile.Key);
                    }
                    infile.Close();
                }

                // prevent the window from being too small
                if (settings.ScreenW < MinScreenW) settings.ScreenW = MinScreenW;
                if (settings.ScreenH < MinScreenH) settings.ScreenH = MinScreenH;

                // icon size can not be zero, so we set a default of 32x32, which is fantasycore's icon size
                if (IconSize == 0)
                {
                    Utils.LogError("EngineSettings: icon_size is undefined. Setting it to 32.");
                    IconSize = 32;
                }
            }
        }

        public class GameplaySettings
        {
            public bool EnablePlaygame;

            public void Load()
            {
                EnablePlaygame = false;

                using FileParser infile = new FileParser();
                // @CLASS EngineSettings: Gameplay|Description of engine/gameplay.txt
                if (infile.Open("engine/gameplay.txt", FileParser.ModFile, FileParser.ErrorNormal))
                {
                    while (infile.Next())
                    {
                        if (infile.Key == "enable_playgame")
                        {
                            EnablePlaygame = Parse.ToBool(infile.Val);
                        }
                        else infile.Error("EngineSettings: '%s' is not a valid key.", infile.Key);
                    }
                    infile.Close();
                }
            }
        }

        public class CombatSettings
        {
            public const int ResourceRoundMethodNone = 0;
            public const int ResourceRoundMethodRound = 1;
            public const int ResourceRoundMethodFloor = 2;
            public const int ResourceRoundMethodCeil = 3;

            public float MinAbsorb;
            public float MaxAbsorb;
            public float MinResist;
            public float MaxResist;
            public float MinBlock;
            public float MaxBlock;
            public float MinAvoidance;
            public float MaxAvoidance;
            public float MinMissDamage;
            public float MaxMissDamage;
            public float MinCritDamage;
            public float MaxCritDamage;
            public float MinOverhitDamage;
            public float MaxOverhitDamage;
            public ushort ResourceRoundMethod;
            public bool OffscreenEnemyEncounters;

            public void Load()
            {
                MinAbsorb = 0;
                MaxAbsorb = 100;
                MinResist = 0;
                MaxResist = 100;
                MinBlock = 0;
                MaxBlock = 100;
                MinAvoidance = 0;
                MaxAvoidance = 100;
                MinMissDamage = 0;
                MaxMissDamage = 0;
                MinCritDamage = 200;
                MaxCritDamage = 200;
                MinOverhitDamage = 100;
                MaxOverhitDamage = 100;
                ResourceRoundMethod = ResourceRoundMethodRound;
                OffscreenEnemyEncounters = false;

                using FileParser infile = new FileParser();
                // @CLASS EngineSettings: Combat|Description of engine/combat.txt
                if (infile.Open("engine/combat.txt", FileParser.ModFile, FileParser.ErrorNormal))
                {
                    while (infile.Next())
                    {
                        if (infile.Key == "absorb_percent")
                        {
                            MinAbsorb = Parse.PopFirstFloat(ref infile.Val);
                            MaxAbsorb = Parse.PopFirstFloat(ref infile.Val);
                            MaxAbsorb = Math.Max(MaxAbsorb, MinAbsorb);
                        }
                        else if (infile.Key == "resist_percent")
                        {
                            MinResist = Parse.PopFirstFloat(ref infile.Val);
                            MaxResist = Parse.PopFirstFloat(ref infile.Val);
                            MaxResist = Math.Max(MaxResist, MinResist);
                        }
                        else if (infile.Key == "block_percent")
                        {
                            MinBlock = Parse.PopFirstFloat(ref infile.Val);
                            MaxBlock = Parse.PopFirstFloat(ref infile.Val);
                            MaxBlock = Math.Max(MaxBlock, MinBlock);
                        }
                        else if (infile.Key == "avoidance_percent")
                        {
                            MinAvoidance = Parse.PopFirstFloat(ref infile.Val);
                            MaxAvoidance = Parse.PopFirstFloat(ref infile.Val);
                            MaxAvoidance = Math.Max(MaxAvoidance, MinAvoidance);
                        }
                        else if (infile.Key == "miss_damage_percent")
                        {
                            MinMissDamage = Parse.PopFirstFloat(ref infile.Val);
                            MaxMissDamage = Parse.PopFirstFloat(ref infile.Val);
                            MaxMissDamage = Math.Max(MaxMissDamage, MinMissDamage);
                        }
                        else if (infile.Key == "crit_damage_percent")
                        {
                            MinCritDamage = Parse.PopFirstFloat(ref infile.Val);
                            MaxCritDamage = Parse.PopFirstFloat(ref infile.Val);
                            MaxCritDamage = Math.Max(MaxCritDamage, MinCritDamage);
                        }
                        else if (infile.Key == "overhit_damage_percent")
                        {
                            MinOverhitDamage = Parse.PopFirstFloat(ref infile.Val);
                            MaxOverhitDamage = Parse.PopFirstFloat(ref infile.Val);
                            MaxOverhitDamage = Math.Max(MaxOverhitDamage, MinOverhitDamage);
                        }
                        else if (infile.Key == "resource_round_method")
                        {
                            if (infile.Val == "none")
                                ResourceRoundMethod = ResourceRoundMethodNone;
                            else if (infile.Val == "round")
                                ResourceRoundMethod = ResourceRoundMethodRound;
                            else if (infile.Val == "floor")
                                ResourceRoundMethod = ResourceRoundMethodFloor;
                            else if (infile.Val == "ceil")
                                ResourceRoundMethod = ResourceRoundMethodCeil;
                            else
                                infile.Error("EngineSettings: '%s' is not a valid resource rounding method.", infile.Val);
                        }
                        else if (infile.Key == "offscreen_enemy_encounters")
                        {
                            OffscreenEnemyEncounters = Parse.ToBool(infile.Val);
                        }
                        else infile.Error("EngineSettings: '%s' is not a valid key.", infile.Key);
                    }
                    infile.Close();
                }
            }

            /// <summary>
            /// C 运行时 roundf() 采用"四舍五入，五入远离零"（round half away from zero），
            /// 与 C# MathF.Round 默认的"银行家舍入"不同，因此显式指定 MidpointRounding.AwayFromZero
            /// 以保持数值行为完全一致。
            /// </summary>
            public float ResourceRound(float resourceVal)
            {
                if (ResourceRoundMethod == ResourceRoundMethodRound)
                {
                    return MathF.Round(resourceVal, MidpointRounding.AwayFromZero);
                }
                else if (ResourceRoundMethod == ResourceRoundMethodFloor)
                {
                    return MathF.Floor(resourceVal);
                }
                else if (ResourceRoundMethod == ResourceRoundMethodCeil)
                {
                    return MathF.Ceiling(resourceVal);
                }
                else
                {
                    // RESOURCE_ROUND_METHOD_NONE
                    return resourceVal;
                }
            }
        }

        public class EquipFlagsSettings
        {
            public class EquipFlag
            {
                public string Id = "";
                public string Name = "";
            }

            // 原始成员名为 "list"；重命名为 Flags 以避免与 System.Collections.Generic.List<T> 类型名冲突。
            public List<EquipFlag> Flags = new List<EquipFlag>();

            public void Load()
            {
                Flags.Clear();

                EquipFlag temp = new EquipFlag();
                EquipFlag current = temp;

                using FileParser infile = new FileParser();
                // @CLASS EngineSettings: Equip flags|Description of engine/equip_flags.txt
                if (infile.Open("engine/equip_flags.txt", FileParser.ModFile, FileParser.ErrorNormal))
                {
                    while (infile.Next())
                    {
                        if (infile.NewSection)
                        {
                            if (infile.Section == "flag")
                            {
                                temp = new EquipFlag();
                                current = temp;
                            }
                        }

                        if (infile.Section != "flag")
                            continue;

                        // if we want to replace a list item by ID, the ID needs to be parsed first
                        // but it is not essential if we're just adding to the list, so this is simply a warning
                        if (infile.Key != "id" && string.IsNullOrEmpty(current.Id))
                        {
                            infile.Error("EngineSettings: Expected 'id', but found '%s'.", infile.Key);
                        }

                        if (infile.Key == "id")
                        {
                            bool foundId = false;
                            for (int i = 0; i < Flags.Count; ++i)
                            {
                                if (Flags[i].Id == infile.Val)
                                {
                                    current = Flags[i];
                                    foundId = true;
                                }
                            }
                            if (!foundId)
                            {
                                Flags.Add(temp);
                                current = Flags[^1];
                                current.Id = infile.Val;
                            }
                        }
                        else if (infile.Key == "name")
                        {
                            current.Name = infile.Val;
                        }
                        else infile.Error("EngineSettings: '%s' is not a valid key.", infile.Key);
                    }
                    infile.Close();
                }
            }

            public int GetIndex(string flag)
            {
                for (int i = 0; i < Flags.Count; ++i)
                {
                    if (Flags[i].Id == flag)
                        return i;
                }

                // not in list, create new flag
                Utils.LogInfo("EngineSettings: Adding unknown equip flag, '%s'.", flag);

                EquipFlag ef = new EquipFlag { Id = flag };
                Flags.Add(ef);

                return Flags.Count - 1;
            }
        }

        public class PrimaryStatsSettings
        {
            public class PrimaryStat
            {
                public string Id = "";
                public string Name = "";
            }

            // 原始成员名为 "list"；重命名为 Stats 以避免与 List<T> 类型名冲突。
            public List<PrimaryStat> Stats = new List<PrimaryStat>();

            public void Load()
            {
                Stats.Clear();

                PrimaryStat temp = new PrimaryStat();
                PrimaryStat current = temp;

                using FileParser infile = new FileParser();
                // @CLASS EngineSettings: Primary Stats|Description of engine/primary_stats.txt
                if (infile.Open("engine/primary_stats.txt", FileParser.ModFile, FileParser.ErrorNormal))
                {
                    while (infile.Next())
                    {
                        if (infile.NewSection)
                        {
                            if (infile.Section == "stat")
                            {
                                temp = new PrimaryStat();
                                current = temp;
                            }
                        }

                        if (infile.Section != "stat")
                            continue;

                        if (infile.Key != "id" && string.IsNullOrEmpty(current.Id))
                        {
                            infile.Error("EngineSettings: Expected 'id', but found '%s'.", infile.Key);
                        }

                        if (infile.Key == "id")
                        {
                            bool foundId = false;
                            for (int i = 0; i < Stats.Count; ++i)
                            {
                                if (Stats[i].Id == infile.Val)
                                {
                                    current = Stats[i];
                                    foundId = true;
                                }
                            }
                            if (!foundId)
                            {
                                Stats.Add(temp);
                                current = Stats[^1];
                                current.Id = infile.Val;
                            }
                        }
                        else if (infile.Key == "name")
                        {
                            current.Name = SharedResources.Msg!.Get(infile.Val);
                        }
                        else infile.Error("EngineSettings: '%s' is not a valid key.", infile.Key);
                    }
                    infile.Close();
                }
            }

            public int GetIndexByID(string id)
            {
                for (int i = 0; i < Stats.Count; ++i)
                {
                    if (id == Stats[i].Id)
                        return i;
                }

                return Stats.Count;
            }
        }

        public class HeroClassesSettings
        {
            public class HeroClass
            {
                public string Name;
                public string Description;
                public int Currency;
                public string Equipment;
                public string Carried;
                public List<int> Primary;
                public List<PowerID> Hotkeys;
                public List<PowerID> Powers;
                public List<string> Statuses;
                public string PowerTree;
                public int DefaultPowerTab;
                public List<int> Options;
                // std::pair<unsigned, std::string> -> 具名元组 (SetId, Items)。
                public List<(uint SetId, string Items)> EquipmentSets;

                public HeroClass()
                {
                    Name = "";
                    Description = "";
                    Currency = 0;
                    Equipment = "";
                    Carried = "";
                    int primaryCount = SharedResources.Eset != null ? SharedResources.Eset.PrimaryStats.Stats.Count : 0;
                    Primary = new List<int>(new int[primaryCount]);
                    Hotkeys = new List<PowerID>(new PowerID[MenuActionBar.SlotMax]);
                    Powers = new List<PowerID>();
                    Statuses = new List<string>();
                    PowerTree = "";
                    DefaultPowerTab = -1;
                    Options = new List<int>();
                    EquipmentSets = new List<(uint SetId, string Items)>();
                }
            }

            // 原始成员名为 "list"；重命名为 Classes 以避免与 List<T> 类型名冲突。
            public List<HeroClass> Classes = new List<HeroClass>();

            public void Load()
            {
                Classes.Clear();

                HeroClass temp = new HeroClass();
                HeroClass current = temp;

                using FileParser infile = new FileParser();
                // @CLASS EngineSettings: Classes|Description of engine/classes.txt
                if (infile.Open("engine/classes.txt", FileParser.ModFile, FileParser.ErrorNormal))
                {
                    while (infile.Next())
                    {
                        if (infile.NewSection)
                        {
                            if (infile.Section == "class")
                            {
                                temp = new HeroClass();
                                current = temp;
                            }
                        }

                        if (infile.Section != "class")
                            continue;

                        // HeroClass uses 'name' as its ID
                        if (infile.Key != "name" && string.IsNullOrEmpty(current.Name))
                        {
                            infile.Error("EngineSettings: Expected 'name', but found '%s'.", infile.Key);
                        }

                        if (infile.Key == "name")
                        {
                            bool foundId = false;
                            for (int i = 0; i < Classes.Count; ++i)
                            {
                                if (Classes[i].Name == infile.Val)
                                {
                                    current = Classes[i];
                                    foundId = true;
                                }
                            }
                            if (!foundId)
                            {
                                Classes.Add(temp);
                                current = Classes[^1];
                                current.Name = infile.Val;
                            }
                        }
                        else if (infile.Key == "description")
                        {
                            current.Description = infile.Val;
                        }
                        else if (infile.Key == "currency")
                        {
                            current.Currency = Parse.ToInt(infile.Val);
                        }
                        else if (infile.Key == "equipment")
                        {
                            current.Equipment = infile.Val;
                        }
                        else if (infile.Key == "equipment_set")
                        {
                            uint setId = (uint)Parse.PopFirstInt(ref infile.Val);
                            if (setId > 0)
                            {
                                bool foundSetId = false;
                                for (int i = 0; i < current.EquipmentSets.Count; ++i)
                                {
                                    if (current.EquipmentSets[i].SetId == setId)
                                    {
                                        current.EquipmentSets[i] = (setId, infile.Val);
                                        foundSetId = true;
                                    }
                                }
                                if (!foundSetId)
                                {
                                    current.EquipmentSets.Add((setId, infile.Val));
                                }
                            }
                            else
                            {
                                infile.Error("EngineSettings: Equipment set ID must be greater than 0.");
                            }
                        }
                        else if (infile.Key == "carried")
                        {
                            current.Carried = infile.Val;
                        }
                        else if (infile.Key == "primary")
                        {
                            string primStat = Parse.PopFirstString(ref infile.Val);
                            int primStatIndex = SharedResources.Eset!.PrimaryStats.GetIndexByID(primStat);

                            if (primStatIndex != SharedResources.Eset.PrimaryStats.Stats.Count)
                            {
                                current.Primary[primStatIndex] = Parse.ToInt(infile.Val);
                            }
                            else
                            {
                                infile.Error("EngineSettings: '%s' is not a valid primary stat.", primStat);
                            }
                        }
                        else if (infile.Key == "actionbar")
                        {
                            current.Hotkeys.Clear();
                            current.Hotkeys.AddRange(new PowerID[MenuActionBar.SlotMax]);

                            for (int i = 0; i < MenuActionBar.SlotMax; i++)
                            {
                                current.Hotkeys[i] = Parse.ToPowerID(Parse.PopFirstString(ref infile.Val));
                            }
                        }
                        else if (infile.Key == "powers")
                        {
                            current.Powers.Clear();

                            string power;
                            while ((power = Parse.PopFirstString(ref infile.Val)) != "")
                            {
                                current.Powers.Add(Parse.ToPowerID(power));
                            }
                        }
                        else if (infile.Key == "campaign")
                        {
                            current.Statuses.Clear();

                            string status;
                            while ((status = Parse.PopFirstString(ref infile.Val)) != "")
                            {
                                current.Statuses.Add(status);
                            }
                        }
                        else if (infile.Key == "power_tree")
                        {
                            current.PowerTree = infile.Val;
                        }
                        else if (infile.Key == "hero_options")
                        {
                            current.Options.Clear();

                            string heroOption;
                            while ((heroOption = Parse.PopFirstString(ref infile.Val)) != "")
                            {
                                current.Options.Add(Parse.ToInt(heroOption));
                            }

                            current.Options.Sort();
                        }
                        else if (infile.Key == "default_power_tab")
                        {
                            current.DefaultPowerTab = Parse.ToInt(infile.Val);
                        }
                        else infile.Error("EngineSettings: '%s' is not a valid key.", infile.Key);
                    }
                    infile.Close();
                }

                // Make a default hero class if none were found
                if (Classes.Count == 0)
                {
                    HeroClass c = new HeroClass();
                    c.Name = "Adventurer";
                    SharedResources.Msg!.Get("Adventurer"); // this is needed for translation
                    Classes.Add(c);
                }
            }

            public HeroClass? GetByName(string name)
            {
                if (string.IsNullOrEmpty(name))
                    return null;

                for (int i = 0; i < Classes.Count; ++i)
                {
                    if (name == Classes[i].Name)
                    {
                        return Classes[i];
                    }
                }

                return null;
            }
        }

        public class DamageTypesSettings
        {
            public class DamageType
            {
                public bool IsElemental;
                public bool IsDeprecatedElement;
                public string Id = "";
                public string Name = "";
                public string NameShort = "";
                public string NameMin = "";
                public string NameMax = "";
                public string NameResist = "";
                public string Description = "";
                public string Min = "";
                public string Max = "";
                public string Resist = "";

                public DamageType()
                {
                    IsElemental = false;
                    IsDeprecatedElement = false;
                }
            }

            // 原始成员名为 "list"；重命名为 Types 以避免与 List<T> 类型名冲突。
            public List<DamageType> Types = new List<DamageType>();
            public int Count; // damage_types.size() * 3, to account for min, max, and resist

            public void Load()
            {
                Types.Clear();
                Count = 0;

                DamageType temp = new DamageType();
                DamageType current = temp;

                using FileParser infile = new FileParser();
                // @CLASS EngineSettings: Damage Types|Description of engine/damage_types.txt
                if (infile.Open("engine/damage_types.txt", FileParser.ModFile, FileParser.ErrorNormal))
                {
                    while (infile.Next())
                    {
                        if (infile.NewSection)
                        {
                            if (infile.Section == "damage_type")
                            {
                                temp = new DamageType();
                                current = temp;
                            }
                        }

                        if (infile.Section != "damage_type")
                            continue;

                        if (infile.Key != "id" && string.IsNullOrEmpty(current.Id))
                        {
                            infile.Error("EngineSettings: Expected 'id', but found '%s'.", infile.Key);
                        }

                        if (infile.Key == "id")
                        {
                            bool foundId = false;
                            for (int i = 0; i < Types.Count; ++i)
                            {
                                if (Types[i].Id == infile.Val)
                                {
                                    current = Types[i];
                                    foundId = true;
                                }
                            }
                            if (!foundId)
                            {
                                Types.Add(temp);
                                current = Types[^1];
                                current.Id = infile.Val;
                            }
                        }
                        else if (infile.Key == "name")
                        {
                            current.Name = SharedResources.Msg!.Get(infile.Val);
                        }
                        else if (infile.Key == "name_short")
                        {
                            current.NameShort = SharedResources.Msg!.Get(infile.Val);
                        }
                        else if (infile.Key == "description")
                        {
                            current.Description = SharedResources.Msg!.Get(infile.Val);
                        }
                        else if (infile.Key == "min")
                        {
                            current.Min = infile.Val;
                        }
                        else if (infile.Key == "max")
                        {
                            current.Max = infile.Val;
                        }
                        else if (infile.Key == "resist")
                        {
                            current.Resist = infile.Val;
                        }
                        else if (infile.Key == "elemental")
                        {
                            current.IsElemental = Parse.ToBool(infile.Val);
                        }
                        else infile.Error("EngineSettings: '%s' is not a valid key.", infile.Key);
                    }
                    infile.Close();
                }

                temp = new DamageType();
                current = temp;

                // For backwards-compatibility, load engine/elements.txt as damage types
                // @CLASS EngineSettings: Elements|(Deprecated in v1.14.85, use engine/damage_types.txt instead) Description of engine/elements.txt
                if (infile.Open("engine/elements.txt", FileParser.ModFile, FileParser.ErrorNone))
                {
                    Utils.LogInfo("EngineSettings: Found deprecated file engine/elements.txt. Please use engine/damage_types.txt instead!");
                    
                    while (infile.Next())
                    {
                        if (infile.NewSection)
                        {
                            if (infile.Section == "element")
                            {
                                temp = new DamageType();
                                current = temp;

                                current.IsElemental = true;
                                current.IsDeprecatedElement = true;
                            }
                        }

                        if (infile.Section != "element")
                            continue;

                        if (infile.Key != "id" && string.IsNullOrEmpty(current.Id))
                        {
                            infile.Error("EngineSettings: Expected 'id', but found '%s'.", infile.Key);
                        }

                        if (infile.Key == "id")
                        {
                            bool foundId = false;
                            for (int i = 0; i < Types.Count; ++i)
                            {
                                if (Types[i].Id == infile.Val)
                                {
                                    current = Types[i];
                                    foundId = true;
                                }
                            }
                            if (!foundId)
                            {
                                Types.Add(temp);
                                current = Types[^1];
                                current.Id = infile.Val;
                            }
                        }
                        else if (infile.Key == "name") current.Name = SharedResources.Msg!.Get(infile.Val);
                        else infile.Error("EngineSettings: '%s' is not a valid key.", infile.Key);
                    }
                    infile.Close();
                }

                // Total stat count. Each damage type has 3: min, max, resist
                Count = Types.Count * 3;

                for (int i = 0; i < Types.Count; ++i)
                {
                    // create missing IDs from the base ID if needed
                    if (string.IsNullOrEmpty(Types[i].Min))
                    {
                        Types[i].Min = "dmg_" + Types[i].Id + "_min";
                    }
                    if (string.IsNullOrEmpty(Types[i].Max))
                    {
                        Types[i].Max = "dmg_" + Types[i].Id + "_max";
                    }
                    if (string.IsNullOrEmpty(Types[i].Resist))
                    {
                        Types[i].Resist = Types[i].Id + "_resist";
                    }

                    // use the IDs if the damage type doesn't have printable names
                    if (string.IsNullOrEmpty(Types[i].Name))
                    {
                        Types[i].Name = Types[i].Id;
                    }
                    if (string.IsNullOrEmpty(Types[i].NameShort))
                    {
                        Types[i].NameShort = Types[i].Name;
                    }
                    if (string.IsNullOrEmpty(Types[i].NameMin))
                    {
                        Types[i].NameMin = SharedResources.Msg!.GetV("%s (Min.)", Types[i].Name);
                    }
                    if (string.IsNullOrEmpty(Types[i].NameMax))
                    {
                        Types[i].NameMax = SharedResources.Msg!.GetV("%s (Max.)", Types[i].Name);
                    }
                    if (string.IsNullOrEmpty(Types[i].NameResist))
                    {
                        Types[i].NameResist = SharedResources.Msg!.GetV("Resist Damage (%s)", Types[i].NameShort);
                    }
                }
            }

            public static int IndexToMin(int listIndex) => listIndex * 3;
            public static int IndexToMax(int listIndex) => (listIndex * 3) + 1;
            public static int IndexToResist(int listIndex) => (listIndex * 3) + 2;
        }

        public class DeathPenaltySettings
        {
            public bool Enabled;
            public bool Permadeath;
            public float Currency;
            public float Xp;
            public float XpCurrent;
            public bool Item;

            public void Load()
            {
                Enabled = true;
                Permadeath = false;
                Currency = 50;
                Xp = 0;
                XpCurrent = 0;
                Item = false;

                using FileParser infile = new FileParser();
                // @CLASS EngineSettings: Death penalty|Description of engine/death_penalty.txt
                if (infile.Open("engine/death_penalty.txt", FileParser.ModFile, FileParser.ErrorNormal))
                {
                    while (infile.Next())
                    {
                        if (infile.Key == "enable") Enabled = Parse.ToBool(infile.Val);
                        else if (infile.Key == "permadeath") Permadeath = Parse.ToBool(infile.Val);
                        else if (infile.Key == "currency") Currency = Parse.ToFloat(infile.Val);
                        else if (infile.Key == "xp_total") Xp = Parse.ToFloat(infile.Val);
                        else if (infile.Key == "xp_current_level") XpCurrent = Parse.ToFloat(infile.Val);
                        else if (infile.Key == "random_item") Item = Parse.ToBool(infile.Val);
                        else infile.Error("EngineSettings: '%s' is not a valid key.", infile.Key);
                    }
                    infile.Close();
                }
            }
        }

        public class TooltipsSettings
        {
            public int Offset;
            public int Width;
            public int Margin;
            public int MarginNpc;
            public int BackgroundBorder;
            public int VisibleMax;

            public void Load()
            {
                Offset = 0;
                Width = 1;
                Margin = 0;
                MarginNpc = 0;
                BackgroundBorder = 0;
                VisibleMax = 3;

                using FileParser infile = new FileParser();
                // @CLASS EngineSettings: Tooltips|Description of engine/tooltips.txt
                if (infile.Open("engine/tooltips.txt", FileParser.ModFile, FileParser.ErrorNormal))
                {
                    while (infile.Next())
                    {
                        if (infile.Key == "tooltip_offset")
                            Offset = Parse.ToInt(infile.Val);
                        else if (infile.Key == "tooltip_width")
                            Width = Parse.ToInt(infile.Val);
                        else if (infile.Key == "tooltip_margin")
                            Margin = Parse.ToInt(infile.Val);
                        else if (infile.Key == "npc_tooltip_margin")
                            MarginNpc = Parse.ToInt(infile.Val);
                        else if (infile.Key == "tooltip_background_border")
                            BackgroundBorder = Parse.ToInt(infile.Val);
                        else if (infile.Key == "tooltip_visible_max")
                        {
                            VisibleMax = Parse.ToInt(infile.Val);

                            if (VisibleMax < 1)
                            {
                                VisibleMax = 1;
                                infile.Error("EngineSettings: tooltip_visible_max must be greater than or equal to 1.");
                            }
                        }
                        else infile.Error("EngineSettings: '%s' is not a valid key.", infile.Key);
                    }
                    infile.Close();
                }
            }
        }

        public class LootSettings
        {
            public int TooltipMargin;
            public bool AutopickupCurrency;
            public float AutopickupRange;
            public string Currency = "";
            public float VendorRatioBuy;
            public float VendorRatioSell;
            public float VendorRatioSellOld;
            public string SfxLoot = "";
            public int DropMax;
            public int DropRadius;
            public float HideRadius;
            public ItemID ExtendedItemsOffset;

            public void Load()
            {
                TooltipMargin = 0;
                AutopickupCurrency = false;
                AutopickupRange = SharedResources.Eset!.Misc.InteractRange;
                Currency = "Gold";
                VendorRatioBuy = 1.0f;
                VendorRatioSell = 0.25f;
                VendorRatioSellOld = 0;
                SfxLoot = "";
                DropMax = 1;
                DropRadius = 1;
                HideRadius = 3.0f;
                ExtendedItemsOffset = 0;

                using FileParser infile = new FileParser();
                // @CLASS EngineSettings: Loot|Description of engine/loot.txt
                if (infile.Open("engine/loot.txt", FileParser.ModFile, FileParser.ErrorNormal))
                {
                    while (infile.Next())
                    {
                        if (infile.Key == "tooltip_margin")
                        {
                            TooltipMargin = Parse.ToInt(infile.Val);
                        }
                        else if (infile.Key == "autopickup_currency")
                        {
                            AutopickupCurrency = Parse.ToBool(infile.Val);
                        }
                        else if (infile.Key == "autopickup_range")
                        {
                            AutopickupRange = Parse.ToFloat(infile.Val);
                        }
                        else if (infile.Key == "currency_name")
                        {
                            Currency = SharedResources.Msg!.Get(infile.Val);
                        }
                        else if (infile.Key == "vendor_ratio_buy")
                        {
                            VendorRatioBuy = Parse.ToFloat(infile.Val);
                        }
                        else if (infile.Key == "vendor_ratio_sell")
                        {
                            VendorRatioSell = Parse.ToFloat(infile.Val);
                        }
                        else if (infile.Key == "vendor_ratio_sell_old")
                        {
                            VendorRatioSellOld = Parse.ToFloat(infile.Val);
                        }
                        else if (infile.Key == "sfx_loot")
                        {
                            SfxLoot = infile.Val;
                        }
                        else if (infile.Key == "drop_max")
                        {
                            DropMax = Math.Max(Parse.ToInt(infile.Val), 1);
                        }
                        else if (infile.Key == "drop_radius")
                        {
                            DropRadius = Math.Max(Parse.ToInt(infile.Val), 1);
                        }
                        else if (infile.Key == "hide_radius")
                        {
                            HideRadius = Parse.ToFloat(infile.Val);
                        }
                        else if (infile.Key == "vendor_ratio")
                        {
                            // (Deprecated in v1.12.85; use 'vendor_ratio_sell' instead)
                            VendorRatioSell = (float)Parse.ToInt(infile.Val) / 100.0f;
                            infile.Error("EngineSettings: vendor_ratio is deprecated. Use 'vendor_ratio_sell=%.2f' instead.", VendorRatioSell);
                        }
                        else if (infile.Key == "vendor_ratio_buyback")
                        {
                            // (Deprecated in v1.12.85; use 'vendor_ratio_sell_old' instead)
                            VendorRatioSellOld = (float)Parse.ToInt(infile.Val) / 100.0f;
                            infile.Error("EngineSettings: vendor_ratio_buyback is deprecated. Use 'vendor_ratio_sell_old=%.2f' instead.", VendorRatioSellOld);
                        }
                        else if (infile.Key == "extended_items_offset")
                        {
                            ExtendedItemsOffset = Parse.ToItemID(infile.Val);
                        }
                        else
                        {
                            infile.Error("EngineSettings: '%s' is not a valid key.", infile.Key);
                        }
                    }
                    infile.Close();
                }
            }
        }

        public class TilesetSettings
        {
            public const int TilesetIsometric = 0;
            public const int TilesetOrthogonal = 1;

            public float UnitsPerPixelX;
            public float UnitsPerPixelY;
            public ushort TileW;
            public ushort TileH;
            public ushort TileWHalf;
            public ushort TileHHalf;
            public ushort Orientation;

            public void Load()
            {
                // reset to defaults
                UnitsPerPixelX = 2;
                UnitsPerPixelY = 4;
                TileW = 64;
                TileH = 32;
                TileWHalf = (ushort)(TileW / 2);
                TileHHalf = (ushort)(TileH / 2);
                Orientation = TilesetIsometric;

                using FileParser infile = new FileParser();
                // @CLASS EngineSettings: Tileset config|Description of engine/tileset_config.txt
                if (infile.Open("engine/tileset_config.txt", FileParser.ModFile, FileParser.ErrorNormal))
                {
                    while (infile.Next())
                    {
                        if (infile.Key == "tile_size")
                        {
                            TileW = (ushort)Parse.PopFirstInt(ref infile.Val);
                            TileH = (ushort)Parse.PopFirstInt(ref infile.Val);
                            TileWHalf = (ushort)(TileW / 2);
                            TileHHalf = (ushort)(TileH / 2);
                        }
                        else if (infile.Key == "orientation")
                        {
                            if (infile.Val == "isometric")
                                Orientation = TilesetIsometric;
                            else if (infile.Val == "orthogonal")
                                Orientation = TilesetOrthogonal;
                        }
                        else
                        {
                            infile.Error("EngineSettings: '%s' is not a valid key.", infile.Key);
                        }
                    }
                    infile.Close();
                }
                else
                {
                    Utils.LogError("Unable to open engine/tileset_config.txt! Defaulting to 64x32 isometric tiles.");
                }

                // Init automatically calculated parameters
                if (Orientation == TilesetIsometric)
                {
                    if (TileW > 0 && TileH > 0)
                    {
                        UnitsPerPixelX = 2.0f / TileW;
                        UnitsPerPixelY = 2.0f / TileH;
                    }
                    else
                    {
                        Utils.LogError("EngineSettings: Tile dimensions must be greater than 0. Resetting to the default size of 64x32.");
                        TileW = 64;
                        TileH = 32;
                    }
                }
                else // TILESET_ORTHOGONAL
                {
                    if (TileW > 0 && TileH > 0)
                    {
                        UnitsPerPixelX = 1.0f / TileW;
                        UnitsPerPixelY = 1.0f / TileH;
                    }
                    else
                    {
                        Utils.LogError("EngineSettings: Tile dimensions must be greater than 0. Resetting to the default size of 64x32.");
                        TileW = 64;
                        TileH = 32;
                    }
                }
                if (UnitsPerPixelX == 0 || UnitsPerPixelY == 0)
                {
                    Utils.LogError("EngineSettings: One of UNITS_PER_PIXEL values is zero! %dx%d", (int)UnitsPerPixelX, (int)UnitsPerPixelY);
                    Utils.LogErrorDialog("EngineSettings: One of UNITS_PER_PIXEL values is zero! %dx%d", (int)UnitsPerPixelX, (int)UnitsPerPixelY);
                    SharedResources.Mods!.ResetModConfig();
                    Utils.Exit(1);
                }
            }
        }

        public class WidgetsSettings
        {
            public Color SelectionRectColor;
            public int SelectionRectCornerSize;
            public Int2 ColorblindHighlightOffset;
            public Int2 TabPadding;
            public int TabTextPadding;
            public LabelInfo SlotQuantityLabel = new LabelInfo();
            public Color SlotQuantityColor;
            public Color SlotQuantityBgColor;
            public LabelInfo SlotHotkeyLabel = new LabelInfo();
            public Color SlotHotkeyColor;
            public Color SlotHotkeyBgColor;
            public Int2 ListboxTextMargin;
            public int HorizontalListTextWidth;
            public Color ScrollbarBgColor;
            public int LogPadding;
            public string SoundActivate = "";

            public void Load()
            {
                // reset to defaults
                SelectionRectColor = new Color(255, 248, 220, 255);
                SelectionRectCornerSize = 4;
                ColorblindHighlightOffset = new Int2(2, 2);

                TabPadding = new Int2(8, 0);
                TabTextPadding = 0;

                SlotQuantityLabel = new LabelInfo();
                SlotQuantityColor = SharedResources.Font!.GetColor(FontEngine.ColorWidgetNormal);
                SlotQuantityBgColor = new Color(0, 0, 0, 0);
                SlotHotkeyLabel = new LabelInfo();
                SlotHotkeyColor = SharedResources.Font.GetColor(FontEngine.ColorWidgetNormal);
                SlotHotkeyLabel.Hidden = true;
                SlotHotkeyBgColor = new Color(0, 0, 0, 0);

                ListboxTextMargin = new Int2(8, 8);

                HorizontalListTextWidth = 150;

                ScrollbarBgColor = new Color(0, 0, 0, 64);

                LogPadding = 4;

                SoundActivate = "";

                using FileParser infile = new FileParser();
                // @CLASS EngineSettings: Widgets|Description of engine/widget_settings.txt
                if (infile.Open("engine/widget_settings.txt", FileParser.ModFile, FileParser.ErrorNone))
                {
                    while (infile.Next())
                    {
                        if (infile.Section == "misc")
                        {
                            if (infile.Key == "selection_rect_color")
                            {
                                SelectionRectColor = Parse.ToRGBA(infile.Val);
                            }
                            else if (infile.Key == "selection_rect_corner_size")
                            {
                                SelectionRectCornerSize = Parse.ToInt(infile.Val);
                            }
                            else if (infile.Key == "colorblind_highlight_offset")
                            {
                                ColorblindHighlightOffset = Parse.ToPoint(infile.Val);
                            }
                        }
                        else if (infile.Section == "tab")
                        {
                            if (infile.Key == "padding")
                            {
                                TabPadding = Parse.ToPoint(infile.Val);
                            }
                            else if (infile.Key == "text_padding")
                            {
                                TabTextPadding = Parse.ToInt(infile.Val);
                            }
                        }
                        else if (infile.Section == "slot")
                        {
                            if (infile.Key == "quantity_label")
                            {
                                SlotQuantityLabel = Parse.PopLabelInfo(infile.Val);
                            }
                            else if (infile.Key == "quantity_color")
                            {
                                SlotQuantityColor = Parse.ToRGB(infile.Val);
                            }
                            else if (infile.Key == "quantity_bg_color")
                            {
                                SlotQuantityBgColor = Parse.ToRGBA(infile.Val);
                            }
                            else if (infile.Key == "hotkey_label")
                            {
                                SlotHotkeyLabel = Parse.PopLabelInfo(infile.Val);
                            }
                            else if (infile.Key == "hotkey_color")
                            {
                                SlotHotkeyColor = Parse.ToRGB(infile.Val);
                            }
                            else if (infile.Key == "hotkey_bg_color")
                            {
                                SlotHotkeyBgColor = Parse.ToRGBA(infile.Val);
                            }
                        }
                        else if (infile.Section == "listbox")
                        {
                            if (infile.Key == "text_margin")
                            {
                                ListboxTextMargin = Parse.ToPoint(infile.Val);
                            }
                        }
                        else if (infile.Section == "horizontal_list")
                        {
                            if (infile.Key == "text_width")
                            {
                                HorizontalListTextWidth = Parse.ToInt(infile.Val);
                            }
                        }
                        else if (infile.Section == "scrollbar")
                        {
                            if (infile.Key == "bg_color")
                            {
                                ScrollbarBgColor = Parse.ToRGBA(infile.Val);
                            }
                        }
                        else if (infile.Section == "log")
                        {
                            if (infile.Key == "padding")
                            {
                                LogPadding = Parse.ToInt(infile.Val);
                            }
                        }
                        else if (infile.Section == "sound")
                        {
                            SoundActivate = infile.Val;
                        }
                    }
                    // 注意：原始 C++ 代码在此分支中没有调用 infile.close()（与本文件其他 Load()
                    // 方法不同），依赖 FileParser 析构函数完成清理。C# 版本同样不显式调用 Close()，
                    // 依赖 using 语句在方法结束时完成清理，逐行保留这一处的不一致风格。
                }
            }
        }

        public class XpTableSettings
        {
            private List<ulong> _xpTable = new List<ulong>();

            public void Load()
            {
                _xpTable.Clear();

                using FileParser infile = new FileParser();
                // @CLASS EngineSettings: XP table|Description of engine/xp_table.txt
                if (infile.Open("engine/xp_table.txt", FileParser.ModFile, FileParser.ErrorNormal))
                {
                    while (infile.Next())
                    {
                        if (infile.Key == "level")
                        {
                            uint lvlId = (uint)Parse.PopFirstInt(ref infile.Val);
                            ulong lvlXp = Parse.ToUnsignedLong(Parse.PopFirstString(ref infile.Val));

                            if (lvlId > (uint)_xpTable.Count)
                            {
                                while ((uint)_xpTable.Count < lvlId)
                                    _xpTable.Add(0);
                            }

                            _xpTable[(int)lvlId - 1] = lvlXp;

                            // validate that XP is greater than previous levels
                            // corrections are done after then entire table is loaded
                            for (int i = 0; i < (int)lvlId - 1; ++i)
                            {
                                if (_xpTable[(int)lvlId - 1] <= _xpTable[i])
                                {
                                    infile.Error("EngineSettings: XP for level %u is less than previous levels.", lvlId);
                                    break;
                                }
                            }
                        }
                    }
                    infile.Close();
                }

                // set invalid XP thresolds to valid values
                for (int i = 1; i < _xpTable.Count; ++i)
                {
                    if (_xpTable[i] <= _xpTable[i - 1])
                    {
                        _xpTable[i] = _xpTable[i - 1] + 1;
                        Utils.LogInfo("EngineSettings: Setting XP for level %u to %lu.", i + 1, _xpTable[i]);
                    }
                }

                if (_xpTable.Count == 0)
                {
                    Utils.LogError("EngineSettings: No XP table defined.");
                    _xpTable.Add(0);
                }
            }

            public ulong GetLevelXP(int level)
            {
                if (level <= 1 || _xpTable.Count == 0)
                    return 0;
                else if (level > _xpTable.Count)
                    return _xpTable[^1];
                else
                    return _xpTable[level - 1];
            }

            public int GetMaxLevel()
            {
                return _xpTable.Count;
            }

            public int GetLevelFromXP(ulong levelXp)
            {
                int level = 0;

                for (int i = 0; i < _xpTable.Count; ++i)
                {
                    if (levelXp >= _xpTable[i])
                        level = i + 1;
                }

                return level;
            }
        }

        public class NumberFormatSettings
        {
            public int PlayerStatbar;
            public int EnemyStatbar;
            public int CombatText;
            public int CharacterMenu;
            public int ItemTooltips;
            public int PowerTooltips;
            public int Durations;
            public int DeathPenalty;

            public void Load()
            {
                PlayerStatbar = 0;
                EnemyStatbar = 0;
                CombatText = 0;
                CharacterMenu = 2;
                ItemTooltips = 2;
                PowerTooltips = 2;
                Durations = 1;
                DeathPenalty = 2;

                using FileParser infile = new FileParser();
                // @CLASS EngineSettings: Number Format|Description of engine/number_format.txt
                if (infile.Open("engine/number_format.txt", FileParser.ModFile, FileParser.ErrorNone))
                {
                    while (infile.Next())
                    {
                        if (infile.Key == "player_statbar")
                            PlayerStatbar = Math.Max(0, Parse.ToInt(infile.Val));
                        else if (infile.Key == "enemy_statbar")
                            EnemyStatbar = Math.Max(0, Parse.ToInt(infile.Val));
                        else if (infile.Key == "combat_text")
                            CombatText = Math.Max(0, Parse.ToInt(infile.Val));
                        else if (infile.Key == "character_menu")
                            CharacterMenu = Math.Max(0, Parse.ToInt(infile.Val));
                        else if (infile.Key == "item_tooltips")
                            ItemTooltips = Math.Max(0, Parse.ToInt(infile.Val));
                        else if (infile.Key == "power_tooltips")
                            PowerTooltips = Math.Max(0, Parse.ToInt(infile.Val));
                        else if (infile.Key == "durations")
                            Durations = Math.Max(0, Parse.ToInt(infile.Val));
                        else if (infile.Key == "death_penalty")
                            DeathPenalty = Math.Max(0, Parse.ToInt(infile.Val));
                        else
                            infile.Error("EngineSettings: '%s' is not a valid key.", infile.Key);
                    }
                    infile.Close();
                }
            }
        }

        public class ResourceStatsSettings
        {
            // statblock & effect
            public const int StatBase = 0;
            public const int StatRegen = 1;
            public const int StatSteal = 2;
            public const int StatResistSteal = 3;

            // effect only
            public const int StatHeal = 4;
            public const int StatHealPercent = 5;

            public const int StatEffectCount = 6;

            public const int StatCount = StatHeal;
            public const int EffectCount = StatEffectCount - StatCount;

            public class ResourceStat
            {
                public List<string> Ids;
                public List<string> Text;
                public List<string> TextDesc;

                public string MenuFilename = "";

                public string TextCombatHeal = "";
                public string TextLogRestore = "";
                public string TextLogLow = "";
                public string TextTooltipHeal = "";
                public string TextTooltipCost = "";

                public ResourceStat()
                {
                    Ids = new List<string>();
                    for (int i = 0; i < StatEffectCount; i++) Ids.Add("");
                    Text = new List<string>();
                    for (int i = 0; i < StatCount; i++) Text.Add("");
                    TextDesc = new List<string>();
                    for (int i = 0; i < StatCount; i++) TextDesc.Add("");
                }
            }

            // 原始成员名为 "list"；重命名为 Stats 以避免与 List<T> 类型名冲突。
            public List<ResourceStat> Stats = new List<ResourceStat>();

            // 原始成员 stat_count/effect_count/stat_effect_count 与上面的类级常量 StatCount/
            // EffectCount/StatEffectCount 同名（仅大小写不同），在 C++ 中因为一个是
            // static const 一个是实例成员而不冲突；C# 帕斯卡命名后会直接冲突，因此实例成员
            // 追加 Value 后缀加以区分。
            public int StatCountValue;
            public int EffectCountValue;
            public int StatEffectCountValue;

            public void Load()
            {
                Stats.Clear();
                StatCountValue = 0;
                EffectCountValue = 0;
                StatEffectCountValue = 0;

                ResourceStat temp = new ResourceStat();
                ResourceStat current = temp;

                using FileParser infile = new FileParser();
                // @CLASS EngineSettings: Resource Stats|Description of engine/resource_stats.txt
                if (infile.Open("engine/resource_stats.txt", FileParser.ModFile, FileParser.ErrorNormal))
                {
                    while (infile.Next())
                    {
                        if (infile.NewSection)
                        {
                            if (infile.Section == "resource_stat")
                            {
                                temp = new ResourceStat();
                                current = temp;
                            }
                        }

                        if (infile.Section != "resource_stat")
                            continue;

                        // ResourceStat uses stat_base as its primary ID
                        if (infile.Key != "stat_base" && string.IsNullOrEmpty(current.Ids[StatBase]))
                        {
                            infile.Error("EngineSettings: Expected 'id', but found '%s'.", infile.Key);
                        }

                        // TODO check for conflicts with built-in Stats and already parsed ResourceStats
                        if (infile.Key == "stat_base")
                        {
                            bool foundId = false;
                            for (int i = 0; i < Stats.Count; ++i)
                            {
                                if (Stats[i].Ids[StatBase] == infile.Val)
                                {
                                    current = Stats[i];
                                    foundId = true;
                                }
                            }
                            if (!foundId)
                            {
                                Stats.Add(temp);
                                current = Stats[^1];
                                current.Ids[StatBase] = infile.Val;
                            }
                        }
                        else if (infile.Key == "stat_regen") current.Ids[StatRegen] = infile.Val;
                        else if (infile.Key == "stat_steal") current.Ids[StatSteal] = infile.Val;
                        else if (infile.Key == "stat_resist_steal") current.Ids[StatResistSteal] = infile.Val;
                        else if (infile.Key == "stat_heal") current.Ids[StatHeal] = infile.Val;
                        else if (infile.Key == "stat_heal_percent") current.Ids[StatHealPercent] = infile.Val;

                        else if (infile.Key == "menu_filename") current.MenuFilename = infile.Val;

                        else if (infile.Key == "text_base") current.Text[StatBase] = SharedResources.Msg!.Get(infile.Val);
                        else if (infile.Key == "text_base_desc") current.TextDesc[StatBase] = SharedResources.Msg!.Get(infile.Val);

                        else if (infile.Key == "text_regen") current.Text[StatRegen] = SharedResources.Msg!.Get(infile.Val);
                        else if (infile.Key == "text_regen_desc") current.TextDesc[StatRegen] = SharedResources.Msg!.Get(infile.Val);

                        else if (infile.Key == "text_steal") current.Text[StatSteal] = SharedResources.Msg!.Get(infile.Val);
                        else if (infile.Key == "text_steal_desc") current.TextDesc[StatSteal] = SharedResources.Msg!.Get(infile.Val);

                        else if (infile.Key == "text_resist_steal") current.Text[StatResistSteal] = SharedResources.Msg!.Get(infile.Val);
                        else if (infile.Key == "text_resist_steal_desc") current.TextDesc[StatResistSteal] = SharedResources.Msg!.Get(infile.Val);

                        else if (infile.Key == "text_combat_heal") current.TextCombatHeal = SharedResources.Msg!.Get(infile.Val);
                        else if (infile.Key == "text_log_restore") current.TextLogRestore = SharedResources.Msg!.Get(infile.Val);
                        else if (infile.Key == "text_log_low") current.TextLogLow = SharedResources.Msg!.Get(infile.Val);
                        else if (infile.Key == "text_tooltip_heal") current.TextTooltipHeal = SharedResources.Msg!.Get(infile.Val);
                        else if (infile.Key == "text_tooltip_cost") current.TextTooltipCost = SharedResources.Msg!.Get(infile.Val);

                        else infile.Error("EngineSettings: '%s' is not a valid key.", infile.Key);
                    }
                    infile.Close();
                }

                StatCountValue = Stats.Count * StatCount; // base, regen, steal, resist_steal
                EffectCountValue = Stats.Count * EffectCount; // heal, heal_percent
                StatEffectCountValue = StatCountValue + EffectCountValue;
            }
        }
    }
}
