// 对应 C++ 源文件：Loot.h + Loot.cpp
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// Loot
    ///
    /// 地面或飞行中的掉落物实例（对应原始 <c>class Loot</c>）。
    ///
    /// 前向引用说明（需人工复核，详见转换报告"依赖关系"部分）：
    /// <see cref="WidgetTooltip"/> 尚未转换为对应的 C# 单元，此处按原始 C++ 声明前向引用。
    ///
    /// 资源管理：<see cref="Animation"/> 与 <see cref="Wtip"/> 对应原始裸指针，生命周期由
    /// <see cref="LoadAnimation"/> / <see cref="CopyFrom"/> / <see cref="Dispose"/> 手动管理
    /// （构造时 <c>new WidgetTooltip()</c>，析构时 <c>delete</c>），因此本类型实现
    /// <see cref="IDisposable"/>，<see cref="Dispose"/> 对应原始析构函数 <c>~Loot()</c>。
    /// </summary>
    public class Loot : IDisposable
    {
        private string _gfx = "";

        public ItemStack Stack = new ItemStack();
        public Vector2 Pos;
        public Animation? Animation;
        public TooltipData Tip = new TooltipData();
        public WidgetTooltip? Wtip;
        public bool TipVisible;
        public bool DroppedByHero;
        public bool OnGround;
        public bool SoundPlayed;

        public Loot()
        {
            _gfx = "";
            Animation = null;
            Wtip = new WidgetTooltip();
            TipVisible = false;
            DroppedByHero = false;
            OnGround = false;
            SoundPlayed = false;
            Tip.Clear();
        }

        /// <summary>
        /// 对应拷贝构造函数 <c>Loot(const Loot&amp; other)</c>：先将动画与工具提示指针置空，
        /// 再委托给 <see cref="CopyFrom"/>（对应原始实现委托给 <c>operator=</c>）。
        /// </summary>
        public Loot(Loot other)
        {
            _gfx = "";
            Animation = null;
            Wtip = null;
            Stack = new ItemStack();
            Pos = default;
            Tip = new TooltipData();
            TipVisible = false;
            DroppedByHero = false;
            OnGround = false;
            SoundPlayed = false;
            CopyFrom(other);
        }

        /// <summary>
        /// 对应赋值运算符 <c>Loot&amp; operator=(const Loot&amp; other)</c>。
        /// </summary>
        public Loot CopyFrom(Loot other)
        {
            if (ReferenceEquals(this, other))
                return this;

            if (!string.IsNullOrEmpty(_gfx))
                SharedResources.Anim!.DecreaseCount(_gfx);
            if (Animation != null)
            {
                Animation.Dispose();
                Animation = null;
            }

            LoadAnimation(other._gfx);
            if (Animation != null && other.Animation != null)
                Animation.SyncTo(other.Animation);

            Stack.Item = other.Stack.Item;
            Stack.Quantity = other.Stack.Quantity;
            Pos.X = other.Pos.X;
            Pos.Y = other.Pos.Y;
            Tip.Lines.Clear();
            Tip.Lines.AddRange(other.Tip.Lines);
            Tip.Colors.Clear();
            Tip.Colors.AddRange(other.Tip.Colors);
            if (Wtip != null)
            {
                Wtip.Dispose();
            }
            Wtip = new WidgetTooltip();
            TipVisible = other.TipVisible;
            DroppedByHero = other.DroppedByHero;
            OnGround = other.OnGround;
            SoundPlayed = other.SoundPlayed;

            return this;
        }

        public void LoadAnimation(string gfx)
        {
            _gfx = gfx;
            if (!string.IsNullOrEmpty(_gfx))
            {
                SharedResources.Anim!.IncreaseCount(_gfx);
                AnimationSet? animationSet = SharedResources.Anim.GetAnimationSet(_gfx);
                Animation = animationSet!.GetAnimation("");
            }
            else
            {
                Animation = null;
            }
        }

        /// <summary>
        /// 若掉落物仍在飞行，则其"飞行掉落"动画尚未播放完毕。
        /// 仅当地面静止时才允许拾取。
        /// </summary>
        public bool IsFlying()
        {
            return Animation != null && !Animation.IsLastFrame() && Animation.TimesPlayed == 0;
        }

        /// <summary>对应析构函数 <c>~Loot()</c>。</summary>
        public void Dispose()
        {
            if (!string.IsNullOrEmpty(_gfx))
                SharedResources.Anim!.DecreaseCount(_gfx);
            if (Animation != null)
            {
                Animation.Dispose();
                Animation = null;
            }
            if (Wtip != null)
            {
                Wtip.Dispose();
                Wtip = null;
            }
            GC.SuppressFinalize(this);
        }
    }
}
