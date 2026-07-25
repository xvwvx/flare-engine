namespace FlareEngine.Sdl
{
    /// <summary>
    /// SDL3 渲染器生命周期标记。
    /// 销毁 renderer 后其关联 texture 已由 SDL 作废，不得再调用 <c>SDL_DestroyTexture</c>。
    /// </summary>
    internal static class Sdl3RenderLifetime
    {
        public static bool IsRendererActive { get; set; }
    }
}
