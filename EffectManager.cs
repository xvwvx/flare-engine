// 对应 C++ 源文件：EffectManager.h + EffectManager.cpp
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// Effect
    ///
    /// 描述一个已经附加到 <see cref="EffectManager"/> 上、正在生效的具体效果实例
    /// （对应原始 <c>class Effect</c>）。
    ///
    /// 命名说明（需人工复核，详见转换报告）：
    /// 原始字段 <c>timer</c>（类型 <c>Timer</c>）与 <c>animation</c>（类型 <c>Animation*</c>），
    /// 按规则直译 PascalCase 后会分别得到 <c>Timer</c>/<c>Animation</c>——与它们自身的类型名完全相同。
    /// 沿用 output/EngineSettings.cs 中对同类问题的处理约定（"类型名与实例属性名不同"），
    /// 这里改为对字段本身重命名为 <see cref="EffectTimer"/>/<see cref="EffectAnimation"/>，
    /// 而不是修改已经在 output/Utils.cs 中确定下来的 <c>Timer</c> 类型名。
    ///
    /// 资源管理：<see cref="EffectAnimation"/> 是通过 <see cref="SharedResources.Anim"/>
    /// （<c>AnimationManager</c>）加载、按文件名做引用计数的独占资源，对应原始 <c>animation</c>
    /// 裸指针的手动 new/delete 语义，因此本类型实现 <see cref="IDisposable"/>，
    /// <see cref="Dispose"/> 对应原始析构函数 <c>~Effect()</c>（其函数体就是 <c>unloadAnimation()</c>）。
    /// </summary>
    public class Effect : IDisposable
    {
        public const int None = 0;
        public const int Damage = 1;
        public const int DamagePercent = 2;
        public const int Hpot = 3;
        public const int HpotPercent = 4;
        public const int Mpot = 5;
        public const int MpotPercent = 6;
        public const int Speed = 7;
        public const int AttackSpeed = 8;
        public const int ResistAll = 9;
        public const int Stun = 10;
        public const int Revive = 11;
        public const int Convert = 12;
        public const int Fear = 13;
        public const int DeathSentence = 14;
        public const int Shield = 15;
        public const int Heal = 16;
        public const int Knockback = 17;
        public const int TypeCount = 18;

        public string Id = "";
        public string Name = "";
        public int Icon;
        /// <summary>对应原始字段 <c>timer</c>（见类级注释中的重命名说明）。</summary>
        public Timer EffectTimer;
        public int Type;
        public float Magnitude;
        public float MagnitudeMax;
        public string AnimationName = "";
        /// <summary>对应原始字段 <c>animation</c>（见类级注释中的重命名说明）。</summary>
        public Animation? EffectAnimation;
        public bool IsFromItem;
        public int Trigger;
        public bool RenderAbove;
        public int PassiveId;
        public int SourceType;
        public bool GroupStack;
        public uint ColorMod;
        public byte AlphaMod;
        public string AttackSpeedAnim = "";
        public bool IsMultiplier;
        public bool IgnoreResist;
        public bool DamageIsTyped;
        public int DamageType;

        public Effect()
        {
            Id = "";
            Name = "";
            Icon = -1;
            EffectTimer = new Timer();
            Type = Effect.None;
            Magnitude = 0;
            MagnitudeMax = 0;
            AnimationName = "";
            EffectAnimation = null;
            IsFromItem = false;
            Trigger = -1;
            RenderAbove = false;
            PassiveId = 0;
            SourceType = Power.SourceTypeHero;
            GroupStack = false;
            ColorMod = new Color(255, 255, 255, 255).EncodeRgba();
            AlphaMod = 255;
            AttackSpeedAnim = "";
            IsMultiplier = false;
            IgnoreResist = false;
            DamageIsTyped = false;
            DamageType = 0;
        }

        /// <summary>
        /// 对应拷贝构造函数 <c>Effect(const Effect&amp; other)</c>：先将动画指针置空，
        /// 再委托给 <see cref="CopyFrom"/>（对应原始实现委托给 <c>operator=</c>）。
        /// </summary>
        public Effect(Effect other)
        {
            EffectTimer = new Timer();
            EffectAnimation = null;
            CopyFrom(other);
        }

        /// <summary>
        /// 对应赋值运算符 <c>Effect&amp; operator=(const Effect&amp; other)</c>。
        /// <see cref="EffectTimer"/>（Timer 是引用类型的类）按值语义逐字段复制 Current/Duration，
        /// 而不是直接复制引用，以保持与原始 C++ 值类型 <c>Timer</c> 成员逐位复制相同的独立性语义。
        /// </summary>
        public Effect CopyFrom(Effect other)
        {
            if (ReferenceEquals(this, other))
                return this;

            UnloadAnimation();
            AnimationName = other.AnimationName;
            LoadAnimation(AnimationName);
            if (EffectAnimation != null && other.EffectAnimation != null)
                EffectAnimation.SyncTo(other.EffectAnimation);

            Id = other.Id;
            Name = other.Name;
            Icon = other.Icon;
            EffectTimer = new Timer();
            EffectTimer.Duration = other.EffectTimer.Duration;
            EffectTimer.Current = other.EffectTimer.Current;
            Type = other.Type;
            Magnitude = other.Magnitude;
            MagnitudeMax = other.MagnitudeMax;
            IsFromItem = other.IsFromItem;
            Trigger = other.Trigger;
            RenderAbove = other.RenderAbove;
            PassiveId = other.PassiveId;
            SourceType = other.SourceType;
            GroupStack = other.GroupStack;
            ColorMod = other.ColorMod;
            AlphaMod = other.AlphaMod;
            AttackSpeedAnim = other.AttackSpeedAnim;
            IsMultiplier = other.IsMultiplier;
            IgnoreResist = other.IgnoreResist;
            DamageIsTyped = other.DamageIsTyped;
            DamageType = other.DamageType;

            return this;
        }

        /// <summary>对应析构函数 <c>~Effect()</c>：函数体即 <c>unloadAnimation()</c>。</summary>
        public void Dispose()
        {
            UnloadAnimation();
        }

        public void LoadAnimation(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                AnimationName = s;
                SharedResources.Anim!.IncreaseCount(AnimationName);
                AnimationSet? animationSet = SharedResources.Anim.GetAnimationSet(AnimationName);
                EffectAnimation = animationSet!.GetAnimation("");
            }
        }

        public void UnloadAnimation()
        {
            if (EffectAnimation != null)
            {
                if (!string.IsNullOrEmpty(AnimationName))
                    SharedResources.Anim!.DecreaseCount(AnimationName);
                EffectAnimation.Dispose();
                EffectAnimation = null;
            }
        }

        public static int GetTypeFromString(string typeStr, bool showError = true)
        {
            if (string.IsNullOrEmpty(typeStr)) return Effect.None;

            if (typeStr == "damage") return Effect.Damage;
            else if (typeStr == "damage_percent") return Effect.DamagePercent;
            else if (typeStr == "hpot") return Effect.Hpot;
            else if (typeStr == "hpot_percent") return Effect.HpotPercent;
            else if (typeStr == "mpot") return Effect.Mpot;
            else if (typeStr == "mpot_percent") return Effect.MpotPercent;
            else if (typeStr == "speed") return Effect.Speed;
            else if (typeStr == "attack_speed") return Effect.AttackSpeed;
            else if (typeStr == "resist_all") return Effect.ResistAll;
            else if (typeStr == "stun") return Effect.Stun;
            else if (typeStr == "revive") return Effect.Revive;
            else if (typeStr == "convert") return Effect.Convert;
            else if (typeStr == "fear") return Effect.Fear;
            else if (typeStr == "death_sentence") return Effect.DeathSentence;
            else if (typeStr == "shield") return Effect.Shield;
            else if (typeStr == "heal") return Effect.Heal;
            else if (typeStr == "knockback") return Effect.Knockback;

            // 历史注释：以下为已废弃的 effect 类型，仍需兼容旧数据
            else if (typeStr == "immunity") return Effect.ResistAll;
            else if (typeStr == "immunity_damage") return Effect.TypeCount + Stats.ResistDamageOverTime;
            else if (typeStr == "immunity_slow") return Effect.TypeCount + Stats.ResistSlow;
            else if (typeStr == "immunity_stun") return Effect.TypeCount + Stats.ResistStun;
            else if (typeStr == "immunity_knockback") return Effect.TypeCount + Stats.ResistKnockback;
            else if (typeStr == "immunity_damage_reflect") return Effect.TypeCount + Stats.ResistDamageReflect;
            else if (typeStr == "immunity_stat_debuff") return Effect.TypeCount + Stats.ResistStatDebuff;
            else if (typeStr == "immunity_hp_steal") return Effect.TypeCount + Stats.ResistHpSteal;
            else if (typeStr == "immunity_mp_steal") return Effect.TypeCount + Stats.ResistMpSteal;

            else
            {
                int offsetIndex = Effect.TypeCount;

                for (int i = 0; i < Stats.Count; ++i)
                {
                    if (typeStr == Stats.Key[i])
                    {
                        return offsetIndex + i;
                    }
                }
                offsetIndex += Stats.Count;

                for (int i = 0; i < SharedResources.Eset!.DamageTypes.Types.Count; ++i)
                {
                    if (typeStr == SharedResources.Eset.DamageTypes.Types[i].Min)
                    {
                        return offsetIndex + EngineSettings.DamageTypesSettings.IndexToMin(i);
                    }
                    else if (typeStr == SharedResources.Eset.DamageTypes.Types[i].Max)
                    {
                        return offsetIndex + EngineSettings.DamageTypesSettings.IndexToMax(i);
                    }
                    else if (typeStr == SharedResources.Eset.DamageTypes.Types[i].Resist)
                    {
                        return offsetIndex + EngineSettings.DamageTypesSettings.IndexToResist(i);
                    }
                }
                offsetIndex += SharedResources.Eset.DamageTypes.Count;

                for (int i = 0; i < SharedResources.Eset.ResourceStats.Stats.Count; ++i)
                {
                    for (int j = 0; j < EngineSettings.ResourceStatsSettings.StatCount; ++j)
                    {
                        if (typeStr == SharedResources.Eset.ResourceStats.Stats[i].Ids[j])
                        {
                            return offsetIndex + (i * EngineSettings.ResourceStatsSettings.StatCount) + j;
                        }
                    }
                }
                offsetIndex += SharedResources.Eset.ResourceStats.StatCountValue;

                for (int i = 0; i < SharedResources.Eset.ResourceStats.Stats.Count; ++i)
                {
                    for (int j = 0; j < EngineSettings.ResourceStatsSettings.EffectCount; ++j)
                    {
                        if (typeStr == SharedResources.Eset.ResourceStats.Stats[i].Ids[EngineSettings.ResourceStatsSettings.StatCount + j])
                        {
                            return offsetIndex + (i * EngineSettings.ResourceStatsSettings.EffectCount) + j;
                        }
                    }
                }
                offsetIndex += SharedResources.Eset.ResourceStats.EffectCountValue;

                for (int i = 0; i < SharedResources.Eset.PrimaryStats.Stats.Count; ++i)
                {
                    if (typeStr == SharedResources.Eset.PrimaryStats.Stats[i].Id)
                    {
                        return offsetIndex + i;
                    }
                }
            }

            if (showError)
                Utils.LogError("EffectManager: '%s' is not a valid effect type.", typeStr);

            return Effect.None;
        }

        public static bool TypeIsStat(int t)
        {
            int offsetIndex = Effect.TypeCount;
            return t >= offsetIndex && t < offsetIndex + Stats.Count;
        }

        public static bool TypeIsDmgMin(int t)
        {
            int offsetIndex = Effect.TypeCount + Stats.Count;
            return t >= offsetIndex && t < offsetIndex + SharedResources.Eset!.DamageTypes.Count && (t - offsetIndex) % 3 == 0;
        }

        public static bool TypeIsDmgMax(int t)
        {
            int offsetIndex = Effect.TypeCount + Stats.Count;
            return t >= offsetIndex && t < offsetIndex + SharedResources.Eset!.DamageTypes.Count && (t - offsetIndex) % 3 == 1;
        }

        public static bool TypeIsResist(int t)
        {
            int offsetIndex = Effect.TypeCount + Stats.Count;
            return t >= offsetIndex && t < offsetIndex + SharedResources.Eset!.DamageTypes.Count && (t - offsetIndex) % 3 == 2;
        }

        public static bool TypeIsResourceStat(int t)
        {
            int offsetIndex = Effect.TypeCount + Stats.Count + SharedResources.Eset!.DamageTypes.Count;
            return t >= offsetIndex && t < offsetIndex + SharedResources.Eset.ResourceStats.StatCountValue;
        }

        public static bool TypeIsResourceEffect(int t)
        {
            int offsetIndex = Effect.TypeCount + Stats.Count + (SharedResources.Eset!.DamageTypes.Count + SharedResources.Eset.ResourceStats.StatCountValue);
            return t >= offsetIndex && t < offsetIndex + SharedResources.Eset.ResourceStats.EffectCountValue;
        }

        public static bool TypeIsPrimary(int t)
        {
            int offsetIndex = Effect.TypeCount + Stats.Count + (SharedResources.Eset!.DamageTypes.Count + SharedResources.Eset.ResourceStats.StatEffectCountValue);
            return t >= offsetIndex && t < offsetIndex + SharedResources.Eset.PrimaryStats.Stats.Count;
        }

        public static bool TypeIsEffectResist(int t)
        {
            return t >= Effect.TypeCount + Stats.ResistDamageOverTime && t <= Effect.TypeCount + Stats.ResistMpSteal;
        }

        public static int GetStatFromType(int t)
        {
            return t - Effect.TypeCount;
        }

        public static int GetDmgFromType(int t)
        {
            return (t - Effect.TypeCount - Stats.Count) / 3;
        }

        public static int GetResourceStatFromType(int t)
        {
            int offsetIndex = (t - Effect.TypeCount - Stats.Count) - SharedResources.Eset!.DamageTypes.Count;

            if (offsetIndex > SharedResources.Eset.ResourceStats.StatCountValue)
            {
                // effect-only stat (e.g. heal)
                int effectOffsetIndex = offsetIndex - SharedResources.Eset.ResourceStats.StatCountValue;
                return effectOffsetIndex / EngineSettings.ResourceStatsSettings.EffectCount;
            }
            else
            {
                return offsetIndex / EngineSettings.ResourceStatsSettings.StatCount;
            }
        }

        public static int GetResourceStatSubIndexFromType(int t)
        {
            int offsetIndex = (t - Effect.TypeCount - Stats.Count) - SharedResources.Eset!.DamageTypes.Count;

            if (offsetIndex > SharedResources.Eset.ResourceStats.StatCountValue)
            {
                // effect-only stat (e.g. heal)
                int effectOffsetIndex = offsetIndex - SharedResources.Eset.ResourceStats.StatCountValue;
                return EngineSettings.ResourceStatsSettings.StatCount + (effectOffsetIndex % EngineSettings.ResourceStatsSettings.EffectCount);
            }
            else
            {
                return offsetIndex % EngineSettings.ResourceStatsSettings.StatCount;
            }
        }

        public static int GetPrimaryFromType(int t)
        {
            return (t - Effect.TypeCount - Stats.Count) - SharedResources.Eset!.DamageTypes.Count - SharedResources.Eset.ResourceStats.StatEffectCountValue;
        }

        /// <summary>handling of deprecated types（原始注释）。</summary>
        public static bool IsImmunityTypeString(string typeStr)
        {
            if (typeStr == "immunity") return true;
            else if (typeStr == "immunity_damage") return true;
            else if (typeStr == "immunity_slow") return true;
            else if (typeStr == "immunity_stun") return true;
            else if (typeStr == "immunity_knockback") return true;
            else if (typeStr == "immunity_damage_reflect") return true;
            else if (typeStr == "immunity_stat_debuff") return true;
            else if (typeStr == "immunity_hp_steal") return true;
            else if (typeStr == "immunity_mp_steal") return true;
            else return false;
        }
    }

    /// <summary>
    /// EffectDef
    ///
    /// 效果的静态定义数据（对应原始 <c>class EffectDef</c>），来自 powers/effects.txt 等配置，
    /// 用于在 <see cref="EffectManager.AddEffect"/> 时构造具体的 <see cref="Effect"/> 实例。
    /// 字段 <c>AnimationName</c> 对应原始 <c>animation</c>（动画文件名字符串），
    /// 按类级注释中同样的理由重命名以避免与 <c>Animation</c> 类型同名。
    /// </summary>
    public class EffectDef
    {
        public string Id = "";
        public int Type;
        public string Name = "";
        public int Icon;
        public string AnimationName = "";
        public bool CanStack;
        public int MaxStacks;
        public bool GroupStack;
        public bool RenderAbove;
        public Color ColorMod;
        public byte AlphaMod;
        public string AttackSpeedAnim = "";
        public bool IgnoreResist;
        public bool DamageIsTyped;
        public int DamageType;

        /// <summary>handling of deprecated types（原始注释）。</summary>
        public bool IsImmunityType;

        public EffectDef()
        {
            Id = "";
            Type = Effect.None;
            Name = "";
            Icon = -1;
            AnimationName = "";
            CanStack = true;
            MaxStacks = -1;
            GroupStack = false;
            RenderAbove = false;
            ColorMod = new Color(255, 255, 255, 255);
            AlphaMod = 255;
            AttackSpeedAnim = "";
            IgnoreResist = false;
            DamageIsTyped = false;
            DamageType = 0;
            IsImmunityType = false;
        }
    }

    /// <summary>
    /// EffectParams
    ///
    /// 调用 <see cref="EffectManager.AddEffect"/> 时传入的运行时参数（对应原始 <c>class EffectParams</c>）。
    /// </summary>
    public class EffectParams
    {
        public bool IsFromItem;
        public bool IsMultiplier;
        public int Duration;
        public int SourceType;
        public float Magnitude;
        public PowerID PowerId;

        public EffectParams()
        {
            IsFromItem = false;
            IsMultiplier = false;
            Duration = 0;
            SourceType = Power.SourceTypeNeutral;
            Magnitude = 0;
            PowerId = EffectManager.NoPower;
        }
    }

    /// <summary>
    /// EffectManager
    ///
    /// 持有并管理一组"危害"（正在生效的攻击、法术等效果，对应原始注释 "hazards"）集合，
    /// 并提供分组操作（对应原始 <c>class EffectManager</c>）。
    ///
    /// 资源管理（需人工复核，详见转换报告）：原始 <c>~EffectManager()</c> 函数体为空，
    /// 但其成员 <c>std::vector&lt;Effect&gt; effect_list</c> 在向量自身被析构时，会隐式地对每个
    /// 元素调用 <c>~Effect()</c>（释放动画引用计数）。C# 的 <see cref="List{T}"/> 不会在自身被回收时
    /// 对元素调用 <see cref="IDisposable.Dispose"/>，因此本类型显式实现 <see cref="IDisposable"/>，
    /// 在 <see cref="Dispose"/> 中逐一释放 <see cref="EffectList"/> 中的元素，以復现原始隐式析构行为。
    /// </summary>
    public class EffectManager : IDisposable
    {
        public List<Effect> EffectList = new List<Effect>();

        // 历史注释（原始头文件）：可考虑将以下字段重命名为 *_per_second，或改为数组
        public float Damage;
        public float DamagePercent;
        public float Hpot;
        public float HpotPercent;
        public float Mpot;
        public float MpotPercent;
        public List<float> ResourceOt = new List<float>();
        public List<float> ResourceOtPercent = new List<float>();

        public float Speed;
        public bool Stun;
        public bool Revive;
        public bool Convert;
        public bool DeathSentence;
        public bool Fear;
        public float KnockbackSpeed;
        public bool DamageIsTyped;

        public List<float> Bonus = new List<float>();
        public List<float> BonusMultiplier = new List<float>();
        public List<int> BonusPrimary = new List<int>();
        public List<float> TypedDamage = new ();
        public List<float> TypedDamagePercent = new ();

        // 历史注释（原始头文件）：可考虑改为数组
        public bool TriggeredOthers;
        public bool TriggeredBlock;
        public bool TriggeredHit;
        public bool TriggeredHalfdeath;
        public bool TriggeredJoincombat;
        public bool TriggeredDeath;
        public bool TriggeredActivePower;

        public bool RefreshStats;

        public const int NoPower = 0;

        public EffectManager()
        {
            ResourceOt = new List<float>(new float[SharedResources.Eset!.ResourceStats.Stats.Count]);
            ResourceOtPercent = new List<float>(new float[SharedResources.Eset.ResourceStats.Stats.Count]);
            Bonus = new List<float>(new float[Stats.Count + SharedResources.Eset.DamageTypes.Count + SharedResources.Eset.ResourceStats.StatEffectCountValue]);
            BonusMultiplier = new List<float>(Bonus.Count);
            for (int i = 0; i < Bonus.Count; ++i)
            {
                BonusMultiplier.Add(1);
            }
            BonusPrimary = new List<int>(new int[SharedResources.Eset.PrimaryStats.Stats.Count]);
            TypedDamage = new List<float>(new float[SharedResources.Eset.DamageTypes.Count]);
            TypedDamagePercent = new List<float>(new float[SharedResources.Eset.DamageTypes.Count]);
            TriggeredOthers = false;
            TriggeredBlock = false;
            TriggeredHit = false;
            TriggeredHalfdeath = false;
            TriggeredJoincombat = false;
            TriggeredDeath = false;
            TriggeredActivePower = false;
            RefreshStats = false;

            ClearStatus();
        }

        /// <summary>
        /// 对应析构函数 <c>~EffectManager()</c>（原始函数体为空）。这里额外释放
        /// <see cref="EffectList"/> 中每个 <see cref="Effect"/> 持有的动画引用，
        /// 以复现原始 <c>std::vector&lt;Effect&gt;</c> 成员被隐式析构时的资源释放效果
        /// （详见类级注释"资源管理"）。
        /// </summary>
        public void Dispose()
        {
            for (int i = EffectList.Count - 1; i >= 0; i--)
            {
                EffectList[i].Dispose();
            }
            EffectList.Clear();
        }

        private void ClearStatus()
        {
            Damage = 0;
            DamagePercent = 0;
            Hpot = 0;
            HpotPercent = 0;
            Mpot = 0;
            MpotPercent = 0;
            Speed = 100;
            Stun = false;
            Revive = false;
            Convert = false;
            DeathSentence = false;
            Fear = false;
            KnockbackSpeed = 0;

            for (int i = 0; i < Bonus.Count; ++i)
            {
                Bonus[i] = 0;
                BonusMultiplier[i] = 1;
            }

            for (int i = 0; i < BonusPrimary.Count; ++i)
            {
                BonusPrimary[i] = 0;
            }

            for (int i = 0; i < ResourceOt.Count; ++i)
            {
                ResourceOt[i] = 0;
                ResourceOtPercent[i] = 0;
            }
            
            for (int i = 0; i < TypedDamage.Count; ++i)
            {
                TypedDamage[i] = 0;
                TypedDamagePercent[i] = 0;
            }
        }

        public void Logic()
        {
            ClearStatus();

            int offsetResourceEffects = Effect.TypeCount + Stats.Count + SharedResources.Eset!.DamageTypes.Count + SharedResources.Eset.ResourceStats.StatCountValue;
            int offsetPrimaryStats = offsetResourceEffects + SharedResources.Eset.ResourceStats.EffectCountValue;

            for (int i = 0; i < EffectList.Count; ++i)
            {
                Effect ei = EffectList[i];

                // @CLASS EffectManager|Description of "type" in powers/effects.txt
                // expire timed effects and total up magnitudes of active effects
                if (ei.EffectTimer.Duration > 0)
                {
                    if (ei.EffectTimer.IsEnd())
                    {
                        //death sentence is only applied at the end of the timer
                        // @TYPE death_sentence|Causes sudden death at the end of the effect duration.
                        if (ei.Type == Effect.DeathSentence) DeathSentence = true;
                        RemoveEffect(i);
                        i--;
                        continue;
                    }
                }

                bool doTimedEffect = ei.EffectTimer.IsWholeSecond() || (ei.EffectTimer.Duration < SharedResources.Settings!.MaxFramesPerSec && ei.EffectTimer.IsBegin());

                // @TYPE damage|Damage per second
                if (ei.Type == Effect.Damage && doTimedEffect)
                {
                    if (!ei.DamageIsTyped)
                    {
                        Damage += ei.Magnitude;
                    }
                    else
                    {
                        TypedDamage[ei.DamageType] += ei.Magnitude;
                    }
                }
                // @TYPE damage_percent|Damage per second (percentage of max HP)
                else if (ei.Type == Effect.DamagePercent && doTimedEffect)
                {
                    if (!ei.DamageIsTyped)
                    {
                        DamagePercent += ei.Magnitude;
                    }
                    else
                    {
                        TypedDamagePercent[ei.DamageType] += ei.Magnitude;
                    }
                }
                // @TYPE hpot|HP restored per second
                else if (ei.Type == Effect.Hpot && doTimedEffect) Hpot += ei.Magnitude;
                // @TYPE hpot_percent|HP restored per second (percentage of max HP)
                else if (ei.Type == Effect.HpotPercent && doTimedEffect) HpotPercent += ei.Magnitude;
                // @TYPE mpot|MP restored per second
                else if (ei.Type == Effect.Mpot && doTimedEffect) Mpot += ei.Magnitude;
                // @TYPE mpot_percent|MP restored per second (percentage of max MP)
                else if (ei.Type == Effect.MpotPercent && doTimedEffect) MpotPercent += ei.Magnitude;
                // @TYPE speed|Changes movement speed. A magnitude of 100 is 100% speed (aka normal speed).
                else if (ei.Type == Effect.Speed) Speed = (ei.Magnitude * Speed) / 100f;
                // @TYPE attack_speed|Changes attack speed. A magnitude of 100 is 100% speed (aka normal speed).
                // attack speed is calculated when GetAttackSpeed() is called

                // @TYPE resist_all|Applies a bonus to all of the non-elemental resistance stats.
                else if (ei.Type == Effect.ResistAll)
                {
                    Bonus[Stats.ResistDamageOverTime] += ei.Magnitude;
                    Bonus[Stats.ResistSlow] += ei.Magnitude;
                    Bonus[Stats.ResistStun] += ei.Magnitude;
                    Bonus[Stats.ResistKnockback] += ei.Magnitude;
                    Bonus[Stats.ResistDamageReflect] += ei.Magnitude;
                    Bonus[Stats.ResistStatDebuff] += ei.Magnitude;
                    Bonus[Stats.ResistHpSteal] += ei.Magnitude;
                    Bonus[Stats.ResistMpSteal] += ei.Magnitude;

                    for (int j = 0; j < SharedResources.Eset.ResourceStats.Stats.Count; ++j)
                    {
                        int resistStealIndex = Stats.Count + SharedResources.Eset.DamageTypes.Count;
                        resistStealIndex += (j * EngineSettings.ResourceStatsSettings.StatEffectCount) + EngineSettings.ResourceStatsSettings.StatResistSteal;
                        Bonus[resistStealIndex] += ei.Magnitude;
                    }
                }

                // @TYPE stun|Can't move or attack. Being attacked breaks stun.
                else if (ei.Type == Effect.Stun) Stun = true;
                // @TYPE revive|Revives the player. Typically attached to a power that triggers when the player dies.
                else if (ei.Type == Effect.Revive) Revive = true;
                // @TYPE convert|Causes an enemy or an ally to switch allegiance
                else if (ei.Type == Effect.Convert) Convert = true;
                // @TYPE fear|Causes enemies to run away
                else if (ei.Type == Effect.Fear) Fear = true;
                // @TYPE knockback|Pushes the target away from the source caster. Speed is the given value divided by the framerate cap.
                else if (ei.Type == Effect.Knockback) KnockbackSpeed = ei.Magnitude / SharedResources.Settings.MaxFramesPerSec;

                // @TYPE ${STAT}|Increases ${STAT}, where ${STAT} is any valid stat_id.
                else if (ei.Type >= Effect.TypeCount && ei.Type < offsetResourceEffects)
                {
                    if (ei.IsMultiplier)
                        BonusMultiplier[ei.Type - Effect.TypeCount] *= ei.Magnitude;
                    else
                        Bonus[ei.Type - Effect.TypeCount] += ei.Magnitude;
                }
                else if (ei.Type >= offsetResourceEffects && ei.Type < offsetPrimaryStats && doTimedEffect)
                {
                    int resourceIndex = Effect.GetResourceStatFromType(ei.Type);
                    int resourceSubIndex = Effect.GetResourceStatSubIndexFromType(ei.Type);

                    if (resourceSubIndex == EngineSettings.ResourceStatsSettings.StatHeal)
                    {
                        ResourceOt[resourceIndex] += ei.Magnitude;
                    }
                    else if (resourceSubIndex == EngineSettings.ResourceStatsSettings.StatHealPercent)
                    {
                        ResourceOtPercent[resourceIndex] += ei.Magnitude;
                    }
                }
                // @TYPE ${PRIMARYSTAT}|Increases ${PRIMARYSTAT}, where ${PRIMARYSTAT} is any of the primary stats defined in engine/primary_stats.txt. Example: physical
                else if (ei.Type >= offsetPrimaryStats)
                {
                    BonusPrimary[ei.Type - offsetPrimaryStats] += (int)ei.Magnitude;
                }

                ei.EffectTimer.Tick();

                // expire shield effects
                if (ei.MagnitudeMax > 0 && ei.Magnitude == 0)
                {
                    // @TYPE shield|Create a damage absorbing barrier based on Mental damage stat. Duration is ignored.
                    if (ei.Type == Effect.Shield)
                    {
                        RemoveEffect(i);
                        i--;
                        continue;
                    }
                }
                // expire effects based on animations
                if ((ei.EffectAnimation != null && ei.EffectAnimation.IsLastFrame()) || ei.EffectAnimation == null)
                {
                    // @TYPE heal|Restore HP based on Mental damage stat.
                    if (ei.Type == Effect.Heal)
                    {
                        RemoveEffect(i);
                        i--;
                        continue;
                    }
                }

                // animate
                if (ei.EffectAnimation != null)
                {
                    if (!ei.EffectAnimation.IsCompleted())
                        ei.EffectAnimation.AdvanceFrame();
                }
            }

            TriggeredActivePower = false;
        }

        public void AddEffect(StatBlock? stats, EffectDef effect, EffectParams @params)
        {
            RefreshStats = true;

            // if we're already immune, don't add negative effects
            if (stats != null && !effect.IgnoreResist)
            {
                if ((effect.Type == Effect.Damage || effect.Type == Effect.DamagePercent) && MathUtils.PercentChanceF(stats.Get(Stats.ResistDamageOverTime)))
                {
                    SharedResources.Comb!.AddString(SharedResources.Msg!.Get("Resist"), stats.Pos, CombatText.MsgMiss);
                    return;
                }
                else if (effect.Type == Effect.Speed && @params.Magnitude < 100 && MathUtils.PercentChanceF(stats.Get(Stats.ResistSlow)))
                {
                    SharedResources.Comb!.AddString(SharedResources.Msg!.Get("Resist"), stats.Pos, CombatText.MsgMiss);
                    return;
                }
                else if (effect.Type == Effect.Stun && MathUtils.PercentChanceF(stats.Get(Stats.ResistStun)))
                {
                    SharedResources.Comb!.AddString(SharedResources.Msg!.Get("Resist"), stats.Pos, CombatText.MsgMiss);
                    return;
                }
                else if (effect.Type == Effect.Knockback && MathUtils.PercentChanceF(stats.Get(Stats.ResistKnockback)))
                {
                    SharedResources.Comb!.AddString(SharedResources.Msg!.Get("Resist"), stats.Pos, CombatText.MsgMiss);
                    return;
                }
                else if (effect.Type > Effect.TypeCount && @params.Magnitude < 0 && MathUtils.PercentChanceF(stats.Get(Stats.ResistStatDebuff)))
                {
                    SharedResources.Comb!.AddString(SharedResources.Msg!.Get("Resist"), stats.Pos, CombatText.MsgMiss);
                    return;
                }
            }
            else
            {
                Utils.LogError("EffectManager: No statblock detected when adding effect");
            }

            // only allow one knockback effect at a time
            if (effect.Type == Effect.Knockback && KnockbackSpeed != 0)
                return;

            bool insertEffect = false;
            int insertPos = 0;
            int stacksApplied = 0;
            int trigger = -1;
            int passiveId = 0;

            if (SharedGameResources.Powers!.IsValid(@params.PowerId))
            {
                Power effectPower = SharedGameResources.Powers.Powers[@params.PowerId];
                trigger = effectPower.PassiveTrigger;
                passiveId = effectPower.Passive ? @params.PowerId : 0;
            }

            for (int i = EffectList.Count; i > 0; i--)
            {
                Effect ei = EffectList[i - 1];

                // while checking only id would be sufficient, it is a slow string compare
                // so we check the type first, which is an int compare, before taking the slow path
                if (ei.Type == effect.Type && ei.Id == effect.Id)
                {
                    // 历史注释：完全移除这一处逻辑是否会破坏向后兼容？
                    if (!SharedResources.Eset!.Misc.PassiveTriggerEffectStacking && trigger > -1 && ei.Trigger == trigger)
                        return; // trigger effects can only be cast once per trigger

                    if (!effect.CanStack)
                    {
                        if ((uint)@params.Duration < ei.EffectTimer.Current && @params.Magnitude == ei.MagnitudeMax)
                        {
                            // Duration is shorter than time remaining for existing effect with same magnitude, so don't bother adding this one
                            // 历史注释：如果新效果的 magnitude 不同呢？也许在那种情况下总是替换旧的更合理
                            return;
                        }

                        RemoveEffect(i - 1);
                    }
                    else
                    {
                        if (effect.Type == Effect.Shield && effect.GroupStack)
                        {
                            ei.Magnitude += @params.Magnitude;

                            if (effect.MaxStacks == -1
                                || (@params.Magnitude != 0 && (int)(ei.MagnitudeMax / @params.Magnitude) < effect.MaxStacks))
                            {
                                ei.MagnitudeMax += @params.Magnitude;
                            }

                            if (ei.Magnitude > ei.MagnitudeMax)
                            {
                                ei.Magnitude = ei.MagnitudeMax;
                            }

                            return;
                        }

                        if (insertEffect == false && effect.MaxStacks != -1)
                        {
                            // to keep stackable effects together, they are inserted after the most recent matching effect
                            // otherwise, they are added to the end of the effect list
                            insertEffect = true;
                            insertPos = i;
                        }

                        stacksApplied++;
                    }
                }
            }

            // if we're adding a debuff resistance effect, remove applicable negative effects
            for (int i = Stats.ResistDamageOverTime; i <= Stats.ResistStatDebuff; ++i)
            {
                float resistChance = stats != null ? stats.Get(i) + @params.Magnitude : @params.Magnitude;

                if ((effect.Type == Effect.ResistAll || effect.Type == Effect.TypeCount + i) && MathUtils.PercentChanceF(resistChance))
                {
                    ClearNegativeEffects(Effect.TypeCount + i);
                }
            }

            Effect e = new Effect();

            e.Id = effect.Id;
            e.Name = effect.Name;
            e.Icon = effect.Icon;
            e.Type = effect.Type;
            e.RenderAbove = effect.RenderAbove;
            e.GroupStack = effect.GroupStack;
            e.ColorMod = effect.ColorMod.EncodeRgba();
            e.AlphaMod = effect.AlphaMod;
            e.AttackSpeedAnim = effect.AttackSpeedAnim;
            e.IgnoreResist = effect.IgnoreResist;
            e.DamageIsTyped= effect.DamageIsTyped;
            e.DamageType = effect.DamageType;

            if (!string.IsNullOrEmpty(effect.AnimationName))
            {
                e.LoadAnimation(effect.AnimationName);
            }

            e.EffectTimer.Duration = (uint)@params.Duration;
            e.Magnitude = e.MagnitudeMax = @params.Magnitude;
            e.IsFromItem = @params.IsFromItem;
            e.IsMultiplier = @params.IsMultiplier;
            e.Trigger = trigger;
            e.PassiveId = passiveId;
            e.SourceType = @params.SourceType;

            if (insertEffect)
            {
                if (effect.MaxStacks != -1 && stacksApplied >= effect.MaxStacks)
                {
                    //Remove the oldest effect of the type
                    RemoveEffect(insertPos - stacksApplied);

                    //All elements have shifted to left
                    insertPos--;
                }

                EffectList.Insert(insertPos, new Effect(e));
            }
            else
            {
                EffectList.Add(new Effect(e));
            }

            e.Dispose();
        }

        private void RemoveEffect(int id)
        {
            EffectList[id].Dispose();
            EffectList.RemoveAt(id);
            RefreshStats = true;
        }

        public void RemoveEffectType(int type)
        {
            for (int i = EffectList.Count; i > 0; i--)
            {
                if (EffectList[i - 1].Type == type) RemoveEffect(i - 1);
            }
        }

        public void RemoveEffectPassive(int id)
        {
            for (int i = EffectList.Count; i > 0; i--)
            {
                if (EffectList[i - 1].PassiveId == id) RemoveEffect(i - 1);
            }
        }

        public void RemoveEffectID(List<(string Id, int Count)> removeEffects)
        {
            for (int i = 0; i < removeEffects.Count; i++)
            {
                int count = removeEffects[i].Count;
                bool removeAll = count == 0 ? true : false;

                for (int j = EffectList.Count; j > 0; j--)
                {
                    if (!removeAll && count <= 0)
                        break;

                    if (EffectList[j - 1].Id == removeEffects[i].Id)
                    {
                        RemoveEffect(j - 1);
                        count--;
                    }
                }
            }
        }

        public void ClearEffects()
        {
            for (int i = EffectList.Count; i > 0; i--)
            {
                RemoveEffect(i - 1);
            }

            ClearStatus();

            // clear triggers
            TriggeredOthers = TriggeredBlock = TriggeredHit = TriggeredHalfdeath = TriggeredJoincombat = TriggeredDeath = TriggeredActivePower = false;
        }

        public void ClearNegativeEffects(int type)
        {
            bool removeDamage = (type == Effect.ResistAll || type == Effect.TypeCount + Stats.ResistDamageOverTime);
            bool removeSlow = (type == Effect.ResistAll || type == Effect.TypeCount + Stats.ResistSlow);
            bool removeStun = (type == Effect.ResistAll || type == Effect.TypeCount + Stats.ResistStun);
            bool removeKnockback = (type == Effect.ResistAll || type == Effect.TypeCount + Stats.ResistKnockback);
            bool removeStatDebuff = (type == Effect.ResistAll || type == Effect.TypeCount + Stats.ResistStatDebuff);

            for (int i = EffectList.Count; i > 0; i--)
            {
                int ei = i - 1;

                if (EffectList[ei].IgnoreResist)
                    continue;

                if (removeDamage && EffectList[ei].Type == Effect.Damage)
                    RemoveEffect(ei);
                else if (removeDamage && EffectList[ei].Type == Effect.DamagePercent)
                    RemoveEffect(ei);
                else if (removeSlow && EffectList[ei].Type == Effect.Speed && EffectList[ei].MagnitudeMax < 100)
                    RemoveEffect(ei);
                else if (removeStun && EffectList[ei].Type == Effect.Stun)
                    RemoveEffect(ei);
                else if (removeKnockback && EffectList[ei].Type == Effect.Knockback)
                    RemoveEffect(ei);
                else if (removeStatDebuff && EffectList[ei].Type > Effect.TypeCount && EffectList[ei].MagnitudeMax < 0)
                    RemoveEffect(ei);
            }
        }

        public void ClearItemEffects()
        {
            for (int i = EffectList.Count; i > 0; i--)
            {
                if (EffectList[i - 1].IsFromItem) RemoveEffect(i - 1);
            }
        }

        public void ClearTriggerEffects(int trigger)
        {
            for (int i = EffectList.Count; i > 0; i--)
            {
                if (EffectList[i - 1].Trigger > -1 && EffectList[i - 1].Trigger == trigger) RemoveEffect(i - 1);
            }
        }

        public float DamageShields(float dmg)
        {
            float overDmg = dmg;

            for (int i = 0; i < EffectList.Count; i++)
            {
                if (EffectList[i].MagnitudeMax > 0 && EffectList[i].Type == Effect.Shield)
                {
                    EffectList[i].Magnitude -= overDmg;
                    if (EffectList[i].Magnitude < 0)
                    {
                        overDmg = MathF.Abs(EffectList[i].Magnitude);
                        EffectList[i].Magnitude = 0;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            return overDmg;
        }

        public bool IsDebuffed()
        {
            for (int i = EffectList.Count; i > 0; i--)
            {
                if (EffectList[i - 1].Type == Effect.Damage) return true;
                else if (EffectList[i - 1].Type == Effect.DamagePercent) return true;
                else if (EffectList[i - 1].Type == Effect.Speed && EffectList[i - 1].MagnitudeMax < 100) return true;
                else if (EffectList[i - 1].Type == Effect.Stun) return true;
                else if (EffectList[i - 1].Type == Effect.Knockback) return true;
                else if (EffectList[i - 1].Type > Effect.TypeCount && EffectList[i - 1].MagnitudeMax < 0) return true;
            }
            return false;
        }

        public void GetCurrentColor(ref Color colorMod)
        {
            uint defaultColor = colorMod.EncodeRgba();
            uint noColor = new Color(255, 255, 255, 255).EncodeRgba();

            for (int i = EffectList.Count; i > 0; i--)
            {
                Effect ei = EffectList[i - 1];
                if (ei.ColorMod == noColor)
                    continue;

                if (ei.ColorMod != defaultColor)
                {
                    colorMod.DecodeRgba(ei.ColorMod);
                    return;
                }
            }
        }

        public void GetCurrentAlpha(ref byte alphaMod)
        {
            byte defaultAlpha = alphaMod;
            byte noAlpha = 255;

            for (int i = EffectList.Count; i > 0; i--)
            {
                Effect ei = EffectList[i - 1];
                if (ei.AlphaMod == noAlpha)
                    continue;

                if (ei.AlphaMod != defaultAlpha)
                {
                    alphaMod = ei.AlphaMod;
                    return;
                }
            }
        }

        public bool HasEffect(string id, int reqCount)
        {
            if (reqCount <= 0)
                return false;

            int count = 0;

            for (int i = EffectList.Count; i > 0; i--)
            {
                if (EffectList[i - 1].Id == id)
                    count++;
            }

            return count >= reqCount;
        }

        public float GetAttackSpeed(string animName)
        {
            float attackSpeed = 100;

            for (int i = 0; i < EffectList.Count; ++i)
            {
                if (EffectList[i].Type != Effect.AttackSpeed)
                    continue;

                if (string.IsNullOrEmpty(EffectList[i].AttackSpeedAnim) || EffectList[i].AttackSpeedAnim == animName)
                {
                    attackSpeed = (EffectList[i].Magnitude * attackSpeed) / 100.0f;
                }
            }

            return attackSpeed;
        }

        public int GetDamageSourceType(int dmgMode)
        {
            if (!(dmgMode == Effect.Damage || dmgMode == Effect.DamagePercent))
                return -1;

            int sourceType = Power.SourceTypeNeutral;

            for (int i = 0; i < EffectList.Count; ++i)
            {
                Effect ei = EffectList[i];
                if (ei.Type == dmgMode)
                {
                    // anything other than ally source type take precedence, so we can return early
                    if (ei.SourceType != Power.SourceTypeAlly)
                        return ei.SourceType;

                    sourceType = ei.SourceType;
                }
            }

            return sourceType;
        }
    }
}
