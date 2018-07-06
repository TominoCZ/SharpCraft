using OpenTK;
using OpenTK.Input;
using SharpCraft.texture;
using SharpCraft.util;

namespace SharpCraft.gui
{
    internal class GuiChat : GuiScreen
    {
        
        private readonly bool _allowTyping;
        private bool _isCaps = true;

        private readonly System.Collections.Generic.Queue<Text> _historyQueue = new System.Collections.Generic.Queue<Text>();

        private int _currentHistoryIdx;


        public GuiChat()
        {
            _allowTyping = true;

            _isCaps = Keyboard.GetState().IsKeyDown(Key.CapsLock);
           // isCaps = false;
        }

        public override void Init()
        {
            _currentHistoryIdx = _historyQueue.Count;

            base.Init();
        }

        // TODO: History for unique commands
        public void ShowHistoryUp()
        {
            if (_historyQueue.Count <= 0)
                return;

            if(_currentHistoryIdx - 1 >= 0)
                _currentHistoryIdx--;
            
            currentInputText = _historyQueue.ToArray()[_currentHistoryIdx];
        }
        public void ShowHistoryDown()
        {
            if (_historyQueue.Count <= 0)
                return;

            if (_currentHistoryIdx + 1 <= _historyQueue.Count - 1)
                _currentHistoryIdx++;

            currentInputText = _historyQueue.ToArray()[_currentHistoryIdx];
        }

        public override void InputText(Key key)
        {
            if (!_allowTyping)
                return;

            string character = "";
            string keyString = key.ToString();
            if (_isCaps == false)
                keyString = keyString.ToLower();

            if (((int)key >= 83 && (int)key <= 108))
            {
                character += keyString;
            }
            // numbers
            else if((int)key >= 109 && (int)key <= 118)
            {
                character = keyString.Remove(0, 6);
            }
            else
            {
                switch (key)
                {
                    case Key.Enter:
                        FinishTyping();
                        return;

                    case Key.BackSpace:
                        if (currentInputText.text.Length > 0)
                            currentInputText.text = currentInputText.text.Remove(currentInputText.text.Length - 1, 1);

                        break;

                    case Key.CapsLock:
                        _isCaps = !_isCaps;
                        break;

                    case Key.Space:
                        character = " ";
                        break;

                    case Key.Slash:
                        character = "/";
                        break;

                }
            }
           
            currentInputText.text += character;

            base.InputText(key);
        }

        private void FinishTyping()
        {
            if (currentInputText.text.Length <= 0)
                return;

            if(_historyQueue.Count >= 6)
            _historyQueue.Dequeue();

            // COMMAND
            bool incorrectComand = true;
            if (currentInputText.text[0] == '/')
            {
                string[] blocks = currentInputText.text.Remove(0, 1).Split(' ');
          
                switch(blocks[0])
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

            _historyQueue.Enqueue(currentInputText);

            base.Init();


            if (incorrectComand)
            {
                if (_historyQueue.Count >= 6)
                    _historyQueue.Dequeue();

                Text incorrectText = new Text
                {
                    text = "INCORRECT COMMAND",
                    colour = new Vector3(255, 0, 0)
                };
                _historyQueue.Enqueue(incorrectText);
            }
        }

        public override void Render(int mouseX, int mouseY)
        {
            if (visible == false)
                return;
    
            const int xCount = 3;
            const int yCount = 2;
            float yPos = SharpCraft.Instance.ClientSize.Height - (background.TextureSize.Height * yCount) - 60;

            DrawBackground(xCount, yCount, 0, yPos);

            RenderText(">" + currentInputText.text, 5, yPos  - 15, 1, currentInputText.colour);

            for(int i = _historyQueue.Count - 1, j = 0; i >= 0; i--,j++)
            {
                RenderText(_historyQueue.ToArray()[i].text.ToString(), 5, (yPos - 16) - (18 * (j + 1)), 1,
                    _historyQueue.ToArray()[i].colour);
            }

            base.Render(mouseX, mouseY);
        }

    }
}
