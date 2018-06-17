using OpenTK;
using SharpCraft.item;
using SharpCraft.texture;

namespace SharpCraft.gui
{
    internal class GuiHUD : Gui
    {
        public override void Render(int mouseX, int mouseY)
        {
           

            var size = SharpCraft.Instance.ClientSize;

            int space = 5;

            int scaledWidth = 32 * 2;
            int scaledHeight = 32 * 2;

            int totalHotbarWidth = 9 * scaledWidth + 8 * space;

            int startPos = size.Width / 2 - totalHotbarWidth / 2;
            var hotbarY = size.Height - 20 - scaledHeight;

            // Render lives first so text overlays
            DrawLives();

            var selectedStack = SharpCraft.Instance.Player.GetEquippedItemStack();
            if (!selectedStack?.IsEmpty == true)
                RenderText(selectedStack.ToString(), size.Width / 2f, hotbarY - 14, 1, true, true);

            for (int i = 0; i < 9; i++)
            {
                var b = i == SharpCraft.Instance.Player.HotbarIndex;

                var x = startPos + i * (scaledWidth + space);
                var y = hotbarY;

                var u = 224;
                var v = 0;

                if (b)
                    v += 32;

                RenderTexture(TextureManager.TEXTURE_GUI_WIDGETS, x, hotbarY, u, v, 32, 32, 2);

                var stack = SharpCraft.Instance.Player.Hotbar[i];

                if (stack == null || stack.IsEmpty)
                    continue;

                if (stack.Item is ItemBlock itemBlock)
                {
                    var block = itemBlock.GetBlock();

                    x += 14;
                    y += 14;

                    RenderBlock(block, x, y, 2.25f);
                }

                if (stack.Count > 1)
                    RenderText(stack.Count.ToString(), x + scaledWidth / 2f - 14, hotbarY + scaledHeight / 2f + 14, 1, true, true);
            }

            RenderText(SharpCraft.Instance.GetFPS() + " FPS", 5, 6, 1, Vector3.UnitY, false, true);

            // debug
            RenderText("Falling: " + SharpCraft.Instance.Player.isFalling + "(" + SharpCraft.Instance.Player.fallDistance + ")", 5, 47, 1, Vector3.UnitY, false, true);
        }

        private void DrawLives()
        {


            Size size = SharpCraft.Instance.ClientSize;

            float currentHealthPercentage = SharpCraft.Instance.Player.healthPercentage;
            int numberOfFullHearts = (int)(currentHealthPercentage / 10);

            float remainder = (currentHealthPercentage % 10.0f) / 10.0f;
            int numberOfHalfHearts = (remainder >= 0.5f) ? 1 : 0; 
            int numberOfEmptyHearts = 10 - numberOfFullHearts; 

            int space = 5;
            const int betweenElementSpace = 1;

            const float elementSizeScale = 0.75f;
            const float elementSizeXY = 32 * elementSizeScale;
            int scaledWidth = 32 * 2;
            int scaledHeight = 32 * 2;

            int totalHotbarWidth = 9 * scaledWidth + 8 * space;
            int startPos = size.Width / 2 - totalHotbarWidth / 2;

            // debug info
            RenderText(currentHealthPercentage + " %", 5, 26, 1, Vector3.UnitY, false, true);

            // Lives
            int livesY = size.Height - 55 - scaledHeight;

            for (int i = 0; i < 10; i++)
            {
                var x = startPos + i * (elementSizeXY + betweenElementSpace);
                var y = livesY;

                var v = 40;

                if (i + 1 <= numberOfFullHearts)
                {
                    var u = 64;

                    RenderTexture(TextureManager.TEXTURE_GUI_WIDGETS, x, livesY, u, v, 32, 32, elementSizeScale);
                }
                else if(i + 1 <= numberOfFullHearts + numberOfHalfHearts)
                {
                    var u = 32;

                    RenderTexture(TextureManager.TEXTURE_GUI_WIDGETS, x, livesY, u, v, 32, 32, elementSizeScale);
                }
                else if(i + 1 <= numberOfFullHearts + numberOfHalfHearts + numberOfEmptyHearts)
                {
                    var u = 0;

                    RenderTexture(TextureManager.TEXTURE_GUI_WIDGETS, x, livesY, u, v, 32, 32, elementSizeScale);
                }
            }
            // end
        }
    }
}