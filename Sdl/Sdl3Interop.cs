using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SDL3;

namespace FlareEngine.Sdl
{
    /// <summary>
    /// 包装 SDL3 原生指针，供渲染接口的 <c>object?</c> 参数传递。
    /// 对应 C++ 中 <c>SDL_Window*</c>、<c>SDL_Renderer*</c>、<c>SDL_Texture*</c>、
    /// <c>SDL_Surface*</c> 等不透明指针在引擎层的传递方式。
    /// </summary>
    internal sealed class SdlHandle
    {
        public IntPtr Ptr { get; }

        public SdlHandle(IntPtr ptr)
        {
            Ptr = ptr;
        }

        public static SdlHandle? From(object? handle)
        {
            if (handle == null)
                return null;
            if (handle is SdlHandle sh)
                return sh;
            if (handle is IntPtr p)
                return p == IntPtr.Zero ? null : new SdlHandle(p);
            return null;
        }

        public static IntPtr Require(object? handle)
        {
            return From(handle)?.Ptr ?? IntPtr.Zero;
        }
    }

    /// <summary>SDL 线程句柄，持有 <see cref="GCHandle"/> 直至 <see cref="WaitThread"/>。</summary>
    internal sealed class SdlThreadHandle
    {
        public IntPtr Thread { get; }
        public GCHandle ContextHandle { get; }

        public SdlThreadHandle(IntPtr thread, GCHandle contextHandle)
        {
            Thread = thread;
            ContextHandle = contextHandle;
        }
    }

    /// <summary>TTF 字体原生句柄（SDL3_ttf）。</summary>
    internal sealed class Sdl3TtfFontHandle : TtfFontHandle
    {
        public IntPtr FontPtr { get; }

        public Sdl3TtfFontHandle(IntPtr fontPtr)
        {
            FontPtr = fontPtr;
        }
    }

    /// <summary>SDL3 互操作辅助：Surface 字段访问与像素写入。</summary>
    internal static class Sdl3Interop
    {
        public static ref SDL.Surface SurfaceRef(IntPtr surfacePtr)
        {
            unsafe
            {
                return ref Unsafe.AsRef<SDL.Surface>(surfacePtr.ToPointer());
            }
        }

        public static SDL.Color ToSdlColor(Stride.Core.Mathematics.Color color)
        {
            return new SDL.Color
            {
                R = color.R,
                G = color.G,
                B = color.B,
                A = color.A
            };
        }

        public static SDL.Rect ToSdlRect(Stride.Core.Mathematics.Rectangle rect)
        {
            return new SDL.Rect
            {
                X = rect.X,
                Y = rect.Y,
                W = rect.Width,
                H = rect.Height
            };
        }

        public static SDL.FRect ToSdlFRect(Stride.Core.Mathematics.Rectangle rect)
        {
            return new SDL.FRect
            {
                X = rect.X,
                Y = rect.Y,
                W = rect.Width,
                H = rect.Height
            };
        }

        public static SDL.FRect? ToSdlFRectNullable(Stride.Core.Mathematics.Rectangle? rect)
        {
            return rect.HasValue ? ToSdlFRect(rect.Value) : null;
        }

        public static uint GetPrimaryDisplayId()
        {
            int count = 0;
            uint[] displays = SDL.GetDisplays(out count);
            if (displays == null || displays.Length == 0 || count <= 0)
                return 0;
            return displays[0];
        }

        public static void WriteSurfaceByte(IntPtr surfacePtr, int byteOffset, byte value)
        {
            unsafe
            {
                ref SDL.Surface surface = ref SurfaceRef(surfacePtr);
                byte* pixels = (byte*)surface.Pixels;
                pixels[byteOffset] = value;
            }
        }

        public static void WriteSurfaceUInt16(IntPtr surfacePtr, int byteOffset, ushort value)
        {
            unsafe
            {
                ref SDL.Surface surface = ref SurfaceRef(surfacePtr);
                byte* pixels = (byte*)surface.Pixels;
                *(ushort*)(pixels + byteOffset) = value;
            }
        }

        public static void WriteSurfaceUInt32(IntPtr surfacePtr, int byteOffset, uint value)
        {
            unsafe
            {
                ref SDL.Surface surface = ref SurfaceRef(surfacePtr);
                byte* pixels = (byte*)surface.Pixels;
                *(uint*)(pixels + byteOffset) = value;
            }
        }

        public static SDL.WindowFlags MapWindowFlags(uint sdl2Flags)
        {
            SDL.WindowFlags flags = 0;
            if ((sdl2Flags & SdlHardwareConstants.WindowResizable) != 0)
                flags |= SDL.WindowFlags.Resizable;
            if ((sdl2Flags & SdlHardwareConstants.WindowFullscreenDesktop) != 0)
                flags |= SDL.WindowFlags.Fullscreen;
            return flags;
        }

        public static SDL.BlendMode MapBlendMode(int blendMode)
        {
            return blendMode switch
            {
                SdlHardwareConstants.BlendModeAdd => SDL.BlendMode.Add,
                _ => SDL.BlendMode.Blend
            };
        }

        public static SDL.TextureAccess MapTextureAccess(int access)
        {
            if (access == SdlHardwareConstants.TextureAccessTarget)
                return SDL.TextureAccess.Target;
            if (access == SdlSoftwareConstants.TextureAccessStreaming)
                return SDL.TextureAccess.Streaming;
            return SDL.TextureAccess.Static;
        }

        public static SDL.PixelFormat MapPixelFormat(uint sdl2Fourcc)
        {
            if (sdl2Fourcc == SdlHardwareConstants.PixelFormatArgb8888)
                return SDL.PixelFormat.ARGB8888;
            return SDL.PixelFormat.RGBA8888;
        }

        public static void CalculateGammaRamp(float gamma, ushort[] ramp)
        {
            for (int i = 0; i < ramp.Length; ++i)
            {
                ramp[i] = (ushort)(Math.Min(1.0, Math.Pow(i / 255.0, gamma)) * 65535.0);
            }
        }
    }
}
