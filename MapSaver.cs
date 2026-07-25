// 对应 C++ 源：MapSaver.h + MapSaver.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using System.Globalization;
using System.IO;
using System.Text;
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// MapSaver
    ///
    /// 将内存中的 <see cref="Map"/> 数据序列化为 flare 地图文本格式（对应 C++ <c>class MapSaver</c>）。
    /// </summary>
    public class MapSaver
    {
        private Map _map;
        private string _destFile;

        private string[] _eventComponentName;

        public MapSaver(Map map)
        {
            _map = map;

            _eventComponentName = new string[EventComponent.EventComponentCount];
            for (int idx = 0; idx < _eventComponentName.Length; ++idx)
            {
                _eventComponentName[idx] = "";
            }

            _eventComponentName[EventComponent.Tooltip] = "tooltip";
            _eventComponentName[EventComponent.Power] = "power";
            _eventComponentName[EventComponent.PowerPath] = "power_path";
            _eventComponentName[EventComponent.PowerDamage] = "power_damage";
            _eventComponentName[EventComponent.PowerStats] = "power_stats";
            _eventComponentName[EventComponent.PowerLevel] = "power_level";
            _eventComponentName[EventComponent.Intermap] = "intermap";
            _eventComponentName[EventComponent.IntermapID] = "intermap_id";
            _eventComponentName[EventComponent.Intramap] = "intramap";
            _eventComponentName[EventComponent.Mapmod] = "mapmod";
            _eventComponentName[EventComponent.MapmodToggle] = "mapmod_toggle";
            _eventComponentName[EventComponent.Soundfx] = "soundfx";
            _eventComponentName[EventComponent.Loot] = "loot"; // HALF-IMPLEMENTED
            _eventComponentName[EventComponent.LootCount] = "loot_count";
            _eventComponentName[EventComponent.Msg] = "msg";
            _eventComponentName[EventComponent.Shakycam] = "shakycam";
            _eventComponentName[EventComponent.RequiresStatus] = "requires_status";
            _eventComponentName[EventComponent.RequiresNotStatus] = "requires_not_status";
            _eventComponentName[EventComponent.RequiresLevel] = "requires_level";
            _eventComponentName[EventComponent.RequiresNotLevel] = "requires_not_level";
            _eventComponentName[EventComponent.RequiresCurrency] = "requires_currency";
            _eventComponentName[EventComponent.RequiresNotCurrency] = "requires_not_currency";
            _eventComponentName[EventComponent.RequiresItem] = "requires_item";
            _eventComponentName[EventComponent.RequiresNotItem] = "requires_not_item";
            _eventComponentName[EventComponent.RequiresClass] = "requires_class";
            _eventComponentName[EventComponent.RequiresNotClass] = "requires_not_class";
            _eventComponentName[EventComponent.RequiresTile] = "requires_tile";
            _eventComponentName[EventComponent.RequiresNotTile] = "requires_not_tile";
            _eventComponentName[EventComponent.SetStatus] = "set_status";
            _eventComponentName[EventComponent.UnsetStatus] = "unset_status";
            _eventComponentName[EventComponent.RemoveCurrency] = "remove_currency";
            _eventComponentName[EventComponent.RemoveItem] = "remove_item";
            _eventComponentName[EventComponent.RewardXp] = "reward_xp";
            _eventComponentName[EventComponent.RewardCurrency] = "reward_currency";
            _eventComponentName[EventComponent.RewardItem] = "reward_item";
            _eventComponentName[EventComponent.RewardLoot] = "reward_loot";
            _eventComponentName[EventComponent.RewardLootCount] = "reward_loot_count";
            _eventComponentName[EventComponent.Restore] = "restore";
            _eventComponentName[EventComponent.Spawn] = "spawn";
            _eventComponentName[EventComponent.SpawnLevel] = "spawn_level";
            _eventComponentName[EventComponent.Stash] = "stash";
            _eventComponentName[EventComponent.Npc] = "npc";
            _eventComponentName[EventComponent.Music] = "music";
            _eventComponentName[EventComponent.Cutscene] = "cutscene";
            _eventComponentName[EventComponent.Repeat] = "repeat";
            _eventComponentName[EventComponent.SaveGame] = "save_game";
            _eventComponentName[EventComponent.Book] = "book";
            _eventComponentName[EventComponent.Script] = "script";
            _eventComponentName[EventComponent.ChanceExec] = "chance_exec";
            _eventComponentName[EventComponent.Respec] = "respec";
            _eventComponentName[EventComponent.ShowOnMinimap] = "show_on_minimap";
            _eventComponentName[EventComponent.ParallaxLayers] = "parallax_layers";
            _eventComponentName[EventComponent.RandomStatus] = "random_status";

            _destFile = _map.Filename;
        }

        /// <summary>
        /// tilesetDefinitions 是供 Tiled 编辑器使用的 tileset 描述字符串。
        /// 游戏加载的地图中不包含此数据，因此从游戏内保存时应传入空字符串。
        /// （原始注释）
        /// </summary>
        public bool SaveMap(string tilesetDefinitions)
        {
            // 语言差异适配：C++ 的 std::ofstream::open 打开失败时不会抛异常，通过 is_open()/bad()
            // 判断状态；C# 的 StreamWriter 构造/写入失败会抛出 IOException。这里用 try/catch
            // 包裹打开与写入过程，日志文案与原始分支保持一致。
            StreamWriter? outfile;
            try
            {
                outfile = new StreamWriter(Filesystem.ConvertSlashes(_destFile), append: false);
            }
            catch (IOException)
            {
                outfile = null;
            }

            if (outfile != null)
            {
                try
                {
                    outfile.Write("## flare-engine generated map file ##" + "\n");

                    WriteHeader(outfile);
                    WriteTilesets(outfile, tilesetDefinitions);
                    WriteLayers(outfile);

                    WriteEvents(outfile);
                    WriteNPCs(outfile);
                    WriteEnemies(outfile);
                }
                catch (IOException)
                {
                    Utils.LogError("MapSaver: Unable to save the map. No write access or disk is full!");
                    return false;
                }
                finally
                {
                    outfile.Dispose();
                }

                return true;
            }
            else
            {
                Utils.LogError("MapSaver: Could not open %s for writing", _destFile);
            }
            return false;
        }

        public bool SaveMap(string file, string tilesetDefinitions)
        {
            _destFile = file;

            return SaveMap(tilesetDefinitions);
        }

        private void WriteHeader(StreamWriter mapFile)
        {
            mapFile.Write("[header]\n");
            mapFile.Write("width=" + _map.W.ToString(CultureInfo.InvariantCulture) + "\n");
            mapFile.Write("height=" + _map.H.ToString(CultureInfo.InvariantCulture) + "\n");
            mapFile.Write("tilewidth=" + "64" + "\n");
            mapFile.Write("tileheight=" + "32" + "\n");
            mapFile.Write("orientation=" + "isometric" + "\n");
            mapFile.Write("music=" + _map.MusicFilename + "\n");
            mapFile.Write("tileset=" + _map.Tileset + "\n");
            mapFile.Write("title=" + _map.Title + "\n");
            mapFile.Write("hero_pos=" + ((int)_map.HeroPos.X).ToString(CultureInfo.InvariantCulture) + "," + ((int)_map.HeroPos.Y).ToString(CultureInfo.InvariantCulture) + "\n");

            if (_map.ProcgenChunks.Count > 0 && _map.ProcgenChunks[0].Count > 0)
            {
                mapFile.Write("procgen_chunks=" + _map.ProcgenChunks[0].Count.ToString(CultureInfo.InvariantCulture) + "," + _map.ProcgenChunks.Count.ToString(CultureInfo.InvariantCulture));
                for (int i = 0; i < _map.ProcgenChunks.Count; ++i)
                {
                    for (int j = 0; j < _map.ProcgenChunks[i].Count; ++j)
                    {
                        Chunk chunk = _map.ProcgenChunks[i][j];
                        mapFile.Write("," + chunk.Type.ToString(CultureInfo.InvariantCulture) + "," + chunk.DoorLevel.ToString(CultureInfo.InvariantCulture) + "," + chunk.Variant.ToString(CultureInfo.InvariantCulture));
                        for (int k = 0; k < chunk.Links.Length; ++k)
                        {
                            mapFile.Write(",");

                            if (chunk.Links[k] != null)
                                mapFile.Write("1");
                            else
                                mapFile.Write("0");
                        }
                    }
                }
                mapFile.Write("\n");
            }

            mapFile.Write("\n");
        }

        private void WriteTilesets(StreamWriter mapFile, string tilesetDefinitions)
        {
            mapFile.Write("[tilesets]\n");

            mapFile.Write(tilesetDefinitions + "\n");

            mapFile.Write("\n");
        }

        private void WriteLayers(StreamWriter mapFile)
        {
            for (int i = 0; i < _map.Layernames.Count; i++)
            {
                mapFile.Write("[layer]\n");

                mapFile.Write("type=" + _map.Layernames[i] + "\n");
                mapFile.Write("data=\n");

                string layer = "";
                for (int line = 0; line < _map.H; line++)
                {
                    StringBuilder mapRow = new StringBuilder();
                    for (int tile = 0; tile < _map.W; tile++)
                    {
                        mapRow.Append(_map.Layers[i][tile][line].ToString(CultureInfo.InvariantCulture)).Append(',');
                    }
                    layer += mapRow.ToString();
                    layer += '\n';
                }
                layer = layer.Substring(0, layer.Length - 2);
                layer += '\n';

                mapFile.Write(layer + "\n");
            }
        }

        private void WriteEnemies(StreamWriter mapFile)
        {
            for (int i = 0; i < _map.EnemyGroups.Count; ++i)
            {
                MapGroup group = _map.EnemyGroups[i];

                mapFile.Write("[enemy]\n");

                if (group.Type == "")
                {
                    mapFile.Write("type=enemy\n");
                }
                else
                {
                    mapFile.Write("type=" + group.Type + "\n");
                }

                mapFile.Write("location=" + group.Pos.X.ToString(CultureInfo.InvariantCulture) + "," + group.Pos.Y.ToString(CultureInfo.InvariantCulture) + "," + group.Area.X.ToString(CultureInfo.InvariantCulture) + "," + group.Area.Y.ToString(CultureInfo.InvariantCulture) + "\n");

                mapFile.Write("category=" + group.Category + "\n");

                if (group.Levelmin != 0 || group.Levelmax != 0)
                {
                    mapFile.Write("level=" + group.Levelmin.ToString(CultureInfo.InvariantCulture) + "," + group.Levelmax.ToString(CultureInfo.InvariantCulture) + "\n");
                }

                if (group.Numbermin != 1 || group.Numbermax != 1)
                {
                    mapFile.Write("number=" + group.Numbermin.ToString(CultureInfo.InvariantCulture) + "," + group.Numbermax.ToString(CultureInfo.InvariantCulture) + "\n");
                }

                if (group.Chance != 100.0f)
                {
                    mapFile.Write("chance=" + group.Chance.ToString(CultureInfo.InvariantCulture) + "\n");
                }

                if (group.Direction != MapGroup.RandomDirection)
                {
                    mapFile.Write("direction=" + group.Direction.ToString(CultureInfo.InvariantCulture) + "\n");
                }

                if (group.Waypoints.Count > 0 && group.WanderRadius == 0)
                {
                    mapFile.Write("waypoints=");
                    for (int j = 0; j < group.Waypoints.Count; ++j)
                    {
                        mapFile.Write(((int)group.Waypoints[j].X).ToString(CultureInfo.InvariantCulture) + "," + ((int)group.Waypoints[j].Y).ToString(CultureInfo.InvariantCulture) + ";");
                    }
                    mapFile.Write("\n");
                }

                if ((group.WanderRadius != MapGroup.DefaultWanderRadius && group.Waypoints.Count == 0))
                {
                    mapFile.Write("wander_radius=" + group.WanderRadius.ToString(CultureInfo.InvariantCulture) + "\n");
                }

                for (int j = 0; j < group.Requirements.Count; ++j)
                {
                    EventComponent ec = group.Requirements[j];

                    if (ec.Type == EventComponent.RequiresStatus)
                    {
                        mapFile.Write("requires_status=" + ec.S + "\n");
                    }
                    else if (ec.Type == EventComponent.RequiresNotStatus)
                    {
                        mapFile.Write("requires_not_status=" + ec.S + "\n");
                    }
                    else if (ec.Type == EventComponent.RequiresLevel)
                    {
                        mapFile.Write("requires_level=" + ec.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                    }
                    else if (ec.Type == EventComponent.RequiresNotLevel)
                    {
                        mapFile.Write("requires_not_level=" + ec.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                    }
                    else if (ec.Type == EventComponent.RequiresCurrency)
                    {
                        mapFile.Write("requires_currency=" + ec.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                    }
                    else if (ec.Type == EventComponent.RequiresNotCurrency)
                    {
                        mapFile.Write("requires_not_currency=" + ec.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                    }
                    else if (ec.Type == EventComponent.RequiresItem)
                    {
                        mapFile.Write("requires_item=" + ec.Id.ToString(CultureInfo.InvariantCulture) + ":" + ec.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                    }
                    else if (ec.Type == EventComponent.RequiresNotItem)
                    {
                        mapFile.Write("requires_not_item=" + ec.Id.ToString(CultureInfo.InvariantCulture) + ":" + ec.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                    }
                    else if (ec.Type == EventComponent.RequiresClass)
                    {
                        mapFile.Write("requires_class=" + ec.S + "\n");
                    }
                    else if (ec.Type == EventComponent.RequiresNotClass)
                    {
                        mapFile.Write("requires_not_class=" + ec.S + "\n");
                    }
                }

                for (int j = 0; j < group.InvincibleRequirements.Count; ++j)
                {
                    EventComponent ec = group.InvincibleRequirements[j];

                    if (ec.Type == EventComponent.RequiresStatus)
                    {
                        mapFile.Write("requires_status=" + ec.S + "\n");
                    }
                    else if (ec.Type == EventComponent.RequiresNotStatus)
                    {
                        mapFile.Write("requires_not_status=" + ec.S + "\n");
                    }
                    // invincible_requirements only accepts status
                }

                if (group.SpawnLevel.Ratio > 0)
                {
                    mapFile.Write("spawn_level=");

                    if (group.SpawnLevel.Mode == SpawnLevel.ModeDefault)
                    {
                        mapFile.Write("default");
                    }
                    else if (group.SpawnLevel.Mode == SpawnLevel.ModeFixed)
                    {
                        mapFile.Write("fixed," + ((int)group.SpawnLevel.Ratio).ToString(CultureInfo.InvariantCulture));
                    }
                    else if (group.SpawnLevel.Mode == SpawnLevel.ModeLevel)
                    {
                        mapFile.Write("source_level," + group.SpawnLevel.Ratio.ToString(CultureInfo.InvariantCulture));
                    }
                    else if (group.SpawnLevel.Mode == SpawnLevel.ModeStat && group.SpawnLevel.Stat < SharedResources.Eset!.PrimaryStats.Stats.Count)
                    {
                        mapFile.Write("source_stat," + group.SpawnLevel.Ratio.ToString(CultureInfo.InvariantCulture) + "," + SharedResources.Eset.PrimaryStats.Stats[group.SpawnLevel.Stat].Id);
                    }

                    mapFile.Write("\n");
                }

                mapFile.Write("\n");
            }
        }

        private void WriteNPCs(StreamWriter mapFile)
        {
            for (int i = 0; i < _map.MapNpcs.Count; ++i)
            {
                MapNpc npc = _map.MapNpcs[i];

                mapFile.Write("[npc]\n");

                if (npc.Type == "")
                {
                    mapFile.Write("type=npc\n");
                }
                else
                {
                    mapFile.Write("type=" + npc.Type + "\n");
                }

                mapFile.Write("location=" + ((int)npc.Pos.X).ToString(CultureInfo.InvariantCulture) + "," + ((int)npc.Pos.Y).ToString(CultureInfo.InvariantCulture) + ",1,1" + "\n");
                mapFile.Write("filename=" + npc.Id + "\n");

                if (npc.Direction != MapNpc.RandomDirection)
                {
                    mapFile.Write("direction=" + npc.Direction.ToString(CultureInfo.InvariantCulture) + "\n");
                }

                if (npc.Waypoints.Count > 0 && npc.WanderRadius == 0)
                {
                    mapFile.Write("waypoints=");
                    for (int j = 0; j < npc.Waypoints.Count; ++j)
                    {
                        mapFile.Write(((int)npc.Waypoints[j].X).ToString(CultureInfo.InvariantCulture) + "," + ((int)npc.Waypoints[j].Y).ToString(CultureInfo.InvariantCulture) + ";");
                    }
                    mapFile.Write("\n");
                }

                if ((npc.WanderRadius != MapNpc.DefaultWanderRadius && npc.Waypoints.Count == 0))
                {
                    mapFile.Write("wander_radius=" + npc.WanderRadius.ToString(CultureInfo.InvariantCulture) + "\n");
                }


                for (int j = 0; j < npc.Requirements.Count; ++j)
                {
                    EventComponent ec = npc.Requirements[j];

                    if (ec.Type == EventComponent.RequiresStatus)
                    {
                        mapFile.Write("requires_status=" + ec.S + "\n");
                    }
                    else if (ec.Type == EventComponent.RequiresNotStatus)
                    {
                        mapFile.Write("requires_not_status=" + ec.S + "\n");
                    }
                    else if (ec.Type == EventComponent.RequiresLevel)
                    {
                        mapFile.Write("requires_level=" + ec.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                    }
                    else if (ec.Type == EventComponent.RequiresNotLevel)
                    {
                        mapFile.Write("requires_not_level=" + ec.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                    }
                    else if (ec.Type == EventComponent.RequiresCurrency)
                    {
                        mapFile.Write("requires_currency=" + ec.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                    }
                    else if (ec.Type == EventComponent.RequiresNotCurrency)
                    {
                        mapFile.Write("requires_not_currency=" + ec.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                    }
                    else if (ec.Type == EventComponent.RequiresItem)
                    {
                        mapFile.Write("requires_item=" + ec.Id.ToString(CultureInfo.InvariantCulture) + ":" + ec.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                    }
                    else if (ec.Type == EventComponent.RequiresNotItem)
                    {
                        mapFile.Write("requires_not_item=" + ec.Id.ToString(CultureInfo.InvariantCulture) + ":" + ec.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                    }
                    else if (ec.Type == EventComponent.RequiresClass)
                    {
                        mapFile.Write("requires_class=" + ec.S + "\n");
                    }
                    else if (ec.Type == EventComponent.RequiresNotClass)
                    {
                        mapFile.Write("requires_not_class=" + ec.S + "\n");
                    }
                }

                mapFile.Write("\n");
            }
        }

        private void WriteEvents(StreamWriter mapFile)
        {
            for (int i = 0; i < _map.Events.Count; i++)
            {
                Event ev = _map.Events[i];

                mapFile.Write("[event]\n");

                if (ev.Type == "")
                {
                    mapFile.Write("type=event\n");
                }
                else
                {
                    mapFile.Write("type=" + ev.Type + "\n");
                }

                Rectangle location = ev.Location;
                mapFile.Write("location=" + location.X.ToString(CultureInfo.InvariantCulture) + "," + location.Y.ToString(CultureInfo.InvariantCulture) + "," + location.Width.ToString(CultureInfo.InvariantCulture) + "," + location.Height.ToString(CultureInfo.InvariantCulture) + "\n");

                if (ev.ActivateType == Event.ActivateOnTrigger)
                {
                    mapFile.Write("activate=on_trigger\n");
                }
                else if (ev.ActivateType == Event.ActivateOnMapexit)
                {
                    mapFile.Write("activate=on_mapexit\n");
                }
                else if (ev.ActivateType == Event.ActivateOnLeave)
                {
                    mapFile.Write("activate=on_leave\n");
                }
                else if (ev.ActivateType == Event.ActivateOnLoad)
                {
                    mapFile.Write("activate=on_load\n");
                }
                else if (ev.ActivateType == Event.ActivateOnClear)
                {
                    mapFile.Write("activate=on_clear\n");
                }
                else if (ev.ActivateType == Event.ActivateStatic)
                {
                    mapFile.Write("activate=static\n");
                }

                Rectangle hotspot = ev.Hotspot;
                if (hotspot.X == location.X && hotspot.Y == location.Y && hotspot.Width == location.Width && hotspot.Height == location.Height)
                {
                    mapFile.Write("hotspot=" + "location" + "\n");
                }
                else if (hotspot.X != 0 && hotspot.Y != 0 && hotspot.Width != 0 && hotspot.Height != 0)
                {
                    mapFile.Write("hotspot=" + hotspot.X.ToString(CultureInfo.InvariantCulture) + "," + hotspot.Y.ToString(CultureInfo.InvariantCulture) + "," + hotspot.Width.ToString(CultureInfo.InvariantCulture) + "," + hotspot.Height.ToString(CultureInfo.InvariantCulture) + "\n");
                }

                if (ev.Cooldown.Duration > 0)
                {
                    string suffix = "ms";
                    int value = (int)(1000.0f * (float)ev.Cooldown.Duration / SharedResources.Settings!.MaxFramesPerSec);
                    if (value % 1000 == 0)
                    {
                        value = (int)(ev.Cooldown.Duration / SharedResources.Settings.MaxFramesPerSec);
                        suffix = "s";
                    }
                    mapFile.Write("cooldown=" + value.ToString(CultureInfo.InvariantCulture) + suffix + "\n");
                }

                if (ev.Delay.Duration > 0)
                {
                    string suffix = "ms";
                    int value = (int)(1000.0f * (float)ev.Delay.Duration / SharedResources.Settings.MaxFramesPerSec);
                    if (value % 1000 == 0)
                    {
                        value = (int)(ev.Delay.Duration / SharedResources.Settings.MaxFramesPerSec);
                        suffix = "s";
                    }
                    mapFile.Write("delay=" + value.ToString(CultureInfo.InvariantCulture) + suffix + "\n");
                }

                Rectangle reachableFrom = ev.ReachableFrom;
                if (reachableFrom.X != 0 && reachableFrom.Y != 0 && reachableFrom.Width != 0 && reachableFrom.Height != 0)
                {
                    mapFile.Write("reachable_from=" + reachableFrom.X.ToString(CultureInfo.InvariantCulture) + "," + reachableFrom.Y.ToString(CultureInfo.InvariantCulture) + "," + reachableFrom.Width.ToString(CultureInfo.InvariantCulture) + "," + reachableFrom.Height.ToString(CultureInfo.InvariantCulture) + "\n");
                }
                WriteEventComponents(mapFile, i);

                mapFile.Write("\n");
            }
        }

        private void WriteEventComponents(StreamWriter mapFile, int eventID)
        {
            List<EventComponent> components = _map.Events[eventID].Components;
            for (int i = 0; i < components.Count; i++)
            {
                EventComponent e = components[i];

                if (e.Type > 0 && e.Type < EventComponent.EventComponentCount)
                {
                    if (e.Type == EventComponent.ProcgenFilename || e.Type == EventComponent.ProcgenLink)
                    {
                        continue;
                    }
                    else if (e.Type == EventComponent.Loot && e.Data[LootManager.LootEcType].Int == LootManager.LootEcTypeTableRow)
                    {
                        continue;
                    }
                    else if (_eventComponentName[e.Type] != "")
                    {
                        mapFile.Write(_eventComponentName[e.Type] + "=");
                    }
                }
                else
                {
                    continue;
                }

                if (e.Type == EventComponent.Tooltip)
                {
                    mapFile.Write(e.S + "\n");
                }
                else if (e.Type == EventComponent.Power)
                {
                    mapFile.Write(e.Id.ToString(CultureInfo.InvariantCulture) + "\n");
                }
                else if (e.Type == EventComponent.PowerPath)
                {
                    mapFile.Write(e.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "," + e.Data[1].Int.ToString(CultureInfo.InvariantCulture) + ",");
                    if (e.Data[4].Bool)
                    {
                        mapFile.Write("hero" + "\n");
                    }
                    else
                    {
                        mapFile.Write(e.Data[2].Int.ToString(CultureInfo.InvariantCulture) + "," + e.Data[3].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                    }
                }
                else if (e.Type == EventComponent.PowerDamage)
                {
                    mapFile.Write(e.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "," + e.Data[1].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                }
                else if (e.Type == EventComponent.PowerStats)
                {
                    mapFile.Write(e.S + "\n");
                }
                else if (e.Type == EventComponent.PowerLevel)
                {
                    mapFile.Write(e.S + "\n");
                }
                else if (e.Type == EventComponent.Intermap)
                {
                    mapFile.Write(e.S);

                    if (e.Data[0].Int != -1 || e.Data[1].Int != -1 || e.Data[3].Int != 0)
                    {
                        mapFile.Write("," + e.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "," + e.Data[1].Int.ToString(CultureInfo.InvariantCulture));
                        if (e.Data[3].Int != 0)
                        {
                            mapFile.Write("," + SharedGameResources.Eventm!.GetIntermapIDString((uint)e.Data[3].Int));
                        }
                    }

                    mapFile.Write("\n");
                }
                else if (e.Type == EventComponent.IntermapID)
                {
                    mapFile.Write(SharedGameResources.Eventm!.GetIntermapIDString((uint)e.Data[0].Int) + "\n");
                }
                else if (e.Type == EventComponent.Intramap)
                {
                    mapFile.Write(e.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "," + e.Data[1].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                }
                else if (e.Type == EventComponent.Mapmod)
                {
                    mapFile.Write(e.S + "," + e.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "," + e.Data[1].Int.ToString(CultureInfo.InvariantCulture) + "," + e.Data[2].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                }
                else if (e.Type == EventComponent.MapmodToggle)
                {
                    mapFile.Write(e.S + "," + e.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "," + e.Data[1].Int.ToString(CultureInfo.InvariantCulture) + "," + e.Data[2].Int.ToString(CultureInfo.InvariantCulture) + "," + e.Data[3].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                }
                else if (e.Type == EventComponent.Soundfx)
                {
                    mapFile.Write(e.S);
                    if (e.Data[0].Int != -1 && e.Data[1].Int != -1)
                    {
                        mapFile.Write("," + e.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "," + e.Data[1].Int.ToString(CultureInfo.InvariantCulture) + "," + (e.Data[2].Bool ? 1 : 0).ToString(CultureInfo.InvariantCulture));
                    }
                    mapFile.Write("\n");
                }
                else if (e.Type == EventComponent.Loot)
                {
                    if (e.Data[LootManager.LootEcType].Int == LootManager.LootEcTypeTable)
                    {
                        mapFile.Write(e.S + "\n");
                    }
                    else if (e.Data[LootManager.LootEcType].Int == LootManager.LootEcTypeSingle)
                    {
                        mapFile.Write(e.S + ",");

                        if (e.Data[LootManager.LootEcChance].Int == 0)
                            mapFile.Write("fixed");
                        else
                            mapFile.Write(e.Data[LootManager.LootEcChance].Int.ToString(CultureInfo.InvariantCulture));

                        mapFile.Write("," + e.Data[LootManager.LootEcQuantityMin].Int.ToString(CultureInfo.InvariantCulture) + "," + e.Data[LootManager.LootEcQuantityMax].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                    }
                }
                else if (e.Type == EventComponent.LootCount)
                {
                    mapFile.Write(e.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "," + e.Data[1].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                }
                else if (e.Type == EventComponent.Msg)
                {
                    mapFile.Write(e.S + "\n");
                }
                else if (e.Type == EventComponent.Shakycam)
                {
                    string suffix = "ms";
                    int value = (int)(1000.0f * e.Data[0].Float / SharedResources.Settings!.MaxFramesPerSec);
                    if (value % 1000 == 0)
                    {
                        value = e.Data[0].Int / SharedResources.Settings.MaxFramesPerSec;
                        suffix = "s";
                    }
                    mapFile.Write(value.ToString(CultureInfo.InvariantCulture) + suffix + "\n");
                }
                else if (e.Type == EventComponent.RequiresStatus)
                {
                    mapFile.Write(e.S + "\n");
                }
                else if (e.Type == EventComponent.RequiresNotStatus)
                {
                    mapFile.Write(e.S + "\n");
                }
                else if (e.Type == EventComponent.RequiresLevel)
                {
                    mapFile.Write(e.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                }
                else if (e.Type == EventComponent.RequiresNotLevel)
                {
                    mapFile.Write(e.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                }
                else if (e.Type == EventComponent.RequiresCurrency)
                {
                    mapFile.Write(e.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                }
                else if (e.Type == EventComponent.RequiresNotCurrency)
                {
                    mapFile.Write(e.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                }
                else if (e.Type == EventComponent.RequiresItem)
                {
                    mapFile.Write(e.Id.ToString(CultureInfo.InvariantCulture) + ":" + e.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                }
                else if (e.Type == EventComponent.RequiresNotItem)
                {
                    mapFile.Write(e.Id.ToString(CultureInfo.InvariantCulture) + ":" + e.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                }
                else if (e.Type == EventComponent.RequiresClass)
                {
                    mapFile.Write(e.S + "\n");
                }
                else if (e.Type == EventComponent.RequiresNotClass)
                {
                    mapFile.Write(e.S + "\n");
                }
                else if (e.Type == EventComponent.RequiresTile)
                {
                    mapFile.Write(e.S + "," + e.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "," + e.Data[1].Int.ToString(CultureInfo.InvariantCulture) + "," + e.Data[2].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                }
                else if (e.Type == EventComponent.RequiresNotTile)
                {
                    mapFile.Write(e.S + "," + e.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "," + e.Data[1].Int.ToString(CultureInfo.InvariantCulture) + "," + e.Data[2].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                }
                else if (e.Type == EventComponent.SetStatus)
                {
                    mapFile.Write(e.S + "\n");
                }
                else if (e.Type == EventComponent.UnsetStatus)
                {
                    mapFile.Write(e.S + "\n");
                }
                else if (e.Type == EventComponent.RemoveCurrency)
                {
                    mapFile.Write(e.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                }
                else if (e.Type == EventComponent.RemoveItem)
                {
                    mapFile.Write(e.Id.ToString(CultureInfo.InvariantCulture) + ":" + e.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                }
                else if (e.Type == EventComponent.RewardXp)
                {
                    mapFile.Write(e.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                }
                else if (e.Type == EventComponent.RewardCurrency)
                {
                    mapFile.Write(e.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                }
                else if (e.Type == EventComponent.RewardItem)
                {
                    mapFile.Write(e.Id.ToString(CultureInfo.InvariantCulture) + ":" + e.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                }
                else if (e.Type == EventComponent.Restore)
                {
                    mapFile.Write(e.S + "\n");
                }
                else if (e.Type == EventComponent.Spawn)
                {
                    mapFile.Write(e.S + "," + e.Data[0].Int.ToString(CultureInfo.InvariantCulture) + "," + e.Data[1].Int.ToString(CultureInfo.InvariantCulture) + "\n");
                }
                else if (e.Type == EventComponent.SpawnLevel)
                {
                    mapFile.Write(e.S + "\n");
                }
                else if (e.Type == EventComponent.Stash)
                {
                    mapFile.Write((e.Data[0].Bool ? 1 : 0).ToString(CultureInfo.InvariantCulture) + "\n");
                }
                else if (e.Type == EventComponent.Npc)
                {
                    mapFile.Write(e.S + "\n");
                }
                else if (e.Type == EventComponent.Music)
                {
                    mapFile.Write(e.S + "\n");
                }
                else if (e.Type == EventComponent.Cutscene)
                {
                    mapFile.Write(e.S + "\n");
                }
                else if (e.Type == EventComponent.Repeat)
                {
                    mapFile.Write((e.Data[0].Bool ? 1 : 0).ToString(CultureInfo.InvariantCulture) + "\n");
                }
                else if (e.Type == EventComponent.SaveGame)
                {
                    mapFile.Write((e.Data[0].Bool ? 1 : 0).ToString(CultureInfo.InvariantCulture) + "\n");
                }
                else if (e.Type == EventComponent.Book)
                {
                    mapFile.Write(e.S + "\n");
                }
                else if (e.Type == EventComponent.Script)
                {
                    mapFile.Write(e.S + "\n");
                }
                else if (e.Type == EventComponent.ChanceExec)
                {
                    mapFile.Write(e.Data[0].Float.ToString(CultureInfo.InvariantCulture) + "\n");
                }
                else if (e.Type == EventComponent.Respec)
                {
                    if (e.Data[0].Int == 3)
                        mapFile.Write("xp");
                    else if (e.Data[0].Int == 2)
                        mapFile.Write("stats");
                    else if (e.Data[0].Int == 1)
                        mapFile.Write("powers");

                    mapFile.Write("," + (e.Data[1].Bool ? 1 : 0).ToString(CultureInfo.InvariantCulture) + "\n");
                }
                else if (e.Type == EventComponent.ShowOnMinimap)
                {
                    mapFile.Write((e.Data[0].Bool ? 1 : 0).ToString(CultureInfo.InvariantCulture) + "\n");
                }
                else if (e.Type == EventComponent.ParallaxLayers)
                {
                    mapFile.Write(e.S + "\n");
                }
                else if (e.Type == EventComponent.RandomStatus)
                {
                    if (e.Data[0].Int == EventComponent.RandomStatusModeAppend)
                        mapFile.Write("append," + e.S + "\n");
                    else if (e.Data[0].Int == EventComponent.RandomStatusModeClear)
                        mapFile.Write("clear" + "\n");
                    else if (e.Data[0].Int == EventComponent.RandomStatusModeRoll)
                        mapFile.Write("roll" + "\n");
                    else if (e.Data[0].Int == EventComponent.RandomStatusModeSet)
                        mapFile.Write("set" + "\n");
                    else if (e.Data[0].Int == EventComponent.RandomStatusModeUnset)
                        mapFile.Write("unset" + "\n");
                }
                else if (e.Type == EventComponent.ProcgenFilename)
                {
                    mapFile.Write(e.S + "\n");
                }
            }
        }
    }
}
