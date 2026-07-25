// <自动生成> 对应 C++ 源文件：StatBlock.h + StatBlock.cpp
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// StatBlock - 实体属性数据类，对应 C++ <c>class StatBlock</c>。
    /// 管理 HP/MP/属性/效果/技能等所有数值。
    /// 用于 Avatar、Entity、敌人和 NPC。
    /// </summary>
    public class StatBlock : IDisposable
    {
        public const int AiPowerMelee = 0;
        public const int AiPowerRanged = 1;
        public const int AiPowerBeacon = 2;
        public const int AiPowerHit = 3;
        public const int AiPowerDeath = 4;
        public const int AiPowerHalfDead = 5;
        public const int AiPowerJoinCombat = 6;
        public const int AiPowerDebuff = 7;
        public const int AiPowerPassivePost = 8;

        public const int EntityStance = 0;
        public const int EntityMove = 1;
        public const int EntityPower = 2;
        public const int EntitySpawn = 3;
        public const int EntityBlock = 4;
        public const int EntityHit = 5;
        public const int EntityDead = 6;
        public const int EntityCritdead = 7;

        public const int CombatDefault = 0;
        public const int CombatAggressive = 1;
        public const int CombatPassive = 2;

        public class AIPower
        {
            public int Type;
            public PowerID Id;
            public float Chance;
            public Timer Cooldown;

            public AIPower()
            {
                Type = AiPowerMelee;
                Id = 0;
                Chance = 0;
                Cooldown = new Timer();
            }
        }

        public const bool CanUsePassive = true;
        public const bool TakeDmgCrit = true;

        private static readonly float Sqrt2 = (float)(1.0 / Math.Sqrt(2.0));

        public static readonly float[] DirectionDeltaX = { -1, -1, -1, 0, 1, 1, 1, 0 };
        public static readonly float[] DirectionDeltaY = { 1, 0, -1, -1, -1, 0, 1, 1 };
        public static readonly float[] SpeedMultiplier = { Sqrt2, 1.0f, Sqrt2, 1.0f, Sqrt2, 1.0f, Sqrt2, 1.0f };

        private bool _statsLoaded;

        public bool Alive;
        public bool Corpse;
        public bool CorpseHasCollision;
        public bool CorpseHasTimeout;
        public bool CorpseRenderBelow;
        public Timer CorpseTimer;
        public bool Hero;
        public bool HeroAlly;
        public bool EnemyAlly;
        public bool Npc;
        public bool Humanoid;
        public bool Lifeform;
        public bool Permadeath;
        public bool Transformed;
        public bool RefreshStats;
        public bool Converted;
        public bool Summoned;
        public PowerID SummonedPowerIndex;
        public bool Encountered;
        public StatBlock? TargetCorpse;
        public StatBlock? TargetNearest;
        public StatBlock? TargetNearestCorpse;
        public float TargetNearestDist;
        public float TargetNearestCorpseDist;
        public PowerID BlockPower;

        public int MovementType;
        public bool Facing;

        public List<string> Categories = new List<string>();

        public string Name = "";

        public int Level;
        public ulong Xp;
        public XPScalingTableID XpScalingTable;
        public bool LevelUp;
        public bool CheckTitle;
        public int StatPointsPerLevel;
        public int PowerPointsPerLevel;

        public List<int> Primary = new List<int>();
        public List<int> PrimaryStarting = new List<int>();

        public List<float> Starting = new List<float>();
        public List<float> Base = new List<float>();
        public List<float> Current = new List<float>();
        public List<float> PerLevel = new List<float>();
        public List<List<float>> PerPrimary = new List<List<float>>();

        public List<int> PrimaryAdditional = new List<int>();

        public string CharacterClass = "";
        public string CharacterSubclass = "";

        public float Hp;
        public float Mp;

        public List<float> ResourceStats = new List<float>();

        public float SpeedDefault;

        public List<FMinMax> ItemBaseDmg = new List<FMinMax>();
        public FMinMax ItemBaseAbs = new FMinMax();

        public float Speed;
        public float ChargeSpeed;

        public HashSet<int> EquipFlags = new HashSet<int>();

        public int TransformDuration;
        public int TransformDurationTotal;
        public bool ManualUntransform;
        public bool TransformWithEquipment;
        public bool UntransformOnHit;
        public EffectManager Effects = new EffectManager();
        public bool Blocking;

        public Vector2 Pos;
        public Vector2 KnockbackSpeed;
        public Vector2 KnockbackSrcpos;
        public Vector2 KnockbackDestpos;
        public byte Direction;

        public Timer CooldownHit;
        public bool CooldownHitEnabled;

        public int CurState;
        public Timer StateTimer;
        public bool HoldState;
        public bool PreventInterrupt;

        public Queue<Vector2> Waypoints = new Queue<Vector2>();
        public Timer WaypointTimer;

        public bool Wander;
        public Rectangle WanderArea;

        public float ChancePursue;
        public float ChanceFlee;

        public List<PowerID> PowersList = new List<PowerID>();
        public List<PowerID> PowersListItems = new List<PowerID>();
        public List<PowerID> PowersPassive = new List<PowerID>();
        public List<AIPower> PowersAi = new List<AIPower>();

        public float MeleeRange;
        public float ThreatRange;
        public float ThreatRangeFar;
        public float FleeRange;
        public int CombatStyle;
        public float HeroStealth;
        public int TurnDelay;
        public bool InCombat;
        public bool JoinCombat;
        public Timer Cooldown;
        public AIPower? ActivatedPower;
        public bool HalfDeadPower;
        public bool SuppressHp;
        public Timer FleeTimer;
        public Timer FleeCooldownTimer;
        public bool PerfectAccuracy;
        public Timer CooldownLos;
        public float RestingHpRegenSeconds;

        public List<EventComponent> LootTable = new List<EventComponent>();
        public Int2 LootCount;

        public bool Teleportation;
        public Vector2 TeleportDestination;

        public int Currency;

        public bool DeathPenalty;

        public StatusID DefeatStatus;
        public StatusID ConvertStatus;
        public StatusID QuestLootRequiresStatus;
        public StatusID QuestLootRequiresNotStatus;
        public ItemID QuestLootId;
        public ItemID FirstDefeatLoot;

        public string GfxBase = "";
        public string GfxHead = "";
        public string GfxPortrait = "";
        public string TransformType = "";

        public string GfxBaseOriginal = "";
        public string GfxHeadOriginal = "";

        public string Animations = "";

        public List<(string AnimName, List<string> Filenames)> SfxAttack = new List<(string, List<string>)>();
        public string SfxStep = "";
        public List<string> SfxHit = new List<string>();
        public List<string> SfxDie = new List<string>();
        public List<string> SfxCritdie = new List<string>();
        public List<string> SfxBlock = new List<string>();
        public string SfxLevelup = "";
        public string SfxLowhp = "";
        public bool SfxLowhpLoop;

        public int MaxSpendableStatPoints;
        public int MaxPointsPerStat;

        public float PrevMaxhp;
        public float PrevMaxmp;
        public float PrevHp;
        public float PrevMp;

        public List<float> PrevMaxResourceStats = new List<float>();
        public List<float> PrevResourceStats = new List<float>();

        public List<StatBlock> Summons = new List<StatBlock>();
        public StatBlock? Summoner;
        public Queue<PowerID> PartyBuffs = new Queue<PowerID>();

        public List<PowerID> PowerFilter = new List<PowerID>();

        public List<EventComponent> InvincibleRequirements = new List<EventComponent>();

        public bool AbortNpcInteract;

        public List<string> LayerReferenceOrder = new List<string>();
        public List<List<uint>> LayerDef = new List<List<uint>>();

        public Dictionary<string, string> AnimationSlots = new Dictionary<string, string>();

        public bool CritdieEnabled;

        public AIPower? AiDebuffPower;
        public AIPower? AiHitPower;

        public static int GetFullStatCount()
        {
            return Stats.Count + SharedResources.Eset!.DamageTypes.Count + SharedResources.Eset.ResourceStats.StatCountValue;
        }

        public StatBlock()
        {
            _statsLoaded = false;
            Alive = true;
            Corpse = false;
            CorpseHasCollision = false;
            CorpseHasTimeout = true;
            CorpseRenderBelow = true;
            CorpseTimer = new Timer();
            Hero = false;
            HeroAlly = false;
            EnemyAlly = false;
            Npc = false;
            Humanoid = false;
            Lifeform = true;
            Permadeath = false;
            Transformed = false;
            RefreshStats = false;
            Converted = false;
            Summoned = false;
            SummonedPowerIndex = 0;
            Encountered = SharedResources.Eset!.Combat.OffscreenEnemyEncounters;
            TargetCorpse = null;
            TargetNearest = null;
            TargetNearestCorpse = null;
            TargetNearestDist = 0;
            TargetNearestCorpseDist = 0;
            BlockPower = 0;
            MovementType = MapCollision.MoveNormal;
            Facing = true;
            Name = "";
            Level = 0;
            Xp = 0;
            XpScalingTable = 0;
            LevelUp = false;
            CheckTitle = false;
            StatPointsPerLevel = 1;
            PowerPointsPerLevel = 1;

            int fullStatCount = GetFullStatCount();
            Starting = new List<float>(new float[fullStatCount]);
            Base = new List<float>(new float[fullStatCount]);
            Current = new List<float>(new float[fullStatCount]);
            PerLevel = new List<float>(new float[fullStatCount]);

            CharacterClass = "";
            CharacterSubclass = "";
            Hp = 0;
            Mp = 0;
            SpeedDefault = 0.1f;

            ItemBaseDmg = new List<FMinMax>();
            for (int i = 0; i < SharedResources.Eset.DamageTypes.Types.Count; ++i)
                ItemBaseDmg.Add(new FMinMax());
            ItemBaseAbs = new FMinMax();

            Speed = 0.1f;
            ChargeSpeed = 0.0f;
            TransformDuration = 0;
            TransformDurationTotal = 0;
            ManualUntransform = false;
            TransformWithEquipment = false;
            UntransformOnHit = false;
            Effects = new EffectManager();
            Blocking = false;
            Pos = default;
            KnockbackSpeed = default;
            KnockbackSrcpos = default;
            KnockbackDestpos = default;
            Direction = 0;
            CooldownHit = new Timer();
            CooldownHitEnabled = false;
            CurState = EntityStance;
            StateTimer = new Timer();
            HoldState = false;
            PreventInterrupt = false;
            Waypoints = new Queue<Vector2>();
            WaypointTimer = new Timer(SharedResources.Settings!.MaxFramesPerSec);
            Wander = false;
            WanderArea = default;
            ChancePursue = 0;
            ChanceFlee = 0;
            PowersList = new List<PowerID>();
            PowersListItems = new List<PowerID>();
            PowersPassive = new List<PowerID>();
            PowersAi = new List<AIPower>();
            MeleeRange = 1.0f;
            ThreatRange = 0;
            ThreatRangeFar = 0;
            FleeRange = 0;
            CombatStyle = CombatDefault;
            HeroStealth = 0;
            TurnDelay = 0;
            InCombat = false;
            JoinCombat = false;
            Cooldown = new Timer();
            ActivatedPower = null;
            HalfDeadPower = false;
            SuppressHp = false;
            FleeTimer = new Timer(SharedResources.Settings.MaxFramesPerSec);
            FleeCooldownTimer = new Timer(SharedResources.Settings.MaxFramesPerSec);
            PerfectAccuracy = false;
            CooldownLos = new Timer();
            RestingHpRegenSeconds = 5.0f;
            Teleportation = false;
            TeleportDestination = default;
            Currency = 0;
            DeathPenalty = false;
            DefeatStatus = 0;
            ConvertStatus = 0;
            QuestLootRequiresStatus = 0;
            QuestLootRequiresNotStatus = 0;
            QuestLootId = 0;
            FirstDefeatLoot = 0;
            GfxBase = "male";
            GfxHead = "head_short";
            GfxPortrait = "";
            TransformType = "";
            GfxBaseOriginal = "";
            GfxHeadOriginal = "";
            Animations = "";
            SfxAttack = new List<(string, List<string>)>();
            SfxStep = "";
            SfxHit = new List<string>();
            SfxDie = new List<string>();
            SfxCritdie = new List<string>();
            SfxBlock = new List<string>();
            SfxLevelup = "";
            SfxLowhp = "";
            SfxLowhpLoop = false;
            MaxSpendableStatPoints = 0;
            MaxPointsPerStat = 0;
            PrevMaxhp = 0;
            PrevMaxmp = 0;
            PrevHp = 0;
            PrevMp = 0;
            Summons = new List<StatBlock>();
            Summoner = null;
            AbortNpcInteract = false;
            LayerReferenceOrder = new List<string>();
            LayerDef = new List<List<uint>>();
            for (int i = 0; i < 8; ++i)
                LayerDef.Add(new List<uint>());
            AnimationSlots = new Dictionary<string, string>();
            CritdieEnabled = false;
            AiDebuffPower = null;
            AiHitPower = null;

            Primary = new List<int>(new int[SharedResources.Eset.PrimaryStats.Stats.Count]);
            PrimaryStarting = new List<int>(new int[SharedResources.Eset.PrimaryStats.Stats.Count]);
            PrimaryAdditional = new List<int>(new int[SharedResources.Eset.PrimaryStats.Stats.Count]);
            PerPrimary = new List<List<float>>();
            for (int i = 0; i < SharedResources.Eset.PrimaryStats.Stats.Count; ++i)
            {
                PerPrimary.Add(new List<float>(new float[GetFullStatCount()]));
            }

            Cooldown.Reset(Timer.End);

            ResourceStats = new List<float>(new float[SharedResources.Eset.ResourceStats.Stats.Count]);
            PrevMaxResourceStats = new List<float>(new float[SharedResources.Eset.ResourceStats.Stats.Count]);
            PrevResourceStats = new List<float>(new float[SharedResources.Eset.ResourceStats.Stats.Count]);

            Starting[Stats.HpMax] = 1;
        }

        /// <summary>
        /// 对应 C++ 已全局导入，此处省略。
        /// Entity ?? <c>new StatBlock(e.Stats)</c>  output/Entity.cs???
        /// </summary>
        public StatBlock(StatBlock other)
        {
            _statsLoaded = other._statsLoaded;
            Alive = other.Alive;
            Corpse = other.Corpse;
            CorpseHasCollision = other.CorpseHasCollision;
            CorpseHasTimeout = other.CorpseHasTimeout;
            CorpseRenderBelow = other.CorpseRenderBelow;
            CorpseTimer = new Timer();
            CorpseTimer.Duration = other.CorpseTimer.Duration;
            CorpseTimer.Current = other.CorpseTimer.Current;
            Hero = other.Hero;
            HeroAlly = other.HeroAlly;
            EnemyAlly = other.EnemyAlly;
            Npc = other.Npc;
            Humanoid = other.Humanoid;
            Lifeform = other.Lifeform;
            Permadeath = other.Permadeath;
            Transformed = other.Transformed;
            RefreshStats = other.RefreshStats;
            Converted = other.Converted;
            Summoned = other.Summoned;
            SummonedPowerIndex = other.SummonedPowerIndex;
            Encountered = other.Encountered;
            TargetCorpse = other.TargetCorpse;
            TargetNearest = other.TargetNearest;
            TargetNearestCorpse = other.TargetNearestCorpse;
            TargetNearestDist = other.TargetNearestDist;
            TargetNearestCorpseDist = other.TargetNearestCorpseDist;
            BlockPower = other.BlockPower;
            MovementType = other.MovementType;
            Facing = other.Facing;
            Categories = new List<string>(other.Categories);
            Name = other.Name;
            Level = other.Level;
            Xp = other.Xp;
            XpScalingTable = other.XpScalingTable;
            LevelUp = other.LevelUp;
            CheckTitle = other.CheckTitle;
            StatPointsPerLevel = other.StatPointsPerLevel;
            PowerPointsPerLevel = other.PowerPointsPerLevel;
            Primary = new List<int>(other.Primary);
            PrimaryStarting = new List<int>(other.PrimaryStarting);
            Starting = new List<float>(other.Starting);
            Base = new List<float>(other.Base);
            Current = new List<float>(other.Current);
            PerLevel = new List<float>(other.PerLevel);
            PerPrimary = new List<List<float>>();
            for (int i = 0; i < other.PerPrimary.Count; ++i)
                PerPrimary.Add(new List<float>(other.PerPrimary[i]));
            PrimaryAdditional = new List<int>(other.PrimaryAdditional);
            CharacterClass = other.CharacterClass;
            CharacterSubclass = other.CharacterSubclass;
            Hp = other.Hp;
            Mp = other.Mp;
            ResourceStats = new List<float>(other.ResourceStats);
            SpeedDefault = other.SpeedDefault;
            ItemBaseDmg = new List<FMinMax>();
            for (int i = 0; i < other.ItemBaseDmg.Count; ++i)
            {
                ItemBaseDmg.Add(new FMinMax { Min = other.ItemBaseDmg[i].Min, Max = other.ItemBaseDmg[i].Max });
            }
            ItemBaseAbs = new FMinMax { Min = other.ItemBaseAbs.Min, Max = other.ItemBaseAbs.Max };
            Speed = other.Speed;
            ChargeSpeed = other.ChargeSpeed;
            EquipFlags = new HashSet<int>(other.EquipFlags);
            TransformDuration = other.TransformDuration;
            TransformDurationTotal = other.TransformDurationTotal;
            ManualUntransform = other.ManualUntransform;
            TransformWithEquipment = other.TransformWithEquipment;
            UntransformOnHit = other.UntransformOnHit;
            Effects = new EffectManager();
            Effects.Damage = other.Effects.Damage;
            Effects.DamagePercent = other.Effects.DamagePercent;
            Effects.Hpot = other.Effects.Hpot;
            Effects.HpotPercent = other.Effects.HpotPercent;
            Effects.Mpot = other.Effects.Mpot;
            Effects.MpotPercent = other.Effects.MpotPercent;
            Effects.ResourceOt = new List<float>(other.Effects.ResourceOt);
            Effects.ResourceOtPercent = new List<float>(other.Effects.ResourceOtPercent);
            Effects.Speed = other.Effects.Speed;
            Effects.Stun = other.Effects.Stun;
            Effects.Revive = other.Effects.Revive;
            Effects.Convert = other.Effects.Convert;
            Effects.DeathSentence = other.Effects.DeathSentence;
            Effects.Fear = other.Effects.Fear;
            Effects.KnockbackSpeed = other.Effects.KnockbackSpeed;
            Effects.Bonus = new List<float>(other.Effects.Bonus);
            Effects.BonusMultiplier = new List<float>(other.Effects.BonusMultiplier);
            Effects.BonusPrimary = new List<int>(other.Effects.BonusPrimary);
            Effects.TriggeredOthers = other.Effects.TriggeredOthers;
            Effects.TriggeredBlock = other.Effects.TriggeredBlock;
            Effects.TriggeredHit = other.Effects.TriggeredHit;
            Effects.TriggeredHalfdeath = other.Effects.TriggeredHalfdeath;
            Effects.TriggeredJoincombat = other.Effects.TriggeredJoincombat;
            Effects.TriggeredDeath = other.Effects.TriggeredDeath;
            Effects.TriggeredActivePower = other.Effects.TriggeredActivePower;
            Effects.RefreshStats = other.Effects.RefreshStats;
            for (int i = 0; i < other.Effects.EffectList.Count; ++i)
                Effects.EffectList.Add(new Effect(other.Effects.EffectList[i]));
            Blocking = other.Blocking;
            Pos = other.Pos;
            KnockbackSpeed = other.KnockbackSpeed;
            KnockbackSrcpos = other.KnockbackSrcpos;
            KnockbackDestpos = other.KnockbackDestpos;
            Direction = other.Direction;
            CooldownHit = new Timer();
            CooldownHit.Duration = other.CooldownHit.Duration;
            CooldownHit.Current = other.CooldownHit.Current;
            CooldownHitEnabled = other.CooldownHitEnabled;
            CurState = other.CurState;
            StateTimer = new Timer();
            StateTimer.Duration = other.StateTimer.Duration;
            StateTimer.Current = other.StateTimer.Current;
            HoldState = other.HoldState;
            PreventInterrupt = other.PreventInterrupt;
            Waypoints = new Queue<Vector2>(other.Waypoints);
            WaypointTimer = new Timer();
            WaypointTimer.Duration = other.WaypointTimer.Duration;
            WaypointTimer.Current = other.WaypointTimer.Current;
            Wander = other.Wander;
            WanderArea = other.WanderArea;
            ChancePursue = other.ChancePursue;
            ChanceFlee = other.ChanceFlee;
            PowersList = new List<PowerID>(other.PowersList);
            PowersListItems = new List<PowerID>(other.PowersListItems);
            PowersPassive = new List<PowerID>(other.PowersPassive);
            PowersAi = new List<AIPower>();
            for (int i = 0; i < other.PowersAi.Count; ++i)
            {
                AIPower p = new AIPower();
                p.Type = other.PowersAi[i].Type;
                p.Id = other.PowersAi[i].Id;
                p.Chance = other.PowersAi[i].Chance;
                p.Cooldown = new Timer();
                p.Cooldown.Duration = other.PowersAi[i].Cooldown.Duration;
                p.Cooldown.Current = other.PowersAi[i].Cooldown.Current;
                PowersAi.Add(p);
            }
            MeleeRange = other.MeleeRange;
            ThreatRange = other.ThreatRange;
            ThreatRangeFar = other.ThreatRangeFar;
            FleeRange = other.FleeRange;
            CombatStyle = other.CombatStyle;
            HeroStealth = other.HeroStealth;
            TurnDelay = other.TurnDelay;
            InCombat = other.InCombat;
            JoinCombat = other.JoinCombat;
            Cooldown = new Timer();
            Cooldown.Duration = other.Cooldown.Duration;
            Cooldown.Current = other.Cooldown.Current;
            ActivatedPower = other.ActivatedPower;
            HalfDeadPower = other.HalfDeadPower;
            SuppressHp = other.SuppressHp;
            FleeTimer = new Timer();
            FleeTimer.Duration = other.FleeTimer.Duration;
            FleeTimer.Current = other.FleeTimer.Current;
            FleeCooldownTimer = new Timer();
            FleeCooldownTimer.Duration = other.FleeCooldownTimer.Duration;
            FleeCooldownTimer.Current = other.FleeCooldownTimer.Current;
            PerfectAccuracy = other.PerfectAccuracy;
            CooldownLos = new Timer();
            CooldownLos.Duration = other.CooldownLos.Duration;
            CooldownLos.Current = other.CooldownLos.Current;
            RestingHpRegenSeconds = other.RestingHpRegenSeconds;
            LootTable = new List<EventComponent>();
            for (int i = 0; i < other.LootTable.Count; ++i)
                LootTable.Add(other.LootTable[i]);
            LootCount = other.LootCount;
            Teleportation = other.Teleportation;
            TeleportDestination = other.TeleportDestination;
            Currency = other.Currency;
            DeathPenalty = other.DeathPenalty;
            DefeatStatus = other.DefeatStatus;
            ConvertStatus = other.ConvertStatus;
            QuestLootRequiresStatus = other.QuestLootRequiresStatus;
            QuestLootRequiresNotStatus = other.QuestLootRequiresNotStatus;
            QuestLootId = other.QuestLootId;
            FirstDefeatLoot = other.FirstDefeatLoot;
            GfxBase = other.GfxBase;
            GfxHead = other.GfxHead;
            GfxPortrait = other.GfxPortrait;
            TransformType = other.TransformType;
            GfxBaseOriginal = other.GfxBaseOriginal;
            GfxHeadOriginal = other.GfxHeadOriginal;
            Animations = other.Animations;
            SfxAttack = new List<(string, List<string>)>();
            for (int i = 0; i < other.SfxAttack.Count; ++i)
                SfxAttack.Add((other.SfxAttack[i].AnimName, new List<string>(other.SfxAttack[i].Filenames)));
            SfxStep = other.SfxStep;
            SfxHit = new List<string>(other.SfxHit);
            SfxDie = new List<string>(other.SfxDie);
            SfxCritdie = new List<string>(other.SfxCritdie);
            SfxBlock = new List<string>(other.SfxBlock);
            SfxLevelup = other.SfxLevelup;
            SfxLowhp = other.SfxLowhp;
            SfxLowhpLoop = other.SfxLowhpLoop;
            MaxSpendableStatPoints = other.MaxSpendableStatPoints;
            MaxPointsPerStat = other.MaxPointsPerStat;
            PrevMaxhp = other.PrevMaxhp;
            PrevMaxmp = other.PrevMaxmp;
            PrevHp = other.PrevHp;
            PrevMp = other.PrevMp;
            PrevMaxResourceStats = new List<float>(other.PrevMaxResourceStats);
            PrevResourceStats = new List<float>(other.PrevResourceStats);
            Summons = new List<StatBlock>(other.Summons);
            Summoner = other.Summoner;
            PartyBuffs = new Queue<PowerID>(other.PartyBuffs);
            PowerFilter = new List<PowerID>(other.PowerFilter);
            InvincibleRequirements = new List<EventComponent>();
            for (int i = 0; i < other.InvincibleRequirements.Count; ++i)
                InvincibleRequirements.Add(other.InvincibleRequirements[i]);
            AbortNpcInteract = other.AbortNpcInteract;
            LayerReferenceOrder = new List<string>(other.LayerReferenceOrder);
            LayerDef = new List<List<uint>>();
            for (int i = 0; i < other.LayerDef.Count; ++i)
                LayerDef.Add(new List<uint>(other.LayerDef[i]));
            AnimationSlots = new Dictionary<string, string>(other.AnimationSlots);
            CritdieEnabled = other.CritdieEnabled;
            AiDebuffPower = other.AiDebuffPower;
            AiHitPower = other.AiHitPower;
        }

        /// <summary>?? <c>~StatBlock()</c>。/summary>
        public void Dispose()
        {
            if (Summoner != null && Summoner.Summons.Count != 0)
            {
                int parentRef = Summoner.Summons.IndexOf(this);

                if (parentRef != -1)
                    Summoner.Summons.RemoveAt(parentRef);

                Summoner = null;
            }

            RemoveSummons();

            if (SharedGameResources.Loot != null)
                SharedGameResources.Loot.RemoveFromEnemiesDroppingLoot(this);
        }

        public float Get(int stat)
        {
            if (stat == Stats.AbsMax)
                return Math.Max(Current[stat], Current[Stats.AbsMin]);
            else
                return Current[stat];
        }

        public int GetPrimary(int index)
        {
            return Primary[index] + PrimaryAdditional[index];
        }

        private bool LoadCoreStat(FileParser infile)
        {
            if (infile.Key == "speed")
            {
                float fvalue = Parse.ToFloat(infile.Val, 0);
                Speed = SpeedDefault = fvalue / SharedResources.Settings!.MaxFramesPerSec;
                return true;
            }
            else if (infile.Key == "cooldown")
            {
                Cooldown.Duration = (uint)Parse.ToDuration(infile.Val);
                return true;
            }
            else if (infile.Key == "cooldown_hit")
            {
                CooldownHit.Duration = (uint)Parse.ToDuration(infile.Val);
                CooldownHitEnabled = true;
                return true;
            }
            else if (infile.Key == "stat")
            {
                string val = infile.Val;
                string stat = Parse.PopFirstString(ref val);
                float value = Parse.PopFirstFloat(ref val);
                int offsetIndex = 0;

                for (int i = 0; i < Stats.Count; ++i)
                {
                    if (Stats.Key[i] == stat)
                    {
                        Starting[i] = value;
                        return true;
                    }
                }
                offsetIndex += Stats.Count;

                for (int i = 0; i < SharedResources.Eset!.DamageTypes.Types.Count; ++i)
                {
                    if (SharedResources.Eset.DamageTypes.Types[i].Min == stat)
                    {
                        Starting[offsetIndex + EngineSettings.DamageTypesSettings.IndexToMin(i)] = value;
                        return true;
                    }
                    else if (SharedResources.Eset.DamageTypes.Types[i].Max == stat)
                    {
                        Starting[offsetIndex + EngineSettings.DamageTypesSettings.IndexToMax(i)] = value;
                        return true;
                    }
                    else if (SharedResources.Eset.DamageTypes.Types[i].Resist == stat)
                    {
                        Starting[offsetIndex + EngineSettings.DamageTypesSettings.IndexToResist(i)] = value;
                        return true;
                    }
                }
                offsetIndex += SharedResources.Eset.DamageTypes.Count;

                for (int i = 0; i < SharedResources.Eset.ResourceStats.Stats.Count; ++i)
                {
                    for (int j = 0; j < EngineSettings.ResourceStatsSettings.StatCount; ++j)
                    {
                        if (SharedResources.Eset.ResourceStats.Stats[i].Ids[j] == stat)
                        {
                            Starting[offsetIndex + (i * EngineSettings.ResourceStatsSettings.StatCount) + j] = value;
                            return true;
                        }
                    }
                }
            }
            else if (infile.Key == "stat_per_level")
            {
                string val = infile.Val;
                string stat = Parse.PopFirstString(ref val);
                float value = Parse.PopFirstFloat(ref val);
                int offsetIndex = 0;

                for (int i = 0; i < Stats.Count; i++)
                {
                    if (Stats.Key[i] == stat)
                    {
                        PerLevel[i] = value;
                        return true;
                    }
                }
                offsetIndex += Stats.Count;

                for (int i = 0; i < SharedResources.Eset!.DamageTypes.Types.Count; ++i)
                {
                    if (SharedResources.Eset.DamageTypes.Types[i].Min == stat)
                    {
                        PerLevel[offsetIndex + EngineSettings.DamageTypesSettings.IndexToMin(i)] = value;
                        return true;
                    }
                    else if (SharedResources.Eset.DamageTypes.Types[i].Max == stat)
                    {
                        PerLevel[offsetIndex + EngineSettings.DamageTypesSettings.IndexToMax(i)] = value;
                        return true;
                    }
                    else if (SharedResources.Eset.DamageTypes.Types[i].Resist == stat)
                    {
                        PerLevel[offsetIndex + EngineSettings.DamageTypesSettings.IndexToResist(i)] = value;
                        return true;
                    }
                }
                offsetIndex += SharedResources.Eset.DamageTypes.Count;

                for (int i = 0; i < SharedResources.Eset.ResourceStats.Stats.Count; ++i)
                {
                    for (int j = 0; j < EngineSettings.ResourceStatsSettings.StatCount; ++j)
                    {
                        if (SharedResources.Eset.ResourceStats.Stats[i].Ids[j] == stat)
                        {
                            PerLevel[offsetIndex + (i * EngineSettings.ResourceStatsSettings.StatCount) + j] = value;
                            return true;
                        }
                    }
                }
            }
            else if (infile.Key == "stat_per_primary")
            {
                string val = infile.Val;
                string primStat = Parse.PopFirstString(ref val);
                int primStatIndex = SharedResources.Eset!.PrimaryStats.GetIndexByID(primStat);
                if (primStatIndex == SharedResources.Eset.PrimaryStats.Stats.Count)
                {
                    infile.Error("StatBlock: '%s' is not a valid primary stat.", primStat);
                    return true;
                }

                string stat = Parse.PopFirstString(ref val);
                float value = Parse.PopFirstFloat(ref val);
                int offsetIndex = 0;

                for (int i = 0; i < Stats.Count; i++)
                {
                    if (Stats.Key[i] == stat)
                    {
                        PerPrimary[primStatIndex][i] = value;
                        return true;
                    }
                }
                offsetIndex += Stats.Count;

                for (int i = 0; i < SharedResources.Eset.DamageTypes.Types.Count; ++i)
                {
                    if (SharedResources.Eset.DamageTypes.Types[i].Min == stat)
                    {
                        PerPrimary[primStatIndex][offsetIndex + EngineSettings.DamageTypesSettings.IndexToMin(i)] = value;
                        return true;
                    }
                    else if (SharedResources.Eset.DamageTypes.Types[i].Max == stat)
                    {
                        PerPrimary[primStatIndex][offsetIndex + EngineSettings.DamageTypesSettings.IndexToMax(i)] = value;
                        return true;
                    }
                    else if (SharedResources.Eset.DamageTypes.Types[i].Resist == stat)
                    {
                        PerPrimary[primStatIndex][offsetIndex + EngineSettings.DamageTypesSettings.IndexToResist(i)] = value;
                        return true;
                    }
                }
                offsetIndex += SharedResources.Eset.DamageTypes.Count;

                for (int i = 0; i < SharedResources.Eset.ResourceStats.Stats.Count; ++i)
                {
                    for (int j = 0; j < EngineSettings.ResourceStatsSettings.StatCount; ++j)
                    {
                        if (SharedResources.Eset.ResourceStats.Stats[i].Ids[j] == stat)
                        {
                            PerPrimary[primStatIndex][offsetIndex + (i * EngineSettings.ResourceStatsSettings.StatCount) + j] = value;
                            return true;
                        }
                    }
                }
            }
            else if (infile.Key == "vulnerable")
            {
                string val = infile.Val;
                string element = Parse.PopFirstString(ref val);
                float value = (Parse.PopFirstFloat(ref val) * -1) + 100;

                infile.Error("StatBlock: 'vulnerable' is deprecated. Use 'stat=%s_resist,%d' instead.", element, (int)value);

                for (int i = 0; i < SharedResources.Eset!.DamageTypes.Types.Count; ++i)
                {
                    if (element == SharedResources.Eset.DamageTypes.Types[i].Id)
                    {
                        Starting[Stats.Count + EngineSettings.DamageTypesSettings.IndexToResist(i)] = value;
                        return true;
                    }
                }
            }
            else if (infile.Key == "power_filter")
            {
                if (SharedGameResources.Powers != null)
                {
                    string val = infile.Val;
                    string powerId = Parse.PopFirstString(ref val);
                    while (powerId != "")
                    {
                        PowerID testId = SharedGameResources.Powers.VerifyID(Parse.ToPowerID(powerId), infile, !PowerManager.AllowZeroId);
                        if (testId > 0)
                        {
                            PowerFilter.Add(Parse.ToPowerID(powerId));
                        }
                        powerId = Parse.PopFirstString(ref val);
                    }
                }
                return true;
            }
            else if (infile.Key == "categories")
            {
                Categories.Clear();
                string val = infile.Val;
                string cat;
                while ((cat = Parse.PopFirstString(ref val)) != "")
                {
                    Categories.Add(cat);
                }
                return true;
            }
            else if (infile.Key == "melee_range")
            {
                MeleeRange = Parse.ToFloat(infile.Val);
                return true;
            }

            return false;
        }

        private bool LoadSfxStat(FileParser infile)
        {
            if (infile.NewSection && (infile.Section == "" || infile.Section == "stats"))
            {
                SfxAttack.Clear();
                SfxHit.Clear();
                SfxDie.Clear();
                SfxCritdie.Clear();
                SfxBlock.Clear();
            }

            if (infile.Key == "sfx_attack")
            {
                string val = infile.Val;
                string animName = Parse.PopFirstString(ref val);
                string filename = Parse.PopFirstString(ref val);

                int foundIndex = SfxAttack.Count;
                for (int i = 0; i < SfxAttack.Count; ++i)
                {
                    if (animName == SfxAttack[i].AnimName)
                    {
                        foundIndex = i;
                        break;
                    }
                }

                if (foundIndex == SfxAttack.Count)
                {
                    SfxAttack.Add((animName, new List<string> { filename }));
                }
                else
                {
                    if (!SfxAttack[foundIndex].Filenames.Contains(filename))
                    {
                        SfxAttack[foundIndex].Filenames.Add(filename);
                    }
                }

                return true;
            }
            else if (infile.Key == "sfx_hit")
            {
                if (!SfxHit.Contains(infile.Val))
                {
                    SfxHit.Add(infile.Val);
                }

                return true;
            }
            else if (infile.Key == "sfx_die")
            {
                if (!SfxDie.Contains(infile.Val))
                {
                    SfxDie.Add(infile.Val);
                }

                return true;
            }
            else if (infile.Key == "sfx_critdie")
            {
                if (!SfxCritdie.Contains(infile.Val))
                {
                    SfxCritdie.Add(infile.Val);
                }

                return true;
            }
            else if (infile.Key == "sfx_block")
            {
                if (!SfxBlock.Contains(infile.Val))
                {
                    SfxBlock.Add(infile.Val);
                }

                return true;
            }
            else if (infile.Key == "sfx_levelup")
            {
                SfxLevelup = infile.Val;

                return true;
            }
            else if (infile.Key == "sfx_lowhp")
            {
                string val = infile.Val;
                SfxLowhp = Parse.PopFirstString(ref val);
                if (val != "") SfxLowhpLoop = Parse.ToBool(val);

                return true;
            }

            return false;
        }

        public bool LoadRenderLayerStat(FileParser infile)
        {
            if (infile.Section == "render_layers")
            {
                if (infile.NewSection)
                {
                    LayerDef = new List<List<uint>>();
                    for (int i = 0; i < 8; ++i)
                        LayerDef.Add(new List<uint>());
                    LayerReferenceOrder = new List<string>();
                    AnimationSlots.Clear();
                }

                if (infile.Key == "layer")
                {
                    string val = infile.Val;
                    uint dir = (uint)Parse.ToDirection(Parse.PopFirstString(ref val));
                    if (dir > 7)
                    {
                        infile.Error("StatBlock: Render layer direction must be in range [0,7]");
                        Utils.LogErrorDialog("StatBlock: Render layer direction must be in range [0,7]");
                        SharedResources.Mods!.ResetModConfig();
                        Utils.Exit(1);
                    }

                    string layer = Parse.PopFirstString(ref val);
                    while (layer != "")
                    {
                        uint refPos;
                        for (refPos = 0; refPos < LayerReferenceOrder.Count; ++refPos)
                            if (layer == LayerReferenceOrder[(int)refPos])
                                break;
                        if (refPos == LayerReferenceOrder.Count)
                            LayerReferenceOrder.Add(layer);
                        LayerDef[(int)dir].Add(refPos);

                        AnimationSlots[layer] = "";

                        layer = Parse.PopFirstString(ref val);
                    }

                    return true;
                }
            }

            return false;
        }

        public bool LoadAnimationSlotStat(FileParser infile)
        {
            if (infile.Section == "animation_slots")
            {
                if (infile.Key == "slot")
                {
                    string val = infile.Val;
                    string slotId = Parse.PopFirstString(ref val);
                    string slotFilename = Parse.PopFirstString(ref val);

                    if (AnimationSlots.ContainsKey(slotId))
                        AnimationSlots[slotId] = slotFilename;
                    else
                        infile.Error("StatBlock: Slot %s does not having a matching render layer", slotId);

                    return true;
                }
            }

            return false;
        }

        private bool IsNpcStat(FileParser infile)
        {
            if (infile.Section == "npc") return true;
            else if (infile.Section == "dialog") return true;

            if (infile.Key == "gfx")
            {
                infile.Error("StatBlock: Warning! 'gfx' is deprecated. Use 'animations' instead.");
                Animations = infile.Val;
                return true;
            }
            else if (infile.Key == "direction") return true;
            else if (infile.Key == "talker") return true;
            else if (infile.Key == "portrait") return true;
            else if (infile.Key == "vendor") return true;
            else if (infile.Key == "vendor_requires_status") return true;
            else if (infile.Key == "vendor_requires_not_status") return true;
            else if (infile.Key == "constant_stock") return true;
            else if (infile.Key == "status_stock") return true;
            else if (infile.Key == "random_stock") return true;
            else if (infile.Key == "random_stock_count") return true;
            else if (infile.Key == "vox_intro") return true;

            return false;
        }

        public void Load(string filename)
        {
            using FileParser infile = new FileParser();
            if (!infile.Open(filename, FileParser.ModFile, FileParser.ErrorNormal))
                return;

            bool clearLoot = true;
            bool fleeRangeDefined = false;

            while (infile.Next())
            {
                if (infile.NewSection && (infile.Section == "" || infile.Section == "stats"))
                {
                    clearLoot = true;
                }

                int num = Parse.ToInt(infile.Val);
                float fnum = Parse.ToFloat(infile.Val);
                bool valid = LoadCoreStat(infile) || LoadSfxStat(infile) || LoadRenderLayerStat(infile) || LoadAnimationSlotStat(infile) || IsNpcStat(infile);

                if (infile.Key == "name") Name = SharedResources.Msg!.Get(infile.Val);
                else if (infile.Key == "humanoid") Humanoid = Parse.ToBool(infile.Val);
                else if (infile.Key == "lifeform") Lifeform = Parse.ToBool(infile.Val);
                else if (infile.Key == "level") Level = num;
                else if (infile.Key == "xp") Xp = (ulong)num;
                else if (infile.Key == "xp_scaling")
                {
                    if (SharedGameResources.XpScaling != null)
                    {
                        XpScalingTable = SharedGameResources.XpScaling.Load(infile.Val);
                    }
                }
                else if (infile.Key == "loot")
                {
                    if (clearLoot)
                    {
                        LootTable.Clear();
                        clearLoot = false;
                    }

                    LootTable.Add(new EventComponent());
                    string lootVal = infile.Val;
                    SharedGameResources.Loot!.ParseLoot(ref lootVal, LootTable[^1], LootTable);
                }
                else if (infile.Key == "loot_count")
                {
                    string val = infile.Val;
                    LootCount.X = Parse.PopFirstInt(ref val);
                    LootCount.Y = Parse.PopFirstInt(ref val);
                    if (LootCount.X != 0 || LootCount.Y != 0)
                    {
                        LootCount.X = Math.Max(LootCount.X, 1);
                        LootCount.Y = Math.Max(LootCount.Y, LootCount.X);
                    }
                }
                else if (infile.Key == "defeat_status") DefeatStatus = SharedGameResources.Camp!.RegisterStatus(infile.Val);
                else if (infile.Key == "convert_status") ConvertStatus = SharedGameResources.Camp!.RegisterStatus(infile.Val);
                else if (infile.Key == "first_defeat_loot")
                {
                    if (SharedGameResources.Items != null)
                        FirstDefeatLoot = SharedGameResources.Items.VerifyID(Parse.ToItemID(infile.Val), infile, ItemManager.VerifyAllowZero, !ItemManager.VerifyAllocate);
                }
                else if (infile.Key == "quest_loot")
                {
                    if (SharedGameResources.Items != null)
                    {
                        string val = infile.Val;
                        string reqStatus = Parse.PopFirstString(ref val);
                        string reqNotStatus = Parse.PopFirstString(ref val);

                        QuestLootId = SharedGameResources.Items.VerifyID(Parse.ToItemID(Parse.PopFirstString(ref val)), infile, ItemManager.VerifyAllowZero, !ItemManager.VerifyAllocate);
                        if (QuestLootId > 0)
                        {
                            QuestLootRequiresStatus = SharedGameResources.Camp!.RegisterStatus(reqStatus);
                            QuestLootRequiresNotStatus = SharedGameResources.Camp!.RegisterStatus(reqNotStatus);
                        }
                    }
                }
                else if (infile.Key == "flying")
                {
                    bool flying = Parse.ToBool(infile.Val);
                    if (flying)
                        MovementType = MapCollision.MoveFlying;
                    else
                        MovementType = MapCollision.MoveNormal;
                }
                else if (infile.Key == "intangible")
                {
                    bool intangible = Parse.ToBool(infile.Val);
                    if (intangible)
                        MovementType = MapCollision.MoveIntangible;
                    else
                        MovementType = MapCollision.MoveNormal;
                }
                else if (infile.Key == "facing") Facing = Parse.ToBool(infile.Val);
                else if (infile.Key == "waypoint_pause") WaypointTimer.Duration = (uint)Parse.ToDuration(infile.Val);
                else if (infile.Key == "turn_delay") TurnDelay = Parse.ToDuration(infile.Val);
                else if (infile.Key == "chance_pursue") ChancePursue = fnum;
                else if (infile.Key == "chance_flee") ChanceFlee = fnum;
                else if (infile.Key == "power")
                {
                    AIPower aiPower = new AIPower();

                    string val = infile.Val;
                    string aiType = Parse.PopFirstString(ref val);

                    if (SharedGameResources.Powers != null)
                        aiPower.Id = SharedGameResources.Powers.VerifyID(Parse.ToPowerID(Parse.PopFirstString(ref val)), infile, !PowerManager.AllowZeroId);

                    if (aiPower.Id == 0)
                        continue;

                    aiPower.Chance = Parse.PopFirstFloat(ref val);

                    if (aiType == "melee") aiPower.Type = AiPowerMelee;
                    else if (aiType == "ranged") aiPower.Type = AiPowerRanged;
                    else if (aiType == "beacon") aiPower.Type = AiPowerBeacon;
                    else if (aiType == "on_hit") aiPower.Type = AiPowerHit;
                    else if (aiType == "on_death") aiPower.Type = AiPowerDeath;
                    else if (aiType == "on_half_dead") aiPower.Type = AiPowerHalfDead;
                    else if (aiType == "on_join_combat") aiPower.Type = AiPowerJoinCombat;
                    else if (aiType == "on_debuff") aiPower.Type = AiPowerDebuff;
                    else
                    {
                        infile.Error("StatBlock: '%s' is not a valid enemy power type.", aiType);
                        continue;
                    }

                    if (aiPower.Type == AiPowerHalfDead)
                        HalfDeadPower = true;

                    PowersAi.Add(aiPower);
                }
                else if (infile.Key == "passive_powers")
                {
                    if (SharedGameResources.Powers != null)
                    {
                        PowersPassive.Clear();
                        string val = infile.Val;
                        string p = Parse.PopFirstString(ref val);
                        while (p != "")
                        {
                            PowerID passiveId = SharedGameResources.Powers.VerifyID(Parse.ToPowerID(p), infile, !PowerManager.AllowZeroId);

                            if (SharedGameResources.Powers.IsValid(passiveId))
                            {
                                PowersPassive.Add(passiveId);

                                Power passivePower = SharedGameResources.Powers.Powers[passiveId];
                                for (int i = 0; i < passivePower.ChainPowers.Count; ++i)
                                {
                                    ChainPower chainPower = passivePower.ChainPowers[i];
                                    if (chainPower.Type == ChainPower.TypePost)
                                    {
                                        AIPower passivePostPower = new AIPower();
                                        passivePostPower.Type = AiPowerPassivePost;
                                        passivePostPower.Id = chainPower.Id;
                                        passivePostPower.Chance = 0;
                                        PowersAi.Add(passivePostPower);
                                    }
                                }
                            }

                            p = Parse.PopFirstString(ref val);
                        }
                    }
                }
                else if (infile.Key == "threat_range")
                {
                    string val = infile.Val;
                    ThreatRange = Parse.PopFirstFloat(ref val);

                    string trFar = Parse.PopFirstString(ref val);
                    if (trFar != "")
                        ThreatRangeFar = Parse.ToFloat(trFar);
                    else
                        ThreatRangeFar = ThreatRange * 2;
                }
                else if (infile.Key == "flee_range")
                {
                    FleeRange = fnum;
                    fleeRangeDefined = true;
                }
                else if (infile.Key == "combat_style")
                {
                    if (infile.Val == "default") CombatStyle = CombatDefault;
                    else if (infile.Val == "aggressive") CombatStyle = CombatAggressive;
                    else if (infile.Val == "passive") CombatStyle = CombatPassive;
                    else infile.Error("StatBlock: Unknown combat style '%s'", infile.Val);
                }
                else if (infile.Key == "animations") Animations = infile.Val;
                else if (infile.Key == "suppress_hp") SuppressHp = Parse.ToBool(infile.Val);
                else if (infile.Key == "flee_duration") FleeTimer.Duration = (uint)Parse.ToDuration(infile.Val);
                else if (infile.Key == "flee_cooldown") FleeCooldownTimer.Duration = (uint)Parse.ToDuration(infile.Val);
                else if (infile.Key == "rarity") { }
                else if (infile.Key == "corpse_has_collision") CorpseHasCollision = Parse.ToBool(infile.Val);
                else if (infile.Key == "corpse_has_timeout") CorpseHasTimeout = Parse.ToBool(infile.Val);
                else if (infile.Key == "corpse_render_below") CorpseRenderBelow = Parse.ToBool(infile.Val);
                else if (infile.Key == "cooldown_los") CooldownLos.Duration = (uint)Parse.ToDuration(infile.Val);
                else if (infile.Key == "resting_hp_regen_time")
                {
                    float t = (float)Parse.ToDuration(infile.Val);
                    RestingHpRegenSeconds = t / SharedResources.Settings!.MaxFramesPerSec;
                }
                else if (!valid)
                {
                    infile.Error("StatBlock: '%s' is not a valid key.", infile.Key);
                }
            }
            infile.Close();

            Hp = Starting[Stats.HpMax];
            Mp = Starting[Stats.MpMax];

            int resourceOffsetIndex = Stats.Count + SharedResources.Eset!.DamageTypes.Count;
            for (int i = 0; i < ResourceStats.Count; ++i)
            {
                ResourceStats[i] = Starting[resourceOffsetIndex + (i * EngineSettings.ResourceStatsSettings.StatCount) + EngineSettings.ResourceStatsSettings.StatBase];
            }

            if (!fleeRangeDefined)
                FleeRange = ThreatRange / 2;

            ApplyEffects();
        }

        public void TakeDamage(float dmg, bool crit, int sourceType)
        {
            Hp -= Effects.DamageShields(dmg);
            if (Hp <= 0)
            {
                Hp = 0;

                Effects.TriggeredDeath = true;

                if (Hero)
                {
                    CurState = EntityDead;
                }
                else
                {
                    if (!HeroAlly || Converted)
                    {
                        if (QuestLootRequiresStatus != 0)
                        {
                            if (!(SharedGameResources.Camp!.CheckStatus(QuestLootRequiresStatus) && !SharedGameResources.Camp!.CheckStatus(QuestLootRequiresNotStatus)))
                            {
                                QuestLootId = 0;
                            }
                        }

                        if (DefeatStatus != 0)
                        {
                            if (FirstDefeatLoot > 0)
                            {
                                if (!SharedGameResources.Camp!.CheckStatus(DefeatStatus))
                                {
                                    QuestLootId = FirstDefeatLoot;
                                }
                            }

                            SharedGameResources.Camp!.SetStatus(DefeatStatus);
                        }

                        float xpMultiplier = 1;
                        if (sourceType == Power.SourceTypeAlly)
                            xpMultiplier = SharedResources.Eset!.Misc.PartyExpPercentage / 100.0f;

                        xpMultiplier *= SharedGameResources.XpScaling!.GetMultiplier(this, SharedGameResources.Pc!.Stats);

                        SharedGameResources.Camp!.RewardXp((float)Xp * xpMultiplier, !CampaignManager.XpShowMsg);

                        SharedGameResources.Loot!.AddEnemyLoot(this);
                    }

                    if (crit && CritdieEnabled)
                        CurState = EntityCritdead;
                    else
                        CurState = EntityDead;

                    if (!CorpseHasCollision)
                        SharedGameResources.Mapr!.Collider.Unblock(Pos.X, Pos.Y);
                }

            }
        }

        public void Recalc()
        {
            if (Hero)
            {
                if (!_statsLoaded) LoadHeroStats();

                RefreshStats = true;

                ulong xpMax = SharedResources.Eset!.Xp.GetLevelXP(SharedResources.Eset.Xp.GetMaxLevel());
                Xp = Math.Min(Xp, xpMax);

                Level = SharedResources.Eset.Xp.GetLevelFromXP(Xp);
                if (Level != 0)
                    CheckTitle = true;
            }

            if (Level < 1)
                Level = 1;

            ApplyEffects();

            Hp = Get(Stats.HpMax);
            Mp = Get(Stats.MpMax);

            for (int i = 0; i < ResourceStats.Count; ++i)
            {
                ResourceStats[i] = GetResourceStat(i, EngineSettings.ResourceStatsSettings.StatBase);
            }

        }

        public void CalcBase()
        {
            float lev0 = (float)Math.Max(Level - 1, 0);

            if (PerPrimary.Count == 0)
            {
                for (int i = 0; i < GetFullStatCount(); ++i)
                {
                    Base[i] = Starting[i] + (lev0 * PerLevel[i]);
                }
            }
            else
            {
                for (int j = 0; j < PerPrimary.Count; ++j)
                {
                    float currentPrimary = (float)Math.Max(GetPrimary(j) - 1, 0);
                    List<float> perPrimaryVec = PerPrimary[j];
                    for (int i = 0; i < GetFullStatCount(); ++i)
                    {
                        if (j == 0)
                            Base[i] = Starting[i] + (lev0 * PerLevel[i]);
                        Base[i] += (currentPrimary * perPrimaryVec[i]);
                    }
                }
            }

            for (int i = 0; i < SharedResources.Eset!.DamageTypes.Types.Count; ++i)
            {
                Base[Stats.Count + EngineSettings.DamageTypesSettings.IndexToMin(i)] += ItemBaseDmg[i].Min;
                Base[Stats.Count + EngineSettings.DamageTypesSettings.IndexToMax(i)] += ItemBaseDmg[i].Max;
                Base[Stats.Count + EngineSettings.DamageTypesSettings.IndexToMin(i)] = Math.Max(Base[Stats.Count + EngineSettings.DamageTypesSettings.IndexToMin(i)], 0.0f);
                Base[Stats.Count + EngineSettings.DamageTypesSettings.IndexToMax(i)] = Math.Max(Base[Stats.Count + EngineSettings.DamageTypesSettings.IndexToMax(i)], Base[Stats.Count + EngineSettings.DamageTypesSettings.IndexToMin(i)]);
            }

            Base[Stats.AbsMin] += ItemBaseAbs.Min;
            Base[Stats.AbsMax] += ItemBaseAbs.Max;
            Base[Stats.AbsMin] = Math.Max(Base[Stats.AbsMin], 0.0f);
            Base[Stats.AbsMax] = Math.Max(Base[Stats.AbsMax], Base[Stats.AbsMin]);
        }

        public void ApplyEffects()
        {
            PrevMaxhp = Math.Max(Get(Stats.HpMax), 1.0f);
            PrevMaxmp = Math.Max(Get(Stats.MpMax), 1.0f);
            PrevHp = Hp;
            PrevMp = Mp;

            for (int i = 0; i < ResourceStats.Count; ++i)
            {
                PrevMaxResourceStats[i] = Math.Max(GetResourceStat(i, EngineSettings.ResourceStatsSettings.StatBase), 1.0f);
                PrevResourceStats[i] = ResourceStats[i];
            }

            for (int i = 0; i < Primary.Count; ++i)
            {
                if (GetPrimary(i) != Primary[i] + Effects.BonusPrimary[i])
                    RefreshStats = true;

                PrimaryAdditional[i] = Effects.BonusPrimary[i];
            }

            CalcBase();

            for (int i = 0; i < GetFullStatCount(); ++i)
            {
                Current[i] = (Base[i] + Effects.Bonus[i]) * Effects.BonusMultiplier[i];
            }

            Current[Stats.HpMax] = Math.Max(Get(Stats.HpMax), 1.0f);
            Current[Stats.MpMax] = Math.Max(Get(Stats.MpMax), 1.0f);

            if (Hp > Get(Stats.HpMax)) Hp = Get(Stats.HpMax);
            if (Mp > Get(Stats.MpMax)) Mp = Get(Stats.MpMax);

            int resourceOffsetIndex = Stats.Count + SharedResources.Eset!.DamageTypes.Count;
            for (int i = 0; i < ResourceStats.Count; ++i)
            {
                int currentIndex = resourceOffsetIndex + (i * EngineSettings.ResourceStatsSettings.StatCount) + EngineSettings.ResourceStatsSettings.StatBase;
                Current[currentIndex] = Math.Max(GetResourceStat(i, EngineSettings.ResourceStatsSettings.StatBase), 1.0f);
                if (ResourceStats[i] > Current[currentIndex])
                {
                    ResourceStats[i] = Current[currentIndex];
                }
            }

            Speed = SpeedDefault;
        }

        public void Logic()
        {
            Alive = !(Hp <= 0 && !Effects.TriggeredDeath && !Effects.Revive);

            if (SharedGameResources.Entitym != null && SharedGameResources.Powers != null)
            {
                while (PartyBuffs.Count > 0)
                {
                    PowerID powerIndex = PartyBuffs.Dequeue();
                    Power buffPower = SharedGameResources.Powers.Powers[powerIndex];

                    for (int i = 0; i < SharedGameResources.Entitym.Entities.Count; ++i)
                    {
                        Entity partyMember = SharedGameResources.Entitym.Entities[i];
                        if (partyMember.Stats.Hp > 0 &&
                           ((partyMember.Stats.HeroAlly && Hero) || (partyMember.Stats.EnemyAlly && partyMember.Stats.Summoner == this)) &&
                           (buffPower.BuffPartyPowerId == 0 || buffPower.BuffPartyPowerId == partyMember.Stats.SummonedPowerIndex)
                        )
                        {
                            SharedGameResources.Powers.Effect(partyMember.Stats, this, powerIndex, (Hero ? Power.SourceTypeHero : Power.SourceTypeEnemy));
                        }
                    }
                }
            }

            Effects.Logic();

            ApplyEffects();

            if (Hero && Effects.RefreshStats)
            {
                RefreshStats = true;
                Effects.RefreshStats = false;
            }

            if (PrevMaxhp != Get(Stats.HpMax))
            {
                Hp = (PrevHp / PrevMaxhp) * Get(Stats.HpMax);
            }
            if (PrevMaxmp != Get(Stats.MpMax))
            {
                Mp = (PrevMp / PrevMaxmp) * Get(Stats.MpMax);
            }

            for (int i = 0; i < ResourceStats.Count; ++i)
            {
                float resourceStatMax = GetResourceStat(i, EngineSettings.ResourceStatsSettings.StatBase);
                if (PrevMaxResourceStats[i] != resourceStatMax)
                {
                    ResourceStats[i] = (PrevResourceStats[i] / PrevMaxResourceStats[i]) * resourceStatMax;
                }
            }

            Cooldown.Tick();
            CooldownLos.Tick();

            for (int i = 0; i < PowersAi.Count; ++i)
            {
                PowersAi[i].Cooldown.Tick();
            }

            if (Hp <= Get(Stats.HpMax) && Hp > 0)
            {
                float hpRegenPerFrame = 0;
                if (!InCombat && !HeroAlly && !Hero && SharedGameResources.Pc!.Stats.Alive)
                {
                    if (RestingHpRegenSeconds > 0)
                    {
                        hpRegenPerFrame = Get(Stats.HpMax) / RestingHpRegenSeconds / SharedResources.Settings!.MaxFramesPerSec;
                    }
                }
                else
                {
                    hpRegenPerFrame = Get(Stats.HpRegen) / 60.0f / SharedResources.Settings!.MaxFramesPerSec;
                }
                Hp += hpRegenPerFrame;
                Hp = Math.Max(0.0f, Math.Min(Hp, Get(Stats.HpMax)));
            }

            if (Mp <= Get(Stats.MpMax) && Hp > 0)
            {
                float mpRegenPerFrame = Get(Stats.MpRegen) / 60.0f / SharedResources.Settings!.MaxFramesPerSec;
                Mp += mpRegenPerFrame;
                Mp = Math.Max(0.0f, Math.Min(Mp, Get(Stats.MpMax)));
            }

            for (int i = 0; i < ResourceStats.Count; ++i)
            {
                float resourceStatMax = GetResourceStat(i, EngineSettings.ResourceStatsSettings.StatBase);
                float resourceStatRegen = GetResourceStat(i, EngineSettings.ResourceStatsSettings.StatRegen);

                if (ResourceStats[i] <= resourceStatMax && Hp > 0)
                {
                    float regenPerFrame = resourceStatRegen / 60.0f / SharedResources.Settings!.MaxFramesPerSec;
                    ResourceStats[i] += regenPerFrame;
                    ResourceStats[i] = Math.Max(0.0f, Math.Min(ResourceStats[i], resourceStatMax));
                }
            }

            if (TransformDuration > 0)
                TransformDuration--;

            if (Effects.Damage > 0 && Hp > 0)
            {
                float damage = Effects.Damage;
                damage = SharedResources.Eset!.Combat.ResourceRound(damage);
                TakeDamage(damage, !TakeDmgCrit, Effects.GetDamageSourceType(Effect.Damage));
                SharedResources.Comb!.AddFloat(damage, Pos, CombatText.MsgTakedmg);
            }
            if (Effects.DamagePercent > 0 && Hp > 0)
            {
                float damage = (Get(Stats.HpMax) * Effects.DamagePercent) / 100;
                damage = SharedResources.Eset!.Combat.ResourceRound(damage);
                TakeDamage(damage, !TakeDmgCrit, Effects.GetDamageSourceType(Effect.DamagePercent));
                SharedResources.Comb!.AddFloat(damage, Pos, CombatText.MsgTakedmg);
            }

            if (Effects.DeathSentence)
                TakeDamage(Get(Stats.HpMax), !TakeDmgCrit, Power.SourceTypeNeutral);

            CooldownHit.Tick();

            if (Effects.Stun)
            {
                StateTimer.Reset(Timer.End);
                ChargeSpeed = 0;
            }

            StateTimer.Tick();

            if (Effects.Hpot > 0)
            {
                float hpot = Effects.Hpot;
                hpot = SharedResources.Eset!.Combat.ResourceRound(hpot);
                SharedResources.Comb!.AddString(SharedResources.Msg!.GetV("+%s HP", Utils.FloatToString(hpot, SharedResources.Eset.NumberFormat.CombatText)), Pos, CombatText.MsgBuff);
                Hp += hpot;
                if (Hp > Get(Stats.HpMax)) Hp = Get(Stats.HpMax);
            }
            if (Effects.HpotPercent > 0)
            {
                float hpot = (Get(Stats.HpMax) * Effects.HpotPercent) / 100;
                hpot = SharedResources.Eset!.Combat.ResourceRound(hpot);
                SharedResources.Comb!.AddString(SharedResources.Msg!.GetV("+%s HP", Utils.FloatToString(hpot, SharedResources.Eset.NumberFormat.CombatText)), Pos, CombatText.MsgBuff);
                Hp += hpot;
                if (Hp > Get(Stats.HpMax)) Hp = Get(Stats.HpMax);
            }
            if (Effects.Mpot > 0)
            {
                float mpot = Effects.Mpot;
                mpot = SharedResources.Eset!.Combat.ResourceRound(mpot);
                SharedResources.Comb!.AddString(SharedResources.Msg!.GetV("+%s MP", Utils.FloatToString(mpot, SharedResources.Eset.NumberFormat.CombatText)), Pos, CombatText.MsgBuff);
                Mp += mpot;
                if (Mp > Get(Stats.MpMax)) Mp = Get(Stats.MpMax);
            }
            if (Effects.MpotPercent > 0)
            {
                float mpot = (Get(Stats.MpMax) * Effects.MpotPercent) / 100;
                mpot = SharedResources.Eset!.Combat.ResourceRound(mpot);
                SharedResources.Comb!.AddString(SharedResources.Msg!.GetV("+%s MP", Utils.FloatToString(mpot, SharedResources.Eset.NumberFormat.CombatText)), Pos, CombatText.MsgBuff);
                Mp += mpot;
                if (Mp > Get(Stats.MpMax)) Mp = Get(Stats.MpMax);
            }
            for (int i = 0; i < ResourceStats.Count; ++i)
            {
                if (Effects.ResourceOt[i] > 0)
                {
                    float resourceMax = GetResourceStat(i, EngineSettings.ResourceStatsSettings.StatBase);
                    float resourceOt = Effects.ResourceOt[i];
                    resourceOt = SharedResources.Eset!.Combat.ResourceRound(resourceOt);
                    SharedResources.Comb!.AddString("+" + Utils.FloatToString(resourceOt, SharedResources.Eset.NumberFormat.CombatText) + " " + SharedResources.Eset.ResourceStats.Stats[i].TextCombatHeal, Pos, CombatText.MsgBuff);
                    ResourceStats[i] += resourceOt;

                    if (ResourceStats[i] > resourceMax)
                        ResourceStats[i] = resourceMax;
                }
                if (Effects.ResourceOtPercent[i] > 0)
                {
                    float resourceMax = GetResourceStat(i, EngineSettings.ResourceStatsSettings.StatBase);
                    float resourceOt = (resourceMax * Effects.ResourceOtPercent[i]) / 100;
                    resourceOt = SharedResources.Eset!.Combat.ResourceRound(resourceOt);
                    SharedResources.Comb!.AddString("+" + Utils.FloatToString(resourceOt, SharedResources.Eset.NumberFormat.CombatText) + " " + SharedResources.Eset.ResourceStats.Stats[i].TextCombatHeal, Pos, CombatText.MsgBuff);
                    ResourceStats[i] += resourceOt;

                    if (ResourceStats[i] > resourceMax)
                        ResourceStats[i] = resourceMax;
                }
            }

            if (Hp == 0)
                RemoveSummons();

            if (Effects.KnockbackSpeed != 0)
            {
                float theta = Utils.CalcTheta(KnockbackSrcpos.X, KnockbackSrcpos.Y, KnockbackDestpos.X, KnockbackDestpos.Y);
                KnockbackSpeed.X = Effects.KnockbackSpeed * MathF.Cos(theta);
                KnockbackSpeed.Y = Effects.KnockbackSpeed * MathF.Sin(theta);

                float posX = Pos.X;
                float posY = Pos.Y;
                SharedGameResources.Mapr!.Collider.Unblock(posX, posY);
                SharedGameResources.Mapr!.Collider.Move(ref posX, ref posY, KnockbackSpeed.X, KnockbackSpeed.Y, MovementType, SharedGameResources.Mapr!.Collider.GetCollideType(Hero));
                Pos = new Vector2(posX, posY);
                SharedGameResources.Mapr!.Collider.Block(Pos.X, Pos.Y, HeroAlly);
            }
            else if (ChargeSpeed != 0.0f)
            {
                float tmpSpeed = ChargeSpeed * SpeedMultiplier[Direction];
                float dx = tmpSpeed * DirectionDeltaX[Direction];
                float dy = tmpSpeed * DirectionDeltaY[Direction];

                float posX = Pos.X;
                float posY = Pos.Y;
                SharedGameResources.Mapr!.Collider.Unblock(posX, posY);
                SharedGameResources.Mapr!.Collider.Move(ref posX, ref posY, dx, dy, MovementType, SharedGameResources.Mapr!.Collider.GetCollideType(Hero));
                Pos = new Vector2(posX, posY);
                SharedGameResources.Mapr!.Collider.Block(Pos.X, Pos.Y, HeroAlly);
            }

            WaypointTimer.Tick();

            if (Hp <= 0 && Effects.Revive)
            {
                Hp = Get(Stats.HpMax);
                Alive = true;
                Corpse = false;
                CurState = EntityStance;
            }
            else if (Hp <= 0 && CurState != EntityDead && CurState != EntityCritdead)
            {
                TakeDamage(0, false, Power.SourceTypeNeutral);
            }

            if (!Hero && Effects.Convert != Converted)
            {
                Converted = !Converted;
                HeroAlly = !HeroAlly;
                if (ConvertStatus != 0)
                {
                    SharedGameResources.Camp!.SetStatus(ConvertStatus);
                }
            }
        }

        public bool CanUsePower(PowerID powerid, bool allowPassive)
        {
            if (!SharedGameResources.Powers!.IsValid(powerid))
                return false;

            Power power = SharedGameResources.Powers.Powers[powerid];

            if (!Alive)
            {
                return false;
            }
            else if (!Hero)
            {
                return true;
            }
            else if (Transformed)
            {
                return Mp >= power.RequiresMp;
            }
            else
            {
                return (
                    SharedGameResources.Powers.CheckPowerCost(power, this)
                    && (!power.Passive || allowPassive)
                    && !power.MetaPower
                    && (!Effects.Stun || (allowPassive && power.Passive))
                    && SharedGameResources.Powers.CheckRequiredResourceState(power, this)
                    && (!power.RequiresCorpse || (TargetCorpse != null && !TargetCorpse.CorpseTimer.IsEnd()) || (TargetNearestCorpse != null && SharedGameResources.Powers.CheckNearestTargeting(power, this, true) && !TargetNearestCorpse.CorpseTimer.IsEnd()))
                    && (CheckRequiredSpawns(power.RequiresSpawns))
                    && (SharedGameResources.MenuPowers != null && SharedGameResources.MenuPowers.MeetsUsageStats(powerid))
                    && (power.Type == Power.TypeSpawn ? !SummonLimitReached(powerid) : true)
                    && !(power.SpawnType == "untransform" && !Transformed)
                    && power.RequiresFlags.All(flag => EquipFlags.Contains(flag))
                    && (!power.BuffParty || (power.BuffParty && SharedGameResources.Entitym != null && SharedGameResources.Entitym.CheckPartyMembers()))
                    && SharedGameResources.Powers.CheckRequiredItems(power, this)
                );
            }

        }

        private void LoadHeroStats()
        {
            Cooldown.Duration = (uint)Parse.ToDuration("66ms");

            using FileParser infile = new FileParser();
            if (infile.Open("engine/stats.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    int value = Parse.ToInt(infile.Val);

                    bool valid = LoadCoreStat(infile) || LoadRenderLayerStat(infile);

                    if (infile.Key == "max_points_per_stat")
                    {
                        MaxPointsPerStat = value;
                    }
                    else if (infile.Key == "sfx_step")
                    {
                        SfxStep = infile.Val;
                    }
                    else if (infile.Key == "stat_points_per_level")
                    {
                        StatPointsPerLevel = value;
                    }
                    else if (infile.Key == "power_points_per_level")
                    {
                        PowerPointsPerLevel = value;
                    }
                    else if (!valid)
                    {
                        infile.Error("StatBlock: '%s' is not a valid key.", infile.Key);
                    }
                }
                infile.Close();
            }

            if (MaxPointsPerStat == 0) MaxPointsPerStat = MaxSpendableStatPoints / 4 + 1;
            _statsLoaded = true;

            MaxSpendableStatPoints = SharedResources.Eset!.Xp.GetMaxLevel() * StatPointsPerLevel;
        }

        public void LoadHeroSfx()
        {
            using FileParser infile = new FileParser();
            if (infile.Open("engine/avatar/" + GfxBase + ".txt", FileParser.ModFile, FileParser.ErrorNone))
            {
                while (infile.Next())
                {
                    LoadSfxStat(infile);
                }
                infile.Close();
            }
        }

        public void RemoveSummons()
        {
            for (int i = 0; i < Summons.Count; ++i)
            {
                Summons[i].TakeDamage(Summons[i].Get(Stats.HpMax), !TakeDmgCrit, Power.SourceTypeNeutral);
                Summons[i].RemoveSummons();
                Summons[i].Summoner = null;
            }

            Summons.Clear();
        }

        public bool SummonLimitReached(PowerID powerId)
        {
            if (!SharedGameResources.Powers!.IsValid(powerId))
                return true;

            Power spawnPower = SharedGameResources.Powers.Powers[powerId];

            int maxSummons = 0;

            if (spawnPower.SpawnLimitMode == Power.SpawnLimitModeFixed)
            {
                maxSummons = (int)spawnPower.SpawnLimitCount;
            }
            else if (spawnPower.SpawnLimitMode == Power.SpawnLimitModeStat)
            {
                int statVal = 1;
                if (spawnPower.SpawnLimitStat < SharedResources.Eset!.PrimaryStats.Stats.Count)
                {
                    statVal = GetPrimary(spawnPower.SpawnLimitStat);
                }
                maxSummons = (int)(spawnPower.SpawnLimitCount * ((float)statVal / spawnPower.SpawnLimitRatio));
            }
            else
            {
                return false;
            }

            maxSummons = Math.Max(maxSummons, 1);

            int qtySummons = 0;

            for (int i = 0; i < Summons.Count; i++)
            {
                if (Summons[i].SummonedPowerIndex == powerId && Summons[i].CurState != EntityDead && Summons[i].CurState != EntityCritdead)
                {
                    qtySummons++;
                }
            }

            return qtySummons >= maxSummons;
        }

        public void UpdateSummonPowerIDs(PowerID oldId, PowerID newId)
        {
            Power oldPwr = SharedGameResources.Powers!.Powers[oldId];

            if (oldPwr.SpawnType == "")
                return;

            bool matchingSpawnTypes = false;
            if (newId != 0)
            {
                Power newPwr = SharedGameResources.Powers.Powers[newId];
                matchingSpawnTypes = (oldPwr.SpawnType == newPwr.SpawnType);
            }

            for (int i = 0; i < Summons.Count; ++i)
            {
                if (Summons[i].SummonedPowerIndex == oldId)
                {
                    if (matchingSpawnTypes)
                    {
                        Summons[i].SummonedPowerIndex = newId;
                    }
                    else
                    {
                        if (oldPwr.SpawnRequiresUnlockedPower)
                        {
                            Summons[i].TakeDamage(Summons[i].Get(Stats.HpMax), !TakeDmgCrit, Power.SourceTypeNeutral);
                            Summons[i].RemoveSummons();
                            Summons[i].Summoner = null;
                        }
                    }
                }
            }

            if (!matchingSpawnTypes)
            {
                for (int i = Summons.Count; i > 0; i--)
                {
                    if (Summons[i - 1].Summoner == null)
                        Summons.RemoveAt(i - 1);
                }
            }
        }

        public void SetWanderArea(int r)
        {
            WanderArea.X = (int)MathF.Floor(Pos.X) - r;
            WanderArea.Y = (int)MathF.Floor(Pos.Y) - r;
            WanderArea.Width = WanderArea.Height = (r * 2) + 1;
        }

        public string GetShortClass()
        {
            if (CharacterSubclass == "")
                return SharedResources.Msg!.Get(CharacterClass);
            else
                return SharedResources.Msg!.Get(CharacterSubclass);
        }

        public string GetLongClass()
        {
            if (CharacterSubclass == "" || CharacterClass == CharacterSubclass)
                return SharedResources.Msg!.Get(CharacterClass);
            else
                return SharedResources.Msg!.Get(CharacterClass) + " / " + SharedResources.Msg!.Get(CharacterSubclass);
        }

        public void AddXp(int amount)
        {
            Xp += (ulong)amount;

            ulong xpMax = SharedResources.Eset!.Xp.GetLevelXP(SharedResources.Eset.Xp.GetMaxLevel());
            Xp = Math.Min(Xp, xpMax);
        }

        public AIPower? GetAIPower(int aiType)
        {
            List<int> possibleIds = new List<int>();

            for (int i = 0; i < PowersAi.Count; ++i)
            {
                if (aiType != PowersAi[i].Type)
                    continue;

                if (!MathUtils.PercentChanceF(PowersAi[i].Chance))
                    continue;

                if (!PowersAi[i].Cooldown.IsEnd())
                    continue;

                if (SharedGameResources.Powers!.Powers[PowersAi[i].Id].Type == Power.TypeSpawn)
                {
                    if (SummonLimitReached(PowersAi[i].Id))
                        continue;
                }

                if (!CheckRequiredSpawns(SharedGameResources.Powers!.Powers[PowersAi[i].Id].RequiresSpawns))
                    continue;

                possibleIds.Add(i);
            }

            if (possibleIds.Count > 0)
            {
                int index = MathUtils.RandBetween(0, possibleIds.Count - 1);
                return PowersAi[possibleIds[index]];
            }

            return null;
        }

        private bool CheckRequiredSpawns(int reqAmount)
        {
            if (reqAmount <= 0)
                return true;

            int liveSummonCount = 0;
            for (int j = 0; j < Summons.Count; ++j)
            {
                if (Summons[j].Hp > 0)
                {
                    ++liveSummonCount;
                }
            }

            if (liveSummonCount < reqAmount)
                return false;

            return true;
        }

        public int GetPowerCooldown(PowerID powerId)
        {
            if (Hero)
            {
                return (int)SharedGameResources.Pc!.PowerCooldownTimers[powerId].Duration;
            }
            else
            {
                for (int i = 0; i < PowersAi.Count; ++i)
                {
                    if (powerId == PowersAi[i].Id)
                        return (int)PowersAi[i].Cooldown.Duration;
                }
            }

            return 0;
        }

        public void SetPowerCooldown(PowerID powerId, int powerCooldown)
        {
            if (Hero)
            {
                SharedGameResources.Pc!.PowerCooldownTimers[powerId].Duration = (uint)powerCooldown;
            }
            else
            {
                for (int i = 0; i < PowersAi.Count; ++i)
                {
                    if (powerId == PowersAi[i].Id)
                    {
                        PowersAi[i].Cooldown.Duration = (uint)powerCooldown;
                        break;
                    }
                }
            }
        }

        public float GetDamageMin(int dmgType)
        {
            return Current[Stats.Count + EngineSettings.DamageTypesSettings.IndexToMin(dmgType)];
        }

        public float GetDamageMax(int dmgType)
        {
            return Math.Max(Current[Stats.Count + EngineSettings.DamageTypesSettings.IndexToMin(dmgType)], Current[Stats.Count + EngineSettings.DamageTypesSettings.IndexToMax(dmgType)]);
        }

        public float GetDamageResist(int dmgType)
        {
            return Current[Stats.Count + EngineSettings.DamageTypesSettings.IndexToResist(dmgType)];
        }

        public float GetResourceStat(int resourceIndex, int fieldOffset)
        {
            int offsetIndex = Stats.Count + SharedResources.Eset!.DamageTypes.Count;
            return Current[offsetIndex + (resourceIndex * 4) + fieldOffset];
        }

        public void CheckGfxPaths()
        {
            if (SharedResources.Mods!.List("animations/avatar/" + GfxBase, !ModManager.ListFullPaths).Count == 0)
            {
                GfxBaseOriginal = GfxBase;

                if (GfxBase.Length >= 6 && GfxBase.Substring(0, 6) == "female")
                    GfxBase = "female";
                else
                    GfxBase = "male";
            }

            if (SharedResources.Mods!.Locate("animations/avatar/" + GfxBase + "/" + GfxHead + ".txt") == "")
            {
                if (SharedResources.Mods!.Locate("animations/avatar/" + GfxBase + "/head_short.txt") != "")
                {
                    GfxHeadOriginal = GfxHead;
                    GfxHead = "head_short";
                }
            }
        }
    }
}
