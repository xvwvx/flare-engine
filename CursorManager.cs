// <自动生成> 对应 C++ 源文件：CursorManager.h + CursorManager.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// CursorManager
    ///
    /// 管理软件模拟鼠标光标（非系统硬件光标）的加载、状态切换与渲染。
    /// 依据 engine/mouse_cursor.txt 配置文件加载"普通/交互/对话/攻击"四种光标的正常与
    /// 低血量（low hp）变体精灵图及其偏移量，并在每帧根据交互状态选择当前应显示的
    /// 光标精灵与偏移量，在硬件光标关闭时由本类负责在鼠标位置处绘制对应精灵。
    ///
    /// C++ 原始实现使用裸指针 <c>Sprite*</c> 持有精灵资源，在析构函数中手动 delete；
    /// C# 版本实现 <see cref="IDisposable"/>，在 <see cref="Dispose"/> 中按原始析构函数
    /// 的逐行顺序释放（详见转换报告）。
    /// </summary>
    public class CursorManager : IDisposable
    {
        // C++: enum { CURSOR_NORMAL, CURSOR_INTERACT, CURSOR_TALK, CURSOR_ATTACK };
        public const int CursorNormal = 0;
        public const int CursorInteract = 1;
        public const int CursorTalk = 2;
        public const int CursorAttack = 3;

        // C++: bool show_cursor;（公有字段，无 getter/setter 包装，原样保留为公有字段）
        public bool ShowCursor;

        // C++: void setLowHP(bool val)（仅有 setter，无对应公有 getter，low_hp 只在类内部读取）。
        // 属性化为"外部可写、内部可读"的 { private get; set; }，与原始封装语义完全对应
        // （外部只能通过 SetLowHp 写入，内部方法直接读取字段值）。
        public bool LowHp { private get; set; }

        private Sprite? _cursorNormal;
        private Sprite? _cursorInteract;
        private Sprite? _cursorTalk;
        private Sprite? _cursorAttack;
        private Sprite? _cursorLhpNormal;
        private Sprite? _cursorLhpInteract;
        private Sprite? _cursorLhpTalk;
        private Sprite? _cursorLhpAttack;

        private Int2 _offsetNormal;
        private Int2 _offsetInteract;
        private Int2 _offsetTalk;
        private Int2 _offsetAttack;
        private Int2 _offsetLhpNormal;
        private Int2 _offsetLhpInteract;
        private Int2 _offsetLhpTalk;
        private Int2 _offsetLhpAttack;

        private Sprite? _cursorCurrent;
        // C++: Point* offset_current;（指向 offset_xxx 成员之一的指针，或 NULL）。
        // offset_xxx 字段一旦在构造函数中被解析赋值后就不再被修改，因此用 Int2?（可空值类型）
        // 保存"当前偏移量的副本"与用指针指向该字段读取到的值完全等价（详见转换报告）。
        private Int2? _offsetCurrent;

        public CursorManager()
        {
            ShowCursor = true;
            _cursorNormal = null;
            _cursorInteract = null;
            _cursorTalk = null;
            _cursorAttack = null;
            _cursorLhpNormal = null;
            _cursorLhpInteract = null;
            _cursorLhpTalk = null;
            _cursorLhpAttack = null;
            _cursorCurrent = null;
            _offsetCurrent = null;
            LowHp = false;

            Image? graphics;
            using FileParser infile = new FileParser();
            // @CLASS CursorManager|Description of engine/mouse_cursor.txt
            if (infile.Open("engine/mouse_cursor.txt", FileParser.ModFile, FileParser.ErrorNone))
            {
                while (infile.Next())
                {
                    if (infile.Key == "normal")
                    {
                        // @ATTR normal|filename|Filename of an image for the normal cursor.
                        graphics = SharedResources.RenderDevice!.LoadImage(Parse.PopFirstString(ref infile.Val), RenderDevice.ErrorNormal);
                        if (graphics != null)
                        {
                            _cursorNormal = graphics.CreateSprite();
                            graphics.Unref();
                        }
                        _offsetNormal = Parse.ToPoint(infile.Val);
                    }
                    else if (infile.Key == "interact")
                    {
                        // @ATTR interact|filename|Filename of an image for the object interaction cursor.
                        graphics = SharedResources.RenderDevice!.LoadImage(Parse.PopFirstString(ref infile.Val), RenderDevice.ErrorNormal);
                        if (graphics != null)
                        {
                            _cursorInteract = graphics.CreateSprite();
                            graphics.Unref();
                        }
                        _offsetInteract = Parse.ToPoint(infile.Val);
                    }
                    else if (infile.Key == "talk")
                    {
                        // @ATTR talk|filename|Filename of an image for the NPC interaction cursor.
                        graphics = SharedResources.RenderDevice!.LoadImage(Parse.PopFirstString(ref infile.Val), RenderDevice.ErrorNormal);
                        if (graphics != null)
                        {
                            _cursorTalk = graphics.CreateSprite();
                            graphics.Unref();
                        }
                        _offsetTalk = Parse.ToPoint(infile.Val);
                    }
                    else if (infile.Key == "attack")
                    {
                        // @ATTR attack|filename|Filename of an image for the cursor when attacking enemies.
                        graphics = SharedResources.RenderDevice!.LoadImage(Parse.PopFirstString(ref infile.Val), RenderDevice.ErrorNormal);
                        if (graphics != null)
                        {
                            _cursorAttack = graphics.CreateSprite();
                            graphics.Unref();
                        }
                        _offsetAttack = Parse.ToPoint(infile.Val);
                    }
                    else if (infile.Key == "lowhp_normal")
                    {
                        // @ATTR lowhp_normal|filename|Filename of an image for the normal cursor when health is low.
                        graphics = SharedResources.RenderDevice!.LoadImage(Parse.PopFirstString(ref infile.Val), RenderDevice.ErrorNormal);
                        if (graphics != null)
                        {
                            _cursorLhpNormal = graphics.CreateSprite();
                            graphics.Unref();
                        }
                        _offsetLhpNormal = Parse.ToPoint(infile.Val);
                    }
                    else if (infile.Key == "lowhp_interact")
                    {
                        // @ATTR lowhp_interact|filename|Filename of an image for the object interaction cursor when health is low.
                        graphics = SharedResources.RenderDevice!.LoadImage(Parse.PopFirstString(ref infile.Val), RenderDevice.ErrorNormal);
                        if (graphics != null)
                        {
                            _cursorLhpInteract = graphics.CreateSprite();
                            graphics.Unref();
                        }
                        _offsetLhpInteract = Parse.ToPoint(infile.Val);
                    }
                    else if (infile.Key == "lowhp_talk")
                    {
                        // @ATTR lowhp_talk|filename|Filename of an image for the NPC interaction cursor when health is low.
                        graphics = SharedResources.RenderDevice!.LoadImage(Parse.PopFirstString(ref infile.Val), RenderDevice.ErrorNormal);
                        if (graphics != null)
                        {
                            _cursorLhpTalk = graphics.CreateSprite();
                            graphics.Unref();
                        }
                        _offsetLhpTalk = Parse.ToPoint(infile.Val);
                    }
                    else if (infile.Key == "lowhp_attack")
                    {
                        // @ATTR lowhp_attack|filename|Filename of an image for the cursor when attacking enemies and health is low.
                        graphics = SharedResources.RenderDevice!.LoadImage(Parse.PopFirstString(ref infile.Val), RenderDevice.ErrorNormal);
                        if (graphics != null)
                        {
                            _cursorLhpAttack = graphics.CreateSprite();
                            graphics.Unref();
                        }
                        _offsetLhpAttack = Parse.ToPoint(infile.Val);
                    }
                    else
                    {
                        infile.Error("CursorManager: '%s' is not a valid key.", infile.Key);
                    }
                }
                infile.Close();
            }
        }

        /// <summary>
        /// 对应 C++ 的 <c>~CursorManager()</c>：按原始析构函数逐行顺序释放各精灵资源。
        /// </summary>
        public void Dispose()
        {
            Utils.LogInfo("Cleaning up: CursorManager");

            _cursorNormal?.Dispose();
            _cursorInteract?.Dispose();
            _cursorTalk?.Dispose();
            _cursorAttack?.Dispose();
            _cursorLhpNormal?.Dispose();
            _cursorLhpInteract?.Dispose();
            _cursorLhpTalk?.Dispose();
            _cursorLhpAttack?.Dispose();

            GC.SuppressFinalize(this);
        }

        public void Logic()
        {
            if (!ShowCursor)
            {
                SharedResources.Inpt!.HideCursor();
                return;
            }

            if (SharedResources.Settings!.HardwareCursor)
            {
                SharedResources.Inpt!.ShowCursor();
                return;
            }

            _cursorCurrent = null;
            _offsetCurrent = null;

            SetCursor(CursorNormal);
        }

        public void Render()
        {
            if (SharedResources.Settings!.HardwareCursor || !ShowCursor) return;

            if (_cursorCurrent != null)
            {
                if (_offsetCurrent != null)
                {
                    _cursorCurrent.SetDest(SharedResources.Inpt!.Mouse.X + _offsetCurrent.Value.X, SharedResources.Inpt.Mouse.Y + _offsetCurrent.Value.Y);
                }
                else
                {
                    _cursorCurrent.SetDest(SharedResources.Inpt!.Mouse.X, SharedResources.Inpt.Mouse.Y);
                }

                SharedResources.RenderDevice!.Render(_cursorCurrent);
            }
        }

        public void SetCursor(int type)
        {
            if (SharedResources.Settings!.HardwareCursor) return;

            if (type == CursorInteract && (_cursorInteract != null || (_cursorLhpInteract != null && LowHp)))
            {
                SharedResources.Inpt!.HideCursor();
                if (LowHp && _cursorLhpInteract != null)
                {
                    _cursorCurrent = _cursorLhpInteract;
                    _offsetCurrent = _offsetLhpInteract;
                }
                else if (_cursorInteract != null)
                {
                    _cursorCurrent = _cursorInteract;
                    _offsetCurrent = _offsetInteract;
                }
            }
            else if (type == CursorTalk && (_cursorTalk != null || (_cursorLhpTalk != null && LowHp)))
            {
                SharedResources.Inpt!.HideCursor();
                if (LowHp && _cursorLhpTalk != null)
                {
                    _cursorCurrent = _cursorLhpTalk;
                    _offsetCurrent = _offsetLhpTalk;
                }
                else if (_cursorTalk != null)
                {
                    _cursorCurrent = _cursorTalk;
                    _offsetCurrent = _offsetTalk;
                }
            }
            else if (type == CursorAttack && (_cursorAttack != null || (_cursorLhpAttack != null && LowHp)))
            {
                SharedResources.Inpt!.HideCursor();
                if (LowHp && _cursorLhpAttack != null)
                {
                    _cursorCurrent = _cursorLhpAttack;
                    _offsetCurrent = _offsetLhpAttack;
                }
                else if (_cursorAttack != null)
                {
                    _cursorCurrent = _cursorAttack;
                    _offsetCurrent = _offsetAttack;
                }
            }
            else if (_cursorNormal != null || (_cursorLhpNormal != null && LowHp))
            {
                SharedResources.Inpt!.HideCursor();
                if (LowHp && _cursorLhpNormal != null)
                {
                    _cursorCurrent = _cursorLhpNormal;
                    _offsetCurrent = _offsetLhpNormal;
                }
                else if (_cursorNormal != null)
                {
                    _cursorCurrent = _cursorNormal;
                    _offsetCurrent = _offsetNormal;
                }
            }
            else
            {
                // system cursor
                _cursorCurrent = null;
                SharedResources.Inpt!.ShowCursor();
            }
        }
    }
}
