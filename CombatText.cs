// <自动生成> 对应 C++ 源文件：CombatText.h + CombatText.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// CombatTextItem（对应 C++ 的 <c>class Combat_Text_Item</c>）
    ///
    /// 单条浮空战斗文字（伤害数字/未命中提示等）的数据。C++ 原始版本用
    /// <c>std::vector&lt;Combat_Text_Item&gt;</c> 按值存放，但 CombatText 的成员函数通过
    /// 迭代器对元素进行原地修改（如 <c>it-&gt;lifespan--</c>），且 Label 是需要跨迭代器
    /// 共享同一份、由 CombatText 手动 new/delete 的裸指针资源。为了在 C# 中保留这种
    /// "通过迭代器/索引拿到的是同一份可变数据"的引用语义，这里使用引用类型 class
    /// （而非 struct），List&lt;CombatTextItem&gt; 中的每一项都是同一个对象的共享引用，
    /// 与原始 vector 元素被迭代器直接修改的效果一致。
    /// </summary>
    public class CombatTextItem
    {
        public WidgetLabel? Label;
        public int Lifespan;
        public Vector2 Pos;
        public float FloatingOffset;
        public string Text;
        public int DisplayType;
        public bool IsNumber;
        public float NumberValue;

        public CombatTextItem()
        {
            Label = null;
            Lifespan = 0;
            Pos = new Vector2();
            FloatingOffset = 0;
            Text = "";
            DisplayType = 0;
            IsNumber = false;
            NumberValue = 0;
        }

        // 对应 C++ 的 ~Combat_Text_Item()：函数体为空，Label 指向的 WidgetLabel
        // 由 CombatText 类统一负责释放（在其 Dispose()/Clear()/Logic() 中显式 Dispose()），
        // 此类自身不持有需要释放的资源，因此无需对应的 C# 方法。
    }

    /// <summary>
    /// CombatText
    ///
    /// 在目标上方显示浮空伤害数字与未命中提示文字。
    /// C++ 原始类通过 new/delete 手动管理每条提示所持有的 WidgetLabel* 裸指针，
    /// 生命周期与 std::vector 中对应元素的增删完全同步（单一所有者，语义上等价于
    /// unique_ptr）；C# 版本改为持有 WidgetLabel（实现 IDisposable），在每个原始
    /// delete 调用点显式调用 Dispose()，并让 CombatText 自身实现 IDisposable，
    /// 对应原始析构函数的清理逻辑与释放顺序（从 vector 头部到尾部逐条释放）。
    /// </summary>
    public class CombatText : IDisposable
    {
        public const int MsgGivedmg = 0;
        public const int MsgTakedmg = 1;
        public const int MsgCrit = 2;
        public const int MsgMiss = 3;
        public const int MsgBuff = 4;

        private Vector2 _cam;
        private readonly List<CombatTextItem> _combatText = new List<CombatTextItem>();

        private readonly Color[] _msgColor = new Color[5];
        private int _duration;
        private int _fadeDuration;
        private float _speed;
        private int _offset;
        private string _fontId;

        public CombatText()
        {
            _msgColor[MsgGivedmg] = SharedResources.Font!.GetColor(FontEngine.ColorCombatGivedmg);
            _msgColor[MsgTakedmg] = SharedResources.Font!.GetColor(FontEngine.ColorCombatTakedmg);
            _msgColor[MsgCrit] = SharedResources.Font!.GetColor(FontEngine.ColorCombatCrit);
            _msgColor[MsgBuff] = SharedResources.Font!.GetColor(FontEngine.ColorCombatBuff);
            _msgColor[MsgMiss] = SharedResources.Font!.GetColor(FontEngine.ColorCombatMiss);

            _duration = SharedResources.Settings!.MaxFramesPerSec; // 1 second
            _fadeDuration = 0;
            _speed = Settings.LogicFps / SharedResources.Settings!.MaxFramesPerSec;
            _offset = 48; // average height of flare-game enemies, so a sensible default
            _fontId = "font_regular";

            // Load config settings
            using FileParser infile = new FileParser();
            // @CLASS CombatText|Description of engine/combat_text.txt
            if (infile.Open("engine/combat_text.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (infile.Key == "duration")
                    {
                        // @ATTR duration|duration|Duration of the combat text in 'ms' or 's'.
                        _duration = Parse.ToDuration(infile.Val);
                    }
                    else if (infile.Key == "speed")
                    {
                        // @ATTR speed|float|Motion speed of the combat text.
                        _speed = (Parse.ToFloat(infile.Val) * Settings.LogicFps) / SharedResources.Settings!.MaxFramesPerSec;
                    }
                    else if (infile.Key == "offset")
                    {
                        // @ATTR offset|int|The vertical offset for the combat text's starting position.
                        _offset = Parse.ToInt(infile.Val);
                    }
                    else if (infile.Key == "fade_duration")
                    {
                        // @ATTR fade_duration|duration|How long the combat text will spend fading out in 'ms' or 's'.
                        _fadeDuration = Parse.ToDuration(infile.Val);
                    }
                    else if (infile.Key == "font")
                    {
                        // @ATTR font|predefined_string|The font to use for combat text.
                        _fontId = infile.Val;
                    }
                    else
                    {
                        infile.Error("CombatText: '%s' is not a valid key.", infile.Key);
                    }
                }
                infile.Close();
            }

            if (_fadeDuration > _duration)
                _fadeDuration = _duration;
        }

        /// <summary>
        /// 对应 C++ 的 <c>~CombatText()</c>：记录清理日志，并从头到尾依次释放每条
        /// 提示持有的 WidgetLabel，释放顺序与原始 <c>while (combat_text.size() > 0)</c>
        /// 循环（始终从 <c>combat_text.begin()</c> 删除）完全一致。
        /// </summary>
        public void Dispose()
        {
            Utils.LogInfo("Cleaning up: CombatText");

            // delete all messages
            while (_combatText.Count > 0)
            {
                _combatText[0].Label?.Dispose();
                _combatText.RemoveAt(0);
            }

            GC.SuppressFinalize(this);
        }

        public void AddString(string message, Vector2 location, int displaytype)
        {
            if (!SharedResources.Settings!.CombatText)
                return;

            // reduce spam of identical messages
            foreach (CombatTextItem it in _combatText)
            {
                if (!it.IsNumber && it.DisplayType == displaytype && it.Lifespan <= _duration && it.Lifespan >= _duration / 2 && it.Pos.X == location.X && it.Pos.Y == location.Y && message == it.Text)
                {
                    return;
                }
            }

            CombatTextItem c = new CombatTextItem();
            c.Pos.X = location.X;
            c.Pos.Y = location.Y;
            c.FloatingOffset = (float)_offset;
            c.Text = message;
            c.Lifespan = _duration;
            c.DisplayType = displaytype;

            c.Label = new WidgetLabel();
            c.Label.SetPos((int)c.Pos.X, (int)c.Pos.Y);
            c.Label.SetJustify(FontEngine.JustifyCenter);
            c.Label.SetVAlign(LabelInfo.ValignBottom);
            c.Label.SetFont(_fontId);
            c.Label.SetText(c.Text);
            c.Label.SetColor(_msgColor[c.DisplayType]);
            _combatText.Add(c);
        }

        public void AddFloat(float num, Vector2 location, int displaytype)
        {
            if (!SharedResources.Settings!.CombatText)
                return;

            // when adding multiple combat text of the same type and position on the same frame, add the num to the existing text
            foreach (CombatTextItem it in _combatText)
            {
                if (it.IsNumber && it.DisplayType == displaytype && it.Lifespan == _duration && it.Pos.X == location.X && it.Pos.Y == location.Y)
                {
                    it.NumberValue += num;
                    it.Text = Utils.FloatToString(it.NumberValue, SharedResources.Eset!.NumberFormat.CombatText);
                    it.Label!.SetText(it.Text);
                    return;
                }
            }

            AddString(Utils.FloatToString(num, SharedResources.Eset!.NumberFormat.CombatText), location, displaytype);

            _combatText[^1].IsNumber = true;
            _combatText[^1].NumberValue = num;
        }

        public void Logic(Vector2 cam)
        {
            _cam = cam;

            for (int it = _combatText.Count; it != 0;)
            {
                --it;

                CombatTextItem item = _combatText[it];

                item.Lifespan--;
                item.FloatingOffset += _speed;

                Int2 scrPos;
                scrPos = Utils.MapToScreen(item.Pos.X, item.Pos.Y, _cam.X, _cam.Y);
                scrPos.Y -= (int)item.FloatingOffset;

                item.Label!.SetPos(scrPos.X, scrPos.Y);

                // try to prevent messages from overlapping
                for (int overlapIt = it; overlapIt != 0;)
                {
                    --overlapIt;

                    CombatTextItem overlapItem = _combatText[overlapIt];

                    Rectangle bounds = item.Label.GetBounds();
                    Rectangle overlapBounds = overlapItem.Label!.GetBounds();
                    if (Utils.RectsOverlap(bounds, overlapBounds))
                    {
                        overlapItem.FloatingOffset += (float)(overlapBounds.Height + (overlapBounds.Y - bounds.Y));

                        scrPos = Utils.MapToScreen(overlapItem.Pos.X, overlapItem.Pos.Y, _cam.X, _cam.Y);
                        scrPos.Y -= (int)overlapItem.FloatingOffset;

                        overlapItem.Label.SetPos(scrPos.X, scrPos.Y);
                    }
                }
            }

            // delete expired messages
            while (_combatText.Count > 0 && _combatText[0].Lifespan <= 0)
            {
                _combatText[0].Label?.Dispose();
                _combatText.RemoveAt(0);
            }
        }

        public void Render()
        {
            if (!SharedResources.Settings!.ShowHud) return;

            foreach (CombatTextItem it in _combatText)
            {
                if (it.Lifespan > 0)
                {
                    // fade out
                    if (it.Lifespan < _fadeDuration)
                        it.Label!.SetAlpha((byte)(((float)it.Lifespan / (float)_fadeDuration) * 255f));

                    it.Label!.Render();
                }
            }
        }

        public void Clear()
        {
            while (_combatText.Count > 0)
            {
                _combatText[0].Label?.Dispose();
                _combatText.RemoveAt(0);
            }
        }
    }
}
