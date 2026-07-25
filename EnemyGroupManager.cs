// <自动生成> 对应 C++ 源文件：EnemyGroupManager.h + EnemyGroupManager.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。

namespace FlareEngine
{
    /// <summary>
    /// EnemyLevel
    ///
    /// 对应 C++ 的 <c>class Enemy_Level</c>：描述单条"敌人条目"的类型名、等级与稀有度。
    /// C++ 原始类型按值（栈对象）在容器间传递与拷贝（<c>push_back</c>/按值返回），
    /// 因此这里选用 C# <c>struct</c>（值类型）而非 <c>class</c>，以精确保留原始的
    /// "每次拷贝都是独立副本"语义，避免多个分类列表意外共享同一引用。
    /// </summary>
    public struct EnemyLevel
    {
        public string Type;
        public int Level;
        public string Rarity;

        public EnemyLevel()
        {
            Type = "";
            Level = 0;
            Rarity = "common";
        }
    }

    /// <summary>
    /// EnemyGroupManager
    ///
    /// 将敌人加载到分类列表中，并管理生成随机分组的敌人。
    /// C++ 原始类不持有任何非托管资源，析构函数仅输出一条清理日志；
    /// 为保留同样的"对象生命周期结束时输出清理日志"语义，这里实现 <see cref="IDisposable"/>，
    /// 调用方应在不再需要该实例时调用 <see cref="Dispose"/>（对应原始 C++ 中 <c>delete</c> 该对象的时机）。
    /// </summary>
    public class EnemyGroupManager : IDisposable
    {
        /// <summary>容器，用于存储敌人数据。</summary>
        private readonly Dictionary<string, List<EnemyLevel>> _categories;

        public EnemyGroupManager()
        {
            _categories = new Dictionary<string, List<EnemyLevel>>();

            List<string> enemyPaths = SharedResources.Mods!.List("enemies", !ModManager.ListFullPaths);

            for (int i = 0; i < enemyPaths.Count; ++i)
            {
                using FileParser infile = new FileParser();

                // @CLASS EnemyGroupManager|Description of enemies in enemies/
                if (!infile.Open(enemyPaths[i], FileParser.ModFile, FileParser.ErrorNormal))
                    return;

                EnemyLevel newEnemy = new EnemyLevel();
                string categoryStr = "";
                infile.NewSection = true;
                bool first = true;
                while (infile.Next())
                {
                    if (infile.NewSection || first)
                    {
                        newEnemy.Type = enemyPaths[i];
                        first = false;
                    }

                    if (infile.Key == "level")
                    {
                        // @ATTR level|int|Level of the enemy
                        newEnemy.Level = Parse.ToInt(infile.Val);
                    }
                    else if (infile.Key == "rarity")
                    {
                        // @ATTR rarity|["common", "uncommon", "rare"]|Enemy rarity
                        newEnemy.Rarity = infile.Val;
                    }
                    else if (infile.Key == "categories")
                    {
                        // @ATTR categories|list(predefined_string)|Comma separated list of enemy categories
                        categoryStr = infile.Val;
                    }
                }
                infile.Close();

                string cat;
                while ((cat = Parse.PopFirstString(ref categoryStr)) != "")
                {
                    // C++ 的 std::map::operator[] 在键不存在时会自动创建一个空 vector 再 push_back；
                    // C# 的 Dictionary 索引器没有这种"自动补齐默认值"的行为，因此需要显式地
                    // TryGetValue + 按需创建列表，逻辑效果与原始的 map::operator[] 完全一致。
                    if (!_categories.TryGetValue(cat, out List<EnemyLevel>? categoryList))
                    {
                        categoryList = new List<EnemyLevel>();
                        _categories[cat] = categoryList;
                    }
                    categoryList.Add(newEnemy);
                }
            }
        }

        /// <summary>
        /// 对应 C++ 析构函数：输出一条清理日志。该类不持有任何非托管资源。
        /// </summary>
        public void Dispose()
        {
            Utils.LogInfo("Cleaning up: EnemyGroupManager");
        }

        /// <summary>获取具有指定特征的一个随机敌人。</summary>
        /// <param name="category">所需分类的敌人</param>
        /// <param name="minlevel">所需等级下限的敌人</param>
        /// <param name="maxlevel">所需等级上限的敌人</param>
        /// <returns>若找到合适条目，返回一个随机的敌人等级描述；若未找到，返回默认值。</returns>
        public EnemyLevel GetRandomEnemy(string category, int minlevel, int maxlevel)
        {
            List<EnemyLevel> enemyCategory;
            if (_categories.TryGetValue(category, out List<EnemyLevel>? foundCategory))
            {
                enemyCategory = foundCategory;
            }
            else
            {
                Utils.LogError("EnemyGroupManager: Could not find enemy category %s, returning empty enemy", category);
                return new EnemyLevel();
            }

            // load only the data that fit the criteria
            List<EnemyLevel> enemyCandidates = new List<EnemyLevel>();
            for (int i = 0; i < enemyCategory.Count; ++i)
            {
                EnemyLevel newEnemy = enemyCategory[i];
                if ((newEnemy.Level >= minlevel && newEnemy.Level <= maxlevel) || (minlevel == 0 && maxlevel == 0))
                {
                    // add more than one time to increase chance of getting
                    // this enemy as result, "rarity" property
                    int addTimes = 0;
                    if (newEnemy.Rarity == "common")
                    {
                        addTimes = 6;
                    }
                    else if (newEnemy.Rarity == "uncommon")
                    {
                        addTimes = 3;
                    }
                    else if (newEnemy.Rarity == "rare")
                    {
                        addTimes = 1;
                    }
                    else
                    {
                        Utils.LogError("EnemyGroupManager: 'rarity' property for enemy '%s' not valid (common|uncommon|rare): %s",
                                newEnemy.Type, newEnemy.Rarity);
                    }

                    // do add, the given number of times
                    for (int j = 0; j < addTimes; ++j)
                    {
                        enemyCandidates.Add(newEnemy);
                    }
                }
            }

            if (enemyCandidates.Count == 0)
            {
                Utils.LogError("EnemyGroupManager: Could not find a suitable enemy category for (%s, %d, %d)", category, minlevel, maxlevel);
                return new EnemyLevel();
            }
            else
            {
                // 对应原始 rand() % enemyCandidates.size()：Program.Rng 是本项目为替代
                // C 运行时全局 rand()/srand() 状态而建立的共享随机数源（详见 main.cs / Program.Rng），
                // 这里保持"生成整数后取模"的原始运算顺序不变，仅替换随机数来源。
                return enemyCandidates[Program.Rng.Next() % enemyCandidates.Count];
            }
        }

        /// <summary>获取属于某个分类的所有敌人。</summary>
        /// <param name="category">所需分类的敌人</param>
        /// <returns>该分类下敌人的等级描述列表；若未找到，返回空列表。</returns>
        public List<EnemyLevel> GetEnemiesInCategory(string category)
        {
            if (!_categories.TryGetValue(category, out List<EnemyLevel>? foundCategory))
            {
                Utils.LogError("EnemyGroupManager: Could not find enemy category %s, returning empty enemy list", category);
                return new List<EnemyLevel>();
            }
            return foundCategory;
        }
    }
}
