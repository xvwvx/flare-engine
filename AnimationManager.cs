// 对应 C++ 源文件：AnimationManager.h + AnimationManager.cpp
using System.Diagnostics;

namespace FlareEngine
{
    /// <summary>
    /// AnimationManager
    ///
    /// 管理所有已加载的 <see cref="AnimationSet"/> 实例，按文件名做引用计数缓存，
    /// 避免同一份动画定义被重复加载/解析。持有者需在不再需要时调用 <see cref="Dispose"/>，
    /// 对应原始 C++ 版本中 <c>delete</c> 该对象时触发析构函数的时机。
    /// </summary>
    public class AnimationManager : IDisposable
    {
        private readonly List<AnimationSet?> _sets = new List<AnimationSet?>();
        private readonly List<string> _names = new List<string>();
        private readonly List<int> _counts = new List<int>();

        public AnimationManager()
        {
        }

        /// <summary>
        /// 对应 C++ 析构函数：先执行 <see cref="CleanUp"/> 释放所有零引用计数的动画集，
        /// 再在调试模式下（对应原始 <c>#ifndef NDEBUG</c>）检查是否还有残留的动画集未释放。
        /// </summary>
        public void Dispose()
        {
            Utils.LogInfo("Cleaning up: AnimationManager");

            CleanUp();
// NDEBUG is used by posix to disable assertions, so use the same MACRO.
#if DEBUG
            if (_names.Count != 0)
            {
                Utils.LogError("AnimationManager: Still holding these animations:");
                for (int i = 0; i < _names.Count; i++)
                {
                    Utils.LogError("%s %d", _names[i], _counts[i]);
                }
            }
            Debug.Assert(_names.Count == 0);
#endif
        }

        /// <param name="filename">要加载的文件名，起始路径位于 animations 文件夹下。</param>
        public AnimationSet? GetAnimationSet(string filename)
        {
            int found = _names.IndexOf(filename);
            if (found != -1)
            {
                if (_sets[found] == null)
                {
                    _sets[found] = new AnimationSet(filename);
                }
                return _sets[found];
            }
            else
            {
                Utils.LogError("AnimationManager::getAnimationSet(): %s not found", filename);
                Utils.LogErrorDialog("AnimationManager::getAnimationSet(): %s not found", filename);
                SharedResources.Mods!.ResetModConfig();
                Utils.Exit(1);
                return null;
            }
        }

        public void DecreaseCount(string name)
        {
            int found = _names.IndexOf(name);
            if (found != -1)
            {
                _counts[found]--;
            }
            else
            {
                Utils.LogError("AnimationManager::decreaseCount(): %s not found", name);
                Utils.LogErrorDialog("AnimationManager::decreaseCount(): %s not found", name);
                Utils.Exit(1);
            }
        }

        public void IncreaseCount(string name)
        {
            int found = _names.IndexOf(name);
            if (found != -1)
            {
                _counts[found]++;
            }
            else
            {
                _sets.Add(null);
                _names.Add(name);
                _counts.Add(1);
            }
        }

        public void CleanUp()
        {
            int i = _sets.Count - 1;
            while (i >= 0)
            {
                if (_counts[i] <= 0)
                {
                    _sets[i]?.Dispose();
                    _counts.RemoveAt(i);
                    _sets.RemoveAt(i);
                    _names.RemoveAt(i);
                }
                --i;
            }
        }

        public void CheckAnimationsInit()
        {
            for (int i = 0; i < _sets.Count; ++i)
            {
                for (int j = 0; j < _sets[i]!.Animations.Count; ++j)
                {
                    _sets[i]!.Animations[j].CheckInit();
                }
            }
        }
    }
}
