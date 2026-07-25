// <自动生成> 对应 C++ 源文件：GameSlotPreview.h + GameSlotPreview.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略�?
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// GameSlotPreview
    ///
    /// 负责 GameStateLoad 界面中角色存档预览的逻辑与渲染�?
    /// </summary>
    public class GameSlotPreview : IDisposable
    {
        private StatBlock? _stats;
        private Int2 _pos;
        private byte _staticDirection;

        /// <summary>
        /// 对应 C++ 中的 <c>unsigned char* direction</c>：该指针可以指向自身�?
        /// <see cref="_staticDirection"/>，也可以指向外部 <see cref="StatBlock"/> 实例�?
        /// direction 字段。C# 没有"指向任意字段"的裸指针等价物（语言范式差异，属于必要适配），
        /// 这里用一个闭包捕获目标字段的 <see cref="Func{TResult}"/> 表达同样�?读取时始终取
        /// 目标当前�?的别名语义：目标始终是某个具体对象在赋值那一刻绑定的字段�?
        /// 与原始指针语义完全一致（重新赋�?<c>direction</c> 本身，而不是通过它写回）�?
        /// </summary>
        private Func<byte> _direction;

        // hold the animations for all equipped items in the right order of drawing.
        private readonly List<AnimationSet?> _animsets = new List<AnimationSet?>();
        // hold the animations for all equipped items in the right order of drawing.
        private readonly List<Animation?> _anims = new List<Animation?>();

        private readonly List<string> _defaultGfx = new List<string>();

        public GameSlotPreview()
        {
            _stats = null;
            _staticDirection = 6;
            _direction = () => _staticDirection;

            // load the hero's animations from hero definition file
            SharedResources.Anim!.IncreaseCount("animations/hero.txt");
            AnimationSet = SharedResources.Anim!.GetAnimationSet("animations/hero.txt");
            ActiveAnimation = AnimationSet!.GetAnimation("");

            // load layer definitions
            LayerDef = new List<List<int>>();
            for (int i = 0; i < 8; i++)
                LayerDef.Add(new List<int>());
            LayerReferenceOrder = new List<string>();

            // NOTE: This is documented in Avatar.cpp
            FileParser infile = new FileParser();
            if (infile.Open("engine/hero_layers.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (infile.Key == "layer")
                    {
                        int dir = Parse.ToDirection(Parse.PopFirstString(ref infile.Val));
                        if (dir > 7)
                        {
                            infile.Error("GameSlotPreview: Hero layer direction must be in range [0,7]");
                            Utils.LogErrorDialog("GameSlotPreview: Hero layer direction must be in range [0,7]");
                            SharedResources.Mods!.ResetModConfig();
                            Utils.Exit(1);
                        }
                        string layer = Parse.PopFirstString(ref infile.Val);
                        while (layer != "")
                        {
                            // check if already in layer_reference:
                            int refPos;
                            for (refPos = 0; refPos < LayerReferenceOrder.Count; ++refPos)
                                if (layer == LayerReferenceOrder[refPos])
                                    break;
                            if (refPos == LayerReferenceOrder.Count)
                                LayerReferenceOrder.Add(layer);
                            LayerDef[dir].Add(refPos);

                            layer = Parse.PopFirstString(ref infile.Val);
                        }
                    }
                    else
                    {
                        infile.Error("GameSlotPreview: '%s' is not a valid key.", infile.Key);
                    }
                }
                infile.Close();
            }

            // There are the positions of the items relative to layer_reference_order
            // so if layer_reference_order=main,body,head,off
            // and we got a layer=3,off,body,head,main
            // then the layer_def[3] looks like (3,1,2,0)
        }

        /// <summary>
        /// 对应 C++ 析构函数：按原始顺序释放当前动画引用计数、当前动画帧对象�?
        /// 再依次释放每套装备动画集的引用计数与动画帧对象，最后触发一次清理�?
        /// </summary>
        public void Dispose()
        {
            SharedResources.Anim!.DecreaseCount("animations/hero.txt");
            if (ActiveAnimation != null)
                ActiveAnimation.Dispose();

            for (int i = 0; i < _animsets.Count; i++)
            {
                if (_animsets[i] != null)
                    SharedResources.Anim!.DecreaseCount(_animsets[i]!.Name);
                _anims[i]?.Dispose();
            }
            SharedResources.Anim!.CleanUp();
        }

        public void SetAnimation(string name)
        {
            if (ActiveAnimation != null)
            {
                if (name == ActiveAnimation.Name)
                    return;

                ActiveAnimation.Dispose();
            }

            ActiveAnimation = AnimationSet!.GetAnimation(name);

            for (int i = 0; i < _animsets.Count; i++)
            {
                _anims[i]?.Dispose();
                if (_animsets[i] != null)
                    _anims[i] = _animsets[i]!.GetAnimation(name);
                else
                    _anims[i] = null;
            }
        }

        public void SetStatBlock(StatBlock? stats)
        {
            _stats = stats;
            if (stats != null)
            {
                _direction = () => stats.Direction;
            }
        }

        public void SetPos(Int2 pos)
        {
            _pos = pos;
        }

        public void SetDirection(byte dir)
        {
            _staticDirection = dir;
            _direction = () => _staticDirection;
        }

        public void LoadGraphics(List<string> imgGfx)
        {
            if (_stats == null)
                return;

            for (int i = 0; i < _animsets.Count; i++)
            {
                if (_animsets[i] != null)
                    SharedResources.Anim!.DecreaseCount(_animsets[i]!.Name);
                _anims[i]?.Dispose();
            }
            _animsets.Clear();
            _anims.Clear();

            for (int i = 0; i < imgGfx.Count; i++)
            {
                if (imgGfx[i] != "")
                {
                    string name = "animations/avatar/" + _stats!.GfxBase + "/" + imgGfx[i] + ".txt";
                    SharedResources.Anim!.IncreaseCount(name);
                    _animsets.Add(SharedResources.Anim!.GetAnimationSet(name));
                    _animsets[^1]!.Parent = AnimationSet;
                    _anims.Add(_animsets[^1]!.GetAnimation(ActiveAnimation!.Name));
                    SetAnimation("stance");
                    if (!_anims[^1]!.SyncTo(ActiveAnimation))
                    {
                        Utils.LogError("GameSlotPreview: Error syncing animation in '%s' to 'animations/hero.txt'.", _animsets[^1]!.Name);
                    }
                }
                else
                {
                    _animsets.Add(null);
                    _anims.Add(null);
                }
            }
            SharedResources.Anim!.CleanUp();

            SetAnimation("stance");
        }

        public void Logic()
        {
            // handle animation
            ActiveAnimation!.AdvanceFrame();
            for (int i = 0; i < _anims.Count; i++)
            {
                if (_anims[i] != null)
                    _anims[i]!.AdvanceFrame();
            }
        }

        public void AddRenders(List<Renderable> r)
        {
            if (_stats == null)
                return;

            byte dir = _direction();

            for (int i = 0; i < LayerDef[dir].Count; ++i)
            {
                int index = LayerDef[dir][i];
                if (index < _anims.Count && _anims[index] != null)
                {
                    Renderable ren = _anims[index]!.GetCurrentFrame(dir);
                    ren.Prio = (ulong)(i + 1);
                    r.Add(ren);
                }
            }
        }

        public void Render()
        {
            List<Renderable> r = new List<Renderable>();
            AddRenders(r);

            for (int i = 0; i < r.Count; ++i)
            {
                if (r[i].Image != null)
                {
                    Rectangle dest = default;
                    dest.X = _pos.X - r[i].Offset.X;
                    dest.Y = _pos.Y - r[i].Offset.Y;
                    SharedResources.RenderDevice!.Render(r[i], ref dest);
                }
            }
        }

        public void LoadDefaultGraphics()
        {
            if (_stats == null)
            {
                Utils.LogError("GameSlotPreview: StatBlock is not set. Can't load default graphics.");
                return;
            }

            _defaultGfx.Clear();

            // fall back to default if it exists
            for (int i = 0; i < LayerReferenceOrder.Count; ++i)
            {
                bool exists = Filesystem.FileExists(SharedResources.Mods!.Locate("animations/avatar/" + _stats!.GfxBase + "/default_" + LayerReferenceOrder[i] + ".txt"));
                if (exists)
                {
                    _defaultGfx.Add("default_" + LayerReferenceOrder[i]);
                }
                else if (LayerReferenceOrder[i] == "head")
                {
                    _defaultGfx.Add(_stats!.GfxHead);
                }
                else
                {
                    _defaultGfx.Add("");
                }
            }
        }

        public void LoadGraphicsFromInventory(MenuInventory? menuInv)
        {
            List<string> previewGfx = new List<string>(_defaultGfx);

            if (SharedGameResources.Items != null && menuInv != null)
            {
                int storageSize = menuInv.Inventory[MenuInventory.Equipment].GetSlotNumber();
                for (int i = 0; i < storageSize; ++i)
                {
                    ItemID itemId = menuInv.Inventory[MenuInventory.Equipment].Storage[i].Item;

                    if (itemId == 0)
                        continue;

                    if (!SharedGameResources.Items!.IsValid(itemId) || !SharedGameResources.Items!.Items[itemId]!.HasName)
                    {
                        // 原始注释：是否需要输出错误提示？（按原始行为保留，此分支�?continue�?
                        continue;
                    }

                    if (!menuInv.IsEquipSlotActive(i))
                    {
                        continue;
                    }

                    if (LayerReferenceOrder.Count != 0)
                    {
                        int found = LayerReferenceOrder.IndexOf(SharedGameResources.Items!.GetItemType(SharedGameResources.Items!.Items[itemId]!.Type).Id);
                        if (found != -1)
                        {
                            int previewIndex = found;
                            if (previewIndex < previewGfx.Count)
                                previewGfx[previewIndex] = SharedGameResources.Items!.Items[itemId]!.Gfx;
                        }
                    }
                }
            }

            LoadGraphics(previewGfx);
        }

        public Animation? ActiveAnimation;
        public AnimationSet? AnimationSet;

        public List<string> LayerReferenceOrder = new List<string>();
        public List<List<int>> LayerDef = new List<List<int>>();
    }
}
