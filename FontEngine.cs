// <自动生成> 对应 C++ 源文件：FontEngine.h + FontEngine.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// FontStyle：单个字体样式的描述（对应 C++ 的 <c>class FontStyle</c>）。
    /// C++ 版本声明了 virtual ~FontStyle(){} 但函数体为空，C# 中无需任何对应代码。
    /// </summary>
    public class FontStyle
    {
        public string Name = "";
        public string Path = "";
        public int Ptsize;
        public bool Blend = true;
        public int LineHeight;
        public int FontHeight;
        public bool Bold;
        public bool Italic;
        public bool Underline;
        public Int2 ShadowOffset = new Int2(1, 1);
    }

    /// <summary>
    /// FontEngine
    ///
    /// 为 FLARE 引擎的文本渲染提供抽象接口。具体的字体渲染实现（如基于 SDL_ttf/FreeType 的
    /// 实现）将作为本类的派生类在后续批次中转换。
    /// </summary>
    public abstract class FontEngine : IDisposable
    {
        public const int JustifyLeft = 0;
        public const int JustifyRight = 1;
        public const int JustifyCenter = 2;

        public const int ColorWhite = 0;
        public const int ColorBlack = 1;
        public const int ColorMenuNormal = 2;
        public const int ColorMenuBonus = 3;
        public const int ColorMenuPenalty = 4;
        public const int ColorWidgetNormal = 5;
        public const int ColorWidgetDisabled = 6;
        public const int ColorCombatGivedmg = 7;
        public const int ColorCombatTakedmg = 8;
        public const int ColorCombatCrit = 9;
        public const int ColorCombatBuff = 10;
        public const int ColorCombatMiss = 11;
        public const int ColorRequirementsNotMet = 12;
        public const int ColorItemBonus = 13;
        public const int ColorItemPenalty = 14;
        public const int ColorItemFlavor = 15;
        public const int ColorHardcoreName = 16;

        // 原始为 size_t，此处是"容器大小"语境，按映射表规则使用 int。
        public const int ColorCount = 17;

        public const bool UseEllipsis = true;
        public const bool ShadowOffset = true;

        private const int BuilderReserve = 128;

        public int CursorY;

        protected List<Color> FontColors;

        protected FontEngine()
        {
            CursorY = 0;

            FontColors = new List<Color>(new Color[ColorCount]);

            FontColors[ColorWhite] = new Color(255, 255, 255, 255);
            FontColors[ColorBlack] = new Color(0, 0, 0, 255);

            // set the font colors
            // @CLASS FontEngine: Font colors|Description of engine/font_colors.txt
            using FileParser infile = new FileParser();
            if (infile.Open("engine/font_colors.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    // @ATTR menu_normal|color|Basic menu text color. Recommended: white.
                    // @ATTR menu_bonus|color|Positive menu text color. Recommended: green.
                    // @ATTR menu_penalty|color|Negative menu text color. Recommended: red.
                    // @ATTR widget_normal|color|Basic widget text color. Recommended: white.
                    // @ATTR widget_disabled|color|Disabled widget text color. Recommended: grey.
                    // @ATTR combat_givedmg|color|Enemy damage text color. Recommended: white.
                    // @ATTR combat_takedmg|color|Player damage text color. Recommended: red.
                    // @ATTR combat_crit|color|Enemy critical damage text color. Recommended: yellow.
                    // @ATTR combat_buff|color|Healing/buff text color. Recommended: green.
                    // @ATTR combat_miss|color|Missed attack text color. Recommended: grey.
                    // @ATTR requirements_not_met|color|Unmet requirements text color. Recommended: red.
                    // @ATTR item_bonus|color|Item bonus text color. Recommended: green.
                    // @ATTR item_penalty|color|Item penalty text color. Recommended: red.
                    // @ATTR item_flavor|color|Item flavor text color. Recommended: grey.
                    // @ATTR hardcore_color_name|color|Permadeath save slot player name color. Recommended: red.
                    int colorId = StringToFontColor(infile.Key);
                    if (colorId < ColorCount)
                        FontColors[colorId] = Parse.ToRGB(infile.Val);
                    else
                        infile.Error("FontEngine: %s is not a valid key.", infile.Key);
                }
                infile.Close();
            }
        }

        /// <summary>
        /// 对应 C++ 的 <c>virtual ~FontEngine()</c>。C# 没有确定性析构，改用 IDisposable，
        /// 由持有本对象的所有者在对应原始 `delete font_engine;` 调用点显式调用 Dispose()
        /// （将在转换到相应调用单元时接入），以保持"清理时输出一条日志"这一行为的可预测时机，
        /// 避免使用非确定性的终结器（finalizer）在 GC 线程上调用日志系统。
        /// </summary>
        public virtual void Dispose()
        {
            Utils.LogInfo("Cleaning up: FontEngine");
            GC.SuppressFinalize(this);
        }

        public Color GetColor(int colorId)
        {
            if (colorId < FontColors.Count)
                return FontColors[colorId];

            // If all else fails, return white;
            return FontColors[ColorWhite];
        }

        protected int StringToFontColor(string val)
        {
            if (val == "menu_normal") return ColorMenuNormal;
            else if (val == "menu_bonus") return ColorMenuBonus;
            else if (val == "menu_penalty") return ColorMenuPenalty;
            else if (val == "widget_normal") return ColorWidgetNormal;
            else if (val == "widget_disabled") return ColorWidgetDisabled;
            else if (val == "combat_givedmg") return ColorCombatGivedmg;
            else if (val == "combat_takedmg") return ColorCombatTakedmg;
            else if (val == "combat_crit") return ColorCombatCrit;
            else if (val == "combat_buff") return ColorCombatBuff;
            else if (val == "combat_miss") return ColorCombatMiss;
            else if (val == "requirements_not_met") return ColorRequirementsNotMet;
            else if (val == "item_bonus") return ColorItemBonus;
            else if (val == "item_penalty") return ColorItemPenalty;
            else if (val == "item_flavor") return ColorItemFlavor;
            else if (val == "hardcore_color_name") return ColorHardcoreName;

            // failed to find color
            else return ColorCount;
        }

        /// <summary>
        /// 使用给定的自动换行宽度，计算显示该文本所需的宽度与高度。
        /// </summary>
        public Int2 CalcSizeWrapped(string textWithNewlines, int width)
        {
            char newline = '\n';

            string text = textWithNewlines;

            // if this contains newlines, recurse
            int checkNewline = text.IndexOf(newline);
            if (checkNewline != -1)
            {
                Int2 p1 = CalcSizeWrapped(text.Substring(0, checkNewline), width);
                Int2 p2 = CalcSizeWrapped(text.Substring(checkNewline + 1), width);
                Int2 p3 = default;

                if (p1.X > p2.X) p3.X = p1.X;
                else p3.X = p2.X;

                p3.Y = p1.Y + p2.Y;
                return p3;
            }

            int height = 0;
            int maxWidth = 0;

            string nextWord;
            string builder = "";
            string builderPrev = "";
            char space = ' ';
            int cursor = 0;
            string fulltext = text + " ";
            string longToken;

            // 原始代码调用 builder.reserve(BUILDER_RESERVE) 预分配底层缓冲区容量；
            // C# 的 string 不可变，没有对应的"预留容量"操作，此处省略（不影响结果，仅是性能提示）。

            nextWord = GetNextToken(fulltext, ref cursor, space);

            while (cursor != -1)
            {
                int oldCursor = cursor;

                builder += nextWord;

                if (CalcSize(builder).X > width)
                {
                    // this word can't fit on this line, so word wrap
                    if (!string.IsNullOrEmpty(builderPrev))
                    {
                        height += GetLineHeight();
                        if (CalcSize(builderPrev).X > maxWidth)
                        {
                            maxWidth = CalcSize(builderPrev).X;
                        }
                    }

                    builder = "";

                    longToken = PopTokenByWidth(ref nextWord, width);

                    if (!string.IsNullOrEmpty(longToken))
                    {
                        while (!string.IsNullOrEmpty(longToken))
                        {
                            if (CalcSize(nextWord).X > maxWidth)
                            {
                                maxWidth = CalcSize(nextWord).X;
                            }
                            height += GetLineHeight();

                            nextWord = longToken;
                            longToken = PopTokenByWidth(ref nextWord, width);

                            if (longToken == nextWord)
                                break;
                        }
                    }

                    builder += nextWord + " ";
                    builderPrev = builder;
                }
                else
                {
                    builder += " ";
                    builderPrev = builder;
                }

                nextWord = GetNextToken(fulltext, ref cursor, space); // next word

                // next token is the same location as the previous token; abort
                if (cursor == oldCursor)
                    break;
            }

            builder = Parse.Trim(builder); // removes whitespace that shouldn't be included in the size
            if (!string.IsNullOrEmpty(builder))
                height += GetLineHeight();
            if (CalcSize(builder).X > maxWidth)
                maxWidth = CalcSize(builder).X;

            // handle blank lines
            if (textWithNewlines == " ")
                height += GetLineHeight();

            Int2 size = default;
            size.X = maxWidth;
            size.Y = height;
            return size;
        }

        protected Rectangle Position(string text, int x, int y, int justify)
        {
            Rectangle destRect = default;
            // calculate actual starting x,y based on justify
            if (justify == JustifyLeft)
            {
                destRect.X = x;
                destRect.Y = y;
            }
            else if (justify == JustifyRight)
            {
                destRect.X = x - CalcSize(text).X;
                destRect.Y = y;
            }
            else if (justify == JustifyCenter)
            {
                destRect.X = x - (CalcSize(text).X / 2);
                destRect.Y = y;
            }
            else
            {
                Utils.LogError("FontEngine::position() given unhandled 'justify=%d', assuming left", justify);
                destRect.X = x;
                destRect.Y = y;
            }
            return destRect;
        }

        /// <summary>
        /// 按给定宽度自动换行渲染文本。
        /// </summary>
        public void Render(string text, int x, int y, int justify, Image target, int width, Color color, bool shadow)
        {
            if (width == 0)
            {
                // a width of 0 means we won't try to wrap text
                RenderInternal(text, x, y, justify, target, color, shadow);
                return;
            }

            string fulltext = text + " ";
            CursorY = y;
            string nextWord;
            string builder = "";
            string builderPrev = "";
            char space = ' ';
            int cursor = 0;
            string longToken;

            nextWord = GetNextToken(fulltext, ref cursor, space);

            while (cursor != -1)
            {
                int oldCursor = cursor;

                builder += nextWord;

                if (CalcSize(builder).X > width)
                {
                    if (!string.IsNullOrEmpty(builderPrev))
                    {
                        RenderInternal(builderPrev, x, CursorY, justify, target, color, shadow);
                        CursorY += GetLineHeight();
                    }
                    builder = "";

                    longToken = PopTokenByWidth(ref nextWord, width);

                    if (!string.IsNullOrEmpty(longToken))
                    {
                        while (!string.IsNullOrEmpty(longToken))
                        {
                            RenderInternal(nextWord, x, CursorY, justify, target, color, shadow);
                            CursorY += GetLineHeight();

                            nextWord = longToken;
                            longToken = PopTokenByWidth(ref nextWord, width);

                            if (longToken == nextWord)
                                break;
                        }
                    }

                    builder += nextWord + " ";
                    builderPrev = builder;
                }
                else
                {
                    builder += " ";
                    builderPrev = builder;
                }

                nextWord = GetNextToken(fulltext, ref cursor, space); // next word

                // next token is the same location as the previous token; abort
                if (cursor == oldCursor)
                    break;
            }

            RenderInternal(builder, x, CursorY, justify, target, color, shadow);
            CursorY += GetLineHeight();
        }

        public void RenderShadowed(string text, int x, int y, int justify, Image target, int width, Color color)
        {
            Render(text, x, y, justify, target, width, GetColor(ColorBlack), ShadowOffset);
            Render(text, x, y, justify, target, width, color, !ShadowOffset);
        }

        public abstract int GetLineHeight();
        public abstract int GetFontHeight();

        public abstract void SetFont(string font);
        public abstract Int2 CalcSize(string text);
        public abstract string TrimTextToWidth(string text, int width, bool useEllipsis, int leftPos);

        protected abstract void RenderInternal(string text, int x, int y, int justify, Image target, Color color, bool shadow);

        /// <summary>
        /// 将字符串 text 裁剪到像素宽度 width。原始字符串会被修改为无法容纳的剩余部分，
        /// 返回值为因超出宽度而被截去的剩余字符串。
        /// </summary>
        protected string PopTokenByWidth(ref string text, int width)
        {
            int newLength = 0;

            for (int i = 0; i <= text.Length; ++i)
            {
                // 对应 C++ 中通过 UTF-8 延续字节掩码 (byte & 0xc0) == 0x80 跳过多字节字符中间位置的判断。
                // C# 字符串是 UTF-16 编码，已经是解码后的码元序列；这里改用 char.IsLowSurrogate
                // 跳过代理项对的后半部分，是这段"不要在字符中间截断"逻辑在 UTF-16 表示下的等价适配。
                if (i < text.Length && char.IsLowSurrogate(text[i]))
                    continue;

                if (CalcSize(text.Substring(0, i)).X > width)
                    break;

                newLength = i;
            }

            if (newLength > 0)
            {
                string ret = text.Substring(newLength);
                text = text.Substring(0, newLength);
                return ret;
            }
            else
            {
                return text;
            }
        }

        // similar to Parse.PopFirstString but does not alter the input string
        protected string GetNextToken(string s, ref int cursor, char separator)
        {
            int seppos = s.IndexOf(separator, cursor);
            if (seppos == -1) // not found
            {
                cursor = -1;
                return "";
            }
            string outs = s.Substring(cursor, seppos - cursor);
            cursor = seppos + 1;
            return outs;
        }
    }
}
