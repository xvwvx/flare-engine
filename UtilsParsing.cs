// <自动生成> 对应 C++ 源文件：UtilsParsing.h + UtilsParsing.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。

using System.Globalization;
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// UtilsParsing
    ///
    /// 通用的 key=value ini 风格文件格式解析工具（对应 C++ 的 <c>namespace Parse</c>）。
    /// </summary>
    public static class Parse
    {
        public static string Trim(string s, string delimiters = " \f\n\r\t\v")
        {
            string tmp = s;
            int lastNotOf = LastIndexNotOfAny(tmp, delimiters);
            tmp = tmp.Substring(0, lastNotOf + 1); // trim right side

            int firstNotOf = FirstIndexNotOfAny(tmp, delimiters);
            if (firstNotOf == -1)
                return string.Empty;
            return tmp.Substring(firstNotOf); // trim left side
        }

        public static string GetSectionTitle(string s)
        {
            int bracket = s.IndexOf(']');
            if (bracket == -1) return ""; // not found
            return s.Substring(1, bracket - 1);
        }

        public static void GetKeyPair(string s, out string key, out string val)
        {
            int separator = s.IndexOf('=');
            if (separator == -1)
            {
                key = "";
                val = "";
                return; // not found
            }

            key = s.Substring(0, separator);
            val = s.Substring(separator + 1);
            key = Trim(key);
            val = Trim(val);
        }

        // strip carriage return if exists
        public static string StripCarriageReturn(string line)
        {
            if (line.Length > 0)
            {
                if (line[^1] == '\r')
                {
                    return line.Substring(0, line.Length - 1);
                }
            }

            return line;
        }

        public static string GetLine(StreamReader infile)
        {
            // This is the standard way to check whether a read failed.
            string? line = infile.ReadLine();
            if (line == null)
                return "";
            line = StripCarriageReturn(line);
            return line;
        }

        /// <summary>
        /// 对应 C++ 中基于 <c>std::type_info</c> + <c>void*</c> 的泛型值解析函数。
        /// C# 没有裸指针到任意成员的等价物，这里改用 <c>System.Type</c> + <c>out object?</c>
        /// 表达同样的"运行时按类型分派解析"语义（属于语言范式差异导致的必要适配，
        /// 分支结构、判定顺序与原始代码逐一对应）。
        /// </summary>
        public static bool TryParseValue(Type type, string value, out object? output)
        {
            output = null;
            bool hasToken = TryExtractToken(value, out string token);

            if (type == typeof(bool))
            {
                int iv = 0;
                bool ok = hasToken && int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out iv);
                output = ok && iv != 0;
                return ok;
            }
            else if (type == typeof(int))
            {
                int iv = 0;
                bool ok = hasToken && int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out iv);
                output = ok ? iv : 0;
                return ok;
            }
            else if (type == typeof(uint))
            {
                uint uv = 0;
                bool ok = hasToken && uint.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out uv);
                output = ok ? uv : 0u;
                return ok;
            }
            else if (type == typeof(short))
            {
                short sv = 0;
                bool ok = hasToken && short.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out sv);
                output = ok ? sv : (short)0;
                return ok;
            }
            else if (type == typeof(ushort))
            {
                ushort usv = 0;
                bool ok = hasToken &&
                          ushort.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out usv);
                output = ok ? usv : (ushort)0;
                return ok;
            }
            else if (type == typeof(char))
            {
                bool ok = hasToken && token.Length > 0;
                output = ok ? token[0] : '\0';
                return ok;
            }
            else if (type == typeof(byte))
            {
                byte bv = 0;
                bool ok = hasToken && byte.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out bv);
                output = ok ? bv : (byte)0;
                return ok;
            }
            else if (type == typeof(float))
            {
                float fv = 0;
                bool ok = hasToken && float.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out fv);
                output = ok ? fv : 0f;
                return ok;
            }
            else if (type == typeof(string))
            {
                output = value;
                return true;
            }
            else
            {
                Utils.LogError("UtilsParsing: %s: a required type is not defined!", nameof(TryParseValue));
                return false;
            }
        }

        /// <summary>
        /// 对应 C++ 的 <c>Parse::toString(const std::type_info&amp;, void*)</c>。
        /// 重命名为 ValueToString 以避免与 object.ToString() 产生名称上的混淆（静态类内不冲突，
        /// 但为清晰起见显式区分）。
        /// </summary>
        public static string ValueToString(Type type, object value)
        {
            if (type == typeof(bool)) return (bool)value ? "1" : "0";
            else if (type == typeof(int)) return ((int)value).ToString(CultureInfo.InvariantCulture);
            else if (type == typeof(uint)) return ((uint)value).ToString(CultureInfo.InvariantCulture);
            else if (type == typeof(short)) return ((short)value).ToString(CultureInfo.InvariantCulture);
            else if (type == typeof(ushort)) return ((ushort)value).ToString(CultureInfo.InvariantCulture);
            else if (type == typeof(char)) return ((char)value).ToString();
            else if (type == typeof(byte)) return ((byte)value).ToString(CultureInfo.InvariantCulture);
            else if (type == typeof(float)) return ((float)value).ToString("G6", CultureInfo.InvariantCulture);
            else if (type == typeof(string)) return (string)value;
            else
            {
                Utils.LogError("UtilsParsing: %s: a required type is not defined!", nameof(ValueToString));
                return "";
            }
        }

        public static int ToInt(string s, int defaultValue = 0)
        {
            int result;
            if (!TryExtractToken(s, out string token) ||
                !int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
                result = defaultValue;
            return result;
        }

        public static float ToFloat(string s, float defaultValue = 0.0f)
        {
            float result;
            if (!TryExtractToken(s, out string token) ||
                !float.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
                result = defaultValue;
            return result;
        }

        public static ulong ToUnsignedLong(string s, ulong defaultValue = 0)
        {
            ulong result;
            if (!TryExtractToken(s, out string token) ||
                !ulong.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
                result = defaultValue;
            return result;
        }

        public static nuint ToSizeT(string s, nuint defaultValue = 0)
        {
            nuint result;
            if (!TryExtractToken(s, out string token) ||
                !nuint.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
                result = defaultValue;
            return result;
        }

        // ItemID/PowerID 假定与 GlobalUsings.cs 中的定义一致地对应 nuint（size_t），
        // 与 ToSizeT 直接兼容赋值。
        public static ItemID ToItemID(string s, ItemID defaultValue = 0)
        {
            return (ItemID)ToSizeT(s, (nuint)defaultValue);
        }

        public static PowerID ToPowerID(string s, PowerID defaultValue = 0)
        {
            return (ItemID)ToSizeT(s, (nuint)defaultValue);
        }

        public static bool ToBool(string value0)
        {
            string value = value0;
            // 注意：原始 C++ 代码调用了 trim(value) 但没有把返回值赋回 value
            // （Parse::trim 是纯函数，不修改传入的引用），因此这里的裁剪调用实际上是无效果的。
            // 依据"逐行逻辑克隆"原则，保留这一（无操作的）行为，不做"修正"。
            Trim(value);
            value = value.ToLowerInvariant();

            if (value == "true") return true;
            if (value == "yes") return true;
            if (value == "1") return true;
            if (value == "false") return false;
            if (value == "no") return false;
            if (value == "0") return false;

            Utils.LogError("UtilsParsing: %s %s doesn't know how to handle %s", "UtilsParsing.cs", nameof(ToBool),
                value);
            return false;
        }

        public static Int2 ToPoint(string value0)
        {
            string value = value0;
            Int2 p = default;
            p.X = PopFirstInt(ref value);
            p.Y = PopFirstInt(ref value);
            return p;
        }

        public static Rectangle ToRect(string value0)
        {
            string value = value0;
            Rectangle r = default;
            r.X = PopFirstInt(ref value);
            r.Y = PopFirstInt(ref value);
            r.Width = PopFirstInt(ref value);
            r.Height = PopFirstInt(ref value);
            return r;
        }

        public static Color ToRGB(string value0)
        {
            string value = value0;
            Color c = default;
            c.R = (byte)PopFirstInt(ref value);
            c.G = (byte)PopFirstInt(ref value);
            c.B = (byte)PopFirstInt(ref value);
            c.A = 255;
            return c;
        }

        public static Color ToRGBA(string value0)
        {
            string value = value0;
            Color c = default;
            c.R = (byte)PopFirstInt(ref value);
            c.G = (byte)PopFirstInt(ref value);
            c.B = (byte)PopFirstInt(ref value);
            c.A = (byte)PopFirstInt(ref value);
            return c;
        }

        /// <summary>
        /// 解析时长字符串，返回以帧数表示的时长。
        /// </summary>
        public static int ToDuration(string s)
        {
            s = s.Trim();

            // 寻找数字和字母的分界点
            int letterStartIndex = 0;
            while (letterStartIndex < s.Length && char.IsDigit(s[letterStartIndex]))
            {
                letterStartIndex++;
            }

            // 截取数字部分与后缀部分
            string numPart = s.Substring(0, letterStartIndex);
            string suffix = s.Substring(letterStartIndex).Trim(); // 移除可能存在的空格

            // 解析数字，如果解析失败则默认为 0
            if (!int.TryParse(numPart, out int val))
            {
                Utils.LogError($"UtilsParsing: Invalid duration number in '{s}'.");
                return 1;
            }

            if (val == 0)
                return val;
            else if (suffix == "s")
                val *= SharedResources.Settings!.MaxFramesPerSec;
            else
            {
                if (suffix != "ms")
                    Utils.LogError("UtilsParsing: Duration of '%d' does not have a suffix. Assuming 'ms'.", val);
                val = (int)MathF.Floor((val * SharedResources.Settings!.MaxFramesPerSec / 1000f) + 0.5f);
            }

            // round back up to 1 if we rounded down to 0 for ms
            if (val < 1) val = 1;

            return val;
        }

        public static int ToDirection(string s)
        {
            int dir;

            if (s == "N")
                dir = 3;
            else if (s == "NE")
                dir = 4;
            else if (s == "E")
                dir = 5;
            else if (s == "SE")
                dir = 6;
            else if (s == "S")
                dir = 7;
            else if (s == "SW")
                dir = 0;
            else if (s == "W")
                dir = 1;
            else if (s == "NW")
                dir = 2;
            else
            {
                dir = ToInt(s);
                if (dir < 0 || dir > 7)
                {
                    // 原始代码此处的 logError 调用缺少 %d 所需的实参（C++ 原始即如此），
                    // 依据"逐行逻辑克隆"原则原样保留，不补充参数。
                    Utils.LogError("UtilsParsing: Direction '%d' is not within range 0-7.");
                    dir = 0;
                }
            }

            return dir;
        }

        public static int ToAlignment(string s, int defaultValue = Utils.AlignTopLeft)
        {
            int align = defaultValue;

            if (s == "topleft")
                align = Utils.AlignTopLeft;
            else if (s == "top")
                align = Utils.AlignTop;
            else if (s == "topright")
                align = Utils.AlignTopRight;
            else if (s == "left")
                align = Utils.AlignLeft;
            else if (s == "center")
                align = Utils.AlignCenter;
            else if (s == "right")
                align = Utils.AlignRight;
            else if (s == "bottomleft")
                align = Utils.AlignBottomLeft;
            else if (s == "bottom")
                align = Utils.AlignBottom;
            else if (s == "bottomright")
                align = Utils.AlignBottomRight;
            else if (s == "frame_topleft")
                align = Utils.AlignFrameTopLeft;
            else if (s == "frame_top")
                align = Utils.AlignFrameTop;
            else if (s == "frame_topright")
                align = Utils.AlignFrameTopRight;
            else if (s == "frame_left")
                align = Utils.AlignFrameLeft;
            else if (s == "frame_center")
                align = Utils.AlignFrameCenter;
            else if (s == "frame_right")
                align = Utils.AlignFrameRight;
            else if (s == "frame_bottomleft")
                align = Utils.AlignFrameBottomLeft;
            else if (s == "frame_bottom")
                align = Utils.AlignFrameBottom;
            else if (s == "frame_bottomright")
                align = Utils.AlignFrameBottomRight;

            return align;
        }

        public static string PopFirstString(ref string s, char separator = '\0')
        {
            string outs;
            int seppos;

            if (separator == '\0')
            {
                seppos = s.IndexOf(',');
                int altSeppos = s.IndexOf(';');

                if (altSeppos != -1 && (seppos == -1 || altSeppos < seppos))
                {
                    seppos = altSeppos; // return the first ',' or ';'
                }
            }
            else
            {
                seppos = s.IndexOf(separator);
            }

            if (seppos == -1)
            {
                outs = s;
                s = "";
            }
            else
            {
                outs = s.Substring(0, seppos);
                s = s.Substring(seppos + 1);
            }

            return outs;
        }

        /// <summary>
        /// 给定一个以十进制数字开头、后跟逗号的字符串，返回该数字，并修改字符串以去除该数字和逗号。
        /// 本质上是一种偷懒的"split"替代方案。
        /// </summary>
        public static int PopFirstInt(ref string s, char separator = '\0')
        {
            string popped = PopFirstString(ref s, separator);
            return ToInt(popped);
        }

        public static float PopFirstFloat(ref string s, char separator = '\0')
        {
            string popped = PopFirstString(ref s, separator);
            return ToFloat(popped);
        }

        public static LabelInfo PopLabelInfo(string val0)
        {
            string val = val0;
            LabelInfo info = new LabelInfo();
            string justify, valign, style;

            string tmp = PopFirstString(ref val);
            if (tmp == "hidden")
            {
                info.Hidden = true;
            }
            else
            {
                info.Hidden = false;
                info.X = ToInt(tmp);
                info.Y = PopFirstInt(ref val);
                justify = PopFirstString(ref val);
                valign = PopFirstString(ref val);
                style = PopFirstString(ref val);

                if (justify == "left") info.Justify = FontEngine.JustifyLeft;
                else if (justify == "center") info.Justify = FontEngine.JustifyCenter;
                else if (justify == "right") info.Justify = FontEngine.JustifyRight;

                if (valign == "top") info.Valign = LabelInfo.ValignTop;
                else if (valign == "center") info.Valign = LabelInfo.ValignCenter;
                else if (valign == "bottom") info.Valign = LabelInfo.ValignBottom;

                if (style != "") info.FontStyle = style;
            }

            return info;
        }

        /// <summary>
        /// 重载：对应原始 <c>check_pair = NULL</c>（不关心是否存在冒号分隔符）的调用方式。
        /// </summary>
        public static ItemStack ToItemQuantityPair(string value)
        {
            return ToItemQuantityPair(value, out _);
        }

        /// <summary>
        /// 对应原始 <c>ItemStack toItemQuantityPair(std::string value, bool* check_pair)</c>，
        /// 用 out 参数替代可为 NULL 的指针参数（见上方重载，语言层面的必要适配）。
        /// </summary>
        public static ItemStack ToItemQuantityPair(string value0, out bool checkPair)
        {
            string value = value0;
            ItemStack r = new ItemStack();

            checkPair = value.IndexOf(':') != -1;

            value += ':';
            r.Item = ToItemID(PopFirstString(ref value, ':'));
            r.Quantity = PopFirstInt(ref value, ':');

            // quantity is always >= 1
            if (r.Quantity == 0)
                r.Quantity = 1;

            return r;
        }

        public static bool SkipLine(string line)
        {
            if (line.Length == 0)
                return true;

            if (line[0] == '#')
                return true;

            return false;
        }

        private static bool TryExtractToken(string value, out string token)
        {
            int start = 0;
            while (start < value.Length && char.IsWhiteSpace(value[start])) start++;
            int end = start;
            while (end < value.Length && !char.IsWhiteSpace(value[end])) end++;
            token = value.Substring(start, end - start);
            return token.Length > 0;
        }

        private static int LastIndexNotOfAny(string s, string chars)
        {
            for (int i = s.Length - 1; i >= 0; i--)
            {
                if (chars.IndexOf(s[i]) == -1)
                    return i;
            }

            return -1;
        }

        private static int FirstIndexNotOfAny(string s, string chars)
        {
            for (int i = 0; i < s.Length; i++)
            {
                if (chars.IndexOf(s[i]) == -1)
                    return i;
            }

            return -1;
        }
    }
}
