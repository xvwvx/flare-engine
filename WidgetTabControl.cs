// <自动生成> 对应 C++ 源文件：WidgetTabControl.h + WidgetTabControl.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// WidgetTabControl
    ///
    /// 带标题栏的标签页控件：管理多个 TabList，支持鼠标点击切换、键盘翻页与溢出时的左右导航按钮。
    /// 持有的 <see cref="Sprite"/>、<see cref="WidgetButton"/> 以及 <see cref="WidgetLabel"/> 缓存资源
    /// 通过 <see cref="IDisposable"/> 显式释放，释放顺序与原始析构函数 <c>~WidgetTabControl()</c> 一致。
    /// <see cref="WidgetButton"/> 是尚未转换的前向依赖单元，本类按预期的最终符号命名引用其 API。
    /// </summary>
    public class WidgetTabControl : Widget, IDisposable
    {
        private Sprite? _activeTabSurface;
        private Sprite? _inactiveTabSurface;

        private readonly List<string> _titles = new List<string>(); // Titles of the tabs.
        private readonly List<Rectangle> _tabs = new List<Rectangle>(); // Rectangles for each tab title on the tab header.
        private readonly List<WidgetLabel> _activeLabels = new List<WidgetLabel>();
        private readonly List<WidgetLabel> _inactiveLabels = new List<WidgetLabel>();
        private readonly List<bool> _enabled = new List<bool>();
        private readonly List<TabList?> _tablists = new List<TabList?>();

        private uint _activeTab;    // Index of the currently active tab.
        private Rectangle _tabsArea;    // Area the tab titles are displayed.
        private bool _lockMain1;
        private bool _dragging;

        private WidgetButton? _buttonPrev;
        private WidgetButton? _buttonNext;

        private bool _showButtons;

        private SoundID _soundActivate;

        public WidgetTabControl()
        {
            _activeTabSurface = null;
            _inactiveTabSurface = null;
            _activeTab = 0;
            _lockMain1 = false;
            _dragging = false;
            _buttonPrev = new WidgetButton(WidgetButton.DirLeftFile);
            _buttonNext = new WidgetButton(WidgetButton.DirRightFile);
            _showButtons = false;
            _soundActivate = 0;

            LoadGraphics();

            ScrollType = ScrollHorizontal;

            if (SharedResources.Eset!.Widgets.SoundActivate.Length != 0)
                _soundActivate = SharedResources.Snd!.Load(SharedResources.Eset.Widgets.SoundActivate, "Widget activate");
        }

        /// <summary>
        /// 对应 C++ 的 <c>~WidgetTabControl()</c>，释放缓存资源并卸载音效。
        /// </summary>
        public void Dispose()
        {
            _activeTabSurface?.Dispose();
            _activeTabSurface = null;
            _inactiveTabSurface?.Dispose();
            _inactiveTabSurface = null;

            _buttonPrev?.Dispose();
            _buttonPrev = null;
            _buttonNext?.Dispose();
            _buttonNext = null;

            SharedResources.Snd!.Unload(_soundActivate);

            for (int i = 0; i < _activeLabels.Count; ++i)
            {
                _activeLabels[i].Dispose();
            }
            for (int i = 0; i < _inactiveLabels.Count; ++i)
            {
                _inactiveLabels[i].Dispose();
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Initialize a tab at a given index. A new tab will be allocated if it does not exist.
        /// </summary>
        public void SetupTab(uint index, string title, TabList? tl)
        {
            if (index + 1 > _titles.Count)
            {
                int newSize = (int)(index + 1);
                while (_titles.Count < newSize)
                    _titles.Add("");
                while (_tabs.Count < newSize)
                    _tabs.Add(new Rectangle());
                while (_activeLabels.Count < newSize)
                    _activeLabels.Add(new WidgetLabel());
                while (_inactiveLabels.Count < newSize)
                    _inactiveLabels.Add(new WidgetLabel());
                while (_enabled.Count < newSize)
                    _enabled.Add(false);
                while (_tablists.Count < newSize)
                    _tablists.Add(null);
            }

            _titles[(int)index] = title;
            _enabled[(int)index] = true;
            _tablists[(int)index] = tl;
        }

        /// <summary>
        /// Returns the index of the open tab.
        ///
        /// For example, if the first tab is open, it will return 0.
        /// </summary>
        public int GetActiveTab()
        {
            return (int)_activeTab;
        }

        /// <summary>
        /// Sets the active tab to a given index.
        /// </summary>
        public void SetActiveTab(uint tab)
        {
            if (tab > _tabs.Count)
                tab = 0;
            else if (tab == (uint)_tabs.Count)
                tab = (uint)(_tabs.Count - 1);

            // Set the tab. If the specified tab is not enabled, get the first enabled tab.
            bool foundTab = false;
            for (uint i = tab; i < (uint)_tabs.Count; ++i)
            {
                if (_enabled[(int)i])
                {
                    _activeTab = i;
                    foundTab = true;
                    break;
                }
            }
            if (!foundTab)
            {
                for (uint i = 0; i < tab; ++i)
                {
                    if (_enabled[(int)i])
                    {
                        _activeTab = i;
                        foundTab = true;
                        break;
                    }
                }
            }

            if (!foundTab)
            {
                // no enabled tabs, just return what we started with
                _activeTab = tab;
            }

            for (uint i = 0; i < (uint)_tabs.Count; ++i)
            {
                if (_tablists[(int)i] != null && i != _activeTab)
                {
                    _tablists[(int)i]!.Lock();
                    _tablists[(int)i]!.Defocus();
                }
            }

            if (_tablists[(int)_activeTab] != null)
            {
                _tablists[(int)_activeTab]!.Unlock();
            }
        }

        /// <summary>
        /// Define the position and size of the tab control area and the child tabs (including text).
        /// Typically called when running window/menu align() rountines.
        ///
        /// @param x       X coordinate of the top-left corner of the widget.
        /// @param y       Y coordinate of the top-left corner of the widget.
        /// @param w       The maximum width of the area allowed for tabs. If this is exceeded, a single tab is show with navigation buttons instead.
        /// </summary>
        public void SetMainArea(int x, int y, int w)
        {
            // Set tabs area.
            _tabsArea.X = x;
            _tabsArea.Y = y;
            _tabsArea.Width = 0;
            _tabsArea.Height = GetTabHeight();

            _showButtons = false;

            int xOffset = _tabsArea.X;

            // update individual tabs
            for (int i = 0; i < _tabs.Count; ++i)
            {
                Rectangle tabRect = _tabs[i];
                tabRect.Y = _tabsArea.Y;
                tabRect.Height = _tabsArea.Height;

                tabRect.X = xOffset;

                _activeLabels[i].SetPos(tabRect.X + SharedResources.Eset!.Widgets.TabPadding.X + SharedResources.Eset.Widgets.TabTextPadding, tabRect.Y + tabRect.Height / 2 + SharedResources.Eset.Widgets.TabPadding.Y);
                _activeLabels[i].SetVAlign(LabelInfo.ValignCenter);
                _activeLabels[i].SetText(_titles[i]);
                _activeLabels[i].SetColor(SharedResources.Font!.GetColor(FontEngine.ColorWidgetNormal));

                _inactiveLabels[i].SetPos(tabRect.X + SharedResources.Eset.Widgets.TabPadding.X + SharedResources.Eset.Widgets.TabTextPadding, tabRect.Y + tabRect.Height / 2 + SharedResources.Eset.Widgets.TabPadding.Y);
                _inactiveLabels[i].SetVAlign(LabelInfo.ValignCenter);
                _inactiveLabels[i].SetText(_titles[i]);
                _inactiveLabels[i].SetColor(SharedResources.Font.GetColor(FontEngine.ColorWidgetDisabled));

                if (_enabled[i])
                {
                    tabRect.Width = _activeLabels[i].GetBounds().Width + (SharedResources.Eset.Widgets.TabPadding.X * 2) + (SharedResources.Eset.Widgets.TabTextPadding * 2);
                    _tabsArea.Width += tabRect.Width;
                    xOffset += tabRect.Width;
                }

                _tabs[i] = tabRect;
            }

            if (_tabsArea.Width > w || _showButtons)
            {
                _showButtons = true;

                int betweenButtons = w - _buttonPrev!.Pos.Width - _buttonNext!.Pos.Width;

                // only one tab will be shown at a time, so center all the tabs between the buttons
                for (int i = 0; i < _tabs.Count; ++i)
                {
                    Rectangle tabRect = _tabs[i];
                    tabRect.X = _tabsArea.X + _buttonPrev.Pos.Width + ((betweenButtons - tabRect.Width) / 2);
                    _activeLabels[i].SetPos(tabRect.X + SharedResources.Eset.Widgets.TabPadding.X + SharedResources.Eset.Widgets.TabTextPadding, tabRect.Y + tabRect.Height / 2 + SharedResources.Eset.Widgets.TabPadding.Y);
                    _inactiveLabels[i].SetPos(tabRect.X + SharedResources.Eset.Widgets.TabPadding.X + SharedResources.Eset.Widgets.TabTextPadding, tabRect.Y + tabRect.Height / 2 + SharedResources.Eset.Widgets.TabPadding.Y);
                    _tabs[i] = tabRect;
                }
            }

            if (!_enabled[(int)_activeTab])
                GetNext();

            int buttonYOffset = _tabsArea.Y + ((_tabsArea.Height - _buttonPrev.Pos.Height) / 2);
            _buttonPrev.SetPos(_tabsArea.X, buttonYOffset);
            _buttonNext.SetPos(_tabsArea.X + w - _buttonNext.Pos.Width, buttonYOffset);
        }

        /// <summary>
        /// Load the graphics for the control.
        /// </summary>
        private void LoadGraphics()
        {
            Image? graphics;
            graphics = SharedResources.RenderDevice!.LoadImage("images/menus/tab_active.png", RenderDevice.ErrorExit);
            if (graphics != null)
            {
                _activeTabSurface = graphics.CreateSprite();
                graphics.Unref();
            }

            graphics = SharedResources.RenderDevice.LoadImage("images/menus/tab_inactive.png", RenderDevice.ErrorExit);
            if (graphics != null)
            {
                _inactiveTabSurface = graphics.CreateSprite();
                graphics.Unref();
            }
        }

        public void Logic()
        {
            Logic(SharedResources.Inpt!.Mouse.X, SharedResources.Inpt.Mouse.Y);
        }

        /// <summary>
        /// Performs one frame of logic.
        ///
        /// It basically checks if it was clicked on the header, and if so changes the active tab.
        /// </summary>
        public void Logic(int x, int y)
        {
            Int2 mouse = new Int2(x, y);
            if (_showButtons)
            {
                if (_enabled[0])
                {
                    _buttonPrev!.Enabled = (_activeTab > 0);
                }
                else
                {
                    _buttonPrev!.Enabled = (_activeTab > GetNextEnabledTab(0));
                }

                uint endTab = (uint)(_tabs.Count - 1);
                if (_enabled[(int)endTab])
                {
                    _buttonNext!.Enabled = (_activeTab < endTab);
                }
                else
                {
                    _buttonNext!.Enabled = (_activeTab < GetPrevEnabledTab(endTab));
                }

                if (_buttonPrev.CheckClickAt(mouse.X, mouse.Y))
                {
                    GetPrev();
                }
                else if (_buttonNext.CheckClickAt(mouse.X, mouse.Y))
                {
                    GetNext();
                }
            }
            else
            {
                // If the click was in the tabs area;
                if (Utils.IsWithinRect(_tabsArea, mouse) && (!_lockMain1 || _dragging))
                {
                    _lockMain1 = false;
                    _dragging = false;

                    InputState inpt = SharedResources.Inpt!;
                    if (inpt.Pressing[Input.Main1])
                    {
                        inpt.Lock[Input.Main1] = true;

                        _dragging = true;

                        // Mark the clicked tab as active_tab.
                        for (uint i = 0; i < (uint)_tabs.Count; i++)
                        {
                            if (Utils.IsWithinRect(_tabs[(int)i], mouse) && _enabled[(int)i])
                            {
                                if (_activeTab != i)
                                    SharedResources.Snd!.Play(_soundActivate, "widget_activate", SoundManager.NoPos, !SoundManager.Loop);

                                _activeTab = i;
                                SetActiveTab(i);
                                break;
                                // return;
                            }
                        }
                    }
                }
                else
                {
                    _lockMain1 = SharedResources.Inpt!.Pressing[Input.Main1];
                }
                if (!SharedResources.Inpt.Pressing[Input.Main1])
                {
                    _dragging = false;
                }
            }

            if (_tablists[(int)_activeTab] != null && _tablists[(int)_activeTab]!.GetCurrent() != -1)
            {
                InputState inpt = SharedResources.Inpt!;
                if (inpt.Pressing[Input.MenuPageNext] && !inpt.Lock[Input.MenuPageNext] && _activeTab < (uint)_tabs.Count)
                {
                    for (uint i = _activeTab + 1; i < (uint)_tabs.Count; ++i)
                    {
                        if (_enabled[(int)i] && _tablists[(int)i] != null)
                        {
                            inpt.Lock[Input.MenuPageNext] = true;
                            _tablists[(int)_activeTab]!.Defocus();
                            _tablists[(int)_activeTab]!.Lock();
                            _tablists[(int)i]!.Unlock();
                            _tablists[(int)i]!.GetNext(!TabList.GetInner, TabList.WidgetSelectAuto);
                            _activeTab = i;
                            break;
                        }
                    }
                }
                else if (inpt.Pressing[Input.MenuPagePrev] && !inpt.Lock[Input.MenuPagePrev] && _activeTab > 0)
                {
                    for (uint i = _activeTab; i > 0; --i)
                    {
                        if (_enabled[(int)(i - 1)] && _tablists[(int)(i - 1)] != null)
                        {
                            inpt.Lock[Input.MenuPagePrev] = true;
                            _tablists[(int)_activeTab]!.Defocus();
                            _tablists[(int)_activeTab]!.Lock();
                            _tablists[(int)(i - 1)]!.Unlock();
                            _tablists[(int)(i - 1)]!.GetPrev(!TabList.GetInner, TabList.WidgetSelectAuto);
                            _activeTab = i - 1;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Renders the widget.
        ///
        /// Remember to render then on top of it the actual content of the {@link getActiveTab() active tab}.
        /// </summary>
        public override void Render()
        {
            for (uint i = 0; i < (uint)_tabs.Count; i++)
            {
                RenderTab(i);
            }

            if (_showButtons)
            {
                _buttonPrev!.Render();
                _buttonNext!.Render();
            }

            // draw selection rectangle
            if (InFocus)
            {
                Int2 topLeft;
                Int2 bottomRight;

                topLeft.X = _tabs[(int)_activeTab].X;
                topLeft.Y = _tabs[(int)_activeTab].Y;
                bottomRight.X = topLeft.X + _tabs[(int)_activeTab].Width;
                bottomRight.Y = topLeft.Y + _tabs[(int)_activeTab].Height;

                SharedResources.RenderDevice!.DrawRectangleCorners(SharedResources.Eset!.Widgets.SelectionRectCornerSize, topLeft, bottomRight, SharedResources.Eset.Widgets.SelectionRectColor);
            }
        }

        /// <summary>
        /// Renders the given tab on the widget header.
        /// </summary>
        private void RenderTab(uint number)
        {
            if (!_enabled[(int)number] || (_showButtons && number != _activeTab))
                return;

            uint i = number;
            Rectangle src = default;
            Rectangle dest = default;

            // Draw tab's background.
            int gfxWidth = _activeTabSurface!.GetGraphicsWidth();
            int widthToRender = _tabs[(int)i].Width - SharedResources.Eset!.Widgets.TabPadding.X; // don't draw the right edge yet
            int renderCursor = 0;

            src.Y = 0;
            src.Height = _tabs[(int)i].Height;
            dest.Y = _tabs[(int)i].Y;

            // repeat the middle part of the image for long tabs
            // src.x and dest.x are assigned here
            while (renderCursor < widthToRender)
            {
                dest.X = _tabs[(int)i].X + renderCursor;
                if (renderCursor == 0)
                {
                    // left edge + middle
                    src.X = 0;
                    src.Width = _tabs[(int)i].Width - SharedResources.Eset.Widgets.TabPadding.X;

                    if (src.Width > gfxWidth - SharedResources.Eset.Widgets.TabPadding.X)
                        src.Width = gfxWidth - SharedResources.Eset.Widgets.TabPadding.X;
                }
                else
                {
                    // only middle
                    src.X = SharedResources.Eset.Widgets.TabPadding.X;
                    src.Width = _tabs[(int)i].Width - (SharedResources.Eset.Widgets.TabPadding.X * 2);

                    if (src.Width > gfxWidth - (SharedResources.Eset.Widgets.TabPadding.X * 2))
                        src.Width = gfxWidth - (SharedResources.Eset.Widgets.TabPadding.X * 2);
                }

                renderCursor += src.Width;

                if (renderCursor > _tabs[(int)i].Width)
                    src.Width = _tabs[(int)i].Width - (renderCursor - src.Width);

                if (i == _activeTab)
                {
                    _activeTabSurface.SetClipFromRect(src);
                    _activeTabSurface.SetDestFromRect(dest);
                    SharedResources.RenderDevice!.Render(_activeTabSurface);
                }
                else
                {
                    _inactiveTabSurface!.SetClipFromRect(src);
                    _inactiveTabSurface.SetDestFromRect(dest);
                    SharedResources.RenderDevice.Render(_inactiveTabSurface);
                }
            }

            // Draw tab's right edge.
            src.X = _activeTabSurface.GetGraphicsWidth() - SharedResources.Eset.Widgets.TabPadding.X;
            src.Width = SharedResources.Eset.Widgets.TabPadding.X;
            dest.X = _tabs[(int)i].X + _tabs[(int)i].Width - SharedResources.Eset.Widgets.TabPadding.X;

            if (i == _activeTab)
            {
                _activeTabSurface.SetClipFromRect(src);
                _activeTabSurface.SetDestFromRect(dest);
                SharedResources.RenderDevice.Render(_activeTabSurface);
            }
            else
            {
                _inactiveTabSurface!.SetClipFromRect(src);
                _inactiveTabSurface.SetDestFromRect(dest);
                SharedResources.RenderDevice.Render(_inactiveTabSurface);
            }

            // Render labels
            if (i == _activeTab)
            {
                _activeLabels[(int)i].Render();
            }
            else
            {
                _inactiveLabels[(int)i].Render();
            }
        }

        public override bool GetNext()
        {
            SetActiveTab(GetNextEnabledTab(_activeTab));
            return true;
        }

        public override bool GetPrev()
        {
            SetActiveTab(GetPrevEnabledTab(_activeTab));
            return true;
        }

        private uint GetNextEnabledTab(uint tab)
        {
            for (uint i = tab + 1; i < (uint)_tabs.Count; ++i)
            {
                if (_enabled[(int)i])
                    return i;
            }
            return tab;
        }

        private uint GetPrevEnabledTab(uint tab)
        {
            for (uint i = tab - 1; i < (uint)_tabs.Count; i--)
            {
                if (_enabled[(int)i])
                    return i;
            }
            return tab;
        }

        public int GetTabHeight()
        {
            return (_activeTabSurface != null ? _activeTabSurface.GetGraphicsHeight() : 0);
        }

        public void SetEnabled(uint index, bool val)
        {
            if (index >= (uint)_enabled.Count)
                return;

            _enabled[(int)index] = val;
        }
    }
}
