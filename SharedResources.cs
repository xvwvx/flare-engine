// <自动生成> 对应 C++ 源文件：SharedResources.h + SharedResources.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。

namespace FlareEngine
{
    /// <summary>
    /// SharedResources
    ///
    /// 引擎中大多数游戏类都会用到的"全局"系统资源。这些类的实例在整个引擎中只需要一份。
    /// 仅包含通用对象，游戏特定的对象不属于这里。原始版本由 main.cpp 负责创建与销毁。
    ///
    /// C++ 原始实现使用一组 `extern ClassName* name;` 裸指针全局变量，初始值为 NULL，
    /// 在 main.cpp 中 new 出实例、在引擎退出时 delete。C# 版本使用一组可为 null 的静态属性
    /// 表达同样的"全局单例，生命周期由 main 管理"语义（对应阶段 B 规则：extern 全局变量 ->
    /// 静态属性）。各属性在对应的拥有者单元被转换之前，类型为占位引用（详见本报告）。
    /// </summary>
    public static class SharedResources
    {
        public static AnimationManager? Anim { get; set; }
        public static CombatText? Comb { get; set; }
        public static CursorManager? Curs { get; set; }
        public static EngineSettings? Eset { get; set; }
        public static FontEngine? Font { get; set; }
        public static IconManager? Icons { get; set; }
        public static InputState? Inpt { get; set; }
        public static MessageEngine? Msg { get; set; }
        public static ModManager? Mods { get; set; }
        public static RenderDevice? RenderDevice { get; set; }
        public static SaveLoad? SaveLoad { get; set; }
        public static Settings? Settings { get; set; }
        public static SoundManager? Snd { get; set; }
        public static TooltipManager? Tooltipm { get; set; }
    }
}
