using System.Runtime.InteropServices;
using SDL3;

namespace FlareEngine.Sdl
{
    /// <summary>
    /// 对应 C++ <c>SDLSoundManager</c> 中使用的 SDL_mixer API。
    /// SDL3 无 Mix_* 通道 API，此处用 Track 模拟 <c>Mix_OpenAudio</c> /
    /// <c>Mix_AllocateChannels(128)</c> / <c>Mix_PlayChannel</c> 等语义。
    /// </summary>
    internal sealed class Sdl3MixerService : ISdlMixer
    {
        private sealed class MixerChunk : ISdlMixerChunk
        {
            public IntPtr Audio = IntPtr.Zero;
            public string Path = "";
        }

        private sealed class MixerMusic : ISdlMixerMusic
        {
            public IntPtr Audio = IntPtr.Zero;
            public string Path = "";
        }

        private sealed class ChannelSlot
        {
            public IntPtr Track = IntPtr.Zero;
            public ISdlMixerChunk? Chunk;
            public bool Paused;
            public float Gain = 1.0f;

            /// <summary>持久化的 stopped 回调 userdata 句柄（存放通道号），避免每次播放都重新分配 GCHandle。</summary>
            public GCHandle CallbackHandle;
        }

        /// <summary>
        /// 强引用持有 stopped 回调委托：SDL 原生侧注册的回调需要托管委托在整个注册期内存活，
        /// 否则轨道停止时回调已回收的委托会直接终止进程
        /// （“A callback was made on a garbage collected delegate”）。
        /// 委托绑定到当前服务实例，只要 mixer 服务存活，委托就保持可达。
        /// </summary>
        private readonly Mixer.TrackStoppedCallback _trackStoppedCallback;

        private IntPtr _mixer = IntPtr.Zero;
        private IntPtr _musicTrack = IntPtr.Zero;
        private MixerMusic? _music;
        private bool _musicPlaying;
        private bool _musicPaused;
        private int _musicVolume = 128;
        private int _maxChannels = 128;
        private ChannelSlot[] _channels = Array.Empty<ChannelSlot>();
        private Action<int>? _channelFinishedCallback;

        public Sdl3MixerService()
        {
            _trackStoppedCallback = OnTrackStopped;
        }

        /// <summary>对应 <c>Mix_OpenAudio(freq, AUDIO_S16SYS, 2, 1024)</c>。</summary>
        public int OpenAudio(uint freq, int format, int channels, int chunksize)
        {
            if (!Mixer.Init())
                return -1;

            unsafe
            {
                SDL.AudioSpec spec = new SDL.AudioSpec
                {
                    Freq = (int)freq,
                    Format = MapAudioFormat(format),
                    Channels = (byte)channels
                };
                _mixer = Mixer.CreateMixerDevice(SDL.AudioDeviceDefaultPlayback, (IntPtr)(&spec));
            }

            return _mixer == IntPtr.Zero ? -1 : 0;
        }

        public string GetSdlError()
        {
            return SDL.GetError() ?? string.Empty;
        }

        /// <summary>对应 <c>SDL_GetCurrentAudioDriver()</c>（C++ 日志：<c>Mix_OpenAudio</c> 成功后打印）。</summary>
        public string GetCurrentAudioDriver()
        {
            return SDL.GetCurrentAudioDriver() ?? string.Empty;
        }

        /// <summary>对应 <c>Mix_AllocateChannels(128)</c>。</summary>
        public void AllocateChannels(int num)
        {
            _maxChannels = num;
            _channels = new ChannelSlot[num];
            for (int i = 0; i < num; ++i)
            {
                _channels[i] = new ChannelSlot
                {
                    Track = Mixer.CreateTrack(_mixer)
                };
            }

            _musicTrack = Mixer.CreateTrack(_mixer);
        }

        public void CloseAudio()
        {
            for (int i = 0; i < _channels.Length; ++i)
            {
                if (_channels[i].CallbackHandle.IsAllocated)
                    _channels[i].CallbackHandle.Free();
            }

            if (_mixer != IntPtr.Zero)
            {
                Mixer.DestroyMixer(_mixer);
                _mixer = IntPtr.Zero;
            }
            Mixer.Quit();
            _channels = Array.Empty<ChannelSlot>();
            _musicTrack = IntPtr.Zero;
            _music = null;
            _musicPlaying = false;
            _musicPaused = false;
        }

        public void Resume(int channel)
        {
            if (channel == -1)
            {
                for (int i = 0; i < _channels.Length; ++i)
                {
                    _channels[i].Paused = false;
                    if (_channels[i].Track != IntPtr.Zero)
                        Mixer.ResumeTrack(_channels[i].Track);
                }
                _musicPaused = false;
                if (_musicTrack != IntPtr.Zero && _musicPlaying)
                    Mixer.ResumeTrack(_musicTrack);
                return;
            }

            if (channel >= 0 && channel < _channels.Length)
            {
                _channels[channel].Paused = false;
                if (_channels[channel].Track != IntPtr.Zero)
                    Mixer.ResumeTrack(_channels[channel].Track);
            }
        }

        public void Pause(int channel)
        {
            if (channel == -1)
            {
                for (int i = 0; i < _channels.Length; ++i)
                {
                    _channels[i].Paused = true;
                    if (_channels[i].Track != IntPtr.Zero)
                        Mixer.PauseTrack(_channels[i].Track);
                }
                _musicPaused = true;
                if (_musicTrack != IntPtr.Zero)
                    Mixer.PauseTrack(_musicTrack);
                return;
            }

            if (channel >= 0 && channel < _channels.Length)
            {
                _channels[channel].Paused = true;
                if (_channels[channel].Track != IntPtr.Zero)
                    Mixer.PauseTrack(_channels[channel].Track);
            }
        }

        public void SetPosition(int channel, short angle, byte distance)
        {
            if (channel < 0 || channel >= _channels.Length)
                return;

            float gain = _channels[channel].Gain * (1.0f - (distance / 255.0f));
            if (_channels[channel].Track != IntPtr.Zero)
                Mixer.SetTrackGain(_channels[channel].Track, gain);
        }

        public void HaltChannel(int channel)
        {
            if (channel >= 0 && channel < _channels.Length && _channels[channel].Track != IntPtr.Zero)
            {
                // SDL3 的 StopTrack 本身会触发 stopped 回调（对应 SDL2 Mix_HaltChannel 触发
                // channel_finished），因此这里不再手动触发，避免 ChannelFinished 被调用两次。
                Mixer.StopTrack(_channels[channel].Track, 0);
                _channels[channel].Chunk = null;
            }
        }

        public ISdlMixerChunk? LoadWav(string filename)
        {
            if (_mixer == IntPtr.Zero)
                return null;

            IntPtr audio = Mixer.LoadAudio(_mixer, filename, false);
            if (audio == IntPtr.Zero)
                return null;

            return new MixerChunk { Audio = audio, Path = filename };
        }

        public string GetMixError()
        {
            return SDL.GetError() ?? string.Empty;
        }

        public void FreeChunk(ISdlMixerChunk chunk)
        {
            if (chunk is MixerChunk native && native.Audio != IntPtr.Zero)
            {
                Mixer.DestroyAudio(native.Audio);
                native.Audio = IntPtr.Zero;
            }
        }

        public void ChannelFinished(Action<int>? callback)
        {
            _channelFinishedCallback = callback;
        }

        public int PlayChannel(int channel, ISdlMixerChunk chunk, int loops)
        {
            if (_mixer == IntPtr.Zero || _channels.Length == 0)
                return -1;
            if (chunk is not MixerChunk native || native.Audio == IntPtr.Zero)
                return -1;

            // 对应 Mix_PlayChannel(-1, ...)：在第一个空闲通道上播放并返回其真实编号。
            // 原实现直接对 channel < 0 返回 -1，导致调用方把 -1 当作通道号存入
            // _playback，ChannelFinished 回调永远匹配不上，声音/音乐无法正常播放。
            if (channel == -1)
            {
                channel = FindFreeChannel();
                if (channel == -1)
                    return -1;
            }

            if (channel < 0 || channel >= _channels.Length)
                return -1;

            ChannelSlot slot = _channels[channel];
            IntPtr track = slot.Track;
            if (track == IntPtr.Zero)
                return -1;

            // 每个通道持久持有回调 userdata（通道号），避免每次播放都新建 GCHandle 造成泄漏。
            if (!slot.CallbackHandle.IsAllocated)
                slot.CallbackHandle = GCHandle.Alloc(channel);

            // 复用强引用持有的委托实例（见 _trackStoppedCallback），避免方法组新建的委托被 GC 回收。
            Mixer.SetTrackStoppedCallback(track, _trackStoppedCallback, GCHandle.ToIntPtr(slot.CallbackHandle));
            Mixer.SetTrackAudio(track, native.Audio);
            Mixer.SetTrackLoops(track, loops);
            slot.Chunk = chunk;
            slot.Gain = 1.0f;
            Mixer.SetTrackGain(track, 1.0f);

            if (!Mixer.PlayTrack(track, 0))
                return -1;

            return channel;
        }

        /// <summary>返回第一个未在播放的通道编号；全部被占用时返回 -1。</summary>
        private int FindFreeChannel()
        {
            for (int i = 0; i < _channels.Length; ++i)
            {
                IntPtr track = _channels[i].Track;
                if (track != IntPtr.Zero && !Mixer.TrackPlaying(track))
                    return i;
            }
            return -1;
        }

        public void Volume(int channel, int volume)
        {
            float gain = volume / 128.0f;
            if (channel >= 0 && channel < _channels.Length)
            {
                _channels[channel].Gain = gain;
                if (_channels[channel].Track != IntPtr.Zero)
                    Mixer.SetTrackGain(_channels[channel].Track, gain);
            }
        }

        public ISdlMixerMusic? LoadMus(string filename)
        {
            if (_mixer == IntPtr.Zero)
                return null;

            IntPtr audio = Mixer.LoadAudio(_mixer, filename, false);
            if (audio == IntPtr.Zero)
                return null;

            return new MixerMusic { Audio = audio, Path = filename };
        }

        public void FreeMusic(ISdlMixerMusic music)
        {
            if (music is MixerMusic native && native.Audio != IntPtr.Zero)
            {
                Mixer.DestroyAudio(native.Audio);
                native.Audio = IntPtr.Zero;
            }
        }

        public void VolumeMusic(int volume)
        {
            _musicVolume = volume;
            if (_musicTrack != IntPtr.Zero)
                Mixer.SetTrackGain(_musicTrack, volume / 128.0f);
        }

        public void PlayMusic(ISdlMixerMusic music, int loops)
        {
            if (_musicTrack == IntPtr.Zero || music is not MixerMusic native || native.Audio == IntPtr.Zero)
                return;

            _music = native;
            Mixer.SetTrackAudio(_musicTrack, native.Audio);
            Mixer.SetTrackLoops(_musicTrack, loops);
            Mixer.SetTrackGain(_musicTrack, _musicVolume / 128.0f);
            Mixer.PlayTrack(_musicTrack, 0);
            _musicPlaying = true;
            _musicPaused = false;
        }

        public void HaltMusic()
        {
            if (_musicTrack != IntPtr.Zero)
                Mixer.StopTrack(_musicTrack, 0);
            _musicPlaying = false;
            _musicPaused = false;
        }

        public void PauseMusic()
        {
            if (_musicTrack != IntPtr.Zero)
                Mixer.PauseTrack(_musicTrack);
            _musicPaused = true;
        }

        public void ResumeMusic()
        {
            if (_musicTrack != IntPtr.Zero)
                Mixer.ResumeTrack(_musicTrack);
            _musicPaused = false;
        }

        public bool PlayingMusic()
        {
            return _musicPlaying && !_musicPaused && _musicTrack != IntPtr.Zero && Mixer.TrackPlaying(_musicTrack);
        }

        private void OnTrackStopped(IntPtr userdata, IntPtr track)
        {
            if (userdata == IntPtr.Zero)
                return;

            // 句柄生命周期由对应 ChannelSlot 管理（CloseAudio 时统一释放），此处只读取通道号，不释放。
            GCHandle handle = GCHandle.FromIntPtr(userdata);
            if (handle.Target is int channel)
                FireChannelFinished(channel);
        }

        private void FireChannelFinished(int channel)
        {
            _channelFinishedCallback?.Invoke(channel);
        }

        /// <summary>将 C++ <c>AUDIO_S16SYS</c> (0x8010) 等 SDL2 音频格式常量映射到 SDL3。</summary>
        private static SDL.AudioFormat MapAudioFormat(int sdl2Format)
        {
            // C++ SDLSoundManager 传入 AUDIO_S16SYS (0x8010)
            if (sdl2Format == 0x8010)
                return SDL.AudioFormat.AudioS16LE;

            return SDL.AudioFormat.AudioS16LE;
        }
    }
}
