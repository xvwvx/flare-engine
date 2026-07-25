// <自动生成> 对应 C++ 源文件：SDLInputState.h + SDLInputState.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using System.Text;
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// SDL2 键盘扫描码常量（基于 SDL 2.0.x），对应 C++ <c>SDL_SCANCODE_*</c> 宏定义。
    /// 保留 SDL2 风格的键码值以保证与引擎其余部分兼容。
    /// </summary>
    public static class SdlScancode
    {
        public const int Unknown = 0;
        public const int A = 4;
        public const int B = 5;
        public const int C = 6;
        public const int D = 7;
        public const int E = 8;
        public const int F = 9;
        public const int Q = 20;
        public const int R = 21;
        public const int S = 22;
        public const int W = 26;
        public const int Digit1 = 30;
        public const int Digit2 = 31;
        public const int Digit3 = 32;
        public const int Digit4 = 33;
        public const int Digit5 = 34;
        public const int Digit6 = 35;
        public const int Slash = 56;
        public const int Backslash = 51;
        public const int F5 = 58;
        public const int F6 = 59;
        public const int F7 = 60;
        public const int F8 = 61;
        public const int J = 13;
        public const int K = 14;
        public const int L = 15;
        public const int M = 16;
        public const int I = 12;
        public const int P = 19;
        public const int V = 25;
        public const int Space = 44;
        public const int Return = 40;
        public const int Escape = 41;
        public const int Backspace = 42;
        public const int Tab = 43;
        public const int Delete = 76;
        public const int PageUp = 75;
        public const int PageDown = 78;
        public const int Up = 82;
        public const int Down = 81;
        public const int Left = 80;
        public const int Right = 79;
        public const int LCtrl = 224;
        public const int LShift = 225;
        public const int LAlt = 226;
        public const int RCtrl = 228;
        public const int RShift = 229;
        public const int RAlt = 230;
        public const int Application = 118;
        public const int AcBack = 278;
    }

    /// <summary>对应 C++ <c>SDL_BUTTON_*</c> 鼠标按键常量。</summary>
    public static class SdlButton
    {
        public const int Left = 1;
        public const int Right = 3;
    }

    /// <summary>对应 C++ <c>SDL_CONTROLLER_BUTTON_*</c> 与 <c>SDL_CONTROLLER_BUTTON_MAX</c>。</summary>
    public static class SdlControllerButton
    {
        public const int A = 0;
        public const int B = 1;
        public const int X = 2;
        public const int Y = 3;
        public const int Back = 4;
        public const int Guide = 5;
        public const int Start = 6;
        public const int Leftstick = 7;
        public const int Rightstick = 8;
        public const int Leftshoulder = 9;
        public const int Rightshoulder = 10;
        public const int DpadUp = 11;
        public const int DpadDown = 12;
        public const int DpadLeft = 13;
        public const int DpadRight = 14;
        public const int Max = 15;
    }

    /// <summary>对应 C++ <c>SDL_CONTROLLER_AXIS_*</c> 与 <c>SDL_CONTROLLER_AXIS_MAX</c>。</summary>
    public static class SdlControllerAxis
    {
        public const int Leftx = 0;
        public const int Lefty = 1;
        public const int Rightx = 2;
        public const int Righty = 3;
        public const int Triggerleft = 4;
        public const int Triggerright = 5;
        public const int Max = 6;
    }

    /// <summary>
    /// SDL2 键盘键值常量（<c>SDL_Keycode</c> / <c>SDLK_*</c>），供 <see cref="SDLInputState.GetKeyName"/> 的 switch 分支使用。
    /// </summary>
    public static class SdlKeycode
    {
        public const int Backspace = 8;
        public const int Tab = 9;
        public const int Return = 13;
        public const int Escape = 27;
        public const int Space = 32;
        public const int Delete = 127;
        public const int Capslock = 1073741881;
        public const int F1 = 1073741882;
        public const int LCtrl = 1073742048;
        public const int LShift = 1073742049;
        public const int LAlt = 1073742050;
        public const int RCtrl = 1073742052;
        public const int RShift = 1073742053;
        public const int RAlt = 1073742054;
        public const int Up = 1073741906;
        public const int Down = 1073741905;
        public const int Right = 1073741903;
        public const int Left = 1073741904;
        public const int Insert = 1073741897;
        public const int Home = 1073741898;
        public const int End = 1073741901;
        public const int PageUp = 1073741899;
        public const int PageDown = 1073741902;
        public const int Numlockclear = 1073741907;
        public const int Printscreen = 1073741894;
        public const int Scrolllock = 1073741893;
        public const int Pause = 1073741896;
    }

    /// <summary>SDL 事件类型常量，对应 SDL 事件枚举值及 UtilsDebug.SdlEventCodes 映射。</summary>
    public static class SdlInputEventType
    {
        public const uint Quit = 0x100;
        public const uint WindowEvent = 0x200;
        public const uint KeyDown = 0x300;
        public const uint KeyUp = 0x301;
        public const uint TextInput = 0x303;
        public const uint MouseMotion = 0x400;
        public const uint MouseButtonDown = 0x401;
        public const uint MouseButtonUp = 0x402;
        public const uint MouseWheel = 0x403;
        public const uint JoyDeviceAdded = 0x605;
        public const uint JoyDeviceRemoved = 0x606;
        public const uint ControllerAxisMotion = 0x650;
        public const uint ControllerButtonDown = 0x651;
        public const uint ControllerButtonUp = 0x652;
        public const uint FingerDown = 0x700;
        public const uint FingerMotion = 0x701;
        public const uint FingerUp = 0x702;
    }

    /// <summary>对应 C++ <c>SDL_WINDOWEVENT_*</c> 窗口事件 ID。</summary>
    public static class SdlWindowEventId
    {
        public const byte SizeChanged = 5;
        public const byte Minimized = 6;
        public const byte Restored = 8;
        public const byte FocusLost = 12;
        public const byte FocusGained = 13;
    }

    /// <summary>对应 C++ <c>SDL_ENABLE</c> / <c>SDL_DISABLE</c>，用于 ShowCursor 控制。</summary>
    public static class SdlCursorVisibility
    {
        public const int Disable = 0;
        public const int Enable = 1;
    }

    /// <summary>对应 C++ <c>SDL_TRUE</c>，用于 GameControllerHasRumble/HasLED 等布尔返回值。</summary>
    public static class SdlBool
    {
        public const int True = 1;
    }

    /// <summary>对应 C++ <c>SDL_TextInputEvent</c>，仅保留 text 字段。</summary>
    public readonly struct SdlTextInputEventData
    {
        public string Text { get; init; }
    }

    /// <summary>对应 C++ <c>SDL_MouseWheelEvent</c>，仅保留 y 滚动量字段。</summary>
    public readonly struct SdlMouseWheelEventData
    {
        public int Y { get; init; }
    }

    /// <summary>对应 C++ <c>SDL_WindowEvent</c>，handle() 中仅使用 event 字段。</summary>
    public readonly struct SdlWindowEventData
    {
        public byte Event { get; init; }
        public int Data1 { get; init; }
        public int Data2 { get; init; }
    }

    /// <summary>对应 C++ <c>SDL_TouchFingerEvent</c>。/summary>
    public readonly struct SdlTouchFingerEventData
    {
        public long FingerId { get; init; }
        public float X { get; init; }
        public float Y { get; init; }
        public float Dx { get; init; }
        public float Dy { get; init; }
    }

    /// <summary>对应 C++ <c>SDL_ControllerButtonEvent</c>。/summary>
    public readonly struct SdlControllerButtonEventData
    {
        public int Which { get; init; }
        public byte Button { get; init; }
    }

    /// <summary>对应 C++ <c>SDL_ControllerAxisEvent</c>。/summary>
    public readonly struct SdlControllerAxisEventData
    {
        public int Which { get; init; }
        public byte Axis { get; init; }
        public short Value { get; init; }
    }

    /// <summary>对应 C++ <c>SDL_JoyDeviceEvent</c>，仅保留 which 字段。</summary>
    public readonly struct SdlJoyDeviceEventData
    {
        public int Which { get; init; }
    }

    /// <summary>
    /// 经 <see cref="ISdlInputService.PollEvent"/> 转换后的通用 SDL 事件结构，对应 C++ <c>SDL_Event</c> union。
    /// 因 C# 不支持 union，改用 <see cref="Type"/> 字段按需读取各子结构。
    /// </summary>
    public struct SdlPollEvent
    {
        public uint Type;
        public SdlTextInputEventData TextInput;
        public SdlWindowEventData Window;
        public SdlMouseMotionEvent Motion;
        public SdlMouseButtonEvent Button;
        public SdlMouseWheelEventData Wheel;
        public SdlKeyboardEvent Key;
        public SdlControllerButtonEventData CButton;
        public SdlControllerAxisEventData CAxis;
        public SdlTouchFingerEventData TFinger;
        public SdlJoyDeviceEventData JDevice;
    }

    /// <summary>
    /// SDL 游戏手柄句柄包装，对应 C++ <c>SDL_GameController*</c>。
    /// 将原生指针封装为托管对象以统一生命周期管理。
    /// </summary>
    public sealed class SdlGameControllerHandle
    {
        /// <summary>SDL3 原生手柄指针，由 <see cref="ISdlInputService"/> 创建时赋值。</summary>
        internal IntPtr NativePtr { get; }

        internal SdlGameControllerHandle(IntPtr nativePtr)
        {
            NativePtr = nativePtr;
        }
    }

    /// <summary>
    /// SDL 输入/手柄/触屏/文本输入 API 抽象接口。所有 SDL 原生调用经此接口注入，
    /// 以便支持 P/Invoke（SDL2）与 SDL3-CS 绑定两种实现。
    /// 具体实例由 output/main.cs 中 <see cref="ISdlApplicationService"/> 初始化时赋值。
    /// </summary>
    public interface ISdlInputService
    {
        bool PollEvent(out SdlPollEvent evt);
        void ShowCursor(int state);
        int NumJoysticks();
        bool IsGameController(int index);
        SdlGameControllerHandle? GameControllerOpen(int joystickIndex);
        void GameControllerClose(SdlGameControllerHandle? gamepad);
        string GameControllerNameForIndex(int index);
        int JoystickInstanceId(SdlGameControllerHandle? gameController);
        int GetKeyFromScancode(int scancode);
        string GetKeyName(int keycode);
        void StartTextInput();
        void StopTextInput();
        int GetScancodeFromName(string name);
        int GameControllerGetButtonFromString(string name);
        int GameControllerGetAxisFromString(string name);
        int GameControllerHasRumble(SdlGameControllerHandle? gamepad);
        void GameControllerRumble(SdlGameControllerHandle? gamepad, ushort lowFreq, ushort highFreq, uint duration);
        int GameControllerHasLed(SdlGameControllerHandle? gamepad);
        void GameControllerSetLed(SdlGameControllerHandle? gamepad, byte r, byte g, byte b);
    }

    /// <summary>
    /// SDLInputState
    ///
    /// 基于 SDL API 的输入状态实现，处理键盘/鼠标/手柄/触屏事件。
    /// 对应 C++ <c>class SDLInputState : public InputState</c>。
    /// </summary>
    public class SDLInputState : InputState
    {
        /// <summary>SDL 输入服务注入点，由 <see cref="ISdlInputService"/> 实现赋值。</summary>
        public static ISdlInputService? Sdl { get; set; }

        private Timer _resizeCooldown;
        private bool _joystickInit;
        private bool _textInput;

        private List<int> _gamepadIds;
        private SdlGameControllerHandle? _gamepad;

        private string[] _xboxButtons;
        private string[] _xboxAxes;

        private List<InputBind> _restrictedBindings;

        /// <summary>对应 C++ <c>SDLInputState::SDLInputState(void)</c> 默认构造函数。</summary>
        public SDLInputState()
        {
            _resizeCooldown = new Timer();
            _joystickInit = false;
            _textInput = false;
            _gamepad = null;

            _gamepadIds = new List<int>();

            _xboxButtons = new string[SdlControllerButton.Max];
            for (int i = 0; i < SdlControllerButton.Max; i++)
                _xboxButtons[i] = "";

            _xboxAxes = new string[SdlControllerAxis.Max * 2];
            for (int i = 0; i < SdlControllerAxis.Max * 2; i++)
                _xboxAxes[i] = "";

            _restrictedBindings = new List<InputBind>();

            Platform.Instance.SetExitEventFilter();

            _restrictedBindings.Add(new InputBind(InputBind.Mouse, SdlButton.Left));
            _restrictedBindings.Add(new InputBind(InputBind.Key, SdlScancode.LCtrl));
            _restrictedBindings.Add(new InputBind(InputBind.Key, SdlScancode.RCtrl));
            _restrictedBindings.Add(new InputBind(InputBind.Key, SdlScancode.LShift));
            _restrictedBindings.Add(new InputBind(InputBind.Key, SdlScancode.RShift));
            _restrictedBindings.Add(new InputBind(InputBind.Key, SdlScancode.LAlt));
            _restrictedBindings.Add(new InputBind(InputBind.Key, SdlScancode.RAlt));
            _restrictedBindings.Add(new InputBind(InputBind.Key, SdlScancode.Delete));
            _restrictedBindings.Add(new InputBind(InputBind.Key, SdlScancode.Backspace));

            InitBindings();

            for (int key = 0; key < KeyCount; key++)
            {
                Pressing[key] = false;
                UnPress[key] = false;
                Lock[key] = false;
            }

            LoadKeyBindings();
            SetCommonStrings();

            for (int i = 0; i < (Sdl?.NumJoysticks() ?? 0); i++)
            {
                if (Sdl != null && Sdl.IsGameController(i))
                {
                    _gamepadIds.Add(i);
                }
            }

            InitJoystick();
        }

        /// <summary>对应 C++ <c>void SDLInputState::setBind(...)</c>。</summary>
        public override void SetBind(int action, int type, int bind, ref string? keybindMsg)
        {
            if (action < 0 || action >= KeyCount)
                return;

            for (int i = 0; i < Binding[action].Count; ++i)
            {
                if (Binding[action][i].Type == type && Binding[action][i].Bind == bind)
                    return;
            }

            if (keybindMsg != null)
            {
                for (int i = 0; i < _restrictedBindings.Count; ++i)
                {
                    if (type == _restrictedBindings[i].Type && bind == _restrictedBindings[i].Bind)
                    {
                        keybindMsg = SharedResources.Msg!.GetV("Can not bind: %s", GetInputBindName(type, bind));
                        return;
                    }
                }

                for (int i = 0; i < KeyCountUser; ++i)
                {
                    for (int j = Binding[i].Count; j > 0; --j)
                    {
                        if (type == Binding[i][j - 1].Type && bind == Binding[i][j - 1].Bind)
                        {
                            keybindMsg = SharedResources.Msg!.GetV("'%s' is no longer bound to:", GetInputBindName(type, bind)) + " '" + BindingName[i] + "'";
                            RemoveBind(i, j - 1);
                        }
                    }
                }
            }

            InputBind inputBind = new InputBind();
            inputBind.Type = type;
            inputBind.Bind = bind;

            Binding[action].Add(inputBind);
        }

        /// <summary>对应 C++ <c>void SDLInputState::removeBind(int action, size_t index)</c>。</summary>
        public override void RemoveBind(int action, int index)
        {
            if (action < 0 || action >= KeyCount)
                return;

            if (index >= Binding[action].Count)
                return;

            Binding[action].RemoveAt(index);
        }

        /// <summary>对应 C++ <c>void SDLInputState::initJoystick()</c>。</summary>
        public override void InitJoystick()
        {
            if (_gamepad != null)
            {
                Sdl?.GameControllerClose(_gamepad);
                _gamepad = null;
            }

            _gamepadIds.Clear();
            for (int i = 0; i < (Sdl?.NumJoysticks() ?? 0); i++)
            {
                if (Sdl != null && Sdl.IsGameController(i))
                {
                    _gamepadIds.Add(i);
                }
            }

            bool enableLogMsg = !_joystickInit || JoysticksChanged;

            if (_gamepadIds.Count == 0)
            {
                if (enableLogMsg)
                {
                    Utils.LogInfo("InputState: No gamepads were found.");
                }
                SharedResources.Settings!.EnableJoystick = false;
            }
            else
            {
                if (enableLogMsg)
                {
                    Utils.LogInfo("InputState: %d gamepad(s) found.", _gamepadIds.Count);
                }
            }

            if (SharedResources.Settings!.JoystickDevice >= _gamepadIds.Count)
                SharedResources.Settings.JoystickDevice = 0;

            for (int i = 0; i < _gamepadIds.Count; ++i)
            {
                if (SharedResources.Settings.EnableJoystick && i == SharedResources.Settings.JoystickDevice)
                {
                    _gamepad = Sdl?.GameControllerOpen(_gamepadIds[i]);
                }
                if (enableLogMsg)
                {
                    if (SharedResources.Settings.EnableJoystick && i == SharedResources.Settings.JoystickDevice)
                    {
                        Utils.LogInfo("InputState: Gamepad #%d, %s [*]", i, Sdl?.GameControllerNameForIndex(i) ?? "");
                    }
                    else
                    {
                        Utils.LogInfo("InputState: Gamepad #%d, %s", i, Sdl?.GameControllerNameForIndex(i) ?? "");
                    }
                }
            }

            SetJoystickLED(DefaultControllerLedColor);
        }

        /// <summary>对应 C++ <c>void SDLInputState::initBindings()</c>。</summary>
        public override void InitBindings()
        {
            for (int key = 0; key < KeyCount; key++)
            {
                Binding[key].Clear();
            }

            string? nullKeybindMsg = null;

            if (Platform.Instance.NeedsAltEscapeKey)
            {
                SetBind(Input.Cancel, InputBind.Key, SdlScancode.Backslash, ref nullKeybindMsg);
            }
            SetBind(Input.Cancel, InputBind.Key, SdlScancode.Escape, ref nullKeybindMsg);

            SetBind(Input.Accept, InputBind.Key, SdlScancode.Return, ref nullKeybindMsg);
            SetBind(Input.Accept, InputBind.Key, SdlScancode.Space, ref nullKeybindMsg);

            if (Platform.Instance.IsMobileDevice)
            {
                SetBind(Input.Cancel, InputBind.Key, SdlScancode.AcBack, ref nullKeybindMsg);
                SetBind(Input.Accept, InputBind.Key, SdlScancode.Application, ref nullKeybindMsg);
            }

            SetBind(Input.Up, InputBind.Key, SdlScancode.W, ref nullKeybindMsg);
            SetBind(Input.Up, InputBind.Key, SdlScancode.Up, ref nullKeybindMsg);

            SetBind(Input.Down, InputBind.Key, SdlScancode.S, ref nullKeybindMsg);
            SetBind(Input.Down, InputBind.Key, SdlScancode.Down, ref nullKeybindMsg);

            SetBind(Input.Left, InputBind.Key, SdlScancode.A, ref nullKeybindMsg);
            SetBind(Input.Left, InputBind.Key, SdlScancode.Left, ref nullKeybindMsg);

            SetBind(Input.Right, InputBind.Key, SdlScancode.D, ref nullKeybindMsg);
            SetBind(Input.Right, InputBind.Key, SdlScancode.Right, ref nullKeybindMsg);

            SetBind(Input.Bar1, InputBind.Key, SdlScancode.Q, ref nullKeybindMsg);
            SetBind(Input.Bar2, InputBind.Key, SdlScancode.E, ref nullKeybindMsg);
            SetBind(Input.Bar3, InputBind.Key, SdlScancode.R, ref nullKeybindMsg);
            SetBind(Input.Bar4, InputBind.Key, SdlScancode.F, ref nullKeybindMsg);
            SetBind(Input.Bar5, InputBind.Key, SdlScancode.Digit1, ref nullKeybindMsg);
            SetBind(Input.Bar6, InputBind.Key, SdlScancode.Digit2, ref nullKeybindMsg);
            SetBind(Input.Bar7, InputBind.Key, SdlScancode.Digit3, ref nullKeybindMsg);
            SetBind(Input.Bar8, InputBind.Key, SdlScancode.Digit4, ref nullKeybindMsg);
            SetBind(Input.Bar9, InputBind.Key, SdlScancode.Digit5, ref nullKeybindMsg);
            SetBind(Input.Bar0, InputBind.Key, SdlScancode.Digit6, ref nullKeybindMsg);

            SetBind(Input.Character, InputBind.Key, SdlScancode.C, ref nullKeybindMsg);
            SetBind(Input.Inventory, InputBind.Key, SdlScancode.I, ref nullKeybindMsg);
            SetBind(Input.Powers, InputBind.Key, SdlScancode.P, ref nullKeybindMsg);
            SetBind(Input.Log, InputBind.Key, SdlScancode.L, ref nullKeybindMsg);

            SetBind(Input.Main1, InputBind.Mouse, SdlButton.Left, ref nullKeybindMsg);
            SetBind(Input.Main2, InputBind.Mouse, SdlButton.Right, ref nullKeybindMsg);

            SetBind(Input.MenuPageNext, InputBind.Key, SdlScancode.PageDown, ref nullKeybindMsg);
            SetBind(Input.MenuPagePrev, InputBind.Key, SdlScancode.PageUp, ref nullKeybindMsg);
            SetBind(Input.MenuActivate, InputBind.Key, SdlScancode.V, ref nullKeybindMsg);

            SetBind(Input.DeveloperMenu, InputBind.Key, SdlScancode.F5, ref nullKeybindMsg);
            SetBind(Input.DeveloperCmd1, InputBind.Key, SdlScancode.F6, ref nullKeybindMsg);
            SetBind(Input.DeveloperCmd2, InputBind.Key, SdlScancode.F7, ref nullKeybindMsg);
            SetBind(Input.DeveloperCmd3, InputBind.Key, SdlScancode.F8, ref nullKeybindMsg);

            SetBind(Input.EquipmentSwap, InputBind.Key, SdlScancode.Tab, ref nullKeybindMsg);
            SetBind(Input.EquipmentSwap, InputBind.Key, SdlScancode.K, ref nullKeybindMsg);
            SetBind(Input.EquipmentSwapPrev, InputBind.Key, SdlScancode.J, ref nullKeybindMsg);
            SetBind(Input.MinimapMode, InputBind.Key, SdlScancode.M, ref nullKeybindMsg);
            SetBind(Input.LootTooltipMode, InputBind.Key, SdlScancode.Slash, ref nullKeybindMsg);
            SetBind(Input.Actionbar, InputBind.Key, SdlScancode.B, ref nullKeybindMsg);

            SetBind(Input.Cancel, InputBind.Gamepad, SdlControllerButton.B, ref nullKeybindMsg);
            SetBind(Input.Accept, InputBind.Gamepad, SdlControllerButton.A, ref nullKeybindMsg);

            SetBind(Input.Right, InputBind.GamepadAxis, (SdlControllerAxis.Leftx * 2), ref nullKeybindMsg);
            SetBind(Input.Down, InputBind.GamepadAxis, (SdlControllerAxis.Lefty * 2), ref nullKeybindMsg);
            SetBind(Input.Left, InputBind.GamepadAxis, (SdlControllerAxis.Leftx * 2) + 1, ref nullKeybindMsg);
            SetBind(Input.Up, InputBind.GamepadAxis, (SdlControllerAxis.Lefty * 2) + 1, ref nullKeybindMsg);

            SetBind(Input.Left, InputBind.Gamepad, SdlControllerButton.DpadLeft, ref nullKeybindMsg);
            SetBind(Input.Right, InputBind.Gamepad, SdlControllerButton.DpadRight, ref nullKeybindMsg);
            SetBind(Input.Up, InputBind.Gamepad, SdlControllerButton.DpadUp, ref nullKeybindMsg);
            SetBind(Input.Down, InputBind.Gamepad, SdlControllerButton.DpadDown, ref nullKeybindMsg);

            SetBind(Input.CycleMenus, InputBind.Gamepad, SdlControllerButton.Back, ref nullKeybindMsg);

            SetBind(Input.Main1, InputBind.GamepadAxis, (SdlControllerAxis.Triggerright * 2), ref nullKeybindMsg);
            SetBind(Input.Main2, InputBind.GamepadAxis, (SdlControllerAxis.Triggerleft * 2), ref nullKeybindMsg);

            SetBind(Input.Actionbar, InputBind.Gamepad, SdlControllerButton.Y, ref nullKeybindMsg);

            SetBind(Input.MenuPageNext, InputBind.Gamepad, SdlControllerButton.Rightshoulder, ref nullKeybindMsg);
            SetBind(Input.MenuPagePrev, InputBind.Gamepad, SdlControllerButton.Leftshoulder, ref nullKeybindMsg);
            SetBind(Input.MenuActivate, InputBind.Gamepad, SdlControllerButton.X, ref nullKeybindMsg);

            SetBind(Input.Pause, InputBind.Gamepad, SdlControllerButton.Start, ref nullKeybindMsg);

            SetBind(Input.AimRight, InputBind.GamepadAxis, (SdlControllerAxis.Rightx * 2), ref nullKeybindMsg);
            SetBind(Input.AimDown, InputBind.GamepadAxis, (SdlControllerAxis.Righty * 2), ref nullKeybindMsg);
            SetBind(Input.AimLeft, InputBind.GamepadAxis, (SdlControllerAxis.Rightx * 2) + 1, ref nullKeybindMsg);
            SetBind(Input.AimUp, InputBind.GamepadAxis, (SdlControllerAxis.Righty * 2) + 1, ref nullKeybindMsg);

            SetBind(Input.Ctrl, InputBind.Key, SdlScancode.LCtrl, ref nullKeybindMsg);
            SetBind(Input.Ctrl, InputBind.Key, SdlScancode.RCtrl, ref nullKeybindMsg);
            SetBind(Input.Shift, InputBind.Key, SdlScancode.LShift, ref nullKeybindMsg);
            SetBind(Input.Shift, InputBind.Key, SdlScancode.RShift, ref nullKeybindMsg);
            SetBind(Input.Alt, InputBind.Key, SdlScancode.LAlt, ref nullKeybindMsg);
            SetBind(Input.Alt, InputBind.Key, SdlScancode.RAlt, ref nullKeybindMsg);
            SetBind(Input.Del, InputBind.Key, SdlScancode.Delete, ref nullKeybindMsg);
            SetBind(Input.Del, InputBind.Key, SdlScancode.Backspace, ref nullKeybindMsg);
            SetBind(Input.TexteditUp, InputBind.Key, SdlScancode.Up, ref nullKeybindMsg);
            SetBind(Input.TexteditDown, InputBind.Key, SdlScancode.Down, ref nullKeybindMsg);
        }

        /// <summary>对应 C++ <c>void SDLInputState::handle()</c>。</summary>
        public override void Handle()
        {
            base.Handle();

            SdlPollEvent evt;

            while (Sdl != null && Sdl.PollEvent(out evt))
            {
                if (DumpEvent)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.WriteDebug(ToDebugSdlEvent(evt));
                    Console.WriteLine(sb.ToString());
                }

                if (evt.Type == SdlInputEventType.TextInput)
                {
                    Inkeys += evt.TextInput.Text;
                }

                switch (evt.Type)
                {
                    case SdlInputEventType.MouseMotion:
                        if (Mode != ModeTouchscreen)
                        {
                            Mode = ModeKeyboardAndMouse;
                            Mouse = ScaleMouse((uint)evt.Motion.X, (uint)evt.Motion.Y);
                            SharedResources.Curs!.ShowCursor = true;
                        }
                        break;
                    case SdlInputEventType.MouseWheel:
                        if (Mode != ModeTouchscreen)
                        {
                            Mode = ModeKeyboardAndMouse;
                            if (evt.Wheel.Y > 0)
                            {
                                ScrollUp = true;
                            }
                            else if (evt.Wheel.Y < 0)
                            {
                                ScrollDown = true;
                            }
                        }
                        break;
                    case SdlInputEventType.MouseButtonDown:
                        if (Mode == ModeTouchscreen)
                        {
                            Mode = ModeKeyboardAndMouse;
                            Mouse = ScaleMouse((uint)evt.Button.X, (uint)evt.Button.Y);
                            SharedResources.Curs!.ShowCursor = true;
                        }
                        else
                        {
                            Mode = ModeKeyboardAndMouse;
                            Mouse = ScaleMouse((uint)evt.Button.X, (uint)evt.Button.Y);
                            SharedResources.Curs!.ShowCursor = true;
                            for (int key = 0; key < KeyCount; key++)
                            {
                                for (int i = 0; i < Binding[key].Count; ++i)
                                {
                                    if (Binding[key][i].Type == InputBind.Mouse && Binding[key][i].Bind == evt.Button.Button)
                                    {
                                        Pressing[key] = true;
                                        UnPress[key] = false;
                                    }
                                }
                            }
                        }
                        break;
                    case SdlInputEventType.MouseButtonUp:
                        Mouse = ScaleMouse((uint)evt.Button.X, (uint)evt.Button.Y);
                        SharedResources.Curs!.ShowCursor = true;
                        for (int key = 0; key < KeyCount; key++)
                        {
                            for (int i = 0; i < Binding[key].Count; ++i)
                            {
                                if (Binding[key][i].Type == InputBind.Mouse && Binding[key][i].Bind == evt.Button.Button)
                                {
                                    UnPress[key] = true;
                                }
                            }
                        }
                        LastButton = evt.Button.Button;
                        break;
                    case SdlInputEventType.WindowEvent:
                        if (evt.Window.Event == SdlWindowEventId.SizeChanged)
                        {
                            _resizeCooldown.Duration = (uint)(SharedResources.Settings!.MaxFramesPerSec / 4);
                        }
                        else if (evt.Window.Event == SdlWindowEventId.Minimized)
                        {
                            if (Platform.Instance.IsMobileDevice)
                            {
                                Utils.LogInfo("InputState: Minimizing app, saving...");
                                SharedResources.SaveLoad!.SaveGame();
                                Utils.LogInfo("InputState: Game saved");
                            }
                            WindowMinimized = true;
                            SharedResources.Snd!.PauseAll();
                            if (SharedGameResources.Menu != null)
                                SharedGameResources.Menu.ShowExitMenu();
                        }
                        else if (evt.Window.Event == SdlWindowEventId.Restored)
                        {
                            WindowRestored = true;
                            SharedResources.Snd!.ResumeAll();
                        }
                        else if (evt.Window.Event == SdlWindowEventId.FocusLost)
                        {
                            if (SharedResources.Settings!.PauseOnFocusLoss && SharedGameResources.Menu != null)
                            {
                                SharedGameResources.Menu.ShowExitMenu();
                            }
                            if (SharedResources.Settings.MuteOnFocusLoss)
                            {
                                SharedResources.Snd!.SetVolumeSFX(0);
                                SharedResources.Snd!.SetVolumeMusic(0);
                            }
                        }
                        else if (evt.Window.Event == SdlWindowEventId.FocusGained)
                        {
                            if (SharedResources.Settings!.MuteOnFocusLoss)
                            {
                                SharedResources.Snd!.SetVolumeSFX(SharedResources.Settings.SoundVolume);
                                SharedResources.Snd!.SetVolumeMusic(SharedResources.Settings.MusicVolume);
                            }
                        }
                        break;
                    case SdlInputEventType.FingerMotion:
                        if (SharedResources.Settings!.Touchscreen)
                        {
                            Mode = ModeTouchscreen;
                            SharedResources.Curs!.ShowCursor = false;
                            Mouse.X = (int)((evt.TFinger.X + evt.TFinger.Dx) * SharedResources.Settings.ViewW);
                            Mouse.Y = (int)((evt.TFinger.Y + evt.TFinger.Dy) * SharedResources.Settings.ViewH);
                            Pressing[Input.Main1] = true;
                            UnPress[Input.Main1] = false;

                            if (evt.TFinger.Dy > 0.005f)
                            {
                                ScrollUp = true;
                            }
                            else if (evt.TFinger.Dy < -0.005f)
                            {
                                ScrollDown = true;
                            }

                            for (int i = 0; i < TouchFingers.Count; ++i)
                            {
                                if (TouchFingers[i].Id == evt.TFinger.FingerId)
                                {
                                    TouchFingers[i].Pos.X = Mouse.X;
                                    TouchFingers[i].Pos.Y = Mouse.Y;
                                }
                            }
                        }
                        break;
                    case SdlInputEventType.FingerDown:
                        if (SharedResources.Settings!.Touchscreen)
                        {
                            Mode = ModeTouchscreen;
                            SharedResources.Curs!.ShowCursor = false;
                            TouchLocked = true;
                            Mouse.X = (int)(evt.TFinger.X * SharedResources.Settings.ViewW);
                            Mouse.Y = (int)(evt.TFinger.Y * SharedResources.Settings.ViewH);
                            Pressing[Input.Main1] = true;
                            UnPress[Input.Main1] = false;

                            FingerData fd = new FingerData();
                            fd.Id = evt.TFinger.FingerId;
                            fd.Pos.X = Mouse.X;
                            fd.Pos.Y = Mouse.Y;
                            TouchFingers.Add(fd);
                        }
                        break;
                    case SdlInputEventType.FingerUp:
                        if (SharedResources.Settings!.Touchscreen)
                        {
                            UnPress[Input.Main1] = false;

                            SharedResources.Curs!.ShowCursor = false;
                            for (int i = 0; i < TouchFingers.Count; ++i)
                            {
                                if (TouchFingers[i].Id == evt.TFinger.FingerId)
                                {
                                    TouchFingers.RemoveAt(i);
                                    break;
                                }
                            }
                            if (TouchFingers.Count == 0)
                            {
                                TouchLocked = false;
                                UnPress[Input.Main1] = true;
                                for (int i = 0; i < Binding[Input.Main1].Count; ++i)
                                {
                                    if (Binding[Input.Main1][i].Type == InputBind.Mouse)
                                    {
                                        LastButton = Binding[Input.Main1][i].Bind;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                Mouse.X = TouchFingers[TouchFingers.Count - 1].Pos.X;
                                Mouse.Y = TouchFingers[TouchFingers.Count - 1].Pos.Y;
                            }
                        }
                        break;
                    case SdlInputEventType.KeyDown:
                        Mode = ModeKeyboardAndMouse;

                        for (int key = 0; key < KeyCount; key++)
                        {
                            for (int i = 0; i < Binding[key].Count; ++i)
                            {
                                if (Binding[key][i].Type == InputBind.Key && Binding[key][i].Bind == evt.Key.Keysym.Scancode)
                                {
                                    Pressing[key] = true;
                                    UnPress[key] = false;
                                }
                            }
                        }
                        break;
                    case SdlInputEventType.KeyUp:
                        Mode = ModeKeyboardAndMouse;

                        for (int key = 0; key < KeyCount; key++)
                        {
                            for (int i = 0; i < Binding[key].Count; ++i)
                            {
                                if (Binding[key][i].Type == InputBind.Key && Binding[key][i].Bind == evt.Key.Keysym.Scancode)
                                {
                                    UnPress[key] = true;
                                }
                            }
                        }

                        LastKey = evt.Key.Keysym.Scancode;
                        break;
                    case SdlInputEventType.ControllerButtonDown:
                    {
                        if (SharedResources.Settings!.EnableJoystick && _gamepad != null)
                        {
                            Mode = ModeJoystick;

                            int joyId = Sdl!.JoystickInstanceId(_gamepad);
                            if (joyId == evt.CButton.Which)
                            {
                                for (int key = 0; key < KeyCount; key++)
                                {
                                    for (int i = 0; i < Binding[key].Count; ++i)
                                    {
                                        if (Binding[key][i].Type == InputBind.Gamepad && Binding[key][i].Bind == evt.CButton.Button)
                                        {
                                            SharedResources.Curs!.ShowCursor = false;
                                            HideCursor();
                                            Pressing[key] = true;
                                            UnPress[key] = false;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    }
                    case SdlInputEventType.ControllerButtonUp:
                    {
                        if (SharedResources.Settings!.EnableJoystick && _gamepad != null)
                        {
                            Mode = ModeJoystick;

                            int joyId = Sdl!.JoystickInstanceId(_gamepad);
                            if (joyId == evt.CButton.Which)
                            {
                                for (int key = 0; key < KeyCount; key++)
                                {
                                    for (int i = 0; i < Binding[key].Count; ++i)
                                    {
                                        if (Binding[key][i].Type == InputBind.Gamepad && Binding[key][i].Bind == evt.CButton.Button)
                                        {
                                            UnPress[key] = true;
                                        }
                                    }
                                }
                                LastJoybutton = evt.CButton.Button;
                            }
                        }
                        break;
                    }
                    case SdlInputEventType.ControllerAxisMotion:
                    {
                        if (SharedResources.Settings!.EnableJoystick && _gamepad != null)
                        {
                            LastJoyaxis = -1;

                            int joyId = Sdl!.JoystickInstanceId(_gamepad);
                            if (joyId == evt.CAxis.Which)
                            {
                                for (int key = 0; key < KeyCount; key++)
                                {
                                    for (int i = 0; i < Binding[key].Count; ++i)
                                    {
                                        int bindAxis = Binding[key][i].Bind / 2;
                                        if (Binding[key][i].Type == InputBind.GamepadAxis && bindAxis == evt.CAxis.Axis)
                                        {
                                            bool isDown;
                                            if (bindAxis == SdlControllerAxis.Triggerleft || bindAxis == SdlControllerAxis.Triggerright)
                                            {
                                                isDown = (evt.CAxis.Value >= SharedResources.Settings.JoyDeadzone);
                                            }
                                            else if (Binding[key][i].Bind % 2 == 0)
                                            {
                                                isDown = (evt.CAxis.Value >= SharedResources.Settings.JoyDeadzone);
                                            }
                                            else
                                            {
                                                isDown = (evt.CAxis.Value <= -(SharedResources.Settings.JoyDeadzone));
                                            }

                                            if (isDown)
                                            {
                                                Mode = ModeJoystick;
                                                SharedResources.Curs!.ShowCursor = false;
                                                HideCursor();
                                                Pressing[key] = true;
                                                UnPress[key] = false;
                                                PressAxis[key] = true;
                                            }
                                            else if (Pressing[key] && Mode == ModeJoystick && PressAxis[key])
                                            {
                                                UnPress[key] = true;
                                                PressAxis[key] = false;
                                            }
                                        }
                                    }
                                }
                                if (evt.CAxis.Value >= SharedResources.Settings.JoyDeadzone)
                                    LastJoyaxis = evt.CAxis.Axis * 2;
                                else if (evt.CAxis.Value <= -(SharedResources.Settings.JoyDeadzone))
                                    LastJoyaxis = (evt.CAxis.Axis * 2) + 1;
                            }
                        }
                        break;
                    }
                    case SdlInputEventType.JoyDeviceAdded:
                        if (!_joystickInit)
                        {
                            _joystickInit = true;
                        }
                        else
                        {
                            Utils.LogInfo("InputState: Joystick added.");
                            JoysticksChanged = true;

                            if (SharedResources.Settings!.JoystickDevice == 0)
                            {
                                SharedResources.Settings.EnableJoystick = true;
                                SharedResources.Settings.JoystickDevice = evt.JDevice.Which;
                            }

                            InitJoystick();
                        }
                        break;
                    case SdlInputEventType.JoyDeviceRemoved:
                        Utils.LogInfo("InputState: Joystick removed.");
                        JoysticksChanged = true;
                        InitJoystick();
                        break;
                    case SdlInputEventType.Quit:
                        Done = true;
                        for (int key = 0; key < KeyCount; key++)
                        {
                            Pressing[key] = false;
                            UnPress[key] = false;
                            Lock[key] = false;
                        }
                        break;
                    default:
                        break;
                }
            }

            if (_resizeCooldown.Duration > 0)
            {
                _resizeCooldown.Tick();

                if (_resizeCooldown.IsEnd())
                {
                    _resizeCooldown.Duration = 0;
                    WindowResized = true;
                    SharedResources.RenderDevice!.WindowResize();
                }
            }

            if (!_joystickInit)
                _joystickInit = true;

            for (int i = 0; i < KeyCount; i++)
            {
                if (SlowRepeat[i])
                {
                    if (!Pressing[i])
                    {
                        RepeatCooldown[i].Duration = SharedResources.Settings!.MaxFramesPerSec;
                    }
                    else if (Pressing[i] && !Lock[i])
                    {
                        Lock[i] = true;
                        int prevDuration = (int)RepeatCooldown[i].Duration;
                        RepeatCooldown[i].Duration = (uint)Math.Max(SharedResources.Settings.MaxFramesPerSec / 10, prevDuration - (SharedResources.Settings.MaxFramesPerSec / 2));
                    }
                    else if (Pressing[i] && Lock[i])
                    {
                        RepeatCooldown[i].Tick();

                        if (RepeatCooldown[i].IsEnd())
                        {
                            Lock[i] = false;
                        }
                    }
                }
            }

            if (!UsingTouchscreen())
                TouchLocked = false;
        }

        /// <summary>对应 C++ <c>void SDLInputState::hideCursor()</c>。</summary>
        public override void HideCursor()
        {
            Sdl?.ShowCursor(SdlCursorVisibility.Disable);
        }

        /// <summary>对应 C++ <c>void SDLInputState::showCursor()</c>。</summary>
        public override void ShowCursor()
        {
            Sdl?.ShowCursor(SdlCursorVisibility.Enable);
        }

        /// <summary>对应 C++ <c>std::string SDLInputState::getJoystickName(int index)</c>。</summary>
        public override string GetJoystickName(int index)
        {
            return Sdl?.GameControllerNameForIndex(index) ?? "";
        }

        /// <summary>对应 C++ <c>std::string SDLInputState::getKeyName(int key, bool get_short_string)</c>。</summary>
        public string GetKeyName(int key, bool getShortString = !GetShortString)
        {
            key = Sdl?.GetKeyFromScancode(key) ?? key;

            if (getShortString)
            {
                switch (key)
                {
                    case SdlKeycode.Backspace: return SharedResources.Msg!.Get("BkSp");
                    case SdlKeycode.Capslock: return SharedResources.Msg!.Get("Caps");
                    case SdlKeycode.Delete: return SharedResources.Msg!.Get("Del");
                    case SdlKeycode.Down: return SharedResources.Msg!.Get("Down");
                    case SdlKeycode.End: return SharedResources.Msg!.Get("End");
                    case SdlKeycode.Escape: return SharedResources.Msg!.Get("Esc");
                    case SdlKeycode.Home: return SharedResources.Msg!.Get("Home");
                    case SdlKeycode.Insert: return SharedResources.Msg!.Get("Ins");
                    case SdlKeycode.LAlt: return SharedResources.Msg!.Get("LAlt");
                    case SdlKeycode.LCtrl: return SharedResources.Msg!.Get("LCtrl");
                    case SdlKeycode.Left: return SharedResources.Msg!.Get("Left");
                    case SdlKeycode.LShift: return SharedResources.Msg!.Get("LShft");
                    case SdlKeycode.Numlockclear: return SharedResources.Msg!.Get("Num");
                    case SdlKeycode.PageDown: return SharedResources.Msg!.Get("PgDn");
                    case SdlKeycode.PageUp: return SharedResources.Msg!.Get("PgUp");
                    case SdlKeycode.Pause: return SharedResources.Msg!.Get("Pause");
                    case SdlKeycode.Printscreen: return SharedResources.Msg!.Get("Print");
                    case SdlKeycode.RAlt: return SharedResources.Msg!.Get("RAlt");
                    case SdlKeycode.RCtrl: return SharedResources.Msg!.Get("RCtrl");
                    case SdlKeycode.Return: return SharedResources.Msg!.Get("Ret");
                    case SdlKeycode.Right: return SharedResources.Msg!.Get("Right");
                    case SdlKeycode.RShift: return SharedResources.Msg!.Get("RShft");
                    case SdlKeycode.Scrolllock: return SharedResources.Msg!.Get("SLock");
                    case SdlKeycode.Space: return SharedResources.Msg!.Get("Spc");
                    case SdlKeycode.Tab: return SharedResources.Msg!.Get("Tab");
                    case SdlKeycode.Up: return SharedResources.Msg!.Get("Up");
                }
            }
            else
            {
                switch (key)
                {
                    case SdlKeycode.Backspace: return SharedResources.Msg!.Get("Backspace");
                    case SdlKeycode.Capslock: return SharedResources.Msg!.Get("CapsLock");
                    case SdlKeycode.Delete: return SharedResources.Msg!.Get("Delete");
                    case SdlKeycode.Down: return SharedResources.Msg!.Get("Down");
                    case SdlKeycode.End: return SharedResources.Msg!.Get("End");
                    case SdlKeycode.Escape: return SharedResources.Msg!.Get("Escape");
                    case SdlKeycode.Home: return SharedResources.Msg!.Get("Home");
                    case SdlKeycode.Insert: return SharedResources.Msg!.Get("Insert");
                    case SdlKeycode.LAlt: return SharedResources.Msg!.Get("Left Alt");
                    case SdlKeycode.LCtrl: return SharedResources.Msg!.Get("Left Ctrl");
                    case SdlKeycode.Left: return SharedResources.Msg!.Get("Left");
                    case SdlKeycode.LShift: return SharedResources.Msg!.Get("Left Shift");
                    case SdlKeycode.Numlockclear: return SharedResources.Msg!.Get("NumLock");
                    case SdlKeycode.PageDown: return SharedResources.Msg!.Get("PageDown");
                    case SdlKeycode.PageUp: return SharedResources.Msg!.Get("PageUp");
                    case SdlKeycode.Pause: return SharedResources.Msg!.Get("Pause");
                    case SdlKeycode.Printscreen: return SharedResources.Msg!.Get("PrintScreen");
                    case SdlKeycode.RAlt: return SharedResources.Msg!.Get("Right Alt");
                    case SdlKeycode.RCtrl: return SharedResources.Msg!.Get("Right Ctrl");
                    case SdlKeycode.Return: return SharedResources.Msg!.Get("Return");
                    case SdlKeycode.Right: return SharedResources.Msg!.Get("Right");
                    case SdlKeycode.RShift: return SharedResources.Msg!.Get("Right Shift");
                    case SdlKeycode.Scrolllock: return SharedResources.Msg!.Get("ScrollLock");
                    case SdlKeycode.Space: return SharedResources.Msg!.Get("Space");
                    case SdlKeycode.Tab: return SharedResources.Msg!.Get("Tab");
                    case SdlKeycode.Up: return SharedResources.Msg!.Get("Up");
                }
            }

            return Sdl?.GetKeyName(key) ?? "";
        }

        /// <summary>对应 C++ <c>std::string SDLInputState::getMouseButtonName(int button, bool get_short_string)</c>。</summary>
        public string GetMouseButtonName(int button, bool getShortString = !GetShortString)
        {
            if (getShortString)
            {
                return SharedResources.Msg!.GetV("M%d", button);
            }
            else
            {
                if (button > 0 && button <= MouseButtonNameCount)
                    return MouseButton[button - 1];
                else
                    return SharedResources.Msg!.GetV("Mouse %d", button);
            }
        }

        /// <summary>对应 C++ <c>std::string SDLInputState::getJoystickButtonName(int button)</c>。</summary>
        public string GetJoystickButtonName(int button)
        {
            if (button < SdlControllerButton.Max)
            {
                return _xboxButtons[button];
            }
            else
            {
                return SharedResources.Msg!.Get("(unknown)");
            }
        }

        /// <summary>对应 C++ <c>std::string SDLInputState::getJoystickAxisName(int axis)</c>。</summary>
        public string GetJoystickAxisName(int axis)
        {
            if (axis < SdlControllerAxis.Max * 2)
            {
                return _xboxAxes[axis];
            }
            else
            {
                return SharedResources.Msg!.Get("(unknown)");
            }
        }

        /// <summary>对应 C++ <c>std::string SDLInputState::getBindingString(int key, bool get_short_string)</c>。</summary>
        public override string GetBindingString(int key, bool getShortString = !GetShortString)
        {
            return GetBindingStringByIndex(key, -1, getShortString);
        }

        /// <summary>对应 C++ <c>std::string SDLInputState::getBindingStringByIndex(...)</c>。</summary>
        public override string GetBindingStringByIndex(int key, int bindingIndex, bool getShortString = !GetShortString)
        {
            string none = "";
            if (!getShortString)
                none = SharedResources.Msg!.Get("(none)");

            if (Binding[key].Count == 0)
            {
                return none;
            }

            int bi = 0;
            if (bindingIndex != -1 && bindingIndex < Binding[key].Count)
                bi = bindingIndex;

            if (Binding[key][bi].Type == InputBind.Key)
                return GetKeyName(Binding[key][bi].Bind, getShortString);
            else if (Binding[key][bi].Type == InputBind.Mouse)
                return GetMouseButtonName(Binding[key][bi].Bind, getShortString);
            else if (Binding[key][bi].Type == InputBind.Gamepad)
                return GetJoystickButtonName(Binding[key][bi].Bind);
            else if (Binding[key][bi].Type == InputBind.GamepadAxis)
                return GetJoystickAxisName(Binding[key][bi].Bind);
            else
                return none;
        }

        /// <summary>对应 C++ <c>std::string SDLInputState::getGamepadBindingString(...)</c>。</summary>
        public override string GetGamepadBindingString(int key, bool getShortString = !GetShortString)
        {
            string none = "";
            if (!getShortString)
                none = SharedResources.Msg!.Get("(none)");

            if (Binding[key].Count == 0)
            {
                return none;
            }

            for (int i = 0; i < Binding[key].Count; ++i)
            {
                if (Binding[key][i].Type == InputBind.Gamepad)
                    return GetJoystickButtonName(Binding[key][i].Bind);
                else if (Binding[key][i].Type == InputBind.GamepadAxis)
                    return GetJoystickAxisName(Binding[key][i].Bind);
            }

            return none;
        }

        /// <summary>对应 C++ <c>std::string SDLInputState::getMovementString()</c>。</summary>
        public override string GetMovementString()
        {
            string output = "[";

            if (SharedResources.Inpt!.UsingTouchscreen())
            {
                output += SharedResources.Msg!.Get("Touch control D-Pad");
            }
            else if (SharedResources.Settings!.EnableJoystick)
            {
                output += GetGamepadBindingString(Input.Left) + "/";
                output += GetGamepadBindingString(Input.Right) + "/";
                output += GetGamepadBindingString(Input.Up) + "/";
                output += GetGamepadBindingString(Input.Down);
            }
            else if (SharedResources.Settings.MouseMove)
            {
                output += (SharedResources.Settings.MouseMoveSwap ? GetBindingString(Input.Main2) : GetBindingString(Input.Main1));
            }
            else
            {
                output += GetBindingString(Input.Left) + "/";
                output += GetBindingString(Input.Right) + "/";
                output += GetBindingString(Input.Up) + "/";
                output += GetBindingString(Input.Down);
            }

            output += "]";
            return output;
        }

        /// <summary>对应 C++ <c>std::string SDLInputState::getAttackString()</c>。</summary>
        public override string GetAttackString()
        {
            string output = "[";

            if (SharedResources.Inpt!.UsingTouchscreen())
            {
                output += SharedResources.Msg!.Get("Touch control buttons");
            }
            else
            {
                output += GetBindingString(Input.Main1);
            }

            output += "]";
            return output;
        }

        /// <summary>对应 C++ <c>int SDLInputState::getNumJoysticks()</c>。</summary>
        public override int GetNumJoysticks()
        {
            return _gamepadIds.Count;
        }

        /// <summary>对应 C++ <c>bool SDLInputState::usingMouse()</c>。</summary>
        public override bool UsingMouse()
        {
            return !SharedResources.Settings!.NoMouse && Mode != ModeJoystick;
        }

        /// <summary>对应 C++ <c>bool SDLInputState::usingTouchscreen()</c>。</summary>
        public override bool UsingTouchscreen()
        {
            return SharedResources.Settings!.Touchscreen && Mode == ModeTouchscreen;
        }

        /// <summary>对应 C++ <c>void SDLInputState::startTextInput()</c>。</summary>
        public override void StartTextInput()
        {
            if (!_textInput)
            {
                Sdl?.StartTextInput();
                _textInput = true;
            }
        }

        /// <summary>对应 C++ <c>void SDLInputState::stopTextInput()</c>。</summary>
        public override void StopTextInput()
        {
            if (_textInput)
            {
                Sdl?.StopTextInput();
                _textInput = false;
            }
        }

        /// <summary>对应 C++ <c>int SDLInputState::getBindFromString(...)</c>，解析键位绑定的字符串表示。</summary>
        protected override int GetBindFromString(string bind, int type)
        {
            if (bind == "-1")
                return -1;

            if (type == InputBind.Mouse)
                return Parse.ToInt(bind);

            string temp = bind;
            if (Parse.PopFirstString(ref temp, ':') == "SDL")
            {
                if (type == InputBind.Key)
                {
                    return Sdl?.GetScancodeFromName(temp) ?? -1;
                }
                else if (type == InputBind.Gamepad)
                {
                    return Sdl?.GameControllerGetButtonFromString(temp) ?? -1;
                }
                else if (type == InputBind.GamepadAxis)
                {
                    string axisName = Parse.PopFirstString(ref temp, ':');
                    int axis = (Sdl?.GameControllerGetAxisFromString(axisName) ?? -1) * 2;

                    if (temp == "-")
                        axis += 1;

                    return axis;
                }
            }

            return Parse.ToInt(bind);
        }

        /// <summary>对应 C++ <c>void SDLInputState::setCommonStrings()</c>。</summary>
        public override void SetCommonStrings()
        {
            BindingName[Input.Cancel] = SharedResources.Msg!.Get("Cancel");
            BindingName[Input.Accept] = SharedResources.Msg!.Get("Accept");
            BindingName[Input.Up] = SharedResources.Msg!.Get("Up");
            BindingName[Input.Down] = SharedResources.Msg!.Get("Down");
            BindingName[Input.Left] = SharedResources.Msg!.Get("Left");
            BindingName[Input.Right] = SharedResources.Msg!.Get("Right");
            BindingName[Input.Bar1] = SharedResources.Msg!.Get("Bar1");
            BindingName[Input.Bar2] = SharedResources.Msg!.Get("Bar2");
            BindingName[Input.Bar3] = SharedResources.Msg!.Get("Bar3");
            BindingName[Input.Bar4] = SharedResources.Msg!.Get("Bar4");
            BindingName[Input.Bar5] = SharedResources.Msg!.Get("Bar5");
            BindingName[Input.Bar6] = SharedResources.Msg!.Get("Bar6");
            BindingName[Input.Bar7] = SharedResources.Msg!.Get("Bar7");
            BindingName[Input.Bar8] = SharedResources.Msg!.Get("Bar8");
            BindingName[Input.Bar9] = SharedResources.Msg!.Get("Bar9");
            BindingName[Input.Bar0] = SharedResources.Msg!.Get("Bar0");
            BindingName[Input.Character] = SharedResources.Msg!.Get("Character");
            BindingName[Input.Inventory] = SharedResources.Msg!.Get("Inventory");
            BindingName[Input.Powers] = SharedResources.Msg!.Get("Powers");
            BindingName[Input.Log] = SharedResources.Msg!.Get("Log");
            BindingName[Input.Main1] = SharedResources.Msg!.Get("Main1");
            BindingName[Input.Main2] = SharedResources.Msg!.Get("Main2");
            BindingName[Input.EquipmentSwap] = SharedResources.Msg!.Get("Next Equip Set");
            BindingName[Input.EquipmentSwapPrev] = SharedResources.Msg!.Get("Previous Equip Set");
            BindingName[Input.MinimapMode] = SharedResources.Msg!.Get("Mini-map Mode");
            BindingName[Input.LootTooltipMode] = SharedResources.Msg!.Get("Loot Tooltip Mode");
            BindingName[Input.Actionbar] = SharedResources.Msg!.Get("Action Bar Edit");
            BindingName[Input.MenuPageNext] = SharedResources.Msg!.Get("Menu: Next Page");
            BindingName[Input.MenuPagePrev] = SharedResources.Msg!.Get("Menu: Previous Page");
            BindingName[Input.MenuActivate] = SharedResources.Msg!.Get("Menu: Activate");
            BindingName[Input.Pause] = SharedResources.Msg!.Get("Pause Game");
            BindingName[Input.CycleMenus] = SharedResources.Msg!.Get("Cycle Menus");
            BindingName[Input.AimUp] = SharedResources.Msg!.Get("Aim Up");
            BindingName[Input.AimDown] = SharedResources.Msg!.Get("Aim Down");
            BindingName[Input.AimLeft] = SharedResources.Msg!.Get("Aim Left");
            BindingName[Input.AimRight] = SharedResources.Msg!.Get("Aim Right");
            BindingName[Input.DeveloperMenu] = SharedResources.Msg!.Get("Developer Menu");
            BindingName[Input.DeveloperCmd1] = SharedResources.Msg!.Get("Developer Command 1");
            BindingName[Input.DeveloperCmd2] = SharedResources.Msg!.Get("Developer Command 2");
            BindingName[Input.DeveloperCmd3] = SharedResources.Msg!.Get("Developer Command 3");
            BindingName[Input.Ctrl] = SharedResources.Msg!.Get("Ctrl");
            BindingName[Input.Shift] = SharedResources.Msg!.Get("Shift");
            BindingName[Input.Alt] = SharedResources.Msg!.Get("Alt");
            BindingName[Input.Del] = SharedResources.Msg!.Get("Delete");

            MouseButton[0] = SharedResources.Msg!.Get("Left Mouse");
            MouseButton[1] = SharedResources.Msg!.Get("Middle Mouse");
            MouseButton[2] = SharedResources.Msg!.Get("Right Mouse");
            MouseButton[3] = SharedResources.Msg!.Get("Wheel Up");
            MouseButton[4] = SharedResources.Msg!.Get("Wheel Down");
            MouseButton[5] = SharedResources.Msg!.Get("Mouse X1");
            MouseButton[6] = SharedResources.Msg!.Get("Mouse X2");

            _xboxButtons[SdlControllerButton.A] = SharedResources.Msg!.Get("X360: A");
            _xboxButtons[SdlControllerButton.B] = SharedResources.Msg!.Get("X360: B");
            _xboxButtons[SdlControllerButton.X] = SharedResources.Msg!.Get("X360: X");
            _xboxButtons[SdlControllerButton.Y] = SharedResources.Msg!.Get("X360: Y");
            _xboxButtons[SdlControllerButton.Back] = SharedResources.Msg!.Get("X360: Back");
            _xboxButtons[SdlControllerButton.Guide] = SharedResources.Msg!.Get("X360: Guide");
            _xboxButtons[SdlControllerButton.Start] = SharedResources.Msg!.Get("X360: Start");
            _xboxButtons[SdlControllerButton.Leftstick] = SharedResources.Msg!.Get("X360: L3");
            _xboxButtons[SdlControllerButton.Rightstick] = SharedResources.Msg!.Get("X360: R3");
            _xboxButtons[SdlControllerButton.Leftshoulder] = SharedResources.Msg!.Get("X360: L1");
            _xboxButtons[SdlControllerButton.Rightshoulder] = SharedResources.Msg!.Get("X360: R1");
            _xboxButtons[SdlControllerButton.DpadUp] = SharedResources.Msg!.Get("X360: D-Up");
            _xboxButtons[SdlControllerButton.DpadDown] = SharedResources.Msg!.Get("X360: D-Down");
            _xboxButtons[SdlControllerButton.DpadLeft] = SharedResources.Msg!.Get("X360: D-Left");
            _xboxButtons[SdlControllerButton.DpadRight] = SharedResources.Msg!.Get("X360: D-Right");

            _xboxAxes[(SdlControllerAxis.Leftx * 2)] = SharedResources.Msg!.Get("X360: Left X+");
            _xboxAxes[(SdlControllerAxis.Leftx * 2) + 1] = SharedResources.Msg!.Get("X360: Left X-");
            _xboxAxes[(SdlControllerAxis.Lefty * 2)] = SharedResources.Msg!.Get("X360: Left Y+");
            _xboxAxes[(SdlControllerAxis.Lefty * 2) + 1] = SharedResources.Msg!.Get("X360: Left Y-");
            _xboxAxes[(SdlControllerAxis.Rightx * 2)] = SharedResources.Msg!.Get("X360: Right X+");
            _xboxAxes[(SdlControllerAxis.Rightx * 2) + 1] = SharedResources.Msg!.Get("X360: Right X-");
            _xboxAxes[(SdlControllerAxis.Righty * 2)] = SharedResources.Msg!.Get("X360: Right Y+");
            _xboxAxes[(SdlControllerAxis.Righty * 2) + 1] = SharedResources.Msg!.Get("X360: Right Y-");
            _xboxAxes[(SdlControllerAxis.Triggerleft * 2)] = SharedResources.Msg!.Get("X360: L2");
            _xboxAxes[(SdlControllerAxis.Triggerright * 2)] = SharedResources.Msg!.Get("X360: R2");
        }

        private string GetInputBindName(int type, int bind)
        {
            if (type == InputBind.Key)
            {
                return GetKeyName(bind);
            }
            else if (type == InputBind.Mouse)
            {
                return GetMouseButtonName(bind);
            }
            else if (type == InputBind.Gamepad)
            {
                return GetJoystickButtonName(bind);
            }
            else if (type == InputBind.GamepadAxis)
            {
                return GetJoystickAxisName(bind);
            }
            return "";
        }

        /// <summary>
        /// 手柄震动。对应 C++ SDL_VERSION_ATLEAST(2,0,18) 条件块中的 SDL_GameControllerRumble,
        /// 由 ISdlInputService 注入实现，低版本 SDL 退化为 no-op。
        /// </summary>
        public override void JoystickRumble(ushort lowFreq, ushort highFreq, uint duration)
        {
            if (_gamepad != null && Mode == ModeJoystick && SharedResources.Settings!.JoystickRumble && Sdl != null && Sdl.GameControllerHasRumble(_gamepad) == SdlBool.True)
            {
                Sdl.GameControllerRumble(_gamepad, lowFreq, highFreq, duration);
            }
        }

        /// <summary>
        /// 手柄 LED 控制。对应 C++ SDL_VERSION_ATLEAST(2,0,14) 条件块中的 SDL_GameControllerSetLED。
        /// 模式与 JoystickRumble 类似。
        /// </summary>
        public override void SetJoystickLED(Color color)
        {
            if (_gamepad != null && Mode == ModeJoystick && Sdl != null && Sdl.GameControllerHasLed(_gamepad) == SdlBool.True)
            {
                Sdl.GameControllerSetLed(_gamepad, color.R, color.G, color.B);
            }
        }

        /// <summary>对应 C++ <c>void SDLInputState::reset()</c>。</summary>
        public override void Reset()
        {
            for (int i = 0; i < KeyCount; ++i)
            {
                if (i >= Input.Up && i <= Input.Right)
                {
                    if (Pressing[i] && !Lock[i])
                        continue;
                }

                Pressing[i] = false;
                Lock[i] = false;
                UnPress[i] = false;
                PressAxis[i] = false;
            }
        }

        /// <summary>
        /// 对应 C++ 析构函数 SDLInputState::~SDLInputState()，关闭 gamepad 句柄并释放资源。
        /// </summary>
        public override void Dispose()
        {
            if (_gamepad != null)
                Sdl?.GameControllerClose(_gamepad);
            base.Dispose();
        }

        /// <summary>
        /// 将 SdlPollEvent 转为 SdlEvent 调试格式，供 UtilsDebug.WriteDebug 输出。
        /// 对应 C++ operator&lt;&lt;(SDL_Event)。未知类型输出 "Unknown event: ..."。
        /// </summary>
        private static SdlEvent ToDebugSdlEvent(SdlPollEvent src)
        {
            SdlEvent evt = new SdlEvent { Type = src.Type };
            switch (src.Type)
            {
                case SdlInputEventType.WindowEvent:
                    evt = new SdlEvent
                    {
                        Type = src.Type,
                        Window = new SdlWindowEvent { Data1 = src.Window.Data1, Data2 = src.Window.Data2 }
                    };
                    break;
                case SdlInputEventType.KeyDown:
                case SdlInputEventType.KeyUp:
                    evt = new SdlEvent { Type = src.Type, Key = src.Key };
                    break;
                case SdlInputEventType.MouseMotion:
                    evt = new SdlEvent { Type = src.Type, Motion = src.Motion };
                    break;
                case SdlInputEventType.MouseButtonDown:
                case SdlInputEventType.MouseButtonUp:
                    evt = new SdlEvent { Type = src.Type, Button = src.Button };
                    break;
                case SdlInputEventType.Quit:
                    evt = new SdlEvent { Type = src.Type, Quit = default };
                    break;
            }
            return evt;
        }
    }
}
