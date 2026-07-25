// <自动生成> 对应 C++ 源文件：TooltipData.h + TooltipData.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// TooltipData 保存一条工具提示的文本行及其行颜色。
    /// 将数据与控件本身分离，便于在模块间传递。
    /// </summary>
    public class TooltipData
    {
        public const int StyleFloat = 0;
        public const int StyleTopLabel = 1;
        public const int StyleAbsolute = 2;

        public List<string> Lines = [];
        public List<Color> Colors = [];

        public TooltipData()
        {
        }

        // 添加支持换行的彩色文本
        public void AddColoredText(string text, Color color)
        {
            if (Lines.Count == 0 || Lines[^1] != "")
            {
                Lines.Add("");
                Colors.Add(color);
            }

            int cur = 0;
            while (cur < text.Length)
            {
                if (text[cur] == '\n')
                {
                    if (Lines[^1] == "")
                        Lines[^1] = " ";

                    Lines.Add("");
                    Colors.Add(color);
                }
                else
                {
                    Lines[^1] += text[cur];
                }

                cur++;
            }

            if (Lines[^1] == "")
                Lines[^1] = " ";
        }

        public void AddText(string text)
        {
            AddColoredText(text, SharedResources.Font!.GetColor(FontEngine.ColorWidgetNormal));
        }

        public void Clear()
        {
            Lines.Clear();
            Colors.Clear();
        }

        public bool IsEmpty()
        {
            return Lines.Count == 0;
        }

        // 比较第一行
        public bool CompareFirstLine(string text)
        {
            if (Lines.Count == 0) return false;
            if (Lines[0] != text) return false;
            return true;
        }

        // 比较所有行
        public bool Compare(TooltipData tip)
        {
            if (Lines.Count != tip.Lines.Count)
                return false;

            for (int i = 0; i < Lines.Count; ++i)
            {
                if (Lines[i] != tip.Lines[i] || Colors[i] != tip.Colors[i])
                    return false;
            }

            return true;
        }
    }
}
