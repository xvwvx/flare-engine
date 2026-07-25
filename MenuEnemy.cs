// <自动生成> 对应 C++ 源文件：MenuEnemy.h + MenuEnemy.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using System.Text;
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// MenuEnemy - 敌人 HUD 血条显示，对应 C++ <c>class MenuEnemy : public Menu</c>。
    /// 在敌人头顶渲染 HP 条和名称标签，通过 IDisposable 管理 Sprite/WidgetLabel。
    /// </summary>
    public class MenuEnemy : Menu, IDisposable
    {
        private Sprite? _barHp;
        private Rectangle _barPos;
        private LabelInfo _textPos;
        private bool _customTextPos;
        private Int2 _barFillOffset;
        private Int2 _barFillSize;
        private string _barGfx;

        private WidgetLabel _labelText;
        private WidgetLabel _labelStats;

        /// <summary>当前锁定的敌人实体引用，对应 C++ <c>enemy</c> 字段。</summary>
        public Entity? Enemy;

        /// <summary>敌人血条超时计时器，对应 C++ <c>timeout</c> 字段。</summary>
        public Timer Timeout;

        public MenuEnemy()
        {
            _barHp = null;
            _customTextPos = false;
            _barFillOffset = new Int2();
            _barFillSize = new Int2(-1, -1);
            _barGfx = "images/menus/enemy_bar_hp.png";
            Enemy = null;
            _labelText = new WidgetLabel();
            _labelStats = new WidgetLabel();
            Timeout = new Timer();

            // disappear after 10 seconds
            Timeout.Duration = (uint)(SharedResources.Settings!.MaxFramesPerSec * 10);

            // Load config settings
            using FileParser infile = new FileParser();
            // @CLASS MenuEnemy|Description of menus/enemy.txt
            if (infile.Open("menus/enemy.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (ParseMenuKey(infile.Key, infile.Val))
                        continue;

                    // @ATTR bar_pos|rectangle|Position and dimensions of the health bar.
                    if (infile.Key == "bar_pos")
                    {
                        _barPos = Parse.ToRect(infile.Val);
                    }
                    // @ATTR text_pos|label|Position of the text displaying the enemy's name and level.
                    else if (infile.Key == "text_pos")
                    {
                        _customTextPos = true;
                        _textPos = Parse.PopLabelInfo(infile.Val);
                    }
                    // @ATTR bar_fill_offset|point|Offset of the bar's fill graphics relative to the bar_pos X/Y.
                    else if (infile.Key == "bar_fill_offset")
                    {
                        _barFillOffset = Parse.ToPoint(infile.Val);
                    }
                    // @ATTR bar_fill_size|int, int : Width, Height|Size of the bar's fill graphics. If not defined, the width/height of bar_pos is used.
                    else if (infile.Key == "bar_fill_size")
                    {
                        _barFillSize = Parse.ToPoint(infile.Val);
                    }
                    // @ATTR bar_gfx|filename|Filename of the image to use for the "fill" of the bar.
                    else if (infile.Key == "bar_gfx")
                    {
                        _barGfx = infile.Val;
                    }
                    // @ATTR timeout|duration|The amount of time before the menu disappears when no enemy is targeted. Defaults to 10 seconds.
                    else if (infile.Key == "timeout")
                    {
                        Timeout.Duration = (uint)Parse.ToDuration(infile.Val);
                    }
                    else
                    {
                        infile.Error("MenuEnemy: '%s' is not a valid key.", infile.Key);
                    }
                }
                infile.Close();
            }

            // default to bar_pos size if bar_fill_size is undefined
            if (_barFillSize.X == -1 || _barFillSize.Y == -1)
            {
                _barFillSize.X = _barPos.Width;
                _barFillSize.Y = _barPos.Height;
            }

            LoadGraphics();

            Align();
        }

        public void LoadGraphics()
        {
            Image? graphics;

            if (_background == null)
                SetBackground("images/menus/enemy_bar.png");

            graphics = SharedResources.RenderDevice!.LoadImage(_barGfx, RenderDevice.ErrorNormal);
            if (graphics != null)
            {
                _barHp = graphics.CreateSprite();
                graphics.Unref();
            }
        }

        public void HandleNewMap()
        {
            Enemy = null;
        }

        public void Logic()
        {
            // after a fixed amount of time, hide the enemy display
            Timeout.Tick();
            if (Timeout.IsEnd())
                Enemy = null;

            if (Enemy != null && Enemy.Stats.Corpse && Enemy.Stats.CorpseTimer.IsEnd() && Enemy.Stats.CorpseHasTimeout)
                Enemy = null;
        }

        public override void Render()
        {
            if (Enemy == null) return;

            Rectangle src = default;
            Rectangle dest = default;
            src.X = 0;
            src.Y = 0;
            src.Width = _barPos.Width;
            src.Height = _barPos.Height;

            dest.X = WindowArea.X + _barPos.X;
            dest.Y = WindowArea.Y + _barPos.Y;
            dest.Width = _barPos.Width;
            dest.Height = _barPos.Height;

            int hpBarLength = 0;
            if (Enemy.Stats.Get(global::FlareEngine.Stats.HpMax) == 0)
                hpBarLength = 0;
            else if (_barHp != null)
            {
                hpBarLength = (int)((Enemy.Stats.Hp * (float)_barFillSize.X) / Enemy.Stats.Get(global::FlareEngine.Stats.HpMax));
                if (hpBarLength == 0 && Enemy.Stats.Hp > 0)
                    hpBarLength = 1;
            }

            // draw hp bar background
            SetBackgroundClip(src);
            SetBackgroundDest(dest);
            base.Render();

            // draw hp bar fill
            if (_barHp != null)
            {
                src.Width = hpBarLength;
                src.Height = _barFillSize.Y;

                dest.X += _barFillOffset.X;
                dest.Y += _barFillOffset.Y;

                _barHp.SetClipFromRect(src);
                _barHp.SetDestFromRect(dest);

                SharedResources.RenderDevice!.Render(_barHp);
            }

            if (!_textPos.Hidden)
            {
                // enemy name display
                _labelText.SetText(SharedResources.Msg!.GetV("%s level %d", Enemy.Stats.Name, Enemy.Stats.Level));
                _labelText.SetColor(SharedResources.Font!.GetColor(FontEngine.ColorMenuNormal));

                if (_customTextPos)
                {
                    _labelText.SetPos(WindowArea.X + _textPos.X, WindowArea.Y + _textPos.Y);
                    _labelText.SetJustify(_textPos.Justify);
                    _labelText.SetVAlign(_textPos.Valign);
                    _labelText.SetFont(_textPos.FontStyle);
                }
                else
                {
                    _labelText.SetPos(WindowArea.X + _barPos.X + _barPos.Width / 2, WindowArea.Y + _barPos.Y);
                    _labelText.SetJustify(FontEngine.JustifyCenter);
                    _labelText.SetVAlign(LabelInfo.ValignBottom);
                }
                _labelText.Render();

                // HP display
                StringBuilder ss = new StringBuilder();
                ss.Clear();
                if (Enemy.Stats.Hp > 0)
                {
                    ss.Append(Utils.FloatToString(Enemy.Stats.Hp, SharedResources.Eset!.NumberFormat.EnemyStatbar)).Append('/').Append(Utils.FloatToString(Enemy.Stats.Get(global::FlareEngine.Stats.HpMax), SharedResources.Eset!.NumberFormat.EnemyStatbar));
                }
                else
                {
                    if (Enemy.Stats.Lifeform)
                        ss.Append(SharedResources.Msg!.Get("Dead"));
                    else
                        ss.Append(SharedResources.Msg!.Get("Destroyed"));
                }
                _labelStats.SetText(ss.ToString());

                if (_barHp != null)
                {
                    // position bar text relative to bar fill if possible
                    _labelStats.SetPos(dest.X + _barFillSize.X / 2, dest.Y + _barFillSize.Y / 2);
                }
                else
                {
                    _labelStats.SetPos(WindowArea.X + _barPos.X + _barPos.Width / 2, WindowArea.Y + _barPos.Y + _barPos.Height / 2);
                }
                _labelStats.SetJustify(FontEngine.JustifyCenter);
                _labelStats.SetVAlign(LabelInfo.ValignCenter);
                _labelStats.Render();
            }
        }

        /// <summary>
        /// 对应 C++<c>~MenuEnemy()</c>。<c>bar_hp</c>。
        /// <summary>
        /// 释放 HP 条图像、文本标签和状态标签，最后调用 Menu.Dispose。
        /// </summary>
        /// </summary>
        public override void Dispose()
        {
            if (_barHp != null)
            {
                _barHp.Dispose();
                _barHp = null;
            }

            _labelStats.Dispose();
            _labelText.Dispose();

            base.Dispose();
        }
    }
}
