﻿using OpenTK;
using SharpCraft.item;
using SharpCraft.texture;
using System;
using System.Drawing;

namespace SharpCraft.gui
{
    internal class GuiHud : Gui
    {
        private float _hotbarX;
        private float _hotbarY;

        private const int HeartsGap = 19;
        private static int _fullHearts = 10;
        private static double _halfHearts = 0.0;
        private static double _totalOccupiedHearts = 0.0;

        public static void UpdateLivesUi(float health)
        {
            _fullHearts = (int)(health / 10);

            float remainder = health % 10.0f / 10.0f;
            _halfHearts = Math.Round(remainder);

            _totalOccupiedHearts = _fullHearts + _halfHearts;
        }

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

                x += 14;
                y += 14;

                RenderStack(stack, x, y, 2.25f);

                if (stack.Count > 1)
                    RenderText(stack.Count.ToString(), x + scaledWidth / 2f - 14, _hotbarY + scaledHeight / 2f + 14, 1, true, true);
            }

            RenderText(SharpCraft.Instance.GetFps() + " FPS", 5, 6, 1, Vector3.UnitY, false, true);
        }

        private void DrawLives()
        {
            // Lives
            float y = _hotbarY - 20;

            for (int i = 0; i < 10; i++)
            {
                int u = 0;

                // Full hearts
                if (i < _fullHearts)
                    u = 32;
                // Half hearts
                else if (i < _totalOccupiedHearts)
                    u = 16;

                RenderTexture(TextureManager.TEXTURE_GUI_WIDGETS, _hotbarX + i * HeartsGap, y, u, 40, 16, 16);
            }
        }
    }
}