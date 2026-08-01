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
        private bool _ctrlCUsed;     // prevent repeated Ctrl+C
        private bool _ctrlVUsed;     // prevent repeated Ctrl+V
        private bool _ctrlXUsed;     // prevent repeated Ctrl+X cut
        private bool _ctrlAUsed;     // prevent repeated Ctrl+A select all
        private int _selStart = -1;  // selection start (char index, -1 = no selection)
        private int _selEnd = -1;    // selection end (char index, exclusive)
        private bool _showCursor = true;   // cursor blink visibility
        private int _cursorBlinkTimer;     // frame counter for blink
        private int _cursorPixelX;         // pixel X position for drawn cursor
        private int _displayedTextStart;   // start index in _text of the displayed _trimmedText
        private int _mouseDragAnchor = -1; // cursor position where mouse drag started (-1 = not dragging)

        /// <summary>Returns true if there is an active selection.</summary>
        private bool HasSelection() => _selStart >= 0 && _selEnd >= 0 && _selEnd != _selStart;

        /// <summary>Returns selection range as (startInclusive, endExclusive), clamping to text bounds.</summary>
        private (int start, int end) GetSelectionRange()
        {
            int s = Math.Max(0, _selStart);
            int e = Math.Min(_text.Length, _selEnd);
            if (s > e) (s, e) = (e, s);
            return (s, e);
        }

        /// <summary>Clears the current selection.</summary>
        private void ClearSelection()
        {
            _selStart = -1;
            _selEnd = -1;
        }

        /// <summary>Converts a mouse pixel X coordinate to a cursor position in _text.</summary>
        private int PixelToCursorPos(int mouseX)
        {
            int relX = mouseX - _fontPos.X;
            if (relX <= 0)
                return _displayedTextStart;

            var font = SharedResources.Font!;

            int cumulative = 0;
            for (int i = 0; i < _trimmedText.Length; i++)
            {
                int charLen = 1;
                if (char.IsHighSurrogate(_trimmedText[i]) && i + 1 < _trimmedText.Length)
                    charLen = 2;
                int charW = font.CalcSize(_trimmedText.Substring(i, charLen)).X;
                if (cumulative + charW / 2 >= relX)
                    return _displayedTextStart + i;
                cumulative += charW;
                i += charLen - 1;
            }
            return _displayedTextStart + _trimmedText.Length;
        }

        /// <summary>Deletes the selected text and returns it (for clipboard), or deletes single char at cursor.</summary>
        private string DeleteSelectionOrChar()
        {
            if (HasSelection())
            {
                var (s, e) = GetSelectionRange();
                string deleted = _text.Substring(s, e - s);
                _text = _text.Remove(s, e - s);
                _cursorPos = s;
                ClearSelection();
                TrimText();
                return deleted;
            }
            else if (_cursorPos > 0)
            {
                int n = _cursorPos - 1;
                while (n > 0 && char.IsLowSurrogate(_text[n])) n--;
                string deleted = _text.Substring(n, _cursorPos - n);
                _text = _text.Remove(n, _cursorPos - n);
                _cursorPos = n;
                TrimText();
                return deleted;
            }
            return "";
        }

        public WidgetInput(string filename)
        {
            _background = null;
            Enabled = true;
            Pressed = false;
            _cursorPos = 0;
            _fontName = "font_regular";
            _soundActivate = 0;
            _ctrlCUsed = false;
            _ctrlVUsed = false;
            _ctrlXUsed = false;
            _ctrlAUsed = false;
            _selStart = -1;
            _selEnd = -1;
            EditMode = false;
            MaxLength = 0;
            OnlyNumbers = false;
            AcceptToDefocus = true;

            LoadGraphics(filename);

            var eset = SharedResources.Eset!;
            var snd = SharedResources.Snd!;

            if (eset.Widgets.SoundActivate.Length != 0)
                _soundActivate = snd.Load(eset.Widgets.SoundActivate, "Widget activate");
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

            var font = SharedResources.Font!;
            font.SetFont(_fontName);
            _fontPos.X = Pos.X + (font.GetFontHeight() / 2);
            _fontPos.Y = Pos.Y + (Pos.Height / 2) - (font.GetFontHeight() / 2);
        }

        protected void LoadGraphics(string filename)
        {
            if (filename == NoFile)
                return;

            // load input background image
            var renderDevice = SharedResources.RenderDevice!;
            Image? graphics = null;
            if (filename != DefaultFile)
            {
                graphics = renderDevice.LoadImage(filename, RenderDevice.ErrorNormal);
            }
            if (graphics == null)
            {
                graphics = renderDevice.LoadImage(DefaultFile, RenderDevice.ErrorExit);
            }
            if (graphics != null)
            {
                _background = graphics.CreateSprite();
                Pos.Width = _background.GetGraphicsWidth();
                Pos.Height = _background.GetGraphicsHeight() / 2;
                graphics.Unref();
            }
        }

        /// <param name="resetBlink">true for user actions (always show cursor and reset blink timer).
        /// false for blink-driven updates (respects _showCursor state).</param>
        protected void TrimText(bool resetBlink = true)
        {
            var font = SharedResources.Font!;

            if (resetBlink)
            {
                _showCursor = true;
                _cursorBlinkTimer = 0;
            }

            int padding = font.GetFontHeight();
            int maxWidth = Pos.Width - padding;

            if (EditMode)
            {
                // Use cursorPos as leftPos so cursor stays visible when text is long
                _trimmedText = font.TrimTextToWidth(_text, maxWidth, !FontEngine.UseEllipsis, _cursorPos);
                _trimmedTextCursor = _trimmedText;

                // Determine where displayed text starts in original _text
                _displayedTextStart = _text.IndexOf(_trimmedText, StringComparison.Ordinal);
                if (_displayedTextStart < 0) _displayedTextStart = 0;

                // Compute cursor pixel X from displayed text
                int cursorOffsetInTrimmed = _cursorPos - _displayedTextStart;
                if (cursorOffsetInTrimmed < 0) cursorOffsetInTrimmed = 0;
                if (cursorOffsetInTrimmed > _trimmedText.Length) cursorOffsetInTrimmed = _trimmedText.Length;
                _cursorPixelX = _fontPos.X + font.CalcSize(_trimmedText.Substring(0, cursorOffsetInTrimmed)).X;
            }
            else
            {
                _trimmedText = font.TrimTextToWidth(_text, maxWidth, !FontEngine.UseEllipsis, _text.Length);
                _trimmedTextCursor = _trimmedText;
                _displayedTextStart = 0;
            }
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

            var inpt = SharedResources.Inpt!;
            var snd = SharedResources.Snd!;

            // main button already in use, new click not allowed
            if (inpt.Lock[Input.Main1]) return false;

            // main click released, so the button state goes back to unpressed
            if (Pressed && !inpt.Lock[Input.Main1])
            {
                Pressed = false;

                if (Utils.IsWithinRect(Pos, mouse))
                {
                    snd.Play(_soundActivate, "widget_activate", SoundManager.NoPos, !SoundManager.Loop);

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
                TrimText();
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

                // --- Mouse drag: cursor positioning and selection ---
                if (inpt.Pressing[Input.Main1] && Utils.IsWithinRect(Pos, mouse))
                {
                    int clickedPos = PixelToCursorPos(mouse.X);
                    if (_mouseDragAnchor < 0)
                    {
                        // Start of drag: position cursor, clear previous selection
                        _mouseDragAnchor = clickedPos;
                        _cursorPos = clickedPos;
                        ClearSelection();
                        TrimText();
                    }
                    else if (clickedPos != _cursorPos)
                    {
                        // Continuing drag: extend selection from anchor to current
                        _cursorPos = clickedPos;
                        _selStart = Math.Min(_mouseDragAnchor, clickedPos);
                        _selEnd = Math.Max(_mouseDragAnchor, clickedPos);
                        TrimText();
                    }
                }
                else if (_mouseDragAnchor >= 0)
                {
                    // Mouse released or left widget — end drag
                    _mouseDragAnchor = -1;
                }

                if (inpt.Inkeys != "")
                {
                    // handle text input — replace selection if any, otherwise insert at cursor
                    if (!OnlyNumbers || (inpt.Inkeys[0] >= 48 && inpt.Inkeys[0] <= 57))
                    {
                        if (HasSelection())
                        {
                            var (s, e) = GetSelectionRange();
                            _text = _text.Remove(s, e - s);
                            _cursorPos = s;
                            ClearSelection();
                        }
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

                // --- Clipboard & selection shortcuts (using raw SDL key state for reliability) ---
                var sdlKeys = SDLInputState.Sdl;
                bool ctrlHeld = sdlKeys != null && (sdlKeys.IsKeyPressed(SdlScancode.LCtrl) || sdlKeys.IsKeyPressed(SdlScancode.RCtrl));
                bool shiftHeld = sdlKeys != null && (sdlKeys.IsKeyPressed(SdlScancode.LShift) || sdlKeys.IsKeyPressed(SdlScancode.RShift));

                // Ctrl+A: select all
                if (ctrlHeld && !_ctrlAUsed && sdlKeys!.IsKeyPressed(SdlScancode.A))
                {
                    _selStart = 0;
                    _selEnd = _text.Length;
                    _ctrlAUsed = true;
                    TrimText();
                }

                // Ctrl+C: copy selection (or all text)
                if (ctrlHeld && !_ctrlCUsed && sdlKeys!.IsKeyPressed(SdlScancode.C))
                {
                    string copyText;
                    if (HasSelection())
                    {
                        var (s, e) = GetSelectionRange();
                        copyText = _text.Substring(s, e - s);
                    }
                    else
                    {
                        copyText = _text;
                    }
                    if (copyText.Length > 0)
                        sdlKeys.SetClipboardText(copyText);
                    _ctrlCUsed = true;
                }

                // Ctrl+X: cut selection (or all text)
                if (ctrlHeld && !_ctrlXUsed && sdlKeys!.IsKeyPressed(SdlScancode.X))
                {
                    string cutText;
                    if (HasSelection())
                    {
                        var (s, e) = GetSelectionRange();
                        cutText = _text.Substring(s, e - s);
                        _text = _text.Remove(s, e - s);
                        _cursorPos = s;
                        ClearSelection();
                        TrimText();
                    }
                    else
                    {
                        cutText = _text;
                        _text = "";
                        _cursorPos = 0;
                        TrimText();
                    }
                    if (cutText.Length > 0)
                        sdlKeys.SetClipboardText(cutText);
                    _ctrlXUsed = true;
                }

                // Ctrl+V: paste, replacing selection if any
                if (ctrlHeld && !_ctrlVUsed && sdlKeys!.IsKeyPressed(SdlScancode.V))
                {
                    string? clip = sdlKeys.GetClipboardText();
                    if (!string.IsNullOrEmpty(clip))
                    {
                        if (HasSelection())
                        {
                            var (s, e) = GetSelectionRange();
                            _text = _text.Remove(s, e - s);
                            _cursorPos = s;
                            ClearSelection();
                        }
                        if (!OnlyNumbers || (clip.Length > 0 && clip[0] >= 48 && clip[0] <= 57))
                        {
                            _text = _text.Insert(_cursorPos, clip);
                            _cursorPos += clip.Length;
                            TrimText();
                        }
                    }
                    _ctrlVUsed = true;
                }

                // Reset combo flags when modifier released
                if (!ctrlHeld)
                {
                    _ctrlCUsed = false;
                    _ctrlVUsed = false;
                    _ctrlXUsed = false;
                    _ctrlAUsed = false;
                }
                // Also reset per-key when the letter key is released, so multiple pastes work while holding Ctrl
                if (sdlKeys != null)
                {
                    if (!sdlKeys.IsKeyPressed(SdlScancode.V)) _ctrlVUsed = false;
                    if (!sdlKeys.IsKeyPressed(SdlScancode.C)) _ctrlCUsed = false;
                    if (!sdlKeys.IsKeyPressed(SdlScancode.X)) _ctrlXUsed = false;
                    if (!sdlKeys.IsKeyPressed(SdlScancode.A)) _ctrlAUsed = false;
                }

                // handle backspace — delete selection or single char
                if (inpt.Pressing[Input.Del] && inpt.RepeatCooldown[Input.Del].IsBegin())
                {
                    DeleteSelectionOrChar();
                }

                // cursor movement with Shift selection (using raw SDL key state for Shift)
                // Skip during Ctrl to avoid A-key→Input.Left binding conflict
                if (!ctrlHeld && _text.Length != 0 && _cursorPos > 0 && inpt.Pressing[Input.Left] && inpt.RepeatCooldown[Input.Left].IsBegin())
                {
                    if (shiftHeld)
                    {
                        if (!HasSelection())
                            _selStart = _selEnd = _cursorPos;
                        if (_cursorPos > 0)
                        {
                            _cursorPos--;
                            _selEnd = _cursorPos;
                        }
                    }
                    else
                    {
                        ClearSelection();
                        _cursorPos--;
                    }
                    TrimText();
                }
                else if (!ctrlHeld && _text.Length != 0 && _cursorPos < _text.Length && inpt.Pressing[Input.Right] && inpt.RepeatCooldown[Input.Right].IsBegin())
                {
                    inpt.Lock[Input.Right] = true;
                    if (shiftHeld)
                    {
                        if (!HasSelection())
                            _selStart = _selEnd = _cursorPos;
                        _cursorPos++;
                        _selEnd = _cursorPos;
                    }
                    else
                    {
                        ClearSelection();
                        _cursorPos++;
                    }
                    TrimText();
                }

                // --- cursor blink (toggle every ~0.5s at current frame rate) ---
                _cursorBlinkTimer++;
                uint maxFps = SharedResources.Settings!.MaxFramesPerSec;
                if (_cursorBlinkTimer >= maxFps / 2)
                {
                    _cursorBlinkTimer = 0;
                    _showCursor = !_showCursor;
                    TrimText(false);
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
            var renderDevice = SharedResources.RenderDevice!;
            var font = SharedResources.Font!;
            var eset = SharedResources.Eset!;
            var settings = SharedResources.Settings!;

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
                renderDevice.Render(_background);
            }

            font.SetFont(_fontName);

            // Render selection highlight
            if (EditMode && HasSelection())
            {
                var (selS, selE) = GetSelectionRange();
                int selStartInDisplayed = selS - _displayedTextStart;
                int selEndInDisplayed = selE - _displayedTextStart;
                if (selStartInDisplayed < 0) selStartInDisplayed = 0;
                if (selEndInDisplayed > _trimmedText.Length) selEndInDisplayed = _trimmedText.Length;
                if (selEndInDisplayed > selStartInDisplayed)
                {
                    string beforeSel = _trimmedText.Substring(0, selStartInDisplayed);
                    string selText = _trimmedText.Substring(selStartInDisplayed, selEndInDisplayed - selStartInDisplayed);
                    int selX = _fontPos.X + font.CalcSize(beforeSel).X;
                    int selW = font.CalcSize(selText).X;
                    int selH = font.GetFontHeight();
                    Color selColor = new Color(64, 128, 255, 128);
                    renderDevice.DrawRectangle(
                        new Int2(selX, _fontPos.Y),
                        new Int2(selX + selW, _fontPos.Y + selH),
                        selColor);
                }
            }

            if (!EditMode)
            {
                font.Render(_trimmedText, _fontPos.X, _fontPos.Y, FontEngine.JustifyLeft, null, 0, font.GetColor(FontEngine.ColorWidgetNormal), !FontEngine.ShadowOffset);
            }
            else
            {
                font.RenderShadowed(_trimmedText, _fontPos.X, _fontPos.Y, FontEngine.JustifyLeft, null, 0, font.GetColor(FontEngine.ColorWidgetNormal));

                // Draw cursor as vertical line
                if (_showCursor)
                {
                    int cursorH = font.GetFontHeight();
                    renderDevice.DrawRectangle(
                        new Int2(_cursorPixelX, _fontPos.Y + 1),
                        new Int2(_cursorPixelX + 1, _fontPos.Y + cursorH - 1),
                        font.GetColor(FontEngine.ColorWidgetNormal));
                }
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
                    renderDevice.DrawRectangleCorners(eset.Widgets.SelectionRectCornerSize, topLeft, bottomRight, eset.Widgets.SelectionRectColor);
                }
            }

            // handle on-screen keyboard
            if (Platform.Instance.IsMobileDevice && EditMode)
            {
                _oskBuf.Clear();
                _oskBuf.AddText(_trimmedTextCursor);
                _oskTip.Render(_oskBuf, new Int2(settings.ViewWHalf + Pos.Width / 2, 0), TooltipData.StyleFloat);
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
                var font = SharedResources.Font!;
                var renderDevice = SharedResources.RenderDevice!;

                int lineHeight = font.GetFontHeight();
                Pos.Width = newWidth;
                Pos.Height = (int)(lineHeight * 1.5);

                int gfxH = Pos.Height * 2;

                Image? temp = renderDevice.CreateImage(Pos.Width, gfxH);
                if (temp != null)
                {
                    Color colorInactive = font.GetColor(FontEngine.ColorWidgetDisabled);
                    Color colorActive = font.GetColor(FontEngine.ColorWidgetNormal);

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
