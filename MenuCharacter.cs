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

            _labelCharacter.SetText(SharedResources.Msg!.Get("Character"));
            _labelCharacter.SetColor(SharedResources.Font!.GetColor(FontEngine.ColorMenuNormal));
            _labelUnspent.SetColor(SharedResources.Font!.GetColor(FontEngine.ColorMenuBonus));

            // 2 is added here to account for CSTAT_NAME and CSTAT_LEVEL
            while (_cstat.Count < SharedResources.Eset!.PrimaryStats.Stats.Count + 2)
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

                _cstat[i].Label.SetColor(SharedResources.Font!.GetColor(FontEngine.ColorMenuNormal));

                _cstat[i].Value.SetVAlign(LabelInfo.ValignCenter);
                _cstat[i].Value.SetColor(SharedResources.Font!.GetColor(FontEngine.ColorMenuNormal));
            }
            _cstat[CStatName].Label!.SetText(SharedResources.Msg!.Get("Name"));
            _cstat[CStatLevel].Label!.SetText(SharedResources.Msg!.Get("Level"));
            for (int i = 0; i < SharedResources.Eset!.PrimaryStats.Stats.Count; ++i)
            {
                _cstat[i + 2].Label!.SetText(SharedResources.Eset!.PrimaryStats.Stats[i].Name);
            }
            _cstat[CStatName].Label!.SetText(SharedResources.Msg!.Get("Name"));
            _cstat[CStatLevel].Label!.SetText(SharedResources.Msg!.Get("Level"));
            for (int i = 0; i < SharedResources.Eset!.PrimaryStats.Stats.Count; ++i)
            {
                _cstat[i + 2].Label!.SetText(SharedResources.Eset!.PrimaryStats.Stats[i].Name);
            }

            _showStat = new List<bool>(new bool[Stats.Count + SharedResources.Eset!.DamageTypes.Count + SharedResources.Eset!.ResourceStats.StatCountValue + 2]);
            for (int i = 0; i < _showStat.Count; i++)
            {
                if (i >= Stats.ResistDamageOverTime && i < Stats.Count)
                {
                    // some stats are hidden by default
                    _showStat[i] = false;
                }
                else if (i >= Stats.Count && i < Stats.Count + SharedResources.Eset!.DamageTypes.Count)
                {
                    int damageSubIndex = i - Stats.Count;
                    int damageIndex = damageSubIndex / 3;
                    int damageSubStat = damageSubIndex % 3;

                    if (SharedResources.Eset!.DamageTypes.Types[damageIndex].IsDeprecatedElement && damageSubStat != 2)
                    {
                        // don't show damage for elements loaded from engine/elements.txt by default
                        _showStat[i] = false;
                    }
                    else if (!SharedResources.Eset!.DamageTypes.Types[damageIndex].IsElemental && damageSubStat == 2)
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
            _primaryUp = new List<bool>(new bool[SharedResources.Eset!.PrimaryStats.Stats.Count]);
            _upgradeButton = new List<WidgetButton?>(new WidgetButton?[SharedResources.Eset!.PrimaryStats.Stats.Count]);

            for (int i = 0; i < SharedResources.Eset!.PrimaryStats.Stats.Count; ++i)
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
                        int primStatIndex = SharedResources.Eset!.PrimaryStats.GetIndexByID(primStat);

                        if (primStatIndex != SharedResources.Eset!.PrimaryStats.Stats.Count)
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
                        int primStatIndex = SharedResources.Eset!.PrimaryStats.GetIndexByID(primStat);

                        if (primStatIndex != SharedResources.Eset!.PrimaryStats.Stats.Count)
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
                        int primStatIndex = SharedResources.Eset!.PrimaryStats.GetIndexByID(primStat);

                        if (primStatIndex != SharedResources.Eset!.PrimaryStats.Stats.Count)
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

            _baseStats = new List<int>(new int[SharedResources.Eset!.PrimaryStats.Stats.Count]);
            _baseStatsAdd = new List<int>(new int[SharedResources.Eset!.PrimaryStats.Stats.Count]);
            _baseBonus = new List<List<float>?>(new List<float>?[SharedResources.Eset!.PrimaryStats.Stats.Count]);

            for (int i = 0; i < SharedResources.Eset!.PrimaryStats.Stats.Count; ++i)
            {
                _baseStats[i] = i;
                _baseStatsAdd[i] = i;
                _baseBonus[i] = SharedGameResources.Pc!.Stats.PerPrimary[i];
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
            SharedGameResources.Pc!.Stats.RefreshStats = false;

            StringBuilder ss = new StringBuilder();

            // update stat text
            string trimmedName;
            if (_nameMaxWidth > 0)
                trimmedName = SharedResources.Font!.TrimTextToWidth(SharedGameResources.Pc!.Stats.Name, _nameMaxWidth, FontEngine.UseEllipsis, 0);
            else
                trimmedName = SharedGameResources.Pc!.Stats.Name;

            _cstat[CStatName].Value!.SetText(trimmedName);

            ss.Clear();
            ss.Append(SharedGameResources.Pc!.Stats.Level);
            _cstat[CStatLevel].Value!.SetText(ss.ToString());
            _cstat[CStatLevel].Value!.SetJustify(FontEngine.JustifyCenter);

            for (int i = 0; i < SharedResources.Eset!.PrimaryStats.Stats.Count; ++i)
            {
                ss.Clear();
                ss.Append(SharedGameResources.Pc!.Stats.GetPrimary(i));
                _cstat[i + 2].Value!.SetText(ss.ToString());
                _cstat[i + 2].Value!.SetJustify(FontEngine.JustifyCenter);
                _cstat[i + 2].Value!.SetColor(BonusColor(SharedGameResources.Pc!.Stats.PrimaryAdditional[i]));
            }

            if (_skillPoints >= 1)
            {
                _labelUnspent!.SetText(SharedResources.Msg!.GetV("Available stat points: %d", _skillPoints));
            }
            else
            {
                _labelUnspent!.SetText("");
            }

            // scrolling stat list
            uint statIndex = 0;
            int resourceOffsetIndex = Stats.Count + SharedResources.Eset!.DamageTypes.Count;
            int speedOffsetIndex = resourceOffsetIndex + SharedResources.Eset!.ResourceStats.StatCountValue;

            ss.Clear();
            ss.Append(SharedResources.Msg!.Get("Core Stats"));
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
                ss.Append(Utils.FloatToString(SharedGameResources.Pc!.Stats.Get(i), 2));
                if (Stats.Percent[i]) ss.Append('%');
                _statList.Set(statIndex, ss.ToString(), StatTooltip(i));
                statIndex++;
            }

            // insert resource stats (execpt stealing)
            for (int j = 0; j < SharedResources.Eset!.ResourceStats.Stats.Count; ++j)
            {
                for (int k = 0; k < EngineSettings.ResourceStatsSettings.StatSteal; ++k)
                {
                    if (_showStat[resourceOffsetIndex + (j * EngineSettings.ResourceStatsSettings.StatCount) + k])
                    {
                        ss.Clear();
                        ss.Append(' ');
                        ss.Append(SharedResources.Eset!.ResourceStats.Stats[j].Text[k]);
                        ss.Append(": ");
                        ss.Append(Utils.FloatToString(SharedGameResources.Pc!.Stats.GetResourceStat(j, k), SharedResources.Eset!.NumberFormat.CharacterMenu));
                        _statList.Set(statIndex, ss.ToString(), ResourceStatTooltip(j, k));
                        statIndex++;
                    }
                }
            }

            ss.Clear();
            ss.Append(SharedResources.Msg!.Get("Offensive Stats"));
            _statList.Set(statIndex, ss.ToString(), "");
            _statList.SetRowHighlight(statIndex, true);
            statIndex++;

            // insert damage stats
            for (int j = 0; j < SharedResources.Eset!.DamageTypes.Types.Count; ++j)
            {
                if (_showStat[Stats.Count + EngineSettings.DamageTypesSettings.IndexToMin(j)] || _showStat[Stats.Count + EngineSettings.DamageTypesSettings.IndexToMax(j)])
                {
                    float minDmg = SharedGameResources.Pc!.Stats.GetDamageMin(j);
                    float maxDmg = SharedGameResources.Pc!.Stats.GetDamageMax(j);

                    ss.Clear();
                    ss.Append(' ');

                    if (SharedResources.Eset!.DamageTypes.Types[j].IsDeprecatedElement)
                        ss.Append(SharedResources.Msg!.GetV("Elemental Damage (%s)", SharedResources.Eset!.DamageTypes.Types[j].Name));
                    else
                        ss.Append(SharedResources.Eset!.DamageTypes.Types[j].Name);

                    ss.Append(": ");
                    ss.Append(Utils.CreateMinMaxString(minDmg, maxDmg, SharedResources.Eset!.NumberFormat.CharacterMenu));

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
                ss.Append(Utils.FloatToString(SharedGameResources.Pc!.Stats.Get(i), 2));
                if (Stats.Percent[i]) ss.Append('%');
                _statList.Set(statIndex, ss.ToString(), StatTooltip(i));
                statIndex++;
            }

            // insert resource stealing stats after HP/MP steal
            for (int j = 0; j < SharedResources.Eset!.ResourceStats.Stats.Count; ++j)
            {
                if (_showStat[resourceOffsetIndex + (j * EngineSettings.ResourceStatsSettings.StatCount) + EngineSettings.ResourceStatsSettings.StatSteal])
                {
                    ss.Clear();
                    ss.Append(' ');
                    ss.Append(SharedResources.Eset!.ResourceStats.Stats[j].Text[EngineSettings.ResourceStatsSettings.StatSteal]);
                    ss.Append(": ");
                    ss.Append(Utils.FloatToString(SharedGameResources.Pc!.Stats.GetResourceStat(j, EngineSettings.ResourceStatsSettings.StatSteal), SharedResources.Eset!.NumberFormat.CharacterMenu));
                    ss.Append('%');
                    _statList.Set(statIndex, ss.ToString(), ResourceStatTooltip(j, EngineSettings.ResourceStatsSettings.StatSteal));
                    statIndex++;
                }
            }

            ss.Clear();
            ss.Append(SharedResources.Msg!.Get("Defensive Stats"));
            _statList.Set(statIndex, ss.ToString(), "");
            _statList.SetRowHighlight(statIndex, true);
            statIndex++;

            ss.Clear();
            ss.Append(' ');
            ss.Append(SharedResources.Msg!.Get("Absorb"));
            ss.Append(": ");
            ss.Append(Utils.CreateMinMaxString(SharedGameResources.Pc!.Stats.Get(Stats.AbsMin), SharedGameResources.Pc!.Stats.Get(Stats.AbsMax), SharedResources.Eset!.NumberFormat.CharacterMenu));
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
                ss.Append(Utils.FloatToString(SharedGameResources.Pc!.Stats.Get(i), 2));
                if (Stats.Percent[i]) ss.Append('%');
                _statList.Set(statIndex, ss.ToString(), StatTooltip(i));
                statIndex++;
            }

            if (_showResists)
            {
                for (int i = 0; i < SharedResources.Eset!.DamageTypes.Types.Count; ++i)
                {
                    if (_showStat[Stats.Count + EngineSettings.DamageTypesSettings.IndexToResist(i)])
                    {
                        ss.Clear();
                        ss.Append(' ');
                        ss.Append(SharedResources.Eset!.DamageTypes.Types[i].NameResist);
                        ss.Append(": ");
                        ss.Append(Utils.FloatToString(SharedGameResources.Pc!.Stats.GetDamageResist(i), SharedResources.Eset!.NumberFormat.CharacterMenu));
                        ss.Append('%');
                        _statList.Set(statIndex, ss.ToString(), ResistTooltip(i));
                        statIndex++;
                    }
                }
            }

            // insert resource stealing stats after HP/MP steal
            for (int j = 0; j < SharedResources.Eset!.ResourceStats.Stats.Count; ++j)
            {
                if (_showStat[resourceOffsetIndex + (j * EngineSettings.ResourceStatsSettings.StatCount) + EngineSettings.ResourceStatsSettings.StatResistSteal])
                {
                    ss.Clear();
                    ss.Append(' ');
                    ss.Append(SharedResources.Eset!.ResourceStats.Stats[j].Text[EngineSettings.ResourceStatsSettings.StatResistSteal]);
                    ss.Append(": ");
                    ss.Append(Utils.FloatToString(SharedGameResources.Pc!.Stats.GetResourceStat(j, EngineSettings.ResourceStatsSettings.StatResistSteal), SharedResources.Eset!.NumberFormat.CharacterMenu));
                    ss.Append('%');
                    _statList.Set(statIndex, ss.ToString(), ResourceStatTooltip(j, EngineSettings.ResourceStatsSettings.StatResistSteal));
                    statIndex++;
                }
            }


            ss.Clear();
            ss.Append(SharedResources.Msg!.Get("Miscellaneous Stats"));
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
                ss.Append(Utils.FloatToString(SharedGameResources.Pc!.Stats.Get(i), 2));
                if (Stats.Percent[i]) ss.Append('%');
                _statList.Set(statIndex, ss.ToString(), StatTooltip(i));
                statIndex++;
            }

            if (_showStat[speedOffsetIndex])
            {
                ss.Clear();
                ss.Append(' ');
                ss.Append(SharedResources.Msg!.Get("Movement Speed"));
                ss.Append(": ");
                ss.Append(SharedGameResources.Pc!.Stats.Effects.Speed);
                ss.Append('%');
                _statList.Set(statIndex, ss.ToString(), "");
                statIndex++;
            }

            if (_showStat[speedOffsetIndex + 1])
            {
                ss.Clear();
                ss.Append(' ');
                ss.Append(SharedResources.Msg!.Get("Attack Speed"));
                ss.Append(": ");
                ss.Append(SharedGameResources.Pc!.Stats.Effects.GetAttackSpeed(""));
                ss.Append('%');
                _statList.Set(statIndex, ss.ToString(), "");
                statIndex++;
            }

            // update tool tips
            _cstat[CStatName].Tip.Clear();
            _cstat[CStatName].Tip.AddText(SharedGameResources.Pc!.Stats.Name);
            _cstat[CStatName].Tip.AddText(SharedGameResources.Pc!.Stats.GetLongClass());

            _cstat[CStatLevel].Tip.Clear();
            _cstat[CStatLevel].Tip.AddText(SharedResources.Msg!.GetV("XP: %lu", SharedGameResources.Pc!.Stats.Xp));
            if (SharedGameResources.Pc!.Stats.Level < SharedResources.Eset!.Xp.GetMaxLevel())
            {
                _cstat[CStatLevel].Tip.AddText(SharedResources.Msg!.GetV("Next: %lu", SharedResources.Eset!.Xp.GetLevelXP(SharedGameResources.Pc!.Stats.Level + 1)));
            }

            for (int j = 2; j < _cstat.Count; ++j)
            {
                _cstat[j].Tip.Clear();
                ss.Clear();
                ss.Append(_cstat[j].Label!.GetText());
                ss.Append(" (");
                ss.Append(SharedGameResources.Pc!.Stats.Primary[_baseStats[j - 2]]);
                if (SharedGameResources.Pc!.Stats.PrimaryAdditional[_baseStatsAdd[j - 2]] > 0)
                {
                    ss.Append('+');
                }
                if (SharedGameResources.Pc!.Stats.PrimaryAdditional[_baseStatsAdd[j - 2]] != 0)
                {
                    ss.Append(SharedGameResources.Pc!.Stats.PrimaryAdditional[_baseStatsAdd[j - 2]]);
                }
                ss.Append(')');
                _cstat[j].Tip.AddText(ss.ToString());

                bool haveBonus = false;
                int resourceStatOffset = Stats.Count + SharedResources.Eset!.DamageTypes.Count;

                for (int i = 0; i < Stats.Count; ++i)
                {
                    // insert resource stats (execpt stealing) before accuracy
                    if (i == Stats.Accuracy)
                    {
                        for (int k = 0; k < SharedResources.Eset!.ResourceStats.Stats.Count; ++k)
                        {
                            for (int l = 0; l < EngineSettings.ResourceStatsSettings.StatSteal; ++l)
                            {
                                int resourceIndex = resourceStatOffset + (k * EngineSettings.ResourceStatsSettings.StatCount) + l;

                                if (_baseBonus[j - 2]![resourceIndex] > 0 && _showStat[resourceIndex])
                                {
                                    if (!haveBonus)
                                    {
                                        _cstat[j].Tip.AddText("\n" + SharedResources.Msg!.Get("Related stats:"));
                                        haveBonus = true;
                                    }
                                    _cstat[j].Tip.AddText(SharedResources.Eset!.ResourceStats.Stats[k].Text[l]);
                                }
                            }
                        }
                    }

                    // damage types are displayed before absorb
                    if (i == Stats.AbsMin)
                    {
                        for (int k = 0; k < SharedResources.Eset!.DamageTypes.Types.Count; ++k)
                        {
                            // damage min
                            if (_baseBonus[j - 2]![Stats.Count + EngineSettings.DamageTypesSettings.IndexToMin(k)] > 0 && _showStat[Stats.Count + EngineSettings.DamageTypesSettings.IndexToMin(k)])
                            {
                                if (!haveBonus)
                                {
                                    _cstat[j].Tip.AddText("\n" + SharedResources.Msg!.Get("Related stats:"));
                                    haveBonus = true;
                                }
                                _cstat[j].Tip.AddText(SharedResources.Eset!.DamageTypes.Types[k].NameMin);
                            }
                            // damage max
                            if (_baseBonus[j - 2]![Stats.Count + EngineSettings.DamageTypesSettings.IndexToMax(k)] > 0 && _showStat[Stats.Count + EngineSettings.DamageTypesSettings.IndexToMax(k)])
                            {
                                if (!haveBonus)
                                {
                                    _cstat[j].Tip.AddText("\n" + SharedResources.Msg!.Get("Related stats:"));
                                    haveBonus = true;
                                }
                                _cstat[j].Tip.AddText(SharedResources.Eset!.DamageTypes.Types[k].NameMax);
                            }
                        }
                    }

                    // non-damage bonuses
                    if (_baseBonus[j - 2]![i] > 0 && _showStat[i])
                    {
                        if (!haveBonus)
                        {
                            _cstat[j].Tip.AddText("\n" + SharedResources.Msg!.Get("Related stats:"));
                            haveBonus = true;
                        }
                        _cstat[j].Tip.AddText(Stats.Name[i]);
                    }
                }

                // insert resource stealing stats after MP steal
                for (int i = 0; i < SharedResources.Eset!.ResourceStats.Stats.Count; ++i)
                {
                    for (int k = EngineSettings.ResourceStatsSettings.StatSteal; k < EngineSettings.ResourceStatsSettings.StatCount; ++k)
                    {
                        int resourceIndex = resourceStatOffset + (i * EngineSettings.ResourceStatsSettings.StatCount) + k;

                        if (_baseBonus[j - 2]![resourceIndex] > 0 && _showStat[resourceIndex])
                        {
                            if (!haveBonus)
                            {
                                _cstat[j].Tip.AddText("\n" + SharedResources.Msg!.Get("Related stats:"));
                                haveBonus = true;
                            }
                            _cstat[j].Tip.AddText(SharedResources.Eset!.ResourceStats.Stats[i].Text[k]);
                        }
                    }
                }

                // resistances
                for (int i = 0; i < SharedResources.Eset!.DamageTypes.Types.Count; ++i)
                {
                    if (_baseBonus[j - 2]![Stats.Count + EngineSettings.DamageTypesSettings.IndexToResist(i)] > 0 && _showStat[Stats.Count + EngineSettings.DamageTypesSettings.IndexToResist(i)])
                    {
                        if (!haveBonus)
                        {
                            _cstat[j].Tip.AddText("\n" + SharedResources.Msg!.Get("Related stats:"));
                            haveBonus = true;
                        }
                        _cstat[j].Tip.AddText(SharedResources.Eset!.DamageTypes.Types[i].NameResist);
                    }
                }
            }
        }

        /// <summary>
        /// Color-coding for positive/negative/no bonus
        /// </summary>
        private Color BonusColor(int stat)
        {
            if (stat > 0) return SharedResources.Font!.GetColor(FontEngine.ColorMenuBonus);
            if (stat < 0) return SharedResources.Font!.GetColor(FontEngine.ColorMenuPenalty);
            return SharedResources.Font!.GetColor(FontEngine.ColorMenuNormal);
        }

        private void TooltipCreateBonusText(int minIndex, int maxIndex, ref string minText, ref string? maxText)
        {
            // per-level bonus
            float minPerLevel = SharedGameResources.Pc!.Stats.PerLevel[minIndex];
            float maxPerLevel = SharedGameResources.Pc!.Stats.PerLevel[maxIndex];

            if (minPerLevel > 0)
            {
                minText += SharedResources.Msg!.GetV("Each level grants %s.", Utils.FloatToString(minPerLevel, SharedResources.Eset!.NumberFormat.CharacterMenu));
            }

            if (maxText != null && maxPerLevel > 0)
            {
                maxText += SharedResources.Msg!.GetV("Each level grants %s.", Utils.FloatToString(maxPerLevel, SharedResources.Eset!.NumberFormat.CharacterMenu));
            }

            // per-primary bonuses
            for (int i = 0; i < SharedResources.Eset!.PrimaryStats.Stats.Count; ++i)
            {
                float minPerPrimary = SharedGameResources.Pc!.Stats.PerPrimary[i][minIndex];
                float maxPerPrimary = SharedGameResources.Pc!.Stats.PerPrimary[i][maxIndex];

                if (minPerPrimary > 0)
                {
                    if (minText.Length != 0)
                        minText += "\n";

                    minText += SharedResources.Msg!.GetV("Each point of %s grants %s.", SharedResources.Eset!.PrimaryStats.Stats[i].Name, Utils.FloatToString(minPerPrimary, SharedResources.Eset!.NumberFormat.CharacterMenu));
                }

                if (maxText != null && maxPerPrimary > 0)
                {
                    if (maxText.Length != 0)
                        maxText += "\n";

                    maxText += SharedResources.Msg!.GetV("Each point of %s grants %s.", SharedResources.Eset!.PrimaryStats.Stats[i].Name, Utils.FloatToString(maxPerPrimary, SharedResources.Eset!.NumberFormat.CharacterMenu));
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
                string maxText = "";

                int maxIndex = Stats.AbsMax;

                TooltipCreateBonusText(minIndex, maxIndex, ref minText, ref maxText);

                if (minText.Length != 0)
                {
                    if (text.Length != 0)
                        text += "\n\n";
                    text += Stats.Name[minIndex] + ":\n" + minText;
                }

                if (maxText.Length != 0)
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
            string text = "";
            string minText = "";
            string maxText = "";

            string description = SharedResources.Eset!.DamageTypes.Types[dmgType].Description;
            if (description.Length != 0)
                text += description;

            int minIndex = Stats.Count + EngineSettings.DamageTypesSettings.IndexToMin(dmgType);
            int maxIndex = Stats.Count + EngineSettings.DamageTypesSettings.IndexToMax(dmgType);

            TooltipCreateBonusText(minIndex, maxIndex, ref minText, ref maxText);

            if (minText.Length != 0)
            {
                if (text.Length != 0)
                    text += "\n\n";
                text += SharedResources.Eset!.DamageTypes.Types[dmgType].NameMin + ":\n" + minText;
            }

            if (maxText.Length != 0)
            {
                if (text.Length != 0)
                    text += "\n\n";
                text += SharedResources.Eset!.DamageTypes.Types[dmgType].NameMax + ":\n" + maxText;
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
            string text = "";
            string bonusText = "";

            string description = SharedResources.Eset!.ResourceStats.Stats[resourceIndex].TextDesc[statIndex];
            if (description.Length != 0)
                text += description;

            int offsetIndex = Stats.Count + SharedResources.Eset!.DamageTypes.Count;
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

            Tablist.Logic();

            if (_closeButton!.CheckClick())
            {
                Visible = false;
                SharedResources.Snd!.Play(SfxClose, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);
            }

            bool haveSkillPoints = CheckSkillPoints();

            if (SharedGameResources.Pc!.Stats.Hp > 0 && haveSkillPoints)
            {
                for (int i = 0; i < SharedResources.Eset!.PrimaryStats.Stats.Count; ++i)
                {
                    if (SharedGameResources.Pc!.Stats.Primary[i] < SharedGameResources.Pc!.Stats.MaxPointsPerStat && !_cstat[i + 2].Label!.IsHidden())
                    {
                        _upgradeButton[i]!.Enabled = true;
                        _upgradeButton[i]!.Tooltip = SharedResources.Msg!.GetV("Upgrade Stat: %s\nUses 1 Stat Point", SharedResources.Eset!.PrimaryStats.Stats[i].Name);
                        Tablist.Add(_upgradeButton[i]!);
                    }
                    else
                    {
                        _upgradeButton[i]!.Enabled = false;
                        _upgradeButton[i]!.Tooltip = "";
                        Tablist.Remove(_upgradeButton[i]!);
                    }
                }

                for (int i = 0; i < SharedResources.Eset!.PrimaryStats.Stats.Count; ++i)
                {
                    if (_upgradeButton[i]!.CheckClick())
                        _primaryUp[i] = true;
                }
            }
            else
            {
                // no skill points to allocate; remove upgrade buttons
                for (int i = 0; i < SharedResources.Eset!.PrimaryStats.Stats.Count; ++i)
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

            if (SharedGameResources.Pc!.Stats.RefreshStats) RefreshStats();
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
            // check to see if there are skill points available
            if (SharedGameResources.Pc!.Stats.Hp > 0 && CheckSkillPoints())
            {
                for (int i = 0; i < SharedResources.Eset!.PrimaryStats.Stats.Count; ++i)
                {
                    if (_primaryUp[i])
                    {
                        SharedGameResources.Pc!.Stats.Primary[i]++;
                        SharedGameResources.Pc!.Stats.Recalc(); // equipment applied by MenuManager
                        _primaryUp[i] = false;
                        return true;
                    }
                }
            }

            return false;
        }

        private bool CheckSkillPoints()
        {
            int spent = 0;
            for (int i = 0; i < SharedResources.Eset!.PrimaryStats.Stats.Count; ++i)
            {
                spent += SharedGameResources.Pc!.Stats.Primary[i] - SharedGameResources.Pc!.Stats.PrimaryStarting[i];
            }

            _skillPoints = ((SharedGameResources.Pc!.Stats.Level - 1) * SharedGameResources.Pc!.Stats.StatPointsPerLevel) - spent;

            return (spent < ((SharedGameResources.Pc!.Stats.Level - 1) * SharedGameResources.Pc!.Stats.StatPointsPerLevel) && spent < SharedGameResources.Pc!.Stats.MaxSpendableStatPoints);
        }

        private void ParseShowStat(FileParser infile)
        {
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

            for (int i = 0; i < SharedResources.Eset!.DamageTypes.Types.Count; ++i)
            {
                if (statName == SharedResources.Eset!.DamageTypes.Types[i].Min)
                {
                    _showStat[offsetIndex + EngineSettings.DamageTypesSettings.IndexToMin(i)] = value;
                    return;
                }
                else if (statName == SharedResources.Eset!.DamageTypes.Types[i].Max)
                {
                    _showStat[offsetIndex + EngineSettings.DamageTypesSettings.IndexToMax(i)] = value;
                    return;
                }
                else if (statName == SharedResources.Eset!.DamageTypes.Types[i].Resist)
                {
                    _showStat[offsetIndex + EngineSettings.DamageTypesSettings.IndexToResist(i)] = value;
                    return;
                }
            }
            offsetIndex += SharedResources.Eset!.DamageTypes.Count;

            for (int i = 0; i < SharedResources.Eset!.ResourceStats.Stats.Count; ++i)
            {
                for (int j = 0; j < EngineSettings.ResourceStatsSettings.StatCount; ++j)
                {
                    if (statName == SharedResources.Eset!.ResourceStats.Stats[i].Ids[j])
                    {
                        _showStat[offsetIndex + (i * EngineSettings.ResourceStatsSettings.StatCount) + j] = value;
                        return;
                    }
                }
            }
            offsetIndex += SharedResources.Eset!.ResourceStats.StatCountValue;

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
