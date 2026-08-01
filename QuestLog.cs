// 对应 C++ 源文件：QuestLog.h + QuestLog.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。

namespace FlareEngine
{
    /// <summary>
    /// QuestLog
    ///
    /// 辅助文本，用于提醒玩家当前进行中的任务。
    /// 对应原始 <c>class QuestLog</c>。
    ///
    /// 持有的 <see cref="MenuLog"/> 引用由构造参数注入，本类不负责其生命周期；
    /// 原始 C++ 析构函数 <c>~QuestLog()</c> 为空，因此本类不实现 <see cref="IDisposable"/>。
    /// </summary>
    public class QuestLog
    {
        /// <summary>
        /// 对应原始嵌套 <c>class Quest</c>：单个任务的显示名称与完成状态 ID。
        /// </summary>
        private class Quest
        {
            public string Name = "";
            public StatusID CompleteStatus;
        }

        private readonly MenuLog _log;

        // 内层 vector 是每个任务的事件链，外层 vector 是任务列表。
        private readonly List<List<EventComponent>> _questSections = new List<List<EventComponent>>();

        private readonly List<int> _activeQuestIds = new List<int>();
        private readonly List<int> _completeQuestIds = new List<int>();
        private readonly List<Quest> _quests = new List<Quest>();

        /// <summary>
        /// 对应 C++ 原始公有字段 <c>newQuestNotification</c>（无 getter/setter 包装）。
        /// </summary>
        public bool NewQuestNotification;

        public QuestLog(MenuLog log)
        {
            _log = log;

            NewQuestNotification = false;
            LoadAll();
        }

        /// <summary>
        /// Load each [mod]/quests/index.txt file
        /// </summary>
        public void LoadAll()
        {
            // load each items.txt file. Individual item IDs can be overwritten with mods.
            List<string> files = SharedResources.Mods!.List("quests", !ModManager.ListFullPaths);
            files.Sort();
            for (int i = 0; i < files.Count; i++)
                Load(files[i]);
        }

        /// <summary>
        /// Load the quests in the specific quest file.
        /// Searches for the last-defined such file in all mods
        ///
        /// <param name="filename">The quest file name and extension, no path</param>
        /// </summary>
        public void Load(string filename)
        {
            using FileParser infile = new FileParser();
            // @CLASS QuestLog|Description of quest files in quests/
            if (!infile.Open(filename, FileParser.ModFile, FileParser.ErrorNormal))
                return;

            var msg = SharedResources.Msg!;
            var camp = SharedGameResources.Camp!;
            var eventm = SharedGameResources.Eventm!;

            _quests.Add(new Quest());

            while (infile.Next())
            {
                if (infile.NewSection)
                {
                    if (infile.Section == "quest")
                    {
                        _questSections.Add(new List<EventComponent>());
                    }
                }

                if (infile.Section == "")
                {
                    if (infile.Key == "name")
                    {
                        // @ATTR name|string|A displayed name for this quest.
                        _quests[^1].Name = msg.Get(infile.Val);
                    }
                    else if (infile.Key == "complete_status")
                    {
                        // @ATTR complete_status|string|If this status is set, the quest will be displayed as completed.
                        _quests[^1].CompleteStatus = camp.RegisterStatus(infile.Val);
                    }

                    continue;
                }

                if (_questSections.Count == 0)
                    continue;

                Event ev = new Event();
                if (infile.Key == "requires_status")
                {
                    // @ATTR quest.requires_status|list(string)|Quest requires this campaign status
                    eventm.LoadEventComponent(infile, ev, null);
                }
                else if (infile.Key == "requires_not_status")
                {
                    // @ATTR quest.requires_not_status|list(string)|Quest requires not having this campaign status.
                    eventm.LoadEventComponent(infile, ev, null);
                }
                else if (infile.Key == "requires_level")
                {
                    // @ATTR quest.requires_level|int|Quest requires hero level
                    eventm.LoadEventComponent(infile, ev, null);
                }
                else if (infile.Key == "requires_not_level")
                {
                    // @ATTR quest.requires_not_level|int|Quest requires not hero level
                    eventm.LoadEventComponent(infile, ev, null);
                }
                else if (infile.Key == "requires_currency")
                {
                    // @ATTR quest.requires_currency|int|Quest requires atleast this much currency
                    eventm.LoadEventComponent(infile, ev, null);
                }
                else if (infile.Key == "requires_not_currency")
                {
                    // @ATTR quest.requires_not_currency|int|Quest requires no more than this much currency
                    eventm.LoadEventComponent(infile, ev, null);
                }
                else if (infile.Key == "requires_item")
                {
                    // @ATTR quest.requires_item|list(item_id)|Quest requires specific item (not equipped)
                    eventm.LoadEventComponent(infile, ev, null);
                }
                else if (infile.Key == "requires_not_item")
                {
                    // @ATTR quest.requires_not_item|list(item_id)|Quest requires not having a specific item (not equipped)
                    eventm.LoadEventComponent(infile, ev, null);
                }
                else if (infile.Key == "requires_class")
                {
                    // @ATTR quest.requires_class|predefined_string|Quest requires this base class
                    eventm.LoadEventComponent(infile, ev, null);
                }
                else if (infile.Key == "requires_not_class")
                {
                    // @ATTR quest.requires_not_class|predefined_string|Quest requires not this base class
                    eventm.LoadEventComponent(infile, ev, null);
                }
                else if (infile.Key == "quest_text")
                {
                    // @ATTR quest.quest_text|string|Text that gets displayed in the Quest log when this quest is active.
                    EventComponent ec = new EventComponent();
                    ec.Type = EventComponent.QuestText;
                    ec.S = msg.Get(infile.Val);

                    // quest group id
                    ec.Data[0].Int = _quests.Count - 1;

                    ev.Components.Add(ec);
                }
                else
                {
                    Utils.LogError("QuestLog: %s is not a valid key.", infile.Key);
                }

                for (int i = 0; i < ev.Components.Count; ++i)
                {
                    if (ev.Components[i].Type != EventComponent.None)
                        _questSections[^1].Add(new EventComponent(ev.Components[i]));
                }
            }
            infile.Close();
        }

        public void Logic()
        {
            CreateQuestList();
        }

        /// <summary>
        /// All active quests are placed in the Quest tab of the Log Menu
        /// </summary>
        public void CreateQuestList()
        {
            var camp = SharedGameResources.Camp!;
            var msg = SharedResources.Msg!;
            var font = SharedResources.Font!;

            List<int> tempQuestIds = new List<int>();
            List<int> tempCompleteQuestIds = new List<int>();

            // check quest requirements
            for (int i = 0; i < _questSections.Count; i++)
            {
                if (camp.CheckRequirementsInVector(_questSections[i]))
                {
                    // passed requirement checks, add ID to active quest list
                    tempQuestIds.Add(i);
                }
            }

            // check quest completion status
            for (int i = 0; i < _quests.Count; ++i)
            {
                if (!string.IsNullOrEmpty(_quests[i].Name) && _quests[i].CompleteStatus != 0)
                {
                    if (camp.CheckStatus(_quests[i].CompleteStatus))
                    {
                        tempCompleteQuestIds.Add(i);
                    }
                }
            }

            // check if we actually need to update the quest log
            bool refreshQuestList = false;
            if (!tempQuestIds.SequenceEqual(_activeQuestIds) || !tempCompleteQuestIds.SequenceEqual(_completeQuestIds))
                refreshQuestList = true;

            // update the quest log
            if (refreshQuestList)
            {
                _activeQuestIds.Clear();
                _activeQuestIds.AddRange(tempQuestIds);
                _completeQuestIds.Clear();
                _completeQuestIds.AddRange(tempCompleteQuestIds);
                NewQuestNotification = true;

                _log.Clear(MenuLog.TypeQuests);

                bool completeHeader = false;
                for (int i = _completeQuestIds.Count; i > 0; i--)
                {
                    if (!completeHeader)
                    {
                        completeHeader = true;
                    }
                    _log.SetNextColor(font.GetColor(FontEngine.ColorWidgetDisabled), MenuLog.TypeQuests);
                    _log.Add(_quests[_completeQuestIds[i - 1]].Name, MenuLog.TypeQuests, WidgetLog.MsgUnique);
                }

                if (completeHeader)
                {
                    StringBuilder ss = new StringBuilder();
                    ss.Append(msg.Get("Completed Quests"));
                    ss.Append(" (");
                    ss.Append(_completeQuestIds.Count);
                    ss.Append(")");
                    _log.SetNextStyle(WidgetLog.FontBold, MenuLog.TypeQuests);
                    _log.SetNextColor(font.GetColor(FontEngine.ColorWidgetDisabled), MenuLog.TypeQuests);
                    _log.Add(ss.ToString(), MenuLog.TypeQuests, WidgetLog.MsgUnique);
                    if (_activeQuestIds.Count != 0)
                        _log.AddSeparator(MenuLog.TypeQuests);
                }

                for (int i = _activeQuestIds.Count; i > 0; i--)
                {
                    int k = _activeQuestIds[i - 1];

                    int iNext = (i > 1) ? i - 2 : 0;
                    int kNext = _activeQuestIds[iNext];

                    // get the group id of the next active quest
                    int nextQuestId = 0;
                    for (int j = 0; j < _questSections[kNext].Count; j++)
                    {
                        if (_questSections[kNext][j].Type == EventComponent.QuestText)
                        {
                            nextQuestId = _questSections[kNext][j].Data[0].Int;
                            break;
                        }
                    }

                    for (int j = 0; j < _questSections[k].Count; j++)
                    {
                        if (_questSections[k][j].Type == EventComponent.QuestText)
                        {
                            _log.Add(_questSections[k][j].S, MenuLog.TypeQuests, WidgetLog.MsgUnique);

                            int questId = _questSections[k][j].Data[0].Int;
                            if (nextQuestId != questId)
                            {
                                if (!string.IsNullOrEmpty(_quests[questId].Name))
                                {
                                    _log.SetNextStyle(WidgetLog.FontBold, MenuLog.TypeQuests);
                                    _log.Add(_quests[questId].Name, MenuLog.TypeQuests, WidgetLog.MsgUnique);
                                }

                                _log.AddSeparator(MenuLog.TypeQuests);
                            }
                            else if (i == 1)
                            {
                                if (!string.IsNullOrEmpty(_quests[questId].Name))
                                {
                                    _log.SetNextStyle(WidgetLog.FontBold, MenuLog.TypeQuests);
                                    _log.Add(_quests[questId].Name, MenuLog.TypeQuests, WidgetLog.MsgUnique);
                                }
                            }

                            break;
                        }
                    }
                }
            }
        }
    }
}
