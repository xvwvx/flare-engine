// <自动生成> 对应 C++ 源文件：ModManager.h + ModManager.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using System.Diagnostics;
using System.IO;

namespace FlareEngine
{
    /// <summary>
    /// Mod
    ///
    /// 对应 C++ 的 <c>class Mod</c>：描述单个 mod 的元数据与依赖关系。
    /// C++ 原始实现通过 <c>Version*</c> 裸指针持有版本对象并在析构/赋值时手动 delete/new；
    /// C# 版本改为直接持有 <see cref="Version"/> 引用（托管对象），在拷贝构造函数中逐字段克隆，
    /// 以保留原始"按值深拷贝"语义。
    /// </summary>
    public class Mod
    {
        public bool IsGameMod;
        public string Name;
        public string Description;
        public Dictionary<string, string> DescriptionLocale;
        public string Game;
        public Version Version;
        public Version EngineMinVersion;
        public Version EngineMaxVersion;
        public List<string> Depends;
        public List<Version> DependsMin;
        public List<Version> DependsMax;

        public Mod()
        {
            IsGameMod = false;
            Name = "";
            Description = "";
            DescriptionLocale = new Dictionary<string, string>();
            Game = "";
            Version = new Version();
            EngineMinVersion = new Version(VersionInfo.Min.X, VersionInfo.Min.Y, VersionInfo.Min.Z);
            EngineMaxVersion = new Version(VersionInfo.Max.X, VersionInfo.Max.Y, VersionInfo.Max.Z);
            Depends = new List<string>();
            DependsMin = new List<Version>();
            DependsMax = new List<Version>();
        }

        /// <summary>对应 C++ 拷贝构造函数 <c>Mod::Mod(const Mod&amp; mod)</c>。</summary>
        public Mod(Mod mod)
        {
            IsGameMod = mod.IsGameMod;
            Name = mod.Name;
            Description = mod.Description;
            DescriptionLocale = new Dictionary<string, string>(mod.DescriptionLocale);
            Game = mod.Game;
            Version = new Version(mod.Version.X, mod.Version.Y, mod.Version.Z);
            EngineMinVersion = new Version(mod.EngineMinVersion.X, mod.EngineMinVersion.Y, mod.EngineMinVersion.Z);
            EngineMaxVersion = new Version(mod.EngineMaxVersion.X, mod.EngineMaxVersion.Y, mod.EngineMaxVersion.Z);
            Depends = new List<string>(mod.Depends);
            DependsMin = new List<Version>();
            DependsMax = new List<Version>();
            for (int i = 0; i < mod.DependsMin.Count; ++i)
            {
                DependsMin.Add(new Version(mod.DependsMin[i].X, mod.DependsMin[i].Y, mod.DependsMin[i].Z));
            }
            for (int i = 0; i < mod.DependsMax.Count; ++i)
            {
                DependsMax.Add(new Version(mod.DependsMax[i].X, mod.DependsMax[i].Y, mod.DependsMax[i].Z));
            }

            Debug.Assert(Depends.Count == DependsMin.Count);
            Debug.Assert(Depends.Count == DependsMax.Count);
        }

        // 对应 C++ 析构函数 Mod::~Mod()：原始实现 delete Version* 与 depends_min/max 指针；
        // C# 中 Version 为托管引用类型，由 GC 回收，无需实现 IDisposable 或终结器。

        /// <summary>对应 C++ <c>bool Mod::operator==(const Mod&amp; mod) const</c>。</summary>
        public static bool operator ==(Mod? a, Mod? b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a is null || b is null)
                return false;
            return a.Name == b.Name;
        }

        /// <summary>对应 C++ <c>bool Mod::operator!=(const Mod&amp; mod) const</c>。</summary>
        public static bool operator !=(Mod? a, Mod? b)
        {
            return !(a == b);
        }

        /// <summary>
        /// C# 语言要求：重载 <c>==</c>/<c>!=</c> 时应同时重写 <c>Equals</c>，
        /// 以便 <c>List.Contains</c> 等行为与 <c>std::find</c> + <c>operator==</c> 一致。
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is Mod other && this == other;
        }

        /// <summary>
        /// C# 语言要求：重载 <c>==</c>/<c>!=</c> 时应同时重写 <c>GetHashCode</c>。
        /// </summary>
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        /// <summary>对应 C++ <c>std::string Mod::getLocaleDescription(const std::string&amp; lang)</c>。</summary>
        public string GetLocaleDescription(string lang)
        {
            if (DescriptionLocale.TryGetValue(lang, out string? localized))
                return localized;
            else
                return Description;
        }
    }

    /// <summary>
    /// ModManager
    ///
    /// 维护活动 mod 列表，并在加载数据文件时按优先级顺序检索 mod。
    /// C++ 原始类析构函数仅输出清理日志；为保留同样的生命周期语义并实现 main.cs 中的
    /// <c>SharedResources.Mods?.Dispose()</c> 调用，此处实现 <see cref="IDisposable"/>。
    /// </summary>
    public class ModManager : IDisposable
    {
        public const bool ListFullPaths = true;

        public const string FallbackMod = "default";
        public const string FallbackGame = "default";

        private readonly Dictionary<string, string> _locCache = new Dictionary<string, string>();
        private readonly List<string> _modPaths = new List<string>();

        private readonly List<string>? _cmdLineMods;

        public List<string> ModDirs = new List<string>();
        public List<Mod> ModList = new List<Mod>();

        public ModManager(List<string>? cmdLineMods)
        {
            _cmdLineMods = cmdLineMods;

            _locCache.Clear();
            ModDirs.Clear();
            ModList.Clear();
            SetPaths();

            List<string> modDirsOther = new List<string>();
            Filesystem.GetDirList(SharedResources.Settings!.PathData + "mods", modDirsOther);
            Filesystem.GetDirList(SharedResources.Settings!.PathUser + "mods", modDirsOther);

            for (int i = 0; i < modDirsOther.Count; ++i)
            {
                if (!ModDirs.Contains(modDirsOther[i]))
                    ModDirs.Add(modDirsOther[i]);
            }

            LoadModList();
            ApplyDepends();

            string activeModsStr = "Active mods: ";
            for (int i = 0; i < ModList.Count; ++i)
            {
                activeModsStr += ModList[i].Name;
                if (ModList[i].Version != VersionInfo.Min)
                    activeModsStr += " (" + ModList[i].Version.GetString() + ")";
                if (i < ModList.Count - 1)
                    activeModsStr += ", ";
            }
            Utils.LogInfo(activeModsStr);
        }

        /// <summary>对应 C++ 析构函数 <c>ModManager::~ModManager()</c>。</summary>
        public void Dispose()
        {
            Utils.LogInfo("Cleaning up: ModManager");
        }

        /// <summary>
        /// mod 列表位于以下位置之一：
        /// 1. [PATH_CONF]/mods.txt
        /// 2. [PATH_DATA]/mods/mods.txt
        /// mods.txt 文件展示 mod 的优先级/加载顺序；每行一个 mod 文件夹名，靠后的 mod 覆盖靠前的 mod。
        /// </summary>
        private void LoadModList()
        {
            bool foundAnyMod = false;
            bool loadedDefaults = false;

            // 默认添加 fallback mod
            // 注意：若在 mod_dirs 中找不到 default mod，游戏将退出
            if (ModDirs.Contains(FallbackMod))
            {
                ModList.Add(LoadMod(FallbackMod));
                foundAnyMod = true;
            }

            // 添加所有其他 mod
            if (_cmdLineMods == null || _cmdLineMods.Count == 0)
            {
                StreamReader? infile = null;
                string line;

                string place1 = Filesystem.ConvertSlashes(SharedResources.Settings!.PathConf + "mods.txt");
                string place2 = Filesystem.ConvertSlashes(SharedResources.Settings!.PathData + "mods/mods.txt");

                try
                {
                    infile = new StreamReader(place1);
                }
                catch (IOException)
                {
                    infile?.Dispose();
                    infile = null;
                }

                if (infile == null)
                {
                    try
                    {
                        infile = new StreamReader(place2);
                    }
                    catch (IOException)
                    {
                        infile?.Dispose();
                        infile = null;
                    }
                }

                if (infile == null)
                {
                    Utils.LogError("ModManager: Error during loadModList() -- couldn't open mods.txt, to be located at:");
                    Utils.LogError("%s\n%s\n", place1, place2);
                }
                else
                {
                    loadedDefaults = true;
                }

                if (infile != null)
                {
                    while (!infile.EndOfStream)
                    {
                        line = Parse.GetLine(infile);

                        if (Parse.SkipLine(line))
                            continue;

                        // 若 mod 存在于 mods 文件夹中则添加
                        if (line != FallbackMod)
                        {
                            if (ModDirs.Contains(line))
                            {
                                ModList.Add(LoadMod(line));
                                foundAnyMod = true;
                            }
                            else
                            {
                                Utils.LogError("ModManager: Mod \"%s\" not found, skipping", line);
                            }
                        }
                    }
                    infile.Dispose();
                }
            }
            else
            {
                for (int i = 0; i < _cmdLineMods.Count; ++i)
                {
                    string line = _cmdLineMods[i];

                    // 若 mod 存在于 mods 文件夹中则添加
                    if (line != FallbackMod)
                    {
                        if (ModDirs.Contains(line))
                        {
                            ModList.Add(LoadMod(line));
                            foundAnyMod = true;
                        }
                        else
                        {
                            Utils.LogError("ModManager: Mod \"%s\" not found, skipping", line);
                        }
                    }
                }
            }

            if (!foundAnyMod && ModList.Count == 1)
            {
                Utils.LogError("ModManager: Couldn't locate any Flare mod. Check if the game data are installed " +
                    "correctly. Expected to find the data in the $XDG_DATA_DIRS path, in " +
                    "/usr/local/share/flare/mods, or in the same folder as the executable. " +
                    "Try placing the mods folder in one of these locations.");
            }

            if (loadedDefaults)
            {
                SaveMods();
            }
        }

        /// <summary>
        /// 查找该数据文件所在的位置（mod 文件名）。
        /// 使用私有 _locCache 以避免过多磁盘 I/O。
        /// </summary>
        public string Locate(string filename)
        {
            string convertedFilename = Filesystem.ConvertSlashes(filename);

            // 若已缓存该位置，直接返回
            if (_locCache.TryGetValue(convertedFilename, out string? cached))
            {
                return cached;
            }

            // 按 mod 顺序搜索该文件名的第一个实例
            string testPath;

            for (int i = ModList.Count; i > 0; i--)
            {
                for (int j = 0; j < _modPaths.Count; j++)
                {
                    testPath = Filesystem.ConvertSlashes(_modPaths[j] + "mods/" + ModList[i - 1].Name + "/" + convertedFilename);
                    if (Filesystem.FileExists(testPath))
                    {
                        _locCache[convertedFilename] = testPath;
                        return testPath;
                    }
                }
            }

            // 全部失败时，若文件存在则直接返回文件名
            testPath = Filesystem.ConvertSlashes(SharedResources.Settings!.PathData + convertedFilename);
            if (!Filesystem.FileExists(testPath))
                testPath = "";

            return testPath;
        }

        /// <summary>
        /// 对应 C++ 文件级全局函数 <c>amendPathToVector</c>。
        /// </summary>
        private static void AmendPathToVector(string path, List<string> vec)
        {
            if (Filesystem.PathExists(path))
            {
                if (Filesystem.IsDirectory(path))
                {
                    Filesystem.GetFileList(path, "txt", vec);
                }
                else
                {
                    vec.Add(path);
                }
            }
        }

        /// <summary>
        /// 返回在所有 mod 中找到的、与给定通用文件名匹配的文件名列表。
        /// 列表顺序与 locate() 的搜索顺序相同，靠后的文件应覆盖靠前的文件。
        /// 将 fullPaths 设为 false 时，列表填充相对文件名，可稍后传给 locate()。
        /// </summary>
        public List<string> List(string path, bool fullPaths)
        {
            List<string> ret = new List<string>();
            string testPath;

            for (int i = 0; i < ModList.Count; ++i)
            {
                for (int j = _modPaths.Count; j > 0; j--)
                {
                    testPath = Filesystem.ConvertSlashes(_modPaths[j - 1] + "mods/" + ModList[i].Name + "/" + path);
                    AmendPathToVector(testPath, ret);
                }
            }

            // 若无路径则无需检查重复
            if (ret.Count == 0)
                return ret;

            if (!fullPaths)
            {
                string convertedPath = Filesystem.ConvertSlashes(path);

                // 将每个文件路径缩减为相对于 mods/ 的路径
                for (int i = 0; i < ret.Count; ++i)
                {
                    int start = ret[i].LastIndexOf(convertedPath, StringComparison.Ordinal);
                    if (start <= ret[i].Length)
                        ret[i] = ret[i].Substring(start, ret[i].Length - start);
                }

                // 移除重复项
                for (int i = 0; i < ret.Count; ++i)
                {
                    for (int j = 0; j < i; ++j)
                    {
                        if (ret[i] == ret[j])
                        {
                            ret.RemoveAt(j);
                            break;
                        }
                    }
                }
            }

            return ret;
        }

        private void SetPaths()
        {
            // 若目录相同则设置标志
            bool uniqPathData = SharedResources.Settings!.PathUser != SharedResources.Settings!.PathData;

            if (SharedResources.Settings!.CustomPathData != "")
            {
                // 使用自定义数据路径时，赋予其最高优先级
                // 实际上完全不使用 PATH_DATA，因为设置 CUSTOM_PATH_DATA 时两者相等
                _modPaths.Add(SharedResources.Settings!.CustomPathData);
                uniqPathData = false;
            }
            _modPaths.Add(SharedResources.Settings!.PathUser);
            if (uniqPathData)
                _modPaths.Add(SharedResources.Settings!.PathData);
        }

        public Mod LoadMod(string name)
        {
            Mod mod = new Mod();
            string line;
            string key;
            string val;

            mod.Name = name;
            bool settingsLoaded = false;
            bool gameplayLoaded = false;

            // @CLASS ModManager|Description of mod settings.txt
            for (int i = 0; i < _modPaths.Count; ++i)
            {
                string path = Filesystem.ConvertSlashes(_modPaths[i] + "mods/" + name + "/settings.txt");
                StreamReader? infile = null;
                try
                {
                    infile = new StreamReader(path);
                    settingsLoaded = true;
                }
                catch (IOException)
                {
                    infile?.Dispose();
                    infile = null;
                }

                if (infile != null)
                {
                    while (!infile.EndOfStream)
                    {
                        line = Parse.GetLine(infile);

                        if (Parse.SkipLine(line))
                            continue;

                        key = "";
                        val = "";

                        Parse.GetKeyPair(line, out key, out val);

                        if (key == "description")
                        {
                            // @ATTR description|string|Some text describing the mod.
                            mod.Description = val;
                        }
                        else if (key == "description_locale")
                        {
                            // @ATTR description_locale|string, string : Language, Translated description|A translated description for a language (specified by 2-letter code).
                            string localeStr = Parse.PopFirstString(ref val);
                            if (localeStr != "")
                                mod.DescriptionLocale[localeStr] = Parse.PopFirstString(ref val);
                        }
                        else if (key == "version")
                        {
                            // @ATTR version|version|The version number of this mod.
                            mod.Version.SetFromString(val);
                        }
                        else if (key == "requires")
                        {
                            // @ATTR requires|list(string)|A comma-separated list of the mods that are required in order to use this mod. The dependency version requirements can also be specified and separated by colons (e.g. fantasycore:0.1:2.0).
                            string dep;
                            val = val + ',';
                            while ((dep = Parse.PopFirstString(ref val)) != "")
                            {
                                string depFull = dep + "::";

                                mod.Depends.Add(Parse.PopFirstString(ref depFull, ':'));

                                Version depMin = new Version();
                                Version depMax = new Version();
                                depMin.SetFromString(Parse.PopFirstString(ref depFull, ':'));
                                depMax.SetFromString(Parse.PopFirstString(ref depFull, ':'));

                                if (depMin != VersionInfo.Min && depMax != VersionInfo.Min && depMin > depMax)
                                    depMax = depMin;

                                // 空 min 版本也是 min 的默认值
                                mod.DependsMin.Add(new Version(depMin.X, depMin.Y, depMin.Z));

                                if (depMax == VersionInfo.Min)
                                    mod.DependsMax.Add(new Version(VersionInfo.Max.X, VersionInfo.Max.Y, VersionInfo.Max.Z));
                                else
                                    mod.DependsMax.Add(new Version(depMax.X, depMax.Y, depMax.Z));

                                Debug.Assert(mod.Depends.Count == mod.DependsMin.Count);
                                Debug.Assert(mod.Depends.Count == mod.DependsMax.Count);
                            }
                        }
                        else if (key == "game")
                        {
                            // @ATTR game|string|The game which this mod belongs to (e.g. flare-game).
                            mod.Game = val;
                        }
                        else if (key == "engine_version_min")
                        {
                            // @ATTR engine_version_min|version|The minimum engine version required to use this mod.
                            mod.EngineMinVersion.SetFromString(val);
                        }
                        else if (key == "engine_version_max")
                        {
                            // @ATTR engine_version_max|version|The maximum engine version required to use this mod.
                            mod.EngineMaxVersion.SetFromString(val);
                        }
                        else
                        {
                            Utils.LogError("ModManager: Mod '%s' contains invalid key: '%s'", name, key);
                        }
                    }
                    infile.Dispose();
                }

                path = Filesystem.ConvertSlashes(_modPaths[i] + "mods/" + name + "/engine/gameplay.txt");
                infile = null;
                try
                {
                    infile = new StreamReader(path);
                    gameplayLoaded = true;
                }
                catch (IOException)
                {
                    infile?.Dispose();
                    infile = null;
                }

                if (infile != null)
                {
                    while (!infile.EndOfStream)
                    {
                        line = Parse.GetLine(infile);

                        if (Parse.SkipLine(line))
                            continue;

                        key = "";
                        val = "";

                        Parse.GetKeyPair(line, out key, out val);

                        if (key == "enable_playgame")
                        {
                            mod.IsGameMod = Parse.ToBool(val);
                        }
                    }
                    infile.Dispose();
                }

                if (settingsLoaded && gameplayLoaded)
                    break;
            }

            // 确保 engine min version <= engine max version
            if (mod.EngineMinVersion != VersionInfo.Min && mod.EngineMinVersion > mod.EngineMaxVersion)
            {
                mod.EngineMaxVersion.X = mod.EngineMinVersion.X;
                mod.EngineMaxVersion.Y = mod.EngineMinVersion.Y;
                mod.EngineMaxVersion.Z = mod.EngineMinVersion.Z;
            }

            return mod;
        }

        public void ApplyDepends()
        {
            List<Mod> newMods = new List<Mod>();
            bool finished = true;
            string game = "";
            if (ModList.Count > 0)
                game = ModList[ModList.Count - 1].Game;

            for (int i = 0; i < ModList.Count; i++)
            {
                // 若 game 不匹配则跳过该 mod
                if (game != FallbackGame && ModList[i].Game != FallbackGame && ModList[i].Game != game && ModList[i].Name != FallbackMod)
                {
                    Utils.LogError("ModManager: Tried to enable \"%s\", but failed. Game does not match \"%s\".", ModList[i].Name, game);
                    continue;
                }

                // 若与当前引擎版本不兼容则跳过
                if (ModList[i].EngineMinVersion > VersionInfo.Engine || VersionInfo.Engine > ModList[i].EngineMaxVersion)
                {
                    Utils.LogError("ModManager: Tried to enable \"%s\", but failed. Not compatible with engine version %s.", ModList[i].Name, VersionInfo.Engine.GetString());
                    continue;
                }

                // 若已在 newMods 列表中则跳过
                if (newMods.Contains(ModList[i]))
                {
                    continue;
                }

                bool dependsMet = true;

                for (int j = 0; j < ModList[i].Depends.Count; j++)
                {
                    bool foundDepend = false;

                    // 尝试将依赖项加入 newMods 列表
                    for (int k = 0; k < newMods.Count; k++)
                    {
                        if (newMods[k].Name == ModList[i].Depends[j])
                        {
                            foundDepend = true;
                        }
                        if (!foundDepend)
                        {
                            // 若尚未拥有该依赖，尝试从可用 mod 列表加载
                            if (ModDirs.Contains(ModList[i].Depends[j]))
                            {
                                Mod newDepend = LoadMod(ModList[i].Depends[j]);
                                if (game != FallbackGame && newDepend.Game != FallbackGame && newDepend.Game != game)
                                {
                                    Utils.LogError("ModManager: Tried to enable dependency \"%s\" for \"%s\", but failed. Game does not match \"%s\".", newDepend.Name, ModList[i].Name, game);
                                    dependsMet = false;
                                    break;
                                }
                                else if (newDepend.EngineMinVersion > VersionInfo.Engine || VersionInfo.Engine > newDepend.EngineMaxVersion)
                                {
                                    Utils.LogError("ModManager: Tried to enable dependency \"%s\" for \"%s\", but failed. Not compatible with engine version %s.", newDepend.Name, ModList[i].Name, VersionInfo.Engine.GetString());
                                    dependsMet = false;
                                    break;
                                }
                                else if (newDepend.Version < ModList[i].DependsMin[j] || newDepend.Version > ModList[i].DependsMax[j])
                                {
                                    Utils.LogError("ModManager: Tried to enable dependency \"%s\" for \"%s\", but failed. Version \"%s\" is required, but only version \"%s\" is available.",
                                            newDepend.Name,
                                            ModList[i].Name,
                                            VersionInfo.CreateVersionReqString(ModList[i].DependsMin[j], ModList[i].DependsMax[j]),
                                            newDepend.Version.GetString()
                                    );
                                    dependsMet = false;
                                    break;
                                }
                                else if (!newMods.Contains(newDepend))
                                {
                                    Utils.LogError("ModManager: Mod \"%s\" requires the \"%s\" mod. Enabling \"%s\" now.", ModList[i].Name, ModList[i].Depends[j], ModList[i].Depends[j]);
                                    newMods.Add(new Mod(newDepend));
                                    finished = false;
                                    break;
                                }
                            }
                            else
                            {
                                Utils.LogError("ModManager: Could not find mod \"%s\", which is required by mod \"%s\". Disabling \"%s\" now.", ModList[i].Depends[j], ModList[i].Name, ModList[i].Name);
                                dependsMet = false;
                                break;
                            }
                        }
                    }
                    if (!dependsMet)
                        break;
                }

                if (dependsMet)
                {
                    if (!newMods.Contains(ModList[i]))
                    {
                        newMods.Add(new Mod(ModList[i]));
                    }
                }
            }

            ModList.Clear();
            for (int i = 0; i < newMods.Count; i++)
            {
                ModList.Add(new Mod(newMods[i]));
            }

            // 递归运行直到不再有未满足的依赖
            if (!finished)
                ApplyDepends();
        }

        public bool HaveFallbackMod()
        {
            for (int i = 0; i < ModList.Count; ++i)
            {
                if (ModList[i].Name == FallbackMod)
                    return true;
            }
            return false;
        }

        public void SaveMods()
        {
            StreamWriter? outfile = null;
            try
            {
                outfile = new StreamWriter(Filesystem.ConvertSlashes(SharedResources.Settings!.PathConf + "mods.txt"), append: false);
            }
            catch (IOException)
            {
                outfile = null;
            }

            if (outfile != null)
            {
                try
                {
                    // comment
                    outfile.Write("## flare-engine mods list file ##\n");

                    outfile.Write("# Mods lower on the list will overwrite data in the entries higher on the list\n");
                    outfile.Write("\n");

                    for (int i = 0; i < ModList.Count; i++)
                    {
                        if (ModList[i].Name != FallbackMod)
                            outfile.Write(ModList[i].Name + "\n");
                    }

                    outfile.Flush();
                }
                catch (IOException)
                {
                    Utils.LogError("GameStateConfig: Unable to save mod list into file. No write access or disk is full!");
                }

                outfile.Dispose();
            }

            Platform.Instance.FsCommit();
        }

        public void ResetModConfig()
        {
            string configPath = SharedResources.Settings!.PathConf + "mods.txt";
            Utils.LogError("ModManager: Game data is either missing or misconfigured. Deleting '%s' in attempt to recover.", configPath);
            Filesystem.RemoveFile(configPath);
        }
    }
}
