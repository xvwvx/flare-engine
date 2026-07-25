// <自动生成> 对应 C++ 源文件：PlatformGCW0.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。

namespace FlareEngine
{
    /// <summary>
    /// PlatformGCW0
    ///
    /// 对应 C++ 源文件 <c>src/PlatformGCW0.cpp</c>：GCW0 掌机平台下，Platform.h 中声明的
    /// <c>Platform</c> 类方法（以及全局单例 <c>extern Platform platform;</c>）的具体实现。
    ///
    /// 依据阶段 B 规则"只处理当前单元、不得回改其他逻辑单元"，本文件不修改已转换的只读参考文件
    /// Platform.cs（其方法未声明 virtual）。为保持与同批次 PlatformWin32.cs / PlatformLinux.cs /
    /// PlatformEmscripten.cs 一致的建模方式，这里将其表达为 <c>PlatformGCW0 : Platform</c> 子类，
    /// 用方法隐藏（<c>new</c>）而非重写提供 GCW0 平台的具体实现。运行期通过基类静态类型
    /// （如 Platform.Instance）调用时不会分派到本类实现，属于已知的集成期风险点（详见报告）。
    /// </summary>
    public class PlatformGCW0 : Platform
    {
        /// <summary>
        /// 对应 C++ 中通过 CMake 编译定义注入的 <c>DATA_INSTALL_DIR</c> 宏
        /// （见 CMakeLists.txt: <c>add_definitions(-DDATA_INSTALL_DIR="...")</c>），
        /// 用于 Linux 打包安装场景下的固定数据目录。规则 6 禁止使用平台/编译宏，
        /// 这里改为一个默认为 null 的静态字段：未设置（对应原始宏未定义）时其值为 null，
        /// <see cref="SetPaths"/> 中对应分支会被整体跳过，与原始 <c>#if defined DATA_INSTALL_DIR</c>
        /// 未定义时的行为完全一致；若打包脚本需要该功能，可在集成阶段为该字段赋值。
        /// </summary>
        private static readonly string? DataInstallDir = null;

        /// <summary>
        /// 对应 C++ <c>std::vector&lt;bool&gt;(count, value)</c> 构造函数：创建一个长度为
        /// <paramref name="count"/>、所有元素均为 <paramref name="value"/> 的列表。
        /// </summary>
        private static List<bool> CreateBoolList(int count, bool value)
        {
            List<bool> list = new List<bool>(count);
            for (int i = 0; i < count; i++)
            {
                list.Add(value);
            }
            return list;
        }

        /// <summary>
        /// 对应 <c>Platform::Platform()</c> 的成员初始化列表 + 构造函数体（GCW0 平台的默认值）。
        /// </summary>
        public PlatformGCW0()
        {
            HasExitButton = true;
            IsMobileDevice = false;
            ForceHardwareCursor = true;
            HasLockFile = true;
            NeedsAltEscapeKey = false;
            FullscreenBypass = false;
            ConfigMenuType = ConfigMenuTypeBase;
            DefaultRenderer = string.Empty;

            ConfigVideo = CreateBoolList(Video.Count, true);
            ConfigAudio = CreateBoolList(Audio.Count, true);
            ConfigGame = CreateBoolList(Game.Count, true);
            ConfigInterface = CreateBoolList(Interface.Count, true);
            ConfigInput = CreateBoolList(Input.Count, true);
            ConfigMisc = CreateBoolList(Misc.Count, true);

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

        // 对应 Platform::~Platform() {}：原始析构函数为空，不释放任何资源，
        // 因此与 output/Platform.cs 的既有决定保持一致，不引入终结器/IDisposable。

        /// <summary>
        /// 对应 <c>Platform::setPaths()</c>：按 freedesktop.org basedir 规范探测并创建
        /// 配置目录 / 用户数据目录，并在若干候选路径中确定游戏数据目录。
        /// </summary>
        public override void SetPaths()
        {
            // attempting to follow this spec:
            // http://standards.freedesktop.org/basedir-spec/basedir-spec-latest.html

            Settings settings = SharedResources.Settings!;

            // set config path (settings, keybindings)
            // $XDG_CONFIG_HOME/flare/
            if (Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") != null)
            {
                settings.PathConf = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME")! + "/flare/";
            }
            // $HOME/.config/flare/
            else if (Environment.GetEnvironmentVariable("HOME") != null)
            {
                settings.PathConf = Environment.GetEnvironmentVariable("HOME")! + "/.config/";
                Filesystem.CreateDir(settings.PathConf);
                settings.PathConf += "flare/";
            }
            // ./config/
            else
            {
                settings.PathConf = "./config/";
            }

            Filesystem.CreateDir(settings.PathConf);

            // set user path (save games)
            // $XDG_DATA_HOME/flare/
            if (Environment.GetEnvironmentVariable("XDG_DATA_HOME") != null)
            {
                settings.PathUser = Environment.GetEnvironmentVariable("XDG_DATA_HOME")! + "/flare/";
            }
            // $HOME/.local/share/flare/
            else if (Environment.GetEnvironmentVariable("HOME") != null)
            {
                settings.PathUser = Environment.GetEnvironmentVariable("HOME")! + "/.local/";
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

            // data folder
            // while settings->path_conf and settings->path_user are created if not found,
            // settings->path_data must already have the game data for the game to work.
            // in most releases the data will be in the same folder as the executable
            // - Windows apps are released as a simple folder
            // - OSX apps are released in a .app folder
            // Official linux distros might put the executable and data files
            // in a more standard location.

            // these flags are set to true when a valid directory is found
            bool pathData = false;

            // if the user specified a data path, try to use it
            if (Filesystem.PathExists(settings.CustomPathData))
            {
                if (!pathData) settings.PathData = settings.CustomPathData;
                pathData = true;
            }
            else if (settings.CustomPathData.Length != 0)
            {
                Utils.LogError("Platform: Could not find specified game data directory.");
                settings.CustomPathData = "";
            }

            // Check for the local data before trying installed ones.
            if (Filesystem.PathExists("./mods"))
            {
                if (!pathData) settings.PathData = "./";
                pathData = true;
            }

            // check $XDG_DATA_DIRS options
            // a list of directories in preferred order separated by :
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

            // 对应原始 #if defined DATA_INSTALL_DIR ... #endif（见 DataInstallDir 字段注释）
            if (DataInstallDir != null)
            {
                if (!pathData) settings.PathData = DataInstallDir + "/";
                if (!pathData && Filesystem.PathExists(settings.PathData)) pathData = true;
            }

            // check /usr/local/share/flare/ and /usr/share/flare/ next
            if (!pathData) settings.PathData = "/usr/local/share/flare/";
            if (!pathData && Filesystem.PathExists(settings.PathData)) pathData = true;

            if (!pathData) settings.PathData = "/usr/share/flare/";
            if (!pathData && Filesystem.PathExists(settings.PathData)) pathData = true;

            // check "games" variants of these
            if (!pathData) settings.PathData = "/usr/local/share/games/flare/";
            if (!pathData && Filesystem.PathExists(settings.PathData)) pathData = true;

            if (!pathData) settings.PathData = "/usr/share/games/flare/";
            if (!pathData && Filesystem.PathExists(settings.PathData)) pathData = true;

            // finally assume the local folder
            if (!pathData) settings.PathData = "./";
        }

        /// <summary>对应 <c>Platform::setExitEventFilter()</c>：原始实现为空。</summary>
        public override void SetExitEventFilter()
        {
        }

        /// <summary>
        /// 对应 <c>Platform::dirCreate()</c>：原始实现调用 POSIX <c>mkdir(path, S_IRWXU|S_IRWXG|S_IRWXO)</c>，
        /// 失败（返回 -1）时调用 <c>perror()</c> 输出错误信息并返回 false。规则 6 禁止 P/Invoke，
        /// 这里改用纯 BCL 的 <see cref="Directory.CreateDirectory(string, UnixFileMode)"/>
        /// （.NET 7+ 提供，仅在 Unix 系统上生效，Windows 上会忽略权限位），用
        /// UnixFileMode 的位组合精确复现 S_IRWXU|S_IRWXG|S_IRWXO（所有者/组/其他均可读写执行）；
        /// 创建失败时捕获异常，用异常信息替代 <c>perror</c> 追加的 <c>strerror(errno)</c> 文本，
        /// 同样返回 false（保留错误码语义，不向上抛出异常）。
        /// 【需人工复核】.NET 的 CreateDirectory 在目标目录已存在时会直接成功返回，
        /// 而原始 POSIX mkdir 在目录已存在时会失败（EEXIST）并返回 false；两者语义在
        /// "目录已存在"这一边界情况上不完全一致。
        /// </summary>
        public override bool DirCreate(string path)
        {
            try
            {
                Directory.CreateDirectory(
                    path,
                    UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                    UnixFileMode.GroupRead | UnixFileMode.GroupWrite | UnixFileMode.GroupExecute |
                    UnixFileMode.OtherRead | UnixFileMode.OtherWrite | UnixFileMode.OtherExecute);
            }
            catch (Exception ex)
            {
                string errorMsg = "Platform::dirCreate (" + path + ")";
                Utils.LogError("%s: %s", errorMsg, ex.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 对应 <c>Platform::dirRemove()</c>：原始实现调用 POSIX <c>rmdir()</c>，失败时
        /// <c>perror()</c> 输出错误信息并返回 false；此处用 <see cref="Directory.Delete(string)"/>
        /// 替代，异常信息替代 <c>strerror(errno)</c> 文本，同样保留错误码语义。
        /// </summary>
        public override bool DirRemove(string path)
        {
            try
            {
                Directory.Delete(path);
            }
            catch (Exception ex)
            {
                string errorMsg = "Platform::dirRemove (" + path + ")";
                Utils.LogError("%s: %s", errorMsg, ex.Message);
                return false;
            }
            return true;
        }

        // unused
        /// <summary>对应 <c>Platform::FSInit()</c>：原始实现为空，标注为 unused。</summary>
        public override void FsInit()
        {
        }

        /// <summary>对应 <c>Platform::FSCheckReady()</c>：原始实现恒返回 true，标注为 unused。</summary>
        public override bool FsCheckReady()
        {
            return true;
        }

        /// <summary>对应 <c>Platform::FSCommit()</c>：原始实现为空，标注为 unused。</summary>
        public override void FsCommit()
        {
        }

        /// <summary>对应 <c>Platform::setScreenSize()</c>：原始实现为空，标注为 unused。</summary>
        public override void SetScreenSize()
        {
        }

        /// <summary>对应 <c>Platform::setFullscreen(bool)</c>：原始实现为空（参数未使用），标注为 unused。</summary>
        public override void SetFullscreen(bool enable)
        {
        }
    }
}
