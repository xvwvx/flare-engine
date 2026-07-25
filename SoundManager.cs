// <自动生成> 对应 C++ 源文件：SoundManager.h + SoundManager.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// SoundManager
    ///
    /// SoundManager 负责音效的加载与播放，每个音效通过一个哈希得到的 SoundID 进行引用。
    /// 若某个音效已被加载，SoundManager.Load() 会返回当前已加载音效对应的 SoundID。
    ///
    /// 本类对应 C++ 抽象基类接口（纯虚函数 = 0），不包含任何具体音频后端实现；
    /// 具体实现（如基于 SDL_mixer 的 SDLSoundManager）将作为派生类在后续单元中单独转换。
    /// </summary>
    public abstract class SoundManager : IDisposable
    {
        /// <summary>对应 C++ <c>static const std::string DEFAULT_CHANNEL;</c>，定义处为 <c>"__global__"</c>。</summary>
        public const string DefaultChannel = "__global__";

        /// <summary>对应 C++ <c>static const FPoint NO_POS;</c>，定义处为 <c>FPoint(0, 0)</c>；FPoint 依据强制规则映射为 Vector2。</summary>
        public static readonly Vector2 NoPos = new Vector2(0, 0);

        public const bool Loop = true;
        public const bool Cleanup = true;

        protected SoundManager()
        {
        }

        /// <summary>
        /// 对应 C++ 的 <c>virtual ~SoundManager()</c>。C# 没有确定性析构，改用 IDisposable，
        /// 由持有本对象的所有者在对应原始 `delete` 调用点显式调用 Dispose()，以保持
        /// "清理时输出一条日志"这一行为的可预测时机，避免使用非确定性的终结器。
        /// </summary>
        public virtual void Dispose()
        {
            Utils.LogInfo("Cleaning up: SoundManager");
            GC.SuppressFinalize(this);
        }

        public abstract SoundID Load(string filename, string errormessage);
        public abstract void Unload(SoundID sid);
        public abstract void Play(SoundID sid, string channel, Vector2 pos, bool loop, bool cleanup = true);
        public abstract void PauseChannel(string channel);
        public abstract void PauseAll();
        public abstract void ResumeAll();
        public abstract void SetVolumeSFX(int value);

        public abstract void LoadMusic(string filename);
        public abstract void UnloadMusic();
        public abstract void PlayMusic();
        public abstract void StopMusic();
        public abstract void SetVolumeMusic(int value);
        public abstract bool IsPlayingMusic();

        public abstract void Logic();
        public abstract void Reset();

        public abstract SoundID GetLastPlayedSID();
    }

    /// <summary>
    /// Playback
    ///
    /// Playback 用于创建播放对象，包含与具体音频 API 无关的、由 SoundManager.Load() 返回的
    /// 音效 id、音效位置、音效时长相关属性，以及应在其上播放该音效的虚拟通道名称。
    /// 对应 C++ 中仅含公有数据成员、无任何业务逻辑的纯数据类。
    /// </summary>
    public class Playback
    {
        public SoundID Sid = -1;
        public string VirtualChannel = "";
        public Vector2 Location = new Vector2();
        public bool Loop = false;
        public bool Paused = false;
        public bool Finished = false;
        public bool Cleanup = true;
    }
}
