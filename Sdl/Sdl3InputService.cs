using SDL3;

namespace FlareEngine.Sdl
{
    /// <summary>
    /// 对应 C++ <c>SDLInputState</c> 中直接调用的 SDL 输入 API。
    /// 事件轮询在 <see cref="SDLInputState.Handle"/> 中完成（等价于 C++ <c>SDL_PollEvent</c> 循环），
    /// 本类只负责底层 SDL 调用的 1:1 桥接。
    /// </summary>
    public sealed class Sdl3InputService : ISdlInputService
    {
        private IntPtr _focusedWindow = IntPtr.Zero;

        /// <summary>对应 C++ <c>while (SDL_PollEvent(&amp;event))</c>，经 <see cref="Sdl3EventMapper"/> 转为 SDL2 事件语义。</summary>
        public bool PollEvent(out SdlPollEvent evt)
        {
            evt = default;
            if (!SDL.PollEvent(out SDL.Event sdlEvent))
                return false;

            // SDL3：将窗口像素坐标转换为 ViewW×ViewH 逻辑坐标（SDL2 由 RenderSetLogicalSize 自动完成）
            Sdl3EventCoordinatesContext.TryConvert(ref sdlEvent);

            if (!Sdl3EventMapper.TryMap(sdlEvent, out evt))
                evt.Type = (uint)sdlEvent.Type;

            return true;
        }

        /// <summary>对应 <c>SDL_ShowCursor</c> / <c>SDL_HideCursor</c>（由 state 区分）。</summary>
        public void ShowCursor(int state)
        {
            if (state == SdlCursorVisibility.Enable)
                SDL.ShowCursor();
            else
                SDL.HideCursor();
        }

        /// <summary>对应 <c>SDL_NumJoysticks()</c>。</summary>
        public int NumJoysticks()
        {
            return Sdl3JoystickTable.JoystickCount;
        }

        /// <summary>对应 <c>SDL_IsGameController(int index)</c>。</summary>
        public bool IsGameController(int index)
        {
            return Sdl3JoystickTable.IsGameController(index);
        }

        /// <summary>对应 <c>SDL_GameControllerOpen(int joystick_index)</c>。</summary>
        public SdlGameControllerHandle? GameControllerOpen(int joystickIndex)
        {
            IntPtr gamepad = Sdl3JoystickTable.OpenGameController(joystickIndex);
            return gamepad == IntPtr.Zero ? null : new SdlGameControllerHandle(gamepad);
        }

        /// <summary>对应 <c>SDL_GameControllerClose</c>。</summary>
        public void GameControllerClose(SdlGameControllerHandle? gamepad)
        {
            if (gamepad?.NativePtr != IntPtr.Zero)
                SDL.CloseGamepad(gamepad.NativePtr);
        }

        /// <summary>对应 <c>SDL_GameControllerNameForIndex(int index)</c>（第 N 个游戏手柄，非槽位索引）。</summary>
        public string GameControllerNameForIndex(int index)
        {
            return Sdl3JoystickTable.GameControllerNameForIndex(index);
        }

        /// <summary>对应 <c>SDL_JoystickInstanceID(SDL_GameControllerGetJoystick(gamepad))</c>。</summary>
        public int JoystickInstanceId(SdlGameControllerHandle? gameController)
        {
            if (gameController?.NativePtr == IntPtr.Zero)
                return -1;

            return Sdl3JoystickTable.JoystickInstanceId(gameController.NativePtr);
        }

        /// <summary>对应 <c>SDL_GetKeyFromScancode</c>。</summary>
        public int GetKeyFromScancode(int scancode)
        {
            return (int)SDL.GetKeyFromScancode((SDL.Scancode)scancode, SDL.Keymod.None, true);
        }

        /// <summary>对应 <c>SDL_GetKeyName</c>。</summary>
        public string GetKeyName(int keycode)
        {
            return SDL.GetKeyName((SDL.Keycode)keycode) ?? string.Empty;
        }

        /// <summary>对应 <c>SDL_StartTextInput()</c>（SDL3 需绑定焦点窗口）。</summary>
        public void StartTextInput()
        {
            EnsureFocusedWindow();
            if (_focusedWindow != IntPtr.Zero)
                SDL.StartTextInput(_focusedWindow);
        }

        /// <summary>对应 <c>SDL_StopTextInput()</c>。</summary>
        public void StopTextInput()
        {
            EnsureFocusedWindow();
            if (_focusedWindow != IntPtr.Zero)
                SDL.StopTextInput(_focusedWindow);
        }

        /// <summary>对应 <c>SDL_GetScancodeFromName</c>。</summary>
        public int GetScancodeFromName(string name)
        {
            return (int)SDL.GetScancodeFromName(name);
        }

        /// <summary>对应 <c>SDL_GameControllerGetButtonFromString</c>。</summary>
        public int GameControllerGetButtonFromString(string name)
        {
            return (int)SDL.GetGamepadButtonFromString(name);
        }

        /// <summary>对应 <c>SDL_GameControllerGetAxisFromString</c>。</summary>
        public int GameControllerGetAxisFromString(string name)
        {
            return (int)SDL.GetGamepadAxisFromString(name);
        }

        /// <summary>对应 <c>SDL_GameControllerHasRumble</c>。</summary>
        public int GameControllerHasRumble(SdlGameControllerHandle? gamepad)
        {
            if (gamepad?.NativePtr == IntPtr.Zero)
                return 0;

            return SDL.GetBooleanProperty(
                SDL.GetGamepadProperties(gamepad.NativePtr),
                SDL.Props.GamepadCapRumbleBoolean,
                false)
                ? SdlBool.True
                : 0;
        }

        /// <summary>对应 <c>SDL_GameControllerRumble</c>。</summary>
        public void GameControllerRumble(SdlGameControllerHandle? gamepad, ushort lowFreq, ushort highFreq, uint duration)
        {
            if (gamepad?.NativePtr != IntPtr.Zero)
                SDL.RumbleGamepad(gamepad.NativePtr, lowFreq, highFreq, duration);
        }

        /// <summary>对应 <c>SDL_GameControllerHasLED</c>。</summary>
        public int GameControllerHasLed(SdlGameControllerHandle? gamepad)
        {
            if (gamepad?.NativePtr == IntPtr.Zero)
                return 0;

            return SDL.GetBooleanProperty(
                SDL.GetGamepadProperties(gamepad.NativePtr),
                SDL.Props.GamepadCapRGBLedBoolean,
                false)
                ? SdlBool.True
                : 0;
        }

        /// <summary>对应 <c>SDL_GameControllerSetLED</c>。</summary>
        public void GameControllerSetLed(SdlGameControllerHandle? gamepad, byte r, byte g, byte b)
        {
            if (gamepad?.NativePtr != IntPtr.Zero)
                SDL.SetGamepadLED(gamepad.NativePtr, r, g, b);
        }

        /// <summary>对应 <c>SDL_GetClipboardText</c>。</summary>
        public string? GetClipboardText()
        {
            return SDL.GetClipboardText();
        }

        /// <summary>对应 <c>SDL_SetClipboardText</c>。</summary>
        public void SetClipboardText(string text)
        {
            SDL.SetClipboardText(text);
        }

        /// <summary>检测指定 scancode 的键是否被按下。</summary>
        public bool IsKeyPressed(int scancode)
        {
            ReadOnlySpan<bool> keys = SDL.GetKeyboardState(out int numKeys);
            if (scancode >= numKeys || scancode < 0)
                return false;
            return keys[scancode];
        }

        private void EnsureFocusedWindow()
        {
            if (_focusedWindow == IntPtr.Zero)
                _focusedWindow = SDL.GetKeyboardFocus();
        }
    }
}
