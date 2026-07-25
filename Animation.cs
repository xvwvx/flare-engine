// 对应 C++ 源文件：Animation.h + Animation.cpp
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// Animation
    ///
    /// 根据动画类型推进帧逻辑并返回可渲染帧（对应原始 <c>class Animation</c>）。
    /// 设计目标是尽可能灵活，使动画不仅可用于角色，也可用于任意游戏内动画对象。
    ///
    /// 资源管理：本类持有的 <see cref="_sprite"/>（<c>AnimationMedia*</c>）与
    /// <see cref="_gfx"/> 中的 <c>Image*</c> 均为非占有引用（原始析构函数为默认实现，
    /// 不 delete 任何指针），因此 <see cref="Dispose"/> 对应原始 <c>~Animation()</c> 的空
    /// 语义，仅满足 <see cref="AnimationManager.CheckAnimationsInit"/> 等调用点对
    /// <see cref="IDisposable"/> 的约定。
    /// </summary>
    public class Animation : IDisposable
    {
        // animations consist of:
        // 1. frames, as defined in the animation data files
        // 2. sub-frames, which are generated in this class. Each is associated with a frame (more than one sub-frame can point to the same frame)
        private const ushort Directions = 8; // may change in the future, but we currently hard-code 8 directions engine-wide

        private const byte AnimtypeNone = 0;
        private const byte AnimtypePlayOnce = 1; // just iterates over the images one time. it holds the final image when finished.
        private const byte AnimtypeLooped = 2; // going over the images again and again.
        private const byte AnimtypeBackForth = 3; // similar to looped, but alternates the playback direction

        private const byte ActiveSubframeEnd = 0;
        private const byte ActiveSubframeStart = 1;
        private const byte ActiveSubframeAll = 2;

        private const byte AnimationCompressed = 0;
        private const byte AnimationUncompressed = 1;

        private bool _init; // image loading is deferred, so this flag is used to do some setup when calling getCurrentFrame() for the first time

        private bool _reversePlayback; // only for type == BACK_FORTH
        private bool _activeFrameTriggered;

        private readonly byte _type; // see ANIMTYPE enum above
        private byte _activeSubFrame;
        private readonly byte _blendMode;
        private readonly byte _alphaMod;
        private byte _format;

        private ushort _totalFrameCount; // the total number of frames for this animation (is different from frame_count for back/forth animations)
        private ushort _curFrame; // counts up until reaching total_frame_count.
        private ushort _subFrame; // which frame in this animation is currently being displayed? range: 0..gfx.size()-1
        private short _timesPlayed; // how often this animation was played (loop counter for type LOOPED)

        private uint _frameCount; // the frame count as it appears in the data files (i.e. not converted to engine frames)

        private readonly Color _colorMod;

        private float _subFrameF; // more granular control over sub_frame
        private float _speed; // how fast the sub-frames advance

        private readonly AnimationMedia? _sprite;

        private (Image? First, Rectangle Second)[] _gfx = []; // graphics for each frame taken from the spritesheet
        private Int2[] _renderOffset = []; // "virtual point on the floor"
        private string[] _keys = [];
        private ushort[] _dirs = [];
        private readonly List<short> _activeFrames = []; // frames that are marked as "active". Active frames are used to trigger various states (i.e power activation or hazard danger)
        private readonly List<ushort> _subFrames = []; // a list of frames to play on each tick
        private ushort[] _subFramesFirst = [];
        private ushort[] _subFramesLast = [];

        private readonly string _name;

        public bool DefaultActiveFrames;

        /// <summary>对应 C++ <c>getName()</c>：返回构造时传入的动画名称。</summary>
        public string Name => _name;

        /// <summary>对应 C++ <c>getTimesPlayed()</c>：循环动画的播放次数计数。</summary>
        public int TimesPlayed => _timesPlayed;

        /// <summary>对应 C++ <c>getFrameCount()</c>：数据文件中的原始帧数。</summary>
        public uint FrameCount => _frameCount;

        public Animation(string name, string type, AnimationMedia? sprite, byte blendMode, byte alphaMod, Color colorMod)
        {
            _init = false;
            _reversePlayback = false;
            _activeFrameTriggered = false;
            _type = type == "play_once" ? AnimtypePlayOnce :
                type == "back_forth" ? AnimtypeBackForth :
                type == "looped" ? AnimtypeLooped :
                AnimtypeNone;
            _activeSubFrame = ActiveSubframeEnd;
            _blendMode = blendMode;
            _alphaMod = alphaMod;
            _format = AnimationCompressed;
            _totalFrameCount = 0;
            _curFrame = 0;
            _subFrame = 0;
            _timesPlayed = 0;
            _frameCount = 0;
            _colorMod = colorMod;
            _subFrameF = 0;
            _speed = 1.0f;
            _sprite = sprite;
            _name = name;
            DefaultActiveFrames = true;

            if (_type == AnimtypeNone)
                Utils.LogError("Animation: Type %s is unknown", type);
        }

        /// <summary>对应 C++ 拷贝构造函数 <c>Animation(const Animation&amp;)</c>。</summary>
        public Animation(Animation other)
        {
            _init = other._init;
            _reversePlayback = other._reversePlayback;
            _activeFrameTriggered = other._activeFrameTriggered;
            _type = other._type;
            _activeSubFrame = other._activeSubFrame;
            _blendMode = other._blendMode;
            _alphaMod = other._alphaMod;
            _format = other._format;
            _totalFrameCount = other._totalFrameCount;
            _curFrame = other._curFrame;
            _subFrame = other._subFrame;
            _timesPlayed = other._timesPlayed;
            _frameCount = other._frameCount;
            _colorMod = other._colorMod;
            _subFrameF = other._subFrameF;
            _speed = other._speed;
            _sprite = other._sprite;
            _name = other._name;
            DefaultActiveFrames = other.DefaultActiveFrames;
            _gfx = other._gfx;
            _renderOffset = other._renderOffset;
            _keys = other._keys;
            _dirs = other._dirs;
            _activeFrames = new List<short>(other._activeFrames);
            _subFrames = new List<ushort>(other._subFrames);
            _subFramesFirst = other._subFramesFirst;
            _subFramesLast = other._subFramesLast;
        }

        /// <summary>
        /// 对应 C++ 默认析构函数：不释放 <see cref="_sprite"/> 或 <see cref="_gfx"/> 中的
        /// <see cref="Image"/> 引用，与原始代码一致。
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        // Traditional way to create an animation.
        // The frames are stored in a grid like fashion, so the individual frame
        // position can be calculated based on a few things.
        // The spritesheet has 8 rows, each containing the data of one direction.
        // Within a row starting at (_position) there will be (_frames) frames,
        // which all belong to this animation.
        // The render_offset is constant for all frames. The render_size is also
        // the grid size.
        public void SetupUncompressed(Int2 renderSize, Int2 renderOffset, ushort position, ushort frames, ushort duration, string key)
        {
            Setup(frames, duration);

            for (ushort i = 0; i < frames; i++)
            {
                int baseIndex = Directions * i;
                for (ushort dir = 0; dir < Directions; dir++)
                {
                    uint f = (uint)(baseIndex + dir);
                    (Image? first, Rectangle second) = _gfx[(int)f];
                    second.X = renderSize.X * (position + i);
                    second.Y = renderSize.Y * dir;
                    second.Width = renderSize.X;
                    second.Height = renderSize.Y;
                    _gfx[(int)f] = (first, second);
                    Int2 ro = _renderOffset[(int)f];
                    ro.X = renderOffset.X;
                    ro.Y = renderOffset.Y;
                    _renderOffset[(int)f] = ro;
                    _keys[(int)f] = key;
                    _dirs[(int)f] = dir;
                }
            }

            _format = AnimationUncompressed;
        }

        public void Setup(ushort frames, ushort duration)
        {
            _frameCount = frames;

            _subFrames.Clear();

            if (frames > 0 && duration % frames == 0)
            {
                // if we can evenly space frames among the duration, do it
                ushort divided = (ushort)(duration / frames);
                for (ushort i = 0; i < frames; ++i)
                {
                    for (uint j = 0; j < divided; ++j)
                    {
                        _subFrames.Add(i);
                    }
                }
            }
            else
            {
                // we can't evenly space frames, so we try using Bresenham's line algorithm to lay them out
                // TODO the plain Bresenham algorithm isn't ideal and can cause weird results. Experimentation is needed here
                int x0 = 0;
                ushort y0 = 0;
                int x1 = duration - 1;
                int y1 = frames - 1;

                int dx = x1 - x0;
                int dy = y1 - y0;

                int D = 2 * dy - dx;

                _subFrames.Add(y0);

                int x = x0 + 1;
                ushort y = y0;

                while (x <= x1)
                {
                    if (D > 0)
                    {
                        y++;
                        _subFrames.Add(y);
                        D = D + ((2 * dy) - (2 * dx));
                    }
                    else
                    {
                        _subFrames.Add(y);
                        D = D + (2 * dy);
                    }
                    x++;
                }
            }

            if (_subFrames.Count != 0)
                _totalFrameCount = (ushort)(_subFrames[^1] + 1);

            if (_type == AnimtypeBackForth)
            {
                _totalFrameCount = (ushort)(2 * _totalFrameCount);
            }
            _curFrame = 0;
            _subFrame = 0;
            _subFrameF = 0;
            _timesPlayed = 0;
            _reversePlayback = false;

            _activeFrames.Add((short)((_totalFrameCount - 1) / 2));

            var dirFrames = (Directions * frames);
            _gfx = new (Image First, Rectangle Second)[dirFrames];
            _renderOffset= new Int2[dirFrames];
            _keys = new string[dirFrames];
            Array.Fill(_keys, ""); // C++ std::string 默认为空串，C# string 默认为 null，需要显式填充以避免 null key 异常
            _dirs = new ushort[dirFrames];
            
            _subFramesFirst = new ushort[dirFrames];
            _subFramesLast = new ushort[dirFrames];
            Array.Fill(_subFramesLast, (ushort)(_subFrames.Count - 1));

            for (int i = 0; i < _totalFrameCount; ++i)
            {
                for (int j = 0; j < _subFrames.Count; ++j)
                {
                    if (_subFrames[j] == i)
                    {
                        _subFramesFirst[i] = (ushort)j;
                        break;
                    }
                }
                for (int j = _subFrames.Count; j > 0; j--)
                {
                    if (_subFrames[j - 1] == i)
                    {
                        _subFramesLast[i] = (ushort)(j - 1);
                        break;
                    }
                }
            }
        }

        public bool AddFrame(ushort index, ushort direction, Rectangle rect, Int2 renderOffset, string key)
        {
            if (index >= _gfx.Length / Directions || direction > Directions - 1)
            {
                return false;
            }

            uint i = (uint)((Directions * index) + direction);
            (Image? first, Rectangle second) = _gfx[(int)i];
            second = rect;
            _gfx[(int)i] = (first, second);
            _renderOffset[(int)i] = renderOffset;
            _keys[(int)i] = key;
            _dirs[(int)i] = direction;
            _format = AnimationCompressed;

            return true;
        }

        public void AdvanceFrame()
        {
            if (_subFrames.Count == 0)
            {
                _subFrame = 0;
                _subFrameF = 0;
                _timesPlayed++;
                return;
            }

            ushort lastBaseIndex = (ushort)(_subFrames.Count - 1);
            switch (_type)
            {
                case AnimtypePlayOnce:

                    if (_subFrame < lastBaseIndex)
                    {
                        _subFrameF += _speed;
                        _subFrame = (ushort)_subFrameF;
                    }
                    else
                        _timesPlayed = 1;
                    break;

                case AnimtypeLooped:
                    if (_subFrame < lastBaseIndex)
                    {
                        _subFrameF += _speed;
                        _subFrame = (ushort)_subFrameF;
                    }
                    else
                    {
                        _subFrame = 0;
                        _subFrameF = 0;
                        _timesPlayed++;
                    }
                    break;

                case AnimtypeBackForth:

                    if (!_reversePlayback)
                    {
                        if (_subFrame < lastBaseIndex)
                        {
                            _subFrameF += _speed;
                            _subFrame = (ushort)_subFrameF;
                        }
                        else
                        {
                            _reversePlayback = true;
                            if (_frameCount == 1)
                                _timesPlayed++;
                        }
                    }
                    else if (_reversePlayback)
                    {
                        if (_subFrame > 0)
                        {
                            _subFrameF -= _speed;
                            _subFrame = (ushort)_subFrameF;
                        }
                        else
                        {
                            _reversePlayback = false;
                            _timesPlayed++;
                        }
                    }
                    break;

                case AnimtypeNone:
                    break;
            }
            _subFrame = (ushort)Math.Max((short)0, (short)_subFrame);
            _subFrame = (_subFrame > lastBaseIndex ? lastBaseIndex : _subFrame);

            _curFrame = _subFrames[_subFrame];
        }

        public Renderable GetCurrentFrame(ushort direction)
        {
            Renderable r = new Renderable();
            if (_subFrames.Count != 0)
            {
                ushort index = (ushort)(Directions * _subFrames[_subFrame] + direction);

                CheckInit();

                r.Src.X = _gfx[index].Second.X;
                r.Src.Y = _gfx[index].Second.Y;
                r.Src.Width = _gfx[index].Second.Width;
                r.Src.Height = _gfx[index].Second.Height;
                r.Offset.X = _renderOffset[index].X;
                r.Offset.Y = _renderOffset[index].Y;
                r.Image = _gfx[index].First;
                r.BlendMode = _blendMode;
                r.ColorMod = _colorMod;
                r.AlphaMod = _alphaMod;
            }
            return r;
        }

        public void Reset()
        {
            _curFrame = 0;
            _subFrame = 0;
            _subFrameF = 0;
            _timesPlayed = 0;
            _reversePlayback = false;
            _activeFrameTriggered = false;
        }

        public bool SyncTo(Animation? other)
        {
            _curFrame = other!._curFrame;
            _subFrame = other._subFrame;
            _subFrameF = other._subFrameF;
            _timesPlayed = other._timesPlayed;
            _reversePlayback = other._reversePlayback;

            if (_subFrame >= _subFrames.Count)
            {
                if (_subFrames.Count == 0)
                {
                    Utils.LogError("Animation: '%s' animation has no frames, but current frame index is greater than 0.", _name);
                    _subFrame = 0;
                    _subFrameF = 0;
                    return false;
                }
                else
                {
                    Utils.LogError("Animation: Current frame index (%d) was larger than the last frame index (%d) when syncing '%s' animation.", _subFrame, _subFrames.Count - 1, _name);
                    _subFrame = (ushort)(_subFrames.Count - 1);
                    _subFrameF = _subFrame;
                    return false;
                }
            }

            return true;
        }

        public void SetActiveFrames(List<short> activeFrames)
        {
            if (activeFrames.Count != 0)
                DefaultActiveFrames = false;

            if (activeFrames.Count == 1 && activeFrames[0] == -1)
            {
                _activeFrames.Clear();
                for (ushort i = 0; i < _totalFrameCount; ++i)
                    _activeFrames.Add((short)i);
            }
            else
            {
                _activeFrames.Clear();
                _activeFrames.AddRange(activeFrames);
            }

            // verify that each active frame is not out of bounds
            // this works under the assumption that frames are not dropped from the middle of animations
            // if an animation has too many frames to display in a specified duration, they are dropped from the end of the frame list
            bool haveLastFrame = _activeFrames.Contains((short)(_totalFrameCount - 1));
            for (uint i = 0; i < _activeFrames.Count; ++i)
            {
                if (_activeFrames[(int)i] >= _totalFrameCount)
                {
                    if (haveLastFrame)
                        _activeFrames.RemoveAt((int)i);
                    else
                    {
                        _activeFrames[(int)i] = (short)(_totalFrameCount - 1);
                        haveLastFrame = true;
                    }
                }
            }
        }

        public void SetActiveSubFrame(string activeSubFrame)
        {
            if (activeSubFrame == "start")
                _activeSubFrame = ActiveSubframeStart;
            else if (activeSubFrame == "all")
                _activeSubFrame = ActiveSubframeAll;
            else
                _activeSubFrame = ActiveSubframeEnd;
        }

        public bool IsFirstFrame()
        {
            return _subFrame == 0 && (float)_subFrame == _subFrameF;
        }

        public bool IsLastFrame()
        {
            return _subFrame == GetLastSubFrame((short)(_totalFrameCount - 1));
        }

        public bool IsSecondLastFrame()
        {
            return _subFrame == (short)GetLastSubFrame((short)(_totalFrameCount - 2));
        }

        public bool IsActiveFrame()
        {
            if (_activeFrames.Count == 0)
                return false;

            // active frames only apply to the initial "forward" play of back/forth animations
            if (_type == AnimtypeBackForth && (_reversePlayback || _timesPlayed > 0))
                return false;

            if (_activeFrames.Contains((short)_curFrame))
            {
                if (_activeSubFrame == ActiveSubframeEnd && _subFrame == GetLastSubFrame((short)_curFrame) && (float)_subFrame == _subFrameF)
                {
                    if (_type == AnimtypePlayOnce)
                        _activeFrameTriggered = true;
                    return true;
                }
                else if (_activeSubFrame == ActiveSubframeStart && _subFrame == GetFirstSubFrame((short)_curFrame) && (float)_subFrame == _subFrameF)
                {
                    if (_type == AnimtypePlayOnce)
                        _activeFrameTriggered = true;
                    return true;
                }
                else if (_activeSubFrame == ActiveSubframeAll)
                {
                    if (_type == AnimtypePlayOnce)
                        _activeFrameTriggered = true;
                    return true;
                }
            }
            else if (_type == AnimtypePlayOnce && IsLastFrame() && !_activeFrameTriggered)
            {
                return true;
            }

            return false;
        }

        public bool IsFrame(short frame)
        {
            // only check the initial "forward" play of back/forth animations
            if (_type == AnimtypeBackForth && (_reversePlayback || _timesPlayed > 0))
                return false;

            return _subFrame == (short)GetLastSubFrame(frame);
        }

        public int GetDuration()
        {
            return (int)((float)_subFrames.Count / _speed);
        }

        public bool IsCompleted()
        {
            return (_type == AnimtypePlayOnce && _timesPlayed > 0);
        }

        public void SetSpeed(float val)
        {
            _speed = val / 100.0f;
        }

        public void CheckInit()
        {
            if (!_init)
            {
                for (ushort i = 0; i < _frameCount; i++)
                {
                    int baseIndex = Directions * i;
                    for (ushort dir = 0; dir < Directions; dir++)
                    {
                        uint f = (uint)(baseIndex + dir);

                        if (_format == AnimationCompressed)
                        {
                            (Image? first, Rectangle second) = _gfx[(int)f];
                            first = _sprite!.GetImageFromKey(_keys[(int)f]);
                            _gfx[(int)f] = (first, second);

                            // not all animations have multiple directions, but we need to handle when attempting to render an animation from any direction
                            // we can try to use the rect data from the 0 direction
                            if (dir == 0 && _gfx[(int)f].First != null)
                            {
                                for (ushort testDir = 1; testDir < Directions; ++testDir)
                                {
                                    uint testIndex = (uint)((Directions * i) + testDir);
                                    if (_dirs[(int)testIndex] == 0)
                                    {
                                        _gfx[(int)testIndex] = (_gfx[(int)f].First, _gfx[(int)f].Second);
                                        _renderOffset[(int)testIndex] = _renderOffset[(int)f];
                                    }
                                }
                            }
                        }
                        else if (_format == AnimationUncompressed)
                        {
                            (Image? first, Rectangle second) = _gfx[(int)f];
                            first = _sprite!.GetImageFromKey(_keys[(int)f]);
                            _gfx[(int)f] = (first, second);

                            // not all animations have multiple directions, but we need to handle when attempting to render an animation from any direction
                            // we can try to use the rect data from the 0 direction
                            // to determine if we should do so, we check if the bottom-right corner of a rect is out-of-bounds of the image
                            if (dir > 0 && _gfx[(int)f].First != null)
                            {
                                if (_gfx[(int)f].Second.X + _gfx[(int)f].Second.Width > _gfx[(int)f].First!.GetWidth() || _gfx[(int)f].Second.Y + _gfx[(int)f].Second.Height > _gfx[(int)f].First!.GetHeight())
                                {
                                    (Image? gf, Rectangle gs) = _gfx[(int)f];
                                    gs = _gfx[baseIndex].Second;
                                    _gfx[(int)f] = (gf, gs);
                                }
                            }
                        }
                    }
                }

                _init = true;
            }
        }

        // given a frame, gets the last sub frame that points to it
        private ushort GetFirstSubFrame(short frame)
        {
            if (_subFrames.Count == 0 || frame < 0 || frame >= _subFramesFirst.Length)
                return 0;

            if (_type == AnimtypeBackForth && _reversePlayback)
            {
                // since the animation is advancing backwards here, the last frame index is actually the first
                return _subFramesLast[frame];
            }
            else
            {
                // normal animation
                return _subFramesFirst[frame];
            }
        }

        // given a frame, gets the last sub frame that points to it
        private ushort GetLastSubFrame(short frame)
        {
            if (_subFrames.Count == 0 || frame < 0 || frame >= _subFramesFirst.Length)
                return 0;

            if (_type == AnimtypeBackForth && _reversePlayback)
            {
                // since the animation is advancing backwards here, the first frame index is actually the last
                return _subFramesFirst[frame];
            }
            else
            {
                // normal animation
                return _subFramesLast[frame];
            }
        }
    }
}
