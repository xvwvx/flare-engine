// <自动生成> 对应 C++ 源文件：Settings.h + Settings.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using System.Globalization;

namespace FlareEngine
{
    /// <summary>
    /// Settings
    ///
    /// 用户可配置的游戏设置。C++ 原始实现通过 <c>std::type_info*</c> + <c>void*</c> 裸指针
    /// 让 config 列表能够以统一的方式读写任意类型的成员变量；C# 没有安全的裸指针等价物，
    /// 改用 <c>Func&lt;object&gt;</c>/<c>Action&lt;object&gt;</c> 委托对（getter/setter）
    /// 分别承载"读取当前值"和"写入新值"的能力，由 SetConfigDefault 时按字段逐一绑定，
    /// 这是本次迁移中处理 C++ 泛型反射式配置系统的标准方案（委托取代裸指针）。
    /// </summary>
    public class Settings
    {
        public const float LogicFps = 60f;

        public const int LootTipsDefault = 0;
        public const int LootTipsShowAll = 1;
        public const int LootTipsHideAll = 2;

        public const int MinimapNormal = 0;
        public const int Minimap2x = 1;
        public const int MinimapHidden = 2;

        public const int LhpWarnNone = 0;
        public const int LhpWarnAll = 1;
        public const int LhpWarnTextCursor = 2;
        public const int LhpWarnTextSound = 3;
        public const int LhpWarnCursorSound = 4;
        public const int LhpWarnText = 5;
        public const int LhpWarnCursor = 6;
        public const int LhpWarnSound = 7;

        public const int JoyDeadzoneMin = 8000;
        public const int JoyDeadzoneMax = 32767;

        // Video Settings
        public bool Fullscreen;
        public ushort ScreenW;
        public ushort ScreenH;
        public bool Hwsurface;
        public bool Vsync;
        public bool TextureFilter;
        public bool DpiScaling;
        public ushort MaxFramesPerSec;
        public string RenderDeviceName = "";
        public bool ChangeGamma;
        public float Gamma;
        public bool ParallaxLayers;
        public ushort MinRenderSize;
        public ushort MaxRenderSize;
        public bool FadeWalls;

        // Audio Settings
        public ushort MusicVolume;
        public ushort SoundVolume;
        public bool MuteOnFocusLoss;
        public uint AudioFreq;

        // Input Settings
        public bool MouseMove;
        public bool MouseMoveSwap;
        public bool MouseMoveAttack;
        public bool EnableJoystick;
        public int JoystickDevice;
        public bool MouseAim;
        public bool NoMouse;
        public int JoyDeadzone;
        public float TouchScale;
        public bool JoystickRumble;

        // Game Settings
        public bool AutoEquip;
        public int AutoLoot;
        public int LowHpWarningType;
        public int LowHpThreshold;

        // Interface Settings
        public bool CombatText;
        public bool ShowFps;
        public bool Colorblind;
        public bool HardwareCursor;
        public bool DevMode;
        public bool DevHud;
        public int LootTooltips;
        public bool StatbarLabels;
        public bool StatbarAutohide;
        public bool Subtitles;
        public int MinimapMode;
        public bool EntityMarkers;
        public bool ItemCompareTips;
        public bool PauseOnFocusLoss;

        // Language Settings
        public string Language = "";

        // Misc
        public int PrevSaveSlot;
        public bool SetupLanguage;
        public bool SetupMousemove;
        public bool EnableThreadedImageLoad;

        // Dev console: shortcut commands
        public string DevCmd1 = "";
        public string DevCmd2 = "";
        public string DevCmd3 = "";

        // NOTE Everything below is not part of the user's settings.txt, but somehow ended up here
        // TODO Move these to more appropriate locations?

        // Path info
        public string PathConf = ""; // user-configurable settings files
        public string PathUser = ""; // important per-user data (saves)
        public string PathData = ""; // common game data
        public string CustomPathData = ""; // user-defined replacement for PATH_DATA
        public bool CustomPathDataClear;
        public bool CustomPathDataSave;
        public string Game = ""; // if set, a sub-dir will be used in PATH_CONF. The config menu's mod list will also be filtered to match.

        // Command-line settings
        public string LoadSlot = "";
        public string LoadScript = "";

        // Misc
        public ushort ViewW;
        public ushort ViewH;
        public ushort ViewWHalf;
        public ushort ViewHHalf;
        public float ViewScaling;
        public float DisplayScale;   // Window size multiplier (1.0 = base resolution, 2.0 = 2x display)

        public bool Audio;

        public bool Touchscreen;
        public bool MouseScaled; // mouse position is automatically scaled to ViewW * ViewH resolution

        public bool ShowHud;

        public float EncounterDist;

        public bool SoftReset;

        public bool SafeVideo;

        /// <summary>
        /// 对应 C++ 的 <c>class ConfigEntry</c>。用 Getter/Setter 委托取代 <c>void* storage</c>，
        /// 用 <c>System.Type</c> 取代 <c>const std::type_info*</c>。
        /// </summary>
        private class ConfigEntry
        {
            public string Name = "";
            public Type Type = typeof(object);
            public string DefaultVal = "";
            public Func<object> Getter = () => (object)0;
            public Action<object> Setter = _ => { };
            public string Comment = "";
        }

        private readonly List<ConfigEntry> _config;

        public Settings()
        {
            PathConf = "";
            PathUser = "";
            PathData = "";
            CustomPathData = "";
            CustomPathDataClear = false;
            CustomPathDataSave = false;
            Game = "";
            LoadSlot = "";
            LoadScript = "";
            ViewW = 0;
            ViewH = 0;
            ViewWHalf = 0;
            ViewHHalf = 0;
            ViewScaling = 1.0f;
            DisplayScale = 2.0f;     // default 2x display
            Audio = true;
            Touchscreen = false;
            MouseScaled = true;
            ShowHud = true;
            EncounterDist = 0; // set in UpdateScreenVars()
            SoftReset = false;
            SafeVideo = false;

            _config = new List<ConfigEntry>(56);
            for (int i = 0; i < 56; i++)
                _config.Add(new ConfigEntry());

            SetConfigDefault(0, "fullscreen", typeof(bool), "1", () => Fullscreen, v => Fullscreen = (bool)v, "Fullscreen mode | 0 = disable, 1 = enable");
            SetConfigDefault(1, "resolution_w", typeof(ushort), "640", () => (ushort)(ScreenW / DisplayScale), v => ScreenW = (ushort)v, "Window size");
            SetConfigDefault(2, "resolution_h", typeof(ushort), "480", () => (ushort)(ScreenH / DisplayScale), v => ScreenH = (ushort)v, "");
            SetConfigDefault(3, "music_volume", typeof(ushort), "96", () => MusicVolume, v => MusicVolume = (ushort)v, "Music and sound volume | 0 = silent, 128 = maximum");
            SetConfigDefault(4, "sound_volume", typeof(ushort), "128", () => SoundVolume, v => SoundVolume = (ushort)v, "");
            SetConfigDefault(5, "combat_text", typeof(bool), "1", () => CombatText, v => CombatText = (bool)v, "Display floating damage text | 0 = disable, 1 = enable");
            SetConfigDefault(6, "mouse_move", typeof(bool), "0", () => MouseMove, v => MouseMove = (bool)v, "Use mouse to move | 0 = disable, 1 = enable");
            SetConfigDefault(7, "hwsurface", typeof(bool), "1", () => Hwsurface, v => Hwsurface = (bool)v, "Hardware surfaces & V-sync. Try disabling for performance. | 0 = disable, 1 = enable");
            SetConfigDefault(8, "vsync", typeof(bool), "1", () => Vsync, v => Vsync = (bool)v, "");
            SetConfigDefault(9, "texture_filter", typeof(bool), "1", () => TextureFilter, v => TextureFilter = (bool)v, "Texture filter quality | 0 = nearest neighbor (worst), 1 = linear (best)");
            SetConfigDefault(10, "dpi_scaling", typeof(bool), "0", () => DpiScaling, v => DpiScaling = (bool)v, "DPI-based render scaling | 0 = disable, 1 = enable");
            SetConfigDefault(11, "parallax_layers", typeof(bool), "1", () => ParallaxLayers, v => ParallaxLayers = (bool)v, "Rendering of parallax map layers | 0 = disable, 1 = enable");
            SetConfigDefault(12, "max_fps", typeof(ushort), "60", () => MaxFramesPerSec, v => MaxFramesPerSec = (ushort)v, "Maximum frames per second | 60 = default");
            SetConfigDefault(13, "renderer", typeof(string), "sdl_hardware", () => RenderDeviceName, v => RenderDeviceName = (string)v, "Default render device. | sdl_hardware = default, Try sdl for compatibility");
            SetConfigDefault(14, "enable_joystick", typeof(bool), "1", () => EnableJoystick, v => EnableJoystick = (bool)v, "Joystick settings.");
            SetConfigDefault(15, "joystick_device", typeof(int), "0", () => JoystickDevice, v => JoystickDevice = (int)v, "");
            SetConfigDefault(16, "joystick_deadzone", typeof(int), "8000", () => JoyDeadzone, v => JoyDeadzone = (int)v, "");
            SetConfigDefault(17, "language", typeof(string), "en", () => Language, v => Language = (string)v, "2-letter language code.");
            SetConfigDefault(18, "change_gamma", typeof(bool), "0", () => ChangeGamma, v => ChangeGamma = (bool)v, "Allow changing screen gamma (experimental) | 0 = disable, 1 = enable");
            SetConfigDefault(19, "gamma", typeof(float), "1.0", () => Gamma, v => Gamma = (float)v, "Screen gamma. Requires change_gamma=1 | 0.5 = darkest, 2.0 = lightest");
            SetConfigDefault(20, "mouse_aim", typeof(bool), "1", () => MouseAim, v => MouseAim = (bool)v, "Use mouse to aim | 0 = disable, 1 = enable");
            SetConfigDefault(21, "no_mouse", typeof(bool), "0", () => NoMouse, v => NoMouse = (bool)v, "Make using mouse secondary, give full control to keyboard | 0 = disable, 1 = enable");
            SetConfigDefault(22, "show_fps", typeof(bool), "0", () => ShowFps, v => ShowFps = (bool)v, "Show frames per second | 0 = disable, 1 = enable");
            SetConfigDefault(23, "colorblind", typeof(bool), "0", () => Colorblind, v => Colorblind = (bool)v, "Enable colorblind help text | 0 = disable, 1 = enable");
            SetConfigDefault(24, "hardware_cursor", typeof(bool), "0", () => HardwareCursor, v => HardwareCursor = (bool)v, "Use the system mouse cursor | 0 = disable, 1 = enable");
            SetConfigDefault(25, "dev_mode", typeof(bool), "0", () => DevMode, v => DevMode = (bool)v, "Developer mode | 0 = disable, 1 = enable");
            SetConfigDefault(26, "dev_hud", typeof(bool), "1", () => DevHud, v => DevHud = (bool)v, "Show additional information on-screen when dev_mode=1 | 0 = disable, 1 = enable");
            SetConfigDefault(27, "loot_tooltips", typeof(int), "0", () => LootTooltips, v => LootTooltips = (int)v, "Loot tooltip mode | 0 = normal, 1 = show all, 2 = hide all");
            SetConfigDefault(28, "statbar_labels", typeof(bool), "0", () => StatbarLabels, v => StatbarLabels = (bool)v, "Always show labels on HP/MP/XP bars | 0 = disable, 1 = enable");
            SetConfigDefault(29, "statbar_autohide", typeof(bool), "1", () => StatbarAutohide, v => StatbarAutohide = (bool)v, "Allow the HP/MP/XP bars to auto-hide on inactivity | 0 = disable, 1 = enable");
            SetConfigDefault(30, "auto_equip", typeof(bool), "1", () => AutoEquip, v => AutoEquip = (bool)v, "Automatically equip items | 0 = disable, 1 = enable");
            SetConfigDefault(31, "subtitles", typeof(bool), "0", () => Subtitles, v => Subtitles = (bool)v, "Subtitles | 0 = disable, 1 = enable");
            SetConfigDefault(32, "minimap_mode", typeof(int), "0", () => MinimapMode, v => MinimapMode = (int)v, "Mini-map display mode | 0 = normal, 1 = 2x zoom, 2 = hidden");
            SetConfigDefault(33, "mouse_move_swap", typeof(bool), "0", () => MouseMoveSwap, v => MouseMoveSwap = (bool)v, "Use 'Main2' as the movement action when mouse_move=1 | 0 = disable, 1 = enable");
            SetConfigDefault(34, "mouse_move_attack", typeof(bool), "1", () => MouseMoveAttack, v => MouseMoveAttack = (bool)v, "Allow attacking with the mouse movement button if an enemy is targeted and in range | 0 = disable, 1 = enable");
            SetConfigDefault(35, "prev_save_slot", typeof(int), "-1", () => PrevSaveSlot, v => PrevSaveSlot = (int)v, "Index of the last used save slot");
            SetConfigDefault(36, "low_hp_warning_type", typeof(int), "1", () => LowHpWarningType, v => LowHpWarningType = (int)v, "Low health warning type settings | 0 = disable, 1 = all, 2 = message & cursor, 3 = message & sound, 4 = cursor & sound , 5 = message, 6 = cursor, 7 = sound");
            SetConfigDefault(37, "low_hp_threshold", typeof(int), "20", () => LowHpThreshold, v => LowHpThreshold = (int)v, "Low HP warning threshold percentage");
            SetConfigDefault(38, "item_compare_tips", typeof(bool), "1", () => ItemCompareTips, v => ItemCompareTips = (bool)v, "Show comparison tooltips for equipped items of the same type | 0 = disable, 1 = enable");
            SetConfigDefault(39, "min_render_size", typeof(ushort), "0", () => MinRenderSize, v => MinRenderSize = (ushort)v, "Overrides the minimum height (in pixels) of the internal render surface | 0 = ignore this setting");
            SetConfigDefault(40, "max_render_size", typeof(ushort), "0", () => MaxRenderSize, v => MaxRenderSize = (ushort)v, "Overrides the maximum height (in pixels) of the internal render surface | 0 = ignore this setting");
            SetConfigDefault(41, "touch_controls", typeof(bool), "0", () => Touchscreen, v => Touchscreen = (bool)v, "Enables touch screen controls | 0 = disable, 1 = enable");
            SetConfigDefault(42, "touch_scale", typeof(float), "1.0", () => TouchScale, v => TouchScale = (float)v, "Factor used to scale the touch controls | 1.0 = 100 percent scale");
            SetConfigDefault(43, "mute_on_focus_loss", typeof(bool), "1", () => MuteOnFocusLoss, v => MuteOnFocusLoss = (bool)v, "Mute game audio when the game window loses focus | 0 = disable, 1 = enable");
            SetConfigDefault(44, "pause_on_focus_loss", typeof(bool), "1", () => PauseOnFocusLoss, v => PauseOnFocusLoss = (bool)v, "Pause game when the game window loses focus | 0 = disable, 1 = enable");
            SetConfigDefault(45, "audio_freq", typeof(uint), "44100", () => AudioFreq, v => AudioFreq = (uint)v, "Audio playback frequency in Hz. Default is 44100");
            SetConfigDefault(46, "dev_cmd_1", typeof(string), "toggle_fps", () => DevCmd1, v => DevCmd1 = (string)v, "Custom developer console shortcut command");
            SetConfigDefault(47, "dev_cmd_2", typeof(string), "toggle_devhud", () => DevCmd2, v => DevCmd2 = (string)v, "Custom developer console shortcut command");
            SetConfigDefault(48, "dev_cmd_3", typeof(string), "toggle_hud", () => DevCmd3, v => DevCmd3 = (string)v, "Custom developer console shortcut command");
            SetConfigDefault(49, "auto_loot", typeof(int), "1", () => AutoLoot, v => AutoLoot = (int)v, "Automatically pick up loot | 0 = disable, 1 = enable, 2 = currency only");
            SetConfigDefault(50, "joystick_rumble", typeof(bool), "1", () => JoystickRumble, v => JoystickRumble = (bool)v, "Enables joystick rumble/vibrartion | 0 = disable, 1 = enable");
            SetConfigDefault(51, "enable_threaded_image_load", typeof(bool), "1", () => EnableThreadedImageLoad, v => EnableThreadedImageLoad = (bool)v, "Enables multi-threaded image loading. Try disabling to reduce memory usage or fix instability.");
            SetConfigDefault(52, "fade_walls", typeof(bool), "1", () => FadeWalls, v => FadeWalls = (bool)v, "Lowers the opacity of walls that are covering the player. 0 = disable, 1 = enable");
            SetConfigDefault(53, "setup_language", typeof(bool), "0", () => SetupLanguage, v => SetupLanguage = (bool)v, "(First-time-launch setup) Language | 0 = show dialog, 1 = no dialog");
            SetConfigDefault(54, "setup_mousemove", typeof(bool), "0", () => SetupMousemove, v => SetupMousemove = (bool)v, "(First-time-launch setup) Mouse movement | 0 = show dialog, 1 = no dialog");
            SetConfigDefault(55, "display_scale", typeof(float), "2.0", () => DisplayScale, v => DisplayScale = Math.Clamp((float)v, 1.0f, 4.0f), "Window display scale multiplier | 1.0 = base resolution, 2.0 = 2x display (default), 4.0 = 4x display");
        }

        private void SetConfigDefault(int index, string name, Type type, string defaultVal, Func<object> getter, Action<object> setter, string comment)
        {
            if (index < 0 || index >= _config.Count)
            {
                Utils.LogError("Settings: Can't set default config value; %u is not a valid index.", index);
                return;
            }

            _config[index].Name = name;
            _config[index].Type = type;
            _config[index].DefaultVal = defaultVal;
            _config[index].Getter = getter;
            _config[index].Setter = setter;
            _config[index].Comment = comment;
        }

        private int GetConfigEntry(string name)
        {
            for (int i = 0; i < _config.Count; i++)
            {
                if (_config[i].Name == name)
                    return i;
            }

            Utils.LogError("Settings: '%s' is not a valid configuration key.", name);
            return _config.Count;
        }

        public void LoadSettings()
        {
            // init defaults
            for (int i = 0; i < _config.Count; i++)
            {
                if (Parse.TryParseValue(_config[i].Type, _config[i].DefaultVal, out object? value) && value != null)
                    _config[i].Setter(value);
            }

            // try read from file
            bool foundSettings = false;

            using FileParser infile = new FileParser();
            if (infile.Open(PathConf + "settings.txt", !FileParser.ModFile, FileParser.ErrorNormal))
            {
                foundSettings = true;
            }
            else if (infile.Open("engine/default_settings.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                foundSettings = true;
            }

            if (!foundSettings)
            {
                LoadMobileDefaults();
                SaveSettings();
            }
            else
            {
                while (infile.Next())
                {
                    int entry = GetConfigEntry(infile.Key);
                    if (entry != _config.Count)
                    {
                        if (Parse.TryParseValue(_config[entry].Type, infile.Val, out object? value) && value != null)
                            _config[entry].Setter(value);
                    }
                }
                infile.Close();

                // validate joystick deadzone value
                // TODO validation for all setting vars?
                if (JoyDeadzone < JoyDeadzoneMin || JoyDeadzone > JoyDeadzoneMax)
                {
                    JoyDeadzone = JoyDeadzoneMin;
                }
            }

            // Force using the software renderer if safe mode is enabled
            if (SafeVideo)
            {
                RenderDeviceName = "sdl";
            }
        }

        /// <summary>
        /// 保存当前主要设置（主要是视频与音频设置）。
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                using StreamWriter outfile = new StreamWriter(PathConf + "settings.txt", append: false);

                // comment
                outfile.Write("## flare-engine settings file ##" + "\n");

                for (int i = 0; i < _config.Count; i++)
                {
                    // write additional newline before the next section
                    if (i != 0 && !string.IsNullOrEmpty(_config[i].Comment))
                        outfile.Write("\n");

                    if (!string.IsNullOrEmpty(_config[i].Comment))
                    {
                        outfile.Write("# " + _config[i].Comment + "\n");
                    }
                    outfile.Write(_config[i].Name + "=" + Parse.ValueToString(_config[i].Type, _config[i].Getter()) + "\n");
                }

                Platform.Instance.FsCommit();
            }
            catch (IOException)
            {
                Utils.LogError("Settings: Unable to write settings file. No write access or disk is full!");
            }
        }

        /// <summary>
        /// 加载除视频设置以外的所有默认设置。
        /// </summary>
        public void LoadDefaults()
        {
            // HACK init defaults except video and one-time flags
            for (int i = 4; i < _config.Count - 2; i++)
            {
                if (Parse.TryParseValue(_config[i].Type, _config[i].DefaultVal, out object? value) && value != null)
                    _config[i].Setter(value);
            }

            LoadMobileDefaults();
        }

        /// <summary>
        /// 为移动设备设置必要的设置项。
        /// </summary>
        private void LoadMobileDefaults()
        {
            if (Platform.Instance.IsMobileDevice)
            {
                MouseMove = false;
                NoMouse = false;
                EnableJoystick = false;
                HardwareCursor = true;
                Touchscreen = true;
                Fullscreen = true;
                DpiScaling = true;
            }
        }

        /// <summary>
        /// 部分变量依赖于 ViewW 与 ViewH，在此处更新它们。
        /// </summary>
        public void UpdateScreenVars()
        {
            EngineSettings eset = SharedResources.Eset!;

            if (eset.Tileset.TileW > 0 && eset.Tileset.TileH > 0)
            {
                if (eset.Tileset.Orientation == EngineSettings.TilesetSettings.TilesetIsometric)
                    EncounterDist = MathF.Sqrt(MathF.Pow((float)(ViewW / eset.Tileset.TileW), 2f) + MathF.Pow((float)(ViewH / eset.Tileset.TileHHalf), 2f)) / 2f;
                else if (eset.Tileset.Orientation == EngineSettings.TilesetSettings.TilesetOrthogonal)
                    EncounterDist = MathF.Sqrt(MathF.Pow((float)(ViewW / eset.Tileset.TileW), 2f) + MathF.Pow((float)(ViewH / eset.Tileset.TileH), 2f)) / 2f;
            }
        }

        public void LogSettings()
        {
            for (int i = 0; i < _config.Count; ++i)
            {
                Utils.LogInfo("Settings: %s=%s", _config[i].Name, ConfigValueToString(_config[i].Type, _config[i].Getter()));
            }
        }

        public void SetCustomPathData()
        {
            string customDataPathFile = Filesystem.ConvertSlashes(PathConf + "custom_data_path.txt");

            if (CustomPathDataClear)
            {
                if (Filesystem.PathExists(customDataPathFile))
                {
                    Utils.LogInfo("Settings: Removing custom data path file: %s", customDataPathFile);
                    Filesystem.RemoveFile(customDataPathFile);
                }
            }

            bool writeFile = false;

            if (string.IsNullOrEmpty(CustomPathData))
            {
                // --data-path command line flag not used, so try loading the saved custom_path_data from config
                try
                {
                    using StreamReader infile = new StreamReader(customDataPathFile);
                    while (!infile.EndOfStream)
                    {
                        string line = Parse.GetLine(infile);

                        if (Parse.SkipLine(line))
                            continue;

                        string trimmedLine = Parse.Trim(line);

                        if (!string.IsNullOrEmpty(trimmedLine))
                        {
                            CustomPathData = trimmedLine;
                            break;
                        }
                    }
                }
                catch (IOException)
                {
                    // 对应原始代码中 infile.open 失败后 infile.good() 恒为 false、循环不执行的行为。
                }
            }
            else if (CustomPathDataSave)
            {
                writeFile = true;
            }

            if (string.IsNullOrEmpty(CustomPathData))
            {
                // no custom_path_data in config, either
                return;
            }

            // Expand leading tilde as home directory
            if (CustomPathData == "~")
            {
                CustomPathData = (Environment.GetEnvironmentVariable("HOME") ?? "") + "/";
            }
            else if (CustomPathData.Length >= 2 && CustomPathData.Substring(0, 2) == "~/")
            {
                string pathEnd = CustomPathData.Substring(2);
                CustomPathData = (Environment.GetEnvironmentVariable("HOME") ?? "") + "/" + pathEnd;
            }

            if (!string.IsNullOrEmpty(CustomPathData))
            {
                // ensure the path has a trailing slash
                CustomPathData = Filesystem.RemoveTrailingSlash(CustomPathData);
                CustomPathData = Filesystem.ConvertSlashes(CustomPathData + "/", Filesystem.KeepTrailingSlash);
            }

            if (Filesystem.PathExists(CustomPathData))
            {
                Utils.LogInfo("Custom data path: \"%s\"", CustomPathData);
                PathData = CustomPathData;

                if (writeFile)
                {
                    // save the custom path to PATH_CONF/custom_data_path.txt
                    Utils.LogInfo("Settings: Saving custom data path file: %s", customDataPathFile);

                    try
                    {
                        using StreamWriter outfile = new StreamWriter(customDataPathFile, append: false);
                        outfile.WriteLine(Filesystem.GetFullPath(CustomPathData));
                    }
                    catch (IOException)
                    {
                        Utils.LogError("Settings: Unable to save the custom data path. No write access or disk is full!");
                    }

                    Platform.Instance.FsCommit();
                }
            }
            else
            {
                Utils.LogError("Invalid custom data path: \"%s\"", CustomPathData);
                CustomPathData = "";
            }
        }

        public void SetGame()
        {
            try
            {
                using StreamReader infile = new StreamReader(Filesystem.ConvertSlashes(PathData + "mods/game.txt"));
                while (!infile.EndOfStream)
                {
                    string line = Parse.GetLine(infile);

                    if (Parse.SkipLine(line))
                        continue;

                    string trimmedLine = Parse.Trim(line);

                    if (!string.IsNullOrEmpty(trimmedLine))
                    {
                        Game = trimmedLine;
                        PathConf = Filesystem.ConvertSlashes(PathConf + Game + "/", Filesystem.KeepTrailingSlash);
                        Filesystem.CreateDir(PathConf);
                        break;
                    }
                }
            }
            catch (IOException)
            {
                // 对应原始代码中 infile.open 失败后 infile.good() 恒为 false、循环不执行的行为。
            }
        }

        /// <summary>
        /// 对应 C++ 的 <c>Settings::configValueToString</c>。与 Parse.ValueToString 逻辑相似，
        /// 但对未知类型静默返回空字符串（而不是记录错误日志），与原始实现保持一致。
        /// </summary>
        private string ConfigValueToString(Type type, object value)
        {
            if (type == typeof(bool)) return (bool)value ? "1" : "0";
            else if (type == typeof(int)) return ((int)value).ToString(CultureInfo.InvariantCulture);
            else if (type == typeof(uint)) return ((uint)value).ToString(CultureInfo.InvariantCulture);
            else if (type == typeof(short)) return ((short)value).ToString(CultureInfo.InvariantCulture);
            else if (type == typeof(ushort)) return ((ushort)value).ToString(CultureInfo.InvariantCulture);
            else if (type == typeof(char)) return ((char)value).ToString();
            else if (type == typeof(byte)) return ((byte)value).ToString(CultureInfo.InvariantCulture);
            else if (type == typeof(float)) return ((float)value).ToString("G6", CultureInfo.InvariantCulture);
            else if (type == typeof(string)) return (string)value;
            else return "";
        }
    }
}
