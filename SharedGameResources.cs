// <自动生成> 对应 C++ 源文件：SharedGameResources.h + SharedGameResources.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。

namespace FlareEngine
{
    /// <summary>
    /// SharedGameResources
    ///
    /// 与 SharedResources 平级的另一组"全局"系统资源，专门存放"游戏特定"（而非通用引擎）
    /// 对象。这些对象在 GameStatePlay 构造函数中创建、在 GameStatePlay 析构函数中删除，
    /// 因此在这段生命周期之间可以在引擎任意位置安全访问；这些对象本身不允许被其所有者
    /// 以外的任何类修改。
    ///
    /// C++ 原始实现使用一组 `extern ClassName* name;` 裸指针全局变量，在 SharedGameResources.cpp
    /// 中定义并初始化为 NULL（真正的 new/delete 由 GameStatePlay 负责）。C# 版本使用一组可为
    /// null 的静态属性表达同样的"全局单例，生命周期由 GameStatePlay 管理"语义（对应阶段 B
    /// 规则：extern 全局变量 -> 静态属性）。各属性引用的类型在其对应的拥有者单元被转换之前，
    /// 均为前向引用（详见本单元报告）。
    /// </summary>
    public static class SharedGameResources
    {
        public static Avatar? Pc { get; set; }
        public static CampaignManager? Camp { get; set; }
        public static EnemyGroupManager? Enemyg { get; set; }
        public static EntityManager? Entitym { get; set; }
        public static EventManager? Eventm { get; set; }
        public static HazardManager? Hazards { get; set; }
        public static ItemManager? Items { get; set; }
        public static LootManager? Loot { get; set; }
        public static MapRenderer? Mapr { get; set; }
        public static MenuActionBar? MenuAct { get; set; }
        public static MenuManager? Menu { get; set; }
        public static MenuPowers? MenuPowers { get; set; }
        public static NPCManager? Npcs { get; set; }
        public static PowerManager? Powers { get; set; }
        public static FogOfWar? Fow { get; set; }
        public static XPScaling? XpScaling { get; set; }
    }
}
