// 对应 C++ 源：SDLSoftwareRenderDevice.h + SDLSoftwareRenderDevice.cpp
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// SDL 软件渲染后端所需的全部原生 API 抽象（SDL2 / SDL_image / SDL_ttf）。
    /// 规则 7：业务逻辑禁止直接 P/Invoke SDL2；具体绑定由后续 SDL 封装单元注入
    /// <see cref="SDLSoftwareRenderDevice.Sdl"/>。
    /// </summary>
    public interface ISdlSoftwareRenderService
    {
        bool SupportsDisplayDpi { get; }

        string GetCurrentVideoDriver();
        int GetNumVideoDisplays();
        int GetDesktopDisplayMode(int displayIndex, out int w, out int h, out int refreshRate);
        int GetCurrentDisplayMode(int displayIndex, out int w, out int h, out int refreshRate);
        int GetDisplayDpi(int displayIndex, out float ddpi, out float hdpi, out float vdpi);
        string GetError();

        void SetHintWithPriority(string name, string value, int priority);
        object? CreateWindow(string title, int x, int y, int w, int h, uint flags);
        void DestroyWindow(object? window);
        object? CreateRenderer(object? window, int index, uint flags);
        void DestroyRenderer(object? renderer);
        void GetRendererInfo(object? renderer, out string name);
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

        void RenderSetLogicalSize(object? renderer, int w, int h);
        object? CreateTexture(object? renderer, uint pixelFormat, int access, int w, int h);
        void DestroyTexture(object? texture);
        void UpdateTexture(object? texture, Rectangle? srcRect, object? surface, int pitch);
        void RenderClear(object? renderer);
        int RenderCopy(object? renderer, object? texture, Rectangle? src, Rectangle? dest);
        void RenderPresent(object? renderer);

        object? CreateRgbSurface(uint flags, int width, int height, int depth, uint rmask, uint gmask, uint bmask, uint amask);
        object? ConvertSurfaceFormat(object? surface, uint pixelFormat, uint flags);
        void FreeSurface(object? surface);
        int FillRect(object? surface, Rectangle? rect, uint color);
        uint MapRgba(object? surface, byte r, byte g, byte b, byte a);
        int GetSurfaceWidth(object? surface);
        int GetSurfaceHeight(object? surface);
        int GetSurfaceBytesPerPixel(object? surface);
        int GetSurfacePitch(object? surface);
        void GetSurfaceCreateFormat(object? surface, out uint flags, out int bitsPerPixel, out uint rmask, out uint gmask, out uint bmask, out uint amask);
        bool MustLockSurface(object? surface);
        void LockSurface(object? surface);
        void UnlockSurface(object? surface);
        void WriteSurfaceByte(object? surface, int byteOffset, byte value);
        void WriteSurfaceUInt16(object? surface, int byteOffset, ushort value);
        void WriteSurfaceUInt32(object? surface, int byteOffset, uint value);
        void SetSurfaceBlendMode(object? surface, int blendMode);
        void SetSurfaceColorMod(object? surface, byte r, byte g, byte b);
        void SetSurfaceAlphaMod(object? surface, byte alpha);
        int BlitSurface(object? src, Rectangle srcRect, object? dst, ref Rectangle dstRect);
        int BlitScaled(object? src, Rectangle? srcRect, object? dst, Rectangle? dstRect);
        void PixelFormatEnumToMasks(uint pixelFormat, ref int bpp, out uint rmask, out uint gmask, out uint bmask, out uint amask);

        object? ImgLoad(string filename);
        string ImgGetError();

        object? TtfRenderUtf8Blended(object? font, string text, Color color);
        object? TtfRenderUtf8Solid(object? font, string text, Color color);

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

    /// <summary>SDL2 混合模式、窗口/渲染器标志等常量（与 SDL2 头文件取值一致）。</summary>
    public static class SdlSoftwareConstants
    {
        public const int BlendModeBlend = 1;
        public const int BlendModeAdd = 2;

        public const uint WindowFullscreenDesktop = 0x00001001;
        public const uint WindowResizable = 0x00000020;
        public const int WindowposCentered = 0x2FFF0000;

        public const uint RendererAccelerated = 0x00000002;
        public const uint RendererSoftware = 0x00000001;
        public const uint RendererPresentVsync = 0x00000004;

        public const int HintOverride = 3;
        public const string HintRenderScaleQuality = "SDL_RENDER_SCALE_QUALITY";

        public const uint PixelFormatArgb8888 = 0x16362004;
        public const int TextureAccessStreaming = 1;
    }

    /// <summary>
    /// 基于 SDL_Surface CPU 缓冲的 <see cref="Image"/> 实现（对应 C++ <c>SDLSoftwareImage</c>）。
    /// </summary>
    public class SDLSoftwareImage : Image
    {
        public object? Surface;

        public SDLSoftwareImage(RenderDevice device)
            : base(device)
        {
            Surface = null;
        }

        internal override void ReleaseNativeResources()
        {
            if (Surface != null)
            {
                SDLSoftwareRenderDevice.Sdl?.FreeSurface(Surface);
                Surface = null;
            }
        }

        public override int GetWidth()
        {
            return Surface != null ? (SDLSoftwareRenderDevice.Sdl?.GetSurfaceWidth(Surface) ?? 0) : 0;
        }

        public override int GetHeight()
        {
            return Surface != null ? (SDLSoftwareRenderDevice.Sdl?.GetSurfaceHeight(Surface) ?? 0) : 0;
        }

        public override void FillWithColor(Color color)
        {
            if (Surface == null) return;

            SDLSoftwareRenderDevice.Sdl?.FillRect(Surface, null, MapRgba(color.R, color.G, color.B, color.A));
        }

        public override void DrawPixel(int x, int y, Color color)
        {
            if (Surface == null) return;

            if (x < 0 || y < 0 || x >= GetWidth() || y >= GetHeight())
                return;

            ISdlSoftwareRenderService? sdl = SDLSoftwareRenderDevice.Sdl;
            if (sdl == null) return;

            uint pixel = MapRgba(color.R, color.G, color.B, color.A);

            int bpp = sdl.GetSurfaceBytesPerPixel(Surface);
            int p = y * sdl.GetSurfacePitch(Surface) + x * bpp;

            if (sdl.MustLockSurface(Surface))
            {
                sdl.LockSurface(Surface);
            }
            switch (bpp)
            {
                case 1:
                    sdl.WriteSurfaceByte(Surface, p, (byte)pixel);
                    break;

                case 2:
                    sdl.WriteSurfaceUInt16(Surface, p, (ushort)pixel);
                    break;

                case 3:
                    if (!BitConverter.IsLittleEndian)
                    {
                        sdl.WriteSurfaceByte(Surface, p + 0, (byte)((pixel >> 16) & 0xff));
                        sdl.WriteSurfaceByte(Surface, p + 1, (byte)((pixel >> 8) & 0xff));
                        sdl.WriteSurfaceByte(Surface, p + 2, (byte)(pixel & 0xff));
                    }
                    else
                    {
                        sdl.WriteSurfaceByte(Surface, p + 0, (byte)(pixel & 0xff));
                        sdl.WriteSurfaceByte(Surface, p + 1, (byte)((pixel >> 8) & 0xff));
                        sdl.WriteSurfaceByte(Surface, p + 2, (byte)((pixel >> 16) & 0xff));
                    }
                    break;

                case 4:
                    sdl.WriteSurfaceUInt32(Surface, p, pixel);
                    break;
            }
            if (sdl.MustLockSurface(Surface))
            {
                sdl.UnlockSurface(Surface);
            }
        }

        public override void DrawLine(int x0, int y0, int x1, int y1, Color color)
        {
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            int maxWidth = GetWidth();
            int maxHeight = GetHeight();

            do
            {
                if (x0 > 0 && y0 > 0 && x0 < maxWidth && y0 < maxHeight)
                {
                    DrawPixel(x0, y0, color);
                }

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err = err - dy;
                    x0 = x0 + sx;
                }
                if (e2 < dx)
                {
                    err = err + dx;
                    y0 = y0 + sy;
                }
            }
            while (x0 != x1 || y0 != y1);
        }

        public override void DrawFilledRect(int x, int y, int w, int h, Color color)
        {
            Rectangle rect = new Rectangle(x, y, w, h);

            SDLSoftwareRenderDevice.Sdl?.FillRect(Surface, rect, SDLSoftwareRenderDevice.Sdl?.MapRgba(Surface, color.R, color.G, color.B, color.A) ?? 0);
        }

        public override Image? Resize(int width, int height)
        {
            if (Surface == null || width <= 0 || height <= 0)
                return null;

            SDLSoftwareImage? scaled = new SDLSoftwareImage(_device);

            if (scaled != null)
            {
                ISdlSoftwareRenderService? sdl = SDLSoftwareRenderDevice.Sdl;
                uint surfaceFlags = 0;
                int bitsPerPixel = 0;
                uint rmask = 0;
                uint gmask = 0;
                uint bmask = 0;
                uint amask = 0;
                if (sdl != null)
                    sdl.GetSurfaceCreateFormat(Surface, out surfaceFlags, out bitsPerPixel, out rmask, out gmask, out bmask, out amask);

                scaled.Surface = sdl?.CreateRgbSurface(surfaceFlags, width, height,
                    bitsPerPixel,
                    rmask, gmask, bmask, amask);

                if (scaled.Surface != null)
                {
                    sdl?.BlitScaled(Surface, null, scaled.Surface, null);

                    Unref();
                    return scaled;
                }
                else
                {
                    scaled = null;
                }
            }

            return null;
        }

        private uint MapRgba(byte r, byte g, byte b, byte a)
        {
            if (Surface == null) return 0;
            return SDLSoftwareRenderDevice.Sdl?.MapRgba(Surface, r, g, b, a) ?? 0;
        }
    }

    /// <summary>
    /// 基于 SDL_BlitSurface CPU 后端的 FLARE 渲染设备（对应 C++ <c>SDLSoftwareRenderDevice</c>）。
    /// </summary>
    public class SDLSoftwareRenderDevice : RenderDevice
    {
        /// <summary>
        /// SDL 软件渲染原生 API 注入点。实现尚未就绪前为 null，各调用点通过 <c>?.</c> 保持中性行为。
        /// </summary>
        public static ISdlSoftwareRenderService? Sdl { get; set; }

        private object? _screen;
        private object? _window;
        private object? _renderer;
        private object? _texture;
        private object? _titlebarIcon;
        private string? _title;
        private uint _backgroundColor;

        private readonly ushort[] _gammaR = new ushort[256];
        private readonly ushort[] _gammaG = new ushort[256];
        private readonly ushort[] _gammaB = new ushort[256];

        public SDLSoftwareRenderDevice()
        {
            _screen = null;
            _window = null;
            _renderer = null;
            _texture = null;
            _titlebarIcon = null;
            _title = null;
            _backgroundColor = 0;

            Utils.LogInfo("RenderDevice: Using SDLSoftwareRenderDevice (software, SDL 2, %s)", Sdl?.GetCurrentVideoDriver() ?? string.Empty);

            Fullscreen = SharedResources.Settings!.Fullscreen;
            Hwsurface = SharedResources.Settings!.Hwsurface;
            Vsync = SharedResources.Settings!.Vsync;
            TextureFilter = SharedResources.Settings!.TextureFilter;

            MinScreen.X = SharedResources.Eset!.Resolutions.MinScreenW;
            MinScreen.Y = SharedResources.Eset!.Resolutions.MinScreenH;

            if (Sdl?.GetDesktopDisplayMode(0, out int desktopW, out int desktopH, out int desktopRefresh) == 0)
            {
                Utils.LogInfo("RenderDevice: %d display(s), using display 0 (%dx%d @ %dhz)", Sdl.GetNumVideoDisplays(), desktopW, desktopH, desktopRefresh);
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
            if (SharedResources.Settings!.SafeVideo)
            {
                SharedResources.Settings!.SafeVideo = false;
                Utils.LogInfo("RenderDevice: Safe mode is enabled. Using minimum video settings.");
                SharedResources.Settings!.Fullscreen = false;
                SharedResources.Settings!.Hwsurface = false;
                SharedResources.Settings!.Vsync = false;
                SharedResources.Settings!.TextureFilter = false;
                SharedResources.Settings!.ScreenW = SharedResources.Eset!.Resolutions.MinScreenW;
                SharedResources.Settings!.ScreenH = SharedResources.Eset!.Resolutions.MinScreenH;
            }

            bool settingsChanged = ((Fullscreen != SharedResources.Settings!.Fullscreen && DestructiveFullscreen) ||
                                    Hwsurface != SharedResources.Settings!.Hwsurface ||
                                    Vsync != SharedResources.Settings!.Vsync ||
                                    TextureFilter != SharedResources.Settings!.TextureFilter ||
                                    IgnoreTextureFilter != SharedResources.Eset!.Resolutions.IgnoreTextureFilter);

            uint wFlags = 0;
            uint rFlags = 0;
            int windowW = SharedResources.Settings!.ScreenW;
            int windowH = SharedResources.Settings!.ScreenH;

            if (SharedResources.Settings!.Fullscreen)
            {
                wFlags = wFlags | SdlSoftwareConstants.WindowFullscreenDesktop;

                if (Sdl?.GetDesktopDisplayMode(0, out int desktopW, out int desktopH, out _) == 0)
                {
                    windowW = desktopW;
                    windowH = desktopH;
                }
            }
            else if (Fullscreen && IsInitialized)
            {
                windowW = SharedResources.Eset!.Resolutions.MinScreenW;
                windowH = SharedResources.Eset!.Resolutions.MinScreenH;
            }

            wFlags = wFlags | SdlSoftwareConstants.WindowResizable;

            if (SharedResources.Settings!.Hwsurface)
            {
                rFlags = rFlags | SdlSoftwareConstants.RendererAccelerated;
            }
            else
            {
                rFlags = rFlags | SdlSoftwareConstants.RendererSoftware;
                SharedResources.Settings!.Vsync = false;
            }
            if (SharedResources.Settings!.Vsync) rFlags = rFlags | SdlSoftwareConstants.RendererPresentVsync;

            if (settingsChanged || !IsInitialized)
            {
                DestroyContext();

                _window = Sdl?.CreateWindow(string.Empty, SdlSoftwareConstants.WindowposCentered, SdlSoftwareConstants.WindowposCentered, windowW, windowH, wFlags);
                if (_window != null)
                {
                    _renderer = Sdl?.CreateRenderer(_window, -1, rFlags);
                    if (_renderer != null)
                    {
                        if (SharedResources.Settings!.TextureFilter && !SharedResources.Eset!.Resolutions.IgnoreTextureFilter)
                            Sdl?.SetHintWithPriority(SdlSoftwareConstants.HintRenderScaleQuality, "1", SdlSoftwareConstants.HintOverride);
                        else
                            Sdl?.SetHintWithPriority(SdlSoftwareConstants.HintRenderScaleQuality, "0", SdlSoftwareConstants.HintOverride);
                    }

                    Sdl?.SetWindowMinimumSize(_window, SharedResources.Eset!.Resolutions.MinScreenW, SharedResources.Eset!.Resolutions.MinScreenH);
                    Sdl?.SetWindowPosition(_window, SdlSoftwareConstants.WindowposCentered, SdlSoftwareConstants.WindowposCentered);

                    Sdl?.SetWindowSize(_window, windowW, windowH);
                }

                if (_window != null && _renderer != null)
                {
                    if (!IsInitialized)
                    {
                        Sdl?.GetWindowGammaRamp(_window, _gammaR, _gammaG, _gammaB);
                        Utils.LogInfo("RenderDevice: Window size is %dx%d", SharedResources.Settings!.ScreenW, SharedResources.Settings!.ScreenH);
                    }

                    Fullscreen = SharedResources.Settings!.Fullscreen;
                    Hwsurface = SharedResources.Settings!.Hwsurface;
                    Vsync = SharedResources.Settings!.Vsync;
                    TextureFilter = SharedResources.Settings!.TextureFilter;
                    IgnoreTextureFilter = SharedResources.Eset!.Resolutions.IgnoreTextureFilter;
                    IsInitialized = true;

                    Utils.LogInfo("RenderDevice: Fullscreen=%d, Hardware surfaces=%d, Vsync=%d, Texture Filter=%d", Fullscreen ? 1 : 0, Hwsurface ? 1 : 0, Vsync ? 1 : 0, TextureFilter ? 1 : 0);

                    string rendererName = "";
                    Sdl?.GetRendererInfo(_renderer, out rendererName);
                    Utils.LogInfo("RenderDevice: Renderer driver is '%s'.", rendererName);

                    if (Sdl?.SupportsDisplayDpi ?? false)
                    {
                        Sdl.GetDisplayDpi(0, out float ddpiOut, out _, out _);
                        Ddpi = ddpiOut;
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
                if (MinScreen.X != SharedResources.Eset!.Resolutions.MinScreenW || MinScreen.Y != SharedResources.Eset!.Resolutions.MinScreenH)
                {
                    MinScreen.X = SharedResources.Eset!.Resolutions.MinScreenW;
                    MinScreen.Y = SharedResources.Eset!.Resolutions.MinScreenH;
                    Sdl?.SetWindowMinimumSize(_window, SharedResources.Eset!.Resolutions.MinScreenW, SharedResources.Eset!.Resolutions.MinScreenH);
                    Sdl?.SetWindowPosition(_window, SdlSoftwareConstants.WindowposCentered, SdlSoftwareConstants.WindowposCentered);
                }

                WindowResize();
                IsInitialized = (_screen != null && _texture != null);
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

                if (SharedResources.Settings!.ChangeGamma)
                    SetGamma(SharedResources.Settings!.Gamma);
                else
                {
                    ResetGamma();
                    SharedResources.Settings!.ChangeGamma = false;
                    SharedResources.Settings!.Gamma = 1.0f;
                }
            }

            return (IsInitialized ? 0 : -1);
        }

        protected override void CreateContextError()
        {
            Utils.LogError("SDLSoftwareRenderDevice: createContext() failed: %s", Sdl?.GetError() ?? string.Empty);
            Utils.LogErrorDialog("SDLSoftwareRenderDevice: createContext() failed: %s", Sdl?.GetError() ?? string.Empty);
        }

        public override int Render(Renderable r, ref Rectangle dest)
        {
            Rectangle src = r.Src;
            Rectangle destRect = dest;

            object? surface = ((SDLSoftwareImage)r.Image!).Surface;

            if (r.BlendMode == Renderable.BlendAdd)
            {
                Sdl?.SetSurfaceBlendMode(surface, SdlSoftwareConstants.BlendModeAdd);
            }
            else
            {
                Sdl?.SetSurfaceBlendMode(surface, SdlSoftwareConstants.BlendModeBlend);
            }

            Sdl?.SetSurfaceColorMod(surface, r.ColorMod.R, r.ColorMod.G, r.ColorMod.B);
            Sdl?.SetSurfaceAlphaMod(surface, r.AlphaMod);

            return Sdl?.BlitSurface(surface, src, _screen, ref destRect) ?? -1;
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

            Rectangle src = MClip;
            Rectangle destRect = MDest;

            object? surface = ((SDLSoftwareImage)r.GetGraphics()!).Surface;
            Sdl?.SetSurfaceColorMod(surface, r.ColorMod.R, r.ColorMod.G, r.ColorMod.B);
            Sdl?.SetSurfaceAlphaMod(surface, r.AlphaMod);

            return Sdl?.BlitSurface(surface, src, _screen, ref destRect) ?? -1;
        }

        public override int RenderToImage(Image srcImage, Rectangle src, Image destImage, Rectangle dest)
        {
            if (srcImage == null || destImage == null) return -1;

            Rectangle srcRect = src;
            Rectangle destRect = dest;

            return Sdl?.BlitSurface(((SDLSoftwareImage)srcImage).Surface, srcRect,
                ((SDLSoftwareImage)destImage).Surface, ref destRect) ?? -1;
        }

        public override Image? RenderTextToImage(FontStyle fontStyle, string text, Color color, bool blended)
        {
            SDLSoftwareImage image = new SDLSoftwareImage(this);

            SDLFontStyle? sdlFont = fontStyle as SDLFontStyle;
            if (sdlFont == null)
                return null;

            if (blended)
                image.Surface = Sdl?.TtfRenderUtf8Blended(sdlFont.Ttfont, text, color);
            else
                image.Surface = Sdl?.TtfRenderUtf8Solid(sdlFont.Ttfont, text, color);

            if (image.Surface != null)
                return image;

            return null;
        }

        public override void DrawPixel(int x, int y, Color color)
        {
            ISdlSoftwareRenderService? sdl = Sdl;
            if (sdl == null) return;

            uint pixel = MapRgba(color.R, color.G, color.B, color.A);

            int bpp = sdl.GetSurfaceBytesPerPixel(_screen);
            int p = y * sdl.GetSurfacePitch(_screen) + x * bpp;

            if (sdl.MustLockSurface(_screen))
            {
                sdl.LockSurface(_screen);
            }
            switch (bpp)
            {
                case 1:
                    sdl.WriteSurfaceByte(_screen, p, (byte)pixel);
                    break;

                case 2:
                    sdl.WriteSurfaceUInt16(_screen, p, (ushort)pixel);
                    break;

                case 3:
                    if (!BitConverter.IsLittleEndian)
                    {
                        sdl.WriteSurfaceByte(_screen, p + 0, (byte)((pixel >> 16) & 0xff));
                        sdl.WriteSurfaceByte(_screen, p + 1, (byte)((pixel >> 8) & 0xff));
                        sdl.WriteSurfaceByte(_screen, p + 2, (byte)(pixel & 0xff));
                    }
                    else
                    {
                        sdl.WriteSurfaceByte(_screen, p + 0, (byte)(pixel & 0xff));
                        sdl.WriteSurfaceByte(_screen, p + 1, (byte)((pixel >> 8) & 0xff));
                        sdl.WriteSurfaceByte(_screen, p + 2, (byte)((pixel >> 16) & 0xff));
                    }
                    break;

                case 4:
                    sdl.WriteSurfaceUInt32(_screen, p, pixel);
                    break;
            }
            if (sdl.MustLockSurface(_screen))
            {
                sdl.UnlockSurface(_screen);
            }

            return;
        }

        public override void DrawLine(int x0, int y0, int x1, int y1, Color color)
        {
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            do
            {
                if (x0 > 0 && y0 > 0 && x0 < SharedResources.Settings!.ViewW && y0 < SharedResources.Settings!.ViewH)
                {
                    DrawPixel(x0, y0, color);
                }

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err = err - dy;
                    x0 = x0 + sx;
                }
                if (e2 < dx)
                {
                    err = err + dx;
                    y0 = y0 + sy;
                }
            }
            while (x0 != x1 || y0 != y1);
        }

        public override void DrawRectangle(Int2 p0, Int2 p1, Color color)
        {
            if (Sdl?.MustLockSurface(_screen) ?? false)
            {
                Sdl.LockSurface(_screen);
            }
            DrawLine(p0.X, p0.Y, p1.X, p0.Y, color);
            DrawLine(p1.X, p0.Y, p1.X, p1.Y, color);
            DrawLine(p0.X, p0.Y, p0.X, p1.Y, color);
            DrawLine(p0.X, p1.Y, p1.X + 1, p1.Y, color);
            if (Sdl?.MustLockSurface(_screen) ?? false)
            {
                Sdl.UnlockSurface(_screen);
            }
        }

        public override void BlankScreen()
        {
            Sdl?.FillRect(_screen, null, _backgroundColor);
            return;
        }

        public override void CommitFrame()
        {
            int pitch = Sdl?.GetSurfacePitch(_screen) ?? 0;
            Sdl?.UpdateTexture(_texture, null, _screen, pitch);
            Sdl?.RenderClear(_renderer);
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

            if (_title != null)
            {
                _title = null;
            }
            if (_titlebarIcon != null)
            {
                Sdl?.FreeSurface(_titlebarIcon);
                _titlebarIcon = null;
            }
            if (_screen != null)
            {
                Sdl?.FreeSurface(_screen);
                _screen = null;
            }
            if (_texture != null)
            {
                Sdl?.DestroyTexture(_texture);
                _texture = null;
            }
            if (_renderer != null)
            {
                Sdl?.DestroyRenderer(_renderer);
                _renderer = null;
            }
            if (_window != null)
            {
                Sdl?.DestroyWindow(_window);
                _window = null;
            }

            return;
        }

        public override Image? CreateImage(int width, int height)
        {
            SDLSoftwareImage image = new SDLSoftwareImage(this);

            uint rmask = 0;
            uint gmask = 0;
            uint bmask = 0;
            uint amask = 0;
            Utils.SetSdlRgba(out rmask, out gmask, out bmask, out amask);

            image.Surface = Sdl?.CreateRgbSurface(0, width, height, RenderDevice.BitsPerPixel, rmask, gmask, bmask, amask);

            if (image.Surface == null)
            {
                Utils.LogError("SDLSoftwareRenderDevice: CreateRGBSurface failed: %s", Sdl?.GetError() ?? string.Empty);
                return null;
            }

            object? cleanup = image.Surface;
            image.Surface = Sdl?.ConvertSurfaceFormat(cleanup, SdlSoftwareConstants.PixelFormatArgb8888, 0);
            Sdl?.FreeSurface(cleanup);

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
            if (_title != null)
            {
                _title = null;
            }
            if (_titlebarIcon != null) Sdl?.FreeSurface(_titlebarIcon);

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

            SDLSoftwareImage? image;
            image = null;
            object? cleanup = Sdl?.ImgLoad(SharedResources.Mods!.Locate(filename));
            if (cleanup == null)
            {
                if (errorType != ErrorNone)
                    Utils.LogError("SDLSoftwareRenderDevice: Couldn't load image: '%s'. %s", filename, Sdl?.ImgGetError() ?? string.Empty);

                if (errorType == ErrorExit)
                {
                    Utils.LogErrorDialog("SDLSoftwareRenderDevice: Couldn't load image: '%s'.\n%s", filename, Sdl?.ImgGetError() ?? string.Empty);
                    SharedResources.Mods!.ResetModConfig();
                    Utils.Exit(1);
                }

                return null;
            }
            else
            {
                image = new SDLSoftwareImage(this);
                image.Surface = Sdl?.ConvertSurfaceFormat(cleanup, SdlSoftwareConstants.PixelFormatArgb8888, 0);
                Sdl?.FreeSurface(cleanup);
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
            WindowResizeInternal();

            Sdl?.RenderSetLogicalSize(_renderer, SharedResources.Settings!.ViewW, SharedResources.Settings!.ViewH);

            if (_texture != null) Sdl?.DestroyTexture(_texture);
            if (_screen != null) Sdl?.FreeSurface(_screen);

            uint rmask = 0;
            uint gmask = 0;
            uint bmask = 0;
            uint amask = 0;
            int bpp = (int)RenderDevice.BitsPerPixel;
            Sdl?.PixelFormatEnumToMasks(SdlSoftwareConstants.PixelFormatArgb8888, ref bpp, out rmask, out gmask, out bmask, out amask);
            _screen = Sdl?.CreateRgbSurface(0, SharedResources.Settings!.ViewW, SharedResources.Settings!.ViewH, bpp, rmask, gmask, bmask, amask);
            _texture = Sdl?.CreateTexture(_renderer, SdlSoftwareConstants.PixelFormatArgb8888, SdlSoftwareConstants.TextureAccessStreaming, SharedResources.Settings!.ViewW, SharedResources.Settings!.ViewH);

            SharedResources.Settings!.UpdateScreenVars();
        }

        public override void SetBackgroundColor(Color color)
        {
            _backgroundColor = Sdl?.MapRgba(_screen, color.R, color.G, color.B, 255) ?? 0;
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
                        Sdl?.SetWindowFullscreen(_window, SdlSoftwareConstants.WindowFullscreenDesktop);
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

                        Sdl?.SetWindowMinimumSize(_window, SharedResources.Eset!.Resolutions.MinScreenW, SharedResources.Eset!.Resolutions.MinScreenH);
                        Sdl?.SetWindowSize(_window, SharedResources.Eset!.Resolutions.MinScreenW, SharedResources.Eset!.Resolutions.MinScreenH);
                        WindowResize();
                        Sdl?.SetWindowPosition(_window, SdlSoftwareConstants.WindowposCentered, SdlSoftwareConstants.WindowposCentered);
                    }
                    Fullscreen = false;
                }
                WindowResize();
            }
        }

        public override ushort GetRefreshRate()
        {
            if (Sdl?.GetCurrentDisplayMode(0, out _, out _, out int refreshRate) == 0)
                return (ushort)refreshRate;
            return 0;
        }

        private static int LoadQueuedImage(object? data)
        {
            QueuedImage image = (QueuedImage)data!;
            SDLSoftwareRenderDevice.Sdl?.LockMutex(image.Mutex);
            image.Surface = SDLSoftwareRenderDevice.Sdl?.ImgLoad(image.LocFilename);
            image.LoadAttempted = true;
            SDLSoftwareRenderDevice.Sdl?.CondSignal(image.Loaded);
            SDLSoftwareRenderDevice.Sdl?.UnlockMutex(image.Mutex);
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
                SDLSoftwareImage image = new SDLSoftwareImage(this);

                if (ImageQueue[i].Surface == null && !ImageQueue[i].LoadAttempted)
                {
                    Sdl?.CondWait(ImageQueue[i].Loaded, ImageQueue[i].Mutex);
                }

                if (ImageQueue[i].Surface != null)
                {
                    image.Surface = ImageQueue[i].Surface;
                }

                if (image.Surface == null)
                {
                    if (ImageQueue[i].ErrorType != ErrorNone)
                        Utils.LogError("SDLSoftwareRenderDevice: Couldn't load image: '%s'. %s", ImageQueue[i].Filename, Sdl?.ImgGetError() ?? string.Empty);

                    if (ImageQueue[i].ErrorType == ErrorExit)
                    {
                        Utils.LogErrorDialog("SDLSoftwareRenderDevice: Couldn't load image: '%s'.\n%s", ImageQueue[i].Filename, Sdl?.ImgGetError() ?? string.Empty);
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

        private uint MapRgba(byte r, byte g, byte b, byte a)
        {
            return Sdl?.MapRgba(_screen, r, g, b, a) ?? 0;
        }
    }
}
