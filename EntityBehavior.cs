// 对应 C++ 源：EntityBehavior.h + EntityBehavior.cpp
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// EntityBehavior
    ///
    /// 实体行为组件：为 Entity 提供 AI 决策（移动、施法等）�?
    /// 对应原始 <c>class EntityBehavior</c>；由 <see cref="Entity.Behavior"/> 持有并在
    /// <see cref="Entity.Logic"/> 中逐帧调用 <see cref="Logic"/>�?
    ///
    /// 前向引用（详�?EntityBehavior.report.txt）：<see cref="Avatar"/>�?see cref="NPC"/>�?
    /// <see cref="Power"/>�?see cref="PowerManager"/>�?see cref="ChainPower"/>�?
    /// <see cref="MapRenderer"/> 等尚未转换单元，按仓库既有约定前向引用�?
    /// </summary>
    public class EntityBehavior : IDisposable
    {
        private static readonly float AllyFleeDistance = 2;
        private static readonly float AllyFollowDistanceWalk = 5.5f;
        private static readonly float AllyFollowDistanceStop = 5;
        private static readonly float AllyTeleportDistance = 40;

        protected Entity E;

        protected const int PathFoundFailThreshold = 1;
        protected const int PathFoundFailWaitSeconds = 2;

        protected Vector2 PrevTarget;
        protected bool Collided;
        protected bool PathFound;
        protected int ChanceCalcPath;
        protected int PathFoundFails;
        protected Timer PathFoundFailTimer = new Timer();
        protected bool WarpToHero;

        protected float TargetDist;
        protected float HeroDist;
        public List<Vector2> Path { get; private set; } = new List<Vector2>();
        public Vector2 PursuePos { get; set; }

        protected bool Los;
        protected bool Fleeing;
        protected bool MoveToSafeDist;
        protected Timer TurnTimer = new Timer();

        protected bool InstantPower;
        protected PowerID ReplacedPowerId;

        public EntityBehavior(Entity e)
        {
            E = e;
            Path = new List<Vector2>();
            PrevTarget = default;
            Collided = false;
            PathFound = false;
            ChanceCalcPath = 0;
            PathFoundFails = 0;
            PathFoundFailTimer = new Timer();
            WarpToHero = false;
            TargetDist = 0;
            HeroDist = 0;
            PursuePos = new Vector2(-1, -1);
            Los = false;
            Fleeing = false;
            MoveToSafeDist = false;
            TurnTimer = new Timer();
            InstantPower = false;
            ReplacedPowerId = 0;

            PathFoundFailTimer.Duration = (uint)(SharedResources.Settings!.MaxFramesPerSec * PathFoundFailWaitSeconds);
            PathFoundFailTimer.Reset(Timer.End);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public void Logic()
        {
            var eset = SharedResources.Eset!;
            var pc = SharedGameResources.Pc!;
            var settings = SharedResources.Settings!;

            if (E.Stats.Corpse)
            {
                if (eset.Misc.CorpseTimeoutEnabled)
                    E.Stats.CorpseTimer.Tick();

                return;
            }

            if (!E.Stats.HeroAlly)
            {
                if (Utils.CalcDist(E.Stats.Pos, pc.Stats.Pos) <= settings.EncounterDist)
                    E.Stats.Encountered = true;

                if (!E.Stats.Encountered)
                    return;
            }

            DoUpkeep();
            FindTarget();
            CheckPower();
            CheckMove();
            UpdateState();

            Fleeing = false;
        }

        private void DoUpkeep()
        {
            var powers = SharedGameResources.Powers!;
            var mapr = SharedGameResources.Mapr!;

            if (E.Stats.Hp > 0 || E.Stats.Effects.TriggeredDeath)
                powers.ActivatePassives(E.Stats);

            E.Stats.Logic();

            if (E.Stats.Teleportation)
            {
                mapr.Collider.Unblock(E.Stats.Pos.X, E.Stats.Pos.Y);

                E.Stats.Pos.X = E.Stats.TeleportDestination.X;
                E.Stats.Pos.Y = E.Stats.TeleportDestination.Y;

                mapr.Collider.Block(E.Stats.Pos.X, E.Stats.Pos.Y, E.Stats.HeroAlly);

                E.Stats.Teleportation = false;
            }
        }

        private void FindTarget()
        {
            var pc = SharedGameResources.Pc!;
            var mapr = SharedGameResources.Mapr!;
            var entitym = SharedGameResources.Entitym!;
            var powers = SharedGameResources.Powers!;
            var eset = SharedResources.Eset!;

            if (E.Stats.CurState == StatBlock.EntityDead || E.Stats.CurState == StatBlock.EntityCritdead)
                return;

            if (E.Stats.Npc && !E.Stats.HeroAlly && !E.Stats.Wander && E.Stats.Waypoints.Count == 0)
                return;

            if (E.Stats.Effects.Stun)
                return;

            if (E.Stats.Npc && SharedGameResources.Menu != null && SharedGameResources.Menu.Talker != null && SharedGameResources.Menu.Talker.Visible && SharedGameResources.Menu.Talker.Npc == (NPC)E)
                return;

            StatBlock? targetStats = null;
            float stealthThreatRange = (E.Stats.ThreatRange * (100 - (float)E.Stats.HeroStealth)) / 100;

            if (pc.Stats.Alive)
            {
                TargetDist = Utils.CalcDist(E.Stats.Pos, pc.Stats.Pos);
                targetStats = pc.Stats;
            }
            else
            {
                TargetDist = 0;
            }
            HeroDist = TargetDist;

            if (E.Stats.HeroAlly && E.Stats.Speed > 0 && (WarpToHero || HeroDist > AllyTeleportDistance) && !E.Stats.InCombat)
            {
                mapr.Collider.Unblock(E.Stats.Pos.X, E.Stats.Pos.Y);
                E.Stats.Pos.X = pc.Stats.Pos.X;
                E.Stats.Pos.Y = pc.Stats.Pos.Y;
                mapr.Collider.Block(E.Stats.Pos.X, E.Stats.Pos.Y, MapCollision.IsAlly);
                HeroDist = 0;
                WarpToHero = false;
            }

            for (int i = 0; i < entitym.Entities.Count; ++i)
            {
                Entity entity = entitym.Entities[i];
                if (!entity.Stats.Alive)
                    continue;

                if ((!E.Stats.HeroAlly && entity.Stats.HeroAlly) || (E.Stats.HeroAlly && !entity.Stats.HeroAlly && entity.Stats.InCombat))
                {
                    float entityDist = Utils.CalcDist(E.Stats.Pos, entity.Stats.Pos);
                    if (targetStats == null || (E.Stats.HeroAlly && targetStats.Hero))
                    {
                        targetStats = entitym.Entities[i].Stats;
                        TargetDist = entityDist;
                        E.Stats.InCombat = true;
                    }
                    else if (entityDist < TargetDist)
                    {
                        targetStats = entitym.Entities[i].Stats;
                        TargetDist = entityDist;
                    }
                }
            }

            if (targetStats != null && TargetDist < E.Stats.ThreatRange && pc.Stats.Alive)
                Los = mapr.Collider.LineOfSight(E.Stats.Pos.X, E.Stats.Pos.Y, targetStats.Pos.X, targetStats.Pos.Y);
            else
                Los = false;

            if (Los)
                E.Stats.CooldownLos.Reset(Timer.Begin);

            if (!E.Stats.InCombat && E.Stats.CombatStyle == StatBlock.CombatAggressive)
            {
                E.Stats.JoinCombat = true;
            }

            bool closeToTarget = false;
            if (!E.Stats.HeroAlly && pc.Stats == targetStats)
                closeToTarget = TargetDist < stealthThreatRange;
            else if (targetStats != null && pc.Stats != targetStats)
                closeToTarget = TargetDist < E.Stats.ThreatRange;

            if (E.Stats.Alive && !E.Stats.InCombat && Los && closeToTarget && E.Stats.CombatStyle != StatBlock.CombatPassive)
            {
                E.Stats.JoinCombat = true;
            }

            if (E.Stats.JoinCombat)
            {
                E.Stats.InCombat = true;
                
                // we need to reset the los cooldown here to prevent getting stuck in the join_combat state
                // this happens when the entity doesn't have los, but enters combat due to being hit (by a beacon or otherwise)
                E.Stats.CooldownLos.Reset(Timer.Begin);

                StatBlock.AIPower? aiPower;
                if (!E.Stats.HeroAlly)
                {
                    aiPower = E.Stats.GetAIPower(StatBlock.AiPowerBeacon);
                    if (aiPower != null)
                    {
                        powers.Activate(aiPower.Id, E.Stats, E.Stats.Pos, E.Stats.Pos);
                    }
                }

                aiPower = E.Stats.GetAIPower(StatBlock.AiPowerJoinCombat);
                if (aiPower != null)
                {
                    E.Stats.CurState = StatBlock.EntityPower;
                    E.Stats.ActivatedPower = aiPower;
                    ReplacedPowerId = powers.CheckReplaceByEffect(aiPower.Id, E.Stats);
                }

                E.Stats.JoinCombat = false;
            }

            if (E.Stats.CombatStyle != StatBlock.CombatAggressive)
            {
                if (TargetDist > E.Stats.ThreatRangeFar)
                    E.Stats.InCombat = false;

                if (E.Stats.CooldownLos.Duration > 0 && E.Stats.CooldownLos.IsEnd())
                    E.Stats.InCombat = false;
            }

            if (!E.Stats.Alive || !pc.Stats.Alive || (targetStats != null && !targetStats.Alive))
                E.Stats.InCombat = false;

            if (E.Stats.HeroAlly && targetStats == pc.Stats)
                E.Stats.InCombat = false;

            if (targetStats != null)
                PursuePos = targetStats.Pos;

            if (E.Stats.Wander && E.Stats.Waypoints.Count == 0)
            {
                Vector2 waypoint = GetWanderPoint();
                E.Stats.Waypoints.Enqueue(waypoint);
                E.Stats.WaypointTimer.Reset(Timer.Begin);
            }

            if (!(E.Stats.InCombat || E.Stats.Waypoints.Count == 0))
            {
                Vector2 waypoint = E.Stats.Waypoints.Peek();
                PursuePos = waypoint;
            }

            if (E.Stats.HeroAlly && eset.Misc.EnableAllyCollisionAi)
            {
                if (!entitym.PlayerBlocked && HeroDist < AllyFleeDistance
                        && mapr.Collider.IsFacing(pc.Stats.Pos.X, pc.Stats.Pos.Y, (char)pc.Stats.Direction, E.Stats.Pos.X, E.Stats.Pos.Y))
                {
                    entitym.PlayerBlocked = true;
                    entitym.PlayerBlockedTimer.Reset(Timer.Begin);
                }

                bool playerCloserThanTarget = Utils.CalcDist(E.Stats.Pos, PursuePos) > Utils.CalcDist(E.Stats.Pos, pc.Stats.Pos);

                if (entitym.PlayerBlocked && (!E.Stats.InCombat || playerCloserThanTarget)
                        && mapr.Collider.IsFacing(pc.Stats.Pos.X, pc.Stats.Pos.Y, (char)pc.Stats.Direction, E.Stats.Pos.X, E.Stats.Pos.Y))
                {
                    Fleeing = true;
                    PursuePos = pc.Stats.Pos;
                }
            }

            if (E.Stats.Effects.Fear) Fleeing = true;

            if (
                    E.Stats.InCombat &&
                    E.Stats.CurState == StatBlock.EntityStance &&
                    !MoveToSafeDist && TargetDist < E.Stats.FleeRange &&
                    TargetDist >= E.Stats.MeleeRange &&
                    MathUtils.PercentChanceF(E.Stats.ChanceFlee) &&
                    E.Stats.FleeCooldownTimer.IsEnd()
                )
            {
                MoveToSafeDist = true;
            }

            if (MoveToSafeDist) Fleeing = true;

            if (Fleeing)
            {
                Vector2 targetPos = PursuePos;

                List<int> fleeDirs = new List<int>();

                int middleDir = Utils.CalcDirection(targetPos.X, targetPos.Y, E.Stats.Pos.X, E.Stats.Pos.Y);
                for (int i = -2; i <= 2; ++i)
                {
                    int testDir = Utils.RotateDirection(middleDir, i);

                    Vector2 testPos = Utils.CalcVector(E.Stats.Pos, testDir, 1);
                    if (mapr.Collider.IsValidPosition(testPos.X, testPos.Y, E.Stats.MovementType, MapCollision.CollideTypeAllEntities))
                    {
                        if (testDir == E.Stats.Direction)
                        {
                            fleeDirs.Clear();
                            fleeDirs.Add(testDir);
                            break;
                        }
                        else
                        {
                            fleeDirs.Add(testDir);
                        }
                    }
                }

                if (fleeDirs.Count == 0)
                {
                    MoveToSafeDist = false;
                    Fleeing = false;
                }
                else
                {
                    int index = MathUtils.RandBetween(0, fleeDirs.Count - 1);
                    PursuePos = Utils.CalcVector(E.Stats.Pos, fleeDirs[index], 1);

                    if (E.Stats.FleeTimer.IsEnd())
                    {
                        E.Stats.FleeTimer.Reset(Timer.Begin);
                    }
                }
            }
        }

        private void CheckPower()
        {
            var powers = SharedGameResources.Powers!;

            if (E.Stats.Effects.Stun || E.Stats.Effects.Fear || Fleeing) return;

            if (!E.Stats.InCombat) return;

            if (!E.Stats.Cooldown.IsEnd()) return;

            if (E.Stats.Npc && SharedGameResources.Menu != null && SharedGameResources.Menu.Talker != null && SharedGameResources.Menu.Talker.Visible && SharedGameResources.Menu.Talker.Npc == (NPC)E)
                return;

            if (E.Stats.CurState == StatBlock.EntityStance || E.Stats.CurState == StatBlock.EntityMove)
            {
                StatBlock.AIPower? aiPower = null;

                if (E.Stats.HalfDeadPower && E.Stats.Hp <= E.Stats.Get(global::FlareEngine.Stats.HpMax) / 2)
                {
                    aiPower = E.Stats.GetAIPower(StatBlock.AiPowerHalfDead);
                }
                else if (TargetDist > E.Stats.MeleeRange)
                {
                    aiPower = E.Stats.GetAIPower(StatBlock.AiPowerRanged);
                }
                else
                {
                    aiPower = E.Stats.GetAIPower(StatBlock.AiPowerMelee);
                }

                if (aiPower != null && powers.IsValid(aiPower.Id))
                {
                    PowerID replacedId = powers.CheckReplaceByEffect(aiPower.Id, E.Stats);
                    if (replacedId == 0)
                    {
                        aiPower = null;
                    }
                    else
                    {
                        Power pwr = powers.Powers[replacedId];
                        if (!Los && (pwr.RequiresLos || pwr.RequiresLosDefault))
                        {
                            aiPower = null;
                        }
                        if (aiPower != null)
                        {
                            E.Stats.CurState = StatBlock.EntityPower;
                            E.Stats.ActivatedPower = aiPower;
                            ReplacedPowerId = replacedId;

                            if (pwr.NewState != Power.StateInstant)
                            {
                                E.ResetActiveAnimation();
                            }
                        }
                    }
                }
            }

            if (E.Stats.CurState != StatBlock.EntityPower && E.Stats.ActivatedPower != null)
            {
                E.Stats.ActivatedPower = null;
            }
        }

        private void CheckMove()
        {
            var mapr = SharedGameResources.Mapr!;
            var pc = SharedGameResources.Pc!;

            if (E.Stats.CurState == StatBlock.EntityDead || E.Stats.CurState == StatBlock.EntityCritdead) return;

            if (E.Stats.Effects.Stun) return;

            if (E.Stats.Npc && SharedGameResources.Menu != null && SharedGameResources.Menu.Talker != null && SharedGameResources.Menu.Talker.Visible && SharedGameResources.Menu.Talker.Npc == (NPC)E)
            {
                if (E.Stats.CurState == StatBlock.EntityMove)
                {
                    E.Stats.CurState = StatBlock.EntityStance;
                }
                return;
            }

            if (!E.Stats.HeroAlly && !E.Stats.InCombat && (E.Stats.Waypoints.Count == 0 || !E.Stats.WaypointTimer.IsEnd()))
            {
                if (E.Stats.CurState == StatBlock.EntityMove)
                {
                    E.Stats.CurState = StatBlock.EntityStance;
                }

                return;
            }

            float realSpeed = E.Stats.Speed * StatBlock.SpeedMultiplier[E.Stats.Direction] * E.Stats.Effects.Speed / 100;

            uint turnTicks = TurnTimer.Current;
            TurnTimer.Duration = (uint)E.Stats.TurnDelay;

            int maxTurnTicks = (realSpeed == 0) ? E.Stats.TurnDelay : (int)(1f / realSpeed);
            if (E.Stats.TurnDelay > maxTurnTicks)
            {
                TurnTimer.Duration = (uint)maxTurnTicks;
            }
            TurnTimer.Current = turnTicks;

            mapr.Collider.Unblock(E.Stats.Pos.X, E.Stats.Pos.Y);

            PathFoundFailTimer.Tick();

            if (E.Stats.Facing)
            {
                TurnTimer.Tick();
                if (TurnTimer.IsEnd())
                {
                    if (!mapr.Collider.LineOfMovement(E.Stats.Pos.X, E.Stats.Pos.Y, PursuePos.X, PursuePos.Y, E.Stats.MovementType))
                    {
                        bool recalculatePath = false;

                        ChanceCalcPath += 5;

                        bool calcPathSuccess = MathUtils.PercentChance(ChanceCalcPath);
                        if (calcPathSuccess)
                            recalculatePath = true;

                        if (Collided)
                            recalculatePath = true;

                        if (!recalculatePath && Path.Count == 0)
                            recalculatePath = true;

                        if (!recalculatePath && Utils.CalcDist(PrevTarget.ToInt2().ToVector2(), PursuePos.ToInt2().ToVector2()) > 1f)
                            recalculatePath = true;

                        if (!PathFound && Collided && !calcPathSuccess)
                        {
                            recalculatePath = false;
                        }
                        else
                        {
                            Collided = false;
                        }

                        if (!PathFoundFailTimer.IsEnd())
                        {
                            recalculatePath = false;
                            ChanceCalcPath = -100;
                        }

                        PrevTarget = PursuePos;

                        if (recalculatePath)
                        {
                            ChanceCalcPath = -100;
                            Path.Clear();
                            PathFound = mapr.Collider.ComputePath(E.Stats.Pos, PursuePos, Path, E.Stats.MovementType, (uint)MapCollision.DefaultPathLimit);

                            if (!PathFound)
                            {
                                PathFoundFails++;
                                if (PathFoundFails >= PathFoundFailThreshold)
                                {
                                    PathFoundFailTimer.Reset(Timer.Begin);
                                }
                            }
                            else
                            {
                                PathFoundFails = 0;
                                PathFoundFailTimer.Reset(Timer.End);
                            }
                        }

                        if (Path.Count != 0)
                        {
                            PursuePos = Path[^1];

                            if (Utils.CalcDist(E.Stats.Pos, PursuePos) <= 1f)
                                Path.RemoveAt(Path.Count - 1);
                        }
                        else if (E.Stats.HeroAlly && PursuePos == pc.Stats.Pos)
                        {
                            WarpToHero = true;
                        }
                    }
                    else
                    {
                        Path.Clear();
                    }

                    if (E.Stats.ChargeSpeed == 0.0f)
                    {
                        E.Stats.Direction = Utils.CalcDirection(E.Stats.Pos.X, E.Stats.Pos.Y, PursuePos.X, PursuePos.Y);
                    }
                    TurnTimer.Reset(Timer.Begin);
                }
            }

            E.Stats.FleeTimer.Tick();
            E.Stats.FleeCooldownTimer.Tick();

            if (E.Stats.CurState == StatBlock.EntityStance)
            {
                CheckMoveStateStance();
            }
            else if (E.Stats.CurState == StatBlock.EntityMove)
            {
                CheckMoveStateMove();
            }

            if (E.Stats.Waypoints.Count != 0)
            {
                Vector2 waypoint = E.Stats.Waypoints.Peek();
                float waypointDist = Utils.CalcDist(waypoint, E.Stats.Pos);

                Vector2 savedPos = E.Stats.Pos;
                E.Move();
                float newDist = Utils.CalcDist(waypoint, E.Stats.Pos);
                E.Stats.Pos = savedPos;

                if (waypointDist <= realSpeed || (waypointDist <= 0.5f && newDist > waypointDist))
                {
                    E.Stats.Pos = waypoint;
                    TurnTimer.Reset(Timer.End);
                    E.Stats.Waypoints.Dequeue();
                    if (E.Stats.Wander)
                    {
                        waypoint = GetWanderPoint();
                    }
                    E.Stats.Waypoints.Enqueue(waypoint);
                    E.Stats.WaypointTimer.Reset(Timer.Begin);
                }
            }

            mapr.Collider.Block(E.Stats.Pos.X, E.Stats.Pos.Y, E.Stats.HeroAlly);
        }

        private void CheckMoveStateStance()
        {
            if (TargetDist >= E.Stats.FleeRange && E.Stats.ChanceFlee > 0 && E.Stats.Waypoints.Count == 0) return;

            bool allyTargetingHero = E.Stats.HeroAlly && !E.Stats.InCombat && HeroDist > AllyFollowDistanceWalk;
            bool shouldMoveToTarget = (E.Stats.InCombat || E.Stats.Waypoints.Count != 0) && ((TargetDist > E.Stats.MeleeRange && MathUtils.PercentChanceF(E.Stats.ChancePursue)) || (TargetDist <= E.Stats.MeleeRange && !Los));

            if (shouldMoveToTarget || Fleeing || allyTargetingHero)
            {
                if (E.Move())
                {
                    E.Stats.CurState = StatBlock.EntityMove;
                }
                else
                {
                    Collided = true;
                    byte prevDirection = E.Stats.Direction;

                    E.Stats.Direction = E.FaceNextBest(PursuePos.X, PursuePos.Y);
                    if (E.Move())
                    {
                        E.Stats.CurState = StatBlock.EntityMove;
                    }
                    else
                        E.Stats.Direction = prevDirection;
                }
            }
        }

        private void CheckMoveStateMove()
        {
            var pc = SharedGameResources.Pc!;
            var entitym = SharedGameResources.Entitym!;

            bool canAttack = true;

            if (!E.Stats.Cooldown.IsEnd())
            {
                canAttack = false;
            }
            else
            {
                canAttack = false;
                for (int i = 0; i < E.Stats.PowersAi.Count; ++i)
                {
                    if (E.Stats.PowersAi[i].Cooldown.IsEnd())
                    {
                        canAttack = true;
                        break;
                    }
                }
            }

            bool stopFleeing = canAttack && Fleeing && E.Stats.FleeTimer.IsEnd() && !MathUtils.PercentChanceF(E.Stats.ChanceFlee);

            if (!stopFleeing && E.Stats.FleeTimer.IsEnd())
            {
                E.Stats.FleeTimer.Current = 1;
            }

            bool allyTargetingHero = E.Stats.HeroAlly && !E.Stats.InCombat && !Fleeing && HeroDist < AllyFollowDistanceStop;
            if (pc.Stats.Alive && ((TargetDist < E.Stats.MeleeRange && !Fleeing) || (MoveToSafeDist && TargetDist >= E.Stats.FleeRange) || stopFleeing || allyTargetingHero))
            {
                if (stopFleeing)
                {
                    E.Stats.FleeCooldownTimer.Reset(Timer.Begin);
                }
                E.Stats.CurState = StatBlock.EntityStance;
                MoveToSafeDist = false;
                Fleeing = false;
            }
            else if (!E.Move())
            {
                Collided = true;
                byte prevDirection = E.Stats.Direction;
                E.Stats.Direction = E.FaceNextBest(PursuePos.X, PursuePos.Y);
                if (!E.Move())
                {
                    if (E.Stats.HeroAlly && entitym.PlayerBlocked && !E.Stats.InCombat)
                    {
                        E.Stats.Direction = pc.Stats.Direction;
                        if (!E.Move())
                        {
                            E.Stats.CurState = StatBlock.EntityStance;
                            E.Stats.Direction = prevDirection;
                        }
                    }
                    else
                    {
                        E.Stats.CurState = StatBlock.EntityStance;
                        E.Stats.Direction = prevDirection;
                    }
                }
            }
        }

        private void CheckOnStatePower(ref StatBlock.AIPower? onStatePower)
        {
            var powers = SharedGameResources.Powers!;

            if (onStatePower == null)
                return;

            StatBlock.AIPower aiPower = onStatePower;
            PowerID replacedId = powers.CheckReplaceByEffect(aiPower.Id, E.Stats);

            if (replacedId != 0)
            {
                Power pwr = powers.Powers[replacedId];
                if (pwr.NewState == Power.StateInstant)
                {
                    powers.Activate(replacedId, E.Stats, E.Stats.Pos, PursuePos);
                }
                else if (E.Stats.CurState == StatBlock.EntityPower)
                {
                    onStatePower = null;
                    return;
                }

                if (pwr.NewState != Power.StateInstant)
                {
                    E.Stats.CurState = Power.StateAttack;
                    E.Stats.ActivatedPower = aiPower;
                    ReplacedPowerId = replacedId;
                }

                for (int i = 0; i < E.Stats.PowersAi.Count; ++i)
                {
                    if (aiPower.Id == E.Stats.PowersAi[i].Id)
                    {
                        E.Stats.PowersAi[i].Cooldown.Duration = (uint)pwr.Cooldown;
                    }
                }
            }

            onStatePower = null;
        }

        private void UpdateState()
        {
            var powers = SharedGameResources.Powers!;
            var eset = SharedResources.Eset!;
            var mapr = SharedGameResources.Mapr!;

            if (E.Stats.Effects.Stun) return;

            CheckOnStatePower(ref E.Stats.AiDebuffPower);
            CheckOnStatePower(ref E.Stats.AiHitPower);

            PowerID powerId, powerIdBase;
            Power epower;
            int powerState;

            if (E.ActiveAnimation != null && !E.Stats.HoldState)
                E.ActiveAnimation.AdvanceFrame();

            for (int i = 0; i < E.Anims.Count; ++i)
            {
                if (E.Anims[i] != null)
                    E.Anims[i]!.AdvanceFrame();
            }

            switch (E.Stats.CurState)
            {
                case StatBlock.EntityStance:

                    E.SetAnimation("stance");
                    break;

                case StatBlock.EntityMove:

                    E.SetAnimation("run");
                    break;

                case StatBlock.EntityPower:

                    if (E.Stats.ActivatedPower == null)
                    {
                        E.Stats.CurState = StatBlock.EntityStance;
                        break;
                    }

                    powerId = ReplacedPowerId;
                    powerIdBase = E.Stats.ActivatedPower.Id;

                    if (!powers.IsValid(powerId) || !powers.IsValid(powerIdBase))
                    {
                        E.Stats.CurState = StatBlock.EntityStance;
                        break;
                    }

                    epower = powers.Powers[powerId];
                    powerState = epower.NewState;
                    E.Stats.PreventInterrupt = epower.PreventInterrupt;

                    if (powerState == Power.StateInstant)
                        InstantPower = true;
                    else if (powerState == Power.StateAttack)
                        E.SetAnimation(epower.AttackAnim);

                    if (E.ActiveAnimation!.IsFirstFrame())
                    {
                        for (int i = 0; i < epower.ChainPowers.Count; ++i)
                        {
                            ChainPower chainPower = epower.ChainPowers[i];
                            if (chainPower.Type == ChainPower.TypePre && MathUtils.PercentChanceF(chainPower.Chance))
                            {
                                powers.Activate(chainPower.Id, E.Stats, E.Stats.Pos, PursuePos);
                            }
                        }

                        float attackSpeed = (E.Stats.Effects.GetAttackSpeed(epower.AttackAnim) * epower.AttackSpeed) / 100.0f;
                        E.ActiveAnimation.SetSpeed(attackSpeed);
                        E.PlayAttackSound(epower.AttackAnim);

                        if (epower.StateDuration > 0)
                            E.Stats.StateTimer.Duration = (uint)epower.StateDuration;

                        if (epower.ChargeSpeed != 0.0f)
                            E.Stats.ChargeSpeed = epower.ChargeSpeed;
                    }

                    if (epower.StateHoldMode == Power.HoldOnFrame)
                    {
                        if (E.ActiveAnimation.IsFrame((short)epower.StateHoldFrame) && !E.Stats.HoldState)
                        {
                            if (!E.Stats.StateTimer.IsEnd())
                                E.Stats.HoldState = true;
                        }
                        if (E.Stats.StateTimer.IsEnd())
                            E.Stats.HoldState = false;
                    }

                    if ((E.ActiveAnimation.IsActiveFrame() || InstantPower) && !E.Stats.HoldState)
                    {
                        powers.Activate(powerId, E.Stats, E.Stats.Pos, PursuePos);

                        for (int i = 0; i < E.Stats.PowersAi.Count; ++i)
                        {
                            if (E.Stats.ActivatedPower.Id == E.Stats.PowersAi[i].Id)
                            {
                                E.Stats.PowersAi[i].Cooldown.Duration = (uint)epower.Cooldown;
                            }
                        }

                        if (E.Stats.ActivatedPower.Type == StatBlock.AiPowerHalfDead)
                        {
                            E.Stats.HalfDeadPower = false;
                        }

                        if (epower.StateHoldMode == Power.HoldOnActiveFrame && !E.Stats.StateTimer.IsEnd())
                            E.Stats.HoldState = true;
                    }

                    if ((E.ActiveAnimation.IsLastFrame() && E.Stats.StateTimer.IsEnd()) || (powerState == Power.StateAttack && E.ActiveAnimation.Name != epower.AttackAnim) || InstantPower)
                    {
                        if (!InstantPower)
                            E.Stats.Cooldown.Reset(Timer.Begin);
                        else
                            InstantPower = false;

                        E.Stats.ActivatedPower = null;
                        E.Stats.PreventInterrupt = false;
                        if (E.Stats.Hp > 0)
                        {
                            E.Stats.CurState = StatBlock.EntityStance;
                        }
                    }
                    break;

                case StatBlock.EntitySpawn:

                    E.SetAnimation("spawn");
                    if (E.ActiveAnimation!.IsLastFrame() || E.ActiveAnimation.Name != "spawn")
                    {
                        E.Stats.CurState = StatBlock.EntityStance;
                    }
                    break;

                case StatBlock.EntityBlock:

                    E.SetAnimation("block");
                    break;

                case StatBlock.EntityHit:

                    E.SetAnimation("hit");
                    if (E.ActiveAnimation!.IsFirstFrame())
                    {
                        E.Stats.Effects.TriggeredHit = true;
                    }
                    if (E.ActiveAnimation.IsLastFrame() || E.ActiveAnimation.Name != "hit")
                        E.Stats.CurState = StatBlock.EntityStance;
                    break;

                case StatBlock.EntityDead:
                    if (E.Stats.Effects.TriggeredDeath) break;

                    E.SetAnimation("die");
                    if (E.ActiveAnimation!.IsFirstFrame())
                    {
                        E.PlaySound(Entity.SoundDie);
                        E.Stats.CorpseTimer.Duration = (uint)eset.Misc.CorpseTimeout;
                    }
                    if ((E.ActiveAnimation.DefaultActiveFrames && E.ActiveAnimation.IsSecondLastFrame()) || (!E.ActiveAnimation.DefaultActiveFrames && E.ActiveAnimation.IsActiveFrame()))
                    {
                        StatBlock.AIPower? aiPower = E.Stats.GetAIPower(StatBlock.AiPowerDeath);
                        if (aiPower != null)
                            powers.Activate(aiPower.Id, E.Stats, E.Stats.Pos, E.Stats.Pos);

                        E.Stats.Effects.ClearEffects();
                    }
                    if (E.ActiveAnimation.IsLastFrame() || E.ActiveAnimation.Name != "die")
                    {
                        E.Stats.Corpse = true;

                        if (!E.Stats.CorpseHasCollision)
                        {
                            mapr.Collider.Unblock(E.Stats.Pos.X, E.Stats.Pos.Y);
                        }

                        if (!mapr.Collider.IsValidPosition(E.Stats.Pos.X, E.Stats.Pos.Y, MapCollision.MoveNormal, MapCollision.CollideTypeAllEntities))
                        {
                            E.Stats.CorpseTimer.Reset(Timer.End);
                        }

                        E.Stats.Pos.Align();
                    }

                    break;

                case StatBlock.EntityCritdead:
                    if (E.Stats.Effects.TriggeredDeath) break;

                    E.SetAnimation("critdie");
                    if (E.ActiveAnimation!.IsFirstFrame())
                    {
                        E.PlaySound(Entity.SoundCritdie);
                        E.Stats.CorpseTimer.Duration = (uint)eset.Misc.CorpseTimeout;
                    }
                    if ((E.ActiveAnimation.DefaultActiveFrames && E.ActiveAnimation.IsSecondLastFrame()) || (!E.ActiveAnimation.DefaultActiveFrames && E.ActiveAnimation.IsActiveFrame()))
                    {
                        StatBlock.AIPower? aiPower = E.Stats.GetAIPower(StatBlock.AiPowerDeath);
                        if (aiPower != null)
                            powers.Activate(aiPower.Id, E.Stats, E.Stats.Pos, E.Stats.Pos);

                        E.Stats.Effects.ClearEffects();
                    }
                    if (E.ActiveAnimation.IsLastFrame() || E.ActiveAnimation.Name != "critdie")
                    {
                        E.Stats.Corpse = true;

                        if (!E.Stats.CorpseHasCollision)
                        {
                            mapr.Collider.Unblock(E.Stats.Pos.X, E.Stats.Pos.Y);
                        }

                        if (!mapr.Collider.IsValidPosition(E.Stats.Pos.X, E.Stats.Pos.Y, MapCollision.MoveNormal, MapCollision.CollideTypeAllEntities))
                        {
                            E.Stats.CorpseTimer.Reset(Timer.End);
                        }

                        E.Stats.Pos.Align();
                    }

                    break;

                default:
                    break;
            }

            if (E.Stats.StateTimer.IsEnd() && E.Stats.HoldState)
                E.Stats.HoldState = false;

            if (E.Stats.CurState != StatBlock.EntityPower && E.Stats.ChargeSpeed != 0.0f)
                E.Stats.ChargeSpeed = 0.0f;
        }

        private Vector2 GetWanderPoint()
        {
            var mapr = SharedGameResources.Mapr!;
            Vector2 waypoint;
            waypoint.X = (float)E.Stats.WanderArea.X + (float)MathUtils.RandBetween(0, E.Stats.WanderArea.Width - 1) + 0.5f;
            waypoint.Y = (float)E.Stats.WanderArea.Y + (float)MathUtils.RandBetween(0, E.Stats.WanderArea.Height - 1) + 0.5f;

            if (mapr.Collider.IsValidPosition(waypoint.X, waypoint.Y, E.Stats.MovementType, mapr.Collider.GetCollideType(E.Stats.Hero)) &&
                mapr.Collider.LineOfMovement(E.Stats.Pos.X, E.Stats.Pos.Y, waypoint.X, waypoint.Y, E.Stats.MovementType))
            {
                return waypoint;
            }
            else
            {
                return E.Stats.Pos;
            }
        }
    }
}
