using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Input;
using SharpCraft.item;
#pragma warning disable 618

namespace SharpCraft.gui
{
    internal class GuiChat : GuiScreen
    {
        private readonly bool _isCaps;

        private static readonly List<string> HistoryQueue = new List<string>();

        private int _currentHistoryIdx;
        private string _currentInputText = "";

        public GuiChat()
        {
            DoesGuiPauseGame = true;

            _isCaps = Keyboard.GetState().IsKeyDown(Key.CapsLock);
            _currentHistoryIdx = HistoryQueue.Count - 1;

            SharpCraft.Instance.KeyDown += OnKeyDown;
        }

        private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.BackSpace:
                    if (_currentInputText.Length > 0)
                        _currentInputText = _currentInputText.Substring(0, _currentInputText.Length - 1);
                    break;
                case Key.Up:
                    ShowHistoryUp();
                    break;
                case Key.Down:
                    ShowHistoryDown();
                    break;
            }
        }

        // TODO: History for unique commands
        public void ShowHistoryUp()
        {
            if (HistoryQueue.Count <= 0)
                return;

            if (_currentHistoryIdx - 1 > 0)
                _currentHistoryIdx--;

            _currentInputText = HistoryQueue[_currentHistoryIdx];
        }

        public void ShowHistoryDown()
        {
            if (HistoryQueue.Count <= 0)
                return;

            if (_currentHistoryIdx + 1 <= HistoryQueue.Count - 1)
                _currentHistoryIdx++;

            _currentInputText = HistoryQueue[_currentHistoryIdx];
        }

        public void SendMessage()
        {
            string msg = _currentInputText.Trim();

            if (msg.Length > 0)
            {
                bool isCommand = msg[0] == '/';

                if (!HistoryQueue.Contains(msg))
                    HistoryQueue.Add(msg);

                if (!isCommand)
                    return;

                try
                {
                    msg = msg.Substring(1, msg.Length - 1);

                    string[] split = msg.Split(' ');

                    var cmd = split[0];

                    switch (cmd)
                    {
                        case "tp":
                            Vector3 position = Vector3.Zero;
                            Vector3 playerPos = SharpCraft.Instance.Player.Pos;

                            for (int i = 0; i < 3; i++)
                            {
                                string arg = split[1 + i];

                                float value;

                                if (arg[0] == '~')
                                {
                                    value = int.Parse("0" + arg.Substring(1, arg.Length - 1)) + playerPos[i];
                                }
                                else
                                {
                                    value = int.Parse(arg);
                                }

                                position[i] = value;
                            }

                            SharpCraft.Instance.Player.TeleportTo(position);
                            break;
                        case "give":
                            string itemName = split[1];
                            int ammount = 1;

                            if (split.Length > 2)
                                ammount = int.Parse(split[2]);

                            if (ItemRegistry.GetItem(itemName) is Item item)
                                SharpCraft.Instance.Player.OnPickup(new ItemStack(item, ammount));

                            break;
                    }
                }
                catch
                {

                }
            }
        }

        public void InputText(char c, bool shift)
        {
            string character = _isCaps || shift ? c.ToString().ToUpper() : c.ToString().ToLower();

            _currentInputText += character;
        }
        /*
        private void FinishTyping()
        {
            if (_currentInputText.Message.Length <= 0)
                return;

            if (_historyQueue.Count >= 6)
                _historyQueue.Dequeue();

            // COMMAND
            bool incorrectComand = true;
            if (CurrentInputText.Message[0] == '/')
            {
                string[] blocks = CurrentInputText.Message.Remove(0, 1).Split(' ');

                switch (blocks[0])
                {
                    case "kill":

                        break;

                    case "duplicate":

                        // duplicate current stack in hand

                        break;

                    case "tp":

                        if (blocks.Length < 4)
                            break;

                        float xPos, yPos, zPos;
                        if (float.TryParse(blocks[1].ToLower(), out xPos) == false)
                            break;

                        if (float.TryParse(blocks[2].ToLower(), out yPos) == false)
                            break;

                        if (float.TryParse(blocks[3].ToLower(), out zPos) == false)
                            break;

                        SharpCraft.Instance.CommandTeleport(xPos, yPos, zPos);
                        incorrectComand = false;

                        break;

                    case "give":

                        if (blocks.Length < 3)
                            break;

                        int amount = 0;
                        if (int.TryParse(blocks[1].ToLower(), out amount) == false)
                            break;

                        SharpCraft.Instance.CommandGive(blocks[2].ToLower(), amount);
                        incorrectComand = false;

                        break;
                }


            }
            else
            {
                incorrectComand = false;
            }

            _historyQueue.Enqueue(CurrentInputText);

            base.Init();


            if (incorrectComand)
            {
                if (_historyQueue.Count >= 6)
                    _historyQueue.Dequeue();

                MediaTypeNames.Text incorrectText = new MediaTypeNames.Text
                {
                    Message = "INCORRECT COMMAND",
                    Colour = new Vector3(255, 0, 0)
                };
                _historyQueue.Enqueue(incorrectText);
            }
        }
        */

        public override void Render(int mouseX, int mouseY)
        {
            const int xCount = 3;
            const int yCount = 2;
            float yPos = SharpCraft.Instance.ClientSize.Height - (Background.TextureSize.Height * yCount) - 60;

            DrawBackground(xCount, yCount, 0, yPos);

            RenderText(">" + _currentInputText + "_", 5, yPos - 15, 1);

            var messages = HistoryQueue.Where(msg => msg.First() != '/').ToArray();

            for (int i = messages.Length - 1, j = 0; i >= 0; i--, j++)
            {
                RenderText(messages[i], 5, yPos - 16 - 18 * (j + 1), 1);
            }

            base.Render(mouseX, mouseY);
        }

        public override void OnClose()
        {
            base.OnClose();

            SharpCraft.Instance.KeyDown -= OnKeyDown;
        }
    }
}
