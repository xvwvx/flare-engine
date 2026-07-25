// <自动生成> 对应 C++ 源文件：CommonIncludes.h
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。

namespace FlareEngine
{
    // CommonIncludes.h 在 C++ 中仅用于集中包含标准库头文件（<algorithm>/<map>/<vector> 等）
    // 以及为 Image/Sprite/Renderable 提供前向声明，避免其他头文件产生循环 #include。
    //
    // 在 C# 中：
    // 1) 标准库类型（List<T>/Dictionary<K,V>/Queue<T> 等）来自已全局导入的 System.Collections.Generic，无需在此重复声明。
    // 2) C# 编译单元不要求前向声明（同一命名空间下的类型可以任意顺序互相引用），
    //    因此 Image/Sprite/Renderable 的前向声明没有对应的运行时逻辑需要迁移。
    // 本文件不产生任何类型定义，仅作为该逻辑单元到 C# 的占位映射说明。
}
