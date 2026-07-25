using System.Runtime.InteropServices;
using SDL3;
using Stride.Core.Mathematics;

namespace FlareEngine.Sdl
{
    /// <summary>
    /// 基于 SDL3-CS 的硬件/软件渲染原生 API 实现。
    /// 方法语义分别对应 C++ <c>SDLHardwareRenderDevice.cpp</c> 与
    /// <c>SDLSoftwareRenderDevice.cpp</c> 中直接调用的 SDL / SDL_image / SDL_ttf API。
    /// </summary>
    public sealed class Sdl3RenderService : ISdlHardwareRenderService, ISdlSoftwareRenderService
    {
        /// <summary>SDL 线程入口上下文，由 <see cref="GCHandle"/> 持有直至 <see cref="WaitThread"/>。</summary>
        private sealed class ThreadContext
        {
            public SdlThreadEntry Callback = null!;
            public object? Userdata;
        }

        public bool SupportsDisplayDpi => true;

        // --- 版本/显示信息 ---

        public string GetCurrentVideoDriver()
        {
            return SDL.GetCurrentVideoDriver() ?? string.Empty;
        }

        public int GetNumVideoDisplays()
        {
            int count = 0;
            SDL.GetDisplays(out count);
            return count;
        }

        public int GetDesktopDisplayMode(int displayIndex, out int w, out int h, out int refreshRate)
        {
            w = 0;
            h = 0;
            refreshRate = 0;
            uint displayId = GetDisplayId(displayIndex);
            if (displayId == 0)
                return -1;

            SDL.DisplayMode? mode = SDL.GetDesktopDisplayMode(displayId);
            if (!mode.HasValue)
                return -1;

            w = mode.Value.W;
            h = mode.Value.H;
            refreshRate = (int)mode.Value.RefreshRate;
            return 0;
        }

        public int GetCurrentDisplayMode(int displayIndex, out int w, out int h, out int refreshRate)
        {
            w = 0;
            h = 0;
            refreshRate = 0;
            uint displayId = GetDisplayId(displayIndex);
            if (displayId == 0)
                return -1;

            SDL.DisplayMode? mode = SDL.GetCurrentDisplayMode(displayId);
            if (!mode.HasValue)
                return -1;

            w = mode.Value.W;
            h = mode.Value.H;
            refreshRate = (int)mode.Value.RefreshRate;
            return 0;
        }

        public int GetDisplayDpi(int displayIndex, out float ddpi, out float hdpi, out float vdpi)
        {
            ddpi = 0;
            hdpi = 0;
            vdpi = 0;
            uint displayId = GetDisplayId(displayIndex);
            if (displayId == 0)
                return -1;

            float scale = SDL.GetDisplayContentScale(displayId);
            ddpi = hdpi = vdpi = scale * 96.0f;
            return 0;
        }

        public string GetError()
        {
            return SDL.GetError() ?? string.Empty;
        }

        // --- Hint / 窗口 / 渲染器 ---

        public void SetHintWithPriority(string name, string value, int priority)
        {
            SDL.SetHintWithPriority(name, value, MapHintPriority(priority));
        }

        public object? CreateWindow(string title, int x, int y, int w, int h, uint flags)
        {
            IntPtr window = SDL.CreateWindow(title, w, h, Sdl3Interop.MapWindowFlags(flags));
            if (window == IntPtr.Zero)
                return null;

            SDL.ShowWindow(window);

            int posX = x == SdlHardwareConstants.WindowposCentered ? (int)SDL.WindowPosCentered() : x;
            int posY = y == SdlHardwareConstants.WindowposCentered ? (int)SDL.WindowPosCentered() : y;
            SDL.SetWindowPosition(window, posX, posY);

            return new SdlHandle(window);
        }

        public void DestroyWindow(object? window)
        {
            IntPtr ptr = SdlHandle.Require(window);
            if (ptr != IntPtr.Zero)
                SDL.DestroyWindow(ptr);
        }

        public object? CreateRenderer(object? window, int index, uint flags)
        {
            IntPtr windowPtr = SdlHandle.Require(window);
            if (windowPtr == IntPtr.Zero)
                return null;

            string? driverName = index >= 0 ? SDL.GetRenderDriver(index) : null;
            IntPtr renderer;

            if ((flags & SdlHardwareConstants.RendererSoftware) != 0)
            {
                renderer = SDL.CreateRenderer(windowPtr, driverName ?? "software");
            }
            else
            {
                renderer = SDL.CreateRenderer(windowPtr, driverName);
            }

            if (renderer == IntPtr.Zero)
                return null;

            bool vsync = (flags & SdlHardwareConstants.RendererPresentVsync) != 0;
            SDL.SetRenderVSync(renderer, vsync ? 1 : 0);

            Sdl3RenderLifetime.IsRendererActive = true;
            return new SdlHandle(renderer);
        }

        public void DestroyRenderer(object? renderer)
        {
            IntPtr ptr = SdlHandle.Require(renderer);
            if (ptr != IntPtr.Zero)
            {
                if (Sdl3EventCoordinatesContext.Renderer == ptr)
                    Sdl3EventCoordinatesContext.Clear();
                Sdl3RenderLifetime.IsRendererActive = false;
                SDL.DestroyRenderer(ptr);
            }
        }

        public void SetWindowMinimumSize(object? window, int minW, int minH)
        {
            SDL.SetWindowMinimumSize(SdlHandle.Require(window), minW, minH);
        }

        public void SetWindowPosition(object? window, int x, int y)
        {
            int posX = x == SdlHardwareConstants.WindowposCentered ? (int)SDL.WindowPosCentered() : x;
            int posY = y == SdlHardwareConstants.WindowposCentered ? (int)SDL.WindowPosCentered() : y;
            SDL.SetWindowPosition(SdlHandle.Require(window), posX, posY);
        }

        public void SetWindowSize(object? window, int w, int h)
        {
            SDL.SetWindowSize(SdlHandle.Require(window), w, h);
        }

        public void SetWindowFullscreen(object? window, uint flags)
        {
            bool fullscreen = (flags & SdlHardwareConstants.WindowFullscreenDesktop) != 0;
            SDL.SetWindowFullscreen(SdlHandle.Require(window), fullscreen);
        }

        public void SetWindowTitle(object? window, string title)
        {
            SDL.SetWindowTitle(SdlHandle.Require(window), title);
        }

        public void SetWindowIcon(object? window, object? surface)
        {
            SDL.SetWindowIcon(SdlHandle.Require(window), SdlHandle.Require(surface));
        }

        public void GetWindowSize(object? window, out int w, out int h)
        {
            w = 0;
            h = 0;
            SDL.GetWindowSize(SdlHandle.Require(window), out w, out h);
        }

        public int GetWindowGammaRamp(object? window, ushort[] rampR, ushort[] rampG, ushort[] rampB)
        {
            return 0;
        }

        public int SetWindowGammaRamp(object? window, ushort[] rampR, ushort[] rampG, ushort[] rampB)
        {
            return 0;
        }

        public void CalculateGammaRamp(float gamma, ushort[] ramp)
        {
            Sdl3Interop.CalculateGammaRamp(gamma, ramp);
        }

        public void GetRendererInfo(object? renderer, out string name)
        {
            name = SDL.GetRendererName(SdlHandle.Require(renderer)) ?? string.Empty;
        }

        // --- 纹理 ---

        public void DestroyTexture(object? texture)
        {
            if (!Sdl3RenderLifetime.IsRendererActive)
                return;

            IntPtr ptr = SdlHandle.Require(texture);
            if (ptr != IntPtr.Zero)
                SDL.DestroyTexture(ptr);
        }

        public int QueryTexture(object? texture, out int w, out int h)
        {
            w = 0;
            h = 0;
            IntPtr ptr = SdlHandle.Require(texture);
            if (ptr == IntPtr.Zero)
                return -1;

            float fw = 0;
            float fh = 0;
            if (!SDL.GetTextureSize(ptr, out fw, out fh))
                return -1;

            w = (int)fw;
            h = (int)fh;
            return 0;
        }

        public object? CreateTexture(object? renderer, uint pixelFormat, int access, int w, int h)
        {
            IntPtr texture = SDL.CreateTexture(
                SdlHandle.Require(renderer),
                Sdl3Interop.MapPixelFormat(pixelFormat),
                Sdl3Interop.MapTextureAccess(access),
                w,
                h);
            return Wrap(texture);
        }

        public object? CreateTextureFromSurface(object? renderer, object? surface)
        {
            IntPtr texture = SDL.CreateTextureFromSurface(
                SdlHandle.Require(renderer),
                SdlHandle.Require(surface));
            return Wrap(texture);
        }

        public void SetTextureBlendMode(object? texture, int blendMode)
        {
            SDL.SetTextureBlendMode(SdlHandle.Require(texture), Sdl3Interop.MapBlendMode(blendMode));
        }

        public void SetTextureColorMod(object? texture, byte r, byte g, byte b)
        {
            SDL.SetTextureColorMod(SdlHandle.Require(texture), r, g, b);
        }

        public void SetTextureAlphaMod(object? texture, byte alpha)
        {
            SDL.SetTextureAlphaMod(SdlHandle.Require(texture), alpha);
        }

        // --- 渲染目标与绘制 ---

        public int SetRenderTarget(object? renderer, object? texture)
        {
            IntPtr texturePtr = texture == null ? IntPtr.Zero : SdlHandle.Require(texture);
            return SDL.SetRenderTarget(SdlHandle.Require(renderer), texturePtr) ? 0 : -1;
        }

        public void SetRenderDrawColor(object? renderer, byte r, byte g, byte b, byte a)
        {
            SDL.SetRenderDrawColor(SdlHandle.Require(renderer), r, g, b, a);
        }

        public void RenderClear(object? renderer)
        {
            SDL.RenderClear(SdlHandle.Require(renderer));
        }

        public void RenderDrawPoint(object? renderer, int x, int y)
        {
            SDL.RenderPoint(SdlHandle.Require(renderer), x, y);
        }

        public void RenderDrawLine(object? renderer, int x0, int y0, int x1, int y1)
        {
            SDL.RenderLine(SdlHandle.Require(renderer), x0, y0, x1, y1);
        }

        public void RenderFillRect(object? renderer, ref Rectangle rect)
        {
            SDL.FRect fRect = Sdl3Interop.ToSdlFRect(rect);
            SDL.RenderFillRect(SdlHandle.Require(renderer), ref fRect);
        }

        public int RenderCopy(object? renderer, object? texture, Rectangle? src, Rectangle? dest)
        {
            IntPtr r = SdlHandle.Require(renderer);
            IntPtr t = SdlHandle.Require(texture);

            if (src.HasValue && dest.HasValue)
            {
                SDL.FRect s = Sdl3Interop.ToSdlFRect(src.Value);
                SDL.FRect d = Sdl3Interop.ToSdlFRect(dest.Value);
                return SDL.RenderTexture(r, t, ref s, ref d) ? 0 : -1;
            }

            if (src.HasValue)
            {
                SDL.FRect s = Sdl3Interop.ToSdlFRect(src.Value);
                return SDL.RenderTexture(r, t, ref s, IntPtr.Zero) ? 0 : -1;
            }

            if (dest.HasValue)
            {
                SDL.FRect d = Sdl3Interop.ToSdlFRect(dest.Value);
                return SDL.RenderTexture(r, t, IntPtr.Zero, ref d) ? 0 : -1;
            }

            return SDL.RenderTexture(r, t, IntPtr.Zero, IntPtr.Zero) ? 0 : -1;
        }

        public int RenderCopyEx(object? renderer, object? texture, Rectangle? src, Rectangle? dest, double angle, Int2? center, int flip)
        {
            IntPtr r = SdlHandle.Require(renderer);
            IntPtr t = SdlHandle.Require(texture);

            SDL.FRect s = src.HasValue ? Sdl3Interop.ToSdlFRect(src.Value) : default;
            SDL.FRect d = dest.HasValue ? Sdl3Interop.ToSdlFRect(dest.Value) : default;
            SDL.FPoint c = center.HasValue
                ? new SDL.FPoint { X = center.Value.X, Y = center.Value.Y }
                : default;
            SDL.FlipMode flipMode = MapFlipMode(flip);

            if (src.HasValue && dest.HasValue)
            {
                return SDL.RenderTextureRotated(r, t, ref s, ref d, angle, ref c, flipMode) ? 0 : -1;
            }

            if (src.HasValue)
            {
                return SDL.RenderTextureRotated(r, t, ref s, IntPtr.Zero, angle, ref c, flipMode) ? 0 : -1;
            }

            if (dest.HasValue)
            {
                return SDL.RenderTextureRotated(r, t, IntPtr.Zero, ref d, angle, ref c, flipMode) ? 0 : -1;
            }

            return SDL.RenderTextureRotated(r, t, IntPtr.Zero, IntPtr.Zero, angle, ref c, flipMode) ? 0 : -1;
        }

        public void RenderPresent(object? renderer)
        {
            SDL.RenderPresent(SdlHandle.Require(renderer));
        }

        public void RenderSetLogicalSize(object? renderer, int w, int h)
        {
            IntPtr rendererPtr = SdlHandle.Require(renderer);
            SDL.SetRenderLogicalPresentation(
                rendererPtr,
                w,
                h,
                SDL.RendererLogicalPresentation.Stretch);
            // SDL3：注册渲染器供输入事件坐标转换（等价于 SDL2 logical size 下的自动映射）
            Sdl3EventCoordinatesContext.SetRenderer(rendererPtr);
        }

        // --- Surface（像素批处理） ---

        public void FreeSurface(object? surface)
        {
            IntPtr ptr = SdlHandle.Require(surface);
            if (ptr != IntPtr.Zero)
                SDL.DestroySurface(ptr);
        }

        public object? CreateRgbSurface(uint flags, int width, int height, int depth, uint rmask, uint gmask, uint bmask, uint amask)
        {
            SDL.PixelFormat format = SDL.GetPixelFormatForMasks(depth, rmask, gmask, bmask, amask);
            IntPtr surface = SDL.CreateSurface(width, height, format);
            return Wrap(surface);
        }

        public uint MapRgba(object? surface, byte r, byte g, byte b, byte a)
        {
            IntPtr surfacePtr = SdlHandle.Require(surface);
            ref SDL.Surface s = ref Sdl3Interop.SurfaceRef(surfacePtr);
            IntPtr formatDetails = SDL.GetPixelFormatDetails(s.Format);
            IntPtr palette = SDL.GetSurfacePalette(surfacePtr);
            return SDL.MapRGBA(formatDetails, palette, r, g, b, a);
        }

        public int GetSurfaceBytesPerPixel(object? surface)
        {
            ref SDL.Surface s = ref Sdl3Interop.SurfaceRef(SdlHandle.Require(surface));
            return (int)SDL.BytesPerPixel(s.Format);
        }

        public int GetSurfacePitch(object? surface)
        {
            return Sdl3Interop.SurfaceRef(SdlHandle.Require(surface)).Pitch;
        }

        public bool MustLockSurface(object? surface)
        {
            ref SDL.Surface s = ref Sdl3Interop.SurfaceRef(SdlHandle.Require(surface));
            return SDL.MustLock(s);
        }

        public void LockSurface(object? surface)
        {
            SDL.LockSurface(SdlHandle.Require(surface));
        }

        public void UnlockSurface(object? surface)
        {
            SDL.UnlockSurface(SdlHandle.Require(surface));
        }

        public int GetSurfacePixelByteOffset(object? surface, int x, int y)
        {
            ref SDL.Surface s = ref Sdl3Interop.SurfaceRef(SdlHandle.Require(surface));
            int bpp = (int)SDL.BytesPerPixel(s.Format);
            return y * s.Pitch + x * bpp;
        }

        public void WriteSurfaceByte(object? surface, int byteOffset, byte value)
        {
            Sdl3Interop.WriteSurfaceByte(SdlHandle.Require(surface), byteOffset, value);
        }

        public void WriteSurfaceUInt16(object? surface, int byteOffset, ushort value)
        {
            Sdl3Interop.WriteSurfaceUInt16(SdlHandle.Require(surface), byteOffset, value);
        }

        public void WriteSurfaceUInt32(object? surface, int byteOffset, uint value)
        {
            Sdl3Interop.WriteSurfaceUInt32(SdlHandle.Require(surface), byteOffset, value);
        }

        // --- 软件渲染专用 Surface 操作 ---

        public object? ConvertSurfaceFormat(object? surface, uint pixelFormat, uint flags)
        {
            IntPtr converted = SDL.ConvertSurface(
                SdlHandle.Require(surface),
                Sdl3Interop.MapPixelFormat(pixelFormat));
            return Wrap(converted);
        }

        public int FillRect(object? surface, Rectangle? rect, uint color)
        {
            IntPtr surfacePtr = SdlHandle.Require(surface);
            if (rect.HasValue)
            {
                SDL.Rect r = Sdl3Interop.ToSdlRect(rect.Value);
                return SDL.FillSurfaceRect(surfacePtr, ref r, color) ? 0 : -1;
            }

            return SDL.FillSurfaceRect(surfacePtr, IntPtr.Zero, color) ? 0 : -1;
        }

        public int GetSurfaceWidth(object? surface)
        {
            return Sdl3Interop.SurfaceRef(SdlHandle.Require(surface)).Width;
        }

        public int GetSurfaceHeight(object? surface)
        {
            return Sdl3Interop.SurfaceRef(SdlHandle.Require(surface)).Height;
        }

        public void GetSurfaceCreateFormat(object? surface, out uint flags, out int bitsPerPixel, out uint rmask, out uint gmask, out uint bmask, out uint amask)
        {
            ref SDL.Surface s = ref Sdl3Interop.SurfaceRef(SdlHandle.Require(surface));
            flags = (uint)s.Flags;
            bitsPerPixel = 0;
            rmask = 0;
            gmask = 0;
            bmask = 0;
            amask = 0;
            SDL.GetMasksForPixelFormat(s.Format, ref bitsPerPixel, ref rmask, ref gmask, ref bmask, ref amask);
        }

        public void SetSurfaceBlendMode(object? surface, int blendMode)
        {
            SDL.SetSurfaceBlendMode(SdlHandle.Require(surface), Sdl3Interop.MapBlendMode(blendMode));
        }

        public void SetSurfaceColorMod(object? surface, byte r, byte g, byte b)
        {
            SDL.SetSurfaceColorMod(SdlHandle.Require(surface), r, g, b);
        }

        public void SetSurfaceAlphaMod(object? surface, byte alpha)
        {
            SDL.SetSurfaceAlphaMod(SdlHandle.Require(surface), alpha);
        }

        public int BlitSurface(object? src, Rectangle srcRect, object? dst, ref Rectangle dstRect)
        {
            SDL.Rect srcR = Sdl3Interop.ToSdlRect(srcRect);
            SDL.Rect dstR = Sdl3Interop.ToSdlRect(dstRect);
            bool ok = SDL.BlitSurface(SdlHandle.Require(src), ref srcR, SdlHandle.Require(dst), ref dstR);
            return ok ? 0 : -1;
        }

        public int BlitScaled(object? src, Rectangle? srcRect, object? dst, Rectangle? dstRect)
        {
            IntPtr srcPtr = SdlHandle.Require(src);
            IntPtr dstPtr = SdlHandle.Require(dst);

            if (srcRect.HasValue && dstRect.HasValue)
            {
                SDL.Rect s = Sdl3Interop.ToSdlRect(srcRect.Value);
                SDL.Rect d = Sdl3Interop.ToSdlRect(dstRect.Value);
                return SDL.BlitSurfaceScaled(srcPtr, ref s, dstPtr, ref d, SDL.ScaleMode.Linear) ? 0 : -1;
            }

            if (srcRect.HasValue)
            {
                SDL.Rect s = Sdl3Interop.ToSdlRect(srcRect.Value);
                return SDL.BlitSurfaceScaled(srcPtr, ref s, dstPtr, IntPtr.Zero, SDL.ScaleMode.Linear) ? 0 : -1;
            }

            if (dstRect.HasValue)
            {
                SDL.Rect d = Sdl3Interop.ToSdlRect(dstRect.Value);
                return SDL.BlitSurfaceScaled(srcPtr, IntPtr.Zero, dstPtr, ref d, SDL.ScaleMode.Linear) ? 0 : -1;
            }

            return SDL.BlitSurfaceScaled(srcPtr, IntPtr.Zero, dstPtr, IntPtr.Zero, SDL.ScaleMode.Linear) ? 0 : -1;
        }

        public void PixelFormatEnumToMasks(uint pixelFormat, ref int bpp, out uint rmask, out uint gmask, out uint bmask, out uint amask)
        {
            rmask = 0;
            gmask = 0;
            bmask = 0;
            amask = 0;
            SDL.GetMasksForPixelFormat(Sdl3Interop.MapPixelFormat(pixelFormat), ref bpp, ref rmask, ref gmask, ref bmask, ref amask);
        }

        public void UpdateTexture(object? texture, Rectangle? srcRect, object? surface, int pitch)
        {
            IntPtr texturePtr = SdlHandle.Require(texture);
            IntPtr surfacePtr = SdlHandle.Require(surface);
            ref SDL.Surface s = ref Sdl3Interop.SurfaceRef(surfacePtr);
            IntPtr pixels = s.Pixels;

            if (srcRect.HasValue)
            {
                SDL.Rect rect = Sdl3Interop.ToSdlRect(srcRect.Value);
                SDL.UpdateTexture(texturePtr, ref rect, pixels, pitch);
            }
            else
            {
                SDL.UpdateTexture(texturePtr, IntPtr.Zero, pixels, pitch);
            }
        }

        // --- SDL_image ---

        public object? ImgLoad(string filename)
        {
            IntPtr surface = SDL3.Image.Load(filename);
            return Wrap(surface);
        }

        public object? ImgLoadTexture(object? renderer, string filename)
        {
            IntPtr texture = SDL3.Image.LoadTexture(SdlHandle.Require(renderer), filename);
            return Wrap(texture);
        }

        public string ImgGetError()
        {
            return SDL.GetError() ?? string.Empty;
        }

        // --- SDL_ttf ---

        public object? TtfRenderUtf8Blended(object? font, string text, Color color)
        {
            IntPtr fontPtr = Sdl3TtfService.GetFontPtr(font);
            if (fontPtr == IntPtr.Zero)
                return null;

            IntPtr surface = TTF.RenderTextBlended(fontPtr, text, 0, Sdl3Interop.ToSdlColor(color));
            return Wrap(surface);
        }

        public object? TtfRenderUtf8Solid(object? font, string text, Color color)
        {
            IntPtr fontPtr = Sdl3TtfService.GetFontPtr(font);
            if (fontPtr == IntPtr.Zero)
                return null;

            IntPtr surface = TTF.RenderTextSolid(fontPtr, text, 0, Sdl3Interop.ToSdlColor(color));
            return Wrap(surface);
        }

        // --- 线程同步 ---

        public object? CreateMutex()
        {
            IntPtr mutex = SDL.CreateMutex();
            return Wrap(mutex);
        }

        public void DestroyMutex(object? mutex)
        {
            IntPtr ptr = SdlHandle.Require(mutex);
            if (ptr != IntPtr.Zero)
                SDL.DestroyMutex(ptr);
        }

        public object? CreateCond()
        {
            IntPtr cond = SDL.CreateCondition();
            return Wrap(cond);
        }

        public void DestroyCond(object? cond)
        {
            IntPtr ptr = SdlHandle.Require(cond);
            if (ptr != IntPtr.Zero)
                SDL.DestroyCondition(ptr);
        }

        public void LockMutex(object? mutex)
        {
            SDL.LockMutex(SdlHandle.Require(mutex));
        }

        public void UnlockMutex(object? mutex)
        {
            SDL.UnlockMutex(SdlHandle.Require(mutex));
        }

        public void CondSignal(object? cond)
        {
            SDL.SignalCondition(SdlHandle.Require(cond));
        }

        public void CondWait(object? cond, object? mutex)
        {
            SDL.WaitCondition(SdlHandle.Require(cond), SdlHandle.Require(mutex));
        }

        public object? CreateThread(SdlThreadEntry callback, string threadName, object? userdata)
        {
            var context = new ThreadContext
            {
                Callback = callback,
                Userdata = userdata
            };
            GCHandle handle = GCHandle.Alloc(context);
            IntPtr thread = SDL.CreateThread(ThreadCallback, threadName, GCHandle.ToIntPtr(handle));
            if (thread == IntPtr.Zero)
            {
                handle.Free();
                return null;
            }

            return new SdlThreadHandle(thread, handle);
        }

        public void WaitThread(object? thread, out int threadReturnCode)
        {
            threadReturnCode = 0;
            if (thread is SdlThreadHandle native)
            {
                SDL.WaitThread(native.Thread, out threadReturnCode);
                if (native.ContextHandle.IsAllocated)
                    native.ContextHandle.Free();
            }
        }

        // --- 内部辅助 ---

        private static int ThreadCallback(IntPtr userdata)
        {
            if (userdata == IntPtr.Zero)
                return -1;

            GCHandle handle = GCHandle.FromIntPtr(userdata);
            if (handle.Target is ThreadContext context)
                return context.Callback(context.Userdata);

            return -1;
        }

        private static uint GetDisplayId(int displayIndex)
        {
            int count = 0;
            uint[] displays = SDL.GetDisplays(out count);
            if (displayIndex < 0 || displayIndex >= count)
                return 0;
            return displays[displayIndex];
        }

        private static SDL.HintPriority MapHintPriority(int priority)
        {
            return priority switch
            {
                SdlHardwareConstants.HintOverride => SDL.HintPriority.Override,
                1 => SDL.HintPriority.Normal,
                _ => SDL.HintPriority.Default
            };
        }

        private static SDL.FlipMode MapFlipMode(int flip)
        {
            return flip switch
            {
                1 => SDL.FlipMode.Horizontal,
                2 => SDL.FlipMode.Vertical,
                _ => SDL.FlipMode.None
            };
        }

        private static SdlHandle? Wrap(IntPtr ptr)
        {
            return ptr == IntPtr.Zero ? null : new SdlHandle(ptr);
        }
    }
}
