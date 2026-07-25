// 对应 C++ 源文件：AStarContainer.h + AStarContainer.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// 对应 C++ <c>typedef std::vector&lt;std::vector&lt;short&gt;&gt; AStar_Grid;</c>，
    /// 直接映射为 <c>List&lt;List&lt;short&gt;&gt;</c>，未引入额外的类型别名。
    /// </summary>

    /// <summary>
    /// A* 开放列表容器（二叉堆 + 地图坐标索引）。
    /// 对应 C++ <c>class AStarContainer</c>。
    /// 所有代码均假设传入的节点与坐标在地图边界内。
    /// </summary>
    public class AStarContainer : IDisposable
    {
        private uint _size;
        private uint _nodeLimit;
        private uint _mapWidth;
        private uint _mapHeight;

        /// <summary>
        /// 对应 C++ <c>std::vector&lt;AStarNode*&gt; nodes;</c>。
        /// 按 f 值构成二叉堆，索引 0 始终为 f 最小节点。
        /// </summary>
        private List<AStarNode?> _nodes;

        /// <summary>
        /// 对应 C++ <c>AStar_Grid map_pos;</c>。
        /// 二维 short 数组，以笛卡尔坐标索引主节点数组；初始值为 -1 表示无对应节点。
        /// </summary>
        private List<List<short>> _mapPos;

        /// <summary>
        /// 对应 C++ <c>AStarContainer(unsigned int, unsigned int, unsigned int)</c>。
        /// </summary>
        public AStarContainer(uint mapWidth, uint mapHeight, uint nodeLimit)
        {
            _size = 0;
            _nodeLimit = nodeLimit;
            _mapWidth = mapWidth;
            _mapHeight = mapHeight;

            _nodes = new List<AStarNode?>();
            for (uint i = 0; i < nodeLimit; i++)
            {
                _nodes.Add(null);
            }

            // initialise the map array. A -1 value will mean there is no node at that position
            _mapPos = new List<List<short>>();
            for (uint i = 0; i < mapWidth; ++i)
            {
                List<short> row = new List<short>();
                for (uint j = 0; j < mapHeight; j++)
                {
                    row.Add(-1);
                }
                _mapPos.Add(row);
            }
        }

        /// <summary>
        /// 对应 C++ <c>~AStarContainer()</c>。
        /// </summary>
        public void Dispose()
        {
            for (uint i = 0; i < _size; i++)
            {
                _nodes[(int)i] = null;
            }
            _nodes.Clear();
        }

        /// <summary>
        /// 对应 C++ <c>int AStarContainer::getSize()</c>。
        /// </summary>
        public int GetSize()
        {
            return (int)_size;
        }

        /// <summary>
        /// 对应 C++ <c>void AStarContainer::add(AStarNode* node)</c>。
        /// 假设节点尚未存在于集合中。
        /// </summary>
        public void Add(AStarNode node)
        {
            if (_size >= _nodeLimit) return;

            // add the new node at the end and update its index
            _nodes[(int)_size] = node;
            _mapPos[node.X][node.Y] = (short)_size;

            // reorder the heap based on f ordering, staring with thenewly added node and working up the tree from there
            int m = (int)_size;

            AStarNode? temp = null;
            while (m != 0)
            {
                // if the current nodes f value is shorter than its parent, they need to be swapped
                if (_nodes[m]!.FinalCost <= _nodes[m / 2]!.FinalCost)
                {
                    temp = _nodes[m / 2];
                    _nodes[m / 2] = _nodes[m];
                    _mapPos[_nodes[m / 2]!.X][_nodes[m / 2]!.Y] = (short)(m / 2);
                    _nodes[m] = temp;
                    _mapPos[_nodes[m]!.X][_nodes[m]!.Y] = (short)m;
                    m = m / 2;
                }
                else
                    break;
            }
            _size++;
        }

        /// <summary>
        /// 对应 C++ <c>AStarNode* AStarContainer::get_shortest_f()</c>。
        /// 假设集合中至少有一个节点。
        /// </summary>
        public AStarNode GetShortestF()
        {
            return _nodes[0]!;
        }

        /// <summary>
        /// 对应 C++ <c>void AStarContainer::remove(AStarNode* node)</c>。
        /// 假设节点存在于集合中。
        /// </summary>
        public void Remove(AStarNode node)
        {
            uint heapIndexv = (uint)(_mapPos[node.X][node.Y] + 1);

            // swap the last node in the list with the node being deleted
            _nodes[(int)heapIndexv - 1] = _nodes[(int)_size - 1];
            _mapPos[_nodes[(int)heapIndexv - 1]!.X][_nodes[(int)heapIndexv - 1]!.Y] = (short)(heapIndexv - 1);

            _size--;

            if (_size == 0)
            {
                _mapPos[node.X][node.Y] = -1;
                return;
            }

            // reorder the heap to maintain the f ordering, starting at the node which replaced the deleted node, and working down the tree

            while (true)
            {
                // start at the node which dropped down the tree on the previous iteration
                uint heapIndexu = heapIndexv;
                if (2 * heapIndexu + 1 <= _size)
                { // if both children exist
                    // Select the lowest of the two children.
                    if (_nodes[(int)heapIndexu - 1]!.FinalCost >= _nodes[(int)(2 * heapIndexu) - 1]!.FinalCost) heapIndexv = 2 * heapIndexu;
                    if (_nodes[(int)heapIndexv - 1]!.FinalCost >= _nodes[(int)(2 * heapIndexu)]!.FinalCost) heapIndexv = 2 * heapIndexu + 1;
                }
                else if (2 * heapIndexu <= _size)
                { // if only child #1 exists
                    // Check if the F cost is greater than the child
                    if (_nodes[(int)heapIndexu - 1]!.FinalCost >= _nodes[(int)(2 * heapIndexu) - 1]!.FinalCost) heapIndexv = 2 * heapIndexu;
                }

                if (heapIndexu != heapIndexv)
                { // If parent's F > one or both of its children, swap them
                    AStarNode? temp = _nodes[(int)heapIndexu - 1];
                    _nodes[(int)heapIndexu - 1] = _nodes[(int)heapIndexv - 1];
                    _mapPos[_nodes[(int)heapIndexu - 1]!.X][_nodes[(int)heapIndexu - 1]!.Y] = (short)(heapIndexu - 1);
                    _nodes[(int)heapIndexv - 1] = temp;
                    _mapPos[_nodes[(int)heapIndexv - 1]!.X][_nodes[(int)heapIndexv - 1]!.Y] = (short)(heapIndexv - 1);
                }
                else
                {
                    break;// if item <= both children, exit loop
                }
            }// Repeat forever

            // remove the node from the map pos index
            _mapPos[node.X][node.Y] = -1;
        }

        /// <summary>
        /// 对应 C++ <c>bool AStarContainer::exists(const Point&amp; pos)</c>。
        /// </summary>
        public bool Exists(Int2 pos)
        {
            return _mapPos[pos.X][pos.Y] != -1;
        }

        /// <summary>
        /// 对应 C++ <c>AStarNode* AStarContainer::get(int x, int y)</c>。
        /// 假设节点存在于集合中。
        /// </summary>
        public AStarNode Get(int x, int y)
        {
            return _nodes[_mapPos[x][y]]!;
        }

        /// <summary>
        /// 对应 C++ <c>bool AStarContainer::isEmpty()</c>。
        /// </summary>
        public bool IsEmpty()
        {
            return _size == 0;
        }

        /// <summary>
        /// 对应 C++ <c>void AStarContainer::updateParent(const Point&amp; pos, const Point&amp; parent_pos, float score)</c>。
        /// 假设节点存在于集合中。
        /// </summary>
        public void UpdateParent(Int2 pos, Int2 parentPos, float score)
        {
            Get(pos.X, pos.Y).Parent = parentPos;
            Get(pos.X, pos.Y).ActualCost = score;

            // reorder the heap based on the new f value of this node. starting at the updated node and working up the tree
            int m = _mapPos[pos.X][pos.Y];
            AStarNode? temp = null;
            while (m != 0)
            {
                // if the current node has a lower f value than its parent in the heap, swap them
                if (_nodes[m]!.FinalCost <= _nodes[m / 2]!.FinalCost)
                {
                    temp = _nodes[m / 2];
                    _nodes[m / 2] = _nodes[m];
                    _mapPos[_nodes[m / 2]!.X][_nodes[m / 2]!.Y] = (short)(m / 2);
                    _nodes[m] = temp;
                    _mapPos[_nodes[m]!.X][_nodes[m]!.Y] = (short)m;
                    m = m / 2;
                }
                else
                    break;
            }
        }
    }

    /// <summary>
    /// A* 关闭列表容器（无堆序，但有地图坐标索引）。
    /// 对应 C++ <c>class AStarCloseContainer</c>。
    /// </summary>
    public class AStarCloseContainer : IDisposable
    {
        private uint _size;
        private uint _nodeLimit;
        private uint _mapWidth;
        private uint _mapHeight;
        private List<AStarNode?> _nodes;
        private List<List<short>> _mapPos;

        /// <summary>
        /// 对应 C++ <c>AStarCloseContainer(unsigned int, unsigned int, unsigned int)</c>。
        /// </summary>
        public AStarCloseContainer(uint mapWidth, uint mapHeight, uint nodeLimit)
        {
            _size = 0;
            _nodeLimit = nodeLimit;
            _mapWidth = mapWidth;
            _mapHeight = mapHeight;

            _nodes = new List<AStarNode?>();
            for (uint i = 0; i < nodeLimit; i++)
            {
                _nodes.Add(null);
            }

            // initialise the map array. A -1 value will mean there is no node at that position
            _mapPos = new List<List<short>>();
            for (uint i = 0; i < mapWidth; ++i)
            {
                List<short> row = new List<short>();
                for (uint j = 0; j < mapHeight; j++)
                {
                    row.Add(-1);
                }
                _mapPos.Add(row);
            }
        }

        /// <summary>
        /// 对应 C++ <c>~AStarCloseContainer()</c>。
        /// </summary>
        public void Dispose()
        {
            for (uint i = 0; i < _size; i++)
            {
                _nodes[(int)i] = null;
            }
            _nodes.Clear();
        }

        /// <summary>
        /// 对应 C++ <c>int AStarCloseContainer::getSize()</c>。
        /// </summary>
        public int GetSize()
        {
            return (int)_size;
        }

        /// <summary>
        /// 对应 C++ <c>void AStarCloseContainer::add(AStarNode* node)</c>。
        /// </summary>
        public void Add(AStarNode node)
        {
            if (_size >= _nodeLimit) return;

            _nodes[(int)_size] = node;
            _mapPos[node.X][node.Y] = (short)_size;
            _size++;
        }

        /// <summary>
        /// 对应 C++ <c>bool AStarCloseContainer::exists(const Point&amp; pos)</c>。
        /// </summary>
        public bool Exists(Int2 pos)
        {
            return _mapPos[pos.X][pos.Y] != -1;
        }

        /// <summary>
        /// 对应 C++ <c>AStarNode* AStarCloseContainer::get(int x, int y)</c>。
        /// </summary>
        public AStarNode Get(int x, int y)
        {
            return _nodes[_mapPos[x][y]]!;
        }

        /// <summary>
        /// 对应 C++ <c>AStarNode* AStarCloseContainer::get_shortest_h()</c>。
        /// </summary>
        public AStarNode GetShortestH()
        {
            AStarNode? current = null;
            float lowestScore = float.MaxValue;
            for (uint i = 0; i < _size; i++)
            {
                if (_nodes[(int)i]!.H < lowestScore)
                {
                    lowestScore = _nodes[(int)i]!.H;
                    current = _nodes[(int)i];
                }
            }
            return current!;
        }
    }
}
