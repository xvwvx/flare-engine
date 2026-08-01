// <自动生成> 对应 C++ 源文件：MapCollision.h + MapCollision.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using System.Diagnostics;
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// MapCollision
    ///
    /// 处理地图物体之间的碰撞（对应 C++ 的 <c>class MapCollision</c>）。
    /// 数学类型映射：C++ <c>Point</c> -> <see cref="Int2"/>；<c>FPoint</c> -> <see cref="Vector2"/>，
    /// 与 output/Utils.cs、output/AStarNode.cs 中既定的映射方式保持一致。
    /// C++ 中的 <c>typedef std::vector&lt;std::vector&lt;unsigned short&gt;&gt; Map_Layer;</c>
    /// 直接映射为 <c>List&lt;List&lt;ushort&gt;&gt;</c>，未引入额外的类型别名
    /// （与 output/Hazard.report.txt、output/EventManager.cs 中对 Map_Layer 的既有处理方式一致）。
    /// </summary>
    public class MapCollision
    {
        // this value is used to determine the greatest possible position within a tile before transitioning to the next tile
        // so if an entity has a position of (1-MIN_TILE_GAP, 0) and moves to the east, they will move to (1,0)
        private const float MinTileGap = 0.001f;

        // collision check types
        private const int CheckMovement = 1;
        private const int CheckSight = 2;

        private bool _hasEmptyTile;

        private float _raycastResolution;
        private float _raycastResolutionRecip;

        // const flags
        public const bool IsAlly = true;
        public const int DefaultPathLimit = 0;

        // collision type
        public const int CollideTypeNone = 0;
        public const int CollideTypeAllEntities = 1;
        public const int CollideTypeHero = 2;
        public const int CollideTypeHazard = 3;

        // movement options
        public const int MoveNormal = 0;
        public const int MoveFlying = 1; // can move through BLOCKS_MOVEMENT (e.g. water)
        public const int MoveIntangible = 2; // can move through BLOCKS_ALL (e.g. walls)

        // collision tile types
        // The numbers 0..6 are the collision tiles as produced by tiled,
        // only 7 and 8 deal with entities on the map
        public const int BlocksNone = 0;
        public const int BlocksAll = 1;
        public const int BlocksMovement = 2;
        public const int BlocksAllHidden = 3;
        public const int BlocksMovementHidden = 4;
        public const int MapOnly = 5;
        public const int MapOnlyAlt = 6;
        public const int BlocksEntities = 7; // hero or enemies are blocking this tile, so any other entity is blocked
        public const int BlocksEnemies = 8; // an ally is standing on that tile, so the hero could pass if ENABLE_ALLY_COLLISION is false

        public List<List<ushort>> Colmap = new List<List<ushort>>();
        public Int2 MapSize;

        /// <summary>
        /// 对应 C++ 构造函数 <c>MapCollision::MapCollision()</c> 的成员初始化列表
        /// （has_empty_tile, raycast_resolution, raycast_resolution_recip, map_size 按声明顺序），
        /// 随后执行构造函数体 <c>colmap.resize(1); colmap[0].resize(1);</c>。
        /// </summary>
        public MapCollision()
        {
            var eset = SharedResources.Eset!;
            _hasEmptyTile = false;
            _raycastResolution = eset.Misc.RaycastResolution;
            _raycastResolutionRecip = 1f / eset.Misc.RaycastResolution;
            MapSize = default;

            ResizeList(Colmap, 1, () => new List<ushort>());
            ResizeList(Colmap[0], 1, () => (ushort)0);
        }

        /// <summary>
        /// 对应 C++ 析构函数 <c>MapCollision::~MapCollision()</c>：函数体为空，
        /// MapCollision 不持有任何非托管资源或实现 IDisposable 的成员（Colmap 是托管 List），
        /// 因此本单元无需实现 IDisposable，也无需生成任何释放逻辑。
        /// </summary>

        /// <summary>
        /// std::vector&lt;T&gt;::resize(n) 的等价实现：放大时以 <paramref name="factory"/> 生成的
        /// 默认值填充新增元素，缩小时从尾部截断，保持重叠部分元素原值不变。
        /// 这是语言层面的必要补充（List&lt;T&gt; 没有内置 Resize），并非 C++ 源码中的逻辑，
        /// 用于逐行复现 colmap.resize(w) / colmap[i].resize(h) 等调用。
        /// </summary>
        private static void ResizeList<T>(List<T> list, int newSize, Func<T> factory)
        {
            if (newSize < list.Count)
            {
                list.RemoveRange(newSize, list.Count - newSize);
            }
            else
            {
                while (list.Count < newSize)
                {
                    list.Add(factory());
                }
            }
        }

        /// <summary>
        /// 对应 C++ 文件作用域全局函数 <c>int sgn(float f)</c>（定义于 MapCollision.cpp 顶部，
        /// 未在任何头文件中声明，仅本单元内部使用）。依据规则"全局函数转换为 static class 中的
        /// 静态方法"，同时为保留其"仅本翻译单元可见"的原始作用域语义，转换为本类的
        /// private static 方法，而非提升为独立的公共静态类。
        /// </summary>
        private static int Sgn(float f)
        {
            if (f > 0) return 1;
            else if (f < 0) return -1;
            else return 0;
        }

        public void SetMap(List<List<ushort>> colmap, ushort w, ushort h)
        {
            _hasEmptyTile = false;

            ResizeList(Colmap, w, () => new List<ushort>());
            for (uint i = 0; i < w; ++i)
            {
                ResizeList(Colmap[(int)i], h, () => (ushort)0);
            }
            for (uint i = 0; i < w; i++)
                for (uint j = 0; j < h; j++)
                {
                    Colmap[(int)i][(int)j] = colmap[(int)i][(int)j];
                    if (Colmap[(int)i][(int)j] == 0)
                        _hasEmptyTile = true;
                }

            MapSize.X = w;
            MapSize.Y = h;
        }

        private bool SmallStep(ref float x, ref float y, float stepX, float stepY, int movementType, int collideType)
        {
            if (IsValidPosition(x + stepX, y + stepY, movementType, collideType))
            {
                x += stepX;
                y += stepY;
                Debug.Assert(IsValidPosition(x, y, movementType, collideType));
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool SmallStepForcedSlideAlongGrid(ref float x, ref float y, float stepX, float stepY, int movementType, int collideType)
        {
            if (IsValidPosition(x + stepX, y, movementType, collideType)) // slide along wall
            {
                if (stepX == 0) return true;
                x += stepX;
                Debug.Assert(IsValidPosition(x, y, movementType, collideType));
            }
            else if (IsValidPosition(x, y + stepY, movementType, collideType))
            {
                if (stepY == 0) return true;
                y += stepY;
                Debug.Assert(IsValidPosition(x, y, movementType, collideType));
            }
            else
            {
                return false;
            }
            return true;
        }

        private bool SmallStepForcedSlide(ref float x, ref float y, float stepX, float stepY, int movementType, int collideType)
        {
            // is there a singular obstacle or corner we can step around?
            // only works if we are moving straight
            const float epsilon = 0.01f;
            if (stepX != 0)
            {
                Debug.Assert(stepY == 0);
                float dy = y - MathF.Floor(y);

                if (IsValidTile((int)x, (int)y + 1, movementType, collideType)
                        && IsValidTile((int)x + Sgn(stepX), (int)y + 1, movementType, collideType)
                        && dy > 0.5)
                {
                    y += MathF.Min(1 - dy + epsilon, MathF.Abs(stepX));
                }
                else if (IsValidTile((int)x, (int)y - 1, movementType, collideType)
                         && IsValidTile((int)x + Sgn(stepX), (int)y - 1, movementType, collideType)
                         && dy < 0.5)
                {
                    y -= MathF.Min(dy + epsilon, MathF.Abs(stepX));
                }
                else
                {
                    return false;
                }
                Debug.Assert(IsValidPosition(x, y, movementType, collideType));
            }
            else if (stepY != 0)
            {
                Debug.Assert(stepX == 0);
                float dx = x - MathF.Floor(x);

                if (IsValidTile((int)x + 1, (int)y, movementType, collideType)
                        && IsValidTile((int)x + 1, (int)y + Sgn(stepY), movementType, collideType)
                        && dx > 0.5)
                {
                    x += MathF.Min(1 - dx + epsilon, MathF.Abs(stepY));
                }
                else if (IsValidTile((int)x - 1, (int)y, movementType, collideType)
                         && IsValidTile((int)x - 1, (int)y + Sgn(stepY), movementType, collideType)
                         && dx < 0.5)
                {
                    x -= MathF.Min(dx + epsilon, MathF.Abs(stepY));
                }
                else
                {
                    return false;
                }
            }
            else
            {
                Debug.Assert(false);
            }
            return true;
        }

        /// <summary>
        /// Process movement for cardinal (90 degree) and ordinal (45 degree) directions
        /// If we encounter an obstacle at 90 degrees, stop.
        /// If we encounter an obstacle at 45 or 135 degrees, slide.
        /// </summary>
        public bool Move(ref float x, ref float y, float remainingStepX, float remainingStepY, int movementType, int collideType)
        {
            // when trying to slide against a bottom or right wall, step_x or step_y can become 0
            // this causes diag to become false, making this function return false
            // we try to catch such a scenario and return true early
            bool forceSlide = (remainingStepX != 0 && remainingStepY != 0);

            while (remainingStepX != 0 || remainingStepY != 0)
            {

                float stepX = 0;
                if (remainingStepX > 0)
                {
                    // find next interesting value, which is either the whole step, or the transition to the next tile
                    stepX = MathF.Min(MathF.Ceiling(x) - x, remainingStepX);
                    // if we are standing on the edge of a tile (ceilf(x) - x == 0), we need to look one tile ahead
                    if (stepX <= MinTileGap) stepX = MathF.Min(1f, remainingStepX);
                }
                else if (remainingStepX < 0)
                {
                    stepX = MathF.Max(MathF.Floor(x) - x, remainingStepX);
                    if (stepX == 0) stepX = MathF.Max(-1f, remainingStepX);
                }

                float stepY = 0;
                if (remainingStepY > 0)
                {
                    stepY = MathF.Min(MathF.Ceiling(y) - y, remainingStepY);
                    if (stepY <= MinTileGap) stepY = MathF.Min(1f, remainingStepY);
                }
                else if (remainingStepY < 0)
                {
                    stepY = MathF.Max(MathF.Floor(y) - y, remainingStepY);
                    if (stepY == 0) stepY = MathF.Max(-1f, remainingStepY);
                }

                remainingStepX -= stepX;
                remainingStepY -= stepY;

                if (!SmallStep(ref x, ref y, stepX, stepY, movementType, collideType))
                {
                    if (forceSlide)
                    {
                        if (!SmallStepForcedSlideAlongGrid(ref x, ref y, stepX, stepY, movementType, collideType))
                            return false;
                    }
                    else
                    {
                        if (!SmallStepForcedSlide(ref x, ref y, stepX, stepY, movementType, collideType))
                            return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Determines whether the grid position is outside the map boundary
        /// </summary>
        private bool IsTileOutsideMap(int tileX, int tileY)
        {
            return (tileX < 0 || tileY < 0 || tileX >= MapSize.X || tileY >= MapSize.Y);
        }

        public bool IsOutsideMap(float tileX, float tileY)
        {
            return (tileX < 0 || tileY < 0 || tileX >= (float)MapSize.X || tileY >= (float)MapSize.Y);
        }

        /// <summary>
        /// A map space is a wall if it contains a wall blocking type (normal or hidden)
        /// A position outside the map boundary is a wall
        /// </summary>
        public bool IsWall(float x, float y)
        {
            // bounds check
            int tileX = (int)x;
            int tileY = (int)y;
            if (IsTileOutsideMap(tileX, tileY)) return true;

            // collision type check
            return (Colmap[tileX][tileY] == BlocksAll || Colmap[tileX][tileY] == BlocksAllHidden);
        }

        /// <summary>
        /// Is this a valid tile for an entity with this movement type?
        /// </summary>
        private bool IsValidTile(int tileX, int tileY, int movementType, int collideType)
        {
            // outside the map isn't valid
            if (IsTileOutsideMap(tileX, tileY)) return false;

            if (collideType == CollideTypeAllEntities)
            {
                if (Colmap[tileX][tileY] == BlocksEnemies)
                    return false;
                if (Colmap[tileX][tileY] == BlocksEntities)
                    return false;
            }
            else if (collideType == CollideTypeHero)
            {
                if (Colmap[tileX][tileY] == BlocksEnemies && !SharedResources.Eset!.Misc.EnableAllyCollision)
                    return true;
            }

            // intangible creatures can be everywhere
            if (movementType == MoveIntangible)
                return true;

            // flying creatures can't be in walls
            if (movementType == MoveFlying)
            {
                return (!(Colmap[tileX][tileY] == BlocksAll || Colmap[tileX][tileY] == BlocksAllHidden));
            }

            if (Colmap[tileX][tileY] == MapOnly || Colmap[tileX][tileY] == MapOnlyAlt)
                return true;

            // normal creatures can only be in empty spaces
            return (Colmap[tileX][tileY] == BlocksNone) || (collideType == CollideTypeHazard && (Colmap[tileX][tileY] == BlocksEnemies || Colmap[tileX][tileY] == BlocksEntities));
        }

        /// <summary>
        /// Is this a valid position for an entity with this movement type?
        /// </summary>
        public bool IsValidPosition(float x, float y, int movementType, int collideType)
        {
            if (x < 0 || y < 0) return false;

            return IsValidTile((int)x, (int)y, movementType, collideType);
        }

        /// <summary>
        /// Does not have the "slide" submovement that move() features
        /// Line can be arbitrary angles.
        /// </summary>
        private bool LineCheck(float x1, float y1, float x2, float y2, int checkType, int movementType)
        {
            float x = x1;
            float y = y1;
            float dx = MathF.Abs(x2 - x1);
            float dy = MathF.Abs(y2 - y1);
            float stepX;
            float stepY;
            int steps = (int)MathF.Max(1f, MathF.Max(dx, dy) * _raycastResolutionRecip);


            if (dx > dy)
            {
                stepX = 1;
                stepY = dy / dx;
            }
            else
            {
                stepY = 1;
                stepX = dx / dy;
            }
            // fix signs
            if (x1 > x2) stepX = -stepX;
            if (y1 > y2) stepY = -stepY;

            stepX *= _raycastResolution;
            stepY *= _raycastResolution;


            if (checkType == CheckSight)
            {
                for (int i = 0; i < steps; i++)
                {
                    x += stepX;
                    y += stepY;
                    if (IsWall(x, y))
                        return false;
                }
            }
            else if (checkType == CheckMovement)
            {
                for (int i = 0; i < steps; i++)
                {
                    x += stepX;
                    y += stepY;
                    if (!IsValidPosition(x, y, movementType, CollideTypeAllEntities))
                        return false;
                }
            }

            return true;
        }

        public bool LineOfSight(float x1, float y1, float x2, float y2)
        {
            return LineCheck(x1, y1, x2, y2, CheckSight, MoveNormal);
        }

        public bool LineOfMovement(float x1, float y1, float x2, float y2, int movementType)
        {
            if (IsOutsideMap(x2, y2)) return false;

            // intangible entities can always move
            if (movementType == MoveIntangible) return true;

            // if the target is blocking, clear it temporarily
            int tileX = (int)x2;
            int tileY = (int)y2;
            bool targetBlocks = false;
            int targetBlocksType = Colmap[tileX][tileY];
            if (Colmap[tileX][tileY] == BlocksEntities || Colmap[tileX][tileY] == BlocksEnemies)
            {
                targetBlocks = true;
                Unblock(x2, y2);
            }

            bool hasMovement = LineCheck(x1, y1, x2, y2, CheckMovement, movementType);

            if (targetBlocks) Block(x2, y2, targetBlocksType == BlocksEnemies);
            return hasMovement;

        }

        /// <summary>
        /// Checks whether the entity in pos 1 is facing the point at pos 2
        /// based on a 180 degree field of vision
        /// </summary>
        public bool IsFacing(float x1, float y1, char direction, float x2, float y2)
        {

            // 180 degree fov
            // switch case 常量使用 (char)N 字面量：C# 要求 char 类型 switch 的 case 标签为 char
            // 常量而非整型字面量，这是语言层面的必要写法调整，未改变原始 8 个方向分支的判断逻辑。
            switch (direction)
            {
                case (char)2: //north west
                    return ((x2 - x1) < ((-1 * y2) - (-1 * y1))) && (((-1 * x2) - (-1 * x1)) > (y2 - y1));
                case (char)3: //north
                    return y2 < y1;
                case (char)4: //north east
                    return (((-1 * x2) - (-1 * x1)) < ((-1 * y2) - (-1 * y1))) && ((x2 - x1) > (y2 - y1));
                case (char)5: //east
                    return x2 > x1;
                case (char)6: //south east
                    return ((x2 - x1) > ((-1 * y2) - (-1 * y1))) && (((-1 * x2) - (-1 * x1)) < (y2 - y1));
                case (char)7: //south
                    return y2 > y1;
                case (char)0: //south west
                    return (((-1 * x2) - (-1 * x1)) > ((-1 * y2) - (-1 * y1))) && ((x2 - x1) < (y2 - y1));
                case (char)1: //west
                    return x2 < x1;
            }
            return false;
        }

        /// <summary>
        /// Compute a path from (x1,y1) to (x2,y2)
        /// Store waypoint inside path
        /// limit is the maximum number of explored node
        /// </summary>
        /// <returns>true if a path is found</returns>
        public bool ComputePath(Vector2 startPos, Vector2 endPos, List<Vector2> path, int movementType, uint limit)
        {

            if (IsOutsideMap(endPos.X, endPos.Y)) return false;

            // default limit set to 10% of the total map size
            if (limit == 0)
                limit = (uint)((MapSize.X * MapSize.Y) / 10);

            // path must be empty
            if (path.Count != 0)
                path.Clear();

            // convert start & end to MapCollision precision
            Int2 start = startPos.ToInt2();
            Int2 end = endPos.ToInt2();

            // if the target square has an entity, temporarily clear it to compute the path
            bool targetBlocks = false;
            int targetBlocksType = Colmap[end.X][end.Y];
            if (Colmap[end.X][end.Y] == BlocksEntities || Colmap[end.X][end.Y] == BlocksEnemies)
            {
                targetBlocks = true;
                Unblock(endPos.X, endPos.Y);
            }

            Int2 current = start;
            AStarNode node = new AStarNode(start);
            node.ActualCost = 0;
            node.H = Utils.CalcDist(start.ToVector2(), end.ToVector2());
            node.Parent = current;

            AStarContainer open = new AStarContainer((uint)MapSize.X, (uint)MapSize.Y, limit);
            AStarCloseContainer close = new AStarCloseContainer((uint)MapSize.X, (uint)MapSize.Y, limit);

            open.Add(node);

            while (!open.IsEmpty() && (uint)close.GetSize() < limit)
            {
                node = open.GetShortestF();

                current.X = node.X;
                current.Y = node.Y;
                close.Add(node);
                open.Remove(node);

                if (current.X == end.X && current.Y == end.Y)
                    break; //path found !

                //limit evaluated nodes to the size of the map
                List<Int2> neighbours = node.GetNeighbours(MapSize.X, MapSize.Y);

                // for every neighbour of current node
                for (int neighbourIt = 0; neighbourIt != neighbours.Count; ++neighbourIt)
                {
                    Int2 neighbour = neighbours[neighbourIt];

                    // do not exceed the node limit when adding nodes
                    if ((uint)open.GetSize() >= limit)
                    {
                        break;
                    }

                    // if neighbour is not free of any collision, skip it
                    if (!IsValidTile(neighbour.X, neighbour.Y, movementType, CollideTypeAllEntities))
                        continue;
                    // if nabour is already in close, skip it
                    if (close.Exists(neighbour))
                        continue;

                    // if neighbour isn't inside open, add it as a new Node
                    if (!open.Exists(neighbour))
                    {
                        AStarNode newNode = new AStarNode(neighbour);
                        newNode.ActualCost = node.ActualCost + Utils.CalcDist(current.ToVector2(), neighbour.ToVector2());
                        newNode.Parent = current;
                        newNode.H = Utils.CalcDist(neighbour.ToVector2(), end.ToVector2());
                        open.Add(newNode);
                    }
                    // else, update it's cost if better
                    else
                    {
                        AStarNode i = open.Get(neighbour.X, neighbour.Y);
                        if (node.ActualCost + Utils.CalcDist(current.ToVector2(), neighbour.ToVector2()) < i.ActualCost)
                        {
                            Int2 pos = new Int2(i.X, i.Y);
                            Int2 parentPos = new Int2(node.X, node.Y);
                            open.UpdateParent(pos, parentPos, node.ActualCost + Utils.CalcDist(current.ToVector2(), neighbour.ToVector2()));
                        }
                    }
                }
            }

            if (!(current.X == end.X && current.Y == end.Y))
            {

                //couldnt find the target so map a path to the closest node found
                node = close.GetShortestH();
                current.X = node.X;
                current.Y = node.Y;

                while (!(current.X == start.X && current.Y == start.Y))
                {
                    path.Add(CollisionToMap(current));
                    current = close.Get(current.X, current.Y).Parent;
                }
            }
            else
            {
                // store path from end to start
                path.Add(CollisionToMap(end));
                while (!(current.X == start.X && current.Y == start.Y))
                {
                    path.Add(CollisionToMap(current));
                    current = close.Get(current.X, current.Y).Parent;
                }
            }
            // reblock target if needed
            if (targetBlocks) Block(endPos.X, endPos.Y, targetBlocksType == BlocksEnemies);

            return path.Count != 0;
        }

        public void Block(float mapX, float mapY, bool isAlly)
        {
            int tileX = (int)mapX;
            int tileY = (int)mapY;

            if (IsTileOutsideMap(tileX, tileY))
                return;

            if (Colmap[tileX][tileY] == BlocksNone)
            {
                if (isAlly)
                    Colmap[tileX][tileY] = BlocksEnemies;
                else
                    Colmap[tileX][tileY] = BlocksEntities;
            }

        }

        public void Unblock(float mapX, float mapY)
        {
            int tileX = (int)mapX;
            int tileY = (int)mapY;

            if (IsTileOutsideMap(tileX, tileY))
                return;

            if (Colmap[tileX][tileY] == BlocksEntities || Colmap[tileX][tileY] == BlocksEnemies)
            {
                Colmap[tileX][tileY] = BlocksNone;
            }

        }

        /// <summary>
        /// Given a target, trys to return one of the 8+ adjacent tiles
        /// Returns the retargeted position on success, returns the original position on failure
        /// </summary>
        public Vector2 GetRandomNeighbor(Int2 target, int range, int movementType, int collideType)
        {
            Vector2 newTarget = target.ToVector2();
            List<Vector2> validTiles = new List<Vector2>();

            for (int i = -range; i <= range; i++)
            {
                for (int j = -range; j <= range; j++)
                {
                    if (i == 0 && j == 0) continue; // skip the middle tile
                    newTarget.X = (float)(target.X + i) + 0.5f;
                    newTarget.Y = (float)(target.Y + j) + 0.5f;
                    if (IsValidPosition(newTarget.X, newTarget.Y, movementType, collideType))
                        validTiles.Add(newTarget);
                }
            }

            if (validTiles.Count != 0)
                return validTiles[Program.Rng.Next(validTiles.Count)];
            else
                return target.ToVector2();
        }

        private Vector2 CollisionToMap(Int2 p)
        {
            Vector2 ret = default;
            ret.X = (float)p.X + 0.5f;
            ret.Y = (float)p.Y + 0.5f;
            return ret;
        }

        /// <summary>
        /// 对应 C++ <c>int getCollideType(bool hero)</c>：该内联方法带参数且含三元判断逻辑，
        /// 不属于"简单 getter/setter"，按规则保留为普通方法而非属性。
        /// </summary>
        public int GetCollideType(bool hero)
        {
            return hero ? CollideTypeHero : CollideTypeAllEntities;
        }

        /// <summary>
        /// 对应 C++ <c>bool hasEmptyTile() { return has_empty_tile; }</c>：
        /// 无参数、无副作用的简单字段访问器，且不存在对应的 setter，转换为只读属性。
        /// </summary>
        public bool HasEmptyTile => _hasEmptyTile;
    }
}
