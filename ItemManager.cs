// 对应 C++ 源文件：ItemManager.h + ItemManager.cpp
using System.Text;
using Stride.Core.Mathematics;

namespace FlareEngine
{
    public class LevelScaledValue
    {
        public bool Randomized;
        public bool ResultRound;
        public bool ResultMinEnabled;
        public bool ResultMaxEnabled;

        public int ItemLevel;

        public float Base;
        public float BaseMax;
        public float BaseStep;

        public float PerItemLevel;
        public float PerItemLevelMax;
        public float PerItemLevelStep;

        public float PerPlayerLevel;
        public float PerPlayerLevelMax;
        public float PerPlayerLevelStep;

        public float ResultMin;
        public float ResultMax;

        public List<float> PerPlayerPrimary = new List<float>();
        public List<float> PerPlayerPrimaryMax = new List<float>();
        public List<float> PerPlayerPrimaryStep = new List<float>();

        public LevelScaledValue()
        {
            Randomized = false;
            ResultRound = false;
            ResultMinEnabled = false;
            ResultMaxEnabled = false;
            ItemLevel = 1;
            Base = 0;
            BaseMax = 0;
            BaseStep = 1;
            PerItemLevel = 0;
            PerItemLevelMax = 0;
            PerItemLevelStep = 1;
            PerPlayerLevel = 0;
            PerPlayerLevelMax = 0;
            PerPlayerLevelStep = 1;
            ResultMin = 0;
            ResultMax = 0;
            int primaryCount = SharedResources.Eset!.PrimaryStats.Stats.Count;
            PerPlayerPrimary = new List<float>(new float[primaryCount]);
            PerPlayerPrimaryMax = new List<float>(new float[primaryCount]);
            PerPlayerPrimaryStep = new List<float>(new float[primaryCount]);
            for (int i = 0; i < primaryCount; ++i)
            {
                PerPlayerPrimaryStep[i] = 1;
            }
        }

        public float Get()
        {
            float result = Base + (PerItemLevel * (float)(ItemLevel - 1)) + (PerPlayerLevel * (float)(SharedGameResources.Pc!.Stats.Level - 1));
            for (int i = 0; i < PerPlayerPrimary.Count; ++i)
            {
                result += PerPlayerPrimary[i] * (float)(SharedGameResources.Pc!.Stats.GetPrimary(i) - 1);
            }

            if (ResultMaxEnabled)
                result = Math.Min(result, ResultMax);
            if (ResultMinEnabled)
                result = Math.Max(result, ResultMin);

            return (ResultRound ? MathF.Round(result) : result);
        }

        public float GetMax()
        {
            float result = BaseMax + (PerItemLevelMax * (float)(ItemLevel - 1)) + (PerPlayerLevelMax * (float)(SharedGameResources.Pc!.Stats.Level - 1));
            for (int i = 0; i < PerPlayerPrimaryMax.Count; ++i)
            {
                result += PerPlayerPrimaryMax[i] * (float)(SharedGameResources.Pc!.Stats.GetPrimary(i) - 1);
            }

            if (ResultMaxEnabled)
                result = Math.Min(result, ResultMax);
            if (ResultMinEnabled)
                result = Math.Max(result, ResultMin);

            return (ResultRound ? MathF.Round(result) : result);
        }

        public float GetStep()
        {
            float result = BaseStep + (PerItemLevelStep * (float)(ItemLevel - 1)) + (PerPlayerLevelStep * (float)(SharedGameResources.Pc!.Stats.Level - 1));
            for (int i = 0; i < PerPlayerPrimaryStep.Count; ++i)
            {
                result += PerPlayerPrimaryStep[i] * (float)(SharedGameResources.Pc!.Stats.GetPrimary(i) - 1);
            }
            return result;
        }

        public void Randomize()
        {
            int stepCount;

            if (Base != BaseMax)
                Randomized = true;

            stepCount = (int)((BaseMax - Base) / BaseStep);
            Base += (float)MathUtils.RandBetween(0, stepCount) * BaseStep;
            BaseMax = Base;

            if (PerItemLevel != PerItemLevelMax)
                Randomized = true;

            stepCount = (int)((PerItemLevelMax - PerItemLevel) / PerItemLevelStep);
            PerItemLevel += (float)MathUtils.RandBetween(0, stepCount) * PerItemLevelStep;
            PerItemLevelMax = PerItemLevel;

            if (PerPlayerLevel != PerPlayerLevelMax)
                Randomized = true;

            stepCount = (int)((PerPlayerLevelMax - PerPlayerLevel) / PerPlayerLevelStep);
            PerPlayerLevel += (float)MathUtils.RandBetween(0, stepCount) * PerPlayerLevelStep;
            PerPlayerLevelMax = PerPlayerLevel;

            for (int i = 0; i < PerPlayerPrimary.Count; ++i)
            {
                if (PerPlayerPrimary[i] != PerPlayerPrimaryMax[i])
                    Randomized = true;

                stepCount = (int)((PerPlayerPrimaryMax[i] - PerPlayerPrimary[i]) / PerPlayerPrimaryStep[i]);
                PerPlayerPrimary[i] += (float)MathUtils.RandBetween(0, stepCount) * PerPlayerPrimaryStep[i];
                PerPlayerPrimaryMax[i] = PerPlayerPrimary[i];
            }
        }

        public void Parse(ref string s)
        {
            Clear();

            string section = global::FlareEngine.Parse.PopFirstString(ref s);

            while (!string.IsNullOrEmpty(section))
            {
                if (section.IndexOf(':') != -1)
                {
                    section += ':'; // ensure there's a trailing colon when we pop the value following the scale type

                    string scaleType = global::FlareEngine.Parse.PopFirstString(ref section, ':');

                    if (scaleType == "base")
                    {
                        Base = global::FlareEngine.Parse.PopFirstFloat(ref section, ':');
                        BaseMax = Math.Max(Base, global::FlareEngine.Parse.PopFirstFloat(ref section, ':'));
                        string stepStr = global::FlareEngine.Parse.PopFirstString(ref section, ':');
                        BaseStep = global::FlareEngine.Parse.ToFloat(stepStr, 1);
                    }
                    else if (scaleType == "item_level")
                    {
                        PerItemLevel = global::FlareEngine.Parse.PopFirstFloat(ref section, ':');
                        PerItemLevelMax = Math.Max(PerItemLevel, global::FlareEngine.Parse.PopFirstFloat(ref section, ':'));
                        string stepStr = global::FlareEngine.Parse.PopFirstString(ref section, ':');
                        PerItemLevelStep = global::FlareEngine.Parse.ToFloat(stepStr, 1);
                    }
                    else if (scaleType == "player_level")
                    {
                        PerPlayerLevel = global::FlareEngine.Parse.PopFirstFloat(ref section, ':');
                        PerPlayerLevelMax = Math.Max(PerPlayerLevel, global::FlareEngine.Parse.PopFirstFloat(ref section, ':'));
                        string stepStr = global::FlareEngine.Parse.PopFirstString(ref section, ':');
                        PerPlayerLevelStep = global::FlareEngine.Parse.ToFloat(stepStr, 1);
                    }
                    else if (scaleType == "round")
                    {
                        ResultRound = global::FlareEngine.Parse.ToBool(global::FlareEngine.Parse.PopFirstString(ref section, ':'));
                    }
                    else if (scaleType == "min")
                    {
                        ResultMin = global::FlareEngine.Parse.PopFirstFloat(ref section, ':');
                        ResultMinEnabled = true;
                    }
                    else if (scaleType == "max")
                    {
                        ResultMax = global::FlareEngine.Parse.PopFirstFloat(ref section, ':');
                        ResultMaxEnabled = true;
                    }
                    else
                    {
                        // player primary stats
                        int primaryIndex = SharedResources.Eset!.PrimaryStats.GetIndexByID(scaleType);
                        if (primaryIndex < SharedResources.Eset.PrimaryStats.Stats.Count)
                        {
                            PerPlayerPrimary[primaryIndex] = global::FlareEngine.Parse.PopFirstFloat(ref section, ':');
                            PerPlayerPrimaryMax[primaryIndex] = Math.Max(PerPlayerPrimary[primaryIndex], global::FlareEngine.Parse.PopFirstFloat(ref section, ':'));
                            string stepStr = global::FlareEngine.Parse.PopFirstString(ref section, ':');
                            PerPlayerPrimaryStep[primaryIndex] = global::FlareEngine.Parse.ToFloat(stepStr, 1);
                        }
                    }
                }
                else
                {
                    // no scale type defined, assume base value
                    Base = global::FlareEngine.Parse.PopFirstFloat(ref section);
                    BaseMax = Base;
                }
                section = global::FlareEngine.Parse.PopFirstString(ref s);
            }
        }

        public void SetBaseFromFloat(float f)
        {
            Base = f;
            BaseMax = Base;
            BaseStep = 1;
        }

        public void Clear()
        {
            Randomized = false;

            ItemLevel = 1;

            Base = 0;
            BaseMax = 0;
            BaseStep = 1;

            PerItemLevel = 0;
            PerItemLevelMax = 0;
            PerItemLevelStep = 1;

            PerPlayerLevel = 0;
            PerPlayerLevelMax = 0;
            PerPlayerLevelStep = 1;

            for (int i = 0; i < PerPlayerPrimary.Count; ++i)
            {
                PerPlayerPrimary[i] = 0;
                PerPlayerPrimaryMax[i] = 0;
                PerPlayerPrimaryStep[i] = 1;
            }
        }

        public string Serialize(bool isMultiplier)
        {
            StringBuilder outSb = new StringBuilder();
            bool comma = false;

            if (Base != 0 || BaseMax != 0)
            {
                outSb.Append("base:");
                if (isMultiplier)
                    outSb.Append(Base * 100).Append('%');
                else
                    outSb.Append(Base);

                comma = true;
            }

            if (PerItemLevel != 0 || PerItemLevelMax != 0)
            {
                if (comma)
                    outSb.Append(',');

                outSb.Append("item_level:");
                if (isMultiplier)
                    outSb.Append(PerItemLevel * 100).Append('%');
                else
                    outSb.Append(PerItemLevel);

                comma = true;
            }

            if (PerPlayerLevel != 0 || PerPlayerLevelMax != 0)
            {
                if (comma)
                    outSb.Append(',');

                outSb.Append("player_level:");
                if (isMultiplier)
                    outSb.Append(PerPlayerLevel * 100).Append('%');
                else
                    outSb.Append(PerPlayerLevel);

                comma = true;
            }

            for (int i = 0; i < PerPlayerPrimary.Count; ++i)
            {
                if (PerPlayerPrimary[i] != 0 || PerPlayerPrimaryMax[i] != 0)
                {
                    if (comma)
                        outSb.Append(',');

                    outSb.Append(SharedResources.Eset!.PrimaryStats.Stats[i].Id).Append(':');
                    if (isMultiplier)
                        outSb.Append(PerPlayerPrimary[i] * 100).Append('%');
                    else
                        outSb.Append(PerPlayerPrimary[i]);

                    comma = true;
                }
            }

            if (ResultRound)
            {
                if (comma)
                    outSb.Append(',');

                outSb.Append("round:true");

                comma = true;
            }

            if (ResultMinEnabled)
            {
                if (comma)
                    outSb.Append(',');

                outSb.Append("min:").Append(ResultMin);

                comma = true;
            }

            if (ResultMaxEnabled)
            {
                if (comma)
                    outSb.Append(',');

                outSb.Append("max:").Append(ResultMax);

                comma = true;
            }

            return outSb.ToString();
        }

        /// <summary>
        /// 对应 C++ �?LevelScaledValue 的编译器生成拷贝赋值语义（逐成员拷贝，�?vector 深拷贝）�?
        /// </summary>
        public void CopyFrom(LevelScaledValue other)
        {
            Randomized = other.Randomized;
            ResultRound = other.ResultRound;
            ResultMinEnabled = other.ResultMinEnabled;
            ResultMaxEnabled = other.ResultMaxEnabled;
            ItemLevel = other.ItemLevel;
            Base = other.Base;
            BaseMax = other.BaseMax;
            BaseStep = other.BaseStep;
            PerItemLevel = other.PerItemLevel;
            PerItemLevelMax = other.PerItemLevelMax;
            PerItemLevelStep = other.PerItemLevelStep;
            PerPlayerLevel = other.PerPlayerLevel;
            PerPlayerLevelMax = other.PerPlayerLevelMax;
            PerPlayerLevelStep = other.PerPlayerLevelStep;
            ResultMin = other.ResultMin;
            ResultMax = other.ResultMax;
            PerPlayerPrimary = new List<float>(other.PerPlayerPrimary);
            PerPlayerPrimaryMax = new List<float>(other.PerPlayerPrimaryMax);
            PerPlayerPrimaryStep = new List<float>(other.PerPlayerPrimaryStep);
        }
    }

    public class LevelScaledMinMax
    {
        public LevelScaledValue Min = new LevelScaledValue();
        public LevelScaledValue Max = new LevelScaledValue();
    }

    public class LootAnimation
    {
        public string Name = "";
        public int Low;
        public int High;

        public LootAnimation()
        {
            Name = "";
            Low = 0;
            High = 0;
        }
    }

    public class BonusData
    {
        public const int Unknown = 0;
        public const int Speed = 1;
        public const int AttackSpeed = 2;
        public const int Stat = 3;
        public const int DamageMin = 4;
        public const int DamageMax = 5;
        public const int ResistElement = 6;
        public const int PrimaryStat = 7;
        public const int ResourceStat = 8;
        public const int PowerLevel = 9;

        public bool IsMultiplier;
        public bool IsExtended; // if true, bonus should be written when creating extended_items.txt in SaveLoad
        public int Type;
        public int Index;
        public int SubIndex; // used for resource stats
        public LevelScaledValue Value = new LevelScaledValue();
        public PowerID PowerId; // for bonus_power_level

        public BonusData()
        {
            IsMultiplier = false;
            IsExtended = false;
            Type = Unknown;
            Index = 0;
            SubIndex = 0;
            Value = new LevelScaledValue();
            PowerId = 0;
        }
    }

    public class SetBonusData : BonusData
    {
        public int Requirement;

        public SetBonusData()
        {
            Requirement = 0;
        }
    }

    public class ItemQuality
    {
        public string Id = "";
        public string Name = "";
        public Color Color = new Color(255, 255, 255, 255);
        public int OverlayIcon;

        public ItemQuality()
        {
            Id = "";
            Name = "";
            Color = new Color(255, 255, 255, 255);
            OverlayIcon = -1;
        }
    }

    public class ItemSet
    {
        public string Name = "";            // item set name displayed on long and short tool tips
        public List<ItemID> Items = new List<ItemID>();      // items, included into set
        public List<SetBonusData> Bonus = new List<SetBonusData>();// vector with stats to increase/decrease
        public Color Color = new Color(255, 255, 255, 255);

        public ItemSet()
        {
            Name = "";
            Color = new Color(255, 255, 255, 255);
        }
    }

    public class ItemStack
    {
        public ItemStack(ItemID item = 0, int quantity = 0)
        {
            Item = item;
            Quantity = quantity;
            CanBuyback = false;
        }

        public ItemStack(Int2 p)
        {
            Item = p.X;
            Quantity = p.Y;
            CanBuyback = false;
        }

        /// <summary>Deep copy (C# reference-type compensation for C++ value-type semantics)</summary>
        public ItemStack Clone()
        {
            return new ItemStack(Item, Quantity) { CanBuyback = CanBuyback };
        }

        public static bool operator <(ItemStack left, ItemStack param)
        {
            return param > left;
        }

        public static bool operator >(ItemStack left, ItemStack param)
        {
            if (left.Item == 0 && param.Item > 0)
            {
                // Make the empty slots the last while sorting
                return true;
            }
            else if (left.Item > 0 && param.Item == 0)
            {
                // Make the empty slots the last while sorting
                return false;
            }
            else if (left.Item == 0 && param.Item == 0)
            {
                return false;
            }
            else if (left.Item == param.Item)
            {
                return left.Quantity > param.Quantity;
            }
            else
            {
                return left.Item > param.Item;
            }
        }

        public bool Empty()
        {
            if (Item != 0 && Quantity > 0)
            {
                return false;
            }
            else if (Item == 0 && Quantity != 0)
            {
                Utils.LogError("ItemStack: Item id is zero, but quantity is %d.", Quantity);
                Clear();
            }
            else if (Item != 0 && Quantity == 0)
            {
                Utils.LogError("ItemStack: Item id is %d, but quantity is zero.", Item);
                Clear();
            }
            return true;
        }

        public void Clear()
        {
            Item = 0;
            Quantity = 0;
            CanBuyback = false;
        }

        public ItemID Item;
        public int Quantity;
        public bool CanBuyback;
    }

    public class ItemType
    {
        public string Id = "";
        public string Name = "";
        public bool AutoPickup;
        public bool AutoActionbar;

        public ItemType()
        {
            Id = "";
            Name = "";
            AutoPickup = false;
            AutoActionbar = false;
        }
    }

    public class ItemRandomizerDef
    {
        public class Option
        {
            public const int LevelSrcBase = 0;
            public const int LevelSrcHero = 1;

            public int LevelSrc;
            public int LevelRangeMin;
            public int LevelRangeMax;
            public int BonusMin;
            public int BonusMax;

            public float Chance;
            public int Quality;

            public Option()
            {
                LevelSrc = LevelSrcBase;
                LevelRangeMin = 0;
                LevelRangeMax = 0;
                BonusMin = 0;
                BonusMax = 0;
                Chance = 100;
                Quality = 0;
            }
        }

        public string Filename = "";

        public List<Option> Options = new List<Option>();
        public List<BonusData> Bonus = new List<BonusData>();

        public ItemRandomizerDef()
        {
            Filename = "";
            Options = new List<Option>();
            Bonus = new List<BonusData>();
        }
    }

    public class Item
    {
        internal string Name = "";     // item name displayed on long and short tool tips

        public const int NoStashNull = 0;
        public const int NoStashIgnore = 1;
        public const int NoStashPrivate = 2;
        public const int NoStashShared = 3;
        public const int NoStashAll = 4;

        public bool HasName;        // flag that is set when the item name is parsed
        public bool BookIsReadable = true; // whether to display "use" or "read" in the tooltip
        public bool QuestItem;
        public bool IsForeign = true; // used to track extended items for clean up during save

        public int Level;            // rough estimate of quality, used in the loot algorithm
        public int Icon;             // icon index on small pixel sheet
        public int MaxQuantity;     // max count per stack
        public int NoStash;
        public int LootDropsMax;

        public ItemID Parent;
        public ItemSetID Set;              // item can be attached to item set
        public SoundID SfxId;
        public PowerID Power;            // this item can be dragged to the action bar and used as a power
        public int Type;     // equipment slot or base item type. An index into ItemManager::item_types
        public int Quality;  // An index into ItemManager::item_qualities

        public ItemRandomizerDef? RandomizerDef;

        public LevelScaledMinMax BaseAbs = new LevelScaledMinMax();          // minimum/maximum absorb amount

        public string Flavor = "";   // optional flavor text describing the item
        public string Book = "";     // book file location
        public string RequiresClass = "";
        public string Sfx = "";           // the item sound when it hits the floor or inventory, etc
        public string Gfx = "";           // the sprite layer shown when this item is equipped
        public string PowerDesc = "";    // shows up in green text on the tooltip
        public string PickupStatus = ""; // when this item is picked up, set a campaign state (usually for quest items)
        public string Stepfx = "";        // sound effect played when walking (armors only)
        public string Script = "";

        public List<int> EquipFlags = new List<int>();   // common values include: melee, ranged, mental, shield
        public List<LevelScaledMinMax> BaseDmg = new List<LevelScaledMinMax>(); // minimum/maximum damage amount
        public List<BonusData> Bonus = new List<BonusData>();   // stat to increase/decrease e.g. hp, accuracy, speed
        public List<LootAnimation> LootAnimation = new List<LootAnimation>();// the flying loot animation for this item
        public List<(PowerID, PowerID)> ReplacePower = new List<(PowerID, PowerID)>();        // alter powers when this item is equipped. The first PowerID is replaced with the second.
        public List<int> DisableSlots = new List<int>(); // if this item is equipped, it will disable slots that match the types in the list
        public List<LevelScaledValue> RequiresStat = new List<LevelScaledValue>();
        public List<ItemStack> CraftingItems = new List<ItemStack>();

        public LevelScaledValue RequiresLevel = new LevelScaledValue();   // Player level must match or exceed this value to use item
        public LevelScaledValue Price = new LevelScaledValue();            // if price = 0 the item cannot be sold
        public LevelScaledValue PriceSell = new LevelScaledValue();       // if price_sell = 0, the sell price is price*vendor_ratio

        public Item()
        {
            Name = "";
            HasName = false;
            BookIsReadable = true;
            QuestItem = false;
            IsForeign = true;
            Level = 0;
            Icon = 0;
            MaxQuantity = int.MaxValue;
            NoStash = NoStashNull;
            LootDropsMax = -1;
            Parent = 0;
            Set = 0;
            SfxId = 0;
            Power = 0;
            Type = 0;
            Quality = 0;
            RandomizerDef = null;
            BaseAbs = new LevelScaledMinMax();
            Flavor = "";
            Book = "";
            RequiresClass = "";
            Sfx = "";
            Gfx = "";
            PowerDesc = "";
            PickupStatus = "";
            Stepfx = "";
            Script = "";
            int dmgCount = (SharedResources.Eset != null ? SharedResources.Eset.DamageTypes.Types.Count : 0);
            BaseDmg = new List<LevelScaledMinMax>(dmgCount);
            for (int i = 0; i < dmgCount; ++i)
                BaseDmg.Add(new LevelScaledMinMax());
            int reqStatCount = (SharedResources.Eset != null ? SharedResources.Eset.PrimaryStats.Stats.Count : 0);
            RequiresStat = new List<LevelScaledValue>(reqStatCount);
            for (int i = 0; i < reqStatCount; ++i)
                RequiresStat.Add(new LevelScaledValue());
            RequiresLevel = new LevelScaledValue();
            Price = new LevelScaledValue();
            PriceSell = new LevelScaledValue();
        }

        public int GetPrice(bool useVendorRatio)
        {
            int newPrice = (int)Price.Get();
            if (newPrice == 0)
                return newPrice;

            newPrice = (int)((float)newPrice * SharedResources.Eset!.Loot.VendorRatioBuy);

            NPC? vendorNpc = ((SharedGameResources.Menu != null && SharedGameResources.Menu!.Vendor != null && SharedGameResources.Menu.Vendor!.Visible) ? SharedGameResources.Menu.Vendor.Npc : null);

            if (useVendorRatio)
            {
                // get vendor ratio from NPC (or fall back to global value)
                float vendorRatioBuy = (vendorNpc != null && vendorNpc.VendorRatioBuy > 0) ? vendorNpc.VendorRatioBuy : SharedResources.Eset.Loot.VendorRatioBuy;

                newPrice = (int)((float)newPrice * vendorRatioBuy);
            }

            return Math.Max(newPrice, 1);
        }

        public int GetSellPrice(bool isNewBuyback)
        {
            int newPrice = 0;
            NPC? vendorNpc = ((SharedGameResources.Menu != null && SharedGameResources.Menu!.Vendor != null && SharedGameResources.Menu.Vendor!.Visible) ? SharedGameResources.Menu.Vendor.Npc : null);

            // get vendor ratio from NPC (or fall back to global value)
            float vendorRatioSell = (vendorNpc != null && vendorNpc.VendorRatioSell > 0) ? vendorNpc.VendorRatioSell : SharedResources.Eset!.Loot.VendorRatioSell;
            float vendorRatioSellOld = (vendorNpc != null && vendorNpc.VendorRatioSellOld > 0) ? vendorNpc.VendorRatioSellOld : SharedResources.Eset.Loot.VendorRatioSellOld;

            if (isNewBuyback || vendorRatioSellOld == 0)
            {
                // default sell price
                int scaledPriceSell = (int)PriceSell.Get();
                if (scaledPriceSell != 0)
                    newPrice = scaledPriceSell;
                else
                    newPrice = (int)((float)GetPrice(!ItemManager.UseVendorRatio) * vendorRatioSell);
            }
            else
            {
                // sell price adjusted because the player can no longer buyback the item at the original sell price
                newPrice = (int)((float)GetPrice(!ItemManager.UseVendorRatio) * vendorRatioSellOld);
            }

            return Math.Max(newPrice, 1);
        }

        public int GetCraftCount()
        {
            int craftCount = int.MaxValue;

            if (CraftingItems.Count == 0)
                return 0;

            for (int i = 0; i < CraftingItems.Count; ++i)
            {
                ItemStack stack = CraftingItems[i];
                int itemCount = SharedGameResources.Menu!.Inv!.Inventory[MenuInventory.Carried].Count(stack.Item);
                itemCount += SharedGameResources.Menu.Inv.Inventory[MenuInventory.Equipment].Count(stack.Item);
                craftCount = Math.Min(craftCount, itemCount / stack.Quantity);
            }

            return craftCount;
        }

        public void UpdateLevelScaling()
        {
            // set item_level for level-scaled values
            RequiresLevel.ItemLevel = Level;
            Price.ItemLevel = Level;
            PriceSell.ItemLevel = Level;

            for (int i = 0; i < RequiresStat.Count; ++i)
            {
                RequiresStat[i].ItemLevel = Level;
            }

            for (int i = 0; i < Bonus.Count; ++i)
            {
                Bonus[i].Value.ItemLevel = Level;
            }

            BaseAbs.Min.ItemLevel = Level;
            BaseAbs.Max.ItemLevel = Level;

            for (int i = 0; i < BaseDmg.Count; ++i)
            {
                BaseDmg[i].Min.ItemLevel = Level;
                BaseDmg[i].Max.ItemLevel = Level;
            }
        }

        /// <summary>
        /// 对应 C++ �?Item 的编译器生成拷贝赋值语义（逐成员深拷贝）�?
        /// </summary>
        public void CopyFrom(Item other)
        {
            Name = other.Name;
            HasName = other.HasName;
            BookIsReadable = other.BookIsReadable;
            QuestItem = other.QuestItem;
            IsForeign = other.IsForeign;
            Level = other.Level;
            Icon = other.Icon;
            MaxQuantity = other.MaxQuantity;
            NoStash = other.NoStash;
            LootDropsMax = other.LootDropsMax;
            Parent = other.Parent;
            Set = other.Set;
            SfxId = other.SfxId;
            Power = other.Power;
            Type = other.Type;
            Quality = other.Quality;
            RandomizerDef = other.RandomizerDef;
            BaseAbs.Min.CopyFrom(other.BaseAbs.Min);
            BaseAbs.Max.CopyFrom(other.BaseAbs.Max);
            Flavor = other.Flavor;
            Book = other.Book;
            RequiresClass = other.RequiresClass;
            Sfx = other.Sfx;
            Gfx = other.Gfx;
            PowerDesc = other.PowerDesc;
            PickupStatus = other.PickupStatus;
            Stepfx = other.Stepfx;
            Script = other.Script;
            EquipFlags = new List<int>(other.EquipFlags);
            BaseDmg = new List<LevelScaledMinMax>(other.BaseDmg.Count);
            for (int i = 0; i < other.BaseDmg.Count; ++i)
            {
                LevelScaledMinMax dmg = new LevelScaledMinMax();
                dmg.Min.CopyFrom(other.BaseDmg[i].Min);
                dmg.Max.CopyFrom(other.BaseDmg[i].Max);
                BaseDmg.Add(dmg);
            }
            Bonus = new List<BonusData>(other.Bonus.Count);
            for (int i = 0; i < other.Bonus.Count; ++i)
            {
                BonusData b = new BonusData();
                b.IsMultiplier = other.Bonus[i].IsMultiplier;
                b.IsExtended = other.Bonus[i].IsExtended;
                b.Type = other.Bonus[i].Type;
                b.Index = other.Bonus[i].Index;
                b.SubIndex = other.Bonus[i].SubIndex;
                b.Value.CopyFrom(other.Bonus[i].Value);
                b.PowerId = other.Bonus[i].PowerId;
                Bonus.Add(b);
            }
            LootAnimation = new List<LootAnimation>(other.LootAnimation);
            ReplacePower = new List<(PowerID, PowerID)>(other.ReplacePower);
            DisableSlots = new List<int>(other.DisableSlots);
            RequiresStat = new List<LevelScaledValue>(other.RequiresStat.Count);
            for (int i = 0; i < other.RequiresStat.Count; ++i)
            {
                LevelScaledValue rs = new LevelScaledValue();
                rs.CopyFrom(other.RequiresStat[i]);
                RequiresStat.Add(rs);
            }
            CraftingItems = new List<ItemStack>(other.CraftingItems.Count);
            for (int i = 0; i < other.CraftingItems.Count; ++i)
            {
                ItemStack cs = new ItemStack(other.CraftingItems[i].Item, other.CraftingItems[i].Quantity);
                cs.CanBuyback = other.CraftingItems[i].CanBuyback;
                CraftingItems.Add(cs);
            }
            RequiresLevel.CopyFrom(other.RequiresLevel);
            Price.CopyFrom(other.Price);
            PriceSell.CopyFrom(other.PriceSell);
        }
    }

    public class ItemManager : IDisposable
    {
        public const int VendorBuy = 0;
        public const int VendorSell = 1;
        public const int VendorCraft = 2;
        public const int PlayerInv = 3;

        public const bool UseVendorRatio = true;
        public const bool DefaultSellPrice = true;
        public const bool TooltipInputHint = true;
        public const bool VerifyAllowZero = true;
        public const bool VerifyAllocate = true;

        private readonly List<ItemRandomizerDef> _randomizerDefs = new List<ItemRandomizerDef>();

        public List<Item?> Items = new List<Item?>();
        public List<ItemType> ItemTypes = new List<ItemType>();
        public List<ItemSet?> ItemSets = new List<ItemSet?>();
        public List<ItemQuality> ItemQualities = new List<ItemQuality>();

        public ItemManager()
        {
            LoadAll();
        }

        public void Dispose()
        {
            Utils.LogInfo("Cleaning up: ItemManager");

            for (int i = 0; i < Items.Count; ++i)
            {
                Items[i] = null;
            }
            for (int i = 0; i < ItemSets.Count; ++i)
            {
                ItemSets[i] = null;
            }
            for (int i = 0; i < _randomizerDefs.Count; ++i)
            {
                _randomizerDefs[i] = null!;
            }
            Items.Clear();
            ItemSets.Clear();
            _randomizerDefs.Clear();
        }

        public static bool CompareItemStack(ItemStack stack1, ItemStack stack2)
        {
            if (stack1.Item == stack2.Item)
                return stack1.Quantity < stack2.Quantity;
            else
                return stack1.Item < stack2.Item;
        }

        public bool IsValid(ItemID itemId)
        {
            return itemId > 0 && itemId < Items.Count && Items[(int)itemId] != null;
        }

        public bool IsValidSet(ItemSetID setId)
        {
            return setId > 0 && setId < ItemSets.Count && ItemSets[(int)setId] != null;
        }

        protected void LoadAll()
        {
            // load each items.txt file. Individual item IDs can be overwritten with mods.
            LoadTypes("items/types.txt");
            LoadQualities("items/qualities.txt");
            LoadItems("items/items.txt");
            LoadExtendedItems(SharedResources.Settings!.PathUser + "saves/" + SharedResources.Eset!.Misc.SavePrefix + "/extended_items.txt");
            LoadSets("items/sets.txt");

            if (Items.Count == 0)
                Utils.LogInfo("ItemManager: No items were found.");
        }

        protected void LoadItems(string filename)
        {
            FileParser infile = new FileParser();

            // @CLASS ItemManager: Items|Description of Items in items/items.txt.
            if (!infile.Open(filename, FileParser.ModFile, FileParser.ErrorNormal))
                return;

            // used to clear vectors when overriding items
            bool clearReqStat = false;
            bool clearBonus = false;
            bool clearLootAnim = false;
            bool clearReplacePower = false;

            ItemID id = 0;
            Item? item = null;
            bool idLine;
            while (infile.Next())
            {
                if (infile.Key == "id")
                {
                    // @ATTR id|item_id|An uniq id of the item used as reference from other classes.
                    idLine = true;
                    id = Parse.ToItemID(infile.Val);
                    if (id < Items.Count && Items[(int)id] != null)
                    {
                        clearReqStat = true;
                        clearBonus = true;
                        clearLootAnim = true;
                        clearReplacePower = true;
                    }
                    else
                    {
                        int targetSize = Math.Max((int)id + 1, Items.Count);
                        while (Items.Count < targetSize)
                            Items.Add(null);
                        Items[(int)id] = new Item();
                    }
                    item = Items[(int)id];

                    // set the max quantity if it has not been done yet
                    if (item!.MaxQuantity == int.MaxValue)
                        item.MaxQuantity = 1;
                }
                else idLine = false;

                if (id < 1)
                {
                    if (idLine) infile.Error("ItemManager: Item index out of bounds 1-%d, skipping item.", int.MaxValue);
                    continue;
                }
                if (idLine) continue;

                if (infile.Key == "name")
                {
                    // @ATTR name|string|Item name displayed on long and short tooltips.
                    item!.Name = SharedResources.Msg!.Get(infile.Val);
                    item.HasName = true;
                }
                else if (infile.Key == "flavor")
                    // @ATTR flavor|string|A description of the item.
                    item!.Flavor = SharedResources.Msg!.Get(infile.Val);
                else if (infile.Key == "level")
                    // @ATTR level|int|The item's level.
                    item!.Level = Parse.ToInt(infile.Val);
                else if (infile.Key == "icon")
                {
                    // @ATTR icon|icon_id|An id for the icon to display for this item.
                    item!.Icon = Parse.ToInt(infile.Val);
                }
                else if (infile.Key == "book")
                {
                    // @ATTR book|filename|A book file to open when this item is activated.
                    item!.Book = infile.Val;
                }
                else if (infile.Key == "book_is_readable")
                {
                    // @ATTR book_is_readable|bool|If true, "read" is displayed in the tooltip instead of "use". Defaults to true.
                    item!.BookIsReadable = global::FlareEngine.Parse.ToBool(infile.Val);
                }
                else if (infile.Key == "quality")
                {
                    // @ATTR quality|predefined_string|Item quality matching an id in items/qualities.txt
                    item!.Quality = (int)GetItemQualityIndexByString(infile.Val);
                }
                else if (infile.Key == "item_type")
                {
                    // @ATTR item_type|predefined_string|Equipment slot matching an id in items/types.txt
                    item!.Type = (int)GetItemTypeIndexByString(infile.Val);
                }
                else if (infile.Key == "equip_flags")
                {
                    // @ATTR equip_flags|list(predefined_string)|A comma separated list of flags to set when this item is equipped. See engine/equip_flags.txt.
                    item!.EquipFlags.Clear();
                    string flag = global::FlareEngine.Parse.PopFirstString(ref infile.Val);

                    while (flag != "")
                    {
                        item.EquipFlags.Add(SharedResources.Eset!.EquipFlags.GetIndex(flag));
                        flag = global::FlareEngine.Parse.PopFirstString(ref infile.Val);
                    }
                }
                else if (infile.Key == "dmg")
                {
                    // @ATTR dmg|predefined_string, float, float : Damage type, Min, Max|Defines the item's base damage type and range. Max may be ommitted and will default to Min.
                    string dmgTypeStr = global::FlareEngine.Parse.PopFirstString(ref infile.Val);

                    int dmgType = SharedResources.Eset!.DamageTypes.Types.Count;
                    for (int i = 0; i < SharedResources.Eset.DamageTypes.Types.Count; ++i)
                    {
                        if (dmgTypeStr == SharedResources.Eset.DamageTypes.Types[i].Id)
                        {
                            dmgType = i;
                            break;
                        }
                    }

                    if (dmgType == SharedResources.Eset.DamageTypes.Types.Count)
                    {
                        infile.Error("ItemManager: '%s' is not a known damage type id.", dmgTypeStr);
                    }
                    else
                    {
                        item!.BaseDmg[dmgType].Min.SetBaseFromFloat(global::FlareEngine.Parse.PopFirstFloat(ref infile.Val));
                        if (infile.Val.Length > 0)
                            item.BaseDmg[dmgType].Max.SetBaseFromFloat(global::FlareEngine.Parse.PopFirstFloat(ref infile.Val));
                        else
                            item.BaseDmg[dmgType].Max.CopyFrom(item.BaseDmg[dmgType].Min);
                    }
                }
                else if (infile.Key == "dmg_min")
                {
                    string dmgTypeStr = global::FlareEngine.Parse.PopFirstString(ref infile.Val);

                    int dmgType = SharedResources.Eset!.DamageTypes.Types.Count;
                    for (int i = 0; i < SharedResources.Eset.DamageTypes.Types.Count; ++i)
                    {
                        if (dmgTypeStr == SharedResources.Eset.DamageTypes.Types[i].Id)
                        {
                            dmgType = i;
                            break;
                        }
                    }

                    if (dmgType == SharedResources.Eset.DamageTypes.Types.Count)
                    {
                        infile.Error("ItemManager: '%s' is not a known damage type id.", dmgTypeStr);
                    }
                    else
                    {
                        string val = infile.Val;
                        item!.BaseDmg[dmgType].Min.Parse(ref val);
                        infile.Val = val;
                    }
                }
                else if (infile.Key == "dmg_max")
                {
                    string dmgTypeStr = global::FlareEngine.Parse.PopFirstString(ref infile.Val);

                    int dmgType = SharedResources.Eset!.DamageTypes.Types.Count;
                    for (int i = 0; i < SharedResources.Eset.DamageTypes.Types.Count; ++i)
                    {
                        if (dmgTypeStr == SharedResources.Eset.DamageTypes.Types[i].Id)
                        {
                            dmgType = i;
                            break;
                        }
                    }

                    if (dmgType == SharedResources.Eset.DamageTypes.Types.Count)
                    {
                        infile.Error("ItemManager: '%s' is not a known damage type id.", dmgTypeStr);
                    }
                    else
                    {
                        string val = infile.Val;
                        item!.BaseDmg[dmgType].Max.Parse(ref val);
                        infile.Val = val;
                    }
                }
                else if (infile.Key == "abs")
                {
                    item!.BaseAbs.Min.SetBaseFromFloat(global::FlareEngine.Parse.PopFirstFloat(ref infile.Val));
                    if (infile.Val.Length > 0)
                        item.BaseAbs.Max.SetBaseFromFloat(global::FlareEngine.Parse.PopFirstFloat(ref infile.Val));
                    else
                        item.BaseAbs.Max.CopyFrom(item.BaseAbs.Min);
                }
                else if (infile.Key == "abs_min")
                {
                    string val = infile.Val;
                    item!.BaseAbs.Min.Parse(ref val);
                    infile.Val = val;
                }
                else if (infile.Key == "abs_max")
                {
                    string val = infile.Val;
                    item!.BaseAbs.Max.Parse(ref val);
                    infile.Val = val;
                }
                else if (infile.Key == "requires_level")
                {
                    string val = infile.Val;
                    item!.RequiresLevel.Parse(ref val);
                    infile.Val = val;
                }
                else if (infile.Key == "requires_stat")
                {
                    if (clearReqStat)
                    {
                        Utils.LogInfo("ItemManager: Item %zu, clearing requires_stat list.", id);
                        item!.RequiresStat.Clear();
                        for (int ri = 0; ri < SharedResources.Eset!.PrimaryStats.Stats.Count; ++ri)
                            item.RequiresStat.Add(new LevelScaledValue());
                        clearReqStat = false;
                    }

                    string s = global::FlareEngine.Parse.PopFirstString(ref infile.Val);
                    int reqStatIndex = SharedResources.Eset!.PrimaryStats.GetIndexByID(s);
                    if (reqStatIndex < SharedResources.Eset.PrimaryStats.Stats.Count)
                    {
                        string val = infile.Val;
                        item!.RequiresStat[reqStatIndex].Parse(ref val);
                        infile.Val = val;
                    }
                    else
                        infile.Error("ItemManager: '%s' is not a valid primary stat.", s);
                }
                else if (infile.Key == "requires_class")
                {
                    item!.RequiresClass = infile.Val;
                }
                else if (infile.Key == "bonus")
                {
                    if (clearBonus)
                    {
                        Utils.LogInfo("ItemManager: Item %zu, clearing bonus list.", id);
                        item!.Bonus.Clear();
                        clearBonus = false;
                    }
                    BonusData bdata = new BonusData();
                    ParseBonus(bdata, infile);
                    item!.Bonus.Add(bdata);
                }
                else if (infile.Key == "bonus_power_level")
                {
                    BonusData bdata = new BonusData();
                    bdata.Type = BonusData.PowerLevel;
                    bdata.PowerId = Parse.ToPowerID(global::FlareEngine.Parse.PopFirstString(ref infile.Val));
                    string val = infile.Val;
                    bdata.Value.Parse(ref val);
                    infile.Val = val;
                    item!.Bonus.Add(bdata);
                }
                else if (infile.Key == "soundfx")
                {
                    item!.Sfx = infile.Val;
                    item.SfxId = SharedResources.Snd!.Load(item.Sfx, "ItemManager");
                }
                else if (infile.Key == "gfx")
                    item!.Gfx = infile.Val;
                else if (infile.Key == "loot_animation")
                {
                    if (clearLootAnim)
                    {
                        Utils.LogInfo("ItemManager: Item %zu, clearing loot_animation list.", id);
                        item!.LootAnimation.Clear();
                        clearLootAnim = false;
                    }
                    LootAnimation la = new LootAnimation();
                    la.Name = global::FlareEngine.Parse.PopFirstString(ref infile.Val);
                    la.Low = Parse.PopFirstInt(ref infile.Val);
                    la.High = Parse.PopFirstInt(ref infile.Val);
                    item!.LootAnimation.Add(la);
                }
                else if (infile.Key == "power")
                {
                    if (Parse.ToInt(infile.Val) > 0)
                        item!.Power = Parse.ToInt(infile.Val);
                    else
                        infile.Error("ItemManager: Power index out of bounds 1-%d, skipping power.", int.MaxValue);
                }
                else if (infile.Key == "replace_power")
                {
                    if (clearReplacePower)
                    {
                        Utils.LogInfo("ItemManager: Item %zu, clearing replace_power list.", id);
                        item!.ReplacePower.Clear();
                        clearReplacePower = false;
                    }
                    PowerID powerIdFirst = Parse.ToPowerID(global::FlareEngine.Parse.PopFirstString(ref infile.Val));
                    PowerID powerIdSecond = Parse.ToPowerID(global::FlareEngine.Parse.PopFirstString(ref infile.Val));
                    item!.ReplacePower.Add((powerIdFirst, powerIdSecond));
                }
                else if (infile.Key == "power_desc")
                    item!.PowerDesc = SharedResources.Msg!.Get(infile.Val);
                else if (infile.Key == "price")
                {
                    string val = infile.Val;
                    item!.Price.Parse(ref val);
                    infile.Val = val;
                }
                else if (infile.Key == "price_per_level")
                {
                    item!.Price.PerPlayerLevel = (float)Parse.ToInt(infile.Val);
                    infile.Error("ItemManager: 'price_per_level' is deprecated. Use 'price=player_level:%d' instead.", (int)item.Price.PerPlayerLevel);
                }
                else if (infile.Key == "price_sell")
                {
                    string val = infile.Val;
                    item!.PriceSell.Parse(ref val);
                    infile.Val = val;
                }
                else if (infile.Key == "max_quantity")
                    item!.MaxQuantity = Parse.ToInt(infile.Val);
                else if (infile.Key == "pickup_status")
                    item!.PickupStatus = infile.Val;
                else if (infile.Key == "stepfx")
                    item!.Stepfx = infile.Val;
                else if (infile.Key == "disable_slots")
                {
                    item!.DisableSlots.Clear();
                    string slotType = global::FlareEngine.Parse.PopFirstString(ref infile.Val);

                    while (slotType != "")
                    {
                        item.DisableSlots.Add((int)GetItemTypeIndexByString(slotType));
                        slotType = global::FlareEngine.Parse.PopFirstString(ref infile.Val);
                    }
                }
                else if (infile.Key == "quest_item")
                {
                    item!.QuestItem = global::FlareEngine.Parse.ToBool(infile.Val);

                    // for legacy reasons, quest items can't be stashed by default
                    if (item.NoStash == Item.NoStashNull)
                        item.NoStash = Item.NoStashAll;
                }
                else if (infile.Key == "no_stash")
                {
                    string temp = global::FlareEngine.Parse.PopFirstString(ref infile.Val);
                    if (temp == "ignore")
                        item!.NoStash = Item.NoStashIgnore;
                    else if (temp == "private")
                        item!.NoStash = Item.NoStashPrivate;
                    else if (temp == "shared")
                        item!.NoStash = Item.NoStashShared;
                    else if (temp == "all")
                        item!.NoStash = Item.NoStashAll;
                    else
                        infile.Error("ItemManager: '%s' is not a valid value for 'no_stash'. Use 'ignore', 'private', 'shared', or 'all'.", temp);
                }
                else if (infile.Key == "script")
                {
                    item!.Script = global::FlareEngine.Parse.PopFirstString(ref infile.Val);
                }
                else if (infile.Key == "loot_drops_max")
                {
                    item!.LootDropsMax = Parse.ToInt(infile.Val);
                }
                else if (infile.Key == "randomizer_def")
                {
                    item!.RandomizerDef = LoadRandomizerDef(infile.Val);
                }
                else if (infile.Key == "crafting_items")
                {
                    item!.CraftingItems.Clear();
                    ItemStack stack;
                    while (infile.Val != "")
                    {
                        stack = Parse.ToItemQuantityPair(global::FlareEngine.Parse.PopFirstString(ref infile.Val));
                        if (!stack.Empty())
                            item.CraftingItems.Add(stack);
                    }
                }
                else
                {
                    infile.Error("ItemManager: '%s' is not a valid key.", infile.Key);
                }

            }
            infile.Close();

            SharedResources.Eset!.Misc.CurrencyId = VerifyID(SharedResources.Eset.Misc.CurrencyId, null, !VerifyAllowZero, VerifyAllocate);

            int countAllocated = 0;
            for (int i = 0; i < Items.Count; ++i)
            {
                item = Items[i];

                if (item == null)
                    continue;
                else
                    countAllocated++;

                // normal items can be stored in either stash
                if (item.NoStash == Item.NoStashNull)
                {
                    item.NoStash = Item.NoStashIgnore;
                }

                item.UpdateLevelScaling();
            }

            int itemCount = Items.Count - 1;
            if (Items.Count == 0)
                itemCount = 0;

            Utils.LogInfo("ItemManager: Item IDs = %zu reserved / %zu allocated / %zu empty / %zu bytes used", itemCount, countAllocated, itemCount - countAllocated, (IntPtr.Size * Items.Count) + (IntPtr.Size * countAllocated));

            if (SharedResources.Eset.Loot.ExtendedItemsOffset < Items.Count)
            {
                SharedResources.Eset.Loot.ExtendedItemsOffset = (ItemID)Items.Count;
            }
            Utils.LogInfo("ItemManager: Extended item offset set to %zu.", SharedResources.Eset.Loot.ExtendedItemsOffset);
        }

        protected void LoadTypes(string filename)
        {
            FileParser infile = new FileParser();

            ItemType temp = new ItemType();
            ItemType current = temp;

            List<string> sortOrder = new List<string>();

            // blank item type at index 0
            ItemTypes.Add(new ItemType());

            // @CLASS ItemManager: Types|Definition of a item types, items/types.txt...
            if (infile.Open(filename, FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (infile.NewSection)
                    {
                        if (infile.Section == "type")
                        {
                            temp = new ItemType();
                            current = temp;
                        }
                    }

                    if (infile.Section == "settings")
                    {
                        if (infile.Key == "sort_order")
                        {
                            sortOrder.Clear();

                            string idStr = global::FlareEngine.Parse.PopFirstString(ref infile.Val);
                            while (!string.IsNullOrEmpty(idStr))
                            {
                                sortOrder.Add(idStr);
                                idStr = global::FlareEngine.Parse.PopFirstString(ref infile.Val);
                            }
                        }
                    }

                    if (infile.Section != "type")
                        continue;

                    if (infile.Key != "id" && string.IsNullOrEmpty(current.Id))
                    {
                        infile.Error("ItemManager: Expected 'id', but found '%s'.", infile.Key);
                    }

                    if (infile.Key == "id")
                    {
                        bool foundId = false;
                        for (int i = 0; i < ItemTypes.Count; ++i)
                        {
                            if (ItemTypes[i].Id == infile.Val)
                            {
                                current = ItemTypes[i];
                                foundId = true;
                            }
                        }
                        if (!foundId)
                        {
                            ItemTypes.Add(temp);
                            current = ItemTypes[^1];
                            current.Id = infile.Val;

                            if (current.Id == "consumable")
                            {
                                current.AutoActionbar = true;
                            }
                        }
                    }
                    else if (infile.Key == "name")
                        current.Name = infile.Val;
                    else if (infile.Key == "auto_pickup")
                        current.AutoPickup = global::FlareEngine.Parse.ToBool(infile.Val);
                    else if (infile.Key == "auto_actionbar")
                        current.AutoActionbar = global::FlareEngine.Parse.ToBool(infile.Val);
                    else
                        infile.Error("ItemManager: '%s' is not a valid key.", infile.Key);
                }
                infile.Close();
            }

            if (sortOrder.Count > 0)
            {
                List<ItemType> itemTypesUnsorted = new List<ItemType>(ItemTypes);
                ItemTypes.Clear();
                ItemTypes.Add(new ItemType());

                for (int i = 0; i < sortOrder.Count; ++i)
                {
                    for (int j = 0; j < itemTypesUnsorted.Count; ++j)
                    {
                        if (sortOrder[i] == itemTypesUnsorted[j].Id)
                        {
                            ItemTypes.Add(itemTypesUnsorted[j]);
                            break;
                        }
                    }
                }

                if (ItemTypes.Count != itemTypesUnsorted.Count)
                {
                    for (int i = 1; i < itemTypesUnsorted.Count; ++i)
                    {
                        if (!sortOrder.Contains(itemTypesUnsorted[i].Id))
                        {
                            ItemTypes.Add(itemTypesUnsorted[i]);
                        }
                    }
                }
            }
        }

        protected void LoadQualities(string filename)
        {
            FileParser infile = new FileParser();

            ItemQuality temp = new ItemQuality();
            ItemQuality current = temp;

            List<string> sortOrder = new List<string>();

            ItemQualities.Add(new ItemQuality());

            if (infile.Open(filename, FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (infile.NewSection)
                    {
                        if (infile.Section == "quality")
                        {
                            temp = new ItemQuality();
                            current = temp;
                        }
                    }

                    if (infile.Section == "settings")
                    {
                        if (infile.Key == "sort_order")
                        {
                            sortOrder.Clear();

                            string idStr = global::FlareEngine.Parse.PopFirstString(ref infile.Val);
                            while (!string.IsNullOrEmpty(idStr))
                            {
                                sortOrder.Add(idStr);
                                idStr = global::FlareEngine.Parse.PopFirstString(ref infile.Val);
                            }
                        }
                    }

                    if (infile.Section != "quality")
                        continue;

                    if (infile.Key != "id" && string.IsNullOrEmpty(current.Id))
                    {
                        infile.Error("ItemManager: Expected 'id', but found '%s'.", infile.Key);
                    }

                    if (infile.Key == "id")
                    {
                        bool foundId = false;
                        for (int i = 0; i < ItemQualities.Count; ++i)
                        {
                            if (ItemQualities[i].Id == infile.Val)
                            {
                                current = ItemQualities[i];
                                foundId = true;
                            }
                        }
                        if (!foundId)
                        {
                            ItemQualities.Add(temp);
                            current = ItemQualities[^1];
                            current.Id = infile.Val;
                        }
                    }
                    else if (infile.Key == "name")
                        current.Name = infile.Val;
                    else if (infile.Key == "color")
                        current.Color = Parse.ToRGB(infile.Val);
                    else if (infile.Key == "overlay_icon")
                        current.OverlayIcon = Parse.ToInt(infile.Val);
                    else
                        infile.Error("ItemManager: '%s' is not a valid key.", infile.Key);
                }
                infile.Close();
            }

            if (sortOrder.Count > 0)
            {
                List<ItemQuality> itemQualitiesUnsorted = new List<ItemQuality>(ItemQualities);
                ItemQualities.Clear();
                ItemQualities.Add(new ItemQuality());

                for (int i = 0; i < sortOrder.Count; ++i)
                {
                    for (int j = 0; j < itemQualitiesUnsorted.Count; ++j)
                    {
                        if (sortOrder[i] == itemQualitiesUnsorted[j].Id)
                        {
                            ItemQualities.Add(itemQualitiesUnsorted[j]);
                            break;
                        }
                    }
                }

                if (ItemQualities.Count != itemQualitiesUnsorted.Count)
                {
                    for (int i = 1; i < itemQualitiesUnsorted.Count; ++i)
                    {
                        if (!sortOrder.Contains(itemQualitiesUnsorted[i].Id))
                        {
                            ItemQualities.Add(itemQualitiesUnsorted[i]);
                        }
                    }
                }
            }
        }

        public string GetItemName(ItemID id)
        {
            if (!IsValid(id) || !Items[(int)id]!.HasName)
                Items[(int)id]!.Name = SharedResources.Msg!.Get("Unknown Item");

            return Items[(int)id]!.Name;
        }

        public int GetItemTypeIndexByString(string type)
        {
            for (int i = 0; i < ItemTypes.Count; ++i)
            {
                if (ItemTypes[i].Id == type)
                    return i;
            }

            int index = ItemTypes.Count;
            ItemTypes.Add(new ItemType { Id = type });

            return index;
        }

        public ItemType GetItemType(int id)
        {
            if (id >= ItemTypes.Count)
            {
                int index = GetItemTypeIndexByString("");
                return ItemTypes[index];
            }

            return ItemTypes[id];
        }

        public int GetItemQualityIndexByString(string idStr)
        {
            for (int i = 0; i < ItemQualities.Count; ++i)
            {
                if (ItemQualities[i].Id == idStr)
                    return i;
            }

            int index = ItemQualities.Count;
            ItemQualities.Add(new ItemQuality { Id = idStr });

            return index;
        }

        public bool CheckAutoPickup(ItemID id)
        {
            return GetItemType(Items[(int)id]!.Type).AutoPickup;
        }

        public Color GetItemColor(ItemID id)
        {
            if (IsValid(id))
            {
                if (Items[(int)id]!.Set > 0)
                {
                    return ItemSets[(int)Items[(int)id]!.Set]!.Color;
                }
                else if (Items[(int)id]!.Quality < ItemQualities.Count)
                {
                    return ItemQualities[Items[(int)id]!.Quality].Color;
                }
            }

            return SharedResources.Font!.GetColor(FontEngine.ColorWidgetNormal);
        }

        public int GetItemIconOverlay(int id)
        {
            if (Items[id]!.Quality < ItemQualities.Count)
            {
                return ItemQualities[Items[id]!.Quality].OverlayIcon;
            }

            return -1;
        }

        protected void LoadSets(string filename)
        {
            FileParser infile = new FileParser();

            if (!infile.Open(filename, FileParser.ModFile, FileParser.ErrorNormal))
                return;

            bool clearBonus = true;

            ItemSet? itemSet = null;
            ItemSetID id = 0;
            bool idLine;
            while (infile.Next())
            {
                if (infile.Key == "id")
                {
                    idLine = true;
                    id = (ItemSetID)Parse.ToSizeT(infile.Val);

                    if (id < ItemSets.Count && ItemSets[(int)id] != null)
                    {
                        clearBonus = true;
                    }
                    else
                    {
                        int targetSize = Math.Max((int)id + 1, ItemSets.Count);
                        while (ItemSets.Count < targetSize)
                            ItemSets.Add(null);
                        ItemSets[(int)id] = new ItemSet();
                    }
                    itemSet = ItemSets[(int)id];
                }
                else idLine = false;

                if (id < 1)
                {
                    if (idLine) infile.Error("ItemManager: Item set index out of bounds 1-%d, skipping set.", int.MaxValue);
                    continue;
                }
                if (idLine) continue;

                if (infile.Key == "name")
                {
                    itemSet!.Name = SharedResources.Msg!.Get(infile.Val);
                }
                else if (infile.Key == "items")
                {
                    itemSet!.Items.Clear();
                    string itemStr = global::FlareEngine.Parse.PopFirstString(ref infile.Val);
                    while (!string.IsNullOrEmpty(itemStr))
                    {
                        ItemID itemId = VerifyID(Parse.ToItemID(itemStr), infile, !VerifyAllowZero, !VerifyAllocate);
                        if (itemId > 0)
                        {
                            Items[(int)itemId]!.Set = id;
                            itemSet.Items.Add(itemId);
                        }
                        itemStr = global::FlareEngine.Parse.PopFirstString(ref infile.Val);
                    }
                }
                else if (infile.Key == "color")
                {
                    itemSet!.Color = Parse.ToRGB(infile.Val);
                }
                else if (infile.Key == "bonus")
                {
                    if (clearBonus)
                    {
                        itemSet!.Bonus.Clear();
                        clearBonus = false;
                    }
                    SetBonusData bonus = new SetBonusData();
                    bonus.Requirement = Parse.PopFirstInt(ref infile.Val);
                    ParseBonus(bonus, infile);
                    itemSet!.Bonus.Add(bonus);
                }
                else if (infile.Key == "bonus_power_level")
                {
                    SetBonusData bonus = new SetBonusData();
                    bonus.Type = BonusData.PowerLevel;
                    bonus.Requirement = Parse.PopFirstInt(ref infile.Val);
                    bonus.PowerId = Parse.ToPowerID(global::FlareEngine.Parse.PopFirstString(ref infile.Val));
                    string val = infile.Val;
                    bonus.Value.Parse(ref val);
                    infile.Val = val;
                    itemSet!.Bonus.Add(bonus);
                }
                else
                {
                    infile.Error("ItemManager: '%s' is not a valid key.", infile.Key);
                }
            }
            infile.Close();

            int countAllocated = 0;
            for (int i = 0; i < ItemSets.Count; ++i)
            {
                itemSet = ItemSets[i];

                if (itemSet == null)
                    continue;
                else
                    countAllocated++;
            }

            int itemSetCount = ItemSets.Count - 1;
            if (ItemSets.Count == 0)
                itemSetCount = 0;

            Utils.LogInfo("ItemManager: Item Set IDs = %zu reserved / %zu allocated / %zu empty / %zu bytes used", itemSetCount, countAllocated, itemSetCount - countAllocated, (IntPtr.Size * ItemSets.Count) + (IntPtr.Size * countAllocated));
        }

        private void ParseBonus(BonusData bdata, FileParser infile)
        {
            string bonusStr = global::FlareEngine.Parse.PopFirstString(ref infile.Val);
            string bonusValueStr = "";

            for (int i = 0; i < infile.Val.Length; ++i)
            {
                if (infile.Val[i] == '%')
                {
                    bdata.IsMultiplier = true;
                }
                else
                {
                    bonusValueStr += infile.Val[i];
                }
            }

            string parseVal = bonusValueStr;
            bdata.Value.Parse(ref parseVal);

            if (bdata.IsMultiplier)
            {
                bdata.Value.Base /= 100;
                bdata.Value.BaseMax /= 100;
                bdata.Value.BaseStep /= 100;
                bdata.Value.PerItemLevel /= 100;
                bdata.Value.PerItemLevelMax /= 100;
                bdata.Value.PerItemLevelStep /= 100;
                bdata.Value.PerPlayerLevel /= 100;
                bdata.Value.PerPlayerLevelMax /= 100;
                bdata.Value.PerPlayerLevelStep /= 100;
                for (int i = 0; i < bdata.Value.PerPlayerPrimary.Count; ++i)
                {
                    bdata.Value.PerPlayerPrimary[i] /= 100;
                    bdata.Value.PerPlayerPrimaryMax[i] /= 100;
                    bdata.Value.PerPlayerPrimaryStep[i] /= 100;
                }
            }

            if (bonusStr == "speed")
            {
                bdata.Type = BonusData.Speed;
                return;
            }
            else if (bonusStr == "attack_speed")
            {
                bdata.Type = BonusData.AttackSpeed;
                return;
            }

            if (bonusStr == "hp_percent")
            {
                infile.Error("ItemManager: 'hp_percent' is deprecated. Converting to 'hp'.");
                bdata.Type = BonusData.Stat;
                bdata.Index = Stats.HpMax;
                bdata.IsMultiplier = true;
                bdata.Value.Base = bdata.Value.BaseMax = (bdata.Value.Base + 100) / 100;
                return;
            }
            else if (bonusStr == "mp_percent")
            {
                infile.Error("ItemManager: 'mp_percent' is deprecated. Converting to 'mp'.");
                bdata.Type = BonusData.Stat;
                bdata.Index = Stats.MpMax;
                bdata.IsMultiplier = true;
                bdata.Value.Base = bdata.Value.BaseMax = (bdata.Value.Base + 100) / 100;
                return;
            }

            for (int i = 0; i < Stats.Count; ++i)
            {
                if (bonusStr == Stats.Key[i])
                {
                    bdata.Type = BonusData.Stat;
                    bdata.Index = i;
                    return;
                }
            }

            for (int i = 0; i < SharedResources.Eset!.DamageTypes.Types.Count; ++i)
            {
                if (bonusStr == SharedResources.Eset.DamageTypes.Types[i].Min)
                {
                    bdata.Type = BonusData.DamageMin;
                    bdata.Index = i;
                    return;
                }
                else if (bonusStr == SharedResources.Eset.DamageTypes.Types[i].Max)
                {
                    bdata.Type = BonusData.DamageMax;
                    bdata.Index = i;
                    return;
                }
                else if (bonusStr == SharedResources.Eset.DamageTypes.Types[i].Resist)
                {
                    bdata.Type = BonusData.ResistElement;
                    bdata.Index = i;
                    return;
                }
            }

            for (int i = 0; i < SharedResources.Eset.PrimaryStats.Stats.Count; ++i)
            {
                if (bonusStr == SharedResources.Eset.PrimaryStats.Stats[i].Id)
                {
                    bdata.Type = BonusData.PrimaryStat;
                    bdata.Index = i;
                    if (bdata.IsMultiplier)
                    {
                        bdata.IsMultiplier = false;
                        infile.Error("ItemManager: Primary stat bonus can't be percentage.");
                    }
                    return;
                }
            }

            for (int i = 0; i < SharedResources.Eset.ResourceStats.Stats.Count; ++i)
            {
                for (int j = 0; j < EngineSettings.ResourceStatsSettings.StatCount; ++j)
                {
                    if (bonusStr == SharedResources.Eset.ResourceStats.Stats[i].Ids[j])
                    {
                        bdata.Type = BonusData.ResourceStat;
                        bdata.Index = i;
                        bdata.SubIndex = j;
                        return;
                    }
                }
            }

            infile.Error("ItemManager: Unknown bonus type '%s'.", bonusStr);
        }

        private void GetBonusString(StringBuilder ss, BonusData bdata)
        {
            float scaledBdataValue = bdata.Value.Get();

            if (bdata.PowerId > 0)
            {
                scaledBdataValue = (float)(int)scaledBdataValue;
            }

            if (bdata.Type == BonusData.Speed)
            {
                ss.Append(SharedResources.Msg!.GetV("%s%% Speed", Utils.FloatToString(scaledBdataValue, SharedResources.Eset!.NumberFormat.ItemTooltips)));
                return;
            }
            else if (bdata.Type == BonusData.AttackSpeed)
            {
                ss.Append(SharedResources.Msg!.GetV("%s%% Attack Speed", Utils.FloatToString(scaledBdataValue, SharedResources.Eset!.NumberFormat.ItemTooltips)));
                return;
            }

            if (bdata.IsMultiplier)
                ss.Append(Utils.FloatToString(scaledBdataValue, SharedResources.Eset!.NumberFormat.ItemTooltips + 2)).Append('×');
            else if (scaledBdataValue > 0)
                ss.Append('+').Append(Utils.FloatToString(scaledBdataValue, SharedResources.Eset!.NumberFormat.ItemTooltips));
            else
                ss.Append(Utils.FloatToString(scaledBdataValue, SharedResources.Eset!.NumberFormat.ItemTooltips));

            if (bdata.Type == BonusData.Stat)
            {
                if (!bdata.IsMultiplier && Stats.Percent[bdata.Index])
                    ss.Append('%');

                ss.Append(' ').Append(Stats.Name[bdata.Index]);
            }
            else if (bdata.Type == BonusData.DamageMin)
            {
                ss.Append(' ').Append(SharedResources.Eset!.DamageTypes.Types[bdata.Index].NameMin);
            }
            else if (bdata.Type == BonusData.DamageMax)
            {
                ss.Append(' ').Append(SharedResources.Eset!.DamageTypes.Types[bdata.Index].NameMax);
            }
            else if (bdata.Type == BonusData.ResistElement)
            {
                if (!bdata.IsMultiplier)
                    ss.Append('%');

                ss.Append(' ').Append(SharedResources.Eset!.DamageTypes.Types[bdata.Index].NameResist);
            }
            else if (bdata.Type == BonusData.PrimaryStat)
            {
                ss.Append(' ').Append(SharedResources.Eset!.PrimaryStats.Stats[bdata.Index].Name);
            }
            else if (SharedGameResources.Powers != null && SharedGameResources.Powers!.IsValid(bdata.PowerId))
            {
                ss.Append(' ').Append(SharedGameResources.Powers!.Powers[(int)bdata.PowerId]!.Name);
                if (SharedGameResources.Menu != null && SharedGameResources.Menu!.Pow != null)
                {
                    string reqStr = SharedGameResources.Menu.Pow!.GetItemBonusPowerReqString(bdata.PowerId);
                    if (!string.IsNullOrEmpty(reqStr))
                        ss.Append(" (").Append(SharedResources.Msg!.GetV("Requires %s", reqStr)).Append(')');
                }
            }
            else if (bdata.Type == BonusData.ResourceStat)
            {
                ss.Append(' ').Append(SharedResources.Eset!.ResourceStats.Stats[bdata.Index].Text[bdata.SubIndex]);
            }
        }

        public void PlaySound(ItemID item, Int2 pos = default)
        {
            if (!IsValid(item))
                return;

            string channelName = "item_" + Items[(int)item]!.SfxId;
            SharedResources.Snd!.Play(Items[(int)item]!.SfxId, channelName, pos.ToVector2(), false);
        }

        public TooltipData GetShortTooltip(ItemStack stack)
        {
            StringBuilder ss = new StringBuilder();
            TooltipData tip = new TooltipData();

            if (stack.Empty() || !IsValid(stack.Item)) return tip;

            if (stack.Quantity > 1)
            {
                ss.Append(GetItemName(stack.Item)).Append(" (").Append(stack.Quantity).Append(')');
            }
            else
            {
                ss.Append(GetItemName(stack.Item));
            }
            tip.AddColoredText(ss.ToString(), GetItemColor(stack.Item));

            return tip;
        }

        public TooltipData GetTooltip(ItemStack stack, StatBlock? stats, int context, bool inputHint)
        {
            TooltipData tip = new TooltipData();

            if (stack.Empty() || !IsValid(stack.Item))
                return tip;

            Item item = Items[(int)stack.Item]!;

            Color color = GetItemColor(stack.Item);

            StringBuilder ss = new StringBuilder();
            if (stack.Quantity == 1)
                ss.Append(GetItemName(stack.Item));
            else
                ss.Append(GetItemName(stack.Item)).Append(" (").Append(stack.Quantity).Append(')');
            tip.AddColoredText(ss.ToString(), color);

            if (item.QuestItem)
            {
                tip.AddColoredText(SharedResources.Msg!.Get("Quest Item"), SharedResources.Font!.GetColor(FontEngine.ColorItemBonus));
            }

            if (stack.Item == SharedResources.Eset!.Misc.CurrencyId)
            {
                if (inputHint)
                    GetTooltipInputHint(ref tip, stack, context);
                return tip;
            }

            if (item.Level != 0)
            {
                tip.AddText(SharedResources.Msg!.GetV("Level %d", item.Level));
            }

            ItemType itemType = GetItemType(item.Type);
            if (!string.IsNullOrEmpty(itemType.Name))
            {
                tip.AddText(SharedResources.Msg!.Get(itemType.Name));
            }

            if (item.DisableSlots.Count > 0)
            {
                for (int i = 0; i < item.DisableSlots.Count; ++i)
                {
                    ItemType disableType = GetItemType(item.DisableSlots[i]);
                    if (!string.IsNullOrEmpty(disableType.Name))
                    {
                        tip.AddColoredText(SharedResources.Msg!.GetV("Prevents use of slot: %s", disableType.Name), SharedResources.Font!.GetColor(FontEngine.ColorWidgetDisabled));
                    }
                }
            }

            if (SharedResources.Settings!.Colorblind && item.Quality < ItemQualities.Count && !string.IsNullOrEmpty(ItemQualities[item.Quality].Name))
            {
                color = SharedResources.Font!.GetColor(FontEngine.ColorWidgetNormal);
                tip.AddColoredText(SharedResources.Msg!.GetV("Quality: %s", SharedResources.Msg!.Get(ItemQualities[item.Quality].Name)), color);
            }

            for (int i = 0; i < SharedResources.Eset!.DamageTypes.Types.Count; ++i)
            {
                if (item.BaseDmg[i].Max.Get() > 0)
                {
                    StringBuilder dmgStr = new StringBuilder();
                    dmgStr.Append(SharedResources.Eset.DamageTypes.Types[i].Name);
                    dmgStr.Append(": ").Append(Utils.CreateMinMaxString(item.BaseDmg[i].Min.Get(), item.BaseDmg[i].Max.Get(), SharedResources.Eset.NumberFormat.ItemTooltips));
                    tip.AddText(dmgStr.ToString());
                }
            }

            if (item.BaseAbs.Max.Get() > 0)
            {
                StringBuilder absStr = new StringBuilder();
                absStr.Append(SharedResources.Msg!.Get("Absorb"));
                absStr.Append(": ").Append(Utils.CreateMinMaxString(item.BaseAbs.Min.Get(), item.BaseAbs.Max.Get(), SharedResources.Eset!.NumberFormat.ItemTooltips));
                tip.AddText(absStr.ToString());
            }

            int bonusCounter = 0;
            while (bonusCounter < item.Bonus.Count)
            {
                ss.Clear();

                BonusData bdata = item.Bonus[bonusCounter];

                float scaledBdataValue = bdata.Value.Get();

                if (bdata.Type == BonusData.Speed || bdata.Type == BonusData.AttackSpeed)
                {
                    if (scaledBdataValue >= 100)
                        color = SharedResources.Font!.GetColor(FontEngine.ColorItemBonus);
                    else
                        color = SharedResources.Font!.GetColor(FontEngine.ColorItemPenalty);
                }
                else if (bdata.IsMultiplier)
                {
                    if (scaledBdataValue >= 1)
                        color = SharedResources.Font!.GetColor(FontEngine.ColorItemBonus);
                    else
                        color = SharedResources.Font!.GetColor(FontEngine.ColorItemPenalty);
                }
                else
                {
                    if (scaledBdataValue > 0)
                        color = SharedResources.Font!.GetColor(FontEngine.ColorItemBonus);
                    else
                        color = SharedResources.Font!.GetColor(FontEngine.ColorItemPenalty);
                }

                GetBonusString(ss, bdata);
                tip.AddColoredText(ss.ToString(), color);
                bonusCounter++;
            }

            if (!string.IsNullOrEmpty(item.PowerDesc))
            {
                tip.AddColoredText(item.PowerDesc, SharedResources.Font!.GetColor(FontEngine.ColorItemBonus));
            }

            int scaledRequiresLevel = (int)item.RequiresLevel.Get();
            if (scaledRequiresLevel > 0)
            {
                if (stats!.Level < scaledRequiresLevel)
                    color = SharedResources.Font!.GetColor(FontEngine.ColorRequirementsNotMet);
                else
                    color = SharedResources.Font!.GetColor(FontEngine.ColorWidgetNormal);

                tip.AddColoredText(SharedResources.Msg!.GetV("Requires Level %d", scaledRequiresLevel), color);
            }

            for (int i = 0; i < SharedResources.Eset!.PrimaryStats.Stats.Count; ++i)
            {
                int scaledRequiresPrimary = (int)item.RequiresStat[i].Get();
                if (scaledRequiresPrimary > 0)
                {
                    if (stats!.GetPrimary(i) < scaledRequiresPrimary)
                        color = SharedResources.Font!.GetColor(FontEngine.ColorRequirementsNotMet);
                    else
                        color = SharedResources.Font!.GetColor(FontEngine.ColorWidgetNormal);

                    tip.AddColoredText(SharedResources.Msg!.GetV("Requires %s %d", SharedResources.Eset.PrimaryStats.Stats[i].Name, scaledRequiresPrimary), color);
                }
            }

            if (!string.IsNullOrEmpty(item.RequiresClass))
            {
                if (item.RequiresClass != stats!.CharacterClass)
                    color = SharedResources.Font!.GetColor(FontEngine.ColorRequirementsNotMet);
                else
                    color = SharedResources.Font!.GetColor(FontEngine.ColorWidgetNormal);

                tip.AddColoredText(SharedResources.Msg!.GetV("Requires Class: %s", SharedResources.Msg!.Get(item.RequiresClass)), color);
            }

            if (!string.IsNullOrEmpty(item.Flavor))
            {
                tip.AddColoredText(Utils.SubstituteVarsInString(item.Flavor, SharedGameResources.Pc), SharedResources.Font!.GetColor(FontEngine.ColorItemFlavor));
            }

            if (item.GetPrice(UseVendorRatio) > 0 && stack.Item != SharedResources.Eset!.Misc.CurrencyId)
            {
                Color currencyColor = GetItemColor(SharedResources.Eset.Misc.CurrencyId);

                int pricePerUnit;
                if (context == VendorBuy)
                {
                    pricePerUnit = item.GetPrice(UseVendorRatio);
                    if (stats!.Currency < pricePerUnit)
                        color = SharedResources.Font!.GetColor(FontEngine.ColorRequirementsNotMet);
                    else
                        color = currencyColor;

                    if (item.MaxQuantity <= 1)
                        tip.AddColoredText(SharedResources.Msg!.GetV("Buy Price: %d %s", pricePerUnit, SharedResources.Eset.Loot.Currency), color);
                    else
                        tip.AddColoredText(SharedResources.Msg!.GetV("Buy Price: %d %s each", pricePerUnit, SharedResources.Eset.Loot.Currency), color);
                }
                else if (context == VendorSell)
                {
                    pricePerUnit = item.GetSellPrice(stack.CanBuyback);
                    if (stats!.Currency < pricePerUnit)
                        color = SharedResources.Font!.GetColor(FontEngine.ColorRequirementsNotMet);
                    else
                        color = currencyColor;

                    if (item.MaxQuantity <= 1)
                        tip.AddColoredText(SharedResources.Msg!.GetV("Buy Price: %d %s", pricePerUnit, SharedResources.Eset.Loot.Currency), color);
                    else
                        tip.AddColoredText(SharedResources.Msg!.GetV("Buy Price: %d %s each", pricePerUnit, SharedResources.Eset.Loot.Currency), color);
                }
                else if (context == VendorCraft)
                {
                    if (item.CraftingItems.Count > 0)
                    {
                        tip.AddColoredText("\n" + SharedResources.Msg!.Get("Crafting requires:"), currencyColor);
                    }
                    for (int i = 0; i < item.CraftingItems.Count; ++i)
                    {
                        ItemStack craftStack = item.CraftingItems[i];
                        ss.Clear();
                        if (craftStack.Quantity == 1)
                            ss.Append(GetItemName(craftStack.Item));
                        else
                            ss.Append(GetItemName(craftStack.Item)).Append(" (").Append(craftStack.Quantity).Append(')');

                        if (SharedGameResources.Camp == null || (SharedGameResources.Camp != null && SharedGameResources.Camp!.CheckItem(craftStack)))
                            color = currencyColor;
                        else
                            color = SharedResources.Font!.GetColor(FontEngine.ColorRequirementsNotMet);

                        tip.AddColoredText(ss.ToString(), color);
                    }
                }
                else if (context == PlayerInv)
                {
                    pricePerUnit = item.GetSellPrice(DefaultSellPrice);
                    if (pricePerUnit == 0)
                        pricePerUnit = 1;

                    if (item.MaxQuantity <= 1)
                        tip.AddColoredText(SharedResources.Msg!.GetV("Sell Price: %d %s", pricePerUnit, SharedResources.Eset.Loot.Currency), currencyColor);
                    else
                        tip.AddColoredText(SharedResources.Msg!.GetV("Sell Price: %d %s each", pricePerUnit, SharedResources.Eset.Loot.Currency), currencyColor);
                }
            }

            if (item.Set > 0)
            {
                int setCount = SharedGameResources.Menu!.Inv!.GetEquippedSetCount(item.Set);

                ItemSet itemSet = ItemSets[(int)item.Set]!;
                bonusCounter = 0;

                tip.AddColoredText("\n" + SharedResources.Msg!.Get("Set:") + ' ' + SharedResources.Msg!.Get(itemSet.Name), itemSet.Color);

                while (bonusCounter < itemSet.Bonus.Count)
                {
                    ss.Clear();

                    SetBonusData bdata = itemSet.Bonus[bonusCounter];

                    ss.Append('[').Append(bdata.Requirement).Append("]: ");

                    GetBonusString(ss, bdata);
                    if (bdata.Requirement <= setCount)
                        tip.AddColoredText(ss.ToString(), itemSet.Color);
                    else
                        tip.AddColoredText(ss.ToString(), SharedResources.Font!.GetColor(FontEngine.ColorWidgetDisabled));
                    bonusCounter++;
                }
            }

            if (inputHint)
                GetTooltipInputHint(ref tip, stack, context);

            return tip;
        }

        private void GetTooltipInputHint(ref TooltipData tip, ItemStack stack, int context)
        {
            bool showActivateMsg = false;
            string activateBindStr = "";

            bool showMoreMsg = false;
            string moreBindStr = "";

            if (SharedResources.Inpt!.Mode == InputState.ModeTouchscreen)
            {
                tip.AddColoredText('\n' + SharedResources.Msg!.Get("Tap icon again for more options"), SharedResources.Font!.GetColor(FontEngine.ColorItemBonus));
            }
            else if (SharedResources.Inpt.Mode == InputState.ModeJoystick)
            {
                if (context == PlayerInv && SharedGameResources.Menu!.Inv!.CanActivateItem(stack.Item))
                {
                    showActivateMsg = true;
                    activateBindStr = SharedResources.Inpt.GetGamepadBindingString(Input.MenuActivate);
                }
                showMoreMsg = true;
                moreBindStr = SharedResources.Inpt.GetGamepadBindingString(Input.Accept);
            }
            else if (!SharedResources.Inpt.UsingMouse())
            {
                if (context == PlayerInv && SharedGameResources.Menu!.Inv!.CanActivateItem(stack.Item))
                {
                    showActivateMsg = true;
                    activateBindStr = SharedResources.Inpt.GetBindingString(Input.MenuActivate);
                }
                showMoreMsg = true;
                moreBindStr = SharedResources.Inpt.GetBindingString(Input.Accept);
            }
            else
            {
                if (context == PlayerInv && SharedGameResources.Menu!.Inv!.CanActivateItem(stack.Item))
                {
                    showActivateMsg = true;
                    activateBindStr = SharedResources.Inpt.GetBindingString(Input.Main2);
                }
            }

            if (showActivateMsg || showMoreMsg)
            {
                tip.AddText("");
            }

            if (showActivateMsg)
            {
                if (Items[(int)stack.Item] != null && !string.IsNullOrEmpty(Items[(int)stack.Item]!.Book) && Items[(int)stack.Item]!.BookIsReadable)
                {
                    tip.AddColoredText(SharedResources.Msg!.GetV("Press [%s] to read", activateBindStr), SharedResources.Font!.GetColor(FontEngine.ColorItemBonus));
                }
                else if (SharedGameResources.Menu!.Inv!.CanActivateItem(stack.Item))
                {
                    tip.AddColoredText(SharedResources.Msg!.GetV("Press [%s] to use", activateBindStr), SharedResources.Font!.GetColor(FontEngine.ColorItemBonus));
                }
            }
            if (showMoreMsg)
            {
                tip.AddColoredText(SharedResources.Msg!.GetV("Press [%s] for more options", moreBindStr), SharedResources.Font!.GetColor(FontEngine.ColorItemBonus));
            }
        }

        public bool RequirementsMet(StatBlock? stats, ItemID itemId)
        {
            if (stats == null || !IsValid(itemId))
                return false;

            Item item = Items[(int)itemId]!;

            int scaledRequiresLevel = (int)item.RequiresLevel.Get();
            if (scaledRequiresLevel > 0 && stats.Level < scaledRequiresLevel)
            {
                return false;
            }

            for (int i = 0; i < SharedResources.Eset!.PrimaryStats.Stats.Count; i++)
            {
                if (stats.GetPrimary(i) < (int)item.RequiresStat[i].Get())
                    return false;
            }

            if (!string.IsNullOrEmpty(item.RequiresClass) && item.RequiresClass != stats.CharacterClass)
            {
                return false;
            }

            return true;
        }

        public ItemID VerifyID(ItemID itemId, FileParser? infile, bool allowZero, bool allocate)
        {
            if ((!allowZero && itemId == 0) || itemId >= Items.Count || (itemId > 0 && Items[(int)itemId] == null))
            {
                if (infile != null)
                    infile.Error("ItemManager: %zu is not a valid item id.", itemId);
                else
                    Utils.LogError("ItemManager: %zu is not a valid item id.", itemId);

                if (itemId > 0 && allocate)
                {
                    if (itemId >= Items.Count)
                    {
                        while (Items.Count < (int)itemId + 1)
                            Items.Add(null);
                    }
                    Items[(int)itemId] = new Item();
                    Utils.LogInfo("ItemManager: Allocated unknown item, %zu.", itemId);
                    return itemId;
                }

                return 0;
            }
            return itemId;
        }

        private ItemRandomizerDef? LoadRandomizerDef(string filename)
        {
            for (int i = 0; i < _randomizerDefs.Count; ++i)
            {
                if (_randomizerDefs[i].Filename == filename)
                    return _randomizerDefs[i];
            }

            FileParser infile = new FileParser();

            if (!infile.Open(filename, FileParser.ModFile, FileParser.ErrorNormal))
                return null;

            _randomizerDefs.Add(new ItemRandomizerDef());
            ItemRandomizerDef ird = _randomizerDefs[^1];
            ird.Filename = filename;

            ItemRandomizerDef.Option? option = null;

            while (infile.Next())
            {
                if (infile.Section == "option")
                {
                    if (infile.NewSection)
                    {
                        ird.Options.Add(new ItemRandomizerDef.Option());
                        option = ird.Options[^1];
                    }
                    if (option != null)
                    {
                        if (infile.Key == "chance")
                        {
                            option.Chance = global::FlareEngine.Parse.ToFloat(infile.Val);
                        }
                        else if (infile.Key == "quality")
                        {
                            option.Quality = GetItemQualityIndexByString(infile.Val);
                        }
                        else if (infile.Key == "bonus_count")
                        {
                            option.BonusMin = Parse.PopFirstInt(ref infile.Val);
                            option.BonusMax = Parse.PopFirstInt(ref infile.Val);
                            option.BonusMax = Math.Max(option.BonusMin, option.BonusMax);
                        }
                        else if (infile.Key == "level_src")
                        {
                            if (infile.Val == "base")
                                option.LevelSrc = ItemRandomizerDef.Option.LevelSrcBase;
                            else if (infile.Val == "hero")
                                option.LevelSrc = ItemRandomizerDef.Option.LevelSrcHero;
                        }
                        else if (infile.Key == "level_range")
                        {
                            option.LevelRangeMin = Parse.PopFirstInt(ref infile.Val);
                            option.LevelRangeMax = Parse.PopFirstInt(ref infile.Val);
                        }
                    }
                }
                else if (infile.Section == "bonuses")
                {
                    if (infile.Key == "bonus")
                    {
                        BonusData bdata = new BonusData();
                        ParseBonus(bdata, infile);
                        ird.Bonus.Add(bdata);
                    }
                    else if (infile.Key == "bonus_power_level")
                    {
                        BonusData bdata = new BonusData();
                        bdata.Type = BonusData.PowerLevel;
                        bdata.PowerId = Parse.ToPowerID(global::FlareEngine.Parse.PopFirstString(ref infile.Val));
                        string val = infile.Val;
                        bdata.Value.Parse(ref val);
                        infile.Val = val;
                        ird.Bonus.Add(bdata);
                    }
                }
            }
            infile.Close();

            return ird;
        }

        private ItemID AllocateExtendedItem(ItemID itemId, ItemID parentId)
        {
            int startId = (int)SharedResources.Eset!.Loot.ExtendedItemsOffset;
            if (itemId != 0)
                startId = Math.Max(startId, (int)itemId);

            if (Items.Count <= startId)
            {
                while (Items.Count < startId + 1)
                    Items.Add(null);
            }

            ItemID extendedItem = 0;
            if (itemId == 0)
            {
                for (int i = startId; i < Items.Count; ++i)
                {
                    if (Items[i] == null)
                    {
                        extendedItem = (ItemID)i;
                        break;
                    }
                }
                if (extendedItem == 0)
                {
                    extendedItem = (ItemID)Items.Count;
                    Items.Add(null);
                }
            }
            else
            {
                extendedItem = itemId;
            }

            Items[(int)extendedItem] = new Item();
            Items[(int)extendedItem]!.CopyFrom(Items[(int)parentId]!);
            Items[(int)extendedItem]!.Parent = parentId;

            return extendedItem;
        }

        public ItemID GetExtendedItem(ItemID itemId)
        {
            if (itemId >= Items.Count || Items[(int)itemId] == null)
            {
                return itemId;
            }

            if (Items[(int)itemId]!.RandomizerDef != null)
            {
                ItemID extendedItem = AllocateExtendedItem(0, itemId);

                ItemRandomizerDef ird = Items[(int)itemId]!.RandomizerDef!;

                List<int> optionIds = new List<int>();
                float optionChance = MathUtils.RandBetweenF(0, 100);
                float optionThreshold = 100;
                int bonusCount = 0;

                for (int i = 0; i < ird.Options.Count; ++i)
                {
                    if (ird.Options[i].Chance >= optionChance)
                    {
                        if (optionChance <= optionThreshold)
                        {
                            if (optionChance != optionThreshold)
                            {
                                optionIds.Clear();
                            }

                            optionThreshold = optionChance;
                        }
                    }

                    if (optionChance <= optionThreshold)
                    {
                        optionIds.Add(i);
                    }
                }
                if (optionIds.Count > 0)
                {
                    int optionRoll = Program.Rng.Next() % optionIds.Count;
                    ItemRandomizerDef.Option option = ird.Options[optionRoll];
                    bonusCount = MathUtils.RandBetween(option.BonusMin, option.BonusMax);
                    if (option.Quality < ItemQualities.Count && !string.IsNullOrEmpty(ItemQualities[option.Quality].Name))
                        Items[(int)extendedItem]!.Quality = option.Quality;

                    if (option.LevelSrc == ItemRandomizerDef.Option.LevelSrcBase)
                    {
                        int min = Math.Max(1, Items[(int)itemId]!.Level + option.LevelRangeMin);
                        int max = Math.Max(min, Items[(int)itemId]!.Level + option.LevelRangeMax);

                        Items[(int)extendedItem]!.Level = MathUtils.RandBetween(min, max);
                    }
                    else if (option.LevelSrc == ItemRandomizerDef.Option.LevelSrcHero)
                    {
                        int min = Math.Max(1, SharedGameResources.Pc!.Stats.Level + option.LevelRangeMin);
                        int max = Math.Min(SharedResources.Eset!.Xp.GetMaxLevel(), Math.Max(min, SharedGameResources.Pc!.Stats.Level + option.LevelRangeMax));

                        Items[(int)extendedItem]!.Level = MathUtils.RandBetween(min, max);
                    }

                }

                Items[(int)extendedItem]!.UpdateLevelScaling();

                Items[(int)extendedItem]!.RequiresLevel.Randomize();
                Items[(int)extendedItem]!.Price.Randomize();
                Items[(int)extendedItem]!.PriceSell.Randomize();

                Items[(int)extendedItem]!.BaseAbs.Min.Randomize();
                Items[(int)extendedItem]!.BaseAbs.Max.Randomize();

                for (int i = 0; i < SharedResources.Eset!.DamageTypes.Types.Count; ++i)
                {
                    Items[(int)extendedItem]!.BaseDmg[i].Min.Randomize();
                    Items[(int)extendedItem]!.BaseDmg[i].Max.Randomize();
                }

                for (int i = 0; i < SharedResources.Eset.PrimaryStats.Stats.Count; ++i)
                {
                    Items[(int)extendedItem]!.RequiresStat[i].Randomize();
                }

                bonusCount = Math.Min(bonusCount, ird.Bonus.Count);

                List<int> bonusIds = new List<int>();
                for (int bi = 0; bi < ird.Bonus.Count; ++bi)
                {
                    bonusIds.Add(bi);
                }

                for (int i = 0; i < bonusCount; ++i)
                {
                    int roll = MathUtils.RandBetween(0, bonusIds.Count - 1);
                    int bonusId = bonusIds[roll];

                    BonusData bdata = ird.Bonus[bonusId];
                    bdata.IsExtended = true;
                    bdata.Value.ItemLevel = Items[(int)extendedItem]!.Level;
                    bdata.Value.Randomize();

                    Items[(int)extendedItem]!.Bonus.Add(bdata);

                    bonusIds.RemoveAt(roll);
                }

                Items[(int)extendedItem]!.RandomizerDef = null;

                Items[(int)extendedItem]!.IsForeign = false;

                return extendedItem;
            }
            else
            {
                return itemId;
            }
        }

        protected void LoadExtendedItems(string filename)
        {
            FileParser infile = new FileParser();

            if (!infile.Open(filename, !FileParser.ModFile, FileParser.ErrorNone))
                return;

            ItemID id = 0;
            Item? item = null;
            bool idLine;
            while (infile.Next())
            {
                if (infile.Key == "id")
                {
                    idLine = true;

                    ItemID parsedItemId = Parse.ToItemID(global::FlareEngine.Parse.PopFirstString(ref infile.Val));

                    if (parsedItemId < Items.Count && Items[(int)parsedItemId] != null)
                    {
                        infile.Error("ItemManager: Existing item with ID %zu found when loading extended items. Skipping.", parsedItemId);
                        continue;
                    }

                    ItemID parentId = VerifyID(Parse.ToItemID(global::FlareEngine.Parse.PopFirstString(ref infile.Val)), infile, !VerifyAllowZero, !VerifyAllocate);

                    if (parentId == 0)
                        continue;

                    id = AllocateExtendedItem(parsedItemId, parentId);
                    item = Items[(int)parsedItemId];
                }
                else
                {
                    idLine = false;
                }
                if (id < 1)
                {
                    if (idLine) infile.Error("ItemManager: Item index out of bounds 1-%d, skipping set.", int.MaxValue);
                    continue;
                }
                if (idLine) continue;

                if (infile.Key == "level")
                {
                    item!.Level = Parse.ToInt(infile.Val);
                }
                else if (infile.Key == "quality")
                {
                    item!.Quality = GetItemQualityIndexByString(infile.Val);
                }
                else if (infile.Key == "requires_level")
                {
                    string val = infile.Val;
                    item!.RequiresLevel.Parse(ref val);
                    infile.Val = val;

                    item.RequiresLevel.Randomized = true;
                }
                else if (infile.Key == "requires_stat")
                {
                    string statId = global::FlareEngine.Parse.PopFirstString(ref infile.Val);
                    int reqStatIndex = SharedResources.Eset!.PrimaryStats.GetIndexByID(statId);

                    if (reqStatIndex < SharedResources.Eset.PrimaryStats.Stats.Count)
                    {
                        string val = infile.Val;
                        item!.RequiresStat[reqStatIndex].Parse(ref val);
                        infile.Val = val;
                        item.RequiresStat[reqStatIndex].Randomized = true;
                    }
                }
                else if (infile.Key == "price")
                {
                    string val = infile.Val;
                    item!.Price.Parse(ref val);
                    infile.Val = val;
                    item.Price.Randomized = true;
                }
                else if (infile.Key == "price_sell")
                {
                    string val = infile.Val;
                    item!.PriceSell.Parse(ref val);
                    infile.Val = val;
                    item.PriceSell.Randomized = true;
                }
                else if (infile.Key == "abs_min")
                {
                    string val = infile.Val;
                    item!.BaseAbs.Min.Parse(ref val);
                    infile.Val = val;
                    item.BaseAbs.Min.Randomized = true;
                }
                else if (infile.Key == "abs_max")
                {
                    string val = infile.Val;
                    item!.BaseAbs.Max.Parse(ref val);
                    infile.Val = val;
                    item.BaseAbs.Max.Randomized = true;
                }
                else if (infile.Key == "dmg_min")
                {
                    string dmgId = global::FlareEngine.Parse.PopFirstString(ref infile.Val);

                    for (int i = 0; i < SharedResources.Eset!.DamageTypes.Types.Count; ++i)
                    {
                        if (dmgId == SharedResources.Eset.DamageTypes.Types[i].Id)
                        {
                            string val = infile.Val;
                            item!.BaseDmg[i].Min.Parse(ref val);
                            infile.Val = val;
                            item.BaseDmg[i].Min.Randomized = true;
                            break;
                        }
                    }
                }
                else if (infile.Key == "dmg_max")
                {
                    string dmgId = global::FlareEngine.Parse.PopFirstString(ref infile.Val);

                    for (int i = 0; i < SharedResources.Eset!.DamageTypes.Types.Count; ++i)
                    {
                        if (dmgId == SharedResources.Eset.DamageTypes.Types[i].Id)
                        {
                            string val = infile.Val;
                            item!.BaseDmg[i].Max.Parse(ref val);
                            infile.Val = val;
                            item.BaseDmg[i].Max.Randomized = true;
                            break;
                        }
                    }
                }
                else if (infile.Key == "bonus")
                {
                    BonusData bdata = new BonusData();
                    bdata.IsExtended = true;
                    ParseBonus(bdata, infile);
                    item!.Bonus.Add(bdata);
                }
                else if (infile.Key == "bonus_power_level")
                {
                    BonusData bdata = new BonusData();
                    bdata.IsExtended = true;
                    bdata.Type = BonusData.PowerLevel;
                    bdata.PowerId = Parse.ToPowerID(global::FlareEngine.Parse.PopFirstString(ref infile.Val));
                    string val = infile.Val;
                    bdata.Value.Parse(ref val);
                    infile.Val = val;
                    item!.Bonus.Add(bdata);
                }
            }
            infile.Close();

            int countAllocated = 0;

            for (int i = (int)SharedResources.Eset!.Loot.ExtendedItemsOffset; i < Items.Count; ++i)
            {
                item = Items[i];

                if (item == null)
                    continue;
                else
                    countAllocated++;

                item.UpdateLevelScaling();
            }
            int extendedItemCount = Items.Count - (int)SharedResources.Eset.Loot.ExtendedItemsOffset;
            if (Items.Count > (int)SharedResources.Eset.Loot.ExtendedItemsOffset)
                extendedItemCount = countAllocated;

            Utils.LogInfo("ItemManager: Extended Item IDs = %zu reserved / %zu allocated / %zu empty / %zu bytes used", extendedItemCount, countAllocated, extendedItemCount - countAllocated, (IntPtr.Size * extendedItemCount) + (IntPtr.Size * countAllocated));
        }

        public void GetExtendedStacks(ItemID itemId, uint quantity, List<ItemStack> stacks)
        {
            ItemStack stack;

            if (Items[(int)itemId]?.RandomizerDef != null)
            {
                for (uint i = 0; i < quantity; ++i)
                {
                    stack = new ItemStack();
                    stack.Item = GetExtendedItem(itemId);
                    stack.Quantity = 1;
                    stacks.Add(stack);
                }
            }
            else
            {
                stack = new ItemStack();
                stack.Item = GetExtendedItem(itemId);
                stack.Quantity = (int)quantity;
                stacks.Add(stack);
            }
        }

    }

    /// <summary>对应 C++ 全局函数 compareItemStack（ItemManager.h �?376 行）�</summary>
    public static class ItemManagerCompare
    {
        public static bool CompareItemStack(ItemStack stack1, ItemStack stack2) => ItemManager.CompareItemStack(stack1, stack2);
    }
}
