// <自动生成> 对应 C++ 源文件：MenuCharacter.h + MenuCharacter.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using System.Text;
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// MenuCharacter - 角色属性菜单，对应 C++ <c>class MenuCharacter : public Menu</c>。
    /// 显示玩家属性、分配属性点，通过 IDisposable 管理子控件。
    /// </summary>
    public class MenuCharacter : Menu, IDisposable
    {
        /// <summary>??<c>CharStat</c>。/summary>
        private class CharStat
        {
            public WidgetLabel? Label;
            public WidgetLabel? Value;
            public Rectangle Hover;
            public TooltipData Tip = new TooltipData();
            public Rectangle ValuePos;

            public void SetHover(int x, int y)
            {
                Hover.X = x + ValuePos.X;
                Hover.Y = y + ValuePos.Y;
                Hover.Width = ValuePos.Width;
                Hover.Height = ValuePos.Height;
            }
        }

        /// <summary>?? <c>CSTAT_NAME</c>。/summary>
        private const int CStatName = 0;

        /// <summary>?? <c>CSTAT_LEVEL</c>。/summary>
        private const int CStatLevel = 1;

        private WidgetButton? _closeButton;
        private WidgetLabel? _labelCharacter;
        private WidgetLabel? _labelUnspent;
        private WidgetListBox? _statList;
        private List<WidgetButton?> _upgradeButton = new List<WidgetButton?>();

        private int _skillPoints;
        private List<bool> _primaryUp = new List<bool>();

        private Int2 _statlistPos;
        private int _statlistRows;
        private int _statlistScrollbarOffset;
        private List<bool> _showStat = new List<bool>();
        private bool _showResists;

        private List<CharStat> _cstat = new List<CharStat>();

        private List<int> _baseStats = new List<int>();
        private List<int> _baseStatsAdd = new List<int>();
        private List<List<float>?> _baseBonus = new List<List<float>?>();

        private int _nameMaxWidth;

        public MenuCharacter()
        {
            _closeButton = new WidgetButton(WidgetButton.CloseFile);
            _labelCharacter = new WidgetLabel();
            _labelUnspent = new WidgetLabel();
            _skillPoints = 0;
            _statlistRows = 10;
            _statlistScrollbarOffset = 0;
            _showResists = true;
            _nameMaxWidth = 0;

            var msg = SharedResources.Msg!;
            var eset = SharedResources.Eset!;
            var font = SharedResources.Font!;
            var pc = SharedGameResources.Pc!;

            _labelCharacter.SetText(msg.Get("Character"));
            _labelCharacter.SetColor(font.GetColor(FontEngine.ColorMenuNormal));
            _labelUnspent.SetColor(font.GetColor(FontEngine.ColorMenuBonus));

            // 2 is added here to account for CSTAT_NAME and CSTAT_LEVEL
            while (_cstat.Count < eset.PrimaryStats.Stats.Count + 2)
            {
                _cstat.Add(new CharStat());
            }

            // Labels for major stats
            for (int i = 0; i < _cstat.Count; ++i)
            {
                _cstat[i].Label = new WidgetLabel();
                _cstat[i].Value = new WidgetLabel();
                _cstat[i].Hover.X = _cstat[i].Hover.Y = 0;
                _cstat[i].Hover.Width = _cstat[i].Hover.Height = 0;

                _cstat[i].Label!.SetColor(font.GetColor(FontEngine.ColorMenuNormal));

                _cstat[i].Value!.SetVAlign(LabelInfo.ValignCenter);
                _cstat[i].Value!.SetColor(font.GetColor(FontEngine.ColorMenuNormal));
            }
            _cstat[CStatName].Label!.SetText(msg.Get("Name"));
            _cstat[CStatLevel].Label!.SetText(msg.Get("Level"));
            for (int i = 0; i < eset.PrimaryStats.Stats.Count; ++i)
            {
                _cstat[i + 2].Label!.SetText(eset.PrimaryStats.Stats[i].Name);
            }
            _cstat[CStatName].Label!.SetText(msg.Get("Name"));
            _cstat[CStatLevel].Label!.SetText(msg.Get("Level"));
            for (int i = 0; i < eset.PrimaryStats.Stats.Count; ++i)
            {
                _cstat[i + 2].Label!.SetText(eset.PrimaryStats.Stats[i].Name);
            }

            _showStat = new List<bool>(new bool[Stats.Count + eset.DamageTypes.Count + eset.ResourceStats.StatCountValue + 2]);
            for (int i = 0; i < _showStat.Count; i++)
            {
                if (i >= Stats.ResistDamageOverTime && i < Stats.Count)
                {
                    // some stats are hidden by default
                    _showStat[i] = false;
                }
                else if (i >= Stats.Count && i < Stats.Count + eset.DamageTypes.Count)
                {
                    int damageSubIndex = i - Stats.Count;
                    int damageIndex = damageSubIndex / 3;
                    int damageSubStat = damageSubIndex % 3;

                    if (eset.DamageTypes.Types[damageIndex].IsDeprecatedElement && damageSubStat != 2)
                    {
                        // don't show damage for elements loaded from engine/elements.txt by default
                        _showStat[i] = false;
                    }
                    else if (!eset.DamageTypes.Types[damageIndex].IsElemental && damageSubStat == 2)
                    {
                        // don't show resists for non-elemental damage types by default
                        _showStat[i] = false;
                    }
                    else
                    {
                        _showStat[i] = true;
                    }
                }
                else
                {
                    _showStat[i] = true;
                }
            }

            // Upgrade buttons
            _primaryUp = new List<bool>(new bool[eset.PrimaryStats.Stats.Count]);
            _upgradeButton = new List<WidgetButton?>(new WidgetButton?[eset.PrimaryStats.Stats.Count]);

            for (int i = 0; i < eset.PrimaryStats.Stats.Count; ++i)
            {
                _primaryUp[i] = false;
                _upgradeButton[i] = new WidgetButton(WidgetButton.UpgradeStatFile);
                _upgradeButton[i]!.Enabled = false;
            }

            // Load config settings
            using FileParser infile = new FileParser();
            // @CLASS MenuCharacter|Description of menus/character.txt
            if (infile.Open("menus/character.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (ParseMenuKey(infile.Key, infile.Val))
                        continue;

                    // @ATTR close|point|Position of the close button.
                    if (infile.Key == "close")
                    {
                        Int2 pos = Parse.ToPoint(infile.Val);
                        _closeButton!.SetBasePos(pos.X, pos.Y, Utils.AlignTopLeft);
                    }
                    // @ATTR label_title|label|Position of the "Character" text.
                    else if (infile.Key == "label_title") _labelCharacter!.SetFromLabelInfo(Parse.PopLabelInfo(infile.Val));
                    // @ATTR upgrade_primary|predefined_string, point : Primary stat name, Button position|Position of the button used to add a stat point to this primary stat.
                    else if (infile.Key == "upgrade_primary")
                    {
                        string val = infile.Val;
                        string primStat = Parse.PopFirstString(ref val);
                        int primStatIndex = eset.PrimaryStats.GetIndexByID(primStat);

                        if (primStatIndex != eset.PrimaryStats.Stats.Count)
                        {
                            Int2 pos = Parse.ToPoint(val);
                            _upgradeButton[primStatIndex]!.SetBasePos(pos.X, pos.Y, Utils.AlignTopLeft);
                        }
                        else
                        {
                            infile.Error("MenuCharacter: '%s' is not a valid primary stat.", primStat);
                        }
                    }
                    // @ATTR statlist|point|Position of the scrollbox containing non-primary stats.
                    else if (infile.Key == "statlist") _statlistPos = Parse.ToPoint(infile.Val);
                    // @ATTR statlist_rows|int|The height of the statlist in rows.
                    else if (infile.Key == "statlist_rows") _statlistRows = Parse.ToInt(infile.Val);
                    // @ATTR statlist_scrollbar_offset|int|Right margin in pixels for the statlist's scrollbar.
                    else if (infile.Key == "statlist_scrollbar_offset") _statlistScrollbarOffset = Parse.ToInt(infile.Val);

                    // @ATTR label_name|label|Position of the "Name" text.
                    else if (infile.Key == "label_name")
                    {
                        _cstat[CStatName].Label!.SetFromLabelInfo(Parse.PopLabelInfo(infile.Val));
                    }
                    // @ATTR label_level|label|Position of the "Level" text.
                    else if (infile.Key == "label_level")
                    {
                        _cstat[CStatLevel].Label!.SetFromLabelInfo(Parse.PopLabelInfo(infile.Val));
                    }
                    // @ATTR label_primary|predefined_string, label : Primary stat name, Text positioning|Position of the text label for this primary stat.
                    else if (infile.Key == "label_primary")
                    {
                        string val = infile.Val;
                        string primStat = Parse.PopFirstString(ref val);
                        int primStatIndex = eset.PrimaryStats.GetIndexByID(primStat);

                        if (primStatIndex != eset.PrimaryStats.Stats.Count)
                        {
                            _cstat[primStatIndex + 2].Label!.SetFromLabelInfo(Parse.PopLabelInfo(val));
                        }
                        else
                        {
                            infile.Error("MenuCharacter: '%s' is not a valid primary stat.", primStat);
                        }
                    }

                    // @ATTR name|rectangle|Position of the player's name and dimensions of the tooltip hotspot.
                    else if (infile.Key == "name")
                    {
                        _cstat[CStatName].ValuePos = Parse.ToRect(infile.Val);
                        _cstat[CStatName].Value!.SetBasePos(_cstat[CStatName].ValuePos.X, _cstat[CStatName].ValuePos.Y + (_cstat[CStatName].ValuePos.Height / 2), Utils.AlignTopLeft);
                    }
                    // @ATTR level|rectangle|Position of the player's level and dimensions of the tooltip hotspot.
                    else if (infile.Key == "level")
                    {
                        _cstat[CStatLevel].ValuePos = Parse.ToRect(infile.Val);
                        _cstat[CStatLevel].Value!.SetBasePos(_cstat[CStatLevel].ValuePos.X + (_cstat[CStatLevel].ValuePos.Width / 2), _cstat[CStatLevel].ValuePos.Y + (_cstat[CStatLevel].ValuePos.Height / 2), Utils.AlignTopLeft);
                    }
                    // @ATTR primary|predefined_string, rectangle : Primary stat name, Hotspot position|Position of this primary stat value display and dimensions of its tooltip hotspot.
                    else if (infile.Key == "primary")
                    {
                        string val = infile.Val;
                        string primStat = Parse.PopFirstString(ref val);
                        int primStatIndex = eset.PrimaryStats.GetIndexByID(primStat);

                        if (primStatIndex != eset.PrimaryStats.Stats.Count)
                        {
                            Rectangle r = Parse.ToRect(val);
                            _cstat[primStatIndex + 2].ValuePos = r;
                            _cstat[primStatIndex + 2].Value!.SetBasePos(r.X + (r.Width / 2), r.Y + (r.Height / 2), Utils.AlignTopLeft);
                        }
                        else
                        {
                            infile.Error("MenuCharacter: '%s' is not a valid primary stat.", primStat);
                        }
                    }

                    // @ATTR unspent|label|Position of the label showing the number of unspent stat points.
                    else if (infile.Key == "unspent") _labelUnspent!.SetFromLabelInfo(Parse.PopLabelInfo(infile.Val));

                    // @ATTR show_resists|bool|Hide the "Resist Damage" stats in the statlist if set to false.
                    else if (infile.Key == "show_resists") _showResists = Parse.ToBool(infile.Val);

                    // @ATTR show_stat|stat_id, bool : Stat ID, Visible|Hide the matching stat ID in the statlist if set to false.
                    else if (infile.Key == "show_stat")
                    {
                        ParseShowStat(infile);
                    }

                    // @ATTR name_max_width|int|The maxiumum width, in pixels, that the character name can occupy until it is abbreviated.
                    else if (infile.Key == "name_max_width") _nameMaxWidth = Parse.ToInt(infile.Val);

                    else
                    {
                        infile.Error("MenuCharacter: '%s' is not a valid key.", infile.Key);
                    }
                }
                infile.Close();
            }

            // stat list
            _statList = new WidgetListBox(_statlistRows, WidgetListBox.CharMenuFile);
            Tablist.Add(_statList);
            _statList.CanSelect = false;
            _statList.ScrollbarOffset = _statlistScrollbarOffset;
            _statList.SetBasePos(_statlistPos.X, _statlistPos.Y, Utils.AlignTopLeft);

            // HACK: During gameplay, the stat list can refresh rapidly when the charcter menu is open and the player has certain effects
            // frequently refreshing trimmed text is slow for Cyrillic characters, so disable it here
            _statList.DisableTextTrim = true;

            if (_background == null)
                SetBackground("images/menus/character.png");

            Align();

            _baseStats = new List<int>(new int[eset.PrimaryStats.Stats.Count]);
            _baseStatsAdd = new List<int>(new int[eset.PrimaryStats.Stats.Count]);
            _baseBonus = new List<List<float>?>(new List<float>?[eset.PrimaryStats.Stats.Count]);

            for (int i = 0; i < eset.PrimaryStats.Stats.Count; ++i)
            {
                _baseStats[i] = i;
                _baseStatsAdd[i] = i;
                _baseBonus[i] = pc.Stats.PerPrimary[i];
            }
        }

        /// <summary>
        /// 对应 C++<c>~MenuCharacter()</c> Widget <see cref="Menu.Dispose"/>??
        /// </summary>
        public override void Dispose()
        {
            _closeButton?.Dispose();
            _closeButton = null;
            _labelCharacter?.Dispose();
            _labelCharacter = null;
            _labelUnspent?.Dispose();
            _labelUnspent = null;
            for (int i = 0; i < _cstat.Count; ++i)
            {
                _cstat[i].Label?.Dispose();
                _cstat[i].Label = null;
                _cstat[i].Value?.Dispose();
                _cstat[i].Value = null;
            }
            for (int i = 0; i < _upgradeButton.Count; ++i)
            {
                _upgradeButton[i]?.Dispose();
                _upgradeButton[i] = null;
            }
            _statList?.Dispose();
            _statList = null;

            base.Dispose();
        }

        public override void Align()
        {
            base.Align();

            // close button
            _closeButton!.SetPos(WindowArea.X, WindowArea.Y);

            // menu title
            _labelCharacter!.SetPos(WindowArea.X, WindowArea.Y);

            // upgrade buttons
            for (int i = 0; i < SharedResources.Eset!.PrimaryStats.Stats.Count; ++i)
            {
                _upgradeButton[i]!.SetPos(WindowArea.X, WindowArea.Y);
            }

            // stat list
            _statList!.SetPos(WindowArea.X, WindowArea.Y);

            for (int i = 0; i < _cstat.Count; ++i)
            {
                // setup static labels
                _cstat[i].Label!.SetPos(WindowArea.X, WindowArea.Y);

                // setup hotspot locations
                _cstat[i].SetHover(WindowArea.X, WindowArea.Y);

                // setup value labels
                _cstat[i].Value!.SetPos(WindowArea.X, WindowArea.Y);
            }

            _labelUnspent!.SetPos(WindowArea.X, WindowArea.Y);
        }

        /// <summary>
        /// Rebuild all stat values and tooltip info
        /// </summary>
        public void RefreshStats()
        {
            var msg = SharedResources.Msg!;
            var eset = SharedResources.Eset!;
            var font = SharedResources.Font!;
            var pc = SharedGameResources.Pc!;

            pc.Stats.RefreshStats = false;

            StringBuilder ss = new StringBuilder();

            // update stat text
            string trimmedName;
            if (_nameMaxWidth > 0)
                trimmedName = font.TrimTextToWidth(pc.Stats.Name, _nameMaxWidth, FontEngine.UseEllipsis, 0);
            else
                trimmedName = pc.Stats.Name;

            _cstat[CStatName].Value!.SetText(trimmedName);

            ss.Clear();
            ss.Append(pc.Stats.Level);
            _cstat[CStatLevel].Value!.SetText(ss.ToString());
            _cstat[CStatLevel].Value!.SetJustify(FontEngine.JustifyCenter);

            for (int i = 0; i < eset.PrimaryStats.Stats.Count; ++i)
            {
                ss.Clear();
                ss.Append(pc.Stats.GetPrimary(i));
                _cstat[i + 2].Value!.SetText(ss.ToString());
                _cstat[i + 2].Value!.SetJustify(FontEngine.JustifyCenter);
                _cstat[i + 2].Value!.SetColor(BonusColor(pc.Stats.PrimaryAdditional[i]));
            }

            if (_skillPoints >= 1)
            {
                _labelUnspent!.SetText(msg.GetV("Available stat points: %d", _skillPoints));
            }
            else
            {
                _labelUnspent!.SetText("");
            }

            // scrolling stat list
            uint statIndex = 0;
            int resourceOffsetIndex = Stats.Count + eset.DamageTypes.Count;
            int speedOffsetIndex = resourceOffsetIndex + eset.ResourceStats.StatCountValue;

            ss.Clear();
            ss.Append(msg.Get("Core Stats"));
            _statList!.Set(statIndex, ss.ToString(), "");
            _statList.SetRowHighlight(statIndex, true);
            statIndex++;

            for (int i = 0; i < Stats.Count; ++i)
            {
                if (Stats.Category[i] != (short)StatCategory.CategoryCore)
                    continue;

                if (!_showStat[i]) continue;

                // Stats::ABS_MIN handles both min and max
                if (i == Stats.AbsMin) continue;

                ss.Clear();
                ss.Append(' ');
                ss.Append(Stats.Name[i]);
                ss.Append(": ");
                ss.Append(Utils.FloatToString(pc.Stats.Get(i), 2));
                if (Stats.Percent[i]) ss.Append('%');
                _statList.Set(statIndex, ss.ToString(), StatTooltip(i));
                statIndex++;
            }

            // insert resource stats (execpt stealing)
            for (int j = 0; j < eset.ResourceStats.Stats.Count; ++j)
            {
                for (int k = 0; k < EngineSettings.ResourceStatsSettings.StatSteal; ++k)
                {
                    if (_showStat[resourceOffsetIndex + (j * EngineSettings.ResourceStatsSettings.StatCount) + k])
                    {
                        ss.Clear();
                        ss.Append(' ');
                        ss.Append(eset.ResourceStats.Stats[j].Text[k]);
                        ss.Append(": ");
                        ss.Append(Utils.FloatToString(pc.Stats.GetResourceStat(j, k), eset.NumberFormat.CharacterMenu));
                        _statList.Set(statIndex, ss.ToString(), ResourceStatTooltip(j, k));
                        statIndex++;
                    }
                }
            }

            ss.Clear();
            ss.Append(msg.Get("Offensive Stats"));
            _statList.Set(statIndex, ss.ToString(), "");
            _statList.SetRowHighlight(statIndex, true);
            statIndex++;

            // insert damage stats
            for (int j = 0; j < eset.DamageTypes.Types.Count; ++j)
            {
                if (_showStat[Stats.Count + EngineSettings.DamageTypesSettings.IndexToMin(j)] || _showStat[Stats.Count + EngineSettings.DamageTypesSettings.IndexToMax(j)])
                {
                    float minDmg = pc.Stats.GetDamageMin(j);
                    float maxDmg = pc.Stats.GetDamageMax(j);

                    ss.Clear();
                    ss.Append(' ');

                    if (eset.DamageTypes.Types[j].IsDeprecatedElement)
                        ss.Append(msg.GetV("Elemental Damage (%s)", eset.DamageTypes.Types[j].Name));
                    else
                        ss.Append(eset.DamageTypes.Types[j].Name);

                    ss.Append(": ");
                    ss.Append(Utils.CreateMinMaxString(minDmg, maxDmg, eset.NumberFormat.CharacterMenu));

                    _statList.Set(statIndex, ss.ToString(), DamageTooltip(j));
                    statIndex++;
                }
            }
            for (int i = 0; i < Stats.Count; ++i)
            {
                if (Stats.Category[i] != (short)StatCategory.CategoryOffense)
                    continue;

                if (!_showStat[i]) continue;

                ss.Clear();
                ss.Append(' ');
                ss.Append(Stats.Name[i]);
                ss.Append(": ");
                ss.Append(Utils.FloatToString(pc.Stats.Get(i), 2));
                if (Stats.Percent[i]) ss.Append('%');
                _statList.Set(statIndex, ss.ToString(), StatTooltip(i));
                statIndex++;
            }

            // insert resource stealing stats after HP/MP steal
            for (int j = 0; j < eset.ResourceStats.Stats.Count; ++j)
            {
                if (_showStat[resourceOffsetIndex + (j * EngineSettings.ResourceStatsSettings.StatCount) + EngineSettings.ResourceStatsSettings.StatSteal])
                {
                    ss.Clear();
                    ss.Append(' ');
                    ss.Append(eset.ResourceStats.Stats[j].Text[EngineSettings.ResourceStatsSettings.StatSteal]);
                    ss.Append(": ");
                    ss.Append(Utils.FloatToString(pc.Stats.GetResourceStat(j, EngineSettings.ResourceStatsSettings.StatSteal), eset.NumberFormat.CharacterMenu));
                    ss.Append('%');
                    _statList.Set(statIndex, ss.ToString(), ResourceStatTooltip(j, EngineSettings.ResourceStatsSettings.StatSteal));
                    statIndex++;
                }
            }

            ss.Clear();
            ss.Append(msg.Get("Defensive Stats"));
            _statList.Set(statIndex, ss.ToString(), "");
            _statList.SetRowHighlight(statIndex, true);
            statIndex++;

            ss.Clear();
            ss.Append(' ');
            ss.Append(msg.Get("Absorb"));
            ss.Append(": ");
            ss.Append(Utils.CreateMinMaxString(pc.Stats.Get(Stats.AbsMin), pc.Stats.Get(Stats.AbsMax), eset.NumberFormat.CharacterMenu));
            _statList.Set(statIndex, ss.ToString(), StatTooltip(Stats.AbsMin));
            statIndex++;

            for (int i = 0; i < Stats.Count; ++i)
            {
                if (Stats.Category[i] != (short)StatCategory.CategoryDefense)
                    continue;

                if (!_showStat[i]) continue;

                // absorb was already added to the list
                if (i == Stats.AbsMin || i == Stats.AbsMax) continue;

                ss.Clear();
                ss.Append(' ');
                ss.Append(Stats.Name[i]);
                ss.Append(": ");
                ss.Append(Utils.FloatToString(pc.Stats.Get(i), 2));
                if (Stats.Percent[i]) ss.Append('%');
                _statList.Set(statIndex, ss.ToString(), StatTooltip(i));
                statIndex++;
            }

            if (_showResists)
            {
                for (int i = 0; i < eset.DamageTypes.Types.Count; ++i)
                {
                    if (_showStat[Stats.Count + EngineSettings.DamageTypesSettings.IndexToResist(i)])
                    {
                        ss.Clear();
                        ss.Append(' ');
                        ss.Append(eset.DamageTypes.Types[i].NameResist);
                        ss.Append(": ");
                        ss.Append(Utils.FloatToString(pc.Stats.GetDamageResist(i), eset.NumberFormat.CharacterMenu));
                        ss.Append('%');
                        _statList.Set(statIndex, ss.ToString(), ResistTooltip(i));
                        statIndex++;
                    }
                }
            }

            // insert resource stealing stats after HP/MP steal
            for (int j = 0; j < eset.ResourceStats.Stats.Count; ++j)
            {
                if (_showStat[resourceOffsetIndex + (j * EngineSettings.ResourceStatsSettings.StatCount) + EngineSettings.ResourceStatsSettings.StatResistSteal])
                {
                    ss.Clear();
                    ss.Append(' ');
                    ss.Append(eset.ResourceStats.Stats[j].Text[EngineSettings.ResourceStatsSettings.StatResistSteal]);
                    ss.Append(": ");
                    ss.Append(Utils.FloatToString(pc.Stats.GetResourceStat(j, EngineSettings.ResourceStatsSettings.StatResistSteal), eset.NumberFormat.CharacterMenu));
                    ss.Append('%');
                    _statList.Set(statIndex, ss.ToString(), ResourceStatTooltip(j, EngineSettings.ResourceStatsSettings.StatResistSteal));
                    statIndex++;
                }
            }


            ss.Clear();
            ss.Append(msg.Get("Miscellaneous Stats"));
            _statList.Set(statIndex, ss.ToString(), "");
            _statList.SetRowHighlight(statIndex, true);
            statIndex++;

            for (int i = 0; i < Stats.Count; ++i)
            {
                if (Stats.Category[i] != (short)StatCategory.CategoryMisc)
                    continue;

                if (!_showStat[i]) continue;

                ss.Clear();
                ss.Append(' ');
                ss.Append(Stats.Name[i]);
                ss.Append(": ");
                ss.Append(Utils.FloatToString(pc.Stats.Get(i), 2));
                if (Stats.Percent[i]) ss.Append('%');
                _statList.Set(statIndex, ss.ToString(), StatTooltip(i));
                statIndex++;
            }

            if (_showStat[speedOffsetIndex])
            {
                ss.Clear();
                ss.Append(' ');
                ss.Append(msg.Get("Movement Speed"));
                ss.Append(": ");
                ss.Append(pc.Stats.Effects.Speed);
                ss.Append('%');
                _statList.Set(statIndex, ss.ToString(), "");
                statIndex++;
            }

            if (_showStat[speedOffsetIndex + 1])
            {
                ss.Clear();
                ss.Append(' ');
                ss.Append(msg.Get("Attack Speed"));
                ss.Append(": ");
                ss.Append(pc.Stats.Effects.GetAttackSpeed(""));
                ss.Append('%');
                _statList.Set(statIndex, ss.ToString(), "");
                statIndex++;
            }

            // update tool tips
            _cstat[CStatName].Tip.Clear();
            _cstat[CStatName].Tip.AddText(pc.Stats.Name);
            _cstat[CStatName].Tip.AddText(pc.Stats.GetLongClass());

            _cstat[CStatLevel].Tip.Clear();
            _cstat[CStatLevel].Tip.AddText(msg.GetV("XP: %lu", pc.Stats.Xp));
            if (pc.Stats.Level < eset.Xp.GetMaxLevel())
            {
                _cstat[CStatLevel].Tip.AddText(msg.GetV("Next: %lu", eset.Xp.GetLevelXP(pc.Stats.Level + 1)));
            }

            for (int j = 2; j < _cstat.Count; ++j)
            {
                _cstat[j].Tip.Clear();
                ss.Clear();
                ss.Append(_cstat[j].Label!.GetText());
                ss.Append(" (");
                ss.Append(pc.Stats.Primary[_baseStats[j - 2]]);
                if (pc.Stats.PrimaryAdditional[_baseStatsAdd[j - 2]] > 0)
                {
                    ss.Append('+');
                }
                if (pc.Stats.PrimaryAdditional[_baseStatsAdd[j - 2]] != 0)
                {
                    ss.Append(pc.Stats.PrimaryAdditional[_baseStatsAdd[j - 2]]);
                }
                ss.Append(')');
                _cstat[j].Tip.AddText(ss.ToString());

                bool haveBonus = false;
                int resourceStatOffset = Stats.Count + eset.DamageTypes.Count;

                for (int i = 0; i < Stats.Count; ++i)
                {
                    // insert resource stats (execpt stealing) before accuracy
                    if (i == Stats.Accuracy)
                    {
                        for (int k = 0; k < eset.ResourceStats.Stats.Count; ++k)
                        {
                            for (int l = 0; l < EngineSettings.ResourceStatsSettings.StatSteal; ++l)
                            {
                                int resourceIndex = resourceStatOffset + (k * EngineSettings.ResourceStatsSettings.StatCount) + l;

                                if (_baseBonus[j - 2]![resourceIndex] > 0 && _showStat[resourceIndex])
                                {
                                    if (!haveBonus)
                                    {
                                        _cstat[j].Tip.AddText("\n" + msg.Get("Related stats:"));
                                        haveBonus = true;
                                    }
                                    _cstat[j].Tip.AddText(eset.ResourceStats.Stats[k].Text[l]);
                                }
                            }
                        }
                    }

                    // damage types are displayed before absorb
                    if (i == Stats.AbsMin)
                    {
                        for (int k = 0; k < eset.DamageTypes.Types.Count; ++k)
                        {
                            // damage min
                            if (_baseBonus[j - 2]![Stats.Count + EngineSettings.DamageTypesSettings.IndexToMin(k)] > 0 && _showStat[Stats.Count + EngineSettings.DamageTypesSettings.IndexToMin(k)])
                            {
                                if (!haveBonus)
                                {
                                    _cstat[j].Tip.AddText("\n" + msg.Get("Related stats:"));
                                    haveBonus = true;
                                }
                                _cstat[j].Tip.AddText(eset.DamageTypes.Types[k].NameMin);
                            }
                            // damage max
                            if (_baseBonus[j - 2]![Stats.Count + EngineSettings.DamageTypesSettings.IndexToMax(k)] > 0 && _showStat[Stats.Count + EngineSettings.DamageTypesSettings.IndexToMax(k)])
                            {
                                if (!haveBonus)
                                {
                                    _cstat[j].Tip.AddText("\n" + msg.Get("Related stats:"));
                                    haveBonus = true;
                                }
                                _cstat[j].Tip.AddText(eset.DamageTypes.Types[k].NameMax);
                            }
                        }
                    }

                    // non-damage bonuses
                    if (_baseBonus[j - 2]![i] > 0 && _showStat[i])
                    {
                        if (!haveBonus)
                        {
                            _cstat[j].Tip.AddText("\n" + msg.Get("Related stats:"));
                            haveBonus = true;
                        }
                        _cstat[j].Tip.AddText(Stats.Name[i]);
                    }
                }

                // insert resource stealing stats after MP steal
                for (int i = 0; i < eset.ResourceStats.Stats.Count; ++i)
                {
                    for (int k = EngineSettings.ResourceStatsSettings.StatSteal; k < EngineSettings.ResourceStatsSettings.StatCount; ++k)
                    {
                        int resourceIndex = resourceStatOffset + (i * EngineSettings.ResourceStatsSettings.StatCount) + k;

                        if (_baseBonus[j - 2]![resourceIndex] > 0 && _showStat[resourceIndex])
                        {
                            if (!haveBonus)
                            {
                                _cstat[j].Tip.AddText("\n" + msg.Get("Related stats:"));
                                haveBonus = true;
                            }
                            _cstat[j].Tip.AddText(eset.ResourceStats.Stats[i].Text[k]);
                        }
                    }
                }

                // resistances
                for (int i = 0; i < eset.DamageTypes.Types.Count; ++i)
                {
                    if (_baseBonus[j - 2]![Stats.Count + EngineSettings.DamageTypesSettings.IndexToResist(i)] > 0 && _showStat[Stats.Count + EngineSettings.DamageTypesSettings.IndexToResist(i)])
                    {
                        if (!haveBonus)
                        {
                            _cstat[j].Tip.AddText("\n" + msg.Get("Related stats:"));
                            haveBonus = true;
                        }
                        _cstat[j].Tip.AddText(eset.DamageTypes.Types[i].NameResist);
                    }
                }
            }
        }

        /// <summary>
        /// Color-coding for positive/negative/no bonus
        /// </summary>
        private Color BonusColor(int stat)
        {
            var font = SharedResources.Font!;
            if (stat > 0) return font.GetColor(FontEngine.ColorMenuBonus);
            if (stat < 0) return font.GetColor(FontEngine.ColorMenuPenalty);
            return font.GetColor(FontEngine.ColorMenuNormal);
        }

        private void TooltipCreateBonusText(int minIndex, int maxIndex, ref string minText, ref string? maxText)
        {
            var msg = SharedResources.Msg!;
            var eset = SharedResources.Eset!;
            var pc = SharedGameResources.Pc!;

            // per-level bonus
            float minPerLevel = pc.Stats.PerLevel[minIndex];
            float maxPerLevel = pc.Stats.PerLevel[maxIndex];

            if (minPerLevel > 0)
            {
                minText += msg.GetV("Each level grants %s.", Utils.FloatToString(minPerLevel, eset.NumberFormat.CharacterMenu));
            }

            if (maxText != null && maxPerLevel > 0)
            {
                maxText += msg.GetV("Each level grants %s.", Utils.FloatToString(maxPerLevel, eset.NumberFormat.CharacterMenu));
            }

            // per-primary bonuses
            for (int i = 0; i < eset.PrimaryStats.Stats.Count; ++i)
            {
                float minPerPrimary = pc.Stats.PerPrimary[i][minIndex];
                float maxPerPrimary = pc.Stats.PerPrimary[i][maxIndex];

                if (minPerPrimary > 0)
                {
                    if (minText.Length != 0)
                        minText += "\n";

                    minText += msg.GetV("Each point of %s grants %s.", eset.PrimaryStats.Stats[i].Name, Utils.FloatToString(minPerPrimary, eset.NumberFormat.CharacterMenu));
                }

                if (maxText != null && maxPerPrimary > 0)
                {
                    if (maxText.Length != 0)
                        maxText += "\n";

                    maxText += msg.GetV("Each point of %s grants %s.", eset.PrimaryStats.Stats[i].Name, Utils.FloatToString(maxPerPrimary, eset.NumberFormat.CharacterMenu));
                }
            }
        }

        /// <summary>
        /// Create tooltip text showing the per_* values of a stat
        /// </summary>
        private string StatTooltip(int stat)
        {
            string text = "";
            string minText = "";

            string description = Stats.Desc[stat];
            if (description.Length != 0)
                text += description;

            int minIndex = stat;

            if (stat == Stats.AbsMin)
            {
                string? maxText = "";

                int maxIndex = Stats.AbsMax;

                TooltipCreateBonusText(minIndex, maxIndex, ref minText, ref maxText);

                if (minText.Length != 0)
                {
                    if (text.Length != 0)
                        text += "\n\n";
                    text += Stats.Name[minIndex] + ":\n" + minText;
                }

                if (maxText!.Length != 0)
                {
                    if (text.Length != 0)
                        text += "\n\n";
                    text += Stats.Name[maxIndex] + ":\n" + maxText;
                }
            }
            else
            {
                string? maxText = null;
                TooltipCreateBonusText(minIndex, minIndex, ref minText, ref maxText);

                if (minText.Length != 0)
                {
                    if (text.Length != 0)
                        text += "\n\n";
                    text += minText;
                }
            }

            return text;
        }

        /// <summary>
        /// Create tooltip text showing the per_* values of a damage stat
        /// </summary>
        private string DamageTooltip(int dmgType)
        {
            var eset = SharedResources.Eset!;
            string text = "";
            string minText = "";
            string? maxText = "";

            string description = eset.DamageTypes.Types[dmgType].Description;
            if (description.Length != 0)
                text += description;

            int minIndex = Stats.Count + EngineSettings.DamageTypesSettings.IndexToMin(dmgType);
            int maxIndex = Stats.Count + EngineSettings.DamageTypesSettings.IndexToMax(dmgType);

            TooltipCreateBonusText(minIndex, maxIndex, ref minText, ref maxText);

            if (minText.Length != 0)
            {
                if (text.Length != 0)
                    text += "\n\n";
                text += eset.DamageTypes.Types[dmgType].NameMin + ":\n" + minText;
            }

            if (maxText!.Length != 0)
            {
                if (text.Length != 0)
                    text += "\n\n";
                text += eset.DamageTypes.Types[dmgType].NameMax + ":\n" + maxText;
            }

            return text;
        }

        /// <summary>
        /// Create tooltip text showing the per_* values of a resistance stat
        /// </summary>
        private string ResistTooltip(int resistType)
        {
            string text = "";
            string bonusText = "";

            text += SharedResources.Msg!.Get("Reduces damage taken from attacks of this damage type.");

            int resistIndex = Stats.Count + EngineSettings.DamageTypesSettings.IndexToResist(resistType);

            string? maxText = null;
            TooltipCreateBonusText(resistIndex, resistIndex, ref bonusText, ref maxText);

            if (bonusText.Length != 0)
            {
                if (text.Length != 0)
                    text += "\n\n";
                text += bonusText;
            }

            return text;
        }

        private string ResourceStatTooltip(int resourceIndex, int statIndex)
        {
            var eset = SharedResources.Eset!;
            string text = "";
            string bonusText = "";

            string description = eset.ResourceStats.Stats[resourceIndex].TextDesc[statIndex];
            if (description.Length != 0)
                text += description;

            int offsetIndex = Stats.Count + eset.DamageTypes.Count;
            int resourceStatIndex = offsetIndex + (resourceIndex * EngineSettings.ResourceStatsSettings.StatCount) + statIndex;

            string? maxText = null;
            TooltipCreateBonusText(resourceStatIndex, resourceStatIndex, ref bonusText, ref maxText);

            if (bonusText.Length != 0)
            {
                if (text.Length != 0)
                    text += "\n\n";
                text += bonusText;
            }

            return text;
        }

        public void Logic()
        {
            if (!Visible) return;

            var eset = SharedResources.Eset!;
            var pc = SharedGameResources.Pc!;
            var snd = SharedResources.Snd!;

            Tablist.Logic();

            if (_closeButton!.CheckClick())
            {
                Visible = false;
                snd.Play(SfxClose, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);
            }

            bool haveSkillPoints = CheckSkillPoints();

            if (pc.Stats.Hp > 0 && haveSkillPoints)
            {
                for (int i = 0; i < eset.PrimaryStats.Stats.Count; ++i)
                {
                    if (pc.Stats.Primary[i] < pc.Stats.MaxPointsPerStat && !_cstat[i + 2].Label!.IsHidden())
                    {
                        _upgradeButton[i]!.Enabled = true;
                        _upgradeButton[i]!.Tooltip = GetUpgradeButtonTooltip(i);
                        Tablist.Add(_upgradeButton[i]!);
                    }
                    else
                    {
                        _upgradeButton[i]!.Enabled = false;
                        _upgradeButton[i]!.Tooltip = "";
                        Tablist.Remove(_upgradeButton[i]!);
                    }
                }

                for (int i = 0; i < eset.PrimaryStats.Stats.Count; ++i)
                {
                    if (_upgradeButton[i]!.CheckClick())
                        _primaryUp[i] = true;
                }
            }
            else
            {
                // no skill points to allocate; remove upgrade buttons
                for (int i = 0; i < eset.PrimaryStats.Stats.Count; ++i)
                {
                    _upgradeButton[i]!.Enabled = false;
                    Tablist.Remove(_upgradeButton[i]!);
                }

                if (Tablist.GetCurrent() >= (int)Tablist.Size())
                {
                    Tablist.Defocus();
                    Tablist.GetNext(TabList.GetInner, TabList.WidgetSelectAuto);
                }
            }

            _statList!.CheckClick();

            if (pc.Stats.RefreshStats) RefreshStats();
        }
        
        public string GetUpgradeButtonTooltip(int primaryIndex)
        {
            var msg = SharedResources.Msg!;
            var eset = SharedResources.Eset!;
            var pc = SharedGameResources.Pc!;
            string tooltip = msg.GetV("Upgrade Stat: %s\nUses 1 Stat Point", eset.PrimaryStats.Stats[primaryIndex].Name);
            bool haveBonus = false;
            int resourceStatOffset = Stats.Count + eset.DamageTypes.Count;

            for (int i = 0; i < Stats.Count; ++i)
            {
                // insert resource stats (except stealing) before accuracy
                if (i == Stats.Accuracy)
                {
                    for (int k = 0; k < eset.ResourceStats.Stats.Count; ++k)
                    {
                        for (int l = 0; l < EngineSettings.ResourceStatsSettings.StatSteal; ++l)
                        {
                            int resourceIndex = resourceStatOffset + (k * EngineSettings.ResourceStatsSettings.StatCount) + l;
                            if (_baseBonus[primaryIndex]![resourceIndex] > 0 && _showStat[resourceIndex])
                            {
                                if (!haveBonus)
                                {
                                    tooltip += "\n";
                                    haveBonus = true;
                                }
                                tooltip += "\n+" + Utils.FloatToString(pc.Stats.PerPrimary[primaryIndex][resourceIndex], eset.NumberFormat.CharacterMenu);
                                tooltip += " " + eset.ResourceStats.Stats[k].Text[l];
                            }
                        }
                    }
                }

                // damage types are displayed before absorb
                if (i == Stats.AbsMin)
                {
                    for (int k = 0; k < eset.DamageTypes.Types.Count; ++k)
                    {
                        // damage min
                        int damageMinIndex = Stats.Count + EngineSettings.DamageTypesSettings.IndexToMin(k);
                        if (_baseBonus[primaryIndex]![damageMinIndex] > 0 && _showStat[damageMinIndex])
                        {
                            if (!haveBonus)
                            {
                                tooltip += "\n";
                                haveBonus = true;
                            }
                            tooltip += "\n+" + Utils.FloatToString(pc.Stats.PerPrimary[primaryIndex][damageMinIndex], eset.NumberFormat.CharacterMenu);
                            tooltip += " " + eset.DamageTypes.Types[k].NameMin;
                        }

                        // damage max
                        int damageMaxIndex = Stats.Count + EngineSettings.DamageTypesSettings.IndexToMax(k);
                        if (_baseBonus[primaryIndex]![damageMaxIndex] > 0 && _showStat[damageMaxIndex])
                        {
                            if (!haveBonus)
                            {
                                tooltip += "\n";
                                haveBonus = true;
                            }
                            tooltip += "\n+" + Utils.FloatToString(pc.Stats.PerPrimary[primaryIndex][damageMaxIndex], eset.NumberFormat.CharacterMenu);
                            tooltip += " " + eset.DamageTypes.Types[k].NameMax;
                        }
                    }
                }

                // non-damage bonuses
                if (_baseBonus[primaryIndex]![i] > 0 && _showStat[i])
                {
                    if (!haveBonus)
                    {
                        tooltip += "\n";
                        haveBonus = true;
                    }
                    tooltip += "\n+" + Utils.FloatToString(pc.Stats.PerPrimary[primaryIndex][i], eset.NumberFormat.CharacterMenu);
                    tooltip += " " + Stats.Name[i];
                }
            }

            // insert resource stealing stats after MP steal
            for (int i = 0; i < eset.ResourceStats.Stats.Count; ++i)
            {
                for (int k = EngineSettings.ResourceStatsSettings.StatSteal; k < EngineSettings.ResourceStatsSettings.StatCount; ++k)
                {
                    int resourceIndex = resourceStatOffset + (i * EngineSettings.ResourceStatsSettings.StatCount) + k;
                    if (_baseBonus[primaryIndex]![resourceIndex] > 0 && _showStat[resourceIndex])
                    {
                        if (!haveBonus)
                        {
                            tooltip += "\n";
                            haveBonus = true;
                        }
                        tooltip += "\n+" + Utils.FloatToString(pc.Stats.PerPrimary[primaryIndex][resourceIndex], eset.NumberFormat.CharacterMenu);
                        tooltip += " " + eset.ResourceStats.Stats[i].Text[k];
                    }
                }
            }

            // resistances
            for (int i = 0; i < eset.DamageTypes.Types.Count; ++i)
            {
                int resistIndex = Stats.Count + EngineSettings.DamageTypesSettings.IndexToResist(i);
                if (_baseBonus[primaryIndex]![resistIndex] > 0 && _showStat[resistIndex])
                {
                    if (!haveBonus)
                    {
                        tooltip += "\n";
                        haveBonus = true;
                    }
                    tooltip += "\n+" + Utils.FloatToString(pc.Stats.PerPrimary[primaryIndex][resistIndex], eset.NumberFormat.CharacterMenu);
                    tooltip += " " + eset.DamageTypes.Types[i].NameResist;
                }
            }

            return tooltip;
        }
        
        public override void Render()
        {
            if (!Visible) return;

            // background
            base.Render();

            // close button
            _closeButton!.Render();

            // title
            _labelCharacter!.Render();

            // unspent points
            _labelUnspent!.Render();

            // labels and values
            for (int i = 0; i < _cstat.Count; ++i)
            {
                if (!_cstat[i].Label!.IsHidden())
                {
                    _cstat[i].Label!.Render();
                    _cstat[i].Value!.Render();
                }
            }

            // upgrade buttons
            for (int i = 0; i < SharedResources.Eset!.PrimaryStats.Stats.Count; ++i)
            {
                if (_upgradeButton[i]!.Enabled) _upgradeButton[i]!.Render();
            }

            _statList!.Render();
        }

        /// <summary>
        /// Display various mouseovers tooltips depending on cursor location
        /// </summary>
        public void RenderTooltips(Int2 position)
        {
            if (!Visible || !Utils.IsWithinRect(WindowArea, position))
                return;

            for (int i = 0; i < _cstat.Count; ++i)
            {
                if (Utils.IsWithinRect(_cstat[i].Hover, position) && !_cstat[i].Tip.IsEmpty() && !_cstat[i].Label!.IsHidden())
                {
                    SharedResources.Tooltipm!.Push(_cstat[i].Tip, position, TooltipData.StyleFloat);
                    break;
                }
            }
        }

        /// <summary>
        /// User might click this menu to upgrade a stat.  Check for this situation.
        /// Return true if a stat was upgraded.
        /// </summary>
        public bool CheckUpgrade()
        {
            var eset = SharedResources.Eset!;
            var pc = SharedGameResources.Pc!;

            // check to see if there are skill points available
            if (pc.Stats.Hp > 0 && CheckSkillPoints())
            {
                for (int i = 0; i < eset.PrimaryStats.Stats.Count; ++i)
                {
                    if (_primaryUp[i])
                    {
                        pc.Stats.Primary[i]++;
                        pc.Stats.Recalc(); // equipment applied by MenuManager
                        _primaryUp[i] = false;
                        return true;
                    }
                }
            }

            return false;
        }

        private bool CheckSkillPoints()
        {
            var eset = SharedResources.Eset!;
            var pc = SharedGameResources.Pc!;

            int spent = 0;
            for (int i = 0; i < eset.PrimaryStats.Stats.Count; ++i)
            {
                spent += pc.Stats.Primary[i] - pc.Stats.PrimaryStarting[i];
            }

            _skillPoints = ((pc.Stats.Level - 1) * pc.Stats.StatPointsPerLevel) - spent;

            return (spent < ((pc.Stats.Level - 1) * pc.Stats.StatPointsPerLevel) && spent < pc.Stats.MaxSpendableStatPoints);
        }

        private void ParseShowStat(FileParser infile)
        {
            var eset = SharedResources.Eset!;

            string val = infile.Val;
            string statName = Parse.PopFirstString(ref val);
            bool value = Parse.ToBool(Parse.PopFirstString(ref val));
            int offsetIndex = 0;

            for (int i = 0; i < Stats.Count; ++i)
            {
                if (statName == Stats.Key[i])
                {
                    _showStat[i] = value;
                    return;
                }
            }
            offsetIndex += Stats.Count;

            for (int i = 0; i < eset.DamageTypes.Types.Count; ++i)
            {
                if (statName == eset.DamageTypes.Types[i].Min)
                {
                    _showStat[offsetIndex + EngineSettings.DamageTypesSettings.IndexToMin(i)] = value;
                    return;
                }
                else if (statName == eset.DamageTypes.Types[i].Max)
                {
                    _showStat[offsetIndex + EngineSettings.DamageTypesSettings.IndexToMax(i)] = value;
                    return;
                }
                else if (statName == eset.DamageTypes.Types[i].Resist)
                {
                    _showStat[offsetIndex + EngineSettings.DamageTypesSettings.IndexToResist(i)] = value;
                    return;
                }
            }
            offsetIndex += eset.DamageTypes.Count;

            for (int i = 0; i < eset.ResourceStats.Stats.Count; ++i)
            {
                for (int j = 0; j < EngineSettings.ResourceStatsSettings.StatCount; ++j)
                {
                    if (statName == eset.ResourceStats.Stats[i].Ids[j])
                    {
                        _showStat[offsetIndex + (i * EngineSettings.ResourceStatsSettings.StatCount) + j] = value;
                        return;
                    }
                }
            }
            offsetIndex += eset.ResourceStats.StatCountValue;

            if (statName == "speed")
            {
                _showStat[offsetIndex] = value;
            }
            else if (statName == "attack_speed")
            {
                _showStat[offsetIndex + 1] = value;
            }
        }

        /// <summary>?? C++  <c>getUnspent()</c>已全局导入，此处省略。/summary>
        public int GetUnspent()
        {
            return _skillPoints;
        }
    }
}
