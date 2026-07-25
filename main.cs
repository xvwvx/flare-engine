// <自动生成> 对应 C++ 源文件：main.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using FlareEngine.Sdl;

namespace FlareEngine
{
    /// <summary>
    /// 命令行参数容器（对应 C++ main.cpp 顶部定义的 <c>class CmdLineArgs</c>）。
    /// 原始类型是纯数据容器（公有字段、无 getter/setter 方法），依据规则保持为公有字段，
    /// 不额外包装为属性。
    /// </summary>
    public class CmdLineArgs
    {
        public string RenderDeviceName = "";
        public List<string> ModList = new List<string>();
    }

    /// <summary>
    /// 对应 main.cpp 顶部按 <c>#ifdef _WIN32 / __ANDROID__ / __IPHONEOS__ / __GCW0__ /
    /// __EMSCRIPTEN__ / #else</c> 编译期选择并 <c>#include</c> 对应 PlatformXxx.cpp 的六路分支。
    /// C# 没有等价的“按目标平台裁剪编译单元”的机制，规则也明确要求不得用 C# 预处理器 #if
    /// 删除任一分支，因此改用一个可在运行时读取/设置的枚举：由宿主程序（或未来的启动器）
    /// 在真正确定目标运行时环境后设置 <see cref="Program.CurrentPlatform"/>，从而使
    /// <see cref="Program.Main"/> 内部原有的全部平台分支逻辑保持 100% 可达、无一被裁剪。
    /// 六个枚举值与原始六路 #ifdef/#elif/#else 分支一一对应。
    /// </summary>
    public enum TargetPlatform
    {
        Windows,
        Android,
        IPhoneOS,
        Gcw0,
        Emscripten,
        /// <summary>对应原始 #else 分支注释：“Linux stuff should work on Mac OSX/BSD/etc, too”。</summary>
        Linux
    }

    /// <summary>
    /// 对应 main.cpp 中直接使用的 <c>SDL_INIT_VIDEO</c> / <c>SDL_INIT_AUDIO</c> /
    /// <c>SDL_INIT_GAMECONTROLLER</c> 标志位组合。规则禁止在业务逻辑中直接调用 SDL2
    /// 静态方法/常量，这里用与原始按位或语义完全一致的 [Flags] 枚举承载，保持
    /// “多个标志位按位或后一次性传入”的写法不变。
    /// </summary>
    [Flags]
    public enum SdlInitFlags
    {
        Video = 1 << 0,
        Audio = 1 << 1,
        GameController = 1 << 2
    }

    /// <summary>
    /// main.cpp 中直接调用的少量 SDL2 API（SDL_Init / SDL_GetError / SDL_PumpEvents /
    /// SDL_GetPerformanceCounter / SDL_GetPerformanceFrequency / SDL_Delay）的抽象接口。
    /// 与 Utils.cs 中的 IPlatformLogService 属于同一设计模式：具体 SDL 实现单元转换完成后，
    /// 通过设置 <see cref="Program.Sdl"/> 完成依赖注入；实现尚未就绪前调用返回中性的
    /// “无操作/零值”，不抛出异常（与 Utils.PlatformLog 的既有约定保持一致）。
    /// </summary>
    public interface ISdlApplicationService
    {
        /// <summary>对应 SDL_Init(flags)。返回值语义不变：小于 0 表示失败。</summary>
        int Init(SdlInitFlags flags);

        /// <summary>对应 SDL_GetError()。</summary>
        string GetError();

        /// <summary>对应 SDL_PumpEvents()。</summary>
        void PumpEvents();

        /// <summary>对应 SDL_GetPerformanceCounter()。</summary>
        ulong GetPerformanceCounter();

        /// <summary>对应 SDL_GetPerformanceFrequency()。</summary>
        ulong GetPerformanceFrequency();

        /// <summary>对应 SDL_Delay(ms)。</summary>
        void Delay(int milliseconds);
    }

    /// <summary>
    /// main.cpp 中 <c>#ifdef __EMSCRIPTEN__</c> 分支直接调用的、与 FLARE 自身类无关的
    /// Emscripten SDK 原生 API（<c>emscripten_set_main_loop</c> /
    /// <c>emscripten_set_main_loop_timing</c>）。这两个调用属于 Emscripten 工具链自身基于
    /// JS 异常展开栈帧实现的 WebAssembly 主循环调度机制，在 .NET/Stride 环境下没有任何等价
    /// 的运行时钩子，也没有可安全绑定的原生库（规则 5 禁止 P/Invoke）。依据规则“确实不适用
    /// 时可保留分支但需在报告中说明理由”，这里同样改用可注入接口保留调用点与完整分支结构，
    /// 具体运行时绑定留给未来的 Emscripten/WebAssembly 宿主实现。
    /// </summary>
    public interface IEmscriptenRuntimeService
    {
        /// <summary>对应 emscripten_set_main_loop_timing(EM_TIMING_SETTIMEOUT, value)。</summary>
        void SetMainLoopTimingSetTimeout(int value);

        /// <summary>对应 emscripten_set_main_loop(callback, fps, simulateInfiniteLoop)。</summary>
        void SetMainLoop(Action callback, int fps, bool simulateInfiniteLoop);
    }

    /// <summary>
    /// Program
    ///
    /// 对应 C++ 源文件 main.cpp：引擎程序入口点，负责命令行解析、初始化/主循环/清理三段式
    /// 生命周期管理，以及“软重启”（soft_reset）流程。main.cpp 中全部文件作用域自由函数
    /// （init / mainLoop / cleanup / getSecondsElapsed / parseArg / parseArgValue /
    /// EmscriptenMainLoop）依据规则“全局函数 -> static class 中的静态方法”全部合并到本类中。
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// 对应 C++ 中 main.cpp 顶部的文件作用域全局变量 <c>GameSwitcher *gswitch;</c>。
        /// 注意：该变量并未出现在 SharedResources.h 中（它是 main.cpp 私有的全局状态），
        /// 因此没有对应的 SharedResources 属性，改用本类的私有静态字段承载。
        /// </summary>
        private static GameSwitcher? _gswitch;

        /// <summary>
        /// 对应 <c>#elif __EMSCRIPTEN__</c> 分支中声明的文件作用域全局变量
        /// <c>bool init_finished = false;</c>，供 <see cref="EmscriptenMainLoop"/> 使用。
        /// </summary>
        private static bool _initFinished;

        /// <summary>
        /// 见 <see cref="TargetPlatform"/> 说明：运行时可配置的目标平台标识，
        /// 取代原始的编译期 #ifdef 平台宏。默认值通过纯 BCL 的 OperatingSystem.IsXxx()
        /// 运行时检测得出（与 UtilsFileSystem.cs 的 ConvertSlashes 使用同一模式），
        /// GCW0 这类小众嵌入式平台没有对应的检测 API，需要宿主显式设置。
        /// </summary>
        public static TargetPlatform CurrentPlatform { get; set; } = DetectDefaultPlatform();

        /// <summary>SDL 相关能力的注入点，参见 <see cref="ISdlApplicationService"/> 说明。</summary>
        public static ISdlApplicationService? Sdl { get; set; }

        /// <summary>Emscripten 运行时能力的注入点，参见 <see cref="IEmscriptenRuntimeService"/> 说明。</summary>
        public static IEmscriptenRuntimeService? Emscripten { get; set; }

        /// <summary>
        /// 对应 C 运行时的全局 rand()/srand() 状态。C++ 版本通过 <c>srand()</c> 播种一份
        /// 进程级全局随机数状态，供其他源文件（本单元范围之外）调用 <c>rand()</c> 使用。
        /// .NET 没有可重新播种的进程级全局随机数生成器，这里改用一个由 main 播种、
        /// 其他未来单元可复用的静态 Random 实例，承载同样的“全局随机数源”语义。
        /// </summary>
        public static Random Rng { get; set; } = new Random();

        private static TargetPlatform DetectDefaultPlatform()
        {
            if (OperatingSystem.IsWindows()) return TargetPlatform.Windows;
            if (OperatingSystem.IsAndroid()) return TargetPlatform.Android;
            if (OperatingSystem.IsIOS()) return TargetPlatform.IPhoneOS;
            if (OperatingSystem.IsBrowser()) return TargetPlatform.Emscripten;
            // GCW0 是嵌入式 Linux 掌机，.NET 没有对应的 OperatingSystem.IsXxx 检测 API，
            // 与原始 #else 分支注释一致，默认归入 Linux（Mac OSX/BSD 等同样落入此分支）。
            return TargetPlatform.Linux;
        }

        private static int SdlInit(SdlInitFlags flags)
        {
            return Sdl?.Init(flags) ?? 0;
        }

        private static string SdlGetError()
        {
            return Sdl?.GetError() ?? string.Empty;
        }

        private static void SdlPumpEvents()
        {
            Sdl?.PumpEvents();
        }

        private static ulong SdlGetPerformanceCounter()
        {
            return Sdl?.GetPerformanceCounter() ?? 0UL;
        }

        private static ulong SdlGetPerformanceFrequency()
        {
            return Sdl?.GetPerformanceFrequency() ?? 0UL;
        }

        private static void SdlDelay(int milliseconds)
        {
            Sdl?.Delay(milliseconds);
        }

        /// <summary>
        /// Game initialization.
        /// </summary>
        private static void Init(CmdLineArgs cmdLineArgs)
        {
            Settings settings = SharedResources.Settings!;

            /**
             * Set system paths
             * PATH_CONF is for user-configurable settings files (e.g. keybindings)
             * PATH_USER is for user-specific data (e.g. save games)
             * PATH_DATA is for common game data (e.g. images, music)
             */
            Platform.Instance.SetPaths();

            // It's *important* that SetCustomPathData() runs before SetGame() because:
            // 1. We want PATH_CONF to be at the base level if we have to save the custom data path
            // 2. We want to use the custom data path in SetGame() when looking for game.txt
            settings.SetCustomPathData();
            settings.SetGame();

            Utils.LockFileCheck();

            Utils.CreateLogFile();
            Utils.LogInfo(VersionInfo.CreateVersionStringFull());

            // log common paths
            Utils.LogInfo("main: PATH_CONF = '%s'", settings.PathConf);
            Utils.LogInfo("main: PATH_USER = '%s'", settings.PathUser);
            Utils.LogInfo("main: PATH_DATA = '%s'", settings.PathData);

            // SDL Inits
            if (SdlInit(SdlInitFlags.Video | SdlInitFlags.Audio | SdlInitFlags.GameController) < 0)
            {
                Utils.LogError("main: Could not initialize SDL: %s", SdlGetError());
                Utils.LogErrorDialog("main: Could not initialize SDL: %s", SdlGetError());
                Utils.Exit(1);
            }

            // Shared Resources set-up

            SharedResources.Mods = new ModManager(cmdLineArgs.ModList);

            if (!SharedResources.Mods!.HaveFallbackMod())
            {
                Utils.LogError("main: Could not find the default mod in the following locations:");
                if (Filesystem.PathExists(settings.PathUser + "mods")) Utils.LogError("%smods/", settings.PathUser);
                if (Filesystem.PathExists(settings.PathData + "mods")) Utils.LogError("%smods/", settings.PathData);
                Utils.LogError("A copy of the default mod is in the \"mods\" directory of the flare-engine repo.");
                Utils.LogError("The repo is located at: https://github.com/flareteam/flare-engine");
                Utils.LogError("Try again after copying the default mod to one of the above directories. Exiting.");
                Utils.LogErrorDialog("main: Could not find the 'default' mod in the following locations:\n\n%smods/\n\n%smods/", settings.PathUser, settings.PathData);
                if (CurrentPlatform == TargetPlatform.Android)
                {
                    PlatformAndroid.DialogInstallHint();
                }
                Utils.Exit(1);
            }

            settings.LoadSettings();
            settings.LogSettings();

            SharedResources.SaveLoad = new SaveLoad();
            SharedResources.Msg = new MessageEngine();
            Utils.LogInfo("MessageEngine: Using language '%s'", settings.Language);
            SharedResources.Font = DeviceList.GetFontEngine();
            SharedResources.Anim = new AnimationManager();
            SharedResources.Comb = new CombatText();

            // Load miscellaneous settings
            SharedResources.Eset = new EngineSettings();
            SharedResources.Eset!.Load();

            SharedResources.Inpt = DeviceList.GetInputManager();
            SharedResources.Icons = null;

            Stats.Init();

            // platform-specific default screen size
            Platform.Instance.SetScreenSize();

            // Create render Device and Rendering Context.
            if (settings.SafeVideo)
                SharedResources.RenderDevice = DeviceList.GetRenderDevice(settings.RenderDeviceName);
            else if (Platform.Instance.DefaultRenderer != "")
                SharedResources.RenderDevice = DeviceList.GetRenderDevice(Platform.Instance.DefaultRenderer);
            else if (cmdLineArgs.RenderDeviceName != "")
                SharedResources.RenderDevice = DeviceList.GetRenderDevice(cmdLineArgs.RenderDeviceName);
            else
                SharedResources.RenderDevice = DeviceList.GetRenderDevice(settings.RenderDeviceName);

            int status = SharedResources.RenderDevice!.CreateContext();

            if (status == -1)
            {
                Utils.LogError("main: Could not create rendering context: %s", SdlGetError());
                Utils.LogErrorDialog("main: Could not create rendering context: %s", SdlGetError());
                Utils.Exit(1);
            }

            // reset the reload_graphics flag
            SharedResources.RenderDevice!.ReloadGraphics();

            SharedResources.Snd = DeviceList.GetSoundManager();

            SharedResources.Tooltipm = new TooltipManager();

            _gswitch = new GameSwitcher();
        }

        private static float GetSecondsElapsed(ulong prevTicks, ulong nowTicks)
        {
            return (float)(nowTicks - prevTicks) / (float)SdlGetPerformanceFrequency();
        }

        private static void MainLoop()
        {
            Settings settings = SharedResources.Settings!;
            InputState inpt = SharedResources.Inpt!;
            RenderDevice renderDevice = SharedResources.RenderDevice!;
            GameSwitcher gswitch = _gswitch!;

            bool done = false;

            float secondsPerFrame = 1f / (float)settings.MaxFramesPerSec;

            ulong prevTicks = SdlGetPerformanceCounter();
            ulong logicTicks = SdlGetPerformanceCounter();

            float lastFps = -1;

            while (!done)
            {
                int loops = 0;
                ulong nowTicks = SdlGetPerformanceCounter();

                while (nowTicks >= logicTicks && loops < settings.MaxFramesPerSec)
                {
                    // Frames where data loading happens (GameState switching and map loading)
                    // take a long time, so our loop here will think that the game "lagged" and
                    // try to compensate. To prevent this compensation, we mark those frames as
                    // "loading frames" and update the logic ticker without actually executing logic.
                    if (gswitch.IsLoadingFrame())
                    {
                        logicTicks = nowTicks;
                        break;
                    }

                    SdlPumpEvents();
                    inpt.Handle();

                    // Skip game logic when minimized
                    // *except* if the player closes the window when minimized. We then continue with the logic to properly exit
                    if (inpt.WindowMinimized && !inpt.WindowRestored && !inpt.Done)
                        break;

                    gswitch.Logic();
                    inpt.ResetScroll();

                    // Engine done means the user escapes the main game menu.
                    // Input done means the user closes the window.
                    done = gswitch.Done || inpt.Done;

                    logicTicks += (ulong)(secondsPerFrame * (float)SdlGetPerformanceFrequency());
                    loops++;

                    // When the app is minimized, no logic gets processed.
                    // As a result, the delta time when restoring the app is large, so the game will skip frames and appear to be running fast.
                    // To counter this, we reset our delta time here when restoring the app
                    if (inpt.WindowMinimized && inpt.WindowRestored)
                    {
                        logicTicks = nowTicks = SdlGetPerformanceCounter();
                        inpt.WindowMinimized = inpt.WindowRestored = false;
                        break;
                    }

                    // don't skip frames if the game is paused
                    if (gswitch.IsPaused())
                    {
                        logicTicks = nowTicks;
                        break;
                    }
                }

                if (!inpt.WindowMinimized)
                {
                    renderDevice.BlankScreen();
                    gswitch.Render();

                    // display the FPS counter
                    if (lastFps != -1)
                    {
                        gswitch.ShowFPS(lastFps);
                    }

                    renderDevice.CommitFrame();

                    // calculate the FPS
                    // if the frame completed quickly, we estimate the delay here
                    float fpsDelay;
                    if (GetSecondsElapsed(prevTicks, SdlGetPerformanceCounter()) < secondsPerFrame)
                    {
                        fpsDelay = secondsPerFrame;
                    }
                    else
                    {
                        fpsDelay = GetSecondsElapsed(prevTicks, SdlGetPerformanceCounter());
                    }
                    if (fpsDelay != 0)
                    {
                        lastFps = (1000f / fpsDelay) / 1000f;
                    }
                    else
                    {
                        lastFps = -1;
                    }
                }

                // delay quick frames
                // thanks to David Gow: https://davidgow.net/handmadepenguin/ch18.html
                if (GetSecondsElapsed(prevTicks, SdlGetPerformanceCounter()) < secondsPerFrame)
                {
                    int delayMs = (int)((secondsPerFrame - GetSecondsElapsed(prevTicks, SdlGetPerformanceCounter())) * 1000f);
                    if (delayMs > 0)
                    {
                        SdlDelay(delayMs);
                    }
                    while (GetSecondsElapsed(prevTicks, SdlGetPerformanceCounter()) < secondsPerFrame)
                    {
                        // Waiting...
                    }
                }
                prevTicks = SdlGetPerformanceCounter();
            }
        }

        private static void Cleanup()
        {
            Utils.LockFileWrite(-1);

            _gswitch?.Dispose();

            SharedResources.Anim?.Dispose();
            SharedResources.Comb?.Dispose();
            SharedResources.Font?.Dispose();
            SharedResources.Inpt?.Dispose();
            SharedResources.Mods?.Dispose();
            SharedResources.Msg?.Dispose();
            SharedResources.Snd?.Dispose();
            SharedResources.SaveLoad?.Dispose();
            SharedResources.Eset?.Dispose();

            if (SharedResources.RenderDevice != null)
                SharedResources.RenderDevice.DestroyContext();
            SharedResources.RenderDevice?.Dispose();

            // 对应 SDL_Quit()：复用 Utils.cs 中已经建立的 IPlatformLogService.QuitNativeSystem()
            // 注入点（该接口的文档已注明其对应 SDL_Quit()），避免为同一个 SDL 调用重复建模。
            Utils.PlatformLog?.QuitNativeSystem();
        }

        private static string ParseArg(string arg)
        {
            string result = "";

            // arguments must start with '--'
            if (arg.Length > 2 && arg[0] == '-' && arg[1] == '-')
            {
                for (int i = 2; i < arg.Length; ++i)
                {
                    if (arg[i] == '=') break;
                    result += arg[i];
                }
            }

            return result;
        }

        private static string ParseArgValue(string arg)
        {
            string result = "";
            bool foundEquals = false;

            for (int i = 0; i < arg.Length; ++i)
            {
                if (foundEquals)
                {
                    result += arg[i];
                }
                if (arg[i] == '=') foundEquals = true;
            }

            return result;
        }

        /// <summary>
        /// 对应 <c>#ifdef __EMSCRIPTEN__</c> 分支中的 <c>EmscriptenMainLoop()</c>，
        /// 由 Emscripten 运行时以固定帧率反复回调，逐帧驱动一次不阻塞的游戏循环。
        /// </summary>
        private static void EmscriptenMainLoop()
        {
            if (!_initFinished)
            {
                if (Platform.Instance.FsCheckReady())
                {
                    // browsers don't have command line args, so pass default struct to init
                    Init(new CmdLineArgs());
                    _initFinished = true;
                    Emscripten?.SetMainLoopTimingSetTimeout((int)(1000 / SharedResources.Settings!.MaxFramesPerSec));
                }
                return;
            }

            SdlPumpEvents();
            SharedResources.Inpt!.Handle();

            _gswitch!.Logic();
            SharedResources.Inpt!.ResetScroll();

            SharedResources.RenderDevice!.BlankScreen();
            _gswitch!.Render();
            SharedResources.RenderDevice!.CommitFrame();
        }

        public static int Main(string[] args)
        {
            Sdl3Bootstrap.Register();

            SharedResources.Settings = new Settings();
            Settings settings = SharedResources.Settings!;

            bool debugEvent = false;
            bool done = false;
            CmdLineArgs cmdLineArgs = new CmdLineArgs();

            // 注意：C# 的 Main(string[] args) 不包含可执行文件自身路径（对应 C++ 的 argv[0]），
            // 因此这里从索引 0 开始遍历，而不是像原始代码那样从 1 开始；除起始索引外，
            // 解析逻辑与分支顺序逐一对应，行为等价。
            for (int i = 0; i < args.Length; i++)
            {
                string argFull = args[i];
                string arg = ParseArg(argFull);
                if (arg == "debug-event")
                {
                    debugEvent = true;
                }
                else if (arg == "data-path")
                {
                    settings.CustomPathData = ParseArgValue(argFull);
                }
                else if (arg == "save-data-path")
                {
                    settings.CustomPathDataSave = true;
                }
                else if (arg == "clear-data-path")
                {
                    settings.CustomPathDataClear = true;
                }
                else if (arg == "version")
                {
                    Utils.LogInfo("%s", VersionInfo.CreateVersionStringFull());
                    done = true;
                }
                else if (arg == "renderer")
                {
                    cmdLineArgs.RenderDeviceName = ParseArgValue(argFull);
                }
                else if (arg == "no-audio")
                {
                    settings.Audio = false;
                }
                else if (arg == "mods")
                {
                    string modListStr = ParseArgValue(argFull);
                    while (!string.IsNullOrEmpty(modListStr))
                    {
                        cmdLineArgs.ModList.Add(Parse.PopFirstString(ref modListStr));
                    }
                }
                else if (arg == "load-slot")
                {
                    settings.LoadSlot = ParseArgValue(argFull);
                }
                else if (arg == "load-script")
                {
                    settings.LoadScript = ParseArgValue(argFull);
                }
                else if (arg == "safe-video")
                {
                    settings.SafeVideo = true;
                }
                else if (arg == "help")
                {
                    Utils.LogInfo("Command line options:\n" +
                        "--help                   Prints this message.\n" +
                        "--version                Prints the release version.\n" +
                        "--data-path=<PATH>       Specifies an exact path to look for mod data.\n" +
                        "--save-data-path         Saves the path specified with --data-path to the user's config.\n" +
                        "--clear-data-path        Removes a saved data-path from the user's config.\n" +
                        "--debug-event            Prints verbose hardware input information.\n" +
                        "--renderer=<RENDERER>    Specifies the rendering backend to use.\n" +
                        "                         The default is 'sdl'.\n" +
                        "--no-audio               Disables sound effects and music.\n" +
                        "--mods=<MOD>,...         Starts the game with only these mods enabled.\n" +
                        "--load-slot=<SLOT>       Loads a save slot by numerical index.\n" +
                        "--load-script=<SCRIPT>   Execute's a script upon loading a saved game.\n" +
                        "                         The script path is mod-relative.\n" +
                        "--safe-video             Launches with the minimum video settings.");
                    done = true;
                }
                else
                {
                    Utils.LogError("'%s' is not a valid command line option. Try '--help' for a list of valid options.", args[i]);
                }
            }

        SoftReset:
            if (!done)
            {
                // 对应 srand(static_cast<unsigned int>(time(NULL)))：为全局随机数状态播种，
                // 详见 Rng 属性说明。
                Rng = new Random(unchecked((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()));

                if (CurrentPlatform == TargetPlatform.Emscripten)
                {
                    Platform.Instance.FsInit();
                    Emscripten?.SetMainLoop(EmscriptenMainLoop, settings.MaxFramesPerSec, true);
                }
                else
                {
                    Init(cmdLineArgs);

                    if (debugEvent)
                        SharedResources.Inpt!.EnableEventLog();

                    MainLoop();
                }

                if (_gswitch != null)
                    _gswitch.SaveUserSettings();

                Cleanup();
            }

            if (settings.SoftReset)
            {
                Utils.LogInfo("main: Restarting Flare...");
                settings.SoftReset = false;
                done = false;
                cmdLineArgs = new CmdLineArgs();
                goto SoftReset;
            }

            // 对应 delete settings；Settings 未实现 IDisposable（无非托管资源需要释放），
            // 在托管环境下释放最后一个强引用、交由 GC 回收即等价于原始的手动 delete。
            SharedResources.Settings = null;

            return 0;
        }
    }
}
