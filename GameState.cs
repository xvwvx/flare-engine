// 对应 C++ 源：GameState.h + GameState.cpp
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// GameState
    ///
    /// 游戏状态基类：标题画面、主游戏、读档、新建等具体状态均派生自本类。
    /// 负责状态切换请求、加载帧计数、加载提示渲染等通用逻辑。
    ///
    /// 资源管理：<see cref="_loadingTip"/> 对应原始裸指针，构造/<see cref="CopyFrom"/> 中
    /// <c>new WidgetTooltip()</c>、析构/<see cref="Dispose"/> 中 <c>delete</c>；
    /// <see cref="_requestedGameState"/> 由 <see cref="SetRequestedGameState"/> 接管所有权并在
    /// 替换时释放旧状态，但基类析构函数不释放它（与原始 C++ 一致，实际释放由
    /// GameSwitcher 等调用方负责）。本类实现 <see cref="IDisposable"/> 对应虚析构函数。
    /// </summary>
    public class GameState : IDisposable
    {
        /// <summary>对应 C++ 公有字段 <c>bool hasMusic;</c>。</summary>
        public bool HasMusic;

        /// <summary>对应 C++ 公有字段 <c>bool has_background;</c>。</summary>
        public bool HasBackground;

        /// <summary>对应 C++ 公有字段 <c>bool has_frame_background;</c>。</summary>
        public bool HasFrameBackground;

        /// <summary>对应 C++ 公有字段 <c>bool reload_music;</c>。</summary>
        public bool ReloadMusic;

        /// <summary>对应 C++ 公有字段 <c>bool reload_backgrounds;</c>。</summary>
        public bool ReloadBackgrounds;

        /// <summary>对应 C++ 公有字段 <c>bool force_refresh_background;</c>。</summary>
        public bool ForceRefreshBackground;

        /// <summary>对应 C++ 公有字段 <c>bool save_settings_on_exit;</c>。</summary>
        public bool SaveSettingsOnExit;

        /// <summary>对应 C++ 公有字段 <c>int load_counter;</c>。</summary>
        public int LoadCounter;

        protected GameState? RequestedGameState;
        protected bool ExitRequested;
        protected WidgetTooltip? LoadingTip;
        protected TooltipData LoadingTipBuf = new TooltipData();

        public GameState()
        {
            HasMusic = false;
            HasBackground = true;
            HasFrameBackground = false;
            ReloadMusic = false;
            ReloadBackgrounds = false;
            ForceRefreshBackground = false;
            SaveSettingsOnExit = true;
            LoadCounter = 0;
            RequestedGameState = null;
            ExitRequested = false;
            LoadingTip = new WidgetTooltip();
            LoadingTipBuf.AddText(SharedResources.Msg!.Get("Loading..."));
        }

        /// <summary>
        /// 对应 C++ 拷贝构造函数 <c>GameState(const GameState&amp; other)</c>（委托 <c>operator=</c> 逻辑）。
        /// 原始拷贝构造无成员初始化列表，成员先由 C++ 默认初始化，再执行赋值运算符函数体。
        /// </summary>
        public GameState(GameState other)
        {
            CopyFrom(other);
        }

        /// <summary>对应 C++ 赋值运算符 <c>GameState&amp; operator=(const GameState&amp; other)</c>。</summary>
        public GameState CopyFrom(GameState other)
        {
            if (ReferenceEquals(this, other))
                return this;

            HasMusic = other.HasMusic;
            HasBackground = other.HasBackground;
            ReloadMusic = other.ReloadMusic;
            ReloadBackgrounds = other.ReloadBackgrounds;
            ForceRefreshBackground = other.ForceRefreshBackground;
            SaveSettingsOnExit = other.SaveSettingsOnExit;
            LoadCounter = other.LoadCounter;
            RequestedGameState = other.RequestedGameState;
            ExitRequested = other.ExitRequested;
            LoadingTip = new WidgetTooltip();
            LoadingTipBuf.AddText(SharedResources.Msg!.Get("Loading..."));

            return this;
        }

        /// <summary>
        /// 对应 C++ 虚析构函数 <c>~GameState()</c>。声明为 <c>virtual</c> 供派生类
        /// <c>override</c> 并在释放自身资源后调用 <c>base.Dispose()</c>。
        /// </summary>
        public virtual void Dispose()
        {
            SharedResources.RenderDevice!.CleanupQueuedImages();

            if (LoadingTip != null)
            {
                LoadingTip.Dispose();
                LoadingTip = null;
            }

            GC.SuppressFinalize(this);
        }

        public virtual void Logic()
        {
        }

        public virtual void Render()
        {
        }

        public virtual void RefreshWidgets()
        {
        }

        public GameState? GetRequestedGameState()
        {
            return RequestedGameState;
        }

        public void SetRequestedGameState(GameState? newState)
        {
            RequestedGameState?.Dispose();
            RequestedGameState = newState;
            RequestedGameState!.SetLoadingFrame();
            RequestedGameState.RefreshWidgets();
        }

        public bool IsExitRequested()
        {
            return ExitRequested;
        }

        public void SetLoadingFrame()
        {
            LoadCounter = 2;
            SharedResources.Inpt!.Reset();
        }

        public virtual bool IsPaused()
        {
            return false;
        }

        public void ShowLoading()
        {
            if (LoadingTip == null)
                return;

            LoadingTip.Render(LoadingTipBuf, new Int2(SharedResources.Settings!.ViewW, SharedResources.Settings.ViewH), TooltipData.StyleFloat);

            SharedResources.RenderDevice!.CommitFrame();
        }
    }
}
