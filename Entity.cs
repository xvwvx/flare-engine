// <自动生成> 对应 C++ 源文件：Entity.h + Entity.cpp
using System.Diagnostics;
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// Entity - 游戏实体基类，对应 C++ <c>class Entity</c>。
    /// 管理动画（ActiveAnimation/Anims）、动画集（Animsets）、行为（Behavior）等。
    /// 动画和行为的生命周期由 Dispose 管理（对应 C++ ~Entity() 中的 delete）。
    /// </summary>
    public class Entity : IDisposable
    {
        /// <summary>对应 C++ <c>Entity::Layer_gfx</c>。/summary>
        public class LayerGfx
        {
            public string Gfx = "";
            public string Type = "";
        }

        public const int SoundHit = 0;
        public const int SoundDie = 1;
        public const int SoundCritdie = 2;
        public const int SoundBlock = 3;

        protected Image? Sprites;

        public List<(string First, List<SoundID> Second)> SoundAttack = new List<(string, List<SoundID>)>();
        public List<SoundID> SoundHitIds = new List<SoundID>();
        public List<SoundID> SoundDieIds = new List<SoundID>();
        public List<SoundID> SoundCritdieIds = new List<SoundID>();
        public List<SoundID> SoundBlockIds = new List<SoundID>();
        public SoundID SoundLevelup;
        public SoundID SoundLowhp;

        public Animation? ActiveAnimation;
        public AnimationSet? AnimationSet;
        public List<AnimationSet?> Animsets = new List<AnimationSet?>();
        public List<Animation?> Anims = new List<Animation?>();

        public StatBlock Stats = new StatBlock();

        public string TypeFilename = "";

        public EntityBehavior? Behavior;

        public Entity()
        {
            SoundLevelup = 0;
            SoundLowhp = 0;
            ActiveAnimation = null;
            AnimationSet = null;
            Behavior = new EntityBehavior(this);
        }

        /// <summary>?? C++ ???<c>Entity(const Entity&amp; e)</c><c>operator=</c> </summary>
        public Entity(Entity e)
        {
            Sprites = e.Sprites;

            SoundAttack = new List<(string, List<SoundID>)>();
            for (int i = 0; i < e.SoundAttack.Count; i++)
            {
                SoundAttack.Add((e.SoundAttack[i].First, new List<SoundID>(e.SoundAttack[i].Second)));
            }
            SoundHitIds = new List<SoundID>(e.SoundHitIds);
            SoundDieIds = new List<SoundID>(e.SoundDieIds);
            SoundCritdieIds = new List<SoundID>(e.SoundCritdieIds);
            SoundBlockIds = new List<SoundID>(e.SoundBlockIds);
            SoundLevelup = e.SoundLevelup;
            SoundLowhp = e.SoundLowhp;

            Stats = new StatBlock(e.Stats);

            ActiveAnimation = null;
            AnimationSet = null;

            LoadAnimations();

            TypeFilename = e.TypeFilename;

            Behavior = new EntityBehavior(this);
        }

        /// <summary>?? C++  <c>~Entity()</c>。/summary>
        public virtual void Dispose()
        {
            if (!string.IsNullOrEmpty(Stats.Animations))
                SharedResources.Anim!.DecreaseCount(Stats.Animations);

            for (int i = 0; i < Animsets.Count; i++)
            {
                if (Animsets[i] != null)
                    SharedResources.Anim!.DecreaseCount(Animsets[i]!.Name);
                Anims[i]?.Dispose();
            }
            SharedResources.Anim!.CleanUp();

            ActiveAnimation?.Dispose();
            Behavior?.Dispose();

            GC.SuppressFinalize(this);
        }

        public void Logic()
        {
            Behavior!.Logic();
        }

        public void LoadSounds()
        {
            LoadSoundsFromStatBlock(null);
        }

        public void LoadSoundsFromStatBlock(StatBlock? srcStats)
        {
            var snd = SharedResources.Snd!;

            UnloadSounds();

            if (srcStats == null) srcStats = Stats;

            for (int i = 0; i < srcStats.SfxAttack.Count; ++i)
            {
                string animName = srcStats.SfxAttack[i].AnimName;
                SoundAttack.Add((animName, new List<SoundID>()));
                for (int j = 0; j < srcStats.SfxAttack[i].Filenames.Count; ++j)
                {
                    SoundID sid = snd.Load(srcStats.SfxAttack[i].Filenames[j], "Entity attack");
                    SoundAttack[^1].Second.Add(sid);
                }
            }

            for (int i = 0; i < srcStats.SfxHit.Count; ++i)
            {
                SoundHitIds.Add(snd.Load(srcStats.SfxHit[i], "Entity was hit"));
            }
            for (int i = 0; i < srcStats.SfxDie.Count; ++i)
            {
                SoundDieIds.Add(snd.Load(srcStats.SfxDie[i], "Entity died"));
            }
            for (int i = 0; i < srcStats.SfxCritdie.Count; ++i)
            {
                SoundCritdieIds.Add(snd.Load(srcStats.SfxCritdie[i], "Entity died from critical hit"));
            }
            for (int i = 0; i < srcStats.SfxBlock.Count; ++i)
            {
                SoundBlockIds.Add(snd.Load(srcStats.SfxBlock[i], "Entity blocked"));
            }

            if (srcStats.SfxLevelup != "")
                SoundLevelup = snd.Load(srcStats.SfxLevelup, "Entity leveled up");

            if (srcStats.SfxLowhp != "")
                SoundLowhp = snd.Load(srcStats.SfxLowhp, "Entity has low hp");
        }

        public void UnloadSounds()
        {
            var snd = SharedResources.Snd!;
            
            for (int i = 0; i < SoundAttack.Count; ++i)
            {
                for (int j = 0; j < SoundAttack[i].Second.Count; ++j)
                {
                    snd.Unload(SoundAttack[i].Second[j]);
                }
            }

            for (int i = 0; i < SoundHitIds.Count; ++i)
            {
                snd.Unload(SoundHitIds[i]);
            }
            for (int i = 0; i < SoundDieIds.Count; ++i)
            {
                snd.Unload(SoundDieIds[i]);
            }
            for (int i = 0; i < SoundCritdieIds.Count; ++i)
            {
                snd.Unload(SoundCritdieIds[i]);
            }
            for (int i = 0; i < SoundBlockIds.Count; ++i)
            {
                snd.Unload(SoundBlockIds[i]);
            }

            snd.Unload(SoundLevelup);
            snd.Unload(SoundLowhp);
        }

        public void PlayAttackSound(string attackName)
        {
            var snd = SharedResources.Snd!;
            for (int i = 0; i < SoundAttack.Count; ++i)
            {
                if (SoundAttack[i].Second.Count != 0 && SoundAttack[i].First == attackName)
                {
                    int randIndex = Program.Rng.Next() % SoundAttack[i].Second.Count;
                    snd.Play(SoundAttack[i].Second[randIndex], SoundManager.DefaultChannel, Stats.Pos, !SoundManager.Loop);
                    return;
                }
            }
        }

        public void PlaySound(int soundType)
        {
            var snd = SharedResources.Snd!;

            if (soundType == Entity.SoundHit && SoundHitIds.Count != 0)
            {
                int randIndex = Program.Rng.Next() % SoundHitIds.Count;
                string channelName = "entity_hit_" + SoundHitIds[randIndex];
                snd.Play(SoundHitIds[randIndex], channelName, Stats.Pos, !SoundManager.Loop);
            }
            else if (soundType == Entity.SoundDie && SoundDieIds.Count != 0)
            {
                int randIndex = Program.Rng.Next() % SoundDieIds.Count;
                string channelName = "entity_die_" + SoundDieIds[randIndex];
                snd.Play(SoundDieIds[randIndex], channelName, Stats.Pos, !SoundManager.Loop);
            }
            else if (soundType == Entity.SoundCritdie && SoundCritdieIds.Count != 0)
            {
                int randIndex = Program.Rng.Next() % SoundCritdieIds.Count;
                string channelName = "entity_critdie_" + SoundCritdieIds[randIndex];
                snd.Play(SoundCritdieIds[randIndex], channelName, Stats.Pos, !SoundManager.Loop);
            }
            else if (soundType == Entity.SoundBlock && SoundBlockIds.Count != 0)
            {
                int randIndex = Program.Rng.Next() % SoundBlockIds.Count;
                string channelName = "entity_block_" + SoundBlockIds[randIndex];
                snd.Play(SoundBlockIds[randIndex], channelName, Stats.Pos, !SoundManager.Loop);
            }
        }

        private void MoveFromOffendingTile()
        {
            var mapr = SharedGameResources.Mapr!;

            // don't bother if there's no possible tile for a stuck entity to move to
            if (!mapr.Collider.HasEmptyTile)
                return;

            // If we got stuck on a tile, which we're not allowed to be on, move away
            // This is just a workaround as we cannot reproduce being stuck easily nor find the
            // errornous code, check https://github.com/flareteam/flare-engine/issues/1058

            // As this method should do nothing while regular gameplay, but only in case of bugs
            // we don't need to care about nice graphical effects, so we may just jump out of the
            // offending tile. The idea is simple: We can only be stuck on a tile by accident,
            // so we got here somehow. We'll try to push this entity to the nearest valid place

            Vector2 originalPos = Stats.Pos;
            bool originalPosIsBad = false;
            int collideType = mapr.Collider.GetCollideType(Stats.Hero);

            while (!mapr.Collider.IsValidPosition(Stats.Pos.X, Stats.Pos.Y, Stats.MovementType, collideType))
            {
                originalPosIsBad = true;

                float pushx = 0;
                float pushy = 0;

                if (mapr.Collider.IsValidPosition(Stats.Pos.X + 1, Stats.Pos.Y, Stats.MovementType, collideType))
                    pushx += 0.1f * (2 - ((float)((int)(Stats.Pos.X + 1)) + 0.5f - Stats.Pos.X));

                if (mapr.Collider.IsValidPosition(Stats.Pos.X - 1, Stats.Pos.Y, Stats.MovementType, collideType))
                    pushx -= 0.1f * (2 - (Stats.Pos.X - ((float)((int)(Stats.Pos.X - 1)) + 0.5f)));

                if (mapr.Collider.IsValidPosition(Stats.Pos.X, Stats.Pos.Y + 1, Stats.MovementType, collideType))
                    pushy += 0.1f * (2 - ((float)((int)(Stats.Pos.Y + 1)) + 0.5f - Stats.Pos.Y));

                if (mapr.Collider.IsValidPosition(Stats.Pos.X, Stats.Pos.Y - 1, Stats.MovementType, collideType))
                    pushy -= 0.1f * (2 - (Stats.Pos.Y - ((float)((int)(Stats.Pos.Y - 1)) + 0.5f)));

                Stats.Pos.X += pushx;
                Stats.Pos.Y += pushy;

                // we don't move, but we're still stuck on an invalid tile,
                // the final life saver before being crushed by an invalid tile:
                // just blink away. This will seriously irritate the player, but there
                // is probably no other easy way to repair the game
                if (pushx == 0 && pushy == 0)
                {
                    Int2 srcPos = Stats.Pos.ToInt2();
                    Vector2 shortestPos = default;
                    float shortestDist = 0;
                    int radius = 1;

                    while (radius <= Math.Max(mapr.W, mapr.H))
                    {
                        for (int i = srcPos.X - radius; i <= srcPos.X + radius; ++i)
                        {
                            for (int j = srcPos.Y - radius; j <= srcPos.Y + radius; ++j)
                            {
                                if (mapr.Collider.IsValidPosition((float)i, (float)j, Stats.MovementType, collideType))
                                {
                                    float testDist = Utils.CalcDist(Stats.Pos, shortestPos);
                                    if (shortestDist == 0 || testDist < shortestDist)
                                    {
                                        shortestDist = testDist;
                                        shortestPos.X = (float)i + 0.5f;
                                        shortestPos.Y = (float)j + 0.5f;
                                    }
                                }
                            }
                        }
                        if (shortestDist != 0)
                        {
                            Stats.Pos = shortestPos;
                            break;
                        }
                        radius++;
                    }
                }
            }

            if (originalPosIsBad)
            {
                Utils.LogInfo("Entity: '%s' was stuck and has been moved: (%g, %g) -> (%g, %g)",
                        Stats.Name,
                        originalPos.X,
                        originalPos.Y,
                        Stats.Pos.X,
                        Stats.Pos.Y);
            }
        }

        /// <summary>
        /// move()
        /// Apply speed to the direction faced.
        ///
        /// @return Returns false if wall collision, otherwise true.
        /// </summary>
        public bool Move()
        {
            if (Stats.Effects.KnockbackSpeed != 0)
                return false;

            if (Stats.Effects.Stun || Stats.Effects.Speed == 0)
                return false;

            if (Stats.ChargeSpeed != 0.0f)
                return false;

            MoveFromOffendingTile();

            var mapr = SharedGameResources.Mapr!;

            float speed = Stats.Speed * StatBlock.SpeedMultiplier[Stats.Direction] * Stats.Effects.Speed / 100;
            float dx = speed * StatBlock.DirectionDeltaX[Stats.Direction];
            float dy = speed * StatBlock.DirectionDeltaY[Stats.Direction];

            bool fullMove = mapr.Collider.Move(ref Stats.Pos.X, ref Stats.Pos.Y, dx, dy, Stats.MovementType, mapr.Collider.GetCollideType(Stats.Hero));

            return fullMove;
        }

        /// <summary>
        /// Whenever a hazard collides with an entity, this function resolves the effect
        /// Called by HazardManager
        ///
        /// Returns false on miss
        /// </summary>
        public bool TakeHit(Hazard h)
        {
            var eset = SharedResources.Eset!;
            var inpt = SharedResources.Inpt!;
            var comb = SharedResources.Comb!;
            var msg = SharedResources.Msg!;
            var hazards = SharedGameResources.Hazards!;
            var powers = SharedGameResources.Powers!;
            var settings = SharedResources.Settings!;
            var mapr = SharedGameResources.Mapr!;

            //check if this enemy should be affected by this hazard based on the category
            if (h.HazardPower!.TargetCategories.Count != 0)
            {
                //the power has a target category requirement, so if it doesnt match, dont continue
                bool matchFound = false;
                for (int i = 0; i < Stats.Categories.Count; i++)
                {
                    if (h.HazardPower!.TargetCategories.Contains(Stats.Categories[i]))
                    {
                        matchFound = true;
                    }
                }
                if (!matchFound)
                    return false;
            }

            // check if this entity allows attacks from this power id
            if (Stats.PowerFilter.Count != 0 && !Stats.PowerFilter.Contains(h.PowerIndex))
            {
                return false;
            }

            //if the target is already dead, they cannot be hit
            if (Stats.CurState == StatBlock.EntityDead || Stats.CurState == StatBlock.EntityCritdead)
                return false;

            // some attacks will always miss enemies of a certain movement type
            if (Stats.MovementType == MapCollision.MoveNormal && !h.HazardPower!.TargetMovementNormal)
                return false;
            else if (Stats.MovementType == MapCollision.MoveFlying && !h.HazardPower!.TargetMovementFlying)
                return false;
            else if (Stats.MovementType == MapCollision.MoveIntangible && !h.HazardPower!.TargetMovementIntangible)
                return false;

            // prevent hazard aoe from hitting targets behind walls
            if (h.HazardPower!.WallsBlockAoe && !mapr.Collider.LineOfMovement(Stats.Pos.X, Stats.Pos.Y, h.Pos.X, h.Pos.Y, MapCollision.MoveNormal))
                return false;

            // some enemies can be invicible based on campaign status
            if (!Stats.Hero && !Stats.HeroAlly && h.SourceType != Power.SourceTypeEnemy)
            {
                if (Stats.InvincibleRequirements.Count != 0 && SharedGameResources.Camp!.CheckRequirementsInVector(Stats.InvincibleRequirements))
                    return false;
            }

            //if the target is an enemy and they are not already in combat, activate a beacon to draw other enemies into battle
            if (!Stats.InCombat && !Stats.Hero && !Stats.HeroAlly && !h.HazardPower!.NoAggro)
            {
                Stats.JoinCombat = true;
            }

            // exit if it was a beacon (to prevent stats.targeted from being set)
            if (h.HazardPower!.Beacon) return false;

            if (h.HazardPower!.Type == Power.TypeMissile && MathUtils.PercentChanceF(Stats.Get(global::FlareEngine.Stats.Reflect)))
            {
                // reflect the missile 180 degrees
                h.SetAngle(h.Angle + (float)MathF.PI);

                // change hazard source to match the reflector's type
                // maybe we should change the source stats pointer to the reflector's StatBlock
                if (h.SourceType == Power.SourceTypeHero || h.SourceType == Power.SourceTypeAlly)
                    h.SourceType = Power.SourceTypeEnemy;
                else if (h.SourceType == Power.SourceTypeEnemy)
                    h.SourceType = Stats.Hero ? Power.SourceTypeHero : Power.SourceTypeAlly;

                // reset the hazard ticks
                h.Lifespan = h.HazardPower!.Lifespan;

                if (ActiveAnimation!.Name == "block")
                {
                    PlaySound(Entity.SoundBlock);
                }

                return false;
            }

            // if it's a miss, do nothing
            float accuracy = h.Accuracy;
            if (h.HazardPower!.ModAccuracyMode == Power.StatModifierModeMultiply)
                accuracy = (accuracy * h.HazardPower!.ModAccuracyValue) / 100;
            else if (h.HazardPower!.ModAccuracyMode == Power.StatModifierModeAdd)
                accuracy += h.HazardPower!.ModAccuracyValue;
            else if (h.HazardPower!.ModAccuracyMode == Power.StatModifierModeAbsolute)
                accuracy = h.HazardPower!.ModAccuracyValue;

            float avoidance = 0;
            if (!h.HazardPower!.TraitAvoidanceIgnore)
            {
                avoidance = Stats.Get(global::FlareEngine.Stats.Avoidance);
            }

            float trueAvoidance = 100 - (accuracy - avoidance);
            bool isOverhit = (trueAvoidance < 0 && !h.SrcStats!.PerfectAccuracy) ? MathUtils.PercentChanceF(MathF.Abs(trueAvoidance)) : false;
            trueAvoidance = Math.Min(Math.Max(trueAvoidance, eset.Combat.MinAvoidance), eset.Combat.MaxAvoidance);

            bool missed = false;
            if (!h.SrcStats!.PerfectAccuracy && MathUtils.PercentChanceF(trueAvoidance))
            {
                missed = true;
            }

            // calculate base damage
            float dmg = 0;

            for (int i = 0; i < h.Damage.Count; ++i)
            {
                float dmgPart = MathUtils.RandBetweenF(h.Damage[i].Min, h.Damage[i].Max);

                if ((i == h.HazardPower!.BaseDamage && h.HazardPower!.ConvertedDamage == h.Damage.Count) || i == h.HazardPower!.ConvertedDamage)
                {
                    // power damage modifiers are only applied to the base damage (or converted damage if that applies)
                    if (h.HazardPower!.ModDamageMode == Power.StatModifierModeMultiply)
                        dmgPart = dmgPart * h.HazardPower!.ModDamageValueMin / 100;
                    else if (h.HazardPower!.ModDamageMode == Power.StatModifierModeAdd)
                        dmgPart += h.HazardPower!.ModDamageValueMin;
                    else if (h.HazardPower!.ModDamageMode == Power.StatModifierModeAbsolute)
                        dmgPart = MathUtils.RandBetweenF(h.HazardPower!.ModDamageValueMin, h.HazardPower!.ModDamageValueMax);
                }
                
                dmg += Stats.ApplyResistToDamage(i, dmgPart);
            }

            if (!h.HazardPower!.TraitArmorPenetration)
            {
                // subtract absorption from armor
                float absorption = MathUtils.RandBetweenF(Stats.Get(global::FlareEngine.Stats.AbsMin), Stats.Get(global::FlareEngine.Stats.AbsMax));

                if (absorption > 0 && dmg > 0)
                {
                    float baseAbsorb = absorption;
                    if (Stats.Effects.TriggeredBlock)
                    {
                        if ((baseAbsorb * 100) / dmg < eset.Combat.MinBlock)
                            absorption = (dmg * eset.Combat.MinBlock) / 100;
                        if ((baseAbsorb * 100) / dmg > eset.Combat.MaxBlock)
                            absorption = (dmg * eset.Combat.MaxBlock) / 100;
                    }
                    else
                    {
                        if ((baseAbsorb * 100) / dmg < eset.Combat.MinAbsorb)
                            absorption = (dmg * eset.Combat.MinAbsorb) / 100;
                        if ((baseAbsorb * 100) / dmg > eset.Combat.MaxAbsorb)
                            absorption = (dmg * eset.Combat.MaxAbsorb) / 100;
                    }

                    // Sometimes, the absorb limits cause absorption to drop to 1
                    // This could be confusing to a player that has something with an absorb of 1 equipped
                    // So we round absorption up in this case
                    if (absorption == 0) absorption = 1;
                }

                dmg = dmg - absorption;
                if (dmg <= 0)
                {
                    dmg = 0;
                    if (!h.HazardPower!.IgnoreZeroDamage)
                    {
                        if (Stats.Effects.TriggeredBlock && eset.Combat.MaxBlock < 100)
                            dmg = 1;
                        else if (!Stats.Effects.TriggeredBlock && eset.Combat.MaxAbsorb < 100)
                            dmg = 1;

                        if (ActiveAnimation!.Name == "block")
                        {
                            PlaySound(Entity.SoundBlock);
                            ResetActiveAnimation();
                        }
                    }
                }
            }

            // check for crits
            float trueCritChance = h.CritChance;

            if (h.HazardPower!.ModCritMode == Power.StatModifierModeMultiply)
                trueCritChance = trueCritChance * h.HazardPower!.ModCritValue / 100;
            else if (h.HazardPower!.ModCritMode == Power.StatModifierModeAdd)
                trueCritChance += h.HazardPower!.ModCritValue;
            else if (h.HazardPower!.ModCritMode == Power.StatModifierModeAbsolute)
                trueCritChance = h.HazardPower!.ModCritValue;

            if (Stats.Effects.Stun || Stats.Effects.Speed < 100)
                trueCritChance += h.HazardPower!.TraitCritsImpaired;

            bool crit = MathUtils.PercentChanceF(trueCritChance);
            if (crit)
            {
                // default is dmg * 2
                dmg = (dmg * MathUtils.RandBetweenF(eset.Combat.MinCritDamage, eset.Combat.MaxCritDamage)) / 100;
                if (!Stats.Hero)
                {
                    mapr.Cam.ShakeTimer.Duration = (uint)(settings.MaxFramesPerSec / 2);
                    inpt.JoystickRumble(InputState.JoystickRumbleStrength, InputState.JoystickRumbleStrength, 500);
                }
            }
            else if (isOverhit)
            {
                dmg = (dmg * MathUtils.RandBetweenF(eset.Combat.MinOverhitDamage, eset.Combat.MaxOverhitDamage)) / 100;
                // Should we use shakycam for overhits?
            }

            // misses cause reduced damage
            if (missed)
            {
                dmg = (dmg * MathUtils.RandBetweenF(eset.Combat.MinMissDamage, eset.Combat.MaxMissDamage)) / 100;
            }

            dmg = eset.Combat.ResourceRound(dmg);

            if (!h.HazardPower!.IgnoreZeroDamage)
            {
                if (dmg == 0)
                {
                    comb.AddString(msg.Get("miss"), Stats.Pos, CombatText.MsgMiss);
                    return false;
                }
                else if (Stats.Hero)
                    comb.AddFloat(dmg, Stats.Pos, CombatText.MsgTakedmg);
                else
                {
                    if (crit || isOverhit)
                        comb.AddFloat(dmg, Stats.Pos, CombatText.MsgCrit);
                    else if (missed)
                        comb.AddFloat(dmg, Stats.Pos, CombatText.MsgMiss);
                    else
                        comb.AddFloat(dmg, Stats.Pos, CombatText.MsgGivedmg);
                }
            }

            // temporarily save the current HP for calculating HP/MP steal on final blow
            float prevHp = Stats.Hp;

            // save debuff status to check for on_debuff powers later
            bool wasDebuffed = Stats.Effects.IsDebuffed();

            // apply damage
            Stats.TakeDamage(dmg, crit, h.SourceType);

            // after effects
            if (dmg > 0 || h.HazardPower!.IgnoreZeroDamage)
            {

                // damage always breaks stun
                Stats.Effects.RemoveEffectType(Effect.Stun);

                SharedGameResources.Powers!.Effect(Stats, h.SrcStats!, h.PowerIndex, h.SourceType);

                // HP/MP steal is cumulative between stat bonus and power bonus
                if (h.SrcStats!.Hp > 0)
                {
                    float hpSteal = h.HazardPower!.HpSteal + h.SrcStats!.Get(global::FlareEngine.Stats.HpSteal);
                    if (hpSteal != 0)
                    {
                        if (MathUtils.PercentChanceF(Stats.Get(global::FlareEngine.Stats.ResistHpSteal)))
                        {
                            comb.AddString(msg.Get("Resist"), Stats.Pos, CombatText.MsgMiss);
                        }
                        else
                        {
                            float stealAmt = (Math.Min(dmg, prevHp) * hpSteal) / 100;
                            stealAmt = eset.Combat.ResourceRound(stealAmt);
                            if (stealAmt >= 1 || eset.NumberFormat.CombatText > 0)
                            {
                                comb.AddString(msg.GetV("+%s HP", Utils.FloatToString(stealAmt, eset.NumberFormat.CombatText)), h.SrcStats!.Pos, CombatText.MsgBuff);
                            }
                            h.SrcStats!.Hp = Math.Min(h.SrcStats!.Hp + stealAmt, h.SrcStats!.Get(global::FlareEngine.Stats.HpMax));
                        }
                    }
                    float mpSteal = h.HazardPower!.MpSteal + h.SrcStats!.Get(global::FlareEngine.Stats.MpSteal);
                    if (mpSteal != 0)
                    {
                        if (MathUtils.PercentChanceF(Stats.Get(global::FlareEngine.Stats.ResistMpSteal)))
                        {
                            comb.AddString(msg.Get("Resist"), Stats.Pos, CombatText.MsgMiss);
                        }
                        else
                        {
                            float stealAmt = (Math.Min(dmg, prevHp) * mpSteal) / 100;
                            stealAmt = eset.Combat.ResourceRound(stealAmt);
                            if (stealAmt >= 1 || eset.NumberFormat.CombatText > 0)
                            {
                                comb.AddString(msg.GetV("+%s MP", Utils.FloatToString(stealAmt, eset.NumberFormat.CombatText)), h.SrcStats!.Pos, CombatText.MsgBuff);
                            }
                            h.SrcStats!.Mp = Math.Min(h.SrcStats!.Mp + stealAmt, h.SrcStats!.Get(global::FlareEngine.Stats.MpMax));
                        }
                    }
                    for (int i = 0; i < h.SrcStats!.ResourceStats.Count; ++i)
                    {
                        float resourceSteal = h.HazardPower!.ResourceSteal[i] + h.SrcStats!.GetResourceStat(i, EngineSettings.ResourceStatsSettings.StatSteal);
                        if (resourceSteal != 0)
                        {
                            if (MathUtils.PercentChanceF(Stats.GetResourceStat(i, EngineSettings.ResourceStatsSettings.StatResistSteal)))
                            {
                                comb.AddString(msg.Get("Resist"), Stats.Pos, CombatText.MsgMiss);
                            }
                            else
                            {
                                float stealAmt = (Math.Min(dmg, prevHp) * resourceSteal) / 100;
                                stealAmt = eset.Combat.ResourceRound(stealAmt);
                                comb.AddString("+" + Utils.FloatToString(stealAmt, eset.NumberFormat.CombatText) + " " + eset.ResourceStats.Stats[i].TextCombatHeal, h.SrcStats!.Pos, CombatText.MsgBuff);
                                h.SrcStats!.ResourceStats[i] = Math.Min(h.SrcStats!.ResourceStats[i] + stealAmt, h.SrcStats!.GetResourceStat(i, EngineSettings.ResourceStatsSettings.StatBase));
                            }
                        }
                    }
                }

                // deal return damage
                if (Stats.Get(global::FlareEngine.Stats.ReturnDamage) > 0)
                {
                    float dmgReturn = (dmg * Stats.Get(global::FlareEngine.Stats.ReturnDamage)) / 100f;
                    dmgReturn = eset.Combat.ResourceRound(dmgReturn);
                    if (dmgReturn > 0)
                    {
                        if (MathUtils.PercentChanceF(h.SrcStats!.Get(global::FlareEngine.Stats.ResistDamageReflect)))
                        {
                            comb.AddString(msg.Get("Resist"), Stats.Pos, CombatText.MsgMiss);
                        }
                        else
                        {
                            // swap the source type when dealing return damage
                            int returnSourceType = Power.SourceTypeNeutral;
                            if (h.SourceType == Power.SourceTypeHero || h.SourceType == Power.SourceTypeAlly)
                                returnSourceType = Power.SourceTypeEnemy;
                            else if (h.SourceType == Power.SourceTypeEnemy)
                                returnSourceType = Stats.Hero ? Power.SourceTypeHero : Power.SourceTypeAlly;

                            h.SrcStats!.TakeDamage(dmgReturn, !StatBlock.TakeDmgCrit, returnSourceType);
                            comb.AddFloat(dmgReturn, h.SrcStats!.Pos, CombatText.MsgGivedmg);
                        }
                    }
                }
            }

            if (dmg > 0 || h.HazardPower!.IgnoreZeroDamage)
            {
                if (!Stats.HeroAlly)
                {
                    // flag the enemy as encountered so we can start running it's state logic
                    // this fixes the problem where an enemy would die off screen, but wouldn't enter their dying animation until the player approached them
                    Stats.Encountered = true;
                }

                // remove effect by ID
                Stats.Effects.RemoveEffectID(h.HazardPower!.RemoveEffects);

                // post power
                for (int i = 0; i < h.HazardPower!.ChainPowers.Count; ++i)
                {
                    ChainPower chainPower = h.HazardPower!.ChainPowers[i];
                    if (chainPower.Type == ChainPower.TypePost && MathUtils.PercentChanceF(chainPower.Chance))
                    {
                        int hazardCount = hazards.H.Count;
                        if (h.HazardPower!.PostHazardsSkipTarget)
                        {
                            // calling this here clears the powers->hazards queue
                            // it's important that we clear the queue first
                            // we'll be using it to determine which hazards are added by the post power
                            hazards.CheckNewHazards();
                        }

                        powers.Activate(chainPower.Id, h.SrcStats!, h.Pos, Stats.Pos);

                        if (h.HazardPower!.PostHazardsSkipTarget)
                        {
                            // populate powers->hazards with any new hazards created by the post power
                            hazards.CheckNewHazards();
                            if (hazards.H.Count > hazardCount)
                            {
                                for (uint j = (uint)hazardCount - 1; j < (uint)hazards.H.Count; ++j)
                                {
                                    hazards.H[(int)j].AddEntity(this);
                                }
                            }
                        }
                    }
                }
            }

            // interrupted to new state
            if (dmg > 0)
            {
                if (Stats.Hero)
                {
                    Stats.AbortNpcInteract = true;
                }

                // entity is dead, no need to contine
                if (Stats.Hp <= 0)
                    return true;

                // play hit sound effect, but only if the hit cooldown is done
                if (Stats.CooldownHit.IsEnd())
                    PlaySound(Entity.SoundHit);

                if (Behavior != null)
                {
                    // if this hit caused a debuff, activate an on_debuff power
                    if (!wasDebuffed && Stats.Effects.IsDebuffed())
                    {
                        StatBlock.AIPower? aiPower = Stats.GetAIPower(StatBlock.AiPowerDebuff);
                        if (aiPower != null)
                        {
                            Stats.AiDebuffPower = aiPower;
                        }
                    }

                    // roll to see if the enemy's ON_HIT power is casted
                    StatBlock.AIPower? aiPowerHit = Stats.GetAIPower(StatBlock.AiPowerHit);
                    if (aiPowerHit != null)
                    {
                        Stats.AiHitPower = aiPowerHit;
                    }
                }

                // don't go through a hit animation if stunned or successfully poised
                // however, critical hits ignore poise
                bool chancePoise = MathUtils.PercentChanceF(Stats.Get(global::FlareEngine.Stats.Poise));

                if (Stats.CooldownHit.IsEnd())
                {
                    Stats.CooldownHit.Reset(Timer.Begin);

                    if (!Stats.Effects.Stun && (!chancePoise || crit) && !Stats.PreventInterrupt)
                    {
                        if (Stats.Hero)
                        {
                            Stats.CurState = StatBlock.EntityHit;
                        }
                        else
                        {
                            if (Stats.CurState == StatBlock.EntityPower)
                            {
                                Stats.Cooldown.Reset(Timer.Begin);
                                Stats.ActivatedPower = null;
                            }
                            Stats.CurState = StatBlock.EntityHit;
                        }

                        if (Stats.UntransformOnHit)
                            Stats.TransformDuration = 0;
                    }
                }

                // handle block post-power
                if (powers.IsValid(Stats.BlockPower))
                {
                    Power blockPower = powers.Powers[Stats.BlockPower]!;
                    for (int i = 0; i < blockPower.ChainPowers.Count; ++i)
                    {
                        ChainPower chainPower = blockPower.ChainPowers[i];
                        if (chainPower.Type == ChainPower.TypePost && Stats.GetPowerCooldown(chainPower.Id) == 0 && MathUtils.PercentChanceF(chainPower.Chance))
                        {
                            powers.Activate(chainPower.Id, Stats, Stats.Pos, Stats.Pos);
                            Stats.SetPowerCooldown(chainPower.Id, powers.Powers[chainPower.Id]!.Cooldown);
                        }
                    }
                }
            }

            return true;
        }

        public void ResetActiveAnimation()
        {
            if (ActiveAnimation != null)
                ActiveAnimation.Reset();

            for (int i = 0; i < Animsets.Count; ++i)
                if (Anims[i] != null)
                    Anims[i]!.Reset();
        }

        /// <summary>
        /// Set the entity's current animation by name
        /// </summary>
        public void SetAnimation(string animationName)
        {

            // if the animation is already the requested one do nothing
            if (ActiveAnimation != null && ActiveAnimation.Name == animationName)
                return;

            if (AnimationSet == null)
                return;

            ActiveAnimation?.Dispose();
            ActiveAnimation = AnimationSet.GetAnimation(animationName);

            if (ActiveAnimation == null)
                Utils.LogError("Entity::setAnimation(%s): not found", animationName);

            for (int i = 0; i < Animsets.Count; ++i)
            {
                Anims[i]?.Dispose();
                if (Animsets[i] != null)
                    Anims[i] = Animsets[i]!.GetAnimation(animationName);
                else
                    Anims[i] = null;
            }
        }

        /// <summary>
        /// The current direction leads to a wall.  Try the next best direction, if one is available.
        /// </summary>
        public byte FaceNextBest(float mapx, float mapy)
        {
            float dx = (float)MathF.Abs(mapx - Stats.Pos.X);
            float dy = (float)MathF.Abs(mapy - Stats.Pos.Y);
            switch (Stats.Direction)
            {
                case 0:
                    if (dy > dx) return 7;
                    else return 1;
                case 1:
                    if (mapy > Stats.Pos.Y) return 0;
                    else return 2;
                case 2:
                    if (dx > dy) return 1;
                    else return 3;
                case 3:
                    if (mapx < Stats.Pos.X) return 2;
                    else return 4;
                case 4:
                    if (dy > dx) return 3;
                    else return 5;
                case 5:
                    if (mapy < Stats.Pos.Y) return 4;
                    else return 6;
                case 6:
                    if (dx > dy) return 5;
                    else return 7;
                case 7:
                    if (mapx > Stats.Pos.X) return 6;
                    else return 0;
            }
            return 0;
        }

        public Rectangle GetRenderBounds(Vector2 cam)
        {
            Rectangle r = default;
            Int2 p = Utils.MapToScreen(Stats.Pos.X, Stats.Pos.Y, cam.X, cam.Y);

            if (Stats.LayerReferenceOrder.Count != 0)
            {
                Int2 topLeft = default;
                Int2 bottomRight = default;
                bool pointInit = false;
                for (int i = 0; i < Stats.LayerDef[Stats.Direction].Count; ++i)
                {
                    int index = (int)Stats.LayerDef[Stats.Direction][i];
                    if (Anims[index] != null)
                    {
                        Renderable ren = Anims[index]!.GetCurrentFrame(Stats.Direction);
                        if (!pointInit)
                        {
                            topLeft.X = p.X - ren.Offset.X;
                            topLeft.Y = p.Y - ren.Offset.Y;
                            bottomRight.X = topLeft.X + ren.Src.Width;
                            bottomRight.Y = topLeft.Y + ren.Src.Height;
                            pointInit = true;
                        }
                        else
                        {
                            Int2 layerTopLeft = new Int2(p.X - ren.Offset.X, p.Y - ren.Offset.Y);
                            Int2 layerBottomRight = new Int2(layerTopLeft.X + ren.Src.Width, layerTopLeft.Y + ren.Src.Height);

                            if (layerTopLeft.X < topLeft.X)
                                topLeft.X = layerTopLeft.X;
                            if (layerTopLeft.Y < topLeft.Y)
                                topLeft.Y = layerTopLeft.Y;
                            if (layerBottomRight.X > bottomRight.X)
                                bottomRight.X = layerBottomRight.X;
                            if (layerBottomRight.Y > bottomRight.Y)
                                bottomRight.Y = layerBottomRight.Y;
                        }
                    }
                }

                if (pointInit)
                {
                    r.X = topLeft.X;
                    r.Y = topLeft.Y;
                    r.Width = bottomRight.X - topLeft.X;
                    r.Height = bottomRight.Y - topLeft.Y;
                }
            }
            else
            {
                if (ActiveAnimation != null)
                {
                    Renderable ren = ActiveAnimation.GetCurrentFrame(Stats.Direction);
                    r.X = p.X - ren.Offset.X;
                    r.Y = p.Y - ren.Offset.Y;
                    r.Width = ren.Src.Width;
                    r.Height = ren.Src.Height;
                }
            }

            return r;
        }

        public void AddRenders(List<Renderable> r)
        {
            var eset = SharedResources.Eset!;
            var settings = SharedResources.Settings!;
            var mapr = SharedGameResources.Mapr;
            if (mapr != null && mapr.Collider.IsOutsideMap(Stats.Pos.X, Stats.Pos.Y))
                return;

            if (Stats.LayerReferenceOrder.Count != 0)
            {
                for (int i = 0; i < Stats.LayerDef[Stats.Direction].Count; ++i)
                {
                    int index = (int)Stats.LayerDef[Stats.Direction][i];
                    if (Anims[index] != null)
                    {
                        Renderable ren = Anims[index]!.GetCurrentFrame(Stats.Direction);
                        ren.MapPos = Stats.Pos;
                        ren.Prio = (ulong)(i + 1);

                        Stats.Effects.GetCurrentColor(ref ren.ColorMod);
                        Stats.Effects.GetCurrentAlpha(ref ren.AlphaMod);

                        // fade out corpses
                        if (!Stats.Hero && Stats.Corpse && Stats.CorpseHasTimeout)
                        {
                            uint fadeTime = (eset.Misc.CorpseTimeout > (int)settings.MaxFramesPerSec) ? settings.MaxFramesPerSec : (uint)eset.Misc.CorpseTimeout;
                            if (fadeTime != 0 && Stats.CorpseTimer.Current <= fadeTime)
                            {
                                ren.AlphaMod = (byte)((float)Stats.CorpseTimer.Current * (ren.AlphaMod / (float)fadeTime));
                            }
                        }

                        ren.Type = GetRenderableType();

                        r.Add(ren);
                    }
                }
            }
            else
            {
                Renderable ren = new Renderable();
                if (ActiveAnimation != null)
                    ren = ActiveAnimation.GetCurrentFrame(Stats.Direction);
                ren.MapPos = Stats.Pos;
                ren.Prio = 1;

                Stats.Effects.GetCurrentColor(ref ren.ColorMod);
                Stats.Effects.GetCurrentAlpha(ref ren.AlphaMod);

                // fade out corpses
                if (!Stats.Hero && Stats.Corpse && Stats.CorpseHasTimeout)
                {
                    uint fadeTime = (eset.Misc.CorpseTimeout > (int)settings.MaxFramesPerSec) ? settings.MaxFramesPerSec : (uint)eset.Misc.CorpseTimeout;
                    if (fadeTime != 0 && Stats.CorpseTimer.Current <= fadeTime)
                    {
                        ren.AlphaMod = (byte)((float)Stats.CorpseTimer.Current * (ren.AlphaMod / (float)fadeTime));
                    }
                }

                ren.Type = GetRenderableType();

                r.Add(ren);
            }

            // add effects
            for (int i = 0; i < Stats.Effects.EffectList.Count; ++i)
            {
                if (Stats.Effects.EffectList[i].EffectAnimation != null && !Stats.Effects.EffectList[i].EffectAnimation!.IsCompleted())
                {
                    Renderable ren = Stats.Effects.EffectList[i].EffectAnimation!.GetCurrentFrame(0);
                    ren.MapPos = Stats.Pos;
                    if (Stats.Effects.EffectList[i].RenderAbove)
                    {
                        if (Stats.LayerReferenceOrder.Count != 0)
                            ren.Prio = (ulong)(Stats.LayerDef[Stats.Direction].Count + 1);
                        else
                            ren.Prio = 2;
                    }
                    else
                    {
                        ren.Prio = 0;
                    }
                    r.Add(ren);
                }
            }
        }

        private byte GetRenderableType()
        {
            if (Stats.Hp > 0)
            {
                if (Stats.Hero)
                    return Renderable.TypeHero;
                else if (Stats.HeroAlly)
                    return Renderable.TypeAlly;
                else if (Stats.InCombat)
                    return Renderable.TypeEnemy;
            }

            return Renderable.TypeNormal;
        }

        public void LoadAnimations()
        {
            var anim = SharedResources.Anim!;

            // load the base animation
            if (AnimationSet == null)
            {
                if (Stats.Animations != "")
                {
                    anim.IncreaseCount(Stats.Animations);
                    AnimationSet = anim.GetAnimationSet(Stats.Animations);
                    if (ActiveAnimation != null)
                        ActiveAnimation.Dispose();
                    ActiveAnimation = AnimationSet!.GetAnimation("");
                }
            }

            for (int i = 0; i < Animsets.Count; ++i)
            {
                if (Animsets[i] != null)
                    anim.DecreaseCount(Animsets[i]!.Name);
                Anims[i]?.Dispose();
            }
            Animsets.Clear();
            Anims.Clear();

            List<LayerGfx> imgGfx = new List<LayerGfx>();

            for (int i = 0; i < Stats.LayerReferenceOrder.Count; ++i)
            {
                LayerGfx gfx = new LayerGfx();
                gfx.Type = Stats.LayerReferenceOrder[i];
                gfx.Gfx = GetGfxFromType(gfx.Type);
                imgGfx.Add(gfx);
            }
            Debug.Assert(Stats.LayerReferenceOrder.Count == imgGfx.Count);

            for (int i = 0; i < imgGfx.Count; ++i)
            {
                if (imgGfx[i].Gfx != "")
                {
                    string name;
                    if (Stats.Hero && !Stats.Transformed)
                        name = "animations/avatar/" + Stats.GfxBase + "/" + imgGfx[i].Gfx + ".txt";
                    else
                        name = imgGfx[i].Gfx;

                    anim.IncreaseCount(name);
                    Animsets.Add(anim.GetAnimationSet(name));
                    Animsets[^1]!.Parent = AnimationSet;
                    Anims.Add(Animsets[^1]!.GetAnimation(ActiveAnimation!.Name));
                    SetAnimation("stance");
                    if (!Anims[^1]!.SyncTo(ActiveAnimation))
                    {
                        Utils.LogError("Entity: Error syncing animation in '%s' to parent animation.", Animsets[^1]!.Name);
                    }
                }
                else
                {
                    Animsets.Add(null);
                    Anims.Add(null);
                }
            }
            SharedResources.Anim!.CleanUp();

            Stats.CritdieEnabled = false;
            if (AnimationSet != null)
            {
                Animation? critdieAnim = AnimationSet.GetAnimation("critdie");
                if (critdieAnim != null)
                {
                    Stats.CritdieEnabled = (critdieAnim.Name == "critdie");
                    critdieAnim.Dispose();
                }
            }

            if (Stats.Hero)
            {
                // set cooldown_hit to duration of hit animation if undefined
                if (!Stats.CooldownHitEnabled)
                {
                    Animation? hitAnim = AnimationSet!.GetAnimation("hit");
                    if (hitAnim != null)
                    {
                        Stats.CooldownHit.Duration = (uint)hitAnim.GetDuration();
                        hitAnim.Dispose();
                    }
                    else
                    {
                        Stats.CooldownHit.Duration = 0;
                    }
                }
            }
        }

        public virtual string GetGfxFromType(string gfxType)
        {
            if (Stats.AnimationSlots.TryGetValue(gfxType, out string? value))
                return value;

            return "";
        }
    }
}
