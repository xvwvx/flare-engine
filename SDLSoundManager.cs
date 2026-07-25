// <自动生成> 对应 C++ 源文件：SDLSoundManager.h + SDLSoundManager.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using FlareEngine.Sdl;
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// 对应 C++ 中 SDLSoundManager.cpp 内嵌的 <c>class Sound</c>（仅由 SDLSoundManager 访问 refCnt）。
    /// </summary>
    internal class Sound
    {
        public ISdlMixerChunk? Chunk;
        public int RefCnt;

        public Sound()
        {
            Chunk = null;
            RefCnt = 0;
        }
    }

    /// <summary>
    /// SDL_mixer 音频块 opaque 句柄；具体实现由 <see cref="ISdlMixer"/> 后端持有。
    /// </summary>
    internal interface ISdlMixerChunk
    {
    }

    /// <summary>
    /// SDL_mixer 音乐 opaque 句柄；具体实现由 <see cref="ISdlMixer"/> 后端持有。
    /// </summary>
    internal interface ISdlMixerMusic
    {
    }

    /// <summary>
    /// SDL_mixer API 抽象接口。SDLSoundManager 业务逻辑仅通过本接口实例调用音频后端，
    /// 禁止在业务逻辑中直接 P/Invoke 或调用 SDL2 静态方法（迁移规则强制要求）。
    /// </summary>
    internal interface ISdlMixer
    {
        int OpenAudio(uint freq, int format, int channels, int chunksize);
        string GetSdlError();
        string GetCurrentAudioDriver();
        void AllocateChannels(int num);
        void CloseAudio();
        void Resume(int channel);
        void Pause(int channel);
        void SetPosition(int channel, short angle, byte distance);
        void HaltChannel(int channel);
        ISdlMixerChunk? LoadWav(string filename);
        string GetMixError();
        void FreeChunk(ISdlMixerChunk chunk);
        void ChannelFinished(Action<int>? callback);
        int PlayChannel(int channel, ISdlMixerChunk chunk, int loops);
        void Volume(int channel, int volume);
        ISdlMixerMusic? LoadMus(string filename);
        void FreeMusic(ISdlMixerMusic music);
        void VolumeMusic(int volume);
        void PlayMusic(ISdlMixerMusic music, int loops);
        void HaltMusic();
        void PauseMusic();
        void ResumeMusic();
        bool PlayingMusic();
    }

    /// <summary>
    /// 纯托管 SDL_mixer 后端占位实现：提供与 SDL_mixer API 契约一致的通道/资源管理语义，
    /// 供 SDLSoundManager 在无原生 SDL 绑定的 .NET 环境中编译与逻辑回归。
    /// 真实音频输出需替换为平台原生 ISdlMixer 实现（见转换报告"需人工复核"）。
    /// </summary>
    internal sealed class SdlMixerBackend : ISdlMixer
    {
        private const int AudioS16Sys = 0x8010;

        private sealed class MixerChunk : ISdlMixerChunk
        {
            public string Path = "";
        }

        private sealed class MixerMusic : ISdlMixerMusic
        {
            public string Path = "";
        }

        private sealed class ChannelState
        {
            public ISdlMixerChunk? Chunk;
            public int Loops;
            public bool Paused;
        }

        private bool _open;
        private int _maxChannels = 128;
        private int _nextChannelId;
        private readonly Dictionary<int, ChannelState> _channels = new Dictionary<int, ChannelState>();
        private Action<int>? _channelFinishedCallback;
        private MixerMusic? _music;
        private bool _musicPlaying;
        private bool _musicPaused;
        private int _musicVolume;

        public int OpenAudio(uint freq, int format, int channels, int chunksize)
        {
            if (format != AudioS16Sys || channels != 2)
                return -1;
            _open = true;
            return 0;
        }

        public string GetSdlError()
        {
            return _open ? "" : "audio subsystem not open";
        }

        public string GetCurrentAudioDriver()
        {
            return "managed-stub";
        }

        public void AllocateChannels(int num)
        {
            _maxChannels = num;
        }

        public void CloseAudio()
        {
            _channels.Clear();
            _music = null;
            _musicPlaying = false;
            _musicPaused = false;
            _open = false;
        }

        public void Resume(int channel)
        {
            if (channel == -1)
            {
                foreach (var kv in _channels)
                    kv.Value.Paused = false;
                _musicPaused = false;
                return;
            }
            if (_channels.TryGetValue(channel, out ChannelState? state))
                state.Paused = false;
        }

        public void Pause(int channel)
        {
            if (channel == -1)
            {
                foreach (var kv in _channels)
                    kv.Value.Paused = true;
                return;
            }
            if (_channels.TryGetValue(channel, out ChannelState? state))
                state.Paused = true;
        }

        public void SetPosition(int channel, short angle, byte distance)
        {
        }

        public void HaltChannel(int channel)
        {
            _channels.Remove(channel);
        }

        public ISdlMixerChunk? LoadWav(string filename)
        {
            return new MixerChunk { Path = filename };
        }

        public string GetMixError()
        {
            return "";
        }

        public void FreeChunk(ISdlMixerChunk chunk)
        {
        }

        public void ChannelFinished(Action<int>? callback)
        {
            _channelFinishedCallback = callback;
        }

        public int PlayChannel(int channel, ISdlMixerChunk chunk, int loops)
        {
            if (!_open)
                return -1;

            if (channel == -1)
            {
                if (_channels.Count >= _maxChannels)
                    return -1;
                channel = _nextChannelId;
                _nextChannelId++;
            }

            _channels[channel] = new ChannelState
            {
                Chunk = chunk,
                Loops = loops,
                Paused = false
            };
            return channel;
        }

        public void Volume(int channel, int volume)
        {
        }

        public ISdlMixerMusic? LoadMus(string filename)
        {
            if (filename == "")
                return null;
            return new MixerMusic { Path = filename };
        }

        public void FreeMusic(ISdlMixerMusic music)
        {
            if (ReferenceEquals(_music, music))
            {
                _music = null;
                _musicPlaying = false;
                _musicPaused = false;
            }
        }

        public void VolumeMusic(int volume)
        {
            _musicVolume = volume;
        }

        public void PlayMusic(ISdlMixerMusic music, int loops)
        {
            _music = music as MixerMusic;
            _musicPlaying = _music != null;
            _musicPaused = false;
        }

        public void HaltMusic()
        {
            _musicPlaying = false;
            _musicPaused = false;
        }

        public void PauseMusic()
        {
            _musicPaused = true;
        }

        public void ResumeMusic()
        {
            _musicPaused = false;
        }

        public bool PlayingMusic()
        {
            return _musicPlaying && !_musicPaused;
        }
    }

    /// <summary>
    /// SDLSoundManager
    ///
    /// SDL implementation of SoundManager
    /// </summary>
    public class SDLSoundManager : SoundManager
    {
        private readonly Dictionary<SoundID, Sound> _sounds = new Dictionary<SoundID, Sound>();
        private readonly Dictionary<string, int> _channels = new Dictionary<string, int>();
        private readonly Dictionary<int, Playback> _playback = new Dictionary<int, Playback>();

        private ISdlMixerMusic? _music;
        private string _musicFilename = "";
        private SoundID _lastPlayedSid = -1;
        private readonly ISdlMixer _mixer;

        public SDLSoundManager()
            : this(Sdl3Bootstrap.CreateMixer())
        {
        }

        internal SDLSoundManager(ISdlMixer mixer)
        {
            _mixer = mixer;
            _music = null;
            _musicFilename = "";
            _lastPlayedSid = -1;

            Settings settings = SharedResources.Settings!;
            if (settings.Audio && _mixer.OpenAudio(settings.AudioFreq, 0x8010, 2, 1024) != 0)
            {
                Utils.LogError("SDLSoundManager: Error during Mix_OpenAudio: %s", _mixer.GetSdlError());
                settings.Audio = false;
            }

            if (settings.Audio)
            {
                Utils.LogInfo("SoundManager: Using SDLSoundManager (SDL2, %s)", _mixer.GetCurrentAudioDriver());
            }

            _mixer.AllocateChannels(128);
            SetVolumeSFX(settings.SoundVolume);
        }

        /// <summary>
        /// 对应 C++ <c>~SDLSoundManager()</c>：先 unloadMusic，再逐条 unload 音效，最后 Mix_CloseAudio。
        /// </summary>
        public override void Dispose()
        {
            UnloadMusic();

            while (_sounds.Count > 0)
            {
                SoundID sid = _sounds.Keys.Min();
                Unload(sid);
            }

            _mixer.CloseAudio();

            base.Dispose();
        }

        public override void Logic()
        {
            if (_playback.Count == 0)
                return;

            List<int> cleanup = new List<int>();

            List<int> playbackKeys = _playback.Keys.OrderBy(k => k).ToList();
            int playbackIndex = 0;
            while (playbackIndex < playbackKeys.Count)
            {
                int channelKey = playbackKeys[playbackIndex];
                Playback playbackEntry = _playback[channelKey];

                /* if sound is finished and should be unloaded add it to cleanup and continue with next */
                if (playbackEntry.Finished && playbackEntry.Cleanup)
                {
                    cleanup.Add(channelKey);
                    playbackIndex++;
                    continue;
                }

                /* dont process playback sounds without location */
                if (playbackEntry.Location.X == 0 && playbackEntry.Location.Y == 0)
                {
                    playbackIndex++;
                    continue;
                }

                /* control mixing playback depending on distance */
                byte dist = 0;
                Avatar? pc = SharedGameResources.Pc;
                EngineSettings eset = SharedResources.Eset!;
                if (pc != null && eset.Misc.SoundFalloff > 0)
                {
                    float v = Utils.CalcDist(pc.Stats.Pos, playbackEntry.Location) / (float)eset.Misc.SoundFalloff;
                    if (playbackEntry.Loop)
                    {
                        if (v < 1.0f && playbackEntry.Paused)
                        {
                            _mixer.Resume(channelKey);
                            playbackEntry.Paused = false;
                        }
                        else if (v > 1.0f && !playbackEntry.Paused)
                        {
                            _mixer.Pause(channelKey);
                            playbackEntry.Paused = true;
                            playbackIndex++;
                            continue;
                        }
                    }

                    /* update sound mix with new distance/location to hero */
                    v = Math.Min(Math.Max(v, 0.0f), 1.0f);
                    dist = (byte)(255.0 * v);
                }

                _mixer.SetPosition(channelKey, 0, dist);
                playbackIndex++;
            }

            /* clenaup finished soundplayback */
            while (cleanup.Count > 0)
            {
                int cleanupChannel = cleanup[cleanup.Count - 1];

                Playback finishedPlayback = _playback[cleanupChannel];

                Unload(finishedPlayback.Sid);

                /* find and erase virtual channel for playback if exists */
                if (_channels.ContainsKey(finishedPlayback.VirtualChannel))
                    _channels.Remove(finishedPlayback.VirtualChannel);

                _playback.Remove(cleanupChannel);

                cleanup.RemoveAt(cleanup.Count - 1);
            }
        }

        public override void Reset()
        {
            if (_playback.Count == 0)
                return;

            List<int> playbackKeys = _playback.Keys.OrderBy(k => k).ToList();
            int playbackIndex = 0;
            while (playbackIndex < playbackKeys.Count)
            {
                int channelKey = playbackKeys[playbackIndex];
                Playback playbackEntry = _playback[channelKey];

                if (playbackEntry.Loop)
                    _mixer.HaltChannel(channelKey);

                playbackIndex++;
            }
            Logic();
        }

        public override SoundID Load(string filename, string errormessage)
        {
            Sound lsnd = new Sound();
            SoundID sid = 0;

            Settings settings = SharedResources.Settings!;
            if (!settings.Audio)
                return 0;

            string realfilename = SharedResources.Mods!.Locate(filename);
            sid = (SoundID)Utils.HashString(realfilename);
            if (_sounds.TryGetValue(sid, out Sound? existing))
            {
                existing.RefCnt++;
                return sid;
            }

            /* load non existing sound */
            lsnd.Chunk = _mixer.LoadWav(realfilename);
            lsnd.RefCnt = 1;
            if (lsnd.Chunk == null)
            {
                Utils.LogError("SoundManager: %s: Loading sound %s (%s) failed: %s", errormessage,
                    realfilename, filename, _mixer.GetMixError());
                return 0;
            }

            /* we might be loading a sound that was previously loaded and in the midst of playing back
             * so we need to update the ref count to prevent unintentional unloading of our "new" sound */
            List<int> playbackKeysForLoad = _playback.Keys.OrderBy(k => k).ToList();
            int playIndex = 0;
            while (playIndex < playbackKeysForLoad.Count)
            {
                Playback playEntry = _playback[playbackKeysForLoad[playIndex]];
                if (playEntry.Sid == sid)
                    lsnd.RefCnt++;
                playIndex++;
            }

            /* instantiate and add sound to manager */
            Sound psnd = new Sound();
            psnd.Chunk = lsnd.Chunk;
            psnd.RefCnt = lsnd.RefCnt;
            _sounds[sid] = psnd;

            return sid;
        }

        public override void Unload(SoundID sid)
        {
            if (!_sounds.TryGetValue(sid, out Sound? snd))
                return;

            if (--snd.RefCnt == 0)
            {
                if (snd.Chunk != null)
                    _mixer.FreeChunk(snd.Chunk);
                _sounds.Remove(sid);
            }
        }

        public override void Play(SoundID sid, string channel, Vector2 pos, bool loop, bool cleanup = true)
        {
            bool hasVirtualChannel = false;

            // since last_played_sid is primarily used for subtitles, it doesn't make sense to count looped sounds
            if (!loop && sid != 0)
                _lastPlayedSid = sid;

            Settings settings = SharedResources.Settings!;
            if (sid == 0 || !settings.Audio)
                return;

            if (!_sounds.TryGetValue(sid, out Sound? snd))
                return;

            /* create playback object and start playback of sound chunk */
            Playback p = new Playback();
            p.Sid = sid;
            p.Location = pos;
            p.VirtualChannel = channel;
            p.Loop = loop;
            p.Finished = false;
            p.Cleanup = cleanup;

            if (p.VirtualChannel != DefaultChannel)
            {
                /* if playback exists, stop it before playin next sound */
                if (_channels.TryGetValue(p.VirtualChannel, out int existingChannel))
                {
                    // temporarily disable the channel finish callback to avoid setting the 'finished' flag when stopping the channel
                    if (!cleanup)
                        _mixer.ChannelFinished(null);

                    _mixer.HaltChannel(existingChannel);
                }

                _channels[p.VirtualChannel] = -1;
                hasVirtualChannel = true;
            }

            // Let playback own a reference to prevent unloading playbacked sound.
            if (!loop)
                snd.RefCnt++;

            _mixer.ChannelFinished(ChannelFinished);
            int c = _mixer.PlayChannel(-1, snd.Chunk!, loop ? -1 : 0);

            if (c == -1)
                Utils.LogError("SoundManager: Failed to play sound, no more channels available.");

            // precalculate mixing volume if sound has a location
            byte d = 0;
            Avatar? pc = SharedGameResources.Pc;
            EngineSettings eset = SharedResources.Eset!;
            if (pc != null && eset.Misc.SoundFalloff > 0 && (p.Location.X != 0 || p.Location.Y != 0))
            {
                float v = 255.0f * (Utils.CalcDist(pc.Stats.Pos, p.Location) / (float)eset.Misc.SoundFalloff);
                v = Math.Min(Math.Max(v, 0.0f), 255.0f);
                d = (byte)v;
            }

            _mixer.SetPosition(c, 0, d);

            if (hasVirtualChannel)
                _channels[p.VirtualChannel] = c;

            _playback[c] = p;
        }

        public override void PauseChannel(string channel)
        {
            if (_channels.TryGetValue(channel, out int channelId))
            {
                _mixer.Pause(channelId);
            }
        }

        public override void PauseAll()
        {
            _mixer.Pause(-1);
            _mixer.PauseMusic();
        }

        public override void ResumeAll()
        {
            _mixer.Resume(-1);
            _mixer.ResumeMusic();
        }

        private void OnChannelFinished(int channel)
        {
            if (!_playback.TryGetValue(channel, out Playback? pit))
                return;

            pit.Finished = true;

            _mixer.SetPosition(channel, 0, 0);
        }

        private static void ChannelFinished(int channel)
        {
            if (SharedResources.Snd is SDLSoundManager mgr)
                mgr.OnChannelFinished(channel);
        }

        public override void SetVolumeSFX(int value)
        {
            _mixer.Volume(-1, value);
        }

        public override void LoadMusic(string filename)
        {
            Settings settings = SharedResources.Settings!;
            if (!settings.Audio)
                return;

            if (filename == _musicFilename)
            {
                if (!IsPlayingMusic())
                    PlayMusic();
                return;
            }

            UnloadMusic();

            if (filename == "")
                return;

            _music = _mixer.LoadMus(SharedResources.Mods!.Locate(filename));
            if (_music != null)
            {
                _musicFilename = filename;
                PlayMusic();
            }
            else
            {
                Utils.LogError("SoundManager: Couldn't load music file '%s': %s", filename, _mixer.GetMixError());
            }
        }

        public override void UnloadMusic()
        {
            StopMusic();
            if (_music != null)
                _mixer.FreeMusic(_music);
            _music = null;
            _musicFilename = "";
        }

        public override void PlayMusic()
        {
            Settings settings = SharedResources.Settings!;
            if (!settings.Audio || _music == null)
                return;

            _mixer.VolumeMusic(settings.MusicVolume);
            _mixer.PlayMusic(_music, -1);
        }

        public override void StopMusic()
        {
            Settings settings = SharedResources.Settings!;
            if (!settings.Audio || _music == null)
                return;

            _mixer.HaltMusic();
        }

        public override void SetVolumeMusic(int value)
        {
            Settings settings = SharedResources.Settings!;
            if (!settings.Audio || _music == null)
                return;

            _mixer.VolumeMusic(value);
        }

        public override bool IsPlayingMusic()
        {
            Settings settings = SharedResources.Settings!;
            return settings.Audio && _music != null && settings.MusicVolume > 0 && _mixer.PlayingMusic();
        }

        public override SoundID GetLastPlayedSID()
        {
            SoundID ret = _lastPlayedSid;
            _lastPlayedSid = -1;
            return ret;
        }
    }
}
