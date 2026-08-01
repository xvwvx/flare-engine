// <自动生成> 对应 C++ 源文件：GameSwitcher.h + GameSwitcher.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// GameSwitcher
    ///
    /// 在几个占据整个视图/控制权的主要游戏模式之间进行切换的状态机处理器。
    ///
    /// 例如：
    /// - 主游戏流程（GameStatePlay）
    /// - 标题画面（GameStateTitle）
    /// - 新建游戏画面（GameStateNew）
    /// - 读取游戏画面（GameStateLoad）
    /// - 过场动画（GameStateCutscene）
    ///
    /// C++ 原始类持有若干裸指针成员（currentState/label_fps/background/background_image/
    /// background_frame），均在析构函数中显式 delete 或 unref()，属于"独占所有权，手动释放"的
    /// unique_ptr 语义；C# 版本据此实现 <see cref="IDisposable"/>，在 <see cref="Dispose"/> 中
    /// 按照原始析构函数逐行的释放顺序进行释放。
    /// </summary>
    public class GameSwitcher : IDisposable
    {
        // 注意：C++ 原始头文件中还声明了 `GameSwitcher(const GameSwitcher &copy);` 一个"仅声明、
        // 从未定义"的拷贝构造函数，这是 C++03 时代用于禁止拷贝的惯用手法（一旦被调用会在链接期报错）。
        // C# 的类是引用类型，本就不存在"隐式按值拷贝"的语义，因此没有与之对应的代码需要迁移，
        // 此处不生成任何成员，仅在本注释与转换报告中记录该设计决策。

        private GameState? _currentState;

        private WidgetLabel? _labelFps;
        private Rectangle _fpsPosition;
        private Color _fpsColor;
        private int _fpsCorner;

        private Sprite? _background;
        private Image? _backgroundImage;
        private string _backgroundFilename;
        private List<string> _backgroundList = new List<string>();

        private Sprite? _backgroundFrame;

        private Timer _fpsUpdate;
        private float _lastFps;

        /// <summary>
        /// 对应 C++ 中的公有字段 <c>bool done;</c>：GameState 请求退出应用程序时置位。
        /// 原始字段无 getter/setter 包装，按既有约定保留为公有字段。
        /// </summary>
        public bool Done;

        public GameSwitcher()
        {
            _background = null;
            _backgroundImage = null;
            _backgroundFilename = "";
            _backgroundFrame = null;
            _fpsUpdate = new Timer();
            _lastFps = 0;

            // Stride.Core.Mathematics.Color 的 default 值全部字段为 0（含 Alpha=0，完全透明），
            // 而 C++ 中 fps_color 字段类型 Color 的默认构造函数是 (r=0,g=0,b=0,a=255)（不透明黑色）。
            // fps_color 未出现在原始构造函数初始化列表中，是由其类类型的默认构造函数隐式初始化的，
            // 因此这里显式赋值以复现同样的隐式初始化结果，避免 Stride.Color 默认值语义不同导致的偏差。
            _fpsColor = new Color(0, 0, 0, 255);

            // 每秒刷新 4 次 FPS 计数器
            _fpsUpdate.Duration = (uint)(SharedResources.Settings!.MaxFramesPerSec / 4);

            // 初始状态为片头过场动画，随后进入标题画面
            GameStateTitle title = new GameStateTitle();
            GameStateCutscene intro = new GameStateCutscene(title);

            _currentState = intro;

            if (!intro.Load("cutscenes/intro.txt"))
            {
                intro.Dispose();
                _currentState = title;
            }

            _labelFps = new WidgetLabel();
            Done = false;
            LoadMusic();
            LoadFPS();

            LoadBackgroundList();

            if (_currentState.HasBackground)
                LoadBackgroundImage();

            if (_currentState.HasFrameBackground)
                LoadBackgroundFrameImage();
        }

        /// <summary>
        /// 对应 C++ 的 <c>~GameSwitcher()</c>，按原始析构函数逐行的顺序释放资源：
        /// 先释放 currentState，再释放 label_fps，卸载音乐，释放背景图，清空背景列表，
        /// 最后释放 background_frame。
        /// </summary>
        public void Dispose()
        {
            _currentState?.Dispose();
            _currentState = null;

            _labelFps?.Dispose();
            _labelFps = null;

            SharedResources.Snd!.UnloadMusic();

            FreeBackground();

            _backgroundList.Clear();

            _backgroundFrame?.Dispose();
            _backgroundFrame = null;

            GC.SuppressFinalize(this);
        }

        public void LoadMusic()
        {
            var settings = SharedResources.Settings!;
            var snd = SharedResources.Snd!;

            if (!settings.Audio) return;

            if (settings.MusicVolume > 0)
            {
                string musicFilename = "";
                using FileParser infile = new FileParser();
                // @CLASS GameSwitcher: Default music|Description of engine/default_music.txt
                if (infile.Open("engine/default_music.txt", FileParser.ModFile, FileParser.ErrorNone))
                {
                    while (infile.Next())
                    {
                        // @ATTR music|filename|Filename of a music file to play during game states that don't already have music.
                        if (infile.Key == "music") musicFilename = infile.Val;
                        else infile.Error("GameSwitcher: '%s' is not a valid key.", infile.Key);
                    }
                    infile.Close();
                }

                // 加载并播放音乐
                snd.LoadMusic(musicFilename);
            }
            else
            {
                snd.StopMusic();
            }
        }

        public void LoadBackgroundImage()
        {
            if (_backgroundList.Count == 0) return;

            if (_backgroundFilename != "") return;

            // 加载背景图片
            int index = Program.Rng.Next() % _backgroundList.Count;
            _backgroundFilename = _backgroundList[index];
            _backgroundImage = SharedResources.RenderDevice!.LoadImage(_backgroundFilename, RenderDevice.ErrorNormal);
            RefreshBackground();
        }

        public void LoadBackgroundFrameImage()
        {
            if (_backgroundFrame != null)
                return;

            Image? gfx = SharedResources.RenderDevice!.LoadImage("images/menus/frame_background.png", RenderDevice.ErrorNormal);
            if (gfx != null)
            {
                _backgroundFrame = gfx.CreateSprite();
                gfx.Unref();
            }
            RefreshBackgroundFrame();
        }

        public void LoadFPS()
        {
            // Load FPS rendering settings
            using FileParser infile = new FileParser();
            // @CLASS GameSwitcher: FPS counter|Description of menus/fps.txt
            if (infile.Open("menus/fps.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    // @ATTR position|int, int, alignment : X, Y, Alignment|Position of the fps counter.
                    if (infile.Key == "position")
                    {
                        _fpsPosition.X = Parse.PopFirstInt(ref infile.Val);
                        _fpsPosition.Y = Parse.PopFirstInt(ref infile.Val);
                        _fpsCorner = Parse.ToAlignment(Parse.PopFirstString(ref infile.Val));
                    }
                    // @ATTR color|color|Color of the fps counter text.
                    else if (infile.Key == "color")
                    {
                        _fpsColor = Parse.ToRGB(infile.Val);
                    }
                    else
                    {
                        infile.Error("GameSwitcher: '%s' is not a valid key.", infile.Key);
                    }
                }
                infile.Close();
            }

            // Delete the label object if it exists (we'll recreate this with showFPS())
            if (_labelFps != null)
            {
                _labelFps.Dispose();
                _labelFps = null;
            }
        }

        public bool IsLoadingFrame()
        {
            if (_currentState!.LoadCounter > 0)
            {
                _currentState.LoadCounter--;
                return true;
            }

            return false;
        }

        public bool IsPaused()
        {
            return _currentState!.IsPaused();
        }

        public void Logic()
        {
            var snd = SharedResources.Snd!;
            var curs = SharedResources.Curs!;
            var tooltipm = SharedResources.Tooltipm!;
            var renderDevice = SharedResources.RenderDevice!;
            var inpt = SharedResources.Inpt!;

            snd.Logic();

            // reset the mouse cursor
            curs.Logic();

            // reset the global tooltip
            tooltipm.Clear();

            // Check if a the game state is to be changed and change it if necessary, deleting the old state
            GameState? newState = _currentState!.GetRequestedGameState();
            if (newState != null)
            {
                if (_currentState.ReloadBackgrounds || renderDevice.ReloadGraphics())
                    LoadBackgroundList();

                _currentState.Dispose();
                _currentState = newState;
                _currentState.LoadCounter++;

                // reload the fps meter position
                LoadFPS();

                // if this game state does not provide music, use the title theme
                if (!_currentState.HasMusic)
                    if (!snd.IsPlayingMusic())
                        LoadMusic();

                // if this game state shows a background image, load it here
                if (_currentState.HasBackground)
                    LoadBackgroundImage();
                else
                    FreeBackground();

                // if this game state shows a frame background image, load it here
                if (_currentState.HasFrameBackground)
                {
                    LoadBackgroundFrameImage();
                }
                else
                {
                    _backgroundFrame?.Dispose();
                    _backgroundFrame = null;
                }
            }

            // resize background image when window is resized
            if ((inpt.WindowResized || _currentState.ForceRefreshBackground) && _currentState.HasBackground)
            {
                RefreshBackground();
                RefreshBackgroundFrame();
                _currentState.ForceRefreshBackground = false;
            }

            _currentState.Logic();

            // Check if the GameState wants to quit the application
            Done = _currentState.IsExitRequested();

            if (_currentState.ReloadMusic)
            {
                LoadMusic();
                _currentState.ReloadMusic = false;
            }
        }

        public void ShowFPS(float fps)
        {
            var settings = SharedResources.Settings!;
            var msg = SharedResources.Msg!;

            if (settings.ShowFps && settings.ShowHud)
            {
                if (_labelFps == null) _labelFps = new WidgetLabel();
                if (_fpsUpdate.IsEnd())
                {
                    _fpsUpdate.Reset(Timer.Begin);

                    float avgFps = (fps + _lastFps) / 2f;
                    _lastFps = fps;
                    string sfps = msg.GetV("%s FPS", Utils.FloatToString(avgFps, 2));
                    Rectangle pos = _fpsPosition;
                    _labelFps.SetPos(pos.X, pos.Y);
                    _labelFps.SetText(sfps);
                    _labelFps.SetColor(_fpsColor);
                    pos = _labelFps.GetBounds();
                    Utils.AlignToScreenEdge(_fpsCorner, ref pos);
                    _labelFps.SetPos(pos.X, pos.Y);
                }
                _labelFps.Render();
                _fpsUpdate.Tick();
            }
        }

        public void SaveUserSettings()
        {
            if (_currentState != null && _currentState.SaveSettingsOnExit)
                SharedResources.Settings!.SaveSettings();
        }

        public void Render()
        {
            var renderDevice = SharedResources.RenderDevice!;
            var tooltipm = SharedResources.Tooltipm!;
            var curs = SharedResources.Curs!;

            renderDevice.LoadQueuedImages();

            if (SharedResources.Anim != null)
                SharedResources.Anim.CheckAnimationsInit();

            // display background
            if (_background != null && _currentState!.HasBackground)
            {
                renderDevice.Render(_background);
            }

            if (_backgroundFrame != null && _currentState.HasFrameBackground)
            {
                renderDevice.Render(_backgroundFrame);
            }

            _currentState.Render();
            tooltipm.Render();
            curs.Render();
        }

        private void LoadBackgroundList()
        {
            _backgroundList.Clear();
            FreeBackground();

            using FileParser infile = new FileParser();
            // @CLASS GameSwitcher: Background images|Description of engine/menu_backgrounds.txt
            if (infile.Open("engine/menu_backgrounds.txt", FileParser.ModFile, FileParser.ErrorNone))
            {
                while (infile.Next())
                {
                    // @ATTR background|repeatable(filename)|Filename of a background image to be added to the pool of random menu backgrounds
                    if (infile.Key == "background") _backgroundList.Add(infile.Val);
                    else infile.Error("GameSwitcher: '%s' is not a valid key.", infile.Key);
                }
                infile.Close();
            }
        }

        private void RefreshBackground()
        {
            if (_backgroundImage != null)
            {
                _backgroundImage.Ref();
                Rectangle dest = Utils.ResizeToScreen(_backgroundImage.GetWidth(), _backgroundImage.GetHeight(), true, Utils.AlignCenter);

                Image? resized = _backgroundImage.Resize(dest.Width, dest.Height);
                if (resized != null)
                {
                    if (_background != null)
                        _background.Dispose();

                    _background = resized.CreateSprite();
                    resized.Unref();
                }

                if (_background != null)
                    _background.SetDestFromRect(dest);
            }
        }

        private void FreeBackground()
        {
            _background?.Dispose();
            _background = null;

            if (_backgroundImage != null)
            {
                _backgroundImage.Unref();
                _backgroundImage = null;
            }

            _backgroundFilename = "";
        }

        private void RefreshBackgroundFrame()
        {
            if (_backgroundFrame != null)
            {
                Rectangle dest = default;
                dest.Width = _backgroundFrame.GetGraphics()!.GetWidth();
                dest.Height = _backgroundFrame.GetGraphics()!.GetHeight();
                Utils.AlignToScreenEdge(Utils.AlignFrameTopLeft, ref dest);
                _backgroundFrame.SetDestFromRect(dest);
            }
        }
    }
}
