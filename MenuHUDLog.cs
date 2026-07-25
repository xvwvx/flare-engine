// <自动生成> 对应 C++ 源文件：MenuHUDLog.h + MenuHUDLog.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// MenuHUDLog
    ///
    /// 用于在屏幕角落短暂显示提示信息（拾取物品、任务更新等）的日志菜单。
    /// 除了正常渐隐的消息列表外，还支持在其它菜单之上以半透明遮罩的形式
    /// 覆盖显示最后一条消息（<see cref="RenderOverlay"/>），并允许点击遮罩将其关闭。
    ///
    /// <see cref="Menu"/> 是尚未转换的前向依赖单元（<c>Menu.h</c>/<c>Menu.cpp</c>），
    /// 本类按预期的最终符号命名（<c>WindowArea</c>、<c>Align()</c>、<c>ParseMenuKey</c>）引用其成员。
    /// 持有的 <c>Sprite</c> 缓存资源（<see cref="_overlayBg"/> 与 <c>_msgBuffer</c> 中的元素）
    /// 通过 <see cref="IDisposable"/> 显式释放，释放顺序与原始析构函数 <c>~MenuHUDLog()</c> 一致
    /// （先释放遮罩背景 <c>overlay_bg</c>，再按下标从 0 到末尾依次释放 <c>msg_buffer</c> 中的元素）。
    /// </summary>
    public class MenuHUDLog : Menu, IDisposable
    {
        /// <summary>对应匿名枚举 <c>MSG_NORMAL</c>：普通消息，如果与上一条消息文本相同则只刷新计时。</summary>
        public const int MsgNormal = 0;

        /// <summary>对应匿名枚举 <c>MSG_UNIQUE</c>：即使与上一条消息文本相同，也强制作为新消息追加。</summary>
        public const int MsgUnique = 1;

        private readonly List<string> _logMsg = new List<string>();
        private readonly List<int> _msgAge = new List<int>();
        private readonly List<Sprite?> _msgBuffer = new List<Sprite?>();

        private int _paragraphSpacing;

        private Sprite? _overlayBg;
        private bool _enableOverlay;
        private bool _clickToDismiss;
        private bool _startAtBottom;
        private bool _overlayAtBottom;

        /// <summary>
        /// 对应 C++ 原始公有字段 <c>hide_overlay</c>（无 getter/setter 包装），
        /// 与 output/Widget.cs、output/WidgetSlider.cs 中"原始就是公有字段则保留为公有字段"的既有约定一致。
        /// </summary>
        public bool HideOverlay;

        public MenuHUDLog()
        {
            _overlayBg = null;
            _enableOverlay = true;
            _clickToDismiss = false;
            _startAtBottom = true;
            _overlayAtBottom = true;
            HideOverlay = false;

            // Load config settings
            using FileParser infile = new FileParser();
            // @CLASS MenuHUDLog|Description of menus/hudlog.txt
            if (infile.Open("menus/hudlog.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (ParseMenuKey(infile.Key, infile.Val))
                        continue;
                    else if (infile.Key == "enable_overlay")
                        // @ATTR enable_overlay|bool|If true, shows an overlay of the last message on top of other menus.
                        _enableOverlay = Parse.ToBool(infile.Val);
                    else if (infile.Key == "start_at_bottom")
                        // @ATTR start_at_bottom|bool|If true, messages start at the bottom and get pushed up. If false, messages start at the top and get pushed down.
                        _startAtBottom = Parse.ToBool(infile.Val);
                    else if (infile.Key == "overlay_at_bottom")
                        // @ATTR overlay_at_bottom|bool|If true, the overlay message will be at the bottom of the HUD log area. If false, it will be at the top.
                        _overlayAtBottom = Parse.ToBool(infile.Val);
                    else
                        infile.Error("MenuHUDLog: '%s' is not a valid key.", infile.Key);
                }
                infile.Close();
            }

            Align();

            SharedResources.Font!.SetFont("font_regular");
            _paragraphSpacing = SharedResources.Font.GetLineHeight() / 2;
        }

        /// <summary>
        /// 对应 C++ 的 <c>~MenuHUDLog()</c>：先释放遮罩背景精灵，再按下标顺序释放消息缓冲区中的每个精灵。
        /// C++ 版本在 <c>~MenuHUDLog()</c> 结束后会隐式调用 <c>~Menu()</c>；<see cref="Menu"/>
        /// 是尚未转换的前向依赖单元，其是否需要在此处补充 <c>base.Dispose()</c> 调用见转换报告"潜在风险"。
        /// </summary>
        public override void Dispose()
        {
            _overlayBg?.Dispose();

            for (int i = 0; i < _msgBuffer.Count; i++)
            {
                if (_msgBuffer[i] != null)
                    _msgBuffer[i]!.Dispose();
            }

            base.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Perform one frame of logic
        /// Age messages
        /// </summary>
        public void Logic()
        {
            for (int i = 0; i < _msgAge.Count; i++)
            {
                if (_msgAge[i] > 0)
                    _msgAge[i]--;
                else
                    Remove(i);
            }

            // click to dismiss messages when rendered on top of other menus
            if (_overlayBg != null && _clickToDismiss)
            {
                InputState inpt = SharedResources.Inpt!;

                if (inpt.Pressing[Input.Main1] && !inpt.Lock[Input.Main1])
                {
                    Rectangle overlayArea = default;
                    overlayArea.X = _overlayBg.Dest.X;
                    overlayArea.Y = _overlayBg.Dest.Y;
                    overlayArea.Width = _overlayBg.GetGraphicsWidth();
                    overlayArea.Height = _overlayBg.GetGraphicsHeight();

                    if (Utils.IsWithinRect(overlayArea, inpt.Mouse))
                    {
                        inpt.Lock[Input.Main1] = true;
                        HideOverlay = true;
                    }
                }
            }
        }

        /// <summary>
        /// New messages appear on the screen for a brief time
        /// </summary>
        public override void Render()
        {
            _clickToDismiss = false;

            if (_msgBuffer.Count == 0)
            {
                return;
            }

            HideOverlay = true;

            Rectangle dest = default;
            dest.X = WindowArea.X + _paragraphSpacing;

            if (_startAtBottom)
            {
                dest.Y = WindowArea.Y + WindowArea.Height;

                // go through new messages
                for (int i = _msgAge.Count; i > 0; i--)
                {
                    if (_msgAge[i - 1] > 0 && dest.Y > WindowArea.Y && _msgBuffer[i - 1] != null)
                    {
                        dest.Y -= _msgBuffer[i - 1]!.GetGraphicsHeight() + _paragraphSpacing;
                        _msgBuffer[i - 1]!.SetDestFromRect(dest);
                        SharedResources.RenderDevice!.Render(_msgBuffer[i - 1]!);
                    }
                    else return; // no more new messages
                }
            }
            else
            {
                dest.Y = WindowArea.Y + _paragraphSpacing;

                // go through new messages
                for (int i = _msgAge.Count; i > 0; i--)
                {
                    int msgHeight = _paragraphSpacing;
                    if (_msgBuffer[i - 1] != null)
                        msgHeight += _msgBuffer[i - 1]!.GetGraphicsHeight();

                    if (_msgAge[i - 1] > 0 && dest.Y + msgHeight < WindowArea.Y + WindowArea.Height && _msgBuffer[i - 1] != null)
                    {
                        // dest.Y -= _msgBuffer[i - 1]!.GetGraphicsHeight() + _paragraphSpacing;
                        _msgBuffer[i - 1]!.SetDestFromRect(dest);
                        SharedResources.RenderDevice!.Render(_msgBuffer[i - 1]!);
                        dest.Y += msgHeight;
                    }
                    else return; // no more new messages
                }
            }

        }

        /// <summary>
        /// Add a new message to the log
        /// </summary>
        public void Add(string s, int type)
        {
            HideOverlay = false;

            // Make sure we don't spam the same message repeatedly
            if (_logMsg.Count == 0 || _logMsg[^1] != s || type == MsgUnique)
            {
                // add new message
                _logMsg.Add(Utils.SubstituteVarsInString(s, SharedGameResources.Pc));
                _msgAge.Add(CalcDuration(_logMsg[^1]));

                // render the log entry and store it in a buffer
                SharedResources.Font!.SetFont("font_regular");
                Int2 size = SharedResources.Font.CalcSizeWrapped(_logMsg[^1], WindowArea.Width - (_paragraphSpacing * 2));
                Image? graphics = SharedResources.RenderDevice!.CreateImage(size.X, size.Y);
                SharedResources.Font.RenderShadowed(_logMsg[^1], 0, 0, FontEngine.JustifyLeft, graphics!, WindowArea.Width - (_paragraphSpacing * 2), SharedResources.Font.GetColor(FontEngine.ColorMenuNormal));
                _msgBuffer.Add(graphics!.CreateSprite());
                graphics.Unref();
            }
            else if (_msgAge.Count != 0)
            {
                _msgAge[^1] = CalcDuration(_logMsg[^1]);
            }

            // force HUD messages to vanish in order
            if (_msgAge.Count > 1)
            {
                int last = _msgAge.Count - 1;
                if (_msgAge[last] < _msgAge[last - 1])
                    _msgAge[last] = _msgAge[last - 1];
            }

        }

        /// <summary>
        /// Remove the given message from the list
        /// </summary>
        public void Remove(int msgIndex)
        {
            if (_msgBuffer[msgIndex] != null)
                _msgBuffer[msgIndex]!.Dispose();
            _msgBuffer.RemoveAt(msgIndex);
            _msgAge.RemoveAt(msgIndex);
            _logMsg.RemoveAt(msgIndex);
        }

        public void Clear()
        {
            for (int i = 0; i < _msgBuffer.Count; i++)
            {
                if (_msgBuffer[i] != null)
                    _msgBuffer[i]!.Dispose();
            }
            _msgBuffer.Clear();
            _msgAge.Clear();
            _logMsg.Clear();
        }

        /// <summary>
        /// Displays the last message with a shaded background
        /// It is meant to be displayed on top of other menus in place of the normal render output
        /// </summary>
        public void RenderOverlay()
        {
            if (_msgBuffer.Count == 0 || HideOverlay || !_enableOverlay)
            {
                _clickToDismiss = false;
                return;
            }

            _clickToDismiss = true;

            int msgHeight = _msgBuffer[^1]!.GetGraphicsHeight() + _paragraphSpacing * 2;
            bool resizeBg = _overlayBg == null || _overlayBg.GetGraphicsHeight() != msgHeight;

            if (resizeBg)
            {
                if (_overlayBg != null)
                {
                    _overlayBg.Dispose();
                    _overlayBg = null;
                }

                Image? temp = SharedResources.RenderDevice!.CreateImage(WindowArea.Width, msgHeight);

                if (temp != null)
                {
                    // fill with translucent black
                    Color bgColor = new Color(0, 0, 0, 255);
                    bgColor.A = 200;
                    temp.FillWithColor(bgColor);

                    _overlayBg = temp.CreateSprite();
                    temp.Unref();
                }
            }

            int startY;
            if (_overlayAtBottom)
                startY = WindowArea.Y + WindowArea.Height - msgHeight;
            else
                startY = WindowArea.Y;

            if (_overlayBg != null)
            {
                _overlayBg.SetDest(WindowArea.X, startY);
                SharedResources.RenderDevice!.Render(_overlayBg);
            }

            Rectangle dest = default;
            dest.X = WindowArea.X + _paragraphSpacing;
            dest.Y = startY + _paragraphSpacing;

            _msgBuffer[^1]!.SetDestFromRect(dest);
            SharedResources.RenderDevice!.Render(_msgBuffer[^1]!);
        }

        /// <summary>
        /// Calculate how long a given message should remain on the HUD
        /// Formula: minimum time plus x frames per character
        /// </summary>
        private int CalcDuration(string s)
        {
            // 5 seconds plus an extra second per 10 letters
            return SharedResources.Settings!.MaxFramesPerSec * 5 + s.Length * (SharedResources.Settings.MaxFramesPerSec / 10);
        }
    }
}
