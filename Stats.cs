// <自动生成> 对应 C++ 源文件：Stats.h + Stats.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。

namespace FlareEngine
{
    /// <summary>
    /// 对应 C++ <c>namespace Stats</c> 中的 <c>enum STAT</c>。
    /// </summary>
    public enum Stat
    {
        HpMax = 0,
        HpRegen,
        MpMax,
        MpRegen,
        Accuracy,
        Avoidance,
        AbsMin,
        AbsMax,
        Crit,
        XpGain,
        CurrencyFind,
        ItemFind,
        Stealth,
        Poise,
        Reflect,
        ReturnDamage,
        HpSteal,
        MpSteal,
        // all the below stats are hidden by default in MenuCharacter
        ResistDamageOverTime,
        ResistSlow,
        ResistStun,
        ResistKnockback,
        ResistStatDebuff,
        ResistDamageReflect,
        ResistHpSteal,
        ResistMpSteal,
        Count
    }

    /// <summary>
    /// 对应 C++ <c>namespace Stats</c> 中的 <c>enum STAT_CATEGORY</c>。
    /// </summary>
    public enum StatCategory
    {
        CategoryCore = 0,
        CategoryOffense,
        CategoryDefense,
        CategoryMisc,
    }

    /// <summary>
    /// Stats
    ///
    /// 对应 C++ <c>namespace Stats</c>：命名空间级全局数组与 <c>init()</c> 函数转为
    /// 静态类中的静态字段与静态方法。数组元素在 <see cref="Init"/> 中按原始顺序填充。
    /// </summary>
    public static class Stats
    {
        public const int Count = (int)Stat.Count;

        public const int HpMax = (int)Stat.HpMax;
        public const int HpRegen = (int)Stat.HpRegen;
        public const int MpMax = (int)Stat.MpMax;
        public const int MpRegen = (int)Stat.MpRegen;
        public const int Accuracy = (int)Stat.Accuracy;
        public const int Avoidance = (int)Stat.Avoidance;
        public const int AbsMin = (int)Stat.AbsMin;
        public const int AbsMax = (int)Stat.AbsMax;
        public const int Crit = (int)Stat.Crit;
        public const int XpGain = (int)Stat.XpGain;
        public const int CurrencyFind = (int)Stat.CurrencyFind;
        public const int ItemFind = (int)Stat.ItemFind;
        public const int Stealth = (int)Stat.Stealth;
        public const int Poise = (int)Stat.Poise;
        public const int Reflect = (int)Stat.Reflect;
        public const int ReturnDamage = (int)Stat.ReturnDamage;
        public const int HpSteal = (int)Stat.HpSteal;
        public const int MpSteal = (int)Stat.MpSteal;
        public const int ResistDamageOverTime = (int)Stat.ResistDamageOverTime;
        public const int ResistSlow = (int)Stat.ResistSlow;
        public const int ResistStun = (int)Stat.ResistStun;
        public const int ResistKnockback = (int)Stat.ResistKnockback;
        public const int ResistStatDebuff = (int)Stat.ResistStatDebuff;
        public const int ResistDamageReflect = (int)Stat.ResistDamageReflect;
        public const int ResistHpSteal = (int)Stat.ResistHpSteal;
        public const int ResistMpSteal = (int)Stat.ResistMpSteal;

        /// <summary>对应 C++ <c>extern std::string KEY[COUNT]</c>。</summary>
        public static string[] Key = new string[Count];

        /// <summary>对应 C++ <c>extern std::string NAME[COUNT]</c>。</summary>
        public static string[] Name = new string[Count];

        /// <summary>对应 C++ <c>extern std::string DESC[COUNT]</c>。</summary>
        public static string[] Desc = new string[Count];

        /// <summary>对应 C++ <c>extern bool PERCENT[COUNT]</c>。</summary>
        public static bool[] Percent = new bool[Count];

        /// <summary>对应 C++ <c>extern short CATEGORY[COUNT]</c>。</summary>
        public static short[] Category = new short[Count];

        // KEY values aren't visible in-game, but they are used for parsing config files like engine/stats.txt
        // NAME values are the translated strings visible in the Character menu and item tooltips
        // DESC values are the translated descriptions of stats visible in Character menu tooltips
        // PERCENT is used to determine if we should treat the value as a percentage when displaying it (i.e. use %)
        /// <summary>对应 C++ <c>void Stats::init()</c>。</summary>
        public static void Init()
        {
            // @CLASS Stats|Description of the base stats which may be used wherever a stat_id is required.

            // @TYPE hp|Hit points
            Key[HpMax] = "hp";
            Name[HpMax] = SharedResources.Msg!.Get("Max HP");
            Desc[HpMax] = SharedResources.Msg!.Get("Total amount of HP.");
            Percent[HpMax] = false;
            Category[HpMax] = (short)StatCategory.CategoryCore;

            // @TYPE hp_regen|HP restored per minute
            Key[HpRegen] = "hp_regen";
            Name[HpRegen] = SharedResources.Msg!.Get("HP Regen");
            Desc[HpRegen] = SharedResources.Msg!.Get("Ticks of HP regen per minute.");
            Percent[HpRegen] = false;
            Category[HpRegen] = (short)StatCategory.CategoryCore;

            // @TYPE mp|Magic points
            Key[MpMax] = "mp";
            Name[MpMax] = SharedResources.Msg!.Get("Max MP");
            Desc[MpMax] = SharedResources.Msg!.Get("Total amount of MP.");
            Percent[MpMax] = false;
            Category[MpMax] = (short)StatCategory.CategoryCore;

            // @TYPE mp_regen|MP restored per minute
            Key[MpRegen] = "mp_regen";
            Name[MpRegen] = SharedResources.Msg!.Get("MP Regen");
            Desc[MpRegen] = SharedResources.Msg!.Get("Ticks of MP regen per minute.");
            Percent[MpRegen] = false;
            Category[MpRegen] = (short)StatCategory.CategoryCore;

            // @TYPE accuracy|Accuracy %. Higher values mean less likely to miss.
            Key[Accuracy] = "accuracy";
            Name[Accuracy] = SharedResources.Msg!.Get("Accuracy");
            Desc[Accuracy] = SharedResources.Msg!.Get("Accuracy rating. The enemy's Avoidance rating is subtracted from this value to calculate your likeliness to land a direct hit.");
            Percent[Accuracy] = true;
            Category[Accuracy] = (short)StatCategory.CategoryOffense;

            // @TYPE avoidance|Avoidance %. Higher values means more likely to not get hit.
            Key[Avoidance] = "avoidance";
            Name[Avoidance] = SharedResources.Msg!.Get("Avoidance");
            Desc[Avoidance] = SharedResources.Msg!.Get("Avoidance rating. This value is subtracted from the enemy's Accuracy rating to calculate their likeliness to land a direct hit.");
            Percent[Avoidance] = true;
            Category[Avoidance] = (short)StatCategory.CategoryDefense;

            // @TYPE absorb_min|Minimum damage absorption
            Key[AbsMin] = "absorb_min";
            Name[AbsMin] = SharedResources.Msg!.GetV("%s (Min.)", SharedResources.Msg!.Get("Absorb"));
            Desc[AbsMin] = SharedResources.Msg!.Get("Reduces the amount of damage taken.");
            Percent[AbsMin] = false;
            Category[AbsMin] = (short)StatCategory.CategoryDefense;

            // @TYPE absorb_max|Maximum damage absorption
            Key[AbsMax] = "absorb_max";
            Name[AbsMax] = SharedResources.Msg!.GetV("%s (Max.)", SharedResources.Msg!.Get("Absorb"));
            Desc[AbsMax] = SharedResources.Msg!.Get("Reduces the amount of damage taken.");
            Percent[AbsMax] = false;
            Category[AbsMax] = (short)StatCategory.CategoryDefense;

            // @TYPE crit|Critical hit chance %
            Key[Crit] = "crit";
            Name[Crit] = SharedResources.Msg!.Get("Critical Hit Chance");
            Desc[Crit] = SharedResources.Msg!.Get("Chance for an attack to do extra damage.");
            Percent[Crit] = true;
            Category[Crit] = (short)StatCategory.CategoryOffense;

            // @TYPE xp_gain|Percentage boost to the amount of experience points gained per kill.
            Key[XpGain] = "xp_gain";
            Name[XpGain] = SharedResources.Msg!.Get("Bonus XP");
            Desc[XpGain] = SharedResources.Msg!.Get("Increases the XP gained per kill.");
            Percent[XpGain] = true;
            Category[XpGain] = (short)StatCategory.CategoryMisc;

            // @TYPE currency_find|Percentage boost to the amount of gold dropped per loot event.
            Key[CurrencyFind] = "currency_find";
            Name[CurrencyFind] = SharedResources.Msg!.GetV("Bonus %s", SharedResources.Eset!.Loot.Currency);
            Desc[CurrencyFind] = SharedResources.Msg!.GetV("Increases the %s found per drop.", SharedResources.Eset!.Loot.Currency);
            Percent[CurrencyFind] = true;
            Category[CurrencyFind] = (short)StatCategory.CategoryMisc;

            // @TYPE item_find|Increases the chance of finding items in loot.
            Key[ItemFind] = "item_find";
            Name[ItemFind] = SharedResources.Msg!.Get("Item Find Chance");
            Desc[ItemFind] = SharedResources.Msg!.Get("Increases the chance that an enemy will drop an item.");
            Percent[ItemFind] = true;
            Category[ItemFind] = (short)StatCategory.CategoryMisc;

            // @TYPE stealth|Decrease the distance required to alert enemies by %
            Key[Stealth] = "stealth";
            Name[Stealth] = SharedResources.Msg!.Get("Stealth");
            Desc[Stealth] = SharedResources.Msg!.Get("Increases your ability to move undetected.");
            Percent[Stealth] = true;
            Category[Stealth] = (short)StatCategory.CategoryDefense;

            // @TYPE poise|Reduced % chance of entering "hit" animation when damaged
            Key[Poise] = "poise";
            Name[Poise] = SharedResources.Msg!.Get("Poise");
            Desc[Poise] = SharedResources.Msg!.Get("Reduces your chance of stumbling when hit.");
            Percent[Poise] = true;
            Category[Poise] = (short)StatCategory.CategoryDefense;

            // @TYPE reflect_chance|Percentage chance to reflect missiles
            Key[Reflect] = "reflect_chance";
            Name[Reflect] = SharedResources.Msg!.Get("Missile Reflect Chance");
            Desc[Reflect] = SharedResources.Msg!.Get("Increases your chance of reflecting missiles back at enemies.");
            Percent[Reflect] = true;
            Category[Reflect] = (short)StatCategory.CategoryDefense;

            // @TYPE return_damage|Deals a percentage of the damage taken back to the attacker
            Key[ReturnDamage] = "return_damage";
            Name[ReturnDamage] = SharedResources.Msg!.Get("Damage Reflection");
            Desc[ReturnDamage] = SharedResources.Msg!.Get("Deals a percentage of damage taken back to the attacker.");
            Percent[ReturnDamage] = true;
            Category[ReturnDamage] = (short)StatCategory.CategoryOffense;

            // @TYPE hp_steal|Percentage of HP stolen when damaging a target
            Key[HpSteal] = "hp_steal";
            Name[HpSteal] = SharedResources.Msg!.Get("HP Steal");
            Desc[HpSteal] = SharedResources.Msg!.Get("Percentage of HP stolen per hit.");
            Percent[HpSteal] = true;
            Category[HpSteal] = (short)StatCategory.CategoryOffense;

            // @TYPE mp_steal|Percentage of MP stolen when damaging a target
            Key[MpSteal] = "mp_steal";
            Name[MpSteal] = SharedResources.Msg!.Get("MP Steal");
            Desc[MpSteal] = SharedResources.Msg!.Get("Percentage of MP stolen per hit.");
            Percent[MpSteal] = true;
            Category[MpSteal] = (short)StatCategory.CategoryOffense;

            // @TYPE resist_damage_over_time|Percentage chance that damage-over-time effects will be negated
            Key[ResistDamageOverTime] = "resist_damage_over_time";
            Name[ResistDamageOverTime] = SharedResources.Msg!.Get("Resist Damage-Over-Time");
            Desc[ResistDamageOverTime] = SharedResources.Msg!.Get("Percentage chance that damage-over-time effects will be negated.");
            Percent[ResistDamageOverTime] = true;
            Category[ResistDamageOverTime] = (short)StatCategory.CategoryDefense;

            // @TYPE resist_slow|Percentage chance that slow effects will be negated
            Key[ResistSlow] = "resist_slow";
            Name[ResistSlow] = SharedResources.Msg!.Get("Resist Slow");
            Desc[ResistSlow] = SharedResources.Msg!.Get("Percentage chance that slow effects will be negated.");
            Percent[ResistSlow] = true;
            Category[ResistSlow] = (short)StatCategory.CategoryDefense;

            // @TYPE resist_stun|Percentage chance that stun effects will be negated
            Key[ResistStun] = "resist_stun";
            Name[ResistStun] = SharedResources.Msg!.Get("Resist Stun");
            Desc[ResistStun] = SharedResources.Msg!.Get("Percentage chance that stun effects will be negated.");
            Percent[ResistStun] = true;
            Category[ResistStun] = (short)StatCategory.CategoryDefense;

            // @TYPE resist_knockback|Percentage chance that knockback effects will be negated
            Key[ResistKnockback] = "resist_knockback";
            Name[ResistKnockback] = SharedResources.Msg!.Get("Resist Knockback");
            Desc[ResistKnockback] = SharedResources.Msg!.Get("Percentage chance that knockback effects will be negated.");
            Percent[ResistKnockback] = true;
            Category[ResistKnockback] = (short)StatCategory.CategoryDefense;

            // @TYPE resist_stat_debuff|Percentage chance that stat debuff effects will be negated
            Key[ResistStatDebuff] = "resist_stat_debuff";
            Name[ResistStatDebuff] = SharedResources.Msg!.Get("Resist Stat Debuffs");
            Desc[ResistStatDebuff] = SharedResources.Msg!.Get("Percentage chance that stat debuff effects will be negated.");
            Percent[ResistStatDebuff] = true;
            Category[ResistStatDebuff] = (short)StatCategory.CategoryDefense;

            // @TYPE resist_damage_reflect|Percentage chance that damage reflection will be negated
            Key[ResistDamageReflect] = "resist_damage_reflect";
            Name[ResistDamageReflect] = SharedResources.Msg!.Get("Resist Damage Reflection");
            Desc[ResistDamageReflect] = SharedResources.Msg!.Get("Percentage chance that damage reflection will be negated.");
            Percent[ResistDamageReflect] = true;
            Category[ResistDamageReflect] = (short)StatCategory.CategoryDefense;

            // @TYPE resist_hp_steal|Percentage chance that HP steal will be negated
            Key[ResistHpSteal] = "resist_hp_steal";
            Name[ResistHpSteal] = SharedResources.Msg!.Get("Resist HP Steal");
            Desc[ResistHpSteal] = SharedResources.Msg!.Get("Percentage chance that HP steal will be negated.");
            Percent[ResistHpSteal] = true;
            Category[ResistHpSteal] = (short)StatCategory.CategoryDefense;

            // @TYPE resist_mp_steal|Percentage chance that MP steal will be negated
            Key[ResistMpSteal] = "resist_mp_steal";
            Name[ResistMpSteal] = SharedResources.Msg!.Get("Resist MP Steal");
            Desc[ResistMpSteal] = SharedResources.Msg!.Get("Percentage chance that MP steal will be negated.");
            Percent[ResistMpSteal] = true;
            Category[ResistMpSteal] = (short)StatCategory.CategoryDefense;
        }
    }
}
