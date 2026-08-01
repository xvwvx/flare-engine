// <自动生成> 对应 C++ 源文件：TooltipManager.h + TooltipManager.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// TooltipManager
    ///
    /// 管理多个 <see cref="WidgetTooltip"/> 实例及其对应的 <see cref="TooltipData"/>、
    /// 屏幕位置与样式，供菜单/地图等模块通过 <see cref="SharedResources.Tooltipm"/> 推送
    /// 并统一渲染工具提示。
    ///
    /// C++ 原始实现使用 <c>std::vector&lt;WidgetTooltip*&gt;</c> 并在析构函数中逐条
    /// <c>delete</c>；C# 版本实现 <see cref="IDisposable"/>，在 <see cref="Dispose"/>
    /// 中按原始析构顺序释放各 <see cref="WidgetTooltip"/> 实例。
    /// </summary>
    public class TooltipManager : IDisposable
    {
        public const int ContextNone = 0;
        public const int ContextMenu = 1;
        public const int ContextMap = 2;

        // C++: uint8_t context;（公有字段，无 getter/setter 包装）
        public byte Context;

        private List<WidgetTooltip?> _tip = [];
        private List<TooltipData> _tipData = [];
        private List<Int2> _pos = [];
        private List<byte> _style = [];

        public TooltipManager()
        {
            var eset = SharedResources.Eset!;

            Context = ContextNone;

            int visibleMax = eset.Tooltips.VisibleMax;
            _tip.Capacity = visibleMax;
            _tipData.Capacity = visibleMax;
            _pos.Capacity = visibleMax;
            _style.Capacity = visibleMax;

            for (int i = 0; i < visibleMax; ++i)
            {
                _tip.Add(new WidgetTooltip());
                if (i > 0)
                {
                    _tip[i]!.Parent = _tip[i - 1];
                }

                _tipData.Add(new TooltipData());
                _pos.Add(new Int2());
                _style.Add(0);
            }
        }

        /// <summary>
        /// 对应 C++ 的 <c>~TooltipManager()</c>：按原始析构函数逐行顺序释放各工具提示控件。
        /// </summary>
        public void Dispose()
        {
            Utils.LogInfo("Cleaning up: TooltipManager");

            for (int i = 0; i < _tip.Count; ++i)
            {
                _tip[i]?.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        public void Clear()
        {
            var eset = SharedResources.Eset!;
            for (int i = 0; i < eset.Tooltips.VisibleMax; ++i)
            {
                _tipData[i].Clear();
            }
        }

        public bool IsEmpty()
        {
            var eset = SharedResources.Eset!;
            for (int i = 0; i < eset.Tooltips.VisibleMax; ++i)
            {
                if (!_tipData[i].IsEmpty())
                    return false;
            }
            return true;
        }

        public void Push(TooltipData tipData, Int2 pos, byte style, int tipIndex = 0)
        {
            if (tipData.IsEmpty() || tipIndex >= SharedResources.Eset!.Tooltips.VisibleMax)
                return;

            // C++: tip_data[tip_index] = _tip_data;（vector 赋值，深拷贝 lines/colors）
            _tipData[tipIndex].Clear();
            _tipData[tipIndex].Lines.AddRange(tipData.Lines);
            _tipData[tipIndex].Colors.AddRange(tipData.Colors);
            _pos[tipIndex] = pos;
            _style[tipIndex] = style;
        }

        public void Render()
        {
            if (!IsEmpty())
            {
                Context = ContextMenu;
            }
            else if (Context != ContextMap)
            {
                Context = ContextNone;
            }

            var eset = SharedResources.Eset!;
            for (int i = 0; i < eset.Tooltips.VisibleMax; ++i)
            {
                _tip[i]!.Render(_tipData[i], _pos[i], _style[i]);
            }
        }
    }
}
