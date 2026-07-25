// 对应 C++ 源：NPC.h + NPC.cpp
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// NPC
    ///
    /// 非战斗型可交互角色（商贩、对话者等），对应原始 <c>class NPC : public Entity</c>。
    ///
    /// 资源管理：<see cref="Portraits"/> 元素对应原始 <c>Sprite*</c>，由
    /// <see cref="LoadGraphics"/> 创建、<see cref="Dispose"/> 释放；<see cref="VoxIntro"/>/
    /// <see cref="VoxQuests"/> 音效 ID 由 <see cref="LoadSound"/> 加载、析构时
    /// <see cref="SharedResources.Snd"/> 卸载。释放顺序与原始 <c>~NPC()</c> 一致
    /// （先 portraits，再 vox 向量，最后 <see cref="Entity.Dispose"/>）。
    ///
    /// 全局指针映射：<c>msg</c> → <see cref="SharedResources.Msg"/>，
    /// <c>camp</c> → <see cref="SharedGameResources.Camp"/>，
    /// <c>items</c> → <see cref="SharedGameResources.Items"/>，
    /// <c>loot</c> → <see cref="SharedGameResources.Loot"/>，
    /// <c>eventm</c> → <see cref="SharedGameResources.Eventm"/>，
    /// <c>entitym</c> → <see cref="SharedGameResources.Entitym"/>，
    /// <c>mapr</c> → <see cref="SharedGameResources.Mapr"/>，
    /// <c>mods</c> → <see cref="SharedResources.Mods"/>，
    /// <c>render_device</c> → <see cref="SharedResources.RenderDevice"/>，
    /// <c>snd</c> → <see cref="SharedResources.Snd"/>。
    /// </summary>
    public class NPC : Entity
    {
        private const int VoxIntro = 0;
        private const int VoxQuest = 1;

        private string _gfx = "";

        private List<EventComponent> _randomTable = new List<EventComponent>();
        private Int2 _randomTableCount;

        private List<EventComponent> _craftRandomTable = new List<EventComponent>();
        private Int2 _craftRandomTableCount;

        private List<StatusID> _vendorRequiresStatus = new List<StatusID>();
        private List<StatusID> _vendorRequiresNotStatus = new List<StatusID>();

        private List<string> _portraitFilenames = new List<string>();

        private List<SoundID> _voxIntro = new List<SoundID>();
        private List<SoundID> _voxQuests = new List<SoundID>();

        public const int VendorMaxStock = 80;
        public const bool GetResponseNodes = true;

        /// <summary>对应 C++ 构造函数 <c>NPC(const Entity&amp; e)</c>。</summary>
        public NPC(Entity e) : base(e)
        {
            Behavior?.Dispose();
            Behavior = new EntityBehavior(this);

            // 对应 C++ NPC 成员初始化列表
            _gfx = "";
            _voxIntro = new List<SoundID>();
            _voxQuests = new List<SoundID>();
            Name = "";
            Direction = 0;
            ShowOnMinimap = true;
            NpcPortrait = null;
            HeroPortrait = null;
            Talker = false;
            Vendor = false;
            ResetBuyback = true;
            Stock = new ItemStorage();
            CraftStock = new ItemStorage();
            VendorRatioBuy = 0;
            VendorRatioSell = 0;
            VendorRatioSellOld = 0;
            Dialog = new List<List<EventComponent>>();

            Stock.Init(VendorMaxStock);
            CraftStock.Init(VendorMaxStock);

            // have NPCs face south by default
            Stats.Direction = 7;

            VendorTabEnabled = new bool[3];
            VendorTabEnabled[ItemManager.VendorBuy] = true;
            VendorTabEnabled[ItemManager.VendorSell] = true;
            VendorTabEnabled[ItemManager.VendorCraft] = false;
        }

        /// <summary>对应 C++ 析构函数 <c>~NPC()</c>。</summary>
        public override void Dispose()
        {
            for (int i = 0; i < Portraits.Count; ++i)
            {
                Portraits[i]?.Dispose();
            }

            while (_voxIntro.Count > 0)
            {
                SharedResources.Snd!.Unload(_voxIntro[^1]);
                _voxIntro.RemoveAt(_voxIntro.Count - 1);
            }
            while (_voxQuests.Count > 0)
            {
                SharedResources.Snd!.Unload(_voxQuests[^1]);
                _voxQuests.RemoveAt(_voxQuests.Count - 1);
            }

            base.Dispose();
        }

        /// <summary>
        /// NPCs are stored in simple config files
        /// </summary>
        /// <param name="npcId">Config file for npc</param>
        public bool Load(string npcId)
        {
            FileParser infile = new FileParser();
            ItemStack stack = new ItemStack();

            _portraitFilenames.Clear();
            _portraitFilenames.Add("");

            // @CLASS NPC|Description of NPCs in npcs/
            if (infile.Open(npcId, FileParser.ModFile, FileParser.ErrorNormal))
            {
                bool clearRandomTable = true;
                bool clearCraftRandomTable = true;

                while (infile.Next())
                {
                    if (infile.Section == "stats")
                    {
                        // handled by StatBlock::load()
                        continue;
                    }
                    else if (infile.Section == "dialog")
                    {
                        if (infile.NewSection)
                        {
                            Dialog.Add(new List<EventComponent>());
                        }
                        EventComponent e = new EventComponent();
                        e.Type = EventComponent.None;
                        if (infile.Key == "id")
                        {
                            // @ATTR dialog.id|string|A unique identifer used to reference this dialog.
                            e.Type = EventComponent.NpcDialogID;
                            e.S = infile.Val;
                        }
                        else if (infile.Key == "him" || infile.Key == "her")
                        {
                            // @ATTR dialog.him|repeatable(string)|A line of dialog from the NPC.
                            // @ATTR dialog.her|repeatable(string)|A line of dialog from the NPC.
                            e.Type = EventComponent.NpcDialogThem;
                            e.S = SharedResources.Msg!.Get(infile.Val);
                        }
                        else if (infile.Key == "you")
                        {
                            // @ATTR dialog.you|repeatable(string)|A line of dialog from the player.
                            e.Type = EventComponent.NpcDialogYou;
                            e.S = SharedResources.Msg!.Get(infile.Val);
                        }
                        else if (infile.Key == "voice")
                        {
                            // @ATTR dialog.voice|repeatable(string)|Filename of a voice sound file to play.
                            e.Type = EventComponent.NpcVoice;
                            e.Data[0].Int = LoadSound(infile.Val, VoxQuest);
                        }
                        else if (infile.Key == "topic")
                        {
                            // @ATTR dialog.topic|string|The name of this dialog topic. Displayed when picking a dialog tree.
                            e.Type = EventComponent.NpcDialogTopic;
                            e.S = SharedResources.Msg!.Get(infile.Val);
                        }
                        else if (infile.Key == "group")
                        {
                            // @ATTR dialog.group|string|Adds this dialog node to the specified group and skips adding it to the dialog tree. For each group, one dialog node is selected at random and added to the dialog tree.
                            e.Type = EventComponent.NpcDialogGroup;
                            e.S = infile.Val;
                        }
                        else if (infile.Key == "allow_movement")
                        {
                            // @ATTR dialog.allow_movement|bool|Restrict the player's mvoement during dialog.
                            e.Type = EventComponent.NpcAllowMovement;
                            e.S = infile.Val;
                        }
                        else if (infile.Key == "portrait_him" || infile.Key == "portrait_her")
                        {
                            // @ATTR dialog.portrait_him|repeatable(filename)|Filename of a portrait to display for the NPC during this dialog.
                            // @ATTR dialog.portrait_her|repeatable(filename)|Filename of a portrait to display for the NPC during this dialog.
                            e.Type = EventComponent.NpcPortraitThem;
                            e.S = infile.Val;
                            _portraitFilenames.Add(e.S);
                        }
                        else if (infile.Key == "portrait_you")
                        {
                            // @ATTR dialog.portrait_you|repeatable(filename)|Filename of a portrait to display for the player during this dialog.
                            e.Type = EventComponent.NpcPortraitYou;
                            e.S = infile.Val;
                            _portraitFilenames.Add(e.S);
                        }
                        else if (infile.Key == "take_a_party")
                        {
                            // @ATTR dialog.take_a_party|bool|Start/stop taking a party with player.
                            e.Type = EventComponent.NpcTakeAParty;
                            e.Data[0].Bool = Parse.ToBool(infile.Val);
                        }
                        else if (infile.Key == "response")
                        {
                            // @ATTR dialog.response|repeatable(string)|A dialog ID to present as a selectable response. This key must precede the dialog text line.
                            e.Type = EventComponent.NpcDialogResponse;
                            e.S = infile.Val;
                        }
                        else if (infile.Key == "response_only")
                        {
                            // @ATTR dialog.response_only|bool|If true, this dialog topic will only appear when explicitly referenced with the "response" key.
                            e.Type = EventComponent.NpcDialogResponseOnly;
                            e.Data[0].Bool = Parse.ToBool(infile.Val);
                        }
                        else
                        {
                            Event ev = new Event();
                            SharedGameResources.Eventm!.LoadEventComponent(infile, ev, null);

                            for (int i = 0; i < ev.Components.Count; ++i)
                            {
                                if (ev.Components[i].Type != EventComponent.None)
                                {
                                    Dialog[^1].Add(ev.Components[i]);
                                }
                            }
                        }

                        if (e.Type != EventComponent.None)
                        {
                            Dialog[^1].Add(e);
                        }
                    }
                    else if (infile.Section == "" || infile.Section == "npc")
                    {
                        Filename = npcId;

                        if (infile.NewSection)
                        {
                            // APPENDed file
                            clearRandomTable = true;
                            clearCraftRandomTable = true;
                        }

                        if (infile.Key == "name")
                        {
                            // @ATTR npc.name|string|NPC's name.
                            Name = SharedResources.Msg!.Get(infile.Val);
                        }
                        else if (infile.Key == "animations" || infile.Key == "gfx")
                        {
                            // TODO "gfx" is deprecated
                        }
                        else if (infile.Key == "direction")
                        {
                            // @ATTR npc.direction|direction|The direction to use for this NPC's stance animation.
                            Direction = Parse.ToDirection(infile.Val);
                        }
                        else if (infile.Key == "show_on_minimap")
                        {
                            // @ATTR npc.show_on_minimap|bool|If true, this NPC will be shown on the minimap. The default is true.
                            ShowOnMinimap = Parse.ToBool(infile.Val);
                        }

                        // handle talkers
                        else if (infile.Key == "talker")
                        {
                            // @ATTR npc.talker|bool|Allows this NPC to be talked to.
                            Talker = Parse.ToBool(infile.Val);
                        }
                        else if (infile.Key == "portrait")
                        {
                            // @ATTR npc.portrait|filename|Filename of the default portrait image.
                            _portraitFilenames[0] = infile.Val;
                        }

                        // handle vendors
                        else if (infile.Key == "vendor")
                        {
                            // @ATTR npc.vendor|bool|Allows this NPC to buy/sell items.
                            Vendor = Parse.ToBool(infile.Val);
                        }
                        else if (infile.Key == "vendor_requires_status")
                        {
                            // @ATTR npc.vendor_requires_status|list(string)|The player must have these statuses in order to use this NPC as a vendor.
                            while (infile.Val != "")
                            {
                                _vendorRequiresStatus.Add(SharedGameResources.Camp!.RegisterStatus(Parse.PopFirstString(ref infile.Val)));
                            }
                        }
                        else if (infile.Key == "vendor_requires_not_status")
                        {
                            // @ATTR npc.vendor_requires_not_status|list(string)|The player must not have these statuses in order to use this NPC as a vendor.
                            while (infile.Val != "")
                            {
                                _vendorRequiresNotStatus.Add(SharedGameResources.Camp!.RegisterStatus(Parse.PopFirstString(ref infile.Val)));
                            }
                        }
                        else if (infile.Key == "constant_stock")
                        {
                            // @ATTR npc.constant_stock|repeatable(list(item_id))|A list of items this vendor has for sale. Quantity can be specified by appending ":Q" to the item_id, where Q is an integer.
                            while (infile.Val != "")
                            {
                                stack = Parse.ToItemQuantityPair(Parse.PopFirstString(ref infile.Val));
                                stack.Item = SharedGameResources.Items!.VerifyID(stack.Item, infile, !ItemManager.VerifyAllowZero, !ItemManager.VerifyAllocate);

                                List<ItemStack> exStacks = new List<ItemStack>();
                                SharedGameResources.Items.GetExtendedStacks(stack.Item, (uint)stack.Quantity, exStacks);
                                for (int i = 0; i < exStacks.Count; ++i)
                                {
                                    Stock.Add(exStacks[i], ItemStorage.NoSlot);
                                }
                            }
                        }
                        else if (infile.Key == "status_stock")
                        {
                            // @ATTR npc.status_stock|repeatable(string, list(item_id)) : Required status, Item(s)|A list of items this vendor will have for sale if the required status is met. Quantity can be specified by appending ":Q" to the item_id, where Q is an integer.
                            if (SharedGameResources.Camp!.CheckStatus(SharedGameResources.Camp.RegisterStatus(Parse.PopFirstString(ref infile.Val))))
                            {
                                while (infile.Val != "")
                                {
                                    stack = Parse.ToItemQuantityPair(Parse.PopFirstString(ref infile.Val));
                                    stack.Item = SharedGameResources.Items!.VerifyID(stack.Item, infile, !ItemManager.VerifyAllowZero, !ItemManager.VerifyAllocate);

                                    List<ItemStack> exStacks = new List<ItemStack>();
                                    SharedGameResources.Items.GetExtendedStacks(stack.Item, (uint)stack.Quantity, exStacks);
                                    for (int i = 0; i < exStacks.Count; ++i)
                                    {
                                        Stock.Add(exStacks[i], ItemStorage.NoSlot);
                                    }
                                }
                            }
                        }
                        else if (infile.Key == "random_stock")
                        {
                            // @ATTR npc.random_stock|list(loot)|Use a loot table to add random items to the stock; either a filename or an inline definition.
                            if (clearRandomTable)
                            {
                                _randomTable.Clear();
                                clearRandomTable = false;
                            }

                            _randomTable.Add(new EventComponent());
                            SharedGameResources.Loot!.ParseLoot(ref infile.Val, _randomTable[^1], _randomTable);
                        }
                        else if (infile.Key == "random_stock_count")
                        {
                            // @ATTR npc.random_stock_count|int, int : Min, Max|Sets the minimum (and optionally, the maximum) amount of random items this npc can have.
                            _randomTableCount.X = Parse.PopFirstInt(ref infile.Val);
                            _randomTableCount.Y = Parse.PopFirstInt(ref infile.Val);
                            if (_randomTableCount.X != 0 || _randomTableCount.Y != 0)
                            {
                                _randomTableCount.X = Math.Max(_randomTableCount.X, 1);
                                _randomTableCount.Y = Math.Max(_randomTableCount.Y, _randomTableCount.X);
                            }
                        }
                        else if (infile.Key == "vendor_ratio_buy")
                        {
                            // @ATTR npc.vendor_ratio_buy|float|NPC-specific version of vendor_ratio_buy from engine/loot.txt. Uses the global setting when set to 0.
                            VendorRatioBuy = Parse.ToFloat(infile.Val);
                        }
                        else if (infile.Key == "vendor_ratio_sell")
                        {
                            // @ATTR npc.vendor_ratio_sell|float|NPC-specific version of vendor_ratio_sell from engine/loot.txt. Uses the global setting when set to 0.
                            VendorRatioSell = Parse.ToFloat(infile.Val);
                        }
                        else if (infile.Key == "vendor_ratio_sell_old")
                        {
                            // @ATTR npc.vendor_ratio_sell_old|float|NPC-specific version of vendor_ratio_sell_old from engine/loot.txt. Uses the global setting when set to 0.
                            VendorRatioSellOld = Parse.ToFloat(infile.Val);
                        }

                        // handle vocals
                        else if (infile.Key == "vox_intro")
                        {
                            // @ATTR npc.vox_intro|repeatable(filename)|Filename of a sound file to play when initially interacting with the NPC.
                            LoadSound(infile.Val, VoxIntro);
                        }

                        else if (infile.Key == "craft_constant_stock")
                        {
                            // @ATTR npc.craft_constant_stock|repeatable(list(item_id))|A list of items this vendor can craft.
                            while (infile.Val != "")
                            {
                                stack = Parse.ToItemQuantityPair(Parse.PopFirstString(ref infile.Val));
                                stack.Item = SharedGameResources.Items!.VerifyID(stack.Item, infile, !ItemManager.VerifyAllowZero, !ItemManager.VerifyAllocate);
                                stack.Quantity = 1;

                                List<ItemStack> exStacks = new List<ItemStack>();
                                SharedGameResources.Items.GetExtendedStacks(stack.Item, (uint)stack.Quantity, exStacks);
                                for (int i = 0; i < exStacks.Count; ++i)
                                {
                                    CraftStock.Add(exStacks[i], ItemStorage.NoSlot);
                                }
                            }
                        }
                        else if (infile.Key == "craft_status_stock")
                        {
                            // @ATTR npc.craft_status_stock|repeatable(string, list(item_id)) : Required status, Item(s)|A list of items this vendor can craft if the required status is met.
                            if (SharedGameResources.Camp!.CheckStatus(SharedGameResources.Camp.RegisterStatus(Parse.PopFirstString(ref infile.Val))))
                            {
                                while (infile.Val != "")
                                {
                                    stack = Parse.ToItemQuantityPair(Parse.PopFirstString(ref infile.Val));
                                    stack.Item = SharedGameResources.Items!.VerifyID(stack.Item, infile, !ItemManager.VerifyAllowZero, !ItemManager.VerifyAllocate);
                                    stack.Quantity = 1;

                                    List<ItemStack> exStacks = new List<ItemStack>();
                                    SharedGameResources.Items.GetExtendedStacks(stack.Item, (uint)stack.Quantity, exStacks);
                                    for (int i = 0; i < exStacks.Count; ++i)
                                    {
                                        CraftStock.Add(exStacks[i], ItemStorage.NoSlot);
                                    }
                                }
                            }
                        }
                        else if (infile.Key == "craft_random_stock")
                        {
                            // @ATTR npc.craft_random_stock|list(loot)|Use a loot table to add random items to the Craft stock; either a filename or an inline definition.
                            if (clearCraftRandomTable)
                            {
                                _craftRandomTable.Clear();
                                clearCraftRandomTable = false;
                            }

                            _craftRandomTable.Add(new EventComponent());
                            SharedGameResources.Loot!.ParseLoot(ref infile.Val, _craftRandomTable[^1], _craftRandomTable);
                        }
                        else if (infile.Key == "craft_random_stock_count")
                        {
                            // @ATTR npc.craft_random_stock_count|int, int : Min, Max|Sets the minimum (and optionally, the maximum) amount of random items this npc can have in their Craft stock.
                            _craftRandomTableCount.X = Parse.PopFirstInt(ref infile.Val);
                            _craftRandomTableCount.Y = Parse.PopFirstInt(ref infile.Val);
                            if (_craftRandomTableCount.X != 0 || _craftRandomTableCount.Y != 0)
                            {
                                _craftRandomTableCount.X = Math.Max(_craftRandomTableCount.X, 1);
                                _craftRandomTableCount.Y = Math.Max(_craftRandomTableCount.Y, _craftRandomTableCount.X);
                            }
                        }

                        else if (infile.Key == "vendor_tab_enabled")
                        {
                            // @ATTR npc.vendor_tab_enabled|repeatable(["buy", "sell", "craft"], bool) : Tab, Enabled|Sets the enabled state of the specified vendor tab for this NPC. The "buy" and "sell" tabs are enabled by default.
                            string tab = Parse.PopFirstString(ref infile.Val);
                            bool enabled = Parse.ToBool(Parse.PopFirstString(ref infile.Val));

                            if (tab == "buy") VendorTabEnabled[ItemManager.VendorBuy] = enabled;
                            else if (tab == "sell") VendorTabEnabled[ItemManager.VendorSell] = enabled;
                            else if (tab == "craft") VendorTabEnabled[ItemManager.VendorCraft] = enabled;
                            else infile.Error("NPC: '%s' is not a valid vendor tab.", tab);
                        }

                        else
                        {
                            infile.Error("NPC: '%s' is not a valid key.", infile.Key);
                        }
                    }
                }
                infile.Close();
            }
            else
            {
                return false;
            }

            LoadAnimations();
            LoadGraphics(); // TODO rename?

            // fill inventory with items from random stock table
            int randCount = MathUtils.RandBetween(_randomTableCount.X, _randomTableCount.Y);

            List<ItemStack> randItemstacks = new List<ItemStack>();
            for (int i = 0; i < randCount; ++i)
            {
                SharedGameResources.Loot!.CheckLoot(_randomTable, null, randItemstacks);
            }
            randItemstacks.Sort((a, b) =>
            {
                if (ItemManager.CompareItemStack(a, b))
                    return -1;
                if (ItemManager.CompareItemStack(b, a))
                    return 1;
                return 0;
            });
            for (int i = 0; i < randItemstacks.Count; ++i)
            {
                Stock.Add(randItemstacks[i], ItemStorage.NoSlot);
            }

            randCount = MathUtils.RandBetween(_craftRandomTableCount.X, _craftRandomTableCount.Y);

            randItemstacks.Clear();
            for (int i = 0; i < randCount; ++i)
            {
                SharedGameResources.Loot!.CheckLoot(_craftRandomTable, null, randItemstacks);
            }
            randItemstacks.Sort((a, b) =>
            {
                if (ItemManager.CompareItemStack(a, b))
                    return -1;
                if (ItemManager.CompareItemStack(b, a))
                    return 1;
                return 0;
            });
            for (int i = 0; i < randItemstacks.Count; ++i)
            {
                CraftStock.Add(randItemstacks[i], ItemStorage.NoSlot);
            }

            // if no vendor tabs are enabled, fall back to the defaults
            bool vendorTabs = false;
            for (int i = 0; i < 3; ++i)
            {
                vendorTabs = vendorTabs || VendorTabEnabled[i];
            }
            if (!vendorTabs)
            {
                VendorTabEnabled[ItemManager.VendorBuy] = true;
                VendorTabEnabled[ItemManager.VendorSell] = true;
            }

            // warn if dialog nodes lack a topic
            string fullFilename = SharedResources.Mods!.Locate(npcId);
            for (int i = 0; i < Dialog.Count; ++i)
            {
                string topic = GetDialogTopic(i);
                if (topic == "")
                {
                    Utils.LogInfo("[%s] NPC: Dialog node %d does not have a topic.", fullFilename, i);
                }
            }

            return true;
        }

        private void LoadGraphics()
        {
            Portraits.Clear();
            for (int i = 0; i < _portraitFilenames.Count; ++i)
            {
                Portraits.Add(null);
            }

            for (int i = 0; i < _portraitFilenames.Count; ++i)
            {
                if (_portraitFilenames[i] != "")
                {
                    Image? graphics;
                    graphics = SharedResources.RenderDevice!.LoadImage(_portraitFilenames[i], RenderDevice.ErrorNormal);
                    if (graphics != null)
                    {
                        Portraits[i] = graphics.CreateSprite();
                        graphics.Unref();
                    }
                }
            }
        }

        /// <summary>
        /// filename assumes the file is in soundfx/npcs/
        /// vox_type is a const int enum, see NPC.h
        /// returns -1 if not loaded or error.
        /// returns index in specific vector where to be found.
        /// </summary>
        private int LoadSound(string fname, int voxType)
        {
            SoundID a = SharedResources.Snd!.Load(fname, "NPC voice");

            if (a == 0)
                return -1;

            if (voxType == VoxIntro)
            {
                _voxIntro.Add(a);
                return _voxIntro.Count - 1;
            }

            if (voxType == VoxQuest)
            {
                _voxQuests.Add(a);
                return _voxQuests.Count - 1;
            }
            return -1;
        }

        public void Logic()
        {
            SharedGameResources.Mapr!.Collider.Unblock(Stats.Pos.X, Stats.Pos.Y);

            base.Logic();
            MoveMapEvents();

            if (!Stats.HeroAlly)
                SharedGameResources.Mapr.Collider.Block(Stats.Pos.X, Stats.Pos.Y, true);
        }

        public bool PlaySoundIntro()
        {
            if (_voxIntro.Count == 0)
                return false;

            int roll = Program.Rng.Next() % _voxIntro.Count;
            SharedResources.Snd!.Play(_voxIntro[roll], "NPC_VOX", Stats.Pos, !SoundManager.Loop);
            return true;
        }

        private bool PlaySoundQuest(int id)
        {
            if (id < 0 || id >= _voxQuests.Count)
                return false;

            SharedResources.Snd!.Play(_voxQuests[id], "NPC_VOX", Stats.Pos, !SoundManager.Loop);
            return true;
        }

        /// <summary>
        /// get list of available dialogs with NPC
        /// </summary>
        public void GetDialogNodes(List<int> result, bool allowResponses)
        {
            result.Clear();
            if (!Talker)
                return;

            string group = "";
            SortedDictionary<string, List<int>> groups = new SortedDictionary<string, List<int>>();

            for (int i = Dialog.Count; i > 0; i--)
            {
                bool isAvailable = true;
                bool isGrouped = false;
                int dindex = i - 1;

                for (int j = 0; j < Dialog[dindex].Count; j++)
                {
                    EventComponent ec = Dialog[dindex][j];

                    if (ec.Type == EventComponent.NpcDialogGroup)
                    {
                        isGrouped = true;
                        group = ec.S;
                    }
                    else if (ec.Type == EventComponent.NpcDialogResponseOnly)
                    {
                        if (ec.Data[0].Bool && !allowResponses)
                        {
                            isAvailable = false;
                            break;
                        }
                    }
                    else
                    {
                        if (SharedGameResources.Camp!.CheckAllRequirements(ec))
                            continue;

                        isAvailable = false;
                        break;
                    }
                }

                if (isAvailable)
                {
                    if (!isGrouped)
                    {
                        result.Add(dindex);
                    }
                    else
                    {
                        if (!groups.ContainsKey(group))
                        {
                            groups[group] = new List<int> { dindex };
                        }
                        else
                        {
                            groups[group].Add(dindex);
                        }
                    }
                }
            }

            /* Iterate over dialoggroups and roll a dialog to add to result */
            if (groups.Count == 0)
                return;

            foreach (KeyValuePair<string, List<int>> groupEntry in groups)
            {
                if (groupEntry.Value.Count == 0)
                    break;

                /* roll a dialog for this group and add to result */
                int di = groupEntry.Value[Program.Rng.Next() % groupEntry.Value.Count];
                result.Insert(0, di);
            }
        }

        public void GetDialogResponses(List<int> result, int nodeId, int eventCursor)
        {
            if (nodeId >= Dialog.Count)
                return;

            if (eventCursor >= Dialog[nodeId].Count)
                return;

            List<int> responseIds = new List<int>();
            for (int i = eventCursor; i > 0; i--)
            {
                if (IsDialogType(Dialog[nodeId][i - 1].Type))
                    break;

                if (Dialog[nodeId][i - 1].Type == EventComponent.NpcDialogResponse)
                    responseIds.Add(i - 1);
            }

            if (responseIds.Count == 0)
                return;

            List<int> nodes = new List<int>();
            GetDialogNodes(nodes, GetResponseNodes);

            for (int i = 0; i < responseIds.Count; i++)
            {
                for (int j = 0; j < nodes.Count; j++)
                {
                    string id = "";

                    for (int k = 0; k < Dialog[nodes[j]].Count; k++)
                    {
                        if (Dialog[nodes[j]][k].Type == EventComponent.NpcDialogID)
                        {
                            id = Dialog[nodes[j]][k].S;
                            break;
                        }
                    }

                    if (id == "")
                        continue;

                    if (id == Dialog[nodeId][responseIds[i]].S)
                    {
                        result.Add(nodes[j]);
                        break;
                    }
                }
            }
        }

        public string GetDialogTopic(int dialogNode)
        {
            if (!Talker)
                return "";

            for (int j = 0; j < Dialog[dialogNode].Count; j++)
            {
                if (Dialog[dialogNode][j].Type == EventComponent.NpcDialogTopic)
                    return Dialog[dialogNode][j].S;
            }

            return "";
        }

        /// <summary>
        /// Check if the hero can move during this dialog branch
        /// </summary>
        public bool CheckMovement(int dialogNode)
        {
            if (dialogNode < Dialog.Count)
            {
                for (int i = 0; i < Dialog[dialogNode].Count; i++)
                {
                    if (Dialog[dialogNode][i].Type == EventComponent.NpcAllowMovement)
                        return Parse.ToBool(Dialog[dialogNode][i].S);
                }
            }
            return true;
        }

        public void MoveMapEvents()
        {
            for (int it = SharedGameResources.Mapr!.Events.Count; it > 0; )
            {
                --it;

                if (SharedGameResources.Mapr.Events[it].Type == Filename)
                {
                    if (Stats.Hp > 0)
                    {
                        // Update event position after NPC has moved
                        SharedGameResources.Mapr.Events[it].Location.X = (int)Stats.Pos.X;
                        SharedGameResources.Mapr.Events[it].Location.Y = (int)Stats.Pos.Y;

                        SharedGameResources.Mapr.Events[it].Hotspot.X = (int)Stats.Pos.X;
                        SharedGameResources.Mapr.Events[it].Hotspot.Y = (int)Stats.Pos.Y;

                        SharedGameResources.Mapr.Events[it].Center.X = (float)SharedGameResources.Mapr.Events[it].Hotspot.X + (float)SharedGameResources.Mapr.Events[it].Hotspot.Width / 2;
                        SharedGameResources.Mapr.Events[it].Center.Y = (float)SharedGameResources.Mapr.Events[it].Hotspot.Y + (float)SharedGameResources.Mapr.Events[it].Hotspot.Height / 2;

                        for (int ci = 0; ci < SharedGameResources.Mapr.Events[it].Components.Count; ci++)
                        {
                            if (SharedGameResources.Mapr.Events[it].Components[ci].Type == EventComponent.NpcHotspot)
                            {
                                SharedGameResources.Mapr.Events[it].Components[ci].Data[0].Int = (int)Stats.Pos.X;
                                SharedGameResources.Mapr.Events[it].Components[ci].Data[1].Int = (int)Stats.Pos.Y;
                            }
                        }
                    }
                    else
                    {
                        // NPC is dead! Remove the map event
                        SharedGameResources.Mapr.Events.RemoveAt(it);
                    }
                }
            }
        }

        public bool CheckVendor()
        {
            if (!Vendor)
                return false;

            for (int i = 0; i < _vendorRequiresStatus.Count; ++i)
            {
                if (!SharedGameResources.Camp!.CheckStatus(_vendorRequiresStatus[i]))
                    return false;
            }

            for (int i = 0; i < _vendorRequiresNotStatus.Count; ++i)
            {
                if (SharedGameResources.Camp!.CheckStatus(_vendorRequiresNotStatus[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Process the current dialog
        ///
        /// Return false if the dialog has ended
        /// </summary>
        public bool ProcessDialog(int dialogNode, ref int eventCursor)
        {
            if (dialogNode >= Dialog.Count)
                return false;

            NpcPortrait = Portraits[0];
            HeroPortrait = null;

            while (eventCursor < Dialog[dialogNode].Count)
            {
                // we've already determined requirements are met, so skip these
                if (Dialog[dialogNode][eventCursor].Type == EventComponent.RequiresStatus)
                {
                    // continue to next event component
                }
                else if (Dialog[dialogNode][eventCursor].Type == EventComponent.RequiresNotStatus)
                {
                    // continue to next event component
                }
                else if (Dialog[dialogNode][eventCursor].Type == EventComponent.RequiresLevel)
                {
                    // continue to next event component
                }
                else if (Dialog[dialogNode][eventCursor].Type == EventComponent.RequiresNotLevel)
                {
                    // continue to next event component
                }
                else if (Dialog[dialogNode][eventCursor].Type == EventComponent.RequiresCurrency)
                {
                    // continue to next event component
                }
                else if (Dialog[dialogNode][eventCursor].Type == EventComponent.RequiresNotCurrency)
                {
                    // continue to next event component
                }
                else if (Dialog[dialogNode][eventCursor].Type == EventComponent.RequiresItem)
                {
                    // continue to next event component
                }
                else if (Dialog[dialogNode][eventCursor].Type == EventComponent.RequiresNotItem)
                {
                    // continue to next event component
                }
                else if (Dialog[dialogNode][eventCursor].Type == EventComponent.RequiresClass)
                {
                    // continue to next event component
                }
                else if (Dialog[dialogNode][eventCursor].Type == EventComponent.RequiresNotClass)
                {
                    // continue to next event component
                }
                else if (Dialog[dialogNode][eventCursor].Type == EventComponent.NpcDialogID)
                {
                    // continue to next event component
                }
                else if (Dialog[dialogNode][eventCursor].Type == EventComponent.NpcDialogResponse)
                {
                    // continue to next event component
                }
                else if (Dialog[dialogNode][eventCursor].Type == EventComponent.NpcDialogResponseOnly)
                {
                    // continue to next event component
                }
                else if (Dialog[dialogNode][eventCursor].Type == EventComponent.NpcDialogThem)
                {
                    return true;
                }
                else if (Dialog[dialogNode][eventCursor].Type == EventComponent.NpcDialogYou)
                {
                    return true;
                }
                else if (Dialog[dialogNode][eventCursor].Type == EventComponent.NpcVoice)
                {
                    PlaySoundQuest(Dialog[dialogNode][eventCursor].Data[0].Int);
                }
                else if (Dialog[dialogNode][eventCursor].Type == EventComponent.NpcPortraitThem)
                {
                    NpcPortrait = Portraits[0];
                    for (int i = 0; i < _portraitFilenames.Count; ++i)
                    {
                        if (Dialog[dialogNode][eventCursor].S == _portraitFilenames[i])
                        {
                            NpcPortrait = Portraits[i];
                            break;
                        }
                    }
                }
                else if (Dialog[dialogNode][eventCursor].Type == EventComponent.NpcPortraitYou)
                {
                    HeroPortrait = null;
                    for (int i = 0; i < _portraitFilenames.Count; ++i)
                    {
                        if (Dialog[dialogNode][eventCursor].S == _portraitFilenames[i])
                        {
                            HeroPortrait = Portraits[i];
                            break;
                        }
                    }
                }
                else if (Dialog[dialogNode][eventCursor].Type == EventComponent.NpcTakeAParty)
                {
                    bool newHeroAlly = Dialog[dialogNode][eventCursor].Data[0].Bool;
                    if (Stats.HeroAlly != newHeroAlly)
                    {
                        Stats.HeroAlly = newHeroAlly;
                        if (Stats.HeroAlly)
                        {
                            SharedGameResources.Entitym!.Entities.Add(this);
                        }
                        else
                        {
                            for (int i = SharedGameResources.Entitym.Entities.Count; i > 0; --i)
                            {
                                if (SharedGameResources.Entitym.Entities[i - 1] == this)
                                    SharedGameResources.Entitym.Entities.RemoveAt(i - 1);
                            }
                        }
                    }
                }
                else if (Dialog[dialogNode][eventCursor].Type == EventComponent.None)
                {
                    // conversation ends
                    return false;
                }

                eventCursor++;
            }
            return false;
        }

        public void ProcessEvent(int dialogNode, int cursor)
        {
            if (dialogNode >= Dialog.Count)
                return;

            Event ev = new Event();

            if (cursor < Dialog[dialogNode].Count && IsDialogType(Dialog[dialogNode][cursor].Type))
            {
                cursor++;
            }

            while (cursor < Dialog[dialogNode].Count && !IsDialogType(Dialog[dialogNode][cursor].Type))
            {
                ev.Components.Add(Dialog[dialogNode][cursor]);
                cursor++;
            }

            SharedGameResources.Eventm!.ExecuteEvent(ev);
        }

        private bool IsDialogType(int eventType)
        {
            return eventType == EventComponent.NpcDialogThem || eventType == EventComponent.NpcDialogYou;
        }

        // general info
        public string Name = "";
        public string Filename = "";

        public int Direction;
        public bool ShowOnMinimap;

        // talker info
        public Sprite? NpcPortrait;
        public Sprite? HeroPortrait;
        public List<Sprite?> Portraits = new List<Sprite?>();
        public bool Talker;

        // vendor info
        public bool Vendor;
        public bool[] VendorTabEnabled = new bool[3];
        public bool ResetBuyback;
        public ItemStorage Stock = new ItemStorage();
        public ItemStorage CraftStock = new ItemStorage();
        public float VendorRatioBuy;
        public float VendorRatioSell;
        public float VendorRatioSellOld;

        // story and dialog options
        // outer vector is addressing the dialog and the inner vector is
        // addressing the events during one dialog
        public List<List<EventComponent>> Dialog = new List<List<EventComponent>>();
    }
}
