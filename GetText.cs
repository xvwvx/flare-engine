// <自动生成> 对应 C++ 源文件：GetText.h + GetText.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using System.IO; // StreamReader；项目 csproj 中 ImplicitUsings 为 disable，需显式引入

namespace FlareEngine
{
    /// <summary>
    /// GetText
    ///
    /// 从 gettext .po 文件格式中抽象出通用的键值对处理逻辑。
    /// 我们（目前）不需要完整的 gettext 功能，因此这是一个简单的实现方案。
    /// C++ 原始类持有 <c>std::ifstream</c> 非托管资源，析构函数中调用 <c>close()</c> 释放；
    /// C# 版本额外实现 <see cref="IDisposable"/>，在 <see cref="Dispose"/> 中调用
    /// <see cref="Close"/>，以便调用方可以使用 <c>using</c> 语句确定性地释放底层文件句柄
    /// （与 output/FileParser.cs 的既有约定一致）。
    /// </summary>
    public class GetText : IDisposable
    {
        private StreamReader? _infile;
        private string _line;

        public string Key;
        public string Val;
        public bool Fuzzy;

        public GetText()
        {
            _infile = null;
            _line = "";
            Key = "";
            Val = "";
            Fuzzy = false;
        }

        /// <summary>
        /// 打开给定文件名用于读取。
        /// </summary>
        /// <returns>打开成功返回 true，否则返回 false。</returns>
        public bool Open(string filename)
        {
            try
            {
                _infile = new StreamReader(filename);
            }
            catch (Exception)
            {
                _infile = null;
            }
            return _infile != null;
        }

        public void Close()
        {
            _infile?.Dispose();
            _infile = null;
        }

        // 将所有 \" 转换为单纯的 "
        // 将所有 \n 转换为换行符
        private string Sanitize(string message)
        {
            string newMessage = message;
            int pos;
            while ((pos = newMessage.IndexOf("\\\"", StringComparison.Ordinal)) != -1)
            {
                newMessage = newMessage.Substring(0, pos) + newMessage.Substring(pos + 1);
            }
            while ((pos = newMessage.IndexOf("\\n", StringComparison.Ordinal)) != -1)
            {
                newMessage = newMessage.Substring(0, pos) + '\n' + newMessage.Substring(pos + 2);
            }
            return newMessage;
        }

        /// <summary>
        /// 前进到下一个键值对。
        /// </summary>
        /// <returns>到达文件末尾返回 false，否则返回 true。</returns>
        public bool Next()
        {
            Key = "";
            Val = "";

            Fuzzy = false;

            while (_infile != null && !_infile.EndOfStream)
            {
                _line = Parse.GetLine(_infile);

                // 检查该行是否为注释，以及该注释是否包含 fuzzy 标记
                if (HasPrefix(_line, "#,") && _line.IndexOf("fuzzy", StringComparison.Ordinal) != -1)
                    Fuzzy = true;

                // 这一行是一个 key
                if (HasPrefix(_line, "msgid"))
                {
                    // 只取引号内包含的内容
                    Key = _line.Substring(6);
                    Key = Substr(Key, 1, Key.Length - 2); // 去掉两端的引号
                    Key = Sanitize(Key);

                    if (Key != "")
                        continue;
                    else
                    {
                        // 这是一个多行值，除非它是第一个 msgid（此时它会是空的，
                        // 在查找匹配的 msgstr 时会被忽略，所以问题不大）。
                        _line = Parse.GetLine(_infile);
                        while (_line.Length > 0 && _line[0] == '\"')
                        {
                            // 去掉两端的双引号。
                            Key += Substr(_line, 1, _line.Length - 2);
                            Key = Sanitize(Key);
                            _line = Parse.GetLine(_infile);
                        }
                    }
                }

                // 这一行是一个 value
                if (HasPrefix(_line, "msgstr"))
                {
                    // 只取引号内包含的内容
                    Val = _line.Substring(7);
                    Val = Substr(Val, 1, Val.Length - 2); // 去掉两端的引号
                    Val = Sanitize(Val);

                    // 处理键值对
                    if (Key != "")
                    {
                        if (Val != "")
                        { // 找到了单行值。
                            return true;
                        }
                        else
                        { // 可能是多行值。
                            _line = Parse.GetLine(_infile);
                            while (_line.Length > 0 && _line[0] == '\"')
                            {
                                // 去掉两端的双引号。
                                Val += Substr(_line, 1, _line.Length - 2);
                                Val = Sanitize(Val);
                                _line = Parse.GetLine(_infile);
                            }
                            if (Val != "")
                            { // 确实是多行值。
                                return true;
                            }
                        }
                    }
                    else
                    {
                        // key 为空；这一行大概率是 po 文件头部
                        // 为下一个 msgid 重置 fuzzy 状态
                        Fuzzy = false;
                    }
                }
            }

            // 到达文件末尾
            return false;
        }

        /// <summary>
        /// 对应 C++ 的 <c>line.compare(0, prefix.length(), prefix) == 0</c>：
        /// 仅当 <paramref name="s"/> 长度足够，且其前 <c>prefix.Length</c> 个字符与
        /// <paramref name="prefix"/> 逐字节一致时才返回 true。
        /// </summary>
        private static bool HasPrefix(string s, string prefix)
        {
            return s.Length >= prefix.Length && string.CompareOrdinal(s, 0, prefix, 0, prefix.Length) == 0;
        }

        /// <summary>
        /// 对应 C++ 的 <c>std::string::substr(pos, count)</c>。
        /// 原始调用点均以 <c>length()-2</c>（<c>size_t</c>，无符号）作为 count 实参：当字符串长度
        /// 小于 2 时会发生无符号下溢，wraps 成一个极大的数，此时 C++ 标准要求把 count 裁剪到
        /// "字符串剩余长度"（即取到末尾），而不会因此抛出异常（只要 pos &lt;= size()）。
        /// <see cref="string.Substring(int,int)"/> 对负数 length 直接抛异常，语义不同，
        /// 因此这里补充该辅助方法以精确复现 C++ 的下溢裁剪行为（语言差异导致的必要适配，
        /// 不改变任何分支/循环逻辑）。
        /// </summary>
        private static string Substr(string s, int pos, int count)
        {
            if (pos > s.Length)
                throw new ArgumentOutOfRangeException(nameof(pos));
            int available = s.Length - pos;
            int len = count < 0 ? available : Math.Min(count, available);
            return s.Substring(pos, len);
        }

        public void Dispose()
        {
            Close();
            GC.SuppressFinalize(this);
        }
    }
}
