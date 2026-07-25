// <自动生成> 对应 C++ 源文件：PlatformIPhoneOS.cpp（无对应头文件；实现 Platform.h 中声明的
// Platform 类各方法在 iOS 平台下的具体版本，另含该源文件内部的匿名命名空间全局函数 isExitEvent）。
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using System.IO;

namespace FlareEngine
{
    /// <summary>
    /// 对应传递给 SDL 事件过滤器回调的事件信息（原始为 <c>SDL_Event*</c>）。
    /// 依据规则"业务逻辑禁止直接调用 SDL2 静态方法/类型，SDL 相关调用必须通过接口实例"，
    /// 这里不引用真实的 SDL 类型，只保留本单元实际用到的最小信息（事件类型）。
    /// 具体的 SDL 绑定（真正解析原始 SDL_Event 并填充本结构）由后续 SDL 封装单元提供。
    /// </summary>
    public enum SdlEventType
    {
        /// <summary>对应 SDL_APP_TERMINATING。</summary>
        AppTerminating,
        /// <summary>本单元逻辑上不关心的其它所有事件类型，统一归为该值。</summary>
        Other
    }

    /// <summary>
    /// 对应传入事件过滤器回调的事件信息（原始为 <c>SDL_Event*</c>）。
    /// </summary>
    public readonly struct SdlEventInfo
    {
        public SdlEventType Type { get; init; }
    }

    /// <summary>
    /// 对应 <c>SDL_EventFilter</c> 回调函数指针类型：<c>int (*)(void* userdata, SDL_Event* event)</c>。
    /// </summary>
    public delegate int SdlEventFilterCallback(object? userdata, SdlEventInfo sdlEvent);

    /// <summary>
    /// SDL 事件过滤器注册服务的抽象接口，封装原始代码中对 <c>SDL_SetEventFilter</c> 的调用
    /// （规则：业务逻辑禁止直接调用 SDL2 静态方法，必须通过接口实例调用）。
    /// 具体的 SDL 绑定实现（真正调用 SDL_SetEventFilter）将由后续 SDL 封装单元提供。
    /// </summary>
    public interface ISdlEventFilterService
    {
        /// <summary>对应 SDL_SetEventFilter(filter, userdata)。</summary>
        void SetEventFilter(SdlEventFilterCallback filter, object? userdata);
    }

    /// <summary>
    /// PlatformIPhoneOS
    ///
    /// 对应 C++ 源文件 PlatformIPhoneOS.cpp：该文件是 Platform.h 中声明的 Platform 类
    /// 在 iOS 平台下的具体实现，与 PlatformWin32.cpp / PlatformLinux.cpp / PlatformAndroid.cpp /
    /// PlatformEmscripten.cpp / PlatformGCW0.cpp 是平级的独立逻辑单元；原始 C++ 构建系统
    /// 按目标平台只编译其中一个 .cpp 文件到最终可执行文件。
    ///
    /// 依据阶段 B 规则"只处理当前单元、不得回改其他逻辑单元"，本文件不修改已转换的只读参考文件
    /// Platform.cs（其方法未声明 virtual）。为保持与同批次 PlatformWin32.cs / PlatformLinux.cs /
    /// PlatformEmscripten.cs 一致的建模方式，这里将其表达为 <c>PlatformIPhoneOS : Platform</c> 子类，
    /// 用方法隐藏（<c>new</c>）而非重写提供 iOS 平台的具体实现。运行期通过基类静态类型
    /// （如 Platform.Instance）调用时不会分派到本类实现，属于已知的集成期风险点（详见报告）。
    /// </summary>
    public class PlatformIPhoneOS : Platform
    {
        /// <summary>
        /// SDL 事件过滤器注册服务的注入点（详见 <see cref="ISdlEventFilterService"/> 说明）。
        /// 在具体 SDL 实现单元完成转换、并对该属性赋值之前，等价于原始代码中
        /// SDL_SetEventFilter 调用被跳过的情况，不会抛出异常。
        /// </summary>
        public static ISdlEventFilterService? EventFilterService { get; set; }

        /// <summary>
        /// 对应 Platform::Platform()（iOS 平台版本）的成员初始化列表与构造函数体：
        /// 设置各配置项类别在设置菜单中的默认可见性，并应用 iOS 平台特有的默认值
        /// （移动设备、强制硬件光标等）。
        /// </summary>
        public PlatformIPhoneOS()
        {
            HasExitButton = false;
            IsMobileDevice = true;
            ForceHardwareCursor = true;
            HasLockFile = false;
            NeedsAltEscapeKey = false;
            FullscreenBypass = false;
            ConfigMenuType = ConfigMenuTypeBase;
            DefaultRenderer = "sdl_hardware";
            ConfigVideo = new List<bool>(Enumerable.Repeat(true, Video.Count));
            ConfigAudio = new List<bool>(Enumerable.Repeat(true, Audio.Count));
            ConfigGame = new List<bool>(Enumerable.Repeat(true, Game.Count));
            ConfigInterface = new List<bool>(Enumerable.Repeat(true, Interface.Count));
            ConfigInput = new List<bool>(Enumerable.Repeat(true, Input.Count));
            ConfigMisc = new List<bool>(Enumerable.Repeat(true, Misc.Count));

            ConfigVideo[Video.Renderer] = false;
            ConfigVideo[Video.Fullscreen] = false;
            ConfigVideo[Video.Hwsurface] = false;
            ConfigVideo[Video.Vsync] = false;
            ConfigVideo[Video.TextureFilter] = false;
            ConfigVideo[Video.DpiScaling] = false;

            ConfigInterface[Interface.HardwareCursor] = false;

            ConfigInput[Input.Joystick] = false;
            ConfigInput[Input.MouseMove] = false;
            ConfigInput[Input.MouseAim] = false;
            ConfigInput[Input.NoMouse] = false;
            ConfigInput[Input.MouseMoveSwap] = false;
            ConfigInput[Input.MouseMoveAttack] = false;
            ConfigInput[Input.JoystickDeadzone] = false;

            ConfigMisc[Misc.Keybinds] = false;
        }

        // 对应 Platform::~Platform() {}：原始析构函数为空实现，未持有任何非托管资源；
        // C# 中无需为"什么都不做"的析构函数提供终结器等价物（等价于省略即安全）。

        /// <summary>
        /// 对应 Platform::setPaths()（iOS 平台版本）。
        /// 原始代码注释："此实现从 Linux 平台拷贝而来，这可能并不正确，但 Settings.cpp 中此前
        /// 的代码路径就是这样处理的"——按逐行等价原则原样保留该行为与该注释所述的历史背景。
        /// </summary>
        public override void SetPaths()
        {
            Settings settings = SharedResources.Settings!;

            // 尝试遵循以下规范：
            // http://standards.freedesktop.org/basedir-spec/basedir-spec-latest.html

            // 设置配置文件路径（settings、keybindings）
            // $XDG_CONFIG_HOME/flare/
            string? xdgConfigHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
            if (xdgConfigHome != null)
            {
                settings.PathConf = xdgConfigHome + "/flare/";
            }
            // $HOME/.config/flare/
            else if (Environment.GetEnvironmentVariable("HOME") is string home1)
            {
                settings.PathConf = home1 + "/.config/";
                Filesystem.CreateDir(settings.PathConf);
                settings.PathConf += "flare/";
            }
            // ./config/
            else
            {
                settings.PathConf = "./config/";
            }

            Filesystem.CreateDir(settings.PathConf);

            // 设置用户路径（存档）
            // $XDG_DATA_HOME/flare/
            string? xdgDataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
            if (xdgDataHome != null)
            {
                settings.PathUser = xdgDataHome + "/flare/";
            }
            // $HOME/.local/share/flare/
            else if (Environment.GetEnvironmentVariable("HOME") is string home2)
            {
                settings.PathUser = home2 + "/.local/";
                Filesystem.CreateDir(settings.PathUser);
                settings.PathUser += "share/";
                Filesystem.CreateDir(settings.PathUser);
                settings.PathUser += "flare/";
            }
            // ./saves/
            else
            {
                settings.PathUser = "./userdata/";
            }

            Filesystem.CreateDir(settings.PathUser);
            Filesystem.CreateDir(settings.PathUser + "mods/");
            Filesystem.CreateDir(settings.PathUser + "saves/");

            // 数据文件夹
            // settings.PathConf 与 settings.PathUser 若不存在会被自动创建，
            // 但 settings.PathData 必须已经存在实际的游戏数据，游戏才能正常运行。
            // 在大多数发行版中，数据会与可执行文件放在同一目录：
            // - Windows 应用以简单文件夹形式发布
            // - OSX 应用以 .app 文件夹形式发布
            // 正式的 Linux 发行版可能会将可执行文件与数据文件放在更标准的位置。

            // 当找到有效目录时，这些标记会被置为 true
            bool pathData = false;

            // 如果用户指定了数据路径，尝试使用它
            if (Filesystem.PathExists(settings.CustomPathData))
            {
                if (!pathData) settings.PathData = settings.CustomPathData;
                pathData = true;
            }
            else if (!string.IsNullOrEmpty(settings.CustomPathData))
            {
                Utils.LogError("Platform: Could not find specified game data directory.");
                settings.CustomPathData = "";
            }

            // 在尝试已安装路径之前，先检查本地数据。
            if (Filesystem.PathExists("./mods"))
            {
                if (!pathData) settings.PathData = "./";
                pathData = true;
            }

            // 检查 $XDG_DATA_DIRS 选项
            // 一组按优先顺序、以 : 分隔的目录列表
            string? xdgDataDirs = Environment.GetEnvironmentVariable("XDG_DATA_DIRS");
            if (xdgDataDirs != null)
            {
                string pathlist = xdgDataDirs;
                string pathtest;
                pathtest = Parse.PopFirstString(ref pathlist, ':');
                while (pathtest != "")
                {
                    if (!pathData)
                    {
                        settings.PathData = pathtest + "/flare/";
                        if (Filesystem.PathExists(settings.PathData)) pathData = true;
                    }
                    if (pathData) break;
                    pathtest = Parse.PopFirstString(ref pathlist, ':');
                }
            }

            // 注：原始代码此处存在 `#if defined DATA_INSTALL_DIR ... #endif` 条件编译块，
            // 用于部分 Linux 发行版打包时由构建系统注入安装前缀路径。这段代码本身就是从
            // Linux 平台拷贝而来的遗留路径（见函数顶部注释），DATA_INSTALL_DIR 宏在 iOS
            // 目标平台的构建配置中从不会被定义，因此该分支在 iOS 实际编译产物中从未被包含。
            // 依据"禁止使用平台宏"的规则，且按预处理器在本平台下的真实展开结果，此处等价省略
            // 该块（若未来 iOS 构建配置开始定义该宏，需要人工补回，详见转换报告）。

            // 接着检查 /usr/local/share/flare/ 与 /usr/share/flare/
            if (!pathData) settings.PathData = "/usr/local/share/flare/";
            if (!pathData && Filesystem.PathExists(settings.PathData)) pathData = true;

            if (!pathData) settings.PathData = "/usr/share/flare/";
            if (!pathData && Filesystem.PathExists(settings.PathData)) pathData = true;

            // 检查这些路径的 "games" 变体
            if (!pathData) settings.PathData = "/usr/local/share/games/flare/";
            if (!pathData && Filesystem.PathExists(settings.PathData)) pathData = true;

            if (!pathData) settings.PathData = "/usr/share/games/flare/";
            if (!pathData && Filesystem.PathExists(settings.PathData)) pathData = true;

            // 最后假定使用本地文件夹
            if (!pathData) settings.PathData = "./";
        }

        /// <summary>
        /// 对应 Platform::setExitEventFilter()（iOS 平台版本）：通过
        /// <see cref="ISdlEventFilterService"/> 注册 <see cref="PlatformIPhoneOS.IsExitEvent"/>
        /// 作为 SDL 事件过滤器（对应 <c>SDL_SetEventFilter(PlatformIPhoneOS::isExitEvent, NULL)</c>）。
        /// </summary>
        public override void SetExitEventFilter()
        {
            EventFilterService?.SetEventFilter(PlatformIPhoneOSHelpers.IsExitEvent, null);
        }

        /// <summary>
        /// 对应 Platform::dirCreate(const std::string&amp;)：创建单层目录（对应 POSIX
        /// <c>mkdir(path, S_IRWXU|S_IRWXG|S_IRWXO)</c>，即所有者/组/其他均可读写执行）。
        /// .NET 的 Directory.CreateDirectory 不接受显式权限位参数，且会递归创建缺失的父目录
        /// （与仅创建单层目录的 mkdir 略有差异，属于跨平台文件系统 API 的必要适配；调用方
        /// Filesystem.CreateDir 在调用前已确认目录不存在，对外的"成功/失败"契约保持一致）。
        /// </summary>
        public override bool DirCreate(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
                return true;
            }
            catch (Exception ex)
            {
                string errorMsg = "Platform::dirCreate (" + path + ")";
                // 对应原始代码中的 perror(error_msg.c_str())：直接向 stderr 输出诊断信息，
                // 与 Utils.LogError（引擎自身的日志系统）是两条不同的输出路径，原样保留这一区分。
                Console.Error.WriteLine(errorMsg + ": " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 对应 Platform::dirRemove(const std::string&amp;)：删除单层空目录
        /// （对应 POSIX <c>rmdir(path)</c>，目录非空时会失败，与 Directory.Delete(path, false) 语义一致）。
        /// </summary>
        public override bool DirRemove(string path)
        {
            try
            {
                Directory.Delete(path, false);
                return true;
            }
            catch (Exception ex)
            {
                string errorMsg = "Platform::dirRemove (" + path + ")";
                Console.Error.WriteLine(errorMsg + ": " + ex.Message);
                return false;
            }
        }

        // 以下几个方法在 iOS 平台上未被使用，与原始 C++ 源码（注释为 "unused"）
        // 一致地保留空实现/固定返回值实现，不做省略。

        /// <summary>对应 Platform::FSInit()（iOS 平台版本，未使用，空实现）。</summary>
        public override void FsInit() { }

        /// <summary>对应 Platform::FSCheckReady()（iOS 平台版本，未使用，恒定返回 true）。</summary>
        public override bool FsCheckReady() { return true; }

        /// <summary>对应 Platform::FSCommit()（iOS 平台版本，未使用，空实现）。</summary>
        public override void FsCommit() { }

        /// <summary>对应 Platform::setScreenSize()（iOS 平台版本，空实现）。</summary>
        public override void SetScreenSize() { }

        /// <summary>对应 Platform::setFullscreen(bool)（iOS 平台版本，空实现）。</summary>
        public override void SetFullscreen(bool enable) { }
    }

    /// <summary>
    /// 对应 C++ 源码中的 <c>namespace PlatformIPhoneOS { int isExitEvent(void*, SDL_Event*); }</c>：
    /// 该命名空间仅用于包裹一个文件内部使用的全局函数，依据规则"全局函数（非成员函数）转为
    /// static class 中的静态方法"，转换为静态类（避免与上方 PlatformIPhoneOS 类型同名，改名为
    /// PlatformIPhoneOSHelpers）。
    /// </summary>
    public static class PlatformIPhoneOSHelpers
    {
        /// <summary>
        /// 对应 PlatformIPhoneOS::isExitEvent(void* userdata, SDL_Event* event)：
        /// 应用退出事件过滤器回调。当收到 SDL_APP_TERMINATING 事件时保存游戏进度，
        /// 之后仍允许该事件继续被 SDL 处理以便应用正常终止；其余事件类型原样放行。
        /// </summary>
        public static int IsExitEvent(object? userdata, SdlEventInfo sdlEvent)
        {
            if (userdata != null) { } // 对应原始代码 `if (userdata) {};`：避免未使用参数的编译警告的占位判断

            if (sdlEvent.Type == SdlEventType.AppTerminating)
            {
                Utils.LogInfo("Terminating app, saving...");
                SharedResources.SaveLoad!.SaveGame();
                Utils.LogInfo("Saved, ready to exit.");
                return 0;
            }
            return 1;
        }
    }
}
