// <自动生成> 对应 C++ 源文件：UtilsDebug.h + UtilsDebug.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using System.Text;
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// 本单元对应 C++ 源码中一系列 <c>std::ostream&amp; operator&lt;&lt;(std::ostream&amp;, const T&amp;)</c>
    /// 重载：把 SDL2 事件结构体以及引擎自定义的 Rect / Point 格式化为可读的调试字符串。
    ///
    /// 依据规则"业务逻辑禁止直接依赖 SDL2 静态方法/类型，必须通过接口实例或最小化的自定义类型
    /// 表达"，本文件不引用任何真实的 SDL2 绑定类型，而是新增一组只读值类型（Sdl 前缀），
    /// 字段命名与真实 SDL2 事件结构体（SDL_Event 及其各 union 成员 window/key/motion/button/
    /// jaxis/jball/jhat/jbutton/quit/syswm）保持一致，便于后续真正的 SDL 绑定单元
    /// （如 SDLInputState）在派发事件时填充这些结构体后调用本文件的方法。
    /// 事件类型编号、SDL_PRESSED/SDL_RELEASED 取值采用标准 SDL2（2.0.x）头文件中长期稳定不变的
    /// 数值，详见 .report.txt 中的风险说明。
    /// </summary>
    public static class SdlEventCodes
    {
        public const uint Quit = 0x100;
        public const uint WindowEvent = 0x200;
        public const uint SysWmEvent = 0x201;
        public const uint KeyDown = 0x300;
        public const uint KeyUp = 0x301;
        public const uint MouseMotion = 0x400;
        public const uint MouseButtonDown = 0x401;
        public const uint MouseButtonUp = 0x402;
        public const uint JoyAxisMotion = 0x600;
        public const uint JoyBallMotion = 0x601;
        public const uint JoyHatMotion = 0x602;
        public const uint JoyButtonDown = 0x603;
        public const uint JoyButtonUp = 0x604;
        public const uint UserEvent = 0x8000;

        public const byte Released = 0;
        public const byte Pressed = 1;
    }

    /// <summary>对应 C++ 中的 <c>SDL_WindowEvent</c>（仅保留本单元实际读取的字段）。</summary>
    public readonly struct SdlWindowEvent
    {
        public int Data1 { get; init; }
        public int Data2 { get; init; }
    }

    /// <summary>对应 C++ 中的 <c>SDL_Keysym</c>。</summary>
    public readonly struct SdlKeysym
    {
        public int Scancode { get; init; }
        public int Sym { get; init; }
        public ushort Mod { get; init; }
    }

    /// <summary>对应 C++ 中的 <c>SDL_KeyboardEvent</c>。</summary>
    public readonly struct SdlKeyboardEvent
    {
        public uint Type { get; init; }
        public byte State { get; init; }
        public SdlKeysym Keysym { get; init; }
    }

    /// <summary>对应 C++ 中的 <c>SDL_MouseMotionEvent</c>。</summary>
    public readonly struct SdlMouseMotionEvent
    {
        public uint State { get; init; }
        public int X { get; init; }
        public int Y { get; init; }
        public int XRel { get; init; }
        public int YRel { get; init; }
    }

    /// <summary>对应 C++ 中的 <c>SDL_MouseButtonEvent</c>。</summary>
    public readonly struct SdlMouseButtonEvent
    {
        public uint Type { get; init; }
        public byte Button { get; init; }
        public byte State { get; init; }
        public int X { get; init; }
        public int Y { get; init; }
    }

    /// <summary>对应 C++ 中的 <c>SDL_JoyAxisEvent</c>。</summary>
    public readonly struct SdlJoyAxisEvent
    {
        public int Which { get; init; }
        public byte Axis { get; init; }
        public short Value { get; init; }
    }

    /// <summary>对应 C++ 中的 <c>SDL_JoyBallEvent</c>。</summary>
    public readonly struct SdlJoyBallEvent
    {
        public int Which { get; init; }
        public byte Ball { get; init; }
        public short XRel { get; init; }
        public short YRel { get; init; }
    }

    /// <summary>对应 C++ 中的 <c>SDL_JoyHatEvent</c>。</summary>
    public readonly struct SdlJoyHatEvent
    {
        public int Which { get; init; }
        public byte Hat { get; init; }
        public byte Value { get; init; }
    }

    /// <summary>对应 C++ 中的 <c>SDL_JoyButtonEvent</c>。</summary>
    public readonly struct SdlJoyButtonEvent
    {
        public uint Type { get; init; }
        public int Which { get; init; }
        public byte Button { get; init; }
        public byte State { get; init; }
    }

    /// <summary>对应 C++ 中的 <c>SDL_QuitEvent</c>（原始重载未读取任何字段）。</summary>
    public readonly struct SdlQuitEvent
    {
    }

    /// <summary>对应 C++ 中的 <c>SDL_SysWMEvent</c>（原始重载未读取任何字段）。</summary>
    public readonly struct SdlSysWmEvent
    {
    }

    /// <summary>
    /// 对应 C++ 中的 <c>SDL_Event</c>（一个 union）。C# 用各具体子事件字段平铺表示，
    /// 字段命名对应真实 SDL2 union 成员名（window/key/motion/button/jaxis/jball/jhat/jbutton/quit/syswm），
    /// 派发时仅访问与 <see cref="Type"/> 对应的那一个字段，与原始 <c>reinterpret_cast</c> 语义等价。
    /// </summary>
    public readonly struct SdlEvent
    {
        public uint Type { get; init; }
        public SdlWindowEvent Window { get; init; }
        public SdlKeyboardEvent Key { get; init; }
        public SdlMouseMotionEvent Motion { get; init; }
        public SdlMouseButtonEvent Button { get; init; }
        public SdlJoyAxisEvent JAxis { get; init; }
        public SdlJoyBallEvent JBall { get; init; }
        public SdlJoyHatEvent JHat { get; init; }
        public SdlJoyButtonEvent JButton { get; init; }
        public SdlQuitEvent Quit { get; init; }
        public SdlSysWmEvent SysWm { get; init; }
    }

    /// <summary>
    /// UtilsDebug
    ///
    /// 对应 UtilsDebug.h/.cpp 中的一系列全局 <c>operator&lt;&lt;</c> 重载（对应 C++ 的全局自由函数，
    /// 依据规则"全局函数转为 static class 中的静态方法"转换为本静态类）。
    /// C# 没有与 <c>std::ostream&amp; operator&lt;&lt;(std::ostream&amp;, const T&amp;)</c> 直接对应的语言特性，
    /// 这里改用扩展方法 <c>WriteDebug(this StringBuilder, T)</c>：以 <see cref="StringBuilder"/> 充当原始的
    /// <c>ostream&amp;</c>，方法体内逐行对应原始的 <c>os &lt;&lt; ...</c> 语句序列，并同样返回该
    /// <see cref="StringBuilder"/> 以保留原始的链式调用（<c>return os;</c>）能力。
    /// </summary>
    public static class UtilsDebug
    {
        /// <summary>对应 <c>operator&lt;&lt;(std::ostream&amp;, const SDL_Event&amp;)</c>。</summary>
        public static StringBuilder WriteDebug(this StringBuilder os, SdlEvent evt)
        {
            switch (evt.Type)
            {
                case SdlEventCodes.WindowEvent:
                    os.WriteDebug(evt.Window);
                    break;
                case SdlEventCodes.KeyUp:
                case SdlEventCodes.KeyDown:
                    os.WriteDebug(evt.Key);
                    break;
                case SdlEventCodes.MouseMotion:
                    os.WriteDebug(evt.Motion);
                    break;
                case SdlEventCodes.MouseButtonUp:
                case SdlEventCodes.MouseButtonDown:
                    os.WriteDebug(evt.Button);
                    break;
                case SdlEventCodes.JoyAxisMotion:
                    os.WriteDebug(evt.JAxis);
                    break;
                case SdlEventCodes.JoyBallMotion:
                    os.WriteDebug(evt.JBall);
                    break;
                case SdlEventCodes.JoyHatMotion:
                    os.WriteDebug(evt.JHat);
                    break;
                case SdlEventCodes.JoyButtonUp:
                case SdlEventCodes.JoyButtonDown:
                    os.WriteDebug(evt.JButton);
                    break;
                case SdlEventCodes.Quit:
                    os.WriteDebug(evt.Quit);
                    break;
                case SdlEventCodes.SysWmEvent:
                    os.WriteDebug(evt.SysWm);
                    break;
                case SdlEventCodes.UserEvent:
                    os.Append("User Event");
                    break;
                default:
                    os.Append("Unknown event: ").Append(evt.Type);
                    return os;
            }

            return os;
        }

        /// <summary>对应 <c>operator&lt;&lt;(std::ostream&amp;, const SDL_WindowEvent&amp;)</c>。</summary>
        public static StringBuilder WriteDebug(this StringBuilder os, SdlWindowEvent evt)
        {
            os.Append("{SDL_WINDOW_EVENT, data1 = ").Append((ushort)evt.Data1)
              .Append(", data2 = ").Append((ushort)evt.Data2).Append('}');
            return os;
        }

        /// <summary>对应 <c>operator&lt;&lt;(std::ostream&amp;, const SDL_KeyboardEvent&amp;)</c>。</summary>
        public static StringBuilder WriteDebug(this StringBuilder os, SdlKeyboardEvent evt)
        {
            os.Append('{');
            if (SdlEventCodes.KeyDown == evt.Type)
            {
                os.Append("SDL_KEYDOWN");
            }
            else if (SdlEventCodes.KeyUp == evt.Type)
            {
                os.Append("SDL_KEYUP");
            }
            else
            {
                os.Append("Unexpected value type: ").Append(evt.Type).Append('}');
                return os;
            }
            os.Append(", state");
            if (SdlEventCodes.Pressed == evt.State)
            {
                os.Append(" = SDL_PRESSED");
            }
            else if (SdlEventCodes.Released == evt.State)
            {
                os.Append(" = SDL_RELEASED");
            }
            else
            {
                os.Append(" = ??").Append(evt.State);
            }
            os.Append(", SDL_keysym: ").WriteDebug(evt.Keysym).Append('}');
            return os;
        }

        /// <summary>对应 <c>operator&lt;&lt;(std::ostream&amp;, const SDL_Keysym&amp;)</c>。</summary>
        public static StringBuilder WriteDebug(this StringBuilder os, SdlKeysym ks)
        {
            os.Append("{scancode = ").Append((ushort)ks.Scancode)
              .Append(", sym = ").Append(ks.Sym).Append(", mod = ").Append(ks.Mod).Append('}');
            return os;
        }

        /// <summary>对应 <c>operator&lt;&lt;(std::ostream&amp;, const SDL_MouseMotionEvent&amp;)</c>。</summary>
        public static StringBuilder WriteDebug(this StringBuilder os, SdlMouseMotionEvent evt)
        {
            os.Append("{SDL_MOUSEMOTION, state = ").Append((ushort)evt.State)
              .Append(", (x,y) = (").Append(evt.X).Append(',').Append(evt.Y).Append(')')
              .Append(", (xrel,yrel) = (").Append(evt.XRel).Append(',').Append(evt.YRel).Append(")}");
            return os;
        }

        /// <summary>对应 <c>operator&lt;&lt;(std::ostream&amp;, const SDL_MouseButtonEvent&amp;)</c>。</summary>
        public static StringBuilder WriteDebug(this StringBuilder os, SdlMouseButtonEvent evt)
        {
            os.Append("{SDL_MOUSEBUTTON, type = ");
            if (SdlEventCodes.MouseButtonDown == evt.Type)
            {
                os.Append("DOWN");
            }
            else if (SdlEventCodes.MouseButtonUp == evt.Type)
            {
                os.Append("UP");
            }
            else
            {
                os.Append("??").Append(evt.Type).Append('}');
                return os;
            }
            os.Append(", button = ").Append((ushort)evt.Button).Append(", state = ");
            if (SdlEventCodes.Pressed == evt.State)
            {
                os.Append("SDL_PRESSED");
            }
            else if (SdlEventCodes.Released == evt.State)
            {
                os.Append("SDL_RELEASED");
            }
            else
            {
                os.Append("??").Append((ushort)evt.State);
            }
            os.Append(", (x,y) = (").Append(evt.X).Append(',').Append(evt.Y).Append(")}");
            return os;
        }

        /// <summary>对应 <c>operator&lt;&lt;(std::ostream&amp;, const SDL_JoyAxisEvent&amp;)</c>。</summary>
        public static StringBuilder WriteDebug(this StringBuilder os, SdlJoyAxisEvent evt)
        {
            os.Append("{SDL_JOYAXIS, which = ").Append((ushort)evt.Which)
              .Append(", axis = ").Append((ushort)evt.Axis).Append(", value = ").Append(evt.Value).Append('}');
            return os;
        }

        /// <summary>对应 <c>operator&lt;&lt;(std::ostream&amp;, const SDL_JoyBallEvent&amp;)</c>。</summary>
        public static StringBuilder WriteDebug(this StringBuilder os, SdlJoyBallEvent evt)
        {
            os.Append("{SDL_JOYBALLMOTION, which = ").Append((ushort)evt.Which)
              .Append(", ball = ").Append((ushort)evt.Ball)
              .Append(", (xrel,yrel) = ").Append("(").Append(evt.XRel).Append(',').Append(evt.YRel).Append(")}");
            return os;
        }

        /// <summary>对应 <c>operator&lt;&lt;(std::ostream&amp;, const SDL_JoyHatEvent&amp;)</c>。</summary>
        public static StringBuilder WriteDebug(this StringBuilder os, SdlJoyHatEvent evt)
        {
            os.Append("{SDL_JOYHATEVENT, which = ").Append((ushort)evt.Which)
              .Append(", hat = ").Append((ushort)evt.Hat)
              .Append(", value = ").Append((ushort)evt.Value).Append('}');
            return os;
        }

        /// <summary>
        /// 对应 <c>operator&lt;&lt;(std::ostream&amp;, const SDL_JoyButtonEvent&amp;)</c>。
        /// 注意：原始 C++ 实现中，前两个分支写入的起始 <c>"{"</c> 未在后续的
        /// <c>"{SDL_JOYBUTTONEVENT, ..."</c> 之前闭合，导致最终字符串出现连续的两个左花括号；
        /// 这是原始源码本身的行为（并非本次转换引入的问题），按规则"逐行逻辑等价"原样保留。
        /// </summary>
        public static StringBuilder WriteDebug(this StringBuilder os, SdlJoyButtonEvent evt)
        {
            if (SdlEventCodes.JoyButtonDown == evt.Type)
            {
                os.Append("{SDL_JOYBUTTONDOWN, ");
            }
            else if (SdlEventCodes.JoyButtonUp == evt.Type)
            {
                os.Append("{SDL_JOYBUTTONUP, ");
            }
            else
            {
                os.Append("{??unknown ").Append(evt.Type);
                return os;
            }
            os.Append("{SDL_JOYBUTTONEVENT, which = ").Append((ushort)evt.Which)
              .Append(", button = ").Append((ushort)evt.Button).Append(", state = ");
            if (SdlEventCodes.Pressed == evt.State)
            {
                os.Append("SDL_PRESSED}");
            }
            else if (SdlEventCodes.Released == evt.State)
            {
                os.Append("SDL_RELEASED}");
            }
            else
            {
                os.Append("??").Append((ushort)evt.State).Append('}');
            }
            return os;
        }

        /// <summary>对应 <c>operator&lt;&lt;(std::ostream&amp;, const SDL_QuitEvent&amp;)</c>（参数未被读取）。</summary>
        public static StringBuilder WriteDebug(this StringBuilder os, SdlQuitEvent evt)
        {
            os.Append("{SDL_QUITEVENT}");
            return os;
        }

        /// <summary>对应 <c>operator&lt;&lt;(std::ostream&amp;, const SDL_SysWMEvent&amp;)</c>（参数未被读取）。</summary>
        public static StringBuilder WriteDebug(this StringBuilder os, SdlSysWmEvent evt)
        {
            os.Append("{SDL_SYSWMEVENT}");
            return os;
        }

        /// <summary>
        /// 对应 <c>operator&lt;&lt;(std::ostream&amp;, const Rect&amp;)</c>。
        /// Rect 依据既有约定（见 Utils.cs）映射为 <see cref="Rectangle"/>：rect.h → Height，rect.w → Width。
        /// </summary>
        public static StringBuilder WriteDebug(this StringBuilder os, Rectangle rect)
        {
            os.Append("(x,y,h,w) = (").Append(rect.X).Append(',').Append(rect.Y).Append(',')
              .Append(rect.Height).Append(',').Append(rect.Width).Append(')');
            return os;
        }

        /// <summary>
        /// 对应 <c>operator&lt;&lt;(std::ostream&amp;, const Point&amp;)</c>。
        /// Point 依据既有约定（见 Utils.cs）映射为 <see cref="Int2"/>。
        /// </summary>
        public static StringBuilder WriteDebug(this StringBuilder os, Int2 p)
        {
            os.Append("(x,y) = (").Append(p.X).Append(',').Append(p.Y).Append(')');
            return os;
        }
    }
}
