// 对应 C++ 源：SDLFontEngine.h + SDLFontEngine.cpp
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// 对应 C++ 的 <c>TTF_Font*</c> 不透明句柄。具体 SDL_ttf 绑定由
    /// <see cref="ISdlTtfService"/> 的实现类持有与管理。
    /// </summary>
    public class TtfFontHandle
    {
    }

    /// <summary>
    /// 对应 SDL_ttf 中的 <c>TTF_STYLE_*</c> 标志位常量。
    /// </summary>
    public static class TtfFontStyleFlags
    {
        public const int Normal = 0;
        public const int Bold = 1 << 0;
        public const int Italic = 1 << 1;
        public const int Underline = 1 << 2;
    }

    /// <summary>
    /// 封装 SDL_ttf API 调用（规则：业务逻辑禁止直接调用 SDL2 静态方法，
    /// 必须通过接口实例调用）。具体实现将由后续 SDL 绑定单元提供并注入
    /// <see cref="SDLFontEngine.TtfService"/>。
    /// </summary>
    public interface ISdlTtfService
    {
        /// <summary>对应 TTF_WasInit()。</summary>
        bool WasInit();

        /// <summary>对应 TTF_Init()；返回值语义不变：-1 表示失败。</summary>
        int Init();

        /// <summary>对应 TTF_GetError()。</summary>
        string GetError();

        /// <summary>对应 TTF_Quit()。</summary>
        void Quit();

        /// <summary>对应 TTF_OpenFont(path, ptSize)；失败时返回 null。</summary>
        TtfFontHandle? OpenFont(string path, int ptSize);

        /// <summary>对应 TTF_CloseFont(font)。</summary>
        void CloseFont(TtfFontHandle? font);

        /// <summary>对应 TTF_FontLineSkip(font)。</summary>
        int FontLineSkip(TtfFontHandle font);

        /// <summary>对应 TTF_SetFontStyle(font, style)。</summary>
        void SetFontStyle(TtfFontHandle font, int style);

        /// <summary>对应 TTF_SizeUTF8(font, text, &amp;w, &amp;h)。</summary>
        void SizeUtf8(TtfFontHandle font, string text, out int w, out int h);
    }

    /// <summary>
    /// SDLFontStyle：对应 C++ 的 <c>class SDLFontStyle : public FontStyle</c>。
    /// C++ 版本声明了 <c>~SDLFontStyle() {}</c> 但函数体为空，C# 中无需任何对应代码。
    /// </summary>
    public class SDLFontStyle : FontStyle, ISdlFontNativeHandle
    {
        public TtfFontHandle? Ttfont;
        public bool UseDefaultStyle = true;

        /// <inheritdoc cref="ISdlFontNativeHandle.TtFont"/>
        public object? TtFont => Ttfont;

        public SDLFontStyle()
        {
            Ttfont = null;
            UseDefaultStyle = true;
        }
    }

    /// <summary>
    /// SDLFontEngine
    ///
    /// 使用 SDL TTF_Font 渲染位图字体（对应 C++ 的 <c>class SDLFontEngine : public FontEngine</c>）。
    /// </summary>
    public class SDLFontEngine : FontEngine
    {
        /// <summary>
        /// SDL_ttf 服务的注入点（详见 <see cref="ISdlTtfService"/> 说明）。
        /// 在具体 SDL 实现单元完成转换、并对该属性赋值之前，TTF 相关调用返回
        /// 中性默认值（WasInit=false、Init=0、OpenFont=null 等），不会抛出异常。
        /// </summary>
        public static ISdlTtfService? TtfService { get; set; }

        private List<SDLFontStyle> _fontStyles = new List<SDLFontStyle>();
        private SDLFontStyle? _activeFont;

        public SDLFontEngine()
        {
            var mods = SharedResources.Mods!;
            _activeFont = null;

            // Initiate SDL_ttf
            if (!TtfWasInit() && TtfInit() == -1)
            {
                Utils.LogError("SDLFontEngine: TTF_Init: %s", TtfGetError());
                Utils.LogErrorDialog("SDLFontEngine: TTF_Init: %s", TtfGetError());
                mods.ResetModConfig();
                Utils.Exit(2);
            }

            SDLFontStyle temp = new SDLFontStyle();
            SDLFontStyle? current = temp;

            // load the fonts
            // @CLASS SDLFontEngine: Font settings|Description of engine/font_settings.txt
            FileParser infile = new FileParser();
            if (infile.Open("engine/font_settings.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (infile.NewSection)
                    {
                        if (infile.Section == "font")
                        {
                            temp = new SDLFontStyle();
                            current = temp;
                        }
                        else if (infile.Section == "font_fallback")
                        {
                            infile.Error("FontEngine: Support for 'font_fallback' has been removed.");
                            continue;
                        }
                    }

                    if (infile.Section != "font")
                        continue;

                    // if we want to replace a list item by ID, the ID needs to be parsed first
                    // but it is not essential if we're just adding to the list, so this is simply a warning
                    if (infile.Key != "id" && current!.Name == "")
                    {
                        infile.Error("SDLFontEngine: Expected 'id', but found '%s'.", infile.Key);
                    }

                    if (infile.Key == "id")
                    {
                        // @ATTR font.id|string|An identifier used to reference this font.
                        bool foundId = false;
                        for (int i = 0; i < _fontStyles.Count; ++i)
                        {
                            if (_fontStyles[i].Name == infile.Val)
                            {
                                current = _fontStyles[i];
                                foundId = true;
                            }
                        }
                        if (!foundId)
                        {
                            _fontStyles.Add(temp);
                            current = _fontStyles[_fontStyles.Count - 1];
                            current.Name = infile.Val;
                        }
                    }
                    else if (infile.Key == "style")
                    {
                        // @ATTR font.style|repeatable(["default", predefined_string], filename, int, bool, bool, bool, bool, int, int) : Language, Font file, Point size, Blending, Bold, Italic, Underline, Shadow offset X, Shadow offset Y|Filename, point size, blend mode, style (optional), and shadow offset (optional) of the font to use for this language. Language can be "default" or a 2-letter region code.
                        string lang = Parse.PopFirstString(ref infile.Val);

                        if ((lang == "default" && current!.UseDefaultStyle) || lang == SharedResources.Settings!.Language)
                        {
                            if (lang != "default")
                                current.UseDefaultStyle = false;

                            current.Path = Parse.PopFirstString(ref infile.Val);
                            current.Ptsize = Parse.PopFirstInt(ref infile.Val);
                            current.Blend = Parse.ToBool(Parse.PopFirstString(ref infile.Val));

                            current.Bold = false;
                            current.Italic = false;
                            current.Underline = false;
                            current.ShadowOffset.X = 1;
                            current.ShadowOffset.Y = 1;

                            string boldStr = Parse.PopFirstString(ref infile.Val);
                            string italicStr = Parse.PopFirstString(ref infile.Val);
                            string underlineStr = Parse.PopFirstString(ref infile.Val);

                            if (boldStr != "")
                                current.Bold = Parse.ToBool(boldStr);
                            if (italicStr != "")
                                current.Italic = Parse.ToBool(italicStr);
                            if (underlineStr != "")
                                current.Underline = Parse.ToBool(underlineStr);

                            string shadowOffX = Parse.PopFirstString(ref infile.Val);
                            string shadowOffY = Parse.PopFirstString(ref infile.Val);

                            if (shadowOffX != "" && shadowOffY != "")
                            {
                                current.ShadowOffset.X = Parse.ToInt(shadowOffX);
                                current.ShadowOffset.Y = Parse.ToInt(shadowOffY);
                            }
                        }
                    }
                }
                infile.Close();
            }

            // load the font files from the styles
            for (int i = 0; i < _fontStyles.Count; ++i)
            {
                SDLFontStyle style = _fontStyles[i];

                string fontPath = mods.Locate(style.Path);

                // check inside the "fonts/" directory if we can't find our font
                if (!Filesystem.FileExists(mods.Locate(style.Path)))
                {
                    fontPath = mods.Locate("fonts/" + style.Path);
                    if (fontPath == "")
                        Utils.LogError("FontEngine: Could not find font file: '%s'", style.Path);
                }

                if (fontPath != "")
                {
                    style.Ttfont = TtfOpenFont(fontPath, style.Ptsize);
                    if (style.Ttfont == null)
                    {
                        Utils.LogError("FontEngine: TTF_OpenFont: %s", TtfGetError());
                    }
                    else
                    {
                        int lineskip = TtfFontLineSkip(style.Ttfont);
                        style.LineHeight = lineskip;
                        style.FontHeight = lineskip;

                        int ttfStyle = TtfFontStyleFlags.Normal;
                        if (style.Bold) ttfStyle |= TtfFontStyleFlags.Bold;
                        if (style.Italic) ttfStyle |= TtfFontStyleFlags.Italic;
                        if (style.Underline) ttfStyle |= TtfFontStyleFlags.Underline;
                        TtfSetFontStyle(style.Ttfont, ttfStyle);
                    }
                }
            }

            // Attempt to set the default active font
            SetFont("font_regular");
            if (!IsActiveFontValid())
            {
                Utils.LogError("FontEngine: Unable to determine default font!");
                Utils.LogErrorDialog("FontEngine: Unable to determine default font!");
            }
        }

        public override void Dispose()
        {
            for (int i = 0; i < _fontStyles.Count; ++i) TtfCloseFont(_fontStyles[i].Ttfont);
            TtfQuit();
            base.Dispose();
        }

        private bool IsActiveFontValid()
        {
            return _activeFont != null && _activeFont.Ttfont != null;
        }

        public override int GetLineHeight()
        {
            if (!IsActiveFontValid())
                return 1;

            return _activeFont!.LineHeight;
        }

        public override int GetFontHeight()
        {
            if (!IsActiveFontValid())
                return 1;

            return _activeFont!.FontHeight;
        }

        /// <summary>
        /// For single-line text, just calculate the width
        /// </summary>
        public override Int2 CalcSize(string text)
        {
            if (!IsActiveFontValid())
                return new Int2(1, 1);

            int w, h;
            TtfSizeUtf8(_activeFont!.Ttfont!, text, out w, out h);

            Int2 result = default;
            result.X = w;
            result.Y = h;
            return result;
        }

        /// <summary>
        /// Fit a string of text into a pixel width
        /// useEllipsis determines how the returned string will appear
        /// Example with "Hello World" (let's assume a monospace font and a width that can fit 6 characters):
        /// useEllipsis == true: "Hello ..."
        /// useEllipsis == false: " World"
        ///
        /// leftPos is only used when useEllipsis is false.
        /// It ensures that this character is visible, chopping the end of the string if needed.
        /// </summary>
        public override string TrimTextToWidth(string text, int width, bool useEllipsis, int leftPos)
        {
            if (width >= CalcSize(text).X)
                return text;

            int textLength = text.Length;
            int retLength = textLength;
            int totalWidth = (useEllipsis ? width - CalcSize("...").X : width);

            for (int i = textLength; i > 0; i--)
            {
                if (useEllipsis)
                {
                    if (totalWidth < CalcSize(text.Substring(0, retLength)).X)
                        retLength = i;
                    else
                        break;
                }
                else if (leftPos < textLength - retLength)
                {
                    if (totalWidth < CalcSize(text.Substring(leftPos, retLength)).X)
                        retLength = i;
                    else
                        break;
                }
                else
                {
                    if (totalWidth < CalcSize(text.Substring(textLength - retLength)).X)
                        retLength = i;
                    else
                        break;
                }

                // 对应 C++ 中通过 UTF-8 延续字节掩码 (byte & 0xc0) == 0x80 跳过多字节字符中间位置的判断。
                // C# 字符串是 UTF-16 编码，已经是解码后的码元序列；这里改用 char.IsLowSurrogate
                // 跳过代理项对的后半部分，是这段"不要在字符中间截断"逻辑在 UTF-16 表示下的等价适配。
                // 注意：C++ std::string::operator[size()] 返回 '\0'（合法），C# 索引 Length 会抛异常，
                // 所以必须额外检查 retLength < textLength。
                while (retLength > 0 && retLength < textLength && char.IsLowSurrogate(text[retLength]))
                {
                    retLength--;
                }
            }

            if (!useEllipsis)
            {
                if (leftPos < textLength - retLength)
                    return text.Substring(leftPos, retLength);
                else
                    return text.Substring(textLength - retLength);
            }
            else
            {
                if (textLength <= 3)
                    return "...";

                if (textLength - retLength < 3)
                    retLength = textLength - 3;

                string retStr = text.Substring(0, retLength);
                retStr = retStr + '.' + '.' + '.';
                return retStr;
            }
        }

        public override void SetFont(string font)
        {
            for (int i = 0; i < _fontStyles.Count; i++)
            {
                if (_fontStyles[i].Ttfont != null && _fontStyles[i].Name == font)
                {
                    _activeFont = _fontStyles[i];
                    return;
                }
            }

            // Unable to find a matching font. Try the first available font style instead
            for (int i = 0; i < _fontStyles.Count; i++)
            {
                if (_fontStyles[i].Ttfont != null)
                {
                    Utils.LogError("FontEngine: Invalid font '%s'. Falling back to '%s'.", font, _fontStyles[i].Name);
                    _activeFont = _fontStyles[i];
                    return;
                }
            }

            Utils.LogError("FontEngine: Invalid font '%s'. No fallback available.", font);
        }

        /// <summary>
        /// Render the given text at (x,y) on the target image.
        /// Justify is left, right, or center
        /// </summary>
        protected override void RenderInternal(string text, int x, int y, int justify, Image target, Color color, bool shadow)
        {
            var renderDevice = SharedResources.RenderDevice!;

            if (!IsActiveFontValid() || text == "")
                return;

            Image? graphics;

            Rectangle destRect;
            if (shadow)
                destRect = Position(text, x + _activeFont!.ShadowOffset.X, y + _activeFont.ShadowOffset.Y, justify);
            else
                destRect = Position(text, x, y, justify);

            // Render text into target
            // We render the same thing twice because blending with itself produces visually clearer text, especially on noisy backgrounds
            graphics = renderDevice.RenderTextToImage(_activeFont!, text, color, _activeFont.Blend);
            if (graphics != null)
            {
                if (target != null)
                {
                    Rectangle clip = default;
                    clip.Width = graphics.GetWidth();
                    clip.Height = graphics.GetHeight();
                    renderDevice.RenderToImage(graphics, clip, target, destRect);
                    renderDevice.RenderToImage(graphics, clip, target, destRect);
                }
                else
                {
                    // no target, so just render to the screen
                    Sprite? tempSprite = graphics.CreateSprite();
                    if (tempSprite != null)
                    {
                        tempSprite.SetDestFromRect(destRect);
                        renderDevice.Render(tempSprite);
                        renderDevice.Render(tempSprite);
                        tempSprite.Dispose();
                    }
                }

                // text is cached, we can free temp resource
                graphics.Unref();
            }
        }

        private static bool TtfWasInit()
        {
            return TtfService?.WasInit() ?? false;
        }

        private static int TtfInit()
        {
            return TtfService?.Init() ?? 0;
        }

        private static string TtfGetError()
        {
            return TtfService?.GetError() ?? string.Empty;
        }

        private static void TtfQuit()
        {
            TtfService?.Quit();
        }

        private static TtfFontHandle? TtfOpenFont(string path, int ptSize)
        {
            return TtfService?.OpenFont(path, ptSize);
        }

        private static void TtfCloseFont(TtfFontHandle? font)
        {
            TtfService?.CloseFont(font);
        }

        private static int TtfFontLineSkip(TtfFontHandle font)
        {
            return TtfService?.FontLineSkip(font) ?? 0;
        }

        private static void TtfSetFontStyle(TtfFontHandle font, int style)
        {
            TtfService?.SetFontStyle(font, style);
        }

        private static void TtfSizeUtf8(TtfFontHandle font, string text, out int w, out int h)
        {
            if (TtfService != null)
                TtfService.SizeUtf8(font, text, out w, out h);
            else
            {
                w = 0;
                h = 0;
            }
        }
    }
}
