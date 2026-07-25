// <自动生成> 对应 C++ 源文件：MenuConfirm.h + MenuConfirm.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// MenuConfirm
    ///
    /// 通用确认对话框菜单：显示标题文本、水平动作列表与关闭按钮。
    /// 持有的 <see cref="WidgetButton"/>、<see cref="WidgetHorizontalList"/> 等资源
    /// 通过 <see cref="IDisposable"/> 显式释放，释放顺序与原始析构函数 <c>~MenuConfirm()</c> 一致
    /// （先 <see cref="ActionList"/>、<see cref="_buttonClose"/>，再 <see cref="_label"/>，最后基类 <see cref="Menu"/>）。
    ///
    /// <see cref="WidgetButton"/>、<see cref="WidgetHorizontalList"/> 为尚未转换的前向依赖单元，
    /// 本类按 <see cref="MenuConfig"/> 及头文件预期的最终符号命名引用其 API（详见转换报告）。
    /// </summary>
    public class MenuConfirm : Menu, IDisposable
    {
        private WidgetButton? _buttonClose;
        private WidgetLabel _label;
        private string _title;

        /// <summary>对应 C++ 原始公有字段 <c>action_list</c>。</summary>
        public WidgetHorizontalList? ActionList;

        /// <summary>对应 C++ 原始公有字段 <c>clicked_confirm</c>。</summary>
        public bool ClickedConfirm;

        /// <summary>对应 C++ 原始公有字段 <c>clicked_cancel</c>。</summary>
        public bool ClickedCancel;

        public MenuConfirm()
        {
            _buttonClose = new WidgetButton(WidgetButton.CloseFile);
            ActionList = new WidgetHorizontalList();
            ClickedConfirm = false;
            ClickedCancel = false;
            _title = "";
            _label = new WidgetLabel();

            _label.SetColor(SharedResources.Font!.GetColor(FontEngine.ColorMenuNormal));

            ActionList!.HasAction = true;
            ActionList.Refresh();

            // Load config settings
            using FileParser infile = new FileParser();
            // @CLASS MenuConfirm|Description of menus/confirm.txt
            if (infile.Open("menus/confirm.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (ParseMenuKey(infile.Key, infile.Val))
                    {
                        // default positions based on window_area
                        if (infile.Key == "pos")
                        {
                            int borderSize = 6;

                            _buttonClose!.SetBasePos(WindowArea.Width, 0, Utils.AlignTopLeft);
                            ActionList!.SetBasePos(WindowArea.Width / 2 - ActionList.Pos.Width / 2, WindowArea.Height - ActionList.Pos.Height - borderSize, Utils.AlignTopLeft);

                            LabelInfo labelInfo = new LabelInfo();
                            labelInfo.X = WindowArea.Width / 2;
                            labelInfo.Y = borderSize;
                            labelInfo.Justify = FontEngine.JustifyCenter;
                            _label.SetFromLabelInfo(labelInfo);
                        }
                        continue;
                    }

                    if (infile.Key == "close")
                    {
                        // @ATTR close|point|Position of the close button.
                        Int2 pos = Parse.ToPoint(infile.Val);
                        _buttonClose!.SetBasePos(pos.X, pos.Y, Utils.AlignTopLeft);
                    }
                    else if (infile.Key == "label_title")
                    {
                        // @ATTR label_title|label|Position of the title text.
                        _label.SetFromLabelInfo(Parse.PopLabelInfo(infile.Val));
                    }
                    else if (infile.Key == "action_list")
                    {
                        // @ATTR action_list|point|Position of the action selector widget.
                        Int2 pos = Parse.ToPoint(infile.Val);
                        ActionList!.SetBasePos(pos.X, pos.Y, Utils.AlignTopLeft);
                    }
                }
                infile.Close();
            }

            Tablist.Add(ActionList);

            if (_background == null)
                SetBackground("images/menus/confirm_bg.png");

            Align();
        }

        /// <summary>
        /// 对应 C++ 的 <c>~MenuConfirm()</c>：先释放 action_list、button_close，
        /// 再释放嵌入的 label，最后调用基类 <see cref="Menu.Dispose"/>。
        /// </summary>
        public override void Dispose()
        {
            ActionList?.Dispose();
            ActionList = null;

            _buttonClose?.Dispose();
            _buttonClose = null;

            _label.Dispose();

            base.Dispose();
        }

        public override void Align()
        {
            base.Align();

            _label.SetText(_title);

            ActionList!.SetPos(WindowArea.X, WindowArea.Y);
            ActionList.Refresh();
            _label.SetPos(WindowArea.X, WindowArea.Y);

            _buttonClose!.SetPos(WindowArea.X, WindowArea.Y);
        }

        public void Logic()
        {
            if (Visible && ActionList!.Enabled)
            {
                Tablist.Logic();

                if (!SharedResources.Inpt!.UsingMouse() && Tablist.GetCurrent() == -1)
                {
                    Tablist.GetNext(!TabList.GetInner, TabList.WidgetSelectAuto);
                }
                else if (SharedResources.Inpt.UsingMouse())
                {
                    Tablist.Defocus();
                }

                ClickedConfirm = false;

                if (ActionList.CheckClick() && ActionList.CheckAction())
                {
                    ClickedConfirm = true;
                }
                else if (_buttonClose!.CheckClick() || (SharedResources.Inpt.Pressing[Input.Cancel] && !SharedResources.Inpt.Lock[Input.Cancel]))
                {
                    if (SharedResources.Inpt.Pressing[Input.Cancel])
                        SharedResources.Inpt.Lock[Input.Cancel] = true;

                    Visible = false;
                    ClickedConfirm = false;
                    ClickedCancel = true;
                }
            }
        }

        public override void Render()
        {
            if (!Visible)
                return;

            // background
            base.Render();

            _label.Render();

            ActionList!.Render();
            _buttonClose!.Render();
        }

        public void SetTitle(string s)
        {
            _title = s;
            Align();
        }

        public void Show()
        {
            Tablist.Defocus();
            Visible = true;
            ClickedConfirm = false;
            ClickedCancel = false;
            ActionList!.Select(0);
            ActionList.Enabled = true;
            Align();
        }
    }
}
