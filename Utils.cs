// <自动生成> 对应 C++ 源文件：Utils.h + Utils.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。

using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Stride.Core.Mathematics;

namespace FlareEngine
{
    // 项目自定义别名 SoundID/StatusID/XPScalingTableID/ItemID/ItemSetID/PowerID
    // 已在 GlobalUsings.cs 中全局定义，此处不再重复声明（规则要求直接使用）。

    /// <summary>
    /// Point / FPoint / Rect / Color 在 C++ 版本中是自定义几何与颜色类型。
    /// 依据强制规则（禁止自定义数学类型，必须使用 Stride.Core.Mathematics），
    /// 分别映射为 Int2 / Vector2 / Rectangle / Color，不在此重新定义同名类。
    /// 以下扩展方法仅保留原始类型无法通过 Stride 内置类型表达的"额外行为"
    /// （截断/加宽转换、坐标对齐、颜色位打包），以完整保留原始逻辑。
    /// </summary>
    public static class GeometryExtensions
    {
        /// <summary>
        /// 对应 C++ 中 <c>explicit Point(const FPoint&amp;)</c>：按截断方式转为整数坐标。
        /// </summary>
        public static Int2 ToInt2(this Vector2 fp)
        {
            return new Int2((int)fp.X, (int)fp.Y);
        }

        /// <summary>
        /// 对应 C++ 中 <c>FPoint(Point)</c>：按加宽方式转为浮点坐标。
        /// </summary>
        public static Vector2 ToVector2(this Int2 p)
        {
            return new Vector2(p.X, p.Y);
        }

        /// <summary>
        /// 对应 C++ 中 <c>FPoint::align()</c>：原地把浮点坐标对齐到 1/16 的最近倍数。
        /// 选择 1/(2^4) 是因为它是一个"好"的浮点数，可以消除99%的舍入误差。
        /// 使用 ref 扩展方法以精确保留原始"原地修改调用者变量"的语义。
        /// </summary>
        public static void Align(this ref Vector2 fp)
        {
            fp.X = MathF.Floor(fp.X / 0.0625f) * 0.0625f;
            fp.Y = MathF.Floor(fp.Y / 0.0625f) * 0.0625f;
        }
    }

    public static class ColorExtensions
    {
        /// <summary>
        /// 对应 C++ 中 <c>Color::encodeRGBA()</c>：将颜色打包为 A 在最低字节、R 在最高字节的 32 位整数。
        /// 注意：此打包顺序与 Stride 内置的 Color.ToRgba() 不同，为保持逻辑等价，禁止替换为内置实现。
        /// </summary>
        public static uint EncodeRgba(this Color c)
        {
            uint result = c.A;
            result |= (uint)c.R << 24;
            result |= (uint)c.G << 16;
            result |= (uint)c.B << 8;
            return result;
        }

        /// <summary>
        /// 对应 C++ 中 <c>Color::decodeRGBA(uint32_t)</c>：原地从打包整数还原颜色分量。
        /// </summary>
        public static void DecodeRgba(this ref Color c, uint encoded)
        {
            c.A = (byte)encoded;
            c.R = (byte)(encoded >> 24);
            c.G = (byte)(encoded >> 16);
            c.B = (byte)(encoded >> 8);
        }
    }

    /// <summary>
    /// 对应 C++ 的 Timer 类：简单的倒计时/正计时计时器。
    /// </summary>
    public class Timer
    {
        public const int End = 0;
        public const int Begin = 1;

        private uint current;
        private uint duration;

        public Timer(uint duration = 0)
        {
            current = 0;
            this.duration = duration;
        }

        /// <summary>
        /// 对应 getCurrent()/setCurrent()：setter 中保留原始的"钳制到 duration 上限"逻辑。
        /// </summary>
        public uint Current
        {
            get => current;
            set
            {
                current = value;
                if (current > duration)
                    current = duration;
            }
        }

        /// <summary>
        /// 对应 getDuration()/setDuration()：setter 中保留原始的"同时重置 current"逻辑。
        /// </summary>
        public uint Duration
        {
            get => duration;
            set
            {
                current = duration = value;
            }
        }

        public bool Tick()
        {
            if (current > 0)
                current--;

            if (current == 0)
                return true;

            return false;
        }

        public bool IsEnd()
        {
            return current == 0;
        }

        public bool IsBegin()
        {
            return current == duration;
        }

        public void Reset(int type)
        {
            if (type == End)
                current = 0;
            else if (type == Begin)
                current = duration;
        }

        public bool IsWholeSecond()
        {
            var settings = SharedResources.Settings!;
            return current % settings.MaxFramesPerSec == 0;
        }
    }

    /// <summary>
    /// 对应 C++ 的 FMinMax 类：一个浮点最小/最大值区间。
    /// </summary>
    public class FMinMax
    {
        public float Min { get; set; }
        public float Max { get; set; }

        public FMinMax()
        {
            Min = 0;
            Max = 0;
        }
    }

    /// <summary>
    /// 对应 C++ 中用于 LOG_MSG 队列的 SDL_LogPriority。
    /// 业务逻辑层不应直接依赖 SDL 类型（规则 7），因此改为最小化的自定义枚举，
    /// 仅保留原始代码实际使用到的两个取值（INFO / ERROR），不改变任何分支判断逻辑。
    /// </summary>
    public enum LogPriority
    {
        Info,
        Error
    }

    /// <summary>
    /// 依据规则 7（业务逻辑禁止直接调用 SDL2 静态方法，必须通过接口实例调用）与规则 39
    /// （SDL 相关类型改用自定义接口封装）新增的抽象接口，封装原始代码中对 SDL 系统日志/
    /// 消息弹窗/退出流程的调用。具体的 SDL 实现将在后续 SDLxxx 单元中提供。
    /// </summary>
    public interface IPlatformLogService
    {
        /// <summary>对应 SDL_LogMessageV，向系统/控制台日志输出一条消息。</summary>
        void NativeLog(LogPriority priority, string message);

        /// <summary>对应 SDL_ShowSimpleMessageBox(SDL_MESSAGEBOX_ERROR, ...)。</summary>
        void ShowErrorMessageBox(string title, string message);

        /// <summary>
        /// 对应 SDL_ShowMessageBox，返回用户点击的按钮 id
        /// （0=Quit, 1=Continue, 2=Reset, 3=Safe Video，与原始按钮顺序一致）。
        /// </summary>
        int ShowLockFileMessageBox(string title, string message);

        /// <summary>对应 SDL_Quit()。</summary>
        void QuitNativeSystem();
    }

    /// <summary>
    /// Utils
    ///
    /// 各种工具结构体、枚举、函数（对应 C++ 的 <c>namespace Utils</c>）。
    /// </summary>
    public static class Utils
    {
        // Alignment: For aligning objects. 0-8 are screen-relative, 9-17 are menu frame relative.
        public const int AlignTopLeft = 0;
        public const int AlignTop = 1;
        public const int AlignTopRight = 2;
        public const int AlignLeft = 3;
        public const int AlignCenter = 4;
        public const int AlignRight = 5;
        public const int AlignBottomLeft = 6;
        public const int AlignBottom = 7;
        public const int AlignBottomRight = 8;
        public const int AlignFrameTopLeft = 9;
        public const int AlignFrameTop = 10;
        public const int AlignFrameTopRight = 11;
        public const int AlignFrameLeft = 12;
        public const int AlignFrameCenter = 13;
        public const int AlignFrameRight = 14;
        public const int AlignFrameBottomLeft = 15;
        public const int AlignFrameBottom = 16;
        public const int AlignFrameBottomRight = 17;

        public static int LockIndex { get; set; }
        public static bool LogFileInit { get; set; }
        public static bool LogFileCreated { get; set; }
        public static string LogPath { get; set; } = string.Empty;
        public static Queue<(LogPriority Priority, string Message)> LogMsg { get; } = new();

        /// <summary>
        /// SDL 相关能力的注入点（详见 IPlatformLogService 说明）。在具体 SDL 实现单元完成转换前，
        /// 若未设置该属性，则等价于原始代码中 SDL 调用失败/无操作的情况（不会抛出异常）。
        /// </summary>
        public static IPlatformLogService? PlatformLog { get; set; }

        public static Vector2 ScreenToMap(int x, int y, float camx, float camy)
        {
            Vector2 r = default;
            EngineSettings eset = SharedResources.Eset!;
            Settings settings = SharedResources.Settings!;
            if (eset.Tileset.Orientation == EngineSettings.TilesetSettings.TilesetIsometric)
            {
                float scrx = (x - settings.ViewWHalf) * 0.5f;
                float scry = (y - settings.ViewHHalf) * 0.5f;

                r.X = (eset.Tileset.UnitsPerPixelX * scrx) + (eset.Tileset.UnitsPerPixelY * scry) + camx;
                r.Y = (eset.Tileset.UnitsPerPixelY * scry) - (eset.Tileset.UnitsPerPixelX * scrx) + camy;
            }
            else
            {
                r.X = (float)(x - settings.ViewWHalf) * (eset.Tileset.UnitsPerPixelX) + camx;
                r.Y = (float)(y - settings.ViewHHalf) * (eset.Tileset.UnitsPerPixelY) + camy;
            }
            return r;
        }

        /// <summary>
        /// 返回摄像机在给定位置时，屏幕上某个 (x,y) 点对应的地图坐标点（原始注释保留其含义，
        /// 尽管命名为 mapToScreen，实现方式与原始代码一致）。
        /// </summary>
        public static Int2 MapToScreen(float x, float y, float camx, float camy)
        {
            Int2 r = default;
            EngineSettings eset = SharedResources.Eset!;
            Settings settings = SharedResources.Settings!;

            // adjust to the center of the viewport
            // we do this calculation first to avoid negative integer division
            float adjustX = (settings.ViewWHalf + 0.5f) * eset.Tileset.UnitsPerPixelX;
            float adjustY = (settings.ViewHHalf + 0.5f) * eset.Tileset.UnitsPerPixelY;

            if (eset.Tileset.Orientation == EngineSettings.TilesetSettings.TilesetIsometric)
            {
                r.X = (int)MathF.Floor(((x - camx - y + camy + adjustX) / eset.Tileset.UnitsPerPixelX) + 0.5f);
                r.Y = (int)MathF.Floor(((x - camx + y - camy + adjustY) / eset.Tileset.UnitsPerPixelY) + 0.5f);
            }
            else if (eset.Tileset.Orientation == EngineSettings.TilesetSettings.TilesetOrthogonal)
            {
                r.X = (int)((x - camx + adjustX) / eset.Tileset.UnitsPerPixelX);
                r.Y = (int)((y - camy + adjustY) / eset.Tileset.UnitsPerPixelY);
            }
            return r;
        }

        /// <summary>
        /// 对位置施加距离和方向参数。
        /// </summary>
        public static Vector2 CalcVector(Vector2 pos, int direction, float dist)
        {
            Vector2 p;
            p.X = pos.X;
            p.Y = pos.Y;

            float distStraight = dist;
            float distDiag = dist * 0.7071f; //  1/sqrt(2)

            switch (direction)
            {
                case 0:
                    p.X -= distDiag;
                    p.Y += distDiag;
                    break;
                case 1:
                    p.X -= distStraight;
                    break;
                case 2:
                    p.X -= distDiag;
                    p.Y -= distDiag;
                    break;
                case 3:
                    p.Y -= distStraight;
                    break;
                case 4:
                    p.X += distDiag;
                    p.Y -= distDiag;
                    break;
                case 5:
                    p.X += distStraight;
                    break;
                case 6:
                    p.X += distDiag;
                    p.Y += distDiag;
                    break;
                case 7:
                    p.Y += distStraight;
                    break;
            }
            return p;
        }

        public static float CalcDist(Vector2 p1, Vector2 p2)
        {
            return MathF.Sqrt((p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y));
        }

        /// <summary>
        /// target 是否在以 center 为中心、radius 为半径的区域内？
        /// </summary>
        public static bool IsWithinRadius(Vector2 center, float radius, Vector2 target)
        {
            return CalcDist(center, target) < radius;
        }

        /// <summary>
        /// target 是否在矩形 r 定义的区域内？
        /// </summary>
        public static bool IsWithinRect(Rectangle r, Int2 target)
        {
            return target.X >= r.X && target.Y >= r.Y && target.X < r.X + r.Width && target.Y < r.Y + r.Height;
        }

        public static byte CalcDirection(float x0, float y0, float x1, float y1)
        {
            float theta = CalcTheta(x0, y0, x1, y1);
            float val = theta / (MathF.PI / 4);
            int dir = (int)(((val < 0) ? MathF.Ceiling(val - 0.5f) : MathF.Floor(val + 0.5f)) + 4);
            dir = (dir + 1) % 8;
            if (dir >= 0)
                return (byte)dir;
            else
                return 0;
        }

        // convert cartesian to polar theta where (x1,x2) is the origin
        public static float CalcTheta(float x1, float y1, float x2, float y2)
        {
            // calculate base angle
            float dx = x2 - x1;
            float dy = y2 - y1;
            float exactDx = x2 - x1;
            float theta;

            // convert cartesian to polar coordinates
            if (exactDx == 0)
            {
                if (dy > 0.0) theta = MathF.PI / 2.0f;
                else theta = -MathF.PI / 2.0f;
            }
            else
            {
                theta = MathF.Atan(dy / dx);
                if (dx < 0.0 && dy >= 0.0) theta += MathF.PI;
                if (dx < 0.0 && dy < 0.0) theta -= MathF.PI;
            }
            return theta;
        }

        public static string AbbreviateKilo(int amount)
        {
            var msg = SharedResources.Msg!;
            StringBuilder ss = new StringBuilder();
            if (amount < 1000)
                ss.Append(amount);
            else
                ss.Append(amount / 1000).Append(msg.Get("k"));

            return ss.ToString();
        }

        public static void AlignToScreenEdge(int alignment, ref Rectangle r)
        {
            Settings settings = SharedResources.Settings!;
            EngineSettings eset = SharedResources.Eset!;

            if (alignment == AlignTopLeft)
            {
                // do nothing
            }
            else if (alignment == AlignTop)
            {
                r.X = (settings.ViewWHalf - r.Width / 2) + r.X;
            }
            else if (alignment == AlignTopRight)
            {
                r.X = (settings.ViewW - r.Width) + r.X;
            }
            else if (alignment == AlignLeft)
            {
                r.Y = (settings.ViewHHalf - r.Height / 2) + r.Y;
            }
            else if (alignment == AlignCenter)
            {
                r.X = (settings.ViewWHalf - r.Width / 2) + r.X;
                r.Y = (settings.ViewHHalf - r.Height / 2) + r.Y;
            }
            else if (alignment == AlignRight)
            {
                r.X = (settings.ViewW - r.Width) + r.X;
                r.Y = (settings.ViewHHalf - r.Height / 2) + r.Y;
            }
            else if (alignment == AlignBottomLeft)
            {
                r.Y = (settings.ViewH - r.Height) + r.Y;
            }
            else if (alignment == AlignBottom)
            {
                r.X = (settings.ViewWHalf - r.Width / 2) + r.X;
                r.Y = (settings.ViewH - r.Height) + r.Y;
            }
            else if (alignment == AlignBottomRight)
            {
                r.X = (settings.ViewW - r.Width) + r.X;
                r.Y = (settings.ViewH - r.Height) + r.Y;
            }
            else if (alignment == AlignFrameTopLeft)
            {
                r.X = ((settings.ViewW - eset.Resolutions.FrameW) / 2) + r.X;
                r.Y = ((settings.ViewH - eset.Resolutions.FrameH) / 2) + r.Y;
            }
            else if (alignment == AlignFrameTop)
            {
                r.X = ((settings.ViewW - eset.Resolutions.FrameW) / 2) + (eset.Resolutions.FrameW / 2 - r.Width / 2) + r.X;
                r.Y = ((settings.ViewH - eset.Resolutions.FrameH) / 2) + r.Y;
            }
            else if (alignment == AlignFrameTopRight)
            {
                r.X = ((settings.ViewW - eset.Resolutions.FrameW) / 2) + (eset.Resolutions.FrameW - r.Width) + r.X;
                r.Y = ((settings.ViewH - eset.Resolutions.FrameH) / 2) + r.Y;
            }
            else if (alignment == AlignFrameLeft)
            {
                r.X = ((settings.ViewW - eset.Resolutions.FrameW) / 2) + r.X;
                r.Y = ((settings.ViewH - eset.Resolutions.FrameH) / 2) + (eset.Resolutions.FrameH / 2 - r.Height / 2) + r.Y;
            }
            else if (alignment == AlignFrameCenter)
            {
                r.X = ((settings.ViewW - eset.Resolutions.FrameW) / 2) + (eset.Resolutions.FrameW / 2 - r.Width / 2) + r.X;
                r.Y = ((settings.ViewH - eset.Resolutions.FrameH) / 2) + (eset.Resolutions.FrameH / 2 - r.Height / 2) + r.Y;
            }
            else if (alignment == AlignFrameRight)
            {
                r.X = ((settings.ViewW - eset.Resolutions.FrameW) / 2) + (eset.Resolutions.FrameW - r.Width) + r.X;
                r.Y = ((settings.ViewH - eset.Resolutions.FrameH) / 2) + (eset.Resolutions.FrameH / 2 - r.Height / 2) + r.Y;
            }
            else if (alignment == AlignFrameBottomLeft)
            {
                r.X = ((settings.ViewW - eset.Resolutions.FrameW) / 2) + r.X;
                r.Y = ((settings.ViewH - eset.Resolutions.FrameH) / 2) + (eset.Resolutions.FrameH - r.Height) + r.Y;
            }
            else if (alignment == AlignFrameBottom)
            {
                r.X = ((settings.ViewW - eset.Resolutions.FrameW) / 2) + (eset.Resolutions.FrameW / 2 - r.Width / 2) + r.X;
                r.Y = ((settings.ViewH - eset.Resolutions.FrameH) / 2) + (eset.Resolutions.FrameH - r.Height) + r.Y;
            }
            else if (alignment == AlignFrameBottomRight)
            {
                r.X = ((settings.ViewW - eset.Resolutions.FrameW) / 2) + (eset.Resolutions.FrameW - r.Width) + r.X;
                r.Y = ((settings.ViewH - eset.Resolutions.FrameH) / 2) + (eset.Resolutions.FrameH - r.Height) + r.Y;
            }
            else
            {
                // do nothing
            }
        }

        /// <summary>
        /// 这些函数提供统一的、printf 风格的日志输出方式。
        /// </summary>
        public static void LogInfo(string format, params object?[] args)
        {
            string message = FormatPrintf(format, args);
            Console.WriteLine("[INFO] " + message);
            PlatformLog?.NativeLog(LogPriority.Info, message);

            if (!LogFileInit)
            {
                LogMsg.Enqueue((LogPriority.Info, message));
            }
            else if (LogFileCreated)
            {
                AppendToLogFile("INFO: ", message);
            }
        }

        public static void LogError(string format, params object?[] args)
        {
            string message = FormatPrintf(format, args);
            Console.WriteLine("[ERROR] " + message);
            PlatformLog?.NativeLog(LogPriority.Error, message);

            if (!LogFileInit)
            {
                LogMsg.Enqueue((LogPriority.Error, message));
            }
            else if (LogFileCreated)
            {
                AppendToLogFile("ERROR: ", message);
            }
        }

        public static void LogErrorDialog(string dialogText, params object?[] args)
        {
            // 与原始实现一致：把 dialogText 本身当作格式串的一部分，再用 args 格式化。
            string combinedFormat = "FLARE Error\n" + dialogText;
            string message = FormatPrintf(combinedFormat, args);
            PlatformLog?.ShowErrorMessageBox("FLARE Error", message);
        }

        public static void CreateLogFile()
        {
            var settings = SharedResources.Settings!;
            LogPath = settings.PathConf + "/flare_log.txt";

            // always create a new log file on each launch
            if (Filesystem.FileExists(LogPath))
            {
                Filesystem.RemoveFile(LogPath);
            }

            try
            {
                using StreamWriter logFile = new StreamWriter(LogPath, append: false);
                LogFileCreated = true;
                logFile.Write("### Flare log file\n\n");

                while (LogMsg.Count > 0)
                {
                    (LogPriority priority, string text) = LogMsg.Peek();
                    if (priority == LogPriority.Info)
                        logFile.Write("INFO: ");
                    else if (priority == LogPriority.Error)
                        logFile.Write("ERROR: ");

                    logFile.Write(text);
                    logFile.Write('\n');

                    LogMsg.Dequeue();
                }
            }
            catch (IOException)
            {
                while (LogMsg.Count > 0)
                    LogMsg.Dequeue();

                LogError("Utils: Could not create log file.");
            }

            LogFileInit = true;
        }

        public static void Exit(int code)
        {
            PlatformLog?.QuitNativeSystem();
            LockFileWrite(-1);
            Environment.Exit(code);
        }

        public static void CreateSaveDir(int slot)
        {
            // game slots are currently 1-4
            if (slot == 0) return;

            Settings settings = SharedResources.Settings!;
            EngineSettings eset = SharedResources.Eset!;

            StringBuilder ss = new StringBuilder();
            ss.Append(settings.PathUser).Append("saves/").Append(eset.Misc.SavePrefix).Append('/');

            Filesystem.CreateDir(ss.ToString());

            ss.Append(slot);
            Filesystem.CreateDir(ss.ToString());

            Filesystem.CreateDir(ss + "/fow/");

            Filesystem.CreateDir(ss + "/maps/");
        }

        public static void RemoveSaveDir(int slot)
        {
            // game slots are currently 1-4
            if (slot == 0) return;

            Settings settings = SharedResources.Settings!;
            EngineSettings eset = SharedResources.Eset!;

            StringBuilder ss = new StringBuilder();
            ss.Append(settings.PathUser).Append("saves/").Append(eset.Misc.SavePrefix).Append('/').Append(slot);

            if (Filesystem.IsDirectory(ss.ToString()))
            {
                Filesystem.RemoveDirRecursive(ss.ToString());
            }
        }

        public static Rectangle ResizeToScreen(int w, int h, bool crop, int align)
        {
            Settings settings = SharedResources.Settings!;
            Rectangle r = default;

            // fit to height
            float ratio = settings.ViewH / (float)h;
            r.Width = (int)((float)w * ratio);
            r.Height = settings.ViewH;

            if (!crop)
            {
                // fit to width
                if (r.Width > settings.ViewW)
                {
                    ratio = settings.ViewW / (float)w;
                    r.Height = (int)((float)h * ratio);
                    r.Width = settings.ViewW;
                }
            }

            AlignToScreenEdge(align, ref r);

            return r;
        }

        public static int StringFindCaseInsensitive(string a0, string b0)
        {
            StringBuilder a = new StringBuilder();
            StringBuilder b = new StringBuilder();

            for (int i = 0; i < a0.Length; ++i)
            {
                a.Append(char.ToLowerInvariant(a0[i]));
            }

            for (int i = 0; i < b0.Length; ++i)
            {
                b.Append(char.ToLowerInvariant(b0[i]));
            }

            return a.ToString().IndexOf(b.ToString(), StringComparison.Ordinal);
        }

        public static string FloatToString(float value, int precision)
        {
            string temp = value.ToString("F" + precision, CultureInfo.InvariantCulture);

            // remove trailing zeros (and the separator if it is not needed)
            if (temp.Contains('.') || temp.Contains(','))
            {
                int i = temp.Length;
                int newLength = i;
                while (i > 0)
                {
                    i--;
                    if (temp[i] == '0')
                        newLength = i;
                    else if (temp[i] != '.' && temp[i] != ',')
                    {
                        break;
                    }
                    else
                    {
                        newLength = i;
                        break;
                    }
                }
                temp = temp.Substring(0, newLength);
            }

            return temp;
        }

        public static string GetDurationString(int duration, int precision)
        {
            Settings settings = SharedResources.Settings!;
            var msg = SharedResources.Msg!;
            float realDuration = duration / (float)settings.MaxFramesPerSec;
            string temp = FloatToString(realDuration, precision);

            if (realDuration == 1f)
            {
                return msg.GetV("%s second", temp);
            }
            else
            {
                return msg.GetV("%s seconds", temp);
            }
        }

        public static string SubstituteVarsInString(string s0, Avatar? avatar)
        {
            var inpt = SharedResources.Inpt!;
            string s = s0;

            int begin = s.IndexOf("${", StringComparison.Ordinal);
            while (begin != -1)
            {
                int end = s.IndexOf('}');

                if (end == -1)
                    break;

                int varLen = end - begin + 1;
                string var = s.Substring(begin, varLen);

                if (avatar != null && var == "${AVATAR_NAME}")
                {
                    s = s.Remove(begin, varLen).Insert(begin, avatar.Stats.Name);
                }
                else if (avatar != null && var == "${AVATAR_CLASS}")
                {
                    s = s.Remove(begin, varLen).Insert(begin, avatar.Stats.GetShortClass());
                }
                else if (var == "${INPUT_MOVEMENT}")
                {
                    s = s.Remove(begin, varLen).Insert(begin, inpt.GetMovementString());
                }
                else if (var == "${INPUT_ATTACK}")
                {
                    s = s.Remove(begin, varLen).Insert(begin, inpt.GetAttackString());
                }
                else
                {
                    LogError("'%s' is not a valid string variable name.", var);
                    // strip the brackets from the variable
                    s = s.Remove(begin, varLen).Insert(begin, var.Substring(2, var.Length - 3));
                }

                begin = s.IndexOf("${", StringComparison.Ordinal);
            }

            return s;
        }

        /// <summary>
        /// 让两个点之间保持在一定范围内。
        /// </summary>
        public static Vector2 ClampDistance(float rangeMin, float rangeMax, Vector2 src, Vector2 target)
        {
            Vector2 limitTarget = target;

            if (rangeMin > 0 || rangeMax > 0)
            {
                if (rangeMin > rangeMax)
                    rangeMax = rangeMin;

                float dist = MathF.Sqrt(MathF.Pow(target.X - src.X, 2f) + MathF.Pow(target.Y - src.Y, 2f));
                if (dist < rangeMin)
                {
                    float ratio = rangeMin / dist;
                    limitTarget.X = src.X + (ratio * (target.X - src.X));
                    limitTarget.Y = src.Y + (ratio * (target.Y - src.Y));
                }
                else if (dist > rangeMax)
                {
                    float ratio = rangeMax / dist;
                    limitTarget.X = src.X + (ratio * (target.X - src.X));
                    limitTarget.Y = src.Y + (ratio * (target.Y - src.Y));
                }
            }

            return limitTarget;
        }

        /// <summary>
        /// 比较两个矩形，返回它们是否重叠。
        /// </summary>
        public static bool RectsOverlap(Rectangle a, Rectangle b)
        {
            Int2 a1 = new Int2(a.X, a.Y);
            Int2 a2 = new Int2(a.X + a.Width, a.Y);
            Int2 a3 = new Int2(a.X, a.Y + a.Height);
            Int2 a4 = new Int2(a.X + a.Width, a.Y + a.Height);

            Int2 b1 = new Int2(b.X, b.Y);
            Int2 b2 = new Int2(b.X + b.Width, b.Y);
            Int2 b3 = new Int2(b.X, b.Y + b.Height);
            Int2 b4 = new Int2(b.X + b.Width, b.Y + b.Height);

            bool aInB = IsWithinRect(b, a1) || IsWithinRect(b, a2) || IsWithinRect(b, a3) || IsWithinRect(b, a4);
            bool bInA = IsWithinRect(a, b1) || IsWithinRect(a, b2) || IsWithinRect(a, b3) || IsWithinRect(a, b4);

            return aInB || bInA;
        }

        public static int RotateDirection(int direction, int val)
        {
            direction += val;
            if (direction > 7)
                direction -= 7;
            else if (direction < 0)
                direction += 7;

            return direction;
        }

        public static string GetTimeString(ulong time)
        {
            StringBuilder ss = new StringBuilder();
            ulong hours = (time / 60) / 60;
            if (hours < 100)
                ss.Append(hours.ToString("D2", CultureInfo.InvariantCulture));
            else
                ss.Append(hours);

            ss.Append(':');
            ulong minutes = (time / 60) % 60;
            ss.Append(minutes.ToString("D2", CultureInfo.InvariantCulture));

            ss.Append(':');
            ulong seconds = time % 60;
            ss.Append(seconds.ToString("D2", CultureInfo.InvariantCulture));

            return ss.ToString();
        }

        /// <summary>
        /// 对应 C++ 中基于 std::collate&lt;char&gt; 的区域相关字符串哈希。
        /// .NET BCL 没有与 std::collate::hash 完全等价的公开 API，这里使用
        /// String.GetHashCode 的序数（Ordinal）版本作为纯 BCL 的等价替代，
        /// 保持"同样的字符串输入产生同样的哈希输出（在同一次进程运行内）"这一使用契约。
        /// 注意：具体哈希数值与原始 C++ 实现不保证按位相同，如有需要按位复现，需人工复核。
        /// </summary>
        public static ulong HashString(string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            uint hash = 2166136261U;
            uint prime = 16777619U;

            foreach (byte b in bytes)
            {
                hash ^= b;
                hash *= prime; // 依靠 uint 自身的 32 位乘法溢出
            }
            return hash;
        }

        public static void LockFileRead()
        {
            if (!Platform.Instance.HasLockFile)
                return;

            var settings = SharedResources.Settings!;
            string lockFilePath = Filesystem.ConvertSlashes(settings.PathConf + "flare_lock");

            try
            {
                using StreamReader infile = new StreamReader(lockFilePath);
                while (!infile.EndOfStream)
                {
                    string? rawLine = infile.ReadLine();
                    string line = rawLine ?? string.Empty;

                    if (line.Length == 0 || line[0] == '#')
                        continue;

                    LockIndex = Parse.ToInt(line);
                }
            }
            catch (IOException)
            {
                // 对应原始代码中 infile.open 失败后 infile.good() 恒为 false、循环不执行的行为。
            }

            if (LockIndex < 0)
                LockIndex = 0;
        }

        public static void LockFileWrite(int increment)
        {
            if (!Platform.Instance.HasLockFile)
                return;

            var settings = SharedResources.Settings!;
            string lockFilePath = settings.PathConf + "flare_lock";

            if (increment < 0)
            {
                if (LockIndex == 0)
                    return;

                // refresh LOCK_INDEX in case any other instances were closed while this instance was running
                LockFileRead();
            }

            try
            {
                using StreamWriter outfile = new StreamWriter(lockFilePath, append: false);
                LockIndex += increment;
                outfile.WriteLine("# Flare lock file. Counts instances of Flare");
                outfile.WriteLine(LockIndex);
            }
            catch (IOException)
            {
                // 对应原始代码中 outfile.is_open() 为 false 时跳过写入的行为。
            }
        }

        public static void LockFileCheck()
        {
            return;
#pragma warning disable CS0162 // 与原始 C++ 源码一致地保留了提前 return 之后的不可达代码
            if (!Platform.Instance.HasLockFile)
                return;

            LockIndex = 0;

            LockFileRead();

            if (LockIndex > 0)
            {
                int buttonId = PlatformLog?.ShowLockFileMessageBox(
                    "Flare",
                    "Flare is unable to launch properly. This may be because it did not exit properly, or because there is another instance running.\n\nIf Flare crashed, it is recommended to try 'Safe Video' mode. This will try launching Flare with the minimum video settings.\n\nIf Flare is already running, you may:\n- 'Quit' Flare (safe, recommended)\n- 'Continue' to launch another copy of Flare.\n- 'Reset' the counter which tracks the number of copies of Flare that are currently running.\n  If this dialog is shown every time you launch Flare, this option should fix it.")
                    ?? 0;

                if (buttonId == 0)
                {
                    LockFileWrite(1);
                    Exit(1);
                }
                else if (buttonId == 2)
                {
                    LockIndex = 0;
                }
                else if (buttonId == 3)
                {
                    LockIndex = 0;
                    SharedResources.Settings!.SafeVideo = true;
                }
            }

            LockFileWrite(1);
#pragma warning restore CS0162
        }

        /// <summary>
        /// 对应 C++ 中根据字节序返回 RGBA 位掩码的 setSDL_RGBA。使用 BitConverter.IsLittleEndian
        /// 作为纯 BCL 的运行时字节序判断，替代原始的编译期 SDL_BYTEORDER 宏分支，
        /// 分支结构与取值保持一致。
        /// </summary>
        public static void SetSdlRgba(out uint rmask, out uint gmask, out uint bmask, out uint amask)
        {
            if (!BitConverter.IsLittleEndian)
            {
                rmask = 0xff000000;
                gmask = 0x00ff0000;
                bmask = 0x0000ff00;
                amask = 0x000000ff;
            }
            else
            {
                rmask = 0x000000ff;
                gmask = 0x0000ff00;
                bmask = 0x00ff0000;
                amask = 0xff000000;
            }
        }

        public static string CreateMinMaxString(float min, float max, int precision)
        {
            string r;
            if (min < max)
            {
                r = string.Empty;
                if (min != max)
                    r = FloatToString(min, precision) + '-';
                r += FloatToString(max, precision);
            }
            else
            {
                r = FloatToString(min, precision);
            }
            return r;
        }

        /// <summary>
        /// 对应 C++ 中 <c>char* Utils::strdup(const std::string&amp;)</c>。
        /// C# 字符串是不可变的托管对象，不存在"手动复制一份堆内存"的必要性；
        /// 为保留调用契约（返回一个独立的字符串实例）而非直接返回同一引用，
        /// 使用字符数组构造一个新的 string 实例，语义上等价于原始的堆复制行为。
        /// </summary>
        public static string StrDup(string str)
        {
            return new string(str.ToCharArray());
        }

        /// <summary>
        /// 最小化的 printf 兼容格式化器，支持 %s/%d/%i/%u/%f(.N)/%c/%x/%X/%%，
        /// 以及可选的 l/ll/z/h 长度修饰符（被忽略，不影响取值语义）。
        /// 用于在纯 .NET BCL 环境下复现原始 C 风格可变参数日志/错误函数的调用约定
        /// （规则 6 禁止 P/Invoke，因此无法直接调用 C 运行时的 vsnprintf）。
        /// </summary>
        public static string FormatPrintf(string format, params object?[] args)
        {
            StringBuilder sb = new StringBuilder();
            int argIndex = 0;

            for (int i = 0; i < format.Length; i++)
            {
                char ch = format[i];
                if (ch != '%')
                {
                    sb.Append(ch);
                    continue;
                }

                i++;
                if (i >= format.Length)
                {
                    sb.Append('%');
                    break;
                }

                if (format[i] == '%')
                {
                    sb.Append('%');
                    continue;
                }

                int flagsStart = i;
                while (i < format.Length && (format[i] == '.' || char.IsDigit(format[i]) || format[i] == '-' || format[i] == '+' || format[i] == ' ' || format[i] == '#'))
                {
                    i++;
                }
                string flagsAndWidth = format.Substring(flagsStart, i - flagsStart);

                while (i < format.Length && (format[i] == 'l' || format[i] == 'z' || format[i] == 'h'))
                {
                    i++;
                }

                if (i >= format.Length)
                {
                    sb.Append('%').Append(flagsAndWidth);
                    break;
                }

                char specifier = format[i];
                object? arg = argIndex < args.Length ? args[argIndex] : null;

                switch (specifier)
                {
                    case 's':
                        sb.Append(arg?.ToString() ?? string.Empty);
                        argIndex++;
                        break;
                    case 'd':
                    case 'i':
                    case 'u':
                        sb.Append(Convert.ToInt64(arg ?? 0, CultureInfo.InvariantCulture));
                        argIndex++;
                        break;
                    case 'f':
                    case 'F':
                        {
                            int precision = 6;
                            int dot = flagsAndWidth.IndexOf('.');
                            if (dot >= 0 && dot + 1 < flagsAndWidth.Length)
                                int.TryParse(flagsAndWidth.Substring(dot + 1), out precision);
                            double value = Convert.ToDouble(arg ?? 0.0, CultureInfo.InvariantCulture);
                            sb.Append(value.ToString("F" + precision, CultureInfo.InvariantCulture));
                            argIndex++;
                        }
                        break;
                    case 'c':
                        sb.Append(Convert.ToChar(arg ?? ' '));
                        argIndex++;
                        break;
                    case 'x':
                        sb.Append(Convert.ToInt64(arg ?? 0, CultureInfo.InvariantCulture).ToString("x", CultureInfo.InvariantCulture));
                        argIndex++;
                        break;
                    case 'X':
                        sb.Append(Convert.ToInt64(arg ?? 0, CultureInfo.InvariantCulture).ToString("X", CultureInfo.InvariantCulture));
                        argIndex++;
                        break;
                    default:
                        sb.Append('%').Append(flagsAndWidth).Append(specifier);
                        break;
                }
            }

            return sb.ToString();
        }

        private static void AppendToLogFile(string prefix, string message)
        {
            try
            {
                using StreamWriter writer = new StreamWriter(LogPath, append: true);
                writer.Write(prefix);
                writer.Write(message);
                writer.Write('\n');
            }
            catch (IOException)
            {
                // 对应原始 C 语言 fopen 失败（返回 NULL）时静默跳过写入的行为。
            }
        }
    }
}
