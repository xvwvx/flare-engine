// <自动生成> 对应 C++ 源文件：WidgetInput.h + WidgetInput.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// WidgetInput
    ///
    /// 带标签的简单文本框，具有聚焦/未聚焦两种背景图像状态。
    /// 持有的 <see cref="Sprite"/> 与屏幕键盘 <see cref="WidgetTooltip"/> 通过
    /// <see cref="IDisposable"/> 显式释放，对应原始析构函数中的 delete background 与 snd->unload。
    /// </summary>
    public class WidgetInput : Widget, IDisposable
    {
        public const string DefaultFile = "images/menus/input.png";
        public const string NoFile = "_NO_FILE_";

        public bool EditMode;
        public uint MaxLength;
        public bool OnlyNumbers;
        public bool AcceptToDefocus;

        protected bool Enabled;
        protected bool Pressed;

        private Sprite? _background;
        private string _text = "";
        private string _trimmedText = "";
        private string _trimmedTextCursor = "";
        private int _cursorPos;
        private Int2 _fontPos;
        private TooltipData _oskBuf = new TooltipData();
        private WidgetTooltip _oskTip = new WidgetTooltip();
        private string _fontName;
        private SoundID _soundActivate;

        public WidgetInput(string filename)
        {
            _background = null;
            Enabled = true;
            Pressed = false;
            _cursorPos = 0;
            _fontName = "font_regular";
            _soundActivate = 0;
            EditMode = false;
            MaxLength = 0;
            OnlyNumbers = false;
            AcceptToDefocus = true;

            LoadGraphics(filename);

            if (SharedResources.Eset!.Widgets.SoundActivate.Length != 0)
                _soundActivate = SharedResources.Snd!.Load(SharedResources.Eset.Widgets.SoundActivate, "Widget activate");
        }

        /// <summary>
        /// 对应 C++ 的 <c>~WidgetInput()</c>，释放缓存的 Sprite 并卸载音效，
        /// 随后释放成员 <see cref="WidgetTooltip"/>（对应 C++ 成员析构顺序）。
        /// </summary>
        public void Dispose()
        {
            _background?.Dispose();
            _background = null;
            SharedResources.Snd!.Unload(_soundActivate);
            _oskTip.Dispose();
            GC.SuppressFinalize(this);
        }

        public override void SetPos(int offsetX, int offsetY)
        {
            Pos.X = PosBase.X + offsetX + LocalFrame.X - LocalOffset.X;
            Pos.Y = PosBase.Y + offsetY + LocalFrame.Y - LocalOffset.Y;
            Utils.AlignToScreenEdge(Alignment, ref Pos);

            SharedResources.Font!.SetFont(_fontName);
            _fontPos.X = Pos.X + (SharedResources.Font.GetFontHeight() / 2);
            _fontPos.Y = Pos.Y + (Pos.Height / 2) - (SharedResources.Font.GetFontHeight() / 2);
        }

        protected void LoadGraphics(string filename)
        {
            if (filename == NoFile)
                return;

            // load input background image
            Image? graphics = null;
            if (filename != DefaultFile)
            {
                graphics = SharedResources.RenderDevice!.LoadImage(filename, RenderDevice.ErrorNormal);
            }
            if (graphics == null)
            {
                graphics = SharedResources.RenderDevice!.LoadImage(DefaultFile, RenderDevice.ErrorExit);
            }
            if (graphics != null)
            {
                _background = graphics.CreateSprite();
                Pos.Width = _background.GetGraphicsWidth();
                Pos.Height = _background.GetGraphicsHeight() / 2;
                graphics.Unref();
            }
        }

        protected void TrimText()
        {
            string textWithCursor = _text;
            textWithCursor = textWithCursor.Insert(_cursorPos, "|");

            int padding = SharedResources.Font!.GetFontHeight();
            _trimmedText = SharedResources.Font.TrimTextToWidth(_text, Pos.Width - padding, !FontEngine.UseEllipsis, _text.Length);

            int trimPos = (_cursorPos > 0 ? _cursorPos - 1 : _cursorPos);
            _trimmedTextCursor = SharedResources.Font.TrimTextToWidth(textWithCursor, Pos.Width - padding, !FontEngine.UseEllipsis, trimPos);
        }

        public override void Activate()
        {
            if (!EditMode)
                EditMode = true;
        }

        protected bool CheckClick(Int2 mouse)
        {
            EnableTablistNav = Enabled;

            // disabled buttons can't be clicked;
            if (!Enabled) return false;

            InputState inpt = SharedResources.Inpt!;

            // main button already in use, new click not allowed
            if (inpt.Lock[Input.Main1]) return false;

            // main click released, so the button state goes back to unpressed
            if (Pressed && !inpt.Lock[Input.Main1])
            {
                Pressed = false;

                if (Utils.IsWithinRect(Pos, mouse))
                {
                    SharedResources.Snd!.Play(_soundActivate, "widget_activate", SoundManager.NoPos, !SoundManager.Loop);

                    // activate upon release
                    return true;
                }
            }

            Pressed = false;

            // detect new click
            if (inpt.Pressing[Input.Main1])
            {
                if (Utils.IsWithinRect(Pos, mouse))
                {

                    inpt.Lock[Input.Main1] = true;
                    Pressed = true;

                }
            }
            return false;
        }

        public void Logic()
        {
            InputState inpt = SharedResources.Inpt!;
            if (LogicAt(inpt.Mouse.X, inpt.Mouse.Y))
                return;
        }

        public bool LogicAt(int x, int y)
        {
            Int2 mouse = new Int2(x, y);

            if (CheckClick(mouse))
            {
                EditMode = true;
                if (Platform.Instance.IsMobileDevice)
                {
                    // Not sure if this is SDL's fault or Android's fault...
                    // But the on screen keyboard only triggers the backspace input on text that was entered with the on screen keyboard
                    // So we have to delete the existing text here.
                    SetText("");
                }
            }

            // if clicking elsewhere unfocus the text box
            InputState inpt = SharedResources.Inpt!;
            if (inpt.Pressing[Input.Main1])
            {
                if (!Utils.IsWithinRect(Pos, mouse))
                {
                    EditMode = false;
                }
            }

            if (EditMode)
            {
                inpt.SlowRepeat[Input.Del] = true;
                inpt.SlowRepeat[Input.Left] = true;
                inpt.SlowRepeat[Input.Right] = true;
                inpt.StartTextInput();

                if (inpt.Inkeys != "")
                {
                    // handle text input
                    // only_numbers will restrict our input to 0-9 characters
                    if (!OnlyNumbers || (inpt.Inkeys[0] >= 48 && inpt.Inkeys[0] <= 57))
                    {
                        _text = _text.Insert(_cursorPos, inpt.Inkeys);
                        _cursorPos += inpt.Inkeys.Length;
                        TrimText();
                    }

                    // HACK: this prevents normal keys from triggering common menu shortcuts
                    for (int i = 0; i < InputState.KeyCount; ++i)
                    {
                        if (inpt.Pressing[i])
                        {
                            inpt.Lock[i] = true;
                            inpt.RepeatCooldown[i].Current = inpt.RepeatCooldown[i].Duration - 1;
                        }
                    }
                }

                // handle backspaces
                if (inpt.Pressing[Input.Del] && inpt.RepeatCooldown[Input.Del].IsBegin())
                {
                    if (_text.Length != 0 && _cursorPos > 0)
                    {
                        // remove utf-8 character
                        // size_t old_cursor_pos = cursor_pos;
                        int n = _cursorPos - 1;
                        while (n > 0 && char.IsLowSurrogate(_text[n]))
                        {
                            n--;
                        }
                        _text = _text.Substring(0, n) + _text.Substring(_cursorPos);
                        _cursorPos -= _cursorPos - n;
                        TrimText();
                    }
                }

                // cursor movement
                if (_text.Length != 0 && _cursorPos > 0 && inpt.Pressing[Input.Left] && inpt.RepeatCooldown[Input.Left].IsBegin())
                {
                    _cursorPos--;
                    TrimText();
                }
                else if (_text.Length != 0 && _cursorPos < _text.Length && inpt.Pressing[Input.Right] && inpt.RepeatCooldown[Input.Right].IsBegin())
                {
                    inpt.Lock[Input.Right] = true;
                    _cursorPos++;
                    TrimText();
                }

                // defocus with Enter or Escape
                if (AcceptToDefocus && inpt.Pressing[Input.Accept] && !inpt.Lock[Input.Accept])
                {
                    inpt.Lock[Input.Accept] = true;
                    EditMode = false;
                }
                else if (inpt.Pressing[Input.Cancel] && !inpt.Lock[Input.Cancel])
                {
                    inpt.Lock[Input.Cancel] = true;
                    EditMode = false;
                }
            }
            else
            {
                inpt.SlowRepeat[Input.Del] = false;
                inpt.SlowRepeat[Input.Left] = false;
                inpt.SlowRepeat[Input.Right] = false;
                inpt.StopTextInput();
            }

            return true;
        }

        public override void Render()
        {
            Rectangle src = default;
            src.X = 0;
            src.Y = (EditMode ? Pos.Height : 0);
            src.Width = Pos.Width;
            src.Height = Pos.Height;

            if (_background != null)
            {
                _background.LocalFrame = LocalFrame;
                _background.SetOffset(LocalOffset);
                _background.SetClipFromRect(src);
                _background.SetDestFromRect(Pos);
                SharedResources.RenderDevice!.Render(_background);
            }

            SharedResources.Font!.SetFont(_fontName);

            if (!EditMode)
            {
                SharedResources.Font.Render(_trimmedText, _fontPos.X, _fontPos.Y, FontEngine.JustifyLeft, null, 0, SharedResources.Font.GetColor(FontEngine.ColorWidgetNormal), !FontEngine.ShadowOffset);
            }
            else
            {
                SharedResources.Font.RenderShadowed(_trimmedTextCursor, _fontPos.X, _fontPos.Y, FontEngine.JustifyLeft, null, 0, SharedResources.Font.GetColor(FontEngine.ColorWidgetNormal));
            }

            if (InFocus && !EditMode)
            {
                Int2 topLeft = default;
                Int2 bottomRight = default;

                topLeft.X = Pos.X + LocalFrame.X - LocalOffset.X;
                topLeft.Y = Pos.Y + LocalFrame.Y - LocalOffset.Y;
                bottomRight.X = topLeft.X + Pos.Width;
                bottomRight.Y = topLeft.Y + Pos.Height;

                // Only draw rectangle if it fits in local frame
                bool draw = true;
                if (LocalFrame.Width != 0 &&
                        (topLeft.X < LocalFrame.X || bottomRight.X > (LocalFrame.X + LocalFrame.Width)))
                {
                    draw = false;
                }
                if (LocalFrame.Height != 0 &&
                        (topLeft.Y < LocalFrame.Y || bottomRight.Y > (LocalFrame.Y + LocalFrame.Height)))
                {
                    draw = false;
                }
                if (draw)
                {
                    SharedResources.RenderDevice!.DrawRectangleCorners(SharedResources.Eset!.Widgets.SelectionRectCornerSize, topLeft, bottomRight, SharedResources.Eset.Widgets.SelectionRectColor);
                }
            }

            // handle on-screen keyboard
            if (Platform.Instance.IsMobileDevice && EditMode)
            {
                _oskBuf.Clear();
                _oskBuf.AddText(_trimmedTextCursor);
                _oskTip.Render(_oskBuf, new Int2(SharedResources.Settings!.ViewWHalf + Pos.Width / 2, 0), TooltipData.StyleFloat);
            }
        }

        public string GetText()
        {
            return _text;
        }

        public void SetText(string text)
        {
            _text = text;
            _cursorPos = _text.Length;
            TrimText();
        }

        public void Resize(int newWidth)
        {
            if (_background != null && newWidth != _background.GetGraphicsWidth())
            {
                _background.Dispose();
                _background = null;
            }

            if (_background == null)
            {
                int lineHeight = SharedResources.Font!.GetFontHeight();
                Pos.Width = newWidth;
                Pos.Height = (int)(lineHeight * 1.5);

                int gfxH = Pos.Height * 2;

                Image? temp = SharedResources.RenderDevice!.CreateImage(Pos.Width, gfxH);
                if (temp != null)
                {
                    Color colorInactive = SharedResources.Font.GetColor(FontEngine.ColorWidgetDisabled);
                    Color colorActive = SharedResources.Font.GetColor(FontEngine.ColorWidgetNormal);

                    int pad = (int)((float)lineHeight * 0.15f);

                    // temp->drawLine(pad, pad, pos.w - pad, pad, color_inactive);
                    // temp->drawLine(pad, pad, pad, pos.h - pad, color_inactive);
                    temp.DrawLine(pad, Pos.Height - pad, Pos.Width - pad, Pos.Height - pad, colorInactive);
                    // temp->drawLine(pos.w - pad, pad, pos.w - pad, pos.h - pad, color_inactive);

                    // temp->drawLine(pad, pad + pos.h, pos.w - pad, pad + pos.h, color_active);
                    // temp->drawLine(pad, pad + pos.h, pad, pos.h - pad + pos.h, color_active);
                    temp.DrawLine(pad, Pos.Height - pad + Pos.Height, Pos.Width - pad, Pos.Height - pad + Pos.Height, colorActive);
                    // temp->drawLine(pos.w - pad, pad + pos.h, pos.w - pad, pos.h - pad + pos.h, color_active);

                    _background = temp.CreateSprite();
                    temp.Unref();
                }
            }
        }

        public void SetFontName(string fontName)
        {
            _fontName = fontName;
        }
    }
}
