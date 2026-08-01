// <自动生成> 对应 C++ 源文件：Map.h + Map.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// 生成等级配置，对应 C++ <c>class SpawnLevel</c>。
    /// 控制 NPC/敌人/玩家召唤物的等级缩放模式。
    /// </summary>
    public class SpawnLevel
    {
        public const byte ModeDefault = 0;
        public const byte ModeFixed = 1;
        public const byte ModeStat = 2;
        public const byte ModeLevel = 3;

        public const byte RatioSourceDefault = 0;
        public const byte RatioSourceHero = 1;

        public byte Mode;
        public byte RatioSource;
        public float Ratio;
        public int Stat;
        public bool IsLegacy;

        public SpawnLevel()
        {
            Mode = ModeDefault;
            RatioSource = 0;
            Ratio = 0;
            Stat = 0;
            IsLegacy = false;
        }

        public void Parse(FileParser infile)
        {
            var eset = SharedResources.Eset!;

            string next = global::FlareEngine.Parse.PopFirstString(ref infile.Val);

            if (next == "default") Mode = ModeDefault;
            else if (next == "fixed") Mode = ModeFixed;
            else if (next == "source_stat") Mode = ModeStat;
            else if (next == "source_level") Mode = ModeLevel;
            else if (next == "hero_stat")
            {
                Mode = ModeStat;
                RatioSource = RatioSourceHero;
            }
            else if (next == "hero_level")
            {
                Mode = ModeLevel;
                RatioSource = RatioSourceHero;
            }
            else if (next == "stat")
            {
                Mode = ModeStat;
                IsLegacy = true;
                infile.Error("SpawnLevel: 'stat' mode is deprecated. Use 'source_stat' instead.");
            }
            else if (next == "level")
            {
                Mode = ModeLevel;
                IsLegacy = true;
                infile.Error("SpawnLevel: 'level' mode is deprecated. Use 'source_level' instead.");
            }
            else infile.Error("SpawnLevel: Unknown spawn level mode '%s'", next);

            if (Mode != ModeDefault)
            {
                Ratio = global::FlareEngine.Parse.PopFirstFloat(ref infile.Val);

                if (Mode != ModeFixed)
                {
                    next = global::FlareEngine.Parse.PopFirstString(ref infile.Val);
                    float nextf = global::FlareEngine.Parse.ToFloat(next);

                    if (IsLegacy)
                    {
                        // legacy format detected!
                        if (nextf != 0)
                        {
                            Ratio = Ratio / nextf;
                        }

                        if (Mode == ModeStat)
                        {
                            next = global::FlareEngine.Parse.PopFirstString(ref infile.Val);
                        }

                    }

                    if (Mode == ModeStat)
                    {
                        int primStatIndex = eset.PrimaryStats.GetIndexByID(next);
    
                        if (primStatIndex != eset.PrimaryStats.Stats.Count)
                        {
                            Stat = primStatIndex;
                        }
                        else
                        {
                            infile.Error("SpawnLevel: '%s' is not a valid primary stat.", next);
                        }
                    }
                }
            }
        }

        public void ParseString(string s)
        {
            var eset = SharedResources.Eset!;

            string parseStr = s;

            string next = global::FlareEngine.Parse.PopFirstString(ref parseStr);

            if (next == "default") Mode = ModeDefault;
            else if (next == "fixed") Mode = ModeFixed;
            else if (next == "source_stat") Mode = ModeStat;
            else if (next == "source_level") Mode = ModeLevel;
            else if (next == "hero_stat")
            {
                Mode = ModeStat;
                RatioSource = RatioSourceHero;
            }
            else if (next == "hero_level")
            {
                Mode = ModeLevel;
                RatioSource = RatioSourceHero;
            }
            else Utils.LogError("SpawnLevel: Unknown spawn level mode '%s'", next);

            if (Mode != ModeDefault)
            {
                Ratio = global::FlareEngine.Parse.PopFirstFloat(ref parseStr);

                if (Mode != ModeFixed)
                {
                    next = global::FlareEngine.Parse.PopFirstString(ref parseStr);

                    if (Mode == ModeStat)
                    {
                        int primStatIndex = eset.PrimaryStats.GetIndexByID(next);

                        if (primStatIndex != eset.PrimaryStats.Stats.Count)
                        {
                            Stat = primStatIndex;
                        }
                        else
                        {
                            Utils.LogError("SpawnLevel: '%s' is not a valid primary stat.", next);
                        }
                    }
                }
            }
        }

        public void ApplyToStatBlock(StatBlock? srcStats, StatBlock? ratioStats)
        {
            if (srcStats == null)
                return;

            if (SharedGameResources.Pc != null && RatioSource == RatioSourceHero)
            {
                ratioStats = SharedGameResources.Pc.Stats;
            }

            if (Mode == ModeFixed)
            {
                srcStats.Level = (int)Ratio;
            }
            else if (ratioStats != null && Ratio != 0)
            {
                if (Mode == ModeLevel)
                {
                    srcStats.Level = (int)((float)ratioStats.Level * Ratio);
                }
                else if (Mode == ModeStat)
                {
                    int statVal = 0;
                    for (int i = 0; i < SharedResources.Eset!.PrimaryStats.Stats.Count; ++i)
                    {
                        if (Stat == i)
                        {
                            statVal = ratioStats.GetPrimary(i);
                            break;
                        }
                    }

                    srcStats.Level = (int)((float)statVal * Ratio);
                }
            }
        }
    }

    /// <summary>
    /// MapGroupC++ <c>class Map_Group</c>。
    /// </summary>
    public class MapGroup
    {
        public const int DefaultWanderRadius = 4;
        public const int RandomDirection = -1;

        public string Type = "";
        public string Category = "";
        public Int2 Pos;
        public Int2 Area;
        public int Levelmin;
        public int Levelmax;
        public int Numbermin;
        public int Numbermax;
        public float Chance;
        public int Direction;
        public List<Vector2> Waypoints = new List<Vector2>();
        public int WanderRadius;
        public List<EventComponent> Requirements = new List<EventComponent>();
        public List<EventComponent> InvincibleRequirements = new List<EventComponent>();
        public SpawnLevel SpawnLevel = new SpawnLevel();

        public MapGroup()
        {
            Type = "";
            Category = "";
            Pos = default;
            Area = new Int2(1, 1);
            Levelmin = 0;
            Levelmax = 0;
            Numbermin = 1;
            Numbermax = 1;
            Chance = 100.0f;
            Direction = RandomDirection;
            Waypoints = new List<Vector2>();
            WanderRadius = DefaultWanderRadius;
            Requirements = new List<EventComponent>();
            InvincibleRequirements = new List<EventComponent>();
            SpawnLevel = new SpawnLevel();
        }

        /// <summary>? C++ 已全局导入，此处省略。/summary>
        public MapGroup(MapGroup other)
        {
            Type = other.Type;
            Category = other.Category;
            Pos = other.Pos;
            Area = other.Area;
            Levelmin = other.Levelmin;
            Levelmax = other.Levelmax;
            Numbermin = other.Numbermin;
            Numbermax = other.Numbermax;
            Chance = other.Chance;
            Direction = other.Direction;
            Waypoints = new List<Vector2>(other.Waypoints);
            WanderRadius = other.WanderRadius;
            Requirements = new List<EventComponent>();
            for (int i = 0; i < other.Requirements.Count; ++i)
            {
                Requirements.Add(new EventComponent(other.Requirements[i]));
            }
            InvincibleRequirements = new List<EventComponent>();
            for (int i = 0; i < other.InvincibleRequirements.Count; ++i)
            {
                InvincibleRequirements.Add(new EventComponent(other.InvincibleRequirements[i]));
            }
            SpawnLevel = other.SpawnLevel;
        }
    }

    /// <summary>
    /// MapNpcC++ <c>class Map_NPC</c>。
    /// </summary>
    public class MapNpc
    {
        public const int DefaultWanderRadius = 0;
        public const int RandomDirection = -1;

        public string Type = "";
        public string Id = "";
        public Vector2 Pos;
        public List<EventComponent> Requirements = new List<EventComponent>();
        public int Direction;
        public List<Vector2> Waypoints = new List<Vector2>();
        public int WanderRadius;

        public MapNpc()
        {
            Type = "";
            Id = "";
            Pos = default;
            Requirements = new List<EventComponent>();
            Direction = RandomDirection;
            Waypoints = new List<Vector2>();
            WanderRadius = DefaultWanderRadius;
        }

        /// <summary>? C++ 已全局导入，此处省略。/summary>
        public MapNpc(MapNpc other)
        {
            Type = other.Type;
            Id = other.Id;
            Pos = other.Pos;
            Requirements = new List<EventComponent>();
            for (int i = 0; i < other.Requirements.Count; ++i)
            {
                Requirements.Add(new EventComponent(other.Requirements[i]));
            }
            Direction = other.Direction;
            Waypoints = new List<Vector2>(other.Waypoints);
            WanderRadius = other.WanderRadius;
        }
    }

    /// <summary>
    /// MapEnemyC++ <c>class Map_Enemy</c>。
    /// </summary>
    public class MapEnemy
    {
        public string Type = "";
        public Vector2 Pos;
        public int Direction;
        public Queue<Vector2> Waypoints = new Queue<Vector2>();
        public int WanderRadius;
        public bool HeroAlly;
        public bool EnemyAlly;
        public PowerID SummonPowerIndex;
        public StatBlock? Summoner;
        public List<EventComponent> Requirements = new List<EventComponent>();
        public List<EventComponent> InvincibleRequirements = new List<EventComponent>();
        public SpawnLevel SpawnLevel = new SpawnLevel();

        public MapEnemy(string type = "", Vector2 pos = default)
        {
            Type = type;
            Pos = pos;
            Direction = Program.Rng.Next() % 8;
            Waypoints = new Queue<Vector2>();
            WanderRadius = MapGroup.DefaultWanderRadius;
            HeroAlly = false;
            EnemyAlly = false;
            SummonPowerIndex = 0;
            Summoner = null;
            Requirements = new List<EventComponent>();
            InvincibleRequirements = new List<EventComponent>();
            SpawnLevel = new SpawnLevel();
        }
    }

    /// <summary>
    /// ChunkC++ <c>class Chunk</c>。
    /// </summary>
    public class Chunk
    {
        public const int TypeEmpty = 0;
        public const int TypeLinks = 1;
        public const int TypeNormal = 2;
        public const int TypeStart = 3;
        public const int TypeEnd = 4;
        public const int TypeKey = 5;
        public const int TypeDoorNorthSouth = 6;
        public const int TypeDoorWestEast = 7;
        public const int TypeBranch = 8;
        public const int TypeCount = 9;

        public const int LinkNorth = 0;
        public const int LinkSouth = 1;
        public const int LinkWest = 2;
        public const int LinkEast = 3;
        public const int LinkCount = 4;

        public int Type;
        public int DoorLevel;
        public int Variant;
        public Chunk?[] Links = new Chunk?[LinkCount];

        public Chunk()
        {
            Type = TypeEmpty;
            DoorLevel = 0;
            Variant = 0;
            Links = new Chunk?[LinkCount];
            for (int i = 0; i < LinkCount; ++i)
            {
                Links[i] = null;
            }
        }

        public bool IsStraight()
        {
            return ((Links[LinkNorth] != null && Links[LinkSouth] != null && Links[LinkWest] == null && Links[LinkEast] == null) || (Links[LinkNorth] == null && Links[LinkSouth] == null && Links[LinkWest] != null && Links[LinkEast] != null));
        }
    }

    /// <summary>
    /// Map - 地图数据类，对应 C++ class Map。
    /// 存储地图图层、事件、敌人组、NPC、瓦片碰撞数据。
    /// C++ 类型映射：Point→Int2, FPoint→Vector2, Rect→Rectangle。
    /// </summary>
    public class Map
    {
        private const bool ExitOnFail = true;

        private const int PathMain = 0;
        private const int PathBranch = 1;

        public const bool LoadProcgenCache = true;

        // C++ protected ?? statblocks?MapRenderer ?
        protected List<StatBlock> Statblocks => _statblocks;

        private List<StatBlock> _statblocks = new List<StatBlock>();

        private string _filename = "";
        private string _tileset = "";

        private List<Int2> _procgenBranchRoots = new List<Int2>();

        private int _procgenDoorsMax;
        private int _procgenDoorSpacingMin;
        private int _procgenBranchesPerDoorLevelMax;

        public string Filename => _filename;
        public string Tileset
        {
            get => _tileset;
            set => _tileset = value;
        }

        public string MusicFilename = "";

        public List<List<List<ushort>>> Layers = new List<List<List<ushort>>>();
        public List<string> Layernames = new List<string>();
        public List<uint> LayernamesHashed = new List<uint>();

        public Queue<MapEnemy> Enemies = new Queue<MapEnemy>();
        public List<MapGroup> EnemyGroups = new List<MapGroup>();
        public List<MapNpc> MapNpcs = new List<MapNpc>();

        public List<Event> Events = new List<Event>();
        public List<Event> DelayedEvents = new List<Event>();

        public string IntermapRandomFilename = "";
        public Queue<EventComponent> IntermapRandomQueue = new Queue<EventComponent>();

        public List<List<Chunk>> ProcgenChunks = new List<List<Chunk>>();

        public string Title = "";
        public ushort W;
        public ushort H;
        public bool HeroPosEnabled;
        public Vector2 HeroPos;
        public string ParallaxFilename = "";
        public Color BackgroundColor;
        public ushort Fogofwar;
        public bool SaveFogofwar;
        public bool ForceSpawnPos;
        public int ProcgenType;
        public StatusID ProcgenUniqueStatusId;
        public string ProcgenResetStatus = "";
        public StatusID ProcgenResetStatusId;

        public Map()
        {
            _filename = "";
            _procgenDoorsMax = 0;
            _procgenDoorSpacingMin = 0;
            _procgenBranchesPerDoorLevelMax = 0;
            Layers = new List<List<List<ushort>>>();
            Events = new List<Event>();
            W = 1;
            H = 1;
            HeroPosEnabled = false;
            HeroPos = default;
            BackgroundColor = new Color(0, 0, 0, 0);
            Fogofwar = SharedResources.Eset!.Misc.Fogofwar;
            SaveFogofwar = SharedResources.Eset.Misc.SaveFogofwar;
            ForceSpawnPos = false;
            ProcgenType = Chunk.TypeEmpty;
            ProcgenUniqueStatusId = 0;
            ProcgenResetStatus = "";
            ProcgenResetStatusId = 0;
        }

        ~Map()
        {
            ClearLayers();
        }

        public void RemoveLayer(uint index)
        {
            Layernames.RemoveAt((int)index);
            Layers.RemoveAt((int)index);
        }

        private static void ResizeList<T>(List<T> list, int newSize, Func<T> factory)
        {
            while (list.Count < newSize)
            {
                list.Add(factory());
            }
            while (list.Count > newSize)
            {
                list.RemoveAt(list.Count - 1);
            }
        }

        private void ClearLayers()
        {
            Layers.Clear();
            Layernames.Clear();
            LayernamesHashed.Clear();
        }

        private void ClearEntities()
        {
            Enemies = new Queue<MapEnemy>();
            EnemyGroups.Clear();
            MapNpcs.Clear();
        }

        public void ClearEvents()
        {
            Events.Clear();
            DelayedEvents.Clear();
            _statblocks.Clear();
        }

        public int Load(string fname, bool loadProcgenCache = false)
        {
            ProcgenChunks.Clear();

            ClearEvents();
            ClearLayers();
            ClearEntities();

            MusicFilename = "";
            ParallaxFilename = "";
            BackgroundColor = new Color(0, 0, 0, 0);
            Fogofwar = SharedResources.Eset!.Misc.Fogofwar;
            SaveFogofwar = SharedResources.Eset.Misc.SaveFogofwar;
            ForceSpawnPos = false;

            W = 1;
            H = 1;
            HeroPosEnabled = false;
            HeroPos.X = 0;
            HeroPos.Y = 0;

            Utils.LogInfo("Map: Loading map '%s'", fname);

            _filename = fname;

            string procgenFilename = GetProcgenFilename();

            // @CLASS Map|Description of maps/
            using FileParser infile = new FileParser();
            if (loadProcgenCache)
            {
                if (!infile.Open(procgenFilename, !FileParser.ModFile, FileParser.ErrorNormal))
                {
                    // couldn't load cached map, try loading the original
                    if (!infile.Open(fname, FileParser.ModFile, FileParser.ErrorNormal))
                        return 0;
                }
            }
            else
            {
                if (!infile.Open(fname, FileParser.ModFile, FileParser.ErrorNormal))
                    return 0;
            }

            while (infile.Next())
            {
                if (infile.NewSection)
                {

                    // for sections that are stored in collections, add a new object here
                    if (infile.Section == "enemy")
                        EnemyGroups.Add(new MapGroup());
                    else if (infile.Section == "npc")
                        MapNpcs.Add(new MapNpc());
                    else if (infile.Section == "event")
                        Events.Add(new Event());

                }
                if (infile.Section == "header")
                    LoadHeader(infile);
                else if (infile.Section == "layer")
                    LoadLayer(infile);
                else if (infile.Section == "enemy")
                    LoadEnemyGroup(infile, EnemyGroups[^1]);
                else if (infile.Section == "npc")
                    LoadNPC(infile);
                else if (infile.Section == "event")
                    SharedGameResources.Eventm!.LoadEvent(infile, Events[^1]);
            }

            infile.Close();

            // generate any procedural regions
            List<Event> procgenRegions = new List<Event>();
            for (int i = 0; i < Events.Count; ++i)
            {
                EventComponent? ecProcgen = Events[i].GetComponent(EventComponent.ProcgenFilename);
                if (ecProcgen != null)
                {
                    procgenRegions.Add(Events[i]);
                }
            }

            if (procgenRegions.Count > 1)
            {
                Utils.LogInfo("Map: Warning! Only 1 'procgen_filename' event is supported.");
            }

            // if a previously generated map exists, load it from disk cache
            // otherwise, save the generated map to disk
            if (procgenRegions.Count > 0)
            {
                if (!SharedGameResources.Camp!.CheckStatus(ProcgenResetStatusId) && Filesystem.FileExists(procgenFilename))
                {
                    return Load(fname, LoadProcgenCache);
                }
                else
                {
                    EventComponent? ecProcgen = procgenRegions[0].GetComponent(EventComponent.ProcgenFilename);
                    ProcGenFillArea(ecProcgen!.S, procgenRegions[0].Location);

                    MapSaver mapSaver = new MapSaver(this);
                    Utils.LogInfo("Saving map: %s", fname);
                    mapSaver.SaveMap(procgenFilename, "");

                    // we can't use the spawn position from the player's save file if we're generating a new map
                    ForceSpawnPos = true;
                }
            }

            // load fog-of-war configuration file
            // TODO does this need to be done on every map load?
            if (Fogofwar != 0)
            {
                SharedGameResources.Fow!.Load();
            }

            // create StatBlocks for events that need powers
            for (uint i = 0; i < Events.Count; ++i)
            {
                EventComponent? ecPower = Events[(int)i].GetComponent(EventComponent.Power);
                if (ecPower != null)
                {
                    // store the index of this StatBlock so that we can find it when the event is activated
                    ecPower.Data[0].Int = AddEventStatBlock(Events[(int)i]);
                }
            }

            // ensure that our map contains a collision layer
            if (!Layernames.Contains("collision"))
            {
                Layernames.Add("collision");
                Layers.Add(new List<List<ushort>>());
                ResizeList(Layers[^1], W, () => new List<ushort>());
                for (int i = 0; i < Layers[^1].Count; ++i)
                {
                    ResizeList(Layers[^1][i], H, () => (ushort)0);
                }
            }

            if (Fogofwar != 0)
            {
                // load the fog-of-war data from disk cache, unless this map was just procedurally generated
                if (SaveFogofwar && procgenRegions.Count == 0)
                {
                    string fowFilename = GetFOWFilename();
                    if (infile.Open(fowFilename, !FileParser.ModFile, FileParser.ErrorNormal))
                    {
                        while (infile.Next())
                        {
                            if (infile.Section == "layer")
                            {
                                if (!LoadLayer(infile, !ExitOnFail))
                                {
                                    for (int i = Layers.Count; i > 0; i--)
                                    {
                                        int layerIndex = i - 1;
                                        if (Layernames[layerIndex] == "fow_fog")
                                        {
                                            Layernames.RemoveAt(layerIndex);
                                            Layers.RemoveAt(layerIndex);
                                        }
                                        else if (Layernames[layerIndex] == "fow_dark")
                                        {
                                            Layernames.RemoveAt(layerIndex);
                                            Layers.RemoveAt(layerIndex);
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        infile.Close();
                    }
                }

                // ensure that our map contains a fog of war layers
                if (!Layernames.Contains("fow_fog"))
                {
                    Layernames.Add("fow_fog");
                    Layers.Add(new List<List<ushort>>());
                    ResizeList(Layers[^1], W, () => new List<ushort>());
                    for (int i = 0; i < Layers[^1].Count; ++i)
                    {
                        ResizeList(Layers[^1][i], H, () => FogOfWar.TileHidden);
                    }
                }

                if (!Layernames.Contains("fow_dark"))
                {
                    Layernames.Add("fow_dark");
                    Layers.Add(new List<List<ushort>>());
                    ResizeList(Layers[^1], W, () => new List<ushort>());
                    for (int i = 0; i < Layers[^1].Count; ++i)
                    {
                        ResizeList(Layers[^1][i], H, () => FogOfWar.TileHidden);
                    }
                }
            }

            if (!HeroPosEnabled)
            {
                Utils.LogError("Map: Hero spawn position (hero_pos) not defined in map header. Defaulting to (0,0).");
            }

            // hash layer names for more performant checking from events
            LayernamesHashed.Clear();
            ResizeList(LayernamesHashed, Layers.Count, () => (uint)0);
            for (int i = 0; i < LayernamesHashed.Count; ++i)
            {
                LayernamesHashed[i] = (uint)Utils.HashString(Layernames[i]);
            }

            return 0;
        }

        private void LoadHeader(FileParser infile)
        {
            if (infile.Key == "title")
            {
                // @ATTR title|string|Title of map
                Title = SharedResources.Msg!.Get(infile.Val);
            }
            else if (infile.Key == "width")
            {
                // @ATTR width|int|Width of map
                W = (ushort)Math.Max(Parse.ToInt(infile.Val), 1);
            }
            else if (infile.Key == "height")
            {
                // @ATTR height|int|Height of map
                H = (ushort)Math.Max(Parse.ToInt(infile.Val), 1);
            }
            else if (infile.Key == "tileset")
            {
                // @ATTR tileset|filename|Filename of a tileset definition to use for map
                _tileset = infile.Val;
            }
            else if (infile.Key == "music")
            {
                // @ATTR music|filename|Filename of background music to use for map
                MusicFilename = infile.Val;
            }
            else if (infile.Key == "hero_pos")
            {
                // @ATTR hero_pos|point|The player will spawn in this location if no point was previously given.
                HeroPos.X = (float)Parse.PopFirstInt(ref infile.Val) + 0.5f;
                HeroPos.Y = (float)Parse.PopFirstInt(ref infile.Val) + 0.5f;
                HeroPosEnabled = true;
            }
            else if (infile.Key == "parallax_layers")
            {
                // @ATTR parallax_layers|filename|Filename of a parallax layers definition.
                ParallaxFilename = infile.Val;
            }
            else if (infile.Key == "background_color")
            {
                // @ATTR background_color|color, int : Color, alpha|Background color for the map.
                BackgroundColor = Parse.ToRGBA(infile.Val);
            }
            else if (infile.Key == "fogofwar")
            {
                // @ATTR fogofwar|int|Set the fog of war type. 0-disabled, 1-minimap, 2-tint, 3-overlay. Overrides engine settings.
                Fogofwar = (ushort)Parse.ToInt(infile.Val);
            }
            else if (infile.Key == "save_fogofwar")
            {
                // @ATTR save_fogofwar|bool|If true, the fog of war layer keeps track of the progress. Overrides engine settings.
                SaveFogofwar = global::FlareEngine.Parse.ToBool(infile.Val);
            }
            else if (infile.Key == "tilewidth")
            {
                // @ATTR tilewidth|int|Inherited from Tiled map file. Unused by engine.
            }
            else if (infile.Key == "tileheight")
            {
                // @ATTR tileheight|int|Inherited from Tiled map file. Unused by engine.
            }
            else if (infile.Key == "procgen_type")
            {
                // TODO rename to "procgen_chunk_type"?
                // @ATTR procgen_type|["links", "normal", "start", "end", "key", "door_north_south", "door_west_east"]|Defines the type of chunk this map is when used in procedural map generation.
                if (infile.Val == "links")
                {
                    ProcgenType = Chunk.TypeLinks;
                }
                else if (infile.Val == "normal")
                {
                    ProcgenType = Chunk.TypeNormal;
                }
                else if (infile.Val == "start")
                {
                    ProcgenType = Chunk.TypeStart;
                }
                else if (infile.Val == "end")
                {
                    ProcgenType = Chunk.TypeEnd;
                }
                else if (infile.Val == "key")
                {
                    ProcgenType = Chunk.TypeKey;
                }
                else if (infile.Val == "door_north_south")
                {
                    ProcgenType = Chunk.TypeDoorNorthSouth;
                }
                else if (infile.Val == "door_west_east")
                {
                    ProcgenType = Chunk.TypeDoorWestEast;
                }
                else
                {
                    infile.Error("Map: '%s' is not a valid procedural generation chunk type.", infile.Val);
                }
            }
            else if (infile.Key == "procgen_unique_status")
            {
                // @ATTR procgen_unique_status|string|Sets this campaign status once this map is placed as a chunk during procedural generation. Once the status is set, the chunk will no longer be eligible to be placed.
                ProcgenUniqueStatusId = SharedGameResources.Camp!.RegisterStatus(infile.Val);
            }
            else if (infile.Key == "procgen_reset_status")
            {
                // @ATTR procgen_reset_status|string|If the map contains a procgen region, it will be regenerated if this campaign status is set.
                ProcgenResetStatus = infile.Val;
                ProcgenResetStatusId = SharedGameResources.Camp!.RegisterStatus(ProcgenResetStatus);
            }
            else if (infile.Key == "orientation")
            {
                // this is only used by Tiled when importing Flare maps
            }
            else if (infile.Key == "procgen_chunks")
            {
                // this is written by MapSaver for procedural maps

                // width/height in chunks
                int cmapW = Parse.PopFirstInt(ref infile.Val);
                int cmapH = Parse.PopFirstInt(ref infile.Val);
                ProcgenChunks.Clear();
                for (int y = 0; y < cmapH; ++y)
                {
                    List<Chunk> row = new List<Chunk>();
                    for (int x = 0; x < cmapW; ++x)
                    {
                        row.Add(new Chunk());
                    }
                    ProcgenChunks.Add(row);
                }

                int cmapX = 0;
                int cmapY = 0;

                string chunkType = global::FlareEngine.Parse.PopFirstString(ref infile.Val);
                while (chunkType.Length != 0 && cmapY < ProcgenChunks.Count && cmapX < ProcgenChunks[cmapY].Count)
                {
                    Chunk chunk = ProcgenChunks[cmapY][cmapX];
                    chunk.Type = Parse.ToInt(chunkType);
                    chunk.DoorLevel = Parse.PopFirstInt(ref infile.Val);
                    chunk.Variant = Parse.PopFirstInt(ref infile.Val);

                    if (Parse.PopFirstInt(ref infile.Val) == 1 && cmapY > 0)
                    {
                        chunk.Links[Chunk.LinkNorth] = ProcgenChunks[cmapY - 1][cmapX];
                    }
                    if (Parse.PopFirstInt(ref infile.Val) == 1 && cmapY + 1 < ProcgenChunks.Count)
                    {
                        chunk.Links[Chunk.LinkSouth] = ProcgenChunks[cmapY + 1][cmapX];
                    }
                    if (Parse.PopFirstInt(ref infile.Val) == 1 && cmapX > 0)
                    {
                        chunk.Links[Chunk.LinkWest] = ProcgenChunks[cmapY][cmapX - 1];
                    }
                    if (Parse.PopFirstInt(ref infile.Val) == 1 && cmapX + 1 < ProcgenChunks[cmapY].Count)
                    {
                        chunk.Links[Chunk.LinkEast] = ProcgenChunks[cmapY][cmapX + 1];
                    }

                    cmapX++;
                    if (cmapX >= ProcgenChunks[cmapY].Count)
                    {
                        cmapX = 0;
                        cmapY++;
                    }

                    chunkType = global::FlareEngine.Parse.PopFirstString(ref infile.Val);
                }
            }
            else
            {
                infile.Error("Map: '%s' is not a valid key.", infile.Key);
            }
        }

        private bool LoadLayer(FileParser infile, bool exitOnFail = ExitOnFail)
        {
            if (infile.Key == "type")
            {
                // @ATTR layer.type|string|Map layer type.
                Layers.Add(new List<List<ushort>>());
                ResizeList(Layers[^1], W, () => new List<ushort>());
                for (int i = 0; i < Layers[^1].Count; ++i)
                {
                    ResizeList(Layers[^1][i], H, () => (ushort)0);
                }
                Layernames.Add(infile.Val);
            }
            else if (infile.Key == "format")
            {
                // @ATTR layer.format|string|Format for map layer, must be 'dec'
                if (infile.Val != "dec")
                {
                    infile.Error("Map: The format of a layer must be 'dec'!");
                    if (exitOnFail)
                    {
                        Utils.LogErrorDialog("Map: The format of a layer must be 'dec'!");
                        SharedResources.Mods!.ResetModConfig();
                        Utils.Exit(1);
                    }
                    return false;
                }
            }
            else if (infile.Key == "data")
            {
                // @ATTR layer.data|raw|Raw map layer data
                // layer map data handled as a special case
                // The next h lines must contain layer data.
                for (int j = 0; j < H; j++)
                {
                    string val = infile.GetRawLine();
                    infile.IncrementLineNum();
                    if (val.Length != 0 && val[val.Length - 1] != ',')
                    {
                        val += ',';
                    }

                    // verify the width of this row
                    int commaCount = 0;
                    for (uint i = 0; i < val.Length; ++i)
                    {
                        if (val[(int)i] == ',') commaCount++;
                    }
                    if (commaCount != W)
                    {
                        infile.Error("Map: A row of layer data has a width not equal to %d.", W);
                        if (exitOnFail)
                        {
                            SharedResources.Mods!.ResetModConfig();
                            Utils.Exit(1);
                        }
                        return false;
                    }

                    for (int i = 0; i < W; i++)
                        Layers[^1][i][j] = (ushort)Parse.PopFirstInt(ref val);
                }
            }
            else
            {
                infile.Error("Map: '%s' is not a valid key.", infile.Key);
            }

            return true;
        }

        private void LoadEnemyGroup(FileParser infile, MapGroup group)
        {
            if (infile.Key == "type")
            {
                // @ATTR enemygroup.type|string|(IGNORED BY ENGINE) The "type" field, as used by Tiled and other mapping tools.
                group.Type = infile.Val;
            }
            else if (infile.Key == "category")
            {
                // @ATTR enemygroup.category|predefined_string|The category of enemies that will spawn in this group.
                group.Category = infile.Val;
            }
            else if (infile.Key == "level")
            {
                // @ATTR enemygroup.level|int, int : Min, Max|Defines the level range of enemies in group. If only one number is given, it's the exact level.
                group.Levelmin = Math.Max(0, Parse.PopFirstInt(ref infile.Val));
                group.Levelmax = Math.Max(Math.Max(0, Parse.ToInt(global::FlareEngine.Parse.PopFirstString(ref infile.Val))), group.Levelmin);
            }
            else if (infile.Key == "location")
            {
                // @ATTR enemygroup.location|rectangle|Location area for enemygroup
                group.Pos.X = Parse.PopFirstInt(ref infile.Val);
                group.Pos.Y = Parse.PopFirstInt(ref infile.Val);
                group.Area.X = Parse.PopFirstInt(ref infile.Val);
                group.Area.Y = Parse.PopFirstInt(ref infile.Val);
            }
            else if (infile.Key == "number")
            {
                // @ATTR enemygroup.number|int, int : Min, Max|Defines the range of enemies in group. If only one number is given, it's the exact amount.
                group.Numbermin = Math.Max(0, Parse.PopFirstInt(ref infile.Val));
                group.Numbermax = Math.Max(Math.Max(0, Parse.ToInt(global::FlareEngine.Parse.PopFirstString(ref infile.Val))), group.Numbermin);
            }
            else if (infile.Key == "chance")
            {
                // @ATTR enemygroup.chance|float|Initial percentage chance that this enemy group will be able to spawn enemies.
                group.Chance = Math.Min(100.0f, Math.Max(0.0f, global::FlareEngine.Parse.PopFirstFloat(ref infile.Val)));
            }
            else if (infile.Key == "direction")
            {
                // @ATTR enemygroup.direction|direction|Direction that enemies will initially face.
                group.Direction = Parse.ToDirection(infile.Val);
            }
            else if (infile.Key == "waypoints")
            {
                // @ATTR enemygroup.waypoints|list(point)|Enemy waypoints; single enemy only; negates wander_radius
                string a = global::FlareEngine.Parse.PopFirstString(ref infile.Val);
                string b = global::FlareEngine.Parse.PopFirstString(ref infile.Val);

                while (a.Length != 0)
                {
                    Vector2 p;
                    p.X = (float)Parse.ToInt(a) + 0.5f;
                    p.Y = (float)Parse.ToInt(b) + 0.5f;
                    group.Waypoints.Add(p);
                    a = global::FlareEngine.Parse.PopFirstString(ref infile.Val);
                    b = global::FlareEngine.Parse.PopFirstString(ref infile.Val);
                }

                // disable wander radius, since we can't have waypoints and wandering at the same time
                group.WanderRadius = 0;
            }
            else if (infile.Key == "wander_radius")
            {
                // @ATTR enemygroup.wander_radius|int|The radius (in tiles) that an enemy will wander around randomly; negates waypoints
                group.WanderRadius = Math.Max(0, Parse.PopFirstInt(ref infile.Val));

                // clear waypoints, since wandering will use the waypoint queue
                group.Waypoints.Clear();
            }
            else if (infile.Key == "requires_status")
            {
                // @ATTR enemygroup.requires_status|list(string)|Statuses required to be set for enemy group to load
                string s;
                while ((s = global::FlareEngine.Parse.PopFirstString(ref infile.Val)) != "")
                {
                    EventComponent ec = new EventComponent();
                    ec.Type = EventComponent.RequiresStatus;
                    ec.Status = SharedGameResources.Camp!.RegisterStatus(s);
                    group.Requirements.Add(ec);
                }
            }
            else if (infile.Key == "requires_not_status")
            {
                // @ATTR enemygroup.requires_not_status|list(string)|Statuses required to be unset for enemy group to load
                string s;
                while ((s = global::FlareEngine.Parse.PopFirstString(ref infile.Val)) != "")
                {
                    EventComponent ec = new EventComponent();
                    ec.Type = EventComponent.RequiresNotStatus;
                    ec.Status = SharedGameResources.Camp!.RegisterStatus(s);
                    group.Requirements.Add(ec);
                }
            }
            else if (infile.Key == "requires_level")
            {
                // @ATTR enemygroup.requires_level|int|Player level must be equal or greater to load enemy group
                EventComponent ec = new EventComponent();
                ec.Type = EventComponent.RequiresLevel;
                ec.Data[0].Int = Parse.PopFirstInt(ref infile.Val);
                group.Requirements.Add(ec);
            }
            else if (infile.Key == "requires_not_level")
            {
                // @ATTR enemygroup.requires_not_level|int|Player level must be lesser to load enemy group
                EventComponent ec = new EventComponent();
                ec.Type = EventComponent.RequiresNotLevel;
                ec.Data[0].Int = Parse.PopFirstInt(ref infile.Val);
                group.Requirements.Add(ec);
            }
            else if (infile.Key == "requires_currency")
            {
                // @ATTR enemygroup.requires_currency|int|Player currency must be equal or greater to load enemy group
                EventComponent ec = new EventComponent();
                ec.Type = EventComponent.RequiresCurrency;
                ec.Data[0].Int = Parse.PopFirstInt(ref infile.Val);
                group.Requirements.Add(ec);
            }
            else if (infile.Key == "requires_not_currency")
            {
                // @ATTR enemygroup.requires_not_currency|int|Player currency must be lesser to load enemy group
                EventComponent ec = new EventComponent();
                ec.Type = EventComponent.RequiresNotCurrency;
                ec.Data[0].Int = Parse.PopFirstInt(ref infile.Val);
                group.Requirements.Add(ec);
            }
            else if (infile.Key == "requires_item")
            {
                // @ATTR enemygroup.requires_item|list(item_id)|Item required to exist in player inventory to load enemy group. Quantity can be specified by appending ":Q" to the item_id, where Q is an integer.
                string s;
                while ((s = global::FlareEngine.Parse.PopFirstString(ref infile.Val)) != "")
                {
                    ItemStack itemStack = Parse.ToItemQuantityPair(s);
                    EventComponent ec = new EventComponent();
                    ec.Type = EventComponent.RequiresItem;
                    ec.Id = itemStack.Item;
                    ec.Data[0].Int = itemStack.Quantity;
                    group.Requirements.Add(ec);
                }
            }
            else if (infile.Key == "requires_not_item")
            {
                // @ATTR enemygroup.requires_not_item|list(item_id)|Item required to not exist in player inventory to load enemy group. Quantity can be specified by appending ":Q" to the item_id, where Q is an integer.
                string s;
                while ((s = global::FlareEngine.Parse.PopFirstString(ref infile.Val)) != "")
                {
                    ItemStack itemStack = Parse.ToItemQuantityPair(s);
                    EventComponent ec = new EventComponent();
                    ec.Type = EventComponent.RequiresNotItem;
                    ec.Id = itemStack.Item;
                    ec.Data[0].Int = itemStack.Quantity;
                    group.Requirements.Add(ec);
                }
            }
            else if (infile.Key == "requires_class")
            {
                // @ATTR enemygroup.requires_class|predefined_string|Player base class required to load enemy group
                EventComponent ec = new EventComponent();
                ec.Type = EventComponent.RequiresClass;
                ec.S = global::FlareEngine.Parse.PopFirstString(ref infile.Val);
                group.Requirements.Add(ec);
            }
            else if (infile.Key == "requires_not_class")
            {
                // @ATTR enemygroup.requires_not_class|predefined_string|Player base class not required to load enemy group
                EventComponent ec = new EventComponent();
                ec.Type = EventComponent.RequiresNotClass;
                ec.S = global::FlareEngine.Parse.PopFirstString(ref infile.Val);
                group.Requirements.Add(ec);
            }
            else if (infile.Key == "invincible_requires_status")
            {
                // @ATTR enemygroup.invincible_requires_status|list(string)|Enemies in this group are invincible to hero attacks when these statuses are set.
                string s;
                while ((s = global::FlareEngine.Parse.PopFirstString(ref infile.Val)) != "")
                {
                    EventComponent ec = new EventComponent();
                    ec.Type = EventComponent.RequiresStatus;
                    ec.Status = SharedGameResources.Camp!.RegisterStatus(s);
                    group.InvincibleRequirements.Add(ec);
                }
            }
            else if (infile.Key == "invincible_requires_not_status")
            {
                // @ATTR enemygroup.invincible_requires_not_status|list(string)|Enemies in this group are invincible to hero attacks when these statuses are not set.
                string s;
                while ((s = global::FlareEngine.Parse.PopFirstString(ref infile.Val)) != "")
                {
                    EventComponent ec = new EventComponent();
                    ec.Type = EventComponent.RequiresNotStatus;
                    ec.Status = SharedGameResources.Camp!.RegisterStatus(s);
                    group.InvincibleRequirements.Add(ec);
                }
            }
            else if (infile.Key == "spawn_level")
            {
                // @ATTR enemygroup.spawn_level|["default", "fixed", "source_level", "source_stat", "hero_level", "hero_stat"], float, predefined_string : Mode, Multiplier, Primary stat|The level of spawned creatures. The need for the last two parameters depends on the mode being used. The "default" mode will just use the entity's normal level and doesn't require any additional parameters. The "fixed" mode sets the multiplier as the enemy level. The level modes multiply with the target's level. The stat modes multiply by one of the target's primary stats. The stat is defined with the last parameter, which is simply the ID of the primary stat that should be used for scaling. Because the map has no level/stats of its own, the source modes use the hero's level/stats.
                group.SpawnLevel.Parse(infile);
            }
            else
            {
                infile.Error("Map: '%s' is not a valid key.", infile.Key);
            }
        }

        private void LoadNPC(FileParser infile)
        {
            if (infile.Key == "type")
            {
                // @ATTR npc.type|string|(IGNORED BY ENGINE) The "type" field, as used by Tiled and other mapping tools.
                MapNpcs[^1].Type = infile.Val;
            }
            else if (infile.Key == "filename")
            {
                // @ATTR npc.filename|filename|Filename of an NPC definition.
                MapNpcs[^1].Id = infile.Val;
            }
            else if (infile.Key == "location")
            {
                // @ATTR npc.location|point|Location of NPC
                MapNpcs[^1].Pos.X = (float)Parse.PopFirstInt(ref infile.Val) + 0.5f;
                MapNpcs[^1].Pos.Y = (float)Parse.PopFirstInt(ref infile.Val) + 0.5f;
            }
            else if (infile.Key == "requires_status")
            {
                // @ATTR npc.requires_status|list(string)|Statuses required to be set for NPC load
                string s;
                while ((s = global::FlareEngine.Parse.PopFirstString(ref infile.Val)) != "")
                {
                    EventComponent ec = new EventComponent();
                    ec.Type = EventComponent.RequiresStatus;
                    ec.Status = SharedGameResources.Camp!.RegisterStatus(s);
                    MapNpcs[^1].Requirements.Add(ec);
                }
            }
            else if (infile.Key == "requires_not_status")
            {
                // @ATTR npc.requires_not_status|list(string)|Statuses required to be unset for NPC load
                string s;
                while ((s = global::FlareEngine.Parse.PopFirstString(ref infile.Val)) != "")
                {
                    EventComponent ec = new EventComponent();
                    ec.Type = EventComponent.RequiresNotStatus;
                    ec.Status = SharedGameResources.Camp!.RegisterStatus(s);
                    MapNpcs[^1].Requirements.Add(ec);
                }
            }
            else if (infile.Key == "requires_level")
            {
                // @ATTR npc.requires_level|int|Player level must be equal or greater to load NPC
                EventComponent ec = new EventComponent();
                ec.Type = EventComponent.RequiresLevel;
                ec.Data[0].Int = Parse.PopFirstInt(ref infile.Val);
                MapNpcs[^1].Requirements.Add(ec);
            }
            else if (infile.Key == "requires_not_level")
            {
                // @ATTR npc.requires_not_level|int|Player level must be lesser to load NPC
                EventComponent ec = new EventComponent();
                ec.Type = EventComponent.RequiresNotLevel;
                ec.Data[0].Int = Parse.PopFirstInt(ref infile.Val);
                MapNpcs[^1].Requirements.Add(ec);
            }
            else if (infile.Key == "requires_currency")
            {
                // @ATTR npc.requires_currency|int|Player currency must be equal or greater to load NPC
                EventComponent ec = new EventComponent();
                ec.Type = EventComponent.RequiresCurrency;
                ec.Data[0].Int = Parse.PopFirstInt(ref infile.Val);
                MapNpcs[^1].Requirements.Add(ec);
            }
            else if (infile.Key == "requires_not_currency")
            {
                // @ATTR npc.requires_not_currency|int|Player currency must be lesser to load NPC
                EventComponent ec = new EventComponent();
                ec.Type = EventComponent.RequiresNotCurrency;
                ec.Data[0].Int = Parse.PopFirstInt(ref infile.Val);
                MapNpcs[^1].Requirements.Add(ec);
            }
            else if (infile.Key == "requires_item")
            {
                // @ATTR npc.requires_item|list(item_id)|Item required to exist in player inventory to load NPC. Quantity can be specified by appending ":Q" to the item_id, where Q is an integer.
                string s;
                while ((s = global::FlareEngine.Parse.PopFirstString(ref infile.Val)) != "")
                {
                    ItemStack itemStack = Parse.ToItemQuantityPair(s);
                    EventComponent ec = new EventComponent();
                    ec.Type = EventComponent.RequiresItem;
                    ec.Id = itemStack.Item;
                    ec.Data[0].Int = itemStack.Quantity;
                    MapNpcs[^1].Requirements.Add(ec);
                }
            }
            else if (infile.Key == "requires_not_item")
            {
                // @ATTR npc.requires_not_item|list(item_id)|Item required to not exist in player inventory to load NPC. Quantity can be specified by appending ":Q" to the item_id, where Q is an integer.
                string s;
                while ((s = global::FlareEngine.Parse.PopFirstString(ref infile.Val)) != "")
                {
                    ItemStack itemStack = Parse.ToItemQuantityPair(s);
                    EventComponent ec = new EventComponent();
                    ec.Type = EventComponent.RequiresNotItem;
                    ec.Id = itemStack.Item;
                    ec.Data[0].Int = itemStack.Quantity;
                    MapNpcs[^1].Requirements.Add(ec);
                }
            }
            else if (infile.Key == "requires_class")
            {
                // @ATTR npc.requires_class|predefined_string|Player base class required to load NPC
                EventComponent ec = new EventComponent();
                ec.Type = EventComponent.RequiresClass;
                ec.S = global::FlareEngine.Parse.PopFirstString(ref infile.Val);
                MapNpcs[^1].Requirements.Add(ec);
            }
            else if (infile.Key == "requires_not_class")
            {
                // @ATTR npc.requires_not_class|predefined_string|Player base class not required to load NPC
                EventComponent ec = new EventComponent();
                ec.Type = EventComponent.RequiresNotClass;
                ec.S = global::FlareEngine.Parse.PopFirstString(ref infile.Val);
                MapNpcs[^1].Requirements.Add(ec);
            }
            else if (infile.Key == "direction")
            {
                // @ATTR npc.direction|direction|Direction that NPC will initially face.
                MapNpcs[^1].Direction = Parse.ToDirection(infile.Val);
            }
            else if (infile.Key == "waypoints")
            {
                // @ATTR npc.waypoints|list(point)|NPC waypoints; negates wander_radius
                string a = global::FlareEngine.Parse.PopFirstString(ref infile.Val);
                string b = global::FlareEngine.Parse.PopFirstString(ref infile.Val);

                while (a.Length != 0)
                {
                    Vector2 p;
                    p.X = (float)Parse.ToInt(a) + 0.5f;
                    p.Y = (float)Parse.ToInt(b) + 0.5f;
                    MapNpcs[^1].Waypoints.Add(p);
                    a = global::FlareEngine.Parse.PopFirstString(ref infile.Val);
                    b = global::FlareEngine.Parse.PopFirstString(ref infile.Val);
                }

                // disable wander radius, since we can't have waypoints and wandering at the same time
                MapNpcs[^1].WanderRadius = 0;
            }
            else if (infile.Key == "wander_radius")
            {
                // @ATTR npc.wander_radius|int|The radius (in tiles) that an NPC will wander around randomly; negates waypoints
                MapNpcs[^1].WanderRadius = Math.Max(0, Parse.PopFirstInt(ref infile.Val));

                // clear waypoints, since wandering will use the waypoint queue
                MapNpcs[^1].Waypoints.Clear();
            }
            else
            {
                infile.Error("Map: '%s' is not a valid key.", infile.Key);
            }
        }

        public int AddEventStatBlock(Event evnt)
        {
            _statblocks.Add(new StatBlock());
            StatBlock statb = _statblocks[^1];

            EventComponent? ecPowerStats = evnt.GetComponent(EventComponent.PowerStats);
            if (ecPowerStats != null)
            {
                statb.Load(ecPowerStats.S);
            }

            EventComponent? ecPowerLevel = evnt.GetComponent(EventComponent.PowerLevel);
            if (ecPowerLevel != null)
            {
                SpawnLevel sl = new SpawnLevel();
                sl.ParseString(ecPowerLevel.S);
                sl.ApplyToStatBlock(statb, SharedGameResources.Pc!.Stats);
            }

            statb.PerfectAccuracy = true; // never miss AND never overhit

            EventComponent? ecPath = evnt.GetComponent(EventComponent.PowerPath);
            if (ecPath != null)
            {
                // source is power path start
                statb.Pos.X = (float)ecPath.Data[0].Int + 0.5f;
                statb.Pos.Y = (float)ecPath.Data[1].Int + 0.5f;
            }
            else
            {
                // source is event location
                statb.Pos.X = (float)evnt.Location.X + 0.5f;
                statb.Pos.Y = (float)evnt.Location.Y + 0.5f;
            }

            EventComponent? ecDamage = evnt.GetComponent(EventComponent.PowerDamage);
            if (ecDamage != null)
            {
                for (int i = 0; i < SharedResources.Eset!.DamageTypes.Types.Count; ++i)
                {
                    if (!SharedResources.Eset.DamageTypes.Types[i].IsElemental)
                    {
                        statb.Starting[Stats.Count + EngineSettings.DamageTypesSettings.IndexToMin(i)] = ecDamage.Data[0].Float; // min
                        statb.Starting[Stats.Count + EngineSettings.DamageTypesSettings.IndexToMax(i)] = ecDamage.Data[1].Float; // min
                    }
                }
            }

            // this is used to store cooldown ticks for a map power
            // the power id, type, etc are not used
            statb.PowersAi.Clear();
            ResizeList(statb.PowersAi, 1, () => new StatBlock.AIPower());

            // make this StatBlock immune to negative status effects
            // this is mostly to prevent a player with a damage return bonus from damaging this StatBlock
            // create a temporary EffectDef for immunity; will be used for map StatBlocks
            EffectDef immunityEffect = new EffectDef();
            immunityEffect.Id = "MAP_EVENT_IMMUNITY";
            immunityEffect.Type = Effect.ResistAll;

            EffectParams immunityParams = new EffectParams();
            immunityParams.Magnitude = 100;
            immunityParams.SourceType = Power.SourceTypeEnemy;

            statb.Effects.AddEffect(statb, immunityEffect, immunityParams);

            // ensure the statblock will be alive
            statb.Hp = statb.Starting[Stats.HpMax] = statb.Current[Stats.HpMax] = 1;

            // ensure that stats are ready to be used by running logic once
            statb.Logic();

            return _statblocks.Count - 1;
        }

        private Chunk? ProcGenWalkSingle(int pathType, int curX, int curY, int walkX, int walkY)
        {
            if (ProcgenChunks.Count == 0)
                return null;

            int mapSizeX = ProcgenChunks[0].Count;
            int mapSizeY = ProcgenChunks.Count;

            if (curX >= mapSizeX || curY >= mapSizeY)
                return null; // NOTE: should we always return a valid chunk instead?

            Chunk prev = ProcgenChunks[curY][curX];

            if ((walkX < 0 && curX == 0) || (walkY < 0 && curY == 0))
                return prev;
            else if ((curX + walkX >= mapSizeX) || (curY + walkY >= mapSizeY))
                return prev;

            Chunk next = ProcgenChunks[curY + walkY][curX + walkX];

            if (next.Type == Chunk.TypeStart)
                return prev;
            else if (next.Type == Chunk.TypeEnd)
                return prev;
            else if (next.Type == Chunk.TypeDoorNorthSouth || next.Type == Chunk.TypeDoorWestEast)
                return prev;

            // walking the main path doesn't allow overlapping existing path
            // branches, on the other hand, can overlap if their door level is the same
            if (pathType == PathMain)
            {
                if (next.Type != Chunk.TypeEmpty)
                    return prev;
            }

            int linkCount = 0;
            for (int i = 0; i < Chunk.LinkCount; ++i)
            {
                if (next.Links[i] != null)
                    linkCount++;
            }

            if (linkCount > 0 && (next.DoorLevel != prev.DoorLevel || Program.Rng.Next() % linkCount > 0))
                return prev;

            return next;
        }

        private int ProcGenCreatePath(int pathType, int desiredLength, int[]? doorCountOut, int startX, int startY)
        {
            if (ProcgenChunks.Count == 0)
                return 0;

            int mapSizeX = ProcgenChunks[0].Count;
            int mapSizeY = ProcgenChunks.Count;

            if (pathType == PathMain)
            {
                // zero out the map data
                for (int i = 0; i < mapSizeY; ++i)
                {
                    for (int j = 0; j < mapSizeX; ++j)
                    {
                        Chunk chunk = ProcgenChunks[i][j];
                        chunk.Type = Chunk.TypeEmpty;
                        chunk.DoorLevel = 0;
                        for (int k = 0; k < Chunk.LinkCount; ++k)
                        {
                            chunk.Links[k] = null;
                        }
                    }
                }

                _procgenBranchRoots.Clear();
            }

            // set start tile
            int curX = 0;
            int curY = 0;
            if (pathType == PathMain)
            {
                curX = Program.Rng.Next() % mapSizeX;
                curY = Program.Rng.Next() % mapSizeY;
            }
            else
            {
                curX = startX;
                curY = startY;
            }
            Chunk? current = ProcgenChunks[curY][curX];

            if (pathType == PathMain)
                current!.Type = Chunk.TypeStart;

            int steps = desiredLength;

            int doorCount = 0;
            int doorChance = 0;
            int doorDist = 0;
            int doorMinDist = _procgenDoorsMax > 0 ? Math.Max(2, desiredLength / _procgenDoorsMax) : 0;
            if (_procgenDoorSpacingMin > 0)
            {
                doorMinDist = Math.Max(2, _procgenDoorSpacingMin);
            }
            int pathLength = 1;
            int branchChance = 50;
            int branchCount = 0;
            int maxBranches = _procgenBranchesPerDoorLevelMax;

            while (steps >= 0)
            {
                int link = Program.Rng.Next() % Chunk.LinkCount;

                Chunk? prev = current;

                int linkRotate = Chunk.LinkCount;
                int rotateDir = MathUtils.PercentChance(50) ? 1 : -1;
                while (current!.Links[link] != null && linkRotate > 0)
                {
                    link += rotateDir;
                    if (link >= Chunk.LinkCount)
                    {
                        link = 0;
                    }
                    else if (link < 0)
                    {
                        link = Chunk.LinkCount - 1;
                    }
                    linkRotate--;
                }

                // if we're on a door chunk, we can only go in the direction opposite of the initial link
                if (current.Type == Chunk.TypeDoorNorthSouth && (link == Chunk.LinkWest || link == Chunk.LinkEast))
                {
                    if (current.Links[Chunk.LinkNorth] != null)
                        link = Chunk.LinkSouth;
                    else if (current.Links[Chunk.LinkSouth] != null)
                        link = Chunk.LinkNorth;
                }
                else if (current.Type == Chunk.TypeDoorWestEast && (link == Chunk.LinkNorth || link == Chunk.LinkSouth))
                {
                    if (current.Links[Chunk.LinkWest] != null)
                        link = Chunk.LinkEast;
                    else if (current.Links[Chunk.LinkEast] != null)
                        link = Chunk.LinkWest;
                }

                switch (link)
                {
                    case Chunk.LinkNorth:
                        current = ProcGenWalkSingle(pathType, curX, curY, 0, -1);
                        if (current != prev)
                        {
                            prev!.Links[Chunk.LinkNorth] = current;
                            current!.Links[Chunk.LinkSouth] = prev;
                            current.Type = Chunk.TypeNormal;
                            curY--;
                            doorDist++;
                            pathLength++;
                        }
                        break;
                    case Chunk.LinkSouth:
                        current = ProcGenWalkSingle(pathType, curX, curY, 0, 1);
                        if (current != prev)
                        {
                            prev!.Links[Chunk.LinkSouth] = current;
                            current!.Links[Chunk.LinkNorth] = prev;
                            current.Type = Chunk.TypeNormal;
                            curY++;
                            doorDist++;
                            pathLength++;
                        }
                        break;
                    case Chunk.LinkEast:
                        current = ProcGenWalkSingle(pathType, curX, curY, 1, 0);
                        if (current != prev)
                        {
                            prev!.Links[Chunk.LinkEast] = current;
                            current!.Links[Chunk.LinkWest] = prev;
                            current.Type = Chunk.TypeNormal;
                            curX++;
                            doorDist++;
                            pathLength++;
                        }
                        break;
                    case Chunk.LinkWest:
                        current = ProcGenWalkSingle(pathType, curX, curY, -1, 0);
                        if (current != prev)
                        {
                            prev!.Links[Chunk.LinkWest] = current;
                            current!.Links[Chunk.LinkEast] = prev;
                            current.Type = Chunk.TypeNormal;
                            curX--;
                            doorDist++;
                            pathLength++;
                        }
                        break;
                };

                if (current != prev)
                {
                    if (pathType == PathMain)
                    {
                        if (prev!.Type != Chunk.TypeStart && prev.IsStraight() && doorCount < _procgenDoorsMax && Program.Rng.Next() % 100 < doorChance)
                        {
                            if (prev.Links[Chunk.LinkNorth] != null && prev.Links[Chunk.LinkSouth] != null)
                                prev.Type = Chunk.TypeDoorNorthSouth;
                            else if (prev.Links[Chunk.LinkWest] != null && prev.Links[Chunk.LinkEast] != null)
                                prev.Type = Chunk.TypeDoorWestEast;
                            else
                            {
                                // shouldn't be able to get here!
                                Utils.LogError("Map: Trying to place door chunk, but links don't form a straight path.");
                            }
                            prev.DoorLevel++;
                            current!.DoorLevel++;
                            doorChance = 0;
                            doorCount++;
                            doorDist = 0;
                            branchCount = 0;
                        }

                        if (doorDist > doorMinDist)
                        {
                            if (doorCount == 0)
                                doorChance = 100;
                            else
                                doorChance += 5;
                        }

                        if (current!.Type == Chunk.TypeNormal && branchCount < maxBranches && Program.Rng.Next() % 100 < branchChance)
                        {
                            _procgenBranchRoots.Add(new Int2(curX, curY));
                            branchChance = 50;
                            branchCount++;
                        }

                        branchChance += 5;
                    }

                    current!.DoorLevel = prev.DoorLevel;
                }

                steps--;
            }

            // last room is the end
            if (pathType == PathMain)
                current!.Type = Chunk.TypeEnd;

            if (doorCountOut != null && doorCountOut.Length > 0)
                doorCountOut[0] = doorCount;

            return pathLength;
        }

        private void ProcGenFillArea(string configFilename, Rectangle area)
        {
            Utils.LogInfo("Procgen filename: %s", configFilename);

            int mapSizeX = 0;
            int mapSizeY = 0;

            int mainPathLengthMin = 0;
            int mainPathLengthMax = 0;
            int mainPathAttemptsMax = 0;
            int branchLengthMin = 0;
            int branchLengthMax = 0;

            List<string> chunkFilenames = new List<string>();

            // @CLASS Map: Procedural generation rules|Description of maps/procgen_rules
            using FileParser infile = new FileParser();
            if (infile.Open(configFilename, FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (infile.Section == "settings")
                    {
                        if (infile.Key == "doors_max")
                        {
                            // @ATTR settings.doors_max|int|Maximum number of door chunks that can be placed in this region.
                            _procgenDoorsMax = Parse.ToInt(infile.Val);
                        }
                        else if (infile.Key == "main_path_length_min")
                        {
                            // @ATTR settings.main_path_length_min|int|Minimum length of the 'main' path (start chunk to end chunk).
                            mainPathLengthMin = Parse.ToInt(infile.Val);
                        }
                        else if (infile.Key == "main_path_length_max")
                        {
                            // @ATTR settings.main_path_length_max|int|Maximum length of the 'main' path (start chunk to end chunk).
                            mainPathLengthMax = Parse.ToInt(infile.Val);
                        }
                        else if (infile.Key == "main_path_attempts_max")
                        {
                            // @ATTR settings.main_path_attempts_max|int|Maximum number of attempts to generate the main path to match the min/max constraints.
                            mainPathAttemptsMax = Parse.ToInt(infile.Val);
                        }
                        else if (infile.Key == "branch_length_min")
                        {
                            // @ATTR settings.branch_length_min|int|Minimum number of steps taken when creating a branch off the main path.
                            branchLengthMin = Parse.ToInt(infile.Val);
                        }
                        else if (infile.Key == "branch_length_max")
                        {
                            // @ATTR settings.branch_length_max|int|Maximum number of steps taken when creating a branch off the main path.
                            branchLengthMax = Parse.ToInt(infile.Val);
                        }
                        else if (infile.Key == "branches_per_door_level_max")
                        {
                            // @ATTR settings.branches_per_door_level_max|int|Maximum number of branches that can be created between each 'door level'. A door level is defined as the set of chunks between a set of 2 door chunks (start and end chunks count as doors here).
                            _procgenBranchesPerDoorLevelMax = Parse.ToInt(infile.Val);
                        }
                        else if (infile.Key == "door_spacing_min")
                        {
                            // @ATTR settings.door_spacing_min|int|The minimum number of chunk length of each door level.
                            _procgenDoorSpacingMin = Parse.ToInt(infile.Val);
                        }
                    }
                    else if (infile.Section == "chunks")
                    {
                        if (infile.Key == "filename")
                        {
                            // @ATTR chunks.filename|repeatable(filename)|Map chunk file.
                            chunkFilenames.Add(infile.Val);
                        }
                    }
                }
                infile.Close();
            }

            // turn fog-of-war off when loading map chunks
            var eset = SharedResources.Eset!;
            ushort tempFow = eset.Misc.Fogofwar;
            eset.Misc.Fogofwar = 0;

            List<List<Map>> chunkMaps = new List<List<Map>>();
            for (int i = 0; i < Chunk.TypeCount; ++i)
            {
                chunkMaps.Add(new List<Map>());
            }

            for (int i = 0; i < chunkFilenames.Count; ++i)
            {
                Map chunkMap = new Map();
                chunkMap.Load(chunkFilenames[i]);

                if (mapSizeX == 0 && chunkMap.W > 0 && chunkMap.H > 0)
                {
                    mapSizeX = area.Width / chunkMap.W;
                    mapSizeY = area.Height / chunkMap.H;
                    Utils.LogInfo("Map: Set procedural generation region to %lux%lu chunks. Each chunk is %lux%lu.", mapSizeX, mapSizeY, chunkMap.W, chunkMap.H);
                }

                if (chunkMap.ProcgenType != Chunk.TypeEmpty)
                    chunkMaps[chunkMap.ProcgenType].Add(chunkMap);
            }

            Rectangle[] linkRects = new Rectangle[4];

            if (chunkMaps[Chunk.TypeLinks].Count > 0)
            {
                // the link rects are only taken from the first link chunk
                Map chunkLinks = chunkMaps[Chunk.TypeLinks][0];

                for (int i = 0; i < chunkLinks.Events.Count; ++i)
                {
                    EventComponent? ec = chunkLinks.Events[i].GetComponent(EventComponent.ProcgenLink);
                    if (ec != null)
                    {
                        if (ec.S == "north")
                        {
                            linkRects[Chunk.LinkNorth] = chunkLinks.Events[i].Location;
                        }
                        else if (ec.S == "south")
                        {
                            linkRects[Chunk.LinkSouth] = chunkLinks.Events[i].Location;
                        }
                        else if (ec.S == "west")
                        {
                            linkRects[Chunk.LinkWest] = chunkLinks.Events[i].Location;
                        }
                        else if (ec.S == "east")
                        {
                            linkRects[Chunk.LinkEast] = chunkLinks.Events[i].Location;
                        }
                    }
                }
            }

            eset.Misc.Fogofwar = tempFow;

            //
            // BEGIN CHUNK GENERATION
            //

            ProcgenChunks.Clear();
            for (int y = 0; y < mapSizeY; ++y)
            {
                List<Chunk> row = new List<Chunk>();
                for (int x = 0; x < mapSizeX; ++x)
                {
                    row.Add(new Chunk());
                }
                ProcgenChunks.Add(row);
            }

            List<List<Chunk?>> normalChunks = new List<List<Chunk?>>();
            _procgenBranchRoots.Clear();

            for (int i = 0; i < _procgenDoorsMax; ++i)
            {
                normalChunks.Add(new List<Chunk?>());
            }

            int mainPathLength = 0;
            mainPathLengthMin = mainPathLengthMin == 0 ? (mapSizeX * mapSizeY) / 2 : Math.Max(3, mainPathLengthMin);
            mainPathLengthMax = mainPathLengthMax == 0 ? mapSizeX * mapSizeY : Math.Max(mainPathLengthMin, mainPathLengthMax);
            int genAttempts = 0;
            int doorCount = 0;

            int desiredMainPathLength = MathUtils.RandBetween(mainPathLengthMin, mainPathLengthMax);

            int[] doorCountHolder = new int[1];
            while ((mainPathLength < mainPathLengthMin || mainPathLength > mainPathLengthMax || (doorCount == 0 && _procgenDoorsMax > 0)) && genAttempts < mainPathAttemptsMax)
            {
                mainPathLength = ProcGenCreatePath(PathMain, desiredMainPathLength, doorCountHolder, 0, 0);
                doorCount = doorCountHolder[0];

                genAttempts++;
            }
            Utils.LogInfo("Map: Generated main path with length=%d and branches=%lu. Generator attempts: %d", mainPathLength, _procgenBranchRoots.Count, genAttempts);

            for (int i = 0; i < _procgenBranchRoots.Count; ++i)
            {
                int branchLength = MathUtils.RandBetween(branchLengthMin, Math.Min(branchLengthMin, branchLengthMax));
                ProcGenCreatePath(PathBranch, branchLength, null, _procgenBranchRoots[i].X, _procgenBranchRoots[i].Y);
                // map_chunks[branch_roots[i].second][branch_roots[i].first].type = Chunk::TYPE_BRANCH;
            }

            Int2 startChunk = default;

            // gather all normal chunks for placing keys/treasure/etc.
            for (int i = 0; i < mapSizeY; ++i)
            {
                for (int j = 0; j < mapSizeX; ++j)
                {
                    Chunk chunk = ProcgenChunks[i][j];

                    if (chunk.Type == Chunk.TypeNormal && chunk.DoorLevel < normalChunks.Count)
                    {
                        normalChunks[chunk.DoorLevel].Add(chunk);
                    }
                    else if (chunk.Type == Chunk.TypeStart)
                    {
                        startChunk.X = j;
                        startChunk.Y = i;
                    }
                }
            }

            for (int i = 0; i < doorCount; ++i)
            {
                if (i >= normalChunks.Count)
                    break;

                if (normalChunks[i].Count == 0)
                {
                    Utils.LogError("Map: Generator tried to place key, but no chunks on current door level: %d", i);
                    continue;
                }

                int chunkIndex = Program.Rng.Next() % normalChunks[i].Count;
                Chunk? chunk = normalChunks[i][chunkIndex];
                chunk!.Type = Chunk.TypeKey;
                normalChunks[i].RemoveAt(chunkIndex);
            }

            List<int> validChunks = new List<int>();

            for (int chunkY = 0; chunkY < ProcgenChunks.Count; ++chunkY)
            {
                for (int chunkX = 0; chunkX < ProcgenChunks[chunkY].Count; ++chunkX)
                {
                    Chunk chunk = ProcgenChunks[chunkY][chunkX];
                    if (chunk.Type == Chunk.TypeEmpty)
                        continue;

                    if (chunkMaps[chunk.Type].Count == 0)
                        continue;

                    validChunks.Clear();
                    for (int i = 0; i < chunkMaps[chunk.Type].Count; ++i)
                    {
                        Map testMap = chunkMaps[chunk.Type][i];
                        if (testMap.ProcgenUniqueStatusId > 0 && SharedGameResources.Camp!.CheckStatus(testMap.ProcgenUniqueStatusId))
                            continue;

                        validChunks.Add(i);
                    }

                    if (validChunks.Count == 0)
                    {
                        Utils.LogError("Map: Generator unable to find valid chunk for region: (%lu, %lu)", chunkX, chunkY);
                        continue;
                    }

                    int validChunkIndex = Program.Rng.Next() % validChunks.Count;

                    // attempt to reduce the occurrences of the same room variants being connected to each other
                    if (chunk.Links[Chunk.LinkWest] != null && chunk.Type == ProcgenChunks[chunkY][chunkX - 1].Type && validChunks[validChunkIndex] == ProcgenChunks[chunkY][chunkX - 1].Variant)
                    {
                        validChunkIndex++;
                        if (validChunkIndex >= validChunks.Count)
                            validChunkIndex = 0;
                    }
                    if (chunk.Links[Chunk.LinkNorth] != null && chunk.Type == ProcgenChunks[chunkY - 1][chunkX].Type && validChunks[validChunkIndex] == ProcgenChunks[chunkY - 1][chunkX].Variant)
                    {
                        validChunkIndex++;
                        if (validChunkIndex >= validChunks.Count)
                            validChunkIndex = 0;
                    }

                    chunk.Variant = validChunks[validChunkIndex];

                    Map chunkMap = chunkMaps[chunk.Type][chunk.Variant];

                    if (chunkMap.ProcgenUniqueStatusId > 0)
                    {
                        SharedGameResources.Camp!.SetStatus(chunkMap.ProcgenUniqueStatusId);
                    }

                    int linkChunkVariant = 0;
                    if (chunkMaps[Chunk.TypeLinks].Count > 1)
                        linkChunkVariant = Program.Rng.Next() % chunkMaps[Chunk.TypeLinks].Count;

                    Map chunkMapLinks = chunkMaps[Chunk.TypeLinks][linkChunkVariant];

                    int xOffset = chunkX * chunkMap.W;
                    int yOffset = chunkY * chunkMap.H;

                    if (chunk.Type == Chunk.TypeStart)
                    {
                        HeroPos.X = chunkMap.HeroPos.X + (float)xOffset;
                        HeroPos.Y = chunkMap.HeroPos.Y + (float)yOffset;
                    }

                    for (int layerIndex = 0; layerIndex < chunkMap.Layers.Count; ++layerIndex)
                    {
                        CopyTileLayer(chunkMap, layerIndex, 0, 0, 0, 0, xOffset, yOffset);

                        // copy tile layers for links
                        if (chunk.Links[Chunk.LinkNorth] != null)
                        {
                            Rectangle r = linkRects[Chunk.LinkNorth];
                            CopyTileLayer(chunkMapLinks, layerIndex, r.X, r.Y, r.X + r.Width, r.Y + r.Height, xOffset, yOffset);
                        }
                        if (chunk.Links[Chunk.LinkSouth] != null)
                        {
                            Rectangle r = linkRects[Chunk.LinkSouth];
                            CopyTileLayer(chunkMapLinks, layerIndex, r.X, r.Y, r.X + r.Width, r.Y + r.Height, xOffset, yOffset);
                        }
                        if (chunk.Links[Chunk.LinkWest] != null)
                        {
                            Rectangle r = linkRects[Chunk.LinkWest];
                            CopyTileLayer(chunkMapLinks, layerIndex, r.X, r.Y, r.X + r.Width, r.Y + r.Height, xOffset, yOffset);
                        }
                        if (chunk.Links[Chunk.LinkEast] != null)
                        {
                            Rectangle r = linkRects[Chunk.LinkEast];
                            CopyTileLayer(chunkMapLinks, layerIndex, r.X, r.Y, r.X + r.Width, r.Y + r.Height, xOffset, yOffset);
                        }
                    }

                    CopyMapObjects(chunkMap, chunk, 0, 0, 0, 0, xOffset, yOffset);

                    if (chunk.Links[Chunk.LinkNorth] != null)
                    {
                        Rectangle r = linkRects[Chunk.LinkNorth];
                        CopyMapObjects(chunkMapLinks, chunk, r.X, r.Y, r.X + r.Width, r.Y + r.Height, xOffset, yOffset);
                    }
                    if (chunk.Links[Chunk.LinkSouth] != null)
                    {
                        Rectangle r = linkRects[Chunk.LinkSouth];
                        CopyMapObjects(chunkMapLinks, chunk, r.X, r.Y, r.X + r.Width, r.Y + r.Height, xOffset, yOffset);
                    }
                    if (chunk.Links[Chunk.LinkWest] != null)
                    {
                        Rectangle r = linkRects[Chunk.LinkWest];
                        CopyMapObjects(chunkMapLinks, chunk, r.X, r.Y, r.X + r.Width, r.Y + r.Height, xOffset, yOffset);
                    }
                    if (chunk.Links[Chunk.LinkEast] != null)
                    {
                        Rectangle r = linkRects[Chunk.LinkEast];
                        CopyMapObjects(chunkMapLinks, chunk, r.X, r.Y, r.X + r.Width, r.Y + r.Height, xOffset, yOffset);
                    }
                }
            }

            // 清理：C++ 需 delete chunk map；C# 由 GC 自动回收 Map 对象
        }

        private void CopyTileLayer(Map src, int layerIndex, int srcX, int srcY, int srcW, int srcH, int xOffset, int yOffset)
        {
            if (layerIndex >= Layers.Count || layerIndex >= src.Layers.Count)
                return;

            if (srcW == 0)
                srcW = src.Layers[layerIndex].Count;
            if (srcH == 0)
                srcH = src.Layers[layerIndex].Count;

            for (int x = srcX; x < srcW; ++x)
            {
                if (x + xOffset >= W)
                    continue;

                for (int y = srcY; y < srcH; ++y)
                {
                    if (y + yOffset >= H)
                        continue;

                    Layers[layerIndex][x + xOffset][y + yOffset] = src.Layers[layerIndex][x][y];
                }
            }
        }

        private void CopyMapObjects(Map src, Chunk chunk, int srcX, int srcY, int srcW, int srcH, int xOffset, int yOffset)
        {
            if (srcW == 0)
                srcW = src.W;
            if (srcH == 0)
                srcH = src.H;

            // copy and apply x/y offset to events
            for (int eventIndex = 0; eventIndex < src.Events.Count; ++eventIndex)
            {
                Event evnt = new Event(src.Events[eventIndex]);

                if ((uint)evnt.Location.X < (uint)srcX || (uint)evnt.Location.Y < (uint)srcY || (uint)evnt.Location.X > (uint)(srcX + srcW) || (uint)evnt.Location.Y > (uint)(srcY + srcH))
                    continue;

                evnt.Location.X += xOffset;
                evnt.Location.Y += yOffset;
                evnt.Hotspot.X += xOffset;
                evnt.Hotspot.Y += yOffset;
                evnt.Center.X += (float)xOffset;
                evnt.Center.Y += (float)yOffset;
                evnt.ReachableFrom.X += xOffset;
                evnt.ReachableFrom.Y += yOffset;

                bool checkRequiredDoorLevel = false;
                bool eventMatchedDoorLevel = false;
                bool eventIsProcgen = false;

                for (int ecIndex = 0; ecIndex < evnt.Components.Count; ++ecIndex)
                {
                    EventComponent ec = evnt.Components[ecIndex];

                    if (ec.Type == EventComponent.ProcgenLink || ec.Type == EventComponent.ProcgenFilename)
                    {
                        eventIsProcgen = true;
                        break;
                    }
                    else if (ec.Type == EventComponent.PowerPath)
                    {
                        ec.Data[0].Int += xOffset;
                        ec.Data[1].Int += yOffset;
                        if (!ec.Data[4].Bool)
                        {
                            ec.Data[2].Int += xOffset;
                            ec.Data[3].Int += yOffset;
                        }
                    }
                    else if (ec.Type == EventComponent.Intramap)
                    {
                        ec.Data[0].Int += xOffset;
                        ec.Data[1].Int += yOffset;
                    }
                    else if (ec.Type == EventComponent.Mapmod)
                    {
                        ec.Data[0].Int += xOffset;
                        ec.Data[1].Int += yOffset;
                    }
                    else if (ec.Type == EventComponent.MapmodToggle)
                    {
                        ec.Data[0].Int += xOffset;
                        ec.Data[1].Int += yOffset;
                    }
                    else if (ec.Type == EventComponent.Soundfx)
                    {
                        // make sure we handle sounds that aren't positional
                        if (!(ec.Data[0].Int == -1 && ec.Data[1].Int == -1) && !(ec.Data[0].Int == 0 && ec.Data[1].Int == 0))
                        {
                            ec.Data[0].Int += xOffset;
                            ec.Data[1].Int += yOffset;
                        }
                    }
                    else if (ec.Type == EventComponent.Spawn)
                    {
                        ec.Data[0].Int += xOffset;
                        ec.Data[1].Int += yOffset;
                    }
                    else if (ec.Type == EventComponent.ProcgenDoorLevel)
                    {
                        checkRequiredDoorLevel = true;
                        if (chunk.DoorLevel == ec.Data[0].Int)
                            eventMatchedDoorLevel = true;
                    }
                    else if (ec.Type == EventComponent.RequiresTile)
                    {
                        ec.Data[0].Int += xOffset;
                        ec.Data[1].Int += yOffset;
                    }
                    else if (ec.Type == EventComponent.RequiresNotTile)
                    {
                        ec.Data[0].Int += xOffset;
                        ec.Data[1].Int += yOffset;
                    }
                }

                if (!eventIsProcgen && (!checkRequiredDoorLevel || eventMatchedDoorLevel))
                    Events.Add(evnt);
            }

            for (int egroupIndex = 0; egroupIndex < src.EnemyGroups.Count; ++egroupIndex)
            {
                MapGroup enemyGroup = new MapGroup(src.EnemyGroups[egroupIndex]);

                if ((uint)enemyGroup.Pos.X < (uint)srcX || (uint)enemyGroup.Pos.Y < (uint)srcY || (uint)enemyGroup.Pos.X > (uint)(srcX + srcW) || (uint)enemyGroup.Pos.Y > (uint)(srcY + srcH))
                    continue;

                enemyGroup.Pos.X += xOffset;
                enemyGroup.Pos.Y += yOffset;

                for (int waypointIndex = 0; waypointIndex < enemyGroup.Waypoints.Count; ++waypointIndex)
                {
                    Vector2 waypoint = enemyGroup.Waypoints[waypointIndex];
                    waypoint.X += (float)xOffset;
                    waypoint.Y += (float)yOffset;
                    enemyGroup.Waypoints[waypointIndex] = waypoint;
                }

                EnemyGroups.Add(enemyGroup);
            }

            for (int npcIndex = 0; npcIndex < src.MapNpcs.Count; ++npcIndex)
            {
                MapNpc npc = new MapNpc(src.MapNpcs[npcIndex]);

                if ((uint)npc.Pos.X < (uint)srcX || (uint)npc.Pos.Y < (uint)srcY || (uint)npc.Pos.X > (uint)(srcX + srcW) || (uint)npc.Pos.Y > (uint)(srcY + srcH))
                    continue;

                npc.Pos.X += (float)xOffset;
                npc.Pos.Y += (float)yOffset;

                for (int waypointIndex = 0; waypointIndex < npc.Waypoints.Count; ++waypointIndex)
                {
                    Vector2 waypoint = npc.Waypoints[waypointIndex];
                    waypoint.X += (float)xOffset;
                    waypoint.Y += (float)yOffset;
                    npc.Waypoints[waypointIndex] = waypoint;
                }

                MapNpcs.Add(npc);
            }
        }

        private string GetProcgenFilename()
        {
            var settings = SharedResources.Settings!;
            var eset = SharedResources.Eset!;
            var saveLoad = SharedResources.SaveLoad!;

            return settings.PathUser + "saves/" + eset.Misc.SavePrefix + "/" + saveLoad.GameSlot + "/maps/" + Utils.HashString(_filename) + ".txt";
        }

        public string GetFOWFilename()
        {
            var settings = SharedResources.Settings!;
            var eset = SharedResources.Eset!;
            var saveLoad = SharedResources.SaveLoad!;

            return settings.PathUser + "saves/" + eset.Misc.SavePrefix + "/" + saveLoad.GameSlot + "/fow/" + Utils.HashString(_filename) + ".txt";
        }
    }
}
