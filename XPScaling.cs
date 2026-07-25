// <自动生成> 对应 C++ 源文件：XPScaling.h + XPScaling.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。

namespace FlareEngine
{
    /// <summary>
    /// XPScalingTable
    ///
    /// 存储 absolute（按敌人绝对等级）与 relative（按与玩家等级差）两套 XP 倍率表。
    /// </summary>
    public class XPScalingTable
    {
        public Dictionary<int, float> Absolute = new Dictionary<int, float>();
        public Dictionary<int, float> Relative = new Dictionary<int, float>();

        public int AbsoluteLevelMin;
        public int AbsoluteLevelMax;
        public int RelativeLevelMin;
        public int RelativeLevelMax;

        public XPScalingTable()
        {
            AbsoluteLevelMin = 0;
            AbsoluteLevelMax = 0;
            RelativeLevelMin = 0;
            RelativeLevelMax = 0;
        }

        // 对应 C++ 析构函数 ~XPScalingTable()：函数体为空，无资源需要释放。
    }

    /// <summary>
    /// XPScaling
    ///
    /// 基于敌人等级的 XP 缩放。C++ 原始析构函数仅输出清理日志；
    /// 为保留同样的生命周期语义并实现 SharedGameResources 中的 Dispose 调用链，
    /// 此处实现 <see cref="IDisposable"/>。
    /// </summary>
    public class XPScaling : IDisposable
    {
        public Dictionary<XPScalingTableID, XPScalingTable> Tables = new Dictionary<XPScalingTableID, XPScalingTable>();

        public XPScaling()
        {
        }

        /// <summary>对应 C++ 析构函数 <c>XPScaling::~XPScaling()</c>。</summary>
        public void Dispose()
        {
            Utils.LogInfo("Cleaning up: XPScaling");
            GC.SuppressFinalize(this);
        }

        public XPScalingTableID Load(string filename)
        {
            XPScalingTableID tableId = (XPScalingTableID)Utils.HashString(filename);
            if (tableId == 0)
                return tableId;

            if (!Tables.ContainsKey(tableId))
                Tables[tableId] = new XPScalingTable();
            else
                return tableId;

            XPScalingTable table = Tables[tableId];

            using FileParser infile = new FileParser();
            // @CLASS XPScaling|Description of enemies/xp_scaling/
            if (infile.Open(filename, FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (infile.Section == "absolute")
                    {
                        if (infile.NewSection)
                        {
                            table.Absolute.Clear();
                        }

                        // @ATTR absolute.level|int, float : Level, Multiplier|The multiplier that will be applied to the rewarded XP when an enemy is at a specific level. If the enemy's level is outside the defined scaling levels, the min or max level is used (whichever is closer).
                        if (infile.Key == "level")
                        {
                            int level = Parse.PopFirstInt(ref infile.Val);
                            float multiplier = Parse.PopFirstFloat(ref infile.Val);

                            if (table.Absolute.Count == 0)
                            {
                                table.AbsoluteLevelMin = table.AbsoluteLevelMax = level;
                            }
                            else
                            {
                                if (level < table.AbsoluteLevelMin)
                                    table.AbsoluteLevelMin = level;
                                if (level > table.AbsoluteLevelMax)
                                    table.AbsoluteLevelMax = level;
                            }
                            table.Absolute[level] = multiplier;
                        }
                        else
                        {
                            infile.Error("XPScaling: '%s' is not a valid key.", infile.Key);
                        }
                    }
                    else if (infile.Section == "relative")
                    {
                        if (infile.NewSection)
                        {
                            table.Relative.Clear();
                        }

                        // @ATTR relative.level|int, float : Level, Multiplier|The multiplier that will be applied to the rewarded XP when an enemy's level is X levels apart from the player's level. If the enemy's level is outside the defined scaling levels, the min or max level is used (whichever is closer).
                        if (infile.Key == "level")
                        {
                            int level = Parse.PopFirstInt(ref infile.Val);
                            float multiplier = Parse.PopFirstFloat(ref infile.Val);

                            if (table.Relative.Count == 0)
                            {
                                table.RelativeLevelMin = table.RelativeLevelMax = level;
                            }
                            else
                            {
                                if (level < table.RelativeLevelMin)
                                    table.RelativeLevelMin = level;
                                if (level > table.RelativeLevelMax)
                                    table.RelativeLevelMax = level;
                            }
                            table.Relative[level] = multiplier;
                        }
                        else
                        {
                            infile.Error("XPScaling: '%s' is not a valid key.", infile.Key);
                        }
                    }
                }

                infile.Close();
            }

            // ensure there are no gaps in the level tables
            for (int i = table.AbsoluteLevelMax - 1; i > table.AbsoluteLevelMin; --i)
            {
                if (!table.Absolute.ContainsKey(i))
                {
                    table.Absolute[i] = table.Absolute[i + 1];
                }
            }
            for (int i = table.RelativeLevelMax - 1; i > table.RelativeLevelMin; --i)
            {
                if (!table.Relative.ContainsKey(i))
                {
                    table.Relative[i] = table.Relative[i + 1];
                }
            }

            return tableId;
        }

        public float GetMultiplier(StatBlock? enemyStats, StatBlock? playerStats)
        {
            float multiplier = 1;

            if (enemyStats == null || playerStats == null || enemyStats.XpScalingTable == 0)
                return multiplier;

            if (Tables.TryGetValue(enemyStats.XpScalingTable, out XPScalingTable? table))
            {
                if (table.Absolute.Count != 0)
                {
                    int level = enemyStats.Level;

                    if (table.Absolute.TryGetValue(level, out float absoluteMultiplier))
                    {
                        multiplier *= absoluteMultiplier;
                    }
                    else
                    {
                        if (level < table.AbsoluteLevelMin)
                            multiplier *= table.Absolute[table.AbsoluteLevelMin];
                        else if (level > table.AbsoluteLevelMax)
                            multiplier *= table.Absolute[table.AbsoluteLevelMax];
                    }
                }

                if (table.Relative.Count != 0)
                {
                    int level = enemyStats.Level - playerStats.Level;

                    if (table.Relative.TryGetValue(level, out float relativeMultiplier))
                    {
                        multiplier *= relativeMultiplier;
                    }
                    else
                    {
                        if (level < table.RelativeLevelMin)
                            multiplier *= table.Relative[table.RelativeLevelMin];
                        else if (level > table.RelativeLevelMax)
                            multiplier *= table.Relative[table.RelativeLevelMax];
                    }
                }
            }

            return multiplier;
        }
    }
}
