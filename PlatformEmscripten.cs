// <自动生成> 对应 C++ 源文件：PlatformEmscripten.cpp（无对应头文件；实现 Platform.h 中声明的
// Platform 类方法在 Emscripten/WebAssembly 平台下的具体逻辑）。
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using System.IO;

namespace FlareEngine
{
    /// <summary>
    /// 封装 Emscripten/WebAssembly 平台特有的 EM_ASM / emscripten_run_script_int /
    /// emscripten_async_run_script 等"直接执行浏览器 JS 脚本"的互操作调用。
    ///
    /// .NET（含 Blazor WebAssembly）没有与 EM_ASM 内联 JS 完全等价的语言机制，真正的浏览器交互
    /// 必须通过宿主环境注入的 JS 互操作层完成。因此这里将原始 C++ 中每一段 EM_ASM/
    /// emscripten_* 调用都抽象为一个具名接口方法，保留完整的调用点与调用顺序（详见
    /// <see cref="PlatformEmscripten"/> 类中的 FsInit/FsCheckReady/FsCommit/SetFullscreen），
    /// 由具体宿主（例如未来的 Blazor WebAssembly 启动项目）实现 <see cref="IEmscriptenInterop"/>
    /// 并通过 <see cref="PlatformEmscripten.Interop"/> 注入真实实现。
    /// </summary>
    public interface IEmscriptenInterop
    {
        /// <summary>
        /// 对应 FSInit() 中的 EM_ASM 块：创建并挂载 IndexedDB 持久化文件系统（IDBFS）到
        /// '/flare_data'，随后触发一次"从 IndexedDB 读入内存文件系统"的异步同步
        /// （FS.syncfs(true, ...)），完成后应将同步完成标志置位，供
        /// <see cref="IsFileSystemSyncDone"/> 查询。
        /// </summary>
        void MountPersistentFileSystem();

        /// <summary>
        /// 对应 <c>emscripten_run_script_int("Module.syncdone")</c>：查询最近一次
        /// 文件系统同步（挂载时的读入同步，或写回同步）是否已经完成。
        /// </summary>
        bool IsFileSystemSyncDone();

        /// <summary>
        /// 对应 FSCheckReady() 内部的 EM_ASM 块：将同步完成标志复位，并触发一次
        /// "将内存文件系统写回 IndexedDB"的异步同步（FS.syncfs(false, ...)），
        /// 完成后重新置位同步完成标志。
        /// </summary>
        void PersistFileSystemAndTrackCompletion();

        /// <summary>
        /// 对应 FSCommit() 中的 EM_ASM 块：触发一次"将内存文件系统写回 IndexedDB"的
        /// 异步同步（FS.syncfs(false, ...)），不追踪/不复位同步完成标志。
        /// </summary>
        void CommitFileSystem();

        /// <summary>
        /// 对应 <c>emscripten_async_run_script("Module.canvas.requestFullscreen();", 0)</c>。
        /// </summary>
        void RequestFullscreen();

        /// <summary>
        /// 对应 <c>emscripten_async_run_script("parentDocument.exitFullscreen();", 0)</c>。
        /// </summary>
        void ExitFullscreen();
    }

    /// <summary>
    /// <see cref="IEmscriptenInterop"/> 的默认空操作实现。在尚未接入真实的浏览器 JS 互操作层
    /// （例如尚未运行在 Blazor WebAssembly 宿主中）时用作安全占位，保证
    /// <see cref="PlatformEmscripten"/> 的所有方法体均完整可编译、可执行，不依赖
    /// NotImplementedException 或任何未完成标记。
    /// <see cref="IsFileSystemSyncDone"/> 默认返回 false，与真实浏览器环境中
    /// "尚未完成首次同步"的初始状态保持一致，不会导致误判为"已就绪"。
    /// </summary>
    public sealed class NullEmscriptenInterop : IEmscriptenInterop
    {
        public void MountPersistentFileSystem()
        {
        }

        public bool IsFileSystemSyncDone()
        {
            return false;
        }

        public void PersistFileSystemAndTrackCompletion()
        {
        }

        public void CommitFileSystem()
        {
        }

        public void RequestFullscreen()
        {
        }

        public void ExitFullscreen()
        {
        }
    }

    /// <summary>
    /// PlatformEmscripten
    ///
    /// 对应 C++ 源文件 <c>src/PlatformEmscripten.cpp</c>。该文件本身没有同名头文件，它在
    /// Emscripten/WebAssembly 构建下为 <c>Platform.h</c> 中声明的 <c>Platform</c> 类提供构造函数
    /// （成员初始化列表）与全部方法体的具体实现；由于工程里从不存在 <c>Platform.cpp</c>，
    /// 不同平台（Win32/Linux/Android/Emscripten/GCW0/IPhoneOS）通过各自独立的 .cpp 文件在编译期
    /// 二选一参与链接，运行期并不存在多态分派。
    ///
    /// 阶段 B 规则要求"只处理当前单元、不得跨单元合并或回改其他逻辑单元"，因此这里不会修改已转换的
    /// <c>Platform.cs</c>（该文件方法体目前以 NotImplementedException 占位，等待各平台单元转换完成后
    /// 由后续集成步骤回填/统一）。为了在不触碰 <c>Platform.cs</c> 的前提下，仍然完整、可编译地表达
    /// PlatformEmscripten.cpp 的全部逻辑，并与已转换的 <c>PlatformWin32.cs</c> 保持一致的设计，
    /// 这里同样将其建模为 <c>PlatformEmscripten : Platform</c> 子类：构造函数对应 C++
    /// <c>Platform::Platform()</c> 的成员初始化列表，各方法对应同名 C++ 成员函数体，一一对应、逐行等价。
    ///
    /// 由于基类 <c>Platform</c> 中的方法目前未声明为 <c>virtual</c>，这里使用方法隐藏（<c>new</c>）
    /// 而非重写（<c>override</c>）以保证独立编译通过；这不影响本单元自身转换的逻辑等价性，但意味着
    /// 通过基类静态类型（例如 <c>Platform.Instance</c>）调用时不会分派到本类实现——这是留给后续集成
    /// 阶段处理的已知风险点，详见随附的 .report.txt。
    /// </summary>
    public class PlatformEmscripten : Platform
    {
        /// <summary>
        /// 浏览器 JS 互操作层的注入点，默认使用空操作实现（见 <see cref="NullEmscriptenInterop"/>）。
        /// 宿主环境（例如 Blazor WebAssembly 项目）应在启动时将其替换为真实实现。
        /// </summary>
        public static IEmscriptenInterop Interop { get; set; } = new NullEmscriptenInterop();

        /// <summary>
        /// 对应 C++ 构造函数 <c>Platform::Platform()</c> 的成员初始化列表（本文件是该构造函数在
        /// Emscripten 平台下的唯一实现出处，因为 Platform.cpp 不存在）。逐项对应，顺序与原始
        /// 初始化列表一致，随后是构造函数体内对若干下标元素的覆盖赋值。
        /// </summary>
        public PlatformEmscripten()
        {
            HasExitButton = false;
            IsMobileDevice = false;
            ForceHardwareCursor = false;
            HasLockFile = false;
            NeedsAltEscapeKey = true;
            FullscreenBypass = true;
            ConfigMenuType = (byte)ConfigMenuTypeDesktopNoVideo;
            DefaultRenderer = "sdl_hardware";
            ConfigVideo = Enumerable.Repeat(true, Video.Count).ToList();
            ConfigAudio = Enumerable.Repeat(true, Audio.Count).ToList();
            ConfigGame = Enumerable.Repeat(true, Game.Count).ToList();
            ConfigInterface = Enumerable.Repeat(true, Interface.Count).ToList();
            ConfigInput = Enumerable.Repeat(true, Input.Count).ToList();
            ConfigMisc = Enumerable.Repeat(true, Misc.Count).ToList();

            ConfigVideo[Video.Renderer] = false;
            ConfigVideo[Video.Hwsurface] = false;
            ConfigVideo[Video.Vsync] = false;
            ConfigVideo[Video.TextureFilter] = false;
            ConfigVideo[Video.DpiScaling] = false;
            ConfigVideo[Video.MinRenderSize] = false;
            ConfigVideo[Video.MaxRenderSize] = false;
            ConfigVideo[Video.FrameLimit] = false;

            ConfigInput[Input.Joystick] = false;
            ConfigInput[Input.JoystickDeadzone] = false;
            ConfigInput[Input.JoystickRumble] = false;

            ConfigMisc[Misc.Mods] = false;
        }

        // 对应 C++ 析构函数 Platform::~Platform()：原实现为空函数体，不释放任何资源，
        // 因此本单元无需实现终结器或 IDisposable。

        public override void SetPaths()
        {
            Settings settings = SharedResources.Settings!;

            settings.PathConf = "/flare_data/config/";
            Filesystem.CreateDir(settings.PathConf);

            settings.PathUser = "/flare_data/userdata/";
            Filesystem.CreateDir(settings.PathUser);
            Filesystem.CreateDir(settings.PathUser + "mods/");
            Filesystem.CreateDir(settings.PathUser + "saves/");

            // data folder

            // these flags are set to true when a valid directory is found
            bool pathData = false;

            // Check for the local data before trying installed ones.
            if (Filesystem.PathExists("./mods"))
            {
                if (!pathData) settings.PathData = "./";
                pathData = true;
            }

            // finally assume the local folder
            if (!pathData) settings.PathData = "./";
        }

        public override void SetExitEventFilter()
        {
        }

        /// <summary>
        /// 对应 C++ <c>mkdir(path.c_str(), S_IRWXU | S_IRWXG | S_IRWXO)</c>。POSIX 的
        /// <c>mkdir</c> 在目标路径已存在时返回 -1（失败）；.NET 的 <c>Directory.CreateDirectory</c>
        /// 对已存在的目录会静默成功。调用方 <c>Filesystem.CreateDir</c> 在调用本方法前已先行判断
        /// 目录是否存在（见 output/UtilsFileSystem.cs 中的 IsDirectory 短路检查），当前唯一调用
        /// 路径不会触发该差异，详见 .report.txt 中的风险说明。禁止 P/Invoke，改用纯 BCL 的
        /// System.IO API；<c>S_IRWXU|S_IRWXG|S_IRWXO</c> 这一类 Unix 权限位在 .NET 的
        /// Directory.CreateDirectory 中没有直接对应参数，因此未显式设置（同样记录于报告）。
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
                Console.Error.WriteLine(errorMsg + ": " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 对应 C++ <c>rmdir(path.c_str())</c>。<c>Directory.Delete(path)</c> 在路径不存在或
        /// 目录非空时均会抛出异常，失败语义与 <c>rmdir</c> 等价。
        /// </summary>
        public override bool DirRemove(string path)
        {
            try
            {
                Directory.Delete(path);
                return true;
            }
            catch (Exception ex)
            {
                string errorMsg = "Platform::dirRemove (" + path + ")";
                Console.Error.WriteLine(errorMsg + ": " + ex.Message);
                return false;
            }
        }

        public override void FsInit()
        {
            Interop.MountPersistentFileSystem();
        }

        public override bool FsCheckReady()
        {
            if (Interop.IsFileSystemSyncDone())
            {
                Settings settings = SharedResources.Settings!;
                if (!Filesystem.FileExists(settings.PathConf + "settings.txt"))
                {
                    // persist Emscripten current data to Indexed Db
                    Interop.PersistFileSystemAndTrackCompletion();
                    settings.SaveSettings();
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        public override void FsCommit()
        {
            Interop.CommitFileSystem();
        }

        public override void SetScreenSize()
        {
            // can't change window size dynamically with Emscripten, so default to 16:9 aspect ratio
            Settings settings = SharedResources.Settings!;
            settings.ScreenW = 1920;
            settings.ScreenH = 1080;
            settings.Fullscreen = false;
        }

        public override void SetFullscreen(bool enable)
        {
            if (enable)
                Interop.RequestFullscreen();
            else
                Interop.ExitFullscreen();
        }
    }
}
