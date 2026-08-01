// 对应 C++ 源：EntityManager.h + EntityManager.cpp
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// EntityManager
    ///
    /// 管理地图上除玩家角色（Avatar）以外的所有 Entity（敌人/友方 NPC/召唤物等）：
    /// 加载并缓存"实体原型"（Prototypes，按 type_id 去重加载一次动画/音效等静态数据）、
    /// 处理换图时的批量生成/清理、处理法术产生的召唤生成、驱动每个 Entity 的逐帧逻辑
    /// 以及收集渲染用的 Renderable 列表。
    ///
    /// C++ 原始版本中 <c>entities</c> 持有的是裸指针（<c>std::vector&lt;Entity*&gt;</c>），
    /// 由 EntityManager 独占所有权并在换图/析构时手动 <c>delete</c>；<c>prototypes</c> 则是
    /// 按值持有的 <c>std::vector&lt;Entity&gt;</c>，其中每个元素随 <c>clear()</c>/析构自动被
    /// 就地析构。C# 版本中 <see cref="Entity"/> 假定为引用类型（class），因此本单元在原始
    /// <c>delete</c> 调用点改为显式调用 <c>Dispose()</c>；对于 <see cref="Prototypes"/>
    /// 这种在 C++ 中"随容器操作隐式析构"的值语义，本单元在对应的 <c>clear()</c> 位置额外
    /// 补充了显式 <c>Dispose()</c> 调用以保持相同的资源释放时机（详见报告"关键转换决策"）。
    /// EntityManager 自身实现 <see cref="IDisposable"/>，对应原始析构函数，释放顺序与原始
    /// 代码逐行一致。
    /// </summary>
    public class EntityManager : IDisposable
    {
        /// <summary>
        /// 对应 C++ 的 <c>size_t loadEntityPrototype(const std::string&amp;)</c>：
        /// 若 <paramref name="typeId"/> 对应的原型已缓存，直接返回其索引；否则加载一份新的
        /// 原型（动画/音效/AI 可能触发的连锁生成类型）并追加到 <see cref="Prototypes"/> 末尾，
        /// 返回新原型的索引。调用方负责通过该索引取用原型（对应原始注释：
        /// callee is responsible for deleting returned entity object，见
        /// <see cref="GetEntityPrototype"/>）。
        /// </summary>
        protected int LoadEntityPrototype(string typeId)
        {
            for (int i = 0; i < Prototypes.Count; i++)
            {
                if (Prototypes[i].TypeFilename == typeId)
                {
                    return i;
                }
            }

            Entity e = new Entity();

            e.Stats.Load(typeId);
            e.TypeFilename = typeId;

            if (e.Stats.Animations == "")
                Utils.LogError("EntityManager: No animation file specified for entity: %s", typeId);

            e.LoadAnimations();
            e.LoadSounds();

            // set cooldown_hit to duration of hit animation if undefined
            if (!e.Stats.CooldownHitEnabled && e.AnimationSet != null)
            {
                Animation? hitAnim = e.AnimationSet.GetAnimation("hit");
                if (hitAnim != null)
                {
                    e.Stats.CooldownHit.Duration = (uint)hitAnim.GetDuration();
                    hitAnim.Dispose();
                }
                else
                {
                    e.Stats.CooldownHit.Duration = 0;
                }
            }

            Prototypes.Add(e);
            int prototype = Prototypes.Count - 1;

            for (int i = 0; i < e.Stats.PowersAi.Count; i++)
            {
                PowerID powerIndex = e.Stats.PowersAi[i].Id;
                if (SharedGameResources.Powers!.IsValid(powerIndex))
                {
                    string spawnType = SharedGameResources.Powers!.Powers[powerIndex].SpawnType;
                    if (spawnType != "" && spawnType != "untransform")
                    {
                        List<EnemyLevel> spawnEnemies = SharedGameResources.Enemyg!.GetEnemiesInCategory(spawnType);
                        for (int j = 0; j < spawnEnemies.Count; j++)
                        {
                            LoadEntityPrototype(spawnEnemies[j].Type);
                        }
                    }
                }
            }

            return prototype;
        }

        /// <summary>callee is responsible for deleting returned entity object（调用方负责释放返回的 Entity 对象）。</summary>
        protected List<Entity> Prototypes = new List<Entity>();

        public EntityManager()
        {
            Entities = new List<Entity>();
            HeroStealth = 0;
            PlayerBlocked = false;
            PlayerBlockedTimer = new Timer((uint)(SharedResources.Settings!.MaxFramesPerSec / 6));

            HandleNewMap();
        }

        /// <summary>
        /// 对应 C++ 的 <c>~EntityManager()</c>：跳过 npc 实体（不属于本管理器所有权），
        /// 其余实体依次 <c>UnloadSounds()</c> + <c>Dispose()</c>，最后释放所有原型。
        /// </summary>
        public void Dispose()
        {
            Utils.LogInfo("Cleaning up: EntityManager");

            for (int i = 0; i < Entities.Count; i++)
            {
                if (Entities[i].Stats.Npc)
                    continue;

                Entities[i].UnloadSounds();
                Entities[i].Dispose();
            }
            for (int i = 0; i < Prototypes.Count; i++)
            {
                Prototypes[i].UnloadSounds();
            }

            GC.SuppressFinalize(this);
        }

        public Entity GetEntityPrototype(string typeId)
        {
            Entity e = new Entity(Prototypes[LoadEntityPrototype(typeId)]);
            return e;
        }

        /// <summary>
        /// When loading a new map, we eliminate existing entities and load the new ones.
        /// The map will have loaded Entity blocks into an array; retrieve the entities and init them
        /// </summary>
        public void HandleNewMap()
        {
            MapEnemy me;
            Queue<Entity> allies = new Queue<Entity>();

            var mapr = SharedGameResources.Mapr!;
            var pc = SharedGameResources.Pc!;
            var powers = SharedGameResources.Powers!;
            var enemyg = SharedGameResources.Enemyg!;

            // delete existing entities
            for (int i = 0; i < Entities.Count; i++)
            {
                if (Entities[i].Stats.Npc)
                    continue;

                if (Entities[i].Stats.HeroAlly && Entities[i].Stats.Alive && Entities[i].Stats.Speed > 0)
                    allies.Enqueue(Entities[i]);
                else
                {
                    Entities[i].UnloadSounds();
                    Entities[i].Dispose();
                }
            }
            Entities.Clear();

            for (int i = 0; i < Prototypes.Count; i++)
            {
                Prototypes[i].UnloadSounds();
                // 对应 C++ prototypes.clear() 隐式析构每个按值持有的 Entity 元素；
                // Prototypes 在 C# 中为引用类型列表，需显式 Dispose 以保持同一时机释放。
                Prototypes[i].Dispose();
            }
            Prototypes.Clear();

            // load new entities
            while (mapr.Enemies.Count > 0)
            {
                me = mapr.Enemies.Dequeue();

                if (me.Type == "")
                {
                    Utils.LogError("EntityManager: Entity(%f, %f) doesn't have type attribute set, skipping", me.Pos.X, me.Pos.Y);
                    continue;
                }

                if (!SharedGameResources.Camp!.CheckRequirementsInVector(me.Requirements))
                    continue;

                Entity e = GetEntityPrototype(me.Type);

                e.Stats.Waypoints = new Queue<Vector2>(me.Waypoints);
                e.Stats.Pos.X = me.Pos.X;
                e.Stats.Pos.Y = me.Pos.Y;
                e.Stats.Direction = (byte)me.Direction;
                e.Stats.Wander = me.WanderRadius > 0;
                e.Stats.SetWanderArea(me.WanderRadius);
                e.Stats.InvincibleRequirements = new List<EventComponent>(me.InvincibleRequirements);

                // Set level
                if (pc != null)
                {
                    me.SpawnLevel.ApplyToStatBlock(e.Stats, pc.Stats);
                }

                // apply Effects and set HP to max HP
                e.Stats.Recalc();

                Entities.Add(e);

                mapr.Collider.Block(me.Pos.X, me.Pos.Y, !MapCollision.IsAlly);
            }

            // 飞行类敌人跨越坑洞时如何生成的问题，原始实现中尚未处理，本次迁移保留原样行为。
            Vector2 spawnPos = mapr.Collider.GetRandomNeighbor(pc.Stats.Pos.ToInt2(), 1, MapCollision.MoveNormal, MapCollision.CollideTypeAllEntities);
            while (allies.Count > 0)
            {
                Entity e = allies.Dequeue();

                //dont need the result of this. its only called to handle animation and sound
                Entity temp = GetEntityPrototype(e.TypeFilename);
                temp.Dispose();

                e.Stats.Pos = spawnPos;
                e.Stats.Direction = pc.Stats.Direction;

                Entities.Add(e);

                mapr.Collider.Block(e.Stats.Pos.X, e.Stats.Pos.Y, MapCollision.IsAlly);
            }

            // load entities that can be spawn by avatar's powers
            for (int i = 0; i < pc.Stats.PowersList.Count; i++)
            {
                PowerID powerIndex = pc.Stats.PowersList[i];
                if (powers.IsValid(powerIndex))
                {
                    string spawnType = powers.Powers[powerIndex].SpawnType;
                    if (spawnType != "" && spawnType != "untransform")
                    {
                        List<EnemyLevel> spawnEnemies = enemyg.GetEnemiesInCategory(spawnType);
                        for (int j = 0; j < spawnEnemies.Count; j++)
                        {
                            LoadEntityPrototype(spawnEnemies[j].Type);
                        }
                    }
                }
            }

            // load entities that can be spawn by powers in the action bar
            if (SharedGameResources.MenuAct != null)
            {
                for (int i = 0; i < SharedGameResources.MenuAct!.Hotkeys.Count; i++)
                {
                    PowerID powerIndex = SharedGameResources.MenuAct!.Hotkeys[i];
                    if (powerIndex != 0)
                    {
                        string spawnType = powers.Powers[powerIndex].SpawnType;
                        if (spawnType != "" && spawnType != "untransform")
                        {
                            List<EnemyLevel> spawnEnemies = enemyg.GetEnemiesInCategory(spawnType);
                            for (int j = 0; j < spawnEnemies.Count; j++)
                            {
                                LoadEntityPrototype(spawnEnemies[j].Type);
                            }
                        }
                    }
                }
            }

            // load entities that can be spawn by map events
            for (int i = 0; i < mapr.Events.Count; i++)
            {
                for (int j = 0; j < mapr.Events[i].Components.Count; j++)
                {
                    if (mapr.Events[i].Components[j].Type == EventComponent.Spawn)
                    {
                        List<EnemyLevel> spawnEnemies = enemyg.GetEnemiesInCategory(mapr.Events[i].Components[j].S);
                        for (int k = 0; k < spawnEnemies.Count; k++)
                        {
                            LoadEntityPrototype(spawnEnemies[k].Type);
                        }
                    }
                }
            }

            SharedResources.Anim!.CleanUp();
        }

        /// <summary>
        /// Powers can cause new entities to spawn
        /// Check PowerManager for any new queued entities
        /// </summary>
        public void HandleSpawn()
        {
            MapEnemy espawn;

            var powers = SharedGameResources.Powers!;
            var mapr = SharedGameResources.Mapr!;
            var pc = SharedGameResources.Pc!;
            var enemyg = SharedGameResources.Enemyg!;

            while (powers.MapEnemies.Count > 0)
            {
                espawn = powers.MapEnemies.Dequeue();

                mapr.Collider.Unblock(espawn.Pos.X, espawn.Pos.Y);

                Entity e = new Entity();

                e.Stats.HeroAlly = espawn.HeroAlly;
                e.Stats.EnemyAlly = espawn.EnemyAlly;
                e.Stats.Summoned = true;
                e.Stats.SummonedPowerIndex = espawn.SummonPowerIndex;

                if (espawn.Summoner != null)
                {
                    e.Stats.Summoner = espawn.Summoner;
                    espawn.Summoner.Summons.Add(e.Stats);
                }

                e.Stats.Direction = (byte)espawn.Direction;

                EnemyLevel el = enemyg.GetRandomEnemy(espawn.Type, 0, 0);
                e.TypeFilename = el.Type;

                if (el.Type != "")
                {
                    e.Stats.Load(el.Type);
                }
                else
                {
                    Utils.LogError("EntityManager: Could not spawn creature type '%s'", espawn.Type);
                    e.Dispose();
                    return;
                }

                if (e.Stats.Animations == "")
                {
                    Utils.LogError("EntityManager: No animation file specified for entity: %s", espawn.Type);
                }
                e.LoadAnimations();
                e.LoadSounds();

                //Set level
                if (powers.IsValid(e.Stats.SummonedPowerIndex))
                {
                    SpawnLevel spawnLevel = powers.Powers[e.Stats.SummonedPowerIndex].SpawnLevel;
                    spawnLevel.ApplyToStatBlock(e.Stats, e.Stats.Summoner);

                    // apply Effects and set HP to max HP
                    e.Stats.Recalc();
                }
                else if (espawn.SpawnLevel.Mode != SpawnLevel.ModeDefault)
                {
                    espawn.SpawnLevel.ApplyToStatBlock(e.Stats, null);
                    e.Stats.Recalc();
                }

                if (mapr.Collider.IsValidPosition(espawn.Pos.X, espawn.Pos.Y, e.Stats.MovementType, MapCollision.CollideTypeAllEntities) || !e.Stats.HeroAlly)
                {
                    e.Stats.Pos.X = espawn.Pos.X;
                    e.Stats.Pos.Y = espawn.Pos.Y;
                }
                else
                {
                    e.Stats.Pos = mapr.Collider.GetRandomNeighbor(pc.Stats.Pos.ToInt2(), 1, e.Stats.MovementType, MapCollision.CollideTypeAllEntities);
                }

                // special animation state for spawning entities
                e.Stats.CurState = StatBlock.EntitySpawn;

                //now apply post effects to the spawned entity
                powers.Effect(e.Stats, (espawn.Summoner != null ? espawn.Summoner : e.Stats), e.Stats.SummonedPowerIndex, e.Stats.HeroAlly ? Power.SourceTypeHero : Power.SourceTypeEnemy);

                //apply party passives
                //synchronise tha party passives in the pc stat block with the passives in the allies stat blocks
                //at the time the summon is spawned, it takes the passives available at that time. if the passives change later, the changes wont affect summons retrospectively. could be exploited with equipment switching
                for (int i = 0; i < pc.Stats.PowersPassive.Count; i++)
                {
                    PowerID pwr = pc.Stats.PowersPassive[i];
                    if (powers.IsValid(pwr) && powers.Powers[pwr].Passive && powers.Powers[pwr].BuffParty && (e.Stats.HeroAlly || e.Stats.EnemyAlly)
                            && (powers.Powers[pwr].BuffPartyPowerId == 0 || powers.Powers[pwr].BuffPartyPowerId == e.Stats.SummonedPowerIndex))
                    {

                        e.Stats.PowersPassive.Add(pwr);
                    }
                }

                for (int i = 0; i < pc.Stats.PowersListItems.Count; i++)
                {
                    PowerID pwr = pc.Stats.PowersListItems[i];
                    if (powers.IsValid(pwr) && powers.Powers[pwr].Passive && powers.Powers[pwr].BuffParty && (e.Stats.HeroAlly || e.Stats.EnemyAlly)
                            && (powers.Powers[pwr].BuffPartyPowerId == 0 || powers.Powers[pwr].BuffPartyPowerId == e.Stats.SummonedPowerIndex))
                    {

                        e.Stats.PowersPassive.Add(pwr);
                    }
                }

                Entities.Add(e);

                mapr.Collider.Block(e.Stats.Pos.X, e.Stats.Pos.Y, e.Stats.HeroAlly);
            }
        }

        public bool CheckPartyMembers()
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                if (Entities[i].Stats.HeroAlly && Entities[i].Stats.Hp > 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// perform logic() for all entities
        /// </summary>
        public void Logic()
        {
            if (PlayerBlocked)
            {
                PlayerBlockedTimer.Tick();
                if (PlayerBlockedTimer.IsEnd())
                    PlayerBlocked = false;
            }

            HandleSpawn();

            bool pcInCombat = false;

            for (int it = 0; it < Entities.Count; ++it)
            {
                // new actions this round
                Entities[it].Stats.HeroStealth = HeroStealth;
                if (!Entities[it].Stats.Npc)
                {
                    Entities[it].Logic();

                    if (!pcInCombat && Entities[it].Stats.Alive && !Entities[it].Stats.HeroAlly && Entities[it].Stats.InCombat)
                        pcInCombat = true;
                }
            }

            var inpt = SharedResources.Inpt!;
            var pc = SharedGameResources.Pc!;
            if (pcInCombat && !pc.Stats.InCombat)
            {
                // if a supported controller is connected, change the LED to red when in combat
                Color ledColor = new Color(255, 0, 0, 255);
                inpt.SetJoystickLED(ledColor);
                pc.Stats.InCombat = true;
            }
            else if (!pcInCombat && pc.Stats.InCombat)
            {
                inpt.SetJoystickLED(InputState.DefaultControllerLedColor);
                pc.Stats.InCombat = false;
            }
        }

        /// <summary>
        /// addRenders()
        /// Map objects need to be drawn in Z order, so we allow a parent object (GameEngine)
        /// to collect all mobile sprites each frame.
        /// </summary>
        public void AddRenders(List<Renderable> r, List<Renderable> rDead)
        {
            for (int it = 0; it < Entities.Count; ++it)
            {
                if (SharedGameResources.Mapr!.Fogofwar > FogOfWar.TypeMinimap)
                {
                    float delta = Utils.CalcDist(SharedGameResources.Pc!.Stats.Pos, Entities[it].Stats.Pos);
                    if (delta > SharedGameResources.Fow!.MaskRadius - 1.0)
                    {
                        continue;
                    }
                }

                bool dead = Entities[it].Stats.Corpse;
                if (!dead || !Entities[it].Stats.CorpseTimer.IsEnd() || !Entities[it].Stats.CorpseHasTimeout)
                {
                    if (dead && Entities[it].Stats.CorpseRenderBelow)
                        Entities[it].AddRenders(rDead);
                    else
                        Entities[it].AddRenders(r);
                }
            }
        }

        // 注：头文件声明了 void checkEnemiesforXP()，但整个 src/ 目录（含本单元的
        // EntityManager.cpp）均没有该方法的函数体定义，也没有任何调用点，是原始代码库
        // 中的遗留/未使用声明。由于没有任何源逻辑可供迁移，本次转换有意不生成该方法，
        // 避免编造未在源码中出现的行为（详见 EntityManager.report.txt"关键转换决策"
        // 第 8 条与"潜在风险"第 3 条）。

        public bool IsCleared()
        {
            if (Entities.Count == 0) return true;

            for (int i = 0; i < Entities.Count; i++)
            {
                if (Entities[i].Stats.Alive && !Entities[i].Stats.HeroAlly)
                    return false;
            }

            return true;
        }

        public void Spawn(string entityType, Int2 target, EventComponent? ecSpawnLevel)
        {
            MapEnemy espawn = new MapEnemy();

            espawn.Type = entityType;
            espawn.Pos = target.ToVector2();
            espawn.Pos.X += 0.5f;
            espawn.Pos.Y += 0.5f;

            // quick spawns start facing a random direction
            espawn.Direction = Program.Rng.Next() % 8;

            if (!SharedGameResources.Mapr!.Collider.IsValidPosition(espawn.Pos.X, espawn.Pos.Y, MapCollision.MoveNormal, MapCollision.CollideTypeNone))
            {
                return;
            }
            else
            {
                SharedGameResources.Mapr!.Collider.Block(espawn.Pos.X, espawn.Pos.Y, !MapCollision.IsAlly);
            }

            if (ecSpawnLevel != null)
            {
                espawn.SpawnLevel.ParseString(ecSpawnLevel.S);
            }

            SharedGameResources.Powers!.MapEnemies.Enqueue(espawn);
        }

        public Entity? EntityFocus(Int2 mouse, Vector2 cam, bool aliveOnly)
        {
            Entity? nearest = null;
            float bestDistance = float.MaxValue;
            Vector2 mousef = mouse.ToVector2();
            Vector2 renderBoundsCenter = default;

            for (int i = 0; i < Entities.Count; i++)
            {
                if (Entities[i].Stats.CurState == StatBlock.EntityDead || Entities[i].Stats.CurState == StatBlock.EntityCritdead)
                {
                    if (aliveOnly)
                        continue;
                    else if (Entities[i].Stats.Corpse && Entities[i].Stats.CorpseTimer.IsEnd() && Entities[i].Stats.CorpseHasTimeout)
                        continue;
                }

                Rectangle renderBounds = Entities[i].GetRenderBounds(cam);
                if (Utils.IsWithinRect(renderBounds, mouse))
                {
                    renderBoundsCenter.X = (float)renderBounds.X + ((float)renderBounds.Width / 2);
                    renderBoundsCenter.Y = (float)renderBounds.Y + ((float)renderBounds.Height / 2);
                    float distance = Utils.CalcDist(mousef, renderBoundsCenter);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        nearest = Entities[i];
                    }
                }
            }
            return nearest;
        }

        /// <summary>
        /// 对应 C++ 形参 <c>float *saved_distance</c>：<c>null</c> 表示原始的空指针（不写回距离），
        /// 非 <c>null</c> 表示存在有效指针，函数会写回 <paramref name="savedDistance"/>。
        /// </summary>
        public Entity? GetNearestEntity(Vector2 pos, bool getCorpse, ref float? savedDistance, float maxRange)
        {
            Entity? nearest = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < Entities.Count; i++)
            {
                if (!getCorpse && (Entities[i].Stats.CurState == StatBlock.EntityDead || Entities[i].Stats.CurState == StatBlock.EntityCritdead))
                {
                    continue;
                }
                if (getCorpse && !Entities[i].Stats.Corpse)
                {
                    continue;
                }

                float distance = Utils.CalcDist(pos, Entities[i].Stats.Pos);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    nearest = Entities[i];
                }
            }

            if (nearest != null && savedDistance != null)
                savedDistance = bestDistance;

            if (savedDistance == null && bestDistance > maxRange)
                nearest = null;

            return nearest;
        }

        // vars
        public List<Entity> Entities;
        public float HeroStealth;

        public bool PlayerBlocked;
        public Timer PlayerBlockedTimer;

        public const bool GetCorpse = true;
        public const bool IsAlive = true;
    }
}
