using SDL3;

namespace FlareEngine.Sdl
{
    /// <summary>
    /// 将 SDL2 的“手柄槽位索引”语义桥接到 SDL3 的实例 ID API。
    /// 对应 C++ 中 <c>SDL_NumJoysticks</c> / <c>SDL_IsGameController(int index)</c> /
    /// <c>SDL_GameControllerOpen(int index)</c> 与
    /// <c>SDL_GameControllerNameForIndex(int index)</c> 的两套索引规则。
    /// </summary>
    internal static class Sdl3JoystickTable
    {
        /// <summary>对应 <c>SDL_NumJoysticks()</c>。</summary>
        public static int JoystickCount
        {
            get
            {
                int count = 0;
                SDL.GetJoysticks(out count);
                return count;
            }
        }

        /// <summary>对应 <c>SDL_IsGameController(int joystick_index)</c>。</summary>
        public static bool IsGameController(int joystickIndex)
        {
            uint[] joysticks = QueryJoysticks(out int count);
            if (joystickIndex < 0 || joystickIndex >= count)
                return false;

            return SDL.IsGamepad(joysticks[joystickIndex]);
        }

        /// <summary>
        /// 对应 <c>SDL_GameControllerOpen(int joystick_index)</c>；
        /// <paramref name="joystickIndex"/> 为全系统手柄槽位，而非“第 N 个游戏手柄”序号。
        /// </summary>
        public static IntPtr OpenGameController(int joystickIndex)
        {
            uint[] joysticks = QueryJoysticks(out int count);
            if (joystickIndex < 0 || joystickIndex >= count)
                return IntPtr.Zero;

            uint instanceId = joysticks[joystickIndex];
            if (!SDL.IsGamepad(instanceId))
                return IntPtr.Zero;

            return SDL.OpenGamepad(instanceId);
        }

        /// <summary>
        /// 对应 <c>SDL_GameControllerNameForIndex(int index)</c>；
        /// 此处 index 为“第 N 个游戏手柄”，与 Open 使用的槽位索引不同。
        /// </summary>
        public static string GameControllerNameForIndex(int gamepadIndex)
        {
            uint[] gamepads = QueryGamepads(out int count);
            if (gamepadIndex < 0 || gamepadIndex >= count)
                return string.Empty;

            return SDL.GetGamepadNameForID(gamepads[gamepadIndex]) ?? string.Empty;
        }

        /// <summary>对应 <c>SDL_JoystickInstanceID(SDL_GameControllerGetJoystick(...))</c>。</summary>
        public static int JoystickInstanceId(IntPtr gamepad)
        {
            if (gamepad == IntPtr.Zero)
                return -1;

            return (int)SDL.GetGamepadID(gamepad);
        }

        private static uint[] QueryJoysticks(out int count)
        {
            count = 0;
            uint[]? joysticks = SDL.GetJoysticks(out count);
            if (joysticks == null || count <= 0)
            {
                count = 0;
                return Array.Empty<uint>();
            }

            return joysticks;
        }

        private static uint[] QueryGamepads(out int count)
        {
            count = 0;
            uint[]? gamepads = SDL.GetGamepads(out count);
            if (gamepads == null || count <= 0)
            {
                count = 0;
                return Array.Empty<uint>();
            }

            return gamepads;
        }
    }
}
