// <自动生成> 对应 C++ 源文件：MapParallax.h + MapParallax.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// MapParallaxLayer
    ///
    /// 单个视差滚动层的数据（对应 C++ 的 <c>class MapParallaxLayer</c>）。
    /// 所有成员在原始 C++ 中均为无附加逻辑的裸公有数据成员（无 get/set 方法），
    /// 依据既有转换约定（参见 output/Hazard.cs、output/Camera.cs 中同类字段的处理方式），
    /// 保留为公有字段而非自动属性：<see cref="FixedSpeed"/>/<see cref="FixedOffset"/>
    /// 在 <see cref="MapParallax.Render"/> 中需要通过 <c>list[i].FixedOffset.X += ...</c>
    /// 的写法原地修改子字段，若改为属性将导致该写法无法通过编译（属性的取值结果不是变量）。
    ///
    /// 前向引用说明（需人工复核，详见转换报告"依赖关系"部分）：
    /// <see cref="Sprite"/> 对应的转换单元（RenderDevice.h/.cpp）尚未转换，这里按其原始
    /// C++ 声明（字段/方法名直译为 PascalCase）前向引用，实际定义将在该单元完成转换时给出。
    /// </summary>
    public class MapParallaxLayer
    {
        public Sprite? Sprite;
        public float Speed;
        public Vector2 FixedSpeed;
        public Vector2 FixedOffset;
        public string MapLayer;

        /// <summary>
        /// 对应 C++ 构造函数初始化列表：<c>sprite(NULL), speed(0), map_layer("")</c>。
        /// 注意：与原始代码一致，fixed_speed/fixed_offset 未在初始化列表中显式赋值，
        /// 而是依赖 FPoint 默认构造函数的零初始化；C# 中 Vector2 字段的默认值同样为 (0,0)，
        /// 因此这里不需要显式赋值即可保持逻辑等价。
        /// </summary>
        public MapParallaxLayer()
        {
            Sprite = null;
            Speed = 0;
            MapLayer = "";
        }
    }

    /// <summary>
    /// MapParallax
    ///
    /// 管理地图的多层视差滚动背景（对应 C++ 的 <c>class MapParallax</c>）。
    ///
    /// 资源管理：<see cref="_layers"/> 中每一项持有的 <see cref="MapParallaxLayer.Sprite"/>
    /// 对应原始裸指针成员，其生命周期由本类手动管理（构造时为 NULL，在 <see cref="Clear"/>
    /// 中 <c>delete</c>）；C# 版本据此实现 <see cref="IDisposable"/>，在 <see cref="Dispose"/>
    /// 中调用 <see cref="Clear"/>，对应原始析构函数 <c>~MapParallax()</c> 的释放顺序。
    ///
    /// 前向引用说明（需人工复核，详见转换报告"依赖关系"部分）：
    /// <see cref="Sprite"/>、<see cref="Image"/>、<see cref="RenderDevice"/> 对应的转换单元
    /// （RenderDevice.h/.cpp）尚未转换，这里按其原始 C++ 声明（字段/方法名直译为 PascalCase）
    /// 前向引用；命名与调用约定参考了 output/GameSwitcher.cs、output/WidgetSlider.cs 中对
    /// 同一组尚未转换类型的既有前向引用方式（如 <c>RenderDevice.ErrorNormal</c>、
    /// <c>Image.CreateSprite()</c>/<c>Image.Unref()</c>、<c>Sprite.GetGraphicsWidth()</c>/
    /// <c>Sprite.GetGraphicsHeight()</c>、<c>Sprite.SetDest(int,int)</c>），以保持跨单元的一致性。
    /// </summary>
    public class MapParallax : IDisposable
    {
        private List<MapParallaxLayer> _layers = new List<MapParallaxLayer>();
        private Vector2 _mapCenter;
        private int _currentLayer;
        private bool _loaded;
        private string _currentFilename;

        /// <summary>
        /// 对应 C++ 构造函数初始化列表：<c>current_layer(0), loaded(false), current_filename("")</c>。
        /// </summary>
        public MapParallax()
        {
            _currentLayer = 0;
            _loaded = false;
            _currentFilename = "";
        }

        /// <summary>
        /// 对应 C++ 析构函数 <c>~MapParallax()</c>：<c>clear();</c>。
        /// </summary>
        public void Dispose()
        {
            Clear();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 对应 C++ <c>void MapParallax::clear()</c>。
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < _layers.Count; ++i)
            {
                _layers[i].Sprite?.Dispose();
            }

            _layers.Clear();

            _loaded = false;
        }

        /// <summary>
        /// 对应 C++ <c>void MapParallax::load(const std::string&amp; filename)</c>。
        /// </summary>
        public void Load(string filename)
        {
            _currentFilename = filename;

            if (_loaded)
                Clear();

            if (!SharedResources.Settings!.ParallaxLayers)
                return;

            // @CLASS MapParallax|Description of maps/parallax/
            using FileParser infile = new FileParser();
            if (infile.Open(filename, FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (infile.NewSection && infile.Section == "layer")
                    {
                        _layers.Add(new MapParallaxLayer());
                    }

                    if (_layers.Count == 0)
                        continue;

                    if (infile.Key == "image")
                    {
                        // @ATTR layer.image|filename|Image file to use as a scrolling background.
                        Image? graphics = SharedResources.RenderDevice!.LoadImage(infile.Val, RenderDevice.ErrorNormal);
                        if (graphics != null)
                        {
                            _layers[^1].Sprite = graphics.CreateSprite();
                            graphics.Unref();
                        }
                    }
                    else if (infile.Key == "speed")
                    {
                        // @ATTR layer.speed|float|Speed at which the background will move relative to the camera.
                        _layers[^1].Speed = (Settings.LogicFps * Parse.ToFloat(infile.Val)) / SharedResources.Settings!.MaxFramesPerSec;
                    }
                    else if (infile.Key == "fixed_speed")
                    {
                        // @ATTR layer.fixed_speed|float, float : X speed, Y speed|Speed at which the background will move independent of the camera movement.
                        _layers[^1].FixedSpeed.X = (Settings.LogicFps * Parse.PopFirstFloat(ref infile.Val)) / SharedResources.Settings!.MaxFramesPerSec;
                        _layers[^1].FixedSpeed.Y = (Settings.LogicFps * Parse.PopFirstFloat(ref infile.Val)) / SharedResources.Settings!.MaxFramesPerSec;
                    }
                    else if (infile.Key == "map_layer")
                    {
                        // @ATTR layer.map_layer|string|The tile map layer that this parallax layer will be rendered on top of.
                        _layers[^1].MapLayer = infile.Val;
                    }
                }

                infile.Close();
            }

            _loaded = true;
        }

        /// <summary>
        /// 对应 C++ <c>void MapParallax::setMapCenter(int x, int y)</c>。
        /// </summary>
        public void SetMapCenter(int x, int y)
        {
            _mapCenter.X = (float)x + 0.5f;
            _mapCenter.Y = (float)y + 0.5f;
        }

        /// <summary>
        /// 对应 C++ <c>void MapParallax::render(const FPoint&amp; cam, const std::string&amp; map_layer)</c>。
        /// </summary>
        public void Render(Vector2 cam, string mapLayer)
        {
            Settings settings = SharedResources.Settings!;

            if (!settings.ParallaxLayers)
            {
                if (_loaded)
                    Clear();

                return;
            }
            else if (!_loaded)
            {
                Load(_currentFilename);
            }

            if (mapLayer.Length == 0)
                _currentLayer = 0;

            for (int i = _currentLayer; i < _layers.Count; ++i)
            {
                if (_layers[i].MapLayer != mapLayer)
                    continue;

                int width = _layers[i].Sprite!.GetGraphicsWidth();
                int height = _layers[i].Sprite!.GetGraphicsHeight();

                _layers[i].FixedOffset.X += _layers[i].FixedSpeed.X;
                _layers[i].FixedOffset.Y += _layers[i].FixedSpeed.Y;

                if (_layers[i].FixedOffset.X > (float)width)
                    _layers[i].FixedOffset.X -= (float)width;
                if (_layers[i].FixedOffset.X < (float)(-width))
                    _layers[i].FixedOffset.X += (float)width;

                if (_layers[i].FixedOffset.Y > (float)height)
                    _layers[i].FixedOffset.Y -= (float)height;
                if (_layers[i].FixedOffset.Y < (float)(-height))
                    _layers[i].FixedOffset.Y += (float)height;

                Vector2 dp;
                dp.X = _mapCenter.X - cam.X;
                dp.Y = _mapCenter.Y - cam.Y;

                Int2 centerTile = Utils.MapToScreen(_mapCenter.X + (dp.X * _layers[i].Speed) + _layers[i].FixedOffset.X, _mapCenter.Y + (dp.Y * _layers[i].Speed) + _layers[i].FixedOffset.Y, cam.X, cam.Y);
                centerTile.X -= width / 2;
                centerTile.Y -= height / 2;

                Int2 drawPos;
                drawPos.X = centerTile.X - (int)MathF.Ceiling((float)(settings.ViewWHalf + centerTile.X) / (float)width) * width;
                drawPos.Y = centerTile.Y - (int)MathF.Ceiling((float)(settings.ViewHHalf + centerTile.Y) / (float)height) * height;
                Int2 startPos = drawPos;

                while (drawPos.X < settings.ViewW)
                {
                    drawPos.Y = startPos.Y;
                    while (drawPos.Y < settings.ViewH)
                    {
                        _layers[i].Sprite!.SetDest(drawPos.X, drawPos.Y);
                        SharedResources.RenderDevice!.Render(_layers[i].Sprite!);

                        drawPos.Y += height;
                    }
                    drawPos.X += width;
                }

                _currentLayer++;
            }
        }
    }
}
