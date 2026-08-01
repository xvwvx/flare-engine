// 对应 C++ 源文件：MenuLog.h + MenuLog.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// MenuLog
    ///
    /// 任务/消息日志菜单，含 Quests 与 Notes 两个标签页，分别绑定独立的 <see cref="WidgetLog"/> 实例。
    /// 持有的 <see cref="WidgetLog"/>、<see cref="WidgetButton"/>、<see cref="WidgetTabControl"/>、
    /// <see cref="WidgetLabel"/> 等资源通过 <see cref="IDisposable"/> 显式释放，
    /// 释放顺序与原始析构函数 <c>~MenuLog()</c> 一致（先各 <c>log[i]</c>，再 closeButton、tabControl，
    /// 然后 label_log 成员析构，最后基类 <see cref="Menu"/>）。
    /// </summary>
    public class MenuLog : Menu, IDisposable
    {
        /// <summary>对应匿名枚举 <c>TYPE_QUESTS</c>。</summary>
        public const int TypeQuests = 0;

        /// <summary>对应匿名枚举 <c>TYPE_MESSAGES</c>。</summary>
        public const int TypeMessages = 1;

        /// <summary>对应 <c>static const int TYPE_COUNT = 2</c>。</summary>
        public const int TypeCount = 2;

        /// <summary>对应 C++ 原始公有字段 <c>tablist_log</c>。</summary>
        public List<TabList> TablistLog = new List<TabList>();

        private readonly WidgetLabel _labelLog = new WidgetLabel();
        private WidgetButton? _closeButton;
        private WidgetTabControl? _tabControl;

        private readonly WidgetLog?[] _log = new WidgetLog?[TypeCount];
        private readonly string[] _tabLabels = new string[TypeCount];
        private readonly Rectangle[] _tabRect = new Rectangle[TypeCount];

        private Rectangle _tabArea;
        private Color _tabBg;

        public MenuLog()
        {
            Visible = false;

            _closeButton = new WidgetButton(WidgetButton.CloseFile);

            // Load config settings
            using FileParser infile = new FileParser();
            // @CLASS MenuLog|Description of menus/log.txt
            if (infile.Open("menus/log.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (ParseMenuKey(infile.Key, infile.Val))
                        continue;

                    // @ATTR label_title|label|Position of the "Log" text.
                    if (infile.Key == "label_title")
                    {
                        _labelLog.SetFromLabelInfo(Parse.PopLabelInfo(infile.Val));
                    }
                    // @ATTR close|point|Position of the close button.
                    else if (infile.Key == "close")
                    {
                        Int2 pos = Parse.ToPoint(infile.Val);
                        _closeButton!.SetBasePos(pos.X, pos.Y, Utils.AlignTopLeft);
                    }
                    // @ATTR tab_area|rectangle|The position of the row of tabs, followed by the dimensions of the log text area.
                    else if (infile.Key == "tab_area")
                    {
                        _tabArea = Parse.ToRect(infile.Val);
                    }
                    else
                    {
                        infile.Error("MenuLog: '%s' is not a valid key.", infile.Key);
                    }
                }
                infile.Close();
            }

            var msg = SharedResources.Msg!;
            var font = SharedResources.Font!;

            _labelLog.SetText(msg.Get("Log"));
            _labelLog.SetColor(font.GetColor(FontEngine.ColorMenuNormal));

            // Initialize the tab control.
            _tabControl = new WidgetTabControl();

            // Store the amount of displayed log messages on each log, and the maximum.
            TablistLog = new List<TabList>(TypeCount);
            for (int i = 0; i < TypeCount; ++i)
            {
                TablistLog.Add(new TabList());
            }
            for (int i = 0; i < TypeCount; ++i)
            {
                _log[i] = new WidgetLog(_tabArea.Width, _tabArea.Height);
                _log[i]!.SetBasePos(_tabArea.X, _tabArea.Y + _tabControl!.GetTabHeight(), Utils.AlignTopLeft);

                TablistLog[i].Add(_log[i]!.GetWidget());
                TablistLog[i].SetPrevTabList(Tablist);
                TablistLog[i].Lock();
            }

            // Define the header.
            _tabControl!.SetupTab((uint)TypeQuests, msg.Get("Quests"), TablistLog[TypeQuests]);
            _tabControl.SetupTab((uint)TypeMessages, msg.Get("Notes"), TablistLog[TypeMessages]);

            if (_background == null)
                SetBackground("images/menus/log.png");

            Align();
        }

        /// <summary>
        /// 对应 C++ 的 <c>~MenuLog()</c>：先释放各 log 实例，再释放 closeButton、tabControl、
        /// label_log，最后调用基类 <see cref="Menu.Dispose"/>。
        /// </summary>
        public override void Dispose()
        {
            for (int i = 0; i < TypeCount; ++i)
            {
                _log[i]?.Dispose();
                _log[i] = null;
            }

            _closeButton?.Dispose();
            _closeButton = null;
            _tabControl?.Dispose();
            _tabControl = null;
            _labelLog.Dispose();

            base.Dispose();
        }

        public override void Align()
        {
            base.Align();

            _tabControl!.SetMainArea(WindowArea.X + _tabArea.X, WindowArea.Y + _tabArea.Y, _tabArea.Width);

            _closeButton!.SetPos(WindowArea.X, WindowArea.Y);

            _labelLog.SetPos(WindowArea.X, WindowArea.Y);

            for (int i = 0; i < TypeCount; ++i)
            {
                _log[i]!.SetPos(WindowArea.X, WindowArea.Y);
            }
        }

        /// <summary>
        /// Perform one frame of logic.
        /// </summary>
        public void Logic()
        {
            if (!Visible) return;

            Tablist.Logic();
            // make shure keyboard navigation leads us to correct tab
            for (int i = 0; i < TypeCount; ++i)
            {
                if (_tabControl!.GetActiveTab() == i)
                {
                    Tablist.SetNextTabList(TablistLog[i]);
                }
                TablistLog[i].Logic();
            }

            if (_closeButton!.CheckClick())
            {
                Visible = false;
                SharedResources.Snd!.Play(SfxClose, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);
            }

            _tabControl!.Logic();
            int activeLog = _tabControl.GetActiveTab();

            _log[activeLog]!.Logic();
        }

        /// <summary>
        /// Render graphics for this frame when the menu is open
        /// </summary>
        public override void Render()
        {
            if (!Visible) return;

            // Background.
            base.Render();

            // Close button.
            _closeButton!.Render();

            // Text overlay.
            _labelLog.Render();

            // Tab control.
            _tabControl!.Render();

            // Display latest log messages for the active tab.
            int activeLog = _tabControl.GetActiveTab();
            _log[activeLog]!.Render();
        }

        /// <summary>
        /// Add a new message to the log.
        /// </summary>
        public void Add(string s, int logType, int msgType)
        {
            _log[logType]!.Add(Utils.SubstituteVarsInString(s, SharedGameResources.Pc), msgType);
        }

        public void SetNextColor(Color color, int logType)
        {
            _log[logType]!.SetNextColor(color);
        }

        public void SetNextStyle(int style, int logType)
        {
            _log[logType]!.SetNextStyle(style);
        }

        /// <summary>
        /// Remove log message with the given identifier.
        /// </summary>
        public void Remove(int msgIndex, int logType)
        {
            _log[logType]!.Remove((uint)msgIndex);
        }

        public void Clear(int logType)
        {
            if (logType >= 0 && logType < TypeCount)
            {
                _log[logType]!.Clear();
            }
        }

        public void ClearAll()
        {
            for (int i = 0; i < TypeCount; ++i)
            {
                _log[i]!.Clear();
            }
        }

        public void AddSeparator(int logType)
        {
            _log[logType]!.AddSeparator();
        }

        public void SetNextTabList(TabList? tl)
        {
            for (int i = 0; i < TypeCount; ++i)
            {
                TablistLog[i].SetNextTabList(tl);
            }
        }

        public override TabList? GetCurrentTabList()
        {
            if (Tablist.GetCurrent() != -1)
            {
                return Tablist;
            }
            else
            {
                for (int i = 0; i < TypeCount; ++i)
                {
                    if (TablistLog[i].GetCurrent() != -1)
                        return TablistLog[i];
                }
            }

            return null;
        }

        public override void DefocusTabLists()
        {
            Tablist.Defocus();
            for (int i = 0; i < TypeCount; ++i)
            {
                TablistLog[i].Defocus();
            }
        }

        public TabList? GetVisibleChildTabList()
        {
            return TablistLog[_tabControl!.GetActiveTab()];
        }
    }
}
