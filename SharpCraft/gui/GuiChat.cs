using OpenTK;
using OpenTK.Input;
using SharpCraft.texture;
using SharpCraft.util;

namespace SharpCraft.gui
{
    internal class GuiChat : GuiScreen
    {
        
        private bool allowTyping = false;
        private bool isCaps = true;

        private System.Collections.Generic.Queue<Text> historyQueue = new System.Collections.Generic.Queue<Text>();

        private int currentHistoryIdx = 0;


        public GuiChat()
        {
            allowTyping = true;

            isCaps = Keyboard.GetState().IsKeyDown(Key.CapsLock);
           // isCaps = false;
        }

        public override void Init()
        {
            currentHistoryIdx = historyQueue.Count;

            base.Init();
        }

        // TODO: History for unique commands
        public void ShowHistoryUP()
        {
            if (historyQueue.Count <= 0)
                return;

            if(currentHistoryIdx - 1 >= 0)
                currentHistoryIdx--;
            
            currentInputText = historyQueue.ToArray()[currentHistoryIdx];
        }
        public void ShowHistoryDOWN()
        {
            if (historyQueue.Count <= 0)
                return;

            if (currentHistoryIdx + 1 <= historyQueue.Count - 1)
                currentHistoryIdx++;

            currentInputText = historyQueue.ToArray()[currentHistoryIdx];
        }

        public override void InputText(Key key)
        {
            if (!allowTyping)
                return;

            string character = "";
            string keyString = key.ToString();
            if (isCaps == false)
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
                        isCaps = !isCaps;
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

            if(historyQueue.Count >= 6)
            historyQueue.Dequeue();

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

            historyQueue.Enqueue(currentInputText);

            base.Init();


            if (incorrectComand)
            {
                if (historyQueue.Count >= 6)
                    historyQueue.Dequeue();

                Text incorrectText = new Text();
                incorrectText.text = "INCORRECT COMMAND";
                incorrectText.colour = new Vector3(255, 0, 0);
                historyQueue.Enqueue(incorrectText);
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

            for(int i = historyQueue.Count - 1, j = 0; i >= 0; i--,j++)
            {
                RenderText(historyQueue.ToArray()[i].text.ToString(), 5, (yPos - 16) - (18 * (j + 1)), 1,
                    historyQueue.ToArray()[i].colour);
            }

            base.Render(mouseX, mouseY);
        }

    }
}
