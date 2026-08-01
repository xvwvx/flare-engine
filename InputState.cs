// <自动生成> 对应 C++ 源文件：InputState.h + InputState.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using System.IO; // StreamWriter/IOException；项目 csproj 中 ImplicitUsings 为 disable，需显式引入
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// Input
    ///
    /// 对应 C++ 源文件中 <c>namespace Input</c> 里的匿名 enum（键位动作 id 列表）。
    /// 沿用本次迁移中"namespace 内的匿名枚举常量 -&gt; static class 内的 const int"的统一约定
    /// （与 Utils.Align*、Platform.Video/Audio/Game/Interface/Input/Misc 等既有转换保持一致），
    /// 并与 output/Widget.cs、output/WidgetSlider.cs 中已经使用的 <c>Input.Up</c>/<c>Input.Down</c>/
    /// <c>Input.Left</c>/<c>Input.Right</c>/<c>Input.Accept</c>/<c>Input.Main1</c> 等前向引用符号完全对齐。
    /// 各常量显式写出数值（而非依赖 C# 编译器的自动递增），以确保与原始 C++ 匿名 enum 从 0 开始
    /// 逐一递增的取值完全一致，不依赖声明顺序。
    /// </summary>
    public static class Input
    {
        public const int Cancel = 0;
        public const int Accept = 1;
        public const int Up = 2;
        public const int Down = 3;
        public const int Left = 4;
        public const int Right = 5;
        public const int Bar1 = 6;
        public const int Bar2 = 7;
        public const int Bar3 = 8;
        public const int Bar4 = 9;
        public const int Bar5 = 10;
        public const int Bar6 = 11;
        public const int Bar7 = 12;
        public const int Bar8 = 13;
        public const int Bar9 = 14;
        public const int Bar0 = 15;
        public const int Character = 16;
        public const int Inventory = 17;
        public const int Powers = 18;
        public const int Log = 19;
        public const int Main1 = 20;
        public const int Main2 = 21;
        public const int EquipmentSwap = 22;
        public const int EquipmentSwapPrev = 23;
        public const int MinimapMode = 24;
        public const int LootTooltipMode = 25;
        public const int Actionbar = 26;
        public const int MenuPageNext = 27;
        public const int MenuPagePrev = 28;
        public const int MenuActivate = 29;
        public const int Pause = 30;
        public const int CycleMenus = 31;
        public const int AimUp = 32;
        public const int AimDown = 33;
        public const int AimLeft = 34;
        public const int AimRight = 35;
        public const int DeveloperMenu = 36;
        public const int DeveloperCmd1 = 37;
        public const int DeveloperCmd2 = 38;
        public const int DeveloperCmd3 = 39;

        // non-modifiable
        public const int Ctrl = 40;
        public const int Shift = 41;
        public const int Alt = 42;
        public const int Del = 43;
        public const int TexteditUp = 44;
        public const int TexteditDown = 45;

        public const int KeyCount = 46;
    }

    /// <summary>
    /// InputBind
    ///
    /// 对应 C++ 的 <c>class InputBind</c>：单条按键/按钮绑定记录（绑定的输入源类型 + 具体键值）。
    /// 原始类的 <c>type</c>/<c>bind</c> 是无逻辑的公有数据成员（并非 getter/setter 方法），
    /// 按仓库既有约定（详见 output/Widget.cs、output/Settings.cs 等）保留为公有字段而非属性。
    /// </summary>
    public class InputBind
    {
        public const int Key = 0;
        public const int Mouse = 1;
        public const int Gamepad = 2;
        public const int GamepadAxis = 3;

        public int Type;
        public int Bind;

        /// <summary>对应 C++ <c>InputBind(int _type = -1, int _bind = -1)</c> 构造函数的成员初始化列表。</summary>
        public InputBind(int type = -1, int bind = -1)
        {
            Type = type;
            Bind = bind;
        }

        // 对应 C++ 析构函数 ~InputBind()：原实现为空函数体，不释放任何资源，
        // 因此本类型无需实现终结器或 IDisposable。
    }

    /// <summary>
    /// InputState
    ///
    /// 处理键盘和鼠标状态。对应 C++ 的抽象基类 <c>class InputState</c>：其中带有 <c>= 0</c> 的
    /// 纯虚函数在 C++ 源码中没有函数体（由派生类 SDLInputState 提供实现，属于另一个独立逻辑单元，
    /// 本批次不处理），因此在 C# 中对应地映射为 <c>abstract</c> 方法声明——这不属于规则 12 所禁止的
    /// "未完成代码/占位符"，而是对纯虚函数本身"无函数体、留给派生类实现"这一语言语义的忠实翻译。
    /// 唯一带函数体的虚函数 <c>handle()</c> 映射为 <c>virtual</c> 方法并保留其完整实现。
    /// </summary>
    public abstract class InputState : IDisposable
    {
        // some mouse buttons are named (e.g. "Left Mouse")
        protected const int MouseButtonNameCount = 7;

        public const bool GetShortString = true;
        public const int KeyCount = Input.KeyCount;
        public const int KeyCountUser = KeyCount - 6; // exclude CTRL, SHIFT, etc from keybinding menu

        /// <summary>
        /// 对应 C++ <c>static const Color DEFAULT_CONTROLLER_LED_COLOR;</c>，定义处为 <c>Color(255, 200, 40)</c>。
        /// 原始 3 参数构造函数会将 alpha 隐式设为 255（与 output/GameSwitcher.cs、output/FontEngine.cs 等
        /// 既有转换约定一致），Stride.Core.Mathematics.Color 无此隐式重载，故显式写出第 4 个 alpha 参数。
        /// </summary>
        public static readonly Color DefaultControllerLedColor = new Color(255, 200, 40, 255);

        public const ushort JoystickRumbleStrength = 32767;

        public const bool LoadUserBinds = true;

        public const int ModeKeyboardAndMouse = 0;
        public const int ModeJoystick = 1;
        public const int ModeTouchscreen = 2;

        /// <summary>对应 C++ <c>std::vector&lt;InputBind&gt; binding[KEY_COUNT];</c>。</summary>
        public List<InputBind>[] Binding;

        /// <summary>对应 C++ <c>std::string binding_name[KEY_COUNT];</c>。</summary>
        public string[] BindingName;

        /// <summary>对应 C++ <c>std::string mouse_button[MOUSE_BUTTON_NAME_COUNT];</c>。</summary>
        public string[] MouseButton;

        public bool[] Pressing;
        public bool[] Lock;

        // handle repeating keys, such as when holding Backspace to delete text in WidgetInput
        public bool[] SlowRepeat;
        public Timer[] RepeatCooldown;

        public bool Done;
        public Int2 Mouse;
        public string Inkeys;
        public int LastKey;
        public int LastButton;
        public int LastJoybutton;
        public int LastJoyaxis;
        public uint Mode;
        public bool ScrollUp;
        public bool ScrollDown;
        public bool LockScroll;
        public bool TouchLocked;
        public bool LockAll;
        public bool WindowMinimized;
        public bool WindowRestored;
        public bool WindowResized;
        public bool JoysticksChanged;
        public bool RefreshHotkeys;

        /// <summary>
        /// FingerData
        ///
        /// 对应 C++ 嵌套类 <c>class FingerData</c>（声明在 InputState.h 的 protected 区段内，
        /// 其自身成员为 public）。C# 中以 <c>protected</c> 嵌套类型表达同样的"仅本类及派生类可见"的可见性。
        /// </summary>
        protected class FingerData
        {
            public long Id;
            public Int2 Pos;
        }

        protected bool[] UnPress;
        protected bool[] PressAxis;
        protected Int2 CurrentTouch;
        protected bool DumpEvent;

        protected List<FingerData> TouchFingers;

        /// <summary>对应 C++ <c>Version* file_version;</c>（构造时 <c>new Version()</c>，析构时 <c>delete</c>）。</summary>
        protected Version FileVersion;

        /// <summary>对应 C++ <c>Version* file_version_min;</c>（构造时 <c>new Version(1, 12, 90)</c>）。</summary>
        protected Version FileVersionMin;

        protected string[] ConfigKeys;

        /// <summary>
        /// 对应 C++ 构造函数 <c>InputState::InputState(void)</c> 的成员初始化列表与函数体。
        /// 语言层面必要适配：C++ 中 <c>std::vector&lt;T&gt; binding[KEY_COUNT]</c>/
        /// <c>std::string binding_name[KEY_COUNT]</c>/<c>Timer repeat_cooldown[KEY_COUNT]</c> 等数组，
        /// 其每个元素都会被自动默认构造（空 vector / 空字符串 / duration=0 的 Timer）；
        /// C# 的引用类型数组只会将每个元素初始化为 null，因此需要显式循环为每个元素分配实例，
        /// 以复现原始"数组内每个元素均已就地默认构造"的语义，不属于逻辑改动。
        /// </summary>
        protected InputState()
        {
            Binding = new List<InputBind>[KeyCount];
            for (int i = 0; i < KeyCount; i++)
                Binding[i] = new List<InputBind>();

            Pressing = new bool[KeyCount];
            Lock = new bool[KeyCount];
            SlowRepeat = new bool[KeyCount];
            RepeatCooldown = new Timer[KeyCount];
            for (int i = 0; i < KeyCount; i++)
                RepeatCooldown[i] = new Timer();

            Done = false;
            Mouse = default;
            LastKey = -1;
            LastButton = -1;
            LastJoybutton = -1;
            LastJoyaxis = -1;
            Mode = ModeKeyboardAndMouse;
            ScrollUp = false;
            ScrollDown = false;
            LockScroll = false;
            TouchLocked = false;
            LockAll = false;
            WindowMinimized = false;
            WindowRestored = false;
            WindowResized = false;
            JoysticksChanged = false;
            RefreshHotkeys = false;

            UnPress = new bool[KeyCount];
            PressAxis = new bool[KeyCount];
            CurrentTouch = default;
            DumpEvent = false;

            FileVersion = new Version();
            FileVersionMin = new Version(1, 12, 90);

            // 未出现在 C++ 初始化列表中的成员：binding_name/mouse_button/touch_fingers/config_keys
            // 在 C++ 中同样是数组内每个元素被隐式默认构造（空字符串/空 vector），此处显式补齐，
            // 以保证与原始默认构造语义一致（详见本方法上方摘要）。
            BindingName = new string[KeyCount];
            for (int i = 0; i < KeyCount; i++)
                BindingName[i] = "";

            MouseButton = new string[MouseButtonNameCount];
            for (int i = 0; i < MouseButtonNameCount; i++)
                MouseButton[i] = "";

            TouchFingers = new List<FingerData>();

            Inkeys = "";

            ConfigKeys = new string[KeyCountUser];
            for (int i = 0; i < KeyCountUser; i++)
                ConfigKeys[i] = "";

            ConfigKeys[Input.Cancel] = "cancel";
            ConfigKeys[Input.Accept] = "accept";
            ConfigKeys[Input.Up] = "up";
            ConfigKeys[Input.Down] = "down";
            ConfigKeys[Input.Left] = "left";
            ConfigKeys[Input.Right] = "right";
            ConfigKeys[Input.Bar1] = "bar1";
            ConfigKeys[Input.Bar2] = "bar2";
            ConfigKeys[Input.Bar3] = "bar3";
            ConfigKeys[Input.Bar4] = "bar4";
            ConfigKeys[Input.Bar5] = "bar5";
            ConfigKeys[Input.Bar6] = "bar6";
            ConfigKeys[Input.Bar7] = "bar7";
            ConfigKeys[Input.Bar8] = "bar8";
            ConfigKeys[Input.Bar9] = "bar9";
            ConfigKeys[Input.Bar0] = "bar0";
            ConfigKeys[Input.Main1] = "main1";
            ConfigKeys[Input.Main2] = "main2";
            ConfigKeys[Input.Character] = "character";
            ConfigKeys[Input.Inventory] = "inventory";
            ConfigKeys[Input.Powers] = "powers";
            ConfigKeys[Input.Log] = "log";
            ConfigKeys[Input.EquipmentSwap] = "equipment_swap";
            ConfigKeys[Input.EquipmentSwapPrev] = "equipment_swap_prev";
            ConfigKeys[Input.MinimapMode] = "minimap_mode";
            ConfigKeys[Input.LootTooltipMode] = "loot_tooltip_mode";
            ConfigKeys[Input.Actionbar] = "actionbar";
            ConfigKeys[Input.MenuPageNext] = "menu_page_next";
            ConfigKeys[Input.MenuPagePrev] = "menu_page_prev";
            ConfigKeys[Input.MenuActivate] = "menu_activate";
            ConfigKeys[Input.Pause] = "pause";
            ConfigKeys[Input.CycleMenus] = "cycle_menus";
            ConfigKeys[Input.AimUp] = "aim_up";
            ConfigKeys[Input.AimDown] = "aim_down";
            ConfigKeys[Input.AimLeft] = "aim_left";
            ConfigKeys[Input.AimRight] = "aim_right";
            ConfigKeys[Input.DeveloperMenu] = "developer_menu";
            ConfigKeys[Input.DeveloperCmd1] = "developer_cmd_1";
            ConfigKeys[Input.DeveloperCmd2] = "developer_cmd_2";
            ConfigKeys[Input.DeveloperCmd3] = "developer_cmd_3";
        }

        /// <summary>
        /// 对应 C++ 虚析构函数 <c>InputState::~InputState()</c>。函数体仅记录清理日志，
        /// 未持有需要手动释放的非托管资源（<c>file_version</c>/<c>file_version_min</c> 均为
        /// 纯托管对象，交由 GC 回收即可）。声明为 <c>virtual</c> 以匹配原始虚析构函数语义，
        /// 供派生类（如 SDLInputState）在其 <c>override</c> 中先释放自身资源、再调用
        /// <c>base.Dispose()</c>，对应 C++ "派生类析构函数体执行完毕后自动调用基类析构函数"的顺序。
        /// </summary>
        public virtual void Dispose()
        {
            Utils.LogInfo("Cleaning up: InputState");
            GC.SuppressFinalize(this);
        }

        public abstract void SetBind(int action, int type, int bind, ref string? keybindMsg);
        public abstract void RemoveBind(int action, int index);

        public abstract void InitJoystick();

        public abstract void SetCommonStrings();

        /// <summary>
        /// Key bindings are found in config/keybindings.txt
        /// 对应 C++ <c>void InputState::loadKeyBindings(bool load_user_binds)</c>。
        /// </summary>
        public void LoadKeyBindings(bool loadUserBinds = LoadUserBinds)
        {
            var mods = SharedResources.Mods!;
            var settings = SharedResources.Settings!;
            var eset = SharedResources.Eset!;

            using FileParser infile = new FileParser();
            bool openedFile = false;
            bool cleanupPathUser = false;
            // 语言层面必要适配：C++ 的 `*file_version = VersionInfo::MIN;` 是"值拷贝赋值"
            // （拷贝 VersionInfo::MIN 的字段值到 *file_version 指向的对象，不改变指针本身，
            // 也不影响 VersionInfo::MIN 本身）。C# 的 Version 是引用类型且没有拷贝赋值运算符，
            // 若直接写 `FileVersion = VersionInfo.Min;` 会让 FileVersion 与共享的静态实例
            // VersionInfo.Min 产生别名，后续对 FileVersion 的修改（如 SetFromString）会污染
            // VersionInfo.Min。因此逐字段拷贝以精确复现原始"值拷贝"语义。
            FileVersion.X = VersionInfo.Min.X;
            FileVersion.Y = VersionInfo.Min.Y;
            FileVersion.Z = VersionInfo.Min.Z;

            // first check for mod keybinds
            if (mods.Locate("engine/default_keybindings.txt") != "")
            {
                if (loadUserBinds && settings.Game == "" && infile.Open(settings.PathUser + "saves/" + eset.Misc.SavePrefix + "/keybindings.txt", !FileParser.ModFile, FileParser.ErrorNone))
                {
                    openedFile = true;
                }
                else if (loadUserBinds && settings.Game != "" && infile.Open(settings.PathConf + "keybindings.txt", !FileParser.ModFile, FileParser.ErrorNone))
                {
                    openedFile = true;
                    cleanupPathUser = true;
                }
                else if (infile.Open("engine/default_keybindings.txt", FileParser.ModFile, FileParser.ErrorNone))
                {
                    openedFile = true;
                }
            }
            else
            {
                // if there are no mod keybinds, fall back to global config
                if (loadUserBinds && infile.Open(settings.PathConf + "keybindings.txt", !FileParser.ModFile, FileParser.ErrorNone))
                {
                    openedFile = true;
                }

                cleanupPathUser = true;
            }

            if (cleanupPathUser)
            {
                // clean up mod keybinds if engine/default_keybindings.txt is not present
                if (Filesystem.FileExists(settings.PathUser + "saves/" + eset.Misc.SavePrefix + "/keybindings.txt"))
                {
                    Utils.LogInfo("InputState: Found unexpected save prefix keybinding file. Removing it now.");
                    Filesystem.RemoveFile(settings.PathUser + "saves/" + eset.Misc.SavePrefix + "/keybindings.txt");
                }
            }

            if (!openedFile)
            {
                SaveKeyBindings();
                return;
            }

            while (infile.Next())
            {
                if (infile.Section == "")
                {
                    if (infile.Key == "file_version")
                        FileVersion.SetFromString(infile.Val);

                    if (FileVersion < FileVersionMin)
                    {
                        Utils.LogError("InputState: Keybindings configuration file is out of date (%s < %s). Resetting to engine defaults.", FileVersion.GetString(), FileVersionMin.GetString());
                        if (Platform.Instance.ConfigMenuType != Platform.ConfigMenuTypeBase)
                        {
                            Utils.LogErrorDialog("InputState: Keybindings configuration file is out of date. Resetting to engine defaults.", FileVersion.GetString(), FileVersionMin.GetString());
                        }
                        SaveKeyBindings();
                        break;
                    }

                    continue;
                }

                if (infile.NewSection && infile.Section == "user")
                {
                    for (int key = 0; key < KeyCountUser; ++key)
                    {
                        Binding[key].Clear();
                    }
                }

                // @CLASS InputState: Default Keybindings|Description of engine/default_keybindings.txt. Use a bind value of '-1' to clear all bindings for an action. Type may be any of the follwing: 0 = Keyboard, 1 = Mouse, 2 = Gamepad button, 3 = Gamepad Axis. Human-readable key and gamepad mapping names may be used by prefixing the bind with "SDL:" (e.g. "SDL:space" or "SDL:leftstick"). If using the "SDL:" prefix for a gamepad axis, add ":-" to the end of the bind to get the negative direction (e.g. "SDL:leftx:-").
                // @ATTR default.cancel|[int, string], int : Bind, Type|Bindings for "Cancel".
                // @ATTR default.accept|[int, string], int : Bind, Type|Bindings for "Accept".
                // @ATTR default.up|[int, string], int : Bind, Type|Bindings for "Up".
                // @ATTR default.down|[int, string], int : Bind, Type|Bindings for "Down".
                // @ATTR default.left|[int, string], int : Bind, Type|Bindings for "Left".
                // @ATTR default.right|[int, string], int : Bind, Type|Bindings for "Right".
                // @ATTR default.bar1|[int, string], int : Bind, Type|Bindings for "Bar1".
                // @ATTR default.bar2|[int, string], int : Bind, Type|Bindings for "Bar2".
                // @ATTR default.bar3|[int, string], int : Bind, Type|Bindings for "Bar3".
                // @ATTR default.bar4|[int, string], int : Bind, Type|Bindings for "Bar4".
                // @ATTR default.bar5|[int, string], int : Bind, Type|Bindings for "Bar5".
                // @ATTR default.bar6|[int, string], int : Bind, Type|Bindings for "Bar6".
                // @ATTR default.bar7|[int, string], int : Bind, Type|Bindings for "Bar7".
                // @ATTR default.bar8|[int, string], int : Bind, Type|Bindings for "Bar8".
                // @ATTR default.bar9|[int, string], int : Bind, Type|Bindings for "Bar9".
                // @ATTR default.bar0|[int, string], int : Bind, Type|Bindings for "Bar0".
                // @ATTR default.main1|[int, string], int : Bind, Type|Bindings for "Main1".
                // @ATTR default.main2|[int, string], int : Bind, Type|Bindings for "Main2".
                // @ATTR default.character|[int, string], int : Bind, Type|Bindings for "Character".
                // @ATTR default.inventory|[int, string], int : Bind, Type|Bindings for "Inventory".
                // @ATTR default.powers|[int, string], int : Bind, Type|Bindings for "Powers".
                // @ATTR default.log|[int, string], int : Bind, Type|Bindings for "Log".
                // @ATTR default.equipment_swap|[int, string], int : Bind, Type|Bindings for "Next Equip Set".
                // @ATTR default.equipment_swap_prev|[int, string], int : Bind, Type|Bindings for "Previous Equip Set".
                // @ATTR default.minimap_mode|[int, string], int : Bind, Type|Bindings for "Mini-map Mode".
                // @ATTR default.loot_tooltip_mode|[int, string], int : Bind, Type|Bindings for "Loot Tooltip Mode".
                // @ATTR default.actionbar|[int, string], int : Bind, Type|Bindings for "Action Bar".
                // @ATTR default.menu_page_next|[int, string], int : Bind, Type|Bindings for "Menu: Next Page".
                // @ATTR default.menu_page_prev|[int, string], int : Bind, Type|Bindings for "Menu: Previous Page".
                // @ATTR default.menu_activate|[int, string], int : Bind, Type|Bindings for "Menu: Activate".
                // @ATTR default.pause|[int, string], int : Bind, Type|Bindings for "Pause Game".
                // @ATTR default.cycle_menus|[int, string], int : Bind, Type|Bindings for "Cycle Menus".
                // @ATTR default.aim_up|[int, string], int : Bind, Type|Bindings for "Aim Up".
                // @ATTR default.aim_down|[int, string], int : Bind, Type|Bindings for "Aim Down".
                // @ATTR default.aim_left|[int, string], int : Bind, Type|Bindings for "Aim Left".
                // @ATTR default.aim_right|[int, string], int : Bind, Type|Bindings for "Aim Right".
                // @ATTR default.developer_menu|[int, string], int : Bind, Type|Bindings for "Developer Menu".
                // @ATTR default.developer_cmd_1|[int, string], int : Bind, Type|Bindings for "Developer Command 1".
                // @ATTR default.developer_cmd_2|[int, string], int : Bind, Type|Bindings for "Developer Command 2".
                // @ATTR default.developer_cmd_3|[int, string], int : Bind, Type|Bindings for "Developer Command 3".

                if (infile.Section == "user" || infile.Section == "default")
                {
                    for (int key = 0; key < KeyCountUser; ++key)
                    {
                        if (infile.Key == ConfigKeys[key])
                        {
                            string bind = Parse.PopFirstString(ref infile.Val);
                            int type = Parse.PopFirstInt(ref infile.Val);

                            InputBind inputBind = new InputBind();
                            inputBind.Type = type;

                            inputBind.Bind = GetBindFromString(bind, inputBind.Type);

                            if (inputBind.Bind == -1)
                            {
                                Binding[key].Clear();
                            }
                            else
                            {
                                Binding[key].Add(inputBind);
                            }
                        }
                    }
                }
            }
            infile.Close();
        }

        /// <summary>
        /// Write current key bindings to config file
        /// 对应 C++ <c>void InputState::saveKeyBindings()</c>。
        /// 语言层面必要适配：原始代码区分"文件打开失败"（<c>outfile.is_open()</c> 为 false，
        /// 整段逻辑静默跳过，不记录任何日志）与"写入失败"（<c>outfile.bad()</c> 为 true，
        /// 记录一条错误日志，但仍继续执行 close/clear/FSCommit）两种情况。C# 的 StreamWriter
        /// 在打开路径非法/无权限时会在构造时立即抛出 IOException，而写入错误在缓冲区被刷新
        /// （Flush/Dispose）之前不一定会立即抛出；因此这里显式调用一次 <c>Flush()</c>
        /// 把潜在的写入期 IOException 提前到与原始 <c>outfile.bad()</c> 检查点等价的位置，
        /// 使"打开失败静默跳过、写入失败记录日志但仍继续 FSCommit"这一分支结构与原始代码保持一致。
        /// </summary>
        public void SaveKeyBindings()
        {
            var mods = SharedResources.Mods!;
            var settings = SharedResources.Settings!;
            var eset = SharedResources.Eset!;

            string outPath;
            if (mods.Locate("engine/default_keybindings.txt") != "")
            {
                if (settings.Game == "")
                {
                    Filesystem.CreateDir(settings.PathUser + "saves/" + eset.Misc.SavePrefix);
                    outPath = settings.PathUser + "saves/" + eset.Misc.SavePrefix + "/keybindings.txt";
                }
                else
                {
                    outPath = settings.PathConf + "keybindings.txt";
                }
            }
            else
            {
                outPath = settings.PathConf + "keybindings.txt";
            }

            StreamWriter? outfile;
            try
            {
                outfile = new StreamWriter(outPath, append: false);
            }
            catch (IOException)
            {
                outfile = null;
            }

            if (outfile != null)
            {
                try
                {
                    outfile.Write("# Keybindings\n");
                    outfile.Write("# FORMAT: {ACTION}={BIND},{TYPE}\n");
                    outfile.Write("# A bind value of -1 means unbound and will clear any existing bindings for that action\n");
                    outfile.Write("# Type may be any of the follwing: 0 = Keyboard, 1 = Mouse, 2 = Gamepad button, 3 = Gamepad Axis.\n");
                    outfile.Write("# Human-readable key and gamepad mapping names may be used by prefixing the bind with \"SDL:\" (e.g. \"SDL:space\" or \"SDL:leftstick\").\n");
                    outfile.Write("# If using the \"SDL:\" prefix for a gamepad axis, add \":-\" to the end of the bind to get the negative direction (e.g. \"SDL:leftx:-\").\n\n");

                    // 对应 `*file_version = *file_version_min;`：逐字段值拷贝（原因同 LoadKeyBindings 顶部注释）。
                    FileVersion.X = FileVersionMin.X;
                    FileVersion.Y = FileVersionMin.Y;
                    FileVersion.Z = FileVersionMin.Z;
                    outfile.Write("file_version=" + FileVersion.GetString() + "\n\n");

                    outfile.Write("[user]\n");
                    for (int key = 0; key < KeyCountUser; ++key)
                    {
                        if (Binding[key].Count == 0)
                        {
                            outfile.Write(ConfigKeys[key] + "=-1\n");
                        }
                        else
                        {
                            for (int i = 0; i < Binding[key].Count; ++i)
                            {
                                outfile.Write(ConfigKeys[key] + "=" + Binding[key][i].Bind + "," + Binding[key][i].Type + "\n");
                            }
                        }
                    }

                    outfile.Flush();
                }
                catch (IOException)
                {
                    Utils.LogError("InputState: Unable to write keybindings config file. No write access or disk is full!");
                }

                outfile.Dispose();

                Platform.Instance.FsCommit();
            }
        }

        /// <summary>对应 C++ <c>virtual void InputState::handle();</c>（有默认实现的虚函数）。</summary>
        public virtual void Handle()
        {
            RefreshHotkeys = false;

            if (LockAll) return;

            Inkeys = "";

            // sometimes buttons are pressed and released in a single event window.
            // in order to properly read these events in game logic, we delay the
            // resetting of their states (done here) until the next frame. this
            // loop also resets the states of other inputs that are no longer being
            // pressed. (joysticks are a little more complex.)
            for (int key = 0; key < KeyCount; key++)
            {
                if (UnPress[key] == true)
                {
                    Pressing[key] = false;
                    UnPress[key] = false;
                    Lock[key] = false;
                }
            }
        }

        public abstract void InitBindings();
        public abstract void HideCursor();
        public abstract void ShowCursor();
        public abstract string GetJoystickName(int index);
        public abstract string GetBindingString(int key, bool getShortString = !GetShortString);
        public abstract string GetBindingStringByIndex(int key, int bindingIndex, bool getShortString = !GetShortString);
        public abstract string GetGamepadBindingString(int key, bool getShortString = !GetShortString);
        public abstract string GetMovementString();
        public abstract string GetAttackString();
        public abstract int GetNumJoysticks();
        public abstract bool UsingMouse();
        public abstract bool UsingTouchscreen();
        public abstract void StartTextInput();
        public abstract void StopTextInput();

        /// <summary>对应 C++ <c>void InputState::enableEventLog();</c>。</summary>
        public void EnableEventLog()
        {
            DumpEvent = true;
        }

        public abstract void JoystickRumble(ushort lowFreq, ushort highFreq, uint duration);
        public abstract void SetJoystickLED(Color color);

        public abstract void Reset();

        /// <summary>对应 C++ <c>void InputState::resetScroll();</c>。</summary>
        public void ResetScroll()
        {
            ScrollUp = false;
            ScrollDown = false;
        }

        /// <summary>对应 C++ <c>void InputState::lockActionBar();</c>。</summary>
        public void LockActionBar()
        {
            Pressing[Input.Bar1] = false;
            Pressing[Input.Bar2] = false;
            Pressing[Input.Bar3] = false;
            Pressing[Input.Bar4] = false;
            Pressing[Input.Bar5] = false;
            Pressing[Input.Bar6] = false;
            Pressing[Input.Bar7] = false;
            Pressing[Input.Bar8] = false;
            Pressing[Input.Bar9] = false;
            Pressing[Input.Bar0] = false;
            Pressing[Input.Main1] = false;
            Pressing[Input.Main2] = false;
            Pressing[Input.MenuActivate] = false;
            Lock[Input.Bar1] = true;
            Lock[Input.Bar2] = true;
            Lock[Input.Bar3] = true;
            Lock[Input.Bar4] = true;
            Lock[Input.Bar5] = true;
            Lock[Input.Bar6] = true;
            Lock[Input.Bar7] = true;
            Lock[Input.Bar8] = true;
            Lock[Input.Bar9] = true;
            Lock[Input.Bar0] = true;
            Lock[Input.Main1] = true;
            Lock[Input.Main2] = true;
            Lock[Input.MenuActivate] = true;
        }

        /// <summary>对应 C++ <c>void InputState::unlockActionBar();</c>。</summary>
        public void UnlockActionBar()
        {
            Lock[Input.Bar1] = false;
            Lock[Input.Bar2] = false;
            Lock[Input.Bar3] = false;
            Lock[Input.Bar4] = false;
            Lock[Input.Bar5] = false;
            Lock[Input.Bar6] = false;
            Lock[Input.Bar7] = false;
            Lock[Input.Bar8] = false;
            Lock[Input.Bar9] = false;
            Lock[Input.Bar0] = false;
            Lock[Input.Main1] = false;
            Lock[Input.Main2] = false;
            Lock[Input.MenuActivate] = false;
        }

        /// <summary>
        /// 对应 C++ <c>Point InputState::scaleMouse(unsigned int x, unsigned int y);</c>。
        /// unsigned int -&gt; uint，与原始参数类型精确对应（规则表：size_t 才优先映射为 int，
        /// 此处是 unsigned int，直接映射为 uint 更贴近原始精度）。
        /// </summary>
        protected Int2 ScaleMouse(uint x, uint y)
        {
            var settings = SharedResources.Settings!;

            if (settings.MouseScaled)
            {
                return new Int2((int)x, (int)y);
            }

            Int2 scaledMouse = default;
            int offsetY = (int)(((settings.ScreenH - settings.ViewH / settings.ViewScaling) / 2) * settings.ViewScaling);

            scaledMouse.X = (int)((float)x * settings.ViewScaling);
            scaledMouse.Y = (int)((float)y * settings.ViewScaling) - offsetY;

            return scaledMouse;
        }

        protected abstract int GetBindFromString(string bind, int type);
    }
}
