// <自动生成> 对应 C++ 源文件：src/AnimationMedia.h + src/AnimationMedia.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。

namespace FlareEngine
{
    /// <summary>
    /// AnimationMedia
    ///
    /// 管理一个动画（Animation）所引用的所有精灵图（sprite image）的延迟加载：
    /// 记录 "键(key，通常是方向/朝向) -> 图片路径" 的映射，图片本身只在第一次被
    /// <see cref="GetImageFromKey"/> 请求时才通过 <c>RenderDevice</c> 实际加载，
    /// 从而避免一次性加载所有朝向的图片。<c>Unref</c> 负责释放已加载图片的引用计数。
    ///
    /// C++ 原始实现使用 <c>std::map&lt;std::string, Image*&gt; sprites</c> 保存已加载
    /// （或占位为 NULL 表示尚未加载）的图片指针，<c>std::map&lt;std::string, std::string&gt; paths</c>
    /// 保存对应的加载路径。C# 版本按类型映射表使用 <c>Dictionary&lt;string, Image?&gt;</c> /
    /// <c>Dictionary&lt;string, string&gt;</c> 表达同样的语义。
    /// </summary>
    public class AnimationMedia
    {
        // C++: std::map<std::string, Image*> sprites;
        private readonly Dictionary<string, Image?> _sprites = [];
        // C++: std::map<std::string, std::string> paths;
        private readonly Dictionary<string, string> _paths = [];
        // C++: std::string first_key;
        private string _firstKey;
        // C++: std::string first_path;
        private string _firstPath;

        // C++: AnimationMedia::AnimationMedia() : first_key(""), first_path("") {}
        public AnimationMedia()
        {
            _firstKey = "";
            _firstPath = "";
        }

        // C++: AnimationMedia::~AnimationMedia() {}
        // 原始析构函数为空：sprites 中缓存的 Image* 在销毁时既不 delete 也不 unref，
        // 引用计数释放完全依赖调用方显式调用 Unref()（见下）。为保持逐行等价，
        // 此处不实现 IDisposable/析构逻辑，避免引入原始代码中不存在的释放行为。

        // C++: void AnimationMedia::loadImage(const std::string& path, const std::string& key)
        public void LoadImage(string path, string key)
        {
            SharedResources.RenderDevice!.PushQueuedImage(path, RenderDevice.ErrorNormal);

            if (!_sprites.ContainsKey(key))
            {
                _sprites[key] = null;
                _paths[key] = path;
            }
            else
            {
                if (_sprites[key] != null)
                    _sprites[key]!.Unref();

                _sprites[key] = null;
                _paths[key] = path;
            }

            if (_sprites.Count == 1)
            {
                _firstKey = key;
                _firstPath = path;
            }
        }

        // C++: Image* AnimationMedia::getImageFromKey(const std::string& key)
        public Image? GetImageFromKey(string key)
        {
            var renderDevice = SharedResources.RenderDevice!;

            if (_sprites.TryGetValue(key, out Image? img))
            {
                if (img == null)
                {
                    img = renderDevice.LoadImage(_paths[key], RenderDevice.ErrorNormal);
                    _sprites[key] = img;
                    if (img == null)
                    {
                        _sprites.Remove(key);
                        return null;
                    }
                }
                return img;
            }
            else if (_sprites.Count != 0)
            {
                // 对应 C++ std::map::operator[] 在键不存在时会自动插入默认值(NULL)这一行为：
                // Dictionary 索引器在键缺失时会抛出异常，因此这里用 TryGetValue 判定
                // “缺失”或“存在但为 null”两种情况，语义与原始 sprites[first_key] 完全等价。
                if (!_sprites.TryGetValue(_firstKey, out Image? firstImg) || firstImg == null)
                {
                    firstImg = renderDevice.LoadImage(_firstPath, RenderDevice.ErrorNormal);
                    _sprites[_firstKey] = firstImg;
                }
                return firstImg;
            }

            return null;
        }

        // C++: void AnimationMedia::unref()
        public void Unref()
        {
            foreach (KeyValuePair<string, Image?> it in _sprites)
            {
                if (it.Value != null)
                    it.Value.Unref();
            }
            _sprites.Clear();
        }
    }
}
