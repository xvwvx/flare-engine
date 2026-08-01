// 对应 C++ 源：SDLHardwareRenderDevice.h + SDLHardwareRenderDevice.cpp
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// SDL 硬件渲染后端所需的全部原生 API 抽象（SDL2 / SDL_image / SDL_ttf）。
    /// 规则 7：业务逻辑禁止直接 P/Invoke SDL2；具体绑定由后续 SDL 封装单元注入
    /// <see cref="SDLHardwareRenderDevice.Sdl"/>。
    /// </summary>
    public interface ISdlHardwareRenderService
    {
        // --- 版本/显示信息 ---
        bool SupportsDisplayDpi { get; }
        string GetCurrentVideoDriver();
        int GetNumVideoDisplays();
        int GetDesktopDisplayMode(int displayIndex, out int w, out int h, out int refreshRate);
        int GetCurrentDisplayMode(int displayIndex, out int w, out int h, out int refreshRate);
        int GetDisplayDpi(int displayIndex, out float ddpi, out float hdpi, out float vdpi);
        string GetError();

        // --- Hint / 窗口 / 渲染器 ---
        void SetHintWithPriority(string name, string value, int priority);
        object? CreateWindow(string title, int x, int y, int w, int h, uint flags);
        void DestroyWindow(object? window);
        object? CreateRenderer(object? window, int index, uint flags);
        void DestroyRenderer(object? renderer);
        void SetWindowMinimumSize(object? window, int minW, int minH);
        void SetWindowPosition(object? window, int x, int y);
        void SetWindowSize(object? window, int w, int h);
        void SetWindowFullscreen(object? window, uint flags);
        void SetWindowTitle(object? window, string title);
        void SetWindowIcon(object? window, object? surface);
        void GetWindowSize(object? window, out int w, out int h);
        int GetWindowGammaRamp(object? window, ushort[] rampR, ushort[] rampG, ushort[] rampB);
        int SetWindowGammaRamp(object? window, ushort[] rampR, ushort[] rampG, ushort[] rampB);
        void CalculateGammaRamp(float gamma, ushort[] ramp);
        void GetRendererInfo(object? renderer, out string name);

        // --- 纹理 ---
        void DestroyTexture(object? texture);
        int QueryTexture(object? texture, out int w, out int h);
        object? CreateTexture(object? renderer, uint pixelFormat, int access, int w, int h);
        object? CreateTextureFromSurface(object? renderer, object? surface);
        void SetTextureBlendMode(object? texture, int blendMode);
        void SetTextureColorMod(object? texture, byte r, byte g, byte b);
        void SetTextureAlphaMod(object? texture, byte alpha);

        // --- 渲染目标与绘制 ---
        int SetRenderTarget(object? renderer, object? texture);
        void SetRenderDrawColor(object? renderer, byte r, byte g, byte b, byte a);
        void RenderClear(object? renderer);
        void RenderDrawPoint(object? renderer, int x, int y);
        void RenderDrawLine(object? renderer, int x0, int y0, int x1, int y1);
        void RenderFillRect(object? renderer, ref Rectangle rect);
        int RenderCopy(object? renderer, object? texture, Rectangle? src, Rectangle? dest);
        int RenderCopyEx(object? renderer, object? texture, Rectangle? src, Rectangle? dest, double angle, Int2? center, int flip);
        void RenderPresent(object? renderer);
        void RenderSetLogicalSize(object? renderer, int w, int h);

        // --- Surface（像素批处理） ---
        void FreeSurface(object? surface);
        object? CreateRgbSurface(uint flags, int width, int height, int depth, uint rmask, uint gmask, uint bmask, uint amask);
        uint MapRgba(object? surface, byte r, byte g, byte b, byte a);
        int GetSurfaceBytesPerPixel(object? surface);
        int GetSurfacePitch(object? surface);
        bool MustLockSurface(object? surface);
        void LockSurface(object? surface);
        void UnlockSurface(object? surface);
        int GetSurfacePixelByteOffset(object? surface, int x, int y);
        void WriteSurfaceByte(object? surface, int byteOffset, byte value);
        void WriteSurfaceUInt16(object? surface, int byteOffset, ushort value);
        void WriteSurfaceUInt32(object? surface, int byteOffset, uint value);

        // --- SDL_image ---
        object? ImgLoad(string filename);
        object? ImgLoadTexture(object? renderer, string filename);
        string ImgGetError();

        // --- SDL_ttf（由 SDLFontEngine 单元提供字体句柄） ---
        object? TtfRenderUtf8Blended(object? font, string text, Color color);
        object? TtfRenderUtf8Solid(object? font, string text, Color color);

        // --- 线程同步 ---
        object? CreateMutex();
        void DestroyMutex(object? mutex);
        object? CreateCond();
        void DestroyCond(object? cond);
        void LockMutex(object? mutex);
        void UnlockMutex(object? mutex);
        void CondSignal(object? cond);
        void CondWait(object? cond, object? mutex);
        object? CreateThread(SdlThreadEntry callback, string threadName, object? userdata);
        void WaitThread(object? thread, out int threadReturnCode);
    }

    /// <summary>对应 SDL 线程入口 <c>int (*)(void* data)</c>。</summary>
    public delegate int SdlThreadEntry(object? userdata);

    /// <summary>
    /// SDLFontEngine 单元中 <c>SDLFontStyle</c> 将提供的 TTF 字体原生句柄访问契约。
    /// </summary>
    public interface ISdlFontNativeHandle
    {
        object? TtFont { get; }
    }

    /// <summary>SDL2 混合模式、窗口/渲染器标志等常量（与 SDL2 头文件取值一致）。</summary>
    public static class SdlHardwareConstants
    {
        public const int BlendModeBlend = 1;
        public const int BlendModeAdd = 2;

        public const uint WindowFullscreenDesktop = 0x00001001;
        public const uint WindowShown = 0x00000004;
        public const uint WindowResizable = 0x00000020;

        public const int WindowposCentered = 0x2FFF0000;

        public const uint RendererAccelerated = 0x00000002;
        public const uint RendererSoftware = 0x00000001;
        public const uint RendererTargetTexture = 0x00000008;
        public const uint RendererPresentVsync = 0x00000004;

        public const int HintOverride = 3;

        public const uint PixelFormatArgb8888 = 0x16362004;
        public const int TextureAccessTarget = 3;

        public const int FlipNone = 0;

        public const int PixelBatchNone = 0;
        public const int PixelBatchAll = 1;
        public const int PixelBatchArea = 2;
    }

    /// <summary>
    /// 基于 SDL 纹理/GPU 后端的 <see cref="Image"/> 实现（对应 C++ <c>SDLHardwareImage</c>）。
    /// </summary>
    public class SDLHardwareImage : Image, IDisposable
    {
        public object? Renderer;
        public object? Surface;
        public object? PixelBatchSurface;
        public int PixelBatchType;
        public Rectangle PixelBatchArea;

        public SDLHardwareImage(RenderDevice device, object? renderer)
            : base(device)
        {
            Renderer = renderer;
            Surface = null;
            PixelBatchSurface = null;
            PixelBatchType = SdlHardwareConstants.PixelBatchNone;
        }

        public void Dispose()
        {
            ReleaseNativeResources();
        }

        internal override void ReleaseNativeResources()
        {
            ISdlHardwareRenderService? sdl = SDLHardwareRenderDevice.Sdl;
            if (Surface != null)
            {
                sdl?.DestroyTexture(Surface);
                Surface = null;
            }
            if (PixelBatchSurface != null)
            {
                sdl?.FreeSurface(PixelBatchSurface);
                PixelBatchSurface = null;
            }
        }

        public override int GetWidth()
        {
            int w = 0;
            int h = 0;
            if (Surface != null)
                SDLHardwareRenderDevice.Sdl?.QueryTexture(Surface, out w, out h);
            return (Surface != null ? w : 0);
        }

        public override int GetHeight()
        {
            int w = 0;
            int h = 0;
            if (Surface != null)
                SDLHardwareRenderDevice.Sdl?.QueryTexture(Surface, out w, out h);
            return (Surface != null ? h : 0);
        }

        public override void FillWithColor(Color color)
        {
            if (Surface == null) return;

            ISdlHardwareRenderService? sdl = SDLHardwareRenderDevice.Sdl;
            sdl?.SetRenderTarget(Renderer, Surface);
            sdl?.SetTextureBlendMode(Surface, SdlHardwareConstants.BlendModeBlend);
            sdl?.SetRenderDrawColor(Renderer, color.R, color.G, color.B, color.A);
            sdl?.RenderClear(Renderer);
            sdl?.SetRenderTarget(Renderer, null);
        }

        public override void DrawPixel(int x, int y, Color color)
        {
            if (Surface == null) return;

            if (x < 0 || y < 0 || x >= GetWidth() || y >= GetHeight())
                return;

            switch (PixelBatchType)
            {
                case SdlHardwareConstants.PixelBatchNone:
                    DrawPixelSingle(x, y, color);
                    break;
                case SdlHardwareConstants.PixelBatchArea:
                    if (x < PixelBatchArea.X) return;
                    if (y < PixelBatchArea.Y) return;
                    if (x > PixelBatchArea.X + PixelBatchArea.Width - 1) return;
                    if (y > PixelBatchArea.Y + PixelBatchArea.Height - 1) return;
                    x = x - PixelBatchArea.X;
                    y = y - PixelBatchArea.Y;
                    DrawPixelBatch(x, y, color);
                    break;
                case SdlHardwareConstants.PixelBatchAll:
                    DrawPixelBatch(x, y, color);
                    break;
            }
        }

        private void DrawPixelSingle(int x, int y, Color color)
        {
            ISdlHardwareRenderService? sdl = SDLHardwareRenderDevice.Sdl;
            sdl?.SetRenderTarget(Renderer, Surface);
            sdl?.SetTextureBlendMode(Surface, SdlHardwareConstants.BlendModeBlend);
            sdl?.SetRenderDrawColor(Renderer, color.R, color.G, color.B, color.A);
            sdl?.RenderDrawPoint(Renderer, x, y);
            sdl?.SetRenderTarget(Renderer, null);
        }

        private void DrawPixelBatch(int x, int y, Color color)
        {
            ISdlHardwareRenderService? sdl = SDLHardwareRenderDevice.Sdl;
            if (sdl == null || PixelBatchSurface == null) return;

            uint pixel = sdl.MapRgba(PixelBatchSurface, color.R, color.G, color.B, color.A);

            int bpp = sdl.GetSurfaceBytesPerPixel(PixelBatchSurface);
            int p = sdl.GetSurfacePixelByteOffset(PixelBatchSurface, x, y);

            if (sdl.MustLockSurface(PixelBatchSurface))
            {
                sdl.LockSurface(PixelBatchSurface);
            }
            switch (bpp)
            {
                case 1:
                    sdl.WriteSurfaceByte(PixelBatchSurface, p, (byte)pixel);
                    break;

                case 2:
                    sdl.WriteSurfaceUInt16(PixelBatchSurface, p, (ushort)pixel);
                    break;

                case 3:
                    if (!BitConverter.IsLittleEndian)
                    {
                        sdl.WriteSurfaceByte(PixelBatchSurface, p + 0, (byte)((pixel >> 16) & 0xff));
                        sdl.WriteSurfaceByte(PixelBatchSurface, p + 1, (byte)((pixel >> 8) & 0xff));
                        sdl.WriteSurfaceByte(PixelBatchSurface, p + 2, (byte)(pixel & 0xff));
                    }
                    else
                    {
                        sdl.WriteSurfaceByte(PixelBatchSurface, p + 0, (byte)(pixel & 0xff));
                        sdl.WriteSurfaceByte(PixelBatchSurface, p + 1, (byte)((pixel >> 8) & 0xff));
                        sdl.WriteSurfaceByte(PixelBatchSurface, p + 2, (byte)((pixel >> 16) & 0xff));
                    }
                    break;

                case 4:
                    sdl.WriteSurfaceUInt32(PixelBatchSurface, p, pixel);
                    break;
            }
            if (sdl.MustLockSurface(PixelBatchSurface))
            {
                sdl.UnlockSurface(PixelBatchSurface);
            }
        }

        public override void DrawLine(int x0, int y0, int x1, int y1, Color color)
        {
            ISdlHardwareRenderService? sdl = SDLHardwareRenderDevice.Sdl;
            sdl?.SetRenderTarget(Renderer, Surface);
            sdl?.SetTextureBlendMode(Surface, SdlHardwareConstants.BlendModeBlend);
            sdl?.SetRenderDrawColor(Renderer, color.R, color.G, color.B, color.A);
            sdl?.RenderDrawLine(Renderer, x0, y0, x1, y1);
            sdl?.SetRenderTarget(Renderer, null);
        }

        public override void DrawFilledRect(int x, int y, int w, int h, Color color)
        {
            Rectangle rect = new Rectangle(x, y, w, h);

            ISdlHardwareRenderService? sdl = SDLHardwareRenderDevice.Sdl;
            sdl?.SetRenderTarget(Renderer, Surface);
            sdl?.SetTextureBlendMode(Surface, SdlHardwareConstants.BlendModeBlend);
            sdl?.SetRenderDrawColor(Renderer, color.R, color.G, color.B, color.A);
            sdl?.RenderFillRect(Renderer, ref rect);
            sdl?.SetRenderTarget(Renderer, null);
        }

        public override void BeginPixelBatch()
        {
            if (Surface == null) return;

            PixelBatchType = SdlHardwareConstants.PixelBatchAll;

            uint rmask;
            uint gmask;
            uint bmask;
            uint amask;
            Utils.SetSdlRgba(out rmask, out gmask, out bmask, out amask);

            ISdlHardwareRenderService? sdl = SDLHardwareRenderDevice.Sdl;
            if (PixelBatchSurface != null)
                sdl?.FreeSurface(PixelBatchSurface);

            PixelBatchSurface = sdl?.CreateRgbSurface(0, GetWidth(), GetHeight(), RenderDevice.BitsPerPixel, rmask, gmask, bmask, amask);
        }

        public override void BeginPixelBatch(ref Rectangle bounds)
        {
            if (Surface == null) return;
            if (bounds.Width <= 0 || bounds.Height <= 0) return;

            PixelBatchType = SdlHardwareConstants.PixelBatchArea;
            PixelBatchArea = bounds;

            uint rmask;
            uint gmask;
            uint bmask;
            uint amask;
            Utils.SetSdlRgba(out rmask, out gmask, out bmask, out amask);

            ISdlHardwareRenderService? sdl = SDLHardwareRenderDevice.Sdl;
            if (PixelBatchSurface != null)
                sdl?.FreeSurface(PixelBatchSurface);

            PixelBatchSurface = sdl?.CreateRgbSurface(0, bounds.Width, bounds.Height, RenderDevice.BitsPerPixel, rmask, gmask, bmask, amask);
        }

        public override void EndPixelBatch()
        {
            if (Surface == null || PixelBatchSurface == null) return;

            ISdlHardwareRenderService? sdl = SDLHardwareRenderDevice.Sdl;
            if (sdl == null) return;

            object? pixelBatchTexture = sdl.CreateTextureFromSurface(Renderer, PixelBatchSurface);

            if (pixelBatchTexture != null)
            {
                sdl.SetRenderTarget(Renderer, Surface);
                sdl.SetTextureBlendMode(Surface, SdlHardwareConstants.BlendModeBlend);

                if (PixelBatchType == SdlHardwareConstants.PixelBatchAll)
                {
                    sdl.RenderCopy(Renderer, pixelBatchTexture, null, null);
                }
                else if (PixelBatchType == SdlHardwareConstants.PixelBatchArea)
                {
                    Rectangle dst = PixelBatchArea;
                    sdl.RenderCopy(Renderer, pixelBatchTexture, null, dst);
                }
                sdl.SetRenderTarget(Renderer, null);

                sdl.DestroyTexture(pixelBatchTexture);
            }
            sdl.FreeSurface(PixelBatchSurface);
            PixelBatchSurface = null;
            PixelBatchType = SdlHardwareConstants.PixelBatchNone;
        }

        public override Image Resize(int width, int height)
        {
            if (Surface == null || width <= 0 || height <= 0)
                return null!;

            SDLHardwareImage scaled = new SDLHardwareImage(_device, Renderer);

            ISdlHardwareRenderService? sdl = SDLHardwareRenderDevice.Sdl;
            scaled.Surface = sdl?.CreateTexture(Renderer, SdlHardwareConstants.PixelFormatArgb8888, SdlHardwareConstants.TextureAccessTarget, width, height);

            if (scaled.Surface != null)
            {
                sdl?.SetRenderTarget(Renderer, scaled.Surface);
                sdl?.RenderCopyEx(Renderer, Surface, null, null, 0, null, SdlHardwareConstants.FlipNone);
                sdl?.SetRenderTarget(Renderer, null);

                Unref();
                return scaled;
            }
            else
            {
                scaled.Dispose();
            }

            return null!;
        }
    }

    /// <summary>
    /// 基于 SDL GPU 纹理后端的 FLARE 渲染设备（对应 C++ <c>SDLHardwareRenderDevice</c>）。
    /// </summary>
    public class SDLHardwareRenderDevice : RenderDevice
    {
        /// <summary>
        /// SDL 硬件渲染原生 API 注入点。实现尚未就绪前为 null，各调用点通过 <c>?.</c> 保持中性行为。
        /// </summary>
        public static ISdlHardwareRenderService? Sdl { get; set; }

        private object? _window;
        private object? _renderer;
        private object? _texture;
        private object? _titlebarIcon;
        private string? _title;
        private Color _backgroundColor;

        private readonly ushort[] _gammaR = new ushort[256];
        private readonly ushort[] _gammaG = new ushort[256];
        private readonly ushort[] _gammaB = new ushort[256];

        public SDLHardwareRenderDevice()
        {
            _window = null;
            _renderer = null;
            _texture = null;
            _titlebarIcon = null;
            _title = null;
            _backgroundColor = new Color(0, 0, 0, 255);

            Utils.LogInfo("Using Render Device: SDLHardwareRenderDevice (hardware, SDL 2, %s)", Sdl?.GetCurrentVideoDriver() ?? string.Empty);

            var settings = SharedResources.Settings!;
            var eset = SharedResources.Eset!;

            Fullscreen = settings.Fullscreen;
            Hwsurface = settings.Hwsurface;
            Vsync = settings.Vsync;
            TextureFilter = settings.TextureFilter;

            MinScreen.X = eset.Resolutions.MinScreenW;
            MinScreen.Y = eset.Resolutions.MinScreenH;

            if (Sdl != null && Sdl.GetDesktopDisplayMode(0, out int desktopW, out int desktopH, out int refreshRate) == 0)
            {
                Utils.LogInfo("RenderDevice: %d display(s), using display 0 (%dx%d @ %dhz)", Sdl.GetNumVideoDisplays(), desktopW, desktopH, refreshRate);
            }

            for (int i = 0; i < 256; ++i)
            {
                _gammaR[i] = 0;
                _gammaG[i] = 0;
                _gammaB[i] = 0;
            }
        }

        protected override int CreateContextInternal()
        {
            var settings = SharedResources.Settings!;
            var eset = SharedResources.Eset!;

            if (OperatingSystem.IsWindows())
            {
                Sdl?.SetHintWithPriority("SDL_RENDER_DRIVER", "opengl", SdlHardwareConstants.HintOverride);
            }

            bool settingsChanged = ((Fullscreen != settings.Fullscreen && DestructiveFullscreen) ||
                                    Hwsurface != settings.Hwsurface ||
                                    Vsync != settings.Vsync ||
                                    TextureFilter != settings.TextureFilter ||
                                    IgnoreTextureFilter != eset.Resolutions.IgnoreTextureFilter);

            uint wFlags = 0;
            uint rFlags = 0;
            int windowW = settings.ScreenW;
            int windowH = settings.ScreenH;

            // Apply display scale multiplier for non-fullscreen mode
            if (!settings.Fullscreen)
            {
                float scale = settings.DisplayScale;
                windowW = (int)(windowW * scale);
                windowH = (int)(windowH * scale);
            }

            if (settings.Fullscreen)
            {
                wFlags = wFlags | SdlHardwareConstants.WindowFullscreenDesktop;

                if (Sdl != null && Sdl.GetDesktopDisplayMode(0, out int desktopW, out int desktopH, out int refreshRate) == 0)
                {
                    windowW = desktopW;
                    windowH = desktopH;
                }
            }
            else if (Fullscreen && IsInitialized)
            {
                float scale = settings.DisplayScale;
                windowW = (int)(eset.Resolutions.MinScreenW * scale);
                windowH = (int)(eset.Resolutions.MinScreenH * scale);
                wFlags = wFlags | SdlHardwareConstants.WindowShown;
            }
            else
            {
                wFlags = wFlags | SdlHardwareConstants.WindowShown;
            }

            wFlags = wFlags | SdlHardwareConstants.WindowResizable;

            if (settings.Hwsurface)
            {
                rFlags = SdlHardwareConstants.RendererAccelerated | SdlHardwareConstants.RendererTargetTexture;
            }
            else
            {
                rFlags = SdlHardwareConstants.RendererSoftware | SdlHardwareConstants.RendererTargetTexture;
                settings.Vsync = false;
            }
            if (settings.Vsync) rFlags = rFlags | SdlHardwareConstants.RendererPresentVsync;

            if (settingsChanged || !IsInitialized)
            {
                DestroyContext();

                _window = Sdl?.CreateWindow(null!, SdlHardwareConstants.WindowposCentered, SdlHardwareConstants.WindowposCentered, windowW, windowH, wFlags);
                if (_window != null)
                {
                    _renderer = Sdl?.CreateRenderer(_window, -1, rFlags);
                    if (_renderer != null)
                    {
                        if (settings.TextureFilter && !eset.Resolutions.IgnoreTextureFilter)
                            Sdl?.SetHintWithPriority("SDL_RENDER_SCALE_QUALITY", "1", SdlHardwareConstants.HintOverride);
                        else
                            Sdl?.SetHintWithPriority("SDL_RENDER_SCALE_QUALITY", "0", SdlHardwareConstants.HintOverride);
                    }

                    int cMinW = eset.Resolutions.MinScreenW;
                    int cMinH = eset.Resolutions.MinScreenH;
                    if (!settings.Fullscreen)
                    {
                        cMinW = (int)(cMinW * settings.DisplayScale);
                        cMinH = (int)(cMinH * settings.DisplayScale);
                    }
                    Sdl?.SetWindowMinimumSize(_window, cMinW, cMinH);
                    Sdl?.SetWindowPosition(_window, SdlHardwareConstants.WindowposCentered, SdlHardwareConstants.WindowposCentered);
                    Sdl?.SetWindowSize(_window, windowW, windowH);
                }

                if (_window != null && _renderer != null)
                {
                    if (!IsInitialized)
                    {
                        Sdl?.GetWindowGammaRamp(_window, _gammaR, _gammaG, _gammaB);
                        Utils.LogInfo("RenderDevice: Window size is %dx%d", settings.ScreenW, settings.ScreenH);
                    }

                    Fullscreen = settings.Fullscreen;
                    Hwsurface = settings.Hwsurface;
                    Vsync = settings.Vsync;
                    TextureFilter = settings.TextureFilter;
                    IgnoreTextureFilter = eset.Resolutions.IgnoreTextureFilter;
                    IsInitialized = true;

                    Utils.LogInfo("RenderDevice: Fullscreen=%d, Hardware surfaces=%d, Vsync=%d, Texture Filter=%d", Fullscreen ? 1 : 0, Hwsurface ? 1 : 0, Vsync ? 1 : 0, TextureFilter ? 1 : 0);

                    string rendererName = "";
                    Sdl?.GetRendererInfo(_renderer, out rendererName);
                    Utils.LogInfo("RenderDevice: Renderer driver is '%s'.", rendererName);

                    if (Sdl?.SupportsDisplayDpi ?? false)
                    {
                        Sdl.GetDisplayDpi(0, out float ddpiValue, out float hdpi, out float vdpi);
                        Ddpi = ddpiValue;
                        Utils.LogInfo("RenderDevice: Display DPI is %f", Ddpi);
                    }
                    else
                    {
                        Utils.LogError("RenderDevice: The SDL version used to compile Flare does not support SDL_GetDisplayDPI(). The virtual_dpi setting will be ignored.");
                    }
                }
            }

            if (IsInitialized)
            {
                if (MinScreen.X != eset.Resolutions.MinScreenW || MinScreen.Y != eset.Resolutions.MinScreenH)
                {
                    MinScreen.X = eset.Resolutions.MinScreenW;
                    MinScreen.Y = eset.Resolutions.MinScreenH;
                    int rMinW = MinScreen.X;
                    int rMinH = MinScreen.Y;
                    if (!settings.Fullscreen)
                    {
                        rMinW = (int)(rMinW * settings.DisplayScale);
                        rMinH = (int)(rMinH * settings.DisplayScale);
                    }
                    Sdl?.SetWindowMinimumSize(_window, rMinW, rMinH);
                    Sdl?.SetWindowPosition(_window, SdlHardwareConstants.WindowposCentered, SdlHardwareConstants.WindowposCentered);
                }

                WindowResize();
                IsInitialized = (_texture != null);
            }

            if (IsInitialized)
            {
                UpdateTitleBar();

                if (SharedResources.Icons != null)
                {
                    SharedResources.Icons.Dispose();
                    SharedResources.Icons = null;
                }
                SharedResources.Icons = new IconManager();
                if (SharedResources.Curs != null)
                {
                    SharedResources.Curs.Dispose();
                    SharedResources.Curs = null;
                }
                SharedResources.Curs = new CursorManager();

                if (settings.ChangeGamma)
                    SetGamma(settings.Gamma);
                else
                {
                    ResetGamma();
                    settings.ChangeGamma = false;
                    settings.Gamma = 1.0f;
                }
            }

            return (IsInitialized ? 0 : -1);
        }

        protected override void CreateContextError()
        {
            Utils.LogError("SDLHardwareRenderDevice: createContext() failed: %s", Sdl?.GetError() ?? string.Empty);
            Utils.LogErrorDialog("SDLHardwareRenderDevice: createContext() failed: %s", Sdl?.GetError() ?? string.Empty);
        }

        public override int Render(Renderable r, ref Rectangle dest)
        {
            dest.Width = r.Src.Width;
            dest.Height = r.Src.Height;
            Rectangle src = r.Src;
            Rectangle _dest = dest;
            Sdl?.SetRenderTarget(_renderer, _texture);

            object? surface = ((SDLHardwareImage)r.Image!).Surface;

            if (r.BlendMode == Renderable.BlendAdd)
            {
                Sdl?.SetTextureBlendMode(surface, SdlHardwareConstants.BlendModeAdd);
            }
            else
            {
                Sdl?.SetTextureBlendMode(surface, SdlHardwareConstants.BlendModeBlend);
            }

            Sdl?.SetTextureColorMod(surface, r.ColorMod.R, r.ColorMod.G, r.ColorMod.B);
            Sdl?.SetTextureAlphaMod(surface, r.AlphaMod);

            return Sdl?.RenderCopy(_renderer, surface, src, _dest) ?? -1;
        }

        public override int Render(Sprite r)
        {
            if (r == null)
            {
                return -1;
            }
            if (!LocalToGlobal(r))
            {
                return -1;
            }

            if (MClip.X < 0)
            {
                MClip.Width -= Math.Abs(MClip.X);
                MDest.X += Math.Abs(MClip.X);
                MClip.X = 0;
            }
            if (MClip.Y < 0)
            {
                MClip.Height -= Math.Abs(MClip.Y);
                MDest.Y += Math.Abs(MClip.Y);
                MClip.Y = 0;
            }

            MDest.Width = MClip.Width;
            MDest.Height = MClip.Height;

            Rectangle src = MClip;
            Rectangle dest = MDest;
            Sdl?.SetRenderTarget(_renderer, _texture);

            object? surface = ((SDLHardwareImage)r.GetGraphics()!).Surface;
            Sdl?.SetTextureColorMod(surface, r.ColorMod.R, r.ColorMod.G, r.ColorMod.B);
            Sdl?.SetTextureAlphaMod(surface, r.AlphaMod);

            return Sdl?.RenderCopy(_renderer, ((SDLHardwareImage)r.GetGraphics()!).Surface, src, dest) ?? -1;
        }

        public override int RenderToImage(Image srcImage, Rectangle src, Image destImage, Rectangle dest)
        {
            if (srcImage == null || destImage == null)
                return -1;

            if ((Sdl?.SetRenderTarget(_renderer, ((SDLHardwareImage)destImage).Surface) ?? -1) != 0)
                return -1;

            dest.Width = src.Width;
            dest.Height = src.Height;
            Rectangle _src = src;
            Rectangle _dest = dest;

            Sdl?.SetTextureBlendMode(((SDLHardwareImage)destImage).Surface, SdlHardwareConstants.BlendModeBlend);
            Sdl?.RenderCopy(_renderer, ((SDLHardwareImage)srcImage).Surface, _src, _dest);
            Sdl?.SetRenderTarget(_renderer, null);
            return 0;
        }

        public override Image? RenderTextToImage(FontStyle fontStyle, string text, Color color, bool blended)
        {
            SDLHardwareImage image = new SDLHardwareImage(this, _renderer);

            object? cleanup;

            ISdlFontNativeHandle? sdlFont = fontStyle as ISdlFontNativeHandle;
            if (sdlFont == null)
            {
                image.Dispose();
                return null;
            }

            if (blended)
            {
                cleanup = Sdl?.TtfRenderUtf8Blended(sdlFont.TtFont, text, color);
            }
            else
            {
                cleanup = Sdl?.TtfRenderUtf8Solid(sdlFont.TtFont, text, color);
            }

            if (cleanup != null)
            {
                image.Surface = Sdl?.CreateTextureFromSurface(_renderer, cleanup);
                Sdl?.FreeSurface(cleanup);
                return image;
            }

            image.Dispose();
            return null;
        }

        public override void DrawPixel(int x, int y, Color color)
        {
            Sdl?.SetRenderDrawColor(_renderer, color.R, color.G, color.B, color.A);
            Sdl?.RenderDrawPoint(_renderer, x, y);
        }

        public override void DrawLine(int x0, int y0, int x1, int y1, Color color)
        {
            Sdl?.SetRenderDrawColor(_renderer, color.R, color.G, color.B, color.A);
            Sdl?.RenderDrawLine(_renderer, x0, y0, x1, y1);
        }

        public override void DrawRectangle(Int2 p0, Int2 p1, Color color)
        {
            DrawLine(p0.X, p0.Y, p1.X, p0.Y, color);
            DrawLine(p1.X, p0.Y, p1.X, p1.Y, color);
            DrawLine(p0.X, p0.Y, p0.X, p1.Y, color);
            DrawLine(p0.X, p1.Y, p1.X + 1, p1.Y, color);
        }

        public override void BlankScreen()
        {
            Sdl?.SetRenderDrawColor(_renderer, 0, 0, 0, 255);
            Sdl?.SetRenderTarget(_renderer, null);
            Sdl?.RenderClear(_renderer);
            Sdl?.SetRenderDrawColor(_renderer, _backgroundColor.R, _backgroundColor.G, _backgroundColor.B, _backgroundColor.A);
            Sdl?.SetRenderTarget(_renderer, _texture);
            Sdl?.RenderClear(_renderer);
            return;
        }

        public override void CommitFrame()
        {
            Sdl?.SetRenderTarget(_renderer, null);
            Sdl?.RenderCopy(_renderer, _texture, null, null);
            Sdl?.RenderPresent(_renderer);
            SharedResources.Inpt!.WindowResized = false;

            return;
        }

        public override void DestroyContext()
        {
            ResetGamma();

            CacheRemoveAll();
            ReloadGraphicsFlag = true;

            if (SharedResources.Icons != null)
            {
                SharedResources.Icons.Dispose();
                SharedResources.Icons = null;
            }
            if (SharedResources.Curs != null)
            {
                SharedResources.Curs.Dispose();
                SharedResources.Curs = null;
            }

            Sdl?.FreeSurface(_titlebarIcon);
            _titlebarIcon = null;

            Sdl?.DestroyTexture(_texture);
            _texture = null;

            Sdl?.DestroyRenderer(_renderer);
            _renderer = null;

            Sdl?.DestroyWindow(_window);
            _window = null;

            if (_title != null)
            {
                _title = null;
            }

            return;
        }

        public override Image? CreateImage(int width, int height)
        {
            SDLHardwareImage image = new SDLHardwareImage(this, _renderer);

            if (width > 0 && height > 0)
            {
                image.Surface = Sdl?.CreateTexture(_renderer, SdlHardwareConstants.PixelFormatArgb8888, SdlHardwareConstants.TextureAccessTarget, width, height);
                if (image.Surface == null)
                {
                    Utils.LogError("SDLHardwareRenderDevice: SDL_CreateTexture failed: %s", Sdl?.GetError() ?? string.Empty);
                    image.Dispose();
                    image = null!;
                }
                else
                {
                    Sdl?.SetRenderTarget(_renderer, image.Surface);
                    Sdl?.SetTextureBlendMode(image.Surface, SdlHardwareConstants.BlendModeBlend);
                    Sdl?.SetRenderDrawColor(_renderer, 0, 0, 0, 0);
                    Sdl?.RenderClear(_renderer);
                    Sdl?.SetRenderTarget(_renderer, null);
                }
            }

            return image;
        }

        public override void SetGamma(float g)
        {
            ushort[] ramp = new ushort[256];
            Sdl?.CalculateGammaRamp(g, ramp);
            Sdl?.SetWindowGammaRamp(_window, ramp, ramp, ramp);
        }

        public override void ResetGamma()
        {
            Sdl?.SetWindowGammaRamp(_window, _gammaR, _gammaG, _gammaB);
        }

        public override void UpdateTitleBar()
        {
            if (_title != null) _title = null;
            _title = null;
            if (_titlebarIcon != null) Sdl?.FreeSurface(_titlebarIcon);
            _titlebarIcon = null;

            if (_window == null) return;

            _title = Utils.StrDup(SharedResources.Msg!.Get(SharedResources.Eset!.Misc.WindowTitle));
            _titlebarIcon = Sdl?.ImgLoad(SharedResources.Mods!.Locate("images/logo/icon.png"));

            if (_title != null) Sdl?.SetWindowTitle(_window, _title);
            if (_titlebarIcon != null) Sdl?.SetWindowIcon(_window, _titlebarIcon);
        }

        public override Image? LoadImage(string filename, int errorType)
        {
            Image? img;
            img = CacheLookup(filename);
            if (img != null) return img;

            SDLHardwareImage image = new SDLHardwareImage(this, _renderer);

            image.Surface = Sdl?.ImgLoadTexture(_renderer, SharedResources.Mods!.Locate(filename));

            if (image.Surface == null)
            {
                image.Dispose();
                if (errorType != ErrorNone)
                    Utils.LogError("SDLHardwareRenderDevice: Couldn't load image: '%s'. %s", filename, Sdl?.ImgGetError() ?? string.Empty);

                if (errorType == ErrorExit)
                {
                    Utils.LogErrorDialog("SDLHardwareRenderDevice: Couldn't load image: '%s'.\n%s", filename, Sdl?.ImgGetError() ?? string.Empty);
                    SharedResources.Mods!.ResetModConfig();
                    Utils.Exit(1);
                }

                return null;
            }

            CacheStore(filename, image);
            return image;
        }

        protected override void GetWindowSize(out ushort screenW, out ushort screenH)
        {
            int w = 0;
            int h = 0;
            Sdl?.GetWindowSize(_window, out w, out h);
            screenW = (ushort)w;
            screenH = (ushort)h;
        }

        public override void WindowResize()
        {
            var settings = SharedResources.Settings!;

            WindowResizeInternal();

            Sdl?.RenderSetLogicalSize(_renderer, settings.ViewW, settings.ViewH);

            if (_texture != null) Sdl?.DestroyTexture(_texture);
            _texture = Sdl?.CreateTexture(_renderer, SdlHardwareConstants.PixelFormatArgb8888, SdlHardwareConstants.TextureAccessTarget, settings.ViewW, settings.ViewH);
            if (_texture != null) Sdl?.SetRenderTarget(_renderer, _texture);

            settings.UpdateScreenVars();
        }

        public override void SetBackgroundColor(Color color)
        {
            _backgroundColor = color;
            _backgroundColor.A = 255;
        }

        public override void SetFullscreen(bool enableFullscreen)
        {
            if (!DestructiveFullscreen)
            {
                if (enableFullscreen)
                {
                    if (Platform.Instance.FullscreenBypass)
                    {
                        Platform.Instance.SetFullscreen(true);
                    }
                    else
                    {
                        Sdl?.SetWindowFullscreen(_window, SdlHardwareConstants.WindowFullscreenDesktop);
                    }
                    Fullscreen = true;
                }
                else if (Fullscreen)
                {
                    if (Platform.Instance.FullscreenBypass)
                    {
                        Platform.Instance.SetFullscreen(false);
                    }
                    else
                    {
                        Sdl?.SetWindowFullscreen(_window, 0);

                        var settings = SharedResources.Settings!;
                        var eset = SharedResources.Eset!;
                        float ds = settings.DisplayScale;
                        int scaledMinW = (int)(eset.Resolutions.MinScreenW * ds);
                        int scaledMinH = (int)(eset.Resolutions.MinScreenH * ds);
                        Sdl?.SetWindowMinimumSize(_window, scaledMinW, scaledMinH);
                        Sdl?.SetWindowSize(_window, scaledMinW, scaledMinH);
                        WindowResize();
                        Sdl?.SetWindowPosition(_window, SdlHardwareConstants.WindowposCentered, SdlHardwareConstants.WindowposCentered);
                    }
                    Fullscreen = false;
                }
                WindowResize();
            }
        }

        public override ushort GetRefreshRate()
        {
            if (Sdl != null && Sdl.GetCurrentDisplayMode(0, out int w, out int h, out int refreshRate) == 0)
                return (ushort)refreshRate;
            return 0;
        }

        private static int LoadQueuedImage(object? data)
        {
            QueuedImage image = (QueuedImage)data!;
            Sdl?.LockMutex(image.Mutex);
            image.Surface = Sdl?.ImgLoad(image.LocFilename);
            image.LoadAttempted = true;
            Sdl?.CondSignal(image.Loaded);
            Sdl?.UnlockMutex(image.Mutex);
            return 0;
        }

        public override void LoadQueuedImages()
        {
            List<object?> threads = new List<object?>(new object?[ImageQueue.Count]);

            for (int i = 0; i < ImageQueue.Count; ++i)
            {
                string threadName = "Image queue: " + ImageQueue[i].Filename;
                ImageQueue[i].Mutex = Sdl?.CreateMutex();
                ImageQueue[i].Loaded = Sdl?.CreateCond();
                threads[i] = Sdl?.CreateThread(LoadQueuedImage, threadName, ImageQueue[i]);
            }

            for (int i = 0; i < ImageQueue.Count; ++i)
            {
                Sdl?.LockMutex(ImageQueue[i].Mutex);
                SDLHardwareImage image = new SDLHardwareImage(this, _renderer);

                if (ImageQueue[i].Surface == null && !ImageQueue[i].LoadAttempted)
                {
                    Sdl?.CondWait(ImageQueue[i].Loaded, ImageQueue[i].Mutex);
                }

                if (ImageQueue[i].Surface != null)
                {
                    image.Surface = Sdl?.CreateTextureFromSurface(_renderer, ImageQueue[i].Surface);
                    Sdl?.FreeSurface(ImageQueue[i].Surface);
                    ImageQueue[i].Surface = null;
                }

                if (image.Surface == null)
                {
                    image.Dispose();
                    if (ImageQueue[i].ErrorType != ErrorNone)
                        Utils.LogError("SDLHardwareRenderDevice: Couldn't load image: '%s'. %s", ImageQueue[i].Filename, Sdl?.ImgGetError() ?? string.Empty);

                    if (ImageQueue[i].ErrorType == ErrorExit)
                    {
                        Utils.LogErrorDialog("SDLHardwareRenderDevice: Couldn't load image: '%s'.\n%s", ImageQueue[i].Filename, Sdl?.ImgGetError() ?? string.Empty);
                        SharedResources.Mods!.ResetModConfig();
                        Utils.Exit(1);
                    }
                }
                else
                {
                    CacheStore(ImageQueue[i].Filename, image);
                    ImageQueueCleanup.Add(image);
                }

                Sdl?.UnlockMutex(ImageQueue[i].Mutex);

                Sdl?.DestroyMutex(ImageQueue[i].Mutex);
                Sdl?.DestroyCond(ImageQueue[i].Loaded);
                ImageQueue[i].Mutex = null;
                ImageQueue[i].Loaded = null;
            }

            for (int i = 0; i < threads.Count; ++i)
            {
                Sdl?.WaitThread(threads[i], out int threadReturn);
            }

            ImageQueue.Clear();
        }
    }
}
