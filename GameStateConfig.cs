// 对应 C++ 源：GameStateConfig.h + GameStateConfig.cpp

namespace FlareEngine
{
    /// <summary>
    /// GameStateConfig
    ///
    /// 处理游戏设置菜单。对应 C++ <c>class GameStateConfig : public GameState</c>。
    /// 持有 <see cref="MenuConfig"/> 实例并在接受/取消时重载引擎子系统。
    /// </summary>
    public class GameStateConfig : GameState
    {
        private MenuConfig? _menuConfig;

        public GameStateConfig()
            : base()
        {
            _menuConfig = new MenuConfig(MenuConfig.IsGameState);

            HasFrameBackground = _menuConfig.ShowFrameBackground;

            // don't save settings if we close the game while in this menu
            SaveSettingsOnExit = false;
        }

        /// <summary>对应 C++ 析构函数 <c>~GameStateConfig()</c>。</summary>
        public override void Dispose()
        {
            if (_menuConfig != null)
            {
                _menuConfig.Dispose();
                _menuConfig = null;
            }

            base.Dispose();
        }

        public override void Logic()
        {
            _menuConfig!.Logic();

            if (_menuConfig.ForceRefreshBackground)
            {
                ForceRefreshBackground = true;
                _menuConfig.ForceRefreshBackground = false;
            }
            if (_menuConfig.ReloadMusic)
            {
                ReloadMusic = true;
                _menuConfig.ReloadMusic = false;
            }

            if (_menuConfig.ClickedAccept)
            {
                _menuConfig.ClickedAccept = false;
                LogicAccept();
            }
            else if (_menuConfig.ClickedCancel)
            {
                _menuConfig.ClickedCancel = false;
                LogicCancel();
            }
        }

        public void LogicAccept()
        {
            var msg = SharedResources.Msg!;
            var snd = SharedResources.Snd!;
            var mods = SharedResources.Mods!;
            var inpt = SharedResources.Inpt!;
            var eset = SharedResources.Eset!;
            var settings = SharedResources.Settings!;
            var tooltipm = SharedResources.Tooltipm;
            var renderDevice = SharedResources.RenderDevice!;

            string newRenderDevice = _menuConfig!.RenderDevice;
            bool frameLimitChanged = _menuConfig.SetFrameLimit();

            inpt.SaveKeyBindings();

            if (_menuConfig.SetMods())
            {
                snd.UnloadMusic();
                ReloadMusic = true;
                ReloadBackgrounds = true;
                mods?.Dispose();
                SharedResources.Mods = new ModManager(null);
                settings.PrevSaveSlot = -1;
            }
            msg.Dispose();
            SharedResources.Msg = new MessageEngine();

            // if mods changed, we may need to use a different set of keybinds, so reload them here
            inpt.LoadKeyBindings(InputState.LoadUserBinds);

            inpt.SetCommonStrings();
            eset.Load();
            Stats.Init();
            RefreshFont();
            if ((settings.EnableJoystick) && (inpt.GetNumJoysticks() > 0))
            {
                inpt.InitJoystick();
            }
            _menuConfig.Cleanup();

            ShowLoading();
            // need to delete the "Loading..." message here, as we're recreating our render context
            if (LoadingTip != null)
            {
                LoadingTip.Dispose();
                LoadingTip = null;
            }

            tooltipm?.Dispose();

            // we can't replace the render device in-place, so soft-reset the game
            // same goes for changing the frame limit
            if (newRenderDevice != settings.RenderDeviceName || frameLimitChanged)
            {
                settings.RenderDeviceName = newRenderDevice;
                inpt.Done = true;
                settings.SoftReset = true;
            }

            renderDevice.CreateContext();
            SharedResources.Tooltipm = new TooltipManager();
            settings.SaveSettings();
            SetRequestedGameState(new GameStateTitle());
        }

        public void LogicCancel()
        {
            SharedResources.Settings!.LoadSettings();
            SharedResources.Inpt!.LoadKeyBindings();
            SharedResources.Msg?.Dispose();
            SharedResources.Msg = new MessageEngine();
            SharedResources.Inpt.SetCommonStrings();
            SharedResources.Eset!.Load();
            Stats.Init();
            RefreshFont();
            _menuConfig!.Update();
            _menuConfig.Cleanup();
            var renderDevice = SharedResources.RenderDevice!;
            var settings = SharedResources.Settings!;
            renderDevice.SetFullscreen(settings.Fullscreen);
            renderDevice.WindowResize();
            renderDevice.UpdateTitleBar();
            ShowLoading();
            SetRequestedGameState(new GameStateTitle());
        }

        public override void Render()
        {
            if (RequestedGameState != null)
            {
                // we're in the process of switching game states, so skip rendering
                return;
            }

            _menuConfig!.Render();
        }

        public void RefreshFont()
        {
            SharedResources.Font?.Dispose();
            SharedResources.Font = DeviceList.GetFontEngine();
            SharedResources.Comb?.Dispose();
            SharedResources.Comb = new CombatText();
        }

        public void SetActiveTab(uint tab)
        {
            _menuConfig!.SetActiveTab(tab);
        }
    }
}
