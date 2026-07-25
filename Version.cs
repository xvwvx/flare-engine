// <自动生成> 对应 C++ 源文件：Version.h + Version.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using System.Globalization;
using System.Text;

namespace FlareEngine
{
    /// <summary>
    /// Version
    ///
    /// 版本号（major.minor.patch），对应 C++ 的 <c>class Version</c>。
    /// </summary>
    public class Version
    {
        /// <summary>主版本号。</summary>
        public ushort X;

        /// <summary>次版本号。</summary>
        public ushort Y;

        /// <summary>补丁版本号。</summary>
        public ushort Z;

        /// <summary>
        /// 对应 C++ 构造函数 <c>Version::Version(unsigned short _x, unsigned short _y, unsigned short _z)</c>
        /// 的成员初始化列表，顺序与原始一致。
        /// </summary>
        public Version(ushort x = 0, ushort y = 0, ushort z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        // 对应 C++ 析构函数 Version::~Version()：原实现为空函数体，不释放任何资源，
        // 因此本单元无需实现终结器或 IDisposable。

        /// <summary>对应 C++ <c>bool Version::operator==(const Version&amp; v)</c>。</summary>
        public static bool operator ==(Version a, Version b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        /// <summary>对应 C++ <c>bool Version::operator!=(const Version&amp; v)</c>。</summary>
        public static bool operator !=(Version a, Version b)
        {
            return !(a == b);
        }

        /// <summary>对应 C++ <c>bool Version::operator&gt;(const Version&amp; v)</c>。</summary>
        public static bool operator >(Version a, Version b)
        {
            if (a.X > b.X)
                return true;
            else if (a.X == b.X && a.Y > b.Y)
                return true;
            else if (a.X == b.X && a.Y == b.Y && a.Z > b.Z)
                return true;
            else
                return false;
        }

        /// <summary>对应 C++ <c>bool Version::operator&gt;=(const Version&amp; v)</c>。</summary>
        public static bool operator >=(Version a, Version b)
        {
            return (a == b || a > b);
        }

        /// <summary>对应 C++ <c>bool Version::operator&lt;(const Version&amp; v)</c>。</summary>
        public static bool operator <(Version a, Version b)
        {
            return !(a >= b);
        }

        /// <summary>对应 C++ <c>bool Version::operator&lt;=(const Version&amp; v)</c>。</summary>
        public static bool operator <=(Version a, Version b)
        {
            return (a == b || a < b);
        }

        /// <summary>
        /// C# 语言要求：重载 <c>==</c>/<c>!=</c> 时应同时重写 <c>Equals</c>，
        /// 以避免编译器警告 CS0660。此方法直接复用上方已转换的 <c>operator==</c> 逻辑，
        /// 不引入任何额外的判等规则，属于语言层面的必要补充，非 C++ 源码原有内容。
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is Version other && this == other;
        }

        /// <summary>
        /// C# 语言要求：重载 <c>==</c>/<c>!=</c> 时应同时重写 <c>GetHashCode</c>，
        /// 以避免编译器警告 CS0661。属于语言层面的必要补充，非 C++ 源码原有内容。
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        /// <summary>对应 C++ <c>std::string Version::getString()</c>。</summary>
        public string GetString()
        {
            StringBuilder ss = new StringBuilder();

            // major
            ss.Append(X.ToString(CultureInfo.InvariantCulture)).Append('.');

            // minor
            if (Y >= 100 || Y == 0)
            {
                ss.Append(Y.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                ss.Append(Y.ToString("D2", CultureInfo.InvariantCulture));
            }

            // don't bother printing if there's no patch version
            if (Z == 0)
                return ss.ToString();

            ss.Append('.');

            // patch
            if (Z >= 100)
            {
                ss.Append(Z.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                ss.Append(Z.ToString("D2", CultureInfo.InvariantCulture));
            }

            return ss.ToString();
        }

        /// <summary>对应 C++ <c>void Version::setFromString(const std::string&amp; s)</c>。</summary>
        public void SetFromString(string s)
        {
            string val = s + '.';

            X = (ushort)Parse.PopFirstInt(ref val, '.');

            string strY = Parse.PopFirstString(ref val, '.');
            Y = (ushort)Parse.ToInt(strY);
            if (strY.Length == 1)
                Y = (ushort)(Y * 10);

            string strZ = Parse.PopFirstString(ref val, '.');
            Z = (ushort)Parse.ToInt(strZ);
            if (strZ.Length == 1)
                Z = (ushort)(Z * 10);
        }
    }

    /// <summary>
    /// 封装对 <c>SDL_GetPlatform()</c> 的调用（规则：业务逻辑禁止直接调用 SDL2 静态方法，
    /// 必须通过接口实例调用）。具体的 SDL 绑定实现将由后续 SDL 封装单元提供并注入
    /// <see cref="VersionInfo.PlatformInfoService"/>。
    /// </summary>
    public interface ISdlPlatformInfoService
    {
        /// <summary>对应 SDL_GetPlatform()，返回当前运行平台名称（如 "Windows"、"Linux"）。</summary>
        string GetPlatformName();
    }

    /// <summary>
    /// VersionInfo
    ///
    /// 对应 C++ 的 <c>namespace VersionInfo</c>：命名空间级常量/全局变量转为静态类的
    /// 常量/静态属性，命名空间级全局函数转为静态方法。
    /// </summary>
    public static class VersionInfo
    {
        /// <summary>对应 C++ <c>const std::string NAME = "Flare";</c>。</summary>
        public const string Name = "Flare";

        /// <summary>
        /// 对应 C++ <c>extern Version ENGINE;</c>，定义处为
        /// <c>Version VersionInfo::ENGINE(1, 15, 36);</c>。
        /// </summary>
        public static Version Engine { get; set; } = new Version(1, 15, 36);

        /// <summary>
        /// 对应 C++ <c>extern Version MIN;</c>，定义处为
        /// <c>Version VersionInfo::MIN(0, 0, 0);</c>。
        /// </summary>
        public static Version Min { get; set; } = new Version(0, 0, 0);

        /// <summary>
        /// 对应 C++ <c>extern Version MAX;</c>，定义处为
        /// <c>Version VersionInfo::MAX(USHRT_MAX, USHRT_MAX, USHRT_MAX);</c>。
        /// <c>USHRT_MAX</c>（来自 &lt;limits.h&gt;）对应 <c>ushort.MaxValue</c>。
        /// </summary>
        public static Version Max { get; set; } = new Version(ushort.MaxValue, ushort.MaxValue, ushort.MaxValue);

        /// <summary>
        /// SDL 平台信息服务的注入点（详见 <see cref="ISdlPlatformInfoService"/> 说明）。
        /// 在具体 SDL 实现单元完成转换、并对该属性赋值之前，等价于原始代码中
        /// <c>SDL_GetPlatform()</c> 返回空字符串的情况，不会抛出异常。
        /// </summary>
        public static ISdlPlatformInfoService? PlatformInfoService { get; set; }

        /// <summary>对应 C++ <c>std::string VersionInfo::createVersionReqString(Version&amp; v1, Version&amp; v2)</c>。</summary>
        public static string CreateVersionReqString(Version v1, Version v2)
        {
            string minVersion = (v1 == Min) ? "" : v1.GetString();
            string maxVersion = (v2 == Max) ? "" : v2.GetString();
            string ret = "";

            if (minVersion != "" || maxVersion != "")
            {
                if (minVersion == maxVersion)
                {
                    ret += minVersion;
                }
                else if (minVersion != "" && maxVersion != "")
                {
                    ret += minVersion + " - " + maxVersion;
                }
                else if (minVersion != "")
                {
                    ret += minVersion + ' ' + (SharedResources.Msg != null ? SharedResources.Msg.Get("or newer") : "or newer");
                }
                else if (maxVersion != "")
                {
                    ret += maxVersion + ' ' + (SharedResources.Msg != null ? SharedResources.Msg.Get("or older") : "or older");
                }
            }

            return ret;
        }

        /// <summary>对应 C++ <c>std::string VersionInfo::createVersionStringFull()</c>。</summary>
        public static string CreateVersionStringFull()
        {
            // example output: Flare 1.0 (Linux)
            string platformName = PlatformInfoService != null ? PlatformInfoService.GetPlatformName() : "";
            return Name + " " + Engine.GetString() + " (" + platformName + ")";
        }
    }
}
