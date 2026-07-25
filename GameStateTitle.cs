// 对应 C++ 源：GameStateTitle.h + GameStateTitle.cpp
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// GameStateTitle
    ///
    /// 标题画面游戏状态：显示主菜单 logo、Play/Config/Credits/Exit 按钮，
    /// 首次启动时的语言与鼠标移动方式选择对话框，以及核心 mod 未启用时的提示。
    ///
    /// 持有的 <see cref="Sprite"/>、<see cref="WidgetButton"/>、<see cref="WidgetLabel"/>、
    /// <see cref="MenuConfirm"/> 等资源通过 <see cref="Dispose"/> 显式释放，释放顺序与
    /// 原始析构函数 <c>~GameStateTitle()</c> 一致。
    /// </summary>
    public class GameStateTitle : GameState
    {
        private const int PromptSelectModsOk = 0;
        private const int PromptSelectModsCancel = 1;

        private const int PromptSelectMousemoveNo = 0;
        private const int PromptSelectMousemoveYes = 1;

        private Sprite? _logo;
        private WidgetButton _buttonPlay;
        private WidgetButton _buttonExit;
        private WidgetButton _buttonCfg;
        private WidgetButton _buttonCredits;
        private WidgetLabel _labelVersion;
        private MenuConfirm? _menuLanguage;
        private MenuConfirm? _menuMovementType;
        private MenuConfirm? _promptSelectMods;

        private TabList _tablist;

        private Int2 _posLogo;
        private int _alignLogo;

        private uint _languageId;
        private List<string> _languageIso = new List<string>();

        /// <summary>对应 C++ 公有字段 <c>bool exit_game;</c>。</summary>
        public bool ExitGame;

        /// <summary>对应 C++ 公有字段 <c>bool load_game;</c>。</summary>
        public bool LoadGame;

        public GameStateTitle()
        {
            _logo = null;
            _buttonPlay = new WidgetButton(WidgetButton.DefaultFile);
            _buttonExit = new WidgetButton(WidgetButton.DefaultFile);
            _buttonCfg = new WidgetButton(WidgetButton.DefaultFile);
            _buttonCredits = new WidgetButton(WidgetButton.DefaultFile);
            _labelVersion = new WidgetLabel();
            _menuLanguage = null;
            _menuMovementType = null;
            _tablist = new TabList();
            _alignLogo = Utils.AlignCenter;
            _languageId = 0;
            ExitGame = false;
            LoadGame = false;

            using FileParser infile = new FileParser();
            // @CLASS GameStateTitle|Description of menus/gametitle.txt
            if (infile.Open("menus/gametitle.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    // @ATTR logo|filename, int, int, alignment : Image file, X, Y, Alignment|Filename and position of the main logo image.
                    if (infile.Key == "logo")
                    {
                        string val = infile.Val;
                        Image? graphics = SharedResources.RenderDevice!.LoadImage(Parse.PopFirstString(ref val), RenderDevice.ErrorNone);
                        if (graphics != null)
                        {
                            _logo = graphics.CreateSprite();
                            graphics.Unref();

                            _posLogo.X = Parse.PopFirstInt(ref val);
                            _posLogo.Y = Parse.PopFirstInt(ref val);
                            _alignLogo = Parse.ToAlignment(Parse.PopFirstString(ref val));
                        }
                    }
                    // @ATTR play_pos|int, int, alignment : X, Y, Alignment|Position of the "Play Game" button.
                    else if (infile.Key == "play_pos")
                    {
                        string val = infile.Val;
                        int x = Parse.PopFirstInt(ref val);
                        int y = Parse.PopFirstInt(ref val);
                        int a = Parse.ToAlignment(Parse.PopFirstString(ref val));
                        _buttonPlay.SetBasePos(x, y, a);
                    }
                    // @ATTR config_pos|int, int, alignment : X, Y, Alignment|Position of the "Configuration" button.
                    else if (infile.Key == "config_pos")
                    {
                        string val = infile.Val;
                        int x = Parse.PopFirstInt(ref val);
                        int y = Parse.PopFirstInt(ref val);
                        int a = Parse.ToAlignment(Parse.PopFirstString(ref val));
                        _buttonCfg.SetBasePos(x, y, a);
                    }
                    // @ATTR credits_pos|int, int, alignment : X, Y, Alignment|Position of the "Credits" button.
                    else if (infile.Key == "credits_pos")
                    {
                        string val = infile.Val;
                        int x = Parse.PopFirstInt(ref val);
                        int y = Parse.PopFirstInt(ref val);
                        int a = Parse.ToAlignment(Parse.PopFirstString(ref val));
                        _buttonCredits.SetBasePos(x, y, a);
                    }
                    // @ATTR exit_pos|int, int, alignment : X, Y, Alignment|Position of the "Exit Game" button.
                    else if (infile.Key == "exit_pos")
                    {
                        string val = infile.Val;
                        int x = Parse.PopFirstInt(ref val);
                        int y = Parse.PopFirstInt(ref val);
                        int a = Parse.ToAlignment(Parse.PopFirstString(ref val));
                        _buttonExit.SetBasePos(x, y, a);
                    }
                    else
                    {
                        infile.Error("GameStateTitle: '%s' is not a valid key.", infile.Key);
                    }
                }
                infile.Close();
            }

            _buttonPlay.SetLabel(SharedResources.Msg!.Get("Play Game"));
            _buttonPlay.Refresh();

            _buttonCfg.SetLabel(SharedResources.Msg.Get("Configuration"));
            _buttonCfg.Refresh();

            _buttonCredits.SetLabel(SharedResources.Msg.Get("Credits"));
            _buttonCredits.Refresh();

            _buttonExit.SetLabel(SharedResources.Msg.Get("Exit Game"));
            _buttonExit.Refresh();

            // set up labels
            _labelVersion.SetJustify(FontEngine.JustifyRight);
            _labelVersion.SetText(VersionInfo.CreateVersionStringFull());
            _labelVersion.SetColor(SharedResources.Font!.GetColor(FontEngine.ColorMenuNormal));

            // Setup tab order
            _tablist.Add(_buttonPlay);
            _tablist.Add(_buttonCfg);
            _tablist.Add(_buttonCredits);
            _tablist.Add(_buttonExit);

            // Core mod not selected dialogue
            _promptSelectMods = new MenuConfirm();
            _promptSelectMods.SetTitle(SharedResources.Msg.Get("Enable a core mod to continue"));
            _promptSelectMods.ActionList!.Append(SharedResources.Msg.Get("Mods"), "");
            _promptSelectMods.ActionList.Append(SharedResources.Msg.Get("Cancel"), "");

            RefreshWidgets();
            ForceRefreshBackground = true;

            if (SharedResources.Eset!.Gameplay.EnablePlaygame && SharedResources.Settings!.LoadSlot.Length != 0)
            {
                ShowLoading();
                SetRequestedGameState(new GameStateLoad());
            }

            SharedResources.RenderDevice.SetBackgroundColor(new Color(0, 0, 0, 0));

            // NOTE The presence of the language setting is used to determine if the
            // language select dialog is displayed. Is this adequate?
            if (!SharedResources.Settings.SetupLanguage && Platform.Instance.ConfigInterface[Platform.Interface.Language])
            {
                _menuLanguage = new MenuConfirm();
                _menuLanguage.SetTitle(SharedResources.Msg.Get("Language"));

                _languageIso.Clear();
                _menuLanguage.ActionList!.Clear();

                if (infile.Open("engine/languages.txt", FileParser.ModFile, FileParser.ErrorNormal))
                {
                    int i = 0;
                    while (infile.Next())
                    {
                        if (infile.Key.Length != 0)
                        {
                            _languageIso.Add(infile.Key);
                            _menuLanguage.ActionList.Append(infile.Val, infile.Val + " [" + infile.Key + "]");

                            if (infile.Key == SharedResources.Settings.Language)
                            {
                                _languageId = (uint)i;
                            }

                            i++;
                        }
                    }
                    infile.Close();
                }

                // no languages found; include English by default
                if (_menuLanguage.ActionList.GetSize() == 0)
                {
                    _languageIso.Add("en");
                    _menuLanguage.ActionList.Append("English", "English [en]");
                    _languageId = 0;
                }

                if (_languageIso.Count <= 1)
                {
                    SharedResources.Settings.SetupLanguage = true;
                }
            }

            // NOTE The presence of the mouse move setting is used to determine if the
            // movement type dialog is displayed. Is this adequate?
            if (!SharedResources.Settings.SetupMousemove && Platform.Instance.ConfigInput[Platform.Input.MouseMove] && SharedResources.Eset.Misc.MouseMoveEnabled)
            {
                _menuMovementType = new MenuConfirm();
                _menuMovementType.SetTitle(SharedResources.Msg.Get("Use mouse to move player?"));
                _menuMovementType.ActionList!.Append(SharedResources.Msg.Get("No"), "");
                _menuMovementType.ActionList.Append(SharedResources.Msg.Get("Yes"), "");
            }

            if (!SharedResources.Eset.Misc.MouseMoveEnabled)
                SharedResources.Settings.MouseMove = false;
        }

        public override void Logic()
        {
            if (!SharedResources.Settings!.SetupLanguage && !_menuLanguage!.Visible && _menuLanguage.ActionList!.GetSize() > 1)
            {
                _menuLanguage.Show();
                _menuLanguage.ActionList.Select(_languageId);
            }
            else if (!SharedResources.Settings.SetupMousemove && _menuMovementType != null && !_menuMovementType.Visible)
            {
                _menuMovementType.Show();
            }

            if (SharedResources.Inpt!.WindowResized)
                RefreshWidgets();

            if (_menuLanguage != null && _menuLanguage.Visible)
            {
                _menuLanguage.Logic();

                if (_menuLanguage.ClickedConfirm)
                {
                    SharedResources.Settings.Language = _languageIso[(int)_menuLanguage.ActionList!.GetSelected()];
                    SharedResources.Settings.SetupLanguage = true;
                    SharedResources.Settings.SaveSettings();

                    SharedResources.Msg!.Dispose();
                    SharedResources.Msg = new MessageEngine();
                    SharedResources.Font!.Dispose();
                    SharedResources.Font = DeviceList.GetFontEngine();
                    SetRequestedGameState(new GameStateTitle());
                }
                else if (_menuLanguage.ClickedCancel)
                {
                    SharedResources.Settings.SetupLanguage = true;
                    SharedResources.Settings.SaveSettings();

                    _menuLanguage.Visible = false;
                    _menuLanguage.ClickedCancel = false;
                }
            }
            else if (_menuMovementType != null && _menuMovementType.Visible)
            {
                _menuMovementType.Logic();

                if (_menuMovementType.ClickedConfirm)
                {
                    if (_menuMovementType.ActionList!.GetSelected() == PromptSelectMousemoveNo)
                    {
                        SharedResources.Settings.MouseMove = false;

                        _menuMovementType.Visible = false;
                        _menuMovementType.ClickedConfirm = false;

                        SharedResources.Settings.SetupMousemove = true;
                        SharedResources.Settings.SaveSettings();
                    }
                    else if (_menuMovementType.ActionList.GetSelected() == PromptSelectMousemoveYes)
                    {
                        SharedResources.Settings.MouseMove = true;

                        _menuMovementType.Visible = false;
                        _menuMovementType.ClickedConfirm = false;

                        SharedResources.Settings.SetupMousemove = true;
                        SharedResources.Settings.SaveSettings();
                    }
                }
                else if (_menuMovementType.ClickedCancel)
                {
                    SharedResources.Settings.MouseMove = false;

                    _menuMovementType.ClickedCancel = false;

                    SharedResources.Settings.SetupMousemove = true;
                    SharedResources.Settings.SaveSettings();
                }
            }
            else if (_promptSelectMods != null && _promptSelectMods.Visible)
            {
                _promptSelectMods.Logic();
                if (_promptSelectMods.ClickedConfirm)
                {
                    if (_promptSelectMods.ActionList!.GetSelected() == PromptSelectModsOk)
                    {
                        ShowLoading();
                        GameStateConfig config = new GameStateConfig();
                        config.SetActiveTab(MenuConfig.ModsTab);
                        SetRequestedGameState(config);

                        _promptSelectMods.Visible = false;
                        _promptSelectMods.ClickedConfirm = false;
                    }
                    else if (_promptSelectMods.ActionList.GetSelected() == PromptSelectModsCancel)
                    {
                        _promptSelectMods.Visible = false;
                        _promptSelectMods.ClickedConfirm = false;
                    }
                }
            }
            else
            {
                if (SharedResources.Inpt.Pressing[Input.Cancel] && !SharedResources.Inpt.Lock[Input.Cancel])
                {
                    SharedResources.Inpt.Lock[Input.Cancel] = true;
                    ExitRequested = true;
                }

                _tablist.Logic();

                bool playClicked = _buttonPlay.CheckClick();

                if (!SharedResources.Inpt.UsingMouse() && _tablist.GetCurrent() == -1)
                {
                    _tablist.GetNext(!TabList.GetInner, TabList.WidgetSelectAuto);
                }

                if (playClicked && !SharedResources.Eset!.Gameplay.EnablePlaygame)
                {
                    _promptSelectMods!.Show();
                }
                else if (playClicked)
                {
                    ShowLoading();

                    // if we don't have any saves, go directly to GameStateNew
                    List<string> saveDirs = new List<string>();
                    Filesystem.GetDirList(SharedResources.Settings.PathUser + "saves/" + SharedResources.Eset.Misc.SavePrefix, saveDirs);
                    if (saveDirs.Count == 0)
                    {
                        GameStateNew newgame = new GameStateNew();
                        newgame.GameSlot = 1;
                        SetRequestedGameState(newgame);
                    }
                    else
                    {
                        SetRequestedGameState(new GameStateLoad());
                    }
                }
                else if (_buttonCfg.CheckClick())
                {
                    ShowLoading();
                    SetRequestedGameState(new GameStateConfig());
                }
                else if (_buttonCredits.CheckClick())
                {
                    ShowLoading();
                    GameStateTitle title = new GameStateTitle();
                    GameStateCutscene credits = new GameStateCutscene(title);

                    if (!credits.Load("cutscenes/credits.txt"))
                    {
                        credits.Dispose();
                        title.Dispose();
                    }
                    else
                    {
                        SetRequestedGameState(credits);
                    }
                }
                else if (Platform.Instance.HasExitButton && _buttonExit.CheckClick())
                {
                    ExitRequested = true;
                }
            }
        }

        /// <summary>
        /// 对应 C++ 私有方法 <c>void refreshWidgets()</c>（非虚函数，不覆盖基类
        /// <see cref="GameState.RefreshWidgets"/> 的空实现）。
        /// </summary>
        private void RefreshWidgets()
        {
            if (_logo != null)
            {
                Rectangle r = new Rectangle();
                r.X = _posLogo.X;
                r.Y = _posLogo.Y;
                r.Width = _logo.GetGraphicsWidth();
                r.Height = _logo.GetGraphicsHeight();
                Utils.AlignToScreenEdge(_alignLogo, ref r);
                _logo.SetDestFromRect(r);
            }

            _buttonPlay.SetPos(0, 0);
            _buttonCfg.SetPos(0, 0);
            _buttonCredits.SetPos(0, 0);
            _buttonExit.SetPos(0, 0);

            _labelVersion.SetPos(SharedResources.Settings!.ViewW, 0);

            if (_menuMovementType != null)
                _menuMovementType.Align();
            if (_promptSelectMods != null)
                _promptSelectMods.Align();
        }

        public override void Render()
        {
            if (_menuLanguage != null && _menuLanguage.Visible)
            {
                _menuLanguage.Render();
            }
            else if (_menuMovementType != null && _menuMovementType.Visible)
            {
                _menuMovementType.Render();
            }
            else
            {
                // display logo
                SharedResources.RenderDevice!.Render(_logo!);

                // display buttons
                _buttonPlay.Render();
                _buttonCfg.Render();
                _buttonCredits.Render();

                if (_promptSelectMods != null && _promptSelectMods.Visible)
                    _promptSelectMods.Render();

                if (Platform.Instance.HasExitButton)
                    _buttonExit.Render();
            }

            // version number
            _labelVersion.Render();
        }

        /// <summary>
        /// 对应 C++ 的 <c>~GameStateTitle()</c>，释放顺序与原始析构函数逐行一致，
        /// 随后调用基类 <see cref="GameState.Dispose"/>。
        /// </summary>
        public override void Dispose()
        {
            if (_logo != null)
            {
                _logo.Dispose();
                _logo = null;
            }

            _buttonPlay.Dispose();
            _buttonCfg.Dispose();
            _buttonCredits.Dispose();
            _buttonExit.Dispose();
            _labelVersion.Dispose();
            _menuLanguage?.Dispose();
            _menuLanguage = null;
            _menuMovementType?.Dispose();
            _menuMovementType = null;

            if (_promptSelectMods != null)
            {
                _promptSelectMods.Dispose();
                _promptSelectMods = null;
            }

            base.Dispose();
        }
    }
}
