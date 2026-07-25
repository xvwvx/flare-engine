// <自动生成> 对应 C++ 源文件：WidgetLabel.h + WidgetLabel.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// LabelInfo：从配置文件解析出的标签位置/对齐信息（见 UtilsParsing.cs 的 Parse.PopLabelInfo）。
    /// </summary>
    public class LabelInfo
    {
        public const int ValignCenter = 0;
        public const int ValignTop = 1;
        public const int ValignBottom = 2;

        public int X;
        public int Y;
        public int Justify;
        public int Valign;
        public bool Hidden;
        public string FontStyle;

        public LabelInfo()
        {
            X = 0;
            Y = 0;
            Justify = FontEngine.JustifyLeft;
            Valign = ValignTop;
            Hidden = false;
            FontStyle = "font_regular";
        }
    }

    /// <summary>
    /// WidgetLabel
    ///
    /// 菜单中使用的简单文本显示控件。相比直接绘制文本，使用本控件有助于处理定位问题。
    /// C++ 原始类提供拷贝构造函数与自定义 operator=，用于表达"复制一份带缓存精灵的标签"这一值语义；
    /// C# 没有可重载的赋值运算符，改用显式的 CopyFrom 方法承载同样的深拷贝逻辑（见下方说明）。
    /// 持有的 Sprite 缓存资源通过 IDisposable 显式释放，对应原始析构函数中的 delete label。
    /// </summary>
    public class WidgetLabel : Widget, IDisposable
    {
        private const int UpdateNone = 0;
        private const int UpdatePos = 1;
        private const int UpdateRecache = 2;

        private int _justify;
        private int _valign;
        private int _maxWidth;
        private int _updateFlag;
        private bool _hidden;
        private bool _windowResizeFlag;
        private byte _alpha;
        private Sprite? _label;

        private string _text;
        private string _fontStyle;
        private Color _color;

        private Rectangle _bounds;

        public const string DefaultFont = "font_regular";

        public WidgetLabel()
        {
            _justify = FontEngine.JustifyLeft;
            _valign = LabelInfo.ValignTop;
            _maxWidth = 0;
            _updateFlag = UpdateNone;
            _hidden = false;
            _windowResizeFlag = false;
            _alpha = 255;
            _label = null;
            _text = "";
            _fontStyle = DefaultFont;
            _color = SharedResources.Font!.GetColor(FontEngine.ColorWidgetNormal);

            _bounds.X = _bounds.Y = 0;
            _bounds.Width = _bounds.Height = 0;
            EnableTablistNav = false;
        }

        /// <summary>
        /// 对应 C++ 的拷贝构造函数 <c>WidgetLabel(const WidgetLabel&amp;)</c>：
        /// 只将 _label 初始化为 null（跳过默认构造函数中查询字体颜色等逻辑），随后通过 CopyFrom
        /// 复制其余全部字段，与原始 <c>: label(NULL) { *this = other; }</c> 完全对应。
        /// </summary>
        public WidgetLabel(WidgetLabel other)
        {
            _label = null;
            _text = "";
            _fontStyle = "";
            CopyFrom(other);
        }

        /// <summary>
        /// 对应 C++ 的 <c>WidgetLabel&amp; operator=(const WidgetLabel&amp;)</c>。
        /// C# 不支持重载赋值运算符，调用方需要把原始的值拷贝 `a = b;` 改写为 `a.CopyFrom(b);`。
        /// </summary>
        public WidgetLabel CopyFrom(WidgetLabel other)
        {
            if (ReferenceEquals(this, other))
                return this;

            // Widget::operator=(other) —— 基类没有自定义赋值运算符，
            // 对应编译器生成的默认逐成员拷贝，这里显式列出全部基类字段。
            InFocus = other.InFocus;
            EnableTablistNav = other.EnableTablistNav;
            TablistNavAlign = other.TablistNavAlign;
            ScrollType = other.ScrollType;
            Pos = other.Pos;
            LocalFrame = other.LocalFrame;
            LocalOffset = other.LocalOffset;
            PosBase = other.PosBase;
            Alignment = other.Alignment;

            _justify = other._justify;
            _valign = other._valign;
            _maxWidth = other._maxWidth;
            _updateFlag = UpdateRecache;
            _hidden = other._hidden;
            _windowResizeFlag = other._windowResizeFlag;
            _alpha = other._alpha;
            _text = other._text;
            _fontStyle = other._fontStyle;
            _color = other._color;
            _bounds = other._bounds;

            _label?.Dispose();
            _label = null;
            Update();

            return this;
        }

        /// <summary>
        /// 对应 C++ 的 <c>~WidgetLabel()</c>，释放缓存的 Sprite。
        /// </summary>
        public void Dispose()
        {
            _label?.Dispose();
            _label = null;
            GC.SuppressFinalize(this);
        }

        public void SetMaxWidth(int width)
        {
            if (width != _maxWidth)
            {
                _maxWidth = width;
                SetUpdateFlag(UpdateRecache);
            }
        }

        public void SetHidden(bool hidden)
        {
            _hidden = hidden;
        }

        public override void SetPos(int offsetX, int offsetY)
        {
            Rectangle oldPos = Pos;
            base.SetPos(offsetX, offsetY);

            if (oldPos.X != Pos.X || oldPos.Y != Pos.Y)
            {
                SetUpdateFlag(UpdatePos);
            }
        }

        public void SetJustify(int justify)
        {
            if (_justify != justify)
            {
                _justify = justify;
                SetUpdateFlag(UpdateRecache);
            }
        }

        public void SetText(string text)
        {
            if (_text != text)
            {
                _text = text;
                SetUpdateFlag(UpdateRecache);
            }
        }

        public void SetVAlign(int valign)
        {
            if (_valign != valign)
            {
                _valign = valign;
                SetUpdateFlag(UpdateRecache);
            }
        }

        public void SetColor(Color color)
        {
            // 与原始代码一致：只比较 r/g/b，不比较 alpha 通道。
            if (_color.R != color.R || _color.G != color.G || _color.B != color.B)
            {
                _color = color;
                SetUpdateFlag(UpdateRecache);
            }
        }

        public void SetFont(string font)
        {
            if (_fontStyle != font)
            {
                _fontStyle = font;
                SetUpdateFlag(UpdateRecache);
            }
        }

        public void SetAlpha(byte alpha)
        {
            if (alpha != _alpha)
            {
                _alpha = alpha;
                SetUpdateFlag(UpdatePos);
            }
        }

        public void SetFromLabelInfo(LabelInfo labelInfo)
        {
            if (PosBase.X != labelInfo.X || PosBase.Y != labelInfo.Y)
                SetUpdateFlag(UpdatePos);

            SetBasePos(labelInfo.X, labelInfo.Y, Alignment);

            SetJustify(labelInfo.Justify);
            SetVAlign(labelInfo.Valign);
            SetFont(labelInfo.FontStyle);
            SetHidden(labelInfo.Hidden);
        }

        public string GetText()
        {
            return _text;
        }

        /// <summary>
        /// 获取标签的尺寸（必要时先重新缓存）。对应 C++ 中返回内部 bounds 成员指针的用法，
        /// C# 使用 ref return 精确复现"返回内部状态引用，允许调用方读取甚至修改"的语义。
        /// </summary>
        public ref Rectangle GetBounds()
        {
            Update();
            return ref _bounds;
        }

        public bool IsHidden()
        {
            return _hidden;
        }

        /// <summary>
        /// 将水平对齐(justify)与垂直对齐(valign)应用到标签位置上。
        /// </summary>
        private void ApplyOffsets()
        {
            // apply JUSTIFY
            if (_justify == FontEngine.JustifyLeft)
                _bounds.X = Pos.X;
            else if (_justify == FontEngine.JustifyRight)
                _bounds.X = Pos.X - _bounds.Width;
            else if (_justify == FontEngine.JustifyCenter)
                _bounds.X = Pos.X - _bounds.Width / 2;

            // apply VALIGN
            if (_valign == LabelInfo.ValignTop)
            {
                _bounds.Y = Pos.Y;
            }
            else if (_valign == LabelInfo.ValignBottom)
            {
                _bounds.Y = Pos.Y - _bounds.Height;
            }
            else if (_valign == LabelInfo.ValignCenter)
            {
                _bounds.Y = Pos.Y - _bounds.Height / 2;
            }

            if (_label != null)
            {
                _label.SetDestFromRect(_bounds);
                _label.AlphaMod = _alpha;
            }
        }

        /// <summary>
        /// 我们缓存渲染好的文本，而不是每帧都重新计算。此函数刷新该缓存。
        /// </summary>
        private void RecacheTextSprite()
        {
            if (_label != null)
            {
                _label.Dispose();
                _label = null;
            }

            if (string.IsNullOrEmpty(_text))
            {
                _bounds.Width = 0;
                _bounds.Height = 0;
                return;
            }

            string tempText = _text;

            SharedResources.Font!.SetFont(_fontStyle);

            Int2 p = SharedResources.Font.CalcSize(tempText);
            if (_maxWidth > 0 && p.X > _maxWidth)
            {
                tempText = SharedResources.Font.TrimTextToWidth(_text, _maxWidth, FontEngine.UseEllipsis, 0);
                p = SharedResources.Font.CalcSize(tempText);
            }

            _bounds.Width = p.X;
            _bounds.Height = Math.Max(p.Y, SharedResources.Font.GetFontHeight());

            Image? image = SharedResources.RenderDevice!.CreateImage(_bounds.Width, _bounds.Height);
            if (image == null) return;

            SharedResources.Font.RenderShadowed(tempText, 0, 0, FontEngine.JustifyLeft, image, 0, _color);
            _label = image.CreateSprite();
            image.Unref();
        }

        /// <summary>
        /// 设置一个标志位，供 Update() 判断有哪些内容发生了变化。
        /// </summary>
        private void SetUpdateFlag(int updateFlag)
        {
            if (updateFlag > _updateFlag || updateFlag == UpdateNone)
                _updateFlag = updateFlag;
        }

        /// <summary>
        /// 根据发生的变化，按需运行 RecacheTextSprite() 与 ApplyOffsets()。
        /// </summary>
        private void Update()
        {
            // we only need to check if the window was resized once per frame
            // yet, this update function may be called multiple times (ex. GetBounds())
            // so we set a flag to prevent unnecessary re-caching
            if (SharedResources.Inpt!.WindowResized && !_windowResizeFlag)
            {
                SetUpdateFlag(UpdateRecache);
                _windowResizeFlag = true;
            }

            if (_updateFlag == UpdateRecache)
                RecacheTextSprite();

            if (_updateFlag >= UpdatePos)
                ApplyOffsets();

            SetUpdateFlag(UpdateNone);
        }

        /// <summary>
        /// 将缓存好的字符串位图绘制到屏幕上。
        /// </summary>
        public override void Render()
        {
            if (_hidden)
                return;

            Update();

            if (_label != null)
            {
                _label.LocalFrame = LocalFrame;
                _label.SetOffset(LocalOffset);
                SharedResources.RenderDevice!.Render(_label);
            }

            // reset flag
            _windowResizeFlag = false;
        }
    }
}
