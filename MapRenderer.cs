// <自动生成> 对应 C++ 源文件：MapRenderer.h + MapRenderer.cpp
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// ??/已全局导入，此处省略。C++  priocompare?calculatePriosIso?calculatePriosOrtho???
    /// </summary>
    internal static class MapRendererRenderHelpers
    {
        public static int Priocompare(Renderable r1, Renderable r2)
        {
            return r1.Prio.CompareTo(r2.Prio);
        }

        public static void CalculatePriosIso(List<Renderable> r)
        {
            for (int it = 0; it < r.Count; ++it)
            {
                uint tilex = (uint)MathF.Floor(r[it].MapPos.X);
                uint tiley = (uint)MathF.Floor(r[it].MapPos.Y);
                int commax = (int)((r[it].MapPos.X - (float)tilex) * (1 << 10));
                int commay = (int)((r[it].MapPos.Y - (float)tiley) * (1 << 10));
                r[it].Prio += ((ulong)(tilex + tiley) << 37) + ((ulong)tilex << 20) + ((ulong)(commax + commay) << 8);
            }
        }

        public static void CalculatePriosOrtho(List<Renderable> r)
        {
            for (int it = 0; it < r.Count; ++it)
            {
                uint tilex = (uint)MathF.Floor(r[it].MapPos.X);
                uint tiley = (uint)MathF.Floor(r[it].MapPos.Y);
                int commay = (int)(r[it].MapPos.Y * (1 << 10));
                r[it].Prio += ((ulong)tiley << 37) + ((ulong)tilex << 20) + ((ulong)commay << 8);
            }
        }
    }

    /// <summary>
    /// MapRenderer - 地图渲染引擎，对应 C++ class MapRenderer : public Map。
    /// 负责地图瓦片、实体、事件区域的渲染，管理 WidgetTooltip 等 UI 覆盖层。
    /// </summary>
    public class MapRenderer : Map, IDisposable
    {
        /// <summary>地图掉落物定义，对应 C++ typedef std::pair&lt;vector&lt;EventComponent&gt;, Point&gt; MapLoot。</summary>
        public class MapLoot
        {
            public List<EventComponent> Components = new List<EventComponent>();
            public Int2 Point;

            // 对应 C++ std::pair ? first/second 
            public List<EventComponent> First => Components;
            public Int2 Second { get => Point; set => Point = value; }
        }

        public const uint ProcgenChunkSize = 32;

        private WidgetTooltip _tip;
        private TooltipData _tipBuf = new TooltipData();
        private Int2 _tipPos;
        private bool _showTooltip;
        private bool _drawnHero;
        private Rectangle _heroBounds;

        private TileSet _tset = new TileSet();
        private MapParallax _mapParallax = new MapParallax();
        private List<int> _hiddenEntities = new List<int>();

        public Camera Cam = new Camera();
        public bool MapChange;
        public MapCollision Collider = new MapCollision();
        public List<MapLoot> Loot = new List<MapLoot>();

        public bool Teleportation;
        public Vector2 TeleportDestination;
        public int TeleportDestinationId;
        public string TeleportMapname = "";
        public string RespawnMap = "";
        public Vector2 RespawnPoint;

        public bool Cutscene;
        public string CutsceneFile = "";

        public bool Stash;
        public Vector2 StashPos;

        public bool EnemiesCleared;

        public string EventNpc = "";

        public bool SaveGame;

        public List<SoundID> Sids = new List<SoundID>();

        public int NpcId;

        public string ShowBook = "";

        public uint IndexObjectlayer;

        public bool IsSpawnMap;

        private bool _disposed;

        public MapRenderer()
        {
            _tip = new WidgetTooltip();
            _tipPos = default;
            _showTooltip = false;
            _drawnHero = false;
            Cam = new Camera();
            MapChange = false;
            Teleportation = false;
            TeleportDestination = default;
            TeleportDestinationId = 0;
            RespawnPoint = default;
            Cutscene = false;
            CutsceneFile = "";
            Stash = false;
            StashPos = default;
            EnemiesCleared = false;
            SaveGame = false;
            NpcId = -1;
            ShowBook = "";
            IndexObjectlayer = 0;
            IsSpawnMap = false;
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;

            Utils.LogInfo("Cleaning up: MapRenderer");

            _tipBuf.Clear();
            ClearMapRendererLayers();
            ClearEvents();
            ClearObjects();

            _tip.Dispose();
            _tset.Dispose();
            _mapParallax.Dispose();

            var snd = SharedResources.Snd!;

            snd.Reset();
            while (Sids.Count > 0)
            {
                snd.Unload(Sids[^1]);
                Sids.RemoveAt(Sids.Count - 1);
            }

            GC.SuppressFinalize(this);
        }

        private void ClearObjects()
        {
            Enemies = new Queue<MapEnemy>();
            EnemyGroups.Clear();
            MapNpcs.Clear();
            Loot.Clear();
        }

        private bool EnemyGroupPlaceEnemy(float x, float y, MapGroup g)
        {
            if (Collider.IsValidPosition(x, y, MapCollision.MoveNormal, MapCollision.CollideTypeNone))
            {
                EnemyLevel enemyLev = SharedGameResources.Enemyg!.GetRandomEnemy(g.Category, g.Levelmin, g.Levelmax);
                if (enemyLev.Type.Length > 0)
                {
                    MapEnemy groupMember = new MapEnemy(enemyLev.Type, new Vector2(x, y));

                    groupMember.Direction = (g.Direction == -1 ? Program.Rng.Next() % 8 : g.Direction);
                    groupMember.WanderRadius = g.WanderRadius;
                    groupMember.Requirements = g.Requirements;
                    groupMember.InvincibleRequirements = g.InvincibleRequirements;

                    if (g.Area.X == 1 && g.Area.Y == 1)
                    {
                        for (int i = 0; i < g.Waypoints.Count; ++i)
                        {
                            groupMember.Waypoints.Enqueue(g.Waypoints[i]);
                        }
                    }

                    groupMember.SpawnLevel = g.SpawnLevel;

                    Enemies.Enqueue(groupMember);
                }
                return true;
            }
            return false;
        }

        private void PushEnemyGroup(MapGroup g)
        {
            if (!MathUtils.PercentChanceF(g.Chance))
            {
                return;
            }

            int enemiesToSpawn = MathUtils.RandBetween(g.Numbermin, g.Numbermax);

            int allowedMisses = 5 * g.Numbermax;

            while (enemiesToSpawn > 0 && allowedMisses > 0)
            {
                float x = (g.Area.X == 0) ? ((float)g.Pos.X + 0.5f) : ((float)(g.Pos.X + (Program.Rng.Next() % g.Area.X))) + 0.5f;
                float y = (g.Area.Y == 0) ? ((float)g.Pos.Y + 0.5f) : ((float)(g.Pos.Y + (Program.Rng.Next() % g.Area.Y))) + 0.5f;

                if (EnemyGroupPlaceEnemy(x, y, g))
                    enemiesToSpawn--;
                else
                    allowedMisses--;
            }
            if (enemiesToSpawn > 0)
            {
                for (int x = g.Pos.X; x < g.Pos.X + g.Area.X && enemiesToSpawn > 0; x++)
                {
                    for (int y = g.Pos.Y; y < g.Pos.Y + g.Area.Y && enemiesToSpawn > 0; y++)
                    {
                        float xpos = (float)x + 0.5f;
                        float ypos = (float)y + 0.5f;
                        if (EnemyGroupPlaceEnemy(xpos, ypos, g))
                            enemiesToSpawn--;
                    }
                }
            }
            if (enemiesToSpawn > 0)
            {
                Utils.LogError("MapRenderer: Could not spawn all enemies in group at %s (x=%d,y=%d,w=%d,h=%d), %d missing (min=%d max=%d)",
                    Filename, g.Pos.X, g.Pos.Y, g.Area.X, g.Area.Y, enemiesToSpawn, g.Numbermin, g.Numbermax);
            }
        }

        private void ClearMapRendererLayers()
        {
            Layers.Clear();
            Layernames.Clear();
            LayernamesHashed.Clear();
            IndexObjectlayer = 0;
        }

        public new int Load(string fname)
        {
            var snd = SharedResources.Snd!;
            var comb = SharedResources.Comb!;
            var powers = SharedGameResources.Powers!;
            var fow = SharedGameResources.Fow!;

            snd.Reset();
            while (Sids.Count > 0)
            {
                snd.Unload(Sids[^1]);
                Sids.RemoveAt(Sids.Count - 1);
            }

            while (powers.MapEnemies.Count > 0)
            {
                powers.MapEnemies.Dequeue();
            }

            comb.Clear();

            _showTooltip = false;
            IsSpawnMap = (fname == "maps/spawn.txt");

            base.Load(fname);

            LoadMusic();

            for (uint i = 0; i < Layers.Count; ++i)
            {
                if (Layernames[(int)i] == "collision")
                {
                    short width = (short)Layers[(int)i].Count;
                    if (width == 0)
                    {
                        Utils.LogError("MapRenderer: Map width is 0. Can't set collision layer.");
                        break;
                    }
                    short height = (short)Layers[(int)i][0].Count;
                    Collider.SetMap(Layers[(int)i], (ushort)width, (ushort)height);
                    RemoveLayer(i);
                }
            }
            for (uint i = 0; i < Layers.Count; ++i)
            {
                if (Layernames[(int)i] == "object")
                    IndexObjectlayer = i;
            }
            if (Fogofwar != 0)
            {
                for (ushort i = 0; i < Layers.Count; ++i)
                {
                    if (Layernames[i] == "fow_dark")
                        fow.DarkLayerId = i;
                    if (Layernames[i] == "fow_fog")
                        fow.FogLayerId = i;
                }
            }

            for (int i = 0; i < EnemyGroups.Count; ++i)
            {
                PushEnemyGroup(EnemyGroups[i]);
            }

            _tset.Load(Tileset);

            List<uint> corrupted = new List<uint>();
            for (uint i = 0; i < Layers.Count; ++i)
            {
                for (uint x = 0; x < Layers[(int)i].Count; ++x)
                {
                    for (uint y = 0; y < Layers[(int)i][(int)x].Count; ++y)
                    {
                        uint tileId = Layers[(int)i][(int)x][(int)y];
                        TileSet tileSet = _tset;

                        if (Fogofwar == FogOfWar.TypeOverlay)
                        {
                            if (i == fow.DarkLayerId) tileSet = fow.TsetDark;
                            if (i == fow.FogLayerId) tileSet = fow.TsetFog;
                        }
                        if (Fogofwar != 0)
                        {
                            if (i == fow.DarkLayerId || i == fow.FogLayerId)
                                continue;
                        }

                        if (tileId > 0 && (tileId >= tileSet.Tiles.Count || tileSet.Tiles[(int)tileId].Tile == null))
                        {
                            if (!corrupted.Contains(tileId))
                            {
                                corrupted.Add(tileId);
                            }
                            Layers[(int)i][(int)x][(int)y] = 0;
                        }
                    }
                }
            }

            if (corrupted.Count > 0)
            {
                Utils.LogError("MapRenderer: Tileset or Map corrupted. A tile has a larger id than the tileset allows or is undefined.");
                while (corrupted.Count > 0)
                {
                    Utils.LogError("MapRenderer: Removing offending tile id %d.", corrupted[^1]);
                    corrupted.RemoveAt(corrupted.Count - 1);
                }
            }

            SetMapParallax(ParallaxFilename);

            SharedResources.RenderDevice!.SetBackgroundColor(BackgroundColor);

            return 0;
        }

        public void LoadMusic()
        {
            var settings = SharedResources.Settings!;
            var snd = SharedResources.Snd!;

            if (!settings.Audio) return;

            if (settings.MusicVolume > 0)
            {
                snd.LoadMusic(MusicFilename);
            }
            else
            {
                snd.StopMusic();
            }
        }

        public void Logic(bool paused)
        {
            var fow = SharedGameResources.Fow!;

            if (Fogofwar != 0)
            {
                fow.Logic();
            }

            _tset.Logic();
            if (Fogofwar == FogOfWar.TypeOverlay)
            {
                fow.TsetDark.Logic();
                fow.TsetFog.Logic();
            }

            if (paused)
                return;

            for (uint i = 0; i < Statblocks.Count; ++i)
            {
                for (int j = 0; j < Statblocks[(int)i].PowersAi.Count; ++j)
                {
                    Statblocks[(int)i].PowersAi[j].Cooldown.Tick();
                }
            }

            for (int it = Events.Count; it > 0; )
            {
                --it;
                if (!Events[it].Delay.IsEnd())
                    Events[it].Delay.Tick();
                else
                    Events[it].Cooldown.Tick();
            }

            for (int it = DelayedEvents.Count; it > 0; )
            {
                --it;

                DelayedEvents[it].Delay.Tick();

                if (DelayedEvents[it].Delay.IsEnd())
                {
                    SharedGameResources.Eventm!.ExecuteDelayedEvent(DelayedEvents[it]);
                    DelayedEvents.RemoveAt(it);
                }
            }

            Cam.Logic();
        }

        public void Render(List<Renderable> r, List<Renderable> rDead)
        {
            _drawnHero = false;

            _mapParallax.Render(Cam.Shake, "");

            _heroBounds = new Rectangle();
            for (int i = 0; i < r.Count; ++i)
            {
                if (r[i].Type == Renderable.TypeHero)
                {
                    Int2 p = Utils.MapToScreen(r[i].MapPos.X, r[i].MapPos.Y, Cam.Shake.X, Cam.Shake.Y);
                    p.X -= r[i].Offset.X;
                    p.Y -= r[i].Offset.Y;
                    Rectangle rClip = r[i].Src;

                    if (p.X < _heroBounds.X || _heroBounds.Width == 0)
                    {
                        _heroBounds.X = p.X;
                    }
                    if (p.X + rClip.Width > _heroBounds.X + _heroBounds.Width || _heroBounds.Width == 0)
                    {
                        _heroBounds.Width = p.X + rClip.Width - _heroBounds.X;
                    }
                    if (p.Y < _heroBounds.Y || _heroBounds.Height == 0)
                    {
                        _heroBounds.Y = p.Y;
                    }
                    if (p.Y + rClip.Height > _heroBounds.Y + _heroBounds.Width || _heroBounds.Height == 0)
                    {
                        _heroBounds.Height = p.Y + rClip.Height - _heroBounds.Y;
                    }
                }
            }

            if (SharedResources.Eset!.Tileset.Orientation == EngineSettings.TilesetSettings.TilesetOrthogonal)
            {
                MapRendererRenderHelpers.CalculatePriosOrtho(r);
                MapRendererRenderHelpers.CalculatePriosOrtho(rDead);
                r.Sort(MapRendererRenderHelpers.Priocompare);
                rDead.Sort(MapRendererRenderHelpers.Priocompare);
                RenderOrtho(r, rDead);
            }
            else
            {
                MapRendererRenderHelpers.CalculatePriosIso(r);
                MapRendererRenderHelpers.CalculatePriosIso(rDead);
                r.Sort(MapRendererRenderHelpers.Priocompare);
                rDead.Sort(MapRendererRenderHelpers.Priocompare);
                RenderIso(r, rDead);
            }
        }

        private void DrawRenderable(List<Renderable> r, int rCursor)
        {
            if (r[rCursor].Image != null)
            {
                Rectangle dest = default;
                Int2 p = Utils.MapToScreen(r[rCursor].MapPos.X, r[rCursor].MapPos.Y, Cam.Shake.X, Cam.Shake.Y);
                dest.X = p.X - r[rCursor].Offset.X;
                dest.Y = p.Y - r[rCursor].Offset.Y;
                SharedResources.RenderDevice!.Render(r[rCursor], ref dest);

                if (r[rCursor].Type == Renderable.TypeHero)
                {
                    _drawnHero = true;
                }
            }
        }

        private void RenderIsoLayer(List<List<ushort>> layerdata, TileSet tileSet)
        {
            short i;
            short j;
            Int2 dest;

            var eset = SharedResources.Eset!;
            var settings = SharedResources.Settings!;
            var renderDevice = SharedResources.RenderDevice!;
            var fow = SharedGameResources.Fow!;

            Int2 upperleft = Utils.ScreenToMap(0, 0, Cam.Shake.X, Cam.Shake.Y).ToInt2();
            short maxTilesWidth = (short)((settings.ViewW / eset.Tileset.TileW) + 2 * _tset.MaxSizeX);
            short maxTilesHeight = (short)((2 * settings.ViewH / eset.Tileset.TileH) + 2 * (_tset.MaxSizeY + 1));

            j = (short)(upperleft.Y - _tset.MaxSizeY / 2 + _tset.MaxSizeX);
            i = (short)(upperleft.X - _tset.MaxSizeY / 2 - _tset.MaxSizeX);

            for (ushort y = (ushort)maxTilesHeight; y > 0; --y)
            {
                short tilesWidth = 0;

                if (i < -1)
                {
                    j = (short)(j + i + 1);
                    tilesWidth = (short)(tilesWidth - (i + 1));
                    i = -1;
                }
                short d = (short)(j - H);
                if (d >= 0)
                {
                    j = (short)(j - d);
                    tilesWidth = (short)(tilesWidth + d);
                    i = (short)(i + d);
                }

                short jEnd = Math.Max((short)(j + i - W + 1), Math.Max((short)(j - maxTilesWidth), (short)0));

                Int2 p = Utils.MapToScreen(i, j, Cam.Shake.X, Cam.Shake.Y);
                p = CenterTile(p);

                while (j > jEnd)
                {
                    --j;
                    ++i;
                    ++tilesWidth;
                    p.X += eset.Tileset.TileW;

                    ushort currentTile = layerdata[i][j];
                    if (currentTile != 0)
                    {
                        TileDef tile = tileSet.Tiles[currentTile];
                        if (tile.Tile != null)
                        {
                            dest.X = p.X - tile.Offset.X;
                            dest.Y = p.Y - tile.Offset.Y;

                            if (Fogofwar == FogOfWar.TypeOverlay)
                            {
                                if (!ReferenceEquals(layerdata, Layers[(int)fow.DarkLayerId]))
                                {
                                    if (Layers[(int)fow.DarkLayerId][i][j] == FogOfWar.TileHidden)
                                    {
                                        Int2 tL = Utils.ScreenToMap(dest.X, dest.Y, Cam.Shake.X, Cam.Shake.Y).ToInt2();
                                        Int2 tR = Utils.ScreenToMap(dest.X + tile.Tile.GetClip().Width, dest.Y, Cam.Shake.X, Cam.Shake.Y).ToInt2();
                                        Int2 bL = Utils.ScreenToMap(dest.X, dest.Y + tile.Tile.GetClip().Height, Cam.Shake.X, Cam.Shake.Y).ToInt2();
                                        Int2 bR = Utils.ScreenToMap(dest.X + tile.Tile.GetClip().Width, dest.Y + tile.Tile.GetClip().Height, Cam.Shake.X, Cam.Shake.Y).ToInt2();

                                        if (tL.X < 0) tL.X = 0;
                                        if (tL.X >= W) tL.X = W - 1;
                                        if (tL.Y < 0) tL.Y = 0;
                                        if (tL.Y >= H) tL.Y = H - 1;

                                        if (tR.X < 0) tR.X = 0;
                                        if (tR.X >= W) tR.X = W - 1;
                                        if (tR.Y < 0) tR.Y = 0;
                                        if (tR.Y >= H) tR.Y = H - 1;

                                        if (bL.X < 0) bL.X = 0;
                                        if (bL.X >= W) bL.X = W - 1;
                                        if (bL.Y < 0) bL.Y = 0;
                                        if (bL.Y >= H) bL.Y = H - 1;

                                        if (bR.X < 0) bR.X = 0;
                                        if (bR.X >= W) bR.X = W - 1;
                                        if (bR.Y < 0) bR.Y = 0;
                                        if (bR.Y >= H) bR.Y = H - 1;

                                        if (Layers[(int)fow.DarkLayerId][tL.X][tL.Y] == FogOfWar.TileHidden)
                                        {
                                            if (Layers[(int)fow.DarkLayerId][tR.X][tR.Y] == FogOfWar.TileHidden)
                                            {
                                                if (Layers[(int)fow.DarkLayerId][bL.X][bL.Y] == FogOfWar.TileHidden)
                                                {
                                                    if (Layers[(int)fow.DarkLayerId][bR.X][bR.Y] == FogOfWar.TileHidden)
                                                    {
                                                        continue;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            tile.Tile.SetDestFromPoint(dest);
                            if (Fogofwar == FogOfWar.TypeTint)
                            {
                                tile.Tile.ColorMod = fow.GetTileColorMod(i, j);
                            }
                            tile.Tile.AlphaMod = 255;
                            if (settings.FadeWalls && eset.Misc.FadeWallAlpha < 255 && CheckTileOverlappingHero(i, j, layerdata))
                            {
                                FadeOverlapTile(tile, i, j, layerdata);
                            }
                            renderDevice.Render(tile.Tile);
                        }
                    }
                }
                j = (short)(j + tilesWidth);
                i = (short)(i - tilesWidth);
                if (y % 2 != 0)
                    i++;
                else
                    j++;
            }
        }

        private void RenderIsoBackObjects(List<Renderable> r)
        {
            for (int it = 0; it < r.Count; ++it)
                DrawRenderable(r, it);
        }

        private void RenderIsoFrontObjects(List<Renderable> r)
        {
            var eset = SharedResources.Eset!;
            var settings = SharedResources.Settings!;
            var renderDevice = SharedResources.RenderDevice!;
            var fow = SharedGameResources.Fow!;

            Int2 dest;

            Int2 upperleft = Utils.ScreenToMap(0, 0, Cam.Shake.X, Cam.Shake.Y).ToInt2();
            short maxTilesWidth = (short)((settings.ViewW / eset.Tileset.TileW) + 2 * _tset.MaxSizeX);
            short maxTilesHeight = (short)(((settings.ViewH / eset.Tileset.TileH) + 2 * _tset.MaxSizeY) * 2);

            int rCursor = 0;
            int rEnd = r.Count;

            short j = (short)(upperleft.Y - _tset.MaxSizeY + _tset.MaxSizeX);
            short i = (short)(upperleft.X - _tset.MaxSizeY - _tset.MaxSizeX);

            while (rCursor < rEnd && ((int)r[rCursor].MapPos.X + (int)r[rCursor].MapPos.Y < i + j || (int)r[rCursor].MapPos.X < i))
                ++rCursor;

            if (IndexObjectlayer >= Layers.Count)
                return;

            Queue<int> renderBehindSW = new Queue<int>();
            Queue<int> renderBehindNE = new Queue<int>();
            Queue<int> renderBehindNone = new Queue<int>();

            List<List<ushort>> drawnTiles = new List<List<ushort>>();
            for (int x = 0; x < W; ++x)
            {
                List<ushort> col = new List<ushort>(H);
                for (int y = 0; y < H; ++y)
                    col.Add(0);
                drawnTiles.Add(col);
            }

            for (ushort y = (ushort)maxTilesHeight; y > 0; --y)
            {
                short tilesWidth = 0;

                if (i < -1)
                {
                    j = (short)(j + i + 1);
                    tilesWidth = (short)(tilesWidth - (i + 1));
                    i = -1;
                }
                short d = (short)(j - H);
                if (d >= 0)
                {
                    j = (short)(j - d);
                    tilesWidth = (short)(tilesWidth + d);
                    i = (short)(i + d);
                }

                short jEnd = Math.Max((short)(j + i - W + 1), Math.Max((short)(j - maxTilesWidth), (short)0));

                Int2 p = Utils.MapToScreen(i, j, Cam.Shake.X, Cam.Shake.Y);
                p = CenterTile(p);
                List<List<ushort>> currentLayer = Layers[(int)IndexObjectlayer];
                bool isLastNETile = false;
                while (j > jEnd)
                {
                    --j;
                    ++i;
                    ++tilesWidth;
                    p.X += eset.Tileset.TileW;

                    bool drawTile = true;

                    int rPreCursor = rCursor;
                    while (rPreCursor < rEnd)
                    {
                        int rCursorX = (int)r[rPreCursor].MapPos.X;
                        int rCursorY = (int)r[rPreCursor].MapPos.Y;

                        if ((rCursorX - 1 == i && rCursorY + 1 == j) || (rCursorX + 1 == i && rCursorY - 1 == j))
                        {
                            drawTile = false;
                            break;
                        }
                        else if (rCursorX + 1 > i || rCursorY + 1 > j)
                        {
                            break;
                        }
                        ++rPreCursor;
                    }

                    if (drawTile && drawnTiles[i][j] == 0)
                    {
                        ushort currentTile = currentLayer[i][j];
                        if (currentTile != 0)
                        {
                            TileDef tile = _tset.Tiles[currentTile];
                            if (tile.Tile != null)
                            {
                                dest.X = p.X - tile.Offset.X;
                                dest.Y = p.Y - tile.Offset.Y;
                                tile.Tile.SetDestFromPoint(dest);

                                if (Fogofwar == FogOfWar.TypeOverlay)
                                {
                                    if (!ReferenceEquals(currentLayer, Layers[(int)fow.DarkLayerId]))
                                    {
                                        if (Layers[(int)fow.DarkLayerId][i][j] == FogOfWar.TileHidden)
                                        {
                                            Int2 tL = Utils.ScreenToMap(dest.X, dest.Y, Cam.Shake.X, Cam.Shake.Y).ToInt2();
                                            Int2 tR = Utils.ScreenToMap(dest.X + tile.Tile.GetClip().Width, dest.Y, Cam.Shake.X, Cam.Shake.Y).ToInt2();
                                            Int2 bL = Utils.ScreenToMap(dest.X, dest.Y + tile.Tile.GetClip().Height, Cam.Shake.X, Cam.Shake.Y).ToInt2();
                                            Int2 bR = Utils.ScreenToMap(dest.X + tile.Tile.GetClip().Width, dest.Y + tile.Tile.GetClip().Height, Cam.Shake.X, Cam.Shake.Y).ToInt2();

                                            if (tL.X < 0) tL.X = 0;
                                            if (tL.X >= W) tL.X = W - 1;
                                            if (tL.Y < 0) tL.Y = 0;
                                            if (tL.Y >= H) tL.Y = H - 1;

                                            if (tR.X < 0) tR.X = 0;
                                            if (tR.X >= W) tR.X = W - 1;
                                            if (tR.Y < 0) tR.Y = 0;
                                            if (tR.Y >= H) tR.Y = H - 1;

                                            if (bL.X < 0) bL.X = 0;
                                            if (bL.X >= W) bL.X = W - 1;
                                            if (bL.Y < 0) bL.Y = 0;
                                            if (bL.Y >= H) bL.Y = H - 1;

                                            if (bR.X < 0) bR.X = 0;
                                            if (bR.X >= W) bR.X = W - 1;
                                            if (bR.Y < 0) bR.Y = 0;
                                            if (bR.Y >= H) bR.Y = H - 1;

                                            if (Layers[(int)fow.DarkLayerId][tL.X][tL.Y] == FogOfWar.TileHidden)
                                            {
                                                if (Layers[(int)fow.DarkLayerId][tR.X][tR.Y] == FogOfWar.TileHidden)
                                                {
                                                    if (Layers[(int)fow.DarkLayerId][bL.X][bL.Y] == FogOfWar.TileHidden)
                                                    {
                                                        if (Layers[(int)fow.DarkLayerId][bR.X][bR.Y] == FogOfWar.TileHidden)
                                                        {
                                                            continue;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                if (Fogofwar == FogOfWar.TypeTint)
                                {
                                    tile.Tile.ColorMod = fow.GetTileColorMod(i, j);
                                }
                                tile.Tile.AlphaMod = 255;
                                if (settings.FadeWalls && eset.Misc.FadeWallAlpha < 255 && CheckTileOverlappingHero(i, j, currentLayer))
                                {
                                    FadeOverlapTile(tile, i, j, currentLayer);
                                }
                                renderDevice.Render(tile.Tile);
                                drawnTiles[i][j] = 1;
                            }
                        }
                    }

                    if (rCursor >= rEnd)
                        continue;

                doLastNETile:
                    Rectangle tileSWBounds = default;
                    Rectangle tileSBounds = default;
                    Int2 tileSWCenter = default;
                    Int2 tileSCenter = default;
                    GetTileBounds((short)(i - 2), (short)(j + 2), currentLayer, ref tileSWBounds, ref tileSWCenter);
                    GetTileBounds((short)(i - 1), (short)(j + 2), currentLayer, ref tileSBounds, ref tileSCenter);

                    Rectangle tileNEBounds = default;
                    Rectangle tileEBounds = default;
                    Int2 tileNECenter = default;
                    Int2 tileECenter = default;
                    GetTileBounds(i, j, currentLayer, ref tileNEBounds, ref tileNECenter);
                    GetTileBounds(i, (short)(j + 1), currentLayer, ref tileEBounds, ref tileECenter);

                    bool drawSWTile = false;
                    bool drawNETile = false;

                    while (rCursor < rEnd)
                    {
                        int rCursorX = (int)r[rCursor].MapPos.X;
                        int rCursorY = (int)r[rCursor].MapPos.Y;

                        if (rCursorX + 1 == i && rCursorY - 1 == j)
                        {
                            drawSWTile = true;
                            drawNETile = !isLastNETile;

                            Int2 rCursorLeft = Utils.MapToScreen(r[rCursor].MapPos.X, r[rCursor].MapPos.Y, Cam.Shake.X, Cam.Shake.Y);
                            rCursorLeft.Y -= r[rCursor].Offset.Y;
                            Int2 rCursorRight = rCursorLeft;
                            rCursorLeft.X -= r[rCursor].Offset.X;
                            rCursorRight.X += r[rCursor].Src.Width - r[rCursor].Offset.X;

                            bool isBehindSW = false;
                            bool isBehindNE = false;

                            if (r[rCursor].Type == Renderable.TypeHero)
                            {
                                rCursorLeft.X = _heroBounds.X;
                                rCursorLeft.Y = _heroBounds.Y + _heroBounds.Height;

                                rCursorRight.X = _heroBounds.X + _heroBounds.Width;
                                rCursorRight.Y = _heroBounds.Y + _heroBounds.Height;
                            }

                            if (Utils.IsWithinRect(tileSBounds, rCursorRight) && Utils.IsWithinRect(tileSWBounds, rCursorLeft))
                            {
                                isBehindSW = true;
                            }

                            if (drawNETile && Utils.IsWithinRect(tileEBounds, rCursorLeft) && Utils.IsWithinRect(tileNEBounds, rCursorRight))
                            {
                                isBehindNE = true;
                            }

                            if (isBehindSW)
                                renderBehindSW.Enqueue(rCursor);
                            else if (isBehindNE)
                                renderBehindNE.Enqueue(rCursor);
                            else
                                renderBehindNone.Enqueue(rCursor);

                            ++rCursor;
                        }
                        else
                        {
                            break;
                        }
                    }

                    while (renderBehindSW.Count > 0)
                    {
                        DrawRenderable(r, renderBehindSW.Dequeue());
                    }

                    if (drawSWTile && i - 2 >= 0 && j + 2 < H && drawnTiles[i - 2][j + 2] == 0)
                    {
                        ushort currentTile = currentLayer[i - 2][j + 2];
                        if (currentTile != 0)
                        {
                            TileDef tile = _tset.Tiles[currentTile];
                            if (tile.Tile != null)
                            {
                                dest.X = tileSWCenter.X - tile.Offset.X;
                                dest.Y = tileSWCenter.Y - tile.Offset.Y;
                                tile.Tile.SetDestFromPoint(dest);
                                if (Fogofwar == FogOfWar.TypeTint)
                                {
                                    tile.Tile.ColorMod = fow.GetTileColorMod(i, j);
                                }
                                tile.Tile.AlphaMod = 255;
                                if (settings.FadeWalls && eset.Misc.FadeWallAlpha < 255 && CheckTileOverlappingHero((short)(i - 2), (short)(j + 2), currentLayer))
                                {
                                    FadeOverlapTile(tile, (short)(i - 2), (short)(j + 2), currentLayer);
                                }
                                renderDevice.Render(tile.Tile);
                                drawnTiles[i - 2][j + 2] = 1;
                            }
                        }
                    }

                    while (renderBehindNE.Count > 0)
                    {
                        DrawRenderable(r, renderBehindNE.Dequeue());
                    }

                    if (drawNETile && !drawTile && drawnTiles[i][j] == 0)
                    {
                        ushort currentTile = currentLayer[i][j];
                        if (currentTile != 0)
                        {
                            TileDef tile = _tset.Tiles[currentTile];
                            if (tile.Tile != null)
                            {
                                dest.X = tileNECenter.X - tile.Offset.X;
                                dest.Y = tileNECenter.Y - tile.Offset.Y;
                                tile.Tile.SetDestFromPoint(dest);
                                if (Fogofwar == FogOfWar.TypeTint)
                                {
                                    tile.Tile.ColorMod = fow.GetTileColorMod(i, j);
                                }
                                tile.Tile.AlphaMod = 255;
                                if (settings.FadeWalls && eset.Misc.FadeWallAlpha < 255 && CheckTileOverlappingHero(i, j, currentLayer))
                                {
                                    FadeOverlapTile(tile, i, j, currentLayer);
                                }
                                renderDevice.Render(tile.Tile);
                                drawnTiles[i][j] = 1;
                            }
                        }
                    }

                    while (renderBehindNone.Count > 0)
                    {
                        DrawRenderable(r, renderBehindNone.Dequeue());
                    }

                    if (isLastNETile)
                    {
                        ++j;
                        --i;
                        isLastNETile = false;
                    }
                    else if (i == W - 1 || j == 0)
                    {
                        --j;
                        ++i;
                        isLastNETile = true;
                        goto doLastNETile;
                    }
                }
                j = (short)(j + tilesWidth);
                i = (short)(i - tilesWidth);
                if (y % 2 != 0)
                    i++;
                else
                    j++;

                while (rCursor < rEnd && ((int)r[rCursor].MapPos.X + (int)r[rCursor].MapPos.Y < i + j || (int)r[rCursor].MapPos.X <= i))
                    ++rCursor;
            }
        }

        private void RenderIso(List<Renderable> r, List<Renderable> rDead)
        {
            uint index = 0;

            while (index < IndexObjectlayer)
            {
                RenderIsoLayer(Layers[(int)index], _tset);
                _mapParallax.Render(Cam.Shake, Layernames[(int)index]);
                index++;
            }

            RenderIsoBackObjects(rDead);
            RenderIsoFrontObjects(r);
            _mapParallax.Render(Cam.Shake, Layernames[(int)index]);

            index++;
            while (index < Layers.Count)
            {
                if (Fogofwar == FogOfWar.TypeOverlay)
                {
                    if (Layernames[(int)index] == "fow_dark")
                    {
                        RenderIsoLayer(Layers[(int)index], SharedGameResources.Fow!.TsetDark);
                    }
                    else if (Layernames[(int)index] == "fow_fog")
                    {
                        RenderIsoLayer(Layers[(int)index], SharedGameResources.Fow.TsetFog);
                    }
                    else
                    {
                        RenderIsoLayer(Layers[(int)index], _tset);
                    }
                }
                else if (Layernames[(int)index] != "fow_dark" && Layernames[(int)index] != "fow_fog")
                {
                    RenderIsoLayer(Layers[(int)index], _tset);
                }
                _mapParallax.Render(Cam.Shake, Layernames[(int)index]);
                index++;
            }

            CheckTooltip();

            DrawDevHUD();
            DrawDevCursor();
        }

        private void RenderOrthoLayer(List<List<ushort>> layerdata, TileSet tileSet)
        {
            Int2 dest;

            var eset = SharedResources.Eset!;
            var settings = SharedResources.Settings!;
            var renderDevice = SharedResources.RenderDevice!;
            var fow = SharedGameResources.Fow!;

            Int2 upperleft = Utils.ScreenToMap(0, 0, Cam.Shake.X, Cam.Shake.Y).ToInt2();

            short startj = (short)Math.Max(0, upperleft.Y);
            short starti = (short)Math.Max(0, upperleft.X);
            short maxTilesWidth = (short)Math.Min(W, starti + (settings.ViewW / eset.Tileset.TileW) + 2 * _tset.MaxSizeX);
            short maxTilesHeight = (short)Math.Min(H, startj + (settings.ViewH / eset.Tileset.TileH) + 2 * _tset.MaxSizeY);

            short i;
            short j;

            for (j = startj; j < maxTilesHeight; j++)
            {
                Int2 p = Utils.MapToScreen(starti, j, Cam.Shake.X, Cam.Shake.Y);
                p = CenterTile(p);
                for (i = starti; i < maxTilesWidth; i++)
                {
                    ushort currentTile = layerdata[i][j];
                    if (currentTile != 0)
                    {
                        TileDef tile = tileSet.Tiles[currentTile];
                        if (tile.Tile != null)
                        {
                            dest.X = p.X - tile.Offset.X;
                            dest.Y = p.Y - tile.Offset.Y;

                            bool skipTileRender = false;

                            if (Fogofwar == FogOfWar.TypeOverlay)
                            {
                                if (!ReferenceEquals(layerdata, Layers[(int)fow.DarkLayerId]))
                                {
                                    if (Layers[(int)fow.DarkLayerId][i][j] == FogOfWar.TileHidden)
                                    {
                                        Int2 tL = Utils.ScreenToMap(dest.X, dest.Y, Cam.Shake.X, Cam.Shake.Y).ToInt2();
                                        Int2 tR = Utils.ScreenToMap(dest.X + tile.Tile.GetClip().Width, dest.Y, Cam.Shake.X, Cam.Shake.Y).ToInt2();
                                        Int2 bL = Utils.ScreenToMap(dest.X, dest.Y + tile.Tile.GetClip().Height, Cam.Shake.X, Cam.Shake.Y).ToInt2();
                                        Int2 bR = Utils.ScreenToMap(dest.X + tile.Tile.GetClip().Width, dest.Y + tile.Tile.GetClip().Height, Cam.Shake.X, Cam.Shake.Y).ToInt2();

                                        if (tL.X < 0) tL.X = 0;
                                        if (tL.X >= W) tL.X = W - 1;
                                        if (tL.Y < 0) tL.Y = 0;
                                        if (tL.Y >= H) tL.Y = H - 1;

                                        if (tR.X < 0) tR.X = 0;
                                        if (tR.X >= W) tR.X = W - 1;
                                        if (tR.Y < 0) tR.Y = 0;
                                        if (tR.Y >= H) tR.Y = H - 1;

                                        if (bL.X < 0) bL.X = 0;
                                        if (bL.X >= W) bL.X = W - 1;
                                        if (bL.Y < 0) bL.Y = 0;
                                        if (bL.Y >= H) bL.Y = H - 1;

                                        if (bR.X < 0) bR.X = 0;
                                        if (bR.X >= W) bR.X = W - 1;
                                        if (bR.Y < 0) bR.Y = 0;
                                        if (bR.Y >= H) bR.Y = H - 1;

                                        if (Layers[(int)fow.DarkLayerId][tL.X][tL.Y] == FogOfWar.TileHidden)
                                        {
                                            if (Layers[(int)fow.DarkLayerId][tR.X][tR.Y] == FogOfWar.TileHidden)
                                            {
                                                if (Layers[(int)fow.DarkLayerId][bL.X][bL.Y] == FogOfWar.TileHidden)
                                                {
                                                    if (Layers[(int)fow.DarkLayerId][bR.X][bR.Y] == FogOfWar.TileHidden)
                                                    {
                                                        skipTileRender = true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            tile.Tile.SetDestFromPoint(dest);
                            if (!skipTileRender)
                            {
                                if (Fogofwar == FogOfWar.TypeTint)
                                {
                                    tile.Tile.ColorMod = fow.GetTileColorMod(i, j);
                                }
                                tile.Tile.AlphaMod = 255;
                                if (settings.FadeWalls && eset.Misc.FadeWallAlpha < 255 && CheckTileOverlappingHero(i, j, layerdata))
                                {
                                    FadeOverlapTile(tile, i, j, layerdata);
                                }
                                renderDevice.Render(tile.Tile);
                            }
                        }
                    }
                    p.X += eset.Tileset.TileW;
                }
            }
        }

        private void RenderOrthoBackObjects(List<Renderable> r)
        {
            for (int it = 0; it < r.Count; ++it)
                DrawRenderable(r, it);
        }

        private void RenderOrthoFrontObjects(List<Renderable> r)
        {
            short i;
            short j;
            Int2 dest;
            int rCursor = 0;
            int rEnd = r.Count;

            var eset = SharedResources.Eset!;
            var settings = SharedResources.Settings!;
            var renderDevice = SharedResources.RenderDevice!;
            var fow = SharedGameResources.Fow!;

            Int2 upperleft = Utils.ScreenToMap(0, 0, Cam.Shake.X, Cam.Shake.Y).ToInt2();

            short startj = (short)Math.Max(0, upperleft.Y);
            short starti = (short)Math.Max(0, upperleft.X);
            short maxTilesWidth = (short)Math.Min(W, starti + (settings.ViewW / eset.Tileset.TileW) + 2 * _tset.MaxSizeX);
            short maxTilesHeight = (short)Math.Min(H, startj + (settings.ViewH / eset.Tileset.TileH) + 2 * _tset.MaxSizeY);

            while (rCursor < rEnd && (int)r[rCursor].MapPos.Y < startj)
                ++rCursor;

            if (IndexObjectlayer >= Layers.Count)
                return;

            for (j = startj; j < maxTilesHeight; j++)
            {
                Int2 p = Utils.MapToScreen(starti, j, Cam.Shake.X, Cam.Shake.Y);
                p = CenterTile(p);
                for (i = starti; i < maxTilesWidth; i++)
                {
                    ushort currentTile = Layers[(int)IndexObjectlayer][i][j];
                    if (currentTile != 0)
                    {
                        TileDef tile = _tset.Tiles[currentTile];
                        if (tile.Tile != null)
                        {
                            dest.X = p.X - tile.Offset.X;
                            dest.Y = p.Y - tile.Offset.Y;
                            tile.Tile.SetDestFromPoint(dest);

                            bool skipTileRender = false;

                            if (Fogofwar == FogOfWar.TypeOverlay)
                            {
                                if (!ReferenceEquals(Layers[(int)IndexObjectlayer], Layers[(int)fow.DarkLayerId]))
                                {
                                    if (Layers[(int)fow.DarkLayerId][i][j] == FogOfWar.TileHidden)
                                    {
                                        Int2 tL = Utils.ScreenToMap(dest.X, dest.Y, Cam.Shake.X, Cam.Shake.Y).ToInt2();
                                        Int2 tR = Utils.ScreenToMap(dest.X + tile.Tile.GetClip().Width, dest.Y, Cam.Shake.X, Cam.Shake.Y).ToInt2();
                                        Int2 bL = Utils.ScreenToMap(dest.X, dest.Y + tile.Tile.GetClip().Height, Cam.Shake.X, Cam.Shake.Y).ToInt2();
                                        Int2 bR = Utils.ScreenToMap(dest.X + tile.Tile.GetClip().Width, dest.Y + tile.Tile.GetClip().Height, Cam.Shake.X, Cam.Shake.Y).ToInt2();

                                        if (tL.X < 0) tL.X = 0;
                                        if (tL.X >= W) tL.X = W - 1;
                                        if (tL.Y < 0) tL.Y = 0;
                                        if (tL.Y >= H) tL.Y = H - 1;

                                        if (tR.X < 0) tR.X = 0;
                                        if (tR.X >= W) tR.X = W - 1;
                                        if (tR.Y < 0) tR.Y = 0;
                                        if (tR.Y >= H) tR.Y = H - 1;

                                        if (bL.X < 0) bL.X = 0;
                                        if (bL.X >= W) bL.X = W - 1;
                                        if (bL.Y < 0) bL.Y = 0;
                                        if (bL.Y >= H) bL.Y = H - 1;

                                        if (bR.X < 0) bR.X = 0;
                                        if (bR.X >= W) bR.X = W - 1;
                                        if (bR.Y < 0) bR.Y = 0;
                                        if (bR.Y >= H) bR.Y = H - 1;

                                        if (Layers[(int)fow.DarkLayerId][tL.X][tL.Y] == FogOfWar.TileHidden)
                                        {
                                            if (Layers[(int)fow.DarkLayerId][tR.X][tR.Y] == FogOfWar.TileHidden)
                                            {
                                                if (Layers[(int)fow.DarkLayerId][bL.X][bL.Y] == FogOfWar.TileHidden)
                                                {
                                                    if (Layers[(int)fow.DarkLayerId][bR.X][bR.Y] == FogOfWar.TileHidden)
                                                    {
                                                        skipTileRender = true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (!skipTileRender)
                            {
                                if (Fogofwar == FogOfWar.TypeTint)
                                {
                                    tile.Tile.ColorMod = fow.GetTileColorMod(i, j);
                                }
                                tile.Tile.AlphaMod = 255;
                                if (settings.FadeWalls && eset.Misc.FadeWallAlpha < 255 && CheckTileOverlappingHero(i, j, Layers[(int)IndexObjectlayer]))
                                {
                                    FadeOverlapTile(tile, i, j, Layers[(int)IndexObjectlayer]);
                                }
                                renderDevice.Render(tile.Tile);
                            }
                        }
                    }
                    p.X += eset.Tileset.TileW;

                    while (rCursor < rEnd && (int)r[rCursor].MapPos.Y == j && (int)r[rCursor].MapPos.X < i)
                        ++rCursor;

                    while (rCursor < rEnd && (int)r[rCursor].MapPos.Y == j && (int)r[rCursor].MapPos.X == i)
                    {
                        DrawRenderable(r, rCursor);
                        ++rCursor;
                    }
                }
                while (rCursor < rEnd && (int)r[rCursor].MapPos.Y <= j)
                    ++rCursor;
            }
        }

        private void RenderOrtho(List<Renderable> r, List<Renderable> rDead)
        {
            uint index = 0;
            while (index < IndexObjectlayer)
            {
                RenderOrthoLayer(Layers[(int)index], _tset);
                _mapParallax.Render(Cam.Shake, Layernames[(int)index]);
                index++;
            }

            RenderOrthoBackObjects(rDead);
            RenderOrthoFrontObjects(r);
            _mapParallax.Render(Cam.Shake, Layernames[(int)index]);

            index++;
            while (index < Layers.Count)
            {
                if (Fogofwar == FogOfWar.TypeOverlay)
                {
                    if (Layernames[(int)index] == "fow_dark")
                    {
                        RenderOrthoLayer(Layers[(int)index], SharedGameResources.Fow!.TsetDark);
                    }
                    else if (Layernames[(int)index] == "fow_fog")
                    {
                        RenderOrthoLayer(Layers[(int)index], SharedGameResources.Fow.TsetFog);
                    }
                    else
                    {
                        RenderOrthoLayer(Layers[(int)index], _tset);
                    }
                }
                else if (Layernames[(int)index] != "fow_dark" && Layernames[(int)index] != "fow_fog")
                {
                    RenderOrthoLayer(Layers[(int)index], _tset);
                }
                _mapParallax.Render(Cam.Shake, Layernames[(int)index]);
                index++;
            }

            CheckTooltip();

            DrawDevHUD();
            DrawDevCursor();
        }

        public void ExecuteOnLoadEvents()
        {
            var settings = SharedResources.Settings!;
            var eventm = SharedGameResources.Eventm!;

            if (settings.LoadScript.Length > 0 && Filename != "maps/spawn.txt")
            {
                Event evnt = new Event();
                EventComponent ec = new EventComponent();

                ec.Type = EventComponent.Script;
                ec.S = settings.LoadScript;
                settings.LoadScript = "";

                evnt.Components.Add(ec);
                eventm.ExecuteEvent(evnt);

                return;
            }

            for (int it = Events.Count; it > 0; )
            {
                --it;

                if (!eventm.IsActive(Events[it]))
                    continue;

                if (Events[it].ActivateType == Event.ActivateOnLoad)
                {
                    if (eventm.ExecuteEvent(Events[it]))
                        Events.RemoveAt(it);
                }
            }

            for (int it = Events.Count; it > 0; )
            {
                --it;

                if (!eventm.IsActive(Events[it]))
                    continue;

                if (Events[it].ActivateType == Event.ActivateStatic)
                {
                    if (eventm.ExecuteEvent(Events[it]))
                        Events.RemoveAt(it);
                }
            }
        }

        public void ExecuteOnMapExitEvents()
        {
            var eventm = SharedGameResources.Eventm!;

            for (int it = 0; it < Events.Count; ++it)
            {
                if (!eventm.IsActive(Events[it]))
                    continue;

                if (Events[it].ActivateType == Event.ActivateOnMapexit)
                    eventm.ExecuteEvent(Events[it]);
            }
        }

        public void CheckEvents(Vector2 loc)
        {
            Int2 maploc;
            maploc.X = (int)loc.X;
            maploc.Y = (int)loc.Y;

            var eventm = SharedGameResources.Eventm!;

            for (int it = Events.Count; it > 0; )
            {
                --it;

                if (!eventm.IsActive(Events[it]))
                    continue;

                if (Events[it].ActivateType == Event.ActivateStatic)
                {
                    if (eventm.ExecuteEvent(Events[it]))
                        Events.RemoveAt(it);
                    continue;
                }

                if (Events[it].ActivateType == Event.ActivateOnClear)
                {
                    if (EnemiesCleared && eventm.ExecuteEvent(Events[it]))
                        Events.RemoveAt(it);
                    continue;
                }

                bool inside = maploc.X >= Events[it].Location.X &&
                              maploc.Y >= Events[it].Location.Y &&
                              maploc.X <= Events[it].Location.X + Events[it].Location.Width - 1 &&
                              maploc.Y <= Events[it].Location.Y + Events[it].Location.Height - 1;

                if (Events[it].ActivateType == Event.ActivateOnLeave)
                {
                    if (inside)
                    {
                        if (Events[it].GetComponent(EventComponent.WasInsideEventArea) == null)
                        {
                            Events[it].Components.Add(new EventComponent());
                            Events[it].Components[^1].Type = EventComponent.WasInsideEventArea;
                        }
                    }
                    else
                    {
                        if (Events[it].GetComponent(EventComponent.WasInsideEventArea) != null)
                        {
                            Events[it].DeleteAllComponents(EventComponent.WasInsideEventArea);
                            if (eventm.ExecuteEvent(Events[it]))
                                Events.RemoveAt(it);
                        }
                    }
                }
                else if (Events[it].ActivateType == Event.ActivateOnTrigger)
                {
                    if (inside)
                        if (eventm.ExecuteEvent(Events[it]))
                            Events.RemoveAt(it);
                }
            }
        }

        public void CheckHotspots()
        {
            if (!SharedResources.Inpt!.UsingMouse())
                return;

            var inpt = SharedResources.Inpt!;
            var pc = SharedGameResources.Pc!;
            var settings = SharedResources.Settings!;
            var eventm = SharedGameResources.Eventm!;
            var eset = SharedResources.Eset!;
            var npcs = SharedGameResources.Npcs!;
            var curs = SharedResources.Curs!;
            var mapr = SharedGameResources.Mapr!;

            _showTooltip = false;

            Int2 mousePos = inpt.Mouse;
            bool mouseMoveTarget = pc.MmTargetObject == Avatar.MmTargetEvent && pc.IsNearMMtarget();
            if (mouseMoveTarget && (pc.Stats.CurState == StatBlock.EntityStance || pc.Stats.CurState == StatBlock.EntityMove))
            {
                pc.Stats.CurState = StatBlock.EntityStance;
                mousePos = Utils.MapToScreen(pc.MmTargetObjectPos.X, pc.MmTargetObjectPos.Y, Cam.Shake.X, Cam.Shake.Y);
            }
            else if (pc.MmTargetObject == Avatar.MmTargetEvent && pc.Stats.CurState == StatBlock.EntityStance)
            {
                pc.Stats.CurState = StatBlock.EntityMove;
            }

            int interactKey = (settings.MouseMove && settings.MouseMoveSwap) ? Input.Main2 : Input.Main1;

            for (int it = Events.Count; it > 0; )
            {
                --it;

                if (!eventm.IsActive(Events[it]))
                    continue;

                if (Events[it].Hotspot.Height == 0)
                    continue;

                if (!Events[it].Cooldown.IsEnd() || !Events[it].Delay.IsEnd())
                    continue;

                EventComponent? npc = Events[it].GetComponent(EventComponent.NpcHotspot);

                for (int x = Events[it].Hotspot.X; x < Events[it].Hotspot.X + Events[it].Hotspot.Width; ++x)
                {
                    for (int y = Events[it].Hotspot.Y; y < Events[it].Hotspot.Y + Events[it].Hotspot.Height; ++y)
                    {
                        bool matched = false;
                        bool isNpc = false;

                        if (npc != null)
                        {
                            isNpc = true;

                            Int2 p = Utils.MapToScreen(npc.Data[0].Int, npc.Data[1].Int, Cam.Shake.X, Cam.Shake.Y);
                            p = CenterTile(p);

                            Rectangle dest = default;
                            if (npc.Id < npcs.Npcs.Count)
                            {
                                dest = npcs.Npcs[npc.Id].GetRenderBounds(Cam.Pos);
                            }

                            if (Utils.IsWithinRect(dest, mousePos))
                            {
                                matched = true;
                                _tipPos.X = dest.X + dest.Width / 2;
                                _tipPos.Y = p.Y - eset.Tooltips.MarginNpc;
                            }
                        }
                        else
                        {
                            for (uint index = 0; index <= IndexObjectlayer; ++index)
                            {
                                Int2 p = Utils.MapToScreen(x, y, Cam.Shake.X, Cam.Shake.Y);
                                p = CenterTile(p);

                                ushort currentTile = Layers[(int)index][x][y];
                                if (currentTile != 0)
                                {
                                    TileDef tile = _tset.Tiles[currentTile];
                                    if (tile.Tile != null)
                                    {
                                        Rectangle dest = default;
                                        dest.X = p.X - tile.Offset.X;
                                        dest.Y = p.Y - tile.Offset.Y;
                                        dest.Width = tile.Tile.GetClip().Width;
                                        dest.Height = tile.Tile.GetClip().Height;

                                        if (Utils.IsWithinRect(dest, mousePos))
                                        {
                                            matched = true;
                                            _tipPos = Utils.MapToScreen(Events[it].Center.X, Events[it].Center.Y, Cam.Shake.X, Cam.Shake.Y);
                                            _tipPos.Y -= eset.Tileset.TileH;
                                        }
                                    }
                                }
                            }
                        }

                        if (matched)
                        {
                            CreateTooltip(Events[it].GetComponent(EventComponent.Tooltip));

                            if (((Events[it].ReachableFrom.Width == 0 && Events[it].ReachableFrom.Height == 0) || Utils.IsWithinRect(Events[it].ReachableFrom, new Int2((int)Cam.Pos.X, (int)Cam.Pos.Y)))
                                    && Utils.CalcDist(pc.Stats.Pos, Events[it].Center) < eset.Misc.InteractRange)
                            {
                                if (!mouseMoveTarget)
                                {
                                    if (isNpc)
                                    {
                                        curs.SetCursor(CursorManager.CursorTalk);
                                    }
                                    else
                                    {
                                        curs.SetCursor(CursorManager.CursorInteract);
                                    }
                                    if (!inpt.Pressing[interactKey]) return;
                                    else if (inpt.Lock[interactKey]) return;
                                    else if (interactKey == Input.Main1 && pc.UsingMain1) return;
                                    else if (interactKey == Input.Main2 && pc.UsingMain2) return;

                                    inpt.Lock[interactKey] = true;
                                }
                                else
                                {
                                    pc.MmTargetObject = Avatar.MmTargetNone;
                                }

                                if (eventm.ExecuteEvent(Events[it]))
                                    Events.RemoveAt(it);
                            }
                            else if (settings.MouseMove)
                            {
                                if (isNpc)
                                {
                                    curs.SetCursor(CursorManager.CursorTalk);
                                }
                                else
                                {
                                    curs.SetCursor(CursorManager.CursorInteract);
                                }

                                if (inpt.Pressing[interactKey] && !inpt.Lock[interactKey])
                                {
                                    inpt.Lock[interactKey] = true;

                                    if (!mapr.Collider.IsValidPosition(Events[it].Center.X, Events[it].Center.Y, pc.Stats.MovementType, MapCollision.CollideTypeHero))
                                    {
                                        Vector2 nearbyTarget = mapr.Collider.GetRandomNeighbor(new Int2((int)Events[it].Center.X, (int)Events[it].Center.Y), 1, pc.Stats.MovementType, MapCollision.CollideTypeHero);
                                        pc.SetDesiredMMTarget(ref nearbyTarget);
                                    }
                                    else
                                    {
                                        Vector2 center = Events[it].Center;
                                        pc.SetDesiredMMTarget(ref center);
                                    }

                                    pc.MmTargetObject = Avatar.MmTargetEvent;
                                    pc.MmTargetObjectPos = Events[it].Center;
                                }
                            }
                            return;
                        }
                        else
                            _showTooltip = false;
                    }
                }
            }
        }

        public void CheckNearestEvent()
        {
            var inpt = SharedResources.Inpt!;
            var eventm = SharedGameResources.Eventm!;
            var eset = SharedResources.Eset!;

            if (!inpt.UsingMouse())
                _showTooltip = false;

            int nearest = -1;
            float bestDistance = float.MaxValue;

            for (int it = Events.Count; it > 0; )
            {
                --it;

                if (!eventm.IsActive(Events[it]))
                    continue;

                if (Events[it].Hotspot.Height == 0)
                    continue;

                if (!Events[it].Cooldown.IsEnd() || !Events[it].Delay.IsEnd())
                    continue;

                float distance = Utils.CalcDist(SharedGameResources.Pc!.Stats.Pos, Events[it].Center);
                if (((Events[it].ReachableFrom.Width == 0 && Events[it].ReachableFrom.Height == 0) || Utils.IsWithinRect(Events[it].ReachableFrom, new Int2((int)Cam.Pos.X, (int)Cam.Pos.Y)))
                        && distance < eset.Misc.InteractRange && distance < bestDistance)
                {
                    bestDistance = distance;
                    nearest = it;
                }
            }

            if (nearest != -1)
            {
                if (!inpt.UsingMouse() || inpt.UsingTouchscreen())
                {
                    CreateTooltip(Events[nearest].GetComponent(EventComponent.Tooltip));
                    _tipPos = Utils.MapToScreen(Events[nearest].Center.X, Events[nearest].Center.Y, Cam.Shake.X, Cam.Shake.Y);
                    if (Events[nearest].GetComponent(EventComponent.NpcHotspot) != null)
                    {
                        _tipPos.Y -= eset.Tooltips.MarginNpc;
                    }
                    else
                    {
                        _tipPos.Y -= eset.Tileset.TileH;
                    }
                }

                if (inpt.Pressing[Input.Accept] && !inpt.Lock[Input.Accept])
                {
                    inpt.Lock[Input.Accept] = true;

                    if (eventm.ExecuteEvent(Events[nearest]))
                        Events.RemoveAt(nearest);
                }
            }
        }

        public void CheckTooltip()
        {
            if (_showTooltip && SharedResources.Settings!.ShowHud && !(SharedResources.Settings.DevMode && SharedGameResources.Menu!.Devconsole!.Visible))
                _tip.Render(_tipBuf, _tipPos, TooltipData.StyleTopLabel);
        }

        private void CreateTooltip(EventComponent? ec)
        {
            if (ec != null && ec.S.Length > 0 && SharedResources.Tooltipm!.Context != TooltipManager.ContextMenu)
            {
                _showTooltip = true;
                if (!_tipBuf.CompareFirstLine(ec.S))
                {
                    _tipBuf.Clear();
                    _tipBuf.AddText(ec.S);
                }
                SharedResources.Tooltipm.Context = TooltipManager.ContextMap;
            }
            else if (SharedResources.Tooltipm!.Context != TooltipManager.ContextMenu)
            {
                SharedResources.Tooltipm.Context = TooltipManager.ContextNone;
            }
        }

        public void ActivatePower(PowerID powerIndex, uint statblockIndex, Vector2 target)
        {
            if (!SharedGameResources.Powers!.IsValid(powerIndex))
            {
                Utils.LogError("MapRenderer: Power index %d is not valid.", powerIndex);
                return;
            }

            if (statblockIndex < Statblocks.Count)
            {
                if (Statblocks[(int)statblockIndex].PowersAi[0].Cooldown.IsEnd())
                {
                    Statblocks[(int)statblockIndex].PowersAi[0].Cooldown.Duration = (uint)SharedGameResources.Powers.Powers[powerIndex]!.Cooldown;
                    SharedGameResources.Powers.Activate(powerIndex, Statblocks[(int)statblockIndex], Statblocks[(int)statblockIndex].Pos, target);
                }
            }
            else
            {
                Utils.LogError("MapRenderer: StatBlock index is out of bounds.");
            }
        }

        public bool IsValidTile(uint tile)
        {
            if (tile == 0)
                return true;

            if (tile >= _tset.Tiles.Count)
                return false;

            return _tset.Tiles[(int)tile].Tile != null;
        }

        public Int2 CenterTile(Int2 p)
        {
            Int2 r = p;

            var eset = SharedResources.Eset!;

            if (eset.Tileset.Orientation == EngineSettings.TilesetSettings.TilesetOrthogonal)
            {
                r.X += eset.Tileset.TileWHalf;
                r.Y += eset.Tileset.TileHHalf;
            }
            else
                r.Y += eset.Tileset.TileHHalf;
            return r;
        }

        private void GetTileBounds(short x, short y, List<List<ushort>> layerdata, ref Rectangle bounds, ref Int2 center)
        {
            if (x >= 0 && x < W && y >= 0 && y < H)
            {
                ushort tileIndex = layerdata[x][y];
                if (tileIndex != 0)
                {
                    TileDef tile = _tset.Tiles[tileIndex];
                    if (tile.Tile == null)
                        return;
                    center = CenterTile(Utils.MapToScreen(x, y, Cam.Shake.X, Cam.Shake.Y));
                    bounds.X = center.X - tile.Offset.X;
                    bounds.Y = center.Y - tile.Offset.Y;
                    bounds.Width = tile.Tile.GetClip().Width;
                    bounds.Height = tile.Tile.GetClip().Height;
                }
            }
        }

        private void DrawDevCursor()
        {
            if (!(SharedResources.Settings!.DevMode && SharedGameResources.Menu!.Devconsole!.Visible))
                return;

            var eset = SharedResources.Eset!;
            var renderDevice = SharedResources.RenderDevice!;
            var pc = SharedGameResources.Pc!;
            var menu = SharedGameResources.Menu!;
            var inpt = SharedResources.Inpt!;

            Color devCursorColor = new Color(255, 255, 0, 255);
            Vector2 target = Utils.ScreenToMap(inpt.Mouse.X, inpt.Mouse.Y, Cam.Shake.X, Cam.Shake.Y);

            if (!Collider.IsOutsideMap(MathF.Floor(target.X), MathF.Floor(target.Y)))
            {
                if (eset.Tileset.Orientation == EngineSettings.TilesetSettings.TilesetOrthogonal)
                {
                    Int2 pTopleft = Utils.MapToScreen(MathF.Floor(target.X), MathF.Floor(target.Y), Cam.Shake.X, Cam.Shake.Y);
                    Int2 pBottomright = new Int2(pTopleft.X + eset.Tileset.TileW, pTopleft.Y + eset.Tileset.TileH);

                    renderDevice.DrawRectangle(pTopleft, pBottomright, devCursorColor);
                }
                else
                {
                    Int2 pLeft = Utils.MapToScreen(MathF.Floor(target.X), MathF.Floor(target.Y + 1), Cam.Shake.X, Cam.Shake.Y);
                    Int2 pTop = new Int2(pLeft.X + eset.Tileset.TileWHalf, pLeft.Y - eset.Tileset.TileHHalf);
                    Int2 pRight = new Int2(pLeft.X + eset.Tileset.TileW, pLeft.Y);
                    Int2 pBottom = new Int2(pLeft.X + eset.Tileset.TileWHalf, pLeft.Y + eset.Tileset.TileHHalf);

                    renderDevice.DrawLine(pLeft.X, pLeft.Y, pTop.X, pTop.Y, devCursorColor);
                    renderDevice.DrawLine(pTop.X, pTop.Y, pRight.X, pRight.Y, devCursorColor);
                    renderDevice.DrawLine(pRight.X, pRight.Y, pBottom.X, pBottom.Y, devCursorColor);
                    renderDevice.DrawLine(pBottom.X, pBottom.Y, pLeft.X, pLeft.Y, devCursorColor);
                }

                if (menu.Devconsole!.DistanceTimer.IsEnd())
                {
                    Int2 p0 = Utils.MapToScreen(menu.Devconsole.Target.X, menu.Devconsole.Target.Y, Cam.Shake.X, Cam.Shake.Y);
                    Int2 p1 = Utils.MapToScreen(pc.Stats.Pos.X, pc.Stats.Pos.Y, Cam.Shake.X, Cam.Shake.Y);
                    renderDevice.DrawLine(p0.X, p0.Y, p1.X, p1.Y, devCursorColor);
                }
            }
        }

        private void DrawDevHUD()
        {
            if (!(SharedResources.Settings!.DevMode && SharedResources.Settings.DevHud))
                return;

            var eset = SharedResources.Eset!;
            var settings = SharedResources.Settings!;
            var renderDevice = SharedResources.RenderDevice!;
            var pc = SharedGameResources.Pc!;
            var entitym = SharedGameResources.Entitym!;
            var hazards = SharedGameResources.Hazards!;

            Color colorHazard = new Color(255, 0, 0, 255);
            Color colorEntity = new Color(0, 255, 0, 255);
            Color colorCam = new Color(255, 255, 0, 255);
            Color colorPath = new Color(0, 255, 255, 255);
            Color colorPathPursue = new Color(0, 127, 127, 255);
            int crossSize = eset.Tileset.TileHHalf / 4;

            int distort = eset.Tileset.Orientation == EngineSettings.TilesetSettings.TilesetOrthogonal ? 1 : 2;

            // 绘制碰撞层（仅遍历当前可视区域，碰撞瓦片按类型着色）
            {
                Int2 upperleft = Utils.ScreenToMap(0, 0, Cam.Shake.X, Cam.Shake.Y).ToInt2();
                Int2 lowerright = Utils.ScreenToMap(settings.ViewW, settings.ViewH, Cam.Shake.X, Cam.Shake.Y).ToInt2();

                int margin = _tset.MaxSizeX + _tset.MaxSizeY + 2;
                int startX = Math.Max(0, Math.Min(upperleft.X, lowerright.X) - margin);
                int startY = Math.Max(0, Math.Min(upperleft.Y, lowerright.Y) - margin);
                int endX = Math.Min(W - 1, Math.Max(upperleft.X, lowerright.X) + margin);
                int endY = Math.Min(H - 1, Math.Max(upperleft.Y, lowerright.Y) + margin);

                // 绘制枚举文本前重置为默认字体，避免上一帧其它界面（菜单/提示等）切换字体后
                // 用错误的字体渲染数字（与本工程其它文本绘制处的 font_regular 重置习惯一致）。
                SharedResources.Font!.SetFont("font_regular");
                for (int j = startY; j <= endY; ++j)
                {
                    for (int i = startX; i <= endX; ++i)
                    {
                        ushort tile = Collider.Colmap[i][j];
                        if (tile == MapCollision.BlocksNone)
                            continue;
                        DrawDevCollisionTile(i, j, GetCollisionColor(tile));
                        DrawDevCollisionValue(i, j, tile);
                    }
                }
            }

            {
                Int2 p0 = Utils.MapToScreen(Cam.Pos.X, Cam.Pos.Y, Cam.Shake.X, Cam.Shake.Y);
                renderDevice.DrawLine(p0.X - crossSize, p0.Y, p0.X + crossSize, p0.Y, colorCam);
                renderDevice.DrawLine(p0.X, p0.Y - crossSize, p0.X, p0.Y + crossSize, colorCam);
            }

            {
                Int2 p0 = Utils.MapToScreen(pc.Stats.Pos.X, pc.Stats.Pos.Y, Cam.Shake.X, Cam.Shake.Y);
                renderDevice.DrawLine(p0.X - crossSize, p0.Y, p0.X + crossSize, p0.Y, colorEntity);
                renderDevice.DrawLine(p0.X, p0.Y - crossSize, p0.X, p0.Y + crossSize, colorEntity);

                List<Vector2> path = pc.Path;

                if (path.Count == 0)
                {
                    ref Vector2 mmTarget = ref pc.MMTarget;
                    if (!(mmTarget.X == -1 && mmTarget.Y == -1))
                    {
                        Int2 p1 = Utils.MapToScreen(mmTarget.X, mmTarget.Y, Cam.Shake.X, Cam.Shake.Y);
                        renderDevice.DrawLine(p0.X, p0.Y, p1.X, p1.Y, colorPathPursue);
                    }
                }
                else
                {
                    Int2 p1;
                    Int2 p2;
                    for (int j = 0; j < path.Count - 1; ++j)
                    {
                        p1 = Utils.MapToScreen(path[j].X, path[j].Y, Cam.Shake.X, Cam.Shake.Y);
                        p2 = Utils.MapToScreen(path[j + 1].X, path[j + 1].Y, Cam.Shake.X, Cam.Shake.Y);
                        renderDevice.DrawLine(p1.X, p1.Y, p2.X, p2.Y, colorPath);
                    }
                    p1 = Utils.MapToScreen(path[^1].X, path[^1].Y, Cam.Shake.X, Cam.Shake.Y);
                    renderDevice.DrawLine(p0.X, p0.Y, p1.X, p1.Y, colorPath);
                }
            }

            for (int i = 0; i < entitym.Entities.Count; ++i)
            {
                Int2 p0 = Utils.MapToScreen(entitym.Entities[i].Stats.Pos.X, entitym.Entities[i].Stats.Pos.Y, Cam.Shake.X, Cam.Shake.Y);
                renderDevice.DrawLine(p0.X - crossSize, p0.Y, p0.X + crossSize, p0.Y, colorEntity);
                renderDevice.DrawLine(p0.X, p0.Y - crossSize, p0.X, p0.Y + crossSize, colorEntity);

                if (entitym.Entities[i].Stats.Corpse)
                    continue;

                List<Vector2> path = entitym.Entities[i].Behavior!.Path;

                if (path.Count == 0)
                {
                    Vector2 pursuePos = entitym.Entities[i].Behavior.PursuePos;
                    if (!(pursuePos.X == -1 && pursuePos.Y == -1))
                    {
                        Int2 p1 = Utils.MapToScreen(pursuePos.X, pursuePos.Y, Cam.Shake.X, Cam.Shake.Y);
                        renderDevice.DrawLine(p0.X, p0.Y, p1.X, p1.Y, colorPathPursue);
                    }
                }
                else
                {
                    Int2 p1;
                    Int2 p2;
                    for (int j = 0; j < path.Count - 1; ++j)
                    {
                        p1 = Utils.MapToScreen(path[j].X, path[j].Y, Cam.Shake.X, Cam.Shake.Y);
                        p2 = Utils.MapToScreen(path[j + 1].X, path[j + 1].Y, Cam.Shake.X, Cam.Shake.Y);
                        renderDevice.DrawLine(p1.X, p1.Y, p2.X, p2.Y, colorPath);
                    }
                    p1 = Utils.MapToScreen(path[^1].X, path[^1].Y, Cam.Shake.X, Cam.Shake.Y);
                    renderDevice.DrawLine(p0.X, p0.Y, p1.X, p1.Y, colorPath);
                }
            }

            for (int i = 0; i < hazards.H.Count; ++i)
            {
                if (hazards.H[i].DelayFrames != 0)
                    continue;

                Int2 p0 = Utils.MapToScreen(hazards.H[i].Pos.X, hazards.H[i].Pos.Y, Cam.Shake.X, Cam.Shake.Y);
                Int2 p1 = Utils.MapToScreen(hazards.H[i].Pos.X + hazards.H[i].HazardPower!.Radius, hazards.H[i].Pos.Y, Cam.Shake.X, Cam.Shake.Y);
                int radius = p1.X - p0.X;
                renderDevice.DrawLine(p0.X - crossSize, p0.Y, p0.X + crossSize, p0.Y, colorHazard);
                renderDevice.DrawLine(p0.X, p0.Y - crossSize, p0.X, p0.Y + crossSize, colorHazard);

                renderDevice.DrawEllipse(p0.X - radius, p0.Y - radius / distort, p0.X + radius, p0.Y + radius / distort, colorHazard, 15);
            }
        }

        /// <summary>根据碰撞瓦片类型返回 dev HUD 使用的颜色。</summary>
        private static Color GetCollisionColor(ushort tile)
        {
            switch (tile)
            {
                case MapCollision.BlocksAll:
                case MapCollision.BlocksAllHidden:
                    return new Color(255, 0, 0, 255);   // 完全阻挡（墙）
                case MapCollision.BlocksMovement:
                case MapCollision.BlocksMovementHidden:
                    return new Color(255, 128, 0, 255); // 阻挡移动
                case MapCollision.MapOnly:
                case MapCollision.MapOnlyAlt:
                    return new Color(0, 200, 0, 255);   // 仅地图（可穿越）
                case MapCollision.BlocksEntities:
                    return new Color(0, 128, 255, 255); // 阻挡实体
                case MapCollision.BlocksEnemies:
                    return new Color(255, 0, 255, 255); // 阻挡敌人
                default:
                    return new Color(255, 255, 255, 255);
            }
        }

        /// <summary>绘制单个碰撞瓦片（正交地图为矩形，等距地图为菱形）。</summary>
        private void DrawDevCollisionTile(int x, int y, Color color)
        {
            if (SharedResources.Eset!.Tileset.Orientation == EngineSettings.TilesetSettings.TilesetOrthogonal)
            {
                Int2 p0 = Utils.MapToScreen(x, y, Cam.Shake.X, Cam.Shake.Y);
                Int2 p1 = new Int2(p0.X + SharedResources.Eset.Tileset.TileW, p0.Y + SharedResources.Eset.Tileset.TileH);
                SharedResources.RenderDevice!.DrawRectangle(p0, p1, color);
            }
            else
            {
                Int2 pLeft = Utils.MapToScreen(x, y + 1, Cam.Shake.X, Cam.Shake.Y);
                Int2 pTop = new Int2(pLeft.X + SharedResources.Eset.Tileset.TileWHalf, pLeft.Y - SharedResources.Eset.Tileset.TileHHalf);
                Int2 pRight = new Int2(pLeft.X + SharedResources.Eset.Tileset.TileW, pLeft.Y);
                Int2 pBottom = new Int2(pLeft.X + SharedResources.Eset.Tileset.TileWHalf, pLeft.Y + SharedResources.Eset.Tileset.TileHHalf);

                SharedResources.RenderDevice!.DrawLine(pLeft.X, pLeft.Y, pTop.X, pTop.Y, color);
                SharedResources.RenderDevice!.DrawLine(pTop.X, pTop.Y, pRight.X, pRight.Y, color);
                SharedResources.RenderDevice!.DrawLine(pRight.X, pRight.Y, pBottom.X, pBottom.Y, color);
                SharedResources.RenderDevice!.DrawLine(pBottom.X, pBottom.Y, pLeft.X, pLeft.Y, color);
            }
        }

        /// <summary>在碰撞瓦片中心绘制其实际碰撞枚举值编号（便于核对碰撞数据）。</summary>
        private void DrawDevCollisionValue(int x, int y, ushort tile)
        {
            Int2 center;
            if (SharedResources.Eset!.Tileset.Orientation == EngineSettings.TilesetSettings.TilesetOrthogonal)
            {
                Int2 p0 = Utils.MapToScreen(x, y, Cam.Shake.X, Cam.Shake.Y);
                center.X = p0.X + SharedResources.Eset.Tileset.TileWHalf;
                center.Y = p0.Y + SharedResources.Eset.Tileset.TileHHalf;
            }
            else
            {
                Int2 pLeft = Utils.MapToScreen(x, y + 1, Cam.Shake.X, Cam.Shake.Y);
                center.X = pLeft.X + SharedResources.Eset.Tileset.TileWHalf;
                center.Y = pLeft.Y;
            }

            string text = tile.ToString();
            int fontHeight = SharedResources.Font!.GetLineHeight();
            SharedResources.Font.RenderShadowed(text, center.X, center.Y - fontHeight / 2, FontEngine.JustifyCenter, null, 0, new Color(255, 255, 255, 255));
        }

        public void SetMapParallax(string mpFilename)
        {
            _mapParallax.Load(mpFilename);
            _mapParallax.SetMapCenter(W / 2, H / 2);
        }

        private bool CheckTileOverlappingHero(short x, short y, List<List<ushort>> layerdata)
        {
            if (!_drawnHero)
                return false;

            Rectangle tileBounds = default;
            Int2 tileCenter = default;
            GetTileBounds(x, y, layerdata, ref tileBounds, ref tileCenter);

            if (Utils.IsWithinRect(tileBounds, new Int2(_heroBounds.X, _heroBounds.Y)))
                return true;

            if (Utils.IsWithinRect(tileBounds, new Int2(_heroBounds.X + _heroBounds.Width, _heroBounds.Y)))
                return true;

            if (Utils.IsWithinRect(tileBounds, new Int2(_heroBounds.X, _heroBounds.Y + _heroBounds.Height)))
                return true;

            if (Utils.IsWithinRect(tileBounds, new Int2(_heroBounds.X + _heroBounds.Width, _heroBounds.Y + _heroBounds.Height)))
                return true;

            if (Utils.IsWithinRect(_heroBounds, new Int2(tileBounds.X, tileBounds.Y)))
                return true;

            if (Utils.IsWithinRect(_heroBounds, new Int2(tileBounds.X + tileBounds.Width, tileBounds.Y)))
                return true;

            if (Utils.IsWithinRect(_heroBounds, new Int2(tileBounds.X, tileBounds.Y + tileBounds.Height)))
                return true;

            if (Utils.IsWithinRect(_heroBounds, new Int2(tileBounds.X + tileBounds.Width, tileBounds.Y + tileBounds.Height)))
                return true;

            if (Utils.IsWithinRect(tileBounds, new Int2(_heroBounds.X + _heroBounds.Width / 2, _heroBounds.Y + _heroBounds.Height / 2)))
                return true;

            if (Utils.IsWithinRect(_heroBounds, new Int2(tileBounds.X + tileBounds.Width / 2, tileBounds.Y + tileBounds.Height / 2)))
                return true;

            return false;
        }

        private void FadeOverlapTile(TileDef tile, short x, short y, List<List<ushort>> layerdata)
        {
            Rectangle tileBounds = default;
            Int2 tileCenter = default;
            GetTileBounds(x, y, layerdata, ref tileBounds, ref tileCenter);
            float tileDist = Utils.CalcDist(new Vector2(_heroBounds.X + _heroBounds.Width / 2, _heroBounds.Y + _heroBounds.Height / 2), new Vector2(tileBounds.X + tileBounds.Width / 2, tileBounds.Y + tileBounds.Height / 2)) / (float)tile.Tile!.GetClip().Height;
            tile.Tile.AlphaMod = (byte)Math.Min(255, Math.Max(SharedResources.Eset!.Misc.FadeWallAlpha, (int)(255f * tileDist)));
        }

        public void DrawProcgenChunkMap(Image? canvas)
        {
            if (canvas == null || ProcgenChunks.Count == 0)
                return;

            int q = (int)(ProcgenChunkSize / 4);

            Color colorGrid = SharedResources.Font!.GetColor(FontEngine.ColorWidgetDisabled);
            Color colorNormal = SharedResources.Font.GetColor(FontEngine.ColorWidgetNormal);
            Color colorStart = new Color(88, 153, 31);
            Color colorEnd = new Color(204, 61, 61);
            Color colorDoor = new Color(230, 115, 23);
            Color colorKey = new Color(46, 118, 153);

            canvas.DrawLine(0, 0, (int)ProcgenChunks[0].Count * (int)ProcgenChunkSize, 0, colorGrid);
            canvas.DrawLine(0, 0, 0, (int)ProcgenChunks.Count * (int)ProcgenChunkSize, colorGrid);
            for (int i = 0; i < ProcgenChunks.Count; ++i)
            {
                int y = (int)(i * ProcgenChunkSize);
                canvas.DrawLine(0, y + (int)ProcgenChunkSize, (int)ProcgenChunks[i].Count * (int)ProcgenChunkSize, y + (int)ProcgenChunkSize, colorGrid);

                for (int j = 0; j < ProcgenChunks[i].Count; ++j)
                {
                    int x = (int)(j * ProcgenChunkSize);
                    canvas.DrawLine(x + (int)ProcgenChunkSize, 0, x + (int)ProcgenChunkSize, (int)ProcgenChunks.Count * (int)ProcgenChunkSize, colorGrid);
                }
            }

            for (int i = 0; i < ProcgenChunks.Count; ++i)
            {
                int y = (int)(i * ProcgenChunkSize);

                for (int j = 0; j < ProcgenChunks[i].Count; ++j)
                {
                    Chunk chunk = ProcgenChunks[i][j];
                    if (chunk.Type == Chunk.TypeEmpty)
                        continue;

                    int x = (int)(j * ProcgenChunkSize);

                    Color color = colorNormal;
                    if (chunk.Type == Chunk.TypeStart)
                        color = colorStart;
                    else if (chunk.Type == Chunk.TypeEnd)
                        color = colorEnd;
                    else if (chunk.Type == Chunk.TypeDoorWestEast || chunk.Type == Chunk.TypeDoorNorthSouth)
                        color = colorDoor;
                    else if (chunk.Type == Chunk.TypeKey)
                        color = colorKey;

                    canvas.DrawFilledRect(x + q, y + q, q * 2, q * 2, color);
                    if (chunk.Links[Chunk.LinkNorth] != null)
                    {
                        canvas.DrawFilledRect(x + q, y, q * 2, q, color);
                    }
                    if (chunk.Links[Chunk.LinkSouth] != null)
                    {
                        canvas.DrawFilledRect(x + q, y + (q * 3), q * 2, q, color);
                    }
                    if (chunk.Links[Chunk.LinkWest] != null)
                    {
                        canvas.DrawFilledRect(x, y + q, q, q * 2, color);
                    }
                    if (chunk.Links[Chunk.LinkEast] != null)
                    {
                        canvas.DrawFilledRect(x + (q * 3), y + q, q, q * 2, color);
                    }
                }
            }

            int legendX = ((int)ProcgenChunks[0].Count * (int)ProcgenChunkSize) + (int)ProcgenChunkSize;
            int lineH = SharedResources.Font.GetLineHeight();

            SharedResources.Font.RenderShadowed(SharedResources.Msg!.Get("Map Legend"), legendX, 0, FontEngine.JustifyLeft, canvas, 0, colorNormal);
            SharedResources.Font.RenderShadowed(SharedResources.Msg.Get("Start"), legendX, lineH, FontEngine.JustifyLeft, canvas, 0, colorStart);
            SharedResources.Font.RenderShadowed(SharedResources.Msg.Get("End"), legendX, lineH * 2, FontEngine.JustifyLeft, canvas, 0, colorEnd);
            SharedResources.Font.RenderShadowed(SharedResources.Msg.Get("Door"), legendX, lineH * 3, FontEngine.JustifyLeft, canvas, 0, colorDoor);
            SharedResources.Font.RenderShadowed(SharedResources.Msg.Get("Key"), legendX, lineH * 4, FontEngine.JustifyLeft, canvas, 0, colorKey);
        }
    }
}
