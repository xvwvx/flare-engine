// <自动生成> 对应 C++ 源文件：PlatformAndroid.cpp（无同名 .h，实现 Platform.h 中声明的
// Platform 类在 Android 平台下的具体行为，以及原文件匿名 namespace PlatformAndroid 中的
// 若干平台私有辅助函数）。
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using System.IO;

namespace FlareEngine
{
    /// <summary>
    /// 对应 C++ 中 <c>PlatformAndroid::isExitEvent</c> 注册给 <c>SDL_SetEventFilter</c> 的
    /// 事件过滤器函数指针类型。第一个参数对应原始的 <c>void* userdata</c>（原始调用点始终传入
    /// NULL，仅为保留签名一致性而保留，本身不参与任何判断逻辑）；第二个参数是对
    /// <c>event->type == SDL_APP_TERMINATING</c> 判断结果的抽象（详见 <see cref="IAndroidNativeService"/>
    /// 顶部说明：业务逻辑层禁止直接依赖 SDL_Event 等 SDL 类型）。
    /// </summary>
    public delegate int AndroidExitEventHandler(object? userdata, bool isAppTerminating);

    /// <summary>
    /// 依据阶段 B 强制规则第 6/7 条（业务逻辑禁止直接调用 SDL2 静态方法 / 禁止 P/Invoke），
    /// 本接口封装 PlatformAndroid.cpp 中所有必须依赖 Android JNI 或 SDL Android 扩展 API
    /// （SDL_AndroidGetJNIEnv / SDL_AndroidGetActivity / SDL_AndroidGetExternalStorageState /
    /// SDL_AndroidGetExternalStoragePath / SDL_SetHintWithPriority / SDL_SetEventFilter /
    /// SDL_ShowMessageBox / SDL_OpenURL / SDL_GetError）才能完成的原生调用。
    /// 具体实现将由后续的 SDL/Android 宿主层单元提供并注入 <see cref="PlatformAndroid.NativeService"/>；
    /// 本单元只保证：一旦注入了正确的实现，<see cref="PlatformAndroid"/> 中的业务逻辑
    /// （字符串处理、路径拼接、分支判断、循环顺序）与原始 C++ 源码逐行等价。
    /// </summary>
    public interface IAndroidNativeService
    {
        /// <summary>对应 <c>PlatformAndroid::getPackageName()</c> 中的一系列 JNI 调用，返回应用包名。</summary>
        string GetPackageName();

        /// <summary>
        /// 对应 <c>PlatformAndroid::getExternalFilesDirs()</c> 中通过 JNI 调用
        /// <c>Context.getExternalFilesDirs(null)</c> 并逐个取 <c>File.getAbsolutePath()</c> 得到的
        /// 原始路径列表（未做任何字符串截断处理，截断逻辑保留在 <see cref="PlatformAndroid.GetExternalFilesDirs"/> 中）。
        /// 若底层数组/元素为空（对应原始代码中的 dirs_array / file_obj / path_string 判空分支），应返回空列表或跳过该项。
        /// </summary>
        List<string> GetRawExternalFilesDirs();

        /// <summary>
        /// 对应 <c>SDL_AndroidGetExternalStorageState() != 0</c> 与 <c>SDL_AndroidGetExternalStoragePath()</c>
        /// 两次调用的组合：状态可用时返回 true 并输出路径，否则返回 false。
        /// </summary>
        bool TryGetExternalStoragePath(out string path);

        /// <summary>对应 <c>SDL_GetError()</c>，仅在 <see cref="TryGetExternalStoragePath"/> 返回 false 时用于日志输出。</summary>
        string GetLastError();

        /// <summary>对应 <c>SDL_SetHintWithPriority(SDL_HINT_ORIENTATIONS, orientations, SDL_HINT_OVERRIDE)</c>。</summary>
        void SetOrientationHint(string orientations);

        /// <summary>对应 <c>SDL_SetEventFilter(filter, userdata)</c>。</summary>
        void SetExitEventFilter(AndroidExitEventHandler filter, object? userdata);

        /// <summary>
        /// 对应 <c>SDL_ShowMessageBox</c>：显示一个带若干按钮的消息框，返回被点击按钮在
        /// <paramref name="buttonLabels"/> 中的下标（原始代码里 0 = "No"，1 = "Yes"）。
        /// </summary>
        int ShowMessageBox(string title, string message, string[] buttonLabels);

        /// <summary>对应 <c>SDL_OpenURL(url)</c>。</summary>
        void OpenUrl(string url);
    }

    /// <summary>
    /// 对应 C++ 源文件 PlatformAndroid.cpp。
    ///
    /// 组织方式说明（详见 PlatformAndroid.report.txt）：
    /// 由于依赖单元 Platform.h -&gt; output/Platform.cs 已作为独立逻辑单元先行转换完成，且该文件
    /// 按规则属于只读引用（本单元禁止修改它），其中的 <c>Platform</c> 类既未声明为 <c>partial</c>，
    /// 各方法也未声明为 <c>virtual</c>（只是以 NotImplementedException 占位，等待"具体平台单元转换完成后
    /// 回填，或在具体子类中重写"——原文如此，见 Platform.report.txt 第 31/47 行）。因此本单元既不能以
    /// partial class 补齐 Platform 的方法体，也不能以继承 + 重写的方式让 <c>Platform.Instance</c>
    /// 的调用点自动分派到 Android 实现。
    ///
    /// 综合上述约束，本单元将 PlatformAndroid.cpp 中的全部内容（既包含原始匿名 namespace
    /// PlatformAndroid 中的 4 个辅助函数，也包含 Platform 类各成员函数在 Android 下的具体实现）
    /// 统一组织为一个静态类 <c>PlatformAndroid</c>，方法命名/参数与 Platform.cs 中对应的实例方法
    /// 逐一对应（如 <see cref="SetPaths"/> 对应 <c>Platform.Instance.SetPaths()</c>）。
    /// 这些静态方法直接读写 <c>Platform.Instance</c> 单例（与原始 C++ 中成员函数隐式访问
    /// 唯一全局对象 <c>platform</c> 的语义完全一致，因为整个程序只构造这一个 Platform 实例）。
    /// 待后续批次将 output/Platform.cs 补充改造为 partial class（或改为 virtual + 子类）时，
    /// 只需让 Platform 对应方法体调用本类同名静态方法即可完成"回填"，不需要再次改动本文件。
    /// </summary>
    public static class PlatformAndroid
    {
        /// <summary>
        /// SDL/JNI 原生能力的注入点，用法与 Utils.cs 中的 <c>Utils.PlatformLog</c> 一致。
        /// 在具体的 Android 宿主层实现单元完成转换并赋值之前，本类中依赖原生调用的分支
        /// 按 <c>?.</c> 空条件运算符处理，等价于原始 SDL 调用不可用/无操作的情况。
        /// </summary>
        public static IAndroidNativeService? NativeService { get; set; }

        /// <summary>对应 <c>PlatformAndroid::getPackageName()</c>。</summary>
        public static string GetPackageName()
        {
            return NativeService?.GetPackageName() ?? string.Empty;
        }

        /// <summary>对应 <c>PlatformAndroid::getExternalFilesDirs(std::vector&lt;std::string&gt;&amp;)</c>。</summary>
        public static void GetExternalFilesDirs(List<string> paths)
        {
            List<string> rawDirs = NativeService?.GetRawExternalFilesDirs() ?? new List<string>();

            for (int i = 0; i < rawDirs.Count; ++i)
            {
                string splitStr = rawDirs[i];
                int splitPos = splitStr.IndexOf("/Android/data", StringComparison.Ordinal);
                if (splitPos != -1)
                {
                    splitStr = splitStr.Substring(0, splitPos);
                }
                paths.Add(splitStr);
            }
        }

        /// <summary>
        /// 对应 <c>PlatformAndroid::isExitEvent(void*, SDL_Event*)</c>。
        /// 原始代码首行 <c>if (userdata) {};</c> 仅用于消除 C++ 编译器的"未使用参数"警告，
        /// 不产生任何运行时效果，因此本方法未翻译该行（保留 userdata 形参本身以维持签名一致）。
        /// </summary>
        public static int IsExitEvent(object? userdata, bool isAppTerminating)
        {
            if (isAppTerminating)
            {
                Utils.LogInfo("Terminating app, saving...");
                SharedResources.SaveLoad!.SaveGame();
                Utils.LogInfo("Saved, ready to exit.");
                return 0;
            }
            return 1;
        }

        /// <summary>对应 <c>PlatformAndroid::dialogInstallHint()</c>。</summary>
        public static void DialogInstallHint()
        {
            int buttonid = 0;
            buttonid = NativeService?.ShowMessageBox(
                "Flare",
                "Flare game data needs to be installed. Visit the wiki page for download & instructions?",
                new[] { "No", "Yes" }) ?? 0;

            if (buttonid == 0)
            {
                // do nothing
            }
            else if (buttonid == 1)
            {
                NativeService?.OpenUrl("https://github.com/flareteam/flare-engine/wiki/Android-port");
            }
        }

        /// <summary>
        /// 对应 <c>Platform::Platform()</c> 构造函数（成员初始化列表 + 函数体）在 Android 平台下的具体取值。
        /// 由于本单元不能修改 output/Platform.cs，本方法需要在 Android 目标下、<c>Platform.Instance</c>
        /// 首次被使用之前显式调用一次，以复现原始 C++ 中 <c>Platform platform;</c> 全局对象
        /// 在 Android 编译条件下的构造效果（见报告"关键转换决策"）。
        /// </summary>
        public static void InitializeAndroidDefaults()
        {
            Platform platform = Platform.Instance;

            platform.HasExitButton = true;
            platform.IsMobileDevice = true;
            platform.ForceHardwareCursor = true;
            platform.HasLockFile = false;
            platform.NeedsAltEscapeKey = false;
            platform.FullscreenBypass = false;
            platform.ConfigMenuType = Platform.ConfigMenuTypeBase;
            platform.DefaultRenderer = "";
            platform.ConfigVideo = CreateBoolList(Platform.Video.Count, true);
            platform.ConfigAudio = CreateBoolList(Platform.Audio.Count, true);
            platform.ConfigGame = CreateBoolList(Platform.Game.Count, true);
            platform.ConfigInterface = CreateBoolList(Platform.Interface.Count, true);
            platform.ConfigInput = CreateBoolList(Platform.Input.Count, true);
            platform.ConfigMisc = CreateBoolList(Platform.Misc.Count, true);

            platform.ConfigVideo[Platform.Video.Renderer] = true;
            platform.ConfigVideo[Platform.Video.Fullscreen] = false;
            platform.ConfigVideo[Platform.Video.Hwsurface] = false;
            platform.ConfigVideo[Platform.Video.Vsync] = false;
            platform.ConfigVideo[Platform.Video.TextureFilter] = false;

            platform.ConfigInterface[Platform.Interface.HardwareCursor] = false;

            platform.ConfigInput[Platform.Input.NoMouse] = false;
            platform.ConfigInput[Platform.Input.TouchControls] = false;

            platform.ConfigMisc[Platform.Misc.Keybinds] = true;

            NativeService?.SetOrientationHint("LandscapeLeft LandscapeRight");
        }

        /// <summary>
        /// 对应 C++ 中 <c>std::vector&lt;bool&gt;(count, value)</c> 这种"构造 N 个相同初值元素"的
        /// vector 构造语法，在 <see cref="InitializeAndroidDefaults"/> 中用于替代原始成员初始化列表。
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

        // 对应 Platform::~Platform()：原始实现为空函数体，不释放任何资源，故此处无需任何代码。

        /// <summary>对应 <c>Platform::setPaths()</c>。</summary>
        public static void SetPaths()
        {
            // settings->path_conf
            // 1. INTERNAL_SD_CARD/Flare
            // 2. EXTERNAL_SD_CARD/Flare
            // 3. App internal storage (/data/...)
            //
            // settings->path_data
            // 1. App external storage (usually internal sd card)
            // 2. INTERNAL_SD_CARD/Flare
            //
            // settings->path_user
            // 1. INTERNAL_SD_CARD/Flare
            // 2. EXTERNAL_SD_CARD/Flare
            Settings settings = SharedResources.Settings!;

            List<string> internalSDList = new List<string>();
            internalSDList.Add("/sdcard");
            internalSDList.Add("/mnt/sdcard");
            internalSDList.Add("/storage/sdcard0");
            internalSDList.Add("/storage/emulated/0");
            internalSDList.Add("/storage/emulated/legacy");
            internalSDList.Add("/mnt/m_internal_storage");

            List<string> externalSDList = new List<string>();
            externalSDList.Add("/mnt/extSdCard");
            externalSDList.Add("/storage/extSdCard");
            externalSDList.Add("/mnt/m_external_sd");

            List<string> sdCards = new List<string>();
            GetExternalFilesDirs(sdCards);

            for (int i = 0; i < sdCards.Count; ++i)
            {
                if (internalSDList.Contains(sdCards[i]))
                    continue;

                externalSDList.Add(sdCards[i]);
            }

            if (NativeService != null && NativeService.TryGetExternalStoragePath(out string externalStoragePath))
            {
                settings.PathData = externalStoragePath;
            }
            else
            {
                Utils.LogError("Platform: Android external storage unavailable: %s", NativeService?.GetLastError() ?? string.Empty);
            }

            for (int i = 0; i < internalSDList.Count; i++)
            {
                if (Filesystem.PathExists(internalSDList[i]))
                {
                    settings.PathUser = internalSDList[i] + "/Flare";
                    settings.PathConf = settings.PathUser + "/config";

                    if (settings.PathData.Length == 0)
                    {
                        // This basically gives the same results as SDL_AndroidGetExternalStoragePath(). Should we even bother?
                        settings.PathData = internalSDList[i] + "/Android/data/" + GetPackageName() + "/files";
                    }

                    break;
                }
            }

            if (settings.PathUser.Length == 0 || !Filesystem.PathExists(settings.PathUser))
            {
                for (int i = 0; i < externalSDList.Count; i++)
                {
                    if (Filesystem.PathExists(externalSDList[i]))
                    {
                        settings.PathUser = externalSDList[i] + "/Flare";
                        settings.PathConf = settings.PathUser + "/config";

                        break;
                    }
                }
            }

            Filesystem.CreateDir(settings.PathUser);

            if (Filesystem.PathExists(settings.PathUser))
            {
                // path_user created outside app directory; create path_conf inside it
                Filesystem.CreateDir(settings.PathConf);
            }
            else
            {
                // unable to create /Flare directory, use app directory instead
                settings.PathUser = settings.PathData + "/userdata";
                settings.PathConf = settings.PathData + "/config";
            }

            Filesystem.CreateDir(settings.PathUser + "/mods");
            Filesystem.CreateDir(settings.PathUser + "/saves");

            settings.PathConf += "/";
            settings.PathUser += "/";
            settings.PathData += "/";

            // create a .nomedia file to prevent game data being added to the Android media library
            try
            {
                using StreamWriter nomedia = new StreamWriter(settings.PathUser + ".nomedia", append: false);
            }
            catch (IOException)
            {
                Utils.LogError("Platform: Unable to create Android .nomedia file.");
            }
        }

        /// <summary>对应 <c>Platform::setExitEventFilter()</c>。</summary>
        public static void SetExitEventFilter()
        {
            NativeService?.SetExitEventFilter(IsExitEvent, null);
        }

        /// <summary>
        /// 对应 <c>Platform::dirCreate(const std::string&amp;)</c>。原始实现使用 POSIX <c>mkdir</c> +
        /// <c>perror</c>；.NET 无对应的 P/Invoke 方案（规则禁止），改用 <c>Directory.CreateDirectory</c>
        /// 并在失败时把等价的错误信息写到标准错误流，尽量复现 <c>perror</c> 的对外可观察行为。
        /// </summary>
        public static bool DirCreate(string path)
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

        /// <summary>对应 <c>Platform::dirRemove(const std::string&amp;)</c>，原始实现使用 POSIX <c>rmdir</c> + <c>perror</c>。</summary>
        public static bool DirRemove(string path)
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

        /// <summary>对应 <c>// unused</c> 注释下的 <c>void Platform::FSInit() {}</c>：空实现。</summary>
        public static void FsInit()
        {
        }

        /// <summary>对应 <c>bool Platform::FSCheckReady() { return true; }</c>。</summary>
        public static bool FsCheckReady()
        {
            return true;
        }

        /// <summary>对应 <c>void Platform::FSCommit() {}</c>：空实现。</summary>
        public static void FsCommit()
        {
        }

        /// <summary>对应 <c>void Platform::setScreenSize() {}</c>：空实现。</summary>
        public static void SetScreenSize()
        {
        }

        /// <summary>对应 <c>void Platform::setFullscreen(bool) {}</c>：空实现（形参在原始代码中未命名）。</summary>
        public static void SetFullscreen(bool enable)
        {
        }
    }

    /// <summary>
    /// 将 <see cref="PlatformAndroid"/> 静态 API 桥接到 <see cref="Platform"/> 虚方法分派，
    /// 使 <see cref="Platform.Instance"/> 在 Android 目标下正确调用 Android 实现。
    /// </summary>
    internal sealed class PlatformAndroidHost : Platform
    {
        public PlatformAndroidHost()
        {
            PlatformAndroid.InitializeAndroidDefaults();
        }

        public override void SetPaths() => PlatformAndroid.SetPaths();
        public override void SetExitEventFilter() => PlatformAndroid.SetExitEventFilter();
        public override bool DirCreate(string path) => PlatformAndroid.DirCreate(path);
        public override bool DirRemove(string path) => PlatformAndroid.DirRemove(path);
        public override void FsInit() => PlatformAndroid.FsInit();
        public override bool FsCheckReady() => PlatformAndroid.FsCheckReady();
        public override void FsCommit() => PlatformAndroid.FsCommit();
        public override void SetScreenSize() => PlatformAndroid.SetScreenSize();
        public override void SetFullscreen(bool enable) => PlatformAndroid.SetFullscreen(enable);
    }
}
