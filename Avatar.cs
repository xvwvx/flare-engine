// <自动生成> 对应 C++ 源文件：Avatar.h + Avatar.cpp
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// ActionData - 玩家行动指令数据，对应 C++ <c>class ActionData</c>。
    /// 存储待执行的动作（技能、物品使用、移动目标等）。
    /// </summary>
    public class ActionData
    {
        public PowerID Power;
        public uint Hotkey;
        public bool InstantItem;
        public bool ActivatedFromInventory;
        public Vector2 Target;

        public ActionData()
        {
            Power = 0;
            Hotkey = 0;
            InstantItem = false;
            ActivatedFromInventory = false;
            Target = default;
        }
    }

    /// <summary>
    /// Avatar - 玩家角色类，对应 C++ <c>class Avatar : public Entity</c>。
    /// 管理玩家的移动、技能冷却、装备外观、变形等逻辑。
    /// HeroStats 和 CharmedStats 为 StatBlock 指针，由 Dispose 管理生命周期。
    /// PowerCooldownTimers/PowerCastTimers 为 Timer 数组，在析构时释放。
    /// </summary>
    public class Avatar : Entity
    {
        private class StepSfx
        {
            public string Id = "";
            public List<string> Steps = new List<string>();
        }

        private const int PathFoundFailThreshold = 1;
        private const int PathFoundFailWaitSeconds = 2;

        private readonly List<StepSfx> _stepDef = new List<StepSfx>();

        private readonly List<SoundID> _soundSteps = new List<SoundID>();

        private short _body;

        private bool _transformTriggered;
        private string _lastTransform = "";

        private int _mmKey;
        private bool _mmIsDistant;

        private readonly Timer _setDirTimer = new Timer();

        private readonly List<Vector2> _path = new List<Vector2>();
        private Vector2 _prevTarget;
        private bool _collided;
        private bool _pathFound;
        private int _chanceCalcPath;
        private int _pathFoundFails;
        private readonly Timer _pathFoundFailTimer = new Timer();

        private Vector2 _mmTarget;
        private Vector2 _mmTargetDesired;

        private readonly List<PowerID> _powerCooldownIds = new List<PowerID>();

        public const int MsgNormal = 0;
        public const int MsgUnique = 1;

        public const int MmTargetNone = 0;
        public const int MmTargetEvent = 1;
        public const int MmTargetLoot = 2;
        public const int MmTargetEntity = 3;

        /// <summary>获取寻路路径点列表，对应 C++ <c>getPath()</c>。</summary>
        public List<Vector2> Path => _path;

        /// <summary>获取鼠标移动目标点，对应 C++ <c>getMMTarget()</c>。</summary>
        public ref Vector2 MMTarget => ref _mmTarget;

        public Queue<(string First, int Second)> LogMsgQueue = new Queue<(string, int)>();

        public string AttackAnim = "";
        public bool SetPowers;
        public bool RevertPowers;
        public PowerID UntransformPower;
        public StatBlock? HeroStats;
        public StatBlock? CharmedStats;
        public Vector2 TransformPos;
        public string TransformMap = "";

        public PowerID CurrentPower;
        public PowerID CurrentPowerOriginal;
        public Vector2 ActTarget;
        public bool DragWalking;
        public bool NewLevelNotification;
        public bool Respawn;
        public bool CloseMenus;
        public bool AllowMovement;
        public List<Timer?> PowerCooldownTimers = new List<Timer?>();
        public List<Timer?> PowerCastTimers = new List<Timer?>();
        public Entity? CursorEnemy;
        public Entity? LockEnemy;
        public ulong TimePlayed;
        public bool QuestlogDismissed;
        public bool UsingMain1;
        public bool UsingMain2;
        public float PrevHp;
        public bool PlayingLowhp;
        public bool TeleportCameraLock;
        public int FeetIndex;
        public int MmTargetObject;
        public Vector2 MmTargetObjectPos;
        public bool BlockXpGain;

        public List<ActionData> ActionQueue = new List<ActionData>();

        public Avatar()
        {
            Settings settings = SharedResources.Settings!;
            PowerManager powers = SharedGameResources.Powers!;

            _mmKey = settings.MouseMoveSwap ? Input.Main2 : Input.Main1;
            _mmIsDistant = false;
            _prevTarget = default;
            _collided = false;
            _pathFound = false;
            _chanceCalcPath = 0;
            _pathFoundFails = 0;
            _mmTarget = new Vector2(-1, -1);
            _mmTargetDesired = new Vector2(-1, -1);
            HeroStats = null;
            CharmedStats = null;
            ActTarget = default;
            DragWalking = false;
            Respawn = false;
            CloseMenus = false;
            AllowMovement = true;
            CursorEnemy = null;
            LockEnemy = null;
            TimePlayed = 0;
            QuestlogDismissed = false;
            UsingMain1 = false;
            UsingMain2 = false;
            PrevHp = 0;
            PlayingLowhp = false;
            TeleportCameraLock = false;
            FeetIndex = -1;
            MmTargetObject = MmTargetNone;
            MmTargetObjectPos = default;
            BlockXpGain = false;

            PowerCooldownTimers.Capacity = powers.Powers.Count;
            PowerCastTimers.Capacity = powers.Powers.Count;
            for (int i = 0; i < powers.Powers.Count; i++)
            {
                PowerCooldownTimers.Add(null);
                PowerCastTimers.Add(null);
            }

            for (int i = 0; i < powers.Powers.Count; ++i)
            {
                if (powers.IsValid(i))
                {
                    _powerCooldownIds.Add(i);
                    PowerCooldownTimers[i] = new Timer();
                    PowerCastTimers[i] = new Timer();
                }
            }

            Init();

            LoadLayerDefinitions();

            StepSfx temp = new StepSfx();
            StepSfx? current = temp;

            using FileParser infile = new FileParser();
            if (infile.Open("items/step_sounds.txt", FileParser.ModFile, FileParser.ErrorNone))
            {
                while (infile.Next())
                {
                    if (infile.Key != "id" && current!.Id == "")
                    {
                        infile.Error("Avatar: Expected 'id', but found '%s'.", infile.Key);
                    }

                    if (infile.Key == "id")
                    {
                        bool foundId = false;
                        for (int i = 0; i < _stepDef.Count; ++i)
                        {
                            if (_stepDef[i].Id == infile.Val)
                            {
                                _stepDef[i] = new StepSfx();
                                current = _stepDef[i];
                                current.Id = infile.Val;
                                foundId = true;
                            }
                        }
                        if (!foundId)
                        {
                            _stepDef.Add(temp);
                            current = _stepDef[^1];
                            current.Id = infile.Val;
                        }
                    }

                    if (infile.Key == "step")
                    {
                        current!.Steps.Add(infile.Val);
                    }
                }
                infile.Close();
            }

            LoadStepFX(Stats.SfxStep);
        }

        public void Init()
        {
            MapRenderer mapr = SharedGameResources.Mapr!;
            PowerManager powers = SharedGameResources.Powers!;
            EngineSettings eset = SharedResources.Eset!;

            Sprites = null;
            Stats.CurState = StatBlock.EntityStance;
            if (mapr.HeroPosEnabled)
            {
                Stats.Pos.X = mapr.HeroPos.X;
                Stats.Pos.Y = mapr.HeroPos.Y;
            }
            CurrentPower = 0;
            CurrentPowerOriginal = 0;
            NewLevelNotification = false;

            Stats.Hero = true;
            Stats.Humanoid = true;
            Stats.Level = 1;
            Stats.Xp = 0;
            for (int i = 0; i < eset.PrimaryStats.Stats.Count; ++i)
            {
                Stats.Primary[i] = Stats.PrimaryStarting[i] = 1;
                Stats.PrimaryAdditional[i] = 0;
            }
            Stats.Speed = 0.2f;
            Stats.Recalc();

            while (LogMsgQueue.Count > 0)
            {
                LogMsgQueue.Dequeue();
            }
            Respawn = false;

            Stats.Cooldown.Reset(Timer.End);

            _body = -1;

            _transformTriggered = false;
            SetPowers = false;
            RevertPowers = false;
            _lastTransform = "";

            UntransformPower = 0;
            for (int i = 0; i < powers.Powers.Count; ++i)
            {
                if (!powers.IsValid(i))
                    continue;

                if (UntransformPower == 0 && powers.Powers[i]!.RequiredItems.Count == 0 && powers.Powers[i]!.SpawnType == "untransform")
                {
                    UntransformPower = i;
                }

                if (PowerCooldownTimers[i] != null)
                    PowerCooldownTimers[i] = new Timer();
                if (PowerCastTimers[i] != null)
                    PowerCastTimers[i] = new Timer();
            }

            Stats.Animations = "animations/hero.txt";
        }

        public void HandleNewMap()
        {
            CursorEnemy = null;
            LockEnemy = null;
            PlayingLowhp = false;

            Stats.TargetCorpse = null;
            Stats.TargetNearest = null;
            Stats.TargetNearestCorpse = null;

            _path.Clear();
            _mmTargetDesired = Stats.Pos;
            MmTargetObjectPos = Stats.Pos;

            MmTargetObject = MmTargetNone;
        }

        /// <summary>
        /// 对应 C++ <c>loadGraphics</c>已全局导入，此处省略。 .cpp 
        /// 已全局导入，此处省略。API ?
        /// </summary>
        public void LoadGraphics(List<LayerGfx> imgGfx)
        {
        }

        private void LoadLayerDefinitions()
        {
            if (Stats.LayerReferenceOrder.Count != 0)
                return;

            Utils.LogError("Avatar: Loading render layers from engine/hero_layers.txt is deprecated! Render layers should be loaded in the 'render_layers' section of engine/stats.txt.");

            using FileParser infile = new FileParser();
            if (infile.Open("engine/hero_layers.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (infile.Section == "")
                        infile.Section = "render_layers";

                    if (!Stats.LoadRenderLayerStat(infile))
                    {
                        infile.Error("Avatar: '%s' is not a valid key.", infile.Key);
                    }
                }
                infile.Close();
            }
        }

        public void LoadStepFX(string stepname)
        {
            SoundManager snd = SharedResources.Snd!;

            string filename = Stats.SfxStep;
            if (stepname != "")
            {
                filename = stepname;
            }

            for (int i = 0; i < _soundSteps.Count; i++)
            {
                snd.Unload(_soundSteps[i]);
            }
            _soundSteps.Clear();

            if (filename == "") return;

            if (stepname == "NULL") return;

            for (int i = 0; i < _stepDef.Count; i++)
            {
                if (_stepDef[i].Id == filename)
                {
                    _soundSteps.Clear();
                    for (int j = 0; j < _stepDef[i].Steps.Count; j++)
                    {
                        _soundSteps.Add(snd.Load(_stepDef[i].Steps[j], "Avatar loading foot steps"));
                    }
                    return;
                }
            }

            Utils.LogError("Avatar: Could not find footstep sounds for '%s'.", filename);
        }

        private bool PressingMove()
        {
            Settings settings = SharedResources.Settings!;
            InputState inpt = SharedResources.Inpt!;

            if (!AllowMovement || TeleportCameraLock)
            {
                return false;
            }
            else if (Stats.Effects.KnockbackSpeed != 0)
            {
                return false;
            }
            else if (settings.MouseMove)
            {
                return _mmIsDistant && !IsNearMMtarget();
            }
            else
            {
                return (inpt.Pressing[Input.Up] && !inpt.Lock[Input.Up]) ||
                       (inpt.Pressing[Input.Down] && !inpt.Lock[Input.Down]) ||
                       (inpt.Pressing[Input.Left] && !inpt.Lock[Input.Left]) ||
                       (inpt.Pressing[Input.Right] && !inpt.Lock[Input.Right]);
            }
        }

        private void SetDirection()
        {
            Settings settings = SharedResources.Settings!;
            InputState inpt = SharedResources.Inpt!;
            MapRenderer mapr = SharedGameResources.Mapr!;
            EngineSettings eset = SharedResources.Eset!;

            if (TeleportCameraLock || !_setDirTimer.IsEnd())
                return;

            int oldDir = Stats.Direction;

            if (settings.MouseMove)
            {
                if (_mmIsDistant)
                {
                    if (inpt.Pressing[_mmKey] && (!inpt.Lock[_mmKey] || DragWalking))
                    {
                        Vector2 mmTargetTest = Utils.ScreenToMap(inpt.Mouse.X, inpt.Mouse.Y, mapr.Cam.Pos.X, mapr.Cam.Pos.Y);
                        if (mapr.Collider.IsValidPosition(mmTargetTest.X, mmTargetTest.Y, Stats.MovementType, MapCollision.CollideTypeHero))
                        {
                            inpt.Lock[_mmKey] = true;
                            _mmTargetDesired = mmTargetTest;
                        }
                    }

                    _mmTarget = _mmTargetDesired;

                    if (_collided || !mapr.Collider.LineOfMovement(Stats.Pos.X, Stats.Pos.Y, _mmTarget.X, _mmTarget.Y, Stats.MovementType))
                    {
                        bool recalculatePath = false;

                        _chanceCalcPath += 5;

                        bool calcPathSuccess = MathUtils.PercentChance(_chanceCalcPath);
                        if (calcPathSuccess)
                            recalculatePath = true;

                        if (_collided)
                            recalculatePath = true;

                        if (!recalculatePath && _path.Count == 0)
                            recalculatePath = true;

                        if (!recalculatePath && Utils.CalcDist(_prevTarget.ToInt2().ToVector2(), _mmTarget.ToInt2().ToVector2()) > 1f)
                            recalculatePath = true;

                        if (!_pathFound && _collided && !calcPathSuccess)
                        {
                            recalculatePath = false;
                        }
                        else
                        {
                            _collided = false;
                        }

                        if (!_pathFoundFailTimer.IsEnd())
                        {
                            recalculatePath = false;
                            _chanceCalcPath = -100;
                        }

                        _prevTarget = _mmTarget;

                        if (recalculatePath)
                        {
                            _chanceCalcPath = -100;
                            _path.Clear();
                            _pathFound = mapr.Collider.ComputePath(Stats.Pos, _mmTarget, _path, Stats.MovementType, MapCollision.DefaultPathLimit);

                            if (!_pathFound)
                            {
                                _pathFoundFails++;
                                if (_pathFoundFails >= PathFoundFailThreshold)
                                {
                                    _pathFoundFailTimer.Reset(Timer.Begin);
                                }
                            }
                            else
                            {
                                _pathFoundFails = 0;
                                _pathFoundFailTimer.Reset(Timer.End);
                            }
                        }

                        if (_path.Count != 0)
                        {
                            _mmTarget = _path[^1];

                            if (Utils.CalcDist(Stats.Pos, _mmTarget) <= 1f)
                                _path.RemoveAt(_path.Count - 1);
                        }
                    }
                    else
                    {
                        _path.Clear();
                    }

                    Stats.Direction = Utils.CalcDirection(Stats.Pos.X, Stats.Pos.Y, _mmTarget.X, _mmTarget.Y);
                }
            }
            else
            {
                bool pressUp = inpt.Pressing[Input.Up] && !inpt.Lock[Input.Up];
                bool pressDown = inpt.Pressing[Input.Down] && !inpt.Lock[Input.Down];
                bool pressLeft = inpt.Pressing[Input.Left] && !inpt.Lock[Input.Left];
                bool pressRight = inpt.Pressing[Input.Right] && !inpt.Lock[Input.Right];

                if (!pressUp && !pressDown && !pressLeft && !pressRight)
                {
                    pressUp = inpt.Pressing[Input.AimUp] && !inpt.Lock[Input.AimUp];
                    pressDown = inpt.Pressing[Input.AimDown] && !inpt.Lock[Input.AimDown];
                    pressLeft = inpt.Pressing[Input.AimLeft] && !inpt.Lock[Input.AimLeft];
                    pressRight = inpt.Pressing[Input.AimRight] && !inpt.Lock[Input.AimRight];
                }

                if (pressUp && pressLeft) Stats.Direction = 1;
                else if (pressUp && pressRight) Stats.Direction = 3;
                else if (pressDown && pressRight) Stats.Direction = 5;
                else if (pressDown && pressLeft) Stats.Direction = 7;
                else if (pressLeft) Stats.Direction = 0;
                else if (pressUp) Stats.Direction = 2;
                else if (pressRight) Stats.Direction = 4;
                else if (pressDown) Stats.Direction = 6;

                if (eset.Tileset.Orientation == EngineSettings.TilesetSettings.TilesetOrthogonal && (pressUp || pressDown || pressLeft || pressRight))
                    Stats.Direction = (byte)((Stats.Direction == 7) ? 0 : Stats.Direction + 1);
            }
            if (settings.MouseMove)
            {
                int delayTicks = settings.MaxFramesPerSec / 2;

                float realSpeed = Stats.Speed * StatBlock.SpeedMultiplier[Stats.Direction] * Stats.Effects.Speed / 100;
                int maxTurnTicks = (int)(Utils.CalcDist(Stats.Pos, _mmTarget) * 0.5f / realSpeed);
                if (delayTicks > maxTurnTicks)
                {
                    _setDirTimer.Duration = (uint)(maxTurnTicks);
                }
                else
                {
                    _setDirTimer.Duration = (uint)(delayTicks);
                }
            }
            else
            {
                if (Stats.Direction != oldDir)
                {
                    _setDirTimer.Duration = (uint)(settings.MaxFramesPerSec / 10);
                }
            }
        }

        /// <summary>
        /// logic()
        /// Handle a single frame.
        /// </summary>
        public new void Logic()
        {
            Settings settings = SharedResources.Settings!;
            InputState inpt = SharedResources.Inpt!;
            MapRenderer mapr = SharedGameResources.Mapr!;
            PowerManager powers = SharedGameResources.Powers!;
            MenuManager menu = SharedGameResources.Menu!;
            MessageEngine msg = SharedResources.Msg!;
            SoundManager snd = SharedResources.Snd!;
            EngineSettings eset = SharedResources.Eset!;
            Avatar pc = SharedGameResources.Pc!;
            SaveLoad saveLoad = SharedResources.SaveLoad!;
            CursorManager curs = SharedResources.Curs!;

            bool restrictPowerUse = false;
            if (settings.MouseMove)
            {
                if (inpt.Pressing[_mmKey] && !inpt.Pressing[Input.Shift] && !menu.Act!.IsWithinSlots(inpt.Mouse) && !menu.Act!.IsWithinMenus(inpt.Mouse))
                {
                    restrictPowerUse = true;
                }
            }

            mapr.Collider.Unblock(Stats.Pos.X, Stats.Pos.Y);

            if ((Stats.Hp > 0 || Stats.Effects.TriggeredDeath) && !Respawn && !_transformTriggered)
                powers.ActivatePassives(Stats);

            if (_transformTriggered)
                _transformTriggered = false;

            if (Stats.Effects.TriggeredBlock && !Stats.Blocking)
            {
                Stats.CurState = StatBlock.EntityStance;
                Stats.Effects.TriggeredBlock = false;
                Stats.Effects.ClearTriggerEffects(Power.TriggerBlock);
                Stats.RefreshStats = true;
                Stats.BlockPower = 0;
            }

            Stats.Logic();

            if (IsDroppedToLowHp())
            {
                if (IsLowHpMessageEnabled())
                {
                    LogMsg(msg.Get("Your health is low!"), MsgNormal);
                }
                if (IsLowHpSoundEnabled() && !PlayingLowhp)
                {
                    snd.Play(SoundLowhp, "lowhp", SoundManager.NoPos, Stats.SfxLowhpLoop, !Stats.SfxLowhpLoop);
                    PlayingLowhp = true;
                }
            }
            if (IsLowHpSoundEnabled() && !IsLowHp() && PlayingLowhp && Stats.SfxLowhpLoop)
            {
                snd.PauseChannel("lowhp");
                PlayingLowhp = false;
            }
            else if (IsLowHpSoundEnabled() && IsLowHp() && !PlayingLowhp && Stats.SfxLowhpLoop)
            {
                snd.Play(SoundLowhp, "lowhp", SoundManager.NoPos, Stats.SfxLowhpLoop, !Stats.SfxLowhpLoop);
                PlayingLowhp = true;
            }
            else if (!IsLowHpSoundEnabled() && PlayingLowhp)
            {
                snd.PauseChannel("lowhp");
                PlayingLowhp = false;
            }

            PrevHp = Stats.Hp;

            if (Stats.Level < eset.Xp.GetMaxLevel() && Stats.Xp >= eset.Xp.GetLevelXP(Stats.Level + 1))
            {
                Stats.LevelUp = true;
                Stats.Level = eset.Xp.GetLevelFromXP(Stats.Xp);
                LogMsg(msg.GetV("Congratulations, you have reached level %d!", Stats.Level), MsgNormal);
                if (pc.Stats.StatPointsPerLevel > 0)
                {
                    LogMsg(msg.Get("You may increase one or more attributes through the Character Menu."), MsgNormal);
                    NewLevelNotification = true;
                }
                if (pc.Stats.PowerPointsPerLevel > 0)
                {
                    LogMsg(msg.Get("You may unlock one or more abilities through the Powers Menu."), MsgNormal);
                }
                Stats.Recalc();
                snd.Play(SoundLevelup, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);

                if (Stats.CurState == StatBlock.EntityDead)
                {
                    Stats.CurState = StatBlock.EntityStance;
                }
            }

            BlockXpGain = (Stats.Hp == 0);

            _mmKey = settings.MouseMoveSwap ? Input.Main2 : Input.Main1;
            if (!inpt.Pressing[_mmKey])
            {
                DragWalking = false;
            }

            UsingMain1 = inpt.Pressing[Input.Main1] && !inpt.Lock[Input.Main1];
            UsingMain2 = inpt.Pressing[Input.Main2] && !inpt.Lock[Input.Main2];

            if (!Stats.Effects.Stun && !Stats.HoldState)
            {
                if (ActiveAnimation != null)
                    ActiveAnimation.AdvanceFrame();

                for (int i = 0; i < Anims.Count; ++i)
                {
                    if (Anims[i] != null)
                        Anims[i]!.AdvanceFrame();
                }
            }

            if (Stats.Transformed && mapr.Collider.IsValidPosition(Stats.Pos.X, Stats.Pos.Y, MapCollision.MoveNormal, MapCollision.CollideTypeHero))
            {
                TransformPos = Stats.Pos;
                TransformMap = mapr.Filename;
            }

            if (settings.MouseMove)
            {
                if (inpt.Pressing[_mmKey])
                {
                    Vector2 target = Utils.ScreenToMap(inpt.Mouse.X, inpt.Mouse.Y, mapr.Cam.Pos.X, mapr.Cam.Pos.Y);
                    if (Stats.CurState == StatBlock.EntityMove)
                    {
                        _mmIsDistant = Utils.CalcDist(Stats.Pos, target) >= eset.Misc.MouseMoveDeadzoneMoving;
                    }
                    else
                    {
                        _mmIsDistant = Utils.CalcDist(Stats.Pos, target) >= eset.Misc.MouseMoveDeadzoneNotMoving;
                    }

                    if (!inpt.Lock[_mmKey])
                    {
                        if (settings.MouseMoveAttack && CursorEnemy != null && !CursorEnemy.Stats.HeroAlly)
                        {
                            inpt.Lock[_mmKey] = true;
                            LockEnemy = CursorEnemy;
                            MmTargetObject = MmTargetEntity;
                        }

                        if (CursorEnemy == null)
                        {
                            LockEnemy = null;
                            if (MmTargetObject == MmTargetEntity)
                                MmTargetObject = MmTargetNone;
                        }
                    }
                }

                if (LockEnemy != null)
                {
                    if (LockEnemy.Stats.Hp <= 0)
                    {
                        LockEnemy = null;
                        MmTargetObject = MmTargetNone;
                    }
                    else
                    {
                        MmTargetObjectPos = LockEnemy.Stats.Pos;
                        SetDesiredMMTarget(ref MmTargetObjectPos);
                    }
                }
            }

            if (TeleportCameraLock && Utils.CalcDist(Stats.Pos, mapr.Cam.Pos) < 0.5f)
            {
                TeleportCameraLock = false;
            }

            _setDirTimer.Tick();
            if (!PressingMove())
            {
                _setDirTimer.Reset(Timer.End);
            }

            if (!Stats.Effects.Stun)
            {
                bool allowedToMove;
                bool allowedToTurn;

                Stats.Blocking = false;

                for (int i = 0; i < ActionQueue.Count; i++)
                {
                    ActionData action = ActionQueue[i];
                    PowerID replacedId = powers.CheckReplaceByEffect(action.Power, Stats);
                    if (replacedId == 0)
                        continue;

                    Power power = powers.Powers[replacedId]!;

                    if (power.NewState == Power.StateInstant)
                    {
                        Vector2 target = action.Target;
                        BeginPower(replacedId, ref target);
                        powers.Activate(replacedId, Stats, Stats.Pos, target);
                        PowerCooldownTimers[action.Power]!.Duration = (uint)(power.Cooldown);
                        PowerCooldownTimers[replacedId]!.Duration = (uint)(power.Cooldown);
                    }
                    else if (Stats.CurState == StatBlock.EntityBlock)
                    {
                        if (power.Type == Power.TypeBlock)
                        {
                            CurrentPower = replacedId;
                            CurrentPowerOriginal = action.Power;
                            ActTarget = action.Target;
                            AttackAnim = power.AttackAnim;

                            Stats.CurState = StatBlock.EntityBlock;
                            BeginPower(replacedId, ref ActTarget);
                            powers.Activate(replacedId, Stats, Stats.Pos, ActTarget);
                            Stats.RefreshStats = true;
                        }
                    }
                    else if (Stats.CurState == StatBlock.EntityStance || Stats.CurState == StatBlock.EntityMove)
                    {
                        CurrentPower = replacedId;
                        CurrentPowerOriginal = action.Power;
                        ActTarget = action.Target;
                        AttackAnim = power.AttackAnim;
                        ResetActiveAnimation();

                        if (power.NewState == Power.StateAttack)
                        {
                            Stats.CurState = StatBlock.EntityPower;
                        }
                        else if (power.Type == Power.TypeBlock)
                        {
                            Stats.CurState = StatBlock.EntityBlock;
                            BeginPower(replacedId, ref ActTarget);
                            powers.Activate(replacedId, Stats, Stats.Pos, ActTarget);
                            Stats.RefreshStats = true;
                        }
                    }
                }

                ActionQueue.Clear();

                switch (Stats.CurState)
                {
                    case StatBlock.EntityStance:

                        SetAnimation("stance");

                        if (settings.MouseMove)
                        {
                            allowedToMove = restrictPowerUse && (!inpt.Lock[_mmKey] || DragWalking);
                            allowedToTurn = allowedToMove;

                            if ((inpt.Pressing[_mmKey] && inpt.Pressing[Input.Shift]))
                            {
                                inpt.Lock[_mmKey] = false;
                            }
                        }
                        else if (!settings.MouseAim)
                        {
                            allowedToMove = !inpt.Pressing[Input.Shift];
                            allowedToTurn = true;
                        }
                        else
                        {
                            allowedToMove = true;
                            allowedToTurn = true;
                        }

                        if (allowedToTurn)
                            SetDirection();

                        if (PressingMove() && allowedToMove)
                        {
                            if (Move())
                            {
                                if (settings.MouseMove && inpt.Pressing[_mmKey])
                                {
                                    DragWalking = true;
                                }

                                Stats.CurState = StatBlock.EntityMove;

                                MmTargetObject = MmTargetNone;
                            }
                        }

                        break;

                    case StatBlock.EntityMove:

                        SetAnimation("run");

                        if (_soundSteps.Count != 0)
                        {
                            int stepfx = Program.Rng.Next() % _soundSteps.Count;

                            if (ActiveAnimation!.IsFirstFrame() || ActiveAnimation.IsActiveFrame())
                                snd.Play(_soundSteps[stepfx], SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);
                        }

                        SetDirection();

                        if (!PressingMove())
                        {
                            Stats.CurState = StatBlock.EntityStance;
                            break;
                        }
                        else if (!Move())
                        {
                            if (settings.MouseMove && !IsNearMMtarget())
                            {
                                _collided = true;
                            }
                            Stats.CurState = StatBlock.EntityStance;
                            break;
                        }
                        else if ((settings.MouseMove || !settings.MouseAim) && inpt.Pressing[Input.Shift])
                        {
                            Stats.CurState = StatBlock.EntityStance;
                            break;
                        }

                        if (settings.MouseMove && inpt.Pressing[_mmKey] && (DragWalking || !inpt.Lock[_mmKey]))
                        {
                            DragWalking = true;
                        }

                        if (ActiveAnimation!.Name != "run")
                            Stats.CurState = StatBlock.EntityStance;

                        break;

                    case StatBlock.EntityPower:

                        SetAnimation(AttackAnim);

                        if (powers.IsValid(CurrentPower))
                        {
                            Power power = powers.Powers[CurrentPower]!;

                            if (!power.Buff && !power.BuffTeleport &&
                                power.Type != Power.TypeTransform &&
                                power.Type != Power.TypeBlock &&
                                !(power.StartingPos == Power.StartingPosSource && power.Speed == 0)
                            )
                            {
                                curs.SetCursor(CursorManager.CursorAttack);
                            }

                            if (ActiveAnimation!.IsFirstFrame())
                            {
                                BeginPower(CurrentPower, ref ActTarget);
                                float attackSpeed = (Stats.Effects.GetAttackSpeed(AttackAnim) * power.AttackSpeed) / 100.0f;
                                ActiveAnimation.SetSpeed(attackSpeed);
                                for (int i = 0; i < Anims.Count; ++i)
                                {
                                    if (Anims[i] != null)
                                        Anims[i]!.SetSpeed(attackSpeed);
                                }
                                PlayAttackSound(AttackAnim);
                                PowerCastTimers[CurrentPower]!.Duration = (uint)(ActiveAnimation.GetDuration());
                                PowerCastTimers[CurrentPowerOriginal]!.Duration = (uint)(ActiveAnimation.GetDuration());
                            }

                            if (power.StateHoldMode == Power.HoldOnFrame)
                            {
                                if (ActiveAnimation.IsFrame((short)power.StateHoldFrame) && !Stats.HoldState)
                                {
                                    if (!Stats.StateTimer.IsEnd())
                                        Stats.HoldState = true;
                                }
                                if (Stats.StateTimer.IsEnd())
                                    Stats.HoldState = false;
                            }

                            if (ActiveAnimation.IsActiveFrame() && !Stats.HoldState)
                            {
                                mapr.Collider.Block(Stats.Pos.X, Stats.Pos.Y, !MapCollision.IsAlly);

                                powers.Activate(CurrentPower, Stats, Stats.Pos, ActTarget);
                                PowerCooldownTimers[CurrentPower]!.Duration = (uint)(power.Cooldown);
                                PowerCooldownTimers[CurrentPowerOriginal]!.Duration = (uint)(power.Cooldown);

                                if (power.StateHoldMode == Power.HoldOnActiveFrame && !Stats.StateTimer.IsEnd())
                                    Stats.HoldState = true;
                            }
                        }

                        if ((ActiveAnimation!.IsLastFrame() && Stats.StateTimer.IsEnd()) || ActiveAnimation.Name != AttackAnim)
                        {
                            Stats.CurState = StatBlock.EntityStance;
                            Stats.Cooldown.Reset(Timer.Begin);
                            Stats.PreventInterrupt = false;
                        }

                        break;

                    case StatBlock.EntityBlock:

                        SetAnimation("block");

                        break;

                    case StatBlock.EntityHit:

                        SetAnimation("hit");

                        if (ActiveAnimation!.IsFirstFrame())
                        {
                            Stats.Effects.TriggeredHit = true;

                            if (powers.IsValid(Stats.BlockPower))
                            {
                                PowerCooldownTimers[Stats.BlockPower]!.Duration = (uint)(powers.Powers[Stats.BlockPower]!.Cooldown);
                                Stats.BlockPower = 0;
                            }
                        }

                        if (ActiveAnimation.TimesPlayed >= 1 || ActiveAnimation.Name != "hit")
                        {
                            Stats.CurState = StatBlock.EntityStance;
                        }

                        break;

                    case StatBlock.EntityDead:
                        if (Stats.Effects.TriggeredDeath) break;

                        if (Stats.Transformed)
                        {
                            Stats.TransformDuration = 0;
                            Untransform();
                        }

                        SetAnimation("die");

                        if (!Stats.Corpse && ActiveAnimation!.IsFirstFrame() && ActiveAnimation.TimesPlayed < 1)
                        {
                            Stats.Effects.ClearEffects();
                            Stats.PowersPassive.Clear();

                            for (int i = 0; i < PowerCooldownTimers.Count; ++i)
                            {
                                if (PowerCooldownTimers[i] != null)
                                    PowerCooldownTimers[i]!.Reset(Timer.End);
                                if (PowerCastTimers[i] != null)
                                    PowerCastTimers[i]!.Reset(Timer.End);
                            }

                            CloseMenus = true;

                            PlaySound(Entity.SoundDie);

                            LogMsg(msg.Get("You are defeated."), MsgNormal);

                            if (Stats.Permadeath)
                            {
                                Stats.DeathPenalty = false;
                                Utils.RemoveSaveDir(saveLoad.GameSlot);
                                menu.Exit!.DisableSave();
                                menu.GameOver!.DisableSave();
                            }
                            else
                            {
                                Stats.DeathPenalty = true;
                            }

                            if (inpt.Pressing[Input.Main1])
                                inpt.Lock[Input.Main1] = true;
                        }

                        if (!Stats.Corpse && (ActiveAnimation!.TimesPlayed >= 1 || ActiveAnimation.Name != "die"))
                        {
                            Stats.Corpse = true;
                            menu.GameOver!.Visible = true;
                        }

                        if (menu.GameOver!.Visible && menu.GameOver!.ContinueClicked)
                        {
                            menu.GameOver!.Close();

                            mapr.Teleportation = true;
                            mapr.TeleportMapname = mapr.RespawnMap;

                            if (Stats.Permadeath)
                            {
                                mapr.TeleportDestination.X = Stats.Pos.X;
                                mapr.TeleportDestination.Y = Stats.Pos.Y;
                            }
                            else
                            {
                                Respawn = true;

                                mapr.TeleportDestination.X = mapr.RespawnPoint.X;
                                mapr.TeleportDestination.Y = mapr.RespawnPoint.Y;
                            }
                        }

                        break;

                    default:
                        break;
                }
            }

            mapr.Cam.SetTarget(Stats.Pos);

            mapr.CheckEvents(Stats.Pos);

            for (int i = 0; i < _powerCooldownIds.Count; ++i)
            {
                PowerID powerId = _powerCooldownIds[i];
                PowerCooldownTimers[powerId]!.Tick();
                PowerCastTimers[powerId]!.Tick();
            }

            mapr.Collider.Block(Stats.Pos.X, Stats.Pos.Y, !MapCollision.IsAlly);

            if (Stats.StateTimer.IsEnd() && Stats.HoldState)
                Stats.HoldState = false;

            if (Stats.CurState != StatBlock.EntityPower && Stats.ChargeSpeed != 0.0f)
                Stats.ChargeSpeed = 0.0f;
        }

        private void BeginPower(PowerID replacedId, ref Vector2 target)
        {
            PowerManager powers = SharedGameResources.Powers!;
            InputState inpt = SharedResources.Inpt!;

            Power power = powers.Powers[replacedId]!;

            if (power.Type == Power.TypeBlock)
                Stats.Blocking = true;

            if (inpt.UsingMouse() && CursorEnemy != null && !inpt.Pressing[Input.Shift])
            {
                target = CursorEnemy.Stats.Pos;
            }

            if (power.Face)
            {
                Stats.Direction = Utils.CalcDirection(Stats.Pos.X, Stats.Pos.Y, target.X, target.Y);
            }

            if (power.StateDuration > 0)
                Stats.StateTimer.Duration = (uint)(power.StateDuration);

            if (power.ChargeSpeed != 0.0f)
                Stats.ChargeSpeed = power.ChargeSpeed;

            Stats.PreventInterrupt = power.PreventInterrupt;

            for (int j = 0; j < power.ChainPowers.Count; ++j)
            {
                ChainPower chainPower = power.ChainPowers[j];
                if (chainPower.Type == ChainPower.TypePre && MathUtils.PercentChanceF(chainPower.Chance))
                {
                    powers.Activate(chainPower.Id, Stats, Stats.Pos, target);
                }
            }
        }

        public void Transform()
        {
            InputState inpt = SharedResources.Inpt!;
            EnemyGroupManager enemyg = SharedGameResources.Enemyg!;
            MapRenderer mapr = SharedGameResources.Mapr!;

            if (Stats.Hp <= 0)
                return;

            inpt.UnlockActionBar();

            CharmedStats?.Dispose();
            CharmedStats = null;

            EnemyLevel el = enemyg.GetRandomEnemy(Stats.TransformType, 0, 0);

            if (el.Type != "")
            {
                CharmedStats = new StatBlock();
                CharmedStats.Load(el.Type);
            }
            else
            {
                Utils.LogError("Avatar: Could not transform into creature type '%s'", Stats.TransformType);
                Stats.TransformType = "";
                return;
            }

            _transformTriggered = true;
            Stats.Transformed = true;
            SetPowers = true;

            HeroStats?.Dispose();
            HeroStats = new StatBlock(Stats);

            HeroStats!.Summons.Clear();

            Stats.Speed = CharmedStats!.Speed;
            Stats.MovementType = CharmedStats.MovementType;
            Stats.Humanoid = CharmedStats.Humanoid;
            Stats.Animations = CharmedStats.Animations;
            Stats.PowersList = CharmedStats.PowersList;
            Stats.PowersPassive = CharmedStats.PowersPassive;
            Stats.Effects.ClearEffects();
            Stats.Animations = CharmedStats.Animations;
            Stats.LayerReferenceOrder = CharmedStats.LayerReferenceOrder;
            Stats.LayerDef = CharmedStats.LayerDef;
            Stats.AnimationSlots = CharmedStats.AnimationSlots;

            SharedResources.Anim!.DecreaseCount(HeroStats.Animations);
            AnimationSet = null;
            LoadAnimations();
            Stats.CurState = StatBlock.EntityStance;

            for (int i = 0; i < (int)Stat.Count; ++i)
            {
                Stats.Starting[i] = Math.Max(Stats.Starting[i], CharmedStats.Starting[i]);
            }

            LoadSoundsFromStatBlock(CharmedStats);
            LoadStepFX("NULL");

            Stats.ApplyEffects();

            TransformPos = Stats.Pos;
            TransformMap = mapr.Filename;
        }

        public void Untransform()
        {
            InputState inpt = SharedResources.Inpt!;
            MapRenderer mapr = SharedGameResources.Mapr!;
            MessageEngine msg = SharedResources.Msg!;

            inpt.UnlockActionBar();

            mapr.Collider.Unblock(Stats.Pos.X, Stats.Pos.Y);
            if (!mapr.Collider.IsValidPosition(Stats.Pos.X, Stats.Pos.Y, MapCollision.MoveNormal, MapCollision.CollideTypeHero))
            {
                LogMsg(msg.Get("Transformation expired. You have been moved back to a safe place."), MsgNormal);
                if (TransformMap != mapr.Filename)
                {
                    mapr.Teleportation = true;
                    mapr.TeleportMapname = TransformMap;
                    mapr.TeleportDestination.X = MathF.Floor(TransformPos.X) + 0.5f;
                    mapr.TeleportDestination.Y = MathF.Floor(TransformPos.Y) + 0.5f;
                    TransformMap = "";
                }
                else
                {
                    Stats.Pos.X = MathF.Floor(TransformPos.X) + 0.5f;
                    Stats.Pos.Y = MathF.Floor(TransformPos.Y) + 0.5f;
                }
            }
            mapr.Collider.Block(Stats.Pos.X, Stats.Pos.Y, !MapCollision.IsAlly);

            Stats.Transformed = false;
            _transformTriggered = true;
            Stats.TransformType = "";
            RevertPowers = true;
            Stats.Effects.ClearEffects();

            Stats.Speed = HeroStats!.Speed;
            Stats.MovementType = HeroStats.MovementType;
            Stats.Humanoid = HeroStats.Humanoid;
            Stats.Animations = HeroStats.Animations;
            CopyEffectManagerFrom(HeroStats.Effects, Stats.Effects);
            Stats.PowersList = HeroStats.PowersList;
            Stats.PowersPassive = HeroStats.PowersPassive;
            Stats.Animations = HeroStats.Animations;
            Stats.LayerReferenceOrder = HeroStats.LayerReferenceOrder;
            Stats.LayerDef = HeroStats.LayerDef;
            Stats.AnimationSlots = HeroStats.AnimationSlots;

            SharedResources.Anim!.DecreaseCount(CharmedStats!.Animations);
            AnimationSet = null;
            LoadAnimations();
            Stats.CurState = StatBlock.EntityStance;

            SetAnimation("run");

            for (int i = 0; i < (int)Stat.Count; ++i)
            {
                Stats.Starting[i] = HeroStats.Starting[i];
            }

            LoadSounds();
            LoadStepFX(Stats.SfxStep);

            CharmedStats.Dispose();
            HeroStats.Dispose();
            CharmedStats = null;
            HeroStats = null;

            Stats.ApplyEffects();
            Stats.UntransformOnHit = false;
        }

        /// <summary>
        /// 对应 C++ <c>stats.effects = hero_stats-&gt;effects</c> 已全局导入，此处省略。已全局导入，此处省略。
        /// </summary>
        private static void CopyEffectManagerFrom(EffectManager source, EffectManager target)
        {
            target.Dispose();
            target.Damage = source.Damage;
            target.DamagePercent = source.DamagePercent;
            target.Hpot = source.Hpot;
            target.HpotPercent = source.HpotPercent;
            target.Mpot = source.Mpot;
            target.MpotPercent = source.MpotPercent;
            target.ResourceOt = new List<float>(source.ResourceOt);
            target.ResourceOtPercent = new List<float>(source.ResourceOtPercent);
            target.Speed = source.Speed;
            target.Stun = source.Stun;
            target.Revive = source.Revive;
            target.Convert = source.Convert;
            target.DeathSentence = source.DeathSentence;
            target.Fear = source.Fear;
            target.KnockbackSpeed = source.KnockbackSpeed;
            target.Bonus = new List<float>(source.Bonus);
            target.BonusMultiplier = new List<float>(source.BonusMultiplier);
            target.BonusPrimary = new List<int>(source.BonusPrimary);
            target.TriggeredOthers = source.TriggeredOthers;
            target.TriggeredBlock = source.TriggeredBlock;
            target.TriggeredHit = source.TriggeredHit;
            target.TriggeredHalfdeath = source.TriggeredHalfdeath;
            target.TriggeredJoincombat = source.TriggeredJoincombat;
            target.TriggeredDeath = source.TriggeredDeath;
            target.TriggeredActivePower = source.TriggeredActivePower;
            target.RefreshStats = source.RefreshStats;
            target.EffectList.Clear();
            for (int i = 0; i < source.EffectList.Count; ++i)
                target.EffectList.Add(new Effect(source.EffectList[i]));
        }

        public void CheckTransform()
        {
            if (Stats.TransformType != "" && Stats.TransformType != "untransform" && Stats.Transformed == false)
                Transform();
            if (Stats.TransformType != "" && Stats.TransformDuration == 0)
                Untransform();
        }

        public void LogMsg(string str, int type)
        {
            LogMsgQueue.Enqueue((str, type));
        }

        public bool IsLowHp()
        {
            if (Stats.Hp == 0)
                return false;
            float hpOnePerc = Math.Max(Stats.Get(global::FlareEngine.Stats.HpMax), 1f) / 100.0f;
            return Stats.Hp / hpOnePerc < (float)SharedResources.Settings!.LowHpThreshold;
        }

        private bool IsDroppedToLowHp()
        {
            float hpOnePerc = Math.Max(Stats.Get(global::FlareEngine.Stats.HpMax), 1f) / 100.0f;
            return (Stats.Hp / hpOnePerc < (float)SharedResources.Settings!.LowHpThreshold) && (PrevHp / hpOnePerc >= (float)SharedResources.Settings!.LowHpThreshold);
        }

        public bool IsLowHpMessageEnabled()
        {
            Settings settings = SharedResources.Settings!;
            return settings.LowHpWarningType == Settings.LhpWarnText ||
                settings.LowHpWarningType == Settings.LhpWarnTextCursor ||
                settings.LowHpWarningType == Settings.LhpWarnTextSound ||
                settings.LowHpWarningType == Settings.LhpWarnAll;
        }

        public bool IsLowHpSoundEnabled()
        {
            Settings settings = SharedResources.Settings!;
            return settings.LowHpWarningType == Settings.LhpWarnSound ||
                settings.LowHpWarningType == Settings.LhpWarnTextSound ||
                settings.LowHpWarningType == Settings.LhpWarnCursorSound ||
                settings.LowHpWarningType == Settings.LhpWarnAll;
        }

        public bool IsLowHpCursorEnabled()
        {
            Settings settings = SharedResources.Settings!;
            return settings.LowHpWarningType == Settings.LhpWarnCursor ||
                settings.LowHpWarningType == Settings.LhpWarnTextCursor ||
                settings.LowHpWarningType == Settings.LhpWarnCursorSound ||
                settings.LowHpWarningType == Settings.LhpWarnAll;
        }

        public override string GetGfxFromType(string gfxType)
        {
            ItemManager items = SharedGameResources.Items!;
            MenuManager menu = SharedGameResources.Menu!;

            FeetIndex = -1;
            string gfx = "";

            if (menu != null && menu.Inv != null)
            {
                MenuItemStorage equipment = menu.Inv!.Inventory[MenuInventory.Equipment];

                for (int i = 0; i < equipment.GetSlotNumber(); i++)
                {
                    if (!menu.Inv!.IsEquipSlotActive(i))
                        continue;

                    ItemType equipItemType = items.GetItemType(equipment.SlotType[i]);

                    if (items.IsValid(equipment[i].Item) && gfxType == equipItemType.Id)
                    {
                        gfx = items.Items[equipment[i].Item]!.Gfx;
                    }
                    if (equipItemType.Id == "feet")
                    {
                        FeetIndex = i;
                    }
                }
            }

            if (gfx == "" && gfxType == "head")
            {
                gfx = Stats.GfxHead;
            }

            if (gfx == "")
            {
                if (Filesystem.FileExists(SharedResources.Mods!.Locate("animations/avatar/" + Stats.GfxBase + "/default_" + gfxType + ".txt")))
                    gfx = "default_" + gfxType;
            }

            return gfx;
        }

        public bool IsNearMMtarget()
        {
            return _path.Count == 0 && Utils.CalcDist(Stats.Pos, _mmTargetDesired) <= Stats.Speed * 2;
        }

        public void SetDesiredMMTarget(ref Vector2 target)
        {
            _mmTarget = _mmTargetDesired = target;
        }

        public override void Dispose()
        {
            Utils.LogInfo("Cleaning up: Avatar");

            CharmedStats?.Dispose();
            HeroStats?.Dispose();

            UnloadSounds();

            for (int i = 0; i < PowerCooldownTimers.Count; ++i)
            {
                PowerCooldownTimers[i] = null;
                PowerCastTimers[i] = null;
            }

            for (int i = 0; i < _soundSteps.Count; i++)
            {
                SharedResources.Snd!.Unload(_soundSteps[i]);
            }

            base.Dispose();
        }
    }
}
