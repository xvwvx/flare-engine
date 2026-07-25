using SDL3;

namespace FlareEngine.Sdl
{
    /// <summary>
    /// 对应 C++ <c>SDLFontEngine</c> 中使用的 SDL_ttf API：
    /// <c>TTF_WasInit</c>、<c>TTF_Init</c>、<c>TTF_OpenFont</c>、<c>TTF_CloseFont</c>、
    /// <c>TTF_FontLineSkip</c>、<c>TTF_SetFontStyle</c>、<c>TTF_SizeUTF8</c>、<c>TTF_Quit</c>。
    /// </summary>
    public sealed class Sdl3TtfService : ISdlTtfService
    {
        /// <summary>对应 <c>TTF_WasInit()</c>。</summary>
        public bool WasInit()
        {
            return TTF.WasInit() != 0;
        }

        /// <summary>对应 <c>TTF_Init()</c>，失败返回 -1。</summary>
        public int Init()
        {
            return TTF.Init() ? 0 : -1;
        }

        /// <summary>对应 <c>TTF_GetError()</c>（SDL3_ttf 复用 SDL 错误栈）。</summary>
        public string GetError()
        {
            return SDL.GetError() ?? string.Empty;
        }

        /// <summary>对应 <c>TTF_Quit()</c>。</summary>
        public void Quit()
        {
            TTF.Quit();
        }

        /// <summary>对应 <c>TTF_OpenFont(path, ptsize)</c>。</summary>
        public TtfFontHandle? OpenFont(string path, int ptSize)
        {
            IntPtr font = TTF.OpenFont(path, ptSize);
            return font == IntPtr.Zero ? null : new Sdl3TtfFontHandle(font);
        }

        /// <summary>对应 <c>TTF_CloseFont</c>。</summary>
        public void CloseFont(TtfFontHandle? font)
        {
            if (font is Sdl3TtfFontHandle native && native.FontPtr != IntPtr.Zero)
                TTF.CloseFont(native.FontPtr);
        }

        /// <summary>对应 <c>TTF_FontLineSkip</c>。</summary>
        public int FontLineSkip(TtfFontHandle font)
        {
            if (font is Sdl3TtfFontHandle native)
                return TTF.GetFontLineSkip(native.FontPtr);
            return 0;
        }

        /// <summary>对应 <c>TTF_SetFontStyle</c>（TTF_STYLE_BOLD / ITALIC / UNDERLINE）。</summary>
        public void SetFontStyle(TtfFontHandle font, int style)
        {
            if (font is not Sdl3TtfFontHandle native)
                return;

            TTF.FontStyleFlags flags = TTF.FontStyleFlags.Normal;
            if ((style & TtfFontStyleFlags.Bold) != 0)
                flags |= TTF.FontStyleFlags.Bold;
            if ((style & TtfFontStyleFlags.Italic) != 0)
                flags |= TTF.FontStyleFlags.Italic;
            if ((style & TtfFontStyleFlags.Underline) != 0)
                flags |= TTF.FontStyleFlags.Underline;
            TTF.SetFontStyle(native.FontPtr, flags);
        }

        /// <summary>对应 <c>TTF_SizeUTF8</c>。</summary>
        public void SizeUtf8(TtfFontHandle font, string text, out int w, out int h)
        {
            w = 0;
            h = 0;
            if (font is Sdl3TtfFontHandle native)
                TTF.GetStringSize(native.FontPtr, text, 0, out w, out h);
        }

        internal static IntPtr GetFontPtr(object? fontHandle)
        {
            if (fontHandle is Sdl3TtfFontHandle native)
                return native.FontPtr;
            return IntPtr.Zero;
        }
    }
}
