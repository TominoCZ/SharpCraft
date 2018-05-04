using System;
using OpenTK;
using SharpCraft.item;
using SharpCraft.texture;

namespace SharpCraft.gui
{
    class GuiHUD : Gui
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

                var stack = SharpCraft.Instance.Player.hotbar[i];

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

            RenderText(Math.Round(1000 / SharpCraft.Instance.LastFrameRenderTime) + " FPS", 5, 6, 1, Vector3.UnitY, false, true);
        }
    }
}