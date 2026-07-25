using SDL3;

namespace FlareEngine.Sdl
{
    /// <summary>
    /// SDL3 鼠标/触摸事件坐标转换上下文。
    /// SDL2 在 <c>SDL_RenderSetLogicalSize</c> 后会自动将事件坐标映射到逻辑分辨率；
    /// SDL3 需显式调用 <c>SDL_ConvertEventToRenderCoordinates</c>，否则 UI 命中区域会偏移。
    /// </summary>
    internal static class Sdl3EventCoordinatesContext
    {
        /// <summary>已设置逻辑分辨率的渲染器（与 C++ 中启用 logical size 的 renderer 对应）。</summary>
        public static IntPtr Renderer { get; private set; }

        public static void SetRenderer(IntPtr renderer)
        {
            Renderer = renderer;
        }

        public static void Clear()
        {
            Renderer = IntPtr.Zero;
        }

        /// <summary>
        /// 将窗口坐标转换为与 <see cref="RenderDevice"/> 渲染坐标一致的事件坐标。
        /// 对应 SDL3 文档：配合 <c>SetRenderLogicalPresentation</c> 使用。
        /// </summary>
        public static void TryConvert(ref SDL.Event evt)
        {
            if (Renderer == IntPtr.Zero)
                return;

            SDL.ConvertEventToRenderCoordinates(Renderer, ref evt);
        }
    }
}
