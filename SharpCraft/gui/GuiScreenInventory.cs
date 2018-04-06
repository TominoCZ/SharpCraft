using SharpCraft.item;
using SharpCraft.texture;

namespace SharpCraft.gui
{
    internal class GuiScreenInventory : GuiScreen
    {
        public GuiScreenInventory()
        {
            DoesGuiPauseGame = false;
        }

        public override void Render(int mouseX, int mouseY)
        {
            DrawBackground();

            var size = SharpCraft.Instance.ClientSize;

            int space = 5;

            int scaledWidth = 32 * 2;
            int scaledHeight = 32 * 2;

            int totalInventoryWidth = 9 * scaledWidth + 8 * space;
            int totalInventoryHeight = 3 * scaledHeight + 2 * space;

            var startPosX = size.Width / 2f - totalInventoryWidth / 2f;
            var startPosY = size.Height / 2f - totalInventoryHeight / 2f;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    var x = startPosX + j * (scaledWidth + space);
                    var y = startPosY + i * (scaledHeight + space);

                    RenderTexture(TextureManager.TEXTURE_GUI_WIDGETS, x, y, 224, 0, 32, 32, 2);

                    var stack = SharpCraft.Instance.Player.inventory[i * 9 + j];

                    if (!stack?.IsEmpty == true)
                    {
                        if (stack.Item is ItemBlock itemBlock)
                        {
                            var block = itemBlock.getBlock();

                            x += 14;
                            y += 14;

                            RenderBlock(block, x, y, 2.25f);
                        }
                    }
                }
            }
        }
    }
}