// <自动生成> 对应 C++ 源文件：FileParser.h + FileParser.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。

namespace FlareEngine
{
    /// <summary>
    /// FileParser
    ///
    /// 抽象通用的 key=value ini 风格文件格式。
    /// C++ 原始类持有 std::ifstream 等非托管资源，析构函数中调用 close() 释放；
    /// C# 版本额外实现 IDisposable，在 Dispose() 中调用 Close()，
    /// 以便调用方可以使用 using 语句确定性地释放底层文件句柄（规则：显式管理需要释放的资源）。
    /// </summary>
    public class FileParser : IDisposable
    {
        public const int ErrorNone = 0;
        public const int ErrorNormal = 1;
        public const bool ModFile = true;

        private List<string> _filenames = new List<string>();
        private uint _currentIndex;
        private bool _isModFile;
        private int _errorMode;
        private string _requestedFilename = "";

        private StreamReader? _infile;
        private string _line = "";

        private uint _lineNumber;

        private FileParser? _includeFp;

        public bool NewSection;
        public string Section = "";
        public string Key = "";
        public string Val = "";

        public FileParser()
        {
            _currentIndex = 0;
            _isModFile = false;
            _errorMode = ErrorNormal;
            _requestedFilename = "";
            _line = "";
            _lineNumber = 0;
            _includeFp = null;
            NewSection = false;
            Section = "";
            Key = "";
            Val = "";
        }

        /// <summary>
        /// 打开一个通用文件名（该通用文件名会由 ModManager 定位）。如果这是一个目录，
        /// 该目录下的所有文件都会被打开。成功打开文件用于读取时返回 true。
        /// </summary>
        public bool Open(string filename, bool isModFile, int errorMode)
        {
            _isModFile = isModFile;
            _errorMode = errorMode;
            _requestedFilename = filename;

            _filenames.Clear();
            if (_isModFile)
            {
                // SharedResources.Mods / ModManager 是尚未转换的未来依赖单元。
                _filenames = SharedResources.Mods!.List(Filesystem.ConvertSlashes(filename), ModManager.ListFullPaths);
            }
            else
            {
                _filenames.Add(Filesystem.ConvertSlashes(filename));
            }
            _currentIndex = 0;
            _lineNumber = 0;

            if (_filenames.Count == 0)
            {
                if (_errorMode != ErrorNone)
                    Utils.LogError("FileParser: Could not open text file: %s: No such file or directory!", filename);
                return false;
            }

            bool ret = false;

            // Cycle through all filenames from the end, stopping when a file is to overwrite all further files.
            for (int i = _filenames.Count; i > 0; i--)
            {
                CloseCurrentInfile();

                try
                {
                    _infile = new StreamReader(_filenames[i - 1]);
                    ret = true;
                }
                catch (Exception)
                {
                    _infile = null;
                    ret = false;
                }

                if (ret)
                {
                    // This will be the first file to be parsed. Seek to the start of the file and leave it open.
                    if (!_infile!.EndOfStream && Parse.Trim(Parse.GetLine(_infile)) != "APPEND")
                    {
                        string testLine = "";

                        // get the first non-comment, non blank line
                        while (!_infile.EndOfStream)
                        {
                            testLine = Parse.Trim(Parse.GetLine(_infile));
                            if (Parse.SkipLine(testLine))
                                continue;
                            else
                                break;
                        }

                        if (testLine != "APPEND")
                        {
                            _currentIndex = (uint)(i - 1);
                            _infile.BaseStream.Seek(0, SeekOrigin.Begin); // reset flags + rewind
                            _infile.DiscardBufferedData();
                            break;
                        }
                    }

                    // don't close the final file if it's the only one with an "APPEND" line
                    if (i > 1)
                    {
                        CloseCurrentInfile();
                    }
                }
                else
                {
                    if (_errorMode != ErrorNone)
                        Utils.LogError("FileParser: Could not open text file: %s", _filenames[i - 1]);
                }
            }

            return ret;
        }

        public void Close()
        {
            if (_includeFp != null)
            {
                _includeFp.Close();
                _includeFp.Dispose();
                _includeFp = null;
            }

            CloseCurrentInfile();
        }

        /// <summary>
        /// 移动到下一个 key-value 对，并记录是否遇到了新的 section 头。
        /// </summary>
        /// <returns>到达 EOF 返回 false，否则返回 true。</returns>
        public bool Next()
        {
            string startsWith;
            NewSection = false;

            while (_currentIndex < _filenames.Count)
            {
                while (_includeFp != null || _infile is { EndOfStream: false })
                {
                    if (_includeFp != null)
                    {
                        if (_includeFp.Next())
                        {
                            NewSection = _includeFp.NewSection;
                            Section = _includeFp.Section;
                            Key = _includeFp.Key;
                            Val = _includeFp.Val;
                            return true;
                        }
                        else
                        {
                            _includeFp.Close();
                            _includeFp.Dispose();
                            _includeFp = null;
                            continue;
                        }
                    }

                    _line = Parse.Trim(Parse.GetLine(_infile));
                    _lineNumber++;

                    if (Parse.SkipLine(_line))
                        continue;

                    startsWith = _line[0].ToString();

                    // set new section if this line is a section declaration
                    if (startsWith == "[")
                    {
                        NewSection = true;
                        Section = Parse.GetSectionTitle(_line);

                        // keep searching for a key-pair
                        continue;
                    }

                    // skip the string used to combine files
                    if (_line == "APPEND") continue;

                    // read from a separate file
                    int firstSpace = _line.IndexOf(' ');

                    if (firstSpace != -1)
                    {
                        string directive = _line.Substring(0, firstSpace);

                        if (directive == "INCLUDE")
                        {
                            string tmp = _line.Substring(firstSpace + 1);

                            if (_requestedFilename != tmp)
                            {
                                _includeFp = new FileParser();
                                if (!_includeFp.Open(tmp, _isModFile, _errorMode))
                                {
                                    _includeFp.Dispose();
                                    _includeFp = null;
                                }

                                if (_includeFp != null)
                                {
                                    // INCLUDE file will inherit the current section
                                    _includeFp.Section = Section;
                                }
                            }
                            else
                            {
                                Error("FileParser: Recursive INCLUDE detected. Did you mean to use APPEND?");
                            }

                            continue;
                        }
                    }

                    // this is a keypair. Perform basic parsing and return
                    Parse.GetKeyPair(_line, out Key, out Val);
                    return true;
                }

                CloseCurrentInfile();

                _currentIndex++;
                if (_currentIndex == _filenames.Count) return false;

                _lineNumber = 0;
                string currentFilename = _filenames[(int)_currentIndex];
                try
                {
                    _infile = new StreamReader(currentFilename);
                }
                catch (Exception)
                {
                    _infile = null;
                }

                if (_infile == null)
                {
                    if (_errorMode != ErrorNone)
                        Utils.LogError("FileParser: Could not open text file: %s", currentFilename);
                    return false;
                }
                // a new file starts a new section
                NewSection = true;
            }

            // hit the end of file
            return false;
        }

        /// <summary>
        /// 从输入文件中获取一行未经解析、未经过滤的原始文本。
        /// </summary>
        public string GetRawLine()
        {
            _line = "";

            if (_infile != null && !_infile.EndOfStream)
            {
                _line = Parse.GetLine(_infile);
            }
            return _line;
        }

        /// <summary>
        /// 对应 C++ 的 C 风格可变参数 <c>error(const char* format, ...)</c>，
        /// 改用 params object?[] 承载可变参数（规则 4）。
        /// </summary>
        public void Error(string format, params object?[] args)
        {
            string buffer = Utils.FormatPrintf(format, args);
            ErrorBuf(buffer);
        }

        private void ErrorBuf(string buffer)
        {
            if (_includeFp != null)
            {
                _includeFp.ErrorBuf(buffer);
            }
            else
            {
                // 注意：与原始代码一致，直接把拼接好的字符串当作格式串传给 LogError，
                // 而不是作为普通参数传入；如果 buffer 中含有 '%' 字符，行为与原始代码一样
                // 会被 FormatPrintf 当成格式说明符处理（原样保留这一潜在行为）。
                string ss = "[" + _filenames[(int)_currentIndex] + ":" + _lineNumber + "] " + buffer;
                Utils.LogError(ss);
            }
        }

        public void IncrementLineNum()
        {
            _lineNumber++;
        }

        private void CloseCurrentInfile()
        {
            _infile?.Dispose();
            _infile = null;
        }

        public void Dispose()
        {
            Close();
            GC.SuppressFinalize(this);
        }
    }
}
