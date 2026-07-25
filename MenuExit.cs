// 对应 C++ 源：MenuExit.h + MenuExit.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。

namespace FlareEngine
{
    /// <summary>
    /// MenuExit
    ///
    /// 暂停/退出菜单，封装 <see cref="MenuConfig"/> 实例处理继续、保存与退出操作。
    /// 持有的 <see cref="MenuConfig"/> 通过 <see cref="IDisposable"/> 显式释放，
    /// 释放顺序与原始析构函数 <c>~MenuExit()</c> 一致（先 <see cref="_menuConfig"/>，再基类 <see cref="Menu"/>）。
    /// </summary>
    public class MenuExit : Menu, IDisposable
    {
        private MenuConfig? _menuConfig;
        private bool _exitClicked;

        /// <summary>对应 C++ 公有字段 <c>bool reload_music;</c>。</summary>
        public bool ReloadMusic;

        public MenuExit()
        {
            _menuConfig = new MenuConfig(!MenuConfig.IsGameState);
            _exitClicked = false;
            ReloadMusic = false;

            _menuConfig.SetHero(SharedGameResources.Pc);
            Align();
        }

        public override void Align()
        {
            base.Align();
            _menuConfig!.RefreshWidgets();
        }

        public void Logic()
        {
            if (Visible)
            {
                _menuConfig!.Logic();
            }

            if (_menuConfig!.ReloadMusic)
            {
                ReloadMusic = true;
                _menuConfig.ReloadMusic = false;
            }

            if (_menuConfig.ClickedPauseContinue)
            {
                Visible = false;
                _menuConfig.ClickedPauseContinue = false;
            }
            else if (_menuConfig.ClickedPauseExit)
            {
                _exitClicked = true;
                _menuConfig.ClickedPauseExit = false;
            }

            else if (_menuConfig.ClickedPauseSave)
            {
                Visible = false;
                _menuConfig.ClickedPauseSave = false;

                SharedGameResources.Mapr!.RespawnPoint = SharedGameResources.Pc!.Stats.Pos;
                SharedResources.SaveLoad!.SaveGame();
            }
        }

        public override void Render()
        {
            if (Visible)
            {
                // background
                base.Render();

                _menuConfig!.Render();
            }
        }

        public void DisableSave()
        {
            _menuConfig!.SetPauseExitText(!MenuConfig.EnableSaveGame);
            _menuConfig.SetPauseSaveEnabled(!MenuConfig.EnableSaveGame);
        }

        public void HandleCancel()
        {
            if (!Visible)
            {
                _menuConfig!.ResetSelectedTab();
                Visible = true;
            }
            else
            {
                if (!_menuConfig!.InputConfirm!.Visible)
                {
                    Visible = false;
                }
                else
                {
                    // MenuManager will have locked CANCEL before calling this
                    // But we need it unlocked for the input_cofirm dialog. So unlock it here
                    SharedResources.Inpt!.Lock[Input.Cancel] = false;
                }
            }
        }

        public bool IsExitRequested()
        {
            return _exitClicked;
        }

        /// <summary>
        /// 对应 C++ 的 <c>~MenuExit()</c>：释放 menu_config，然后调用基类 <see cref="Menu.Dispose"/>。
        /// </summary>
        public override void Dispose()
        {
            if (_menuConfig != null)
            {
                _menuConfig.Dispose();
                _menuConfig = null;
            }

            base.Dispose();
        }
    }
}
