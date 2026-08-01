// 对应 C++ 源：GameStateCutscene.h + GameStateCutscene.cpp
using Stride.Core.Mathematics;

namespace FlareEngine
{
    public class CutsceneSettings
    {
        public Vector2 CaptionMargins;
        public Color CaptionBackground;
        public float VscrollSpeed;
        public float VscrollSpeedFast;

        public CutsceneSettings()
        {
            CaptionBackground = new Color(0, 0, 0, 200);
            VscrollSpeed = 0.5f * Settings.LogicFps / SharedResources.Settings!.MaxFramesPerSec;
            VscrollSpeedFast = VscrollSpeed * 16;
        }
    }

    public class SceneComponent
    {
        public string Type;
        public string S;
        public int X;
        public int Y;
        public int Z;

        public SceneComponent()
        {
            Type = "";
            S = "";
            X = 0;
            Y = 0;
            Z = 0;
        }
    }

    public class VScrollComponent
    {
        public Int2 Pos;
        public Sprite? Image;
        public Int2 ImageSize;
        public WidgetLabel? Text;
        public int SeparatorH;

        public VScrollComponent()
        {
            Image = null;
            Text = null;
            SeparatorH = 0;
        }
    }

    public class Scene : IDisposable
    {
        private const int SkipNone = 0;
        private const int SkipSubscene = 1;
        private const int SkipPrev = 2;
        private const int SkipNext = 3;
        private const int SkipVscrollBack = 4;

        public const int NoChange = 0;
        public const int Prev = 1;
        public const int Next = 2;
        public const int Done = 3;

        public const int CutsceneScaleNone = 0;
        public const int CutsceneScaleHeight = 1;
        public const int CutsceneScaleScreen = 2;

        public const int CutsceneStatic = 0;
        public const int CutsceneVscroll = 1;

        private CutsceneSettings _cutsceneSettings;
        private int _frameCounter;
        private int _pauseFrames;
        private string _caption;
        private Sprite? _art;
        private Sprite? _artScaled;
        private int _artScaleType;
        private Int2 _artSize;
        private SoundID _sid;
        private WidgetScrollBox? _captionBox;
        private WidgetButton _buttonPrev;
        private WidgetButton _buttonNext;
        private WidgetButton _buttonClose;
        private WidgetButton? _buttonAdvance;
        private int _vscrollOffset;
        private float _vscrollY;
        private int _subIndex;
        private int _prevSubIndex;

        public short CutsceneType;
        public bool IsFirstScene;
        public bool IsLastScene;
        public List<int> Subscenes = new List<int>();
        public List<SceneComponent> Components = new List<SceneComponent>();
        public List<VScrollComponent> VscrollComponents = new List<VScrollComponent>();

        public Scene(CutsceneSettings settings, short cutsceneType)
        {
            _cutsceneSettings = settings;
            _frameCounter = 0;
            _pauseFrames = 0;
            _caption = "";
            _art = null;
            _artScaled = null;
            _artScaleType = CutsceneScaleNone;
            _sid = -1;
            _captionBox = null;
            _buttonPrev = new WidgetButton(WidgetButton.DirLeftFile);
            _buttonNext = new WidgetButton(WidgetButton.DirRightFile);
            _buttonClose = new WidgetButton(WidgetButton.CloseFile);
            _buttonAdvance = null;
            _vscrollOffset = 0;
            _vscrollY = 0;
            _subIndex = 0;
            _prevSubIndex = 0;
            CutsceneType = cutsceneType;
            IsFirstScene = false;
            IsLastScene = false;
        }

        public Scene(Scene other)
        {
            CopyFrom(other);
        }

        public Scene CopyFrom(Scene other)
        {
            if (ReferenceEquals(this, other))
                return this;

            _cutsceneSettings = other._cutsceneSettings;
            _frameCounter = other._frameCounter;
            _pauseFrames = other._pauseFrames;
            _caption = other._caption;
            _art = null;
            _artScaled = null;
            _artScaleType = other._artScaleType;
            _sid = other._sid;
            _captionBox = null;
            _buttonPrev = new WidgetButton(WidgetButton.DirLeftFile);
            _buttonNext = new WidgetButton(WidgetButton.DirRightFile);
            _buttonClose = new WidgetButton(WidgetButton.CloseFile);
            _buttonAdvance = null;
            _vscrollOffset = other._vscrollOffset;
            _vscrollY = other._vscrollY;
            _subIndex = other._subIndex;
            _prevSubIndex = other._prevSubIndex;
            CutsceneType = other.CutsceneType;
            IsFirstScene = other.IsFirstScene;
            IsLastScene = other.IsLastScene;

            return this;
        }

        public void Dispose()
        {
            ClearArt();
            ClearSound();
            _captionBox?.Dispose();
            _captionBox = null;
            _buttonPrev.Dispose();
            _buttonNext.Dispose();
            _buttonClose.Dispose();

            for (int i = 0; i < VscrollComponents.Count; ++i)
            {
                if (VscrollComponents[i].Image != null)
                    VscrollComponents[i].Image!.Dispose();
                if (VscrollComponents[i].Text != null)
                    VscrollComponents[i].Text!.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        private void ClearArt()
        {
            _art?.Dispose();
            _art = null;
            _artScaled?.Dispose();
            _artScaled = null;

            _artSize = new Int2(0, 0);
        }

        private void ClearSound()
        {
            if (_sid != 0)
            {
                SharedResources.Snd!.Unload(_sid);
            }
            _sid = 0;
        }

        public void Reset()
        {
            _frameCounter = 0;
            _pauseFrames = 0;
            _caption = "";
            ClearArt();
            ClearSound();
            _captionBox?.Dispose();
            _captionBox = null;
            _vscrollOffset = 0;
            _vscrollY = 0;
            for (int i = 0; i < VscrollComponents.Count; ++i)
            {
                if (VscrollComponents[i].Image != null)
                    VscrollComponents[i].Image!.Dispose();
                if (VscrollComponents[i].Text != null)
                    VscrollComponents[i].Text!.Dispose();
            }
            VscrollComponents.Clear();

            _prevSubIndex = 0;
            if (_subIndex > 0)
                _prevSubIndex = _subIndex - 1;

            _subIndex = 0;

            RefreshWidgets();
        }

        public int Logic()
        {
            var snd = SharedResources.Snd;
            var inpt = SharedResources.Inpt!;
            var settings = SharedResources.Settings!;
            var renderDevice = SharedResources.RenderDevice!;

            _buttonPrev.Enabled = !(IsFirstScene && _subIndex == 0);

            if (IsLastScene && (CutsceneType == CutsceneVscroll || _subIndex + 1 >= Subscenes.Count))
                _buttonAdvance = _buttonClose;
            else
                _buttonAdvance = _buttonNext;

            int skip = SkipNone;
            if (_buttonPrev.CheckClick())
            {
                skip = SkipPrev;
            }
            else if (_buttonAdvance.CheckClick())
            {
                skip = SkipNext;
            }
            else if (inpt.Pressing[Input.Main1] && Utils.IsWithinRect(_buttonPrev.Pos, inpt.Mouse))
            {
                inpt.Lock[Input.Main1] = true;
            }

            if (!_buttonPrev.Pressed && !_buttonAdvance.Pressed)
            {
                if (inpt.Pressing[Input.Main1] && (!inpt.Lock[Input.Main1] || CutsceneType == CutsceneVscroll))
                {
                    inpt.Lock[Input.Main1] = true;
                    skip = SkipSubscene;
                }
                else if (inpt.Pressing[Input.Accept] && (!inpt.Lock[Input.Accept] || CutsceneType == CutsceneVscroll))
                {
                    inpt.Lock[Input.Accept] = true;
                    skip = SkipSubscene;
                }
                else if (inpt.Pressing[Input.Right] && !inpt.Lock[Input.Right])
                {
                    inpt.Lock[Input.Right] = true;
                    skip = SkipNext;
                }
                else if (_buttonPrev.Enabled && inpt.Pressing[Input.Left] && !inpt.Lock[Input.Left])
                {
                    inpt.Lock[Input.Left] = true;
                    skip = SkipPrev;
                }
                else if (inpt.Pressing[Input.Cancel] && !inpt.Lock[Input.Cancel])
                {
                    inpt.Lock[Input.Cancel] = true;
                    return Done;
                }
                else if (CutsceneType == CutsceneVscroll && inpt.Pressing[Input.Up])
                {
                    skip = SkipVscrollBack;
                }
                else if (CutsceneType == CutsceneVscroll && inpt.Pressing[Input.Down])
                {
                    skip = SkipSubscene;
                }
            }

            if (CutsceneType == CutsceneStatic)
            {
                if (skip == SkipPrev)
                {
                    if (_subIndex == 0)
                    {
                        Reset();
                        return Prev;
                    }
                    else
                    {
                        Reset();
                    }
                }
                else if (skip == SkipNone && _pauseFrames != 0 && (_frameCounter < _pauseFrames || _pauseFrames == -1))
                {
                    if (_pauseFrames > 0)
                        ++_frameCounter;
                    return NoChange;
                }
                else if (skip == SkipSubscene || skip == SkipNext || (_pauseFrames != 0 && _frameCounter == _pauseFrames))
                {
                    _subIndex++;
                }

                string imageFilename = "";
                string sfxFilename = "";

                if (_subIndex < Subscenes.Count)
                {
                    for (int i = Subscenes[_subIndex]; i < Components.Count; ++i)
                    {
                        if (Components[i].Type == "caption")
                        {
                            _caption = Components[i].S;
                        }
                        else if (Components[i].Type == "image")
                        {
                            imageFilename = Components[i].S;
                            _artScaleType = Components[i].X;
                        }
                        else if (Components[i].Type == "soundfx")
                        {
                            sfxFilename = Components[i].S;
                        }
                        else if (Components[i].Type == "pause")
                        {
                            if (_subIndex < _prevSubIndex)
                            {
                                _subIndex++;
                            }
                            else
                            {
                                _pauseFrames = Components[i].X;
                                _frameCounter = 0;
                                break;
                            }

                            if (_subIndex == _prevSubIndex)
                            {
                                _prevSubIndex = 0;
                            }
                        }
                    }
                }

                if (imageFilename.Length != 0)
                {
                    ClearArt();

                    Image? graphics = renderDevice.LoadImage(imageFilename, RenderDevice.ErrorNormal);
                    if (graphics != null)
                    {
                        _art = graphics.CreateSprite();
                        _artSize.X = _art.GetGraphicsWidth();
                        _artSize.Y = _art.GetGraphicsHeight();
                        graphics.Unref();
                    }
                }

                if (sfxFilename.Length != 0)
                {
                    ClearSound();

                    _sid = snd!.Load(sfxFilename, "Cutscenes");
                    snd.Play(_sid, SoundManager.DefaultChannel, SoundManager.NoPos, !SoundManager.Loop);
                }

                if (_subIndex >= Subscenes.Count || Subscenes[_subIndex] >= Components.Count)
                    return Next;

                RefreshWidgets();
            }
            else if (CutsceneType == CutsceneVscroll)
            {
                if (skip == SkipPrev)
                {
                    Reset();
                    return Prev;
                }

                if (VscrollComponents.Count == 0)
                {
                    int nextY = 0;
                    for (int i = 0; i < Components.Count; ++i)
                    {
                        if (Components[i].Type == "text")
                        {
                            VScrollComponent vsc = new VScrollComponent();
                            vsc.Pos.X = settings.ViewW / 2;
                            vsc.Pos.Y = settings.ViewH / 2 + nextY;

                            vsc.Text = new WidgetLabel();
                            if (vsc.Text != null)
                            {
                                vsc.Text.SetPos(vsc.Pos.X, vsc.Pos.Y);
                                vsc.Text.SetJustify(FontEngine.JustifyCenter);
                                vsc.Text.SetText(Components[i].S);
                                vsc.Text.SetFont("font_captions");
                                nextY += vsc.Text.GetBounds().Height;
                            }

                            VscrollComponents.Add(vsc);
                        }
                        else if (Components[i].Type == "image")
                        {
                            VScrollComponent vsc = new VScrollComponent();

                            Image? graphics = renderDevice.LoadImage(Components[i].S, RenderDevice.ErrorNormal);
                            if (graphics != null)
                            {
                                vsc.Image = graphics.CreateSprite();
                                if (vsc.Image != null)
                                {
                                    vsc.ImageSize.X = vsc.Image.GetGraphicsWidth();
                                    vsc.ImageSize.Y = vsc.Image.GetGraphicsHeight();

                                    vsc.Pos.X = settings.ViewW / 2 - vsc.ImageSize.X / 2;
                                    vsc.Pos.Y = settings.ViewH / 2 + nextY;

                                    nextY += vsc.ImageSize.Y;

                                    VscrollComponents.Add(vsc);
                                }
                                graphics.Unref();
                            }
                        }
                        else if (Components[i].Type == "separator")
                        {
                            VScrollComponent vsc = new VScrollComponent();
                            vsc.Pos.Y = settings.ViewH / 2 + nextY + Components[i].X / 2;
                            nextY += Components[i].X;

                            VscrollComponents.Add(vsc);
                        }
                    }
                }

                _vscrollOffset = (int)_vscrollY;
                if (skip == SkipNext)
                {
                    return Next;
                }
                else if (skip == SkipSubscene)
                {
                    _vscrollY += _cutsceneSettings.VscrollSpeedFast;
                }
                else if (skip == SkipVscrollBack)
                {
                    _vscrollY -= _cutsceneSettings.VscrollSpeedFast;
                    if (_vscrollY < 0)
                        _vscrollY = 0;
                }
                else
                {
                    _vscrollY += _cutsceneSettings.VscrollSpeed;
                }

                RefreshWidgets();

                if (VscrollComponents.Count != 0)
                {
                    VScrollComponent vsc = VscrollComponents[VscrollComponents.Count - 1];
                    if (vsc.Text != null && (vsc.Text.GetBounds().Y + vsc.Text.GetBounds().Height < 0))
                    {
                        return Next;
                    }
                    else if ((vsc.Pos.Y + vsc.SeparatorH) - _vscrollOffset < 0)
                    {
                        return Next;
                    }
                }
            }

            return NoChange;
        }

        public void RefreshWidgets()
        {
            var settings = SharedResources.Settings!;
            var font = SharedResources.Font!;

            if (CutsceneType == CutsceneStatic)
            {
                if (_caption.Length != 0)
                {
                    int captionWidth = settings.ViewW - (int)(settings.ViewW * (_cutsceneSettings.CaptionMargins.X * 2.0f));
                    font.SetFont("font_captions");
                    int padding = font.GetLineHeight() / 4;
                    Int2 captionSize = font.CalcSizeWrapped(_caption, captionWidth);
                    Int2 captionSizePadded = new Int2(captionSize.X + padding * 2, captionSize.Y + padding * 2);

                    if (_captionBox == null)
                    {
                        _captionBox = new WidgetScrollBox(captionSizePadded.X, captionSizePadded.Y);
                        _captionBox.SetBasePos(0, 0, Utils.AlignBottom);
                        _captionBox.Bg = _cutsceneSettings.CaptionBackground;
                        _captionBox.Resize(captionSizePadded.X, captionSizePadded.Y);
                    }
                    else
                    {
                        _captionBox.Pos.Height = captionSizePadded.Y;
                        _captionBox.Resize(captionSizePadded.X, captionSizePadded.Y);
                    }

                    _captionBox.SetPos(0, (int)((float)settings.ViewH * _cutsceneSettings.CaptionMargins.Y) * (-1));

                    font.RenderShadowed(_caption, (padding / 2) + (captionSizePadded.X / 2), padding,
                        FontEngine.JustifyCenter,
                        _captionBox.Contents!.GetGraphics()!,
                        captionWidth,
                        font.GetColor(FontEngine.ColorWhite));
                }

                if (_art != null)
                {
                    Rectangle artDest = default;
                    if (_artScaleType != CutsceneScaleNone)
                    {
                        if (_artScaleType == CutsceneScaleScreen)
                            artDest = Utils.ResizeToScreen(_artSize.X, _artSize.Y, false, Utils.AlignCenter);
                        else if (_artScaleType == CutsceneScaleHeight)
                            artDest = Utils.ResizeToScreen(_artSize.X, _artSize.Y, true, Utils.AlignCenter);

                        _art.GetGraphics()!.Ref();
                        Image? resized = _art.GetGraphics()!.Resize(artDest.Width, artDest.Height);
                        if (resized != null)
                        {
                            if (_artScaled != null)
                            {
                                _artScaled.Dispose();
                            }
                            _artScaled = resized.CreateSprite();
                            resized.Unref();
                        }

                        if (_artScaled != null)
                            _artScaled.SetDestFromRect(artDest);
                    }
                    else
                    {
                        artDest.Width = _artSize.X;
                        artDest.Height = _artSize.Y;

                        Utils.AlignToScreenEdge(Utils.AlignCenter, ref artDest);
                        _art.SetDestFromRect(artDest);
                    }
                }
            }
            else if (CutsceneType == CutsceneVscroll)
            {
                for (int i = 0; i < VscrollComponents.Count; ++i)
                {
                    if (VscrollComponents[i].Text != null)
                    {
                        VscrollComponents[i].Text!.SetPos(settings.ViewW / 2, VscrollComponents[i].Pos.Y - _vscrollOffset);
                    }
                    else if (VscrollComponents[i].Image != null)
                    {
                        int x = settings.ViewW / 2 - VscrollComponents[i].ImageSize.X / 2;
                        int y = VscrollComponents[i].Pos.Y - _vscrollOffset;
                        VscrollComponents[i].Image!.SetDest(x, y);
                    }
                }
            }

            _buttonPrev.SetBasePos(0, 0, Utils.AlignTopLeft);
            _buttonPrev.SetPos(_buttonPrev.Pos.Width / 2, _buttonPrev.Pos.Height / 2);
            _buttonNext.SetBasePos(0, 0, Utils.AlignTopRight);
            _buttonNext.SetPos(-(_buttonNext.Pos.Width / 2), _buttonNext.Pos.Height / 2);
            _buttonClose.SetBasePos(0, 0, Utils.AlignTopRight);
            _buttonClose.SetPos(-(_buttonClose.Pos.Width / 2), _buttonClose.Pos.Height / 2);
        }

        public void Render()
        {
            var inpt = SharedResources.Inpt!;
            var settings = SharedResources.Settings!;
            var renderDevice = SharedResources.RenderDevice!;

            if (inpt.WindowResized)
                RefreshWidgets();

            if (CutsceneType == CutsceneStatic)
            {
                if (_artScaled != null)
                {
                    renderDevice.Render(_artScaled);
                }
                else if (_art != null)
                {
                    renderDevice.Render(_art);
                }

                if (_captionBox != null && _caption != "")
                {
                    _captionBox.Render();
                }
            }
            else if (CutsceneType == CutsceneVscroll)
            {
                for (int i = 0; i < VscrollComponents.Count; ++i)
                {
                    VScrollComponent vsc = VscrollComponents[i];

                    if (vsc.Text != null)
                    {
                        if (vsc.Text.GetBounds().Y <= settings.ViewH && (vsc.Text.GetBounds().Y + vsc.Text.GetBounds().Height >= 0))
                        {
                            vsc.Text.Render();
                        }
                    }
                    else if (vsc.Image != null)
                    {
                        Int2 dest = vsc.Image.GetDest();
                        if (dest.Y <= settings.ViewH && (dest.Y + vsc.ImageSize.Y >= 0))
                        {
                            renderDevice.Render(vsc.Image);
                        }
                    }
                }
            }

            bool onlyScene = IsFirstScene && IsLastScene;
            if (!onlyScene || (CutsceneType == CutsceneStatic && Subscenes.Count > 1))
                _buttonPrev.Render();

            _buttonAdvance!.Render();
        }
    }

    public class GameStateCutscene : GameState
    {
        private GameState? _previousGamestate;
        private string _destMap;
        private Int2 _destPos;

        private int _sceneIndex;
        private List<Scene> _scenes;
        private string _music;
        private bool _initialized;
        private int _status;

        public int GameSlot;

        public GameStateCutscene(GameState? gameState)
        {
            _previousGamestate = gameState;
            _destMap = "";
            _destPos = default;
            _sceneIndex = 0;
            _scenes = new List<Scene>();
            _music = "";
            _initialized = false;
            _status = Scene.NoChange;
            GameSlot = -1;
            HasBackground = false;
        }

        public override void Dispose()
        {
            if (_music.Length != 0)
                SharedResources.Snd!.StopMusic();

            for (int i = 0; i < _scenes.Count; ++i)
            {
                _scenes[i].Dispose();
            }
            _scenes.Clear();

            base.Dispose();
        }

        public override void Logic()
        {
            var settings = SharedResources.Settings!;
            var snd = SharedResources.Snd!;
            var saveLoad = SharedResources.SaveLoad!;

            if (!_initialized)
            {
                if (settings.MusicVolume > 0 && _music.Length != 0)
                {
                    snd.StopMusic();
                    snd.LoadMusic(_music);
                }

                _initialized = true;
            }

            if (_scenes.Count == 0 || _sceneIndex >= _scenes.Count)
            {
                if (GameSlot != -1)
                {
                    ShowLoading();
                    GameStatePlay gsp = new GameStatePlay();
                    gsp.ResetGame();
                    saveLoad.GameSlot = GameSlot;
                    saveLoad.LoadGame();

                    SetRequestedGameState(gsp);
                    return;
                }

                ShowLoading();
                SetRequestedGameState(_previousGamestate);
                return;
            }

            if (_sceneIndex < _scenes.Count)
                _status = _scenes[_sceneIndex].Logic();
        }

        public override void Render()
        {
            if (_scenes.Count != 0 && _sceneIndex < _scenes.Count)
            {
                _scenes[_sceneIndex].Render();

                if (_status == Scene.Done)
                {
                    _sceneIndex = _scenes.Count;
                }
                else if (_status == Scene.Next)
                {
                    _sceneIndex++;
                }
                else if (_status == Scene.Prev && _sceneIndex > 0)
                {
                    _scenes[_sceneIndex].Reset();
                    _sceneIndex--;
                    _scenes[_sceneIndex].Reset();
                }
            }
        }

        public bool Load(string filename)
        {
            var msg = SharedResources.Msg!;
            CutsceneSettings cutsceneSettings = new CutsceneSettings();
            FileParser infile = new FileParser();

            if (!infile.Open(filename, FileParser.ModFile, FileParser.ErrorNormal))
                return false;

            Utils.LogInfo("GameStateCutscene: Loading cutscene '%s'", filename);

            while (infile.Next())
            {
                if (infile.NewSection)
                {
                    if (infile.Section == "scene")
                    {
                        _scenes.Add(new Scene(cutsceneSettings, Scene.CutsceneStatic));
                        _scenes[_scenes.Count - 1].Subscenes.Add(0);
                    }
                    else if (infile.Section == "vscroll")
                    {
                        if (_scenes.Count == 0 || _scenes[_scenes.Count - 1].CutsceneType != Scene.CutsceneVscroll)
                        {
                            _scenes.Add(new Scene(cutsceneSettings, Scene.CutsceneVscroll));
                        }
                    }
                }

                if (infile.Section.Length == 0)
                {
                    if (infile.Key == "caption_margins")
                    {
                        cutsceneSettings.CaptionMargins.X = Parse.PopFirstFloat(ref infile.Val) / 100.0f;
                        cutsceneSettings.CaptionMargins.Y = Parse.PopFirstFloat(ref infile.Val) / 100.0f;
                    }
                    else if (infile.Key == "caption_background")
                    {
                        cutsceneSettings.CaptionBackground = Parse.ToRGBA(infile.Val);
                    }
                    else if (infile.Key == "vscroll_speed")
                    {
                        cutsceneSettings.VscrollSpeed = Parse.ToFloat(infile.Val) * Settings.LogicFps / SharedResources.Settings!.MaxFramesPerSec;
                        cutsceneSettings.VscrollSpeedFast = cutsceneSettings.VscrollSpeed * 2;
                    }
                    else if (infile.Key == "menu_backgrounds")
                    {
                        HasBackground = true;
                    }
                    else if (infile.Key == "music")
                    {
                        _music = infile.Val;
                        HasMusic = true;
                    }
                    else
                    {
                        infile.Error("GameStateCutscene: '%s' is not a valid key.", infile.Key);
                    }
                }
                else if (infile.Section == "scene")
                {
                    SceneComponent sc = new SceneComponent();

                    if (infile.Key == "caption")
                    {
                        sc.Type = infile.Key;
                        sc.S = msg.Get(infile.Val);
                    }
                    else if (infile.Key == "image")
                    {
                        sc.Type = infile.Key;
                        sc.S = Parse.PopFirstString(ref infile.Val);
                        sc.X = Parse.PopFirstInt(ref infile.Val);
                        if (sc.X < Scene.CutsceneScaleNone || sc.X > Scene.CutsceneScaleScreen)
                        {
                            infile.Error("GameStateCutscene: '%d' is not a valid scaling type.", sc.X);
                            sc.X = Scene.CutsceneScaleNone;
                        }
                    }
                    else if (infile.Key == "pause")
                    {
                        sc.Type = infile.Key;
                        string temp = Parse.PopFirstString(ref infile.Val);
                        if (temp == "-1")
                            sc.X = -1;
                        else
                            sc.X = Parse.ToDuration(temp);
                        _scenes[_scenes.Count - 1].Subscenes.Add(_scenes[_scenes.Count - 1].Components.Count + 1);
                    }
                    else if (infile.Key == "soundfx")
                    {
                        sc.Type = infile.Key;
                        sc.S = infile.Val;
                    }
                    else
                    {
                        infile.Error("GameStateCutscene: '%s' is not a valid key.", infile.Key);
                    }

                    if (sc.Type != "")
                        _scenes[_scenes.Count - 1].Components.Add(sc);
                }
                else if (infile.Section == "vscroll")
                {
                    SceneComponent sc = new SceneComponent();

                    if (infile.Key == "text")
                    {
                        sc.Type = infile.Key;
                        sc.S = msg.Get(infile.Val);
                    }
                    else if (infile.Key == "image")
                    {
                        sc.Type = infile.Key;
                        sc.S = infile.Val;
                    }
                    else if (infile.Key == "separator")
                    {
                        sc.Type = infile.Key;
                        sc.X = Parse.ToInt(infile.Val);
                    }
                    else
                    {
                        infile.Error("GameStateCutscene: '%s' is not a valid key.", infile.Key);
                    }

                    if (sc.Type != "")
                        _scenes[_scenes.Count - 1].Components.Add(sc);
                }
                else
                {
                    infile.Error("GameStateCutscene: '%s' is not a valid section.", infile.Section);
                }
            }

            infile.Close();

            if (_scenes.Count == 0)
            {
                Utils.LogInfo("GameStateCutscene: No scenes defined in cutscene file %s", filename);
                return false;
            }
            else
            {
                if (_scenes[_scenes.Count - 1].Components[_scenes[_scenes.Count - 1].Components.Count - 1].Type == "pause")
                {
                    _scenes[_scenes.Count - 1].Subscenes.RemoveAt(_scenes[_scenes.Count - 1].Subscenes.Count - 1);
                }

                _scenes[0].IsFirstScene = true;
                _scenes[_scenes.Count - 1].IsLastScene = true;
            }

            SharedResources.RenderDevice!.SetBackgroundColor(new Color(0, 0, 0, 0));

            return true;
        }
    }
}
