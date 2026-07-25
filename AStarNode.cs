// <自动生成> 对应 C++ 源文件：AStarNode.h + AStarNode.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// AStarNode
    ///
    /// A* 算法中使用的节点，对应 C++ 的 <c>class AStarNode</c>。
    /// </summary>
    public class AStarNode
    {
        /// <summary>
        /// 对应 C++ 全局常量 <c>const int node_stride = 1;</c>（节点之间的最小间距）。
        /// 由于该常量仅在 AStarNode.h 中声明并被 AStarNode 自身使用，转换后保留为本类的
        /// 公有常量，供后续依赖本单元的逻辑单元（如 AStarContainer）引用。
        /// </summary>
        public const int NodeStride = 1;

        // position
        /// <summary>对应 C++ 受保护字段 <c>int x;</c>。</summary>
        protected int _x;
        /// <summary>对应 C++ 受保护字段 <c>int y;</c>。</summary>
        protected int _y;

        // exact cost from first Node
        /// <summary>对应 C++ 受保护字段 <c>float g;</c>。</summary>
        protected float _g;
        // cost to last node
        /// <summary>对应 C++ 受保护字段 <c>float h;</c>。</summary>
        protected float _h;
        // Parent is where this Node come from.
        /// <summary>对应 C++ 受保护字段 <c>Point parent;</c>。</summary>
        protected Int2 _parent;

        /// <summary>
        /// 对应 C++ 默认构造函数 <c>AStarNode::AStarNode()</c> 的成员初始化列表，
        /// 顺序与原始一致：x(0), y(0), g(0), h(0), parent()。
        /// </summary>
        public AStarNode()
        {
            _x = 0;
            _y = 0;
            _g = 0;
            _h = 0;
            _parent = default;
        }

        /// <summary>
        /// 对应 C++ 构造函数 <c>explicit AStarNode(const Point &amp;p)</c>。
        /// 注意：与原始源码逐行一致——该构造函数虽接收 <paramref name="p"/>，
        /// 但 parent 仍初始化为默认值 <c>Point()</c>，而非 <paramref name="p"/> 本身，
        /// 此为原始 C++ 逻辑，未做修正（详见报告"潜在风险"）。
        /// </summary>
        public AStarNode(Int2 p)
        {
            _x = p.X;
            _y = p.Y;
            _g = 0;
            _h = 0;
            _parent = default;
        }

        /// <summary>
        /// 对应 C++ <c>int AStarNode::getX() const</c>。只读属性：原始代码中无对应的 setX()。
        /// </summary>
        public int X => _x;

        /// <summary>
        /// 对应 C++ <c>int AStarNode::getY() const</c>。只读属性：原始代码中无对应的 setY()。
        /// </summary>
        public int Y => _y;

        /// <summary>
        /// 对应 C++ <c>float AStarNode::getH() const</c> 与 <c>void AStarNode::setEstimatedCost(const float H)</c>。
        /// 两者均为对同一字段 h 的平凡访问（无附加逻辑），且原始代码中不存在同名的 getEstimatedCost()/setH()，
        /// 故合并为单个读写属性 H，get 对应 getH()，set 对应 setEstimatedCost()，逻辑完全等价。
        /// </summary>
        public float H
        {
            get => _h;
            set => _h = value;
        }

        /// <summary>
        /// 对应 C++ <c>Point AStarNode::getParent() const</c> 与 <c>void AStarNode::setParent(const Point&amp; p)</c>。
        /// </summary>
        public Int2 Parent
        {
            get => _parent;
            set => _parent = value;
        }

        /// <summary>
        /// 对应 C++ <c>std::list&lt;Point&gt; AStarNode::getNeighbours(int limitX, int limitY) const</c>。
        /// 返回当前节点周围全部邻居坐标的列表，分支判断顺序与 push_back 调用顺序逐行保留。
        /// </summary>
        public List<Int2> GetNeighbours(int limitX = 0, int limitY = 0)
        {
            Int2 toAdd = default;
            List<Int2> res = new List<Int2>();
            if (_x > NodeStride && _y > NodeStride)
            {
                toAdd.X = _x - NodeStride;
                toAdd.Y = _y - NodeStride;
                res.Add(toAdd);
            }
            if (_x > NodeStride && (limitY == 0 || _y < limitY - NodeStride))
            {
                toAdd.X = _x - NodeStride;
                toAdd.Y = _y + NodeStride;
                res.Add(toAdd);
            }
            if (_y > NodeStride && (limitX == 0 || _x < limitX - NodeStride))
            {
                toAdd.X = _x + NodeStride;
                toAdd.Y = _y - NodeStride;
                res.Add(toAdd);
            }
            if ((limitX == 0 || _x < limitX - NodeStride) && (limitY == 0 || _y < limitY - NodeStride))
            {
                toAdd.X = _x + NodeStride;
                toAdd.Y = _y + NodeStride;
                res.Add(toAdd);
            }
            if (_x > NodeStride)
            {
                toAdd.X = _x - NodeStride;
                toAdd.Y = _y;
                res.Add(toAdd);
            }
            if (_y > NodeStride)
            {
                toAdd.X = _x;
                toAdd.Y = _y - NodeStride;
                res.Add(toAdd);
            }
            if (limitX == 0 || _x < limitX - NodeStride)
            {
                toAdd.X = _x + NodeStride;
                toAdd.Y = _y;
                res.Add(toAdd);
            }
            if (limitY == 0 || _y < limitY - NodeStride)
            {
                toAdd.X = _x;
                toAdd.Y = _y + NodeStride;
                res.Add(toAdd);
            }

            return res;
        }

        /// <summary>
        /// 对应 C++ <c>float AStarNode::getActualCost() const</c> 与
        /// <c>void AStarNode::setActualCost(const float G)</c>。
        /// </summary>
        public float ActualCost
        {
            get => _g;
            set => _g = value;
        }

        /// <summary>
        /// 对应 C++ <c>float AStarNode::getFinalCost() const</c>：<c>return g+h*2.f;</c>。
        /// 运算顺序（先乘后加）与原始表达式逐字符保留。
        /// </summary>
        public float FinalCost => _g + _h * 2f;

        /// <summary>
        /// 对应 C++ <c>bool AStarNode::operator&lt;(const AStarNode&amp; n) const</c>：
        /// <c>return getFinalCost() &lt; n.getFinalCost();</c>。
        /// </summary>
        public static bool operator <(AStarNode a, AStarNode b)
        {
            return a.FinalCost < b.FinalCost;
        }

        /// <summary>
        /// C# 语言要求：重载 <c>&lt;</c> 时必须同时定义配对的 <c>&gt;</c>（CS0216），
        /// 原始 C++ 源码未定义 operator&gt;。此实现直接基于已转换的 operator&lt; 取反方向
        /// （a &gt; b 等价于 b &lt; a），未引入任何 C++ 源码之外的比较规则，属于语言层面的必要补充。
        /// </summary>
        public static bool operator >(AStarNode a, AStarNode b)
        {
            return b < a;
        }

        /// <summary>
        /// 对应 C++ <c>bool AStarNode::operator==(const AStarNode&amp; n) const</c>：
        /// <c>return x == n.x &amp;&amp; y == n.y;</c>。
        /// </summary>
        public static bool operator ==(AStarNode a, AStarNode b)
        {
            return a._x == b._x && a._y == b._y;
        }

        /// <summary>
        /// C# 语言要求：重载 <c>==</c> 时必须同时定义配对的 <c>!=</c>（CS0216），
        /// 原始 C++ 源码未定义 <c>AStarNode::operator!=(const AStarNode&amp;)</c>。
        /// 此实现直接复用已转换的 operator== 取反，未引入额外判等规则。
        /// </summary>
        public static bool operator !=(AStarNode a, AStarNode b)
        {
            return !(a == b);
        }

        /// <summary>
        /// 对应 C++ <c>bool AStarNode::operator==(const Point&amp; p) const</c>：
        /// <c>return x == p.x &amp;&amp; y == p.y;</c>。
        /// </summary>
        public static bool operator ==(AStarNode a, Int2 p)
        {
            return a._x == p.X && a._y == p.Y;
        }

        /// <summary>
        /// 对应 C++ <c>bool AStarNode::operator!=(const Point&amp; p) const</c>：
        /// <c>return x != p.x || y != p.y;</c>。
        /// </summary>
        public static bool operator !=(AStarNode a, Int2 p)
        {
            return a._x != p.X || a._y != p.Y;
        }

        /// <summary>
        /// C# 语言规则：重载 <c>==</c>/<c>!=</c> 时应同时重写 <c>Equals</c>，
        /// 以避免编译器警告 CS0660。直接复用 AStarNode 与 AStarNode 之间的 operator== 逻辑，
        /// 不引入任何额外的判等规则，属于语言层面的必要补充，非 C++ 源码原有内容。
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is AStarNode otherNode)
                return this == otherNode;
            if (obj is Int2 otherPoint)
                return this == otherPoint;
            return false;
        }

        /// <summary>
        /// C# 语言规则：重载 <c>==</c>/<c>!=</c> 时应同时重写 <c>GetHashCode</c>，
        /// 以避免编译器警告 CS0661。属于语言层面的必要补充，非 C++ 源码原有内容。
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(_x, _y);
        }
    }
}
