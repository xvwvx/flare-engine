// <自动生成> 对应 C++ 源文件：MenuMiniMap.h + MenuMiniMap.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// 小地图上的一个实体像素点（对应 C++ <c>class PixelEntity</c>）。
    /// 原始版本通过 <c>Color*</c> 指向 <see cref="MenuMiniMap"/> 内的颜色成员；
    /// C# 在构造时按引用复制 <see cref="Color"/> 值。由于 <see cref="MenuMiniMap.FillEntities"/>
    /// 每帧都会重建列表，与原始每帧 <c>new PixelEntity(..., &amp;color_*)</c> 的行为等价。
    /// </summary>
    public class PixelEntity
    {
        public int X;
        public int Y;
        public Color Color;

        public PixelEntity(int x, int y, ref Color color)
        {
            X = x;
            Y = y;
            Color = color;
        }
    }

    /// <summary>
    /// MenuMiniMap
    ///
    /// 屏幕上的小地图菜单，负责预渲染地图瓦片、实时更新实体标记并支持缩放。
    /// <see cref="Menu"/> 是尚未转换的前向依赖单元（<c>Menu.h</c>/<c>Menu.cpp</c>），
    /// 本类按预期的最终符号命名（<c>WindowArea</c>、<c>Align()</c>、<c>ParseMenuKey</c>）引用其成员。
    /// 持有的 <c>Sprite</c>、<c>WidgetLabel</c>、<c>WidgetButton</c> 缓存资源
    /// 通过 <see cref="IDisposable"/> 显式释放，释放顺序与原始析构函数 <c>~MenuMiniMap()</c> 一致。
    /// </summary>
    public class MenuMiniMap : Menu, IDisposable
    {
        private const int TileHero = 1;
        private const int TileEnemy = 2;
        private const int TileNpc = 3;
        private const int TileTeleport = 4;
        private const int TileAlly = 5;

        private Color _colorWall;
        private Color _colorObst;
        private Color _colorHero;
        private Color _colorEnemy;
        private Color _colorAlly;
        private Color _colorNpc;
        private Color _colorTeleport;

        private Sprite? _mapSurface;
        private Sprite? _mapSurface2x;
        private Sprite? _mapSurfaceEntities;
        private Sprite? _mapSurfaceEntities2x;
        private Int2 _mapSize;

        private Rectangle _pos;
        private WidgetLabel? _label;
        private Sprite? _compass;
        private Rectangle _mapArea;
        private WidgetButton? _buttonConfig;

        private float _visibleRadius;
        private int _currentZoom;
        private int _baseZoom;
        private bool _lockZoomChange;

        private readonly List<PixelEntity> _entities = new List<PixelEntity>();

        /// <summary>
        /// 对应 C++ 原始公有字段 <c>clicked_config</c>（无 getter/setter 包装）。
        /// </summary>
        public bool ClickedConfig;

        public MenuMiniMap()
        {
            _colorWall = new Color(128, 128, 128, 255);
            _colorObst = new Color(64, 64, 64, 255);
            _colorHero = new Color(255, 255, 255, 255);
            _colorEnemy = new Color(255, 0, 0, 255);
            _colorAlly = new Color(255, 255, 0, 255);
            _colorNpc = new Color(0, 255, 0, 255);
            _colorTeleport = new Color(0, 191, 255, 255);
            _mapSurface = null;
            _mapSurface2x = null;
            _mapSurfaceEntities = null;
            _mapSurfaceEntities2x = null;
            _label = new WidgetLabel();
            _compass = null;
            _buttonConfig = null;
            _visibleRadius = 0;
            _currentZoom = 1;
            _baseZoom = 1;
            _lockZoomChange = false;
            ClickedConfig = false;

            // Load config settings
            using FileParser infile = new FileParser();
            // @CLASS MenuMiniMap|Description of menus/minimap.txt
            if (infile.Open("menus/minimap.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (ParseMenuKey(infile.Key, infile.Val))
                        continue;

                    // @ATTR map_pos|rectangle|Position and dimensions of the map.
                    if (infile.Key == "map_pos")
                    {
                        _pos = Parse.ToRect(infile.Val);
                    }
                    // @ATTR text_pos|label|Position of the text label with the map name.
                    else if (infile.Key == "text_pos")
                    {
                        _label!.SetFromLabelInfo(Parse.PopLabelInfo(infile.Val));
                    }
                    // @ATTR color_wall|color, int : Color, Alpha|Color used for walls.
                    else if (infile.Key == "color_wall")
                    {
                        _colorWall = Parse.ToRGBA(infile.Val);
                    }
                    // @ATTR color_obst|color, int : Color, Alpha|Color used for small obstacles and pits.
                    else if (infile.Key == "color_obst")
                    {
                        _colorObst = Parse.ToRGBA(infile.Val);
                    }
                    // @ATTR color_hero|color, int : Color, Alpha|Color used for the player character.
                    else if (infile.Key == "color_hero")
                    {
                        _colorHero = Parse.ToRGBA(infile.Val);
                    }
                    // @ATTR color_enemy|color, int : Color, Alpha|Color used for enemies engaged in combat.
                    else if (infile.Key == "color_enemy")
                    {
                        _colorEnemy = Parse.ToRGBA(infile.Val);
                    }
                    // @ATTR color_ally|color, int : Color, Alpha|Color used for allies.
                    else if (infile.Key == "color_ally")
                    {
                        _colorAlly = Parse.ToRGBA(infile.Val);
                    }
                    // @ATTR color_npc|color, int : Color, Alpha|Color used for NPCs.
                    else if (infile.Key == "color_npc")
                    {
                        _colorNpc = Parse.ToRGBA(infile.Val);
                    }
                    // @ATTR color_teleport|color, int : Color, Alpha|Color used for intermap teleports.
                    else if (infile.Key == "color_teleport")
                    {
                        _colorTeleport = Parse.ToRGBA(infile.Val);
                    }
                    // @ATTR button_config|point|Position of the 'Configuration' button. The button will be hidden if not defined.
                    else if (infile.Key == "button_config")
                    {
                        if (_buttonConfig == null)
                        {
                            _buttonConfig = new WidgetButton(WidgetButton.ConfigMenuFile);
                        }
                        Int2 p = Parse.ToPoint(infile.Val);
                        _buttonConfig.SetBasePos(p.X, p.Y, Utils.AlignTopLeft);
                    }
                    // @ATTR default_zoom_level|int|Zoom level of the map when viewed at "1x". By default, this is 1, which equates to each tile being 1 pixel tall.
                    else if (infile.Key == "default_zoom_level")
                    {
                        _baseZoom = Parse.ToInt(infile.Val);
                    }
                    else
                    {
                        infile.Error("MenuMiniMap: '%s' is not a valid key.", infile.Key);
                    }
                }
                infile.Close();
            }

            _visibleRadius = (float)Math.Max(_pos.Width, _pos.Height) * 0.7071f;

            _label!.SetColor(SharedResources.Font!.GetColor(FontEngine.ColorMenuNormal));

            // load compass image
            Image? gfx = null;
            if (SharedResources.Eset!.Tileset.Orientation == EngineSettings.TilesetSettings.TilesetIsometric)
            {
                gfx = SharedResources.RenderDevice!.LoadImage("images/menus/compass_iso.png", RenderDevice.ErrorNormal);
            }
            else if (SharedResources.Eset.Tileset.Orientation == EngineSettings.TilesetSettings.TilesetOrthogonal)
            {
                gfx = SharedResources.RenderDevice!.LoadImage("images/menus/compass_ortho.png", RenderDevice.ErrorNormal);
            }
            if (gfx != null)
            {
                _compass = gfx.CreateSprite();
                gfx.Unref();
            }

            if (_buttonConfig != null)
                _buttonConfig.Tooltip = SharedResources.Msg!.Get("Configuration");

            Align();
        }

        /// <summary>
        /// 对应 C++ 的 <c>~MenuMiniMap()</c>：释放顺序与原始析构函数逐行一致。
        /// C++ 版本在 <c>~MenuMiniMap()</c> 结束后会隐式调用 <c>~Menu()</c>；见转换报告。
        /// </summary>
        public override void Dispose()
        {
            _mapSurface?.Dispose();
            _mapSurface2x?.Dispose();
            _mapSurfaceEntities?.Dispose();
            _mapSurfaceEntities2x?.Dispose();

            _label?.Dispose();
            _compass?.Dispose();
            _buttonConfig?.Dispose();

            ClearEntities();

            base.Dispose();
            GC.SuppressFinalize(this);
        }

        public override void Align()
        {
            base.Align();
            _label!.SetPos(WindowArea.X, WindowArea.Y);

            if (_buttonConfig != null)
                _buttonConfig.SetPos(WindowArea.X, WindowArea.Y);

            _mapArea.X = WindowArea.X + _pos.X;
            _mapArea.Y = WindowArea.Y + _pos.Y;
            _mapArea.Width = _pos.Width;
            _mapArea.Height = _pos.Height;

            // compass
            Int2 compassPos = default;
            compassPos.X = WindowArea.X + _pos.X + _pos.Width - _compass!.GetGraphicsWidth();
            compassPos.Y = _pos.Y + WindowArea.Y;
            _compass!.SetDestFromPoint(compassPos);
        }

        public void SetMapTitle(string mapTitle)
        {
            _label!.SetText(mapTitle);
        }

        private void CreateMapSurface(ref Sprite? targetSurface, int w, int h)
        {
            if (targetSurface != null)
            {
                targetSurface.Dispose();
                targetSurface = null;
            }

            Image? graphics;
            graphics = SharedResources.RenderDevice!.CreateImage(w, h);
            if (graphics != null)
            {
                targetSurface = graphics.CreateSprite();
                targetSurface!.GetGraphics()!.FillWithColor(new Color(0, 0, 0, 0));
                graphics.Unref();
            }
        }

        public void Logic()
        {
            if (!SharedResources.Settings!.ShowHud)
                return;

            InputState inpt = SharedResources.Inpt!;

            if (inpt.Pressing[Input.MinimapMode] && !inpt.Lock[Input.MinimapMode])
            {
                inpt.Lock[Input.MinimapMode] = true;
                SharedResources.Settings.MinimapMode++;
                if (SharedResources.Settings.MinimapMode > Settings.Minimap2x)
                    SharedResources.Settings.MinimapMode = Settings.MinimapNormal;
            }

            if (SharedResources.Settings.MinimapMode == Settings.MinimapHidden)
                return;

            if (inpt.UsingMouse())
            {
                bool isWithinMaparea = Utils.IsWithinRect(_mapArea, inpt.Mouse);

                if (!_lockZoomChange)
                    _lockZoomChange = inpt.Pressing[Input.Main1] && !isWithinMaparea;
                else if (!inpt.Pressing[Input.Main1])
                    _lockZoomChange = false;

                if (isWithinMaparea && inpt.Pressing[Input.Main1] && !inpt.Lock[Input.Main1] && !_lockZoomChange)
                {
                    inpt.Lock[Input.Main1] = true;
                    if (SharedResources.Settings.MinimapMode == Settings.MinimapNormal)
                        SharedResources.Settings.MinimapMode = Settings.Minimap2x;
                    else if (SharedResources.Settings.MinimapMode == Settings.Minimap2x)
                        SharedResources.Settings.MinimapMode = Settings.MinimapNormal;
                }
            }

            if (_buttonConfig != null)
            {
                _buttonConfig.Enabled = !SharedGameResources.Pc!.Stats.Corpse;
                if (!(SharedGameResources.Pc.UsingMain1 || SharedGameResources.Pc.UsingMain2) && _buttonConfig.CheckClick())
                {
                    ClickedConfig = true;
                }
            }
        }

        public override void Render()
        {
        }

        public void Render(Vector2 heroPos)
        {
            if (!SharedResources.Settings!.ShowHud || SharedResources.Settings.MinimapMode == Settings.MinimapHidden)
                return;

            base.Render();

            _label!.Render();

            if (SharedResources.Settings.MinimapMode == Settings.MinimapNormal)
                _currentZoom = 1 * _baseZoom;
            else if (SharedResources.Settings.MinimapMode == Settings.Minimap2x)
                _currentZoom = 2 * _baseZoom;

            RenderMapSurface(heroPos);

            if (_compass != null)
            {
                SharedResources.RenderDevice!.Render(_compass);
            }

            if (_buttonConfig != null)
                _buttonConfig.Render();
        }

        public void Prerender(MapCollision collider, int mapW, int mapH)
        {
            _mapSize.X = mapW;
            _mapSize.Y = mapH;

            if (SharedResources.Eset!.Tileset.Orientation == EngineSettings.TilesetSettings.TilesetIsometric)
            {
                PrerenderIso(collider, ref _mapSurface, ref _mapSurfaceEntities, _baseZoom);
                PrerenderIso(collider, ref _mapSurface2x, ref _mapSurfaceEntities2x, _baseZoom * 2);
            }
            else
            {
                // eset->tileset.TILESET_ORTHOGONAL
                PrerenderOrtho(collider, ref _mapSurface, ref _mapSurfaceEntities, _baseZoom);
                PrerenderOrtho(collider, ref _mapSurface2x, ref _mapSurfaceEntities2x, _baseZoom * 2);
            }
        }

        public void Update(MapCollision collider, ref Rectangle bounds)
        {
            if (SharedResources.Eset!.Tileset.Orientation == EngineSettings.TilesetSettings.TilesetIsometric)
            {
                UpdateIso(collider, ref _mapSurface, _baseZoom, ref bounds);
                UpdateIso(collider, ref _mapSurface2x, _baseZoom * 2, ref bounds);
            }
            else
            {
                // eset->tileset.TILESET_ORTHOGONAL
                UpdateOrtho(collider, ref _mapSurface, _baseZoom, ref bounds);
                UpdateOrtho(collider, ref _mapSurface2x, _baseZoom * 2, ref bounds);
            }
        }

        private void RenderMapSurface(Vector2 heroPos)
        {
            Int2 heroOffset;
            if (SharedResources.Eset!.Tileset.Orientation == EngineSettings.TilesetSettings.TilesetIsometric)
            {
                Int2 hPos = heroPos.ToInt2();
                heroOffset.X = hPos.X - hPos.Y + Math.Max(_mapSize.X, _mapSize.Y);
                heroOffset.Y = hPos.X + hPos.Y;
            }
            else
            {
                // eset->tileset.TILESET_ORTHOGONAL
                heroOffset = heroPos.ToInt2();
            }

            Int2 entityOffset;
            entityOffset.X = (_currentZoom * heroOffset.X) - _pos.Width / 2;
            entityOffset.Y = (_currentZoom * heroOffset.Y) - _pos.Height / 2;

            Rectangle clip = default;
            clip.X = (_currentZoom * heroOffset.X) - _pos.Width / 2;
            clip.Y = (_currentZoom * heroOffset.Y) - _pos.Height / 2;
            clip.Width = _pos.Width;
            clip.Height = _pos.Height;

            Rectangle clipEntities = default;
            clipEntities.X = 0;
            clipEntities.Y = 0;
            clipEntities.Width = _pos.Width;
            clipEntities.Height = _pos.Height;

            Sprite? targetSurface = null;
            Sprite? targetSurfaceEntities = null;
            if (SharedResources.Settings!.MinimapMode == Settings.MinimapNormal && _mapSurface != null)
            {
                targetSurface = _mapSurface;
                targetSurfaceEntities = _mapSurfaceEntities;
            }
            else if (SharedResources.Settings.MinimapMode == Settings.Minimap2x && _mapSurface2x != null)
            {
                targetSurface = _mapSurface2x;
                targetSurfaceEntities = _mapSurfaceEntities2x;
            }

            if (targetSurface != null)
            {
                targetSurface.SetClipFromRect(clip);
                targetSurface.SetDestFromRect(_mapArea);
                SharedResources.RenderDevice!.Render(targetSurface);
            }

            if (targetSurfaceEntities != null)
            {
                if (SharedResources.Eset!.Tileset.Orientation == EngineSettings.TilesetSettings.TilesetIsometric)
                {
                    RenderEntitiesIso(targetSurfaceEntities, _currentZoom, entityOffset);
                }
                else
                {
                    // eset->tileset.TILESET_ORTHOGONAL
                    RenderEntitiesOrtho(targetSurfaceEntities, _currentZoom, entityOffset);
                }

                targetSurfaceEntities.SetClipFromRect(clipEntities);
                targetSurfaceEntities.SetDestFromRect(_mapArea);
                SharedResources.RenderDevice!.Render(targetSurfaceEntities);
            }
        }

        private void PrerenderOrtho(MapCollision collider, ref Sprite? tileSurface, ref Sprite? entitySurface, int zoom)
        {
            int surfaceSize = Math.Max(_mapSize.X + zoom, _mapSize.Y + zoom) * zoom;
            CreateMapSurface(ref tileSurface, surfaceSize, surfaceSize);
            CreateMapSurface(ref entitySurface, _pos.Width, _pos.Height);

            if (tileSurface == null)
                return;

            Rectangle bounds = default;
            bounds.X = 0;
            bounds.Y = 0;
            bounds.Width = _mapSize.X;
            bounds.Height = _mapSize.Y;
            UpdateOrtho(collider, ref tileSurface, zoom, ref bounds);
        }

        private void UpdateOrtho(MapCollision collider, ref Sprite? tileSurface, int zoom, ref Rectangle bounds)
        {
            if (tileSurface == null)
                return;

            Image? targetImg = tileSurface.GetGraphics();

            Color drawColor = default;

            if (bounds.X == 0 && bounds.Y == 0 && bounds.Width == _mapSize.X && bounds.Height == _mapSize.Y)
            {
                targetImg!.BeginPixelBatch();
            }
            else
            {
                Int2 hero = SharedGameResources.Pc!.Stats.Pos.ToInt2();

                Rectangle clip = default;
                clip.X = (zoom * hero.X) - _pos.Width / 2;
                clip.Y = (zoom * hero.Y) - _pos.Height / 2;
                clip.Width = _pos.Width;
                clip.Height = _pos.Height;

                if (clip.X < 0) clip.X = 0;
                if (clip.Y < 0) clip.Y = 0;
                if (clip.X + clip.Width > targetImg!.GetWidth()) clip.Width = targetImg.GetWidth() - clip.X;
                if (clip.Y + clip.Height > targetImg.GetHeight()) clip.Height = targetImg.GetHeight() - clip.Y;

                targetImg.BeginPixelBatch(ref clip);
            }

            for (int i = bounds.X; i < bounds.Width; i++)
            {
                for (int j = bounds.Y; j < bounds.Height; j++)
                {
                    bool drawTile = true;
                    int tileType = collider.Colmap[i][j];

                    if (tileType == 1 || tileType == 5) drawColor = _colorWall;
                    else if (tileType == 2 || tileType == 6) drawColor = _colorObst;
                    else drawTile = false;

                    if (SharedResources.Eset!.Misc.Fogofwar > 0)
                    {
                        tileType = SharedGameResources.Mapr!.Layers[SharedGameResources.Fow!.DarkLayerId][i][j];
                        if (tileType != 0) drawTile = false;
                    }

                    if (drawTile && drawColor.A != 0)
                    {
                        for (int l = 0; l < zoom; l++)
                        {
                            for (int k = 0; k < zoom; k++)
                            {
                                targetImg!.DrawPixel((zoom * i) + k - 1, (zoom * j) + l - 1, drawColor);
                            }
                        }
                    }
                }
            }

            targetImg!.EndPixelBatch();
        }

        private void PrerenderIso(MapCollision collider, ref Sprite? tileSurface, ref Sprite? entitySurface, int zoom)
        {
            int surfaceSize = Math.Max(_mapSize.X + zoom, _mapSize.Y + zoom) * 2 * zoom;
            CreateMapSurface(ref tileSurface, surfaceSize, surfaceSize);
            CreateMapSurface(ref entitySurface, _pos.Width, _pos.Height);

            if (tileSurface == null)
                return;

            Rectangle bounds = default;
            bounds.X = 0;
            bounds.Y = 0;
            bounds.Width = _mapSize.X;
            bounds.Height = _mapSize.Y;
            UpdateIso(collider, ref tileSurface, zoom, ref bounds);
        }

        private void UpdateIso(MapCollision collider, ref Sprite? tileSurface, int zoom, ref Rectangle bounds)
        {
            if (tileSurface == null)
                return;

            Color drawColor = default;
            int tileType;

            Int2 entPos;
            Image? targetImg = tileSurface.GetGraphics();

            if (bounds.X == 0 && bounds.Y == 0 && bounds.Width == _mapSize.X && bounds.Height == _mapSize.Y)
            {
                targetImg!.BeginPixelBatch();
            }
            else
            {
                Int2 hero = SharedGameResources.Pc!.Stats.Pos.ToInt2();
                Int2 heroOffset;
                heroOffset.X = hero.X - hero.Y + Math.Max(_mapSize.X, _mapSize.Y);
                heroOffset.Y = hero.X + hero.Y;

                Rectangle clip = default;
                clip.X = (zoom * heroOffset.X) - _pos.Width / 2;
                clip.Y = (zoom * heroOffset.Y) - _pos.Height / 2;
                clip.Width = _pos.Width;
                clip.Height = _pos.Height;

                if (clip.X < 0) clip.X = 0;
                if (clip.Y < 0) clip.Y = 0;
                if (clip.X + clip.Width > targetImg!.GetWidth()) clip.Width = targetImg.GetWidth() - clip.X;
                if (clip.Y + clip.Height > targetImg.GetHeight()) clip.Height = targetImg.GetHeight() - clip.Y;

                targetImg.BeginPixelBatch(ref clip);
            }

            for (int i = bounds.X; i < bounds.Width; i++)
            {
                for (int j = bounds.Y; j < bounds.Height; j++)
                {
                    tileType = collider.Colmap[i][j];
                    bool drawTile = true;

                    if (tileType == 1 || tileType == 5) drawColor = _colorWall;
                    else if (tileType == 2 || tileType == 6) drawColor = _colorObst;
                    else drawTile = false;

                    // fog of war
                    if (SharedResources.Eset!.Misc.Fogofwar > 0)
                    {
                        tileType = SharedGameResources.Mapr!.Layers[SharedGameResources.Fow!.DarkLayerId][i][j];
                        if (tileType != 0) drawTile = false;
                    }

                    if (drawTile)
                    {
                        entPos.X = zoom * (i - j + Math.Max(_mapSize.X, _mapSize.Y));
                        entPos.Y = zoom * (i + j) - 1;

                        for (int l = 0; l < zoom; l++)
                        {
                            for (int k = 0; k < zoom; k++)
                            {
                                targetImg!.DrawPixel(entPos.X + k, entPos.Y + l, drawColor);
                                targetImg.DrawPixel(entPos.X + k - zoom, entPos.Y + l, drawColor);
                            }
                        }
                    }
                }
            }

            targetImg!.EndPixelBatch();
        }

        private void RenderEntitiesOrtho(Sprite? entitySurface, int zoom, Int2 entityOffset)
        {
            if (entitySurface == null)
                return;

            Image? targetImg = entitySurface.GetGraphics();
            targetImg!.FillWithColor(new Color(0, 0, 0, 0));

            ClearEntities();
            FillEntities();

            targetImg.BeginPixelBatch();

            for (int i = 0; i < _entities.Count; i++)
            {
                for (int l = 0; l < zoom; l++)
                {
                    for (int k = 0; k < zoom; k++)
                    {
                        targetImg.DrawPixel(zoom * _entities[i].X - entityOffset.X + k - 1, zoom * _entities[i].Y - entityOffset.Y + l - 1, _entities[i].Color);
                    }
                }
            }

            targetImg.EndPixelBatch();
        }

        private void RenderEntitiesIso(Sprite? entitySurface, int zoom, Int2 entityOffset)
        {
            if (entitySurface == null)
                return;

            Int2 entPos;

            Image? targetImg = entitySurface.GetGraphics();
            targetImg!.FillWithColor(new Color(0, 0, 0, 0));

            ClearEntities();
            FillEntities();

            targetImg.BeginPixelBatch();

            for (int i = 0; i < _entities.Count; i++)
            {
                entPos.X = zoom * (_entities[i].X - _entities[i].Y + Math.Max(_mapSize.X, _mapSize.Y)) - entityOffset.X;
                entPos.Y = zoom * (_entities[i].X + _entities[i].Y) - entityOffset.Y - 1;

                for (int l = 0; l < zoom; l++)
                {
                    for (int k = 0; k < zoom; k++)
                    {
                        targetImg.DrawPixel(entPos.X + k, entPos.Y + l, _entities[i].Color);
                        targetImg.DrawPixel(entPos.X + k - zoom, entPos.Y + l, _entities[i].Color);
                    }
                }
            }

            targetImg.EndPixelBatch();
        }

        private void ClearEntities()
        {
            for (int i = 0; i < _entities.Count; i++)
            {
                // 对应 C++ delete entities[i]; — PixelEntity 无托管资源，由 GC 回收
            }
            _entities.Clear();
        }

        private void FillEntities()
        {
            Int2 hero = SharedGameResources.Pc!.Stats.Pos.ToInt2();

            if (hero.X >= 0 && hero.Y >= 0 && hero.X < _mapSize.X && hero.Y < _mapSize.Y)
            {
                _entities.Add(new PixelEntity(hero.X, hero.Y, ref _colorHero));
            }

            for (int i = 0; i < SharedGameResources.Mapr!.Events.Count; ++i)
            {
                EventComponent? ecMinimap = SharedGameResources.Mapr.Events[i].GetComponent(EventComponent.ShowOnMinimap);
                if (ecMinimap != null && ecMinimap.Data[0].Int == 0)
                    continue;

                if (SharedGameResources.Mapr.Events[i].GetComponent(EventComponent.NpcHotspot) != null && SharedGameResources.Eventm!.IsActive(SharedGameResources.Mapr.Events[i]))
                {
                    if (SharedGameResources.Mapr.Fogofwar != 0)
                    {
                        float delta = Utils.CalcDist(SharedGameResources.Pc.Stats.Pos, SharedGameResources.Mapr.Events[i].Center);
                        if (delta > SharedGameResources.Fow!.MaskRadius)
                        {
                            continue;
                        }
                    }
                    if (Utils.CalcDist(SharedGameResources.Pc.Stats.Pos, new Vector2(SharedGameResources.Mapr.Events[i].Location.X, SharedGameResources.Mapr.Events[i].Location.Y)) <= _visibleRadius)
                    {
                        _entities.Add(new PixelEntity(SharedGameResources.Mapr.Events[i].Location.X, SharedGameResources.Mapr.Events[i].Location.Y, ref _colorNpc));
                    }
                }
                else if ((SharedGameResources.Mapr.Events[i].ActivateType == Event.ActivateOnTrigger || SharedGameResources.Mapr.Events[i].ActivateType == Event.ActivateOnInteract) && SharedGameResources.Mapr.Events[i].GetComponent(EventComponent.Intermap) != null && SharedGameResources.Eventm.IsActive(SharedGameResources.Mapr.Events[i]))
                {
                    Int2 eventPos = default;
                    eventPos.X = SharedGameResources.Mapr.Events[i].Location.X;
                    eventPos.Y = SharedGameResources.Mapr.Events[i].Location.Y;
                    for (int j = eventPos.X; j < eventPos.X + SharedGameResources.Mapr.Events[i].Location.Width; ++j)
                    {
                        for (int k = eventPos.Y; k < eventPos.Y + SharedGameResources.Mapr.Events[i].Location.Height; ++k)
                        {
                            if (SharedGameResources.Mapr.Fogofwar != 0)
                                if (SharedGameResources.Mapr.Layers[SharedGameResources.Fow!.DarkLayerId][eventPos.X][eventPos.Y] == FogOfWar.TileHidden) continue;

                            if (Utils.CalcDist(SharedGameResources.Pc.Stats.Pos, new Vector2(j, k)) <= _visibleRadius)
                            {
                                _entities.Add(new PixelEntity(j, k, ref _colorTeleport));
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < SharedGameResources.Entitym!.Entities.Count; ++i)
            {
                Entity e = SharedGameResources.Entitym.Entities[i];
                if (e.Stats.Hp > 0)
                {
                    if (SharedGameResources.Mapr.Fogofwar != 0)
                    {
                        float delta = Utils.CalcDist(SharedGameResources.Pc.Stats.Pos, e.Stats.Pos);
                        if (delta > SharedGameResources.Fow!.MaskRadius)
                        {
                            continue;
                        }
                    }
                    if (e.Stats.HeroAlly)
                    {
                        if (Utils.CalcDist(SharedGameResources.Pc.Stats.Pos, new Vector2(e.Stats.Pos.X, e.Stats.Pos.Y)) <= _visibleRadius)
                        {
                            _entities.Add(new PixelEntity((int)e.Stats.Pos.X, (int)e.Stats.Pos.Y, ref _colorAlly));
                        }
                    }
                    else if (e.Stats.InCombat)
                    {
                        if (Utils.CalcDist(SharedGameResources.Pc.Stats.Pos, new Vector2(e.Stats.Pos.X, e.Stats.Pos.Y)) <= _visibleRadius)
                        {
                            _entities.Add(new PixelEntity((int)e.Stats.Pos.X, (int)e.Stats.Pos.Y, ref _colorEnemy));
                        }
                    }
                }
                else if (e.Stats.CorpseHasCollision)
                {
                    if (Utils.CalcDist(SharedGameResources.Pc.Stats.Pos, new Vector2(e.Stats.Pos.X, e.Stats.Pos.Y)) <= _visibleRadius)
                    {
                        _entities.Add(new PixelEntity((int)e.Stats.Pos.X, (int)e.Stats.Pos.Y, ref _colorObst));
                    }
                }
            }
        }
    }
}
