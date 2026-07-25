// <自动生成> 对应 C++ 源文件：IconManager.h + IconManager.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// IconSet
    ///
    /// 代表一个已加载的图标图集：引用图集精灵图（<see cref="Gfx"/>），以及该图集覆盖的
    /// 图标 ID 区间 [<see cref="IdBegin"/>, <see cref="IdEnd"/>] 与每行的图标列数
    /// （<see cref="Columns"/>）。
    ///
    /// C++ 原始实现中 <c>Sprite *gfx</c> 由 <c>IconManager</c> 独占所有权（在
    /// <c>IconManager::~IconManager()</c> 中手动 delete）；C# 版本中 <see cref="Gfx"/>
    /// 保持为可空引用字段，由 <see cref="IconManager"/> 负责通过 IDisposable 释放
    /// （详见转换报告）。
    /// </summary>
    public class IconSet
    {
        public Sprite? Gfx;
        public int IdBegin;
        public int IdEnd;
        public int Columns;

        public IconSet()
        {
            Gfx = null;
            IdBegin = 0;
            IdEnd = 0;
            Columns = 1;
        }
    }

    /// <summary>
    /// IconManager
    ///
    /// 管理一个或多个图标图集（icon set）的加载，并负责在渲染时根据图标 ID 定位到
    /// 正确的图集与源矩形区域，随后交给渲染设备绘制或渲染到离屏图像。图标图集通过
    /// engine/icons.txt 配置加载；若该文件不存在，则回退为加载单个 legacy 图集
    /// images/icons/icons.png。
    ///
    /// C++ 原始实现中 <c>icon_sets</c> 内每个 <c>IconSet</c> 独占一个 <c>Sprite*</c>，
    /// 在析构函数中按正向遍历顺序手动 delete；C# 版本实现 <see cref="IDisposable"/>，
    /// 在 <see cref="Dispose"/> 中保持相同的遍历顺序释放。
    /// </summary>
    public class IconManager : IDisposable
    {
        private List<IconSet> _iconSets = new List<IconSet>();
        private IconSet? _currentSet;
        private Rectangle _currentSrc;
        private Rectangle _currentDest;

        // C++: Point text_offset;（公有字段，无 getter/setter 包装，原样保留为公有字段）
        public Int2 TextOffset;

        public IconManager()
        {
            _currentSet = null;

            using FileParser infile = new FileParser();

            // @CLASS IconManager|Description of engine/icons.txt
            if (infile.Open("engine/icons.txt", FileParser.ModFile, FileParser.ErrorNone))
            {
                while (infile.Next())
                {
                    if (infile.Key == "icon_set")
                    {
                        // @ATTR icon_set|repeatable(icon_id, filename) : First ID, Image file|Defines an icon graphics file to load, as well as the index of the first icon.
                        int firstId = Parse.PopFirstInt(ref infile.Val);
                        string filename = Parse.PopFirstString(ref infile.Val);

                        _iconSets.Add(new IconSet());
                        if (!LoadIconSet(_iconSets[_iconSets.Count - 1], filename, firstId))
                        {
                            _iconSets.RemoveAt(_iconSets.Count - 1);
                        }
                    }
                    else if (infile.Key == "text_offset")
                    {
                        // @ATTR text_offset|point|A pixel offset from the top-left to place item quantity text on icons.
                        TextOffset = Parse.ToPoint(infile.Val);
                    }
                }
                infile.Close();
            }

            if (_iconSets.Count == 0)
            {
                // no icons.txt file, so load icons.png legacy-style
                _iconSets.Add(new IconSet());
                if (!LoadIconSet(_iconSets[_iconSets.Count - 1], "images/icons/icons.png", 0))
                {
                    _iconSets.RemoveAt(_iconSets.Count - 1);
                }
            }
        }

        /// <summary>
        /// 对应 C++ 的 <c>~IconManager()</c>：按照原始析构函数的顺序（正向遍历 icon_sets）
        /// 释放每个 IconSet 持有的 Sprite 资源。
        /// </summary>
        public void Dispose()
        {
            Utils.LogInfo("Cleaning up: IconManager");

            for (int i = 0; i < _iconSets.Count; ++i)
            {
                _iconSets[i].Gfx?.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        private bool LoadIconSet(IconSet iconSet, string filename, int firstId)
        {
            if (SharedResources.RenderDevice == null || SharedResources.Eset!.Resolutions.IconSize == 0)
                return false;

            Image? graphics = SharedResources.RenderDevice!.LoadImage(filename, RenderDevice.ErrorNormal);
            if (graphics != null)
            {
                iconSet.Gfx = graphics.CreateSprite();
                graphics.Unref();
            }

            if (iconSet.Gfx != null)
            {
                int rows = iconSet.Gfx.GetGraphicsHeight() / SharedResources.Eset.Resolutions.IconSize;
                iconSet.Columns = iconSet.Gfx.GetGraphicsWidth() / SharedResources.Eset.Resolutions.IconSize;

                if (iconSet.Columns == 0)
                {
                    // prevent divide-by-zero
                    iconSet.Columns = 1;
                }

                iconSet.IdBegin = firstId;
                iconSet.IdEnd = firstId + (iconSet.Columns * rows) - 1;

                return true;
            }

            return false;
        }

        public void SetIcon(int iconId, Int2 destPos)
        {
            if (_iconSets.Count == 0)
            {
                _currentSet = null;
                return;
            }

            for (int i = _iconSets.Count; i > 0; --i)
            {
                // we iterate backwards through the set list, since sets at the end have priority when sets overlap
                if (iconId >= _iconSets[i - 1].IdBegin && iconId <= _iconSets[i - 1].IdEnd)
                {
                    _currentSet = _iconSets[i - 1];
                    break;
                }
                else if (i - 1 == 0)
                {
                    // we've reached the end of the set list, but could not find our icon
                    _currentSet = null;
                    return;
                }
            }

            int offsetId = iconId - _currentSet!.IdBegin;
            _currentSrc.X = (offsetId % _currentSet.Columns) * SharedResources.Eset!.Resolutions.IconSize;
            _currentSrc.Y = (offsetId / _currentSet.Columns) * SharedResources.Eset.Resolutions.IconSize;
            _currentSrc.Width = _currentSrc.Height = SharedResources.Eset.Resolutions.IconSize;
            _currentSet.Gfx!.SetClipFromRect(_currentSrc);

            _currentDest.X = destPos.X;
            _currentDest.Y = destPos.Y;
            _currentSet.Gfx.SetDestFromRect(_currentDest);
        }

        public void RenderToImage(Image? img)
        {
            if (_currentSet == null)
                return;

            if (img != null)
            {
                SharedResources.RenderDevice!.RenderToImage(_currentSet.Gfx!.GetGraphics()!, _currentSrc, img, _currentDest);
            }
        }

        public void Render()
        {
            if (_currentSet == null)
                return;

            SharedResources.RenderDevice!.Render(_currentSet.Gfx!);
        }
    }
}
