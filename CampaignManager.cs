// <自动生成> 对应 C++ 源文件：CampaignManager.h + CampaignManager.cpp

namespace FlareEngine
{
    /// <summary>
    /// CampaignManager - 剧情状态管理器，对应 C++ <c>class CampaignManager</c>。
    /// 管理游戏中的布尔状态标记（任务进度、开关等）。
    /// </summary>
    public class CampaignManager : IDisposable
    {
        /// <summary>?? <c>typedef std::map&lt;StatusID, std::pair&lt;bool, std::string&gt;&gt; StatusMap</c> </summary>
        private class StatusEntry
        {
            public bool IsSet;
            public string Name = "";
        }

        /// <summary> <c>static const bool XP_SHOW_MSG = true</c>。/summary>
        public const bool XpShowMsg = true;

        public Queue<ItemStack> DropStack = new Queue<ItemStack>();

        /// <summary>经验值加成倍率（如 1.25 = +25% XP）。</summary>
        public float BonusXp;

        private readonly SortedDictionary<StatusID, StatusEntry> _status;

        private readonly List<StatusID> _randomStatusPool;

        private StatusID _randomStatus;

        public CampaignManager()
        {
            BonusXp = 0.0f;
            _randomStatus = 0;
            _status = new SortedDictionary<StatusID, StatusEntry>();
            _randomStatusPool = new List<StatusID>();
        }

        public StatusID RegisterStatus(string s)
        {
            if (string.IsNullOrEmpty(s))
                return 0;

            StatusID newId = (StatusID)Utils.HashString(s);

            // check if this status was already registered
            if (_status.ContainsKey(newId))
                return newId;

            // register a new status
            StatusEntry entry = new StatusEntry();
            entry.IsSet = false;
            entry.Name = s;
            _status[newId] = entry;
            return newId;
        }

        /// <summary>
        /// Take the savefile campaign= and convert to status array
        /// </summary>
        public void SetAll(string s)
        {
            string str = s + ',';
            string token;
            while (!string.IsNullOrEmpty(str))
            {
                token = Parse.PopFirstString(ref str);
                if (!string.IsNullOrEmpty(token))
                    SetStatus(RegisterStatus(token));
            }
        }

        /// <summary>
        /// Convert status array to savefile campaign= (status csv)
        /// </summary>
        public string GetAll()
        {
            string output = "";

            List<StatusID> keys = new List<StatusID>(_status.Keys);
            for (int i = 0; i < keys.Count; ++i)
            {
                StatusID key = keys[i];
                if (_status[key].IsSet)
                    output += _status[key].Name;

                if (i + 1 < keys.Count && _status[keys[i + 1]].IsSet)
                {
                    output += ',';
                }
            }
            return output;
        }

        public bool CheckStatus(StatusID s)
        {
            if (_status.TryGetValue(s, out StatusEntry? entry) && entry.IsSet)
                return true;

            return false;
        }

        public void SetStatus(StatusID s)
        {
            // if it's already set, don't set it again
            if (CheckStatus(s)) return;

            if (!_status.ContainsKey(s))
                _status[s] = new StatusEntry();
            _status[s].IsSet = true;
            SharedGameResources.Pc!.Stats.CheckTitle = true;
        }

        public void UnsetStatus(StatusID s)
        {
            // if it's already unset, don't unset it again
            if (!CheckStatus(s)) return;

            if (!_status.ContainsKey(s))
                _status[s] = new StatusEntry();
            _status[s].IsSet = false;
            SharedGameResources.Pc!.Stats.CheckTitle = true;
        }

        public void ResetAllStatuses()
        {
            foreach (KeyValuePair<StatusID, StatusEntry> kvp in _status)
            {
                kvp.Value.IsSet = false;
            }
        }

        public void GetSetStatusStrings(List<string> statusStrings)
        {
            foreach (KeyValuePair<StatusID, StatusEntry> kvp in _status)
            {
                if (kvp.Value.IsSet)
                    statusStrings.Add(kvp.Value.Name);
            }
        }

        public bool CheckCurrency(int quantity)
        {
            return SharedGameResources.Menu!.Inv!.Inventory[MenuInventory.Carried].Contain(SharedResources.Eset!.Misc.CurrencyId, quantity);
        }

        public bool CheckItem(ItemStack istack)
        {
            var menu = SharedGameResources.Menu!;

            if (menu.Inv!.Inventory[MenuInventory.Carried].Contain(istack.Item, istack.Quantity))
                return true;
            else
                return menu.Inv.EquipmentContain(istack.Item, istack.Quantity);
        }

        public void RemoveCurrency(int quantity)
        {
            var eset = SharedResources.Eset!;
            var menu = SharedGameResources.Menu!;

            int maxAmount = Math.Min(quantity, menu.Inv!.Currency);

            if (maxAmount > 0)
            {
                menu.Inv.RemoveCurrency(maxAmount);
                SharedGameResources.Pc!.LogMsg(SharedResources.Msg!.GetV("%d %s removed.", maxAmount, eset.Loot.Currency), Avatar.MsgUnique);
                SharedGameResources.Items!.PlaySound(eset.Misc.CurrencyId);
            }
        }

        public void RemoveItem(ItemStack istack)
        {
            if (istack.Empty())
                return;

            var eset = SharedResources.Eset!;
            var msg = SharedResources.Msg!;
            var items = SharedGameResources.Items!;
            var menu = SharedGameResources.Menu!;
            var pc = SharedGameResources.Pc!;

            if (istack.Item == eset.Misc.CurrencyId)
            {
                RemoveCurrency(istack.Quantity);
                return;
            }

            int itemCount = menu.Inv!.Inventory[MenuInventory.Carried].Count(istack.Item) + menu.Inv.Inventory[MenuInventory.Equipment].Count(istack.Item);
            int maxAmount = Math.Min(itemCount, istack.Quantity);

            if (menu.Inv.Remove(istack.Item, maxAmount))
            {
                if (maxAmount > 1)
                    pc.LogMsg(msg.GetV("%s x%d removed.", items.GetItemName(istack.Item), maxAmount), Avatar.MsgUnique);
                else if (maxAmount == 1)
                    pc.LogMsg(msg.GetV("%s removed.", items.GetItemName(istack.Item)), Avatar.MsgUnique);

                if (maxAmount > 0)
                    items.PlaySound(istack.Item);
            }
        }

        public void RewardItem(ItemStack istack)
        {
            if (istack.Empty())
                return;
            
            var eset = SharedResources.Eset!;
            var msg = SharedResources.Msg!;
            var items = SharedGameResources.Items!;
            var menu = SharedGameResources.Menu!;
            var pc = SharedGameResources.Pc!;

            menu.Inv!.Add(istack, MenuInventory.Carried, ItemStorage.NoSlot, MenuInventory.AddPlaySound, MenuInventory.AddAutoEquip);

            if (istack.Item == eset.Misc.CurrencyId)
            {
                pc.LogMsg(msg.GetV("You receive %d %s.", istack.Quantity, eset.Loot.Currency), Avatar.MsgUnique);
            }
            else
            {
                if (istack.Quantity > 1)
                    pc.LogMsg(msg.GetV("You receive %s x%d.", items.GetItemName(istack.Item), istack.Quantity), Avatar.MsgUnique);
                else if (istack.Quantity == 1)
                    pc.LogMsg(msg.GetV("You receive %s.", items.GetItemName(istack.Item)), Avatar.MsgUnique);
            }
        }

        public void RewardCurrency(int amount)
        {
            ItemStack stack = new ItemStack();
            stack.Item = SharedResources.Eset!.Misc.CurrencyId;
            stack.Quantity = amount;

            RewardItem(stack);
        }

        public void RewardXp(float amount, bool showMessage)
        {
            var pc = SharedGameResources.Pc!;
            var msg = SharedResources.Msg!;

            if (pc.BlockXpGain)
                return;

            BonusXp += (amount * (100.0f + (float)pc.Stats.Get(Stats.XpGain))) / 100.0f;

            int wholeXp = (int)BonusXp;
            pc.Stats.AddXp(wholeXp);
            BonusXp -= (float)wholeXp; // remainder

            pc.Stats.RefreshStats = true;

            if (showMessage)
                pc.LogMsg(msg.GetV("You receive %d XP.", (int)amount), Avatar.MsgUnique);
        }

        public void RestoreHpMp(string s)
        {
            var eset = SharedResources.Eset!;
            var msg = SharedResources.Msg!;
            var items = SharedGameResources.Items!;
            var pc = SharedGameResources.Pc!;

            string restoreStr = s;
            string restoreMode = Parse.PopFirstString(ref restoreStr);

            while (!string.IsNullOrEmpty(restoreMode))
            {
                if (restoreMode == "hp")
                {
                    pc.Stats.Hp = pc.Stats.Get(global::FlareEngine.Stats.HpMax);
                    pc.LogMsg(msg.Get("HP restored."), Avatar.MsgUnique);
                }
                else if (restoreMode == "mp")
                {
                    pc.Stats.Mp = pc.Stats.Get(global::FlareEngine.Stats.MpMax);
                    pc.LogMsg(msg.Get("MP restored."), Avatar.MsgUnique);
                }
                else if (restoreMode == "hpmp")
                {
                    pc.Stats.Hp = pc.Stats.Get(global::FlareEngine.Stats.HpMax);
                    pc.Stats.Mp = pc.Stats.Get(global::FlareEngine.Stats.MpMax);
                    pc.LogMsg(msg.Get("HP and MP restored."), Avatar.MsgUnique);
                }
                else if (restoreMode == "status")
                {
                    pc.Stats.Effects.ClearNegativeEffects(Effect.ResistAll);
                    pc.LogMsg(msg.Get("Negative effects removed."), Avatar.MsgUnique);
                }
                else if (restoreMode == "all")
                {
                    pc.Stats.Hp = pc.Stats.Get(global::FlareEngine.Stats.HpMax);
                    pc.Stats.Mp = pc.Stats.Get(global::FlareEngine.Stats.MpMax);
                    pc.Stats.Effects.ClearNegativeEffects(Effect.ResistAll);
                    pc.LogMsg(msg.Get("HP and MP restored, negative effects removed"), Avatar.MsgUnique);

                    for (int i = 0; i < eset.ResourceStats.Stats.Count; ++i)
                    {
                        pc.Stats.ResourceStats[i] = pc.Stats.GetResourceStat(i, EngineSettings.ResourceStatsSettings.StatBase);
                        pc.LogMsg(eset.ResourceStats.Stats[i].TextLogRestore, Avatar.MsgUnique);
                    }
                }
                else
                {
                    for (int i = 0; i < eset.ResourceStats.Stats.Count; ++i)
                    {
                        if (restoreMode == eset.ResourceStats.Stats[i].Ids[EngineSettings.ResourceStatsSettings.StatBase])
                        {
                            pc.Stats.ResourceStats[i] = pc.Stats.GetResourceStat(i, EngineSettings.ResourceStatsSettings.StatBase);
                            pc.LogMsg(eset.ResourceStats.Stats[i].TextLogRestore, Avatar.MsgUnique);
                        }
                    }
                }

                restoreMode = Parse.PopFirstString(ref restoreStr);
            }
        }

        public bool CheckAllRequirements(EventComponent ec)
        {
            var pc = SharedGameResources.Pc!;
            var mapr = SharedGameResources.Mapr!;

            if (ec.Type == EventComponent.RequiresStatus)
            {
                if (CheckStatus(ec.Status))
                    return true;
            }
            else if (ec.Type == EventComponent.RequiresNotStatus)
            {
                if (!CheckStatus(ec.Status))
                    return true;
            }
            else if (ec.Type == EventComponent.RequiresCurrency)
            {
                if (CheckCurrency(ec.Data[0].Int))
                    return true;
            }
            else if (ec.Type == EventComponent.RequiresNotCurrency)
            {
                if (!CheckCurrency(ec.Data[0].Int))
                    return true;
            }
            else if (ec.Type == EventComponent.RequiresItem)
            {
                if (CheckItem(new ItemStack(ec.Id, ec.Data[0].Int)))
                    return true;
            }
            else if (ec.Type == EventComponent.RequiresNotItem)
            {
                if (!CheckItem(new ItemStack(ec.Id, ec.Data[0].Int)))
                    return true;
            }
            else if (ec.Type == EventComponent.RequiresLevel)
            {
                if (pc.Stats.Level >= ec.Data[0].Int)
                    return true;
            }
            else if (ec.Type == EventComponent.RequiresNotLevel)
            {
                if (pc.Stats.Level < ec.Data[0].Int)
                    return true;
            }
            else if (ec.Type == EventComponent.RequiresClass)
            {
                if (pc.Stats.CharacterClass == ec.S)
                    return true;
            }
            else if (ec.Type == EventComponent.RequiresNotClass)
            {
                if (pc.Stats.CharacterClass != ec.S)
                    return true;
            }
            else if (ec.Type == EventComponent.RequiresTile)
            {
                int index = mapr.Layernames.IndexOf(ec.S);
                if (index == -1) index = mapr.LayernamesHashed.Count;
                if (mapr != null && index < mapr.Layers.Count && ec.Data[0].Int >= 0 && ec.Data[0].Int < mapr.W && ec.Data[1].Int >= 0 && ec.Data[1].Int < mapr.H)
                    if (mapr.Layers[index][ec.Data[0].Int][ec.Data[1].Int] == (ushort)ec.Data[2].Int)
                        return true;
            }
            else if (ec.Type == EventComponent.RequiresNotTile)
            {
                int index = mapr.Layernames.IndexOf(ec.S);
                if (index == -1) index = mapr.LayernamesHashed.Count;
                if (mapr != null && index < mapr.Layers.Count && ec.Data[0].Int >= 0 && ec.Data[0].Int < mapr.W && ec.Data[1].Int >= 0 && ec.Data[1].Int < mapr.H)
                    if (mapr.Layers[index][ec.Data[0].Int][ec.Data[1].Int] != (ushort)ec.Data[2].Int)
                        return true;
            }
            else
            {
                // Event component is not a requirement check
                // treat it as if the "requirement" was met
                return true;
            }

            // requirement check failed
            return false;
        }

        public bool CheckRequirementsInVector(List<EventComponent> ecVec)
        {
            for (int i = 0; i < ecVec.Count; ++i)
            {
                if (!CheckAllRequirements(ecVec[i]))
                    return false;
            }

            return true;
        }

        public void RandomStatusAppend(StatusID s)
        {
            if (!_randomStatusPool.Contains(s))
            {
                if (_randomStatusPool.Count == 0)
                    _randomStatus = s;

                _randomStatusPool.Add(s);
            }
        }

        public void RandomStatusClear()
        {
            _randomStatusPool.Clear();
            _randomStatus = 0;
        }

        public void RandomStatusRoll()
        {
            if (_randomStatusPool.Count == 0)
                return;

            _randomStatus = _randomStatusPool[MathUtils.RandBetween(0, _randomStatusPool.Count - 1)];
        }

        public void RandomStatusSet()
        {
            if (_randomStatusPool.Count == 0)
                return;

            SetStatus(_randomStatus);
        }

        public void RandomStatusUnset()
        {
            if (_randomStatusPool.Count == 0)
                return;

            UnsetStatus(_randomStatus);
        }

        public void Dispose()
        {
            Utils.LogInfo("Cleaning up: CampaignManager");
        }
    }
}
