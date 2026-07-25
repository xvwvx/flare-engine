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
            ScrollpaneSeparatorColor = SharedResources.Font!.GetColor(FontEngine.ColorWidgetDisabled);
            NewRenderDevice = SharedResources.Settings!.RenderDeviceName;
            InputConfirmTimer = new Timer((uint)(SharedResources.Settings.MaxFramesPerSec * 10));
            InputAction = 0;
            KeybindTipTimer = new Timer((uint)(SharedResources.Settings.MaxFramesPerSec * 5));
            KeybindTip = new WidgetTooltip();
            ClickedAccept = false;
            ClickedCancel = false;
            ForceRefreshBackground = false;
            ReloadMusic = false;
            ClickedPauseContinue = false;
            ClickedPauseExit = false;
            ClickedPauseSave = false;
            ShowFrameBackground = false;

            InputConfirm.SetTitle(SharedResources.Msg!.Get("Assign:"));
            InputConfirm.ActionList!.Append(SharedResources.Msg.Get("New"), "");
            InputConfirm.ActionList.Append(SharedResources.Msg.Get("Clear"), "");

            DefaultsConfirm.SetTitle(SharedResources.Msg.Get("Reset ALL settings?"));
            DefaultsConfirm.ActionList!.Append(SharedResources.Msg.Get("No"), "");
            DefaultsConfirm.ActionList.Append(SharedResources.Msg.Get("Yes"), "");

            Image? graphics;
            graphics = SharedResources.RenderDevice!.LoadImage("images/menus/config.png", FlareEngine.RenderDevice.ErrorNormal);
            if (graphics != null)
            {
                Background = graphics.CreateSprite();
                graphics.Unref();
            }

            OkButton.SetLabel(SharedResources.Msg.Get("OK"));
            DefaultsButton.SetLabel(SharedResources.Msg.Get("Defaults"));
            CancelButton.SetLabel(SharedResources.Msg.Get("Cancel"));

            PauseContinueBtn.SetLabel(SharedResources.Msg.Get("Continue"));
            SetPauseExitText(EnableSaveGame);
            PauseSaveBtn.SetLabel(SharedResources.Msg.Get("Save Game"));
            SetPauseSaveEnabled(EnableSaveGame);
            PauseTimeText.SetText(Utils.GetTimeString(0));
            PauseTimeText.SetJustify(FontEngine.JustifyRight);
            PauseTimeText.SetVAlign(LabelInfo.ValignCenter);

            ActivemodsLstb.MultiSelect = true;
            for (int i = 0; i < SharedResources.Mods!.ModList.Count; i++)
            {
                if (SharedResources.Mods.ModList[i].Name != ModManager.FallbackMod)
                    ActivemodsLstb.Append(SharedResources.Mods.ModList[i].Name, CreateModTooltip(SharedResources.Mods.ModList[i]));
            }

            string activeGame = "";
            for (int i = SharedResources.Mods.ModList.Count; i > 0; i--)
            {
                Mod tempMod = SharedResources.Mods.ModList[i - 1];
                if (tempMod.Game.Length != 0)
                {
                    activeGame = tempMod.Game;
                    break;
                }
            }

            List<string> modGames = new List<string>();

            InactivemodsLstb.MultiSelect = true;
            for (int i = 0; i < SharedResources.Mods.ModDirs.Count; i++)
            {
                Mod tempMod = SharedResources.Mods.LoadMod(SharedResources.Mods.ModDirs[i]);
                if (SharedResources.Mods.ModDirs[i] != ModManager.FallbackMod && (activeGame.Length == 0 || tempMod.Game != activeGame))
                {
                    if (tempMod.Game == ModManager.FallbackGame || SharedResources.Settings.Game.Length == 0 || (SharedResources.Settings.Game.Length != 0 && SharedResources.Settings.Game == tempMod.Game))
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
                for (int j = 0; j < SharedResources.Mods.ModList.Count; j++)
                {
                    if (SharedResources.Mods.ModDirs[i] == SharedResources.Mods.ModList[j].Name)
                    {
                        skipMod = true;
                        break;
                    }
                }
                if (!skipMod && SharedResources.Mods.ModDirs[i] != ModManager.FallbackMod)
                {
                    InactivemodsLstb.Append(SharedResources.Mods.ModDirs[i], CreateModTooltip(tempMod));
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
                modGames.Add(SharedResources.Msg.Get("<unknown>"));
            }

            InactivemodsFilterLstb.Append(SharedResources.Msg.Get("All Mods"), "");
            InactivemodsFilterLstb.Append(SharedResources.Msg.Get("All Core Mods"), "");
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

            LootTooltipLstb.Append(SharedResources.Msg.Get("Default"), SharedResources.Msg.Get("Show all loot tooltips, except for those that would be obscured by the player or an enemy. Temporarily show all loot tooltips with 'Alt'."));
            LootTooltipLstb.Append(SharedResources.Msg.Get("Show all"), SharedResources.Msg.Get("Always show loot tooltips. Temporarily hide all loot tooltips with 'Alt'."));
            LootTooltipLstb.Append(SharedResources.Msg.Get("Hidden"), SharedResources.Msg.Get("Always hide loot tooltips, except for when a piece of loot is hovered with the mouse cursor. Temporarily show all loot tooltips with 'Alt'."));

            MinimapLstb.Append(SharedResources.Msg.Get("Visible"), "");
            MinimapLstb.Append(SharedResources.Msg.Get("Visible (2x zoom)"), "");
            MinimapLstb.Append(SharedResources.Msg.Get("Hidden"), "");

            string lhpwPrefix = SharedResources.Msg.Get("Controls the type of warning to be activated when the player is below the low health threshold.");
            string lhpwWarning1 = SharedResources.Msg.Get("- Display a message");
            string lhpwWarning2 = SharedResources.Msg.Get("- Play a sound");
            string lhpwWarning3 = SharedResources.Msg.Get("- Change the cursor");

            LowHpWarningLstb.Append(SharedResources.Msg.Get("Disabled"), lhpwPrefix);
            LowHpWarningLstb.Append(SharedResources.Msg.Get("All"), lhpwPrefix + "\n\n" + lhpwWarning1 + '\n' + lhpwWarning2 + '\n' + lhpwWarning3);
            LowHpWarningLstb.Append(SharedResources.Msg.Get("Message & Cursor"), lhpwPrefix + "\n\n" + lhpwWarning1 + '\n' + lhpwWarning3);
            LowHpWarningLstb.Append(SharedResources.Msg.Get("Message & Sound"), lhpwPrefix + "\n\n" + lhpwWarning1 + '\n' + lhpwWarning2);
            LowHpWarningLstb.Append(SharedResources.Msg.Get("Sound & Cursor"), lhpwPrefix + "\n\n" + lhpwWarning2 + '\n' + lhpwWarning3);
            LowHpWarningLstb.Append(SharedResources.Msg.Get("Message"), lhpwPrefix + "\n\n" + lhpwWarning1);
            LowHpWarningLstb.Append(SharedResources.Msg.Get("Cursor"), lhpwPrefix + "\n\n" + lhpwWarning3);
            LowHpWarningLstb.Append(SharedResources.Msg.Get("Sound"), lhpwPrefix + "\n\n" + lhpwWarning2);

            for (int i = 1; i <= 10; ++i)
            {
                LowHpThresholdLstb.Append((i * 5).ToString() + "%", SharedResources.Msg.Get("When the player's health drops below the given threshold, the low health notifications are triggered if one or more of them is enabled."));
            }

            string autoLootDesc = SharedResources.Msg.Get("When enabled, eligible loot will be picked up automatically when nearby.");
            AutoLootLstb.Append(SharedResources.Msg.Get("Disabled"), autoLootDesc);
            AutoLootLstb.Append(SharedResources.Msg.Get("Enabled"), autoLootDesc);
            AutoLootLstb.Append(SharedResources.Msg.Get("Only currency"), autoLootDesc);

            _frameLimits.Add(30);
            _frameLimits.Add(60);
            _frameLimits.Add(120);
            _frameLimits.Add(240);
            if (!_frameLimits.Contains(SharedResources.Settings.MaxFramesPerSec))
                _frameLimits.Add(SharedResources.Settings.MaxFramesPerSec);
            ushort refreshRate = SharedResources.RenderDevice.GetRefreshRate();
            if (refreshRate > 0 && !_frameLimits.Contains(refreshRate))
                _frameLimits.Add(refreshRate);

            _frameLimits.Sort();
            for (int i = 0; i < _frameLimits.Count; ++i)
            {
                FrameLimitLstb.Append(_frameLimits[i].ToString(), SharedResources.Msg.Get("The maximum frame rate that the game will be allowed to run at."));
            }

            string minRenderSizeTooltip = SharedResources.Msg.Get("The render size refers to the height in pixels of the surface used to draw the game. Mods define the allowed render sizes, but this option allows overriding the minimum size.");
            MinRenderSizeLstb.Append(SharedResources.Msg.Get("Default"), minRenderSizeTooltip);

            string maxRenderSizeTooltip = SharedResources.Msg.Get("The render size refers to the height in pixels of the surface used to draw the game. Mods define the allowed render sizes, but this option allows overriding the maximum size.");
            MaxRenderSizeLstb.Append(SharedResources.Msg.Get("Default"), maxRenderSizeTooltip);

            _virtualHeights.AddRange(SharedResources.Eset!.Resolutions.VirtualHeights);

            if (SharedResources.Settings.MinRenderSize > 0 && !_virtualHeights.Contains(SharedResources.Settings.MinRenderSize))
                _virtualHeights.Add(SharedResources.Settings.MinRenderSize);

            if (SharedResources.Settings.MaxRenderSize > 0 && !_virtualHeights.Contains(SharedResources.Settings.MaxRenderSize))
                _virtualHeights.Add(SharedResources.Settings.MaxRenderSize);

            _virtualHeights.Sort();
            for (int i = 0; i < _virtualHeights.Count; ++i)
            {
                string heightStr = _virtualHeights[i].ToString();
                MinRenderSizeLstb.Append(heightStr, minRenderSizeTooltip);
                MaxRenderSizeLstb.Append(heightStr, maxRenderSizeTooltip);
            }

            SharedResources.Inpt.JoysticksChanged = false;

            Init();

            SharedResources.RenderDevice.SetBackgroundColor(new Color(0, 0, 0, 0));
        }

        public void Dispose()
        {
            Cleanup();
            GC.SuppressFinalize(this);
        }

        public void Init()
        {
            TabControl!.SetupTab(ExitTab, SharedResources.Msg!.Get("Exit"), TablistExit);
            TabControl.SetupTab(VideoTab, SharedResources.Msg.Get("Video"), TablistVideo);
            TabControl.SetupTab(AudioTab, SharedResources.Msg.Get("Audio"), TablistAudio);
            TabControl.SetupTab(GameTab, SharedResources.Msg.Get("Game"), TablistGame);
            TabControl.SetupTab(InterfaceTab, SharedResources.Msg.Get("Interface"), TablistInterface);
            TabControl.SetupTab(InputTab, SharedResources.Msg.Get("Input"), TablistInput);
            TabControl.SetupTab(KeybindsTab, SharedResources.Msg.Get("Keybindings"), TablistKeybinds);
            TabControl.SetupTab(ModsTab, SharedResources.Msg.Get("Mods"), TablistMods);

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

            _cfgTabs[ExitTab].SetOptionWidgets(ExitOptionContinue, PauseContinueLb, PauseContinueBtn, SharedResources.Msg.Get("Paused"));
            _cfgTabs[ExitTab].SetOptionWidgets(ExitOptionSave, PauseSaveLb, PauseSaveBtn, "");
            _cfgTabs[ExitTab].SetOptionWidgets(ExitOptionExit, PauseExitLb, PauseExitBtn, "");
            _cfgTabs[ExitTab].SetOptionWidgets(ExitOptionTimePlayed, PauseTimeLb, PauseTimeText, SharedResources.Msg.Get("Time Played"));

            if (!(EnableSaveGame && SharedResources.Eset!.Misc.SaveAnywhere))
            {
                _cfgTabs[ExitTab].SetOptionEnabled(ExitOptionSave, false);
            }

            _cfgTabs[VideoTab].SetOptionWidgets(Platform.Video.Renderer, RendererLb, RendererLstb, SharedResources.Msg.Get("Renderer"));
            _cfgTabs[VideoTab].SetOptionWidgets(Platform.Video.Fullscreen, FullscreenLb, FullscreenCb, SharedResources.Msg.Get("Full Screen Mode"));
            _cfgTabs[VideoTab].SetOptionWidgets(Platform.Video.Hwsurface, HwsurfaceLb, HwsurfaceCb, SharedResources.Msg.Get("Hardware surfaces"));
            _cfgTabs[VideoTab].SetOptionWidgets(Platform.Video.Vsync, VsyncLb, VsyncCb, SharedResources.Msg.Get("V-Sync"));
            _cfgTabs[VideoTab].SetOptionWidgets(Platform.Video.TextureFilter, TextureFilterLb, TextureFilterCb, SharedResources.Msg.Get("Texture Filtering"));
            _cfgTabs[VideoTab].SetOptionWidgets(Platform.Video.DpiScaling, DpiScalingLb, DpiScalingCb, SharedResources.Msg.Get("DPI scaling"));
            _cfgTabs[VideoTab].SetOptionWidgets(Platform.Video.ParallaxLayers, ParallaxLayersLb, ParallaxLayersCb, SharedResources.Msg.Get("Parallax Layers"));
            _cfgTabs[VideoTab].SetOptionWidgets(Platform.Video.MinRenderSize, MinRenderSizeLb, MinRenderSizeLstb, SharedResources.Msg.Get("Minimum Render Size"));
            _cfgTabs[VideoTab].SetOptionWidgets(Platform.Video.MaxRenderSize, MaxRenderSizeLb, MaxRenderSizeLstb, SharedResources.Msg.Get("Maximum Render Size"));
            _cfgTabs[VideoTab].SetOptionWidgets(Platform.Video.FrameLimit, FrameLimitLb, FrameLimitLstb, SharedResources.Msg.Get("Frame Limit"));
            _cfgTabs[VideoTab].SetOptionWidgets(Platform.Video.ThreadedImageLoad, ThreadedImageLoadLb, ThreadedImageLoadCb, SharedResources.Msg.Get("Threaded Image Loading"));
            _cfgTabs[VideoTab].SetOptionWidgets(Platform.Video.FadeWalls, FadeWallsLb, FadeWallsCb, SharedResources.Msg.Get("Show player behind walls"));

            _cfgTabs[AudioTab].SetOptionWidgets(Platform.Audio.Sfx, SoundVolumeLb, SoundVolumeSl, SharedResources.Msg.Get("Sound Volume"));
            _cfgTabs[AudioTab].SetOptionWidgets(Platform.Audio.Music, MusicVolumeLb, MusicVolumeSl, SharedResources.Msg.Get("Music Volume"));
            _cfgTabs[AudioTab].SetOptionWidgets(Platform.Audio.MuteOnFocusLoss, MuteOnFocusLossLb, MuteOnFocusLossCb, SharedResources.Msg.Get("Mute audio when window loses focus"));

            _cfgTabs[GameTab].SetOptionWidgets(Platform.Game.AutoEquip, AutoEquipLb, AutoEquipCb, SharedResources.Msg.Get("Automatically equip items"));
            _cfgTabs[GameTab].SetOptionWidgets(Platform.Game.AutoLoot, AutoLootLb, AutoLootLstb, SharedResources.Msg.Get("Automatically pick up loot"));
            _cfgTabs[GameTab].SetOptionWidgets(Platform.Game.LowHpWarningType, LowHpWarningLb, LowHpWarningLstb, SharedResources.Msg.Get("Low health notification"));
            _cfgTabs[GameTab].SetOptionWidgets(Platform.Game.LowHpThreshold, LowHpThresholdLb, LowHpThresholdLstb, SharedResources.Msg.Get("Low health threshold"));

            _cfgTabs[InterfaceTab].SetOptionWidgets(Platform.Interface.Language, LanguageLb, LanguageLstb, SharedResources.Msg.Get("Language"));
            _cfgTabs[InterfaceTab].SetOptionWidgets(Platform.Interface.ShowFps, ShowFpsLb, ShowFpsCb, SharedResources.Msg.Get("Show FPS"));
            _cfgTabs[InterfaceTab].SetOptionWidgets(Platform.Interface.HardwareCursor, HardwareCursorLb, HardwareCursorCb, SharedResources.Msg.Get("Use system mouse cursor"));
            _cfgTabs[InterfaceTab].SetOptionWidgets(Platform.Interface.Colorblind, ColorblindLb, ColorblindCb, SharedResources.Msg.Get("Colorblind Mode"));
            _cfgTabs[InterfaceTab].SetOptionWidgets(Platform.Interface.DevMode, DevModeLb, DevModeCb, SharedResources.Msg.Get("Developer Mode"));
            _cfgTabs[InterfaceTab].SetOptionWidgets(Platform.Interface.Subtitles, SubtitlesLb, SubtitlesCb, SharedResources.Msg.Get("Subtitles"));
            _cfgTabs[InterfaceTab].SetOptionWidgets(Platform.Interface.LootTooltips, LootTooltipLb, LootTooltipLstb, SharedResources.Msg.Get("Loot tooltip visibility"));
            _cfgTabs[InterfaceTab].SetOptionWidgets(Platform.Interface.MinimapMode, MinimapLb, MinimapLstb, SharedResources.Msg.Get("Mini-map mode"));
            _cfgTabs[InterfaceTab].SetOptionWidgets(Platform.Interface.StatbarLabels, StatbarLabelsLb, StatbarLabelsCb, SharedResources.Msg.Get("Always show stat bar labels"));
            _cfgTabs[InterfaceTab].SetOptionWidgets(Platform.Interface.StatbarAutohide, StatbarAutohideLb, StatbarAutohideCb, SharedResources.Msg.Get("Allow stat bar auto-hiding"));
            _cfgTabs[InterfaceTab].SetOptionWidgets(Platform.Interface.CombatText, CombatTextLb, CombatTextCb, SharedResources.Msg.Get("Show combat text"));
            _cfgTabs[InterfaceTab].SetOptionWidgets(Platform.Interface.ItemCompareTips, ItemCompareTipsLb, ItemCompareTipsCb, SharedResources.Msg.Get("Show item comparison tooltips"));
            _cfgTabs[InterfaceTab].SetOptionWidgets(Platform.Interface.PauseOnFocusLoss, PauseOnFocusLossLb, PauseOnFocusLossCb, SharedResources.Msg.Get("Pause game when window loses focus"));

            _cfgTabs[InputTab].SetOptionWidgets(Platform.Input.Joystick, JoystickDeviceLb, JoystickDeviceLstb, SharedResources.Msg.Get("Joystick"));
            _cfgTabs[InputTab].SetOptionWidgets(Platform.Input.MouseMove, MouseMoveLb, MouseMoveCb, SharedResources.Msg.Get("Move hero using mouse"));
            _cfgTabs[InputTab].SetOptionWidgets(Platform.Input.MouseAim, MouseAimLb, MouseAimCb, SharedResources.Msg.Get("Mouse aim"));
            _cfgTabs[InputTab].SetOptionWidgets(Platform.Input.NoMouse, NoMouseLb, NoMouseCb, SharedResources.Msg.Get("Do not use mouse"));
            _cfgTabs[InputTab].SetOptionWidgets(Platform.Input.MouseMoveSwap, MouseMoveSwapLb, MouseMoveSwapCb, SharedResources.Msg.Get("Swap mouse movement button"));
            _cfgTabs[InputTab].SetOptionWidgets(Platform.Input.MouseMoveAttack, MouseMoveAttackLb, MouseMoveAttackCb, SharedResources.Msg.Get("Attack with mouse movement"));
            _cfgTabs[InputTab].SetOptionWidgets(Platform.Input.JoystickDeadzone, JoystickDeadzoneLb, JoystickDeadzoneSl, SharedResources.Msg.Get("Joystick Deadzone"));
            _cfgTabs[InputTab].SetOptionWidgets(Platform.Input.JoystickRumble, JoystickRumbleLb, JoystickRumbleCb, SharedResources.Msg.Get("Joystick Rumble"));
            _cfgTabs[InputTab].SetOptionWidgets(Platform.Input.TouchControls, TouchControlsLb, TouchControlsCb, SharedResources.Msg.Get("Touch Controls"));
            _cfgTabs[InputTab].SetOptionWidgets(Platform.Input.TouchScale, TouchScaleLb, TouchScaleSl, SharedResources.Msg.Get("Touch Gamepad Scaling"));

            for (int i = 0; i < KeybindsLstb.Count; ++i)
            {
                _cfgTabs[KeybindsTab].SetOptionWidgets(i, KeybindsLb[i], KeybindsLstb[i], SharedResources.Inpt.BindingName[i]);
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

            if (!SharedResources.Eset.Misc.MouseMoveEnabled)
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

            HwsurfaceCb!.Tooltip = SharedResources.Msg!.Get("Will try to store surfaces in video memory versus system memory. The effect this has on performance depends on the renderer.");
            VsyncCb!.Tooltip = SharedResources.Msg.Get("Prevents screen tearing. Disable if you experience \"stuttering\" in windowed mode or input lag.");
            DpiScalingCb!.Tooltip = SharedResources.Msg.Get("When enabled, this uses the screen DPI in addition to the window dimensions to scale the rendering resolution. Otherwise, only the window dimensions are used.");
            ParallaxLayersCb!.Tooltip = SharedResources.Msg.Get("This enables parallax (non-tile) layers. Disabling this setting can improve performance in some cases.");
            ThreadedImageLoadCb!.Tooltip = SharedResources.Msg.Get("Use multiple CPU threads when loading some images. Try disabling this option if you experience instability during loading.");
            FadeWallsCb!.Tooltip = SharedResources.Msg.Get("Lowers the opacity of wall tiles that are covering the player. Disabling this option may improve performace in some circumstances.");
            ColorblindCb!.Tooltip = SharedResources.Msg.Get("Provides additional text for information that is primarily conveyed through color.");
            StatbarAutohideCb!.Tooltip = SharedResources.Msg.Get("Some mods will automatically hide the stat bars when they are inactive. Disabling this option will keep them displayed at all times.");
            AutoEquipCb!.Tooltip = SharedResources.Msg.Get("When enabled, empty equipment slots will be filled with applicable items when they are obtained.");
            ItemCompareTipsCb!.Tooltip = SharedResources.Msg.Get("When enabled, tooltips for equipped items of the same type are shown next to standard item tooltips.");
            NoMouseCb!.Tooltip = SharedResources.Msg.Get("This allows the game to be controlled entirely with the keyboard (or joystick).");
            MouseMoveSwapCb!.Tooltip = SharedResources.Msg.Get("When 'Move hero using mouse' is enabled, this setting controls if 'Main1' or 'Main2' is used to move the hero. If enabled, 'Main2' will move the hero instead of 'Main1'.");
            MouseMoveAttackCb!.Tooltip = SharedResources.Msg.Get("When 'Move hero using mouse' is enabled, this setting controls if the Power assigned to the movement button can be used by targeting an enemy. If this setting is disabled, it is required to use 'Shift' to access the Power assigned to the movement button.");
            MouseAimCb!.Tooltip = SharedResources.Msg.Get("The player's attacks will be aimed in the direction of the mouse cursor when this is enabled.");
            TouchControlsCb!.Tooltip = SharedResources.Msg.Get("When enabled, a virtual gamepad will be added in-game. Other interactions, such as drag-and-drop behavior, are also altered to better suit touch input.");

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
                PlaceLabeledWidget(ActivemodsLb, ActivemodsLstb, x1, y1, x2, y2, SharedResources.Msg!.Get("Active Mods"));
                ActivemodsLb!.SetJustify(FontEngine.JustifyCenter);
            }
            else if (infile.Key == "activemods_height")
            {
                ActivemodsLstb!.SetHeight(x1);
            }
            else if (infile.Key == "inactivemods")
            {
                PlaceLabeledWidget(InactivemodsLb, InactivemodsLstb, x1, y1, x2, y2, SharedResources.Msg!.Get("Available Mods"));
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
                ActivemodsDeactivateBtn!.SetLabel(SharedResources.Msg!.Get("<< Disable"));
                ActivemodsDeactivateBtn.SetBasePos(x1, y1, Utils.AlignTopLeft);
                ActivemodsDeactivateBtn.Refresh();
            }
            else if (infile.Key == "inactivemods_activate")
            {
                InactivemodsActivateBtn!.SetLabel(SharedResources.Msg!.Get("Enable >>"));
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
            FullscreenCb!.SetChecked(SharedResources.Settings!.Fullscreen);
            HwsurfaceCb!.SetChecked(SharedResources.Settings.Hwsurface);
            VsyncCb!.SetChecked(SharedResources.Settings.Vsync);
            TextureFilterCb!.SetChecked(SharedResources.Settings.TextureFilter);
            DpiScalingCb!.SetChecked(SharedResources.Settings.DpiScaling);
            ParallaxLayersCb!.SetChecked(SharedResources.Settings.ParallaxLayers);
            ThreadedImageLoadCb!.SetChecked(SharedResources.Settings.EnableThreadedImageLoad);
            FadeWallsCb!.SetChecked(SharedResources.Settings.FadeWalls);

            RefreshRenderers();

            for (int i = 0; i < _frameLimits.Count; ++i)
            {
                if (_frameLimits[i] == SharedResources.Settings.MaxFramesPerSec)
                {
                    FrameLimitLstb!.Select((uint)i);
                    break;
                }
            }

            if (SharedResources.Settings.MinRenderSize == 0)
            {
                MinRenderSizeLstb!.Select(0);
            }
            else
            {
                for (int i = 0; i < _virtualHeights.Count; ++i)
                {
                    if (_virtualHeights[i] == SharedResources.Settings.MinRenderSize)
                    {
                        MinRenderSizeLstb!.Select((uint)(i + 1));
                        break;
                    }
                }
            }

            if (SharedResources.Settings.MaxRenderSize == 0)
            {
                MaxRenderSizeLstb!.Select(0);
            }
            else
            {
                for (int i = 0; i < _virtualHeights.Count; ++i)
                {
                    if (_virtualHeights[i] == SharedResources.Settings.MaxRenderSize)
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
            if (SharedResources.Settings!.Audio)
            {
                MusicVolumeSl!.Set(0, 128, SharedResources.Settings.MusicVolume);
                SharedResources.Snd!.SetVolumeMusic(SharedResources.Settings.MusicVolume);
                SoundVolumeSl!.Set(0, 128, SharedResources.Settings.SoundVolume);
                SharedResources.Snd.SetVolumeSFX(SharedResources.Settings.SoundVolume);
            }
            else
            {
                MusicVolumeSl!.Set(0, 128, 0);
                SoundVolumeSl!.Set(0, 128, 0);
            }
            MuteOnFocusLossCb!.SetChecked(SharedResources.Settings.MuteOnFocusLoss);

            _cfgTabs[AudioTab].Scrollbox!.Refresh();
        }

        public void UpdateGame()
        {
            _cfgTabs[GameTab].Scrollbox!.Refresh();

            AutoEquipCb!.SetChecked(SharedResources.Settings!.AutoEquip);
            AutoLootLstb!.Select((uint)SharedResources.Settings.AutoLoot);
            LowHpWarningLstb!.Select((uint)SharedResources.Settings.LowHpWarningType);
            LowHpThresholdLstb!.Select((uint)((SharedResources.Settings.LowHpThreshold / 5) - 1));
        }

        public void UpdateInterface()
        {
            ShowFpsCb!.SetChecked(SharedResources.Settings!.ShowFps);
            ColorblindCb!.SetChecked(SharedResources.Settings.Colorblind);
            HardwareCursorCb!.SetChecked(SharedResources.Settings.HardwareCursor);
            DevModeCb!.SetChecked(SharedResources.Settings.DevMode);
            SubtitlesCb!.SetChecked(SharedResources.Settings.Subtitles);
            StatbarLabelsCb!.SetChecked(SharedResources.Settings.StatbarLabels);
            StatbarAutohideCb!.SetChecked(SharedResources.Settings.StatbarAutohide);
            CombatTextCb!.SetChecked(SharedResources.Settings.CombatText);
            ItemCompareTipsCb!.SetChecked(SharedResources.Settings.ItemCompareTips);
            PauseOnFocusLossCb!.SetChecked(SharedResources.Settings.PauseOnFocusLoss);

            LootTooltipLstb!.Select((uint)SharedResources.Settings.LootTooltips);
            MinimapLstb!.Select((uint)SharedResources.Settings.MinimapMode);

            RefreshLanguages();

            _cfgTabs[InterfaceTab].Scrollbox!.Refresh();
        }

        public void UpdateInput()
        {
            MouseAimCb!.SetChecked(SharedResources.Settings!.MouseAim);
            NoMouseCb!.SetChecked(SharedResources.Settings.NoMouse);
            if (SharedResources.Eset!.Misc.MouseMoveEnabled)
            {
                MouseMoveCb!.SetChecked(SharedResources.Settings.MouseMove);
                MouseMoveSwapCb!.SetChecked(SharedResources.Settings.MouseMoveSwap);
                MouseMoveAttackCb!.SetChecked(SharedResources.Settings.MouseMoveAttack);
            }
            JoystickRumbleCb!.SetChecked(SharedResources.Settings.JoystickRumble);
            TouchControlsCb!.SetChecked(SharedResources.Settings.Touchscreen);

            if (SharedResources.Settings.EnableJoystick && SharedResources.Inpt!.GetNumJoysticks() > 0)
            {
                SharedResources.Inpt.InitJoystick();
                JoystickDeviceLstb!.Select((uint)(SharedResources.Settings.JoystickDevice + 1));
            }

            JoystickDeadzoneSl!.Set(Settings.JoyDeadzoneMin, Settings.JoyDeadzoneMax, SharedResources.Settings.JoyDeadzone);
            TouchScaleSl!.Set(TouchScaleMin, TouchScaleMax, (int)(SharedResources.Settings.TouchScale * 100.0));

            _cfgTabs[InputTab].Scrollbox!.Refresh();
        }

        public void UpdateKeybinds()
        {
            for (int i = 0; i < KeybindsLstb.Count; i++)
            {
                KeybindsLstb[i]!.Clear();
                if (SharedResources.Inpt!.Binding[i].Count == 0)
                {
                    KeybindsLstb[i]!.Append(SharedResources.Inpt.GetBindingStringByIndex(i, -1), "");
                }
                else
                {
                    string tooltipText = SharedResources.Msg!.Get("Bindings for:") + " " + SharedResources.Inpt.BindingName[i] + "\n";

                    for (int j = 0; j < SharedResources.Inpt.Binding[i].Count; ++j)
                    {
                        tooltipText += SharedResources.Inpt.GetBindingStringByIndex(i, j);
                        if (j + 1 != SharedResources.Inpt.Binding[i].Count)
                        {
                            tooltipText += "\n";
                        }
                    }
                    for (int j = 0; j < SharedResources.Inpt.Binding[i].Count; ++j)
                    {
                        KeybindsLstb[i]!.Append(SharedResources.Inpt.GetBindingStringByIndex(i, j), tooltipText);
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
            if (SharedResources.Inpt!.WindowResized)
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
                    SharedResources.Settings!.HardwareCursor = true;
                    HardwareCursorCb!.SetChecked(SharedResources.Settings.HardwareCursor);
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
                if (!SharedResources.Inpt!.UsingMouse() && !Tablists[i]!.IsLocked() && Tablists[i]!.GetCurrent() == -1 && TablistMain.GetCurrent() == -1)
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
                else if (CancelButton!.CheckClick() || (SharedResources.Inpt.UsingMouse() && SharedResources.Inpt.Pressing[Input.Cancel] && !SharedResources.Inpt.Lock[Input.Cancel]))
                {
                    if (SharedResources.Inpt.Pressing[Input.Cancel])
                        SharedResources.Inpt.Lock[Input.Cancel] = true;

                    ClickedCancel = true;
                    return false;
                }
                else if (!SharedResources.Inpt.UsingMouse() && SharedResources.Inpt.Pressing[Input.Cancel] && !SharedResources.Inpt.Lock[Input.Cancel])
                {
                    SharedResources.Inpt.Lock[Input.Cancel] = true;

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
            DefaultsConfirm!.Logic();
            if (DefaultsConfirm.ClickedConfirm)
            {
                if (DefaultsConfirm.ActionList!.GetSelected() == DefaultsConfirmOptionYes)
                {
                    SharedResources.Settings!.Fullscreen = false;
                    SharedResources.Settings.LoadDefaults();
                    SharedResources.RenderDevice!.SetFullscreen(SharedResources.Settings.Fullscreen);
                    SharedResources.Eset!.Load();
                    SharedResources.Inpt!.InitBindings();
                    SharedResources.Inpt.LoadKeyBindings(!InputState.LoadUserBinds);
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
            _cfgTabs[VideoTab].Scrollbox!.Logic();
            Int2 mouse = _cfgTabs[VideoTab].Scrollbox.InputAssist(SharedResources.Inpt!.Mouse);

            if (_cfgTabs[VideoTab].Options[Platform.Video.Fullscreen].Enabled && FullscreenCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.Fullscreen = FullscreenCb.IsChecked;
                SharedResources.RenderDevice!.SetFullscreen(SharedResources.Settings.Fullscreen);
                RefreshWindowSize();
            }
            else if (_cfgTabs[VideoTab].Options[Platform.Video.Hwsurface].Enabled && HwsurfaceCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.Hwsurface = HwsurfaceCb.IsChecked;
            }
            else if (_cfgTabs[VideoTab].Options[Platform.Video.Vsync].Enabled && VsyncCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.Vsync = VsyncCb.IsChecked;
            }
            else if (_cfgTabs[VideoTab].Options[Platform.Video.TextureFilter].Enabled && TextureFilterCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.TextureFilter = TextureFilterCb.IsChecked;
            }
            else if (_cfgTabs[VideoTab].Options[Platform.Video.DpiScaling].Enabled && DpiScalingCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.DpiScaling = DpiScalingCb.IsChecked;
                RefreshWindowSize();
            }
            else if (_cfgTabs[VideoTab].Options[Platform.Video.ParallaxLayers].Enabled && ParallaxLayersCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.ParallaxLayers = ParallaxLayersCb.IsChecked;
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
                    SharedResources.Settings!.MinRenderSize = 0;
                }
                else
                {
                    SharedResources.Settings!.MinRenderSize = _virtualHeights[index - 1];
                    if (SharedResources.Settings.MaxRenderSize < SharedResources.Settings.MinRenderSize)
                    {
                        SharedResources.Settings.MaxRenderSize = 0;
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
                    SharedResources.Settings!.MaxRenderSize = 0;
                }
                else
                {
                    SharedResources.Settings!.MaxRenderSize = _virtualHeights[index - 1];
                    if (SharedResources.Settings.MaxRenderSize < SharedResources.Settings.MinRenderSize)
                    {
                        SharedResources.Settings.MinRenderSize = 0;
                        MinRenderSizeLstb!.Select(0);
                    }
                }
                RefreshWindowSize();
            }
            else if (_cfgTabs[VideoTab].Options[Platform.Video.ThreadedImageLoad].Enabled && ThreadedImageLoadCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.EnableThreadedImageLoad = ThreadedImageLoadCb.IsChecked;
            }
            else if (_cfgTabs[VideoTab].Options[Platform.Video.FadeWalls].Enabled && FadeWallsCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.FadeWalls = FadeWallsCb.IsChecked;
            }
        }

        public void LogicAudio()
        {
            _cfgTabs[AudioTab].Scrollbox!.Logic();
            Int2 mouse = _cfgTabs[AudioTab].Scrollbox.InputAssist(SharedResources.Inpt!.Mouse);

            if (_cfgTabs[AudioTab].Options[Platform.Audio.MuteOnFocusLoss].Enabled && MuteOnFocusLossCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.MuteOnFocusLoss = MuteOnFocusLossCb.IsChecked;
            }
            else if (SharedResources.Settings!.Audio)
            {
                if (_cfgTabs[AudioTab].Options[Platform.Audio.Music].Enabled && MusicVolumeSl!.CheckClickAt(mouse.X, mouse.Y))
                {
                    if (SharedResources.Settings.MusicVolume == 0)
                        ReloadMusic = true;
                    SharedResources.Settings.MusicVolume = (ushort)MusicVolumeSl.Value;
                    SharedResources.Snd!.SetVolumeMusic(SharedResources.Settings.MusicVolume);
                }
                else if (_cfgTabs[AudioTab].Options[Platform.Audio.Sfx].Enabled && SoundVolumeSl!.CheckClickAt(mouse.X, mouse.Y))
                {
                    SharedResources.Settings.SoundVolume = (ushort)SoundVolumeSl.Value;
                    SharedResources.Snd!.SetVolumeSFX(SharedResources.Settings.SoundVolume);
                }
            }
        }

        public void LogicGame()
        {
            _cfgTabs[GameTab].Scrollbox!.Logic();
            Int2 mouse = _cfgTabs[GameTab].Scrollbox.InputAssist(SharedResources.Inpt!.Mouse);

            if (_cfgTabs[GameTab].Options[Platform.Game.AutoEquip].Enabled && AutoEquipCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.AutoEquip = AutoEquipCb.IsChecked;
            }
            else if (_cfgTabs[GameTab].Options[Platform.Game.AutoLoot].Enabled && AutoLootLstb!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.AutoLoot = (int)AutoLootLstb.GetSelected();
            }
            else if (_cfgTabs[GameTab].Options[Platform.Game.LowHpWarningType].Enabled && LowHpWarningLstb!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.LowHpWarningType = (int)LowHpWarningLstb.GetSelected();
            }
            else if (_cfgTabs[GameTab].Options[Platform.Game.LowHpThreshold].Enabled && LowHpThresholdLstb!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.LowHpThreshold = ((int)LowHpThresholdLstb.GetSelected() + 1) * 5;
            }
        }

        public void LogicInterface()
        {
            _cfgTabs[InterfaceTab].Scrollbox!.Logic();
            Int2 mouse = _cfgTabs[InterfaceTab].Scrollbox.InputAssist(SharedResources.Inpt!.Mouse);

            if (_cfgTabs[InterfaceTab].Options[Platform.Interface.Language].Enabled && LanguageLstb!.CheckClickAt(mouse.X, mouse.Y))
            {
                uint langId = LanguageLstb.GetSelected();
                if (langId != LanguageLstb.GetSize())
                    SharedResources.Settings!.Language = LanguageIso[(int)langId];
            }
            else if (_cfgTabs[InterfaceTab].Options[Platform.Interface.ShowFps].Enabled && ShowFpsCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.ShowFps = ShowFpsCb.IsChecked;
            }
            else if (_cfgTabs[InterfaceTab].Options[Platform.Interface.Colorblind].Enabled && ColorblindCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.Colorblind = ColorblindCb.IsChecked;
            }
            else if (_cfgTabs[InterfaceTab].Options[Platform.Interface.HardwareCursor].Enabled && HardwareCursorCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.HardwareCursor = HardwareCursorCb.IsChecked;
            }
            else if (_cfgTabs[InterfaceTab].Options[Platform.Interface.DevMode].Enabled && DevModeCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.DevMode = DevModeCb.IsChecked;
            }
            else if (_cfgTabs[InterfaceTab].Options[Platform.Interface.Subtitles].Enabled && SubtitlesCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.Subtitles = SubtitlesCb.IsChecked;
            }
            else if (_cfgTabs[InterfaceTab].Options[Platform.Interface.LootTooltips].Enabled && LootTooltipLstb!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.LootTooltips = (int)LootTooltipLstb.GetSelected();
            }
            else if (_cfgTabs[InterfaceTab].Options[Platform.Interface.MinimapMode].Enabled && MinimapLstb!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.MinimapMode = (int)MinimapLstb.GetSelected();
            }
            else if (_cfgTabs[InterfaceTab].Options[Platform.Interface.StatbarLabels].Enabled && StatbarLabelsCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.StatbarLabels = StatbarLabelsCb.IsChecked;
            }
            else if (_cfgTabs[InterfaceTab].Options[Platform.Interface.StatbarAutohide].Enabled && StatbarAutohideCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.StatbarAutohide = StatbarAutohideCb.IsChecked;
            }
            else if (_cfgTabs[InterfaceTab].Options[Platform.Interface.CombatText].Enabled && CombatTextCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.CombatText = CombatTextCb.IsChecked;
            }
            else if (_cfgTabs[InterfaceTab].Options[Platform.Interface.ItemCompareTips].Enabled && ItemCompareTipsCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.ItemCompareTips = ItemCompareTipsCb.IsChecked;
            }
            else if (_cfgTabs[InterfaceTab].Options[Platform.Interface.PauseOnFocusLoss].Enabled && PauseOnFocusLossCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.PauseOnFocusLoss = PauseOnFocusLossCb.IsChecked;
            }
        }

        public void LogicInput()
        {
            _cfgTabs[InputTab].Scrollbox!.Logic();
            Int2 mouse = _cfgTabs[InputTab].Scrollbox.InputAssist(SharedResources.Inpt!.Mouse);

            if (SharedResources.Inpt.JoysticksChanged)
            {
                RefreshJoysticks();
                if (SharedResources.Settings!.EnableJoystick && SharedResources.Inpt.GetNumJoysticks() > 0)
                {
                    SharedResources.Inpt.InitJoystick();
                    JoystickDeviceLstb!.Select((uint)(SharedResources.Settings.JoystickDevice + 1));
                }
                else
                {
                    JoystickDeviceLstb!.Select(0);
                }

                SharedResources.Inpt.JoysticksChanged = false;
            }

            if (_cfgTabs[InputTab].Options[Platform.Input.MouseMove].Enabled && MouseMoveCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                if (MouseMoveCb.IsChecked)
                {
                    SharedResources.Settings!.MouseMove = true;
                    EnableMouseOptions();
                }
                else SharedResources.Settings!.MouseMove = false;
            }
            else if (_cfgTabs[InputTab].Options[Platform.Input.MouseAim].Enabled && MouseAimCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                if (MouseAimCb.IsChecked)
                {
                    SharedResources.Settings!.MouseAim = true;
                    EnableMouseOptions();
                }
                else SharedResources.Settings!.MouseAim = false;
            }
            else if (_cfgTabs[InputTab].Options[Platform.Input.NoMouse].Enabled && NoMouseCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                if (NoMouseCb.IsChecked)
                {
                    SharedResources.Settings!.NoMouse = true;
                    DisableMouseOptions();
                }
                else SharedResources.Settings!.NoMouse = false;
            }
            else if (_cfgTabs[InputTab].Options[Platform.Input.MouseMoveSwap].Enabled && MouseMoveSwapCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.MouseMoveSwap = MouseMoveSwapCb.IsChecked;
            }
            else if (_cfgTabs[InputTab].Options[Platform.Input.MouseMoveAttack].Enabled && MouseMoveAttackCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.MouseMoveAttack = MouseMoveAttackCb.IsChecked;
            }
            else if (_cfgTabs[InputTab].Options[Platform.Input.JoystickDeadzone].Enabled && JoystickDeadzoneSl!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.JoyDeadzone = JoystickDeadzoneSl.Value;
            }
            else if (_cfgTabs[InputTab].Options[Platform.Input.Joystick].Enabled && JoystickDeviceLstb!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.JoystickDevice = (int)JoystickDeviceLstb.GetSelected() - 1;
                SharedResources.Settings.EnableJoystick = (SharedResources.Settings.JoystickDevice != -1);
                SharedResources.Inpt!.JoysticksChanged = true;
                SharedResources.Inpt.InitJoystick();
                SharedResources.Inpt.JoysticksChanged = false;
            }
            else if (_cfgTabs[InputTab].Options[Platform.Input.JoystickRumble].Enabled && JoystickRumbleCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.JoystickRumble = JoystickRumbleCb.IsChecked;
            }
            else if (_cfgTabs[InputTab].Options[Platform.Input.TouchControls].Enabled && TouchControlsCb!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.Touchscreen = TouchControlsCb.IsChecked;
            }
            else if (_cfgTabs[InputTab].Options[Platform.Input.TouchScale].Enabled && TouchScaleSl!.CheckClickAt(mouse.X, mouse.Y))
            {
                SharedResources.Settings!.TouchScale = (float)TouchScaleSl.Value * 0.01f;
            }
        }

        public void LogicKeybinds()
        {
            _cfgTabs[KeybindsTab].Scrollbox!.Logic();
            Int2 mouse = _cfgTabs[KeybindsTab].Scrollbox.InputAssist(SharedResources.Inpt!.Mouse);

            for (int i = 0; i < KeybindsLstb.Count; i++)
            {
                if (!_cfgTabs[KeybindsTab].Options[i].Enabled)
                    continue;

                if (KeybindsLstb[i]!.CheckClickAt(mouse.X, mouse.Y))
                {
                    if (KeybindsLstb[i]!.CheckAction())
                    {
                        InputConfirm!.SetTitle(SharedResources.Msg!.Get("Assign:") + ' ' + SharedResources.Inpt!.BindingName[i]);
                        InputConfirmTimer.Reset(Timer.Begin);
                        InputConfirm.Show();
                        InputAction = i;
                        SharedResources.Inpt.LastButton = -1;
                        SharedResources.Inpt.LastKey = -1;
                        SharedResources.Inpt.LastJoybutton = -1;
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
            Rectangle pos = new Rectangle();
            pos.X = (SharedResources.Settings!.ViewW - SharedResources.Eset!.Resolutions.FrameW) / 2 + BackgroundOffset.X;
            pos.Y = (SharedResources.Settings.ViewH - SharedResources.Eset.Resolutions.FrameH) / 2 + BackgroundOffset.Y;

            if (Background != null)
            {
                Background.SetDestFromRect(pos);
                SharedResources.RenderDevice!.Render(Background);
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
            TabControl!.SetMainArea(((SharedResources.Settings!.ViewW - SharedResources.Eset!.Resolutions.FrameW) / 2) + TabOffset.X, ((SharedResources.Settings.ViewH - SharedResources.Eset.Resolutions.FrameH) / 2) + TabOffset.Y, SharedResources.Eset.Resolutions.FrameW);

            Frame.X = ((SharedResources.Settings.ViewW - SharedResources.Eset.Resolutions.FrameW) / 2) + FrameOffset.X;
            Frame.Y = ((SharedResources.Settings.ViewH - SharedResources.Eset.Resolutions.FrameH) / 2) + TabControl.GetTabHeight() + FrameOffset.Y;

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
            SharedResources.RenderDevice!.WindowResize();
            SharedResources.Inpt!.WindowResized = true;
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
            List<Mod> tempList = new List<Mod>();
            for (int i = 0; i < SharedResources.Mods!.ModList.Count; i++)
                tempList.Add(new Mod(SharedResources.Mods.ModList[i]));
            SharedResources.Mods.ModList.Clear();
            SharedResources.Mods.ModList.Add(SharedResources.Mods.LoadMod(ModManager.FallbackMod));

            for (int i = 0; i < ActivemodsLstb!.Size; i++)
            {
                if (ActivemodsLstb.GetValue(i) != "")
                    SharedResources.Mods.ModList.Add(SharedResources.Mods.LoadMod(ActivemodsLstb.GetValue(i)));
            }

            SharedResources.Mods.ApplyDepends();

            bool changed = SharedResources.Mods.ModList.Count != tempList.Count;
            if (!changed)
            {
                for (int i = 0; i < SharedResources.Mods.ModList.Count; i++)
                {
                    if (SharedResources.Mods.ModList[i] != tempList[i])
                    {
                        changed = true;
                        break;
                    }
                }
            }

            if (changed)
            {
                SharedResources.Mods.SaveMods();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void FilterMods()
        {
            InactivemodsLstb!.Clear();
            int gameIndex = (int)InactivemodsFilterLstb!.GetSelected();
            int unknownGameIndex = (int)InactivemodsFilterLstb.GetSize();
            if (_modFilterUnknown)
            {
                unknownGameIndex = (int)InactivemodsFilterLstb.GetSize() - 1;
            }

            for (int i = 0; i < SharedResources.Mods!.ModDirs.Count; i++)
            {
                bool skipMod = false;
                for (int j = 0; j < ActivemodsLstb!.Size; j++)
                {
                    if (SharedResources.Mods.ModDirs[i] == ActivemodsLstb.GetValue(j))
                    {
                        skipMod = true;
                        break;
                    }
                }
                if (!skipMod && SharedResources.Mods.ModDirs[i] != ModManager.FallbackMod)
                {
                    Mod tempMod = SharedResources.Mods.LoadMod(SharedResources.Mods.ModDirs[i]);

                    bool gameMatches = (SharedResources.Settings!.Game.Length == 0 || (SharedResources.Settings.Game.Length != 0 && SharedResources.Settings.Game == tempMod.Game) || tempMod.Game == ModManager.FallbackGame);
                    bool indexIsAll = (gameIndex == 0 || (gameIndex == 1 && tempMod.IsGameMod));
                    bool indexAndGameUnknown = (gameIndex == unknownGameIndex && tempMod.Game.Length == 0);
                    bool indexAndGameMatch = (tempMod.Game == InactivemodsFilterLstb.GetValue());

                    if (gameMatches && (indexIsAll || indexAndGameUnknown || indexAndGameMatch))
                    {
                        InactivemodsLstb.Append(SharedResources.Mods.ModDirs[i], CreateModTooltip(tempMod));
                    }
                }
            }
            InactivemodsLstb.Sort();
            InactivemodsLstb.Refresh();
        }

        public string CreateModTooltip(Mod? mod)
        {
            string ret = "";
            if (mod != null)
            {
                string modVer = (mod.Version == VersionInfo.Min) ? "" : mod.Version.GetString();
                string engineVer = VersionInfo.CreateVersionReqString(mod.EngineMinVersion, mod.EngineMaxVersion);

                ret = mod.Name + '\n';

                if (mod.IsGameMod)
                {
                    ret += SharedResources.Msg!.Get("Core mod") + '\n';
                }

                string modDescription = mod.GetLocaleDescription(SharedResources.Settings!.Language);
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
                    ret += SharedResources.Msg!.Get("Version:") + ' ' + modVer;
                }
                if (mod.Game.Length != 0 && mod.Game != ModManager.FallbackGame)
                {
                    middleSection = true;
                    ret += '\n';
                    ret += SharedResources.Msg!.Get("Game:") + ' ' + mod.Game;
                }
                if (engineVer.Length != 0)
                {
                    middleSection = true;
                    ret += '\n';
                    ret += SharedResources.Msg!.Get("Engine version:") + ' ' + engineVer;
                }

                if (middleSection)
                    ret += '\n';

                if (mod.Depends.Count != 0)
                {
                    ret += '\n';
                    ret += SharedResources.Msg!.Get("Requires mods:") + '\n';
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
            SharedResources.Inpt!.Pressing[action] = false;
            SharedResources.Inpt.Lock[action] = false;

            InputConfirm!.Visible = false;
            InputConfirmTimer.Reset(Timer.End);
            KeybindTipTimer.Reset(Timer.End);

            SharedResources.Inpt.RefreshHotkeys = true;

            UpdateKeybinds();
        }

        public void ScanKey(int action)
        {
            if (!InputConfirm!.Visible)
                return;

            if (InputConfirm.ClickedConfirm)
            {
                InputConfirm.ClickedConfirm = false;

                if (InputConfirm.ActionList!.GetSelected() == InputConfirmOptionNew)
                {
                    InputConfirm.ActionList.Enabled = false;
                    InputConfirm.Align();

                    SharedResources.Inpt!.LastButton = -1;
                    SharedResources.Inpt.LastKey = -1;
                    SharedResources.Inpt.LastJoybutton = -1;
                }
                else if (InputConfirm.ActionList.GetSelected() == InputConfirmOptionClear)
                {
                    int selectedBind = (int)KeybindsLstb![action]!.GetSelected();
                    if (action == Input.Main1 && selectedBind == 0)
                    {
                        KeybindMsg = SharedResources.Msg!.Get("Can not remove this binding.");
                    }
                    else
                    {
                        SharedResources.Inpt!.RemoveBind(action, selectedBind);
                        ConfirmKey(action);
                    }
                }
            }

            if (!InputConfirm.ActionList!.Enabled)
            {
                if (SharedResources.Inpt!.LastKey != -1)
                {
                    string? msgRef = KeybindMsg;
                    SharedResources.Inpt.SetBind(action, InputBind.Key, SharedResources.Inpt.LastKey, ref msgRef);
                    KeybindMsg = msgRef ?? "";
                    ConfirmKey(action);
                }
                else if (SharedResources.Inpt.LastButton != -1)
                {
                    string? msgRef = KeybindMsg;
                    SharedResources.Inpt.SetBind(action, InputBind.Mouse, SharedResources.Inpt.LastButton, ref msgRef);
                    KeybindMsg = msgRef ?? "";
                    ConfirmKey(action);
                }
                else if (SharedResources.Inpt.LastJoybutton != -1)
                {
                    string? msgRef = KeybindMsg;
                    SharedResources.Inpt.SetBind(action, InputBind.Gamepad, SharedResources.Inpt.LastJoybutton, ref msgRef);
                    KeybindMsg = msgRef ?? "";
                    ConfirmKey(action);
                }
                else if (SharedResources.Inpt.LastJoyaxis != -1)
                {
                    string? msgRef = KeybindMsg;
                    SharedResources.Inpt.SetBind(action, InputBind.GamepadAxis, SharedResources.Inpt.LastJoyaxis, ref msgRef);
                    KeybindMsg = msgRef ?? "";
                    ConfirmKey(action);
                }
            }
        }

        public void EnableMouseOptions()
        {
            SharedResources.Settings!.NoMouse = false;
            NoMouseCb!.SetChecked(SharedResources.Settings.NoMouse);
        }

        public void DisableMouseOptions()
        {
            SharedResources.Settings!.MouseAim = false;
            MouseAimCb!.SetChecked(SharedResources.Settings.MouseAim);

            SharedResources.Settings.MouseMove = false;
            MouseMoveCb!.SetChecked(SharedResources.Settings.MouseMove);

            SharedResources.Settings.NoMouse = true;
            NoMouseCb!.SetChecked(SharedResources.Settings.NoMouse);
        }

        public void RefreshRenderers()
        {
            RendererLstb!.Clear();

            List<string> rdName = new List<string>();
            List<string> rdDesc = new List<string>();
            DeviceList.CreateRenderDeviceList(SharedResources.Msg!, rdName, rdDesc);

            for (int i = 0; i < rdName.Count; ++i)
            {
                RendererLstb.Append(rdName[i], rdDesc[i]);
                if (rdName[i] == SharedResources.Settings!.RenderDeviceName)
                {
                    RendererLstb.Select((uint)i);
                }
            }
        }

        public void RefreshJoysticks()
        {
            JoystickDeviceLstb!.Clear();
            JoystickDeviceLstb.Append(SharedResources.Msg!.Get("(none)"), "");
            JoystickDeviceLstb.Enabled = SharedResources.Inpt!.GetNumJoysticks() > 0;

            for (int i = 0; i < SharedResources.Inpt.GetNumJoysticks(); ++i)
            {
                string joystickName = SharedResources.Inpt.GetJoystickName(i);
                if (joystickName != "")
                    JoystickDeviceLstb.Append(joystickName, joystickName);
            }

            JoystickDeviceLstb.Refresh();
        }

        public void SetPauseExitText(bool enableSave)
        {
            PauseExitBtn!.SetLabel((SharedResources.Eset!.Misc.SaveOnexit && enableSave) ? SharedResources.Msg!.Get("Save & Exit") : SharedResources.Msg!.Get("Exit"));
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
            int frameLimitIndex = (int)FrameLimitLstb!.GetSelected();
            if (frameLimitIndex < _frameLimits.Count)
            {
                if (SharedResources.Settings!.MaxFramesPerSec != _frameLimits[frameLimitIndex])
                {
                    Utils.LogInfo("MenuConfig: Changing frame limit from %d to %d.", SharedResources.Settings.MaxFramesPerSec, _frameLimits[frameLimitIndex]);
                    SharedResources.Settings.MaxFramesPerSec = _frameLimits[frameLimitIndex];
                    return true;
                }
            }
            return false;
        }
    }
}
