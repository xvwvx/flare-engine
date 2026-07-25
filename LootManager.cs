// 对应 C++ 源文件：LootManager.h + LootManager.cpp
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// LootManager
    ///
    /// 管理地面掉落物（对应原始 <c>class LootManager</c>）。
    ///
    /// 资源管理：C++ 原始版本对 <c>std::vector&lt;Loot&gt;</c>、预加载的
    /// <c>std::vector&lt;Animation*&gt;*</c> 缓存以及音效 ID 使用手动 new/delete。
    /// C# 版本假定 <see cref="Loot"/> 与 <see cref="Animation"/> 实现
    /// <see cref="IDisposable"/>；在每次原始 <c>delete</c> / 容器销毁点调用
    /// <c>Dispose()</c>；本类自身也实现 <see cref="IDisposable"/>，
    /// <see cref="Dispose"/> 对应原始析构函数，释放顺序与 C++ 逐行一致。
    ///
    /// 全局指针映射：<c>eset</c> → <see cref="SharedResources.Eset"/>，
    /// <c>snd</c> → <see cref="SharedResources.Snd"/>，
    /// <c>items</c> → <see cref="SharedGameResources.Items"/>，
    /// <c>anim</c> → <see cref="SharedResources.Anim"/>，
    /// <c>pc</c> → <see cref="SharedGameResources.Pc"/>，
    /// <c>mapr</c> → <see cref="SharedGameResources.Mapr"/>，
    /// <c>inpt</c> → <see cref="SharedResources.Inpt"/>，
    /// <c>settings</c> → <see cref="SharedResources.Settings"/>，
    /// <c>camp</c> → <see cref="SharedGameResources.Camp"/>，
    /// <c>mods</c> → <see cref="SharedResources.Mods"/>，
    /// <c>msg</c> → <see cref="SharedResources.Msg"/>，
    /// <c>curs</c> → <see cref="SharedResources.Curs"/>，
    /// <c>entitym</c> → <see cref="SharedGameResources.Entitym"/>，
    /// <c>fow</c> → <see cref="SharedGameResources.Fow"/>。
    /// </summary>
    public class LootManager : IDisposable
    {
        private SoundID _sfxLoot;
        private string _sfxLootChannel = "";

        // loot 条目对应 ItemManager 中的物品索引
        private List<Loot> _loot = new List<Loot>();

        // 应掉落战利品但尚未处理的敌人
        private List<StatBlock?> _enemiesDroppingLoot = new List<StatBlock?>();

        // loot/ 目录下文件定义的掉落表
        private Dictionary<string, List<EventComponent>> _lootTables = new Dictionary<string, List<EventComponent>>();

        // 防止同一格子上重复掉落：记录需要解除阻挡的格子
        private List<Int2> _tilesToUnblock = new List<Int2>();

        private List<List<Animation?>?> _animations = new List<List<Animation?>?>();

        public const bool DroppedByHero = true;

        public const int LootEcPosX = 0;
        public const int LootEcPosY = 1;
        public const int LootEcChance = 2;
        public const int LootEcQuantityMin = 3;
        public const int LootEcQuantityMax = 4;
        public const int LootEcMaxDrops = 5;
        public const int LootEcRequiresLevelMin = 6;
        public const int LootEcRequiresLevelMax = 7;
        public const int LootEcQuantityPerLevelMin = 8;
        public const int LootEcQuantityPerLevelMax = 9;
        public const int LootEcType = 10;

        public const int LootEcTypeSingle = 0;
        public const int LootEcTypeTable = 1;
        public const int LootEcTypeTableRow = 2;

        public LootManager()
        {
            _sfxLoot = 0;
            _sfxLootChannel = "loot";
            if (!string.IsNullOrEmpty(SharedResources.Eset!.Loot.SfxLoot))
            {
                _sfxLoot = SharedResources.Snd!.Load(SharedResources.Eset.Loot.SfxLoot, "LootManager dropping loot");
            }

            LoadGraphics();
            LoadLootTables();
        }

        /// <summary>
        /// 每个物品的 loot 字段指向该物品的"飞行掉落"动画；此处预加载物品数据库中使用的全部动画。
        /// </summary>
        private void LoadGraphics()
        {
            if (_animations.Count != 0)
            {
                Utils.LogError("LootManger: loadGraphics() detected existing animations, aborting.");
                return;
            }

            for (int i = 0; i < SharedGameResources.Items!.Items.Count; ++i)
                _animations.Add(null);

            for (int i = 1; i < SharedGameResources.Items.Items.Count; ++i)
            {
                Item? item = SharedGameResources.Items.Items[i];

                if (item == null || item.LootAnimation.Count == 0)
                    continue;

                _animations[i] = new List<Animation?>(new Animation?[item.LootAnimation.Count]);

                for (int j = 0; j < item.LootAnimation.Count; ++j)
                {
                    SharedResources.Anim!.IncreaseCount(item.LootAnimation[j].Name);
                    _animations[i]![j] = SharedResources.Anim.GetAnimationSet(item.LootAnimation[j].Name)!.GetAnimation("");
                }
            }
        }

        public void HandleNewMap()
        {
            for (int i = 0; i < _loot.Count; ++i)
                _loot[i].Dispose();
            _loot.Clear();
            _enemiesDroppingLoot.Clear();
        }

        public void Logic()
        {
            if (SharedResources.Inpt!.Pressing[Input.LootTooltipMode] && !SharedResources.Inpt.Lock[Input.LootTooltipMode])
            {
                SharedResources.Inpt.Lock[Input.LootTooltipMode] = true;
                SharedResources.Settings!.LootTooltips++;
                if (SharedResources.Settings.LootTooltips > Settings.LootTipsHideAll)
                    SharedResources.Settings.LootTooltips = Settings.LootTipsDefault;

                if (SharedResources.Settings.LootTooltips == Settings.LootTipsHideAll)
                    SharedGameResources.Pc!.LogMsg(SharedResources.Msg!.Get("Loot tooltip visibility") + ": " + SharedResources.Msg.Get("Hidden"), Avatar.MsgUnique);
                else if (SharedResources.Settings.LootTooltips == Settings.LootTipsDefault)
                    SharedGameResources.Pc.LogMsg(SharedResources.Msg.Get("Loot tooltip visibility") + ": " + SharedResources.Msg.Get("Default"), Avatar.MsgUnique);
                else if (SharedResources.Settings.LootTooltips == Settings.LootTipsShowAll)
                    SharedGameResources.Pc.LogMsg(SharedResources.Msg.Get("Loot tooltip visibility") + ": " + SharedResources.Msg.Get("Show All"), Avatar.MsgUnique);
            }

            for (int it = 0; it < _loot.Count; ++it)
            {
                if (_loot[it].Animation != null)
                {
                    _loot[it].Animation!.AdvanceFrame();
                    if (!_loot[it].OnGround && ((_loot[it].Animation!.DefaultActiveFrames && _loot[it].Animation.IsSecondLastFrame()) || (!_loot[it].Animation.DefaultActiveFrames && _loot[it].Animation.IsActiveFrame())))
                    {
                        _loot[it].OnGround = true;
                    }
                }

                if (_loot[it].OnGround && !_loot[it].SoundPlayed && !_loot[it].Stack.Empty())
                {
                    Int2 pos = new Int2((int)_loot[it].Pos.X, (int)_loot[it].Pos.Y);
                    SharedGameResources.Items!.PlaySound(_loot[it].Stack.Item, pos);
                    _loot[it].SoundPlayed = true;
                }
            }

            CheckEnemiesForLoot();
            CheckMapForLoot();

            for (int i = 0; i < _tilesToUnblock.Count; i++)
            {
                SharedGameResources.Mapr!.Collider.Unblock(_tilesToUnblock[i].X, _tilesToUnblock[i].Y);
            }
            _tilesToUnblock.Clear();
        }

        /// <summary>显示地面上所有掉落物的工具提示。</summary>
        public void RenderTooltips(Vector2 cam)
        {
            if (!SharedResources.Settings!.ShowHud) return;

            Int2 dest;
            bool tooltipBelow = true;
            Rectangle screenRect = new Rectangle(0, 0, SharedResources.Settings.ViewW, SharedResources.Settings.ViewH);

            for (int it = 0; it < _loot.Count; )
            {
                _loot[it].TipVisible = false;

                if (_loot[it].OnGround)
                {
                    if (SharedGameResources.Mapr!.Fogofwar > FogOfWar.TypeMinimap)
                    {
                        float delta = Utils.CalcDist(SharedGameResources.Pc!.Stats.Pos, _loot[it].Pos);
                        if (delta > SharedGameResources.Fow!.MaskRadius - 1.0f)
                        {
                            break;
                        }
                    }

                    Int2 p = Utils.MapToScreen(_loot[it].Pos.X, _loot[it].Pos.Y, cam.X, cam.Y);
                    if (!Utils.IsWithinRect(screenRect, p))
                    {
                        ++it;
                        continue;
                    }

                    dest.X = p.X;
                    dest.Y = p.Y + SharedResources.Eset!.Tileset.TileHHalf;

                    dest.Y -= SharedResources.Eset.Loot.TooltipMargin;

                    Rectangle hover = new Rectangle();
                    hover.X = p.X - SharedResources.Eset.Tileset.TileWHalf;
                    hover.Y = p.Y - SharedResources.Eset.Tileset.TileHHalf;
                    hover.Width = SharedResources.Eset.Tileset.TileW;
                    hover.Height = SharedResources.Eset.Tileset.TileH;

                    bool forcedVisibility = false;
                    bool defaultVisibility = true;

                    if (SharedResources.Settings.LootTooltips == Settings.LootTipsDefault && SharedResources.Eset.Loot.HideRadius > 0)
                    {
                        if (Utils.CalcDist(SharedGameResources.Pc!.Stats.Pos, _loot[it].Pos) < SharedResources.Eset.Loot.HideRadius)
                        {
                            defaultVisibility = false;
                        }
                        else
                        {
                            float? savedDistance = null;
                            Entity? testEnemy = SharedGameResources.Entitym!.GetNearestEntity(_loot[it].Pos, !EntityManager.GetCorpse, ref savedDistance, SharedResources.Eset.Loot.HideRadius);
                            if (testEnemy != null)
                            {
                                defaultVisibility = false;
                            }
                        }
                    }
                    else if (SharedResources.Settings.LootTooltips == Settings.LootTipsHideAll)
                    {
                        defaultVisibility = false;
                    }
                    else if (SharedResources.Settings.LootTooltips == Settings.LootTipsShowAll && SharedResources.Inpt!.Pressing[Input.Alt])
                    {
                        defaultVisibility = false;
                    }

                    if (!defaultVisibility)
                        forcedVisibility = Utils.IsWithinRect(hover, SharedResources.Inpt!.Mouse) || (SharedResources.Inpt.Pressing[Input.Alt] && SharedResources.Settings.LootTooltips != Settings.LootTipsShowAll);

                    if (defaultVisibility || forcedVisibility)
                    {
                        _loot[it].TipVisible = true;

                        if (_loot[it].Tip.IsEmpty())
                        {
                            if (!_loot[it].Stack.Empty())
                            {
                                _loot[it].Tip = SharedGameResources.Items!.GetShortTooltip(_loot[it].Stack);
                            }
                        }

                        _loot[it].Wtip!.Prerender(_loot[it].Tip, dest, TooltipData.StyleTopLabel);
                        for (int testIt = 0; testIt < it; )
                        {
                            if (_loot[testIt].TipVisible && Utils.RectsOverlap(_loot[testIt].Wtip!.Bounds, _loot[it].Wtip!.Bounds))
                            {
                                if (tooltipBelow)
                                    dest.Y = _loot[testIt].Wtip!.Bounds.Y + _loot[testIt].Wtip!.Bounds.Height + SharedResources.Eset!.Tooltips.Offset;
                                else
                                    dest.Y = _loot[testIt].Wtip!.Bounds.Y - _loot[testIt].Wtip!.Bounds.Height + SharedResources.Eset!.Tooltips.Offset;

                                _loot[it].Wtip!.Bounds.Y = dest.Y;
                            }

                            ++testIt;
                        }

                        _loot[it].Wtip!.Render(_loot[it].Tip, dest, TooltipData.StyleTopLabel);

                        if (SharedResources.Settings.LootTooltips == Settings.LootTipsHideAll && !SharedResources.Inpt!.Pressing[Input.Alt])
                            break;
                    }
                }

                tooltipBelow = !tooltipBelow;

                ++it;
            }
        }

        /// <summary>处理标记为应掉落战利品的敌人。</summary>
        private void CheckEnemiesForLoot()
        {
            for (int i = 0; i < _enemiesDroppingLoot.Count; ++i)
            {
                StatBlock? e = _enemiesDroppingLoot[i];

                if (e == null)
                    continue;

                if (e.QuestLootId != 0)
                {
                    List<EventComponent> questLootTable = new List<EventComponent>();
                    EventComponent ec = new EventComponent();
                    ec.Type = EventComponent.Loot;
                    ec.Id = e.QuestLootId;
                    ec.Data[LootEcQuantityMin].Int = 1;
                    ec.Data[LootEcQuantityMax].Int = 1;
                    ec.Data[LootEcChance].Float = 0;

                    questLootTable.Add(ec);
                    CheckLoot(questLootTable, e.Pos, null);
                }

                if (e.LootTable.Count != 0)
                {
                    int drops;
                    if (e.LootCount.Y != 0)
                    {
                        drops = MathUtils.RandBetween(e.LootCount.X, e.LootCount.Y);
                    }
                    else
                    {
                        drops = MathUtils.RandBetween(1, SharedResources.Eset!.Loot.DropMax);
                    }

                    for (int j = 0; j < drops; ++j)
                    {
                        CheckLoot(e.LootTable, e.Pos, null);
                    }

                    e.LootTable.Clear();
                }
            }
            _enemiesDroppingLoot.Clear();
        }

        /// <summary>处理地图事件中名为 "loot" 的组件。</summary>
        private void CheckMapForLoot()
        {
            for (int i = 0; i < SharedGameResources.Mapr!.Loot.Count; ++i)
            {
                int drops;
                if (SharedGameResources.Mapr.Loot[i].Second.Y != 0)
                {
                    drops = MathUtils.RandBetween(SharedGameResources.Mapr.Loot[i].Second.X, SharedGameResources.Mapr.Loot[i].Second.Y);
                }
                else
                {
                    drops = MathUtils.RandBetween(1, SharedResources.Eset!.Loot.DropMax);
                }

                while (drops > 0)
                {
                    CheckLoot(SharedGameResources.Mapr.Loot[i].First, null, null);
                    drops--;
                }
            }

            SharedGameResources.Mapr.Loot.Clear();
        }

        public void AddEnemyLoot(StatBlock e)
        {
            _enemiesDroppingLoot.Add(e);
        }

        public void CheckLoot(List<EventComponent> lootTable, Vector2? pos, List<ItemStack>? itemstackVec)
        {
            Vector2 p;
            EventComponent ec;
            ItemStack newLoot = new ItemStack();
            List<EventComponent> possibleIds = new List<EventComponent>();

            float chance = MathUtils.RandBetweenF(0, 100);

            for (int i = lootTable.Count; i > 0; i--)
            {
                ec = lootTable[i - 1];

                if (ec.Data[LootEcType].Int == LootEcTypeTable)
                    continue;

                if (ec.Data[LootEcChance].Float == 0)
                {
                    if (ec.Status == 0 || (ec.Status > 0 && SharedGameResources.Camp!.CheckStatus(ec.Status)))
                    {
                        CheckLootComponent(ec, pos, itemstackVec);
                    }
                    lootTable.RemoveAt(i - 1);
                }
            }

            float threshold = (float)(SharedGameResources.Pc!.Stats.Get(Stats.ItemFind) + 100);
            for (int i = 0; i < lootTable.Count; i++)
            {
                ec = lootTable[i];

                if (ec.Data[LootEcType].Int == LootEcTypeTable)
                    continue;

                float realChance = ec.Data[LootEcChance].Float;

                if (ec.Id != 0)
                {
                    realChance = realChance * (SharedGameResources.Pc!.Stats.Get(Stats.ItemFind) + 100.0f) / 100.0f;
                }

                bool levelCheck = true;
                if (ec.Data[LootEcRequiresLevelMin].Int > 0 && ec.Data[LootEcRequiresLevelMax].Int > 0)
                {
                    levelCheck = SharedGameResources.Pc!.Stats.Level >= ec.Data[LootEcRequiresLevelMin].Int && SharedGameResources.Pc.Stats.Level <= ec.Data[LootEcRequiresLevelMax].Int;
                }

                if (realChance >= chance && levelCheck && (ec.Status == 0 || (ec.Status > 0 && SharedGameResources.Camp!.CheckStatus(ec.Status))))
                {
                    if (realChance <= threshold)
                    {
                        if (realChance != threshold)
                        {
                            possibleIds.Clear();
                        }

                        threshold = realChance;
                    }

                    if (chance <= threshold && ec.Data[LootEcMaxDrops].Int != 0)
                    {
                        possibleIds.Add(ec);
                    }
                }
            }

            if (possibleIds.Count != 0)
            {
                int chosenLoot = Program.Rng.Next() % possibleIds.Count;
                ec = possibleIds[chosenLoot];
                CheckLootComponent(ec, pos, itemstackVec);

                if (ec.Data[LootEcMaxDrops].Int > 0)
                {
                    ec.Data[LootEcMaxDrops].Int--;
                }
            }
        }

        public void AddLoot(ItemStack stack, Vector2 pos, bool droppedByHero)
        {
            if (stack.Empty() || !SharedGameResources.Items!.IsValid(stack.Item))
                return;

            Loot ld = new Loot();
            ld.Stack = stack;
            ld.Pos.X = pos.X;
            ld.Pos.Y = pos.Y;
            ld.Pos.Align();
            ld.DroppedByHero = droppedByHero;

            for (int it = _loot.Count; it != 0; )
            {
                --it;
                if (_loot[it].Stack.Item == ld.Stack.Item && _loot[it].Pos.X == ld.Pos.X && _loot[it].Pos.Y == ld.Pos.Y)
                {
                    _loot[it].Stack.Quantity += ld.Stack.Quantity;
                    _loot[it].Tip.Clear();
                    SharedResources.Snd!.Play(_sfxLoot, _sfxLootChannel, pos, false);
                    ld.Dispose();
                    return;
                }
            }

            if (SharedGameResources.Items.Items[stack.Item]!.LootAnimation.Count != 0)
            {
                int index = SharedGameResources.Items.Items[stack.Item]!.LootAnimation.Count - 1;

                for (int i = 0; i < SharedGameResources.Items.Items[stack.Item]!.LootAnimation.Count; ++i)
                {
                    int low = SharedGameResources.Items.Items[stack.Item]!.LootAnimation[i].Low;
                    int high = SharedGameResources.Items.Items[stack.Item]!.LootAnimation[i].High;
                    if (stack.Quantity >= low && (stack.Quantity <= high || high == 0))
                    {
                        index = i;
                        break;
                    }
                }
                ld.LoadAnimation(SharedGameResources.Items.Items[stack.Item]!.LootAnimation[index].Name);
            }
            else
            {
                ld.OnGround = true;
            }

            _loot.Add(ld);
            SharedResources.Snd!.Play(_sfxLoot, _sfxLootChannel, pos, false);
        }

        /// <summary>点击地图拾取掉落物；需要相机位置将屏幕坐标转换为地图坐标。</summary>
        public ItemStack CheckPickup(Int2 mouse, Vector2 cam, Vector2 heroPos)
        {
            ItemStack lootStack = new ItemStack();

            if (SharedResources.Inpt!.UsingMouse())
            {
                Int2 mousePos = mouse;

                bool mouseMoveTarget = SharedGameResources.Pc!.MmTargetObject == Avatar.MmTargetLoot && SharedGameResources.Pc.IsNearMMtarget();
                if (mouseMoveTarget && (SharedGameResources.Pc.Stats.CurState == StatBlock.EntityStance || SharedGameResources.Pc.Stats.CurState == StatBlock.EntityMove))
                {
                    SharedGameResources.Pc.Stats.CurState = StatBlock.EntityStance;
                    mousePos = Utils.MapToScreen(SharedGameResources.Pc.MmTargetObjectPos.X, SharedGameResources.Pc.MmTargetObjectPos.Y, cam.X, cam.Y);
                }
                else if (SharedGameResources.Pc.MmTargetObject == Avatar.MmTargetLoot && SharedGameResources.Pc.Stats.CurState == StatBlock.EntityStance)
                {
                    SharedGameResources.Pc.Stats.CurState = StatBlock.EntityMove;
                }

                int itMatch = -1;
                for (int it = _loot.Count; it != 0; )
                {
                    --it;

                    if (!_loot[it].IsFlying())
                    {
                        Int2 p = Utils.MapToScreen(_loot[it].Pos.X, _loot[it].Pos.Y, cam.X, cam.Y);

                        Rectangle r = new Rectangle();
                        r.X = p.X - SharedResources.Eset!.Tileset.TileWHalf;
                        r.Y = p.Y - SharedResources.Eset.Tileset.TileHHalf;
                        r.Width = SharedResources.Eset.Tileset.TileW;
                        r.Height = SharedResources.Eset.Tileset.TileH;

                        if (_loot[it].TipVisible && Utils.IsWithinRect(_loot[it].Wtip!.Bounds, mousePos))
                        {
                            itMatch = it;

                            break;
                        }
                        else if (itMatch == -1 && Utils.IsWithinRect(r, mousePos))
                        {
                            itMatch = it;
                        }
                    }
                }

                int interactKey = (SharedResources.Settings!.MouseMove && SharedResources.Settings.MouseMoveSwap) ? Input.Main2 : Input.Main1;

                if (itMatch != -1 && !_loot[itMatch].Stack.Empty())
                {
                    if (Utils.CalcDist(heroPos, _loot[itMatch].Pos) < SharedResources.Eset!.Misc.InteractRange)
                    {
                        SharedResources.Curs!.SetCursor(CursorManager.CursorInteract);

                        if (!mouseMoveTarget && SharedResources.Inpt!.Pressing[interactKey] && !SharedResources.Inpt.Lock[interactKey])
                        {
                            SharedResources.Inpt.Lock[interactKey] = true;
                            lootStack = _loot[itMatch].Stack;
                            _loot[itMatch].Dispose();
                            _loot.RemoveAt(itMatch);
                            return lootStack;
                        }
                        else if (mouseMoveTarget)
                        {
                            SharedGameResources.Pc!.MmTargetObject = Avatar.MmTargetNone;
                            lootStack = _loot[itMatch].Stack;
                            _loot[itMatch].Dispose();
                            _loot.RemoveAt(itMatch);
                            return lootStack;
                        }
                    }
                    else if (SharedResources.Settings.MouseMove)
                    {
                        SharedResources.Curs!.SetCursor(CursorManager.CursorInteract);

                        if (SharedResources.Inpt!.Pressing[interactKey] && !SharedResources.Inpt.Lock[interactKey])
                        {
                            SharedResources.Inpt.Lock[interactKey] = true;

                            Vector2 targetPos = _loot[itMatch].Pos;
                            SharedGameResources.Pc!.SetDesiredMMTarget(ref targetPos);

                            SharedGameResources.Pc.MmTargetObject = Avatar.MmTargetLoot;
                            SharedGameResources.Pc.MmTargetObjectPos = _loot[itMatch].Pos;
                        }
                    }
                }
            }

            if (SharedResources.Inpt!.Pressing[Input.Accept] && !SharedResources.Inpt.Lock[Input.Accept])
            {
                lootStack = CheckNearestPickup(heroPos);
                if (!lootStack.Empty())
                {
                    SharedResources.Inpt.Lock[Input.Accept] = true;
                }
            }

            return lootStack;
        }

        /// <summary>若引擎启用了自动拾取，则尝试拾取（目前仅检查货币）。</summary>
        public ItemStack CheckAutoPickup(Vector2 heroPos)
        {
            ItemStack lootStack = new ItemStack();

            if (SharedResources.Settings!.AutoLoot == 0)
                return lootStack;

            for (int it = _loot.Count; it != 0; )
            {
                --it;
                if (!_loot[it].DroppedByHero && Utils.CalcDist(heroPos, _loot[it].Pos) < SharedResources.Eset!.Loot.AutopickupRange && !_loot[it].IsFlying())
                {
                    bool doPickup = false;
                    if (_loot[it].Stack.Item == SharedResources.Eset!.Misc.CurrencyId && SharedResources.Eset.Loot.AutopickupCurrency)
                    {
                        doPickup = true;
                    }
                    else if (SharedResources.Settings.AutoLoot == 1 && SharedGameResources.Items!.CheckAutoPickup(_loot[it].Stack.Item))
                    {
                        doPickup = true;
                    }

                    if (doPickup)
                    {
                        lootStack = _loot[it].Stack;
                        _loot[it].Dispose();
                        _loot.RemoveAt(it);
                        return lootStack;
                    }
                }
            }
            return lootStack;
        }

        public ItemStack CheckNearestPickup(Vector2 heroPos)
        {
            ItemStack lootStack = new ItemStack();

            float bestDistance = float.MaxValue;

            int nearest = -1;

            for (int it = _loot.Count; it != 0; )
            {
                --it;

                float distance = Utils.CalcDist(heroPos, _loot[it].Pos);
                if (distance < SharedResources.Eset!.Misc.InteractRange && distance < bestDistance)
                {
                    bestDistance = distance;
                    nearest = it;
                }
            }

            if (nearest != -1 && !_loot[nearest].Stack.Empty())
            {
                lootStack = _loot[nearest].Stack;
                _loot[nearest].Dispose();
                _loot.RemoveAt(nearest);
                return lootStack;
            }

            return lootStack;
        }

        public void AddRenders(List<Renderable> ren, List<Renderable> renDead)
        {
            for (int it = 0; it < _loot.Count; ++it)
            {
                if (SharedGameResources.Mapr != null && SharedGameResources.Mapr.Collider.IsOutsideMap(_loot[it].Pos.X, _loot[it].Pos.Y))
                    continue;

                if (SharedGameResources.Mapr!.Fogofwar > FogOfWar.TypeMinimap)
                {
                    float delta = Utils.CalcDist(SharedGameResources.Pc!.Stats.Pos, _loot[it].Pos);
                    if (delta > SharedGameResources.Fow!.MaskRadius - 1.0f)
                    {
                        continue;
                    }
                }

                if (_loot[it].Animation != null)
                {
                    Renderable r = _loot[it].Animation!.GetCurrentFrame(0);
                    r.MapPos.X = _loot[it].Pos.X;
                    r.MapPos.Y = _loot[it].Pos.Y;

                    if (_loot[it].Animation!.IsLastFrame())
                        renDead.Add(r);
                    else
                        ren.Add(r);
                }
            }
        }

        public void ParseLoot(ref string val, EventComponent? e, List<EventComponent>? ecList)
        {
            if (e == null) return;

            string chance;
            bool firstIsFilename = false;
            int lootEcType = e.Data[LootEcType].Int;

            e.S = Parse.PopFirstString(ref val);

            if (e.S == "currency")
                e.Id = SharedResources.Eset!.Misc.CurrencyId;
            else if (Parse.ToInt(e.S, -1) != -1)
                e.Id = SharedGameResources.Items!.VerifyID(Parse.ToItemID(e.S), null, !ItemManager.VerifyAllowZero, !ItemManager.VerifyAllocate);
            else if (ecList != null)
            {
                e.Data[LootEcType].Int = LootEcTypeTable;

                GetLootTable(e.S, ecList);
                firstIsFilename = true;
            }

            if (!firstIsFilename)
            {
                e.Type = EventComponent.Loot;

                chance = Parse.PopFirstString(ref val);
                if (chance == "fixed")
                    e.Data[LootEcChance].Float = 0;
                else
                    e.Data[LootEcChance].Float = Parse.ToFloat(chance);

                e.Data[LootEcQuantityMin].Int = Math.Max(Parse.PopFirstInt(ref val), 1);
                e.Data[LootEcQuantityMax].Int = Math.Max(Parse.PopFirstInt(ref val), e.Data[LootEcQuantityMin].Int);

                if (SharedGameResources.Items!.IsValid(e.Id))
                    e.Data[LootEcMaxDrops].Int = SharedGameResources.Items.Items[e.Id]!.LootDropsMax;
            }

            if (ecList != null)
            {
                string repeatVal = Parse.PopFirstString(ref val);
                while (repeatVal != "")
                {
                    ecList.Add(new EventComponent());
                    EventComponent ec = ecList[ecList.Count - 1];
                    ec.Type = EventComponent.Loot;
                    ec.Data[LootEcType].Int = lootEcType;

                    ec.S = repeatVal;
                    if (ec.S == "currency")
                        ec.Id = SharedResources.Eset!.Misc.CurrencyId;
                    else if (Parse.ToInt(ec.S, -1) != -1)
                        ec.Id = SharedGameResources.Items!.VerifyID(Parse.ToItemID(ec.S), null, !ItemManager.VerifyAllowZero, !ItemManager.VerifyAllocate);
                    else
                    {
                        ec.Data[LootEcType].Int = LootEcTypeTable;

                        GetLootTable(repeatVal, ecList);

                        repeatVal = Parse.PopFirstString(ref val);
                        continue;
                    }

                    chance = Parse.PopFirstString(ref val);
                    if (chance == "fixed")
                        ec.Data[LootEcChance].Float = 0;
                    else
                        ec.Data[LootEcChance].Float = Parse.ToFloat(chance);

                    ec.Data[LootEcQuantityMin].Int = Math.Max(Parse.PopFirstInt(ref val), 1);
                    ec.Data[LootEcQuantityMax].Int = Math.Max(Parse.PopFirstInt(ref val), ec.Data[LootEcQuantityMin].Int);

                    if (SharedGameResources.Items!.IsValid(ec.Id))
                        ec.Data[LootEcMaxDrops].Int = SharedGameResources.Items.Items[ec.Id]!.LootDropsMax;

                    repeatVal = Parse.PopFirstString(ref val);
                }
            }
        }

        private void LoadLootTables()
        {
            List<string> filenames = SharedResources.Mods!.List("loot", !ModManager.ListFullPaths);

            for (int i = 0; i < filenames.Count; i++)
            {
                FileParser infile = new FileParser();
                if (!infile.Open(filenames[i], FileParser.ModFile, FileParser.ErrorNormal))
                    continue;

                if (!_lootTables.ContainsKey(filenames[i]))
                    _lootTables[filenames[i]] = new List<EventComponent>();
                List<EventComponent> ecList = _lootTables[filenames[i]];
                EventComponent? ec = null;
                bool skipToNext = false;

                while (infile.Next())
                {
                    if (infile.Section == "")
                    {
                        if (infile.Key == "loot")
                        {
                            ecList.Add(new EventComponent());
                            ec = ecList[ecList.Count - 1];
                            ec.Data[LootEcType].Int = LootEcTypeTableRow;
                            string lootVal = infile.Val;
                            ParseLoot(ref lootVal, ec, ecList);
                        }
                        else if (infile.Key == "status_loot")
                        {
                            ecList.Add(new EventComponent());
                            ec = ecList[ecList.Count - 1];
                            ec.Data[LootEcType].Int = LootEcTypeTableRow;
                            string statusLootVal = infile.Val;
                            ec.Status = SharedGameResources.Camp!.RegisterStatus(Parse.PopFirstString(ref statusLootVal));
                            ParseLoot(ref statusLootVal, ec, ecList);
                        }
                    }
                    else if (infile.Section == "loot")
                    {
                        if (infile.NewSection)
                        {
                            ecList.Add(new EventComponent());
                            ec = ecList[ecList.Count - 1];
                            ec.Type = EventComponent.Loot;
                            ec.Data[LootEcType].Int = LootEcTypeTableRow;
                            skipToNext = false;
                        }

                        if (skipToNext || ec == null)
                            continue;

                        if (infile.Key == "id")
                        {
                            ec.S = infile.Val;

                            if (ec.S == "currency")
                                ec.Id = SharedResources.Eset!.Misc.CurrencyId;
                            else if (Parse.ToInt(ec.S, -1) != -1)
                                ec.Id = SharedGameResources.Items!.VerifyID(Parse.ToItemID(ec.S), null, !ItemManager.VerifyAllowZero, !ItemManager.VerifyAllocate);
                            else
                            {
                                skipToNext = true;
                                infile.Error("LootManager: Invalid item id for loot.");
                            }

                            if (!skipToNext && SharedGameResources.Items!.IsValid(ec.Id))
                            {
                                ec.Data[LootEcMaxDrops].Int = SharedGameResources.Items.Items[ec.Id]!.LootDropsMax;
                            }
                        }
                        else if (infile.Key == "chance")
                        {
                            if (infile.Val == "fixed")
                                ec.Data[LootEcChance].Float = 0;
                            else
                                ec.Data[LootEcChance].Float = Parse.ToFloat(infile.Val);
                        }
                        else if (infile.Key == "quantity")
                        {
                            string quantityVal = infile.Val;
                            ec.Data[LootEcQuantityMin].Int = Math.Max(Parse.PopFirstInt(ref quantityVal), 1);
                            ec.Data[LootEcQuantityMax].Int = Math.Max(Parse.PopFirstInt(ref quantityVal), ec.Data[LootEcQuantityMin].Int);
                        }
                        else if (infile.Key == "requires_status")
                        {
                            string requiresStatusVal = infile.Val;
                            ec.Status = SharedGameResources.Camp!.RegisterStatus(Parse.PopFirstString(ref requiresStatusVal));
                        }
                        else if (infile.Key == "requires_level")
                        {
                            string requiresLevelVal = infile.Val;
                            ec.Data[LootEcRequiresLevelMin].Int = Math.Max(Parse.PopFirstInt(ref requiresLevelVal), 0);
                            ec.Data[LootEcRequiresLevelMax].Int = Math.Max(Parse.PopFirstInt(ref requiresLevelVal), ec.Data[LootEcRequiresLevelMin].Int);
                        }
                        else if (infile.Key == "quantity_per_level")
                        {
                            string quantityPerLevelVal = infile.Val;
                            ec.Data[LootEcQuantityPerLevelMin].Int = Math.Max(Parse.PopFirstInt(ref quantityPerLevelVal), 0);
                            ec.Data[LootEcQuantityPerLevelMax].Int = Math.Max(Parse.PopFirstInt(ref quantityPerLevelVal), ec.Data[LootEcQuantityPerLevelMin].Int);
                        }
                    }
                }

                infile.Close();
            }
        }

        private void GetLootTable(string filename, List<EventComponent> ecList)
        {
            foreach (KeyValuePair<string, List<EventComponent>> entry in _lootTables)
            {
                if (entry.Key == Filesystem.ConvertSlashes(filename))
                {
                    List<EventComponent> lootDefs = entry.Value;
                    for (int i = 0; i < lootDefs.Count; ++i)
                    {
                        ecList.Add(new EventComponent(lootDefs[i]));
                    }
                    break;
                }
            }
        }

        private void CheckLootComponent(EventComponent ec, Vector2? pos, List<ItemStack>? itemstackVec)
        {
            Vector2 p;
            ItemStack newLoot = new ItemStack();
            Int2 src;

            if (pos != null)
            {
                src = pos.Value.ToInt2();
            }
            else
            {
                src.X = ec.Data[LootEcPosX].Int;
                src.Y = ec.Data[LootEcPosY].Int;
            }
            p.X = (float)src.X + 0.5f;
            p.Y = (float)src.Y + 0.5f;

            if (!SharedGameResources.Mapr!.Collider.IsValidPosition(p.X, p.Y, MapCollision.MoveNormal, MapCollision.CollideTypeAllEntities))
            {
                p = SharedGameResources.Mapr.Collider.GetRandomNeighbor(src, SharedResources.Eset!.Loot.DropRadius, MapCollision.MoveNormal, MapCollision.CollideTypeAllEntities);

                if (!SharedGameResources.Mapr.Collider.IsValidPosition(p.X, p.Y, MapCollision.MoveNormal, MapCollision.CollideTypeAllEntities))
                {
                    p = SharedGameResources.Pc!.Stats.Pos;
                }
                else
                {
                    if (src.X == (int)p.X && src.Y == (int)p.Y)
                        p = SharedGameResources.Pc!.Stats.Pos;

                    SharedGameResources.Mapr.Collider.Block(p.X, p.Y, !MapCollision.IsAlly);
                    _tilesToUnblock.Add(p.ToInt2());
                }
            }

            List<ItemStack> exStacks = new List<ItemStack>();

            int quantityMin = ec.Data[LootEcQuantityMin].Int + ((SharedGameResources.Pc!.Stats.Level - 1) * ec.Data[LootEcQuantityPerLevelMin].Int);
            int quantityMax = ec.Data[LootEcQuantityMax].Int + ((SharedGameResources.Pc!.Stats.Level - 1) * ec.Data[LootEcQuantityPerLevelMax].Int);

            newLoot.Quantity = MathUtils.RandBetween(quantityMin, quantityMax);

            if (ec.Id == 0 || ec.Id == SharedResources.Eset!.Misc.CurrencyId)
            {
                newLoot.Item = SharedResources.Eset!.Misc.CurrencyId;
                newLoot.Quantity = (int)((float)newLoot.Quantity * (100 + SharedGameResources.Pc!.Stats.Get(Stats.CurrencyFind)) / 100);
                exStacks.Add(newLoot);
            }
            else
            {
                newLoot.Item = ec.Id;
                SharedGameResources.Items!.GetExtendedStacks(newLoot.Item, (uint)newLoot.Quantity, exStacks);
            }

            for (int i = 0; i < exStacks.Count; ++i)
            {
                if (itemstackVec != null)
                    itemstackVec.Add(exStacks[i]);
                else
                    AddLoot(exStacks[i], p, !DroppedByHero);
            }
        }

        public void RemoveFromEnemiesDroppingLoot(StatBlock sb)
        {
            for (int i = _enemiesDroppingLoot.Count; i > 0; i--)
            {
                if (_enemiesDroppingLoot[i - 1] == sb)
                    _enemiesDroppingLoot[i - 1] = null;
            }
        }

        /// <summary>对应 C++ 析构函数 <c>~LootManager()</c>。</summary>
        public void Dispose()
        {
            Utils.LogInfo("Cleaning up: LootManager");

            for (int i = 0; i < _animations.Count; ++i)
            {
                if (_animations[i] == null)
                    continue;

                if (SharedGameResources.Items!.IsValid(i))
                {
                    for (int j = 0; j < SharedGameResources.Items.Items[i]!.LootAnimation.Count; ++j)
                    {
                        SharedResources.Anim!.DecreaseCount(SharedGameResources.Items.Items[i]!.LootAnimation[j].Name);
                        if (_animations[i]![j] != null)
                            _animations[i]![j]!.Dispose();
                    }
                }
                _animations[i] = null;
            }

            for (int i = 0; i < _loot.Count; ++i)
                _loot[i].Dispose();
            _loot.Clear();

            SharedResources.Anim!.CleanUp();

            SharedResources.Snd!.Unload(_sfxLoot);

            GC.SuppressFinalize(this);
        }
    }
}
