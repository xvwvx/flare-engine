// <自动生成> 对应 C++ 源文件：PlatformLinux.cpp（无对应头文件；实现 Platform.h 中声明的
// Platform 类方法在 Linux 平台下的具体逻辑）。
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using System.IO;

namespace FlareEngine
{
    /// <summary>
    /// PlatformLinux
    ///
    /// 对应 C++ 源文件 <c>src/PlatformLinux.cpp</c>。该文件本身没有同名头文件，它在 Linux 构建下为
    /// <c>Platform.h</c> 中声明的 <c>Platform</c> 类提供构造函数（成员初始化列表）与全部方法体的具体
    /// 实现；由于工程里从不存在 <c>Platform.cpp</c>，不同平台（Win32/Linux/Android/...）通过各自独立
    /// 的 .cpp 文件在编译期二选一参与链接，运行期并不存在多态分派。
    ///
    /// 阶段 B 规则要求"只处理当前单元、不得跨单元合并或回改其他逻辑单元"，因此这里不会修改已转换的
    /// <c>Platform.cs</c>（该文件方法体目前以 NotImplementedException 占位，等待各平台单元转换完成后
    /// 由后续集成步骤回填/统一）。为了在不触碰 <c>Platform.cs</c> 的前提下，仍然完整、可编译地表达
    /// PlatformLinux.cpp 的全部逻辑，这里将其建模为 <c>PlatformLinux : Platform</c> 子类：
    /// 构造函数对应 C++ <c>Platform::Platform()</c> 的成员初始化列表，各方法对应同名 C++ 成员函数体，
    /// 一一对应、逐行等价（与已转换的 <c>PlatformWin32.cs</c> 采用完全一致的建模方式）。
    ///
    /// 由于基类 <c>Platform</c> 中的方法目前未声明为 <c>virtual</c>，这里使用方法隐藏（<c>new</c>）
    /// 而非重写（<c>override</c>）以保证独立编译通过；这不影响本单元自身转换的逻辑等价性，但意味着
    /// 通过基类静态类型（例如 <c>Platform.Instance</c>）调用时不会分派到本类实现——这是留给后续集成
    /// 阶段处理的已知风险点，详见随附的 .report.txt。
    /// </summary>
    public class PlatformLinux : Platform
    {
        /// <summary>
        /// 对应 C++ 中可选的构建期宏 <c>DATA_INSTALL_DIR</c>（通常由 Linux 发行版打包脚本通过构建
        /// 系统定义，例如传入 <c>-DDATA_INSTALL_DIR=/opt/flare/share/flare</c>）。原始代码用
        /// <c>#if defined DATA_INSTALL_DIR</c> 判断该宏是否被定义；由于规则禁止使用平台宏/预处理
        /// 指令，这里改用一个默认值为 <c>null</c>（表示"未定义"，对应绝大多数默认构建）的静态属性
        /// 承载同样的"可选构建期配置项"语义，需要该行为的发行版打包脚本可在启动时显式赋值。
        /// </summary>
        public static string? DataInstallDir { get; set; } = null;

        /// <summary>
        /// 对应 C++ 构造函数 <c>Platform::Platform()</c> 的成员初始化列表（本文件是该构造函数在
        /// Linux 平台下的实现出处，因为 Platform.cpp 不存在）。逐项对应，顺序与原始初始化列表一致。
        /// </summary>
        public PlatformLinux()
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

        /// <summary>
        /// 对应 C++ <c>Platform::setPaths()</c>。尝试遵循
        /// http://standards.freedesktop.org/basedir-spec/basedir-spec-latest.html 规范，
        /// 依次确定配置路径（PathConf）、用户数据路径（PathUser）与游戏数据路径（PathData）。
        /// 逐行保留原始分支结构与求值顺序。
        /// </summary>
        public override void SetPaths()
        {
            Settings settings = SharedResources.Settings!;

            // 设置配置路径（settings、keybindings）
            // $XDG_CONFIG_HOME/flare/
            if (Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") != null)
            {
                settings.PathConf = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") + "/flare/";
            }
            // $HOME/.config/flare/
            else if (Environment.GetEnvironmentVariable("HOME") != null)
            {
                settings.PathConf = Environment.GetEnvironmentVariable("HOME") + "/.config/";
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
            if (Environment.GetEnvironmentVariable("XDG_DATA_HOME") != null)
            {
                settings.PathUser = Environment.GetEnvironmentVariable("XDG_DATA_HOME") + "/flare/";
            }
            // $HOME/.local/share/flare/
            else if (Environment.GetEnvironmentVariable("HOME") != null)
            {
                settings.PathUser = Environment.GetEnvironmentVariable("HOME") + "/.local/";
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
            // settings.PathConf 与 settings.PathUser 在不存在时会被自动创建，
            // 而 settings.PathData 必须已经包含游戏运行所需的数据文件，游戏才能正常工作。
            // 在大多数发行版本中，数据文件与可执行文件位于同一目录：
            // - Windows 应用以简单文件夹形式发布
            // - OSX 应用发布在 .app 文件夹内
            // 正式的 Linux 发行版可能会把可执行文件与数据文件放在更"标准"的位置。

            // 找到有效目录时，该标志会被置为 true
            bool pathData = false;

            // 如果用户指定了数据路径，尝试使用它
            if (Filesystem.PathExists(settings.CustomPathData))
            {
                settings.PathData = settings.CustomPathData;
                pathData = true;
            }
            else if (!string.IsNullOrEmpty(settings.CustomPathData))
            {
                Utils.LogError("Platform: Could not find specified game data directory.");
                settings.CustomPathData = "";
            }

            // 在尝试已安装路径之前，先检查本地数据
            if (Filesystem.PathExists("./mods"))
            {
                if (!pathData) settings.PathData = "./";
                pathData = true;
            }

            // 检查 $XDG_DATA_DIRS 选项
            // 一个按优先顺序排列、以 : 分隔的目录列表
            if (Environment.GetEnvironmentVariable("XDG_DATA_DIRS") != null)
            {
                string pathlist = Environment.GetEnvironmentVariable("XDG_DATA_DIRS")!;
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

            // 对应 #if defined DATA_INSTALL_DIR ... #endif（见 DataInstallDir 字段说明）
            if (!pathData && DataInstallDir != null) settings.PathData = DataInstallDir + "/";
            if (!pathData && Filesystem.PathExists(settings.PathData)) pathData = true;

            // 接下来检查 /usr/local/share/flare/ 与 /usr/share/flare/
            if (!pathData) settings.PathData = "/usr/local/share/flare/";
            if (!pathData && Filesystem.PathExists(settings.PathData)) pathData = true;

            if (!pathData) settings.PathData = "/usr/share/flare/";
            if (!pathData && Filesystem.PathExists(settings.PathData)) pathData = true;

            // 检查以上路径的 "games" 变体
            if (!pathData) settings.PathData = "/usr/local/share/games/flare/";
            if (!pathData && Filesystem.PathExists(settings.PathData)) pathData = true;

            if (!pathData) settings.PathData = "/usr/share/games/flare/";
            if (!pathData && Filesystem.PathExists(settings.PathData)) pathData = true;

            // 最终假定为本地文件夹
            if (!pathData)
            {
                // 对应 C++ 的 char abs_path[1024]; memset(abs_path, 0, 1024);
                // C# char[] 元素默认值即为 '\0'，与 memset 清零语义一致。
                char[] absPath = new char[1024];

                // 对应 readlink("/proc/self/exe", abs_path, 1024)：.NET 没有与 readlink 完全等价、
                // 且不涉及 P/Invoke 的 API，这里使用跨平台的 BCL API Environment.ProcessPath
                // 获取当前进程可执行文件的绝对路径，语义等价（均返回可执行文件的绝对路径），
                // 用其字符串长度对应原始 readlink 的返回值 len。
                string? exePath = Environment.ProcessPath;
                int len = exePath != null ? exePath.Length : -1;

                if (len >= 0 && len < 1024)
                {
                    exePath!.CopyTo(0, absPath, 0, len);

                    // 从 abs_path 中去掉可执行文件名，只保留目录部分（含末尾的 '/'）
                    const char BreakPoint = '/';
                    const char BreakString = '\0';
                    for (int i = len; i >= 0; --i)
                    {
                        if (absPath[i] == BreakPoint)
                        {
                            absPath[i + 1] = BreakString;
                            break;
                        }
                    }

                    // 对应 std::string(abs_path)：从字符数组构造字符串时在第一个 '\0' 处截断。
                    int terminatorIndex = Array.IndexOf(absPath, BreakString);
                    settings.PathData = terminatorIndex >= 0 ? new string(absPath, 0, terminatorIndex) : new string(absPath);
                }
                else
                {
                    // 无法获取可执行文件路径，因此直接使用工作目录
                    settings.PathData = "./";
                }
            }
        }

        /// <summary>
        /// 对应 C++ <c>mkdir(path.c_str(), S_IRWXU | S_IRWXG | S_IRWXO)</c>。POSIX 的 mkdir 在
        /// 目标路径已存在时（无论文件还是目录）也会失败并返回 -1，因此这里先显式检查路径是否已
        /// 存在以复现相同的失败语义，而不是直接调用 Directory.CreateDirectory（其对已存在的目录
        /// 会静默成功，与原始行为不等价）。请求的 0777 权限位由 Directory.CreateDirectory 的默认
        /// 行为提供（在 Linux 上同样会被进程 umask 掩码，与 mkdir 的权限语义一致），因此无需额外
        /// 指定 UnixFileMode。
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
        /// 对应 C++ <c>rmdir(path.c_str())</c>。Directory.Delete(path, false)（非递归）在目录
        /// 不存在或非空时均会抛出异常，失败语义与 rmdir 等价。
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

        // unused
        public override void FsInit() { }
        public override bool FsCheckReady() { return true; }
        public override void FsCommit() { }
        public override void SetScreenSize() { }
        public override void SetFullscreen(bool enable) { }
        public override void SetExitEventFilter() { }
    }
}
