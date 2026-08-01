// <自动生成> 对应 C++ 源文件：WidgetLog.h + WidgetLog.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// WidgetLog
    ///
    /// 可滚动的多行文本日志控件，消息按添加顺序存储，渲染时自底向上绘制到内部
    /// <see cref="WidgetScrollBox"/> 的内容区域。
    /// 持有的 <see cref="WidgetScrollBox"/> 通过 <see cref="IDisposable"/> 显式释放，
    /// 对应原始析构函数中 <c>delete scroll_box;</c> 与 <c>clear()</c> 的调用顺序。
    ///
    /// <see cref="WidgetScrollBox"/> 是尚未转换的前向依赖单元（<c>WidgetScrollBox.h</c>），
    /// 本类按预期的最终符号命名（<c>Contents</c>、<c>Update</c>、<c>Resize</c>、
    /// <c>ScrollToTop</c> 等）引用其成员。
    /// </summary>
    public class WidgetLog : Widget, IDisposable
    {
        /// <summary>对应匿名枚举 <c>FONT_REGULAR</c>。</summary>
        public const int FontRegular = 0;

        /// <summary>对应匿名枚举 <c>FONT_BOLD</c>。</summary>
        public const int FontBold = 1;

        /// <summary>对应匿名枚举 <c>MSG_NORMAL</c>：若与上一条消息文本相同则跳过追加。</summary>
        public const int MsgNormal = 0;

        /// <summary>对应匿名枚举 <c>MSG_UNIQUE</c>：即使与上一条消息文本相同也强制追加。</summary>
        public const int MsgUnique = 1;

        /// <summary>对应 <c>static const unsigned MAX_MESSAGES = 50</c>。</summary>
        public const uint MaxMessages = 50;

        private WidgetScrollBox? _scrollBox;
        private int _lineHeight;
        private int _paragraphSpacing;
        private int _padding;
        private uint _maxMessages;

        private readonly List<string> _messages = new List<string>();
        private readonly List<Color> _colors = new List<Color>();
        private readonly List<int> _styles = new List<int>();
        private readonly List<bool> _separators = new List<bool>();

        private bool _updated;

        private Color _nextColor;
        private int _nextStyle;

        private string _fontName;
        private string _fontBoldName;

        public WidgetLog(int width, int height)
        {
            var font = SharedResources.Font!;
            var eset = SharedResources.Eset!;

            _scrollBox = new WidgetScrollBox(width, height);
            _padding = eset.Widgets.LogPadding;
            _maxMessages = MaxMessages;
            _updated = false;
            _nextColor = font.GetColor(FontEngine.ColorMenuNormal);
            _nextStyle = FontRegular;
            _fontName = "font_regular";
            _fontBoldName = "font_bold";

            SetFont(FontRegular);
        }

        /// <summary>
        /// 对应 C++ 的 <c>~WidgetLog()</c>：先释放 scroll_box，再调用 clear()。
        /// </summary>
        public void Dispose()
        {
            _scrollBox?.Dispose();
            _scrollBox = null;
            Clear();
            GC.SuppressFinalize(this);
        }

        public override void SetBasePos(int x, int y, int a)
        {
            base.SetBasePos(x, y, a);
            _scrollBox!.SetBasePos(x, y, a);
        }

        public override void SetPos(int offsetX, int offsetY)
        {
            base.SetPos(offsetX, offsetY);
            _scrollBox!.SetPos(offsetX, offsetY);
        }

        private void SetFont(int style)
        {
            var font = SharedResources.Font!;

            if (style == FontBold)
            {
                font.SetFont(_fontBoldName);
            }
            else
            {
                font.SetFont(_fontName);
            }
            _lineHeight = font.GetLineHeight();
            _paragraphSpacing = _lineHeight / 2;
        }

        public void Logic()
        {
            _scrollBox!.Logic();
        }

        public override void Render()
        {
            if (_updated)
            {
                Refresh();
                _updated = false;
            }
            _scrollBox!.Render();
        }

        private void Refresh()
        {
            var font = SharedResources.Font!;
            var msg = SharedResources.Msg!;

            int y;
            int y2;
            y = y2 = _padding;

            int contentWidth = _scrollBox!.Pos.Width - (_padding * 2);

            // Resize the scrollbox content area first
            for (int i = 0; i < _messages.Count; i++)
            {
                SetFont(_styles[i]);
                Int2 size = font.CalcSizeWrapped(_messages[i], contentWidth);
                y += size.Y + _paragraphSpacing;

                if (_separators[i])
                    y += _paragraphSpacing + 1;
            }
            y += (_padding * 2);

            _scrollBox.Resize(_scrollBox.Pos.Width, y);

            // HACK: Sometimes the text buffer is too big to fit in a GPU texture for SDLHardwareRenderDevice
            // To get around this, we try to fall back to using the MAX_MESSAGES limit
            // This isn't a great fix and needs a better solution. Multiple buffers?
            if (_scrollBox.Contents == null || _scrollBox.Contents!.GetGraphics() == null)
            {
                SetMaxMessages(MaxMessages);
                SetNextColor(font.GetColor(FontEngine.ColorMenuPenalty));
                Add(msg.Get("ERROR: Text output too large"), MsgUnique);

                y = y2 = _padding;

                // Resize the scrollbox content area first
                for (int i = 0; i < _messages.Count; i++)
                {
                    SetFont(_styles[i]);
                    Int2 size = font.CalcSizeWrapped(_messages[i], contentWidth);
                    y += size.Y + _paragraphSpacing;

                    if (_separators[i])
                        y += _paragraphSpacing + 1;
                }
                y += (_padding * 2);

                _scrollBox.Resize(_scrollBox.Pos.Width, y);
            }

            // Render messages into the scrollbox area
            for (int i = _messages.Count; i > 0; i--)
            {
                SetFont(_styles[i - 1]);
                Int2 size = font.CalcSizeWrapped(_messages[i - 1], contentWidth);
                Image? renderTarget = _scrollBox.Contents!.GetGraphics();

                if (_separators.Count > 0 && _separators[i - 1])
                {
                    renderTarget!.DrawLine(_padding, y2, _padding + contentWidth - 1, y2, font.GetColor(FontEngine.ColorWidgetDisabled));
                    y2 += _paragraphSpacing;
                }
                font.RenderShadowed(_messages[i - 1], _padding, y2, FontEngine.JustifyLeft, renderTarget!, contentWidth, _colors[i - 1]);
                y2 += size.Y + _paragraphSpacing;

            }
        }

        /// <summary>
        /// 对应 C++ 内联 <c>getWidget()</c>：返回内部 scroll_box 指针以便加入 TabList。
        /// C# 中 <see cref="WidgetScrollBox"/> 本身继承 <see cref="Widget"/>，直接返回即可。
        /// </summary>
        public Widget GetWidget()
        {
            return _scrollBox!;
        }

        public void Add(string s, int type)
        {
            // First, make sure we're not repeating the last log message, to avoid spam
            if (_messages.Count == 0 || _messages[_messages.Count - 1] != s || type == MsgUnique)
            {
                // If we have too many messages, remove the oldest ones
                while (_messages.Count >= _maxMessages)
                {
                    Remove(0);
                }

                // Add the new message.
                _messages.Add(s);
                _colors.Add(_nextColor);
                _styles.Add(_nextStyle);
                _separators.Add(false);
                _updated = true;

                var font = SharedResources.Font!;
                _nextColor = font.GetColor(FontEngine.ColorMenuNormal);
                _nextStyle = FontRegular;
            }
        }

        public void SetNextColor(Color color)
        {
            _nextColor = color;
        }

        public void SetNextStyle(int style)
        {
            _nextStyle = style;
        }

        public void Remove(uint msgIndex)
        {
            if (msgIndex < _messages.Count)
            {
                _messages.RemoveAt((int)msgIndex);
                _colors.RemoveAt((int)msgIndex);
                _styles.RemoveAt((int)msgIndex);
                _separators.RemoveAt((int)msgIndex);
                _updated = true;
            }
        }

        public void Clear()
        {
            _messages.Clear();
            _colors.Clear();
            _styles.Clear();
            _separators.Clear();
            _updated = true;

            var font = SharedResources.Font!;
            _nextColor = font.GetColor(FontEngine.ColorMenuNormal);
            _nextStyle = FontRegular;
        }

        public void SetMaxMessages(uint count)
        {
            if (count > MaxMessages)
                _maxMessages = count;
            else
                _maxMessages = MaxMessages;
        }

        public void AddSeparator()
        {
            if (_messages.Count == 0) return;

            _separators[_separators.Count - 1] = true;
            _updated = true;
        }

        public bool IsEmpty()
        {
            return _messages.Count == 0;
        }

        public void Resize(int w, int h)
        {
            bool doRefresh = false;

            if (h > _scrollBox!.Contents!.GetGraphicsHeight())
                doRefresh = true;
            if (w != _scrollBox.Pos.Width)
                doRefresh = true;

            _scrollBox.Pos.Width = w;
            _scrollBox.Pos.Height = h;

            if (doRefresh)
                Refresh();

            _scrollBox.Update = false;
            _scrollBox.ScrollToTop();
        }

        public void SetFontNames(string fontName, string fontBoldName)
        {
            _fontName = fontName;
            _fontBoldName = fontBoldName;
        }

        public Image? SetupDrawBuffer(int bufH)
        {
            Clear();
            Resize(_scrollBox!.Pos.Width, Math.Max(_scrollBox.Pos.Height, bufH));
            Refresh();
            _updated = false;

            if (_scrollBox.Contents != null)
                return _scrollBox.Contents!.GetGraphics();
            else
                return null;
        }
    }
}
