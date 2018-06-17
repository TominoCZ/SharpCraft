using OpenTK;
using SharpCraft.block;
using SharpCraft.item;
using SharpCraft.texture;
using System;

namespace SharpCraft.gui
{
    internal class GuiHUD : Gui
    {
        private float _hotbarX;
        private float _hotbarY;

        public override void Render(int mouseX, int mouseY)
        {
            Size size = SharpCraft.Instance.ClientSize;

            int space = 5;

            int scaledWidth = 32 * 2;
            int scaledHeight = 32 * 2;

            int totalHotbarWidth = 9 * scaledWidth + 8 * space;

            _hotbarX = size.Width / 2 - totalHotbarWidth / 2;
            _hotbarY = size.Height - 20 - scaledHeight;

            // Render lives first so text overlays
            DrawLives();

            ItemStack selectedStack = SharpCraft.Instance.Player.GetEquippedItemStack();
            if (!selectedStack?.IsEmpty == true)
                RenderText(selectedStack.ToString(), size.Width / 2f, _hotbarY - 14, 1, true, true);

            for (int i = 0; i < 9; i++)
            {
                bool b = i == SharpCraft.Instance.Player.HotbarIndex;

                float x = _hotbarX + i * (scaledWidth + space);
                float y = _hotbarY;

                int u = 224;
                int v = 0;

                if (b)
                    v += 32;

                RenderTexture(TextureManager.TEXTURE_GUI_WIDGETS, x, y, u, v, 32, 32, 2);

                ItemStack stack = SharpCraft.Instance.Player.Hotbar[i];

                if (stack == null || stack.IsEmpty)
                    continue;

                if (stack.Item is ItemBlock itemBlock)
                {
                    EnumBlock block = itemBlock.GetBlock();

                    x += 14;
                    y += 14;

                    RenderBlock(block, x, y, 2.25f);
                }

                if (stack.Count > 1)
                    RenderText(stack.Count.ToString(), x + scaledWidth / 2f - 14, _hotbarY + scaledHeight / 2f + 14, 1, true, true);
            }

            RenderText(SharpCraft.Instance.GetFPS() + " FPS", 5, 6, 1, Vector3.UnitY, false, true);

            // debug info
            // RenderText("Falling: " + SharpCraft.Instance.Player.isFalling + "(" + SharpCraft.Instance.Player.fallDistance + ")", 5, 47, 1, Vector3.UnitY, false, true);
        }

        private void DrawLives()
        {
            float health = SharpCraft.Instance.Player.Health;

            int fullHearts = (int)(health / 10);

            float remainder = health % 10.0f / 10.0f;
            double halves = Math.Round(remainder);

            int gap = 3;

            // Lives
            float y = _hotbarY - 20;

            for (int i = 0; i < 10; i++)
            {
                int u = 0;

                // Full hearts
                if (i + 1 <= fullHearts)
                    u = 32;
                // Half hearts
                else if (i + 1 <= fullHearts + halves)
                    u = 16;

                RenderTexture(TextureManager.TEXTURE_GUI_WIDGETS, _hotbarX + i * (16 + gap), y, u, 40, 16, 16);
            }
        }
    }
}