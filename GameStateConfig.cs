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
            string newRenderDevice = _menuConfig!.RenderDevice;
            bool frameLimitChanged = _menuConfig.SetFrameLimit();

            SharedResources.Inpt!.SaveKeyBindings();

            if (_menuConfig.SetMods())
            {
                SharedResources.Snd!.UnloadMusic();
                ReloadMusic = true;
                ReloadBackgrounds = true;
                SharedResources.Mods?.Dispose();
                SharedResources.Mods = new ModManager(null);
                SharedResources.Settings!.PrevSaveSlot = -1;
            }
            SharedResources.Msg?.Dispose();
            SharedResources.Msg = new MessageEngine();

            // if mods changed, we may need to use a different set of keybinds, so reload them here
            SharedResources.Inpt.LoadKeyBindings(InputState.LoadUserBinds);

            SharedResources.Inpt.SetCommonStrings();
            SharedResources.Eset!.Load();
            Stats.Init();
            RefreshFont();
            if ((SharedResources.Settings.EnableJoystick) && (SharedResources.Inpt.GetNumJoysticks() > 0))
            {
                SharedResources.Inpt.InitJoystick();
            }
            _menuConfig.Cleanup();

            ShowLoading();
            // need to delete the "Loading..." message here, as we're recreating our render context
            if (LoadingTip != null)
            {
                LoadingTip.Dispose();
                LoadingTip = null;
            }

            SharedResources.Tooltipm?.Dispose();

            // we can't replace the render device in-place, so soft-reset the game
            // same goes for changing the frame limit
            if (newRenderDevice != SharedResources.Settings.RenderDeviceName || frameLimitChanged)
            {
                SharedResources.Settings.RenderDeviceName = newRenderDevice;
                SharedResources.Inpt.Done = true;
                SharedResources.Settings.SoftReset = true;
            }

            SharedResources.RenderDevice!.CreateContext();
            SharedResources.Tooltipm = new TooltipManager();
            SharedResources.Settings.SaveSettings();
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
            SharedResources.RenderDevice!.SetFullscreen(SharedResources.Settings.Fullscreen);
            SharedResources.RenderDevice.WindowResize();
            SharedResources.RenderDevice.UpdateTitleBar();
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
