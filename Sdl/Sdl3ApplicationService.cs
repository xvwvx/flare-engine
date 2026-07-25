using SDL3;

namespace FlareEngine.Sdl
{
    /// <summary>
    /// 对应 C++ <c>main.cpp</c> 中直接调用的 SDL 应用级 API：
    /// <c>SDL_Init</c>、<c>SDL_GetError</c>、<c>SDL_PumpEvents</c>、
    /// <c>SDL_GetPerformanceCounter</c>、<c>SDL_GetPerformanceFrequency</c>、<c>SDL_Delay</c>。
    /// 音频子系统由 <see cref="Sdl3MixerService"/> 在 <c>Mix_OpenAudio</c> 时初始化，
    /// 不在此处调用 <c>Mix_Init</c>（与 C++ <c>SDLSoundManager</c> 构造函数一致）。
    /// </summary>
    public sealed class Sdl3ApplicationService : ISdlApplicationService
    {
        /// <summary>对应 <c>SDL_Init(SDL_INIT_VIDEO | SDL_INIT_AUDIO | SDL_INIT_GAMECONTROLLER)</c>。</summary>
        public int Init(SdlInitFlags flags)
        {
            SDL.InitFlags sdlFlags = 0;
            if ((flags & SdlInitFlags.Video) != 0)
                sdlFlags |= SDL.InitFlags.Video;
            if ((flags & SdlInitFlags.Audio) != 0)
                sdlFlags |= SDL.InitFlags.Audio;
            if ((flags & SdlInitFlags.GameController) != 0)
                sdlFlags |= SDL.InitFlags.Gamepad;

            if (!SDL.Init(sdlFlags))
                return -1;

            return 0;
        }

        /// <summary>对应 <c>SDL_GetError()</c>。</summary>
        public string GetError()
        {
            return SDL.GetError() ?? string.Empty;
        }

        /// <summary>对应 <c>SDL_PumpEvents()</c>（<c>mainLoop</c> 每帧调用）。</summary>
        public void PumpEvents()
        {
            SDL.PumpEvents();
        }

        /// <summary>对应 <c>SDL_GetPerformanceCounter()</c>。</summary>
        public ulong GetPerformanceCounter()
        {
            return SDL.GetPerformanceCounter();
        }

        /// <summary>对应 <c>SDL_GetPerformanceFrequency()</c>。</summary>
        public ulong GetPerformanceFrequency()
        {
            return SDL.GetPerformanceFrequency();
        }

        /// <summary>对应 <c>SDL_Delay(ms)</c>（帧率限制分支）。</summary>
        public void Delay(int milliseconds)
        {
            SDL.Delay((uint)milliseconds);
        }
    }
}
