// <自动生成> 对应 C++ 源文件：Platform.h
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。

namespace FlareEngine
{
    /// <summary>
    /// 平台相关功能的抽象接口。C++ 版本中，Platform 类的方法实现分散在多个按平台条件编译的
    /// .cpp 文件中（PlatformWin32.cpp / PlatformLinux.cpp / PlatformAndroid.cpp /
    /// PlatformEmscripten.cpp / PlatformGCW0.cpp / PlatformIPhoneOS.cpp），这些文件在依赖关系上
    /// 是与本单元（Platform.h，无同名 .cpp）平级的独立逻辑单元，将在后续批次单独转换。
    /// 依据阶段 B 规则 2："仅 .h -> 直接转换该头文件内的所有声明"，本单元只翻译头文件中的声明结构；
    /// 各方法体在此阶段以 NotImplementedException 占位，等待对应平台单元转换完成后回填。
    /// </summary>
    public class Platform
    {
        public const int ConfigMenuTypeBase = 0;
        public const int ConfigMenuTypeDesktop = 1;
        public const int ConfigMenuTypeDesktopNoVideo = 2;

        public static class Video
        {
            public const int Renderer = 0;
            public const int Fullscreen = 1;
            public const int Hwsurface = 2;
            public const int Vsync = 3;
            public const int TextureFilter = 4;
            public const int DpiScaling = 5;
            public const int ParallaxLayers = 6;
            public const int MinRenderSize = 7;
            public const int MaxRenderSize = 8;
            public const int FrameLimit = 9;
            public const int ThreadedImageLoad = 10;
            public const int FadeWalls = 11;
            public const int Count = 12;
        }

        public static class Audio
        {
            public const int Sfx = 0;
            public const int Music = 1;
            public const int MuteOnFocusLoss = 2;
            public const int Count = 3;
        }

        public static class Game
        {
            public const int AutoEquip = 0;
            public const int AutoLoot = 1;
            public const int LowHpWarningType = 2;
            public const int LowHpThreshold = 3;
            public const int Count = 4;
        }

        public static class Interface
        {
            public const int Language = 0;
            public const int Subtitles = 1;
            public const int Colorblind = 2;
            public const int MinimapMode = 3;
            public const int LootTooltips = 4;
            public const int ItemCompareTips = 5;
            public const int CombatText = 6;
            public const int StatbarLabels = 7;
            public const int StatbarAutohide = 8;
            public const int HardwareCursor = 9;
            public const int PauseOnFocusLoss = 10;
            public const int ShowFps = 11;
            public const int DevMode = 12;
            public const int Count = 13;
        }

        public static class Input
        {
            public const int MouseMove = 0;
            public const int MouseMoveSwap = 1;
            public const int MouseMoveAttack = 2;
            public const int MouseAim = 3;
            public const int NoMouse = 4;
            public const int Joystick = 5;
            public const int JoystickDeadzone = 6;
            public const int JoystickRumble = 7;
            public const int TouchControls = 8;
            public const int TouchScale = 9;
            public const int Count = 10;
        }

        public static class Misc
        {
            public const int Keybinds = 0;
            public const int Mods = 1;
            public const int Count = 2;
        }

        /// <summary>
        /// 对应 C++ 的 <c>extern Platform platform;</c> 全局单例实例。
        /// 按 <see cref="Program.CurrentPlatform"/> 延迟创建具体平台实现（等价于 C++ 编译期链接单一 Platform*.cpp）。
        /// </summary>
        public static Platform Instance => _instance ??= CreateInstance();
        private static Platform? _instance;

        /// <summary>按当前目标平台构造具体 <see cref="Platform"/> 实现。</summary>
        internal static Platform CreateInstance()
        {
            return Program.CurrentPlatform switch
            {
                TargetPlatform.Windows => new PlatformWin32(),
                TargetPlatform.Android => new PlatformAndroidHost(),
                TargetPlatform.IPhoneOS => new PlatformIPhoneOS(),
                TargetPlatform.Gcw0 => new PlatformGCW0(),
                TargetPlatform.Emscripten => new PlatformEmscripten(),
                _ => new PlatformLinux(),
            };
        }

        /// <summary>供测试或宿主在首次访问 <see cref="Instance"/> 前替换平台实现。</summary>
        internal static void SetInstance(Platform platform)
        {
            _instance = platform;
        }

        public bool HasExitButton { get; set; }
        public bool IsMobileDevice { get; set; }
        public bool ForceHardwareCursor { get; set; }
        public bool HasLockFile { get; set; }
        public bool NeedsAltEscapeKey { get; set; }
        public bool FullscreenBypass { get; set; }
        public byte ConfigMenuType { get; set; }
        public string DefaultRenderer { get; set; } = string.Empty;

        public List<bool> ConfigVideo { get; set; } = new List<bool>();
        public List<bool> ConfigAudio { get; set; } = new List<bool>();
        public List<bool> ConfigGame { get; set; } = new List<bool>();
        public List<bool> ConfigInterface { get; set; } = new List<bool>();
        public List<bool> ConfigInput { get; set; } = new List<bool>();
        public List<bool> ConfigMisc { get; set; } = new List<bool>();

        public Platform()
        {
            // 各字段的默认值由具体平台单元 (PlatformWin32/PlatformLinux/...) 的构造函数实现设置，
            // 此处仅声明结构，等待后续批次转换回填。
        }

        public virtual void SetPaths()
        {
            throw new NotImplementedException("Platform.SetPaths: 未找到当前平台的 SetPaths 实现。");
        }

        public virtual void SetExitEventFilter()
        {
            throw new NotImplementedException("Platform.SetExitEventFilter: 未找到当前平台的实现。");
        }

        public virtual bool DirCreate(string path)
        {
            throw new NotImplementedException("Platform.DirCreate: 未找到当前平台的实现。");
        }

        public virtual bool DirRemove(string path)
        {
            throw new NotImplementedException("Platform.DirRemove: 未找到当前平台的实现。");
        }

        public virtual void FsInit()
        {
            throw new NotImplementedException("Platform.FsInit: 未找到当前平台的实现。");
        }

        public virtual bool FsCheckReady()
        {
            throw new NotImplementedException("Platform.FsCheckReady: 未找到当前平台的实现。");
        }

        public virtual void FsCommit()
        {
            throw new NotImplementedException("Platform.FsCommit: 未找到当前平台的实现。");
        }

        public virtual void SetScreenSize()
        {
            throw new NotImplementedException("Platform.SetScreenSize: 未找到当前平台的实现。");
        }

        public virtual void SetFullscreen(bool enable)
        {
            throw new NotImplementedException("Platform.SetFullscreen: 未找到当前平台的实现。");
        }
    }
}
