namespace FlareEngine.Sdl
{
    /// <summary>
    /// 程序启动时将 SDL3 原生服务注入各引擎静态注入点。
    /// 对应 C++ 中各子系统直接使用 SDL API 的模式，在 C# 侧通过接口分派实现：
    /// <list type="bullet">
    /// <item><see cref="Program.Sdl"/> ← main.cpp 的 SDL_Init / PumpEvents 等</item>
    /// <item><see cref="SDLInputState.Sdl"/> ← SDLInputState.cpp</item>
    /// <item><see cref="SDLFontEngine.TtfService"/> ← SDLFontEngine.cpp 的 TTF_* 调用</item>
    /// <item><see cref="SDLHardwareRenderDevice.Sdl"/> / <see cref="SDLSoftwareRenderDevice.Sdl"/> ← 渲染设备 .cpp</item>
    /// <item><see cref="SDLSoundManager"/> 构造时通过 <see cref="CreateMixer"/> 获取 ← SDLSoundManager.cpp</item>
    /// </list>
    /// </summary>
    public static class Sdl3Bootstrap
    {
        private static Sdl3RenderService? _renderService;

        /// <summary>注册全部 SDL3 绑定（幂等，可重复调用）。应在 <see cref="Program.Main"/> 最开头调用。</summary>
        public static void Register()
        {
            Program.Sdl ??= new Sdl3ApplicationService();
            SDLFontEngine.TtfService ??= new Sdl3TtfService();
            SDLInputState.Sdl ??= new Sdl3InputService();
            _renderService ??= new Sdl3RenderService();
            SDLHardwareRenderDevice.Sdl ??= _renderService;
            SDLSoftwareRenderDevice.Sdl ??= _renderService;
        }

        /// <summary>
        /// 供 <see cref="SDLSoundManager"/> 在 <c>Mix_OpenAudio</c> 前获取 mixer 后端，
        /// 对应 C++ 中 SDLSoundManager 直接使用 SDL_mixer API。
        /// </summary>
        internal static ISdlMixer CreateMixer()
        {
            return new Sdl3MixerService();
        }
    }
}
