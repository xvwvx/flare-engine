// <自动生成> 对应 C++ 源文件：MenuTalker.h + MenuTalker.cpp
// 注意：System, System.Collections.Generic, System.Linq, System.Threading.Tasks 已全局导入，此处省略。
using Stride.Core.Mathematics;

namespace FlareEngine
{
    /// <summary>
    /// MenuTalker
    ///
    /// NPC 对话菜单：显示对话文本、头像、话题选项，并可在话题选择与具体对话节点之间切换。
    /// 持有的 <see cref="Sprite"/>、<see cref="WidgetLabel"/>、<see cref="WidgetScrollBox"/>、
    /// <see cref="WidgetButton"/> 等资源通过 <see cref="IDisposable"/> 显式释放，
    /// 释放顺序与原始析构函数 <c>~MenuTalker()</c> 一致（先 <see cref="ClearActionButtons"/>，
    /// 再 portrait、label_name、textbox、advanceButton、closeButton，最后基类 <see cref="Menu"/>）。
    ///
    /// <see cref="WidgetScrollBox"/>、<see cref="WidgetButton"/>、<see cref="NPC"/>、
    /// <see cref="MenuManager"/>、<see cref="MenuVendor"/>、<see cref="MenuInventory"/>、
    /// <see cref="Avatar"/>、<see cref="StatBlock"/> 等为尚未转换或部分转换的前向依赖单元，
    /// 本类按预期的最终符号命名引用其成员（详见转换报告）。
    /// </summary>
    public class MenuTalker : Menu, IDisposable
    {
        private class Action
        {
            public const int NoNode = -1;
            public const bool VendorTrade = true;

            public WidgetButton? Btn;
            public int NodeId;
            public bool IsVendor;

            public Action()
            {
                Btn = null;
                NodeId = 0;
                IsVendor = false;
            }
        }

        private Sprite? _portrait;
        private string _heroName;
        private string _heroClass;

        private int _dialogNode;
        private int _eventCursor;
        private bool _firstInteraction;

        private Rectangle _dialogPos;
        private Rectangle _textPos;
        private Int2 _textOffset;
        private Rectangle _portraitHe;
        private Rectangle _portraitYou;

        private string _fontWho;
        private string _fontDialog;

        private WidgetLabel? _labelName;
        private WidgetScrollBox? _textbox;

        private readonly List<Action> _actions = new List<Action>();

        private Color _topicColorNormal;
        private Color _topicColorHover;
        private Color _topicColorPressed;

        private Color _tradeColorNormal;
        private Color _tradeColorHover;
        private Color _tradeColorPressed;

        /// <summary>对应 C++ 原始公有字段 <c>npc</c>。</summary>
        public NPC? Npc;

        /// <summary>对应 C++ 原始公有字段 <c>advanceButton</c>。</summary>
        public WidgetButton? AdvanceButton;

        /// <summary>对应 C++ 原始公有字段 <c>closeButton</c>。</summary>
        public WidgetButton? CloseButton;

        /// <summary>对应 C++ 原始公有字段 <c>npc_from_map</c>。</summary>
        public bool NpcFromMap;

        public MenuTalker()
        {
            _portrait = null;
            _heroName = "";
            _heroClass = "";
            _dialogNode = -1;
            _eventCursor = 0;
            _firstInteraction = false;
            _fontWho = "font_regular";
            _fontDialog = "font_regular";
            _topicColorNormal = SharedResources.Font!.GetColor(FontEngine.ColorMenuBonus);
            _topicColorHover = SharedResources.Font!.GetColor(FontEngine.ColorWidgetNormal);
            _topicColorPressed = SharedResources.Font!.GetColor(FontEngine.ColorWidgetDisabled);
            _tradeColorNormal = SharedResources.Font!.GetColor(FontEngine.ColorMenuBonus);
            _tradeColorHover = SharedResources.Font!.GetColor(FontEngine.ColorWidgetNormal);
            _tradeColorPressed = SharedResources.Font!.GetColor(FontEngine.ColorWidgetDisabled);
            Npc = null;
            AdvanceButton = new WidgetButton(WidgetButton.DirRightFile);
            CloseButton = new WidgetButton(WidgetButton.CloseFile);
            NpcFromMap = true;

            // Load config settings
            using FileParser infile = new FileParser();
            // @CLASS MenuTalker|Description of menus/talker.txt
            if (infile.Open("menus/talker.txt", FileParser.ModFile, FileParser.ErrorNormal))
            {
                while (infile.Next())
                {
                    if (ParseMenuKey(infile.Key, infile.Val))
                        continue;

                    // @ATTR close|point|Position of the close button.
                    if (infile.Key == "close")
                    {
                        Int2 pos = Parse.ToPoint(infile.Val);
                        CloseButton!.SetBasePos(pos.X, pos.Y, Utils.AlignTopLeft);
                    }
                    // @ATTR advance|point|Position of the button to advance dialog.
                    else if (infile.Key == "advance")
                    {
                        Int2 pos = Parse.ToPoint(infile.Val);
                        AdvanceButton!.SetBasePos(pos.X, pos.Y, Utils.AlignTopLeft);
                    }
                    // @ATTR dialogbox|rectangle|Position and dimensions of the text box graphics.
                    else if (infile.Key == "dialogbox") _dialogPos = Parse.ToRect(infile.Val);
                    // @ATTR dialogtext|rectangle|Rectangle where the dialog text is placed.
                    else if (infile.Key == "dialogtext") _textPos = Parse.ToRect(infile.Val);
                    // @ATTR text_offset|point|Margins for the left/right and top/bottom of the dialog text.
                    else if (infile.Key == "text_offset") _textOffset = Parse.ToPoint(infile.Val);
                    // @ATTR portrait_he|rectangle|Position and dimensions of the NPC portrait graphics.
                    else if (infile.Key == "portrait_he") _portraitHe = Parse.ToRect(infile.Val);
                    // @ATTR portrait_you|rectangle|Position and dimensions of the player's portrait graphics.
                    else if (infile.Key == "portrait_you") _portraitYou = Parse.ToRect(infile.Val);
                    // @ATTR font_who|predefined_string|Font style to use for the name of the currently talking person.
                    else if (infile.Key == "font_who") _fontWho = infile.Val;
                    // @ATTR font_dialog|predefined_string|Font style to use for the dialog text.
                    else if (infile.Key == "font_dialog") _fontDialog = infile.Val;

                    // @ATTR topic_color_normal|color|The normal color for topic text.
                    else if (infile.Key == "topic_color_normal") _topicColorNormal = Parse.ToRGB(infile.Val);
                    // @ATTR topic_color_hover|color|The color for topic text when highlighted.
                    else if (infile.Key == "topic_color_hover") _topicColorHover = Parse.ToRGB(infile.Val);
                    // @ATTR topic_color_pressed|color|The color for topic text when clicked.
                    else if (infile.Key == "topic_color_pressed") _topicColorPressed = Parse.ToRGB(infile.Val);

                    // @ATTR trade_color_normal|color|The normal color for the "Trade" text.
                    else if (infile.Key == "trade_color_normal") _tradeColorNormal = Parse.ToRGB(infile.Val);
                    // @ATTR trade_color_hover|color|The color for the "Trade" text when highlighted.
                    else if (infile.Key == "trade_color_hover") _tradeColorHover = Parse.ToRGB(infile.Val);
                    // @ATTR trade_color_normal|color|The color for the "Trade" text when clicked.
                    else if (infile.Key == "trade_color_pressed") _tradeColorPressed = Parse.ToRGB(infile.Val);

                    else infile.Error("MenuTalker: '%s' is not a valid key.", infile.Key);
                }
                infile.Close();
            }

            _labelName = new WidgetLabel();
            _labelName.SetBasePos(_textPos.X + _textOffset.X, _textPos.Y + _textOffset.Y, Utils.AlignTopLeft);
            _labelName.SetColor(SharedResources.Font!.GetColor(FontEngine.ColorMenuNormal));

            _textbox = new WidgetScrollBox(_textPos.Width, _textPos.Height - (_textOffset.Y * 2));
            _textbox.SetBasePos(_textPos.X, _textPos.Y + _textOffset.Y, Utils.AlignTopLeft);
            _textbox.ShowFocusWhenScrollbarDisabled = false;

            if (_background == null)
                SetBackground("images/menus/dialog_box.png");

            Align();
        }

        /// <summary>
        /// 对应 C++ 的 <c>~MenuTalker()</c>：先释放动作按钮，再依次释放 portrait、label_name、
        /// textbox、advanceButton、closeButton，最后调用基类 <see cref="Menu.Dispose"/>。
        /// </summary>
        public override void Dispose()
        {
            ClearActionButtons();

            _portrait?.Dispose();
            _labelName?.Dispose();
            _textbox?.Dispose();
            AdvanceButton?.Dispose();
            CloseButton?.Dispose();

            base.Dispose();
        }

        public override void Align()
        {
            base.Align();

            AdvanceButton!.SetPos(WindowArea.X, WindowArea.Y);
            CloseButton!.SetPos(WindowArea.X, WindowArea.Y);

            _labelName!.SetPos(WindowArea.X, WindowArea.Y);

            _textbox!.SetPos(WindowArea.X, WindowArea.Y + _labelName.GetBounds().Height);
            _textbox.Pos.Height = _textPos.Height - (_textOffset.Y * 2);
        }

        public void ChooseDialogNode(int requestedNode)
        {
            _eventCursor = 0;

            if (requestedNode == -1)
            {
                // display the topic list (or automatically select a topic if there's only one)
                _dialogNode = -1;
                CreateActionBuffer();

                // need to set the portrait here since we don't call processDialog()
                if (Npc!.Portraits.Count != 0)
                    Npc.NpcPortrait = Npc.Portraits[0];

                if (_actions.Count == 1 && _firstInteraction)
                {
                    ExecuteAction(0);
                }
                else if (_actions.Count == 0)
                {
                    SetNPC(null); // end dialog
                }
            }
            else
            {
                _dialogNode = requestedNode;
                Npc!.ProcessEvent(_dialogNode, _eventCursor);
                if (Npc.ProcessDialog(_dialogNode, ref _eventCursor))
                    CreateBuffer();
                else
                    SetNPC(null); // end dialog
            }

            _firstInteraction = false;
        }

        /// <summary>
        /// Menu interaction (enter/space/click to continue)
        /// </summary>
        public void Logic()
        {
            if (!Visible || Npc == null)
                return;

            if (!SharedResources.Inpt!.UsingMouse() && Tablist.GetCurrent() == -1)
            {
                Tablist.SetCurrent(_textbox);
            }
            Tablist.EnableActivate = _actions.Count != 0;

            Tablist.Logic();

            if (AdvanceButton!.CheckClick() || CloseButton!.CheckClick())
            {
                // button was clicked
                if (CloseButton.Enabled)
                {
                    NextDialog();
                    SetNPC(null);
                }
                else
                {
                    NextDialog();
                }
            }
            else if ((AdvanceButton.Enabled || CloseButton.Enabled) && SharedResources.Inpt.Pressing[Input.Accept] && !SharedResources.Inpt.Lock[Input.Accept])
            {
                // pressed next/more
                SharedResources.Inpt.Lock[Input.Accept] = true;
                if (CloseButton.Enabled)
                {
                    NextDialog();
                    SetNPC(null);
                }
                else
                {
                    NextDialog();
                }
            }
            else
            {
                _textbox!.Logic();

                Int2 mouse = _textbox.InputAssist(SharedResources.Inpt.Mouse);
                for (int i = 0; i < _actions.Count; ++i)
                {
                    if (_actions[i].Btn!.CheckClickAt(mouse.X, mouse.Y))
                    {
                        ExecuteAction(i);
                        break;
                    }
                }

                Rectangle lockArea = _dialogPos;
                lockArea.X += WindowArea.X;
                lockArea.Y += WindowArea.Y;
                if (SharedResources.Inpt.Pressing[Input.Main1] && !SharedResources.Inpt.Lock[Input.Main1] && Utils.IsWithinRect(lockArea, SharedResources.Inpt.Mouse))
                {
                    SharedResources.Inpt.Lock[Input.Main1] = true;
                }
            }
        }

        public void CreateActionBuffer()
        {
            CreateActionButtons(-1);

            int buttonHeight = 0;
            for (int i = 0; i < _actions.Count; ++i)
            {
                buttonHeight += _actions[i].Btn!.Pos.Height;
            }

            int buttonY = 0;
            for (int i = 0; i < _actions.Count; ++i)
            {
                _actions[i].Btn!.Pos.X = _textOffset.X;
                _actions[i].Btn!.Pos.Y = buttonY;
                _actions[i].Btn!.Refresh();

                buttonY += _actions[i].Btn!.Pos.Height;
            }

            _labelName!.SetText(Npc!.Name);
            _labelName.SetFont(_fontWho);
            _textbox!.Resize(_textbox.Pos.Width, buttonHeight);

            Align();

            CloseButton!.Enabled = true;
            AdvanceButton!.Enabled = false;

            SetupTabList();
        }

        public void CreateBuffer()
        {
            ClearActionButtons();

            if ((uint)_dialogNode >= Npc!.Dialog.Count || _eventCursor >= Npc.Dialog[_dialogNode].Count)
                return;

            CreateActionButtons(_dialogNode);

            int buttonHeight = 0;
            for (int i = 0; i < _actions.Count; ++i)
            {
                buttonHeight += _actions[i].Btn!.Pos.Height;
            }

            string line;

            // speaker name
            int etype = Npc.Dialog[_dialogNode][(int)_eventCursor].Type;
            string who = "";

            if (etype == EventComponent.NpcDialogThem)
            {
                who = Npc.Name;
            }
            else if (etype == EventComponent.NpcDialogYou)
            {
                who = _heroName;
            }

            _labelName!.SetText(who);
            _labelName.SetFont(_fontWho);


            line = Utils.SubstituteVarsInString(Npc.Dialog[_dialogNode][(int)_eventCursor].S, SharedGameResources.Pc);

            // render dialog text to the scrollbox buffer
            Int2 lineSize = SharedResources.Font!.CalcSizeWrapped(line, _textbox!.Pos.Width - (_textOffset.X * 2));
            _textbox.Resize(_textbox.Pos.Width, lineSize.Y + buttonHeight);
            SharedResources.Font.SetFont(_fontDialog);
            SharedResources.Font.Render(
                line,
                _textOffset.X,
                0,
                FontEngine.JustifyLeft,
                _textbox.Contents!.GetGraphics()!,
                _textPos.Width - _textOffset.X * 2,
                SharedResources.Font.GetColor(FontEngine.ColorMenuNormal),
                !FontEngine.ShadowOffset
            );

            int buttonY = 0;
            for (int i = 0; i < _actions.Count; ++i)
            {
                _actions[i].Btn!.Pos.X = _textOffset.X;
                _actions[i].Btn!.Pos.Y = lineSize.Y + buttonY;
                _actions[i].Btn!.Refresh();

                buttonY += _actions[i].Btn!.Pos.Height;
            }

            Align();

            if (_actions.Count != 0)
            {
                AdvanceButton!.Enabled = false;
                CloseButton!.Enabled = false;
            }
            else if (Npc.Dialog[_dialogNode].Count != 0 && _eventCursor < Npc.Dialog[_dialogNode].Count - 1 && Npc.Dialog[_dialogNode][(int)_eventCursor + 1].Type != EventComponent.None)
            {
                AdvanceButton!.Enabled = true;
                CloseButton!.Enabled = false;
            }
            else
            {
                AdvanceButton!.Enabled = false;
                CloseButton!.Enabled = true;
            }

            SetupTabList();
        }

        public override void Render()
        {
            if (!Visible) return;
            Rectangle src = default;
            Rectangle dest = default;

            int offsetX = WindowArea.X;
            int offsetY = WindowArea.Y;

            // dialog box
            src.X = 0;
            src.Y = 0;
            dest.X = offsetX + _dialogPos.X;
            dest.Y = offsetY + _dialogPos.Y;
            src.Width = dest.Width = _dialogPos.Width;
            src.Height = dest.Height = _dialogPos.Height;

            SetBackgroundClip(src);
            SetBackgroundDest(dest);
            base.Render();

            if ((uint)_dialogNode < Npc!.Dialog.Count && _eventCursor < Npc.Dialog[_dialogNode].Count)
            {
                // show active portrait
                int etype = Npc.Dialog[_dialogNode][(int)_eventCursor].Type;
                if (etype == EventComponent.NpcDialogThem)
                {
                    if (Npc.NpcPortrait != null)
                    {
                        src.Width = dest.Width = _portraitHe.Width;
                        src.Height = dest.Height = _portraitHe.Height;
                        dest.X = offsetX + _portraitHe.X;
                        dest.Y = offsetY + _portraitHe.Y;

                        Npc.NpcPortrait.SetClipFromRect(src);
                        Npc.NpcPortrait.SetDestFromRect(dest);
                        SharedResources.RenderDevice!.Render(Npc.NpcPortrait);
                    }
                }
                else if (etype == EventComponent.NpcDialogYou)
                {
                    if (Npc.HeroPortrait != null)
                    {
                        src.Width = dest.Width = _portraitYou.Width;
                        src.Height = dest.Height = _portraitYou.Height;
                        dest.X = offsetX + _portraitYou.X;
                        dest.Y = offsetY + _portraitYou.Y;
                        Npc.HeroPortrait.SetClipFromRect(src);
                        Npc.HeroPortrait.SetDestFromRect(dest);
                        SharedResources.RenderDevice!.Render(Npc.HeroPortrait);
                    }
                    else if (_portrait != null)
                    {
                        src.Width = dest.Width = _portraitYou.Width;
                        src.Height = dest.Height = _portraitYou.Height;
                        dest.X = offsetX + _portraitYou.X;
                        dest.Y = offsetY + _portraitYou.Y;
                        _portrait.SetClipFromRect(src);
                        _portrait.SetDestFromRect(dest);
                        SharedResources.RenderDevice!.Render(_portrait);
                    }
                }
            }
            else if (_dialogNode == -1 && Npc.NpcPortrait != null)
            {
                src.Width = dest.Width = _portraitHe.Width;
                src.Height = dest.Height = _portraitHe.Height;
                dest.X = offsetX + _portraitHe.X;
                dest.Y = offsetY + _portraitHe.Y;

                Npc.NpcPortrait.SetClipFromRect(src);
                Npc.NpcPortrait.SetDestFromRect(dest);
                SharedResources.RenderDevice!.Render(Npc.NpcPortrait);
            }

            // name & dialog text
            _labelName!.Render();
            _textbox!.Render();

            // show advance button if there are more event components, or close button if not
            if (AdvanceButton!.Enabled)
                AdvanceButton.Render();
            else if (CloseButton!.Enabled)
                CloseButton.Render();
        }

        public void SetHero(StatBlock stats)
        {
            _heroName = stats.Name;
            _heroClass = stats.GetShortClass();

            _portrait?.Dispose();

            if (stats.GfxPortrait == "") return;

            Image? graphics;
            graphics = SharedResources.RenderDevice!.LoadImage(stats.GfxPortrait, RenderDevice.ErrorNormal);
            if (graphics != null)
            {
                _portrait = graphics.CreateSprite();
                graphics.Unref();
            }
        }

        public void SetNPC(NPC? npc)
        {
            if (Npc != npc)
            {
                _firstInteraction = true;
            }

            Npc = npc;

            if (npc == null)
            {
                Visible = false;
                _firstInteraction = false;
                Tablist.Defocus();
                return;
            }

            Visible = true;
        }

        private void CreateActionButtons(int nodeId)
        {
            if (Npc == null)
                return;

            ClearActionButtons();

            List<int> nodes = new List<int>();
            if (nodeId == -1)
            {
                // primary topic selection
                Npc.GetDialogNodes(nodes, !NPC.GetResponseNodes);
            }
            else
            {
                // dialog responses
                Npc.GetDialogResponses(nodes, nodeId, _eventCursor);
            }

            // add standard topics
            for (int i = nodes.Count; i > 0; i--)
            {
                string topic = Npc.GetDialogTopic(nodes[i - 1]);
                if (topic == "")
                {
                    topic = SharedResources.Msg!.GetV("<dialog node %d>", nodes[i - 1]);
                }

                AddAction(topic, nodes[i - 1], !Action.VendorTrade);
            }

            // add "Trade" topic
            if (nodeId == -1 && Npc.CheckVendor())
            {
                AddAction(SharedResources.Msg!.Get("Trade"), Action.NoNode, Action.VendorTrade);
            }

            for (int i = 0; i < _actions.Count; ++i)
            {
                _actions[i].Btn!.TablistNavAlign = TabList.NavAlignLeft;
                _textbox!.AddChildWidget(_actions[i].Btn);
            }
        }

        private void ClearActionButtons()
        {
            for (int i = 0; i < _actions.Count; ++i)
            {
                _actions[i].Btn!.Dispose();
            }
            _actions.Clear();
            _textbox!.ClearChildWidgets();
        }

        private void ExecuteAction(int index)
        {
            if (index >= _actions.Count)
                return;

            int nodeId = _actions[index].NodeId;

            if (_actions[index].IsVendor)
            {
                // defocus the talker menu tablist. Otherwise, CANCEL needs to be pressed twice to exit the vendor screen
                DefocusTabLists();

                // begin trading
                NPC? tempNpc = Npc;
                SharedGameResources.Menu!.CloseAll();
                SharedGameResources.Menu.Vendor!.SetNPC(tempNpc);
                SharedGameResources.Menu.Inv!.Visible = true;
            }
            else if (nodeId != -1)
            {
                // begin talking
                ChooseDialogNode(nodeId);
                if (Npc != null && NpcFromMap)
                {
                    SharedGameResources.Pc!.AllowMovement = Npc.CheckMovement(nodeId);
                }
            }
        }

        private void NextDialog()
        {
            bool more = false;

            if (_dialogNode != -1)
            {
                Npc!.ProcessEvent(_dialogNode, _eventCursor);
                _eventCursor++;
                more = Npc.ProcessDialog(_dialogNode, ref _eventCursor);
            }
            else
            {
                more = false;
            }

            if (more)
                CreateBuffer();
            else
            {
                if (_dialogNode != -1)
                {
                    // return to the topic selection
                    int prevNode = _dialogNode;
                    ChooseDialogNode(-1);

                    // when returning to the topic selection, a topic is auto-selected if there is only one
                    // in this case, we don't want to repeat the same topic, so we check for that here
                    if (_actions.Count == 0 && _dialogNode == prevNode)
                        SetNPC(null);
                }
                else
                    SetNPC(null); // end dialog
            }
        }

        private void SetupTabList()
        {
            Tablist.Clear();

            Tablist.Add(_textbox);
        }

        private void AddAction(string label, int nodeId, bool isVendor)
        {
            if ((nodeId != Action.NoNode && isVendor) || (nodeId == Action.NoNode && !isVendor))
            {
                Utils.LogError("MenuTalker: addAction() parameters are incompatible, skipping action.");
                return;
            }

            _actions.Add(new Action());

            _actions[^1].Btn = new WidgetButton(WidgetButton.NoFile);
            _actions[^1].Btn.SetLabel(label);
            _actions[^1].Btn.SetTextFont(_fontDialog);

            if (nodeId != Action.NoNode)
            {
                _actions[^1].NodeId = nodeId;
                _actions[^1].Btn.SetTextColor(WidgetButton.ButtonNormal, _topicColorNormal);
                _actions[^1].Btn.SetTextColor(WidgetButton.ButtonHover, _topicColorHover);
                _actions[^1].Btn.SetTextColor(WidgetButton.ButtonPressed, _topicColorPressed);
            }
            else
            {
                _actions[^1].IsVendor = true;
                _actions[^1].Btn.SetTextColor(WidgetButton.ButtonNormal, _tradeColorNormal);
                _actions[^1].Btn.SetTextColor(WidgetButton.ButtonHover, _tradeColorHover);
                _actions[^1].Btn.SetTextColor(WidgetButton.ButtonPressed, _tradeColorPressed);
            }
        }
    }
}
