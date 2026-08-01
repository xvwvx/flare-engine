// <自动生成> 对应 C++ 源文件：PowerManager.h + PowerManager.cpp
using Stride.Core.Mathematics;

namespace FlareEngine
{
    public class PostEffect
    {
        public string Id = "";
        public float Magnitude;
        public int Duration;
        public float Chance = 100;
        public bool TargetSrc;
        public bool IsMultiplier;

        // cache so we don't need to look up EffectDef or call Effect::getTypeFromString() as often
        public int EffectType;
        public EffectDef? EffectPtr;

        public PostEffect()
        {
            Id = "";
            Magnitude = 0;
            Duration = 0;
            Chance = 100;
            TargetSrc = false;
            IsMultiplier = false;
            EffectType = 0;
            EffectPtr = null;
        }
    }

    public class PowerReplaceByEffect
    {
        public PowerID PowerId;
        public int Count;
        public string EffectId = "";

        public PowerReplaceByEffect()
        {
            PowerId = 0;
            Count = 0;
            EffectId = "";
        }
    }

    public class PowerRequiredItem
    {
        public ItemID Id;
        public int Quantity;
        public bool Equipped;

        public PowerRequiredItem()
        {
            Id = 0;
            Quantity = 0;
            Equipped = false;
        }
    }

    public class ChainPower
    {
        public const byte TypePre = 0;
        public const byte TypePost = 1;
        public const byte TypeWall = 2;
        public const byte TypeExpire = 3;

        public PowerID Id;
        public byte Type;
        public float Chance = 100;

        public ChainPower()
        {
            Id = 0;
            Type = 0;
            Chance = 100;
        }
    }

    /// <summary>
    /// Power 定义类，对应 C++ <c>class Power</c>。
    /// 包含技能的所有属性：伤害、消耗、冷却、效果等。
    /// </summary>
    public class Power
    {
        public class ResourceState
        {
            public int State;
            public float Value;

            public ResourceState()
            {
                State = ResourceStateIgnore;
                Value = 0;
            }
        }

        public const int ResourceStateAny = 0;
        public const int ResourceStateAll = 1;
        public const int ResourceStateAnyHpmp = 2;

        public const int ResourceStateIgnore = 0;
        public const int ResourceStatePercent = 1;
        public const int ResourceStateNotPercent = 2;
        public const int ResourceStatePercentExact = 3;

        public const int TypeFixed = 0;
        public const int TypeMissile = 1;
        public const int TypeRepeater = 2;
        public const int TypeSpawn = 3;
        public const int TypeTransform = 4;
        public const int TypeEffect = 5;
        public const int TypeBlock = 6;

        public const int StateInstant = 1;
        public const int StateAttack = 2;

        public const int StartingPosSource = 0;
        public const int StartingPosTarget = 1;
        public const int StartingPosMelee = 2;
        public const int StartingPosMeleeUnlocked = 3;

        public const int TriggerBlock = 0;
        public const int TriggerHit = 1;
        public const int TriggerHalfdeath = 2;
        public const int TriggerJoincombat = 3;
        public const int TriggerDeath = 4;
        public const int TriggerActivePower = 5;

        public const int SpawnLimitModeFixed = 0;
        public const int SpawnLimitModeStat = 1;
        public const int SpawnLimitModeUnlimited = 2;

        public const int StatModifierModeMultiply = 0;
        public const int StatModifierModeAdd = 1;
        public const int StatModifierModeAbsolute = 2;

        public const int SourceTypeHero = 0;
        public const int SourceTypeNeutral = 1;
        public const int SourceTypeEnemy = 2;
        public const int SourceTypeAlly = 3;

        public const int ScriptTriggerCast = 0;
        public const int ScriptTriggerHit = 1;
        public const int ScriptTriggerWall = 2;

        public const int HoldOnActiveFrame = 0;
        public const int HoldOnFrame = 1;

        public bool PreventInterrupt;
        public bool Face;
        public bool Beacon;
        public bool Passive;
        public bool MetaPower;
        public bool MetaPowerProvidesTooltip = true;
        public bool NoActionbar;
        public bool Sacrifice;
        public bool RequiresLos;
        public bool RequiresLosDefault = true;
        public bool RequiresEmptyTarget;
        public bool Consumable;
        public bool RequiresTargeting;
        public bool SfxHitEnable;
        public bool Directional;
        public bool AimAssist;
        public bool OnFloor;
        public bool CompleteAnimation;
        public bool UseHazard;
        public bool NoAttack;
        public bool NoAggro;
        public bool RelativePos;
        public bool Multitarget;
        public bool Multihit;
        public bool ExpireWithCaster;
        public bool IgnoreZeroDamage;
        public bool LockTargetToDirection;
        public bool TargetParty;
        public bool TraitArmorPenetration;
        public bool TraitAvoidanceIgnore;
        public bool ManualUntransform;
        public bool KeepEquipment;
        public bool UntransformOnHit;
        public bool Buff;
        public bool BuffTeleport;
        public bool BuffParty;
        public bool WallReflect;
        public bool TargetMovementNormal = true;
        public bool TargetMovementFlying = true;
        public bool TargetMovementIntangible = true;
        public bool WallsBlockAoe;
        public bool RequiresCorpse;
        public bool RemoveCorpse;
        public bool PostHazardsSkipTarget;
        public bool CanTriggerPassives;
        public bool PassiveEffectsPersist;
        public bool SpawnRequiresUnlockedPower = true;

        public byte SpawnLimitMode;
        public byte StateHoldMode;

        public ushort VisualRandom;
        public ushort VisualOption;
        public ushort StateHoldFrame;

        public int Type = -1;
        public int Icon = -1;
        public int NewState = -1;
        public int StateDuration;
        public int SourceType = -1;
        public int Count = 1;
        public int PassiveTrigger = -1;
        public int RequiresSpawns;
        public int Cooldown;
        public int RequiresHpmpStateMode = ResourceStateAny;
        public int RequiresResourceStatStateMode = ResourceStateAll;
        public int SfxIndex = -1;
        public int Lifespan;
        public int StartingPos = StartingPosSource;
        public int MovementType = MapCollision.MoveFlying;
        public int ModAccuracyMode = -1;
        public int ModCritMode = -1;
        public int ModDamageMode = -1;
        public int Delay;
        public int TransformDuration;
        public int TargetNeighbor;
        public int ScriptTrigger = -1;

        public float RequiresMp;
        public float RequiresHp;
        public float Speed;
        public float ChargeSpeed;
        public float AttackSpeed = 100.0f;
        public float Radius;
        public float TargetRange;
        public float CombatRange;
        public float ModAccuracyValue = 100;
        public float ModCritValue = 100;
        public float ModDamageValueMin = 100;
        public float ModDamageValueMax;
        public float HpSteal;
        public float MpSteal;
        public float MissileAngle;
        public float AngleVariance;
        public float SpeedVariance;
        public float SpawnLimitCount = 1;
        public float SpawnLimitRatio = 1;
        public float TraitCritsImpaired;
        public float TargetNearest;

        public int BaseDamage;
        public int ConvertedDamage;
        public int SpawnLimitStat;

        public SoundID SfxHit;
        public PowerID BuffPartyPowerId;

        public ResourceState RequiresHpState = new ResourceState();
        public ResourceState RequiresMpState = new ResourceState();

        public SpawnLevel SpawnLevel = new SpawnLevel();

        public string Name = "";
        public string Description = "";
        public string AttackAnim = "";
        public string AnimationName = "";
        public string SpawnType = "";
        public string Script = "";

        public List<float> RequiresResourceStat = new List<float>();
        public List<PowerRequiredItem> RequiredItems = new List<PowerRequiredItem>();
        public List<ResourceState> RequiresResourceStatState = new List<ResourceState>();
        public List<string> TargetCategories = new List<string>();
        public List<float> ResourceSteal = new List<float>();
        public List<PostEffect> PostEffects = new List<PostEffect>();
        public List<ChainPower> ChainPowers = new List<ChainPower>();
        public List<(string Id, int Count)> RemoveEffects = new List<(string Id, int Count)>();
        public List<PowerReplaceByEffect> ReplaceByEffect = new List<PowerReplaceByEffect>();
        public List<int> DisableEquipSlots = new List<int>();
        public List<PowerID> DispelPowerIds = new List<PowerID>();
        public HashSet<int> RequiresFlags = new HashSet<int>();

        public Power()
        {
            PreventInterrupt = false;
            Face = false;
            Beacon = false;
            Passive = false;
            MetaPower = false;
            MetaPowerProvidesTooltip = true;
            NoActionbar = false;
            Sacrifice = false;
            RequiresLos = false;
            RequiresLosDefault = true;
            RequiresEmptyTarget = false;
            Consumable = false;
            RequiresTargeting = false;
            SfxHitEnable = false;
            Directional = false;
            AimAssist = false;
            OnFloor = false;
            CompleteAnimation = false;
            UseHazard = false;
            NoAttack = false;
            NoAggro = false;
            RelativePos = false;
            Multitarget = false;
            Multihit = false;
            ExpireWithCaster = false;
            IgnoreZeroDamage = false;
            LockTargetToDirection = false;
            TargetParty = false;
            TraitArmorPenetration = false;
            TraitAvoidanceIgnore = false;
            ManualUntransform = false;
            KeepEquipment = false;
            UntransformOnHit = false;
            Buff = false;
            BuffTeleport = false;
            BuffParty = false;
            WallReflect = false;
            TargetMovementNormal = true;
            TargetMovementFlying = true;
            TargetMovementIntangible = true;
            WallsBlockAoe = false;
            RequiresCorpse = false;
            RemoveCorpse = false;
            PostHazardsSkipTarget = false;
            CanTriggerPassives = false;
            PassiveEffectsPersist = false;
            SpawnRequiresUnlockedPower = true;

            SpawnLimitMode = SpawnLimitModeUnlimited;
            StateHoldMode = HoldOnActiveFrame;

            VisualRandom = 0;
            VisualOption = 0;
            StateHoldFrame = 0;

            Type = -1;
            Icon = -1;
            NewState = -1;
            StateDuration = 0;
            SourceType = -1;
            Count = 1;
            PassiveTrigger = -1;
            RequiresSpawns = 0;
            Cooldown = 0;
            RequiresHpmpStateMode = ResourceStateAny;
            RequiresResourceStatStateMode = ResourceStateAll;
            SfxIndex = -1;
            Lifespan = 0;
            StartingPos = StartingPosSource;
            MovementType = MapCollision.MoveFlying;
            ModAccuracyMode = -1;
            ModCritMode = -1;
            ModDamageMode = -1;
            Delay = 0;
            TransformDuration = 0;
            TargetNeighbor = 0;
            ScriptTrigger = -1;

            RequiresMp = 0;
            RequiresHp = 0;
            Speed = 0;
            ChargeSpeed = 0.0f;
            AttackSpeed = 100.0f;
            Radius = 0;
            TargetRange = 0;
            CombatRange = 0;
            ModAccuracyValue = 100;
            ModCritValue = 100;
            ModDamageValueMin = 100;
            ModDamageValueMax = 0;
            HpSteal = 0;
            MpSteal = 0;
            MissileAngle = 0;
            AngleVariance = 0;
            SpeedVariance = 0;
            SpawnLimitCount = 1;
            SpawnLimitRatio = 1;
            TraitCritsImpaired = 0;
            TargetNearest = 0;

            BaseDamage = SharedResources.Eset != null ? SharedResources.Eset.DamageTypes.Types.Count : 0;
            ConvertedDamage = SharedResources.Eset != null ? SharedResources.Eset.DamageTypes.Types.Count : 0;
            SpawnLimitStat = 0;

            SfxHit = 0;
            BuffPartyPowerId = 0;

            RequiresHpState = new ResourceState();
            RequiresMpState = new ResourceState();

            SpawnLevel = new SpawnLevel();

            Name = "";
            Description = "";
            AttackAnim = "";
            AnimationName = "";
            SpawnType = "";
            Script = "";

            int resourceStatCount = SharedResources.Eset != null ? SharedResources.Eset.ResourceStats.Stats.Count : 0;
            RequiresResourceStat = new List<float>(new float[resourceStatCount]);
            RequiresResourceStatState = new List<ResourceState>(resourceStatCount);
            for (int i = 0; i < resourceStatCount; ++i)
                RequiresResourceStatState.Add(new ResourceState());
            ResourceSteal = new List<float>(new float[resourceStatCount]);
        }
    }

    /// <summary>
    /// PowerManager 技能管理器，对应 C++ <c>class PowerManager</c>。
    /// 负责技能的加载、激活、碰撞检测与生命周期管理。
    /// </summary>
    public class PowerManager : IDisposable
    {
        private MapCollision? _collider;

        private List<Animation?> _powerAnimations = new List<Animation?>();
        private List<Animation?> _effectAnimations = new List<Animation?>();

        public const bool AllowZeroId = true;

        public List<EffectDef> Effects = new List<EffectDef>();
        public List<Power?> Powers = new List<Power?>();

        public Queue<Hazard> Hazards = new Queue<Hazard>();
        public Queue<MapEnemy> MapEnemies = new Queue<MapEnemy>();

        public List<SoundID> Sfx = new List<SoundID>();

        public List<ItemID> UsedItems = new List<ItemID>();
        public List<ItemID> UsedEquippedItems = new List<ItemID>();

        public PowerManager()
        {
            _collider = null;
            UsedItems = new List<ItemID>();
            UsedEquippedItems = new List<ItemID>();
            LoadEffects();
            LoadPowers();
        }

        public void Dispose()
        {
            Utils.LogInfo("Cleaning up: PowerManager");

            for (int i = 0; i < Powers.Count; ++i)
            {
                if (Powers[i] == null)
                    continue;

                if (!string.IsNullOrEmpty(Powers[i]!.AnimationName))
                {
                    SharedResources.Anim!.DecreaseCount(Powers[i]!.AnimationName);
                }

                Powers[i] = null;
                _powerAnimations[i]?.Dispose();
                _powerAnimations[i] = null;
            }

            for (int i = 0; i < Effects.Count; ++i)
            {
                if (string.IsNullOrEmpty(Effects[i].AnimationName))
                    continue;

                SharedResources.Anim!.DecreaseCount(Effects[i].AnimationName);

                if (_effectAnimations[i] != null)
                {
                    _effectAnimations[i]!.Dispose();
                    _effectAnimations[i] = null;
                }
            }

            for (int i = 0; i < Sfx.Count; i++)
            {
                SharedResources.Snd!.Unload(Sfx[i]);
            }
            Sfx.Clear();

            while (Hazards.Count > 0)
            {
                Hazards.Peek().Dispose();
                Hazards.Dequeue();
            }

            GC.SuppressFinalize(this);
        }

        public bool IsValid(PowerID powerId)
        {
            return powerId > 0 && powerId < Powers.Count && Powers[powerId] != null;
        }

        public void HandleNewMap(MapCollision collider)
        {
            _collider = collider;
        }

        private void LoadEffects()
        {
            using FileParser infile = new FileParser();

            // @CLASS PowerManager: Effects|Description of powers/effects.txt
            if (!infile.Open("powers/effects.txt", FileParser.ModFile, FileParser.ErrorNormal))
                return;
            
            var eset = SharedResources.Eset!;

            EffectDef temp = new EffectDef();
            EffectDef current = temp;

            while (infile.Next())
            {
                if (infile.NewSection)
                {
                    if (infile.Section == "effect")
                    {
                        temp = new EffectDef();
                        current = temp;
                    }
                }

                if (infile.Section != "effect")
                    continue;

                if (infile.Key != "id" && string.IsNullOrEmpty(current.Id))
                {
                    infile.Error("PowerManager: Expected 'id', but found '%s'.", infile.Key);
                }

                if (infile.Key == "id")
                {
                    if (!string.IsNullOrEmpty(infile.Val))
                    {
                        bool foundId = false;
                        for (int i = 0; i < Effects.Count; ++i)
                        {
                            if (Effects[i].Id == infile.Val)
                            {
                                current = Effects[i];
                                foundId = true;
                            }
                        }

                        if (!foundId)
                        {
                            Effects.Add(temp);
                            _effectAnimations.Add(null);
                            current = Effects[^1];
                            current.Id = infile.Val;

                            if (global::FlareEngine.Effect.GetTypeFromString(current.Id, false) != global::FlareEngine.Effect.None)
                            {
                                infile.Error("PowerManager: Warning! Effect ID '%s' collides with built-in type.", current.Id);
                            }
                        }
                    }
                }
                else if (infile.Key == "type")
                {
                    current.Type = global::FlareEngine.Effect.GetTypeFromString(infile.Val);
                    current.IsImmunityType = global::FlareEngine.Effect.IsImmunityTypeString(infile.Val);
                    if (current.IsImmunityType)
                    {
                        infile.Error("PowerManager: '%s' is deprecated. Replace with a corresponding 'resist' effect.", infile.Val);
                    }
                }
                else if (infile.Key == "name")
                {
                    current.Name = infile.Val;
                }
                else if (infile.Key == "icon")
                {
                    current.Icon = Parse.ToInt(infile.Val);
                }
                else if (infile.Key == "animation")
                {
                    current.AnimationName = infile.Val;
                }
                else if (infile.Key == "can_stack")
                {
                    current.CanStack = Parse.ToBool(infile.Val);
                }
                else if (infile.Key == "max_stacks")
                {
                    current.MaxStacks = Parse.ToInt(infile.Val);
                }
                else if (infile.Key == "group_stack")
                {
                    current.GroupStack = Parse.ToBool(infile.Val);
                }
                else if (infile.Key == "render_above")
                {
                    current.RenderAbove = Parse.ToBool(infile.Val);
                }
                else if (infile.Key == "color_mod")
                {
                    current.ColorMod = Parse.ToRGB(infile.Val);
                }
                else if (infile.Key == "alpha_mod")
                {
                    current.AlphaMod = (byte)Parse.ToInt(infile.Val);
                }
                else if (infile.Key == "attack_speed_anim")
                {
                    current.AttackSpeedAnim = infile.Val;
                }
                else if (infile.Key == "ignore_resist")
                {
                    current.IgnoreResist = Parse.ToBool(infile.Val);
                }
                else if (infile.Key == "damage_type")
                {
                    current.DamageIsTyped = false;
                    for (int i = 0; i < eset.DamageTypes.Count; i++)
                    {
                        if (eset.DamageTypes.Types[i].Id == infile.Val)
                        {
                            current.DamageType = i;
                            current.DamageIsTyped = true;
                            break;
                        }

                        if (!current.DamageIsTyped)
                        {
                            infile.Error("PowerManager: '%s' is not a known damage type.", infile.Key);
                        }
                    }
                }
                else
                {
                    infile.Error("PowerManager: '%s' is not a valid key.", infile.Key);
                }
            }

            for (int i = 0; i < Effects.Count; ++i)
            {
                if (!string.IsNullOrEmpty(Effects[i].AnimationName))
                {
                    SharedResources.Anim!.IncreaseCount(Effects[i].AnimationName);
                    _effectAnimations[i] = SharedResources.Anim!.GetAnimationSet(Effects[i].AnimationName)!.GetAnimation("");
                }
            }
        }

        private void LoadPowers()
        {
            using FileParser infile = new FileParser();

            if (!infile.Open("powers/powers.txt", FileParser.ModFile, FileParser.ErrorNormal))
                return;

            bool clearPostEffects = false;

            PowerID inputId = 0;
            Power? power = null;
            bool idLine = false;

            while (infile.Next())
            {
                if (infile.Key == "id")
                {
                    idLine = true;
                    inputId = Parse.ToPowerID(infile.Val);
                    if (inputId < Powers.Count && Powers[inputId] != null)
                    {
                        clearPostEffects = true;
                    }
                    else
                    {
                        int newSize = Math.Max(inputId + 1, Powers.Count);
                        while (Powers.Count < newSize)
                        {
                            Powers.Add(null);
                            _powerAnimations.Add(null);
                        }
                        Powers[inputId] = new Power();
                    }
                    power = Powers[inputId];

                    continue;
                }
                else idLine = false;

                if (inputId < 1)
                {
                    if (idLine) infile.Error("PowerManager: Power index out of bounds 1-%d, skipping power.", int.MaxValue);
                }
                if (idLine)
                    continue;

                string val = infile.Val;

                if (infile.Key == "type")
                {
                    if (val == "fixed") power!.Type = Power.TypeFixed;
                    else if (val == "missile") power!.Type = Power.TypeMissile;
                    else if (val == "repeater") power!.Type = Power.TypeRepeater;
                    else if (val == "spawn") power!.Type = Power.TypeSpawn;
                    else if (val == "transform") power!.Type = Power.TypeTransform;
                    else if (val == "block") power!.Type = Power.TypeBlock;
                    else infile.Error("PowerManager: Unknown type '%s'", val);
                }
                else if (infile.Key == "name")
                {
                    power!.Name = SharedResources.Msg!.Get(val);
                }
                else if (infile.Key == "description")
                {
                    power!.Description = SharedResources.Msg!.Get(val);
                }
                else if (infile.Key == "icon")
                {
                    power!.Icon = Parse.ToInt(val);
                }
                else if (infile.Key == "new_state")
                {
                    if (val == "instant") power!.NewState = Power.StateInstant;
                    else
                    {
                        power!.NewState = Power.StateAttack;
                        power!.AttackAnim = val;
                    }
                }
                else if (infile.Key == "state_duration")
                {
                    power!.StateDuration = Parse.ToDuration(val);
                }
                else if (infile.Key == "state_hold_mode")
                {
                    string mode = Parse.PopFirstString(ref val);
                    if (mode == "active_frame")
                    {
                        power!.StateHoldMode = Power.HoldOnActiveFrame;
                    }
                    else if (mode == "frame")
                    {
                        power!.StateHoldMode = Power.HoldOnFrame;
                        power!.StateHoldFrame = (ushort)Parse.PopFirstInt(ref val);
                    }
                    else
                    {
                        infile.Error("PowerManager: '%s' is not a valid state hold mode.", mode);
                    }
                }
                else if (infile.Key == "prevent_interrupt")
                {
                    power!.PreventInterrupt = Parse.ToBool(val);
                }
                else if (infile.Key == "face")
                {
                    power!.Face = Parse.ToBool(val);
                }
                else if (infile.Key == "source_type")
                {
                    if (val == "hero") power!.SourceType = Power.SourceTypeHero;
                    else if (val == "neutral") power!.SourceType = Power.SourceTypeNeutral;
                    else if (val == "enemy") power!.SourceType = Power.SourceTypeEnemy;
                    else infile.Error("PowerManager: Unknown source_type '%s'", val);
                }
                else if (infile.Key == "beacon")
                {
                    power!.Beacon = Parse.ToBool(val);
                }
                else if (infile.Key == "count")
                {
                    power!.Count = Parse.ToInt(val);
                }
                else if (infile.Key == "passive")
                {
                    power!.Passive = Parse.ToBool(val);
                }
                else if (infile.Key == "passive_trigger")
                {
                    if (val == "on_block") power!.PassiveTrigger = Power.TriggerBlock;
                    else if (val == "on_hit") power!.PassiveTrigger = Power.TriggerHit;
                    else if (val == "on_halfdeath") power!.PassiveTrigger = Power.TriggerHalfdeath;
                    else if (val == "on_joincombat") power!.PassiveTrigger = Power.TriggerJoincombat;
                    else if (val == "on_death") power!.PassiveTrigger = Power.TriggerDeath;
                    else if (val == "on_active_power") power!.PassiveTrigger = Power.TriggerActivePower;
                    else infile.Error("PowerManager: Unknown passive trigger '%s'", val);
                }
                else if (infile.Key == "meta_power")
                {
                    power!.MetaPower = Parse.ToBool(val);
                }
                else if (infile.Key == "meta_power_provides_tooltip")
                {
                    power!.MetaPowerProvidesTooltip = Parse.ToBool(val);
                }
                else if (infile.Key == "no_actionbar")
                {
                    power!.NoActionbar = Parse.ToBool(val);
                }
                else if (infile.Key == "requires_flags")
                {
                    power!.RequiresFlags.Clear();
                    string flag = Parse.PopFirstString(ref val);

                    while (flag != "")
                    {
                        power!.RequiresFlags.Add(SharedResources.Eset!.EquipFlags.GetIndex(flag));
                        flag = Parse.PopFirstString(ref val);
                    }
                }
                else if (infile.Key == "requires_mp")
                {
                    power!.RequiresMp = Parse.ToFloat(val);
                }
                else if (infile.Key == "requires_hp")
                {
                    power!.RequiresHp = Parse.ToFloat(val);
                }
                else if (infile.Key == "requires_resource_stat")
                {
                    string statId = Parse.PopFirstString(ref val);
                    float statVal = Parse.PopFirstFloat(ref val);

                    bool foundStatId = false;
                    for (int i = 0; i < power!.RequiresResourceStat.Count; ++i)
                    {
                        if (statId == SharedResources.Eset!.ResourceStats.Stats[i].Ids[EngineSettings.ResourceStatsSettings.StatBase])
                        {
                            power!.RequiresResourceStat[i] = statVal;
                            foundStatId = true;
                            break;
                        }
                    }

                    if (!foundStatId)
                    {
                        infile.Error("PowerManager: '%s' is not a valid resource stat.", statId);
                    }
                }
                else if (infile.Key == "sacrifice")
                {
                    power!.Sacrifice = Parse.ToBool(val);
                }
                else if (infile.Key == "requires_los")
                {
                    power!.RequiresLos = Parse.ToBool(val);
                    power!.RequiresLosDefault = false;
                }
                else if (infile.Key == "requires_empty_target")
                {
                    power!.RequiresEmptyTarget = Parse.ToBool(val);
                }
                else if (infile.Key == "requires_item")
                {
                    PowerRequiredItem pri = new PowerRequiredItem();
                    pri.Id = SharedGameResources.Items!.VerifyID(Parse.ToItemID(Parse.PopFirstString(ref val)), infile, !ItemManager.VerifyAllowZero, !ItemManager.VerifyAllocate);
                    if (pri.Id > 0)
                    {
                        pri.Quantity = Parse.ToInt(Parse.PopFirstString(ref val), 1);
                        pri.Equipped = false;
                        power!.RequiredItems.Add(pri);
                    }
                }
                else if (infile.Key == "requires_equipped_item")
                {
                    PowerRequiredItem pri = new PowerRequiredItem();
                    pri.Id = SharedGameResources.Items!.VerifyID(Parse.ToItemID(Parse.PopFirstString(ref val)), infile, !ItemManager.VerifyAllowZero, !ItemManager.VerifyAllocate);
                    if (pri.Id > 0)
                    {
                        pri.Quantity = Parse.PopFirstInt(ref val);
                        pri.Equipped = true;

                        if (pri.Quantity > 1)
                        {
                            infile.Error("PowerManager: Only 1 equipped item can be consumed at a time.");
                            pri.Quantity = Math.Min(pri.Quantity, 1);
                        }

                        power!.RequiredItems.Add(pri);
                    }
                }
                else if (infile.Key == "requires_targeting")
                {
                    power!.RequiresTargeting = Parse.ToBool(val);
                }
                else if (infile.Key == "requires_spawns")
                {
                    power!.RequiresSpawns = Parse.ToInt(val);
                }
                else if (infile.Key == "cooldown")
                {
                    power!.Cooldown = Parse.ToDuration(val);
                }
                else if (infile.Key == "requires_hpmp_state")
                {
                    string mode = Parse.PopFirstString(ref val);
                    string stateHp = Parse.PopFirstString(ref val);
                    string stateHpVal = Parse.PopFirstString(ref val);
                    string stateMp = Parse.PopFirstString(ref val);
                    string stateMpVal = Parse.PopFirstString(ref val);

                    power!.RequiresHpState.Value = Parse.ToFloat(stateHpVal);
                    power!.RequiresMpState.Value = Parse.ToFloat(stateMpVal);

                    if (stateHp == "percent")
                    {
                        power!.RequiresHpState.State = Power.ResourceStatePercent;
                    }
                    else if (stateHp == "not_percent")
                    {
                        power!.RequiresHpState.State = Power.ResourceStateNotPercent;
                    }
                    else if (stateHp == "percent_exact")
                    {
                        power!.RequiresHpState.State = Power.ResourceStatePercentExact;
                    }
                    else if (stateHp == "ignore" || stateHp == "")
                    {
                        power!.RequiresHpState.State = Power.ResourceStateIgnore;
                    }
                    else
                    {
                        infile.Error("PowerManager: '%s' is not a valid hp/mp state. Use 'percent', 'not_percent', or 'ignore'.", stateHp);
                    }

                    if (stateMp == "percent")
                    {
                        power!.RequiresMpState.State = Power.ResourceStatePercent;
                    }
                    else if (stateMp == "not_percent")
                    {
                        power!.RequiresMpState.State = Power.ResourceStateNotPercent;
                    }
                    else if (stateHp == "percent_exact")
                    {
                        power!.RequiresMpState.State = Power.ResourceStatePercentExact;
                    }
                    else if (stateMp == "ignore" || stateMp == "")
                    {
                        power!.RequiresMpState.State = Power.ResourceStateIgnore;
                    }
                    else
                    {
                        infile.Error("PowerManager: '%s' is not a valid hp/mp state. Use 'percent', 'not_percent', or 'ignore'.", stateMp);
                    }

                    if (mode == "any")
                    {
                        power!.RequiresHpmpStateMode = Power.ResourceStateAny;
                    }
                    else if (mode == "all")
                    {
                        power!.RequiresHpmpStateMode = Power.ResourceStateAll;
                    }
                    else if (mode == "hp")
                    {
                        infile.Error("PowerManager: 'hp' has been deprecated. Use 'all' or 'any'.");

                        power!.RequiresHpmpStateMode = Power.ResourceStateAll;
                        power!.RequiresMpState.State = Power.ResourceStateIgnore;
                    }
                    else if (mode == "mp")
                    {
                        infile.Error("PowerManager: 'mp' has been deprecated. Use 'any' or 'all'.");

                        power!.RequiresHpmpStateMode = Power.ResourceStateAll;
                        power!.RequiresMpState.State = power!.RequiresHpState.State;
                        power!.RequiresMpState.Value = power!.RequiresHpState.Value;
                        power!.RequiresHpState.State = Power.ResourceStateIgnore;
                    }
                    else
                    {
                        infile.Error("PowerManager: Please specify 'any' or 'all'.");
                    }
                }
                else if (infile.Key == "requires_resource_stat_state")
                {
                    string statId = Parse.PopFirstString(ref val);
                    string state = Parse.PopFirstString(ref val);
                    float value = Parse.PopFirstFloat(ref val);

                    for (int i = 0; i < power!.RequiresResourceStatState.Count; ++i)
                    {
                        if (statId == SharedResources.Eset!.ResourceStats.Stats[i].Ids[EngineSettings.ResourceStatsSettings.StatBase])
                        {
                            power!.RequiresResourceStatState[i].Value = value;

                            if (state == "percent")
                            {
                                power!.RequiresResourceStatState[i].State = Power.ResourceStatePercent;
                            }
                            else if (state == "not_percent")
                            {
                                power!.RequiresResourceStatState[i].State = Power.ResourceStateNotPercent;
                            }
                            else if (state == "percent_exact")
                            {
                                power!.RequiresResourceStatState[i].State = Power.ResourceStatePercentExact;
                            }
                            else if (state == "" || state == "ignore")
                            {
                                power!.RequiresResourceStatState[i].State = Power.ResourceStateIgnore;
                            }
                            else
                            {
                                infile.Error("PowerManager: '%s' is not a valid resource stat state. Use 'percent', 'not_percent', or 'ignore'.", state);
                            }

                            break;
                        }
                    }
                }
                else if (infile.Key == "requires_resource_stat_state_mode")
                {
                    string mode = Parse.PopFirstString(ref val);

                    if (mode == "all")
                    {
                        power!.RequiresResourceStatStateMode = Power.ResourceStateAll;
                    }
                    else if (mode == "any")
                    {
                        power!.RequiresResourceStatStateMode = Power.ResourceStateAny;
                    }
                    else if (mode == "any_hpmp")
                    {
                        power!.RequiresResourceStatStateMode = Power.ResourceStateAnyHpmp;
                    }
                    else
                    {
                        infile.Error("PowerManager: '%s' is not a valid mode. Possible modes include: 'all', 'any', or 'any_hpmp'.", mode);
                    }
                }
                else if (infile.Key == "animation")
                {
                    power!.AnimationName = val;
                }
                else if (infile.Key == "soundfx")
                {
                    power!.SfxIndex = LoadSFX(val);
                }
                else if (infile.Key == "soundfx_hit")
                {
                    int sfxId = LoadSFX(val);
                    if (sfxId != -1)
                    {
                        power!.SfxHit = Sfx[sfxId];
                        power!.SfxHitEnable = true;
                    }
                }
                else if (infile.Key == "directional")
                {
                    power!.Directional = Parse.ToBool(val);
                }
                else if (infile.Key == "visual_option")
                {
                    power!.VisualOption = (ushort)Parse.ToInt(val);
                }
                else if (infile.Key == "visual_random")
                {
                    power!.VisualRandom = (ushort)Parse.ToInt(val);
                }
                else if (infile.Key == "aim_assist")
                {
                    power!.AimAssist = Parse.ToBool(val);
                }
                else if (infile.Key == "speed")
                {
                    power!.Speed = Parse.ToFloat(val) / SharedResources.Settings!.MaxFramesPerSec;
                }
                else if (infile.Key == "lifespan")
                {
                    power!.Lifespan = Parse.ToDuration(val);
                }
                else if (infile.Key == "floor")
                {
                    power!.OnFloor = Parse.ToBool(val);
                }
                else if (infile.Key == "complete_animation")
                {
                    power!.CompleteAnimation = Parse.ToBool(val);
                }
                else if (infile.Key == "charge_speed")
                {
                    power!.ChargeSpeed = Parse.ToFloat(val) / SharedResources.Settings!.MaxFramesPerSec;
                }
                else if (infile.Key == "attack_speed")
                {
                    power!.AttackSpeed = Parse.ToFloat(val);
                    if (power!.AttackSpeed < 100)
                    {
                        Utils.LogInfo("PowerManager: Attack speeds less than 100 are unsupported.");
                        power!.AttackSpeed = 100;
                    }
                }
                else if (infile.Key == "use_hazard")
                {
                    power!.UseHazard = Parse.ToBool(val);
                }
                else if (infile.Key == "no_attack")
                {
                    power!.NoAttack = Parse.ToBool(val);
                }
                else if (infile.Key == "no_aggro")
                {
                    power!.NoAggro = Parse.ToBool(val);
                }
                else if (infile.Key == "radius")
                {
                    power!.Radius = Parse.ToFloat(val);
                }
                else if (infile.Key == "base_damage")
                {
                    for (int i = 0; i < SharedResources.Eset!.DamageTypes.Types.Count; ++i)
                    {
                        if (val == SharedResources.Eset.DamageTypes.Types[i].Id)
                        {
                            power!.BaseDamage = i;
                            break;
                        }
                    }

                    if (power!.BaseDamage == SharedResources.Eset.DamageTypes.Types.Count)
                    {
                        infile.Error("PowerManager: Unknown base_damage '%s'", val);
                    }
                }
                else if (infile.Key == "starting_pos")
                {
                    if (val == "source") power!.StartingPos = Power.StartingPosSource;
                    else if (val == "target") power!.StartingPos = Power.StartingPosTarget;
                    else if (val == "melee") power!.StartingPos = Power.StartingPosMelee;
                    else if (val == "melee_unlocked") power!.StartingPos = Power.StartingPosMeleeUnlocked;
                    else infile.Error("PowerManager: Unknown starting_pos '%s'", val);
                }
                else if (infile.Key == "relative_pos")
                {
                    power!.RelativePos = Parse.ToBool(val);
                }
                else if (infile.Key == "multitarget")
                {
                    power!.Multitarget = Parse.ToBool(val);
                }
                else if (infile.Key == "multihit")
                {
                    power!.Multihit = Parse.ToBool(val);
                }
                else if (infile.Key == "expire_with_caster")
                {
                    power!.ExpireWithCaster = Parse.ToBool(val);
                }
                else if (infile.Key == "ignore_zero_damage")
                {
                    power!.IgnoreZeroDamage = Parse.ToBool(val);
                }
                else if (infile.Key == "lock_target_to_direction")
                {
                    power!.LockTargetToDirection = Parse.ToBool(val);
                }
                else if (infile.Key == "movement_type")
                {
                    if (val == "ground") power!.MovementType = MapCollision.MoveNormal;
                    else if (val == "flying") power!.MovementType = MapCollision.MoveFlying;
                    else if (val == "intangible") power!.MovementType = MapCollision.MoveIntangible;
                    else infile.Error("PowerManager: Unknown movement_type '%s'", val);
                }
                else if (infile.Key == "trait_armor_penetration")
                {
                    power!.TraitArmorPenetration = Parse.ToBool(val);
                }
                else if (infile.Key == "trait_avoidance_ignore")
                {
                    power!.TraitAvoidanceIgnore = Parse.ToBool(val);
                }
                else if (infile.Key == "trait_crits_impaired")
                {
                    power!.TraitCritsImpaired = Parse.ToFloat(val);
                }
                else if (infile.Key == "trait_elemental")
                {
                    for (int i = 0; i < SharedResources.Eset!.DamageTypes.Types.Count; ++i)
                    {
                        if (val == SharedResources.Eset.DamageTypes.Types[i].Id)
                        {
                            power!.ConvertedDamage = i;
                            break;
                        }
                    }
                }
                else if (infile.Key == "target_range")
                {
                    power!.TargetRange = Parse.PopFirstFloat(ref val);
                }
                else if (infile.Key == "hp_steal")
                {
                    power!.HpSteal = Parse.ToFloat(val);
                }
                else if (infile.Key == "mp_steal")
                {
                    power!.MpSteal = Parse.ToFloat(val);
                }
                else if (infile.Key == "resource_steal")
                {
                    string statId = Parse.PopFirstString(ref val);
                    float statValue = Parse.PopFirstFloat(ref val);

                    for (int i = 0; i < power!.ResourceSteal.Count; ++i)
                    {
                        if (statId == SharedResources.Eset!.ResourceStats.Stats[i].Ids[EngineSettings.ResourceStatsSettings.StatBase])
                        {
                            power!.ResourceSteal[i] = statValue;
                            break;
                        }
                    }
                }
                else if (infile.Key == "missile_angle")
                {
                    power!.MissileAngle = Parse.ToFloat(val);
                }
                else if (infile.Key == "angle_variance")
                {
                    power!.AngleVariance = Parse.ToFloat(val);
                }
                else if (infile.Key == "speed_variance")
                {
                    power!.SpeedVariance = Parse.ToFloat(val);
                }
                else if (infile.Key == "delay")
                {
                    power!.Delay = Parse.ToDuration(val);
                }
                else if (infile.Key == "transform_duration")
                {
                    power!.TransformDuration = Parse.ToDuration(val);
                }
                else if (infile.Key == "manual_untransform")
                {
                    power!.ManualUntransform = Parse.ToBool(val);
                }
                else if (infile.Key == "keep_equipment")
                {
                    power!.KeepEquipment = Parse.ToBool(val);
                }
                else if (infile.Key == "untransform_on_hit")
                {
                    power!.UntransformOnHit = Parse.ToBool(val);
                }
                else if (infile.Key == "buff")
                {
                    power!.Buff = Parse.ToBool(val);
                }
                else if (infile.Key == "buff_teleport")
                {
                    power!.BuffTeleport = Parse.ToBool(val);
                }
                else if (infile.Key == "buff_party")
                {
                    power!.BuffParty = Parse.ToBool(val);
                }
                else if (infile.Key == "buff_party_power_id")
                {
                    power!.BuffPartyPowerId = Parse.ToPowerID(val);
                }
                else if (infile.Key == "post_effect" || infile.Key == "post_effect_src")
                {
                    if (clearPostEffects)
                    {
                        power!.PostEffects.Clear();
                        clearPostEffects = false;
                    }
                    PostEffect pe = new PostEffect();
                    pe.Id = Parse.PopFirstString(ref val);
                    if (!IsValidEffect(pe.Id))
                    {
                        infile.Error("PowerManager: Unknown effect '%s'", pe.Id);
                    }
                    else
                    {
                        if (infile.Key == "post_effect_src")
                            pe.TargetSrc = true;

                        string magnitudeStr = Parse.PopFirstString(ref val);
                        if (!string.IsNullOrEmpty(magnitudeStr))
                        {
                            if (magnitudeStr[magnitudeStr.Length - 1] == '%')
                            {
                                pe.IsMultiplier = true;
                                magnitudeStr = magnitudeStr.Substring(0, magnitudeStr.Length - 1);
                                pe.Magnitude = Parse.ToFloat(magnitudeStr) / 100;
                            }
                            else
                            {
                                pe.Magnitude = Parse.ToFloat(magnitudeStr);
                            }
                        }

                        if (pe.Id == "hp_percent")
                        {
                            infile.Error("PowerManager: 'hp_percent' is deprecated. Converting to hp.");
                            pe.Id = "hp";
                            pe.IsMultiplier = true;
                            pe.Magnitude = (pe.Magnitude + 100) / 100;
                        }
                        else if (pe.Id == "mp_percent")
                        {
                            infile.Error("PowerManager: 'mp_percent' is deprecated. Converting to mp.");
                            pe.Id = "mp";
                            pe.IsMultiplier = true;
                            pe.Magnitude = (pe.Magnitude + 100) / 100;
                        }

                        pe.Duration = Parse.ToDuration(Parse.PopFirstString(ref val));
                        string chance = Parse.PopFirstString(ref val);
                        if (!string.IsNullOrEmpty(chance))
                        {
                            pe.Chance = Parse.ToFloat(chance);
                        }

                        bool isImmunityType = false;
                        pe.EffectPtr = GetEffectDef(pe.Id);

                        if (pe.EffectPtr != null)
                        {
                            pe.EffectType = pe.EffectPtr.Type;
                            isImmunityType = pe.EffectPtr.IsImmunityType;
                        }
                        else
                        {
                            pe.EffectType = global::FlareEngine.Effect.GetTypeFromString(pe.Id);
                            isImmunityType = global::FlareEngine.Effect.IsImmunityTypeString(pe.Id);
                        }

                        if (isImmunityType && (pe.EffectType == global::FlareEngine.Effect.ResistAll || global::FlareEngine.Effect.TypeIsEffectResist(pe.EffectType)))
                        {
                            infile.Error("PowerManager: Post effect '%s' matches a deprecated type. Converting to a resistance with 100 magnitude.", pe.Id);
                            pe.Magnitude = 100;
                        }

                        power!.PostEffects.Add(pe);
                    }
                }
                else if (infile.Key == "pre_power")
                {
                    ChainPower chainPower = new ChainPower();
                    chainPower.Type = ChainPower.TypePre;
                    chainPower.Id = Parse.ToPowerID(Parse.PopFirstString(ref val));
                    string chance = Parse.PopFirstString(ref val);
                    if (!string.IsNullOrEmpty(chance))
                    {
                        chainPower.Chance = Parse.ToFloat(chance);
                    }
                    if (chainPower.Id > 0)
                    {
                        power!.ChainPowers.Add(chainPower);
                    }
                }
                else if (infile.Key == "post_power")
                {
                    ChainPower chainPower = new ChainPower();
                    chainPower.Type = ChainPower.TypePost;
                    chainPower.Id = Parse.ToPowerID(Parse.PopFirstString(ref val));
                    string chance = Parse.PopFirstString(ref val);
                    if (!string.IsNullOrEmpty(chance))
                    {
                        chainPower.Chance = Parse.ToFloat(chance);
                    }
                    if (chainPower.Id > 0)
                    {
                        power!.ChainPowers.Add(chainPower);
                    }
                }
                else if (infile.Key == "wall_power")
                {
                    ChainPower chainPower = new ChainPower();
                    chainPower.Type = ChainPower.TypeWall;
                    chainPower.Id = Parse.ToPowerID(Parse.PopFirstString(ref val));
                    string chance = Parse.PopFirstString(ref val);
                    if (!string.IsNullOrEmpty(chance))
                    {
                        chainPower.Chance = Parse.ToFloat(chance);
                    }
                    if (chainPower.Id > 0)
                    {
                        power!.ChainPowers.Add(chainPower);
                    }
                }
                else if (infile.Key == "expire_power")
                {
                    ChainPower chainPower = new ChainPower();
                    chainPower.Type = ChainPower.TypeExpire;
                    chainPower.Id = Parse.ToPowerID(Parse.PopFirstString(ref val));
                    string chance = Parse.PopFirstString(ref val);
                    if (!string.IsNullOrEmpty(chance))
                    {
                        chainPower.Chance = Parse.ToFloat(chance);
                    }
                    if (chainPower.Id > 0)
                    {
                        power!.ChainPowers.Add(chainPower);
                    }
                }
                else if (infile.Key == "wall_reflect")
                {
                    power!.WallReflect = Parse.ToBool(val);
                }
                else if (infile.Key == "spawn_type")
                {
                    power!.SpawnType = val;
                }
                else if (infile.Key == "spawn_limit")
                {
                    string mode = Parse.PopFirstString(ref val);
                    if (mode == "fixed") power!.SpawnLimitMode = Power.SpawnLimitModeFixed;
                    else if (mode == "stat") power!.SpawnLimitMode = Power.SpawnLimitModeStat;
                    else if (mode == "unlimited") power!.SpawnLimitMode = Power.SpawnLimitModeUnlimited;
                    else infile.Error("PowerManager: Unknown spawn_limit_mode '%s'", mode);

                    if (power!.SpawnLimitMode != Power.SpawnLimitModeUnlimited)
                    {
                        power!.SpawnLimitCount = Parse.PopFirstInt(ref val);

                        if (power!.SpawnLimitMode == Power.SpawnLimitModeStat)
                        {
                            power!.SpawnLimitRatio = Parse.PopFirstFloat(ref val);

                            string stat = Parse.PopFirstString(ref val);
                            int primStatIndex = SharedResources.Eset!.PrimaryStats.GetIndexByID(stat);

                            if (primStatIndex != SharedResources.Eset.PrimaryStats.Stats.Count)
                            {
                                power!.SpawnLimitStat = primStatIndex;
                            }
                            else
                            {
                                infile.Error("PowerManager: '%s' is not a valid primary stat.", stat);
                            }
                        }
                    }
                }
                else if (infile.Key == "spawn_level")
                {
                    power!.SpawnLevel.Parse(infile);
                }
                else if (infile.Key == "spawn_requires_unlocked_power")
                {
                    power!.SpawnRequiresUnlockedPower = Parse.ToBool(val);
                }
                else if (infile.Key == "target_neighbor")
                {
                    power!.TargetNeighbor = Parse.ToInt(val);
                }
                else if (infile.Key == "target_party")
                {
                    power!.TargetParty = Parse.ToBool(val);
                }
                else if (infile.Key == "target_categories")
                {
                    power!.TargetCategories.Clear();
                    string cat;
                    while ((cat = Parse.PopFirstString(ref val)) != "")
                    {
                        power!.TargetCategories.Add(cat);
                    }
                }
                else if (infile.Key == "modifier_accuracy")
                {
                    string mode = Parse.PopFirstString(ref val);
                    if (mode == "multiply") power!.ModAccuracyMode = Power.StatModifierModeMultiply;
                    else if (mode == "add") power!.ModAccuracyMode = Power.StatModifierModeAdd;
                    else if (mode == "absolute") power!.ModAccuracyMode = Power.StatModifierModeAbsolute;
                    else infile.Error("PowerManager: Unknown stat_modifier_mode '%s'", mode);

                    power!.ModAccuracyValue = Parse.PopFirstFloat(ref val);
                }
                else if (infile.Key == "modifier_damage")
                {
                    string mode = Parse.PopFirstString(ref val);
                    if (mode == "multiply") power!.ModDamageMode = Power.StatModifierModeMultiply;
                    else if (mode == "add") power!.ModDamageMode = Power.StatModifierModeAdd;
                    else if (mode == "absolute") power!.ModDamageMode = Power.StatModifierModeAbsolute;
                    else infile.Error("PowerManager: Unknown stat_modifier_mode '%s'", mode);

                    power!.ModDamageValueMin = Parse.PopFirstFloat(ref val);
                    power!.ModDamageValueMax = Parse.PopFirstFloat(ref val);
                }
                else if (infile.Key == "modifier_critical")
                {
                    string mode = Parse.PopFirstString(ref val);
                    if (mode == "multiply") power!.ModCritMode = Power.StatModifierModeMultiply;
                    else if (mode == "add") power!.ModCritMode = Power.StatModifierModeAdd;
                    else if (mode == "absolute") power!.ModCritMode = Power.StatModifierModeAbsolute;
                    else infile.Error("PowerManager: Unknown stat_modifier_mode '%s'", mode);

                    power!.ModCritValue = Parse.PopFirstFloat(ref val);
                }
                else if (infile.Key == "target_movement_normal")
                {
                    power!.TargetMovementNormal = Parse.ToBool(val);
                }
                else if (infile.Key == "target_movement_flying")
                {
                    power!.TargetMovementFlying = Parse.ToBool(val);
                }
                else if (infile.Key == "target_movement_intangible")
                {
                    power!.TargetMovementIntangible = Parse.ToBool(val);
                }
                else if (infile.Key == "walls_block_aoe")
                {
                    power!.WallsBlockAoe = Parse.ToBool(val);
                }
                else if (infile.Key == "script")
                {
                    string trigger = Parse.PopFirstString(ref val);
                    if (trigger == "on_cast") power!.ScriptTrigger = Power.ScriptTriggerCast;
                    else if (trigger == "on_hit") power!.ScriptTrigger = Power.ScriptTriggerHit;
                    else if (trigger == "on_wall") power!.ScriptTrigger = Power.ScriptTriggerWall;
                    else infile.Error("PowerManager: Unknown script trigger '%s'", trigger);

                    power!.Script = Parse.PopFirstString(ref val);
                }
                else if (infile.Key == "remove_effect")
                {
                    string first = Parse.PopFirstString(ref val);
                    int second = Parse.PopFirstInt(ref val);
                    power!.RemoveEffects.Add((first, second));
                }
                else if (infile.Key == "replace_by_effect")
                {
                    PowerReplaceByEffect prbe = new PowerReplaceByEffect();
                    prbe.PowerId = Parse.ToPowerID(Parse.PopFirstString(ref val));
                    prbe.EffectId = Parse.PopFirstString(ref val);
                    prbe.Count = Parse.PopFirstInt(ref val);
                    power!.ReplaceByEffect.Add(prbe);
                }
                else if (infile.Key == "requires_corpse")
                {
                    if (val == "consume")
                    {
                        power!.RequiresCorpse = true;
                        power!.RemoveCorpse = true;
                    }
                    else
                    {
                        power!.RequiresCorpse = Parse.ToBool(val);
                        power!.RemoveCorpse = false;
                    }
                }
                else if (infile.Key == "target_nearest")
                {
                    power!.TargetNearest = Parse.ToFloat(val);
                }
                else if (infile.Key == "disable_equip_slots")
                {
                    power!.DisableEquipSlots.Clear();
                    string slotType = Parse.PopFirstString(ref val);

                    while (slotType != "")
                    {
                        power!.DisableEquipSlots.Add(SharedGameResources.Items!.GetItemTypeIndexByString(slotType));
                        slotType = Parse.PopFirstString(ref val);
                    }
                }
                else if (infile.Key == "post_hazards_skip_target")
                {
                    power!.PostHazardsSkipTarget = Parse.ToBool(val);
                }
                else if (infile.Key == "can_trigger_passives")
                {
                    power!.CanTriggerPassives = Parse.ToBool(val);
                }
                else if (infile.Key == "passive_effects_persist")
                {
                    power!.PassiveEffectsPersist = Parse.ToBool(val);
                }
                else if (infile.Key == "dispel")
                {
                    power!.DispelPowerIds.Clear();
                    string dispelId = Parse.PopFirstString(ref val);

                    while (dispelId != "")
                    {
                        power!.DispelPowerIds.Add(Parse.ToPowerID(dispelId));
                        dispelId = Parse.PopFirstString(ref val);
                    }
                }

                else infile.Error("PowerManager: '%s' is not a valid key", infile.Key);
            }

            int countAllocated = 0;
            for (int i = 0; i < Powers.Count; ++i)
            {
                power = Powers[i];

                if (power == null)
                    continue;
                else
                    countAllocated++;

                if (!string.IsNullOrEmpty(power.AnimationName))
                {
                    SharedResources.Anim!.IncreaseCount(power.AnimationName);
                    _powerAnimations[i] = SharedResources.Anim!.GetAnimationSet(power.AnimationName)!.GetAnimation("");
                }

                power.BuffPartyPowerId = VerifyID(power.BuffPartyPowerId, null, AllowZeroId);

                for (int j = power.ChainPowers.Count; j > 0; --j)
                {
                    int index = j - 1;
                    power.ChainPowers[index].Id = VerifyID(power.ChainPowers[index].Id, null, !AllowZeroId);
                    if (power.ChainPowers[index].Id == 0)
                    {
                        if (power.ChainPowers[index].Type == ChainPower.TypePre)
                            Utils.LogError("PowerManager: Removed pre_power from power %zu.", i);
                        else if (power.ChainPowers[index].Type == ChainPower.TypePost)
                            Utils.LogError("PowerManager: Removed post_power from power %zu.", i);
                        else if (power.ChainPowers[index].Type == ChainPower.TypeWall)
                            Utils.LogError("PowerManager: Removed wall_power from power %zu.", i);

                        power.ChainPowers.RemoveAt(index);
                    }
                }

                for (int j = power.ReplaceByEffect.Count; j > 0; --j)
                {
                    int index = j - 1;
                    power.ReplaceByEffect[index].PowerId = VerifyID(power.ReplaceByEffect[index].PowerId, null, !AllowZeroId);
                    if (power.ReplaceByEffect[index].PowerId == 0)
                    {
                        Utils.LogError("PowerManager: Removed replace_by_effect from power %zu.", i);
                        power.ReplaceByEffect.RemoveAt(index);
                    }
                }

                {
                    if (!((!power.UseHazard && power.Type == Power.TypeFixed) || power.NoAttack))
                    {
                        if (power.Type == Power.TypeFixed)
                        {
                            if (power.RelativePos)
                            {
                                power.CombatRange += power.ChargeSpeed * (float)power.Lifespan;
                            }
                            if (power.StartingPos == Power.StartingPosTarget)
                            {
                                power.CombatRange = float.MaxValue - power.Radius;
                            }
                        }
                        else if (power.Type == Power.TypeMissile)
                        {
                            power.CombatRange += power.Speed * (float)power.Lifespan;
                        }
                        else if (power.Type == Power.TypeRepeater)
                        {
                            power.CombatRange += power.Speed * (float)power.Count;
                        }

                        power.CombatRange += (power.Radius / 2f);
                    }
                }
            }
            Utils.LogInfo("PowerManager: Power IDs = %zu reserved / %zu allocated / %zu empty / %zu bytes used", Powers.Count - 1, countAllocated, Powers.Count - 1 - countAllocated, (IntPtr.Size * Powers.Count) + (countAllocated * 800));

            if (SharedGameResources.Items != null)
            {
                for (int i = 1; i < SharedGameResources.Items.Items.Count; ++i)
                {
                    Item? item = SharedGameResources.Items.Items[i];

                    if (item == null)
                        continue;

                    item.Power = VerifyID(item.Power, null, AllowZeroId);

                    for (int j = item.Bonus.Count; j > 0; --j)
                    {
                        int index = j - 1;
                        if (item.Bonus[index].Type == BonusData.PowerLevel)
                        {
                            item.Bonus[index].PowerId = VerifyID(item.Bonus[index].PowerId, null, !AllowZeroId);

                            if (item.Bonus[index].PowerId == 0)
                                item.Bonus.RemoveAt(index);
                        }
                    }

                    for (int j = item.ReplacePower.Count; j > 0; --j)
                    {
                        int index = j - 1;
                        item.ReplacePower[index] = (
                            VerifyID(item.ReplacePower[index].Item1, null, !AllowZeroId),
                            VerifyID(item.ReplacePower[index].Item2, null, !AllowZeroId)
                        );

                        if (item.ReplacePower[index].Item1 == 0 || item.ReplacePower[index].Item2 == 0)
                            item.ReplacePower.RemoveAt(index);
                    }
                }
            }
        }

                private bool IsValidEffect(string type)
        {
            if (type == "speed")
                return true;
            if (type == "attack_speed")
                return true;

            if (type == "hp_percent")
                return true;
            if (type == "mp_percent")
                return true;

            for (int i = 0; i < SharedResources.Eset!.PrimaryStats.Stats.Count; ++i)
            {
                if (type == SharedResources.Eset.PrimaryStats.Stats[i].Id)
                    return true;
            }

            for (int i = 0; i < SharedResources.Eset!.DamageTypes.Types.Count; ++i)
            {
                if (type == SharedResources.Eset.DamageTypes.Types[i].Min)
                    return true;
                else if (type == SharedResources.Eset.DamageTypes.Types[i].Max)
                    return true;
                else if (type == SharedResources.Eset.DamageTypes.Types[i].Resist)
                    return true;
            }

            for (int i = 0; i < Stats.Count; ++i)
            {
                if (type == Stats.Key[i])
                    return true;
            }

            if (GetEffectDef(type) != null)
                return true;

            if (global::FlareEngine.Effect.GetTypeFromString(type) != global::FlareEngine.Effect.None)
                return true;

            return false;
        }

        private int LoadSFX(string filename)
        {
            SoundID sid = SharedResources.Snd!.Load(filename, "PowerManager sfx");
            int it = Sfx.IndexOf(sid);
            if (it == -1)
            {
                Sfx.Add(sid);
                return Sfx.Count - 1;
            }

            return it;
        }

        public bool HasValidTarget(PowerID powerIndex, StatBlock srcStats, Vector2 target)
        {
            if (_collider == null) return false;

            Power power = Powers[powerIndex]!;

            Vector2 limitTarget = Utils.ClampDistance(0, power.TargetRange, srcStats.Pos, target);

            if (power.RequiresLos && !_collider.LineOfSight(srcStats.Pos.X, srcStats.Pos.Y, limitTarget.X, limitTarget.Y))
                return false;

            if (power.RequiresEmptyTarget && !_collider.IsValidPosition(limitTarget.X, limitTarget.Y, power.MovementType, MapCollision.CollideTypeAllEntities))
                return false;

            if (power.BuffTeleport && power.TargetNeighbor < 1 && !_collider.IsValidPosition(limitTarget.X, limitTarget.Y, srcStats.MovementType, MapCollision.CollideTypeAllEntities))
                return false;

            return true;
        }

        private void InitHazard(PowerID powerIndex, StatBlock srcStats, Vector2 origin, Vector2 target, Hazard haz)
        {
            haz.SrcStats = srcStats;

            haz.PowerIndex = powerIndex;
            haz.HazardPower = Powers[powerIndex];

            if (haz.HazardPower!.SourceType == -1)
            {
                if (srcStats.Hero) haz.SourceType = Power.SourceTypeHero;
                else if (srcStats.HeroAlly) haz.SourceType = Power.SourceTypeAlly;
                else haz.SourceType = Power.SourceTypeEnemy;
            }
            else
            {
                haz.SourceType = haz.HazardPower!.SourceType;
            }

            haz.CritChance = srcStats.Get(Stats.Crit);
            haz.Accuracy = srcStats.Get(Stats.Accuracy);

            if (haz.HazardPower!.BaseDamage != haz.Damage.Count)
            {
                for (int i = 0; i < haz.Damage.Count; ++i)
                {
                    if (haz.HazardPower!.ConvertedDamage != haz.Damage.Count)
                    {
                        if (i == haz.HazardPower!.ConvertedDamage || i == haz.HazardPower!.BaseDamage)
                        {
                            haz.Damage[haz.HazardPower!.ConvertedDamage].Min += srcStats.GetDamageMin(i);
                            haz.Damage[haz.HazardPower!.ConvertedDamage].Max += srcStats.GetDamageMax(i);
                        }
                    }
                    else if (i == haz.HazardPower!.BaseDamage || (!SharedResources.Eset!.DamageTypes.Types[haz.HazardPower!.BaseDamage].IsElemental && SharedResources.Eset.DamageTypes.Types[i].IsElemental))
                    {
                        haz.Damage[i].Min += srcStats.GetDamageMin(i);
                        haz.Damage[i].Max += srcStats.GetDamageMax(i);
                    }
                }
            }

            if (haz.HazardPower!.AnimationName != "")
            {
                haz.LoadAnimation(haz.HazardPower!.AnimationName);
            }

            if (haz.HazardPower!.Directional)
            {
                haz.Direction = Utils.CalcDirection(origin.X, origin.Y, target.X, target.Y);
            }
            else if (haz.HazardPower!.VisualRandom != 0)
            {
                haz.Direction = (ushort)(Program.Rng.Next() % haz.HazardPower!.VisualRandom);
                haz.Direction += haz.HazardPower!.VisualOption;
            }
            else if (haz.HazardPower!.VisualOption != 0)
            {
                haz.Direction = haz.HazardPower!.VisualOption;
            }

            haz.BaseSpeed = haz.HazardPower!.Speed;
            haz.Lifespan = haz.HazardPower!.Lifespan;
            haz.Active = !haz.HazardPower!.NoAttack;

            if (haz.HazardPower!.StartingPos == Power.StartingPosSource)
            {
                haz.Pos = origin;
            }
            else if (haz.HazardPower!.StartingPos == Power.StartingPosTarget)
            {
                haz.Pos = Utils.ClampDistance(0, haz.HazardPower!.TargetRange, origin, target);
            }
            else if (haz.HazardPower!.StartingPos == Power.StartingPosMelee)
            {
                haz.Pos = Utils.CalcVector(origin, srcStats.Direction, srcStats.MeleeRange);
            }
            else if (haz.HazardPower!.StartingPos == Power.StartingPosMeleeUnlocked)
            {
                haz.Pos = Utils.ClampDistance(srcStats.MeleeRange, srcStats.MeleeRange, origin, target);
            }

            if (haz.HazardPower!.TargetNeighbor > 0 && _collider != null)
            {
                haz.Pos = _collider.GetRandomNeighbor(new Int2((int)haz.Pos.X, (int)haz.Pos.Y), haz.HazardPower!.TargetNeighbor, haz.HazardPower!.MovementType, MapCollision.CollideTypeHazard);
            }

            if (haz.HazardPower!.RelativePos)
            {
                haz.RelativePos = true;
                haz.PosOffset.X = srcStats.Pos.X - haz.Pos.X;
                haz.PosOffset.Y = srcStats.Pos.Y - haz.Pos.Y;
            }
        }

        private void Buff(PowerID powerIndex, StatBlock srcStats, Vector2 origin, Vector2 target)
        {
            Power power = Powers[powerIndex]!;

            if (power.BuffTeleport)
            {
                Vector2 limitTarget = Utils.ClampDistance(0, power.TargetRange, srcStats.Pos, target);
                if (power.TargetNeighbor > 0 && _collider != null)
                {
                    Vector2 newTarget = _collider.GetRandomNeighbor(new Int2((int)limitTarget.X, (int)limitTarget.Y), power.TargetNeighbor, power.MovementType, MapCollision.CollideTypeAllEntities);
                    if (MathF.Floor(newTarget.X) == MathF.Floor(limitTarget.X) && MathF.Floor(newTarget.Y) == MathF.Floor(limitTarget.Y))
                    {
                        srcStats.Teleportation = false;
                    }
                    else
                    {
                        srcStats.Teleportation = true;
                        srcStats.TeleportDestination.X = newTarget.X;
                        srcStats.TeleportDestination.Y = newTarget.Y;
                    }
                }
                else
                {
                    srcStats.Teleportation = true;
                    srcStats.TeleportDestination.X = limitTarget.X;
                    srcStats.TeleportDestination.Y = limitTarget.Y;
                }
            }

            if (power.Buff || (power.BuffParty && (srcStats.HeroAlly || srcStats.EnemyAlly)))
            {
                int sourceType = srcStats.Hero ? Power.SourceTypeHero : (srcStats.HeroAlly ? Power.SourceTypeAlly : Power.SourceTypeEnemy);
                Effect(srcStats, srcStats, powerIndex, sourceType);
            }

            if (power.BuffParty && !power.Passive)
            {
                srcStats.PartyBuffs.Enqueue(powerIndex);
            }

            if (!power.UseHazard)
            {
                srcStats.Effects.RemoveEffectID(power.RemoveEffects);

                if (!power.Passive)
                {
                    for (int i = 0; i < power.ChainPowers.Count; ++i)
                    {
                        ChainPower chainPower = power.ChainPowers[i];
                        if (chainPower.Type == ChainPower.TypePost && MathUtils.PercentChanceF(chainPower.Chance))
                        {
                            Activate(chainPower.Id, srcStats, origin, srcStats.Pos);
                        }
                    }
                }
            }
        }

        private void PlaySound(PowerID powerIndex, Vector2 soundPos)
        {
            if (Powers[powerIndex]!.SfxIndex != -1)
                SharedResources.Snd!.Play(Sfx[Powers[powerIndex]!.SfxIndex], SoundManager.DefaultChannel, soundPos, !SoundManager.Loop);
        }

        private static EffectDef CopyEffectDef(EffectDef src)
        {
            EffectDef dst = new EffectDef();
            dst.Id = src.Id;
            dst.Type = src.Type;
            dst.Name = src.Name;
            dst.Icon = src.Icon;
            dst.AnimationName = src.AnimationName;
            dst.CanStack = src.CanStack;
            dst.MaxStacks = src.MaxStacks;
            dst.GroupStack = src.GroupStack;
            dst.RenderAbove = src.RenderAbove;
            dst.ColorMod = src.ColorMod;
            dst.AlphaMod = src.AlphaMod;
            dst.AttackSpeedAnim = src.AttackSpeedAnim;
            dst.IgnoreResist = src.IgnoreResist;
            dst.IsImmunityType = src.IsImmunityType;
            return dst;
        }

        public bool Effect(StatBlock targetStats, StatBlock casterStats, PowerID powerIndex, int sourceType)
        {
            if (!IsValid(powerIndex))
                return false;

            Power pwr = Powers[powerIndex]!;
            for (int i = 0; i < pwr.PostEffects.Count; ++i)
            {
                PostEffect pe = pwr.PostEffects[i];

                if (!MathUtils.PercentChanceF(pe.Chance))
                    continue;

                EffectDef effectData = new EffectDef();
                EffectDef? effectPtr = pwr.PostEffects[i].EffectPtr;

                float magnitude = pe.Magnitude;
                int duration = pe.Duration;

                StatBlock destStats = pe.TargetSrc ? casterStats : targetStats;
                if (destStats.Hp <= 0 && !(effectData.Type == global::FlareEngine.Effect.Revive || (effectData.Type == global::FlareEngine.Effect.None && pe.Id == "revive")))
                    continue;

                if (effectPtr != null)
                {
                    effectData = CopyEffectDef(effectPtr);

                    if (effectData.Type == global::FlareEngine.Effect.Shield)
                    {
                        if (pwr.BaseDamage == SharedResources.Eset!.DamageTypes.Types.Count)
                            continue;

                        if (pwr.ModDamageMode == Power.StatModifierModeMultiply)
                            magnitude = casterStats.GetDamageMax(pwr.BaseDamage) * pwr.ModDamageValueMin / 100;
                        else if (pwr.ModDamageMode == Power.StatModifierModeAdd)
                            magnitude = casterStats.GetDamageMax(pwr.BaseDamage) + pwr.ModDamageValueMin;
                        else if (pwr.ModDamageMode == Power.StatModifierModeAbsolute)
                            magnitude = MathUtils.RandBetweenF(pwr.ModDamageValueMin, pwr.ModDamageValueMax);
                        else
                            magnitude = casterStats.GetDamageMax(pwr.BaseDamage);

                        magnitude = SharedResources.Eset!.Combat.ResourceRound(magnitude);
                        SharedResources.Comb!.AddString(SharedResources.Msg!.GetV("+%s Shield", Utils.FloatToString(magnitude, SharedResources.Eset.NumberFormat.CombatText)), destStats.Pos, CombatText.MsgBuff);
                    }
                    else if (effectData.Type == global::FlareEngine.Effect.Heal)
                    {
                        if (pwr.BaseDamage == SharedResources.Eset!.DamageTypes.Types.Count)
                            continue;

                        magnitude = MathUtils.RandBetweenF(casterStats.GetDamageMin(pwr.BaseDamage), casterStats.GetDamageMax(pwr.BaseDamage));

                        if (pwr.ModDamageMode == Power.StatModifierModeMultiply)
                            magnitude = magnitude * pwr.ModDamageValueMin / 100;
                        else if (pwr.ModDamageMode == Power.StatModifierModeAdd)
                            magnitude += pwr.ModDamageValueMin;
                        else if (pwr.ModDamageMode == Power.StatModifierModeAbsolute)
                            magnitude = MathUtils.RandBetweenF(pwr.ModDamageValueMin, pwr.ModDamageValueMax);

                        SharedResources.Comb!.AddString(SharedResources.Msg!.GetV("+%s HP", Utils.FloatToString(magnitude, SharedResources.Eset.NumberFormat.CombatText)), destStats.Pos, CombatText.MsgBuff);
                        destStats.Hp += magnitude;
                        if (destStats.Hp > destStats.Get(global::FlareEngine.Stats.HpMax)) destStats.Hp = destStats.Get(global::FlareEngine.Stats.HpMax);
                    }
                    else if (effectData.Type == global::FlareEngine.Effect.Knockback)
                    {
                        if (destStats.SpeedDefault == 0)
                        {
                            continue;
                        }
                        destStats.KnockbackSrcpos = pe.TargetSrc ? targetStats.Pos : casterStats.Pos;
                        destStats.KnockbackDestpos = pe.TargetSrc ? casterStats.Pos : targetStats.Pos;
                    }
                }
                else
                {
                    effectData.Id = pe.Id;
                    effectData.Type = pe.EffectType;
                }

                EffectParams effectParams = new EffectParams();
                effectParams.Duration = duration;
                effectParams.Magnitude = magnitude;
                effectParams.SourceType = sourceType;
                effectParams.PowerId = powerIndex;
                effectParams.IsMultiplier = pe.IsMultiplier;

                destStats.Effects.AddEffect(destStats, effectData, effectParams);
            }

            return true;
        }

                private bool Fixed(PowerID powerIndex, StatBlock srcStats, Vector2 origin, Vector2 target)
        {
            Power power = Powers[powerIndex]!;

            if (power.UseHazard)
            {
                int delayIterator = 0;
                for (int i = 0; i < power.Count; i++)
                {
                    Hazard haz = new Hazard(_collider);
                    InitHazard(powerIndex, srcStats, origin, target, haz);

                    haz.DelayFrames = delayIterator;
                    delayIterator += power.Delay;

                    Hazards.Enqueue(haz);
                }
            }

            Buff(powerIndex, srcStats, origin, target);

            if (Hazards.Count > 0)
                PlaySound(powerIndex, Hazards.Last().Pos);
            else
                PlaySound(powerIndex, origin);

            PayPowerCost(powerIndex, srcStats);
            return true;
        }

        private bool Missile(PowerID powerIndex, StatBlock srcStats, Vector2 origin, Vector2 target)
        {
            Power power = Powers[powerIndex]!;

            Vector2 src;
            if (power.StartingPos == Power.StartingPosTarget)
            {
                src = target;
            }
            else
            {
                src = origin;
            }

            float theta = Utils.CalcTheta(src.X, src.Y, target.X, target.Y);

            int delayIterator = 0;

            for (int i = 0; i < power.Count; i++)
            {
                Hazard haz = new Hazard(_collider);
                InitHazard(powerIndex, srcStats, origin, target, haz);

                float offsetAngle = ((1.0f - (float)power.Count) / 2 + (float)i) * (power.MissileAngle * MathF.PI / 180.0f);
                float variance = 0;
                if (power.AngleVariance != 0)
                {
                    variance = MathUtils.RandBetweenF(power.AngleVariance * -1f, power.AngleVariance) * MathF.PI / 180.0f;
                }
                float alpha = theta + offsetAngle + variance;

                float speedVar = 0;
                if (power.SpeedVariance != 0)
                {
                    float var = power.SpeedVariance;
                    speedVar = ((var * 2.0f * (float)Program.Rng.NextDouble()) - var);
                    speedVar *= Settings.LogicFps / (float)SharedResources.Settings!.MaxFramesPerSec;
                }

                haz.BaseSpeed += speedVar;
                haz.SetAngle(alpha);

                haz.DelayFrames = delayIterator;
                delayIterator += power.Delay;

                Hazards.Enqueue(haz);
            }

            PayPowerCost(powerIndex, srcStats);

            if (Hazards.Count > 0)
                PlaySound(powerIndex, Hazards.Last().Pos);
            else
                PlaySound(powerIndex, origin);

            return true;
        }

        private bool Repeater(PowerID powerIndex, StatBlock srcStats, Vector2 origin, Vector2 target)
        {
            Power power = Powers[powerIndex]!;

            Vector2 locationIterator;
            Vector2 speed;
            int delayIterator = 0;

            float theta = Utils.CalcTheta(origin.X, origin.Y, target.X, target.Y);

            float repeaterSpeed = (power.Speed * SharedResources.Settings!.MaxFramesPerSec) / Settings.LogicFps;

            speed.X = repeaterSpeed * MathF.Cos(theta);
            speed.Y = repeaterSpeed * MathF.Sin(theta);

            locationIterator = origin;

            Hazard? parentHaz = null;
            bool firstHitWall = false;
            for (int i = 0; i < power.Count; i++)
            {

                locationIterator.X += speed.X;
                locationIterator.Y += speed.Y;

                if (_collider != null && !_collider.IsValidPosition(locationIterator.X, locationIterator.Y, power.MovementType, MapCollision.CollideTypeHazard))
                {
                    if (i == 0)
                        firstHitWall = true;
                    else
                        break;
                }

                Hazard haz = new Hazard(_collider);
                InitHazard(powerIndex, srcStats, origin, target, haz);

                haz.Pos = locationIterator;
                haz.DelayFrames = delayIterator;
                delayIterator += power.Delay;

                if (i == 0 && power.Count > 1)
                {
                    parentHaz = haz;
                }
                else if (parentHaz != null && i > 0)
                {
                    haz.Parent = parentHaz;
                    parentHaz.Children.Add(haz);
                }

                Hazards.Enqueue(haz);

                if (firstHitWall)
                    break;
            }

            PayPowerCost(powerIndex, srcStats);

            if (Hazards.Count > 0)
                PlaySound(powerIndex, Hazards.Last().Pos);
            else
                PlaySound(powerIndex, origin);

            return true;

        }

        private bool Spawn(PowerID powerIndex, StatBlock srcStats, Vector2 origin, Vector2 target)
        {
            if (_collider == null)
                return false;

            Power power = Powers[powerIndex]!;

            MapEnemy espawn = new MapEnemy();
            espawn.Type = power.SpawnType;
            espawn.Summoner = srcStats;

            if (power.StartingPos == Power.StartingPosSource)
            {
                espawn.Pos = origin;
            }
            else if (power.StartingPos == Power.StartingPosTarget)
            {
                espawn.Pos = target;
            }
            else if (power.StartingPos == Power.StartingPosMelee)
            {
                espawn.Pos = Utils.CalcVector(origin, srcStats.Direction, srcStats.MeleeRange);
            }
            else if (power.StartingPos == Power.StartingPosMeleeUnlocked)
            {
                espawn.Pos = Utils.ClampDistance(srcStats.MeleeRange, srcStats.MeleeRange, origin, target);
            }

            if (power.TargetNeighbor > 0 && _collider != null)
            {
                espawn.Pos = _collider.GetRandomNeighbor(new Int2((int)espawn.Pos.X, (int)espawn.Pos.Y), power.TargetNeighbor, power.MovementType, MapCollision.CollideTypeAllEntities);
            }

            if (!_collider.IsValidPosition(espawn.Pos.X, espawn.Pos.Y, power.MovementType, MapCollision.CollideTypeAllEntities))
            {
                espawn.Pos = _collider.GetRandomNeighbor(new Int2((int)origin.X, (int)origin.Y), 1, power.MovementType, MapCollision.CollideTypeAllEntities);
            }

            if (!_collider.IsValidPosition(espawn.Pos.X, espawn.Pos.Y, power.MovementType, MapCollision.CollideTypeAllEntities))
            {
                return false;
            }

            espawn.Direction = Utils.CalcDirection(origin.X, origin.Y, target.X, target.Y);
            espawn.SummonPowerIndex = powerIndex;
            espawn.HeroAlly = srcStats.Hero || srcStats.HeroAlly;
            espawn.EnemyAlly = !srcStats.Hero;

            _collider.Block(espawn.Pos.X, espawn.Pos.Y, espawn.HeroAlly);

            for (int i = 0; i < power.Count; i++)
            {
                MapEnemies.Enqueue(espawn);
            }

            Buff(powerIndex, srcStats, origin, target);

            PayPowerCost(powerIndex, srcStats);

            PlaySound(powerIndex, espawn.Pos);

            return true;
        }

        private bool Transform(PowerID powerIndex, StatBlock srcStats, Vector2 target)
        {
            if (_collider == null)
                return false;

            Power power = Powers[powerIndex]!;

            SharedResources.Inpt!.LockActionBar();

            if (srcStats.Transformed && power.SpawnType != "untransform")
            {
                SharedGameResources.Pc!.LogMsg(SharedResources.Msg!.Get("You are already transformed, untransform first."), Avatar.MsgNormal);
                return false;
            }

            if (power.SpawnType == "untransform" && srcStats.Transformed)
            {
                _collider.Unblock(srcStats.Pos.X, srcStats.Pos.Y);
                if (_collider.IsValidPosition(srcStats.Pos.X, srcStats.Pos.Y, MapCollision.MoveNormal, MapCollision.CollideTypeHero))
                {
                    srcStats.TransformDuration = 0;
                    srcStats.TransformType = "untransform";
                }
                else
                {
                    SharedGameResources.Pc!.LogMsg(SharedResources.Msg!.Get("Could not untransform at this position."), Avatar.MsgNormal);
                    SharedResources.Inpt!.UnlockActionBar();
                    _collider.Block(srcStats.Pos.X, srcStats.Pos.Y, false);
                    return false;
                }
                _collider.Block(srcStats.Pos.X, srcStats.Pos.Y, false);
            }
            else
            {
                if (power.TransformDuration == 0)
                {
                    srcStats.TransformDuration = -1;
                }
                else if (power.TransformDuration > 0)
                {
                    srcStats.TransformDuration = power.TransformDuration;
                }

                srcStats.TransformType = power.SpawnType;
            }

            Buff(powerIndex, srcStats, srcStats.Pos, target);

            srcStats.ManualUntransform = power.ManualUntransform;
            srcStats.TransformWithEquipment = power.KeepEquipment;
            srcStats.UntransformOnHit = power.UntransformOnHit;

            PlaySound(powerIndex, srcStats.Pos);

            PayPowerCost(powerIndex, srcStats);

            return true;
        }

        private bool Block(PowerID powerIndex, StatBlock srcStats)
        {
            Power power = Powers[powerIndex]!;

            if (srcStats.Effects.TriggeredBlock)
                return false;

            srcStats.Effects.TriggeredBlock = true;
            srcStats.BlockPower = powerIndex;

            power.PassiveTrigger = Power.TriggerBlock;
            Effect(srcStats, srcStats, powerIndex, Power.SourceTypeHero);

            PlaySound(powerIndex, srcStats.Pos);

            PayPowerCost(powerIndex, srcStats);

            return true;
        }

        public PowerID CheckReplaceByEffect(PowerID powerIndex, StatBlock srcStats)
        {
            if (!IsValid(powerIndex))
                return 0;

            Power power = Powers[powerIndex]!;

            for (int i = 0; i < power.ReplaceByEffect.Count; ++i)
            {
                if (srcStats.Effects.HasEffect(power.ReplaceByEffect[i].EffectId, power.ReplaceByEffect[i].Count))
                {
                    return power.ReplaceByEffect[i].PowerId;
                }
            }

            return powerIndex;
        }

        public bool Activate(PowerID powerIndex, StatBlock srcStats, Vector2 origin, Vector2 target)
        {
            if (!IsValid(powerIndex))
                return false;

            Power power = Powers[powerIndex]!;

            if (srcStats.Hero)
            {
                if (power.RequiresMp > srcStats.Mp)
                    return false;

                for (int i = 0; i < power.RequiresResourceStat.Count; ++i)
                {
                    if (power.RequiresResourceStat[i] > srcStats.ResourceStats[i])
                        return false;
                }

                if (srcStats.TargetCorpse == null && srcStats.TargetNearestCorpse != null && CheckNearestTargeting(power, srcStats, true))
                    srcStats.TargetCorpse = srcStats.TargetNearestCorpse;

                if (power.RequiresCorpse && srcStats.TargetCorpse == null)
                    return false;
            }

            if (srcStats.Hp > 0 && power.Sacrifice == false && power.RequiresHp >= srcStats.Hp)
                return false;

            if (power.Type == Power.TypeBlock)
                return Block(powerIndex, srcStats);

            if (power.ScriptTrigger == Power.ScriptTriggerCast)
            {
                SharedGameResources.Eventm!.ExecuteScript(power.Script, origin.X, origin.Y);
            }

            Vector2 newTarget = target;
            if (power.LockTargetToDirection)
            {
                float dist = Utils.CalcDist(origin, newTarget);
                int dir = Utils.CalcDirection(origin.X, origin.Y, newTarget.X, newTarget.Y);
                newTarget = Utils.CalcVector(origin, dir, dist);
            }
            else if (power.Speed != 0)
            {
                if (origin.X == target.X && origin.Y == target.Y)
                {
                    float dist = Math.Max(0.1f, srcStats.MeleeRange);
                    if (srcStats.Pos.X == origin.X && srcStats.Pos.Y == origin.Y)
                    {
                        newTarget = Utils.CalcVector(origin, srcStats.Direction, dist);
                    }
                    else
                    {
                        newTarget = Utils.CalcVector(origin, Program.Rng.Next() % 8, dist);
                    }
                }
            }

            if (!power.Passive && power.CanTriggerPassives)
            {
                srcStats.Effects.TriggeredActivePower = true;
            }

            switch (power.Type)
            {
                case Power.TypeFixed:
                    return Fixed(powerIndex, srcStats, origin, newTarget);
                case Power.TypeMissile:
                    return Missile(powerIndex, srcStats, origin, newTarget);
                case Power.TypeRepeater:
                    return Repeater(powerIndex, srcStats, origin, newTarget);
                case Power.TypeSpawn:
                    return Spawn(powerIndex, srcStats, origin, newTarget);
                case Power.TypeTransform:
                    return Transform(powerIndex, srcStats, newTarget);
            }

            return false;
        }

        private void PayPowerCost(PowerID powerIndex, StatBlock srcStats)
        {
            Power power = Powers[powerIndex]!;

            if (srcStats != null)
            {
                if (srcStats.Hero)
                {
                    srcStats.Mp -= power.RequiresMp;

                    for (int i = 0; i < power.RequiresResourceStat.Count; ++i)
                    {
                        srcStats.ResourceStats[i] -= power.RequiresResourceStat[i];
                    }

                    for (int i = 0; i < power.RequiredItems.Count; ++i)
                    {
                        PowerRequiredItem pri = power.RequiredItems[i];
                        if (pri.Id > 0)
                        {
                            if (pri.Equipped && UsedEquippedItems.Contains(pri.Id))
                            {
                                continue;
                            }

                            int quantity = pri.Quantity;
                            while (quantity > 0)
                            {
                                if (pri.Equipped)
                                    UsedEquippedItems.Add(pri.Id);
                                else
                                    UsedItems.Add(pri.Id);

                                quantity--;
                            }
                        }
                    }
                }
                if (power.RequiresHp > 0)
                {
                    srcStats.TakeDamage(power.RequiresHp, !StatBlock.TakeDmgCrit, Power.SourceTypeNeutral);
                }

                if (power.RequiresCorpse && power.RemoveCorpse && srcStats.TargetCorpse != null)
                {
                    srcStats.TargetCorpse.CorpseTimer.Reset(Timer.End);
                    srcStats.TargetCorpse = null;
                }
            }
        }

        public void ActivatePassives(StatBlock srcStats)
        {
            bool activatedPassive = false;
            bool triggeredOthers = false;
            for (int i = 0; i < srcStats.PowersPassive.Count; i++)
            {
                activatedPassive |= ActivatePassiveByTrigger(srcStats.PowersPassive[i], srcStats, ref triggeredOthers);
            }

            for (int i = 0; i < srcStats.PowersListItems.Count; i++)
            {
                activatedPassive |= ActivatePassiveByTrigger(srcStats.PowersListItems[i], srcStats, ref triggeredOthers);
            }

            if (triggeredOthers)
                srcStats.Effects.TriggeredOthers = true;

            srcStats.Effects.TriggeredHit = false;
            srcStats.Effects.TriggeredDeath = false;

            ActivatePassivePostPowers(srcStats);

            if (activatedPassive && srcStats.Hero)
                SharedGameResources.Menu!.Inv!.ApplyEquipment();
        }

        private bool ActivatePassiveByTrigger(PowerID powerId, StatBlock srcStats, ref bool triggeredOthers)
        {
            Power power = Powers[powerId]!;

            if (power.Passive)
            {
                int trigger = power.PassiveTrigger;

                if (trigger != Power.TriggerDeath)
                {
                    if (srcStats.Hp == 0)
                    {
                        bool isRevivePassive = false;
                        for (int i = 0; i < power.PostEffects.Count; ++i)
                        {
                            if (power.PostEffects[i].EffectType == global::FlareEngine.Effect.Revive)
                            {
                                isRevivePassive = true;
                                break;
                            }
                        }
                        if (!isRevivePassive)
                            return false;
                    }
                }

                if (trigger == -1)
                {
                    if (srcStats.Effects.TriggeredOthers)
                        return false;
                    else
                        triggeredOthers = true;
                }
                else if (trigger == Power.TriggerBlock && !srcStats.Effects.TriggeredBlock)
                    return false;
                else if (trigger == Power.TriggerHit && !srcStats.Effects.TriggeredHit)
                    return false;
                else if (trigger == Power.TriggerHalfdeath && !srcStats.Effects.TriggeredHalfdeath)
                {
                    if (srcStats.Hp > srcStats.Get(global::FlareEngine.Stats.HpMax) / 2)
                        return false;
                    else
                        srcStats.Effects.TriggeredHalfdeath = true;
                }
                else if (trigger == Power.TriggerJoincombat && !srcStats.Effects.TriggeredJoincombat)
                {
                    if (!srcStats.InCombat)
                        return false;
                    else
                        srcStats.Effects.TriggeredJoincombat = true;
                }
                else if (trigger == Power.TriggerDeath && !srcStats.Effects.TriggeredDeath)
                {
                    return false;
                }
                else if (trigger == Power.TriggerActivePower && !srcStats.Effects.TriggeredActivePower)
                {
                    return false;
                }

                Activate(powerId, srcStats, srcStats.Pos, srcStats.Pos);
                srcStats.RefreshStats = true;

                for (int i = 0; i < power.ChainPowers.Count; ++i)
                {
                    ChainPower chainPower = power.ChainPowers[i];
                    if (chainPower.Type == ChainPower.TypePost)
                    {
                        srcStats.SetPowerCooldown(chainPower.Id, Powers[chainPower.Id]!.Cooldown);
                    }
                }

                return true;
            }
            return false;
        }

        public void ActivateSinglePassive(StatBlock srcStats, PowerID id)
        {
            if (!IsValid(id))
                return;

            Power power = Powers[id]!;

            if (!power.Passive) return;

            if (power.PassiveTrigger == -1)
            {
                if (srcStats.Hp == 0)
                {
                    bool isRevivePassive = false;
                    for (int i = 0; i < power.PostEffects.Count; ++i)
                    {
                        if (power.PostEffects[i].EffectType == global::FlareEngine.Effect.Revive)
                        {
                            isRevivePassive = true;
                            break;
                        }
                    }
                    if (!isRevivePassive)
                        return;
                }

                Activate(id, srcStats, srcStats.Pos, srcStats.Pos);
                srcStats.RefreshStats = true;
                srcStats.Effects.TriggeredOthers = true;

                for (int i = 0; i < power.ChainPowers.Count; ++i)
                {
                    ChainPower chainPower = power.ChainPowers[i];
                    if (chainPower.Type == ChainPower.TypePost)
                    {
                        srcStats.SetPowerCooldown(chainPower.Id, Powers[chainPower.Id]!.Cooldown);
                    }
                }
            }
        }

        private void ActivatePassivePostPowers(StatBlock srcStats)
        {
            for (int i = 0; i < srcStats.PowersPassive.Count; ++i)
            {
                if (!srcStats.CanUsePower(srcStats.PowersPassive[i], StatBlock.CanUsePassive))
                    continue;

                Power passivePower = Powers[srcStats.PowersPassive[i]]!;

                for (int j = 0; j < passivePower.ChainPowers.Count; ++j)
                {
                    ChainPower chainPower = passivePower.ChainPowers[j];
                    if (Powers[chainPower.Id]!.NewState != Power.StateInstant)
                        continue;

                    if (passivePower.Type == Power.TypeBlock)
                        continue;

                    if (srcStats.GetPowerCooldown(chainPower.Id) == 0 && srcStats.CanUsePower(chainPower.Id, !StatBlock.CanUsePassive))
                    {
                        if (MathUtils.PercentChanceF(chainPower.Chance))
                        {
                            Activate(chainPower.Id, srcStats, srcStats.Pos, srcStats.Pos);
                            srcStats.SetPowerCooldown(chainPower.Id, Powers[chainPower.Id]!.Cooldown);
                        }
                    }
                }
            }
        }

        public EffectDef? GetEffectDef(string id)
        {
            for (int i = 0; i < Effects.Count; ++i)
            {
                if (Effects[i].Id == id)
                {
                    return Effects[i];
                }
            }
            return null;
        }

        public PowerID VerifyID(PowerID powerId, FileParser? infile, bool allowZero)
        {
            if ((!allowZero && powerId == 0) || powerId >= Powers.Count || (powerId > 0 && Powers[powerId] == null))
            {
                if (infile != null)
                    infile.Error("PowerManager: %d is not a valid power id.", powerId);
                else
                    Utils.LogError("PowerManager: %d is not a valid power id.", powerId);

                return 0;
            }
            return powerId;
        }

        public bool CheckNearestTargeting(Power pow, StatBlock srcStats, bool checkCorpses)
        {
            if (srcStats == null)
                return false;

            if (pow.TargetNearest <= 0)
                return true;

            if (!checkCorpses && srcStats.TargetNearest != null && pow.TargetNearest > srcStats.TargetNearestDist)
                return true;
            else if (checkCorpses && srcStats.TargetNearestCorpse != null && pow.TargetNearest > srcStats.TargetNearestCorpseDist)
                return true;

            return false;
        }

        public bool CheckRequiredItems(Power pow, StatBlock srcStats)
        {
            for (int i = 0; i < pow.RequiredItems.Count; ++i)
            {
                if (pow.RequiredItems[i].Id > 0)
                {
                    if (pow.RequiredItems[i].Equipped)
                    {
                        if (!SharedGameResources.Menu!.Inv!.EquipmentContain(pow.RequiredItems[i].Id, 1))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!SharedGameResources.Items!.RequirementsMet(srcStats, pow.RequiredItems[i].Id))
                        {
                            return false;
                        }

                        int quantity = Math.Max(1, pow.RequiredItems[i].Quantity);
                        if (!SharedGameResources.Menu!.Inv!.Inventory[MenuInventory.Carried].Contain(pow.RequiredItems[i].Id, quantity))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public bool CheckRequiredResourceState(Power pow, StatBlock srcStats)
        {
            bool hpOk = true;
            bool mpOk = true;

            if (pow.RequiresHpState.State == Power.ResourceStatePercent && srcStats.Hp * 100 < (srcStats.Get(global::FlareEngine.Stats.HpMax) * pow.RequiresHpState.Value))
                hpOk = false;
            else if (pow.RequiresHpState.State == Power.ResourceStateNotPercent && srcStats.Hp * 100 >= (srcStats.Get(global::FlareEngine.Stats.HpMax) * pow.RequiresHpState.Value))
                hpOk = false;
            else if (pow.RequiresHpState.State == Power.ResourceStatePercentExact && srcStats.Hp * 100 != (srcStats.Get(global::FlareEngine.Stats.HpMax) * pow.RequiresHpState.Value))
                hpOk = false;

            if (pow.RequiresMpState.State == Power.ResourceStatePercent && srcStats.Mp * 100 < (srcStats.Get(global::FlareEngine.Stats.MpMax) * pow.RequiresMpState.Value))
                mpOk = false;
            else if (pow.RequiresMpState.State == Power.ResourceStateNotPercent && srcStats.Mp * 100 >= (srcStats.Get(global::FlareEngine.Stats.MpMax) * pow.RequiresMpState.Value))
                mpOk = false;
            else if (pow.RequiresMpState.State == Power.ResourceStatePercentExact && srcStats.Mp * 100 != (srcStats.Get(global::FlareEngine.Stats.MpMax) * pow.RequiresMpState.Value))
                mpOk = false;

            bool hpmpOk = true;
            if (pow.RequiresHpmpStateMode == Power.ResourceStateAll)
                hpmpOk = hpOk && mpOk;
            else if (pow.RequiresHpmpStateMode == Power.ResourceStateAny)
                hpmpOk = hpOk || mpOk;

            bool resourceStatOk = true;
            int resourceStatEnabledCount = 0;
            int resourceStatFailCount = 0;

            for (int i = 0; i < pow.RequiresResourceStatState.Count; ++i)
            {
                if (pow.RequiresResourceStatState[i].State != Power.ResourceStateIgnore)
                    resourceStatEnabledCount++;
                else
                    continue;

                float resourceMax = srcStats.GetResourceStat(i, EngineSettings.ResourceStatsSettings.StatBase);
                if (pow.RequiresResourceStatState[i].State == Power.ResourceStatePercent && srcStats.ResourceStats[i] * 100 < resourceMax * pow.RequiresResourceStatState[i].Value)
                    resourceStatFailCount++;
                else if (pow.RequiresResourceStatState[i].State == Power.ResourceStateNotPercent && srcStats.ResourceStats[i] * 100 >= resourceMax * pow.RequiresResourceStatState[i].Value)
                    resourceStatFailCount++;
                else if (pow.RequiresResourceStatState[i].State == Power.ResourceStatePercentExact && srcStats.ResourceStats[i] * 100 != resourceMax * pow.RequiresResourceStatState[i].Value)
                    resourceStatFailCount++;
            }

            if (resourceStatEnabledCount > 0)
            {
                if (pow.RequiresResourceStatStateMode == Power.ResourceStateAnyHpmp && !hpmpOk && resourceStatFailCount == resourceStatEnabledCount)
                    resourceStatOk = false;
                else if (pow.RequiresResourceStatStateMode == Power.ResourceStateAny && resourceStatFailCount == resourceStatEnabledCount)
                    resourceStatOk = false;
                else if (pow.RequiresResourceStatStateMode == Power.ResourceStateAll && resourceStatFailCount > 0)
                    resourceStatOk = false;
            }

            if (pow.RequiresResourceStatStateMode == Power.ResourceStateAnyHpmp)
                return hpmpOk || resourceStatOk;
            else
                return hpmpOk && resourceStatOk;
        }

        public bool CheckCombatRange(PowerID powerIndex, StatBlock srcStats, Vector2 target)
        {
            if (!IsValid(powerIndex))
                return false;

            Power pow = Powers[powerIndex]!;

            if (pow.CombatRange == 0)
                return false;

            float combatRange = pow.CombatRange;
            float targetRange = pow.TargetRange + (pow.Radius / 2f);

            if (pow.StartingPos == Power.StartingPosMelee || pow.StartingPos == Power.StartingPosMeleeUnlocked)
            {
                combatRange += srcStats.MeleeRange;
            }

            if (pow.TargetRange > 0 && targetRange < combatRange)
            {
                combatRange = targetRange;
            }

            return Utils.CalcDist(srcStats.Pos, target) <= combatRange;
        }

        public bool CheckPowerCost(Power pow, StatBlock srcStats)
        {
            if (srcStats.Mp < pow.RequiresMp)
                return false;

            if (!pow.Sacrifice && srcStats.Hp < pow.RequiresHp)
                return false;

            for (int i = 0; i < pow.RequiresResourceStat.Count; ++i)
            {
                if (srcStats.ResourceStats[i] < pow.RequiresResourceStat[i])
                    return false;
            }

            return true;
        }
    }
}
