// <自动生成> 对应 C++ 源文件：Widget.h + Widget.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// Widget：所有 UI 控件都需要实现的基础接口。
    /// </summary>
    public abstract class Widget
    {
        public const int ScrollVertical = 0;
        public const int ScrollHorizontal = 1;
        public const int ScrollTwoDirections = 2;

        public bool InFocus;
        public bool EnableTablistNav; // when disabled, this widget will be skipped during tablist navigation
        public byte TablistNavAlign; // used to determine the "center" point used when calculating relative distance between tablist widgets
        public byte ScrollType;
        public Rectangle Pos; // This is the position of the button within the screen
        public Rectangle LocalFrame; // Local reference frame is this is a daughter widget
        public Int2 LocalOffset; // Offset in local frame is this is a daughter widget
        public Int2 PosBase; // the initial x/y position of this widget, often from a config file
        public int Alignment;

        protected Widget()
        {
            InFocus = false;
            EnableTablistNav = true;
            TablistNavAlign = TabList.NavAlignCenter;
            ScrollType = ScrollTwoDirections;
            Alignment = Utils.AlignTopLeft;
        }

        public abstract void Render();

        public virtual void Activate()
        {
        }

        public virtual void Deactivate()
        {
        }

        public virtual void Defocus()
        {
            InFocus = false;
        }

        // getNext and getPrev should be implemented if the widget has items internally that can be iterated
        public virtual bool GetNext()
        {
            return false;
        }

        public virtual bool GetPrev()
        {
            return false;
        }

        public virtual void SetBasePos(int x, int y, int a)
        {
            PosBase.X = x;
            PosBase.Y = y;
            Alignment = a;
        }

        public virtual void SetPos(int offsetX, int offsetY)
        {
            Pos.X = PosBase.X + offsetX;
            Pos.Y = PosBase.Y + offsetY;
            Utils.AlignToScreenEdge(Alignment, ref Pos);
        }
    }

    /// <summary>
    /// TabList：管理一组 Widget 之间基于键盘/手柄的 Tab 键导航焦点。
    /// </summary>
    public class TabList
    {
        public const int WidgetSelectAuto = 0;
        public const int WidgetSelectLeft = 1;
        public const int WidgetSelectRight = 2;
        public const int WidgetSelectUp = 3;
        public const int WidgetSelectDown = 4;

        public const byte NavAlignCenter = 0;
        public const byte NavAlignLeft = 1;
        public const byte NavAlignRight = 2;

        public const bool GetInner = true;

        private List<Widget> _widgets = new List<Widget>();
        private int _current = -1;
        private int _previous = -1;
        private bool _locked;
        private byte _scrolltype;
        private int _mvLeft;
        private int _mvRight;
        private int _activate;
        private TabList? _prevTablist;
        private TabList? _nextTablist;
        private readonly Timer _scrollTimer = new Timer();

        public bool EnableActivate; // when disabled, Input.Accept won't trigger Activate()
        public bool IsInnerTablist;

        public TabList()
        {
            _current = -1;
            _previous = -1;
            _locked = false;
            _scrolltype = Widget.ScrollTwoDirections;
            // InputState/Input 是尚未转换的未来依赖单元，此处按预期的最终符号命名引用。
            _mvLeft = Input.Left;
            _mvRight = Input.Right;
            _activate = Input.Accept;
            _prevTablist = null;
            _nextTablist = null;
            EnableActivate = true;
            IsInnerTablist = false;

            _scrollTimer.Duration = (uint)(SharedResources.Settings!.MaxFramesPerSec / 4);
        }

        /// <summary>对应 C++ TabList 指针的布尔语义（非空即为真）。</summary>
        public static implicit operator bool(TabList? tabList) => tabList != null;

        public void Lock()
        {
            _locked = true;
            if (CurrentIsValid())
                _widgets[_current].Defocus();
        }

        public void Unlock()
        {
            _locked = false;
            if (CurrentIsValid())
                _widgets[_current].InFocus = true;
        }

        public void Add(Widget? widget)
        {
            if (widget == null)
                return;

            if (!_widgets.Contains(widget))
                _widgets.Add(widget);
        }

        public void Remove(Widget? widget)
        {
            if (widget != null)
                _widgets.Remove(widget);
        }

        public void Clear()
        {
            _widgets.Clear();
            _current = -1;
        }

        public void SetCurrent(Widget? widget)
        {
            if (widget == null)
            {
                _current = -1;
                return;
            }

            for (int i = 0; i < _widgets.Count; ++i)
            {
                if (_widgets[i] == widget)
                {
                    _current = i;
                    _widgets[i].InFocus = true;
                }
                else
                {
                    _widgets[i].Defocus();
                }
            }
        }

        public int GetCurrent()
        {
            return _current;
        }

        public Widget? GetWidgetByIndex(int index)
        {
            if (index >= 0 && index < _widgets.Count)
            {
                return _widgets[index];
            }
            else
            {
                return null;
            }
        }

        public uint Size()
        {
            return (uint)_widgets.Count;
        }

        private bool CurrentIsValid()
        {
            return _current >= 0 && _current < _widgets.Count;
        }

        private bool PreviousIsValid()
        {
            return _previous >= 0 && _previous < _widgets.Count;
        }

        public Widget? GetNext(bool inner, int dir)
        {
            if (_widgets.Count == 0)
            {
                if (_nextTablist != null && _nextTablist.Size() > 0)
                {
                    Defocus();
                    _locked = true;
                    _nextTablist.Unlock();
                    return _nextTablist.GetNext(!GetInner, WidgetSelectAuto);
                }
                else if (_prevTablist != null && _prevTablist.Size() > 0)
                {
                    Defocus();
                    _locked = true;
                    _prevTablist.Unlock();
                    return _prevTablist.GetPrev(!GetInner, WidgetSelectAuto);
                }
                return null;
            }

            if (CurrentIsValid())
            {
                if (inner && _widgets[_current].GetNext())
                    return null;

                _widgets[_current].Defocus();
            }

            int next = -1;
            if (dir == WidgetSelectAuto)
            {
                next = GetNextIndex();
                if (next != -1)
                    _current = next;
            }
            else
            {
                next = GetNextRelativeIndex(dir);
                if (next != -1)
                    _current = next;
                else
                {
                    if (_nextTablist == null)
                    {
                        next = GetNextIndex();
                        if (next != -1)
                            _current = next;
                    }
                    else
                    {
                        Defocus();
                        _locked = true;
                        _nextTablist.Unlock();
                        return _nextTablist.GetNext(!GetInner, WidgetSelectAuto);
                    }
                }
            }

            _widgets[_current].InFocus = true;
            return _widgets[_current];
        }

        public Widget? GetPrev(bool inner, int dir)
        {
            if (_widgets.Count == 0)
            {
                if (_prevTablist != null && _prevTablist.Size() > 0)
                {
                    Defocus();
                    _locked = true;
                    _prevTablist.Unlock();
                    return _prevTablist.GetPrev(!GetInner, WidgetSelectAuto);
                }
                else if (_nextTablist != null && _nextTablist.Size() > 0)
                {
                    Defocus();
                    _locked = true;
                    _nextTablist.Unlock();
                    return _nextTablist.GetNext(!GetInner, WidgetSelectAuto);
                }
                return null;
            }

            if (CurrentIsValid())
            {
                if (inner && _widgets[_current].GetPrev())
                    return null;

                _widgets[_current].Defocus();
            }

            int next = -1;
            if (_current == -1)
            {
                next = GetNextIndex();
                if (next != -1)
                    _current = next;
            }
            else if (dir == WidgetSelectAuto)
            {
                next = GetPrevIndex();
                if (next != -1)
                    _current = next;
            }
            else
            {
                next = GetNextRelativeIndex(dir);
                if (next != -1)
                    _current = next;
                else
                {
                    if (_prevTablist == null)
                    {
                        next = GetPrevIndex();
                        if (next != -1)
                            _current = next;
                    }
                    else
                    {
                        Defocus();
                        _locked = true;
                        _prevTablist.Unlock();
                        return _prevTablist.GetPrev(!GetInner, WidgetSelectAuto);
                    }
                }
            }

            _widgets[_current].InFocus = true;
            return _widgets[_current];
        }

        public int GetNextIndex()
        {
            int nextWidget = -1;

            for (int i = _current + 1; i < _widgets.Count; ++i)
            {
                if (_widgets[i].EnableTablistNav)
                {
                    nextWidget = i;
                    break;
                }
            }

            if (nextWidget == -1 && _current >= 0)
            {
                for (int i = 0; i < _current; ++i)
                {
                    if (_widgets[i].EnableTablistNav)
                    {
                        nextWidget = i;
                        break;
                    }
                }
            }

            return nextWidget;
        }

        public int GetPrevIndex()
        {
            if (_current == -1)
                return GetNextIndex();

            int prevWidget = -1;

            for (int i = _current; i > 0; --i)
            {
                if (_widgets[i - 1].EnableTablistNav)
                {
                    prevWidget = i - 1;
                    break;
                }
            }

            if (prevWidget == -1)
            {
                for (int i = _widgets.Count - 1; i > _current; --i)
                {
                    if (_widgets[i].EnableTablistNav)
                    {
                        prevWidget = i;
                        break;
                    }
                }
            }

            return prevWidget;
        }

        /// <summary>
        /// dir 使用 int 承载，语义对应原始 uint8_t 方向常量（WIDGET_SELECT_*）。
        /// </summary>
        public int GetNextRelativeIndex(int dir)
        {
            if (_current == -1 || _current >= _widgets.Count)
                return -1;

            int next = _current;
            float minDistance = -1;

            for (int i = 0; i < _widgets.Count; ++i)
            {
                if (_current == i)
                    continue;

                if (!_widgets[i].EnableTablistNav)
                    continue;

                Rectangle cPos = _widgets[_current].Pos;
                Rectangle iPos = _widgets[i].Pos;

                Vector2 p1 = new Vector2(cPos.X, cPos.Y + cPos.Height / 2f);
                Vector2 p2 = new Vector2(iPos.X, iPos.Y + iPos.Height / 2f);
                if (_widgets[i].TablistNavAlign == NavAlignCenter)
                {
                    p1.X += cPos.Width / 2f;
                    p2.X += iPos.Width / 2f;
                }
                else if (_widgets[i].TablistNavAlign == NavAlignRight)
                {
                    p1.X += cPos.Width;
                    p2.X += iPos.Width;
                }

                if (dir == WidgetSelectLeft && p1.X <= p2.X)
                    continue;
                else if (dir == WidgetSelectRight && p1.X >= p2.X)
                    continue;
                else if (dir == WidgetSelectUp && p1.Y <= p2.Y)
                    continue;
                else if (dir == WidgetSelectDown && p1.Y >= p2.Y)
                    continue;

                float dist = 0;
                float baseDist = Utils.CalcDist(p1, p2);
                float weight = 1.5f;
                if (dir == WidgetSelectRight || dir == WidgetSelectLeft)
                    dist = baseDist + (MathF.Abs(MathF.Abs(p1.Y) - MathF.Abs(p2.Y)) * weight);
                else if (dir == WidgetSelectUp || dir == WidgetSelectDown)
                    dist = baseDist + (MathF.Abs(MathF.Abs(p1.X) - MathF.Abs(p2.X)) * weight);

                if (minDistance == -1 || dist < minDistance)
                {
                    minDistance = dist;
                    next = i;
                }
            }

            if (next == _current)
            {
                // if we're not linked to any other tablists, try wrapping from the screen edges
                if (!IsInnerTablist && _nextTablist == null && _prevTablist == null)
                {
                    next = _current;
                    minDistance = -1;

                    for (int i = 0; i < _widgets.Count; ++i)
                    {
                        if (_current == i)
                            continue;

                        if (!_widgets[i].EnableTablistNav)
                            continue;

                        Rectangle cPos = _widgets[_current].Pos;
                        Rectangle iPos = _widgets[i].Pos;

                        if (dir == WidgetSelectLeft)
                            cPos.X = SharedResources.Settings!.ViewW;
                        else if (dir == WidgetSelectRight)
                            cPos.X = 0;
                        else if (dir == WidgetSelectUp)
                            cPos.Y = SharedResources.Settings!.ViewH;
                        else if (dir == WidgetSelectDown)
                            cPos.Y = 0;

                        Vector2 p1 = new Vector2(cPos.X, cPos.Y + cPos.Height / 2f);
                        Vector2 p2 = new Vector2(iPos.X, iPos.Y + iPos.Height / 2f);
                        if (_widgets[i].TablistNavAlign == NavAlignCenter)
                        {
                            p1.X += cPos.Width / 2f;
                            p2.X += iPos.Width / 2f;
                        }
                        else if (_widgets[i].TablistNavAlign == NavAlignRight)
                        {
                            p1.X += cPos.Width;
                            p2.X += iPos.Width;
                        }

                        float dist = Utils.CalcDist(p1, p2);

                        if (minDistance == -1 || dist < minDistance)
                        {
                            minDistance = dist;
                            next = i;
                        }
                    }
                }

                if (next == _current)
                    return -1;
            }

            return next;
        }

        public void DeactivatePrevious()
        {
            if (PreviousIsValid() && _previous != _current)
            {
                _widgets[_previous].Deactivate();
            }
        }

        public void Activate()
        {
            if (CurrentIsValid())
            {
                _widgets[_current].Activate();
                _previous = _current;
            }
        }

        public void Defocus()
        {
            for (int i = 0; i < _widgets.Count; ++i)
            {
                _widgets[i].Defocus();
            }

            _current = -1;
        }

        public void SetPrevTabList(TabList? tl)
        {
            _prevTablist = tl;
        }

        public void SetNextTabList(TabList? tl)
        {
            _nextTablist = tl;
        }

        public void SetScrollType(byte scrolltype)
        {
            _scrolltype = scrolltype;
        }

        public bool IsLocked()
        {
            return _locked;
        }

        public void Logic()
        {
            if (_locked) return;

            // InputState 是尚未转换的未来依赖单元，此处按预期的最终符号命名引用。
            InputState inpt = SharedResources.Inpt!;

            if (!inpt.UsingMouse())
            {
                byte innerScrolltype = Widget.ScrollVertical;

                if (CurrentIsValid() && _widgets[_current].ScrollType != Widget.ScrollTwoDirections)
                {
                    innerScrolltype = _widgets[_current].ScrollType;
                }

                if (_scrolltype == Widget.ScrollVertical || _scrolltype == Widget.ScrollTwoDirections)
                {
                    if (inpt.Pressing[Input.Down] && (!inpt.Lock[Input.Down] || _scrollTimer.IsEnd()))
                    {
                        inpt.Lock[Input.Down] = true;
                        _scrollTimer.Reset(Timer.Begin);

                        if (innerScrolltype == Widget.ScrollVertical)
                            GetNext(GetInner, WidgetSelectDown);
                        else if (innerScrolltype == Widget.ScrollHorizontal)
                            GetNext(!GetInner, WidgetSelectDown);
                    }
                    else if (inpt.Pressing[Input.Up] && (!inpt.Lock[Input.Up] || _scrollTimer.IsEnd()))
                    {
                        inpt.Lock[Input.Up] = true;
                        _scrollTimer.Reset(Timer.Begin);

                        if (innerScrolltype == Widget.ScrollVertical)
                            GetPrev(GetInner, WidgetSelectUp);
                        else if (innerScrolltype == Widget.ScrollHorizontal)
                            GetPrev(!GetInner, WidgetSelectUp);
                    }
                }

                if (_scrolltype == Widget.ScrollHorizontal || _scrolltype == Widget.ScrollTwoDirections)
                {
                    if (inpt.Pressing[_mvLeft] && (!inpt.Lock[_mvLeft] || _scrollTimer.IsEnd()))
                    {
                        inpt.Lock[_mvLeft] = true;
                        _scrollTimer.Reset(Timer.Begin);

                        if (innerScrolltype == Widget.ScrollVertical)
                            GetPrev(!GetInner, WidgetSelectLeft);
                        else if (innerScrolltype == Widget.ScrollHorizontal)
                            GetPrev(GetInner, WidgetSelectLeft);
                    }
                    else if (inpt.Pressing[_mvRight] && (!inpt.Lock[_mvRight] || _scrollTimer.IsEnd()))
                    {
                        inpt.Lock[_mvRight] = true;
                        _scrollTimer.Reset(Timer.Begin);

                        if (innerScrolltype == Widget.ScrollVertical)
                            GetNext(!GetInner, WidgetSelectRight);
                        else if (innerScrolltype == Widget.ScrollHorizontal)
                            GetNext(GetInner, WidgetSelectRight);
                    }
                }

                if (inpt.Pressing[_activate] && !inpt.Lock[_activate] && EnableActivate)
                {
                    inpt.Lock[_activate] = true;
                    DeactivatePrevious(); // Deactivate previously activated item
                    Activate(); // Activate the currently infocus item
                }
            }

            // If mouse is clicked, defocus current tabindex item
            if (inpt.UsingMouse() && inpt.Pressing[Input.Main1] && !inpt.Lock[Input.Main1] && CurrentIsValid() && !Utils.IsWithinRect(_widgets[GetCurrent()].Pos, inpt.Mouse))
            {
                Defocus();
            }

            // Also defocus if we start using the mouse
            // we need to disable this for touchscreen devices so that item tooltips will work
            if (!inpt.UsingTouchscreen() && _current != -1 && inpt.UsingMouse())
            {
                Defocus();
            }

            if (IsInnerTablist)
                _scrollTimer.Reset(Timer.Begin);
            else
                _scrollTimer.Tick();

            if (!inpt.Pressing[_mvLeft] && !inpt.Pressing[_mvRight] && !inpt.Pressing[Input.Up] && !inpt.Pressing[Input.Down])
                _scrollTimer.Reset(Timer.Begin);
        }

        public void SetHorizontalKeys(int leftKey, int rightKey)
        {
            _mvLeft = leftKey;
            _mvRight = rightKey;
        }

        public void SetActivateKey(int activateKey)
        {
            _activate = activateKey;
        }
    }
}
