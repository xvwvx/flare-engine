// 对应 C++ 源：MenuConfig.h + MenuConfig.cpp
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// MenuConfig：游戏设置/暂停菜单。持有的 Widget、Sprite、MenuConfirm 等资源通过
    /// <see cref="IDisposable"/> 显式释放，释放顺序与原始析构函数 <c>~MenuConfig()</c> 一致。
    /// </summary>
    public class MenuConfig : IDisposable
    {
        private class ConfigOption
        {
            public bool Enabled;
            public WidgetLabel? Label;
            public Widget? Widget;

            public ConfigOption()
            {
                Enabled = false;
                Label = null;
                Widget = null;
            }
        }

        private class ConfigTab
        {
            public WidgetScrollBox? Scrollbox;
            public int EnabledCount;
            public List<ConfigOption> Options = new List<ConfigOption>();

            public ConfigTab()
            {
                Scrollbox = null;
                EnabledCount = 0;
            }

            public void SetOptionWidgets(int index, WidgetLabel? lb, Widget? w, string lbText)
            {
                if (!Options[index].Enabled)
                {
                    Options[index].Enabled = true;
                    EnabledCount++;
                }
                Options[index].Label = lb;
                Options[index].Label!.SetText(lbText);
                Options[index].Widget = w;
                Options[index].Widget!.TablistNavAlign = TabList.NavAlignRight;
            }

            public void SetOptionEnabled(int index, bool enable)
            {
                if (Options[index].Enabled && !enable)
                {
                    Options[index].Enabled = false;
                    if (EnabledCount > 0)
                        EnabledCount--;
                }
                else if (!Options[index].Enabled && enable)
                {
                    Options[index].Enabled = true;
                    EnabledCount++;
                }
            }

            public int GetEnabledIndex(int optionIndex)
            {
                int r = -1;
                for (int i = 0; i < Options.Count; ++i)
                {
                    if (Options[i].Enabled)
                        r++;
                    if (i == optionIndex)
                        break;
                }
                return (r == -1 ? 0 : r);
            }
        }

        private const int TouchScaleMin = 75;
        private const int TouchScaleMax = 125;

        private const int DefaultsConfirmOptionNo = 0;
        private const int DefaultsConfirmOptionYes = 1;

        private const int InputConfirmOptionNew = 0;
        private const int InputConfirmOptionClear = 1;

        private readonly List<ConfigTab> _cfgTabs = new List<ConfigTab>();

        private readonly bool _isGameState;
        private bool _enableGamestateButtons;
        private Avatar? _hero;

        private readonly List<ushort> _frameLimits = new List<ushort>();
        private readonly List<ushort> _virtualHeights = new List<ushort>();

        private bool _keybindsVisibleEquipswap;
        private readonly List<bool> _keybindsVisibleActionbar;
        private readonly List<bool> _keybindsVisibleMenus;

        private bool _modFilterUnknown;

        public const bool IsGameState = true;
        public const bool EnableSaveGame = true;

        public const short TabCount = 8;
        public const int ExitTab = 0;
        public const int VideoTab = 1;
        public const int AudioTab = 2;
        public const int GameTab = 3;
        public const int InterfaceTab = 4;
        public const int InputTab = 5;
        public const int KeybindsTab = 6;
        public const int ModsTab = 7;
        public const short NoTab = -1;

        public const int ExitOptionContinue = 0;
        public const int ExitOptionSave = 1;
        public const int ExitOptionExit = 2;
        public const int ExitOptionTimePlayed = 3;

        public TabList Tablist = new TabList();
        public TabList TablistMain = new TabList();
        public TabList TablistExit = new TabList();
        public TabList TablistVideo = new TabList();
        public TabList TablistAudio = new TabList();
        public TabList TablistGame = new TabList();
        public TabList TablistInterface = new TabList();
        public TabList TablistInput = new TabList();
        public TabList TablistKeybinds = new TabList();
        public TabList TablistMods = new TabList();

        public List<TabList?> Tablists = new List<TabList?>();

        public List<int> Optiontab = new List<int>();
        public List<Widget?> ChildWidget = new List<Widget?>();

        public WidgetTabControl? TabControl;
        public WidgetButton? OkButton;
        public WidgetButton? DefaultsButton;
        public WidgetButton? CancelButton;
        public Sprite? Background;
        public MenuConfirm? InputConfirm;
        public MenuConfirm? DefaultsConfirm;

        public WidgetLabel? PauseContinueLb;
        public WidgetButton? PauseContinueBtn;
        public WidgetLabel? PauseExitLb;
        public WidgetButton? PauseExitBtn;
        public WidgetLabel? PauseSaveLb;
        public WidgetButton? PauseSaveBtn;
        public WidgetLabel? PauseTimeLb;
        public WidgetLabel? PauseTimeText;

        public WidgetHorizontalList? RendererLstb;
        public WidgetLabel? RendererLb;
        public WidgetCheckBox? FullscreenCb;
        public WidgetLabel? FullscreenLb;
        public WidgetCheckBox? HwsurfaceCb;
        public WidgetLabel? HwsurfaceLb;
        public WidgetCheckBox? VsyncCb;
        public WidgetLabel? VsyncLb;
        public WidgetCheckBox? TextureFilterCb;
        public WidgetLabel? TextureFilterLb;
        public WidgetCheckBox? DpiScalingCb;
        public WidgetLabel? DpiScalingLb;
        public WidgetCheckBox? ParallaxLayersCb;
        public WidgetLabel? ParallaxLayersLb;
        public WidgetHorizontalList? FrameLimitLstb;
        public WidgetLabel? FrameLimitLb;
        public WidgetHorizontalList? MinRenderSizeLstb;
        public WidgetLabel? MinRenderSizeLb;
        public WidgetHorizontalList? MaxRenderSizeLstb;
        public WidgetLabel? MaxRenderSizeLb;
        public WidgetCheckBox? ThreadedImageLoadCb;
        public WidgetLabel? ThreadedImageLoadLb;
        public WidgetCheckBox? FadeWallsCb;
        public WidgetLabel? FadeWallsLb;

        public WidgetSlider? MusicVolumeSl;
        public WidgetLabel? MusicVolumeLb;
        public WidgetSlider? SoundVolumeSl;
        public WidgetLabel? SoundVolumeLb;
        public WidgetCheckBox? MuteOnFocusLossCb;
        public WidgetLabel? MuteOnFocusLossLb;

        public WidgetCheckBox? AutoEquipCb;
        public WidgetLabel? AutoEquipLb;
        public WidgetHorizontalList? AutoLootLstb;
        public WidgetLabel? AutoLootLb;
        public WidgetHorizontalList? LowHpWarningLstb;
        public WidgetLabel? LowHpWarningLb;
        public WidgetHorizontalList? LowHpThresholdLstb;
        public WidgetLabel? LowHpThresholdLb;

        public WidgetCheckBox? ShowFpsCb;
        public WidgetLabel? ShowFpsLb;
        public WidgetCheckBox? HardwareCursorCb;
        public WidgetLabel? HardwareCursorLb;
        public WidgetCheckBox? ColorblindCb;
        public WidgetLabel? ColorblindLb;
        public WidgetCheckBox? DevModeCb;
        public WidgetLabel? DevModeLb;
        public WidgetCheckBox? SubtitlesCb;
        public WidgetLabel? SubtitlesLb;
        public WidgetHorizontalList? LootTooltipLstb;
        public WidgetLabel? LootTooltipLb;
        public WidgetHorizontalList? MinimapLstb;
        public WidgetLabel? MinimapLb;
        public WidgetCheckBox? StatbarLabelsCb;
        public WidgetLabel? StatbarLabelsLb;
        public WidgetCheckBox? StatbarAutohideCb;
        public WidgetLabel? StatbarAutohideLb;
        public WidgetCheckBox? CombatTextCb;
        public WidgetLabel? CombatTextLb;
        public WidgetCheckBox? ItemCompareTipsCb;
        public WidgetLabel? ItemCompareTipsLb;
        public WidgetCheckBox? PauseOnFocusLossCb;
        public WidgetLabel? PauseOnFocusLossLb;

        public WidgetHorizontalList? JoystickDeviceLstb;
        public WidgetLabel? JoystickDeviceLb;
        public WidgetCheckBox? MouseMoveCb;
        public WidgetLabel? MouseMoveLb;
        public WidgetCheckBox? MouseAimCb;
        public WidgetLabel? MouseAimLb;
        public WidgetCheckBox? NoMouseCb;
        public WidgetLabel? NoMouseLb;
        public WidgetCheckBox? MouseMoveSwapCb;
        public WidgetLabel? MouseMoveSwapLb;
        public WidgetCheckBox? MouseMoveAttackCb;
        public WidgetLabel? MouseMoveAttackLb;
        public WidgetSlider? JoystickDeadzoneSl;
        public WidgetLabel? JoystickDeadzoneLb;
        public WidgetCheckBox? JoystickRumbleCb;
        public WidgetLabel? JoystickRumbleLb;
        public WidgetCheckBox? TouchControlsCb;
        public WidgetLabel? TouchControlsLb;
        public WidgetSlider? TouchScaleSl;
        public WidgetLabel? TouchScaleLb;

        public WidgetListBox? ActivemodsLstb;
        public WidgetLabel? ActivemodsLb;
        public WidgetListBox? InactivemodsLstb;
        public WidgetLabel? InactivemodsLb;
        public WidgetHorizontalList? LanguageLstb;
        public WidgetLabel? LanguageLb;
        public WidgetButton? ActivemodsShiftupBtn;
        public WidgetButton? ActivemodsShiftdownBtn;
        public WidgetButton? ActivemodsDeactivateBtn;
        public WidgetButton? InactivemodsActivateBtn;
        public WidgetHorizontalList? InactivemodsFilterLstb;

        public int ActiveTab;

        public Int2 Frame;
        public Int2 FrameOffset;
        public Int2 TabOffset;
        public Int2 BackgroundOffset;
        public Rectangle Scrollpane;
        public Color ScrollpaneColor;
        public Int2 ScrollpanePadding;
        public Color ScrollpaneSeparatorColor;
        public Int2 SecondaryOffset;

        public List<string> LanguageIso = new List<string>();

        public string NewRenderDevice = "";
        public List<Rectangle> VideoModes = new List<Rectangle>();

        public List<WidgetLabel?> KeybindsLb = new List<WidgetLabel?>();
        public List<WidgetHorizontalList?> KeybindsLstb = new List<WidgetHorizontalList?>();

        public Timer InputConfirmTimer = new Timer();
        public int InputAction;

        public string KeybindMsg = "";
        public Timer KeybindTipTimer = new Timer();
        public WidgetTooltip? KeybindTip;

        public bool ClickedAccept;
        public bool ClickedCancel;
        public bool ForceRefreshBackground;
        public bool ReloadMusic;
        public bool ClickedPauseContinue;
        public bool ClickedPauseExit;
        public bool ClickedPauseSave;
        public bool ShowFrameBackground;

        public string RenderDevice => RendererLstb!.GetValue();

        public MenuConfig(bool isGameState)
        {
            var font = SharedResources.Font!;
            var settings = SharedResources.Settings!;
            var msg = SharedResources.Msg!;
            var renderDevice = SharedResources.RenderDevice!;
            var mods = SharedResources.Mods!;
            var eset = SharedResources.Eset!;
            var inpt = SharedResources.Inpt!;

            _isGameState = isGameState;
            _enableGamestateButtons = false;
            _hero = null;
            _keybindsVisibleEquipswap = false;
            _keybindsVisibleActionbar = new List<bool>(10);
            for (int i = 0; i < 10; i++)
                _keybindsVisibleActionbar.Add(false);
            _keybindsVisibleMenus = new List<bool>(4);
            _keybindsVisibleMenus.Add(true);
            _keybindsVisibleMenus.Add(true);
            _keybindsVisibleMenus.Add(true);
            _keybindsVisibleMenus.Add(true);
            _modFilterUnknown = false;

            TabControl = new WidgetTabControl();
            OkButton = new WidgetButton(WidgetButton.DefaultFile);
            DefaultsButton = new WidgetButton(WidgetButton.DefaultFile);
            CancelButton = new WidgetButton(WidgetButton.DefaultFile);
            Background = null;
            InputConfirm = new MenuConfirm();
            DefaultsConfirm = new MenuConfirm();

            PauseContinueLb = new WidgetLabel();
            PauseContinueBtn = new WidgetButton(WidgetButton.DefaultFile);
            PauseExitLb = new WidgetLabel();
            PauseExitBtn = new WidgetButton(WidgetButton.DefaultFile);
            PauseSaveLb = new WidgetLabel();
            PauseSaveBtn = new WidgetButton(WidgetButton.DefaultFile);
            PauseTimeLb = new WidgetLabel();
            PauseTimeText = new WidgetLabel();

            RendererLstb = new WidgetHorizontalList();
            RendererLb = new WidgetLabel();
            FullscreenCb = new WidgetCheckBox(WidgetCheckBox.DefaultFile);
            FullscreenLb = new WidgetLabel();
            HwsurfaceCb = new WidgetCheckBox(WidgetCheckBox.DefaultFile);
            HwsurfaceLb = new WidgetLabel();
            VsyncCb = new WidgetCheckBox(WidgetCheckBox.DefaultFile);
            VsyncLb = new WidgetLabel();
            TextureFilterCb = new WidgetCheckBox(WidgetCheckBox.DefaultFile);
            TextureFilterLb = new WidgetLabel();
            DpiScalingCb = new WidgetCheckBox(WidgetCheckBox.DefaultFile);
            DpiScalingLb = new WidgetLabel();
            ParallaxLayersCb = new WidgetCheckBox(WidgetCheckBox.DefaultFile);
            ParallaxLayersLb = new WidgetLabel();
            FrameLimitLstb = new WidgetHorizontalList();
            FrameLimitLb = new WidgetLabel();
            MinRenderSizeLstb = new WidgetHorizontalList();
            MinRenderSizeLb = new WidgetLabel();
            MaxRenderSizeLstb = new WidgetHorizontalList();
            MaxRenderSizeLb = new WidgetLabel();
            ThreadedImageLoadCb = new WidgetCheckBox(WidgetCheckBox.DefaultFile);
            ThreadedImageLoadLb = new WidgetLabel();
            FadeWallsCb = new WidgetCheckBox(WidgetCheckBox.DefaultFile);
            FadeWallsLb = new WidgetLabel();

            MusicVolumeSl = new WidgetSlider(WidgetSlider.DefaultFile);
            MusicVolumeLb = new WidgetLabel();
            SoundVolumeSl = new WidgetSlider(WidgetSlider.DefaultFile);
            SoundVolumeLb = new WidgetLabel();
            MuteOnFocusLossCb = new WidgetCheckBox(WidgetCheckBox.DefaultFile);
            MuteOnFocusLossLb = new WidgetLabel();

            AutoEquipCb = new WidgetCheckBox(WidgetCheckBox.DefaultFile);
            AutoEquipLb = new WidgetLabel();
            AutoLootLstb = new WidgetHorizontalList();
            AutoLootLb = new WidgetLabel();
            LowHpWarningLstb = new WidgetHorizontalList();
            LowHpWarningLb = new WidgetLabel();
            LowHpThresholdLstb = new WidgetHorizontalList();
            LowHpThresholdLb = new WidgetLabel();

            ShowFpsCb = new WidgetCheckBox(WidgetCheckBox.DefaultFile);
            ShowFpsLb = new WidgetLabel();
            HardwareCursorCb = new WidgetCheckBox(WidgetCheckBox.DefaultFile);
            HardwareCursorLb = new WidgetLabel();
            ColorblindCb = new WidgetCheckBox(WidgetCheckBox.DefaultFile);
            ColorblindLb = new WidgetLabel();
            DevModeCb = new WidgetCheckBox(WidgetCheckBox.DefaultFile);
            DevModeLb = new WidgetLabel();
            SubtitlesCb = new WidgetCheckBox(WidgetCheckBox.DefaultFile);
            SubtitlesLb = new WidgetLabel();
            LootTooltipLstb = new WidgetHorizontalList();
            LootTooltipLb = new WidgetLabel();
            MinimapLstb = new WidgetHorizontalList();
            MinimapLb = new WidgetLabel();
            StatbarLabelsCb = new WidgetCheckBox(WidgetCheckBox.DefaultFile);
            StatbarLabelsLb = new WidgetLabel();
            StatbarAutohideCb = new WidgetCheckBox(WidgetCheckBox.DefaultFile);
            StatbarAutohideLb = new WidgetLabel();
            CombatTextCb = new WidgetCheckBox(WidgetCheckBox.DefaultFile);
            CombatTextLb = new WidgetLabel();
            ItemCompareTipsCb = new WidgetCheckBox(WidgetCheckBox.DefaultFile);
            ItemCompareTipsLb = new WidgetLabel();
            PauseOnFocusLossCb = new WidgetCheckBox(WidgetCheckBox.DefaultFile);
            PauseOnFocusLossLb = new WidgetLabel();

            JoystickDeviceLstb = new WidgetHorizontalList();
            JoystickDeviceLb = new WidgetLabel();
            MouseMoveCb = new WidgetCheckBox(WidgetCheckBox.DefaultFile);
            MouseMoveLb = new WidgetLabel();
            MouseAimCb = new WidgetCheckBox(WidgetCheckBox.DefaultFile);
            MouseAimLb = new WidgetLabel();
            NoMouseCb = new WidgetCheckBox(WidgetCheckBox.DefaultFile);
            NoMouseLb = new WidgetLabel();
            MouseMoveSwapCb = new WidgetCheckBox(WidgetCheckBox.DefaultFile);
            MouseMoveSwapLb = new WidgetLabel();
            MouseMoveAttackCb = new WidgetCheckBox(WidgetCheckBox.DefaultFile);
            MouseMoveAttackLb = new WidgetLabel();
            JoystickDeadzoneSl = new WidgetSlider(WidgetSlider.DefaultFile);
            JoystickDeadzoneLb = new WidgetLabel();
            JoystickRumbleCb = new WidgetCheckBox(WidgetCheckBox.DefaultFile);
            JoystickRumbleLb = new WidgetLabel();
            TouchControlsCb = new WidgetCheckBox(WidgetCheckBox.DefaultFile);
            TouchControlsLb = new WidgetLabel();
            TouchScaleSl = new WidgetSlider(WidgetSlider.DefaultFile);
            TouchScaleLb = new WidgetLabel();

            ActivemodsLstb = new WidgetListBox(10, WidgetListBox.DefaultFile);
            ActivemodsLb = new WidgetLabel();
            InactivemodsLstb = new WidgetListBox(10, WidgetListBox.DefaultFile);
            InactivemodsLb = new WidgetLabel();
            LanguageLstb = new WidgetHorizontalList();
            LanguageLb = new WidgetLabel();
            ActivemodsShiftupBtn = new WidgetButton(WidgetButton.DirUpFile);
            ActivemodsShiftdownBtn = new WidgetButton(WidgetButton.DirDownFile);
            ActivemodsDeactivateBtn = new WidgetButton(WidgetButton.DefaultFile);
            InactivemodsActivateBtn = new WidgetButton(WidgetButton.DefaultFile);
            InactivemodsFilterLstb = new WidgetHorizontalList();

            ActiveTab = 0;
            Frame = new Int2(0, 0);
            FrameOffset = new Int2(11, 8);
            TabOffset = new Int2(3, 0);
            BackgroundOffset = new Int2(0, TabControl.GetTabHeight() - (TabControl.GetTabHeight() / 16));
            ScrollpaneColor = new Color(0, 0, 0, 0);
            ScrollpanePadding = new Int2(8, 40);
            ScrollpaneSeparatorColor = font.GetColor(FontEngine.ColorWidgetDisabled);
            NewRenderDevice = settings.RenderDeviceName;
            InputConfirmTimer = new Timer((uint)(settings.MaxFramesPerSec * 10));
            InputAction = 0;
            KeybindTipTimer = new Timer((uint)(settings.MaxFramesPerSec * 5));
            KeybindTip = new WidgetTooltip();
            ClickedAccept = false;
            ClickedCancel = false;
            ForceRefreshBackground = false;
            ReloadMusic = false;
            ClickedPauseContinue = false;
            ClickedPauseExit = false;
            ClickedPauseSave = false;
            ShowFrameBackground = false;

            InputConfirm.SetTitle(msg.Get("Assign:"));
            InputConfirm.ActionList!.Append(msg.Get("New"), "");
            InputConfirm.ActionList.Append(msg.Get("Clear"), "");

            DefaultsConfirm.SetTitle(msg.Get("Reset ALL settings?"));
            DefaultsConfirm.ActionList!.Append(msg.Get("No"), "");
            DefaultsConfirm.ActionList.Append(msg.Get("Yes"), "");

            Image? graphics;
            graphics = renderDevice.LoadImage("images/menus/config.png", FlareEngine.RenderDevice.ErrorNormal);
            if (graphics != null)
            {
                Background = graphics.CreateSprite();
                graphics.Unref();
            }

            OkButton.SetLabel(msg.Get("OK"));
            DefaultsButton.SetLabel(msg.Get("Defaults"));
            CancelButton.SetLabel(msg.Get("Cancel"));

            PauseContinueBtn.SetLabel(msg.Get("Continue"));
            SetPauseExitText(EnableSaveGame);
            PauseSaveBtn.SetLabel(msg.Get("Save Game"));
            SetPauseSaveEnabled(EnableSaveGame);
            PauseTimeText.SetText(Utils.GetTimeString(0));
            PauseTimeText.SetJustify(FontEngine.JustifyRight);
            PauseTimeText.SetVAlign(LabelInfo.ValignCenter);

            ActivemodsLstb.MultiSelect = true;
            for (int i = 0; i < mods.ModList.Count; i++)
            {
                if (mods.ModList[i].Name != ModManager.FallbackMod)
                    ActivemodsLstb.Append(mods.ModList[i].Name, CreateModTooltip(mods.ModList[i]));
            }

            string activeGame = "";
            for (int i = mods.ModList.Count; i > 0; i--)
            {
                Mod tempMod = mods.ModList[i - 1];
                if (tempMod.Game.Length != 0)
                {
                    activeGame = tempMod.Game;
                    break;
                }
            }

            List<string> modGames = new List<string>();

            InactivemodsLstb.MultiSelect = true;
            for (int i = 0; i < mods.ModDirs.Count; i++)
            {
                Mod tempMod = mods.LoadMod(mods.ModDirs[i]);
                if (mods.ModDirs[i] != ModManager.FallbackMod && (activeGame.Length == 0 || tempMod.Game != activeGame))
                {
                    if (tempMod.Game == ModManager.FallbackGame || settings.Game.Length == 0 || (settings.Game.Length != 0 && settings.Game == tempMod.Game))
                    {
                        if (tempMod.Game.Length == 0)
                        {
                            _modFilterUnknown = true;
                        }
                        else if (!modGames.Contains(tempMod.Game))
                        {
                            modGames.Add(tempMod.Game);
                        }
                    }
                }

                bool skipMod = false;
                for (int j = 0; j < mods.ModList.Count; j++)
                {
                    if (mods.ModDirs[i] == mods.ModList[j].Name)
                    {
                        skipMod = true;
                        break;
                    }
                }
                if (!skipMod && mods.ModDirs[i] != ModManager.FallbackMod)
                {
                    InactivemodsLstb.Append(mods.ModDirs[i], CreateModTooltip(tempMod));
                }
            }
            InactivemodsLstb.Sort();
            FilterMods();

            modGames.Sort();
            if (activeGame.Length != 0)
            {
                modGames.Insert(0, activeGame);
            }
            if (_modFilterUnknown)
            {
                modGames.Add(msg.Get("<unknown>"));
            }

            InactivemodsFilterLstb.Append(msg.Get("All Mods"), "");
            InactivemodsFilterLstb.Append(msg.Get("All Core Mods"), "");
            for (int i = 0; i < modGames.Count; ++i)
            {
                if (modGames[i].Length != 0)
                    InactivemodsFilterLstb.Append(modGames[i], "");
            }

            RefreshJoysticks();

            for (int i = 0; i < InputState.KeyCountUser; i++)
            {
                KeybindsLb.Add(new WidgetLabel());
                KeybindsLstb.Add(new WidgetHorizontalList());
                KeybindsLstb[i]!.HasAction = true;
                KeybindsLstb[i]!.MaxVisibleActions = 1;
            }

            LootTooltipLstb.Append(msg.Get("Default"), msg.Get("Show all loot tooltips, except for those that would be obscured by the player or an enemy. Temporarily show all loot tooltips with 'Alt'."));
            LootTooltipLstb.Append(msg.Get("Show all"), msg.Get("Always show loot tooltips. Temporarily hide all loot tooltips with 'Alt'."));
            LootTooltipLstb.Append(msg.Get("Hidden"), msg.Get("Always hide loot tooltips, except for when a piece of loot is hovered with the mouse cursor. Temporarily show all loot tooltips with 'Alt'."));

            MinimapLstb.Append(msg.Get("Visible"), "");
            MinimapLstb.Append(msg.Get("Visible (2x zoom)"), "");
            MinimapLstb.Append(msg.Get("Hidden"), "");

            string lhpwPrefix = msg.Get("Controls the type of warning to be activated when the player is below the low health threshold.");
            string lhpwWarning1 = msg.Get("- Display a message");
            string lhpwWarning2 = msg.Get("- Play a sound");
            string lhpwWarning3 = msg.Get("- Change the cursor");

            LowHpWarningLstb.Append(msg.Get("Disabled"), lhpwPrefix);
            LowHpWarningLstb.Append(msg.Get("All"), lhpwPrefix + "\n\n" + lhpwWarning1 + '\n' + lhpwWarning2 + '\n' + lhpwWarning3);
            LowHpWarningLstb.Append(msg.Get("Message & Cursor"), lhpwPrefix + "\n\n" + lhpwWarning1 + '\n' + lhpwWarning3);
            LowHpWarningLstb.Append(msg.Get("Message & Sound"), lhpwPrefix + "\n\n" + lhpwWarning1 + '\n' + lhpwWarning2);
            LowHpWarningLstb.Append(msg.Get("Sound & Cursor"), lhpwPrefix + "\n\n" + lhpwWarning2 + '\n' + lhpwWarning3);
            LowHpWarningLstb.Append(msg.Get("Message"), lhpwPrefix + "\n\n" + lhpwWarning1);
            LowHpWarningLstb.Append(msg.Get("Cursor"), lhpwPrefix + "\n\n" + lhpwWarning3);
            LowHpWarningLstb.Append(msg.Get("Sound"), lhpwPrefix + "\n\n" + lhpwWarning2);

            for (int i = 1; i <= 10; ++i)
            {
                LowHpThresholdLstb.Append((i * 5).ToString() + "%", msg.Get("When the player's health drops below the given threshold, the low health notifications are triggered if one or more of them is enabled."));
            }

            string autoLootDesc = msg.Get("When enabled, eligible loot will be picked up automatically when nearby.");
            AutoLootLstb.Append(msg.Get("Disabled"), autoLootDesc);
            AutoLootLstb.Append(msg.Get("Enabled"), autoLootDesc);
            AutoLootLstb.Append(msg.Get("Only currency"), autoLootDesc);

            _frameLimits.Add(30);
            _frameLimits.Add(60);
            _frameLimits.Add(120);
            _frameLimits.Add(240);
            if (!_frameLimits.Contains(settings.MaxFramesPerSec))
                _frameLimits.Add(settings.MaxFramesPerSec);
            ushort refreshRate = renderDevice.GetRefreshRate();
            if (refreshRate > 0 && !_frameLimits.Contains(refreshRate))
                _frameLimits.Add(refreshRate);

            _frameLimits.Sort();
            for (int i = 0; i < _frameLimits.Count; ++i)
            {
                FrameLimitLstb.Append(_frameLimits[i].ToString(), msg.Get("The maximum frame rate that the game will be allowed to run at."));
            }

            string minRenderSizeTooltip = msg.Get("The render size refers to the height in pixels of the surface used to draw the game. Mods define the allowed render sizes, but this option allows overriding the minimum size.");
            MinRenderSizeLstb.Append(msg.Get("Default"), minRenderSizeTooltip);

            string maxRenderSizeTooltip = msg.Get("The render size refers to the height in pixels of the surface used to draw the game. Mods define the allowed render sizes, but this option allows overriding the maximum size.");
            MaxRenderSizeLstb.Append(msg.Get("Default"), maxRenderSizeTooltip);

            _virtualHeights.AddRange(eset.Resolutions.VirtualHeights);

            if (settings.MinRenderSize > 0 && !_virtualHeights.Contains(settings.MinRenderSize))
                _virtualHeights.Add(settings.MinRenderSize);

            if (settings.MaxRenderSize > 0 && !_virtualHeights.Contains(settings.MaxRenderSize))
                _virtualHeights.Add(settings.MaxRenderSize);

            _virtualHeights.Sort();
            for (int i = 0; i < _virtualHeights.Count; ++i)
            {
                string heightStr = _virtualHeights[i].ToString();
                MinRenderSizeLstb.Append(heightStr, minRenderSizeTooltip);
                MaxRenderSizeLstb.Append(heightStr, maxRenderSizeTooltip);
            }

            inpt.JoysticksChanged = false;

            Init();

            renderDevice.SetBackgroundColor(new Color(0, 0, 0, 0));
        }

        public void Dispose()
        {
            Cleanup();
            GC.SuppressFinalize(this);
        }

        public void Init()
        {
            var msg = SharedResources.Msg!;
            var eset = SharedResources.Eset!;
            var inpt = SharedResources.Inpt!;

            TabControl!.SetupTab(ExitTab, msg.Get("Exit"), TablistExit);
            TabControl.SetupTab(VideoTab, msg.Get("Video"), TablistVideo);
            TabControl.SetupTab(AudioTab, msg.Get("Audio"), TablistAudio);
            TabControl.SetupTab(GameTab, msg.Get("Game"), TablistGame);
            TabControl.SetupTab(InterfaceTab, msg.Get("Interface"), TablistInterface);
            TabControl.SetupTab(InputTab, msg.Get("Input"), TablistInput);
            TabControl.SetupTab(KeybindsTab, msg.Get("Keybindings"), TablistKeybinds);
            TabControl.SetupTab(ModsTab, msg.Get("Mods"), TablistMods);

            ReadConfig();

            _cfgTabs.Clear();
            for (int i = 0; i < TabCount - 1; i++)
                _cfgTabs.Add(new ConfigTab());
            _cfgTabs[ExitTab].Options = new List<ConfigOption>(new ConfigOption[4]);
            for (int i = 0; i < 4; i++) _cfgTabs[ExitTab].Options[i] = new ConfigOption();
            _cfgTabs[VideoTab].Options = new List<ConfigOption>(new ConfigOption[Platform.Video.Count]);
            for (int i = 0; i < Platform.Video.Count; i++) _cfgTabs[VideoTab].Options[i] = new ConfigOption();
            _cfgTabs[AudioTab].Options = new List<ConfigOption>(new ConfigOption[Platform.Audio.Count]);
            for (int i = 0; i < Platform.Audio.Count; i++) _cfgTabs[AudioTab].Options[i] = new ConfigOption();
            _cfgTabs[GameTab].Options = new List<ConfigOption>(new ConfigOption[Platform.Game.Count]);
            for (int i = 0; i < Platform.Game.Count; i++) _cfgTabs[GameTab].Options[i] = new ConfigOption();
            _cfgTabs[InterfaceTab].Options = new List<ConfigOption>(new ConfigOption[Platform.Interface.Count]);
            for (int i = 0; i < Platform.Interface.Count; i++) _cfgTabs[InterfaceTab].Options[i] = new ConfigOption();
            _cfgTabs[InputTab].Options = new List<ConfigOption>(new ConfigOption[Platform.Input.Count]);
            for (int i = 0; i < Platform.Input.Count; i++) _cfgTabs[InputTab].Options[i] = new ConfigOption();
            _cfgTabs[KeybindsTab].Options = new List<ConfigOption>(new ConfigOption[InputState.KeyCountUser]);
            for (int i = 0; i < InputState.KeyCountUser; i++) _cfgTabs[KeybindsTab].Options[i] = new ConfigOption();

            _cfgTabs[ExitTab].SetOptionWidgets(ExitOptionContinue, PauseContinueLb, PauseContinueBtn, msg.Get("Paused"));
            _cfgTabs[ExitTab].SetOptionWidgets(ExitOptionSave, PauseSaveLb, PauseSaveBtn, "");
            _cfgTabs[ExitTab].SetOptionWidgets(ExitOptionExit, PauseExitLb, PauseExitBtn, "");
            _cfgTabs[ExitTab].SetOptionWidgets(ExitOptionTimePlayed, PauseTimeLb, PauseTimeText, msg.Get("Time Played"));

            if (!(EnableSaveGame && eset.Misc.SaveAnywhere))
            {
                _cfgTabs[ExitTab].SetOptionEnabled(ExitOptionSave, false);
            }

            _cfgTabs[VideoTab].SetOptionWidgets(Platform.Video.Renderer, RendererLb, RendererLstb, msg.Get("Renderer"));
            _cfgTabs[VideoTab].SetOptionWidgets(Platform.Video.Fullscreen, FullscreenLb, FullscreenCb, msg.Get("Full Screen Mode"));
            _cfgTabs[VideoTab].SetOptionWidgets(Platform.Video.Hwsurface, HwsurfaceLb, HwsurfaceCb, msg.Get("Hardware surfaces"));
            _cfgTabs[VideoTab].SetOptionWidgets(Platform.Video.Vsync, VsyncLb, VsyncCb, msg.Get("V-Sync"));
            _cfgTabs[VideoTab].SetOptionWidgets(Platform.Video.TextureFilter, TextureFilterLb, TextureFilterCb, msg.Get("Texture Filtering"));
            _cfgTabs[VideoTab].SetOptionWidgets(Platform.Video.DpiScaling, DpiScalingLb, DpiScalingCb, msg.Get("DPI scaling"));
            _cfgTabs[VideoTab].SetOptionWidgets(Platform.Video.ParallaxLayers, ParallaxLayersLb, ParallaxLayersCb, msg.Get("Parallax Layers"));
            _cfgTabs[VideoTab].SetOptionWidgets(Platform.Video.MinRenderSize, MinRenderSizeLb, MinRenderSizeLstb, msg.Get("Minimum Render Size"));
            _cfgTabs[VideoTab].SetOptionWidgets(Platform.Video.MaxRenderSize, MaxRenderSizeLb, MaxRenderSizeLstb, msg.Get("Maximum Render Size"));
            _cfgTabs[VideoTab].SetOptionWidgets(Platform.Video.FrameLimit, FrameLimitLb, FrameLimitLstb, msg.Get("Frame Limit"));
            _cfgTabs[VideoTab].SetOptionWidgets(Platform.Video.ThreadedImageLoad, ThreadedImageLoadLb, ThreadedImageLoadCb, msg.Get("Threaded Image Loading"));
            _cfgTabs[VideoTab].SetOptionWidgets(Platform.Video.FadeWalls, FadeWallsLb, FadeWallsCb, msg.Get("Show player behind walls"));

            _cfgTabs[AudioTab].SetOptionWidgets(Platform.Audio.Sfx, SoundVolumeLb, SoundVolumeSl, msg.Get("Sound Volume"));
            _cfgTabs[AudioTab].SetOptionWidgets(Platform.Audio.Music, MusicVolumeLb, MusicVolumeSl, msg.Get("Music Volume"));
            _cfgTabs[AudioTab].SetOptionWidgets(Platform.Audio.MuteOnFocusLoss, MuteOnFocusLossLb, MuteOnFocusLossCb, msg.Get("Mute audio when window loses focus"));

            _cfgTabs[GameTab].SetOptionWidgets(Platform.Game.AutoEquip, AutoEquipLb, AutoEquipCb, msg.Get("Automatically equip items"));
            _cfgTabs[GameTab].SetOptionWidgets(Platform.Game.AutoLoot, AutoLootLb, AutoLootLstb, msg.Get("Automatically pick up loot"));
            _cfgTabs[GameTab].SetOptionWidgets(Platform.Game.LowHpWarningType, LowHpWarningLb, LowHpWarningLstb, msg.Get("Low health notification"));
            _cfgTabs[GameTab].SetOptionWidgets(Platform.Game.LowHpThreshold, LowHpThresholdLb, LowHpThresholdLstb, msg.Get("Low health threshold"));

            _cfgTabs[InterfaceTab].SetOptionWidgets(Platform.Interface.Language, LanguageLb, LanguageLstb, msg.Get("Language"));
            _cfgTabs[InterfaceTab].SetOptionWidgets(Platform.Interface.ShowFps, ShowFpsLb, ShowFpsCb, msg.Get("Show FPS"));
            _cfgTabs[InterfaceTab].SetOptionWidgets(Platform.Interface.HardwareCursor, HardwareCursorLb, HardwareCursorCb, msg.Get("Use system mouse cursor"));
            _cfgTabs[InterfaceTab].SetOptionWidgets(Platform.Interface.Colorblind, ColorblindLb, ColorblindCb, msg.Get("Colorblind Mode"));
            _cfgTabs[InterfaceTab].SetOptionWidgets(Platform.Interface.DevMode, DevModeLb, DevModeCb, msg.Get("Developer Mode"));
            _cfgTabs[InterfaceTab].SetOptionWidgets(Platform.Interface.Subtitles, SubtitlesLb, SubtitlesCb, msg.Get("Subtitles"));
            _cfgTabs[InterfaceTab].SetOptionWidgets(Platform.Interface.LootTooltips, LootTooltipLb, LootTooltipLstb, msg.Get("Loot tooltip visibility"));
            _cfgTabs[InterfaceTab].SetOptionWidgets(Platform.Interface.MinimapMode, MinimapLb, MinimapLstb, msg.Get("Mini-map mode"));
            _cfgTabs[InterfaceTab].SetOptionWidgets(Platform.Interface.StatbarLabels, StatbarLabelsLb, StatbarLabelsCb, msg.Get("Always show stat bar labels"));
            _cfgTabs[InterfaceTab].SetOptionWidgets(Platform.Interface.StatbarAutohide, StatbarAutohideLb, StatbarAutohideCb, msg.Get("Allow stat bar auto-hiding"));
            _cfgTabs[InterfaceTab].SetOptionWidgets(Platform.Interface.CombatText, CombatTextLb, CombatTextCb, msg.Get("Show combat text"));
            _cfgTabs[InterfaceTab].SetOptionWidgets(Platform.Interface.ItemCompareTips, ItemCompareTipsLb, ItemCompareTipsCb, msg.Get("Show item comparison tooltips"));
            _cfgTabs[InterfaceTab].SetOptionWidgets(Platform.Interface.PauseOnFocusLoss, PauseOnFocusLossLb, PauseOnFocusLossCb, msg.Get("Pause game when window loses focus"));

            _cfgTabs[InputTab].SetOptionWidgets(Platform.Input.Joystick, JoystickDeviceLb, JoystickDeviceLstb, msg.Get("Joystick"));
            _cfgTabs[InputTab].SetOptionWidgets(Platform.Input.MouseMove, MouseMoveLb, MouseMoveCb, msg.Get("Move hero using mouse"));
            _cfgTabs[InputTab].SetOptionWidgets(Platform.Input.MouseAim, MouseAimLb, MouseAimCb, msg.Get("Mouse aim"));
            _cfgTabs[InputTab].SetOptionWidgets(Platform.Input.NoMouse, NoMouseLb, NoMouseCb, msg.Get("Do not use mouse"));
            _cfgTabs[InputTab].SetOptionWidgets(Platform.Input.MouseMoveSwap, MouseMoveSwapLb, MouseMoveSwapCb, msg.Get("Swap mouse movement button"));
            _cfgTabs[InputTab].SetOptionWidgets(Platform.Input.MouseMoveAttack, MouseMoveAttackLb, MouseMoveAttackCb, msg.Get("Attack with mouse movement"));
            _cfgTabs[InputTab].SetOptionWidgets(Platform.Input.JoystickDeadzone, JoystickDeadzoneLb, JoystickDeadzoneSl, msg.Get("Joystick Deadzone"));
            _cfgTabs[InputTab].SetOptionWidgets(Platform.Input.JoystickRumble, JoystickRumbleLb, JoystickRumbleCb, msg.Get("Joystick Rumble"));
            _cfgTabs[InputTab].SetOptionWidgets(Platform.Input.TouchControls, TouchControlsLb, TouchControlsCb, msg.Get("Touch Controls"));
            _cfgTabs[InputTab].SetOptionWidgets(Platform.Input.TouchScale, TouchScaleLb, TouchScaleSl, msg.Get("Touch Gamepad Scaling"));

            for (int i = 0; i < KeybindsLstb.Count; ++i)
            {
                _cfgTabs[KeybindsTab].SetOptionWidgets(i, KeybindsLb[i], KeybindsLstb[i], inpt.BindingName[i]);
                if (i >= Input.Bar1 && i <= Input.Bar0 && !_keybindsVisibleActionbar[i - Input.Bar1])
                {
                    _cfgTabs[KeybindsTab].SetOptionEnabled(i, false);
                }
                else if ((i == Input.EquipmentSwap || i == Input.EquipmentSwapPrev) && !_keybindsVisibleEquipswap)
                {
                    _cfgTabs[KeybindsTab].SetOptionEnabled(i, false);
                }
                else if (i >= Input.Character && i <= Input.Log && !_keybindsVisibleMenus[i - Input.Character])
                {
                    _cfgTabs[KeybindsTab].SetOptionEnabled(i, false);
                }
            }

            if (!_isGameState)
            {
                _cfgTabs[VideoTab].SetOptionEnabled(Platform.Video.Renderer, false);
                _cfgTabs[VideoTab].SetOptionEnabled(Platform.Video.Hwsurface, false);
                _cfgTabs[VideoTab].SetOptionEnabled(Platform.Video.Vsync, false);
                _cfgTabs[VideoTab].SetOptionEnabled(Platform.Video.TextureFilter, false);
                _cfgTabs[VideoTab].SetOptionEnabled(Platform.Video.FrameLimit, false);

                _cfgTabs[InterfaceTab].SetOptionEnabled(Platform.Interface.Language, false);
                _cfgTabs[InterfaceTab].SetOptionEnabled(Platform.Interface.DevMode, false);

                TabControl.SetEnabled((uint)ModsTab, false);
                TabControl.SetEnabled((uint)ExitTab, true);
                _enableGamestateButtons = false;
            }
            else
            {
                _cfgTabs[ExitTab].SetOptionEnabled(ExitOptionContinue, false);
                _cfgTabs[ExitTab].SetOptionEnabled(ExitOptionSave, false);
                _cfgTabs[ExitTab].SetOptionEnabled(ExitOptionExit, false);
                _cfgTabs[ExitTab].SetOptionEnabled(ExitOptionTimePlayed, false);
                TabControl.SetEnabled((uint)ExitTab, false);
                _enableGamestateButtons = true;
            }

            for (int i = 0; i < Platform.Video.Count; ++i)
            {
                if (!Platform.Instance.ConfigVideo[i])
                    _cfgTabs[VideoTab].SetOptionEnabled(i, false);
            }
            for (int i = 0; i < Platform.Audio.Count; ++i)
            {
                if (!Platform.Instance.ConfigAudio[i])
                    _cfgTabs[AudioTab].SetOptionEnabled(i, false);
            }
            for (int i = 0; i < Platform.Game.Count; ++i)
            {
                if (!Platform.Instance.ConfigGame[i])
                    _cfgTabs[GameTab].SetOptionEnabled(i, false);
            }
            for (int i = 0; i < Platform.Interface.Count; ++i)
            {
                if (!Platform.Instance.ConfigInterface[i])
                    _cfgTabs[InterfaceTab].SetOptionEnabled(i, false);
            }
            for (int i = 0; i < Platform.Input.Count; ++i)
            {
                if (!Platform.Instance.ConfigInput[i])
                    _cfgTabs[InputTab].SetOptionEnabled(i, false);
            }
            if (!Platform.Instance.ConfigMisc[Platform.Misc.Keybinds])
            {
                for (int i = 0; i < KeybindsLstb.Count; ++i)
                {
                    _cfgTabs[KeybindsTab].SetOptionEnabled(i, false);
                }
            }
            if (!Platform.Instance.ConfigMisc[Platform.Misc.Mods])
            {
                TabControl.SetEnabled((uint)ModsTab, false);
            }

            if (!eset.Misc.MouseMoveEnabled)
            {
                _cfgTabs[InputTab].SetOptionEnabled(Platform.Input.MouseMove, false);
                _cfgTabs[InputTab].SetOptionEnabled(Platform.Input.MouseMoveSwap, false);
                _cfgTabs[InputTab].SetOptionEnabled(Platform.Input.MouseMoveAttack, false);
            }

            for (int i = 0; i < _cfgTabs.Count; ++i)
            {
                if (_cfgTabs[i].EnabledCount == 0 && i != GameTab)
                {
                    TabControl.SetEnabled((uint)i, false);
                }

                _cfgTabs[i].Scrollbox = new WidgetScrollBox(Scrollpane.Width, Scrollpane.Height);
                _cfgTabs[i].Scrollbox!.SetBasePos(Scrollpane.X, Scrollpane.Y, Utils.AlignTopLeft);
                _cfgTabs[i].Scrollbox.Bg = ScrollpaneColor;
                _cfgTabs[i].Scrollbox.Resize(Scrollpane.Width, _cfgTabs[i].EnabledCount * ScrollpanePadding.Y);

                for (int j = 0; j < _cfgTabs[i].Options.Count; ++j)
                {
                    PlaceLabeledWidgetAuto(i, j);
                }
            }

            AddChildWidgets();
            SetupTabList();

            RefreshWidgets();

            Update();
        }

        public void ReadConfig()
        {
            var msg = SharedResources.Msg!;
            FileParser infile = new FileParser();
            if (infile.Open("menus/config.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (ParseKeyButtons(infile))
                        continue;

                    string val = infile.Val;
                    int x1 = Parse.PopFirstInt(ref val);
                    int y1 = Parse.PopFirstInt(ref val);
                    int x2 = Parse.PopFirstInt(ref val);
                    int y2 = Parse.PopFirstInt(ref val);

                    if (ParseKey(infile, ref x1, ref y1, ref x2, ref y2))
                        continue;
                    else
                        infile.Error("MenuConfig: '%s' is not a valid key.", infile.Key);
                }
                infile.Close();
            }

            HwsurfaceCb!.Tooltip = msg.Get("Will try to store surfaces in video memory versus system memory. The effect this has on performance depends on the renderer.");
            VsyncCb!.Tooltip = msg.Get("Prevents screen tearing. Disable if you experience \"stuttering\" in windowed mode or input lag.");
            DpiScalingCb!.Tooltip = msg.Get("When enabled, this uses the screen DPI in addition to the window dimensions to scale the rendering resolution. Otherwise, only the window dimensions are used.");
            ParallaxLayersCb!.Tooltip = msg.Get("This enables parallax (non-tile) layers. Disabling this setting can improve performance in some cases.");
            ThreadedImageLoadCb!.Tooltip = msg.Get("Use multiple CPU threads when loading some images. Try disabling this option if you experience instability during loading.");
            FadeWallsCb!.Tooltip = msg.Get("Lowers the opacity of wall tiles that are covering the player. Disabling this option may improve performace in some circumstances.");
            ColorblindCb!.Tooltip = msg.Get("Provides additional text for information that is primarily conveyed through color.");
            StatbarAutohideCb!.Tooltip = msg.Get("Some mods will automatically hide the stat bars when they are inactive. Disabling this option will keep them displayed at all times.");
            AutoEquipCb!.Tooltip = msg.Get("When enabled, empty equipment slots will be filled with applicable items when they are obtained.");
            ItemCompareTipsCb!.Tooltip = msg.Get("When enabled, tooltips for equipped items of the same type are shown next to standard item tooltips.");
            NoMouseCb!.Tooltip = msg.Get("This allows the game to be controlled entirely with the keyboard (or joystick).");
            MouseMoveSwapCb!.Tooltip = msg.Get("When 'Move hero using mouse' is enabled, this setting controls if 'Main1' or 'Main2' is used to move the hero. If enabled, 'Main2' will move the hero instead of 'Main1'.");
            MouseMoveAttackCb!.Tooltip = msg.Get("When 'Move hero using mouse' is enabled, this setting controls if the Power assigned to the movement button can be used by targeting an enemy. If this setting is disabled, it is required to use 'Shift' to access the Power assigned to the movement button.");
            MouseAimCb!.Tooltip = msg.Get("The player's attacks will be aimed in the direction of the mouse cursor when this is enabled.");
            TouchControlsCb!.Tooltip = msg.Get("When enabled, a virtual gamepad will be added in-game. Other interactions, such as drag-and-drop behavior, are also altered to better suit touch input.");

            if (infile.Open("menus/actionbar.txt", FileParser.ModFile, FileParser.ErrorNone))
            {
                while (infile.Next())
                {
                    if (infile.Key == "slot")
                    {
                        string val = infile.Val;
                        uint index = (uint)Parse.PopFirstInt(ref val);
                        if (index > 0 && index <= 10)
                        {
                            _keybindsVisibleActionbar[(int)index - 1] = true;
                        }
                    }
                }
                infile.Close();
            }
            else
            {
                for (int i = 0; i < _keybindsVisibleActionbar.Count; ++i)
                {
                    _keybindsVisibleActionbar[i] = true;
                }
            }

            if (infile.Open("menus/character.txt", FileParser.ModFile, FileParser.ErrorNone))
            {
                while (infile.Next())
                {
                    if (infile.Key == "enabled")
                    {
                        _keybindsVisibleMenus[0] = Parse.ToBool(infile.Val);
                    }
                }
                infile.Close();
            }

            if (infile.Open("menus/inventory.txt", FileParser.ModFile, FileParser.ErrorNone))
            {
                while (infile.Next())
                {
                    if (infile.Key == "enabled")
                    {
                        _keybindsVisibleMenus[1] = Parse.ToBool(infile.Val);
                    }
                    else if (infile.Key == "set_button" || infile.Key == "set_previous" || infile.Key == "set_next" || infile.Key == "label_equipment_set")
                    {
                        _keybindsVisibleEquipswap = true;
                    }
                    else if (infile.Key == "equipment_slot")
                    {
                        string val = infile.Val;
                        Parse.PopFirstInt(ref val);
                        Parse.PopFirstInt(ref val);
                        Parse.PopFirstString(ref val);
                        int equipSet = Parse.PopFirstInt(ref val);
                        if (equipSet > 0)
                        {
                            _keybindsVisibleEquipswap = true;
                        }
                    }
                }
                infile.Close();
            }
            else
            {
                _keybindsVisibleEquipswap = true;
            }

            if (infile.Open("menus/powers.txt", FileParser.ModFile, FileParser.ErrorNone))
            {
                while (infile.Next())
                {
                    if (infile.Key == "enabled")
                    {
                        _keybindsVisibleMenus[2] = Parse.ToBool(infile.Val);
                    }
                }
                infile.Close();
            }

            if (infile.Open("menus/log.txt", FileParser.ModFile, FileParser.ErrorNone))
            {
                while (infile.Next())
                {
                    if (infile.Key == "enabled")
                    {
                        _keybindsVisibleMenus[3] = Parse.ToBool(infile.Val);
                    }
                }
                infile.Close();
            }
        }

        public bool ParseKeyButtons(FileParser infile)
        {
            var msg = SharedResources.Msg!;

            if (infile.Key == "button_ok")
            {
                string val = infile.Val;
                int x = Parse.PopFirstInt(ref val);
                int y = Parse.PopFirstInt(ref val);
                int a = Parse.ToAlignment(Parse.PopFirstString(ref val));
                OkButton!.SetBasePos(x, y, a);
            }
            else if (infile.Key == "button_defaults")
            {
                string val = infile.Val;
                int x = Parse.PopFirstInt(ref val);
                int y = Parse.PopFirstInt(ref val);
                int a = Parse.ToAlignment(Parse.PopFirstString(ref val));
                DefaultsButton!.SetBasePos(x, y, a);
            }
            else if (infile.Key == "button_cancel")
            {
                string val = infile.Val;
                int x = Parse.PopFirstInt(ref val);
                int y = Parse.PopFirstInt(ref val);
                int a = Parse.ToAlignment(Parse.PopFirstString(ref val));
                CancelButton!.SetBasePos(x, y, a);
            }
            else if (infile.Key == "show_frame_background")
            {
                ShowFrameBackground = Parse.ToBool(infile.Val);
            }
            else
            {
                return false;
            }

            return true;
        }

        public bool ParseKey(FileParser infile, ref int x1, ref int y1, ref int x2, ref int y2)
        {
            var msg = SharedResources.Msg!;
            if (infile.Key == "listbox_scrollbar_offset")
            {
                ActivemodsLstb!.ScrollbarOffset = x1;
                InactivemodsLstb!.ScrollbarOffset = x1;
            }
            else if (infile.Key == "frame_offset")
            {
                FrameOffset.X = x1;
                FrameOffset.Y = y1;
            }
            else if (infile.Key == "tab_offset")
            {
                TabOffset.X = x1;
                TabOffset.Y = y1;
            }
            else if (infile.Key == "background_offset")
            {
                BackgroundOffset.X = x1;
                BackgroundOffset.Y = y1;
            }
            else if (infile.Key == "activemods")
            {
                PlaceLabeledWidget(ActivemodsLb, ActivemodsLstb, x1, y1, x2, y2, msg.Get("Active Mods"));
                ActivemodsLb!.SetJustify(FontEngine.JustifyCenter);
            }
            else if (infile.Key == "activemods_height")
            {
                ActivemodsLstb!.SetHeight(x1);
            }
            else if (infile.Key == "inactivemods")
            {
                PlaceLabeledWidget(InactivemodsLb, InactivemodsLstb, x1, y1, x2, y2, msg.Get("Available Mods"));
                InactivemodsLb!.SetJustify(FontEngine.JustifyCenter);
            }
            else if (infile.Key == "inactivemods_height")
            {
                InactivemodsLstb!.SetHeight(x1);
            }
            else if (infile.Key == "activemods_shiftup")
            {
                ActivemodsShiftupBtn!.SetBasePos(x1, y1, Utils.AlignTopLeft);
                ActivemodsShiftupBtn.Refresh();
            }
            else if (infile.Key == "activemods_shiftdown")
            {
                ActivemodsShiftdownBtn!.SetBasePos(x1, y1, Utils.AlignTopLeft);
                ActivemodsShiftdownBtn.Refresh();
            }
            else if (infile.Key == "activemods_deactivate")
            {
                ActivemodsDeactivateBtn!.SetLabel(msg.Get("<< Disable"));
                ActivemodsDeactivateBtn.SetBasePos(x1, y1, Utils.AlignTopLeft);
                ActivemodsDeactivateBtn.Refresh();
            }
            else if (infile.Key == "inactivemods_activate")
            {
                InactivemodsActivateBtn!.SetLabel(msg.Get("Enable >>"));
                InactivemodsActivateBtn.SetBasePos(x1, y1, Utils.AlignTopLeft);
                InactivemodsActivateBtn.Refresh();
            }
            else if (infile.Key == "inactivemods_filter")
            {
                InactivemodsFilterLstb!.SetBasePos(x1, y1, Utils.AlignTopLeft);
                InactivemodsFilterLstb.Refresh();
            }
            else if (infile.Key == "scrollpane")
            {
                Scrollpane.X = x1;
                Scrollpane.Y = y1;
                Scrollpane.Width = x2;
                Scrollpane.Height = y2;
            }
            else if (infile.Key == "scrollpane_padding")
            {
                ScrollpanePadding.X = x1;
                ScrollpanePadding.Y = y1;
            }
            else if (infile.Key == "scrollpane_separator_color")
            {
                ScrollpaneSeparatorColor.R = (byte)x1;
                ScrollpaneSeparatorColor.G = (byte)y1;
                ScrollpaneSeparatorColor.B = (byte)x2;
            }
            else if (infile.Key == "scrollpane_bg_color")
            {
                ScrollpaneColor.R = (byte)x1;
                ScrollpaneColor.G = (byte)y1;
                ScrollpaneColor.B = (byte)x2;
                ScrollpaneColor.A = (byte)y2;
            }
            else return false;

            return true;
        }

        public void AddChildWidgets()
        {
            for (int i = 0; i < _cfgTabs.Count; ++i)
            {
                for (int j = 0; j < _cfgTabs[i].Options.Count; ++j)
                {
                    if (_cfgTabs[i].Options[j].Enabled)
                    {
                        _cfgTabs[i].Scrollbox!.AddChildWidget(_cfgTabs[i].Options[j].Widget!);
                        _cfgTabs[i].Scrollbox.AddChildWidget(_cfgTabs[i].Options[j].Label!);
                    }

                    AddChildWidget(_cfgTabs[i].Options[j].Widget, NoTab);
                    AddChildWidget(_cfgTabs[i].Options[j].Label, NoTab);
                }
            }

            AddChildWidget(ActivemodsLstb, ModsTab);
            AddChildWidget(ActivemodsLb, ModsTab);
            AddChildWidget(InactivemodsLstb, ModsTab);
            AddChildWidget(InactivemodsLb, ModsTab);
            AddChildWidget(ActivemodsShiftupBtn, ModsTab);
            AddChildWidget(ActivemodsShiftdownBtn, ModsTab);
            AddChildWidget(ActivemodsDeactivateBtn, ModsTab);
            AddChildWidget(InactivemodsActivateBtn, ModsTab);
            AddChildWidget(InactivemodsFilterLstb, ModsTab);
        }

        public void SetupTabList()
        {
            if (_enableGamestateButtons)
            {
                TablistMain.Add(OkButton);
                TablistMain.Add(DefaultsButton);
                TablistMain.Add(CancelButton);
            }
            TablistMain.Lock();

            TablistExit.SetScrollType(Widget.ScrollVertical);
            TablistExit.Add(_cfgTabs[ExitTab].Scrollbox);
            TablistExit.Lock();

            TablistVideo.SetScrollType(Widget.ScrollVertical);
            TablistVideo.Add(_cfgTabs[VideoTab].Scrollbox);
            TablistVideo.Lock();

            TablistAudio.SetScrollType(Widget.ScrollVertical);
            TablistAudio.Add(_cfgTabs[AudioTab].Scrollbox);
            TablistAudio.Lock();

            TablistGame.SetScrollType(Widget.ScrollVertical);
            TablistGame.Add(_cfgTabs[GameTab].Scrollbox);
            TablistGame.Lock();

            TablistInterface.SetScrollType(Widget.ScrollVertical);
            TablistInterface.Add(_cfgTabs[InterfaceTab].Scrollbox);
            TablistInterface.Lock();

            TablistInput.SetScrollType(Widget.ScrollVertical);
            TablistInput.Add(_cfgTabs[InputTab].Scrollbox);
            TablistInput.Lock();

            TablistKeybinds.SetScrollType(Widget.ScrollVertical);
            TablistKeybinds.Add(_cfgTabs[KeybindsTab].Scrollbox);
            TablistKeybinds.Lock();

            TablistMods.Add(InactivemodsLstb);
            TablistMods.Add(ActivemodsLstb);
            TablistMods.Add(InactivemodsActivateBtn);
            TablistMods.Add(ActivemodsDeactivateBtn);
            TablistMods.Add(ActivemodsShiftupBtn);
            TablistMods.Add(ActivemodsShiftdownBtn);
            TablistMods.Add(InactivemodsFilterLstb);
            TablistMods.Lock();

            Tablists.Clear();
            Tablists.Add(Tablist);
            Tablists.Add(TablistMain);
            Tablists.Add(TablistExit);
            Tablists.Add(TablistVideo);
            Tablists.Add(TablistAudio);
            Tablists.Add(TablistGame);
            Tablists.Add(TablistInterface);
            Tablists.Add(TablistInput);
            Tablists.Add(TablistKeybinds);
            Tablists.Add(TablistMods);
        }

        public void Update()
        {
            UpdateVideo();
            UpdateAudio();
            UpdateGame();
            UpdateInterface();
            UpdateInput();
            UpdateKeybinds();
            UpdateMods();
        }

        public void UpdateVideo()
        {
            var settings = SharedResources.Settings!;

            FullscreenCb!.SetChecked(settings.Fullscreen);
            HwsurfaceCb!.SetChecked(settings.Hwsurface);
            VsyncCb!.SetChecked(settings.Vsync);
            TextureFilterCb!.SetChecked(settings.TextureFilter);
            DpiScalingCb!.SetChecked(settings.DpiScaling);
            ParallaxLayersCb!.SetChecked(settings.ParallaxLayers);
            ThreadedImageLoadCb!.SetChecked(settings.EnableThreadedImageLoad);
            FadeWallsCb!.SetChecked(settings.FadeWalls);

            RefreshRenderers();

            for (int i = 0; i < _frameLimits.Count; ++i)
            {
                if (_frameLimits[i] == settings.MaxFramesPerSec)
                {
                    FrameLimitLstb!.Select((uint)i);
                    break;
                }
            }

            if (settings.MinRenderSize == 0)
            {
                MinRenderSizeLstb!.Select(0);
            }
            else
            {
                for (int i = 0; i < _virtualHeights.Count; ++i)
                {
                    if (_virtualHeights[i] == settings.MinRenderSize)
                    {
                        MinRenderSizeLstb!.Select((uint)(i + 1));
                        break;
                    }
                }
            }

            if (settings.MaxRenderSize == 0)
            {
                MaxRenderSizeLstb!.Select(0);
            }
            else
            {
                for (int i = 0; i < _virtualHeights.Count; ++i)
                {
                    if (_virtualHeights[i] == settings.MaxRenderSize)
                    {
                        MaxRenderSizeLstb!.Select((uint)(i + 1));
                        break;
                    }
                }
            }

            RefreshWindowSize();

            _cfgTabs[VideoTab].Scrollbox!.Refresh();
        }

        public void UpdateAudio()
        {
            var settings = SharedResources.Settings!;
            var snd = SharedResources.Snd!;

            if (settings.Audio)
            {
                MusicVolumeSl!.Set(0, 128, settings.MusicVolume);
                snd.SetVolumeMusic(settings.MusicVolume);
                SoundVolumeSl!.Set(0, 128, settings.SoundVolume);
                snd.SetVolumeSFX(settings.SoundVolume);
            }
            else
            {
                MusicVolumeSl!.Set(0, 128, 0);
                SoundVolumeSl!.Set(0, 128, 0);
            }
            MuteOnFocusLossCb!.SetChecked(settings.MuteOnFocusLoss);

            _cfgTabs[AudioTab].Scrollbox!.Refresh();
        }

        public void UpdateGame()
        {
            var settings = SharedResources.Settings!;

            _cfgTabs[GameTab].Scrollbox!.Refresh();

            AutoEquipCb!.SetChecked(settings.AutoEquip);
            AutoLootLstb!.Select((uint)settings.AutoLoot);
            LowHpWarningLstb!.Select((uint)settings.LowHpWarningType);
            LowHpThresholdLstb!.Select((uint)((settings.LowHpThreshold / 5) - 1));
        }

        public void UpdateInterface()
        {
            var settings = SharedResources.Settings!;

            ShowFpsCb!.SetChecked(settings.ShowFps);
            ColorblindCb!.SetChecked(settings.Colorblind);
            HardwareCursorCb!.SetChecked(settings.HardwareCursor);
            DevModeCb!.SetChecked(settings.DevMode);
            SubtitlesCb!.SetChecked(settings.Subtitles);
            StatbarLabelsCb!.SetChecked(settings.StatbarLabels);
            StatbarAutohideCb!.SetChecked(settings.StatbarAutohide);
            CombatTextCb!.SetChecked(settings.CombatText);
            ItemCompareTipsCb!.SetChecked(settings.ItemCompareTips);
            PauseOnFocusLossCb!.SetChecked(settings.PauseOnFocusLoss);

            LootTooltipLstb!.Select((uint)settings.LootTooltips);
            MinimapLstb!.Select((uint)settings.MinimapMode);

            RefreshLanguages();

            _cfgTabs[InterfaceTab].Scrollbox!.Refresh();
        }

        public void UpdateInput()
        {
            var settings = SharedResources.Settings!;
            var eset = SharedResources.Eset!;
            var inpt = SharedResources.Inpt!;

            MouseAimCb!.SetChecked(settings.MouseAim);
            NoMouseCb!.SetChecked(settings.NoMouse);
            if (eset.Misc.MouseMoveEnabled)
            {
                MouseMoveCb!.SetChecked(settings.MouseMove);
                MouseMoveSwapCb!.SetChecked(settings.MouseMoveSwap);
                MouseMoveAttackCb!.SetChecked(settings.MouseMoveAttack);
            }
            JoystickRumbleCb!.SetChecked(settings.JoystickRumble);
            TouchControlsCb!.SetChecked(settings.Touchscreen);

            if (settings.EnableJoystick && inpt.GetNumJoysticks() > 0)
            {
                inpt.InitJoystick();
                JoystickDeviceLstb!.Select((uint)(settings.JoystickDevice + 1));
            }

            JoystickDeadzoneSl!.Set(Settings.JoyDeadzoneMin, Settings.JoyDeadzoneMax, settings.JoyDeadzone);
            TouchScaleSl!.Set(TouchScaleMin, TouchScaleMax, (int)(settings.TouchScale * 100.0));

            _cfgTabs[InputTab].Scrollbox!.Refresh();
        }

        public void UpdateKeybinds()
        {
            var inpt = SharedResources.Inpt!;
            var msg = SharedResources.Msg!;

            for (int i = 0; i < KeybindsLstb.Count; i++)
            {
                KeybindsLstb[i]!.Clear();
                if (inpt.Binding[i].Count == 0)
                {
                    KeybindsLstb[i]!.Append(inpt.GetBindingStringByIndex(i, -1), "");
                }
                else
                {
                    string tooltipText = msg.Get("Bindings for:") + " " + inpt.BindingName[i] + "\n";

                    for (int j = 0; j < inpt.Binding[i].Count; ++j)
                    {
                        tooltipText += inpt.GetBindingStringByIndex(i, j);
                        if (j + 1 != inpt.Binding[i].Count)
                        {
                            tooltipText += "\n";
                        }
                    }
                    for (int j = 0; j < inpt.Binding[i].Count; ++j)
                    {
                        KeybindsLstb[i]!.Append(inpt.GetBindingStringByIndex(i, j), tooltipText);
                    }
                }
                KeybindsLstb[i]!.Refresh();
            }
            _cfgTabs[KeybindsTab].Scrollbox!.Refresh();
        }

        public void UpdateMods()
        {
            ActivemodsLstb!.Refresh();
            InactivemodsLstb!.Refresh();
        }

        public void Logic()
        {
            var inpt = SharedResources.Inpt!;
            var settings = SharedResources.Settings!;

            if (inpt.WindowResized)
                RefreshWidgets();

            if (DefaultsConfirm!.Visible)
            {
                LogicDefaults();
                return;
            }
            else if (InputConfirm!.Visible)
            {
                InputConfirm.Logic();
                ScanKey(InputAction);

                if (!InputConfirm.ActionList!.Enabled)
                    InputConfirmTimer.Tick();

                if (InputConfirmTimer.IsEnd())
                    InputConfirm.Visible = false;

                return;
            }
            else
            {
                if (!LogicMain())
                    return;
            }

            ActiveTab = TabControl!.GetActiveTab();

            if (ActiveTab == ExitTab)
            {
                if (_hero != null)
                {
                    PauseTimeText!.SetText(Utils.GetTimeString(_hero.TimePlayed));
                }
                Tablist.SetNextTabList(TablistExit);
                TablistMain.SetPrevTabList(TablistExit);
                LogicExit();
            }
            if (ActiveTab == VideoTab)
            {
                Tablist.SetNextTabList(TablistVideo);
                TablistMain.SetPrevTabList(TablistVideo);
                LogicVideo();
            }
            else if (ActiveTab == AudioTab)
            {
                Tablist.SetNextTabList(TablistAudio);
                TablistMain.SetPrevTabList(TablistAudio);
                LogicAudio();
            }
            else if (ActiveTab == GameTab)
            {
                Tablist.SetNextTabList(TablistGame);
                TablistMain.SetPrevTabList(TablistGame);
                LogicGame();
            }
            else if (ActiveTab == InterfaceTab)
            {
                Tablist.SetNextTabList(TablistInterface);
                TablistMain.SetPrevTabList(TablistInterface);
                LogicInterface();

                if (Platform.Instance.ForceHardwareCursor)
                {
                    settings.HardwareCursor = true;
                    HardwareCursorCb!.SetChecked(settings.HardwareCursor);
                }
            }
            else if (ActiveTab == InputTab)
            {
                Tablist.SetNextTabList(TablistInput);
                TablistMain.SetPrevTabList(TablistInput);
                LogicInput();
            }
            else if (ActiveTab == KeybindsTab)
            {
                Tablist.SetNextTabList(TablistKeybinds);
                TablistMain.SetPrevTabList(TablistKeybinds);
                LogicKeybinds();
            }
            else if (ActiveTab == ModsTab)
            {
                Tablist.SetNextTabList(TablistMods);
                TablistMain.SetPrevTabList(TablistMods);
                LogicMods();
            }
        }

        public bool LogicMain()
        {
            var inpt = SharedResources.Inpt!;

            for (int i = 0; i < ChildWidget.Count; i++)
            {
                if (ChildWidget[i]!.InFocus && Optiontab[i] != NoTab)
                {
                    TabControl!.SetActiveTab((uint)Optiontab[i]);
                    break;
                }
            }

            TabControl!.Logic();

            for (int i = 0; i < Tablists.Count; ++i)
            {
                Tablists[i]!.Logic();
                if (!inpt.UsingMouse() && !Tablists[i]!.IsLocked() && Tablists[i]!.GetCurrent() == -1 && TablistMain.GetCurrent() == -1)
                {
                    Tablists[i]!.GetNext(!TabList.GetInner, TabList.WidgetSelectAuto);
                }
            }

            if (_enableGamestateButtons)
            {
                if (OkButton!.CheckClick())
                {
                    ClickedAccept = true;
                    return false;
                }
                else if (DefaultsButton!.CheckClick())
                {
                    DefaultsConfirm!.Show();
                    return true;
                }
                else if (CancelButton!.CheckClick() || (inpt.UsingMouse() && inpt.Pressing[Input.Cancel] && !inpt.Lock[Input.Cancel]))
                {
                    if (inpt.Pressing[Input.Cancel])
                        inpt.Lock[Input.Cancel] = true;

                    ClickedCancel = true;
                    return false;
                }
                else if (!inpt.UsingMouse() && inpt.Pressing[Input.Cancel] && !inpt.Lock[Input.Cancel])
                {
                    inpt.Lock[Input.Cancel] = true;

                    if (TablistMain.GetCurrent() == -1)
                    {
                        for (int i = 0; i < Tablists.Count; ++i)
                        {
                            Tablists[i]!.Defocus();
                        }
                        Tablist.Lock();
                        TablistMain.Unlock();
                        TablistMain.GetNext(!TabList.GetInner, TabList.WidgetSelectAuto);
                    }
                    else
                    {
                        TablistMain.Defocus();
                        TablistMain.Lock();
                        Tablist.Unlock();
                        Tablist.GetNext(!TabList.GetInner, TabList.WidgetSelectAuto);
                    }
                }
            }

            return true;
        }

        public void LogicDefaults()
        {
            var settings = SharedResources.Settings!;
            var renderDevice = SharedResources.RenderDevice!;
            var eset = SharedResources.Eset!;
            var inpt = SharedResources.Inpt!;

            DefaultsConfirm!.Logic();
            if (DefaultsConfirm.ClickedConfirm)
            {
                if (DefaultsConfirm.ActionList!.GetSelected() == DefaultsConfirmOptionYes)
                {
                    settings.Fullscreen = false;
                    settings.LoadDefaults();
                    renderDevice.SetFullscreen(settings.Fullscreen);
                    eset.Load();
                    inpt.InitBindings();
                    inpt.LoadKeyBindings(!InputState.LoadUserBinds);
                    Update();
                    RefreshWindowSize();
                }
                DefaultsConfirm.Visible = false;
                DefaultsConfirm.ClickedConfirm = false;
            }
        }

        public void LogicExit()
        {
            _cfgTabs[ExitTab].Scrollbox!.Logic();
            Int2 mouse = _cfgTabs[ExitTab].Scrollbox.InputAssist(SharedResources.Inpt!.Mouse);

            if (_cfgTabs[ExitTab].Options[ExitOptionContinue].Enabled && PauseContinueBtn!.CheckClickAt(mouse.X, mouse.Y))
            {
                ClickedPauseContinue = true;
            }
            else if (_cfgTabs[ExitTab].Options[ExitOptionSave].Enabled && PauseSaveBtn!.CheckClickAt(mouse.X, mouse.Y))
            {
                ClickedPauseSave = true;
            }
            else if (_cfgTabs[ExitTab].Options[ExitOptionExit].Enabled && PauseExitBtn!.CheckClickAt(mouse.X, mouse.Y))
            {
                ClickedPauseExit = true;
            }
        }

        public void LogicVideo()
        {
            var inpt = SharedResources.Inpt!;
            var settings = SharedResources.Settings!;
            var renderDevice = SharedResources.RenderDevice!;

            _cfgTabs[VideoTab].Scrollbox!.Logic();
            Int2 mouse = _cfgTabs[VideoTab].Scrollbox.InputAssist(inpt.Mouse);

            if (_cfgTabs[VideoTab].Options[Platform.Video.Fullscreen].Enabled && FullscreenCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.Fullscreen = FullscreenCb.IsChecked;
                renderDevice.SetFullscreen(settings.Fullscreen);
                RefreshWindowSize();
            }
            else if (_cfgTabs[VideoTab].Options[Platform.Video.Hwsurface].Enabled && HwsurfaceCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.Hwsurface = HwsurfaceCb.IsChecked;
            }
            else if (_cfgTabs[VideoTab].Options[Platform.Video.Vsync].Enabled && VsyncCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.Vsync = VsyncCb.IsChecked;
            }
            else if (_cfgTabs[VideoTab].Options[Platform.Video.TextureFilter].Enabled && TextureFilterCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.TextureFilter = TextureFilterCb.IsChecked;
            }
            else if (_cfgTabs[VideoTab].Options[Platform.Video.DpiScaling].Enabled && DpiScalingCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.DpiScaling = DpiScalingCb.IsChecked;
                RefreshWindowSize();
            }
            else if (_cfgTabs[VideoTab].Options[Platform.Video.ParallaxLayers].Enabled && ParallaxLayersCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.ParallaxLayers = ParallaxLayersCb.IsChecked;
            }
            else if (_cfgTabs[VideoTab].Options[Platform.Video.Renderer].Enabled && RendererLstb!.CheckClickAt(mouse.X, mouse.Y))
            {
                NewRenderDevice = RendererLstb.GetValue();
            }
            else if (_cfgTabs[VideoTab].Options[Platform.Video.FrameLimit].Enabled && FrameLimitLstb!.CheckClickAt(mouse.X, mouse.Y))
            {
            }
            else if (_cfgTabs[VideoTab].Options[Platform.Video.MinRenderSize].Enabled && MinRenderSizeLstb!.CheckClickAt(mouse.X, mouse.Y))
            {
                int index = (int)MinRenderSizeLstb.GetSelected();
                if (index == 0)
                {
                    settings.MinRenderSize = 0;
                }
                else
                {
                    settings.MinRenderSize = _virtualHeights[index - 1];
                    if (settings.MaxRenderSize < settings.MinRenderSize)
                    {
                        settings.MaxRenderSize = 0;
                        MaxRenderSizeLstb!.Select(0);
                    }
                }
                RefreshWindowSize();
            }
            else if (_cfgTabs[VideoTab].Options[Platform.Video.MaxRenderSize].Enabled && MaxRenderSizeLstb!.CheckClickAt(mouse.X, mouse.Y))
            {
                int index = (int)MaxRenderSizeLstb.GetSelected();
                if (index == 0)
                {
                    settings.MaxRenderSize = 0;
                }
                else
                {
                    settings.MaxRenderSize = _virtualHeights[index - 1];
                    if (settings.MaxRenderSize < settings.MinRenderSize)
                    {
                        settings.MinRenderSize = 0;
                        MinRenderSizeLstb!.Select(0);
                    }
                }
                RefreshWindowSize();
            }
            else if (_cfgTabs[VideoTab].Options[Platform.Video.ThreadedImageLoad].Enabled && ThreadedImageLoadCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.EnableThreadedImageLoad = ThreadedImageLoadCb.IsChecked;
            }
            else if (_cfgTabs[VideoTab].Options[Platform.Video.FadeWalls].Enabled && FadeWallsCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.FadeWalls = FadeWallsCb.IsChecked;
            }
        }

        public void LogicAudio()
        {
            var inpt = SharedResources.Inpt!;
            var settings = SharedResources.Settings!;
            var snd = SharedResources.Snd!;

            _cfgTabs[AudioTab].Scrollbox!.Logic();
            Int2 mouse = _cfgTabs[AudioTab].Scrollbox.InputAssist(inpt.Mouse);

            if (_cfgTabs[AudioTab].Options[Platform.Audio.MuteOnFocusLoss].Enabled && MuteOnFocusLossCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.MuteOnFocusLoss = MuteOnFocusLossCb.IsChecked;
            }
            else if (settings.Audio)
            {
                if (_cfgTabs[AudioTab].Options[Platform.Audio.Music].Enabled && MusicVolumeSl!.CheckClickAt(mouse.X, mouse.Y))
                {
                    if (settings.MusicVolume == 0)
                        ReloadMusic = true;
                    settings.MusicVolume = (ushort)MusicVolumeSl.Value;
                    snd.SetVolumeMusic(settings.MusicVolume);
                }
                else if (_cfgTabs[AudioTab].Options[Platform.Audio.Sfx].Enabled && SoundVolumeSl!.CheckClickAt(mouse.X, mouse.Y))
                {
                    settings.SoundVolume = (ushort)SoundVolumeSl.Value;
                    snd.SetVolumeSFX(settings.SoundVolume);
                }
            }
        }

        public void LogicGame()
        {
            var inpt = SharedResources.Inpt!;
            var settings = SharedResources.Settings!;

            _cfgTabs[GameTab].Scrollbox!.Logic();
            Int2 mouse = _cfgTabs[GameTab].Scrollbox.InputAssist(inpt.Mouse);

            if (_cfgTabs[GameTab].Options[Platform.Game.AutoEquip].Enabled && AutoEquipCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.AutoEquip = AutoEquipCb.IsChecked;
            }
            else if (_cfgTabs[GameTab].Options[Platform.Game.AutoLoot].Enabled && AutoLootLstb!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.AutoLoot = (int)AutoLootLstb.GetSelected();
            }
            else if (_cfgTabs[GameTab].Options[Platform.Game.LowHpWarningType].Enabled && LowHpWarningLstb!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.LowHpWarningType = (int)LowHpWarningLstb.GetSelected();
            }
            else if (_cfgTabs[GameTab].Options[Platform.Game.LowHpThreshold].Enabled && LowHpThresholdLstb!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.LowHpThreshold = ((int)LowHpThresholdLstb.GetSelected() + 1) * 5;
            }
        }

        public void LogicInterface()
        {
            var inpt = SharedResources.Inpt!;
            var settings = SharedResources.Settings!;

            _cfgTabs[InterfaceTab].Scrollbox!.Logic();
            Int2 mouse = _cfgTabs[InterfaceTab].Scrollbox.InputAssist(inpt.Mouse);

            if (_cfgTabs[InterfaceTab].Options[Platform.Interface.Language].Enabled && LanguageLstb!.CheckClickAt(mouse.X, mouse.Y))
            {
                uint langId = LanguageLstb.GetSelected();
                if (langId != LanguageLstb.GetSize())
                    settings.Language = LanguageIso[(int)langId];
            }
            else if (_cfgTabs[InterfaceTab].Options[Platform.Interface.ShowFps].Enabled && ShowFpsCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.ShowFps = ShowFpsCb.IsChecked;
            }
            else if (_cfgTabs[InterfaceTab].Options[Platform.Interface.Colorblind].Enabled && ColorblindCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.Colorblind = ColorblindCb.IsChecked;
            }
            else if (_cfgTabs[InterfaceTab].Options[Platform.Interface.HardwareCursor].Enabled && HardwareCursorCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.HardwareCursor = HardwareCursorCb.IsChecked;
            }
            else if (_cfgTabs[InterfaceTab].Options[Platform.Interface.DevMode].Enabled && DevModeCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.DevMode = DevModeCb.IsChecked;
            }
            else if (_cfgTabs[InterfaceTab].Options[Platform.Interface.Subtitles].Enabled && SubtitlesCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.Subtitles = SubtitlesCb.IsChecked;
            }
            else if (_cfgTabs[InterfaceTab].Options[Platform.Interface.LootTooltips].Enabled && LootTooltipLstb!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.LootTooltips = (int)LootTooltipLstb.GetSelected();
            }
            else if (_cfgTabs[InterfaceTab].Options[Platform.Interface.MinimapMode].Enabled && MinimapLstb!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.MinimapMode = (int)MinimapLstb.GetSelected();
            }
            else if (_cfgTabs[InterfaceTab].Options[Platform.Interface.StatbarLabels].Enabled && StatbarLabelsCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.StatbarLabels = StatbarLabelsCb.IsChecked;
            }
            else if (_cfgTabs[InterfaceTab].Options[Platform.Interface.StatbarAutohide].Enabled && StatbarAutohideCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.StatbarAutohide = StatbarAutohideCb.IsChecked;
            }
            else if (_cfgTabs[InterfaceTab].Options[Platform.Interface.CombatText].Enabled && CombatTextCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.CombatText = CombatTextCb.IsChecked;
            }
            else if (_cfgTabs[InterfaceTab].Options[Platform.Interface.ItemCompareTips].Enabled && ItemCompareTipsCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.ItemCompareTips = ItemCompareTipsCb.IsChecked;
            }
            else if (_cfgTabs[InterfaceTab].Options[Platform.Interface.PauseOnFocusLoss].Enabled && PauseOnFocusLossCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.PauseOnFocusLoss = PauseOnFocusLossCb.IsChecked;
            }
        }

        public void LogicInput()
        {
            var inpt = SharedResources.Inpt!;
            var settings = SharedResources.Settings!;

            _cfgTabs[InputTab].Scrollbox!.Logic();
            Int2 mouse = _cfgTabs[InputTab].Scrollbox.InputAssist(inpt.Mouse);

            if (inpt.JoysticksChanged)
            {
                RefreshJoysticks();
                if (settings.EnableJoystick && inpt.GetNumJoysticks() > 0)
                {
                    inpt.InitJoystick();
                    JoystickDeviceLstb!.Select((uint)(settings.JoystickDevice + 1));
                }
                else
                {
                    JoystickDeviceLstb!.Select(0);
                }

                inpt.JoysticksChanged = false;
            }

            if (_cfgTabs[InputTab].Options[Platform.Input.MouseMove].Enabled && MouseMoveCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                if (MouseMoveCb.IsChecked)
                {
                    settings.MouseMove = true;
                    EnableMouseOptions();
                }
                else settings.MouseMove = false;
            }
            else if (_cfgTabs[InputTab].Options[Platform.Input.MouseAim].Enabled && MouseAimCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                if (MouseAimCb.IsChecked)
                {
                    settings.MouseAim = true;
                    EnableMouseOptions();
                }
                else settings.MouseAim = false;
            }
            else if (_cfgTabs[InputTab].Options[Platform.Input.NoMouse].Enabled && NoMouseCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                if (NoMouseCb.IsChecked)
                {
                    settings.NoMouse = true;
                    DisableMouseOptions();
                }
                else settings.NoMouse = false;
            }
            else if (_cfgTabs[InputTab].Options[Platform.Input.MouseMoveSwap].Enabled && MouseMoveSwapCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.MouseMoveSwap = MouseMoveSwapCb.IsChecked;
            }
            else if (_cfgTabs[InputTab].Options[Platform.Input.MouseMoveAttack].Enabled && MouseMoveAttackCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.MouseMoveAttack = MouseMoveAttackCb.IsChecked;
            }
            else if (_cfgTabs[InputTab].Options[Platform.Input.JoystickDeadzone].Enabled && JoystickDeadzoneSl!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.JoyDeadzone = JoystickDeadzoneSl.Value;
            }
            else if (_cfgTabs[InputTab].Options[Platform.Input.Joystick].Enabled && JoystickDeviceLstb!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.JoystickDevice = (int)JoystickDeviceLstb.GetSelected() - 1;
                settings.EnableJoystick = (settings.JoystickDevice != -1);
                inpt.JoysticksChanged = true;
                inpt.InitJoystick();
                inpt.JoysticksChanged = false;
            }
            else if (_cfgTabs[InputTab].Options[Platform.Input.JoystickRumble].Enabled && JoystickRumbleCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.JoystickRumble = JoystickRumbleCb.IsChecked;
            }
            else if (_cfgTabs[InputTab].Options[Platform.Input.TouchControls].Enabled && TouchControlsCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.Touchscreen = TouchControlsCb.IsChecked;
            }
            else if (_cfgTabs[InputTab].Options[Platform.Input.TouchScale].Enabled && TouchScaleSl!.CheckClickAt(mouse.X, mouse.Y))
            {
                settings.TouchScale = (float)TouchScaleSl.Value * 0.01f;
            }
        }

        public void LogicKeybinds()
        {
            var inpt = SharedResources.Inpt!;
            var msg = SharedResources.Msg!;

            _cfgTabs[KeybindsTab].Scrollbox!.Logic();
            Int2 mouse = _cfgTabs[KeybindsTab].Scrollbox.InputAssist(inpt.Mouse);

            for (int i = 0; i < KeybindsLstb.Count; i++)
            {
                if (!_cfgTabs[KeybindsTab].Options[i].Enabled)
                    continue;

                if (KeybindsLstb[i]!.CheckClickAt(mouse.X, mouse.Y))
                {
                    if (KeybindsLstb[i]!.CheckAction())
                    {
                        InputConfirm!.SetTitle(msg.Get("Assign:") + ' ' + inpt.BindingName[i]);
                        InputConfirmTimer.Reset(Timer.Begin);
                        InputConfirm.Show();
                        InputAction = i;
                        inpt.LastButton = -1;
                        inpt.LastKey = -1;
                        inpt.LastJoybutton = -1;
                    }
                }
            }
        }

        public void LogicMods()
        {
            if (ActivemodsLstb!.CheckClick())
            {
            }
            else if (InactivemodsLstb!.CheckClick())
            {
            }
            else if (ActivemodsShiftupBtn!.CheckClick())
            {
                ActivemodsLstb.ShiftUp();
            }
            else if (ActivemodsShiftdownBtn!.CheckClick())
            {
                ActivemodsLstb.ShiftDown();
            }
            else if (ActivemodsDeactivateBtn!.CheckClick())
            {
                DisableMods();
            }
            else if (InactivemodsActivateBtn!.CheckClick())
            {
                EnableMods();
            }
            else if (InactivemodsFilterLstb!.CheckClick())
            {
                FilterMods();
            }
        }

        public void Render()
        {
            var settings = SharedResources.Settings!;
            var eset = SharedResources.Eset!;
            var renderDevice = SharedResources.RenderDevice!;

            Rectangle pos = new Rectangle();
            pos.X = (settings.ViewW - eset.Resolutions.FrameW) / 2 + BackgroundOffset.X;
            pos.Y = (settings.ViewH - eset.Resolutions.FrameH) / 2 + BackgroundOffset.Y;

            if (Background != null)
            {
                Background.SetDestFromRect(pos);
                renderDevice.Render(Background);
            }

            TabControl!.Render();

            if (_enableGamestateButtons)
            {
                OkButton!.Render();
                CancelButton!.Render();
                DefaultsButton!.Render();
            }

            RenderTabContents();
            RenderDialogs();
        }

        public void RenderTabContents()
        {
            if (ActiveTab == ExitTab)
            {
                if (_cfgTabs[ActiveTab].Scrollbox!.Update)
                {
                    _cfgTabs[ActiveTab].Scrollbox.Refresh();

                    Image? renderTarget = _cfgTabs[ActiveTab].Scrollbox.Contents!.GetGraphics();
                    int offset = (EnableSaveGame && SharedResources.Eset!.Misc.SaveAnywhere) ? 3 : 2;
                    renderTarget!.DrawLine(ScrollpanePadding.X, offset * ScrollpanePadding.Y, Scrollpane.Width - ScrollpanePadding.X - 1, offset * ScrollpanePadding.Y, ScrollpaneSeparatorColor);
                }
                _cfgTabs[ActiveTab].Scrollbox.Render();
            }
            else if (ActiveTab <= KeybindsTab)
            {
                if (_cfgTabs[ActiveTab].Scrollbox!.Update)
                {
                    _cfgTabs[ActiveTab].Scrollbox.Refresh();

                    Image? renderTarget = _cfgTabs[ActiveTab].Scrollbox.Contents!.GetGraphics();

                    for (int i = 1; i < _cfgTabs[ActiveTab].EnabledCount; ++i)
                    {
                        renderTarget!.DrawLine(ScrollpanePadding.X, i * ScrollpanePadding.Y, Scrollpane.Width - ScrollpanePadding.X - 1, i * ScrollpanePadding.Y, ScrollpaneSeparatorColor);
                    }
                }
                _cfgTabs[ActiveTab].Scrollbox.Render();
            }

            for (int i = 0; i < ChildWidget.Count; i++)
            {
                if (Optiontab[i] == ActiveTab && Optiontab[i] != NoTab)
                    ChildWidget[i]!.Render();
            }
        }

        public void RenderDialogs()
        {
            if (DefaultsConfirm!.Visible)
                DefaultsConfirm.Render();

            if (InputConfirm!.Visible)
                InputConfirm.Render();

            if (ActiveTab == KeybindsTab && KeybindMsg.Length != 0)
            {
                TooltipData keybindTipData = new TooltipData();
                keybindTipData.AddText(KeybindMsg);

                if (KeybindTipTimer.IsEnd())
                    KeybindTipTimer.Reset(Timer.Begin);

                KeybindTipTimer.Tick();

                if (!KeybindTipTimer.IsEnd())
                {
                    KeybindTip!.Render(keybindTipData, new Int2(SharedResources.Settings!.ViewW, 0), TooltipData.StyleFloat);
                }
                else
                {
                    KeybindMsg = "";
                }
            }
            else
            {
                KeybindMsg = "";
                KeybindTipTimer.Reset(Timer.End);
            }
        }

        public void PlaceLabeledWidget(WidgetLabel? lb, Widget? w, int x1, int y1, int x2, int y2, string str, int justify = 0)
        {
            if (w != null)
            {
                w.SetBasePos(x2, y2, Utils.AlignTopLeft);
            }

            if (lb != null)
            {
                lb.SetBasePos(x1, y1, Utils.AlignTopLeft);
                lb.SetText(str);
                lb.SetJustify(justify);
            }
        }

        public void PlaceLabeledWidgetAuto(int tab, int cfgIndex)
        {
            WidgetLabel? lb = _cfgTabs[tab].Options[cfgIndex].Label;
            Widget? w = _cfgTabs[tab].Options[cfgIndex].Widget;
            int enabledIndex = _cfgTabs[tab].GetEnabledIndex(cfgIndex);

            if (w != null)
            {
                int yOffset = Math.Max(ScrollpanePadding.Y - w.Pos.Height, 0) / 2;
                w.SetBasePos(Scrollpane.Width - w.Pos.Width - ScrollpanePadding.X, (enabledIndex * ScrollpanePadding.Y) + yOffset, Utils.AlignTopLeft);
                w.SetPos(0, 0);
            }

            if (lb != null)
            {
                lb.SetBasePos(ScrollpanePadding.X, (enabledIndex * ScrollpanePadding.Y) + ScrollpanePadding.Y / 2, Utils.AlignTopLeft);
                lb.SetPos(0, 0);
                lb.SetVAlign(LabelInfo.ValignCenter);
            }
        }

        public void RefreshWidgets()
        {
            var settings = SharedResources.Settings!;
            var eset = SharedResources.Eset!;

            TabControl!.SetMainArea(((settings.ViewW - eset.Resolutions.FrameW) / 2) + TabOffset.X, ((settings.ViewH - eset.Resolutions.FrameH) / 2) + TabOffset.Y, eset.Resolutions.FrameW);

            Frame.X = ((settings.ViewW - eset.Resolutions.FrameW) / 2) + FrameOffset.X;
            Frame.Y = ((settings.ViewH - eset.Resolutions.FrameH) / 2) + TabControl.GetTabHeight() + FrameOffset.Y;

            for (int i = 0; i < ChildWidget.Count; ++i)
            {
                if (Optiontab[i] != NoTab)
                    ChildWidget[i]!.SetPos(Frame.X, Frame.Y);
            }

            OkButton!.SetPos(0, 0);
            DefaultsButton!.SetPos(0, 0);
            CancelButton!.SetPos(0, 0);

            DefaultsConfirm!.Align();

            for (int i = 0; i < _cfgTabs.Count; ++i)
            {
                _cfgTabs[i].Scrollbox!.SetPos(Frame.X, Frame.Y);
            }

            InputConfirm!.Align();
        }

        public void RefreshWindowSize()
        {
            var renderDevice = SharedResources.RenderDevice!;
            var inpt = SharedResources.Inpt!;

            renderDevice.WindowResize();
            inpt.WindowResized = true;
            RefreshWidgets();
            ForceRefreshBackground = true;
        }

        public void AddChildWidget(Widget? w, int tab)
        {
            ChildWidget.Add(w);
            Optiontab.Add(tab);
        }

        public void RefreshLanguages()
        {
            LanguageIso.Clear();
            LanguageLstb!.Clear();

            FileParser infile = new FileParser();
            if (infile.Open("engine/languages.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                int i = 0;
                while (infile.Next())
                {
                    if (infile.Key.Length != 0)
                    {
                        LanguageIso.Add(infile.Key);
                        LanguageLstb.Append(infile.Val, infile.Val + " [" + infile.Key + "]");

                        if (infile.Key == SharedResources.Settings!.Language)
                        {
                            LanguageLstb.Select((uint)i);
                        }

                        i++;
                    }
                }
                infile.Close();
            }

            if (LanguageLstb.GetSize() == 0)
            {
                LanguageIso.Add("en");
                LanguageLstb.Append("English", "English [en]");
                LanguageLstb.Select(0);
            }
        }

        public void RefreshFont()
        {
            SharedResources.Font?.Dispose();
            SharedResources.Font = DeviceList.GetFontEngine();
            SharedResources.Comb?.Dispose();
            SharedResources.Comb = new CombatText();
        }

        public void EnableMods()
        {
            for (int i = 0; i < InactivemodsLstb!.Size; i++)
            {
                if (InactivemodsLstb.IsSelected(i))
                {
                    ActivemodsLstb!.Append(InactivemodsLstb.GetValue(i), InactivemodsLstb.GetTooltip(i));
                    InactivemodsLstb.Remove(i);
                    i--;
                }
            }
        }

        public void DisableMods()
        {
            int gameIndex = (int)InactivemodsFilterLstb!.GetSelected();

            for (int i = 0; i < ActivemodsLstb!.Size; i++)
            {
                if (ActivemodsLstb.IsSelected(i) && ActivemodsLstb.GetValue(i) != ModManager.FallbackMod)
                {
                    Mod tempMod = SharedResources.Mods!.LoadMod(ActivemodsLstb.GetValue(i));
                    if (tempMod.Game == "")
                    {
                        InactivemodsFilterLstb.Select(1);
                    }
                    else if (gameIndex != 0)
                    {
                        for (uint j = 2; j < InactivemodsFilterLstb.GetSize(); ++j)
                        {
                            InactivemodsFilterLstb.Select(j);
                            if (tempMod.Game == InactivemodsFilterLstb.GetValue())
                            {
                                FilterMods();
                                break;
                            }
                        }
                    }

                    InactivemodsLstb!.Append(ActivemodsLstb.GetValue(i), ActivemodsLstb.GetTooltip(i));
                    ActivemodsLstb.Remove(i);
                    i--;
                }
            }
            InactivemodsLstb!.Sort();
        }

        public bool SetMods()
        {
            var mods = SharedResources.Mods!;
            List<Mod> tempList = new List<Mod>();
            for (int i = 0; i < mods.ModList.Count; i++)
                tempList.Add(new Mod(mods.ModList[i]));
            mods.ModList.Clear();
            mods.ModList.Add(mods.LoadMod(ModManager.FallbackMod));

            for (int i = 0; i < ActivemodsLstb!.Size; i++)
            {
                if (ActivemodsLstb.GetValue(i) != "")
                    mods.ModList.Add(mods.LoadMod(ActivemodsLstb.GetValue(i)));
            }

            mods.ApplyDepends();

            bool changed = mods.ModList.Count != tempList.Count;
            if (!changed)
            {
                for (int i = 0; i < mods.ModList.Count; i++)
                {
                    if (mods.ModList[i] != tempList[i])
                    {
                        changed = true;
                        break;
                    }
                }
            }

            if (changed)
            {
                mods.SaveMods();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void FilterMods()
        {
            var mods = SharedResources.Mods!;
            var settings = SharedResources.Settings!;

            InactivemodsLstb!.Clear();
            int gameIndex = (int)InactivemodsFilterLstb!.GetSelected();
            int unknownGameIndex = (int)InactivemodsFilterLstb.GetSize();
            if (_modFilterUnknown)
            {
                unknownGameIndex = (int)InactivemodsFilterLstb.GetSize() - 1;
            }

            for (int i = 0; i < mods.ModDirs.Count; i++)
            {
                bool skipMod = false;
                for (int j = 0; j < ActivemodsLstb!.Size; j++)
                {
                    if (mods.ModDirs[i] == ActivemodsLstb.GetValue(j))
                    {
                        skipMod = true;
                        break;
                    }
                }
                if (!skipMod && mods.ModDirs[i] != ModManager.FallbackMod)
                {
                    Mod tempMod = mods.LoadMod(mods.ModDirs[i]);

                    bool gameMatches = (settings.Game.Length == 0 || (settings.Game.Length != 0 && settings.Game == tempMod.Game) || tempMod.Game == ModManager.FallbackGame);
                    bool indexIsAll = (gameIndex == 0 || (gameIndex == 1 && tempMod.IsGameMod));
                    bool indexAndGameUnknown = (gameIndex == unknownGameIndex && tempMod.Game.Length == 0);
                    bool indexAndGameMatch = (tempMod.Game == InactivemodsFilterLstb.GetValue());

                    if (gameMatches && (indexIsAll || indexAndGameUnknown || indexAndGameMatch))
                    {
                        InactivemodsLstb.Append(mods.ModDirs[i], CreateModTooltip(tempMod));
                    }
                }
            }
            InactivemodsLstb.Sort();
            InactivemodsLstb.Refresh();
        }

        public string CreateModTooltip(Mod? mod)
        {
            var msg = SharedResources.Msg!;
            var settings = SharedResources.Settings!;

            string ret = "";
            if (mod != null)
            {
                string modVer = (mod.Version == VersionInfo.Min) ? "" : mod.Version.GetString();
                string engineVer = VersionInfo.CreateVersionReqString(mod.EngineMinVersion, mod.EngineMaxVersion);

                ret = mod.Name + '\n';

                if (mod.IsGameMod)
                {
                    ret += msg.Get("Core mod") + '\n';
                }

                string modDescription = mod.GetLocaleDescription(settings.Language);
                if (modDescription.Length != 0)
                {
                    ret += '\n';
                    ret += modDescription + '\n';
                }

                bool middleSection = false;
                if (modVer.Length != 0)
                {
                    middleSection = true;
                    ret += '\n';
                    ret += msg.Get("Version:") + ' ' + modVer;
                }
                if (mod.Game.Length != 0 && mod.Game != ModManager.FallbackGame)
                {
                    middleSection = true;
                    ret += '\n';
                    ret += msg.Get("Game:") + ' ' + mod.Game;
                }
                if (engineVer.Length != 0)
                {
                    middleSection = true;
                    ret += '\n';
                    ret += msg.Get("Engine version:") + ' ' + engineVer;
                }

                if (middleSection)
                    ret += '\n';

                if (mod.Depends.Count != 0)
                {
                    ret += '\n';
                    ret += msg.Get("Requires mods:") + '\n';
                    for (int i = 0; i < mod.Depends.Count; ++i)
                    {
                        ret += "-  " + mod.Depends[i];
                        string dependVer = VersionInfo.CreateVersionReqString(mod.DependsMin[i], mod.DependsMax[i]);
                        if (dependVer != "")
                            ret += " (" + dependVer + ")";
                        if (i < mod.Depends.Count - 1)
                            ret += '\n';
                    }
                }

                if (ret.Length != 0 && ret[ret.Length - 1] == '\n')
                    ret = ret.Substring(0, ret.Length - 1);
            }
            return ret;
        }

        public void ConfirmKey(int action)
        {
            var inpt = SharedResources.Inpt!;

            inpt.Pressing[action] = false;
            inpt.Lock[action] = false;

            InputConfirm!.Visible = false;
            InputConfirmTimer.Reset(Timer.End);
            KeybindTipTimer.Reset(Timer.End);

            inpt.RefreshHotkeys = true;

            UpdateKeybinds();
        }

        public void ScanKey(int action)
        {
            var inpt = SharedResources.Inpt!;
            var msg = SharedResources.Msg!;

            if (!InputConfirm!.Visible)
                return;

            if (InputConfirm.ClickedConfirm)
            {
                InputConfirm.ClickedConfirm = false;

                if (InputConfirm.ActionList!.GetSelected() == InputConfirmOptionNew)
                {
                    InputConfirm.ActionList.Enabled = false;
                    InputConfirm.Align();

                    inpt.LastButton = -1;
                    inpt.LastKey = -1;
                    inpt.LastJoybutton = -1;
                }
                else if (InputConfirm.ActionList.GetSelected() == InputConfirmOptionClear)
                {
                    int selectedBind = (int)KeybindsLstb![action]!.GetSelected();
                    if (action == Input.Main1 && selectedBind == 0)
                    {
                        KeybindMsg = msg.Get("Can not remove this binding.");
                    }
                    else
                    {
                        inpt.RemoveBind(action, selectedBind);
                        ConfirmKey(action);
                    }
                }
            }

            if (!InputConfirm.ActionList!.Enabled)
            {
                if (inpt.LastKey != -1)
                {
                    string? msgRef = KeybindMsg;
                    inpt.SetBind(action, InputBind.Key, inpt.LastKey, ref msgRef);
                    KeybindMsg = msgRef ?? "";
                    ConfirmKey(action);
                }
                else if (inpt.LastButton != -1)
                {
                    string? msgRef = KeybindMsg;
                    inpt.SetBind(action, InputBind.Mouse, inpt.LastButton, ref msgRef);
                    KeybindMsg = msgRef ?? "";
                    ConfirmKey(action);
                }
                else if (inpt.LastJoybutton != -1)
                {
                    string? msgRef = KeybindMsg;
                    inpt.SetBind(action, InputBind.Gamepad, inpt.LastJoybutton, ref msgRef);
                    KeybindMsg = msgRef ?? "";
                    ConfirmKey(action);
                }
                else if (inpt.LastJoyaxis != -1)
                {
                    string? msgRef = KeybindMsg;
                    inpt.SetBind(action, InputBind.GamepadAxis, inpt.LastJoyaxis, ref msgRef);
                    KeybindMsg = msgRef ?? "";
                    ConfirmKey(action);
                }
            }
        }

        public void EnableMouseOptions()
        {
            var settings = SharedResources.Settings!;
            settings.NoMouse = false;
            NoMouseCb!.SetChecked(settings.NoMouse);
        }

        public void DisableMouseOptions()
        {
            var settings = SharedResources.Settings!;

            settings.MouseAim = false;
            MouseAimCb!.SetChecked(settings.MouseAim);

            settings.MouseMove = false;
            MouseMoveCb!.SetChecked(settings.MouseMove);

            settings.NoMouse = true;
            NoMouseCb!.SetChecked(settings.NoMouse);
        }

        public void RefreshRenderers()
        {
            var msg = SharedResources.Msg!;
            var settings = SharedResources.Settings!;

            RendererLstb!.Clear();

            List<string> rdName = new List<string>();
            List<string> rdDesc = new List<string>();
            DeviceList.CreateRenderDeviceList(msg, rdName, rdDesc);

            for (int i = 0; i < rdName.Count; ++i)
            {
                RendererLstb.Append(rdName[i], rdDesc[i]);
                if (rdName[i] == settings.RenderDeviceName)
                {
                    RendererLstb.Select((uint)i);
                }
            }
        }

        public void RefreshJoysticks()
        {
            var msg = SharedResources.Msg!;
            var inpt = SharedResources.Inpt!;

            JoystickDeviceLstb!.Clear();
            JoystickDeviceLstb.Append(msg.Get("(none)"), "");
            JoystickDeviceLstb.Enabled = inpt.GetNumJoysticks() > 0;

            for (int i = 0; i < inpt.GetNumJoysticks(); ++i)
            {
                string joystickName = inpt.GetJoystickName(i);
                if (joystickName != "")
                    JoystickDeviceLstb.Append(joystickName, joystickName);
            }

            JoystickDeviceLstb.Refresh();
        }

        public void SetPauseExitText(bool enableSave)
        {
            var eset = SharedResources.Eset!;
            var msg = SharedResources.Msg!;
            PauseExitBtn!.SetLabel((eset.Misc.SaveOnexit && enableSave) ? msg.Get("Save & Exit") : msg.Get("Exit"));
        }

        public void SetPauseSaveEnabled(bool enableSave)
        {
            PauseSaveBtn!.Enabled = enableSave && SharedResources.Eset!.Misc.SaveAnywhere;
        }

        public void ResetSelectedTab()
        {
            Update();

            TabControl!.SetActiveTab(0);

            for (int i = 0; i < _cfgTabs.Count; ++i)
            {
                _cfgTabs[i].Scrollbox!.ScrollToTop();
            }

            for (int i = 0; i < Tablists.Count; ++i)
            {
                Tablists[i]!.Defocus();
            }

            InputConfirm!.Visible = false;
            InputConfirmTimer.Reset(Timer.End);
            KeybindTipTimer.Reset(Timer.End);
        }

        public int GetActiveTab()
        {
            return TabControl!.GetActiveTab();
        }

        public void SetActiveTab(uint tab)
        {
            TabControl!.SetActiveTab(tab);
        }

        public void Cleanup()
        {
            if (Background != null)
            {
                Background.Dispose();
                Background = null;
            }

            if (TabControl != null)
            {
                TabControl.Dispose();
                TabControl = null;
            }

            if (OkButton != null)
            {
                OkButton.Dispose();
                OkButton = null;
            }
            if (DefaultsButton != null)
            {
                DefaultsButton.Dispose();
                DefaultsButton = null;
            }
            if (CancelButton != null)
            {
                CancelButton.Dispose();
                CancelButton = null;
            }

            CleanupTabContents();
            CleanupDialogs();

            LanguageIso.Clear();
        }

        public void CleanupTabContents()
        {
            for (int i = 0; i < ChildWidget.Count; ++i)
            {
                if (ChildWidget[i] != null)
                {
                    if (ChildWidget[i] is IDisposable disposable)
                        disposable.Dispose();
                    ChildWidget[i] = null;
                }
            }
            ChildWidget.Clear();

            for (int i = 0; i < _cfgTabs.Count; ++i)
            {
                if (_cfgTabs[i].Scrollbox != null)
                {
                    _cfgTabs[i].Scrollbox.Dispose();
                    _cfgTabs[i].Scrollbox = null;
                }
            }
        }

        public void CleanupDialogs()
        {
            if (DefaultsConfirm != null)
            {
                DefaultsConfirm.Dispose();
                DefaultsConfirm = null;
            }
            if (InputConfirm != null)
            {
                InputConfirm.Dispose();
                InputConfirm = null;
            }
            if (KeybindTip != null)
            {
                KeybindTip.Dispose();
                KeybindTip = null;
            }
        }

        public void SetHero(Avatar? hero)
        {
            _hero = hero;
        }

        public bool SetFrameLimit()
        {
            var settings = SharedResources.Settings!;

            int frameLimitIndex = (int)FrameLimitLstb!.GetSelected();
            if (frameLimitIndex < _frameLimits.Count)
            {
                if (settings.MaxFramesPerSec != _frameLimits[frameLimitIndex])
                {
                    Utils.LogInfo("MenuConfig: Changing frame limit from %d to %d.", settings.MaxFramesPerSec, _frameLimits[frameLimitIndex]);
                    settings.MaxFramesPerSec = _frameLimits[frameLimitIndex];
                    return true;
                }
            }
            return false;
        }
    }
}
