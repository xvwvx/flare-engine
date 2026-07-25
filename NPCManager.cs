// 对应 C++ 源文件：NPCManager.h + NPCManager.cpp
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// NPCManager
    ///
    /// 管理非战斗型 NPC（最常见的是商贩与可对话城镇居民），对应原始 <c>class NPCManager</c>。
    ///
    /// 资源管理：C++ 原始版本对 <c>std::vector&lt;NPC*&gt;</c> 中的每个元素以及
    /// <c>WidgetTooltip *tip</c> 使用 <c>new</c>/<c>delete</c> 手动管理生命周期。
    /// C# 版本沿用本项目既有约定（见 output/EntityManager.cs、output/Loot.cs 等）：
    /// 假定 <see cref="NPC"/> 与 <see cref="WidgetTooltip"/> 实现 <see cref="IDisposable"/>，
    /// 在每个原始 <c>delete</c> 调用点改为显式 <c>Dispose()</c>；本类自身也实现
    /// <see cref="IDisposable"/>，<see cref="Dispose"/> 对应原始析构函数，释放顺序与
    /// 原始代码逐行一致（先释放所有 NPC，再 <see cref="_tipBuf"/>.Clear()，最后释放
    /// <see cref="_tip"/>）。
    ///
    /// 全局指针映射：<c>mapr</c> → <see cref="SharedGameResources.Mapr"/>，
    /// <c>pc</c> → <see cref="SharedGameResources.Pc"/>，
    /// <c>camp</c> → <see cref="SharedGameResources.Camp"/>，
    /// <c>entitym</c> → <see cref="SharedGameResources.Entitym"/>，
    /// <c>fow</c> → <see cref="SharedGameResources.Fow"/>，
    /// <c>eset</c> → <see cref="SharedResources.Eset"/>。
    /// </summary>
    public class NPCManager : IDisposable
    {
        private WidgetTooltip? _tip;
        private TooltipData _tipBuf = new TooltipData();

        public List<NPC> Npcs = new List<NPC>();

        public NPCManager()
        {
            _tip = new WidgetTooltip();
            _tipBuf = new TooltipData();
        }

        public void AddRenders(List<Renderable> r)
        {
            for (int i = 0; i < Npcs.Count; i++)
            {
                if (SharedGameResources.Mapr != null && SharedGameResources.Mapr.Collider.IsOutsideMap(Npcs[i].Stats.Pos.X, Npcs[i].Stats.Pos.Y))
                    continue;

                if (SharedGameResources.Mapr!.Fogofwar > FogOfWar.TypeMinimap)
                {
                    float delta = Utils.CalcDist(SharedGameResources.Pc!.Stats.Pos, Npcs[i].Stats.Pos);
                    if (delta > SharedGameResources.Fow!.MaskRadius - 1.0)
                    {
                        continue;
                    }
                }
                Npcs[i].AddRenders(r);
            }
        }

        public void HandleNewMap()
        {

            ItemStack itemRoll = new ItemStack();

            SortedDictionary<string, NPC> allies = new SortedDictionary<string, NPC>();

            // remove existing NPCs
            for (int i = 0; i < Npcs.Count; i++)
            {
                if (Npcs[i].Stats.HeroAlly && !Npcs[i].Stats.Corpse && Npcs[i].Stats.CurState != StatBlock.EntityDead && Npcs[i].Stats.CurState != StatBlock.EntityCritdead && Npcs[i].Stats.Speed > 0.0f)
                {
                    allies[Npcs[i].Filename] = Npcs[i];
                }
                else
                {
                    Npcs[i].Dispose();
                }
            }

            Npcs.Clear();

            // read the queued NPCs in the map file
            for (int i = 0; i < SharedGameResources.Mapr!.MapNpcs.Count; ++i)
            {
                MapNpc mn = SharedGameResources.Mapr!.MapNpcs[i];

                if (!SharedGameResources.Camp!.CheckRequirementsInVector(mn.Requirements))
                    continue;

                // ally npc that was moved from another map should not be loaded once again
                if (allies.ContainsKey(mn.Id))
                {
                    continue;
                }

                NPC npc;
                Entity entity = SharedGameResources.Entitym!.GetEntityPrototype(mn.Id);
                if (entity != null)
                {
                    npc = new NPC(entity);
                    entity.Dispose();
                }
                else
                {
                    npc = new NPC(new Entity());
                }

                if (!npc.Load(mn.Id))
                {
                    continue;
                }

                npc.Stats.Pos.X = mn.Pos.X;
                npc.Stats.Pos.Y = mn.Pos.Y;
                npc.Stats.HeroAlly = false;
                npc.Stats.Npc = true;
                if (mn.Direction != -1)
                    npc.Stats.Direction = (byte)mn.Direction;

                for (int j = 0; j < mn.Waypoints.Count; ++j)
                {
                    npc.Stats.Waypoints.Enqueue(mn.Waypoints[j]);
                }
                npc.Stats.Wander = mn.WanderRadius > 0;
                npc.Stats.SetWanderArea(mn.WanderRadius);

                // npc->stock.sort();
                Npcs.Add(npc);
                CreateMapEvent(npc, Npcs.Count);
                if (!SharedGameResources.Mapr!.Collider.IsValidPosition(npc.Stats.Pos.X, npc.Stats.Pos.Y, MapCollision.MoveNormal, MapCollision.CollideTypeNone))
                    Utils.LogInfo("NPC: Collision tile detected at NPC position (%.2f, %.2f).", npc.Stats.Pos.X, npc.Stats.Pos.Y);
            }

            while (allies.Count > 0)
            {
                KeyValuePair<string, NPC> allyEntry = allies.First();
                NPC npc = allyEntry.Value;
                allies.Remove(allyEntry.Key);

                npc.Stats.Pos = SharedGameResources.Mapr!.Collider.GetRandomNeighbor(SharedGameResources.Pc!.Stats.Pos.ToInt2(), 1, npc.Stats.MovementType, MapCollision.CollideTypeAllEntities);
                npc.Stats.Direction = SharedGameResources.Pc!.Stats.Direction;

                Npcs.Add(npc);
                CreateMapEvent(npc, Npcs.Count);

                SharedGameResources.Mapr!.Collider.Block(npc.Stats.Pos.X, npc.Stats.Pos.Y, !MapCollision.IsAlly);

                SharedGameResources.Entitym!.Entities.Add(npc);
            }

        }

        public void CreateMapEvent(NPC npc, int npcs)
        {
            // create a map event for provided npc
            Event ev = new Event();
            EventComponent ec = new EventComponent();

            // the event hotspot is a 1x1 tile at the npc's feet
            ev.ActivateType = Event.ActivateOnInteract;
            ev.KeepAfterTrigger = true;
            Rectangle location = new Rectangle();
            location.X = (int)npc.Stats.Pos.X;
            location.Y = (int)npc.Stats.Pos.Y;
            location.Width = location.Height = 1;
            ev.Location = ev.Hotspot = location;
            ev.Center.X = (float)ev.Hotspot.X + (float)ev.Hotspot.Width / 2;
            ev.Center.Y = (float)ev.Hotspot.Y + (float)ev.Hotspot.Height / 2;

            ec.Type = EventComponent.NpcID;
            ec.Data[0].Int = npcs - 1;
            ev.Components.Add(ec);

            ec = new EventComponent();
            ec.Type = EventComponent.Tooltip;
            ec.S = npc.Name;
            ev.Components.Add(ec);

            ec = new EventComponent();
            ec.Type = EventComponent.NpcHotspot;
            ec.Data[0].Int = (int)npc.Stats.Pos.X;
            ec.Data[1].Int = (int)npc.Stats.Pos.Y;
            ec.Id = Npcs.Count;
            for (int i = 0; i < Npcs.Count; ++i)
            {
                if (npc == Npcs[i])
                {
                    ec.Id = i;
                    break;
                }
            }
            ev.Components.Add(ec);

            ev.Type = npc.Filename;

            SharedGameResources.Mapr!.Events.Add(ev);
        }

        public void Logic()
        {
            for (int i = 0; i < Npcs.Count; i++)
            {
                Npcs[i].Logic();
            }
        }

        public int GetId(string npcName)
        {
            for (int i = 0; i < Npcs.Count; i++)
            {
                if (Npcs[i].Filename == npcName) return i;
            }

            // could not find NPC, try loading it here
            NPC npc;
            Entity entity = SharedGameResources.Entitym!.GetEntityPrototype(npcName);
            if (entity != null)
            {
                npc = new NPC(entity);
                entity.Dispose();
            }
            else
            {
                npc = new NPC(new Entity());
            }

            if (npc != null)
            {
                npc.Load(npcName);
                Npcs.Add(npc);
                return Npcs.Count - 1;
            }

            return -1;
        }

        public Entity? NpcFocus(Int2 mouse, Vector2 cam, bool aliveOnly)
        {
            for (int i = 0; i < Npcs.Count; i++)
            {
                if (aliveOnly && (Npcs[i].Stats.CurState == StatBlock.EntityDead || Npcs[i].Stats.CurState == StatBlock.EntityCritdead))
                {
                    continue;
                }
                if (!Npcs[i].Stats.HeroAlly)
                {
                    continue;
                }

                if (Utils.IsWithinRect(Npcs[i].GetRenderBounds(cam), mouse))
                {
                    return Npcs[i];
                }
            }
            return null;
        }

        public Entity? GetNearestNpc(Vector2 pos, bool getCorpse = false)
        {
            Entity? nearest = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < Npcs.Count; i++)
            {
                if (!getCorpse && (Npcs[i].Stats.CurState == StatBlock.EntityDead || Npcs[i].Stats.CurState == StatBlock.EntityCritdead))
                {
                    continue;
                }
                if (getCorpse && !Npcs[i].Stats.Corpse)
                {
                    continue;
                }
                if (!Npcs[i].Stats.HeroAlly)
                {
                    continue;
                }

                float distance = Utils.CalcDist(pos, Npcs[i].Stats.Pos);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    nearest = Npcs[i];
                }
            }

            if (bestDistance > SharedResources.Eset!.Misc.InteractRange)
                nearest = null;

            return nearest;
        }

        /// <summary>
        /// 对应 C++ 的 <c>~NPCManager()</c>：先记录清理日志，再按原始 for 循环顺序
        /// 释放 <see cref="Npcs"/> 中的每个 NPC，然后清空 <see cref="_tipBuf"/>，
        /// 最后释放 <see cref="_tip"/>。
        /// </summary>
        public void Dispose()
        {
            Utils.LogInfo("Cleaning up: NPCManager");

            for (int i = 0; i < Npcs.Count; i++)
            {
                Npcs[i].Dispose();
            }

            _tipBuf.Clear();
            _tip?.Dispose();
            _tip = null;

            GC.SuppressFinalize(this);
        }
    }
}
