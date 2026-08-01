// 对应 C++ 源文件：Hazard.h + Hazard.cpp
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// Hazard
    ///
    /// 可以对英雄或生物造成伤害的独立对象（对应原始注释："Stand-alone object that can
    /// harm the hero or creatures"）。每当某个攻击发生时都会生成一个 Hazard 实例。
    ///
    /// 前向引用说明（需人工复核，详见转换报告"依赖关系"部分）：
    /// <see cref="Animation"/>、<see cref="AnimationSet"/>、<see cref="AnimationManager"/>、
    /// <see cref="MapCollision"/>、<see cref="Power"/>、<see cref="PowerManager"/>、
    /// <see cref="RenderDevice"/>、<see cref="Renderable"/>、<see cref="StatBlock"/>、
    /// <see cref="Entity"/> 均尚未转换为对应的 C# 单元，这里按其原始 C++ 声明（字段/方法名
    /// 直译为 PascalCase）前向引用，实际定义将在各自的转换单元中给出。
    ///
    /// 命名说明（沿用 output/EffectManager.cs 中 Effect.EffectTimer / Effect.EffectAnimation
    /// 的处理约定）：原始字段 <c>power</c>（类型 <c>Power*</c>）按规则直译 PascalCase 后会得到
    /// <c>Power</c>——与它自身的类型名 <see cref="Power"/> 完全相同。为避免在类型与实例成员之间
    /// 产生歧义（尤其是未来其他单元里可能出现的 <c>Power.SOURCE_TYPE_*</c> 等静态成员访问），
    /// 这里改为对字段本身重命名为 <see cref="HazardPower"/>，而不是修改 <c>Power</c> 类型名本身。
    ///
    /// 资源管理：<see cref="_activeAnimation"/> 对应原始裸指针 <c>activeAnimation</c>，其生命周期
    /// 由 <see cref="LoadAnimation"/> / <see cref="Dispose"/> 手动管理（构造时为 NULL，
    /// <c>delete</c> 时释放），因此本类型实现 <see cref="IDisposable"/>，<see cref="Dispose"/>
    /// 对应原始析构函数 <c>~Hazard()</c>，其中"孤儿子代重新挂到兄弟节点""从父节点子列表中移除自身"
    /// 的顺序与原始实现逐行一致。
    /// </summary>
    public class Hazard : IDisposable
    {
        public bool Active;
        public bool RemoveNow;
        public bool HitWall;
        public bool RelativePos;
        public bool SfxHitPlayed;

        public List<FMinMax> Damage = new List<FMinMax>();
        public float CritChance;
        public float Accuracy;
        public int SourceType;
        public float BaseSpeed;
        public int Lifespan; // 倒计时至零
        public ushort Direction; // 既可以是方向，也可以是选项/随机数
        public int DelayFrames;
        public float Angle; // 弧度制

        public StatBlock? SrcStats;
        /// <summary>对应原始字段 <c>power</c>（见类级注释中的重命名说明）。</summary>
        public Power? HazardPower;
        public PowerID PowerIndex;

        public Vector2 Pos;
        public Vector2 Speed;
        public Vector2 PosOffset;

        // 用于将多个 hazard 链接在一起，例如 repeater（连锁攻击）
        public Hazard? Parent;
        public List<Hazard> Children = new List<Hazard>();

        public Vector2 PrevPos;

        private MapCollision? _collider;
        private Animation? _activeAnimation;
        private string _animationName = "";

        // 记录已经命中过的实体
        private List<Entity> _entitiesCollided = new List<Entity>();

        public Hazard(MapCollision? collider)
        {
            Active = true;
            RemoveNow = false;
            HitWall = false;
            RelativePos = false;
            SfxHitPlayed = false;

            int damageCount = SharedResources.Eset!.DamageTypes.Types.Count;
            Damage = new List<FMinMax>(damageCount);
            for (int i = 0; i < damageCount; ++i)
            {
                Damage.Add(new FMinMax());
            }
            CritChance = 0;
            Accuracy = 0;
            SourceType = 0;
            BaseSpeed = 0;
            Lifespan = 1;
            Direction = 0;
            DelayFrames = 0;
            Angle = 0;

            SrcStats = null;
            HazardPower = null;
            PowerIndex = 0;

            Pos = default;
            Speed = default;
            PosOffset = default;

            Parent = null;
            Children = new List<Hazard>();

            PrevPos = default;

            _collider = collider;
            _activeAnimation = null;
            _animationName = "";

            _entitiesCollided = new List<Entity>();
        }

        /// <summary>
        /// 对应拷贝构造函数 <c>Hazard(const Hazard&amp; other)</c>：先将动画指针置空，
        /// 再委托给 <see cref="CopyFrom"/>（对应原始实现委托给 <c>operator=</c>）。
        /// </summary>
        public Hazard(Hazard other)
        {
            _activeAnimation = null;
            CopyFrom(other);
        }

        /// <summary>
        /// 对应赋值运算符 <c>Hazard&amp; operator=(const Hazard&amp; other)</c>。
        ///
        /// 需人工复核：与原始实现完全一致地保留了以下细节——当 <c>other._animationName</c>
        /// 非空时，先把 <c>_animationName</c> 覆盖为该值，然后才调用 <see cref="LoadAnimation"/>；
        /// 由于 <see cref="LoadAnimation"/> 内部是根据（此时已经被覆盖过的）<c>_animationName</c>
        /// 去执行"先减引用计数再加引用计数"的逻辑，实际减少的是新动画名的引用计数而不是旧动画名的，
        /// 这是原始 C++ 代码本身的行为（可能是潜在缺陷），本次转换不做修正，仅逐行保留。
        /// </summary>
        public Hazard CopyFrom(Hazard other)
        {
            if (ReferenceEquals(this, other))
                return this;

            Active = other.Active;
            RemoveNow = other.RemoveNow;
            HitWall = other.HitWall;
            RelativePos = other.RelativePos;
            SfxHitPlayed = other.SfxHitPlayed;

            // 需人工复核：output/Utils.cs 中 FMinMax 被定义为 class（引用类型），而 C++ 原始
            // FMinMax 是按值拷贝的简单结构体；此处逐行对应 `damage[i] = other.damage[i];`，
            // 但其效果是让 this.Damage[i] 与 other.Damage[i] 共享同一个 FMinMax 实例（别名），
            // 而不是像原始代码那样得到两个独立、取值相同的对象。这是继承自既有 Utils.cs
            // 类型选择的偏差，本单元不做修正，仅如实转写。
            Resize(Damage, other.Damage.Count);
            for (int i = 0; i < Damage.Count; ++i)
            {
                Damage[i] = other.Damage[i];
            }
            CritChance = other.CritChance;
            Accuracy = other.Accuracy;
            SourceType = other.SourceType;
            BaseSpeed = other.BaseSpeed;
            Lifespan = other.Lifespan;
            Direction = other.Direction;
            DelayFrames = other.DelayFrames;
            Angle = other.Angle;

            SrcStats = other.SrcStats;
            HazardPower = other.HazardPower;
            PowerIndex = other.PowerIndex;

            Pos = other.Pos;
            Speed = other.Speed;
            PosOffset = other.PosOffset;

            Parent = other.Parent;
            Children = new List<Hazard>(other.Children);

            if (!string.IsNullOrEmpty(other._animationName))
            {
                _animationName = other._animationName;
                LoadAnimation(_animationName);
            }

            _collider = other._collider;
            _entitiesCollided = new List<Entity>(other._entitiesCollided);

            return this;
        }

        /// <summary>
        /// 对应析构函数 <c>~Hazard()</c>：
        /// 1) 若本节点没有父节点但有子节点，把第一个子节点提升为新的父节点，其余子节点改挂到新父节点下，
        ///    并把本节点已记录的"已命中实体"转交给新父节点；
        /// 2) 否则若本节点有父节点，从父节点的子节点列表中移除自身；
        /// 3) 释放动画引用计数与动画对象；
        /// 4) 触发一次动画管理器清理。
        /// 以上四步的先后顺序与原始实现逐行一致。
        /// </summary>
        public void Dispose()
        {
            var anim = SharedResources.Anim!;
            if (Parent == null && Children.Count != 0)
            {
                // 把下一个子节点提升为现有子节点们的父节点
                Hazard newParent = Children[0];
                newParent.Parent = null;

                for (int i = 1; i < Children.Count; ++i)
                {
                    Children[i].Parent = newParent;
                    newParent.Children.Add(Children[i]);
                }

                for (int i = 0; i < _entitiesCollided.Count; ++i)
                {
                    newParent.AddEntity(_entitiesCollided[i]);
                }
            }
            else if (Parent != null)
            {
                // 从父节点的子节点列表中移除本节点
                for (int i = 0; i < Parent.Children.Count; ++i)
                {
                    if (ReferenceEquals(Parent.Children[i], this))
                    {
                        Parent.Children.RemoveAt(i);
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(_animationName))
            {
                anim.DecreaseCount(_animationName);
            }

            if (_activeAnimation != null)
            {
                _activeAnimation.Dispose();
                _activeAnimation = null;
            }

            anim.CleanUp();
        }

        public void Logic()
        {
            // 若该 hazard 正处于延迟状态，则不执行任何动作
            if (DelayFrames > 0)
            {
                DelayFrames--;
                return;
            }

            // 处理计时器
            if (Lifespan > 0) Lifespan--;

            if (HazardPower!.ExpireWithCaster && !SrcStats!.Alive)
                Lifespan = 0;

            if (_activeAnimation != null)
                _activeAnimation.AdvanceFrame();

            PrevPos = Pos;

            // 处理移动
            bool checkCollide = false;
            if (!(Speed.X == 0 && Speed.Y == 0))
            {
                Pos.X += Speed.X;
                Pos.Y += Speed.Y;
                checkCollide = true;
            }
            else if (!(PosOffset.X == 0 && PosOffset.Y == 0))
            {
                Pos.X = SrcStats!.Pos.X - PosOffset.X;
                Pos.Y = SrcStats!.Pos.Y - PosOffset.Y;
                checkCollide = true;
            }
            else if (RelativePos)
            {
                Pos.X = SrcStats!.Pos.X;
                Pos.Y = SrcStats!.Pos.Y;
            }

            if (checkCollide && _collider != null)
            {
                // 非常简化的碰撞体，可能会在角落处打滑
                // 甚至在速度超过地块尺寸时穿过较薄的墙壁
                if (!_collider.IsValidPosition(Pos.X, Pos.Y, HazardPower!.MovementType, MapCollision.CollideTypeHazard))
                {
                    HitWall = true;

                    if (HazardPower!.WallReflect)
                    {
                        Reflect();
                    }
                    else
                    {
                        Lifespan = 0;
                        if (_collider.IsOutsideMap(Pos.X, Pos.Y))
                            RemoveNow = true;
                    }
                }
            }
        }

        private void Reflect()
        {
            if (_collider != null && !_collider.IsWall(Pos.X - Speed.X, Pos.Y))
            {
                Speed.X *= -1;
                Pos.X += Speed.X;
            }
            else if (!_collider!.IsWall(Pos.X, Pos.Y - Speed.Y))
            {
                Speed.Y *= -1;
                Pos.Y += Speed.Y;
            }
            else
            {
                Speed.X *= -1;
                Speed.Y *= -1;
                Pos.X += Speed.X;
                Pos.Y += Speed.Y;
            }

            if (HazardPower!.Directional)
                Direction = Utils.CalcDirection(Pos.X, Pos.Y, Pos.X + Speed.X, Pos.Y + Speed.Y);
        }

        public void LoadAnimation(string s)
        {
            var anim = SharedResources.Anim!;

            if (!string.IsNullOrEmpty(_animationName))
            {
                anim.DecreaseCount(_animationName);
            }
            if (_activeAnimation != null)
            {
                _activeAnimation.Dispose();
            }
            _activeAnimation = null;
            _animationName = s;
            if (_animationName != "")
            {
                anim.IncreaseCount(_animationName);
                AnimationSet? animationSet = anim.GetAnimationSet(_animationName);
                _activeAnimation = animationSet!.GetAnimation("");
            }

            anim.CleanUp();
        }

        public bool IsDangerousNow()
        {
            return Active && (DelayFrames == 0) &&
                   ((_activeAnimation != null && _activeAnimation.IsActiveFrame())
                     || _activeAnimation == null);
        }

        public bool HasEntity(Entity ent)
        {
            if (HazardPower!.Multihit)
            {
                return false;
            }

            if (Parent != null)
            {
                return Parent.HasEntity(ent);
            }
            else
            {
                if (_entitiesCollided.Contains(ent))
                    return true;
                return false;
            }
        }

        public void AddEntity(Entity ent)
        {
            if (Parent != null)
            {
                Parent.AddEntity(ent);
            }
            else
            {
                _entitiesCollided.Add(ent);
            }
        }

        /// <summary>
        /// 需人工复核：<see cref="Renderable"/> 尚未转换，其字段 <c>Prio</c>（对应原始
        /// <c>uint64_t prio</c>）的最终 C# 类型尚不确定。这里按其原始整型语义直接赋值
        /// 字面量 0 / 2，若最终类型不是 <c>int</c> 可隐式转换的整型（例如 <c>ulong</c>），
        /// 需要在 Renderable 单元转换完成后为此处补充显式转换。
        /// </summary>
        public void AddRenderable(List<Renderable> r, List<Renderable> rDead)
        {
            if (DelayFrames == 0 && _activeAnimation != null)
            {
                Renderable re = _activeAnimation.GetCurrentFrame(Direction);
                re.MapPos.X = Pos.X;
                re.MapPos.Y = Pos.Y;
                re.Prio = HazardPower!.OnFloor ? 0ul : 2ul;
                (HazardPower!.OnFloor ? rDead : r).Add(re);
            }
        }

        public void SetAngle(float angle)
        {
            Angle = angle;
            while (Angle >= MathF.PI * 2) Angle -= MathF.PI * 2;
            while (Angle < 0.0f) Angle += MathF.PI * 2;

            Speed.X = BaseSpeed * MathF.Cos(Angle);
            Speed.Y = BaseSpeed * MathF.Sin(Angle);

            if (HazardPower!.Directional)
                Direction = Utils.CalcDirection(Pos.X, Pos.Y, Pos.X + Speed.X, Pos.Y + Speed.Y);
        }

        /// <summary>
        /// 对应 C++ <c>std::vector&lt;FMinMax&gt;::resize(newSize)</c>：缩小时从尾部截断，
        /// 放大时在末尾补齐新的 <see cref="FMinMax"/> 默认实例，元素顺序保持不变。
        /// List&lt;T&gt; 没有内建的 resize 操作，因此提供该私有辅助方法以逐行还原原始语义
        /// （沿用 output/DeviceList.cs 中同类问题的处理约定）。
        /// </summary>
        private static void Resize(List<FMinMax> list, int newSize)
        {
            if (newSize < list.Count)
            {
                list.RemoveRange(newSize, list.Count - newSize);
            }
            else
            {
                while (list.Count < newSize)
                {
                    list.Add(new FMinMax());
                }
            }
        }
    }
}
