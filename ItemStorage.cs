// <自动生成> 对应 C++ 源文件：ItemStorage.h + ItemStorage.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using System.Globalization;
using System.Text;

namespace FlareEngine
{
    /// <summary>
    /// ItemStorage
    ///
    /// 玩家背包/仓库/商店库存等场景通用的物品格子容器，对应 C++ 的 <c>class ItemStorage</c>。
    /// 内部用一个固定长度的 <c>ItemStack[]</c> 表示所有格子，并提供增删、排序、CSV 存档
    /// 序列化等操作。<c>MenuItemStorage</c>（尚未转换）直接继承本类并访问其 protected 成员，
    /// 因此 <see cref="_slotNumber"/> 等字段保留 protected 可见性，供子类直接使用。
    ///
    /// 前向引用说明：ItemManager/Item/ItemStack/MessageEngine 均尚未转换为 .cs，本文件按
    /// "预期 PascalCase 命名" 规则对其成员进行前向引用，具体假设签名见配套的
    /// ItemStorage.report.txt。
    /// </summary>
    public class ItemStorage : IDisposable
    {
        /// <summary>
        /// 对应 C++ protected 静态方法 <c>compareCurrencyItemStack(const void*, const void*)</c>。
        /// 与 <see cref="CompareItemStack"/> 等系列方法一样，用 <c>object?</c> 模拟原始的
        /// <c>const void*</c> 参数，方法体内转型为 <see cref="ItemStack"/>，与原始
        /// <c>static_cast&lt;const ItemStack*&gt;(...)</c> 逐行对应。
        /// </summary>
        protected static int CompareCurrencyItemStack(object? a, object? b)
        {
            ItemStack i1 = (ItemStack)a!;
            ItemStack i2 = (ItemStack)b!;

            EngineSettings? eset = SharedResources.Eset;
            if (eset != null)
            {
                if (i1.Item == eset.Misc.CurrencyId && i2.Item != eset.Misc.CurrencyId)
                    return -1;
                else if (i1.Item != eset.Misc.CurrencyId && i2.Item == eset.Misc.CurrencyId)
                    return 1;
                else if (i1.Item == eset.Misc.CurrencyId && i2.Item == eset.Misc.CurrencyId)
                    // C++ 原始写法 `return (i1->quantity < i2->quantity);` 依赖 bool 到 int 的
                    // 隐式转换（true->1，false->0）。C# 不支持该隐式转换，这里用三元表达式
                    // 显式还原相同的数值结果，属于语言层面的必要适配，未改变比较结果。
                    return (i1.Quantity < i2.Quantity) ? 1 : 0;
            }

            return 0;
        }

        /// <summary>对应 C++ protected 字段 <c>int slot_number;</c>。</summary>
        protected int _slotNumber;
        /// <summary>对应 C++ protected 字段 <c>int next_sort_mode;</c>。</summary>
        protected int _nextSortMode;

        /// <summary>对应 C++ <c>static const int NO_SLOT = -1;</c>。</summary>
        public const int NoSlot = -1;

        // 对应 C++ 匿名 enum { SORT_NONE=0, SORT_TYPE, SORT_QUALITY, SORT_LEVEL,
        // SORT_SELL_PRICE, SORT_ID, SORT_BUY_PRICE }。原始枚举值只作为普通 int 使用
        // （参与 next_sort_mode 的自增/取余等运算），因此按照本项目既有惯例保留为
        // int 常量而非 C# enum 类型，避免引入额外的强制类型转换。
        public const int SortNone = 0;
        public const int SortType = 1;
        public const int SortQuality = 2;
        public const int SortLevel = 3;
        public const int SortSellPrice = 4;
        public const int SortId = 5; // not used in SortNext(), must be explicitly selected with Sort()
        public const int SortBuyPrice = 6;

        /// <summary>
        /// 用于对 <see cref="ItemStack"/> 数组进行 qsort 风格排序；不要与用于 C++
        /// <c>std::sort</c> 的 <c>ItemManager::compareItemStack()</c> 混淆。
        /// </summary>
        public static int CompareItemStack(object? a, object? b)
        {
            int currencyCompare = CompareCurrencyItemStack(a, b);
            if (currencyCompare != 0)
                return currencyCompare;

            ItemStack i1 = (ItemStack)a!;
            ItemStack i2 = (ItemStack)b!;

            if (i1 > i2)
                return 1;
            else if (i1.Item == i2.Item && i1.Quantity == i2.Quantity)
                return 0;
            else
                return -1;
        }

        public static int CompareItemStackByType(object? a, object? b)
        {
            ItemStack i1 = (ItemStack)a!;
            ItemStack i2 = (ItemStack)b!;
            ItemManager? items = SharedGameResources.Items;
            if (items != null && i1.Item != 0 && i2.Item != 0)
            {
                int currencyCompare = CompareCurrencyItemStack(a, b);
                if (currencyCompare != 0)
                    return currencyCompare;

                Item itemA = items.Items[i1.Item]!;
                Item itemB = items.Items[i2.Item]!;

                if (itemA.Type == 0 && itemB.Type != 0)
                    return 1;
                else if (itemA.Type != 0 && itemB.Type == 0)
                    return -1;
                else if (itemA.Type > itemB.Type)
                    return 1;
                else if (itemA.Type == itemB.Type && itemA.Icon > itemB.Icon)
                    return 1;
                else if (itemA.Type == itemB.Type && itemA.Icon == itemB.Icon && i1.Quantity < i2.Quantity)
                    return 1;
                else if (itemA.Type == itemB.Type && itemA.Icon == itemB.Icon && i1.Quantity == i2.Quantity)
                    return CompareItemStack(a, b);
                else
                    return -1;
            }
            return CompareItemStack(a, b);
        }

        public static int CompareItemStackByBuyPrice(object? a, object? b)
        {
            ItemStack i1 = (ItemStack)a!;
            ItemStack i2 = (ItemStack)b!;
            ItemManager? items = SharedGameResources.Items;
            if (items != null && i1.Item != 0 && i2.Item != 0)
            {
                int currencyCompare = CompareCurrencyItemStack(a, b);
                if (currencyCompare != 0)
                    return currencyCompare;

                Item itemA = items.Items[i1.Item]!;
                Item itemB = items.Items[i2.Item]!;

                int priceA = itemA.GetPrice(!ItemManager.UseVendorRatio);
                int priceB = itemB.GetPrice(!ItemManager.UseVendorRatio);
                if (priceA < priceB)
                    return 1;
                else if (priceA == priceB)
                    return CompareItemStackByType(a, b);
                else
                    return -1;
            }
            return CompareItemStackByType(a, b);
        }

        public static int CompareItemStackBySellPrice(object? a, object? b)
        {
            ItemStack i1 = (ItemStack)a!;
            ItemStack i2 = (ItemStack)b!;
            ItemManager? items = SharedGameResources.Items;
            if (items != null && i1.Item != 0 && i2.Item != 0)
            {
                int currencyCompare = CompareCurrencyItemStack(a, b);
                if (currencyCompare != 0)
                    return currencyCompare;

                Item itemA = items.Items[i1.Item]!;
                Item itemB = items.Items[i2.Item]!;

                int priceA = itemA.GetSellPrice(ItemManager.DefaultSellPrice);
                int priceB = itemB.GetSellPrice(ItemManager.DefaultSellPrice);
                if (priceA < priceB)
                    return 1;
                else if (priceA == priceB)
                    return CompareItemStackByType(a, b);
                else
                    return -1;
            }
            return CompareItemStackByType(a, b);
        }

        public static int CompareItemStackByQuality(object? a, object? b)
        {
            ItemStack i1 = (ItemStack)a!;
            ItemStack i2 = (ItemStack)b!;
            ItemManager? items = SharedGameResources.Items;
            if (items != null && i1.Item != 0 && i2.Item != 0)
            {
                int currencyCompare = CompareCurrencyItemStack(a, b);
                if (currencyCompare != 0)
                    return currencyCompare;

                Item itemA = items.Items[i1.Item]!;
                Item itemB = items.Items[i2.Item]!;

                if (itemA.Quality > itemB.Quality)
                    return 1;
                else if (itemA.Quality == itemB.Quality)
                    return CompareItemStackByType(a, b);
                else
                    return -1;
            }
            return CompareItemStackByType(a, b);
        }

        public static int CompareItemStackByLevel(object? a, object? b)
        {
            ItemStack i1 = (ItemStack)a!;
            ItemStack i2 = (ItemStack)b!;
            ItemManager? items = SharedGameResources.Items;
            if (items != null && i1.Item != 0 && i2.Item != 0)
            {
                int currencyCompare = CompareCurrencyItemStack(a, b);
                if (currencyCompare != 0)
                    return currencyCompare;

                Item itemA = items.Items[i1.Item]!;
                Item itemB = items.Items[i2.Item]!;

                if (itemA.Level == 0 && itemB.Level != 0)
                    return 1;
                else if (itemA.Level != 0 && itemB.Level == 0)
                    return -1;
                else if (itemA.Level > itemB.Level)
                    return -1;
                else if (itemA.Level == itemB.Level)
                    return CompareItemStackByType(a, b);
                else
                    return 1;
            }
            return CompareItemStackByType(a, b);
        }

        /// <summary>
        /// 对应 C++ 构造函数初始化列表：slot_number(0), next_sort_mode(SORT_NONE+1),
        /// storage(NULL), sort_tooltip(NULL)，顺序保留。
        /// </summary>
        public ItemStorage()
        {
            _slotNumber = 0;
            _nextSortMode = SortNone + 1;
            Storage = null;
            SortTooltip = null;
        }

        /// <summary>
        /// 对应 C++ 析构函数 <c>~ItemStorage() { delete[] storage; }</c>。
        /// <see cref="Storage"/> 中的元素均为托管对象，这里只需释放数组引用，
        /// 交由 GC 回收，无需逐元素处理。
        /// </summary>
        public void Dispose()
        {
            Storage = null;
            GC.SuppressFinalize(this);
        }

        public void Init(int slotNumber)
        {
            if (Storage != null && _slotNumber == slotNumber)
                return; // already initialized

            _slotNumber = slotNumber;

            if (Storage != null)
                Storage = null;

            // 注意：C++ 版本 `new ItemStack[slot_number]` 会为每个元素调用默认构造函数，
            // 数组本身即是已初始化对象的连续存储。C# 的 ItemStack[]（引用类型数组）默认
            // 元素为 null，因此这里显式为每个槽位创建一个新的 ItemStack 实例，
            // 属于语言层面的必要补充，随后仍保留原始的显式 Clear() 调用。
            Storage = new ItemStack[_slotNumber];

            for (int i = 0; i < _slotNumber; i++)
            {
                Storage[i] = new ItemStack();
                Storage[i].Clear();
            }
        }

        /// <summary>
        /// 对应 C++ <c>ItemStack &amp; operator[](int slot)</c>。原始版本返回引用，
        /// 兼具读写语义，这里用带 get/set 的索引器还原同样的用法（读取与整槽赋值）。
        /// </summary>
        public ItemStack this[int slot]
        {
            get => Storage![slot];
            set => Storage![slot] = value;
        }

        /// <summary>
        /// Take the savefile CSV list of items id and convert to storage array
        /// </summary>
        public void SetItems(string s)
        {
            ItemManager items = SharedGameResources.Items!;
            string itemList = s + ',';
            for (int i = 0; i < _slotNumber; i++)
            {
                Storage![i].Item = items.VerifyID(Parse.ToItemID(Parse.PopFirstString(ref itemList)), null, ItemManager.VerifyAllowZero, ItemManager.VerifyAllocate);
            }
        }

        /// <summary>
        /// Take the savefile CSV list of items quantities and convert to storage array
        /// </summary>
        public void SetQuantities(string s)
        {
            string quantityList = s + ',';
            for (int i = 0; i < _slotNumber; i++)
            {
                Storage![i].Quantity = Parse.PopFirstInt(ref quantityList);
                if (Storage[i].Quantity < 0)
                {
                    Utils.LogError("ItemStorage: Items quantity on position %d is negative, setting to zero", i);
                    Storage[i].Quantity = 0;
                }
            }
        }

        public void SetForeign(bool isForeign)
        {
            ItemManager items = SharedGameResources.Items!;
            for (int i = 0; i < _slotNumber; ++i)
            {
                if (items.IsValid(Storage![i].Item) && items.Items[Storage[i].Item]!.Parent != 0)
                {
                    items.Items[Storage[i].Item]!.IsForeign = isForeign;
                }
            }
        }

        /// <summary>对应 C++ <c>int getSlotNumber()</c>：无附加逻辑的简单只读访问器，转为属性。</summary>
        public int SlotNumber => _slotNumber;

        /// <summary>
        /// Convert storage array to a CSV list of items id for savefile
        /// </summary>
        public string GetItems()
        {
            StringBuilder ss = new StringBuilder();
            for (int i = 0; i < _slotNumber; i++)
            {
                ss.Append(Storage![i].Item.ToString(CultureInfo.InvariantCulture));
                if (i < _slotNumber - 1) ss.Append(',');
            }
            return ss.ToString();
        }

        /// <summary>
        /// Convert storage array to a CSV list of items quantities for savefile
        /// </summary>
        public string GetQuantities()
        {
            StringBuilder ss = new StringBuilder();
            for (int i = 0; i < _slotNumber; i++)
            {
                ss.Append(Storage![i].Quantity.ToString(CultureInfo.InvariantCulture));
                if (i < _slotNumber - 1) ss.Append(',');
            }
            return ss.ToString();
        }

        public void Clear()
        {
            for (int i = 0; i < _slotNumber; i++)
            {
                Storage![i].Clear();
            }
        }

        /// <summary>
        /// Insert item into first available carried slot, preferably in the optionnal specified slot
        /// Returns an ItemStack containing anything that couldn't fit
        /// </summary>
        /// <param name="stack">Stack of items</param>
        /// <param name="slot">Slot number where it will try to store the item</param>
        public ItemStack Add(ItemStack stack, int slot)
        {
            ItemManager items = SharedGameResources.Items!;
            if (!stack.Empty() && items.IsValid(stack.Item))
            {
                _nextSortMode = SortNone + 1;
                RefreshSortTooltip();

                int maxQuantity = items.Items[stack.Item]!.MaxQuantity;
                if (slot > -1)
                {
                    // a slot is specified
                    if (Storage![slot].Item != 0 && Storage[slot].Item != stack.Item)
                    {
                        // the proposed slot isn't available
                        slot = -1;
                    }
                }
                if (slot == -1)
                {
                    // first search of stack to complete if the item is stackable
                    int i = 0;
                    while (maxQuantity > 1 && slot == -1 && i < _slotNumber)
                    {
                        if (Storage![i].Item == stack.Item && Storage[i].Quantity < maxQuantity && Storage[i].CanBuyback == stack.CanBuyback)
                        {
                            slot = i;
                        }
                        i++;
                    }
                    // then an empty slot
                    i = 0;
                    while (slot == -1 && i < _slotNumber)
                    {
                        if (Storage![i].Empty())
                        {
                            slot = i;
                        }
                        i++;
                    }
                }
                if (slot != -1)
                {
                    // Add
                    int quantityAdded = Math.Min(stack.Quantity, maxQuantity - Storage![slot].Quantity);
                    Storage[slot].Item = stack.Item;
                    Storage[slot].CanBuyback = stack.CanBuyback;
                    Storage[slot].Quantity += quantityAdded;
                    stack.Quantity -= quantityAdded;
                    // Add back the remaining, recursivly, until there's no more left to add or we run out of space.
                    if (stack.Quantity > 0)
                    {
                        return Add(stack, NoSlot);
                    }
                    // everything added successfully, so return an empty ItemStack
                    return new ItemStack();
                }
                else
                {
                    // Returns an ItemStack containing the remaining quantity if we run out of space.
                    // This stack will likely be dropped on the ground
                    return stack;
                }
            }
            return new ItemStack();
        }

        /// <summary>
        /// Subtract an item from the specified slot, or remove it if it's the last
        /// </summary>
        /// <param name="slot">Slot number</param>
        public void Subtract(int slot, int quantity)
        {
            _nextSortMode = SortNone + 1;
            RefreshSortTooltip();

            Storage![slot].Quantity -= quantity;
            if (Storage[slot].Quantity <= 0)
            {
                Storage[slot].Clear();
            }
        }

        /// <summary>
        /// Remove a quantity of a given item by its ID
        /// </summary>
        public bool Remove(ItemID item, int quantity)
        {
            if (item == 0)
                return false;

            const int lowestQuantity = int.MaxValue;

            _nextSortMode = SortNone + 1;
            RefreshSortTooltip();

            while (quantity > 0)
            {
                int lowestSlot = -1;

                for (int i = 0; i < _slotNumber; i++)
                {
                    if (Storage![i].Item == item && Storage[i].Quantity < lowestQuantity)
                    {
                        lowestSlot = i;
                    }
                }

                // could not find item id, can't remove anything
                if (lowestSlot == -1)
                    return false;

                if (quantity <= Storage![lowestSlot].Quantity)
                {
                    // take from a single stack
                    Subtract(lowestSlot, quantity);
                    quantity = 0;
                }
                else
                {
                    // need to take from another stack
                    quantity = quantity - Storage[lowestSlot].Quantity;
                    Subtract(lowestSlot, Storage[lowestSlot].Quantity);
                }
            }
            return true;
        }

        public void Sort(int mode)
        {
            // 注意：C++ 版本使用 qsort(storage, slot_number, sizeof(ItemStack), comparator)
            // 对 [0, slot_number) 区间原地排序。C# 用 Array.Sort 的 (array, index, length,
            // comparer) 重载表达同样的“只排序前 slot_number 个元素”的语义，并通过
            // Comparer<ItemStack>.Create 包装既有的 CompareXxx(object?, object?) 静态方法，
            // 避免重复实现比较逻辑。qsort 与 Array.Sort 内部算法不同，对“比较结果相等”的
            // 元素相对顺序均不作稳定性保证，两者均为不稳定排序，行为等价。
            if (mode == SortId)
                Array.Sort(Storage!, 0, _slotNumber, Comparer<ItemStack>.Create((x, y) => CompareItemStack(x, y)));
            else if (mode == SortType)
                Array.Sort(Storage!, 0, _slotNumber, Comparer<ItemStack>.Create((x, y) => CompareItemStackByType(x, y)));
            else if (mode == SortBuyPrice)
                Array.Sort(Storage!, 0, _slotNumber, Comparer<ItemStack>.Create((x, y) => CompareItemStackByBuyPrice(x, y)));
            else if (mode == SortSellPrice)
                Array.Sort(Storage!, 0, _slotNumber, Comparer<ItemStack>.Create((x, y) => CompareItemStackBySellPrice(x, y)));
            else if (mode == SortQuality)
                Array.Sort(Storage!, 0, _slotNumber, Comparer<ItemStack>.Create((x, y) => CompareItemStackByQuality(x, y)));
            else if (mode == SortLevel)
                Array.Sort(Storage!, 0, _slotNumber, Comparer<ItemStack>.Create((x, y) => CompareItemStackByLevel(x, y)));

            // anything else is treated as SortNone and no sorting is done

            RefreshSortTooltip();
        }

        public void SortNext()
        {
            Sort(_nextSortMode);

            _nextSortMode++;
            if (_nextSortMode == SortId)
                _nextSortMode = SortNone + 1;

            RefreshSortTooltip();
        }

        public bool Full(ItemStack stack)
        {
            ItemManager items = SharedGameResources.Items!;
            if (stack.Empty() || !items.IsValid(stack.Item))
                return false;

            for (int i = 0; i < _slotNumber; i++)
            {
                if (Storage![i].Item == stack.Item && items.IsValid(stack.Item) && Storage[i].Quantity < items.Items[stack.Item]!.MaxQuantity)
                {
                    if (stack.Quantity + Storage[i].Quantity >= items.Items[stack.Item]!.MaxQuantity)
                    {
                        stack.Quantity -= Storage[i].Quantity;
                        continue;
                    }
                    return false;
                }
                if (Storage[i].Empty())
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Get the number of the specified item carried (not equipped)
        /// </summary>
        public int Count(ItemID item)
        {
            int itemCount = 0;
            for (int i = 0; i < _slotNumber; i++)
            {
                if (Storage![i].Item == item)
                {
                    itemCount += Storage[i].Quantity;
                }
            }
            return itemCount;
        }

        /// <summary>
        /// Check to see if the given item is equipped
        /// </summary>
        public bool Contain(ItemID item, int quantity)
        {
            int totalQuantity = 0;
            for (int i = 0; i < _slotNumber; i++)
            {
                if (Storage![i].Item == item)
                    totalQuantity += Storage[i].Quantity;
                if (totalQuantity >= quantity)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Clear slots that contain an item, but have a quantity of 0
        /// </summary>
        public void Clean()
        {
            for (int i = 0; i < _slotNumber; i++)
            {
                if (Storage![i].Item > 0 && Storage[i].Quantity < 1)
                {
                    Utils.LogInfo("ItemStorage: Removing item with id %d, which has a quantity of 0", Storage[i].Item);
                    Storage[i].Clear();
                }
                else if (Storage[i].Item == 0 && Storage[i].Quantity != 0)
                {
                    Utils.LogInfo("ItemStorage: Removing item with id 0, which has a quantity of %d", Storage[i].Quantity);
                    Storage[i].Clear();
                }
            }
        }

        /// <summary>
        /// Returns true if the storage is empty
        /// </summary>
        public bool Empty()
        {
            if (Storage == null)
                return true;

            for (int i = 0; i < _slotNumber; ++i)
            {
                if (!Storage[i].Empty())
                    return false;
            }

            return true;
        }

        public int GetSlotNumber()
        {
            return _slotNumber;
        }

        public void RefreshSortTooltip()
        {
            if (SortTooltip == null)
                return;

            MessageEngine msg = SharedResources.Msg!;

            if (_nextSortMode == SortType)
                SortTooltip(msg.Get("Sort by: Type"));
            else if (_nextSortMode == SortQuality)
                SortTooltip(msg.Get("Sort by: Quality"));
            else if (_nextSortMode == SortLevel)
                SortTooltip(msg.Get("Sort by: Level"));
            else if (_nextSortMode == SortSellPrice)
                SortTooltip(msg.Get("Sort by: Sell Price"));
        }

        /// <summary>对应 C++ 公有字段 <c>ItemStack* storage;</c>，各槽位物品数组。</summary>
        public ItemStack[]? Storage;

        /// <summary>
        /// 对应 C++ 公有字段 <c>std::string* sort_tooltip;</c>：指向外部（通常是某个排序按钮的
        /// tooltip 文本）的裸指针别名，<c>RefreshSortTooltip()</c> 通过它写回排序提示文案。
        /// C# 没有指向托管字符串字段的指针/引用可以长期持有，这里改用 <c>Action&lt;string&gt;</c>
        /// 回调表达同样的"写入外部持有的字符串"语义：调用方在赋值时传入形如
        /// <c>value =&gt; button.Tooltip = value</c> 的委托，语言层面的必要适配，
        /// 行为与原始裸指针别名等价（均只写不读）。
        /// </summary>
        public Action<string>? SortTooltip;
    }
}
