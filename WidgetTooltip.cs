// <自动生成> 对应 C++ 源文件：WidgetTooltip.h + WidgetTooltip.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// WidgetTooltip
    ///
    /// 工具提示控件：缓存 TTF 栅格化结果并按屏幕象限计算浮动位置。
    /// 持有的背景 Image（引用计数）与 Sprite 缓存通过 <see cref="IDisposable"/> 显式释放，
    /// 对应原始析构函数中的 background->unref() 与 delete sprite_buf。
    /// </summary>
    public class WidgetTooltip : IDisposable
    {
        public Rectangle Bounds;
        public WidgetTooltip? Parent;

        private Image? _background;
        private TooltipData _dataBuf = new TooltipData();
        private Sprite? _spriteBuf;

        public WidgetTooltip()
        {
            var renderDevice = SharedResources.RenderDevice!;
            Parent = null;
            _background = renderDevice.LoadImage("images/menus/tooltips.png", RenderDevice.ErrorNone);
            _spriteBuf = null;
        }

        /// <summary>
        /// 对应 C++ 的 <c>~WidgetTooltip()</c>，释放背景 Image 引用与缓存 Sprite，
        /// 释放顺序与原始析构函数一致（先 background->unref()，后 delete sprite_buf）。
        /// </summary>
        public void Dispose()
        {
            if (_background != null)
            {
                _background.Unref();
                _background = null;
            }

            _spriteBuf?.Dispose();
            _spriteBuf = null;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 已知文本总尺寸与原点位置，计算背景与文本的起始绘制位置。
        /// </summary>
        public Int2 CalcPosition(byte style, Int2 pos, Int2 size)
        {
            var eset = SharedResources.Eset!;
            var settings = SharedResources.Settings!;

            Int2 tipPos = default;

            // TopLabel style is fixed and centered over the origin
            if (style == TooltipData.StyleTopLabel)
            {
                tipPos.X = pos.X - size.X / 2;
                tipPos.Y = pos.Y - eset.Tooltips.Offset;
            }
            // Float style changes position based on the screen quadrant of the origin
            // (usually used for tooltips which are long and we don't want them to overflow
            //  off the end of the screen)
            else if (style == TooltipData.StyleFloat)
            {
                // get the "root" tooltip so we can see if there's more empty space on the left or right
                // this helps us determine where child tooltips should go
                WidgetTooltip? root = this;
                while (root != null)
                {
                    if (root.Parent != null)
                        root = root.Parent;
                    else
                        break;
                }

                // upper left
                if (pos.X < settings.ViewWHalf && pos.Y < settings.ViewHHalf)
                {
                    if (Parent != null)
                    {
                        if (root != null && root.Bounds.X > settings.ViewW - (root.Bounds.X + root.Bounds.Width))
                            tipPos.X = Parent.Bounds.X - size.X;
                        else
                            tipPos.X = Parent.Bounds.X + Parent.Bounds.Width;
                    }
                    else
                        tipPos.X = pos.X + eset.Tooltips.Offset;

                    tipPos.Y = pos.Y + eset.Tooltips.Offset;
                }
                // upper right
                else if (pos.X >= settings.ViewWHalf && pos.Y < settings.ViewHHalf)
                {
                    if (Parent != null)
                    {
                        if (root != null && root.Bounds.X < settings.ViewW - (root.Bounds.X + root.Bounds.Width))
                            tipPos.X = Parent.Bounds.X + Parent.Bounds.Width;
                        else
                            tipPos.X = Parent.Bounds.X - size.X;
                    }
                    else
                        tipPos.X = pos.X - eset.Tooltips.Offset - size.X;

                    tipPos.Y = pos.Y + eset.Tooltips.Offset;
                }
                // lower left
                else if (pos.X < settings.ViewWHalf && pos.Y >= settings.ViewHHalf)
                {
                    if (Parent != null)
                    {
                        if (root != null && root.Bounds.X > settings.ViewW - (root.Bounds.X + root.Bounds.Width))
                            tipPos.X = Parent.Bounds.X - size.X;
                        else
                            tipPos.X = Parent.Bounds.X + Parent.Bounds.Width;
                    }
                    else
                        tipPos.X = pos.X + eset.Tooltips.Offset;

                    tipPos.Y = pos.Y - eset.Tooltips.Offset - size.Y;
                }
                // lower right
                else if (pos.X >= settings.ViewWHalf && pos.Y >= settings.ViewHHalf)
                {
                    if (Parent != null)
                    {
                        if (root != null && root.Bounds.X < settings.ViewW - (root.Bounds.X + root.Bounds.Width))
                            tipPos.X = Parent.Bounds.X + Parent.Bounds.Width;
                        else
                            tipPos.X = Parent.Bounds.X - size.X;
                    }
                    else
                        tipPos.X = pos.X - eset.Tooltips.Offset - size.X;

                    tipPos.Y = pos.Y - eset.Tooltips.Offset - size.Y;
                }

                // very large tooltips might still be off screen at this point
                // so we try to constrain them to the screen bounds
                // we give priority to being able to read the top-left of the tooltip over the bottom-right
                // EXCEPTION: If the tooltip is a child of another, we don't constrain the x-axis
                if (tipPos.X + size.X > settings.ViewW && Parent == null)
                    tipPos.X = settings.ViewW - size.X;

                if (tipPos.Y + size.Y > settings.ViewH)
                    tipPos.Y = settings.ViewH - size.Y;

                if (tipPos.X < 0 && Parent == null)
                    tipPos.X = 0;

                if (tipPos.Y < 0)
                    tipPos.Y = 0;

                // try clamping x offset to middle of screen. This prevents most cases where child tips would go offscreen
                if (pos.X < settings.ViewWHalf && Parent == null && tipPos.X + size.X > settings.ViewWHalf)
                {
                    Rectangle testRect = new Rectangle(settings.ViewWHalf - size.X, tipPos.Y, size.X, size.Y);
                    if (!Utils.IsWithinRect(testRect, pos))
                        tipPos.X -= (tipPos.X + size.X - settings.ViewWHalf);
                }
                else if (pos.X >= settings.ViewWHalf && Parent == null && tipPos.X < settings.ViewWHalf)
                {
                    Rectangle testRect = new Rectangle(settings.ViewWHalf, tipPos.Y, size.X, size.Y);
                    if (!Utils.IsWithinRect(testRect, pos))
                        tipPos.X += (settings.ViewWHalf - tipPos.X);
                }
            }
            else if (style == TooltipData.StyleAbsolute)
            {
                tipPos.X = pos.X;
                tipPos.Y = pos.Y;
            }

            return tipPos;
        }

        /// <summary>
        /// 在需要时创建缓存文本缓冲，并设置工具提示的位置与边界。
        /// </summary>
        public void Prerender(TooltipData tip, Int2 pos, byte style)
        {
            if (_spriteBuf == null || !tip.Compare(_dataBuf))
            {
                if (!CreateBuffer(tip)) return;
            }

            Int2 size = default;
            size.X = _spriteBuf!.GetGraphicsWidth();
            size.Y = _spriteBuf.GetGraphicsHeight();

            Int2 tipPos = CalcPosition(style, pos, size);

            _spriteBuf.SetDestFromPoint(tipPos);

            Bounds.X = tipPos.X;
            Bounds.Y = tipPos.Y;
            Bounds.Width = size.X;
            Bounds.Height = size.Y;
        }

        /// <summary>
        /// 工具提示位置取决于源点的屏幕象限。
        /// 若存在缓存则绘制缓存，否则渲染工具提示并写入缓存。
        /// </summary>
        public void Render(TooltipData tip, Int2 pos, byte style)
        {
            if (tip.IsEmpty())
                return;

            Prerender(tip, pos, style);
            SharedResources.RenderDevice!.Render(_spriteBuf!);
        }

        /// <summary>
        /// 渲染文字较多的工具提示（TTF 转栅格）开销较大；
        /// 因此只执行一次并缓存结果，而非每帧重复。
        /// </summary>
        public bool CreateBuffer(TooltipData tip)
        {
            var eset = SharedResources.Eset!;
            var font = SharedResources.Font!;
            var renderDevice = SharedResources.RenderDevice!;

            if (tip.Lines.Count == 0)
            {
                tip.Lines.Clear();
                tip.Lines.Add("");
                tip.Colors.Clear();
                tip.Colors.Add(default);
            }

            // concat multi-line tooltip, used in determining total display size
            string fulltext;
            fulltext = tip.Lines[0];
            for (int i = 1; i < tip.Lines.Count; i++)
            {
                fulltext = fulltext + "\n" + tip.Lines[i];
            }

            font.SetFont("font_regular");

            // calculate the full size to display a multi-line tooltip
            Int2 size = font.CalcSizeWrapped(fulltext, eset.Tooltips.Width - (eset.Tooltips.Margin * 2));

            // WARNING: dynamic memory allocation. Be careful of memory leaks.
            if (_spriteBuf != null)
            {
                _spriteBuf.Dispose();
                _spriteBuf = null;
            }

            Image? graphics;
            graphics = renderDevice.CreateImage(size.X + (eset.Tooltips.Margin * 2), size.Y + (eset.Tooltips.Margin * 2));

            if (graphics == null)
            {
                Utils.LogError("WidgetTooltip: Could not create tooltip buffer.");
                return false;
            }

            // style the tooltip background
            if (_background == null)
            {
                graphics.FillWithColor(new Color(0, 0, 0, 255));
            }
            else
            {
                Rectangle src = default;
                Rectangle dest = default;

                // top left
                src.X = 0;
                src.Y = 0;
                src.Width = graphics.GetWidth() - eset.Tooltips.BackgroundBorder;
                src.Height = graphics.GetHeight() - eset.Tooltips.BackgroundBorder;
                dest.X = 0;
                dest.Y = 0;
                renderDevice.RenderToImage(_background, src, graphics, dest);

                // right
                src.X = _background.GetWidth() - eset.Tooltips.BackgroundBorder;
                src.Y = 0;
                src.Width = eset.Tooltips.BackgroundBorder;
                src.Height = graphics.GetHeight() - eset.Tooltips.BackgroundBorder;
                dest.X = graphics.GetWidth() - eset.Tooltips.BackgroundBorder;
                dest.Y = 0;
                renderDevice.RenderToImage(_background, src, graphics, dest);

                // bottom
                src.X = 0;
                src.Y = _background.GetHeight() - eset.Tooltips.BackgroundBorder;
                src.Width = graphics.GetWidth() - eset.Tooltips.BackgroundBorder;
                src.Height = eset.Tooltips.BackgroundBorder;
                dest.X = 0;
                dest.Y = graphics.GetHeight() - eset.Tooltips.BackgroundBorder;
                renderDevice.RenderToImage(_background, src, graphics, dest);

                // bottom right
                src.X = _background.GetWidth() - eset.Tooltips.BackgroundBorder;
                src.Y = _background.GetHeight() - eset.Tooltips.BackgroundBorder;
                src.Width = eset.Tooltips.BackgroundBorder;
                src.Height = eset.Tooltips.BackgroundBorder;
                dest.X = graphics.GetWidth() - eset.Tooltips.BackgroundBorder;
                dest.Y = graphics.GetHeight() - eset.Tooltips.BackgroundBorder;
                renderDevice.RenderToImage(_background, src, graphics, dest);
            }

            int cursorY = eset.Tooltips.Margin;

            for (int i = 0; i < tip.Lines.Count; i++)
            {
                if (_background != null)
                    font.RenderShadowed(tip.Lines[i], eset.Tooltips.Margin, cursorY, FontEngine.JustifyLeft, graphics, size.X, tip.Colors[i]);
                else
                    font.Render(tip.Lines[i], eset.Tooltips.Margin, cursorY, FontEngine.JustifyLeft, graphics, size.X, tip.Colors[i], !FontEngine.ShadowOffset);

                cursorY = font.CursorY;
            }

            _spriteBuf = graphics.CreateSprite();
            graphics.Unref();

            // C++ 的 data_buf = tip 为值拷贝（复制 lines/colors 向量内容）；
            // TooltipData 在 C# 中为引用类型，此处逐成员复制以保持 Compare 语义等价。
            _dataBuf.Lines.Clear();
            _dataBuf.Lines.AddRange(tip.Lines);
            _dataBuf.Colors.Clear();
            _dataBuf.Colors.AddRange(tip.Colors);
            return true;
        }
    }
}
