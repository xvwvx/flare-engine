// <自动生成> 对应 C++ 源文件：MessageEngine.h + MessageEngine.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。

namespace FlareEngine
{
    /// <summary>
    /// MessageEngine
    ///
    /// 通过将消息与类似 gettext 格式的 .po 文件进行比对，实现 FLARE 中的消息翻译。
    /// 该类主要用于确保 FLARE 具备灵活性与可翻译性。
    /// </summary>
    public class MessageEngine : IDisposable
    {
        private readonly Dictionary<string, string> _messages = new Dictionary<string, string>();

        public MessageEngine()
        {
            Utils.LogInfo("MessageEngine: Using language '%s'", SharedResources.Settings!.Language);

            // check to see if the language setting is available in engine/languages.txt
            using FileParser configFile = new FileParser();
            bool foundLanguage = false;
            string fallbackLanguage = "";

            if (configFile.Open("engine/languages.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (configFile.Next())
                {
                    if (configFile.Key == SharedResources.Settings!.Language)
                    {
                        foundLanguage = true;
                        break;
                    }
                    if (fallbackLanguage == "" || configFile.Key == "en")
                        fallbackLanguage = configFile.Key;
                }
                configFile.Close();
            }

            if (fallbackLanguage == "")
                fallbackLanguage = "en";

            if (!foundLanguage && SharedResources.Settings!.Language != fallbackLanguage)
            {
                Utils.LogError("MessageEngine: Unable to find '%s' in engine/languages.txt. Falling back to '%s'.", SharedResources.Settings!.Language, fallbackLanguage);
                SharedResources.Settings!.Language = fallbackLanguage;
            }


            using GetText infile = new GetText();

            // SharedResources.Mods / ModManager 是尚未转换的未来依赖单元；调用形态
            // （List(string,bool) 与 ListFullPaths 常量）沿用 output/FileParser.cs 中
            // 已建立的前向引用约定，与之保持一致。
            List<string> engineFiles = SharedResources.Mods!.List("languages/engine." + SharedResources.Settings!.Language + ".po", ModManager.ListFullPaths);
            if (engineFiles.Count == 0 && SharedResources.Settings!.Language != "en")
                Utils.LogError("MessageEngine: Unable to open basic translation files located in languages/engine.%s.po", SharedResources.Settings!.Language);

            for (int i = 0; i < engineFiles.Count; ++i)
            {
                if (infile.Open(engineFiles[i]))
                {
                    while (infile.Next())
                    {
                        if (!infile.Fuzzy)
                            _messages.TryAdd(infile.Key, infile.Val);
                    }
                    infile.Close();
                }
            }

            List<string> dataFiles = SharedResources.Mods!.List("languages/data." + SharedResources.Settings!.Language + ".po", ModManager.ListFullPaths);
            if (dataFiles.Count == 0 && SharedResources.Settings!.Language != "en")
                Utils.LogError("MessageEngine: Unable to open basic translation files located in languages/data.%s.po", SharedResources.Settings!.Language);

            for (int i = 0; i < dataFiles.Count; ++i)
            {
                if (infile.Open(dataFiles[i]))
                {
                    while (infile.Next())
                    {
                        if (!infile.Fuzzy)
                            _messages.TryAdd(infile.Key, infile.Val);
                    }
                    infile.Close();
                }
            }
        }

        /// <summary>对应 C++ 析构函数：仅记录一条清理日志，不持有需要释放的非托管资源。</summary>
        public void Dispose()
        {
            Utils.LogInfo("Cleaning up: MessageEngine");
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 该 Get() 函数用于维护那些不期望 C/printf 风格格式化的字符串。
        /// 我们允许 mod 数据中的字符串不必转义 '%'，所以这类字符串不能不经处理就传给 GetV()。
        /// 同时我们也尽可能在引擎字符串上使用本方法，因为它应当比 GetV() 重新构建字符串更高效。
        /// </summary>
        public string Get(string key)
        {
            string message = GetMessageOrInsertDefault(key);
            if (message == "") message = key;
            return Unescape(message);
        }

        // NOTE: key is not passed by reference because doing so would result in undefined behavior when using va_start()
        // C# 没有与 va_start()/va_list 等价的机制，此处沿用仓库既有约定（见 output/Utils.cs 的
        // LogInfo/LogError、output/FileParser.cs 的 Error）用 params object?[] 承载可变参数，
        // 并通过 Utils.FormatPrintf 完成格式化，语义与原始 vsnprintf(buffer, buffer_size, format, args) 等价。
        public string GetV(string key, params object?[] args)
        {
            string message = GetMessageOrInsertDefault(key);
            if (message == "") message = key;

            string format = message;

            return Utils.FormatPrintf(format, args);
        }

        // unescape c formatted string
        private string Unescape(string val0)
        {
            string val = val0;

            // unescape percentage %% to %
            int pos;
            while ((pos = val.IndexOf("%%", StringComparison.Ordinal)) != -1)
                val = val.Substring(0, pos) + "%" + val.Substring(pos + 2);

            return val;
        }

        /// <summary>
        /// 对应 C++ 的 <c>std::map&lt;std::string,std::string&gt;::operator[]</c>：若键不存在，
        /// 会插入一个默认构造（空字符串）的元素并返回其引用——这是一个会修改底层容器的副作用，
        /// 与 <c>_messages.TryGetValue</c>（只读）语义不同，因此单独封装为本方法以精确复现该行为
        /// （语言范式差异导致的必要适配，见报告"关键转换决策"）。
        /// </summary>
        private string GetMessageOrInsertDefault(string key)
        {
            if (!_messages.TryGetValue(key, out string? message))
            {
                message = "";
                _messages[key] = message;
            }
            return message;
        }
    }
}
