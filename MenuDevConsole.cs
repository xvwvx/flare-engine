// 对应 C++ 源文件：MenuDevConsole.h + MenuDevConsole.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using System.Text;
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// MenuDevConsole
    ///
    /// 开发者控制台菜单：支持命令输入、历史滚动、地图/实体检查、快捷命令与 resize 拖拽。
    /// 持有的 <see cref="WidgetButton"/>、<see cref="WidgetInput"/>、<see cref="WidgetLog"/>、
    /// <see cref="Sprite"/>（resize 手柄）通过 <see cref="IDisposable"/> 显式释放，
    /// 释放顺序与原始析构函数 <c>~MenuDevConsole()</c> 一致（先各 widget，再 resize 手柄，最后基类 <see cref="Menu"/>）。
    /// </summary>
    public class MenuDevConsole : Menu, IDisposable
    {
        private WidgetButton? _buttonClose;
        private WidgetInput? _inputBox;
        private WidgetLog? _logHistory;

        private Rectangle _historyArea;

        private bool _firstOpen;

        private int _inputScrollbackPos;
        private List<string> _inputScrollback = new List<string>();

        private int _setShortcutSlot;

        private float _consoleHeight;
        private Rectangle _resizeArea;
        private Sprite? _resizeHandle;
        private bool _draggingResize;

        private int _scrollbarW;

        private string _fontName;
        private string _fontBoldName;
        private Color _backgroundColor;
        private Color _resizeHandleColor;

        /// <summary>对应 C++ 原始公有字段 <c>target</c>（FPoint）。</summary>
        public Vector2 Target;

        /// <summary>对应 C++ 原始公有字段 <c>distance_timer</c>。</summary>
        public Timer DistanceTimer;

        public MenuDevConsole()
        {
            _firstOpen = false;
            _inputScrollbackPos = 0;
            _setShortcutSlot = 0;
            _consoleHeight = 0.4f;
            _resizeHandle = null;
            _draggingResize = false;
            _fontName = "font_regular";
            _fontBoldName = "font_bold";
            _backgroundColor = new Color(0, 0, 0, 200);
            _resizeHandleColor = new Color(255, 255, 255, 63);
            DistanceTimer = new Timer();

            // dorkster: dumb work-around for the fact that I made WidgetScrollBox report dimensions without the scrollbar
            // So we create a temporary scrollbar to get the dimensions we'll need later.
            WidgetScrollBar scrollbar = new WidgetScrollBar(WidgetScrollBar.DefaultFile);
            _scrollbarW = scrollbar.GetBounds().Width;
            scrollbar.Dispose();

            Settings settings = SharedResources.Settings!;
            DistanceTimer.Duration = settings.MaxFramesPerSec;

            _buttonClose = new WidgetButton(WidgetButton.CloseFile);
            Tablist.Add(_buttonClose);

            // Load config settings
            using FileParser infile = new FileParser();
            // @CLASS MenuDevConsole|Description of menus/devconsole.txt
            if (infile.Open("menus/devconsole.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    // @ATTR font|predefined_string|Regular font to use. Defaults to "font_regular".
                    if (infile.Key == "font")
                    {
                        _fontName = infile.Val;
                    }
                    // @ATTR font_bold|predefined_string|Regular font to use. Defaults to "font_bold".
                    else if (infile.Key == "font_bold")
                    {
                        _fontBoldName = infile.Val;
                    }
                    // @ATTR color_background|color, alpha|Background fill color for menu.
                    else if (infile.Key == "color_background")
                    {
                        _backgroundColor = Parse.ToRGBA(infile.Val);
                    }
                    // @ATTR color_resize_handle|color, alpha|Fill color for resize handle at the bottom of the menu.
                    else if (infile.Key == "color_resize_handle")
                    {
                        _resizeHandleColor = Parse.ToRGBA(infile.Val);
                    }

                    else infile.Error("MenuDevConsole: '%s' is not a valid key.", infile.Key);
                }
                infile.Close();
            }

            _inputBox = new WidgetInput(WidgetInput.NoFile);
            _inputBox.SetFontName(_fontName);

            Tablist.Add(_inputBox);

            _logHistory = new WidgetLog(1, 1);
            _logHistory.SetFontNames(_fontName, _fontBoldName);
            Tablist.Add(_logHistory.GetWidget());

            Align();
            Reset();
            _inputBox.AcceptToDefocus = false;
        }

        /// <summary>
        /// 对应 C++ 的 <c>~MenuDevConsole()</c>：先释放各 widget 与 resize 手柄，再调用基类 <see cref="Menu.Dispose"/>。
        /// </summary>
        public override void Dispose()
        {
            _buttonClose?.Dispose();
            _buttonClose = null;
            _inputBox?.Dispose();
            _inputBox = null;
            _logHistory?.Dispose();
            _logHistory = null;
            _resizeHandle?.Dispose();
            _resizeHandle = null;

            base.Dispose();
        }

        public override void Align()
        {
            Settings settings = SharedResources.Settings!;

            WindowArea.X = 0;
            WindowArea.Y = 0;
            WindowArea.Width = settings.ViewW;
            WindowArea.Height = (int)(settings.ViewH * _consoleHeight);

            SetBackgroundColor(_backgroundColor);

            _buttonClose!.SetBasePos(0, 0, Utils.AlignTopRight);
            _buttonClose.SetPos(WindowArea.X, WindowArea.Y);

            _inputBox!.SetBasePos(0, 0, Utils.AlignTopLeft);
            _inputBox.SetPos(WindowArea.X, WindowArea.Y);
            _inputBox.Resize(WindowArea.Width - _buttonClose.Pos.Width);

            int logYOffset = Math.Max(_inputBox.Pos.Height, _buttonClose.Pos.Height);
            int resizeHandleSize = (int)(settings.ViewH * 0.02);

            _logHistory!.SetBasePos(0, logYOffset, Utils.AlignTopLeft);
            _logHistory.Resize(WindowArea.Width - _scrollbarW, WindowArea.Height - logYOffset - resizeHandleSize);
            _logHistory.SetPos(WindowArea.X, WindowArea.Y);

            _resizeArea.X = WindowArea.X;
            _resizeArea.Y = WindowArea.Y + WindowArea.Height - resizeHandleSize;
            _resizeArea.Width = WindowArea.Width;
            _resizeArea.Height = resizeHandleSize;

            if (_resizeHandle != null && _resizeHandle.GetGraphicsWidth() != _resizeArea.Width)
            {
                _resizeHandle.Dispose();
                _resizeHandle = null;
            }

            if (_resizeHandle == null)
            {
                Image? temp = SharedResources.RenderDevice!.CreateImage(_resizeArea.Width, _resizeArea.Height);
                if (temp != null)
                {
                    temp.FillWithColor(_resizeHandleColor);
                    _resizeHandle = temp.CreateSprite();
                    temp.Unref();
                }
            }
            _resizeHandle!.SetDestFromRect(_resizeArea);
        }

        public void Logic()
        {
            InputState inpt = SharedResources.Inpt!;
            Settings settings = SharedResources.Settings!;
            MessageEngine msg = SharedResources.Msg!;
            FontEngine font = SharedResources.Font!;
            MapRenderer mapr = SharedGameResources.Mapr!;
            Avatar pc = SharedGameResources.Pc!;

            if (!Visible && _firstOpen && _logHistory!.IsEmpty())
            {
                _firstOpen = false;
            }

            // handle shortcut keys
            // these are supposed to work even if the console is hidden
            if (inpt.Pressing[Input.DeveloperCmd1] && !inpt.Lock[Input.DeveloperCmd1])
            {
                inpt.Lock[Input.DeveloperCmd1] = true;

                _inputBox!.SetText(settings.DevCmd1);
                Execute();
                return;
            }
            else if (inpt.Pressing[Input.DeveloperCmd2] && !inpt.Lock[Input.DeveloperCmd2])
            {
                inpt.Lock[Input.DeveloperCmd2] = true;

                _inputBox!.SetText(settings.DevCmd2);
                Execute();
                return;
            }
            else if (inpt.Pressing[Input.DeveloperCmd3] && !inpt.Lock[Input.DeveloperCmd3])
            {
                inpt.Lock[Input.DeveloperCmd3] = true;

                _inputBox!.SetText(settings.DevCmd3);
                Execute();
                return;
            }

            if (Visible)
            {
                if (!_firstOpen)
                {
                    _firstOpen = true;
                    _logHistory!.Add(msg.GetV("Use '%s' to inspect with the cursor.", inpt.GetBindingString(Input.Main2)), WidgetLog.MsgNormal);

                    _logHistory.SetNextColor(font.GetColor(FontEngine.ColorMenuBonus));
                    _logHistory.Add("exec \"msg=Hello World\"", WidgetLog.MsgNormal);

                    _logHistory.Add(msg.Get("Arguments with spaces should be enclosed with double quotes. Example:"), WidgetLog.MsgNormal);
                    _logHistory.Add(msg.Get("Type 'help' to get a list of commands.") + ' ', WidgetLog.MsgNormal);

                    _logHistory.SetNextStyle(WidgetLog.FontBold);
                    _logHistory.Add(msg.Get("Developer Console"), WidgetLog.MsgNormal);
                }

                if (!_inputBox!.EditMode)
                {
                    Tablist.Logic();
                }

                _inputBox.Logic();
                _logHistory!.Logic();

                if (_draggingResize && !inpt.Lock[Input.Main1])
                    _draggingResize = false;

                if (inpt.Pressing[Input.Cancel])
                {
                    Visible = false;
                    _inputBox.EditMode = false;
                    _inputBox.Logic();
                    Reset();
                }
                else if (_buttonClose!.CheckClick())
                {
                    Visible = false;
                    Reset();
                }
                else if (_inputBox.EditMode && inpt.Pressing[Input.Accept] && !inpt.Lock[Input.Accept])
                {
                    inpt.Lock[Input.Accept] = true;
                    Execute();
                }
                else if (_inputBox.EditMode && inpt.Pressing[Input.TexteditUp] && !inpt.Lock[Input.TexteditUp])
                {
                    inpt.Lock[Input.TexteditUp] = true;
                    if (_inputScrollback.Count != 0)
                    {
                        if (_inputScrollbackPos != 0)
                            _inputScrollbackPos--;
                        _inputBox.SetText(_inputScrollback[_inputScrollbackPos]);
                    }
                }
                else if (_inputBox.EditMode && inpt.Pressing[Input.TexteditDown] && !inpt.Lock[Input.TexteditDown])
                {
                    inpt.Lock[Input.TexteditDown] = true;
                    if (_inputScrollback.Count != 0)
                    {
                        _inputScrollbackPos++;
                        if (_inputScrollbackPos < _inputScrollback.Count)
                        {
                            _inputBox.SetText(_inputScrollback[_inputScrollbackPos]);
                        }
                        else
                        {
                            _inputScrollbackPos = _inputScrollback.Count;
                            _inputBox.SetText("");
                        }
                    }
                }
                else if ((inpt.Pressing[Input.Main1] && !inpt.Lock[Input.Main1] && Utils.IsWithinRect(_resizeArea, inpt.Mouse)) || _draggingResize)
                {
                    inpt.Lock[Input.Main1] = true;
                    _draggingResize = true;
                    _consoleHeight = Math.Min(1.0f, Math.Max(0.25f, (float)inpt.Mouse.Y / (float)settings.ViewH));
                    Align();
                }

                bool mouseInWindow = Utils.IsWithinRect(WindowArea, inpt.Mouse);

                if (inpt.Pressing[Input.Main2] && !inpt.Lock[Input.Main2] && !mouseInWindow)
                {
                    inpt.Lock[Input.Main2] = true;
                    Target = Utils.ScreenToMap(inpt.Mouse.X, inpt.Mouse.Y, mapr.Cam.Pos.X, mapr.Cam.Pos.Y);

                    _logHistory!.AddSeparator();

                    // print cursor position in map units & pixels
                    StringBuilder ss = new StringBuilder();
                    if (!mapr.Collider.IsOutsideMap(MathF.Floor(Target.X), MathF.Floor(Target.Y)))
                    {
                        GetTileInfo();
                        GetEntityInfo();
                        GetPlayerInfo();

                        ss.Append("X=").Append(Target.X).Append(", Y=").Append(Target.Y);
                        ss.Append("  |  X=").Append(inpt.Mouse.X).Append(msg.Get("px")).Append(", Y=").Append(inpt.Mouse.Y).Append(msg.Get("px"));
                        _logHistory.Add(ss.ToString(), WidgetLog.MsgNormal);
                    }
                    else
                    {
                        ss.Append("X=").Append(inpt.Mouse.X).Append(msg.Get("px")).Append(", Y=").Append(inpt.Mouse.Y).Append(msg.Get("px"));
                        _logHistory.Add(ss.ToString(), WidgetLog.MsgNormal);
                    }
                }

                if (inpt.Pressing[Input.Main2] && !mouseInWindow)
                {
                    DistanceTimer.Tick();

                    // print target distance from the player
                    if (DistanceTimer.IsEnd())
                    {
                        StringBuilder ss = new StringBuilder();
                        ss.Append(msg.Get("Distance")).Append(": ").Append(Utils.CalcDist(Target, pc.Stats.Pos));
                        _logHistory!.Add(ss.ToString(), WidgetLog.MsgNormal);
                    }
                }
                else
                {
                    DistanceTimer.Reset(Timer.Begin);
                }
            }
        }

        private void GetPlayerInfo()
        {
            Avatar pc = SharedGameResources.Pc!;
            MessageEngine msg = SharedResources.Msg!;
            FontEngine font = SharedResources.Font!;

            if (!((int)Target.X == (int)pc.Stats.Pos.X && (int)Target.Y == (int)pc.Stats.Pos.Y))
                return;

            StringBuilder ss = new StringBuilder();
            ss.Append(msg.Get("Entity")).Append(": ").Append(pc.Stats.Name).Append("  |  X=").Append(pc.Stats.Pos.X).Append(", Y=").Append(pc.Stats.Pos.Y);
            _logHistory!.SetNextColor(font.GetColor(FontEngine.ColorMenuBonus));
            _logHistory.Add(ss.ToString(), WidgetLog.MsgNormal);

            // TODO print more player data
        }

        private void GetTileInfo()
        {
            MapRenderer mapr = SharedGameResources.Mapr!;
            MessageEngine msg = SharedResources.Msg!;

            Int2 tile = Target.ToInt2();

            StringBuilder ss = new StringBuilder();
            for (int i = 0; i < mapr.Layers.Count; ++i)
            {
                if (mapr.Layers[i][tile.X][tile.Y] == 0)
                    continue;
                ss.Clear();
                ss.Append("    ").Append(mapr.Layernames[i]).Append('=').Append(mapr.Layers[i][tile.X][tile.Y]);
                _logHistory!.Add(ss.ToString(), WidgetLog.MsgNormal);
            }

            ss.Clear();
            ss.Append("    ").Append("collision=").Append(mapr.Collider.Colmap[tile.X][tile.Y]).Append(" (");
            switch (mapr.Collider.Colmap[tile.X][tile.Y])
            {
                case MapCollision.BlocksNone: ss.Append(msg.Get("none")); break;
                case MapCollision.BlocksAll: ss.Append(msg.Get("wall")); break;
                case MapCollision.BlocksMovement: ss.Append(msg.Get("short wall / pit")); break;
                case MapCollision.BlocksAllHidden: ss.Append(msg.Get("wall")); break;
                case MapCollision.BlocksMovementHidden: ss.Append(msg.Get("short wall / pit")); break;
                case MapCollision.BlocksEntities: ss.Append(msg.Get("entity")); break;
                case MapCollision.BlocksEnemies: ss.Append(msg.Get("entity, ally")); break;
                default: ss.Append(msg.Get("none")); break;
            }
            ss.Append(')');
            _logHistory!.Add(ss.ToString(), WidgetLog.MsgNormal);

            ss.Clear();
            ss.Append(msg.Get("Tile")).Append(": X=").Append(tile.X).Append(", Y=").Append(tile.Y);
            _logHistory.Add(ss.ToString(), WidgetLog.MsgNormal);
        }

        private void GetEntityInfo()
        {
            EntityManager entitym = SharedGameResources.Entitym!;
            NPCManager npcs = SharedGameResources.Npcs!;
            MessageEngine msg = SharedResources.Msg!;
            FontEngine font = SharedResources.Font!;

            StringBuilder ss = new StringBuilder();
            // enemies & ally creatures
            for (int i = 0; i < entitym.Entities.Count; ++i)
            {
                Entity e = entitym.Entities[i];
                if (!((int)Target.X == (int)e.Stats.Pos.X && (int)Target.Y == (int)e.Stats.Pos.Y))
                    continue;

                ss.Clear();
                ss.Append(msg.Get("Entity")).Append(": ").Append(e.Stats.Name).Append("  |  X=").Append(e.Stats.Pos.X).Append(", Y=").Append(e.Stats.Pos.Y);
                _logHistory!.SetNextColor(font.GetColor(FontEngine.ColorMenuBonus));
                _logHistory.Add(ss.ToString(), WidgetLog.MsgNormal);

                // TODO print more entity data
            }

            // non-ally NPCs
            for (int i = 0; i < npcs.Npcs.Count; ++i)
            {
                Entity e = npcs.Npcs[i];
                if (!((int)Target.X == (int)e.Stats.Pos.X && (int)Target.Y == (int)e.Stats.Pos.Y))
                    continue;

                ss.Clear();
                ss.Append(msg.Get("Entity")).Append(": ").Append(e.Stats.Name).Append("  |  X=").Append(e.Stats.Pos.X).Append(", Y=").Append(e.Stats.Pos.Y);
                _logHistory!.SetNextColor(font.GetColor(FontEngine.ColorMenuBonus));
                _logHistory.Add(ss.ToString(), WidgetLog.MsgNormal);

                // TODO print more entity data
            }
        }

        public override void Render()
        {
            if (!Visible)
                return;

            // background
            base.Render();

            _buttonClose!.Render();
            _inputBox!.Render();

            if (!_draggingResize)
                _logHistory!.Render();

            SharedResources.RenderDevice!.Render(_resizeHandle!);
        }

        public bool InputFocus()
        {
            return Visible && _inputBox!.EditMode;
        }

        private void Reset()
        {
            _inputBox!.SetText("");
            _inputBox.EditMode = true;
            _setShortcutSlot = 0;
            // log_history->clear();
        }

        public void CloseWindow()
        {
            Visible = false;
            _inputBox!.EditMode = false;
            _inputBox.Logic();
            Reset();
            Tablist.Defocus();
        }

        private void Execute()
        {
            Settings settings = SharedResources.Settings!;
            MessageEngine msg = SharedResources.Msg!;
            FontEngine font = SharedResources.Font!;
            InputState inpt = SharedResources.Inpt!;
            CampaignManager camp = SharedGameResources.Camp!;
            ItemManager items = SharedGameResources.Items!;
            ModManager mods = SharedResources.Mods!;
            PowerManager powers = SharedGameResources.Powers!;
            EventManager eventm = SharedGameResources.Eventm!;
            MapRenderer mapr = SharedGameResources.Mapr!;

            string command = _inputBox!.GetText();
            if (command == "") return;

            _inputScrollback.Add(command);
            _inputScrollbackPos = _inputScrollback.Count;
            _inputBox.SetText("");

            _logHistory!.AddSeparator();
            _logHistory.SetNextColor(font.GetColor(FontEngine.ColorWidgetDisabled));
            _logHistory.Add(command, WidgetLog.MsgUnique);

            bool startsWithSlash = (command[0] == '/');
            if (startsWithSlash)
            {
                command = "exec " + Parse.Trim(command.Substring(1)); // remove the slash
            }
            command = Parse.Trim(command);

            // setting a dev shortcut command; no need to process the command
            if (_setShortcutSlot > 0)
            {
                if (_setShortcutSlot == 1)
                {
                    settings.DevCmd1 = command;
                }
                else if (_setShortcutSlot == 2)
                {
                    settings.DevCmd2 = command;
                }
                else if (_setShortcutSlot == 3)
                {
                    settings.DevCmd3 = command;
                }
                _setShortcutSlot = 0;
                settings.SaveSettings();
                _logHistory.SetNextColor(font.GetColor(FontEngine.ColorMenuBonus));
                _logHistory.Add(msg.Get("Shortcut saved."), WidgetLog.MsgUnique);
                return;
            }

            List<string> args = new List<string>();
            command += ' ';

            string arg = Parse.PopFirstString(ref command, ' ');
            while (arg != "")
            {
                args.Add(arg);

                if (command.Length != 0 && command[0] == '"')
                {
                    command = command.Substring(1); // remove first quote
                    arg = Parse.PopFirstString(ref command, '"');
                    command = command.Substring(1); // remove trailing space
                }
                else
                {
                    arg = Parse.PopFirstString(ref command, ' ');
                }
            }


            if (args.Count == 0)
            {
                return;
            }

            if (args[0] == "help")
            {
                _logHistory.Add("procgen_map - " + msg.Get("For procedural maps, prints a color-coded map."), WidgetLog.MsgUnique);
                _logHistory.Add("add_power - " + msg.Get("adds a power to the action bar"), WidgetLog.MsgUnique);
                _logHistory.Add("toggle_fps - " + msg.Get("turns on/off the display of the FPS counter"), WidgetLog.MsgUnique);
                _logHistory.Add("toggle_hud - " + msg.Get("turns on/off all of the HUD elements"), WidgetLog.MsgUnique);
                _logHistory.Add("toggle_devhud - " + msg.Get("turns on/off the developer hud"), WidgetLog.MsgUnique);
                _logHistory.Add("list_powers - " + msg.Get("Prints a list of powers that match a search term. No search term will list all items"), WidgetLog.MsgUnique);
                _logHistory.Add("list_maps - " + msg.Get("Prints out all the map filenames located in the \"maps/\" directory."), WidgetLog.MsgUnique);
                _logHistory.Add("list_status - " + msg.Get("Prints out the active campaign statuses that match a search term. No search term will list all active statuses"), WidgetLog.MsgUnique);
                _logHistory.Add("list_items - " + msg.Get("Prints a list of items that match a search term. No search term will list all items"), WidgetLog.MsgUnique);
                _logHistory.Add("set_shortcut - " + msg.Get("Assign a console command to a shortcut key."), WidgetLog.MsgUnique);
                _logHistory.Add("exec - " + msg.Get("parses a series of event components and executes them as a single event"), WidgetLog.MsgUnique);
                _logHistory.Add("/ - " + msg.Get("parses a series of event components and executes them as a single event"), WidgetLog.MsgUnique);
                _logHistory.Add("clear - " + msg.Get("clears the command history"), WidgetLog.MsgUnique);
                _logHistory.Add("help - " + msg.Get("displays this text"), WidgetLog.MsgUnique);
            }
            else if (args[0] == "clear")
            {
                _logHistory.Clear();
            }
            else if (args[0] == "toggle_devhud")
            {
                settings.DevHud = !settings.DevHud;
                _logHistory.Add(msg.Get("Toggled the developer hud"), WidgetLog.MsgUnique);
            }
            else if (args[0] == "toggle_hud")
            {
                settings.ShowHud = !settings.ShowHud;
                _logHistory.Add(msg.Get("Toggled the hud"), WidgetLog.MsgUnique);
            }
            else if (args[0] == "toggle_fps")
            {
                settings.ShowFps = !settings.ShowFps;
                _logHistory.Add(msg.Get("Toggled the FPS counter"), WidgetLog.MsgUnique);
            }
            else if (args[0] == "list_status")
            {
                string searchTerms = "";
                for (int i = 1; i < args.Count; i++)
                {
                    searchTerms += args[i];

                    if (i + 1 != args.Count)
                        searchTerms += ' ';
                }
                List<int> matchingIds = new List<int>();
                List<string> statusStrings = new List<string>();
                camp.GetSetStatusStrings(statusStrings);

                for (int i = 0; i < statusStrings.Count; ++i)
                {
                    if (searchTerms.Length != 0 && Utils.StringFindCaseInsensitive(statusStrings[i], searchTerms) == -1)
                        continue;

                    matchingIds.Add(i);
                }

                if (matchingIds.Count != 0)
                {
                    _logHistory.SetMaxMessages((uint)matchingIds.Count);

                    for (int i = matchingIds.Count; i > 0; i--)
                    {
                        _logHistory.Add(statusStrings[matchingIds[i - 1]], WidgetLog.MsgNormal);
                    }

                    _logHistory.SetMaxMessages(WidgetLog.MaxMessages); // reset
                }
            }
            else if (args[0] == "list_items")
            {
                StringBuilder ss = new StringBuilder();

                string searchTerms = "";
                for (int i = 1; i < args.Count; i++)
                {
                    searchTerms += args[i];

                    if (i + 1 != args.Count)
                        searchTerms += ' ';
                }

                List<int> matchingIds = new List<int>();

                for (int i = 1; i < items.Items.Count; ++i)
                {
                    Item? item = items.Items[i];

                    if (item == null || !item.HasName)
                        continue;

                    string itemName = items.GetItemName(i);
                    if (searchTerms.Length != 0 && Utils.StringFindCaseInsensitive(itemName, searchTerms) == -1)
                        continue;

                    matchingIds.Add(i);
                }

                if (matchingIds.Count != 0)
                {
                    _logHistory.SetMaxMessages((uint)matchingIds.Count);

                    for (int i = matchingIds.Count; i > 0; i--)
                    {
                        int id = matchingIds[i - 1];

                        ss.Clear();
                        ss.Append(items.GetItemName(id)).Append(" (").Append(id).Append(')');
                        _logHistory.SetNextColor(items.GetItemColor(id));
                        _logHistory.Add(ss.ToString(), WidgetLog.MsgUnique);
                    }

                    _logHistory.SetMaxMessages(WidgetLog.MaxMessages); // reset
                }
            }
            else if (args[0] == "list_maps")
            {
                List<string> mapFilenames = mods.List("maps", !ModManager.ListFullPaths);

                for (int i = 0; i < mapFilenames.Count; ++i)
                {
                    // Remove "maps/" from all of the filenames so that it doesn't affect search results
                    // We'll still print out the "maps/" later during the output
                    mapFilenames[i] = mapFilenames[i].Remove(0, 5);
                }

                mapFilenames.Sort();

                string searchTerms = "";
                for (int i = 1; i < args.Count; i++)
                {
                    searchTerms += args[i];

                    if (i + 1 != args.Count)
                        searchTerms += ' ';
                }

                List<int> matchingIds = new List<int>();

                for (int i = 0; i < mapFilenames.Count; ++i)
                {
                    if (searchTerms.Length != 0 && Utils.StringFindCaseInsensitive(mapFilenames[i], searchTerms) == -1)
                        continue;

                    matchingIds.Add(i);
                }

                if (matchingIds.Count != 0)
                {
                    _logHistory.SetMaxMessages((uint)matchingIds.Count);

                    for (int i = matchingIds.Count; i > 0; i--)
                    {
                        _logHistory.Add("maps/" + mapFilenames[matchingIds[i - 1]], WidgetLog.MsgUnique);
                    }

                    _logHistory.SetMaxMessages(WidgetLog.MaxMessages); // reset
                }
            }
            else if (args[0] == "list_powers")
            {
                StringBuilder ss = new StringBuilder();

                string searchTerms = "";
                for (int i = 1; i < args.Count; i++)
                {
                    searchTerms += args[i];

                    if (i + 1 != args.Count)
                        searchTerms += ' ';
                }

                List<int> matchingIds = new List<int>();

                for (int i = 1; i < powers.Powers.Count; ++i)
                {
                    if (powers.Powers[i] == null)
                        continue;

                    if (searchTerms.Length != 0 && Utils.StringFindCaseInsensitive(powers.Powers[i]!.Name, searchTerms) == -1)
                        continue;

                    matchingIds.Add(i);
                }

                if (matchingIds.Count != 0)
                {
                    _logHistory.SetMaxMessages((uint)matchingIds.Count);

                    for (int i = matchingIds.Count; i > 0; i--)
                    {
                        int id = matchingIds[i - 1];

                        ss.Clear();
                        ss.Append(powers.Powers[id]!.Name).Append(" (").Append(id).Append(')');
                        _logHistory.Add(ss.ToString(), WidgetLog.MsgUnique);
                    }

                    _logHistory.SetMaxMessages(WidgetLog.MaxMessages); // reset
                }
            }
            else if (args[0] == "add_power")
            {
                if (args.Count != 2)
                {
                    _logHistory.SetNextColor(font.GetColor(FontEngine.ColorMenuPenalty));
                    _logHistory.Add(msg.Get("ERROR: Incorrect number of arguments"), WidgetLog.MsgUnique);
                    _logHistory.SetNextColor(font.GetColor(FontEngine.ColorMenuBonus));
                    _logHistory.Add(msg.Get("HINT:") + ' ' + args[0] + ' ' + msg.Get("<id>"), WidgetLog.MsgUnique);
                }
                else
                {
                    SharedGameResources.MenuAct!.AddPower(Parse.ToInt(args[1]), MenuActionBar.UseEmptySlot);
                }
            }
            else if (args[0] == "set_shortcut")
            {
                if (args.Count != 2)
                {
                    _logHistory.Add(msg.GetV("3 | <%s> | %s", inpt.GetBindingString(Input.DeveloperCmd3), settings.DevCmd3), WidgetLog.MsgUnique);
                    _logHistory.Add(msg.GetV("2 | <%s> | %s", inpt.GetBindingString(Input.DeveloperCmd2), settings.DevCmd2), WidgetLog.MsgUnique);
                    _logHistory.Add(msg.GetV("1 | <%s> | %s", inpt.GetBindingString(Input.DeveloperCmd1), settings.DevCmd1), WidgetLog.MsgUnique);
                    _logHistory.Add(msg.Get("Current shortcuts:"), WidgetLog.MsgUnique);
                    _logHistory.SetNextColor(font.GetColor(FontEngine.ColorMenuBonus));
                    _logHistory.Add(msg.Get("HINT:") + ' ' + args[0] + " [1-3]", WidgetLog.MsgUnique);
                }
                else
                {
                    int customCmdIndex = Parse.ToInt(args[1]);
                    if (customCmdIndex >= 0 && customCmdIndex < 4)
                    {
                        _setShortcutSlot = customCmdIndex;
                        _logHistory.SetNextColor(font.GetColor(FontEngine.ColorMenuBonus));
                        _logHistory.Add(msg.GetV("Enter the command you wish to save to slot %d.", _setShortcutSlot), WidgetLog.MsgUnique);
                    }
                    else
                    {
                        _logHistory.SetNextColor(font.GetColor(FontEngine.ColorMenuPenalty));
                        _logHistory.Add(msg.Get("ERROR: Not a valid shortcut slot."), WidgetLog.MsgUnique);
                    }
                }
            }
            else if (args[0] == "procgen_map")
            {
                if (mapr.ProcgenChunks.Count == 0)
                {
                    _logHistory.Add(msg.Get("Procedural chunk maps are only available for generated maps. This map is either not a procedural map, or the chunk data is corrupt."), WidgetLog.MsgUnique);
                }
                else
                {
                    int chunkMapH = (int)(mapr.ProcgenChunks.Count * MapRenderer.ProcgenChunkSize);
                    mapr.DrawProcgenChunkMap(_logHistory.SetupDrawBuffer(chunkMapH));
                }
            }
            else if (startsWithSlash || args[0] == "exec")
            {
                if (args.Count > 1)
                {
                    Event evnt = new Event();

                    for (int i = 1; i < args.Count; ++i)
                    {
                        string ecArg = args[i];
                        string key = Parse.PopFirstString(ref ecArg, '=');
                        string val = ecArg;

                        if (!eventm.LoadEventComponentString(key, ref val, evnt, null))
                        {
                            _logHistory.SetNextColor(font.GetColor(FontEngine.ColorMenuPenalty));
                            _logHistory.Add(msg.GetV("ERROR: '%s' is not a valid event key", key), WidgetLog.MsgUnique);
                        }
                    }

                    if (eventm.IsActive(evnt))
                    {
                        eventm.ExecuteEvent(evnt);
                    }
                }
                else
                {
                    _logHistory.SetNextColor(font.GetColor(FontEngine.ColorMenuPenalty));
                    _logHistory.Add(msg.Get("ERROR: Too few arguments"), WidgetLog.MsgUnique);
                    _logHistory.SetNextColor(font.GetColor(FontEngine.ColorMenuBonus));
                    _logHistory.Add(msg.Get("HINT:") + ' ' + args[0] + ' ' + msg.Get("<key>=<val> <key>=<val> ..."), WidgetLog.MsgUnique);
                }
            }
            else
            {
                _logHistory.SetNextColor(font.GetColor(FontEngine.ColorMenuPenalty));
                _logHistory.Add(msg.Get("ERROR: Unknown command"), WidgetLog.MsgUnique);
                _logHistory.SetNextColor(font.GetColor(FontEngine.ColorMenuBonus));
                _logHistory.Add(msg.Get("HINT: Type help"), WidgetLog.MsgUnique);
            }
        }
    }
}
