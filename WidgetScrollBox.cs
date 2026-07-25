// <自动生成> 对应 C++ 源文件：WidgetScrollBox.h + WidgetScrollBox.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// WidgetScrollBox
    ///
    /// 可滚动内容区域控件：内部维护离屏 Sprite 缓冲（Contents）与子 Widget 列表，
    /// 配合 <see cref="WidgetScrollBar"/> 实现垂直滚动、平滑滚动与 Tab 导航。
    /// 持有的 Contents Sprite 与 ScrollBar 通过 <see cref="IDisposable"/> 显式释放，
    /// 对应原始析构函数中 <c>delete contents; delete scrollbar;</c> 的顺序。
    /// </summary>
    public class WidgetScrollBox : Widget, IDisposable
    {
        private const int ScrollSpeedCoarseMod = 4;
        private const int ScrollSpeedSmoothMod = 3;

        public Sprite? Contents;
        public bool Update;
        public bool ShowFocusWhenScrollbarDisabled;
        public Color Bg;

        public TabList Tablist;

        private readonly List<Widget> _children = new List<Widget>();
        private int _currentChild;

        private float _cursor;
        private float _cursorTarget;
        private WidgetScrollBar? _scrollbar;

        private Int2 _contentsSize;

        public WidgetScrollBox(int width, int height)
        {
            Contents = null;
            Update = true;
            ShowFocusWhenScrollbarDisabled = true;
            Bg = new Color(0, 0, 0, 0);
            Tablist = new TabList();
            _cursor = 0;
            _cursorTarget = 0;
            _scrollbar = new WidgetScrollBar(WidgetScrollBar.DefaultFile);

            Pos.X = Pos.Y = 0;
            Pos.Width = width;
            Pos.Height = height;
            _currentChild = -1;
            ScrollType = ScrollVertical;

            Resize(width, height);
            Tablist.SetScrollType(ScrollTwoDirections);
            Tablist.IsInnerTablist = true;
        }

        /// <summary>
        /// 对应 C++ 的 <c>~WidgetScrollBox()</c>：先释放 contents，再释放 scrollbar。
        /// </summary>
        public void Dispose()
        {
            Contents?.Dispose();
            Contents = null;
            _scrollbar?.Dispose();
            _scrollbar = null;
            GC.SuppressFinalize(this);
        }

        public override void SetPos(int offsetX, int offsetY)
        {
            base.SetPos(offsetX, offsetY);

            if (Contents != null && _scrollbar != null)
            {
                _scrollbar.Refresh(Pos.X + Pos.Width, Pos.Y, Pos.Height, (int)_cursor, _contentsSize.Y - Pos.Height);
            }
        }

        public void AddChildWidget(Widget child)
        {
            if (!_children.Contains(child))
            {
                _children.Add(child);
                Tablist.Add(child);
                child.LocalFrame = Pos;
            }
        }

        public void ClearChildWidgets()
        {
            _currentChild = -1;
            _children.Clear();
            Tablist.Clear();
        }

        private void Scroll(int amount)
        {
            _cursorTarget += amount;
            if (_cursorTarget < 0)
            {
                _cursorTarget = 0;
            }
            else if (Contents != null && _cursorTarget > _contentsSize.Y - Pos.Height)
            {
                _cursorTarget = _contentsSize.Y - Pos.Height;
            }
            Refresh();
        }

        private void ScrollTo(int amount)
        {
            _cursor = amount;
            if (_cursor < 0)
            {
                _cursor = 0;
            }
            else if (Contents != null && _cursor > _contentsSize.Y - Pos.Height)
            {
                _cursor = _contentsSize.Y - Pos.Height;
            }
            _cursorTarget = _cursor;
            Refresh();
        }

        private void ScrollToSmooth(int amount)
        {
            amount -= Pos.Height / 2;

            _cursorTarget = amount;
            if (_cursorTarget < 0)
            {
                _cursorTarget = 0;
            }
            else if (Contents != null && _cursorTarget > _contentsSize.Y - Pos.Height)
            {
                _cursorTarget = _contentsSize.Y - Pos.Height;
            }
            Refresh();
        }

        private void ScrollDown()
        {
            int amount = Pos.Height / ScrollSpeedCoarseMod;
            Scroll(amount);
        }

        private void ScrollUp()
        {
            int amount = Pos.Height / ScrollSpeedCoarseMod;
            Scroll(-amount);
        }

        public void ScrollToTop()
        {
            ScrollTo(0);
        }

        public Int2 InputAssist(Int2 mouse)
        {
            Int2 newMouse = new Int2();
            if (Utils.IsWithinRect(Pos, mouse))
            {
                newMouse.X = mouse.X - Pos.X;
                newMouse.Y = mouse.Y - Pos.Y + (int)_cursor;
            }
            else
            {
                // x position is maintained for dragging of WidgetSlider knobs
                newMouse.X = mouse.X - Pos.X;
                newMouse.Y = -1;
            }
            return newMouse;
        }

        public void Logic()
        {
            Logic(SharedResources.Inpt!.Mouse.X, SharedResources.Inpt!.Mouse.Y);
            if (InFocus)
            {
                if (_currentChild == -1 && _children.Count != 0)
                    GetNext();
                Tablist.Logic();
            }
            else if (_currentChild != -1 || Tablist.GetCurrent() != -1)
            {
                Tablist.Defocus();
                _currentChild = -1;
            }
        }

        public void Logic(int x, int y)
        {
            Int2 mouse = new Int2(x, y);

            InputState inpt = SharedResources.Inpt!;

            if (Utils.IsWithinRect(Pos, mouse))
            {
                inpt.LockScroll = true;
                if (inpt.ScrollUp) ScrollUp();
                if (inpt.ScrollDown) ScrollDown();
            }
            else
            {
                inpt.LockScroll = false;
            }

            // check ScrollBar clicks
            if (Contents != null && _contentsSize.Y > Pos.Height && _scrollbar != null)
            {
                switch (_scrollbar.CheckClickAt(mouse.X, mouse.Y))
                {
                    case WidgetScrollBar.ClickUp:
                        ScrollUp();
                        break;
                    case WidgetScrollBar.ClickDown:
                        ScrollDown();
                        break;
                    case WidgetScrollBar.ClickKnob:
                        _cursor = _cursorTarget = _scrollbar.Value;
                        break;
                    default:
                        break;
                }
            }

            if (_cursorTarget < _cursor)
            {
                _cursor -= (Pos.Height * ScrollSpeedSmoothMod + (_cursor - _cursorTarget)) / SharedResources.Settings!.MaxFramesPerSec;
                if (_cursor < _cursorTarget)
                    _cursor = _cursorTarget;
            }
            else if (_cursorTarget > _cursor)
            {
                _cursor += (Pos.Height * ScrollSpeedSmoothMod + (_cursorTarget - _cursor)) / SharedResources.Settings!.MaxFramesPerSec;
                if (_cursor > _cursorTarget)
                    _cursor = _cursorTarget;
            }

            // getPrev() and getNext() aren't called when pressing left and right
            int currentChild = Tablist.GetCurrent();
            if (currentChild != -1)
            {
                if (_children[currentChild].InFocus)
                {
                    if (_children[currentChild].Pos.Y < (int)_cursor || _children[currentChild].Pos.Y > (int)_cursor + Pos.Height)
                        ScrollToSmooth(_children[currentChild].Pos.Y);
                }
            }
        }

        public void Resize(int w, int h)
        {
            Pos.Width = w;

            if (Pos.Height > h)
                h = Pos.Height;

            _contentsSize.X = w;
            _contentsSize.Y = h;

            _cursor = _cursorTarget = 0;

            Update = true;
            Refresh();
        }

        public void Refresh()
        {
            if (Update)
            {
                if (Contents != null)
                {
                    Contents.Dispose();
                    Contents = null;
                }

                Image? graphics;
                graphics = SharedResources.RenderDevice!.CreateImage(_contentsSize.X, _contentsSize.Y);
                if (graphics != null)
                {
                    Contents = graphics.CreateSprite();
                    graphics.Unref();
                }

                if (Contents != null)
                {
                    Contents.GetGraphics()!.FillWithColor(Bg);
                }
            }

            if (Contents != null && _scrollbar != null)
            {
                _scrollbar.Refresh(Pos.X + Pos.Width, Pos.Y, Pos.Height, (int)_cursorTarget, _contentsSize.Y - Pos.Height);
            }
        }

        public override void Render()
        {
            Update = false;

            Rectangle src = new Rectangle();
            Rectangle dest = new Rectangle();
            dest = Pos;
            src.X = 0;
            src.Y = (int)_cursor;
            src.Width = Pos.Width;
            src.Height = Pos.Height;

            int contentHeight = 0;

            // draw content buffer, minus child widgets
            if (Contents != null)
            {
                contentHeight = _contentsSize.Y;
                Contents.LocalFrame = LocalFrame;
                Contents.SetOffset(LocalOffset);
                Contents.SetClipFromRect(src);
                Contents.SetDestFromRect(dest);
                SharedResources.RenderDevice!.Render(Contents);
            }

            // draw child widgets
            for (int i = 0; i < _children.Count; i++)
            {
                _children[i].LocalFrame = Pos;
                _children[i].LocalOffset.Y = (int)_cursor;
                _children[i].Render();
            }

            // draw scrollbar
            if (contentHeight > Pos.Height && _scrollbar != null)
            {
                _scrollbar.LocalFrame = LocalFrame;
                _scrollbar.LocalOffset = LocalOffset;
                _scrollbar.Render();
            }

            // draw focus rectangle around the scrollbar
            if (InFocus && _children.Count == 0 && (ShowFocusWhenScrollbarDisabled || contentHeight > Pos.Height))
            {
                Int2 topLeft = new Int2();
                Int2 bottomRight = new Int2();
                Rectangle sbRect = _scrollbar!.GetBounds();

                topLeft.X = sbRect.X + LocalFrame.X - LocalOffset.X;
                topLeft.Y = sbRect.Y + LocalFrame.Y - LocalOffset.Y;
                bottomRight.X = topLeft.X + sbRect.Width;
                bottomRight.Y = topLeft.Y + sbRect.Height;

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
        }

        public override bool GetNext()
        {
            if (_children.Count == 0)
            {
                int prevCursor = (int)_cursor;
                int bottom = Contents != null ? _contentsSize.Y - Pos.Height : 0;

                ScrollDown();

                if ((int)_cursor == bottom && prevCursor == bottom)
                    return false;

                return true;
            }

            if (_currentChild != -1)
            {
                _children[_currentChild].InFocus = false;
                _currentChild = Tablist.GetNextRelativeIndex(TabList.WidgetSelectDown);
                if (_currentChild != -1)
                    Tablist.SetCurrent(_children[_currentChild]);
            }
            else
            {
                if (!_children[0].EnableTablistNav)
                {
                    Tablist.GetNext(!TabList.GetInner, TabList.WidgetSelectAuto);
                    _currentChild = Tablist.GetCurrent();
                }
                else
                {
                    _currentChild = 0;
                    Tablist.SetCurrent(_children[_currentChild]);
                }
            }

            if (_currentChild != -1)
            {
                _children[_currentChild].InFocus = true;
                ScrollToSmooth(_children[_currentChild].Pos.Y);
            }
            else
            {
                return false;
            }

            return true;
        }

        public override bool GetPrev()
        {
            if (_children.Count == 0)
            {
                int prevCursor = (int)_cursor;

                ScrollUp();

                if (_cursor == 0 && prevCursor == 0)
                    return false;

                return true;
            }

            if (_currentChild != -1)
            {
                _children[_currentChild].InFocus = false;
                _currentChild = Tablist.GetNextRelativeIndex(TabList.WidgetSelectUp);
                if (_currentChild != -1)
                    Tablist.SetCurrent(_children[_currentChild]);
            }
            else
            {
                if (!_children[0].EnableTablistNav)
                {
                    _currentChild = Tablist.GetNextRelativeIndex(TabList.WidgetSelectDown);
                    Tablist.SetCurrent(_children[_currentChild]);
                }
                else
                {
                    _currentChild = 0;
                    Tablist.SetCurrent(_children[_currentChild]);
                }
            }

            if (_currentChild != -1)
            {
                _children[_currentChild].InFocus = true;
                ScrollToSmooth(_children[_currentChild].Pos.Y);
            }
            else
            {
                return false;
            }

            return true;
        }

        public override void Activate()
        {
            if (_currentChild != -1)
                _children[_currentChild].Activate();
        }
    }
}
