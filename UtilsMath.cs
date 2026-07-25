// <自动生成> 对应 C++ 源文件：UtilsMath.h
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。

namespace FlareEngine
{
    /// <summary>
    /// 对应 C++ 的 <c>namespace Math</c>（仅含内联自由函数）。
    /// C# 没有"命名空间内的自由函数"这一概念，且静态类名不可与 System.Math 冲突，
    /// 因此使用静态类 MathUtils 承载这些静态方法（符合阶段 B 规则："全局函数转换为静态类中的静态方法"）。
    /// </summary>
    public static class MathUtils
    {
        // C++ 中 rand()/RAND_MAX 是 C 运行时的全局伪随机数发生器，纯 .NET BCL 没有与之直接对应的
        // 全局静态入口（System.Random 必须持有实例）。为保持“无需显式初始化即可调用”的原始使用方式，
        // 这里使用一个进程级共享的 System.Random 实例模拟 rand() 的全局语义。
        // RandMax 近似 C 运行时 RAND_MAX 语义，用于 RandBetweenF 的归一化除法，取值参照桌面平台常见实现。
        private static readonly Random Rand = new Random();
        private const int RandMax = int.MaxValue;

        /// <summary>
        /// 返回 value 的符号。
        /// </summary>
        public static int Signum(int value)
        {
            return (0 < value ? 1 : 0) - (value < 0 ? 1 : 0);
        }

        /// <summary>
        /// 返回 minVal 与 maxVal 之间的随机整数。
        /// </summary>
        public static int RandBetween(int minVal, int maxVal)
        {
            if (minVal == maxVal) return minVal;
            int d = maxVal - minVal;
            return minVal + (Rand.Next(0, RandMax) % (d + Signum(d)));
        }

        public static float RandBetweenF(float minVal, float maxVal)
        {
            if (minVal == maxVal) return minVal;
            return minVal + (((float)Rand.Next(0, RandMax) / (float)RandMax) * (maxVal - minVal));
        }

        /// <summary>
        /// 以给定百分比几率返回 true。
        /// </summary>
        public static bool PercentChance(int percent)
        {
            return Rand.Next(0, RandMax) % 100 < percent;
        }

        public static bool PercentChanceF(float percent)
        {
            return RandBetweenF(0, 100) < percent;
        }
    }
}
