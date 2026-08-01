// 对应 C++ 源文件：EventManager.h + EventManager.cpp
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// EventComponent
    ///
    /// 描述一个事件的单个"组件"（一条 key=value 配置项被解析后的结果），
    /// 对应原始 <c>class EventComponent</c>。
    ///
    /// 命名与结构说明（需人工复核，详见转换报告）：
    /// 原始 <c>union ECData</c>（Int/Float/Bool 共享同一块内存）在本文件的全部调用点中，
    /// 每个 <see cref="Data"/> 下标始终只以单一类型读写（未观察到"写 Int、读 Bool"这类
    /// 跨类型复用同一下标的用法），因此这里改用三个互相独立的字段表达 <see cref="ECData"/>，
    /// 而不是使用 <c>[StructLayout(LayoutKind.Explicit)]</c> 显式内存重叠布局。
    /// 在当前已知调用点下二者行为等价，但如果未来存在跨类型复用同一下标的隐藏用法，
    /// 需要改回显式内存重叠布局。
    ///
    /// 由于 <see cref="Event"/> 在多处需要"值语义"的整体拷贝（如脚本缓存、延迟事件队列），
    /// 而 C# 引用类型不会在容器间自动深拷贝，这里为 <see cref="EventComponent"/> 额外提供了
    /// 一个拷贝构造函数（原始 C++ 版本依赖编译器自动生成的成员逐一拷贝构造函数，语义等价）。
    /// </summary>
    public class EventComponent
    {
        // 对应原始头文件中第一个匿名 enum（组件类型）。
        public const int None = 0;
        public const int Tooltip = 1;
        public const int Intermap = 2;
        public const int IntermapID = 3;
        public const int Intramap = 4;
        public const int Mapmod = 5;
        public const int MapmodToggle = 6;
        public const int Soundfx = 7;
        public const int Loot = 8;
        public const int LootCount = 9;
        public const int Msg = 10;
        public const int Shakycam = 11;
        public const int RequiresStatus = 12;
        public const int RequiresNotStatus = 13;
        public const int RequiresLevel = 14;
        public const int RequiresNotLevel = 15;
        public const int RequiresCurrency = 16;
        public const int RequiresNotCurrency = 17;
        public const int RequiresItem = 18;
        public const int RequiresNotItem = 19;
        public const int RequiresClass = 20;
        public const int RequiresNotClass = 21;
        public const int RequiresTile = 22;
        public const int RequiresNotTile = 23;
        public const int SetStatus = 24;
        public const int UnsetStatus = 25;
        public const int RemoveCurrency = 26;
        public const int RemoveItem = 27;
        public const int RewardXp = 28;
        public const int RewardCurrency = 29;
        public const int RewardItem = 30;
        public const int RewardLoot = 31;
        public const int RewardLootCount = 32;
        public const int Restore = 33;
        public const int Power = 34;
        public const int PowerPath = 35;
        public const int PowerDamage = 36;
        public const int PowerStats = 37;
        public const int PowerLevel = 38;
        public const int Spawn = 39;
        public const int SpawnLevel = 40;
        public const int Stash = 41;
        public const int Npc = 42;
        public const int Music = 43;
        public const int Cutscene = 44;
        public const int Repeat = 45;
        public const int SaveGame = 46;
        public const int Book = 47;
        public const int Script = 48;
        public const int ChanceExec = 49;
        public const int Respec = 50;
        public const int ShowOnMinimap = 51;
        public const int ParallaxLayers = 52;
        public const int RandomStatus = 53;
        public const int ProcgenFilename = 54;
        public const int ProcgenLink = 55;
        public const int ProcgenDoorLevel = 56;
        public const int NpcID = 57;
        public const int NpcHotspot = 58;
        public const int NpcDialogThem = 59;
        public const int NpcDialogYou = 60;
        public const int NpcVoice = 61;
        public const int NpcDialogTopic = 62;
        public const int NpcDialogGroup = 63;
        public const int NpcDialogID = 64;
        public const int NpcDialogResponse = 65;
        public const int NpcDialogResponseOnly = 66;
        public const int NpcAllowMovement = 67;
        public const int NpcPortraitThem = 68;
        public const int NpcPortraitYou = 69;
        public const int QuestText = 70;
        public const int WasInsideEventArea = 71;
        public const int NpcTakeAParty = 72;
        public const int EventComponentCount = 73;

        // 对应原始头文件中第二个匿名 enum（random_status 的操作模式）。
        public const int RandomStatusModeAppend = 1;
        public const int RandomStatusModeClear = 2;
        public const int RandomStatusModeRoll = 3;
        public const int RandomStatusModeSet = 4;
        public const int RandomStatusModeUnset = 5;

        public const int DataCount = 11;

        /// <summary>
        /// 对应原始 <c>union ECData</c>。详见类级注释中的说明：由于未观察到跨类型复用同一
        /// 下标的用法，这里使用三个独立字段代替显式内存重叠。
        /// </summary>
        public struct ECData
        {
            public int Int;
            public float Float;
            public bool Bool;
        }

        public int Type;
        public string S = "";
        public StatusID Status;
        public int Id;
        public ECData[] Data = new ECData[DataCount];

        public EventComponent()
        {
            Type = None;
            S = "";
            Status = 0;
            Id = 0;
            for (int x = 0; x < DataCount; ++x)
            {
                Data[x].Int = 0;
            }
        }

        /// <summary>
        /// 拷贝构造函数（原始 C++ 依赖编译器生成的默认成员逐一拷贝，语义等价）。
        /// 详见类级注释"资源管理"说明。
        /// </summary>
        public EventComponent(EventComponent other)
        {
            Type = other.Type;
            S = other.S;
            Status = other.Status;
            Id = other.Id;
            Data = (ECData[])other.Data.Clone();
        }
    }

    /// <summary>
    /// Event
    ///
    /// 描述地图/NPC 中的一个事件（触发条件 + 一组 <see cref="EventComponent"/>），
    /// 对应原始 <c>class Event</c>。
    ///
    /// 资源管理（需人工复核，详见转换报告）：原始 C++ 版本在按值拷贝 <c>Event</c>
    /// （脚本缓存 std::map、mapr-&gt;delayed_events 等容器的整体赋值/push_back）时，
    /// 编译器生成的拷贝构造函数会对 <c>components</c> 向量做深拷贝。C# 中
    /// <see cref="Event"/> 是引用类型，直接复用引用会导致多个容器共享同一个实例，
    /// 与原始值语义不符，因此这里额外提供了拷贝构造函数，并在
    /// <see cref="EventManager"/> 中所有"值拷贝进容器"的位置显式调用它。
    /// </summary>
    public class Event
    {
        public const int ActivateOnTrigger = 0;
        public const int ActivateOnInteract = 1;
        public const int ActivateOnMapexit = 2;
        public const int ActivateOnLeave = 3;
        public const int ActivateOnLoad = 4;
        public const int ActivateOnClear = 5;
        public const int ActivateStatic = 6;

        public string Type = "";
        public int ActivateType;
        public List<EventComponent> Components = new List<EventComponent>();
        public Rectangle Location;
        public Rectangle Hotspot;
        /// <summary>events that run multiple times pause this long in frames（原始注释）。</summary>
        public Timer Cooldown;
        public Timer Delay;
        /// <summary>if this event has been triggered once, should this event be kept? If so, this event can be triggered multiple times.（原始注释）</summary>
        public bool KeepAfterTrigger;
        public Vector2 Center;
        public Rectangle ReachableFrom;

        public Event()
        {
            Type = "";
            ActivateType = ActivateOnTrigger;
            Components = new List<EventComponent>();
            Location = new Rectangle();
            Hotspot = new Rectangle();
            Cooldown = new Timer();
            Delay = new Timer();
            KeepAfterTrigger = true;
            Center = new Vector2(-1, -1);
            ReachableFrom = new Rectangle();
        }

        /// <summary>
        /// 拷贝构造函数（原始 C++ 依赖编译器生成的默认成员逐一拷贝，包括 std::vector
        /// 的深拷贝，语义等价）。详见类级注释"资源管理"说明。Timer 没有拷贝构造函数，
        /// 这里沿用 output/EffectManager.cs 中已建立的惯例：新建实例后逐字段复制
        /// Duration/Current。
        /// </summary>
        public Event(Event other)
        {
            Type = other.Type;
            ActivateType = other.ActivateType;
            Components = new List<EventComponent>();
            for (int i = 0; i < other.Components.Count; i++)
            {
                Components.Add(new EventComponent(other.Components[i]));
            }
            Location = other.Location;
            Hotspot = other.Hotspot;
            Cooldown = new Timer();
            Cooldown.Duration = other.Cooldown.Duration;
            Cooldown.Current = other.Cooldown.Current;
            Delay = new Timer();
            Delay.Duration = other.Delay.Duration;
            Delay.Current = other.Delay.Current;
            KeepAfterTrigger = other.KeepAfterTrigger;
            Center = other.Center;
            ReachableFrom = other.ReachableFrom;
        }

        /// <summary>
        /// returns a pointer to the event component within the components list
        /// no need to free the pointer by caller
        /// NULL will be returned if no such event is found（原始注释）
        /// </summary>
        public EventComponent? GetComponent(int type)
        {
            for (int it = 0; it < Components.Count; ++it)
                if (Components[it].Type == type)
                    return Components[it];
            return null;
        }

        /// <summary>
        /// 对应原始 <c>Event::deleteAllComponents</c>。
        /// 需人工复核：原始实现存在"erase 后再自增迭代器"的经典越界跳过缺陷——
        /// <c>it = components.erase(it)</c> 已经让 <c>it</c> 指向被移除元素之后的下一个元素，
        /// 紧接着 for 循环头部的 <c>++it</c> 又会再前进一步，导致紧邻的连续匹配项被跳过、
        /// 不会被移除。这里用等价的下标写法完整保留了这一原始（有缺陷的）行为，
        /// 未做"修正"。
        /// </summary>
        public void DeleteAllComponents(int type)
        {
            for (int it = 0; it < Components.Count; ++it)
                if (Components[it].Type == type)
                    Components.RemoveAt(it);
        }
    }

    /// <summary>
    /// EventManager
    ///
    /// 负责事件（<see cref="Event"/>/<see cref="EventComponent"/>）的加载与执行，
    /// 对应原始 <c>class EventManager</c>。
    /// </summary>
    public class EventManager : IDisposable
    {
        public EventManager()
        {
        }

        /// <summary>对应析构函数 <c>~EventManager()</c>。</summary>
        public void Dispose()
        {
            Utils.LogInfo("Cleaning up: EventManager");
            GC.SuppressFinalize(this);
        }

        public void LoadEvent(FileParser infile, Event? evnt)
        {
            if (evnt == null) return;
            // @CLASS EventManager|Description of events in maps/ and npcs/

            if (infile.Key == "type")
            {
                // @ATTR event.type|string|(IGNORED BY ENGINE) The "type" field, as used by Tiled and other mapping tools.
                evnt.Type = infile.Val;
            }
            else if (infile.Key == "activate")
            {
                // @ATTR event.activate|["on_trigger", "on_interact", "on_load", "on_leave", "on_mapexit", "on_clear", "static"]|Set the state in which the event will be activated (map events only). on_trigger = the player is standing in the event area or the player interacts with the hotspot. on_interact = the player ineracts with the hotspot. on_mapexit = as the player leaves the map. on_leave = as the player steps outside of an event area they were previously inside of. on_load = as the player enters a map. on_clear = all of the enemies on a map have been defeated. static = constantly, every frame.
                if (infile.Val == "on_trigger")
                {
                    evnt.ActivateType = Event.ActivateOnTrigger;
                }
                else if (infile.Val == "on_interact")
                {
                    evnt.ActivateType = Event.ActivateOnInteract;
                }
                else if (infile.Val == "on_mapexit")
                {
                    // no need to set keep_after_trigger to false correctly, it's ignored anyway
                    evnt.ActivateType = Event.ActivateOnMapexit;
                }
                else if (infile.Val == "on_leave")
                {
                    evnt.ActivateType = Event.ActivateOnLeave;
                }
                else if (infile.Val == "on_load")
                {
                    evnt.ActivateType = Event.ActivateOnLoad;
                    evnt.KeepAfterTrigger = false;
                }
                else if (infile.Val == "on_clear")
                {
                    evnt.ActivateType = Event.ActivateOnClear;
                    evnt.KeepAfterTrigger = false;
                }
                else if (infile.Val == "static")
                {
                    evnt.ActivateType = Event.ActivateStatic;
                }
                else
                {
                    infile.Error("EventManager: Event activation type '%s' unknown. Defaulting to 'on_trigger'.", infile.Val);
                    evnt.ActivateType = Event.ActivateOnTrigger;
                }
            }
            else if (infile.Key == "location")
            {
                // @ATTR event.location|rectangle|Defines the location area for the event.
                evnt.Location.X = Parse.PopFirstInt(ref infile.Val);
                evnt.Location.Y = Parse.PopFirstInt(ref infile.Val);
                evnt.Location.Width = Parse.PopFirstInt(ref infile.Val);
                evnt.Location.Height = Parse.PopFirstInt(ref infile.Val);

                if (evnt.Center.X == -1 && evnt.Center.Y == -1)
                {
                    evnt.Center.X = (float)evnt.Location.X + (float)evnt.Location.Width / 2;
                    evnt.Center.Y = (float)evnt.Location.Y + (float)evnt.Location.Height / 2;
                }
            }
            else if (infile.Key == "hotspot")
            {
                //  @ATTR event.hotspot|["location", rectangle]|Event uses location as hotspot or defined by rect.
                if (infile.Val == "location")
                {
                    evnt.Hotspot.X = evnt.Location.X;
                    evnt.Hotspot.Y = evnt.Location.Y;
                    evnt.Hotspot.Width = evnt.Location.Width;
                    evnt.Hotspot.Height = evnt.Location.Height;
                }
                else
                {
                    evnt.Hotspot.X = Parse.PopFirstInt(ref infile.Val);
                    evnt.Hotspot.Y = Parse.PopFirstInt(ref infile.Val);
                    evnt.Hotspot.Width = Parse.PopFirstInt(ref infile.Val);
                    evnt.Hotspot.Height = Parse.PopFirstInt(ref infile.Val);
                }

                evnt.Center.X = (float)evnt.Hotspot.X + (float)evnt.Hotspot.Width / 2;
                evnt.Center.Y = (float)evnt.Hotspot.Y + (float)evnt.Hotspot.Height / 2;
            }
            else if (infile.Key == "cooldown")
            {
                // @ATTR event.cooldown|duration|Duration for event cooldown in 'ms' or 's'.
                evnt.Cooldown.Duration = (uint)Parse.ToDuration(infile.Val);
                evnt.Cooldown.Reset(Timer.End);
            }
            else if (infile.Key == "delay")
            {
                // @ATTR event.delay|duration|Event will execute after a specified duration.
                evnt.Delay.Duration = (uint)Parse.ToDuration(infile.Val);
                evnt.Delay.Reset(Timer.End);
            }
            else if (infile.Key == "reachable_from")
            {
                // @ATTR event.reachable_from|rectangle|If the hero is inside this rectangle, they can activate the event.
                evnt.ReachableFrom.X = Parse.PopFirstInt(ref infile.Val);
                evnt.ReachableFrom.Y = Parse.PopFirstInt(ref infile.Val);
                evnt.ReachableFrom.Width = Parse.PopFirstInt(ref infile.Val);
                evnt.ReachableFrom.Height = Parse.PopFirstInt(ref infile.Val);
            }
            else
            {
                LoadEventComponent(infile, evnt, null);
            }
        }

        public void LoadEventComponent(FileParser infile, Event? evnt, EventComponent? ec)
        {
            if (!LoadEventComponentString(infile.Key, ref infile.Val, evnt, ec))
            {
                infile.Error("EventManager: '%s' is not a valid key.", infile.Key);
            }
        }

        public bool LoadEventComponentString(string key, ref string val, Event? evnt, EventComponent? ec)
        {
            var msg = SharedResources.Msg!;
            var loot = SharedGameResources.Loot!;
            var camp = SharedGameResources.Camp!;
            var items = SharedGameResources.Items!;

            EventComponent? e = null;
            if (evnt != null)
            {
                evnt.Components.Add(new EventComponent());
                e = evnt.Components[^1];
            }
            else if (ec != null)
            {
                e = ec;
            }

            if (e == null) return true;

            e.Type = EventComponent.None;

            if (key == "tooltip")
            {
                // @ATTR event.tooltip|string|Tooltip for event
                e.Type = EventComponent.Tooltip;

                e.S = msg.Get(val);
            }
            else if (key == "intermap")
            {
                // @ATTR event.intermap|filename, int, int, string : Map file, X, Y, Spawn point ID|Jump to specific map. X/Y (optional) can be used to specifiy the spawn position. Alternatively, use position (-1, -1) and an ID to use the location of an intermap_id event as the spawn point.
                e.Type = EventComponent.Intermap;

                e.S = Parse.PopFirstString(ref val);
                e.Data[0].Int = -1;
                e.Data[1].Int = -1;

                string testX = Parse.PopFirstString(ref val);
                if (!string.IsNullOrEmpty(testX))
                {
                    e.Data[0].Int = Parse.ToInt(testX);
                    e.Data[1].Int = Parse.PopFirstInt(ref val);

                    string testSpawnId = Parse.PopFirstString(ref val);
                    if (!string.IsNullOrEmpty(testSpawnId))
                    {
                        e.Data[3].Int = GetIntermapID(testSpawnId);
                    }
                }

                e.Data[2].Bool = false; // not a map list
            }
            else if (key == "intermap_id")
            {
                // @ATTR event.intermap_id|string|Identifier used to specify an alternate spawn point for intermap events.
                e.Type = EventComponent.IntermapID;

                e.Data[0].Int = GetIntermapID(val);
            }
            else if (key == "intermap_random")
            {
                // @ATTR event.intermap_random|filename|Pick a random map from a map list file and teleport to it.
                e.Type = EventComponent.Intermap;

                e.S = Parse.PopFirstString(ref val);
                e.Data[2].Bool = true; // flag that tells an intermap event that it contains a map list
            }
            else if (key == "intramap")
            {
                // @ATTR event.intramap|int, int : X, Y|Jump to specific position within current map.
                e.Type = EventComponent.Intramap;

                e.Data[0].Int = Parse.PopFirstInt(ref val);
                e.Data[1].Int = Parse.PopFirstInt(ref val);
            }
            else if (key == "mapmod")
            {
                // @ATTR event.mapmod|list(predefined_string, int, int, int) : Layer, X, Y, Tile ID|Modify map tiles
                e.Type = EventComponent.Mapmod;

                e.S = Parse.PopFirstString(ref val);
                e.Data[0].Int = Parse.PopFirstInt(ref val);
                e.Data[1].Int = Parse.PopFirstInt(ref val);
                e.Data[2].Int = Parse.PopFirstInt(ref val);

                // add repeating mapmods
                if (evnt != null)
                {
                    string repeatVal = Parse.PopFirstString(ref val);
                    while (repeatVal != "")
                    {
                        evnt.Components.Add(new EventComponent());
                        e = evnt.Components[^1];
                        e.Type = EventComponent.Mapmod;
                        e.S = repeatVal;
                        e.Data[0].Int = Parse.PopFirstInt(ref val);
                        e.Data[1].Int = Parse.PopFirstInt(ref val);
                        e.Data[2].Int = Parse.PopFirstInt(ref val);

                        repeatVal = Parse.PopFirstString(ref val);
                    }
                }
            }
            else if (key == "mapmod_toggle")
            {
                // @ATTR event.mapmod_toggle|list(predefined_string, int, int, int, int) : Layer, X, Y, Tile ID A, Tile ID B|Same as mapmod, except this will alternate between two tile IDs.
                e.Type = EventComponent.MapmodToggle;

                e.S = Parse.PopFirstString(ref val);
                e.Data[0].Int = Parse.PopFirstInt(ref val);
                e.Data[1].Int = Parse.PopFirstInt(ref val);
                e.Data[2].Int = Parse.PopFirstInt(ref val);
                e.Data[3].Int = Parse.PopFirstInt(ref val);

                // add repeating mapmods
                if (evnt != null)
                {
                    string repeatVal = Parse.PopFirstString(ref val);
                    while (repeatVal != "")
                    {
                        evnt.Components.Add(new EventComponent());
                        e = evnt.Components[^1];
                        e.Type = EventComponent.MapmodToggle;
                        e.S = repeatVal;
                        e.Data[0].Int = Parse.PopFirstInt(ref val);
                        e.Data[1].Int = Parse.PopFirstInt(ref val);
                        e.Data[2].Int = Parse.PopFirstInt(ref val);
                        e.Data[3].Int = Parse.PopFirstInt(ref val);

                        repeatVal = Parse.PopFirstString(ref val);
                    }
                }
            }
            else if (key == "soundfx")
            {
                // @ATTR event.soundfx|filename, int, int, bool : Sound file, X, Y, loop|Filename of a sound to play. Optionally, it can be played at a specific location and/or looped. Note: Sounds attached to 'on_load' events will loop by default.
                e.Type = EventComponent.Soundfx;

                e.S = Parse.PopFirstString(ref val);
                e.Data[0].Int = -1;
                e.Data[1].Int = -1;
                e.Data[2].Bool = false;
                e.Data[3].Bool = false; // flag to determine if looping is explicitly set

                string s = Parse.PopFirstString(ref val);
                if (!string.IsNullOrEmpty(s))
                    e.Data[0].Int = Parse.ToInt(s);

                s = Parse.PopFirstString(ref val);
                if (!string.IsNullOrEmpty(s))
                    e.Data[1].Int = Parse.ToInt(s);

                s = Parse.PopFirstString(ref val);
                if (!string.IsNullOrEmpty(s))
                {
                    e.Data[2].Bool = Parse.ToBool(s);
                    e.Data[3].Bool = true;
                }
            }
            else if (key == "loot")
            {
                // @ATTR event.loot|list(loot)|Add loot to the event.
                e.Type = EventComponent.Loot;

                loot.ParseLoot(ref val, e, evnt!.Components);
            }
            else if (key == "loot_count")
            {
                // @ATTR event.loot_count|int, int : Min, Max|Sets the minimum (and optionally, the maximum) amount of loot this event can drop. Overrides the global drop_max setting.
                e.Type = EventComponent.LootCount;

                e.Data[0].Int = Parse.PopFirstInt(ref val);
                e.Data[1].Int = Parse.PopFirstInt(ref val);
                if (e.Data[0].Int != 0 || e.Data[1].Int != 0)
                {
                    e.Data[0].Int = Math.Max(e.Data[0].Int, 1);
                    e.Data[1].Int = Math.Max(e.Data[1].Int, e.Data[0].Int);
                }
            }
            else if (key == "msg")
            {
                // @ATTR event.msg|string|Adds a message to be displayed for the event.
                e.Type = EventComponent.Msg;

                e.S = msg.Get(val);
            }
            else if (key == "shakycam")
            {
                // @ATTR event.shakycam|duration|Makes the camera shake for this duration in 'ms' or 's'.
                e.Type = EventComponent.Shakycam;

                e.Data[0].Int = Parse.ToDuration(val);
            }
            else if (key == "requires_status")
            {
                // @ATTR event.requires_status|list(string)|Event requires list of statuses
                e.Type = EventComponent.RequiresStatus;

                e.S = Parse.PopFirstString(ref val);
                e.Status = camp.RegisterStatus(e.S);

                // add repeating requires_status
                if (evnt != null)
                {
                    string repeatVal = Parse.PopFirstString(ref val);
                    while (repeatVal != "")
                    {
                        evnt.Components.Add(new EventComponent());
                        e = evnt.Components[^1];
                        e.Type = EventComponent.RequiresStatus;
                        e.S = repeatVal;
                        e.Status = camp.RegisterStatus(repeatVal);

                        repeatVal = Parse.PopFirstString(ref val);
                    }
                }
            }
            else if (key == "requires_not_status")
            {
                // @ATTR event.requires_not_status|list(string)|Event requires not list of statuses
                e.Type = EventComponent.RequiresNotStatus;

                e.S = Parse.PopFirstString(ref val);
                e.Status = camp.RegisterStatus(e.S);

                // add repeating requires_not
                if (evnt != null)
                {
                    string repeatVal = Parse.PopFirstString(ref val);
                    while (repeatVal != "")
                    {
                        evnt.Components.Add(new EventComponent());
                        e = evnt.Components[^1];
                        e.Type = EventComponent.RequiresNotStatus;
                        e.S = repeatVal;
                        e.Status = camp.RegisterStatus(repeatVal);

                        repeatVal = Parse.PopFirstString(ref val);
                    }
                }
            }
            else if (key == "requires_level")
            {
                // @ATTR event.requires_level|int|Event requires hero level
                e.Type = EventComponent.RequiresLevel;

                e.Data[0].Int = Parse.PopFirstInt(ref val);
            }
            else if (key == "requires_not_level")
            {
                // @ATTR event.requires_not_level|int|Event requires not hero level
                e.Type = EventComponent.RequiresNotLevel;

                e.Data[0].Int = Parse.PopFirstInt(ref val);
            }
            else if (key == "requires_currency")
            {
                // @ATTR event.requires_currency|int|Event requires atleast this much currency
                e.Type = EventComponent.RequiresCurrency;

                e.Data[0].Int = Parse.PopFirstInt(ref val);
            }
            else if (key == "requires_not_currency")
            {
                // @ATTR event.requires_not_currency|int|Event requires no more than this much currency
                e.Type = EventComponent.RequiresNotCurrency;

                e.Data[0].Int = Parse.PopFirstInt(ref val);
            }
            else if (key == "requires_item")
            {
                // @ATTR event.requires_item|list(item_id)|Event requires specific item (not equipped). Quantity can be specified by appending ":Q" to the item_id, where Q is an integer.
                e.Type = EventComponent.RequiresItem;

                ItemStack itemStack = Parse.ToItemQuantityPair(Parse.PopFirstString(ref val));
                e.Id = itemStack.Item;
                e.Data[0].Int = itemStack.Quantity;

                // add repeating requires_item
                if (evnt != null)
                {
                    string repeatVal = Parse.PopFirstString(ref val);
                    while (repeatVal != "")
                    {
                        evnt.Components.Add(new EventComponent());
                        e = evnt.Components[^1];
                        e.Type = EventComponent.RequiresItem;
                        itemStack = Parse.ToItemQuantityPair(repeatVal);
                        e.Id = itemStack.Item;
                        e.Data[0].Int = itemStack.Quantity;

                        repeatVal = Parse.PopFirstString(ref val);
                    }
                }
            }
            else if (key == "requires_not_item")
            {
                // @ATTR event.requires_not_item|list(item_id)|Event requires not having a specific item (not equipped). Quantity can be specified by appending ":Q" to the item_id, where Q is an integer.
                e.Type = EventComponent.RequiresNotItem;

                ItemStack itemStack = Parse.ToItemQuantityPair(Parse.PopFirstString(ref val));
                e.Id = itemStack.Item;
                e.Data[0].Int = itemStack.Quantity;

                // add repeating requires_not_item
                if (evnt != null)
                {
                    string repeatVal = Parse.PopFirstString(ref val);
                    while (repeatVal != "")
                    {
                        evnt.Components.Add(new EventComponent());
                        e = evnt.Components[^1];
                        e.Type = EventComponent.RequiresNotItem;
                        itemStack = Parse.ToItemQuantityPair(repeatVal);
                        e.Id = itemStack.Item;
                        e.Data[0].Int = itemStack.Quantity;

                        repeatVal = Parse.PopFirstString(ref val);
                    }
                }
            }
            else if (key == "requires_class")
            {
                // @ATTR event.requires_class|predefined_string|Event requires this base class
                e.Type = EventComponent.RequiresClass;

                e.S = Parse.PopFirstString(ref val);
            }
            else if (key == "requires_not_class")
            {
                // @ATTR event.requires_not_class|predefined_string|Event requires not this base class
                e.Type = EventComponent.RequiresNotClass;

                e.S = Parse.PopFirstString(ref val);
            }
            else if (key == "requires_tile")
            {
                // @ATTR event.requires_tile|list(predefined_string, int, int, int) : Layer, X, Y, Tile ID|Event requires tile at this layer and X/Y to match the ID
                e.Type = EventComponent.RequiresTile;

                e.S = Parse.PopFirstString(ref val);
                e.Id = (int)Utils.HashString(e.S);
                e.Data[0].Int = Parse.PopFirstInt(ref val);
                e.Data[1].Int = Parse.PopFirstInt(ref val);
                e.Data[2].Int = Parse.PopFirstInt(ref val);

                // add repeating tiles
                if (evnt != null)
                {
                    string repeatVal = Parse.PopFirstString(ref val);
                    while (repeatVal != "")
                    {
                        evnt.Components.Add(new EventComponent());
                        e = evnt.Components[^1];
                        e.Type = EventComponent.RequiresTile;
                        e.S = repeatVal;
                        e.Id = (int)Utils.HashString(e.S);
                        e.Data[0].Int = Parse.PopFirstInt(ref val);
                        e.Data[1].Int = Parse.PopFirstInt(ref val);
                        e.Data[2].Int = Parse.PopFirstInt(ref val);

                        repeatVal = Parse.PopFirstString(ref val);
                    }
                }
            }
            else if (key == "requires_not_tile")
            {
                // @ATTR event.requires_not_tile|list(predefined_string, int, int, int) : Layer, X, Y, Tile ID|Event requires tile at this layer and X/Y to not match the ID
                e.Type = EventComponent.RequiresNotTile;

                e.S = Parse.PopFirstString(ref val);
                e.Id = (int)Utils.HashString(e.S);
                e.Data[0].Int = Parse.PopFirstInt(ref val);
                e.Data[1].Int = Parse.PopFirstInt(ref val);
                e.Data[2].Int = Parse.PopFirstInt(ref val);

                // add repeating tiles
                if (evnt != null)
                {
                    string repeatVal = Parse.PopFirstString(ref val);
                    while (repeatVal != "")
                    {
                        evnt.Components.Add(new EventComponent());
                        e = evnt.Components[^1];
                        e.Type = EventComponent.RequiresNotTile;
                        e.S = repeatVal;
                        e.Id = (int)Utils.HashString(e.S);
                        e.Data[0].Int = Parse.PopFirstInt(ref val);
                        e.Data[1].Int = Parse.PopFirstInt(ref val);
                        e.Data[2].Int = Parse.PopFirstInt(ref val);

                        repeatVal = Parse.PopFirstString(ref val);
                    }
                }
            }
            else if (key == "set_status")
            {
                // @ATTR event.set_status|list(string)|Sets specified statuses
                e.Type = EventComponent.SetStatus;

                e.S = Parse.PopFirstString(ref val);
                e.Status = camp.RegisterStatus(e.S);

                // add repeating set_status
                if (evnt != null)
                {
                    string repeatVal = Parse.PopFirstString(ref val);
                    while (repeatVal != "")
                    {
                        evnt.Components.Add(new EventComponent());
                        e = evnt.Components[^1];
                        e.Type = EventComponent.SetStatus;
                        e.S = repeatVal;
                        e.Status = camp.RegisterStatus(repeatVal);

                        repeatVal = Parse.PopFirstString(ref val);
                    }
                }
            }
            else if (key == "unset_status")
            {
                // @ATTR event.unset_status|list(string)|Unsets specified statuses
                e.Type = EventComponent.UnsetStatus;

                e.S = Parse.PopFirstString(ref val);
                e.Status = camp.RegisterStatus(e.S);

                // add repeating unset_status
                if (evnt != null)
                {
                    string repeatVal = Parse.PopFirstString(ref val);
                    while (repeatVal != "")
                    {
                        evnt.Components.Add(new EventComponent());
                        e = evnt.Components[^1];
                        e.Type = EventComponent.UnsetStatus;
                        e.S = repeatVal;
                        e.Status = camp.RegisterStatus(repeatVal);

                        repeatVal = Parse.PopFirstString(ref val);
                    }
                }
            }
            else if (key == "remove_currency")
            {
                // @ATTR event.remove_currency|int|Removes specified amount of currency from hero inventory
                e.Type = EventComponent.RemoveCurrency;

                e.Data[0].Int = Math.Max(Parse.ToInt(val), 0);
            }
            else if (key == "remove_item")
            {
                // @ATTR event.remove_item|list(item_id)|Removes specified item from hero inventory. Quantity can be specified by appending ":Q" to the item_id, where Q is an integer.
                e.Type = EventComponent.RemoveItem;

                ItemStack itemStack = Parse.ToItemQuantityPair(Parse.PopFirstString(ref val));
                e.Id = itemStack.Item;
                e.Data[0].Int = itemStack.Quantity;

                // add repeating remove_item
                if (evnt != null)
                {
                    string repeatVal = Parse.PopFirstString(ref val);
                    while (repeatVal != "")
                    {
                        evnt.Components.Add(new EventComponent());
                        e = evnt.Components[^1];
                        e.Type = EventComponent.RemoveItem;
                        itemStack = Parse.ToItemQuantityPair(repeatVal);
                        e.Id = itemStack.Item;
                        e.Data[0].Int = itemStack.Quantity;

                        repeatVal = Parse.PopFirstString(ref val);
                    }
                }
            }
            else if (key == "reward_xp")
            {
                // @ATTR event.reward_xp|int|Reward hero with specified amount of experience points.
                e.Type = EventComponent.RewardXp;

                e.Data[0].Int = Math.Max(Parse.ToInt(val), 0);
            }
            else if (key == "reward_currency")
            {
                // @ATTR event.reward_currency|int|Reward hero with specified amount of currency.
                e.Type = EventComponent.RewardCurrency;

                e.Data[0].Int = Math.Max(Parse.ToInt(val), 0);
            }
            else if (key == "reward_item")
            {
                // @ATTR event.reward_item|(list(item_id)|Reward hero with a specified item. Quantity can be specified by appending ":Q" to the item_id, where Q is an integer. To maintain backwards compatibility, the quantity must be defined for at least the first item in the list in order to use this syntax.
                // @ATTR event.reward_item|item_id, int : Item, Quantity|Reward hero with y number of item x. NOTE: This syntax is maintained for backwards compatibility. It is recommended to use the above syntax instead.
                e.Type = EventComponent.RewardItem;

                bool checkPair = false;
                ItemStack itemStack = Parse.ToItemQuantityPair(Parse.PopFirstString(ref val), out checkPair);

                if (!checkPair)
                {
                    // item:quantity syntax not detected, falling back to the old syntax
                    e.Id = items.VerifyID(itemStack.Item, null, !ItemManager.VerifyAllowZero, ItemManager.VerifyAllocate);
                    e.Data[0].Int = Math.Max(Parse.PopFirstInt(ref val), 1);
                }
                else
                {
                    e.Id = itemStack.Item;
                    e.Data[0].Int = itemStack.Quantity;

                    // add repeating reward_item
                    if (evnt != null)
                    {
                        string repeatVal = Parse.PopFirstString(ref val);
                        while (repeatVal != "")
                        {
                            evnt.Components.Add(new EventComponent());
                            e = evnt.Components[^1];
                            e.Type = EventComponent.RewardItem;
                            itemStack = Parse.ToItemQuantityPair(repeatVal);
                            e.Id = itemStack.Item;
                            e.Data[0].Int = itemStack.Quantity;

                            repeatVal = Parse.PopFirstString(ref val);
                        }
                    }
                }
            }
            else if (key == "reward_loot")
            {
                // @ATTR event.reward_loot|list(loot)|Reward hero with random loot.
                e.Type = EventComponent.RewardLoot;

                e.S = val;
            }
            else if (key == "reward_loot_count")
            {
                // @ATTR event.reward_loot_count|int, int : Min, Max|Sets the minimum (and optionally, the maximum) amount of loot that reward_loot can give the hero. Defaults to 1.
                e.Type = EventComponent.RewardLootCount;

                e.Data[0].Int = Math.Max(Parse.PopFirstInt(ref val), 1);
                e.Data[1].Int = Math.Max(Parse.PopFirstInt(ref val), e.Data[0].Int);
            }
            else if (key == "restore")
            {
                // @ATTR event.restore|list(["hp", "mp", "hpmp", "status", "all", predefined_string])|Restore the hero's HP, MP, and/or status. Resource stat base IDs are also valid.
                e.Type = EventComponent.Restore;

                e.S = val;
            }
            else if (key == "power")
            {
                // @ATTR event.power|power_id|Specify power coupled with event.
                e.Type = EventComponent.Power;

                e.Id = Parse.ToPowerID(val);

                if (SharedGameResources.Powers != null)
                    e.Id = SharedGameResources.Powers.VerifyID(e.Id, null, !PowerManager.AllowZeroId);
                else
                    Utils.LogError("EventManager: Unable to verify Power ID '%d'. PowerManager hasn't been initialized.", e.Id);
            }
            else if (key == "power_path")
            {
                // @ATTR event.power_path|int, int, ["hero", point] : Source X, Source Y, Destination|Path that an event power will take.
                e.Type = EventComponent.PowerPath;

                // x,y are src, if s=="hero" we target the hero,
                // else we'll use values in a,b as coordinates
                e.Data[0].Int = Parse.PopFirstInt(ref val);
                e.Data[1].Int = Parse.PopFirstInt(ref val);

                e.Data[4].Bool = false;
                string dest = Parse.PopFirstString(ref val);
                if (dest == "hero")
                {
                    e.Data[4].Bool = true;
                }
                else
                {
                    e.Data[2].Int = Parse.ToInt(dest);
                    e.Data[3].Int = Parse.PopFirstInt(ref val);
                }
            }
            else if (key == "power_damage")
            {
                // @ATTR event.power_damage|float, float : Min, Max|Range of power damage
                e.Type = EventComponent.PowerDamage;

                e.Data[0].Float = Parse.PopFirstFloat(ref val);
                e.Data[1].Float = Parse.PopFirstFloat(ref val);
            }
            else if (key == "power_stats")
            {
                // @ATTR event.power_stats|filename|Entity stat file to use for this event's power spawner.
                e.Type = EventComponent.PowerStats;

                e.S = val;
            }
            else if (key == "power_level")
            {
                // @ATTR event.power_level|["default", "fixed", "source_level", "source_stat", "hero_level", "hero_stat"], float, predefined_string : Mode, Multiplier, Primary stat|Level of this event's power spawner. The need for the last two parameters depends on the mode being used. The "default" mode will just use the entity's normal level and doesn't require any additional parameters. The "fixed" mode sets the multiplier as the enemy level. The level modes multiply with the target's level. The stat modes multiply by one of the target's primary stats. The stat is defined with the last parameter, which is simply the ID of the primary stat that should be used for scaling. Because the map has no level/stats of its own, the source modes use the hero's level/stats.
                e.Type = EventComponent.PowerLevel;

                e.S = val; // will be parsed in Map::addEventStatBlock()
            }
            else if (key == "spawn")
            {
                // @ATTR event.spawn|list(predefined_string, int, int) : Enemy category, X, Y|Spawn an enemy from this category at location
                e.Type = EventComponent.Spawn;

                e.S = Parse.PopFirstString(ref val);
                e.Data[0].Int = Parse.PopFirstInt(ref val);
                e.Data[1].Int = Parse.PopFirstInt(ref val);

                // add repeating spawn
                if (evnt != null)
                {
                    string repeatVal = Parse.PopFirstString(ref val);
                    while (repeatVal != "")
                    {
                        evnt.Components.Add(new EventComponent());
                        e = evnt.Components[^1];
                        e.Type = EventComponent.Spawn;

                        e.S = repeatVal;
                        e.Data[0].Int = Parse.PopFirstInt(ref val);
                        e.Data[1].Int = Parse.PopFirstInt(ref val);

                        repeatVal = Parse.PopFirstString(ref val);
                    }
                }
            }
            else if (key == "spawn_level")
            {
                // @ATTR event.spawn_level|["default", "fixed", "source_level", "source_stat", "hero_level", "hero_stat"], float, predefined_string : Mode, Multiplier, Primary stat|Level of the spawned entities. The need for the last two parameters depends on the mode being used. The "default" mode will just use the entity's normal level and doesn't require any additional parameters. The "fixed" mode sets the multiplier as the entity level. The level modes multiply with the target's level. The stat modes multiply by one of the target's primary stats. The stat is defined with the last parameter, which is simply the ID of the primary stat that should be used for scaling. Because the map has no level/stats of its own, the source modes use the hero's level/stats.
                e.Type = EventComponent.SpawnLevel;

                e.S = val; // will be parsed in EntityManager::spawn()
            }
            else if (key == "stash")
            {
                // @ATTR event.stash|bool|If true, the Stash menu if opened.
                e.Type = EventComponent.Stash;

                e.Data[0].Bool = Parse.ToBool(val);
            }
            else if (key == "npc")
            {
                // @ATTR event.npc|filename|Filename of an NPC to start dialog with.
                e.Type = EventComponent.Npc;

                e.S = val;
            }
            else if (key == "music")
            {
                // @ATTR event.music|filename|Change background music to specified file.
                e.Type = EventComponent.Music;

                e.S = val;
            }
            else if (key == "cutscene")
            {
                // @ATTR event.cutscene|filename|Show specified cutscene by filename.
                e.Type = EventComponent.Cutscene;

                e.S = val;
            }
            else if (key == "repeat")
            {
                // @ATTR event.repeat|bool|If true, the event to be triggered again.
                e.Type = EventComponent.Repeat;

                e.Data[0].Bool = Parse.ToBool(val);
            }
            else if (key == "save_game")
            {
                // @ATTR event.save_game|bool|If true, the game is saved when the event is triggered. The respawn position is set to where the player is standing.
                e.Type = EventComponent.SaveGame;

                e.Data[0].Bool = Parse.ToBool(val);
            }
            else if (key == "book")
            {
                // @ATTR event.book|["close", filename]|Opens a book by filename. 'close' can be used in place of the filename to close an already open book.
                e.Type = EventComponent.Book;

                e.S = val;
            }
            else if (key == "script")
            {
                // @ATTR event.script|filename|Loads and executes an Event from a file.
                e.Type = EventComponent.Script;

                e.S = val;
            }
            else if (key == "chance_exec")
            {
                // @ATTR event.chance_exec|float|Percentage chance that this event will execute when triggered.
                e.Type = EventComponent.ChanceExec;

                e.Data[0].Float = Parse.PopFirstFloat(ref val);
            }
            else if (key == "respec")
            {
                // @ATTR event.respec|["xp", "stats", "powers"], bool : Respec mode, Ignore class defaults|Resets various aspects of the character's progression. Resetting "xp" also resets "stats". Resetting "stats" also resets "powers".
                e.Type = EventComponent.Respec;

                string mode = Parse.PopFirstString(ref val);
                string useEngineDefaults = Parse.PopFirstString(ref val);

                if (mode == "xp")
                {
                    e.Data[0].Int = 3;
                }
                else if (mode == "stats")
                {
                    e.Data[0].Int = 2;
                }
                else if (mode == "powers")
                {
                    e.Data[0].Int = 1;
                }

                if (!string.IsNullOrEmpty(useEngineDefaults))
                    e.Data[1].Bool = Parse.ToBool(useEngineDefaults);
            }
            else if (key == "show_on_minimap")
            {
                // @ATTR event.show_on_minimap|bool|If true, this event will be shown on the minimap if it is the appropriate type (e.g. an intermap teleport).
                e.Type = EventComponent.ShowOnMinimap;

                e.Data[0].Bool = Parse.ToBool(val);
            }
            else if (key == "parallax_layers")
            {
                // @ATTR event.parallax_layers|filename|Filename of a parallax layers definition to load.
                e.Type = EventComponent.ParallaxLayers;

                e.S = val;
            }
            else if (key == "random_status")
            {
                // @ATTR event.random_status|repeatable(["append", "clear", "roll", "set", "unset"], list(string)) : Action, Statuses (append action only)|Used to randomly pick a status from a list, and then set or unset it. Statuses are added to the list with the "append" action. The "roll" action will randomly pick from the list and set it as the current random status. The "set" and "unset" commands will function like set_status and unset_status, with the parameter being the current random status. Lastly, the "clear" action will empty the pool of random statuses. It is recommended to clear the list before you use it, as well as after you're done to prevent unintended side-effects.
                e.Type = EventComponent.RandomStatus;

                string mode = Parse.PopFirstString(ref val);
                if (mode == "append")
                {
                    e.Data[0].Int = EventComponent.RandomStatusModeAppend;

                    e.S = Parse.PopFirstString(ref val);
                    e.Status = camp.RegisterStatus(e.S);

                    // add repeating random_status
                    if (evnt != null)
                    {
                        string repeatVal = Parse.PopFirstString(ref val);
                        while (repeatVal != "")
                        {
                            evnt.Components.Add(new EventComponent());
                            e = evnt.Components[^1];
                            e.Type = EventComponent.RandomStatus;
                            e.Data[0].Int = EventComponent.RandomStatusModeAppend;
                            e.S = repeatVal;
                            e.Status = camp.RegisterStatus(repeatVal);

                            repeatVal = Parse.PopFirstString(ref val);
                        }
                    }
                }
                else if (mode == "clear")
                    e.Data[0].Int = EventComponent.RandomStatusModeClear;
                else if (mode == "roll")
                    e.Data[0].Int = EventComponent.RandomStatusModeRoll;
                else if (mode == "set")
                    e.Data[0].Int = EventComponent.RandomStatusModeSet;
                else if (mode == "unset")
                    e.Data[0].Int = EventComponent.RandomStatusModeUnset;
                else
                    Utils.LogError("EventManager: '%s' is not a valid random_status action.", mode);
            }
            else if (key == "procgen_filename")
            {
                // @ATTR event.procgen_filename|filename|Filename of procedural map generation rules file.
                e.Type = EventComponent.ProcgenFilename;

                e.S = val;
            }
            else if (key == "procgen_link")
            {
                // @ATTR event.procgen_link|["north", "south", "west", "east"]|Only used in maps with the procgen_type of "links". Defines a region to be used as a link. The tiles and objects in this region will replace the matching region of the target chunk that has the specified link.
                e.Type = EventComponent.ProcgenLink;

                e.S = val;
            }
            else if (key == "procgen_door_level")
            {
                // @ATTR event.procgen_door_level|int|Event will only be active on a specific door level. For procedural generation chunks only.
                e.Type = EventComponent.ProcgenDoorLevel;

                e.Data[0].Int = Parse.PopFirstInt(ref val);

                // add repeating door levels
                if (evnt != null)
                {
                    string repeatVal = Parse.PopFirstString(ref val);
                    while (repeatVal != "")
                    {
                        evnt.Components.Add(new EventComponent());
                        e = evnt.Components[^1];
                        e.Type = EventComponent.ProcgenDoorLevel;
                        e.Data[0].Int = Parse.ToInt(repeatVal);
                        e.S = repeatVal;

                        repeatVal = Parse.PopFirstString(ref val);
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        public bool ExecuteEvent(Event e)
        {
            return ExecuteEventInternal(e, !SkipDelay);
        }

        public bool ExecuteDelayedEvent(Event e)
        {
            return ExecuteEventInternal(e, SkipDelay);
        }

        public bool IsActive(Event e)
        {
            return SharedGameResources.Camp!.CheckRequirementsInVector(e.Components);
        }

        public void ExecuteScript(string filename, float x, float y)
        {
            var mapr = SharedGameResources.Mapr!;

            using FileParser scriptFile = new FileParser();
            Queue<Event> scriptEvnt = new Queue<Event>();

            bool foundInCache = _scriptCache.TryGetValue(filename, out Queue<Event>? cachedEvnt);

            if (!foundInCache)
            {
                // std::queue 没有等价于 back() 的 API，这里用一个额外的局部引用
                // 跟踪"最近一次入队的元素"，在本方法解析文件的这段代码中，
                // 只会入队、不会出队，因而与原始 back() 语义完全等价。
                Event? currentScriptEvnt = null;

                if (scriptFile.Open(filename, FileParser.ModFile, FileParser.ErrorNormal))
                {
                    while (scriptFile.Next())
                    {
                        if (scriptFile.NewSection && scriptFile.Section == "event")
                        {
                            Event tmpEvnt = new Event();
                            scriptEvnt.Enqueue(tmpEvnt);
                            currentScriptEvnt = tmpEvnt;
                        }

                        if (scriptEvnt.Count == 0)
                            continue;

                        if (scriptFile.Key == "script" && Filesystem.ConvertSlashes(scriptFile.Val) == Filesystem.ConvertSlashes(filename))
                        {
                            scriptFile.Error("EventManager: Calling a script from within itself is not allowed.");
                            continue;
                        }

                        if (scriptFile.Key == "delay")
                        {
                            // 'delay' is not an EventComponent, but we allow it in scripts
                            currentScriptEvnt!.Delay.Duration = (uint)Parse.ToDuration(scriptFile.Val);
                            currentScriptEvnt.Delay.Reset(Timer.Begin);
                        }
                        else
                        {
                            LoadEventComponent(scriptFile, currentScriptEvnt, null);
                        }
                    }
                    scriptFile.Close();
                }

                // 深拷贝存入缓存：原始 C++ map<string, queue<Event>> 的赋值会对 Event
                // （及其内部 vector<EventComponent>）做深拷贝，保证下次取用缓存时得到的是
                // 未被后续 playback 逻辑修改过的独立副本（详见 Event 拷贝构造函数的说明）。
                Queue<Event> cacheCopy = new Queue<Event>();
                foreach (Event ev in scriptEvnt)
                {
                    cacheCopy.Enqueue(new Event(ev));
                }
                _scriptCache[filename] = cacheCopy;
            }
            else
            {
                scriptEvnt = new Queue<Event>();
                foreach (Event ev in cachedEvnt!)
                {
                    scriptEvnt.Enqueue(new Event(ev));
                }
            }

            while (scriptEvnt.Count > 0)
            {
                Event evnt = scriptEvnt.Peek();

                // set event location from x/y
                int locX = (int)x;
                evnt.Hotspot.X = locX;
                evnt.Location.X = locX;
                int locY = (int)y;
                evnt.Hotspot.Y = locY;
                evnt.Location.Y = locY;
                evnt.Hotspot.Width = 1;
                evnt.Location.Width = 1;
                evnt.Hotspot.Height = 1;
                evnt.Location.Height = 1;
                evnt.Center.X = (float)evnt.Location.X + 0.5f;
                evnt.Center.Y = (float)evnt.Location.Y + 0.5f;

                // create StatBlocks if we need them
                EventComponent? ecPower = evnt.GetComponent(EventComponent.Power);
                if (ecPower != null)
                {
                    ecPower.Data[0].Int = mapr.AddEventStatBlock(evnt);
                }

                if (evnt.Delay.Duration > 0)
                {
                    // handle delayed events
                    mapr.DelayedEvents.Add(new Event(evnt));
                }
                else if (IsActive(evnt))
                {
                    ExecuteEvent(evnt);
                }
                scriptEvnt.Dequeue();
            }
        }

        public string GetIntermapIDString(uint index)
        {
            // zero is reserved as blank, so offset by 1
            if (index - 1 < (uint)_intermapIds.Count)
                return _intermapIds[(int)(index - 1)];

            return "";
        }

        private const bool SkipDelay = true;

        /// <summary>
        /// A particular event has been triggered.
        /// Process all of this events components.
        ///
        /// @param The triggered event
        /// @param Delay ignore flag
        /// @return Returns true if the event shall not be run again.
        /// （原始注释）
        /// </summary>
        private bool ExecuteEventInternal(Event ev, bool skipDelay)
        {
            var mapr = SharedGameResources.Mapr!;
            var camp = SharedGameResources.Camp!;
            var pc = SharedGameResources.Pc!;
            var msg = SharedResources.Msg!;
            var snd = SharedResources.Snd!;
            var loot = SharedGameResources.Loot!;
            var items = SharedGameResources.Items!;
            var entitym = SharedGameResources.Entitym!;
            var menu = SharedGameResources.Menu!;
            var inpt = SharedResources.Inpt!;
            var settings = SharedResources.Settings!;
            var eset = SharedResources.Eset!;
            var mods = SharedResources.Mods!;

            // skip executing events that are on cooldown
            if (!ev.Delay.IsEnd() || !ev.Cooldown.IsEnd()) return false;

            // need to know this for early returns
            EventComponent? ecRepeat = ev.GetComponent(EventComponent.Repeat);
            if (ecRepeat != null)
            {
                ev.KeepAfterTrigger = ecRepeat.Data[0].Bool;
            }

            // Delay event execution
            // When an event is delayed, we create a copy and push it to mapr->delayed_events.
            // The original starts both the cooldown and delay timers.
            // The delay will finish, followed by the cooldown, which gives the correct timing for repeating events.
            // The copy only starts the delay timer. The cooldown is not needed because the copy never repeats.
            if (ev.Delay.Duration > 0 && !skipDelay)
            {
                ev.Delay.Reset(Timer.Begin);
                mapr.DelayedEvents.Add(new Event(ev));
                ev.Cooldown.Reset(Timer.Begin);

                return !ev.KeepAfterTrigger;
            }

            // set cooldown
            ev.Cooldown.Reset(Timer.Begin);

            // if chance_exec roll fails, don't execute the event
            // we respect the value of "repeat", even if the event doesn't execute
            EventComponent? ecChanceExec = ev.GetComponent(EventComponent.ChanceExec);
            if (ecChanceExec != null && !MathUtils.PercentChanceF(ecChanceExec.Data[0].Float))
            {
                return !ev.KeepAfterTrigger;
            }

            EventComponent ec;
            MapRenderer.MapLoot? mapLoot = null;

            for (int i = 0; i < ev.Components.Count; ++i)
            {
                ec = ev.Components[i];

                if (ec.Type == EventComponent.SetStatus)
                {
                    camp.SetStatus(ec.Status);
                }
                else if (ec.Type == EventComponent.UnsetStatus)
                {
                    camp.UnsetStatus(ec.Status);
                }
                else if (ec.Type == EventComponent.Intermap)
                {
                    if (ec.Data[2].Bool)
                    {
                        // this is intermap_random
                        string mapList = ec.S;
                        EventComponent randomEc = GetRandomMapFromFile(mapList);

                        ec.S = randomEc.S;
                        ec.Data[0].Int = randomEc.Data[0].Int;
                        ec.Data[1].Int = randomEc.Data[1].Int;
                    }

                    if (Filesystem.FileExists(mods.Locate(ec.S)))
                    {
                        mapr.Teleportation = true;
                        mapr.TeleportMapname = ec.S;

                        if (ec.Data[0].Int == -1 && ec.Data[1].Int == -1)
                        {
                            // the teleport destination will be set to the map's hero_pos once the map is loaded
                            mapr.TeleportDestination.X = -1;
                            mapr.TeleportDestination.Y = -1;
                            mapr.TeleportDestinationId = ec.Data[3].Int;
                        }
                        else
                        {
                            mapr.TeleportDestination.X = (float)ec.Data[0].Int + 0.5f;
                            mapr.TeleportDestination.Y = (float)ec.Data[1].Int + 0.5f;
                            mapr.TeleportDestinationId = 0;
                        }
                    }
                    else
                    {
                        ev.KeepAfterTrigger = false;
                        pc.LogMsg(msg.Get("Unknown destination"), Avatar.MsgUnique);
                        Utils.LogInfo("EventManager: Unknown intermap destination (%s)", ec.S);
                    }
                }
                else if (ec.Type == EventComponent.Intramap)
                {
                    mapr.Teleportation = true;
                    mapr.TeleportMapname = "";
                    mapr.TeleportDestination.X = (float)ec.Data[0].Int + 0.5f;
                    mapr.TeleportDestination.Y = (float)ec.Data[1].Int + 0.5f;
                }
                else if (ec.Type == EventComponent.Mapmod)
                {
                    int tileX = ec.Data[0].Int;
                    int tileY = ec.Data[1].Int;
                    ushort tileId = (ushort)ec.Data[2].Int;

                    if (ec.S == "collision")
                    {
                        if (tileX >= 0 && tileX < mapr.W && tileY >= 0 && tileY < mapr.H)
                        {
                            mapr.Collider.Colmap[tileX][tileY] = tileId;
                            mapr.MapChange = true;
                        }
                        else
                            Utils.LogError("EventManager: Mapmod at position (%d, %d) is out of bounds 0-255.", tileX, tileY);
                    }
                    else
                    {
                        int index = mapr.Layernames.IndexOf(ec.S);
                        if (index == -1) index = mapr.Layernames.Count;

                        if (!mapr.IsValidTile(tileId))
                            Utils.LogError("EventManager: Mapmod at position (%d, %d) contains invalid tile id (%d).", tileX, tileY, tileId);
                        else if (index >= mapr.Layers.Count)
                            Utils.LogError("EventManager: Mapmod at position (%d, %d) is on an invalid layer.", tileX, tileY);
                        else if (tileX >= 0 && tileX < mapr.W && tileY >= 0 && tileY < mapr.H)
                            mapr.Layers[index][tileX][tileY] = tileId;
                        else
                            Utils.LogError("EventManager: Mapmod at position (%d, %d) is out of bounds 0-255.", tileX, tileY);
                    }
                }
                else if (ec.Type == EventComponent.MapmodToggle)
                {
                    int tileX = ec.Data[0].Int;
                    int tileY = ec.Data[1].Int;
                    ushort tileA = (ushort)ec.Data[2].Int;
                    ushort tileB = (ushort)ec.Data[3].Int;

                    if (ec.S == "collision")
                    {
                        if (tileX >= 0 && tileX < mapr.W && tileY >= 0 && tileY < mapr.H)
                        {
                            // List<T> 不支持像 C++ 引用那样返回可写的元素别名，这里改为
                            // "先读出快照、再按同样条件写回"的等价写法（详见转换报告）。
                            ushort mapTile = mapr.Collider.Colmap[tileX][tileY];
                            if (mapTile == tileA)
                            {
                                mapr.Collider.Colmap[tileX][tileY] = tileB;
                                mapr.MapChange = true;
                            }
                            else if (mapTile == tileB)
                            {
                                mapr.Collider.Colmap[tileX][tileY] = tileA;
                                mapr.MapChange = true;
                            }
                        }
                        else
                            Utils.LogError("EventManager: Mapmod at position (%d, %d) is out of bounds 0-255.", tileX, tileY);
                    }
                    else
                    {
                        int index = mapr.Layernames.IndexOf(ec.S);
                        if (index == -1) index = mapr.Layernames.Count;

                        if (!mapr.IsValidTile(tileA))
                            Utils.LogError("EventManager: Mapmod at position (%d, %d) contains invalid tile id (%d).", tileX, tileY, tileA);
                        else if (!mapr.IsValidTile(tileB))
                            Utils.LogError("EventManager: Mapmod at position (%d, %d) contains invalid tile id (%d).", tileX, tileY, tileB);
                        else if (index >= mapr.Layers.Count)
                            Utils.LogError("EventManager: Mapmod at position (%d, %d) is on an invalid layer.", tileX, tileY);
                        else if (tileX >= 0 && tileX < mapr.W && tileY >= 0 && tileY < mapr.H)
                        {
                            ushort mapTile = mapr.Layers[index][tileX][tileY];
                            if (mapTile == tileA)
                            {
                                mapTile = tileB;
                            }
                            else if (mapTile == tileB)
                            {
                                mapTile = tileA;
                            }
                            mapr.Layers[index][tileX][tileY] = mapTile;
                        }
                        else
                            Utils.LogError("EventManager: Mapmod at position (%d, %d) is out of bounds 0-255.", tileX, tileY);
                    }
                }
                else if (ec.Type == EventComponent.Soundfx)
                {
                    Vector2 pos = new Vector2(0, 0);
                    bool loop = false;

                    if (ec.Data[0].Int != -1 && ec.Data[1].Int != -1)
                    {
                        if (ec.Data[0].Int != 0 && ec.Data[1].Int != 0)
                        {
                            pos.X = (float)ec.Data[0].Int + 0.5f;
                            pos.Y = (float)ec.Data[1].Int + 0.5f;
                        }
                    }
                    else if (ev.Location.X != 0 && ev.Location.Y != 0)
                    {
                        pos.X = (float)ev.Location.X + 0.5f;
                        pos.Y = (float)ev.Location.Y + 0.5f;
                    }

                    // loop sound if loop flag is set *or* if sound is attached to 'on_load' event and loop flag was not specified
                    if ((ev.ActivateType == Event.ActivateOnLoad && !ec.Data[3].Bool) || ec.Data[2].Bool)
                        loop = true;

                    SoundID sid = snd.Load(ec.S, "MapRenderer background soundfx");

                    snd.Play(sid, SoundManager.DefaultChannel, pos, loop);
                    mapr.Sids.Add(sid);
                }
                else if (ec.Type == EventComponent.Loot)
                {
                    EventComponent? ecLootcount = ev.GetComponent(EventComponent.LootCount);
                    Int2 lootCount = default;
                    if (ecLootcount != null)
                    {
                        lootCount.X = ecLootcount.Data[0].Int;
                        lootCount.Y = ecLootcount.Data[1].Int;
                    }

                    ec.Data[LootManager.LootEcPosX].Int = ev.Hotspot.X;
                    ec.Data[LootManager.LootEcPosY].Int = ev.Hotspot.Y;

                    if (mapLoot == null)
                    {
                        mapr.Loot.Add(new MapRenderer.MapLoot());
                        mapLoot = mapr.Loot[^1];
                    }
                    if (mapLoot != null)
                    {
                        mapLoot.First.Add(new EventComponent(ec));
                        mapLoot.Second = lootCount;
                    }
                }
                else if (ec.Type == EventComponent.Msg)
                {
                    pc.LogMsg(ec.S, Avatar.MsgUnique);
                }
                else if (ec.Type == EventComponent.Shakycam)
                {
                    mapr.Cam.ShakeTimer.Duration = (uint)ec.Data[0].Int;
                    inpt.JoystickRumble(InputState.JoystickRumbleStrength, InputState.JoystickRumbleStrength, (uint)((ec.Data[0].Int * 1000) / settings.MaxFramesPerSec));
                }
                else if (ec.Type == EventComponent.RemoveCurrency)
                {
                    camp.RemoveCurrency(ec.Data[0].Int);
                }
                else if (ec.Type == EventComponent.RemoveItem)
                {
                    camp.RemoveItem(new ItemStack(ec.Id, ec.Data[0].Int));
                }
                else if (ec.Type == EventComponent.RewardXp)
                {
                    camp.RewardXp((float)ec.Data[0].Int, CampaignManager.XpShowMsg);
                }
                else if (ec.Type == EventComponent.RewardCurrency)
                {
                    camp.RewardCurrency(ec.Data[0].Int);
                }
                else if (ec.Type == EventComponent.RewardItem)
                {
                    List<ItemStack> exStacks = new List<ItemStack>();
                    items.GetExtendedStacks(ec.Id, (uint)ec.Data[0].Int, exStacks);
                    for (int j = 0; j < exStacks.Count; ++j)
                    {
                        camp.RewardItem(exStacks[j]);
                    }
                }
                else if (ec.Type == EventComponent.RewardLoot)
                {
                    List<EventComponent> randomTable = new List<EventComponent>();
                    Int2 randomTableCount = new Int2(1, 1);

                    EventComponent? ecLootcount = ev.GetComponent(EventComponent.RewardLootCount);
                    if (ecLootcount != null)
                    {
                        randomTableCount.X = ecLootcount.Data[0].Int;
                        randomTableCount.Y = ecLootcount.Data[1].Int;
                    }

                    randomTable.Add(new EventComponent());
                    loot.ParseLoot(ref ec.S, randomTable[^1], randomTable);

                    uint randCount = (uint)MathUtils.RandBetween(randomTableCount.X, randomTableCount.Y);
                    List<ItemStack> randItemstacks = new List<ItemStack>();
                    for (uint j = 0; j < randCount; ++j)
                    {
                        loot.CheckLoot(randomTable, null, randItemstacks);
                    }
                    for (int j = 0; j < randItemstacks.Count; ++j)
                    {
                        if (randItemstacks[j].Item == eset.Misc.CurrencyId)
                            camp.RewardCurrency(randItemstacks[j].Quantity);
                        else
                            camp.RewardItem(randItemstacks[j]);
                    }
                }
                else if (ec.Type == EventComponent.Restore)
                {
                    camp.RestoreHpMp(ec.S);
                }
                else if (ec.Type == EventComponent.Spawn)
                {
                    Int2 spawnPos = default;
                    spawnPos.X = ec.Data[0].Int;
                    spawnPos.Y = ec.Data[1].Int;
                    entitym.Spawn(ec.S, spawnPos, ev.GetComponent(EventComponent.SpawnLevel));
                }
                else if (ec.Type == EventComponent.Power)
                {
                    EventComponent? ecPath = ev.GetComponent(EventComponent.PowerPath);
                    Vector2 target = default;

                    if (ecPath != null)
                    {
                        // targets hero option
                        if (ecPath.Data[4].Bool)
                        {
                            target.X = pc.Stats.Pos.X;
                            target.Y = pc.Stats.Pos.Y;
                        }
                        // targets fixed path option
                        else
                        {
                            target.X = (float)ecPath.Data[2].Int + 0.5f;
                            target.Y = (float)ecPath.Data[3].Int + 0.5f;
                        }
                    }
                    // no path specified, targets self location
                    else
                    {
                        target.X = (float)ev.Location.X + 0.5f;
                        target.Y = (float)ev.Location.Y + 0.5f;
                    }

                    // ec->id is power id
                    // ec->data[0] is statblock index
                    mapr.ActivatePower(ec.Id, (uint)ec.Data[0].Int, target);
                }
                else if (ec.Type == EventComponent.Stash)
                {
                    mapr.Stash = ec.Data[0].Bool;
                    if (mapr.Stash)
                    {
                        mapr.StashPos.X = (float)ev.Location.X + 0.5f;
                        mapr.StashPos.Y = (float)ev.Location.Y + 0.5f;
                    }
                }
                else if (ec.Type == EventComponent.Npc)
                {
                    mapr.EventNpc = ec.S;
                }
                else if (ec.Type == EventComponent.Music)
                {
                    mapr.MusicFilename = ec.S;
                    mapr.LoadMusic();
                }
                else if (ec.Type == EventComponent.Cutscene)
                {
                    mapr.Cutscene = true;
                    mapr.CutsceneFile = ec.S;
                }
                else if (ec.Type == EventComponent.Repeat)
                {
                    ev.KeepAfterTrigger = ec.Data[0].Bool;
                }
                else if (ec.Type == EventComponent.SaveGame)
                {
                    mapr.SaveGame = ec.Data[0].Bool;
                }
                else if (ec.Type == EventComponent.NpcID)
                {
                    mapr.NpcId = ec.Data[0].Int;
                }
                else if (ec.Type == EventComponent.Book)
                {
                    mapr.ShowBook = ec.S;
                }
                else if (ec.Type == EventComponent.Script)
                {
                    if (ev.Center.X != -1 && ev.Center.Y != -1)
                        ExecuteScript(ec.S, ev.Center.X, ev.Center.Y);
                    else
                        ExecuteScript(ec.S, pc.Stats.Pos.X, pc.Stats.Pos.Y);
                }
                else if (ec.Type == EventComponent.Respec)
                {
                    bool useEngineDefaults = ec.Data[1].Bool;
                    EngineSettings.HeroClassesSettings.HeroClass? pcClass;
                    pcClass = eset.HeroClasses.GetByName(pc.Stats.CharacterClass);

                    if (ec.Data[0].Int == 3)
                    {
                        // xp
                        pc.Stats.Level = 1;
                        pc.Stats.Xp = 0;
                    }
                    if (ec.Data[0].Int >= 2)
                    {
                        // stats
                        for (int j = 0; j < eset.PrimaryStats.Stats.Count; ++j)
                        {
                            pc.Stats.Primary[j] = 1;
                            pc.Stats.PrimaryAdditional[j] = 0;

                            if (pcClass != null && !useEngineDefaults)
                            {
                                pc.Stats.Primary[j] += pcClass.Primary[j];
                                pc.Stats.PrimaryStarting[j] = pc.Stats.Primary[j];
                            }
                        }

                        pc.Stats.Recalc();
                        menu.Inv!.ApplyEquipment();
                        pc.Stats.Logic();
                    }
                    if (ec.Data[0].Int >= 1)
                    {
                        // powers
                        pc.Stats.PowersList.Clear();
                        pc.Stats.PowersPassive.Clear();
                        pc.Stats.Effects.ClearEffects();
                        SharedGameResources.MenuPowers!.ResetToBasePowers();
                        if (pcClass != null && !useEngineDefaults)
                        {
                            for (int j = 0; j < pcClass.Powers.Count; j++)
                            {
                                pc.Stats.PowersList.Add(pcClass.Powers[j]);
                            }
                        }
                        SharedGameResources.MenuPowers.SetUnlockedPowers();

                        SharedGameResources.MenuAct!.Clear(MenuActionBar.ClearSkipItems);
                        if (pcClass != null && !useEngineDefaults)
                        {
                            menu.Act!.Set(pcClass.Hotkeys, MenuActionBar.SetSkipEmpty);
                        }
                        menu.Pow!.NewPowerNotification = false;

                        pc.Respawn = true; // re-applies equipment, also revives the player
                        pc.Stats.RefreshStats = true;
                    }
                }
                else if (ec.Type == EventComponent.ParallaxLayers)
                {
                    mapr.SetMapParallax(ec.S);
                }
                else if (ec.Type == EventComponent.RandomStatus)
                {
                    if (ec.Data[0].Int == EventComponent.RandomStatusModeAppend)
                        camp.RandomStatusAppend(ec.Status);
                    else if (ec.Data[0].Int == EventComponent.RandomStatusModeClear)
                        camp.RandomStatusClear();
                    else if (ec.Data[0].Int == EventComponent.RandomStatusModeRoll)
                        camp.RandomStatusRoll();
                    else if (ec.Data[0].Int == EventComponent.RandomStatusModeSet)
                        camp.RandomStatusSet();
                    else if (ec.Data[0].Int == EventComponent.RandomStatusModeUnset)
                        camp.RandomStatusUnset();
                }
            }
            return !ev.KeepAfterTrigger;
        }

        private EventComponent GetRandomMapFromFile(string fname)
        {
            var mapr = SharedGameResources.Mapr!;

            // map pool is the same, so pick the next one in the "playlist"
            if (fname == mapr.IntermapRandomFilename && mapr.IntermapRandomQueue.Count > 0)
            {
                EventComponent ec = mapr.IntermapRandomQueue.Dequeue();
                return ec;
            }

            // starting a new map pool, so clear the queue
            while (mapr.IntermapRandomQueue.Count > 0)
            {
                mapr.IntermapRandomQueue.Dequeue();
            }

            using FileParser infile = new FileParser();
            List<EventComponent> ecList = new List<EventComponent>();

            // @CLASS EventManager: Random Map List|Description of maps/random/lists/
            if (infile.Open(fname, FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    // @ATTR map|filename, int, int : Map file, X, Y|Adds a map and optional spawn position to the random list of maps to teleport to.
                    if (infile.Key == "map")
                    {
                        EventComponent ec = new EventComponent();
                        ec.S = Parse.PopFirstString(ref infile.Val);
                        if (ecList.Count == 0 || ec.S != mapr.Filename)
                        {
                            ec.Data[0].Int = -1;
                            ec.Data[1].Int = -1;

                            string testX = Parse.PopFirstString(ref infile.Val);
                            if (!string.IsNullOrEmpty(testX))
                            {
                                ec.Data[0].Int = Parse.ToInt(testX);
                                ec.Data[1].Int = Parse.PopFirstInt(ref infile.Val);
                            }

                            ecList.Add(ec);
                        }
                    }
                }

                infile.Close();
            }

            if (ecList.Count == 0)
            {
                mapr.IntermapRandomFilename = "";
                return new EventComponent();
            }
            else
            {
                mapr.IntermapRandomFilename = fname;

                while (ecList.Count > 0)
                {
                    int index = Program.Rng.Next() % ecList.Count;
                    mapr.IntermapRandomQueue.Enqueue(new EventComponent(ecList[index]));
                    ecList.RemoveAt(index);
                }

                EventComponent ec = mapr.IntermapRandomQueue.Dequeue();
                return ec;
            }
        }

        private int GetIntermapID(string s)
        {
            int it = _intermapIds.IndexOf(s);
            if (it == -1)
            {
                _intermapIds.Add(s);
                return _intermapIds.Count;
            }
            else
            {
                // zero is reserved as blank, so offset by 1
                return it + 1;
            }
        }

        private Dictionary<string, Queue<Event>> _scriptCache = new Dictionary<string, Queue<Event>>();

        private List<string> _intermapIds = new List<string>();
    }
}
