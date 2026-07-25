// <自动生成> 对应 C++ 源文件：Menu.h + Menu.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// Menu
    ///
    /// 所有菜单 UI 的基类，负责窗口区域、背景精灵、屏幕对齐以及配置键解析等通用逻辑。
    /// 持有的 <see cref="_background"/> 精灵通过 <see cref="IDisposable"/> 显式释放，
    /// 释放顺序与原始析构函数 <c>~Menu()</c> 一致。
    /// </summary>
    public class Menu : IDisposable
    {
        public bool Visible;
        public bool Enabled;
        public Rectangle WindowArea;
        public int Alignment;
        public SoundID SfxOpen;
        public SoundID SfxClose;
        public TabList Tablist;

        protected Sprite? _background;

        private Int2 _windowAreaBase;

        public Menu()
        {
            Visible = false;
            Enabled = true;
            Alignment = Utils.AlignTopLeft;
            SfxOpen = 0;
            SfxClose = 0;
            _background = null;
            Tablist = new TabList();
        }

        /// <summary>
        /// 对应 C++ 的 <c>~Menu()</c>：释放背景精灵。
        /// 派生类若实现 <see cref="IDisposable"/>，应在释放完自身资源后调用 <c>base.Dispose()</c>，
        /// 以还原 C++ 中"派生类析构函数体执行完毕后隐式调用基类析构函数"的顺序。
        /// </summary>
        public virtual void Dispose()
        {
            if (_background != null)
            {
                _background.Dispose();
                _background = null;
            }

            GC.SuppressFinalize(this);
        }

        public void SetBackground(string backgroundImage)
        {
            Image? graphics;

            if (_background != null)
            {
                _background.Dispose();
                _background = null;
            }

            graphics = SharedResources.RenderDevice!.LoadImage(backgroundImage, RenderDevice.ErrorNormal);
            if (graphics != null)
            {
                _background = graphics.CreateSprite();
                _background.SetClip(0, 0, WindowArea.Width, WindowArea.Height);
                _background.SetDestFromRect(WindowArea);
                graphics.Unref();
            }
        }

        public void SetBackgroundDest(Rectangle dest)
        {
            if (_background != null)
                _background.SetDestFromRect(dest);
        }

        public void SetBackgroundClip(Rectangle clip)
        {
            if (_background != null)
                _background.SetClipFromRect(clip);
        }

        public void SetBackgroundColor(Color color)
        {
            if (_background != null)
            {
                _background.Dispose();
                _background = null;
            }

            // fill the background rectangle
            Image? temp = SharedResources.RenderDevice!.CreateImage(WindowArea.Width, WindowArea.Height);
            if (temp != null)
            {
                // translucent black background
                temp.FillWithColor(color);
                _background = temp.CreateSprite();
                temp.Unref();
            }
        }

        public virtual void Render()
        {
            if (_background != null)
                SharedResources.RenderDevice!.Render(_background);
        }

        /// <summary>
        /// Aligns the menu relative to one of these positions:
        /// topleft, top, topright, left, center, right, bottomleft, bottom, bottomright
        /// </summary>
        public virtual void Align()
        {
            WindowArea.X = _windowAreaBase.X;
            WindowArea.Y = _windowAreaBase.Y;

            Utils.AlignToScreenEdge(Alignment, ref WindowArea);

            if (_background != null)
            {
                _background.SetClip(0, 0, WindowArea.Width, WindowArea.Height);
                _background.SetDestFromRect(WindowArea);
            }
        }

        public virtual void SetWindowPos(int x, int y)
        {
            _windowAreaBase.X = x;
            _windowAreaBase.Y = y;
        }

        /// <summary>
        /// When reading menu config files, we use this to set common variables
        /// </summary>
        public virtual bool ParseMenuKey(string key, string val)
        {
            // @CLASS Menu|Description of menus in menus/
            string value = val;

            if (key == "pos")
            {
                // @ATTR pos|rectangle|Menu position and dimensions
                value = value + ',';
                WindowArea = Parse.ToRect(value);
                SetWindowPos(WindowArea.X, WindowArea.Y);
            }
            else if (key == "align")
            {
                // @ATTR align|alignment|Position relative to screen edges
                Alignment = Parse.ToAlignment(value);
            }
            else if (key == "soundfx_open")
            {
                // @ATTR soundfx_open|filename|Filename of a sound to play when opening this menu.
                SfxOpen = SharedResources.Snd!.Load(value, "Menu open tab");
            }
            else if (key == "soundfx_close")
            {
                // @ATTR soundfx_close|filename|Filename of a sound to play when closing this menu.
                SfxClose = SharedResources.Snd!.Load(value, "Menu close tab");
            }
            else if (key == "background")
            {
                // @ATTR background|filename|Filename of the background image for this menu.
                SetBackground(value);
            }
            else if (key == "enabled")
            {
                // @ATTR enabled|bool|Used to toggle the ability to open this menu. Only used for Character, Inventory, Powers, and Log menus.
                Enabled = Parse.ToBool(value);
            }
            else
            {
                //not a common key
                return false;
            }

            return true;
        }

        public virtual TabList? GetCurrentTabList()
        {
            if (Tablist.GetCurrent() != -1)
                return Tablist;

            return null;
        }

        public virtual void DefocusTabLists()
        {
            Tablist.Defocus();
        }
    }
}
