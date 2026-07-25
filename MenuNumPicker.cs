// 对应 C++ 源：MenuNumPicker.h + MenuNumPicker.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// MenuNumPicker
    ///
    /// 数量选择对话框：文本输入框配合增减按钮，支持长按加速与边界环绕。
    /// 持有的 <see cref="WidgetButton"/>、<see cref="WidgetInput"/> 通过 <see cref="IDisposable"/> 显式释放；
    /// 嵌入的 <see cref="WidgetLabel"/> 在 <see cref="Dispose"/> 中释放 Sprite 缓存。
    /// 释放顺序与原始析构函数 <c>~MenuNumPicker()</c> 一致
    /// （先四个按钮与输入框，再 <see cref="_label"/>，最后基类 <see cref="Menu"/>）。
    /// </summary>
    public class MenuNumPicker : Menu, IDisposable
    {
        private WidgetButton? _buttonOk;
        private WidgetButton? _buttonUp;
        private WidgetButton? _buttonDown;
        private WidgetButton? _buttonClose;
        private WidgetInput? _inputBox;
        private WidgetLabel _label;

        private int _value;
        private int _valueMin;
        private int _valueMax;

        private int _spinTicks;
        private int _spinIncrement;
        private int _spinDelay;

        private bool _canWrap;

        /// <summary>对应 C++ 原始公有字段 <c>confirm_clicked</c>。</summary>
        public bool ConfirmClicked;

        /// <summary>对应 C++ 原始公有字段 <c>cancel_clicked</c>。</summary>
        public bool CancelClicked;

        public MenuNumPicker()
        {
            _value = 0;
            _valueMin = 0;
            _valueMax = int.MaxValue;
            _spinTicks = 0;
            _spinIncrement = 1;
            _spinDelay = SharedResources.Settings!.MaxFramesPerSec / 6;
            ConfirmClicked = false;
            CancelClicked = false;

            _buttonOk = new WidgetButton(WidgetButton.DefaultFile);
            _buttonOk.SetLabel(SharedResources.Msg!.Get("OK"));

            _buttonUp = new WidgetButton(WidgetButton.DirUpFile);
            _buttonDown = new WidgetButton(WidgetButton.DirDownFile);

            _buttonClose = new WidgetButton(WidgetButton.CloseFile);

            _inputBox = new WidgetInput(WidgetInput.DefaultFile);
            _inputBox.OnlyNumbers = true;

            _label = new WidgetLabel();
            _label.SetText(SharedResources.Msg.Get("Enter amount:"));
            _label.SetColor(SharedResources.Font!.GetColor(FontEngine.ColorMenuNormal));

            // Load config settings
            using FileParser infile = new FileParser();
            // @CLASS MenuNumPicker|Description of menus/num_picker.txt
            if (infile.Open("menus/num_picker.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (ParseMenuKey(infile.Key, infile.Val))
                        continue;
                    else if (infile.Key == "label_title")
                    {
                        // @ATTR label_title|label|Position of the "Enter amount:" text.
                        _label.SetFromLabelInfo(Parse.PopLabelInfo(infile.Val));
                    }
                    else if (infile.Key == "confirm")
                    {
                        // @ATTR confirm|point|Position of the "OK" button.
                        Int2 pos = Parse.ToPoint(infile.Val);
                        _buttonOk.SetBasePos(pos.X, pos.Y, Utils.AlignTopLeft);
                    }
                    else if (infile.Key == "increase")
                    {
                        // @ATTR increase|point|Position of the button used to increase the value.
                        Int2 pos = Parse.ToPoint(infile.Val);
                        _buttonUp.SetBasePos(pos.X, pos.Y, Utils.AlignTopLeft);
                    }
                    else if (infile.Key == "decrease")
                    {
                        // @ATTR decrease|point|Position of the button used to decrease the value.
                        Int2 pos = Parse.ToPoint(infile.Val);
                        _buttonDown.SetBasePos(pos.X, pos.Y, Utils.AlignTopLeft);
                    }
                    else if (infile.Key == "close")
                    {
                        // @ATTR close|point|Position of the button used to close the number picker window.
                        Int2 pos = Parse.ToPoint(infile.Val);
                        _buttonClose.SetBasePos(pos.X, pos.Y, Utils.AlignTopLeft);
                    }
                    else if (infile.Key == "input")
                    {
                        // @ATTR input|point|Position of the text input box.
                        Int2 pos = Parse.ToPoint(infile.Val);
                        _inputBox.SetBasePos(pos.X, pos.Y, Utils.AlignTopLeft);
                    }
                    else
                        infile.Error("MenuNumPicker: '{0}' is not a valid key.", infile.Key);
                }
                infile.Close();
            }

            if (_background == null)
                SetBackground("images/menus/num_picker_bg.png");

            Tablist.Add(_buttonOk);
            Tablist.Add(_buttonUp);
            Tablist.Add(_buttonDown);

            Align();
        }

        /// <summary>
        /// 对应 C++ 的 <c>~MenuNumPicker()</c>：先释放四个按钮与输入框，
        /// 再释放嵌入 label，最后调用基类 <see cref="Menu.Dispose"/>。
        /// </summary>
        public override void Dispose()
        {
            _buttonOk?.Dispose();
            _buttonOk = null;

            _buttonUp?.Dispose();
            _buttonUp = null;

            _buttonDown?.Dispose();
            _buttonDown = null;

            _inputBox?.Dispose();
            _inputBox = null;

            _buttonClose?.Dispose();
            _buttonClose = null;

            _label.Dispose();

            base.Dispose();
        }

        public override void Align()
        {
            base.Align();

            _buttonOk!.SetPos(WindowArea.X, WindowArea.Y);
            _buttonUp!.SetPos(WindowArea.X, WindowArea.Y);
            _buttonDown!.SetPos(WindowArea.X, WindowArea.Y);
            _buttonClose!.SetPos(WindowArea.X, WindowArea.Y);

            _inputBox!.SetPos(WindowArea.X, WindowArea.Y);

            _label.SetPos(WindowArea.X, WindowArea.Y);

            UpdateInput();
        }

        public void Logic()
        {
            if (Visible)
            {
                if (!_inputBox!.EditMode && _spinTicks == 0)
                {
                    Tablist.Logic();
                }

                _inputBox.Logic();

                if (SharedResources.Inpt!.Pressing[Input.Cancel] && !SharedResources.Inpt.Lock[Input.Cancel])
                {
                    SharedResources.Inpt.Lock[Input.Cancel] = true;
                    CancelClicked = true;
                }
                else if (_buttonClose!.CheckClick())
                {
                    CancelClicked = true;
                }
                else if (_buttonOk!.CheckClick())
                {
                    ConfirmClicked = true;
                }
                else if (_buttonUp!.CheckClick() && _spinTicks < _spinDelay)
                {
                    _canWrap = (_value == _valueMax);
                    IncreaseValue(1);
                }
                else if (_buttonDown!.CheckClick() && _spinTicks < _spinDelay)
                {
                    _canWrap = (_value == _valueMin);
                    DecreaseValue(1);
                }
                else
                {
                    if (_buttonUp.Pressed || _buttonDown.Pressed)
                    {
                        _spinTicks++;
                    }
                    else
                    {
                        _spinTicks = 0;
                        _spinIncrement = 1;
                        _canWrap = (_value == _valueMax || _value == _valueMin);
                    }

                    if (_spinTicks > 0 && _spinTicks % _spinDelay == 0)
                    {
                        for (int i = 1; i <= 6; ++i)
                        {
                            if (_spinTicks % (_spinDelay * 10 * i) == 0)
                                _spinIncrement = (int)MathF.Pow(10, i);
                        }
                        if (_buttonUp.Pressed)
                        {
                            IncreaseValue(_spinIncrement);
                        }
                        else if (_buttonDown.Pressed)
                        {
                            DecreaseValue(_spinIncrement);
                        }
                    }
                }

                if (ConfirmClicked)
                {
                    SetValue(Parse.ToInt(_inputBox.GetText()));
                    UpdateInput();
                }

                // cancel_clicked is handled in MenuManager; need to stay visible
            }
        }

        public override void Render()
        {
            if (Visible)
            {
                // background
                base.Render();

                _label.Render();

                _buttonOk!.Render();
                _buttonUp!.Render();
                _buttonDown!.Render();
                _buttonClose!.Render();
                _inputBox!.Render();
            }
        }

        public void SetValueBounds(int low, int high)
        {
            _valueMin = low;
            _valueMax = high;
            _value = _valueMin;
            UpdateInput();
        }

        public void SetValue(int val)
        {
            _value = Math.Min(Math.Max(val, _valueMin), _valueMax);
            UpdateInput();
        }

        public void IncreaseValue(int val)
        {
            SetValue(Parse.ToInt(_inputBox!.GetText()));

            _value += val;

            if (_value >= _valueMax)
            {
                if (_canWrap)
                {
                    _value = _valueMin;
                }
                else
                {
                    _value = _valueMax;
                }
            }
            else if (_value <= _valueMin)
            {
                _value = _valueMin;
            }

            _canWrap = false;

            UpdateInput();
        }

        public void DecreaseValue(int val)
        {
            SetValue(Parse.ToInt(_inputBox!.GetText()));

            _value -= val;

            if (_value <= _valueMin)
            {
                if (_canWrap)
                {
                    _value = _valueMax;
                }
                else
                {
                    _value = _valueMin;
                }
            }
            else if (_value >= _valueMax)
            {
                _value = _valueMax;
            }

            _canWrap = false;

            UpdateInput();
        }

        public int GetValue()
        {
            return _value;
        }

        protected void UpdateInput()
        {
            if (_inputBox == null) return;

            _inputBox.SetText(_value.ToString());
        }

        public void CloseWindow()
        {
            ConfirmClicked = false;
            CancelClicked = false;
            Visible = false;
            Tablist.Defocus();
        }
    }
}
