// <自动生成> 对应 C++ 源文件：DeviceList.h + DeviceList.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。

namespace FlareEngine
{
    /// <summary>
    /// DeviceList
    ///
    /// 对应 C++ 原始的一组全局工厂函数（DeviceList.h 未声明任何类，只声明了自由函数）。
    /// 依据阶段 B 规则："全局函数（非成员函数）转为 static class 中的静态方法"，
    /// 这里将其归并为静态类 <c>DeviceList</c>，方法名沿用 PascalCase 命名。
    ///
    /// 本单元不直接调用任何 SDL2 静态 API：渲染设备、字体引擎、音频管理器、输入状态
    /// 均通过各自的接口/抽象类实例化（<c>SDLSoftwareRenderDevice</c>、
    /// <c>SDLHardwareRenderDevice</c>、<c>SDLFontEngine</c>、<c>SDLSoundManager</c>、
    /// <c>SDLInputState</c>），SDL 相关细节被封装在这些派生类各自的转换单元中，
    /// 与规则"SDL2 静态调用必须通过接口实例调用"保持一致。
    /// </summary>
    public static class DeviceList
    {
        /// <summary>
        /// 根据配置名称创建对应的渲染设备实例。"sdl" 为默认值。
        /// </summary>
        public static RenderDevice GetRenderDevice(string name)
        {
            // "sdl" is the default
            if (name != "")
            {
                if (name == "sdl") return new SDLSoftwareRenderDevice();
                else if (name == "sdl_hardware") return new SDLHardwareRenderDevice();
                else
                {
                    Utils.LogError("DeviceList: Render device '%s' not found. Falling back to the default.", name);
                    return new SDLHardwareRenderDevice();
                }
            }
            else
            {
                return new SDLHardwareRenderDevice();
            }
        }

        /// <summary>
        /// 填充可选渲染设备的名称与说明文字列表，供设置界面展示。
        /// </summary>
        public static void CreateRenderDeviceList(MessageEngine msg, List<string> rdName, List<string> rdDesc)
        {
            rdName.Clear();
            rdDesc.Clear();

            Resize(rdName, 2);
            Resize(rdDesc, 2);

            rdName[0] = "sdl";
            rdDesc[0] = msg.Get("SDL software renderer\n\nOften slower, but less likely to have issues.");

            rdName[1] = "sdl_hardware";
            rdDesc[1] = msg.Get("SDL hardware renderer\n\nThe default renderer that is often faster than the SDL software renderer.");
        }

        public static FontEngine GetFontEngine()
        {
            return new SDLFontEngine();
        }

        public static SoundManager GetSoundManager()
        {
            return new SDLSoundManager();
        }

        public static InputState GetInputManager()
        {
            return new SDLInputState();
        }

        /// <summary>
        /// 对应 C++ <c>std::vector&lt;std::string&gt;::resize(newSize)</c>：
        /// 缩小时从尾部截断，放大时用默认值（空字符串）补齐末尾，元素顺序保持不变。
        /// List&lt;T&gt; 没有内建的 resize 操作，因此提供该私有辅助方法以逐行还原原始语义。
        /// </summary>
        private static void Resize(List<string> list, int newSize)
        {
            if (newSize < list.Count)
            {
                list.RemoveRange(newSize, list.Count - newSize);
            }
            else
            {
                while (list.Count < newSize)
                {
                    list.Add("");
                }
            }
        }
    }
}
