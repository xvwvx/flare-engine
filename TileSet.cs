// <自动生成> 对应 C++ 源文件：TileSet.h + TileSet.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// TileDef
    ///
    /// 通过图块在 tileset 精灵图中的源位置 <see cref="Offset"/> 以及渲染时应用的
    /// 偏移量来描述一个图块。偏移量从左上角量到地面逻辑中点。
    ///
    /// C++ 原始实现中 <c>Sprite *tile</c> 由 <see cref="TileSet"/> 在 load/reset
    /// 时手动 delete；C# 版本中 <see cref="Tile"/> 保持为可空引用字段，由
    /// <see cref="TileSet"/> 负责通过 <see cref="IDisposable"/> 释放。
    /// </summary>
    public class TileDef
    {
        public Int2 Offset;
        public Sprite? Tile;

        public TileDef()
        {
            Tile = null;
        }
    }

    /// <summary>
    /// TileSet
    ///
    /// 图块集存储与文件加载（对应 C++ 的 <c>class TileSet</c>）。
    ///
    /// 资源管理：<see cref="_sprites"/> 与 <see cref="Tiles"/> 中每项的
    /// <see cref="TileDef.Tile"/> 均为本类独占持有的裸指针成员，在
    /// <see cref="Reset"/> 与析构路径中手动释放；C# 版本据此实现
    /// <see cref="IDisposable"/>，释放顺序与原始 C++ 析构函数一致。
    /// </summary>
    public class TileSet : IDisposable
    {
        private class TileAnim
        {
            // Number of frames in this animation. if 0 no animation.
            // 1 makes no sense as it would produce astatic animation.
            public ushort Frames;
            public ushort CurrentFrame;
            public ushort Duration;
            public List<Int2> Pos = new List<Int2>();
            public List<ushort> FrameDuration = new List<ushort>();

            public TileAnim()
            {
                Frames = 0;
                CurrentFrame = 0;
                Duration = 0;
            }
        }

        private string _currentFilename = "";
        private List<Sprite?> _sprites = new List<Sprite?>();
        private List<TileAnim> _anim = new List<TileAnim>();

        public List<TileDef> Tiles = new List<TileDef>();
        public int MaxSizeX;
        public int MaxSizeY;

        public TileSet()
        {
            Reset();
        }

        public void Dispose()
        {
            for (int i = 0; i < _sprites.Count; ++i)
            {
                _sprites[i]?.Dispose();
            }

            for (int i = 0; i < Tiles.Count; ++i)
            {
                Tiles[i].Tile?.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        private void Reset()
        {
            for (int i = 0; i < _sprites.Count; ++i)
            {
                _sprites[i]?.Dispose();
            }
            _sprites.Clear();

            for (int i = 0; i < Tiles.Count; ++i)
            {
                Tiles[i].Tile?.Dispose();
            }

            Tiles.Clear();
            _anim.Clear();

            MaxSizeX = 0;
            MaxSizeY = 0;
        }

        private void LoadGraphics(string filename, ref Sprite? sprite)
        {
            if (sprite != null)
            {
                sprite.Dispose();
                sprite = null;
            }

            if (filename.Length == 0)
                return;

            Image? graphics = SharedResources.RenderDevice!.LoadImage(filename, RenderDevice.ErrorNormal);
            if (graphics != null)
            {
                sprite = graphics.CreateSprite();
                graphics.Unref();
            }
        }

        public void Load(string filename)
        {
            if (_currentFilename == filename) return;

            Reset();

            List<string> imageFilenames = new List<string>();
            List<int> tileImages = new List<int>();
            List<Rectangle> tileClips = new List<Rectangle>();
            List<Int2> tileOffsets = new List<Int2>();

            using FileParser infile = new FileParser();

            // @CLASS TileSet|Description of tilesets in tilesets/
            if (infile.Open(filename, FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if ((infile.NewSection && infile.Section == "tileset") || (_sprites.Count == 0 && infile.Section.Length == 0))
                    {
                        imageFilenames.Add("");
                        _sprites.Add(null);
                    }

                    if (infile.Key == "img")
                    {
                        // @ATTR tileset.img|filename|Filename of a tile sheet image.
                        imageFilenames[^1] = infile.Val;
                    }
                    else if (infile.Key == "tile")
                    {
                        // @ATTR tileset.tile|int, int, int, int, int, int, int : Index, X, Y, Width, Height, X offset, Y offset|A single tile definition.

                        int index = Parse.PopFirstInt(ref infile.Val);

                        if (index >= Tiles.Count)
                        {
                            int newSize = index + 1;
                            while (Tiles.Count < newSize)
                            {
                                Tiles.Add(new TileDef());
                                tileImages.Add(0);
                                tileClips.Add(new Rectangle());
                                tileOffsets.Add(new Int2());
                            }
                        }

                        Rectangle clip = new Rectangle();
                        clip.X = Parse.PopFirstInt(ref infile.Val);
                        clip.Y = Parse.PopFirstInt(ref infile.Val);
                        clip.Width = Parse.PopFirstInt(ref infile.Val);
                        clip.Height = Parse.PopFirstInt(ref infile.Val);

                        Int2 offset = new Int2();
                        offset.X = Parse.PopFirstInt(ref infile.Val);
                        offset.Y = Parse.PopFirstInt(ref infile.Val);

                        tileImages[index] = imageFilenames.Count - 1;
                        tileClips[index] = clip;
                        tileOffsets[index] = offset;
                    }
                    else if (infile.Key == "animation")
                    {
                        // @ATTR tileset.animation|list(int, int, int, duration) : Tile index, X, Y, duration|An animation for a tile. Durations are in 'ms' or 's'.

                        ushort frame = 0;
                        int index = Parse.PopFirstInt(ref infile.Val);

                        if (index >= _anim.Count)
                        {
                            int newSize = index + 1;
                            while (_anim.Count < newSize)
                                _anim.Add(new TileAnim());
                        }

                        string repeatVal = Parse.PopFirstString(ref infile.Val);
                        while (repeatVal != "")
                        {
                            _anim[index].Frames++;
                            _anim[index].Pos.Add(new Int2());
                            _anim[index].FrameDuration.Add(0);
                            Int2 pos = _anim[index].Pos[frame];
                            pos.X = Parse.ToInt(repeatVal);
                            pos.Y = Parse.PopFirstInt(ref infile.Val);
                            _anim[index].Pos[frame] = pos;
                            _anim[index].FrameDuration[frame] = (ushort)Parse.ToDuration(Parse.PopFirstString(ref infile.Val));

                            frame++;
                            repeatVal = Parse.PopFirstString(ref infile.Val);
                        }
                    }
                    else
                    {
                        infile.Error("TileSet: '%s' is not a valid key.", infile.Key);
                    }
                }
                infile.Close();
            }

            // load tileset images
            for (int i = 0; i < imageFilenames.Count; ++i)
            {
                Sprite? sprite = _sprites[i];
                LoadGraphics(imageFilenames[i], ref sprite);
                _sprites[i] = sprite;
            }

            // set up individual tile sprites
            for (int i = 0; i < Tiles.Count; ++i)
            {
                if (_sprites[tileImages[i]] == null)
                    continue;

                Tiles[i].Tile = _sprites[tileImages[i]]!.GetGraphics()!.CreateSprite();
                Tiles[i].Tile.SetClipFromRect(tileClips[i]);
                Tiles[i].Offset = tileOffsets[i];

                MaxSizeX = Math.Max(MaxSizeX, (Tiles[i].Tile!.GetClip().Width / SharedResources.Eset!.Tileset.TileW) + 1);
                MaxSizeY = Math.Max(MaxSizeY, (Tiles[i].Tile!.GetClip().Height / SharedResources.Eset!.Tileset.TileH) + 1);
            }

            _currentFilename = filename;
        }

        public void Logic()
        {
            for (int i = 0; i < _anim.Count; ++i)
            {
                TileAnim an = _anim[i];
                if (an.Frames == 0)
                    continue;
                if (Tiles[i].Tile != null && an.Duration >= an.FrameDuration[an.CurrentFrame])
                {
                    Rectangle clip = Tiles[i].Tile!.GetClip();
                    clip.X = an.Pos[an.CurrentFrame].X;
                    clip.Y = an.Pos[an.CurrentFrame].Y;
                    Tiles[i].Tile.SetClipFromRect(clip);
                    an.Duration = 0;
                    an.CurrentFrame = (ushort)((an.CurrentFrame + 1) % an.Frames);
                }
                an.Duration++;
            }
        }
    }
}
