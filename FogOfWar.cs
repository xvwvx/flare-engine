// <自动生成> 对应 C++ 源文件：FogOfWar.h + FogOfWar.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// FogOfWar
    ///
    /// 战争迷雾的逻辑与渲染例程（对应 C++ <c>class FogOfWar</c>）。
    ///
    /// 资源管理：C++ 原始实现通过 <c>unsigned short *def_mask</c> 裸指针持有掩码数组，
    /// 在 <c>load()</c> 多次重建以及 <c>~FogOfWar()</c> 中 <c>delete[]</c> 释放；
    /// C# 版本使用 <see cref="_defMask"/> 可空数组表达同样语义，并在
    /// <see cref="Dispose"/> 中清空引用；成员 <see cref="TsetDark"/> / <see cref="TsetFog"/>
    /// 为 <see cref="TileSet"/> 值成员，其析构在 C++ 中隐式调用，此处于
    /// <see cref="Dispose"/> 末尾显式调用 <see cref="TileSet.Dispose"/> 以保持释放顺序一致。
    ///
    /// 全局指针映射：<c>mapr</c> → <see cref="SharedGameResources.Mapr"/>，
    /// <c>pc</c> → <see cref="SharedGameResources.Pc"/>，
    /// <c>menu</c> → <see cref="SharedGameResources.Menu"/>。
    /// </summary>
    public class FogOfWar : IDisposable
    {
        public const int TypeNone = 0;
        public const int TypeMinimap = 1;
        public const int TypeTint = 2;
        public const int TypeOverlay = 3;

        public static ushort TileHidden = 0;

        public ushort DarkLayerId;
        public ushort FogLayerId;
        public string TilesetDark = "";
        public string TilesetFog = "";
        public string MaskDefinition = "engine/fow_mask.txt";
        public TileSet TsetDark = new TileSet();
        public TileSet TsetFog = new TileSet();
        public int MaskRadius;

        private int _bitsPerTile;
        private Dictionary<string, int> _defBits = new Dictionary<string, int>();
        private Dictionary<string, int> _defTiles = new Dictionary<string, int>();
        private ushort[]? _defMask;

        private Rectangle _bounds;
        private Color _colorSight;
        private Color _colorFog;
        private Color _colorDark;

        private bool _updateMinimap;
        private bool _loaded;

        private Vector2 _prevHeroPos;

        public FogOfWar()
        {
            DarkLayerId = 0;
            FogLayerId = 0;
            MaskDefinition = "engine/fow_mask.txt";
            MaskRadius = 0;
            _bitsPerTile = 0;
            _defMask = null;
            _bounds = new Rectangle(0, 0, 0, 0);
            _colorSight = new Color(255, 255, 255, 255);
            _colorFog = new Color(128, 128, 128, 255);
            _colorDark = new Color(0, 0, 0, 255);
            _updateMinimap = true;
            _loaded = false;
            _prevHeroPos = new Vector2(-1, -1);
        }

        public int Load()
        {
            var mapr = SharedGameResources.Mapr!;
            using FileParser infile = new FileParser();
            // @CLASS FogOfWar|Description of engine/fow_mask.txt
            if (!infile.Open(MaskDefinition, FileParser.ModFile, FileParser.ErrorNormal))
                return 0;

            bool invalidConfig = false;

            if (!_loaded)
            {
                Utils.LogInfo("FogOfWar: Loading mask '%s'", MaskDefinition);

                while (infile.Next())
                {
                    if (infile.Section == "header")
                    {
                        LoadHeader(infile);
                    }
                    else if (infile.Section == "bits")
                    {
                        if (infile.NewSection)
                        {
                            _defBits.Clear();
                            _defTiles.Clear();
                            if (_defMask != null)
                            {
                                _defMask = null;
                            }
                        }

                        LoadDefBit(infile);
                    }
                    else if (infile.Section == "tiles")
                    {
                        if (infile.NewSection)
                        {
                            _defTiles.Clear();
                            if (_defMask != null)
                            {
                                _defMask = null;
                            }
                            if (_defBits.Count == 0)
                            {
                                infile.Error("FogOfWar: Unable to load tile section because no bits are defined.");
                            }
                        }

                        LoadDefTile(infile);
                    }
                    else if (infile.Section == "mask")
                    {
                        if (infile.NewSection)
                        {
                            if (_defMask != null)
                            {
                                _defMask = null;
                            }
                            if (_defTiles.Count == 0)
                            {
                                infile.Error("FogOfWar: Unable to load mask section because no tiles are defined.");
                            }
                        }

                        LoadDefMask(infile);
                    }
                }

                if (_bitsPerTile > 0 && _defBits.Count == 0)
                {
                    Utils.LogError("FogOfWar: No bits defined, but bits_per_tile > 0");
                    invalidConfig = true;
                }
                else if (_bitsPerTile + 1 != _defBits.Count)
                {
                    Utils.LogError("FogOfWar: Found %u bits, but bits_per_tile is %d. Setting bits_per_tile to %u.", _defBits.Count - 1, _bitsPerTile, _defBits.Count - 1);
                    _bitsPerTile = _defBits.Count - 1;
                }

                if (_defMask == null)
                {
                    Utils.LogError("FogOfWar: No mask defined.");
                    invalidConfig = true;
                }

                if (invalidConfig)
                {
                    _bitsPerTile = 0;
                    if (_defMask != null)
                    {
                        _defMask = null;
                    }
                }
                else
                {
                    for (ushort i = 0; i < _bitsPerTile; i++)
                    {
                        TileHidden = (ushort)(TileHidden | (ushort)(1 << i));
                    }
                }

                _defTiles.Clear();
                _defBits.Clear();
            }

            if (_defMask == null)
            {
                mapr.Fogofwar = FogOfWar.TypeNone;
                invalidConfig = true;
            }

            if (mapr.Fogofwar == FogOfWar.TypeOverlay)
            {
                if (TilesetDark.Length == 0)
                {
                    if (!_loaded)
                        Utils.LogError("FogOfWar: tileset_dark is not set");

                    mapr.Fogofwar = FogOfWar.TypeTint;
                }
                if (TilesetFog.Length == 0)
                {
                    if (!_loaded)
                        Utils.LogError("FogOfWar: tileset_fog is not set");

                    mapr.Fogofwar = FogOfWar.TypeTint;
                }
                if (!invalidConfig && !_loaded && mapr.Fogofwar == FogOfWar.TypeOverlay)
                {
                    TsetDark.Load(TilesetDark);
                    TsetFog.Load(TilesetFog);
                }
            }

            _loaded = true;

            return 0;
        }

        public void Logic()
        {
            var pc = SharedGameResources.Pc!;
            var menu = SharedGameResources.Menu!;
            var mapr = SharedGameResources.Mapr!;

            if (_prevHeroPos.X == pc.Stats.Pos.X && _prevHeroPos.Y == pc.Stats.Pos.Y)
                return;

            _prevHeroPos = pc.Stats.Pos;

            UpdateTiles();
            if (_updateMinimap)
            {
                CalcMiniBoundaries();
                menu.Mini!.Update(mapr.Collider, ref _bounds);
                _updateMinimap = false;
            }
        }

        public void HandleIntramapTeleport()
        {
            var mapr = SharedGameResources.Mapr!;
            CalcBoundaries();

            for (int x = _bounds.X; x <= _bounds.Width; x++)
            {
                for (int y = _bounds.Y; y <= _bounds.Height; y++)
                {
                    if (x >= 0 && y >= 0 && x < mapr.W && y < mapr.H)
                    {
                        mapr.Layers[FogLayerId][x][y] = TileHidden;
                    }
                }
            }
        }

        public Color GetTileColorMod(short x, short y)
        {
            var mapr = SharedGameResources.Mapr!;
            if (mapr.Layers[DarkLayerId][x][y] == 0 && mapr.Layers[FogLayerId][x][y] > 0)
                return _colorFog;
            else if (mapr.Layers[DarkLayerId][x][y] > 0)
                return _colorDark;
            else
                return _colorSight;
        }

        /// <summary>对应 C++ 析构函数 <c>~FogOfWar()</c>。</summary>
        public void Dispose()
        {
            Utils.LogInfo("Cleaning up: FogOfWar");

            _defMask = null;

            TsetFog.Dispose();
            TsetDark.Dispose();

            GC.SuppressFinalize(this);
        }

        private void CalcBoundaries()
        {
            var pc = SharedGameResources.Pc!;
            _bounds.X = (short)pc.Stats.Pos.X - MaskRadius;
            _bounds.Y = (short)pc.Stats.Pos.Y - MaskRadius;
            _bounds.Width = (short)pc.Stats.Pos.X + MaskRadius;
            _bounds.Height = (short)pc.Stats.Pos.Y + MaskRadius;
        }

        private void CalcMiniBoundaries()
        {
            var pc = SharedGameResources.Pc!;
            var mapr = SharedGameResources.Mapr!;
            _bounds.X = (short)pc.Stats.Pos.X - MaskRadius;
            _bounds.Y = (short)pc.Stats.Pos.Y - MaskRadius;
            _bounds.Width = (short)pc.Stats.Pos.X + MaskRadius;
            _bounds.Height = (short)pc.Stats.Pos.Y + MaskRadius;

            if (_bounds.X < 0) _bounds.X = 0;
            if (_bounds.Y < 0) _bounds.Y = 0;
            if (_bounds.Width > mapr.W) _bounds.Width = mapr.W;
            if (_bounds.Height > mapr.H) _bounds.Height = mapr.H;
        }

        private void UpdateTiles()
        {
            var mapr = SharedGameResources.Mapr!;
            if (_defMask == null)
                return;

            CalcBoundaries();
            int mask = 0;

            for (int x = _bounds.X; x <= _bounds.Width; x++)
            {
                for (int y = _bounds.Y; y <= _bounds.Height; y++)
                {
                    if (x >= 0 && y >= 0 && x < mapr.W && y < mapr.H)
                    {
                        ushort prevDarkTile = mapr.Layers[DarkLayerId][x][y];

                        mapr.Layers[DarkLayerId][x][y] = (ushort)(mapr.Layers[DarkLayerId][x][y] & _defMask[mask]);
                        mapr.Layers[FogLayerId][x][y] = _defMask[mask];

                        if (prevDarkTile != mapr.Layers[DarkLayerId][x][y])
                        {
                            _updateMinimap = true;
                        }
                    }
                    mask++;
                }
            }
        }

        private void LoadHeader(FileParser infile)
        {
            if (infile.Key == "radius")
            {
                // @ATTR header.radius|int|Fog of war mask radius, also how far the player can see.
                MaskRadius = Parse.ToInt(infile.Val);
            }
            else if (infile.Key == "bits_per_tile")
            {
                // @ATTR header.bits_per_tile|int|How may bits(subdivisions) a tile is made of. In powers of two. Example: if it is set to 4 then the tile will be subdivided in 4, let's say North, South, East, West.
                _bitsPerTile = Math.Max(Parse.ToInt(infile.Val), 1);
            }
            else if (infile.Key == "color_dark")
            {
                // @ATTR header.color_dark|color|Tint color for dark tiles. Used by fog of war type 2-tint.
                _colorDark = Parse.ToRGB(infile.Val);
            }
            else if (infile.Key == "color_fog")
            {
                // @ATTR header.color_fog|color|Tint color for fog tiles. Used by fog of war type 2-tint.
                _colorFog = Parse.ToRGB(infile.Val);
            }
            else if (infile.Key == "tileset_dark")
            {
                // @ATTR header.tileset_dark|filename|Filename of a tileset definition to use for unvisited areas. Used by fog of war type 3-overlay.
                TilesetDark = infile.Val;
            }
            else if (infile.Key == "tileset_fog")
            {
                // @ATTR header.tileset_fog|filename|Filename of a tileset definition to use for foggy areas. Used by fog of war type 3-overlay.
                TilesetFog = infile.Val;
            }
            else
            {
                infile.Error("FogOfWar: '%s' is not a valid key.", infile.Key);
            }
        }

        private void LoadDefBit(FileParser infile)
        {
            // @ATTR bits.bit|string, int: Name, Value|A fog of war bit definition can have any name. Better to keep it simple and short. There must be a bit definition that has the value 0. Example: If we have 4 bits per tile then we define: bit=BIT_0,0, bit=BIT_N,1, bit=BIT_W,2, bit=BIT_S,3, bit=BIT_E,4.
            if (infile.Key == "bit")
            {
                int bit = 0;
                string bitName = Parse.PopFirstString(ref infile.Val);
                int val = Parse.PopFirstInt(ref infile.Val);

                if (val > 0)
                {
                    bit = 1 << (val - 1);
                }

                if (_defBits.Count < _bitsPerTile + 1)
                {
                    // 对应 C++ std::map::insert：键已存在时不覆盖
                    if (!_defBits.ContainsKey(bitName))
                        _defBits.Add(bitName, bit);
                }
                else
                {
                    infile.Error("FogOfWar: bits_per_tile is '%u' but found more", _bitsPerTile);
                }
            }
            else
            {
                infile.Error("FogOfWar: '%s' is not a valid key.", infile.Key);
            }
        }

        private void LoadDefTile(FileParser infile)
        {
            // @ATTR tiles.tile|string, repeatable(predefined_string): Name, Bit definitions|A fog of war tile definition can have any name. Better to keep it simple and short. There must be a tile definition that contains no bits and a tile definition that contains all bits. Example: A tile containing North and West bits will be tile=NW,BIT_N,BIT_W.
            if (infile.Key == "tile")
            {
                if (_defBits.Count == 0)
                    return;

                string tileName = Parse.PopFirstString(ref infile.Val);
                string val = Parse.StripCarriageReturn(infile.Val);

                string bit;
                int tileBits = 0;

                int prevComma = 0;

                while (prevComma < val.Length)
                {
                    int comma = val.IndexOf(',', prevComma + 1);
                    if (prevComma == 0)
                    {
                        if (comma == -1)
                            bit = val.Substring(0);
                        else
                            bit = val.Substring(0, comma - prevComma);
                    }
                    else
                    {
                        if (comma == -1)
                            bit = val.Substring(prevComma + 1);
                        else
                            bit = val.Substring(prevComma + 1, comma - prevComma - 1);
                    }

                    bit = Parse.Trim(bit);

                    if (_defBits.TryGetValue(bit, out int bitValue))
                        tileBits = tileBits | bitValue;
                    else
                    {
                        infile.Error("FogOfWar: Bit definition '%s' not found.", bit);
                    }

                    prevComma = comma == -1 ? val.Length : comma;
                }

                if (!_defTiles.ContainsKey(tileName))
                    _defTiles.Add(tileName, tileBits);
            }
        }

        private void LoadDefMask(FileParser infile)
        {
            // @ATTR mask.data|raw|The mask definition is a matrix (2\*radius+1 by 2\*radius+1) that contains fog of war tile definitions. All the margins of the matrix must be the tile definition that contains all bits.
            if (infile.Key == "data")
            {
                if (_defTiles.Count == 0)
                    return;

                _defMask = new ushort[(MaskRadius * 2 + 1) * (MaskRadius * 2 + 1)];
                string val;
                string tileDef;
                int k = 0;

                for (int j = 0; j < MaskRadius * 2 + 1; j++)
                {
                    val = infile.GetRawLine();
                    infile.IncrementLineNum();
                    if (val.Length > 0 && val[val.Length - 1] != ',')
                    {
                        val += ',';
                    }

                    // verify the width of this row
                    int commaCount = 0;
                    for (uint i = 0; i < val.Length; ++i)
                    {
                        if (val[(int)i] == ',') commaCount++;
                    }
                    if (commaCount != MaskRadius * 2 + 1)
                    {
                        // mask data is broken! Clear def_mask and abort.
                        infile.Error("FogOfWar: Mask data row %d/%d has a width not equal to %d.", j + 1, MaskRadius * 2 + 1, MaskRadius * 2 + 1);
                        _defMask = null;
                        break;
                    }

                    for (int i = 0; i < MaskRadius * 2 + 1; i++)
                    {
                        tileDef = Parse.PopFirstString(ref val, ',');
                        if (_defTiles.TryGetValue(tileDef, out int tileBits))
                        {
                            _defMask[k++] = (ushort)tileBits;
                        }
                        else
                            infile.Error("FogOfWar: Tile definition '%s' not found.", tileDef);
                    }
                }
            }
        }
    }
}
