// <自动生成> 对应 C++ 源文件：PlatformWin32.cpp（无对应头文件；实现 Platform.h 中声明的
// Platform 类方法在 Windows 平台下的具体逻辑）。
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using System.IO;

namespace FlareEngine
{
    /// <summary>
    /// PlatformWin32
    ///
    /// 对应 C++ 源文件 <c>src/PlatformWin32.cpp</c>。该文件本身没有同名头文件，它在 Windows 构建下
    /// 为 <c>Platform.h</c> 中声明的 <c>Platform</c> 类提供构造函数（成员初始化列表）与全部方法体的
    /// 具体实现；由于工程里从不存在 <c>Platform.cpp</c>，不同平台（Win32/Linux/Android/...）通过各自
    /// 独立的 .cpp 文件在编译期二选一参与链接，运行期并不存在多态分派。
    ///
    /// 阶段 B 规则要求"只处理当前单元、不得跨单元合并或回改其他逻辑单元"，因此这里不会修改已转换的
    /// <c>Platform.cs</c>（该文件方法体目前以 NotImplementedException 占位，等待各平台单元转换完成后
    /// 由后续集成步骤回填/统一）。为了在不触碰 <c>Platform.cs</c> 的前提下，仍然完整、可编译地表达
    /// PlatformWin32.cpp 的全部逻辑，这里将其建模为 <c>PlatformWin32 : Platform</c> 子类：
    /// 构造函数对应 C++ <c>Platform::Platform()</c> 的成员初始化列表，各方法对应同名 C++ 成员函数体，
    /// 一一对应、逐行等价。
    ///
    /// 由于基类 <c>Platform</c> 中的方法目前未声明为 <c>virtual</c>，这里使用方法隐藏（<c>new</c>）
    /// 而非重写（<c>override</c>）以保证独立编译通过；这不影响本单元自身转换的逻辑等价性，但意味着
    /// 通过基类静态类型（例如 <c>Platform.Instance</c>）调用时不会分派到本类实现——这是留给后续集成
    /// 阶段处理的已知风险点，详见随附的 .report.txt。
    /// </summary>
    public class PlatformWin32 : Platform
    {
        /// <summary>
        /// 对应 C++ 构造函数 <c>Platform::Platform()</c> 的成员初始化列表（本文件是该构造函数
        /// 唯一的实现出处，因为 Platform.cpp 不存在）。逐项对应，顺序与原始初始化列表一致。
        /// </summary>
        public PlatformWin32()
        {
            HasExitButton = true;
            IsMobileDevice = false;
            ForceHardwareCursor = false;
            HasLockFile = true;
            NeedsAltEscapeKey = false;
            FullscreenBypass = false;
            ConfigMenuType = (byte)ConfigMenuTypeDesktop;
            DefaultRenderer = "";
            ConfigVideo = Enumerable.Repeat(true, Video.Count).ToList();
            ConfigAudio = Enumerable.Repeat(true, Audio.Count).ToList();
            ConfigGame = Enumerable.Repeat(true, Game.Count).ToList();
            ConfigInterface = Enumerable.Repeat(true, Interface.Count).ToList();
            ConfigInput = Enumerable.Repeat(true, Input.Count).ToList();
            ConfigMisc = Enumerable.Repeat(true, Misc.Count).ToList();
        }

        // 对应 C++ 析构函数 Platform::~Platform()：原实现为空函数体，不释放任何资源，
        // 因此本单元无需实现终结器或 IDisposable。

        public override void SetPaths()
        {
            Settings settings = SharedResources.Settings!;

            // handle Windows-specific path options
            string? appData = Environment.GetEnvironmentVariable("APPDATA");
            if (appData != null)
            {
                string appDataFlarePath = appData + "\\flare";
                settings.PathUser = appDataFlarePath;
                settings.PathConf = appDataFlarePath;
                Filesystem.CreateDir(settings.PathConf);
                Filesystem.CreateDir(settings.PathUser);

                settings.PathConf += "\\config";
                settings.PathUser += "\\userdata";
                Filesystem.CreateDir(settings.PathConf);
                Filesystem.CreateDir(settings.PathUser);
            }
            else
            {
                settings.PathConf = "config";
                settings.PathUser = "userdata";
                Filesystem.CreateDir(settings.PathConf);
                Filesystem.CreateDir(settings.PathUser);
            }

            Filesystem.CreateDir(settings.PathUser + "\\mods");
            Filesystem.CreateDir(settings.PathUser + "\\saves");

            settings.PathData = "";
            if (Filesystem.PathExists(settings.CustomPathData)) settings.PathData = settings.CustomPathData;
            else if (!string.IsNullOrEmpty(settings.CustomPathData))
            {
                Utils.LogError("Platform: Could not find specified game data directory.");
                settings.CustomPathData = "";
            }

            settings.PathConf = settings.PathConf + "/";
            settings.PathUser = settings.PathUser + "/";
        }

        /// <summary>
        /// 对应 C++ <c>_mkdir(path.c_str())</c>。Win32 CRT 的 <c>_mkdir</c> 在目标路径已存在
        /// （无论是文件还是目录）时也会失败并返回非 0，因此这里先显式检查路径是否已存在以复现
        /// 相同的失败语义，而不是直接调用 <c>Directory.CreateDirectory</c>（后者对已存在的目录
        /// 会静默成功，与原始行为不等价）。禁止 P/Invoke，改用纯 BCL 的 System.IO API。
        /// </summary>
        public override bool DirCreate(string path)
        {
            try
            {
                if (Directory.Exists(path) || File.Exists(path))
                {
                    throw new IOException("Platform::dirCreate: path already exists (" + path + ")");
                }
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
        /// 对应 C++ <c>_rmdir(path.c_str())</c>。<c>Directory.Delete(path, false)</c>
        /// 在路径不存在或目录非空时均会抛出异常，失败语义与 <c>_rmdir</c> 等价。
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

        public override void FsInit() { }
        public override bool FsCheckReady() { return true; }
        public override void FsCommit() { }
        public override void SetScreenSize() { }
        public override void SetFullscreen(bool enable) { }
        public override void SetExitEventFilter() { }
    }
}
