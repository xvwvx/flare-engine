// 对应 C++ 源文件：AnimationSet.h + AnimationSet.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using System.Diagnostics;
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// AnimationSet
    ///
    /// 动画集包含一个实体的全部动画，它们共用同一份精灵图（spritesheet）。
    /// 本类负责在析构时释放 spritesheet 相关资源。
    ///
    /// C++ 原始实现：<c>Animation*</c> 元素由本类 <c>new</c> 创建、由本类 <c>delete</c> 释放。
    /// <see cref="GetAnimation"/> 返回调用方独占持有的副本（对应 <c>new Animation(*...)</c>）。
    /// C# 版本实现 <see cref="IDisposable"/>，释放顺序与原始析构函数一致。
    /// </summary>
    public class AnimationSet : IDisposable
    {
        // i.e. animations/goblin_runner.txt, matches the animations filename.
        private readonly string _name;
        private string _imagefile;
        // has always a non-null animation, in case of successfull load it contains the first animation in the animation file.
        private Animation? _defaultAnimation;
        private bool _loaded;
        private AnimationSet? _parent;

        public List<Animation?> Animations { get; } = new List<Animation?>();

        public AnimationMedia? Sprite;

        public AnimationSet(string animationname)
        {
            _name = animationname;
            _loaded = false;
            _parent = null;
            _imagefile = "";
            Sprite = new AnimationMedia();
            _defaultAnimation = new Animation("default", "play_once", Sprite, Renderable.BlendNormal, 255, new Color(255, 255, 255, 255));
            _defaultAnimation.SetupUncompressed(new Int2(), new Int2(), 0, 1, 0, "");
        }

        public string Name => _name;

        public AnimationSet? Parent
        {
            set => _parent = value;
        }

        /// <summary>
        /// 调用方负责释放返回的 <see cref="Animation"/>（对应 C++ 注释 "callee is responsible to free"）。
        /// 按名称查找动画；未找到时返回默认动画的副本。
        /// </summary>
        public Animation? GetAnimation(string name)
        {
            if (!_loaded)
                Load();

            if (!string.IsNullOrEmpty(name))
            {
                for (int i = 0; i < Animations.Count; i++)
                {
                    if (Animations[i]!.Name == name)
                        return new Animation(Animations[i]!);
                }
            }

            return new Animation(_defaultAnimation!);
        }

        private uint GetAnimationFrames(string name)
        {
            if (!_loaded)
                Load();
            for (int i = 0; i < Animations.Count; i++)
                if (Animations[i]!.Name == name)
                    return Animations[i]!.FrameCount;
            return 0;
        }

        private void Load()
        {
            Debug.Assert(!_loaded);
            _loaded = true;

            FileParser parser = new FileParser();
            // @CLASS AnimationSet|Description of animations in animations/
            if (string.IsNullOrEmpty(_name) || !parser.Open(_name, FileParser.ModFile, FileParser.ErrorNormal))
                return;

            string animName = "";
            ushort position = 0;
            ushort frames = 0;
            ushort duration = 0;
            byte blendMode = Renderable.BlendNormal;
            byte alphaMod = 255;
            Color colorMod = new Color(255, 255, 255, 255);
            Int2 renderSize = new Int2();
            Int2 renderOffset = new Int2();
            string type = "";
            string startingAnimation = "";
            bool firstSection = true;
            bool compressedLoading = false; // is reset every section to false, set by frame keyword
            Animation? newanim = null;
            List<short> activeFrames = new List<short>();
            string activeSubFrame = "";
            string imageId = "";

            ushort parentAnimFrames = 0;

            // Parse the file and on each new section create an animation object from the data parsed previously
            while (parser.Next())
            {
                // create the animation if finished parsing a section
                if (parser.NewSection)
                {
                    if (!firstSection && !compressedLoading)
                    {
                        Animation? a = new Animation(animName, type, Sprite, blendMode, alphaMod, colorMod);
                        a.SetupUncompressed(renderSize, renderOffset, position, frames, duration, imageId);
                        if (activeFrames.Count != 0)
                        {
                            a.SetActiveFrames(activeFrames);
                            a.SetActiveSubFrame(activeSubFrame);
                        }
                        activeFrames.Clear();
                        Animations.Add(a);
                    }
                    firstSection = false;
                    compressedLoading = false;

                    if (_parent is not null)
                    {
                        parentAnimFrames = (ushort)_parent.GetAnimationFrames(parser.Section);
                    }
                }
                if (string.IsNullOrEmpty(parser.Section))
                {
                    if (parser.Key == "image")
                    {
                        // @ATTR image|filename, string : Filename, ID|Filename of sprite-sheet image along with an identifier string. The identifier string may be omitted if there is only a single image.
                        string imgFilename = Parse.PopFirstString(ref parser.Val);
                        string imgId = Parse.PopFirstString(ref parser.Val);
                        Sprite!.LoadImage(imgFilename, imgId);
                    }
                    else if (parser.Key == "render_size")
                    {
                        // @ATTR render_size|int, int : Width, Height|Width and height of animation.
                        renderSize.X = Parse.PopFirstInt(ref parser.Val);
                        renderSize.Y = Parse.PopFirstInt(ref parser.Val);
                    }
                    else if (parser.Key == "render_offset")
                    {
                        // @ATTR render_offset|int, int : X offset, Y offset|Render x/y offset.
                        renderOffset.X = Parse.PopFirstInt(ref parser.Val);
                        renderOffset.Y = Parse.PopFirstInt(ref parser.Val);
                    }
                    else if (parser.Key == "blend_mode")
                    {
                        // @ATTR blend_mode|["normal", "add"]|The type of blending used when rendering this animation.
                        string bmodeStr = Parse.PopFirstString(ref parser.Val);
                        if (bmodeStr == "normal")
                            blendMode = Renderable.BlendNormal;
                        else if (bmodeStr == "add")
                            blendMode = Renderable.BlendAdd;
                        else
                        {
                            parser.Error("AnimationSet: '%s' is not a valid blend mode.", parser.Key);
                            blendMode = Renderable.BlendNormal;
                        }
                    }
                    else if (parser.Key == "alpha_mod")
                    {
                        // @ATTR alpha_mod|int|Changes the default alpha of this animation. 255 is fully opaque.
                        alphaMod = (byte)Parse.PopFirstInt(ref parser.Val);
                    }
                    else if (parser.Key == "color_mod")
                    {
                        // @ATTR color_mod|color|Changes the default color mod of this animation. "255,255,255" is no color mod.
                        colorMod = Parse.ToRGB(parser.Val);
                    }
                    else
                    {
                        parser.Error("AnimationSet: '%s' is not a valid key.", parser.Key);
                    }
                }
                else
                {
                    if (parser.Key == "position")
                    {
                        // @ATTR animation.position|int|Number of frames to the right to use as the first frame. Unpacked animations only.
                        position = (ushort)Parse.ToInt(parser.Val);
                    }
                    else if (parser.Key == "frames")
                    {
                        // @ATTR animation.frames|int|The total number of frames
                        frames = (ushort)Parse.ToInt(parser.Val);
                        if (_parent != null && frames != parentAnimFrames)
                        {
                            parser.Error("AnimationSet: Frame count {0} != {1} for matching animation in {2}", frames, parentAnimFrames, _parent.Name);
                            frames = parentAnimFrames;
                        }
                    }
                    else if (parser.Key == "duration")
                    {
                        // @ATTR animation.duration|duration|The duration of the entire animation in 'ms' or 's'.
                        duration = (ushort)Parse.ToDuration(parser.Val);
                    }
                    else if (parser.Key == "type")
                        // @ATTR animation.type|["play_once", "back_forth", "looped"]|How to loop (or not loop) this animation.
                        type = parser.Val;
                    else if (parser.Key == "active_frame")
                    {
                        // @ATTR animation.active_frame|[list(int), "all"]|A list of frames marked as "active". Also, "all" can be used to mark all frames as active.
                        activeFrames.Clear();
                        string nv = Parse.PopFirstString(ref parser.Val);
                        if (nv == "all")
                        {
                            activeFrames.Add(-1);
                        }
                        else
                        {
                            while (nv != "")
                            {
                                activeFrames.Add((short)Parse.ToInt(nv));
                                nv = Parse.PopFirstString(ref parser.Val);
                            }
                            activeFrames.Sort();
                            int uniqueEnd = 0;
                            for (int uniqueIndex = 1; uniqueIndex < activeFrames.Count; uniqueIndex++)
                            {
                                if (activeFrames[uniqueIndex] != activeFrames[uniqueEnd])
                                {
                                    uniqueEnd++;
                                    activeFrames[uniqueEnd] = activeFrames[uniqueIndex];
                                }
                            }
                            if (uniqueEnd + 1 < activeFrames.Count)
                                activeFrames.RemoveRange(uniqueEnd + 1, activeFrames.Count - uniqueEnd - 1);
                        }
                    }
                    else if (parser.Key == "active_sub_frame")
                    {
                        // @ATTR animation.active_sub_frame|["end", "start", "all"]|Each frame of animation gets rendered for multiple "sub-frames" based on playback speed and the max_fps setting. This property controls which of these sub-frames will trigger active frames. Defaults to "end".
                        if (parser.Val == "end" || parser.Val == "start" || parser.Val == "all")
                            activeSubFrame = parser.Val;
                        else
                            parser.Error("AnimationSet: '%s' is not a valid parameter for active_sub_frame.", parser.Val);
                    }
                    else if (parser.Key == "image")
                    {
                        // @ATTR animation.image|predefined_string|Uncompressed animations only. Sets the image to be used by its ID. For compressed animations, use the last parameter of the 'frame' property instead.
                        imageId = parser.Val;
                    }
                    else if (parser.Key == "frame")
                    {
                        // @ATTR animation.frame|int, int, int, int, int, int, int, int, string: Index, Direction, X, Y, Width, Height, X offset, Y offset, Image ID|A single frame of a compressed animation. The image ID may be omitted, in which case the first available image will be used.
                        if (compressedLoading == false) // first frame statement in section
                        {
                            newanim = new Animation(animName, type, Sprite, blendMode, alphaMod, colorMod);
                            newanim.Setup(frames, duration);
                            if (activeFrames.Count != 0)
                            {
                                newanim.SetActiveFrames(activeFrames);
                                newanim.SetActiveSubFrame(activeSubFrame);
                            }
                            activeFrames.Clear();
                            Animations.Add(newanim);
                            compressedLoading = true;
                        }
                        // frame = index, direction, x, y, w, h, offsetx, offsety, image
                        Rectangle r = new Rectangle();
                        Int2 offset = new Int2();
                        string frameVal = parser.Val;
                        ushort index = (ushort)Parse.PopFirstInt(ref frameVal);
                        ushort direction = (ushort)Parse.ToDirection(Parse.PopFirstString(ref frameVal));
                        r.X = Parse.PopFirstInt(ref frameVal);
                        r.Y = Parse.PopFirstInt(ref frameVal);
                        r.Width = Parse.PopFirstInt(ref frameVal);
                        r.Height = Parse.PopFirstInt(ref frameVal);
                        offset.X = Parse.PopFirstInt(ref frameVal);
                        offset.Y = Parse.PopFirstInt(ref frameVal);
                        string key = frameVal;
                        if (!newanim!.AddFrame(index, direction, r, offset, key))
                        {
                            parser.Error("AnimationSet: Frame index ({0}) is out of bounds [0, {1}].", index, frames);
                        }
                    }
                    else
                    {
                        parser.Error("AnimationSet: '%s' is not a valid key.", parser.Key);
                    }
                }

                if (animName == "")
                {
                    // This is the first animation
                    startingAnimation = parser.Section;
                }
                animName = parser.Section;
            }
            parser.Close();

            if (!compressedLoading)
            {
                // add final animation
                Animation? a = new Animation(animName, type, Sprite, blendMode, alphaMod, colorMod);
                a.SetupUncompressed(renderSize, renderOffset, position, frames, duration, imageId);
                if (activeFrames.Count != 0)
                {
                    a.SetActiveFrames(activeFrames);
                    a.SetActiveSubFrame(activeSubFrame);
                }
                activeFrames.Clear();
                Animations.Add(a);
            }

            if (startingAnimation != "")
            {
                Animation? a = GetAnimation(startingAnimation);
                _defaultAnimation?.Dispose();
                _defaultAnimation = a;
            }
        }

        /// <summary>
        /// 对应 C++ 析构函数：先 <c>sprite->unref()</c>，再逐项 <c>delete</c> 动画与默认动画，最后 <c>delete sprite</c>。
        /// </summary>
        public void Dispose()
        {
            if (Sprite != null)
                Sprite.Unref();
            for (int i = 0; i < Animations.Count; ++i)
                Animations[i]?.Dispose();
            _defaultAnimation?.Dispose();
            Sprite = null;
            GC.SuppressFinalize(this);
        }
    }
}
