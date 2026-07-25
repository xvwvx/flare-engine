// 对应 C++ 源：RenderDevice.h + RenderDevice.cpp
using System.Diagnostics;
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// Sprite 表示 Image 中的一块可绘制区域。
    /// 仅能通过 <see cref="Image.CreateSprite"/> 创建；销毁时释放对源 Image 的引用。
    /// </summary>
    public class Sprite : IDisposable
    {
        public Rectangle LocalFrame;
        public Color ColorMod;
        public byte AlphaMod;

        internal Image? _image;
        public Rectangle Clip;
        public Int2 Offset;
        public Int2 Dest;

        internal Sprite(Image image)
        {
            LocalFrame = default;
            ColorMod = new Color(255, 255, 255, 255);
            AlphaMod = 255;
            _image = image;
            Clip = default;
            Offset = default;
            Dest = default;
            _image.Ref();
        }

        ~Sprite()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_image != null)
            {
                // 仅在显式 Dispose（disposing=true）时释放 SDL 原生资源；
                // 终结器（disposing=false）运行时 SDL 上下文可能已销毁，
                // 此时只清空引用，避免 0xC0000005 访问违规。
                if (disposing)
                    _image.Unref();
                _image = null;
            }
        }

        public Image? GetGraphics()
        {
            return _image;
        }

        public void SetOffset(Int2 offset)
        {
            Offset = offset;
        }

        public Int2 GetOffset()
        {
            return Offset;
        }

        public void SetClipFromRect(Rectangle clip)
        {
            Clip = clip;

            // don't exceed the dimensions of the image
            int targetW = GetGraphicsWidth();
            int targetH = GetGraphicsHeight();
            if (Clip.X + Clip.Width > targetW)
                Clip.Width = targetW - Clip.X;
            if (Clip.Y + Clip.Height > targetH)
                Clip.Height = targetH - Clip.Y;
        }

        public void SetClip(int x, int y, int w, int h)
        {
            SetClipFromRect(new Rectangle(x, y, w, h));
        }

        public Rectangle GetClip()
        {
            return Clip;
        }

        public void SetDestFromRect(Rectangle destRect)
        {
            Dest.X = destRect.X;
            Dest.Y = destRect.Y;
        }

        public void SetDestFromPoint(Int2 destPoint)
        {
            Dest.X = destPoint.X;
            Dest.Y = destPoint.Y;
        }

        public void SetDest(int x, int y)
        {
            Dest.X = x;
            Dest.Y = y;
        }

        public Int2 GetDest()
        {
            return Dest;
        }

        public int GetGraphicsWidth()
        {
            return _image!.GetWidth();
        }

        public int GetGraphicsHeight()
        {
            return _image!.GetHeight();
        }
    }

    /// <summary>
    /// 图像资源抽象基类，由 <see cref="RenderDevice"/> 创建并拥有。
    /// 使用引用计数；最后一个引用释放时从缓存移除并销毁。
    /// </summary>
    public abstract class Image
    {
        protected RenderDevice _device;
        protected uint _refCounter;

        protected Image(RenderDevice device)
        {
            _device = device;
            _refCounter = 1;
        }

        public void Ref()
        {
            ++_refCounter;
        }

        public void Unref()
        {
            --_refCounter;
            if (_refCounter == 0)
            {
                _device.FreeImage(this);
                // 对应 C++ unref()==0 → delete this → ~SDLHardwareImage/~SDLSoftwareImage
                ReleaseNativeResources();
            }
        }

        public uint RefCount
        {
            get => _refCounter;
        }

        public virtual int GetWidth()
        {
            return 0;
        }

        public virtual int GetHeight()
        {
            return 0;
        }

        public abstract void FillWithColor(Color color);
        public abstract void DrawPixel(int x, int y, Color color);
        public abstract void DrawLine(int x0, int y0, int x1, int y1, Color color);
        public abstract void DrawFilledRect(int x, int y, int w, int h, Color color);

        public virtual void BeginPixelBatch()
        {
        }

        public virtual void BeginPixelBatch(ref Rectangle bounds)
        {
            if (bounds.X != 0) { } // suppress unused paramater warning
        }

        public virtual void EndPixelBatch()
        {
        }

        /// <summary>释放派生类持有的 SDL 原生资源（纹理/Surface）。</summary>
        internal virtual void ReleaseNativeResources()
        {
        }

        public abstract Image Resize(int width, int height);

        public Sprite CreateSprite()
        {
            Sprite sprite;
            sprite = new Sprite(this);
            sprite.SetClip(0, 0, GetWidth(), GetHeight());
            return sprite;
        }
    }

    /// <summary>
    /// 可渲染对象描述，携带图像、源矩形、地图位置与混合信息。
    /// </summary>
    public class Renderable
    {
        public const int BlendNormal = 0;
        public const int BlendAdd = 1;

        public const int TypeNormal = 0;
        public const int TypeHero = 1;
        public const int TypeEnemy = 2;
        public const int TypeAlly = 3;

        public Image? Image;
        public Rectangle Src;

        public Vector2 MapPos;
        public Int2 Offset;
        public ulong Prio;

        public byte BlendMode;
        public Color ColorMod;
        public byte AlphaMod;

        public byte Type;

        public Renderable()
        {
            Image = null;
            Src = default;
            MapPos = default;
            Offset = default;
            Prio = 0;
            BlendMode = BlendNormal;
            ColorMod = new Color(255, 255, 255, 255);
            AlphaMod = 255;
            Type = TypeNormal;
        }
    }

    /// <summary>
    /// 异步图像加载队列条目。
    /// <see cref="Mutex"/> 与 <see cref="Loaded"/> 为 SDL 同步原语的 opaque 占位，
    /// 由 SDL 渲染设备派生类在其转换单元中赋值与使用。
    /// </summary>
    public class QueuedImage
    {
        public object? Surface;
        public int ErrorType;
        public bool LoadAttempted;
        public string Filename;
        public string LocFilename;
        public object? Mutex;
        public object? Loaded;

        public QueuedImage()
        {
            Surface = null;
            ErrorType = 0;
            LoadAttempted = false;
            Filename = "";
            LocFilename = "";
            Mutex = null;
            Loaded = null;
        }
    }

    /// <summary>
    /// FLARE 引擎渲染设备抽象接口。
    /// </summary>
    public abstract class RenderDevice : IDisposable
    {
        public const int ErrorNone = 0;
        public const int ErrorNormal = 1;
        public const int ErrorExit = 2;

        public const byte BitsPerPixel = 32;

        protected bool Fullscreen;
        protected bool Hwsurface;
        protected bool Vsync;
        protected bool TextureFilter;
        protected bool IgnoreTextureFilter;
        protected Int2 MinScreen;
        protected bool DestructiveFullscreen;

        protected bool IsInitialized;
        protected bool ReloadGraphicsFlag;

        protected float Ddpi;

        protected Rectangle MClip;
        protected Rectangle MDest;

        protected List<QueuedImage> ImageQueue = new List<QueuedImage>();
        protected List<Image?> ImageQueueCleanup = new List<Image?>();

        private Dictionary<string, Image?> _cache = new Dictionary<string, Image?>();

        protected RenderDevice()
        {
            Fullscreen = false;
            Hwsurface = false;
            Vsync = false;
            TextureFilter = false;
            IgnoreTextureFilter = false;
            MinScreen = new Int2(640, 480);
            DestructiveFullscreen = false;
            IsInitialized = false;
            ReloadGraphicsFlag = false;
            Ddpi = 0;
        }

        ~RenderDevice()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Utils.LogInfo("Cleaning up: RenderDevice");
        }

        public int CreateContext()
        {
            int status = CreateContextInternal();

            if (status == -1)
            {
                Utils.LogError("RenderDevice: createContext() failed, trying previous settings.");
                // try previous setting first
                SharedResources.Settings!.Fullscreen = Fullscreen;
                SharedResources.Settings!.Hwsurface = Hwsurface;
                SharedResources.Settings!.Vsync = Vsync;
                SharedResources.Settings!.TextureFilter = TextureFilter;

                status = CreateContextInternal();
            }

            if (status == -1)
            {
                Utils.LogError("RenderDevice: createContext() failed, disabling all options.");
                // last resort, try turning everything off
                SharedResources.Settings!.Fullscreen = false;
                SharedResources.Settings!.Hwsurface = false;
                SharedResources.Settings!.Vsync = false;
                SharedResources.Settings!.TextureFilter = false;

                status = CreateContextInternal();
            }

            if (status == -1)
            {
                // all attempts have failed, abort!
                CreateContextError();
                Utils.Exit(1);
            }

            return status;
        }

        public virtual void DestroyContext()
        {
            if (_cache.Count != 0)
            {
                Dictionary<string, Image?>.Enumerator it;
                Utils.LogError("RenderDevice: Image cache still holding these images:");
                it = _cache.GetEnumerator();
                while (it.MoveNext())
                {
                    Utils.LogError("%s %d", it.Current.Key, it.Current.Value!.RefCount);
                }
            }
            Debug.Assert(_cache.Count == 0);
        }

        public abstract void SetGamma(float g);
        public abstract void ResetGamma();
        public abstract void UpdateTitleBar();

        public abstract Image? LoadImage(string filename, int errorType);
        public abstract Image? CreateImage(int width, int height);

        public void FreeImage(Image? image)
        {
            if (image == null) return;

            CacheRemove(image);
        }

        public abstract int Render(Sprite r);
        public abstract int Render(Renderable r, ref Rectangle dest);
        public abstract int RenderToImage(Image srcImage, Rectangle src, Image destImage, Rectangle dest);
        public abstract Image? RenderTextToImage(FontStyle fontStyle, string text, Color color, bool blended);
        public abstract void BlankScreen();
        public abstract void CommitFrame();
        public abstract void DrawPixel(int x, int y, Color color);
        public abstract void DrawLine(int x0, int y0, int x1, int y1, Color color);
        public abstract void DrawRectangle(Int2 p0, Int2 p1, Color color);

        public void DrawRectangleCorners(int csize, Int2 p0, Int2 p1, Color color)
        {
            // if corner size if "blank", draw a standard rectangle
            if (csize <= 0)
            {
                DrawRectangle(p0, p1, color);
                return;
            }

            // top left
            DrawLine(p0.X, p0.Y, p0.X + csize, p0.Y, color);
            DrawLine(p0.X, p0.Y, p0.X, p0.Y + csize, color);

            // top right
            DrawLine(p1.X - csize, p0.Y, p1.X, p0.Y, color);
            DrawLine(p1.X, p0.Y, p1.X, p0.Y + csize, color);

            // bottom left
            DrawLine(p0.X, p1.Y, p0.X + csize, p1.Y, color);
            DrawLine(p0.X, p1.Y - csize, p0.X, p1.Y, color);

            // bottom right
            DrawLine(p1.X - csize, p1.Y, p1.X + 1, p1.Y, color);
            DrawLine(p1.X, p1.Y - csize, p1.X, p1.Y, color);
        }

        public void DrawEllipse(int x0, int y0, int x1, int y1, Color color, float step)
        {
            float rx = (float)(x1 - x0) / 2f;
            float ry = (float)(y1 - y0) / 2f;
            float cx = (float)(x1 + x0) / 2f;
            float cy = (float)(y1 + y0) / 2f;

            float rad = (step / 180) * MathF.PI;

            float lastx = 0;
            float lasty = 0;

            for (float i = 0; i < MathF.PI * 2; i += rad)
            {
                float curx = cx + MathF.Cos(i) * rx;
                float cury = cy + MathF.Sin(i) * ry;

                if (i > 0)
                    DrawLine((int)lastx, (int)lasty, (int)curx, (int)cury, color);

                lastx = curx;
                lasty = cury;
            }
        }

        public abstract void WindowResize();

        public virtual void SetBackgroundColor(Color color)
        {
            // print out the color to avoid unused variable compiler warning
            Utils.LogInfo("RenderDevice: Trying to set background color to (%d,%d,%d,%d).", color.R, color.G, color.B, color.A);
            Utils.LogError("RenderDevice: Renderer does not support setting background color!");
        }

        public virtual void SetFullscreen(bool enableFullscreen)
        {
            Utils.LogInfo("RenderDevice: Trying to set fullscreen=%d, without recreating the rendering context, but setFullscreen() is not implemented for this renderer.", enableFullscreen ? 1 : 0);
        }

        public virtual ushort GetRefreshRate()
        {
            Utils.LogInfo("RenderDevice: getRefreshRate() not implemented");
            return 0;
        }

        public bool ReloadGraphics()
        {
            if (ReloadGraphicsFlag)
            {
                ReloadGraphicsFlag = false;
                return true;
            }

            return false;
        }

        public void PushQueuedImage(string filename, int errorType)
        {
            if (!SharedResources.Settings!.EnableThreadedImageLoad)
                return;

            Image? cacheTest = CacheLookup(filename);
            if (cacheTest != null)
            {
                // image already in cache. We need to decrease the ref count because the lookup would have increased it
                cacheTest.Unref();
                return;
            }

            for (int i = 0; i < ImageQueue.Count; ++i)
            {
                if (ImageQueue[i].Filename == filename)
                {
                    // image already in queue
                    return;
                }
            }

            QueuedImage queuedImage = new QueuedImage();
            queuedImage.Filename = filename;
            queuedImage.LocFilename = SharedResources.Mods!.Locate(filename);
            queuedImage.ErrorType = errorType;

            ImageQueue.Add(queuedImage);
        }

        public abstract void LoadQueuedImages();

        public void CleanupQueuedImages()
        {
            for (int i = 0; i < ImageQueueCleanup.Count; ++i)
            {
                ImageQueueCleanup[i]!.Unref();
            }
            ImageQueueCleanup.Clear();
        }

        protected bool LocalToGlobal(Sprite r)
        {
            MClip = r.GetClip();

            int left = r.GetDest().X - r.GetOffset().X;
            int right = left + MClip.Width;
            int up = r.GetDest().Y - r.GetOffset().Y;
            int down = up + MClip.Height;

            // Check whether we need to render.
            // If so, compute the correct clipping.
            if (r.LocalFrame.Width != 0)
            {
                if (left > r.LocalFrame.Width)
                {
                    return false;
                }
                if (right < 0)
                {
                    return false;
                }
                if (left < 0)
                {
                    MClip.X = MClip.X - left;
                    left = 0;
                }
                right = (right < r.LocalFrame.Width ? right : r.LocalFrame.Width);
                MClip.Width = right - left;
            }
            if (r.LocalFrame.Height != 0)
            {
                if (up > r.LocalFrame.Height)
                {
                    return false;
                }
                if (down < 0)
                {
                    return false;
                }
                if (up < 0)
                {
                    MClip.Y = MClip.Y - up;
                    up = 0;
                }
                down = (down < r.LocalFrame.Height ? down : r.LocalFrame.Height);
                MClip.Height = down - up;
            }

            MDest.X = left + r.LocalFrame.X;
            MDest.Y = up + r.LocalFrame.Y;

            return true;
        }

        protected Image? CacheLookup(string filename)
        {
            if (_cache.TryGetValue(filename, out Image? image))
            {
                image!.Ref();
                return image;
            }
            return null;
        }

        protected void CacheStore(string filename, Image? image)
        {
            if (image == null) return;
            _cache[filename] = image;
        }

        protected void CacheRemove(Image? image)
        {
            string? foundKey = null;
            foreach (KeyValuePair<string, Image?> entry in _cache)
            {
                if (entry.Value == image)
                {
                    foundKey = entry.Key;
                    break;
                }
            }

            if (foundKey != null)
            {
                _cache.Remove(foundKey);
            }
        }

        protected void CacheRemoveAll()
        {
            foreach (KeyValuePair<string, Image?> entry in _cache)
            {
                entry.Value?.ReleaseNativeResources();
            }
            _cache.Clear();
        }

        protected void WindowResizeInternal()
        {
            ushort oldViewW = SharedResources.Settings!.ViewW;
            ushort oldViewH = SharedResources.Settings!.ViewH;
            ushort oldScreenW = SharedResources.Settings!.ScreenW;
            ushort oldScreenH = SharedResources.Settings!.ScreenH;

            GetWindowSize(out SharedResources.Settings!.ScreenW, out SharedResources.Settings!.ScreenH);

            ushort tempScreenH;
            if (SharedResources.Settings!.DpiScaling && Ddpi > 0 && SharedResources.Eset!.Resolutions.VirtualDpi > 0)
            {
                tempScreenH = (ushort)((float)SharedResources.Settings!.ScreenH * (SharedResources.Eset!.Resolutions.VirtualDpi / Ddpi));
            }
            else
            {
                tempScreenH = SharedResources.Settings!.ScreenH;
            }
            SharedResources.Settings!.ViewH = tempScreenH;

            // scale virtual height when outside of VIRTUAL_HEIGHTS range
            ushort minRenderSize = SharedResources.Settings!.MinRenderSize;
            ushort maxRenderSize = SharedResources.Settings!.MaxRenderSize;

            if (maxRenderSize == 0 && SharedResources.Eset!.Resolutions.VirtualHeights.Count != 0)
                maxRenderSize = SharedResources.Eset!.Resolutions.VirtualHeights[^1];

            if (minRenderSize > 0 && minRenderSize > SharedResources.Settings!.ViewH)
                SharedResources.Settings!.ViewH = minRenderSize;

            if (SharedResources.Eset!.Resolutions.VirtualHeights.Count != 0)
            {
                if (minRenderSize < SharedResources.Eset!.Resolutions.VirtualHeights[0] && tempScreenH < SharedResources.Eset!.Resolutions.VirtualHeights[0])
                    SharedResources.Settings!.ViewH = SharedResources.Eset!.Resolutions.VirtualHeights[0];
                else if (tempScreenH >= maxRenderSize)
                    SharedResources.Settings!.ViewH = maxRenderSize;
            }

            SharedResources.Settings!.ViewHHalf = (ushort)(SharedResources.Settings!.ViewH / 2);

            SharedResources.Settings!.ViewScaling = (float)SharedResources.Settings!.ViewH / (float)SharedResources.Settings!.ScreenH;
            SharedResources.Settings!.ViewW = (ushort)((float)SharedResources.Settings!.ScreenW * SharedResources.Settings!.ViewScaling);

            // letterbox if too tall
            ushort minScreenW = (ushort)Math.Max(SharedResources.Eset!.Resolutions.MinScreenW, SharedResources.Eset!.Resolutions.FrameW);
            if (SharedResources.Settings!.ViewW < minScreenW)
            {
                SharedResources.Settings!.ViewW = minScreenW;
                SharedResources.Settings!.ViewScaling = (float)SharedResources.Settings!.ViewW / (float)SharedResources.Settings!.ScreenW;
            }

            SharedResources.Settings!.ViewWHalf = (ushort)(SharedResources.Settings!.ViewW / 2);

            if (SharedResources.Settings!.ViewW != oldViewW || SharedResources.Settings!.ViewH != oldViewH)
            {
                Utils.LogInfo("RenderDevice: Internal render size is %dx%d", SharedResources.Settings!.ViewW, SharedResources.Settings!.ViewH);
            }
            if (SharedResources.Settings!.ScreenW != oldScreenW || SharedResources.Settings!.ScreenH != oldScreenH)
            {
                Utils.LogInfo("RenderDevice: Window size changed to %dx%d", SharedResources.Settings!.ScreenW, SharedResources.Settings!.ScreenH);
            }
        }

        protected abstract int CreateContextInternal();
        protected abstract void CreateContextError();

        protected abstract void GetWindowSize(out ushort screenW, out ushort screenH);
    }
}
