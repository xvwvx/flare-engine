// 对应 C++ 源文件：MenuGameOver.h + MenuGameOver.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// MenuGameOver
    ///
    /// 玩家死亡后显示的“Game Over”菜单，含 Continue 与 Exit（或 Save &amp; Exit）按钮。
    /// 持有的 <see cref="WidgetButton"/> 通过 <see cref="IDisposable"/> 显式释放；
    /// 嵌入的 <see cref="WidgetLabel"/> 在 <see cref="Dispose"/> 中释放 Sprite 缓存。
    /// 释放顺序与原始析构函数 <c>~MenuGameOver()</c> 一致
    /// （先 <see cref="_buttonContinue"/>、<see cref="_buttonExit"/>，再 <see cref="_label"/>，最后基类 <see cref="Menu"/>）。
    /// </summary>
    public class MenuGameOver : Menu, IDisposable
    {
        private WidgetButton? _buttonContinue;
        private WidgetButton? _buttonExit;
        private readonly WidgetLabel _label = new WidgetLabel();

        /// <summary>对应 C++ 原始公有字段 <c>continue_clicked</c>。</summary>
        public bool ContinueClicked;

        /// <summary>对应 C++ 原始公有字段 <c>exit_clicked</c>。</summary>
        public bool ExitClicked;

        public MenuGameOver()
        {
            _buttonContinue = new WidgetButton(WidgetButton.DefaultFile);
            _buttonExit = new WidgetButton(WidgetButton.DefaultFile);
            ContinueClicked = false;
            ExitClicked = false;

            // default layout
            WindowArea = new Rectangle(0, 0, 192, 100);
            Alignment = Utils.AlignCenter;
            _buttonContinue.SetBasePos(WindowArea.Width / 2 - _buttonContinue.Pos.Width / 2, WindowArea.Height / 2 - _buttonContinue.Pos.Height / 2, Utils.AlignTopLeft);
            _buttonExit.SetBasePos(WindowArea.Width / 2 - _buttonExit.Pos.Width / 2, WindowArea.Height / 2 + _buttonExit.Pos.Height / 2, Utils.AlignTopLeft);
            _label.SetJustify(FontEngine.JustifyCenter);
            _label.SetBasePos(WindowArea.Width / 2, 8, Utils.AlignTopLeft);

            // Load config settings
            using FileParser infile = new FileParser();
            // @CLASS MenuGameOver|Description of menus/game_over.txt
            if (infile.Open("menus/game_over.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (ParseMenuKey(infile.Key, infile.Val))
                        continue;
                    else if (infile.Key == "label_title")
                    {
                        // @ATTR label_title|label|Position of the "Game Over" text.
                        _label.SetFromLabelInfo(Parse.PopLabelInfo(infile.Val));
                    }
                    else if (infile.Key == "button_continue")
                    {
                        // @ATTR button_continue|point|Position of the "Continue" button.
                        Int2 pos = Parse.ToPoint(infile.Val);
                        _buttonContinue.SetBasePos(pos.X, pos.Y, Utils.AlignTopLeft);
                    }
                    else if (infile.Key == "button_exit")
                    {
                        // @ATTR button_exit|point|Position of the "Exit" button.
                        Int2 pos = Parse.ToPoint(infile.Val);
                        _buttonExit.SetBasePos(pos.X, pos.Y, Utils.AlignTopLeft);
                    }
                }
                infile.Close();
            }

            var msg = SharedResources.Msg!;
            var font = SharedResources.Font!;
            var eset = SharedResources.Eset!;

            _label.SetText(msg.Get("Game Over"));
            _label.SetColor(font.GetColor(FontEngine.ColorMenuNormal));

            _buttonContinue.SetLabel(msg.Get("Continue"));
            if (eset.Misc.SaveOnexit)
                _buttonExit.SetLabel(msg.Get("Save & Exit"));
            else
                _buttonExit.SetLabel(msg.Get("Exit"));

            Tablist.Add(_buttonContinue);
            Tablist.Add(_buttonExit);

            if (_background == null)
                SetBackground("images/menus/game_over.png");

            Align();

            Visible = false;
        }

        /// <summary>
        /// 对应 C++ 的 <c>~MenuGameOver()</c>：先释放两个按钮，再释放嵌入 label，最后调用基类 <see cref="Menu.Dispose"/>。
        /// </summary>
        public override void Dispose()
        {
            _buttonContinue?.Dispose();
            _buttonContinue = null;

            _buttonExit?.Dispose();
            _buttonExit = null;

            _label.Dispose();

            base.Dispose();
        }

        public override void Align()
        {
            base.Align();

            _buttonContinue!.SetPos(WindowArea.X, WindowArea.Y);
            _buttonExit!.SetPos(WindowArea.X, WindowArea.Y);
            _label.SetPos(WindowArea.X, WindowArea.Y);
        }

        public void Logic()
        {
            if (!Visible)
                return;

            Tablist.Logic();

            if (_buttonContinue!.CheckClick())
            {
                ContinueClicked = true;
            }
            else if (_buttonExit!.CheckClick())
            {
                ExitClicked = true;
            }
        }

        public void Close()
        {
            Visible = false;
            ContinueClicked = false;
            ExitClicked = false;
            Tablist.Defocus();
        }

        public void DisableSave()
        {
            _buttonContinue!.Enabled = false;
            _buttonExit!.SetLabel(SharedResources.Msg!.Get("Exit"));
        }

        public override void Render()
        {
            if (!Visible)
                return;

            // background
            base.Render();

            _label.Render();

            _buttonContinue!.Render();
            _buttonExit!.Render();
        }
    }
}
