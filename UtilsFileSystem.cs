// <自动生成> 对应 C++ 源文件：UtilsFileSystem.h + UtilsFileSystem.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。

namespace FlareEngine
{
    /// <summary>
    /// UtilsFileSystem
    ///
    /// 各种文件系统函数包装器（对应 C++ 的 <c>namespace Filesystem</c>）。
    /// 原始实现基于 POSIX 的 stat/dirent/opendir 等 API 来"隐藏操作系统相关实现"；
    /// .NET 的 System.IO（File/Directory/Path）本身已经是跨平台抽象，因此直接使用纯 BCL API
    /// 重新实现同样的对外行为契约，不使用任何 P/Invoke 或平台宏。
    /// </summary>
    public static class Filesystem
    {
        public const bool KeepTrailingSlash = true;

        /// <summary>
        /// 检查目录/文件夹是否存在。
        /// </summary>
        public static bool PathExists(string path)
        {
            string p = ConvertSlashes(path);
            return File.Exists(p) || Directory.Exists(p);
        }

        /// <summary>
        /// 如果该文件夹不存在，则创建它。
        /// </summary>
        public static void CreateDir(string path)
        {
            if (IsDirectory(path, false))
                return;

            Platform.Instance.DirCreate(ConvertSlashes(path));
        }

        /// <summary>
        /// 检查文件是否存在。filename 参数应包含该文件的完整路径。
        /// </summary>
        public static bool FileExists(string filename)
        {
            if (IsDirectory(filename, false))
                return false;

            return File.Exists(ConvertSlashes(filename));
        }

        /// <summary>
        /// 返回给定文件夹中所有具有给定扩展名的文件名（结果通过 files 列表追加返回）。
        /// 保留原始"字符串末尾字面匹配"的匹配方式（不要求匹配前存在'.'分隔符）。
        /// </summary>
        public static int GetFileList(string dir, string ext, List<string> files)
        {
            string convertedDir = ConvertSlashes(dir);
            IEnumerable<string> entries;
            try
            {
                entries = Directory.EnumerateFileSystemEntries(convertedDir);
            }
            catch (Exception)
            {
                // 对应原始代码中 opendir() 返回 NULL 的情况（目录不存在或无权限）。
                return -1;
            }

            int extlen = ext.Length;
            foreach (string entryPath in entries)
            {
                string filename = Path.GetFileName(entryPath);
                if (filename.Length > extlen)
                {
                    if (filename.Substring(filename.Length - extlen, extlen) == ext)
                        files.Add(ConvertSlashes(dir + "/" + filename));
                }
            }
            return 0;
        }

        /// <summary>
        /// 返回给定目录中所有子目录名（结果通过 dirs 列表追加返回）。
        /// </summary>
        public static int GetDirList(string dir, List<string> dirs)
        {
            IEnumerable<string> entries;
            try
            {
                entries = Directory.EnumerateFileSystemEntries(dir);
            }
            catch (Exception)
            {
                return -1;
            }

            foreach (string entryPath in entries)
            {
                // do not use dirp->d_type, it's not portable
                string directory = Path.GetFileName(entryPath);
                string modDir = ConvertSlashes(dir + "/" + directory);
                if (Directory.Exists(modDir) && directory != "." && directory != "..")
                {
                    dirs.Add(directory);
                }
            }
            return 0;
        }

        public static bool IsDirectory(string path, bool showError = true)
        {
            string cleanPath = ConvertSlashes(path);
            if (!File.Exists(cleanPath) && !Directory.Exists(cleanPath))
            {
                if (showError)
                {
                    Utils.LogError("Filesystem::isDirectory (%s): No such file or directory", cleanPath);
                }
                return false;
            }
            else
            {
                return Directory.Exists(cleanPath);
            }
        }

        public static bool RemoveFile(string file)
        {
            string cleanPath = ConvertSlashes(file);
            try
            {
                File.Delete(cleanPath);
                return true;
            }
            catch (Exception)
            {
                Utils.LogError("Filesystem::removeFile (%s)", cleanPath);
                return false;
            }
        }

        public static bool RemoveDir(string dir)
        {
            if (!IsDirectory(dir))
                return false;

            return Platform.Instance.DirRemove(ConvertSlashes(dir));
        }

        public static bool RemoveDirRecursive(string dir)
        {
            List<string> dirList = new List<string>();
            List<string> fileList = new List<string>();

            GetDirList(dir, dirList);
            while (dirList.Count > 0)
            {
                RemoveDirRecursive(dir + "/" + dirList[^1]);
                dirList.RemoveAt(dirList.Count - 1);
            }

            GetFileList(dir, "txt", fileList);
            while (fileList.Count > 0)
            {
                RemoveFile(fileList[^1]);
                fileList.RemoveAt(fileList.Count - 1);
            }

            RemoveDir(dir);

            return true;
        }

        /// <summary>
        /// 以与操作系统无关的方式将字符串转换为文件系统路径字符串。
        /// C++ 原始实现通过编译期宏 _WIN32 选择分隔符；此处使用纯 BCL 的运行时平台判断
        /// OperatingSystem.IsWindows() 达到相同效果（规则 6：禁止平台宏，但允许运行时的
        /// 跨平台判断 API）。
        /// </summary>
        public static string ConvertSlashes(string path0, bool keepTrailingSlash = false)
        {
            char[] path = path0.ToCharArray();

            char goodSep;
            char badSep;
            if (OperatingSystem.IsWindows())
            {
                goodSep = '\\';
                badSep = '/';
            }
            else
            {
                goodSep = '/';
                badSep = '\\';
            }

            for (int i = 0; i < path.Length; i++)
            {
                if (path[i] == badSep)
                {
                    path[i] = goodSep;
                }
            }

            string result = new string(path);

            if (!keepTrailingSlash)
                return RemoveTrailingSlash(result);
            else
                return result;
        }

        public static bool RenameFile(string oldfile0, string newfile0)
        {
            string oldfile = ConvertSlashes(oldfile0);
            string newfile = ConvertSlashes(newfile0);

            try
            {
                File.Move(oldfile, newfile, overwrite: true);
                return true;
            }
            catch (Exception)
            {
                Utils.LogError("Filesystem::renameFile (%s -> %s)", oldfile, newfile);
                return false;
            }
        }

        public static string RemoveTrailingSlash(string path)
        {
            // windows
            if (path.Length > 0 && path[^1] == '\\')
                return path.Substring(0, path.Length - 1);

            // everything else
            // allow for *nix root /
            else if (path.Length > 1 && path[^1] == '/')
                return path.Substring(0, path.Length - 1);

            // no trailing slash found
            else
                return path;
        }

        /// <summary>
        /// 对应 C++ 中按平台分别使用 _fullpath (Windows) / realpath (POSIX) 的实现。
        /// .NET 的 Path.GetFullPath 已是跨平台实现，直接使用即可，不需要区分平台分支。
        /// </summary>
        public static string GetFullPath(string path)
        {
            return Path.GetFullPath(path);
        }
    }
}
