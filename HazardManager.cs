// 对应 C++ 源：HazardManager.h + HazardManager.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略�?
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// HazardManager
    ///
    /// 持有当前场上所�?Hazard（正在生效的攻击、法术等）的集合，并处理它们的群体逻辑
    /// （生命周期到期、撞墙特效、连锁法术、命中判定、驱散等）�?
    ///
    /// C++ 原始版本�?<c>std::vector&lt;Hazard*&gt;</c> 中的每个元素使用 <c>new</c>/<c>delete</c>
    /// 手动管理生命周期（单一所有者：HazardManager 创建/持有/销毁自己集合中�?Hazard）�?
    /// C# 版本沿用本项目既有约定（�?output/CombatText.cs、output/AnimationManager.cs 等）�?
    /// 假定 <c>Hazard</c> 未来转换时实�?<see cref="IDisposable"/>，在每个原始 <c>delete h[i];</c>
    /// 调用点改为显式调�?<c>H[i].Dispose()</c>�?c>HazardManager</c> 自身也实�?
    /// <see cref="IDisposable"/>，对应原始析构函数，释放顺序与原始代码逐行一致�?
    /// </summary>
    public class HazardManager : IDisposable
    {
        public List<Hazard> H = new List<Hazard>();
        public Entity? LastEnemy;

        public HazardManager()
        {
            LastEnemy = null;
        }

        /// <summary>
        /// 对应 C++ �?<c>~HazardManager()</c>。先记录清理日志，再按原�?for 循环顺序
        /// （从头到尾）释放 <see cref="H"/> 中的每个 Hazard，最后清�?<see cref="LastEnemy"/>
        /// 引用（原始代码中 <c>h.clear();</c> 在析构函数中被注释掉，因此这里同样不调用
        /// <c>H.Clear()</c>，仅保留同样的注释以说明这一点）�?
        /// </summary>
        public void Dispose()
        {
            Utils.LogInfo("Cleaning up: HazardManager");

            for (int i = 0; i < H.Count; i++)
                H[i].Dispose();
            // H.Clear(); not needed in destructor
            LastEnemy = null;

            GC.SuppressFinalize(this);
        }

        public void Logic()
        {
            var powers = SharedGameResources.Powers!;
            var eventm = SharedGameResources.Eventm!;
            var entitym = SharedGameResources.Entitym!;
            var pc = SharedGameResources.Pc!;

            // remove all hazards with lifespan 0.  Most hazards still display their last frame.
            for (int i = H.Count; i > 0; i--)
            {
                if (H[i - 1].Lifespan == 0)
                {
                    for (int j = 0; j < H[i - 1].HazardPower!.ChainPowers.Count; ++j)
                    {
                        ChainPower chainPower = H[i - 1].HazardPower!.ChainPowers[j];
                        if (chainPower.Type == ChainPower.TypeExpire && MathUtils.PercentChanceF(chainPower.Chance))
                        {
                            powers.Activate(chainPower.Id, H[i - 1].SrcStats, H[i - 1].Pos, H[i - 1].Pos);

                            if (powers.Powers[chainPower.Id].Directional)
                            {
                                powers.Hazards.Last().Direction = H[i - 1].Direction;
                            }
                        }
                    }

                    H[i - 1].Dispose();
                    H.RemoveAt(i - 1);
                }
            }

            CheckNewHazards();

            // handle single-frame transforms
            for (int i = H.Count; i > 0; i--)
            {
                int hIndex = i - 1;
                Hazard hazard = H[hIndex];

                hazard.Logic();

                // remove all hazards that need to die immediately (e.g. exit the map)
                if (hazard.RemoveNow)
                {
                    hazard.Dispose();
                    H.RemoveAt(hIndex);
                    continue;
                }


                // if a moving hazard hits a wall, check for an after-effect
                if (hazard.HitWall)
                {
                    if (hazard.HazardPower!.ScriptTrigger == Power.ScriptTriggerWall)
                    {
                        eventm.ExecuteScript(hazard.HazardPower!.Script, hazard.Pos.X, hazard.Pos.Y);
                    }

                    for (int j = 0; j < hazard.HazardPower!.ChainPowers.Count; ++j)
                    {
                        ChainPower chainPower = hazard.HazardPower!.ChainPowers[j];
                        if (chainPower.Type == ChainPower.TypeWall && MathUtils.PercentChanceF(chainPower.Chance))
                        {
                            powers.Activate(chainPower.Id, hazard.SrcStats, hazard.Pos, hazard.Pos);

                            if (powers.Powers[chainPower.Id].Directional)
                            {
                                powers.Hazards.Last().Direction = hazard.Direction;
                            }
                        }
                    }

                    // clear wall hit
                    hazard.HitWall = false;
                }

                // handle collisions
                if (hazard.IsDangerousNow())
                {

                    // process hazards that can hurt enemies & allies
                    for (int eIndex = 0; eIndex < entitym.Entities.Count; eIndex++)
                    {
                        Entity e = entitym.Entities[eIndex];

                        // hero/ally powers can only hit allies if target_party is true
                        if ((hazard.SourceType == Power.SourceTypeHero || hazard.SourceType == Power.SourceTypeAlly) && e.Stats.HeroAlly && !hazard.HazardPower!.TargetParty)
                        {
                            continue;
                        }

                        // enemy hazard can't hurt other enemies
                        if (hazard.SourceType == Power.SourceTypeEnemy && !e.Stats.HeroAlly)
                        {
                            continue;
                        }

                        // only check living enemies
                        if (e.Stats.Hp > 0 && hazard.Active)
                        {
                            if (Utils.IsWithinRadius(hazard.Pos, hazard.HazardPower!.Radius, e.Stats.Pos))
                            {
                                if (!hazard.HasEntity(e))
                                {
                                    // hit!
                                    hazard.AddEntity(e);
                                    HitEntity(hIndex, e.TakeHit(hazard));
                                    if (!hazard.HazardPower!.Beacon)
                                    {
                                        LastEnemy = e;
                                    }
                                }
                            }
                        }

                    }

                    // process hazards that can hurt the hero
                    if (hazard.SourceType != Power.SourceTypeHero && hazard.SourceType != Power.SourceTypeAlly)
                    { //enemy or neutral sources
                        if (pc.Stats.Hp > 0 && hazard.Active)
                        {
                            if (Utils.IsWithinRadius(hazard.Pos, hazard.HazardPower!.Radius, pc.Stats.Pos))
                            {
                                if (!hazard.HasEntity(pc))
                                {
                                    // hit!
                                    hazard.AddEntity(pc);
                                    HitEntity(hIndex, pc.TakeHit(hazard));
                                }
                            }
                        }
                    }

                    // dispel hazards can remove other hazards by ID
                    for (int j = 0; j < hazard.HazardPower!.DispelPowerIds.Count; ++j)
                    {
                        PowerID dispelId = hazard.HazardPower!.DispelPowerIds[j];

                        for (int k = 0; k < H.Count; ++k)
                        {
                            if (dispelId != H[k].PowerIndex)
                                continue;

                            if (hazard.SourceType == Power.SourceTypeNeutral ||
                                (hazard.SourceType == Power.SourceTypeEnemy && H[k].SourceType != Power.SourceTypeEnemy) ||
                                (hazard.SourceType != Power.SourceTypeEnemy && H[k].SourceType == Power.SourceTypeEnemy))
                            {
                                if (Utils.IsWithinRadius(hazard.Pos, hazard.HazardPower!.Radius, H[k].Pos))
                                {
                                    H[k].Lifespan = 0;
                                    // H[k].RemoveNow = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Look for hazards generated this frame
        /// </summary>
        public void CheckNewHazards()
        {
            var powers = SharedGameResources.Powers!;

            // check PowerManager for hazards
            while (powers.Hazards.Count > 0)
            {
                Hazard newHaz = powers.Hazards.Peek();
                powers.Hazards.Dequeue();

                H.Add(newHaz);
            }
        }

        /// <summary>
        /// Reset all hazards and get new collision object
        /// </summary>
        public void HandleNewMap()
        {
            for (int i = 0; i < H.Count; i++)
            {
                H[i].Dispose();
            }
            H.Clear();
            LastEnemy = null;
        }

        /// <summary>
        /// addRenders()
        /// Map objects need to be drawn in Z order, so we allow a parent object (GameEngine)
        /// to collect all mobile sprites each frame.
        /// </summary>
        public void AddRenders(List<Renderable> r, List<Renderable> rDead)
        {
            for (int i = 0; i < H.Count; i++)
            {
                if (SharedGameResources.Mapr != null && SharedGameResources.Mapr.Collider.IsOutsideMap(H[i].Pos.X, H[i].Pos.Y))
                    continue;

                H[i].AddRenderable(r, rDead);
            }
        }

        private void HitEntity(int index, bool hit)
        {
            var snd = SharedResources.Snd!;
            var eventm = SharedGameResources.Eventm!;

            if (!hit) return;

            if (!H[index].HazardPower!.Multitarget)
            {
                H[index].Active = false;
                if (!H[index].HazardPower!.CompleteAnimation) H[index].Lifespan = 0;
            }
            if (H[index].HazardPower!.SfxHitEnable && !H[index].SfxHitPlayed)
            {
                snd.Play(H[index].HazardPower!.SfxHit, SoundManager.DefaultChannel, H[index].Pos, !SoundManager.Loop);
                H[index].SfxHitPlayed = true;
            }

            if (H[index].HazardPower!.ScriptTrigger == Power.ScriptTriggerHit)
            {
                eventm.ExecuteScript(H[index].HazardPower!.Script, H[index].Pos.X, H[index].Pos.Y);
            }
        }
    }
}
