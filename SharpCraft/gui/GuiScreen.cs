using OpenTK;
using OpenTK.Input;
using SharpCraft.texture;
using System;
using System.Collections.Generic;

namespace SharpCraft.gui
{
    internal class GuiScreen : Gui
    {
        private static GuiTexture background_default;
        private static GuiTexture background;

        protected List<GuiButton> buttons = new List<GuiButton>();

        public bool DoesGuiPauseGame { get; protected set; } = true;

        public GuiScreen()
        {
            background_default = new GuiTexture(TextureManager.LoadTexture("gui/bg"), Vector2.Zero, Vector2.One * 16, 8);
            background = new GuiTexture(TextureManager.LoadTexture("gui/bg_transparent"), Vector2.Zero, Vector2.One * 16, 8);
        }

        public override void Render(int mouseX, int mouseY)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                //GuiButton btn = buttons[i];

                buttons[i].Render(mouseX, mouseY);
                //hovered? HoverColor : Vector3.One
                //if (btn is GuiItemSlot slot && !slot.stack?.IsEmpty == true && slot.stack.Count > 1)
                //RenderText(slot.stack.Count.ToString(), slot.PosX + 32 * slot.Scale / 2f, slot.PosY + 32 * slot.Scale / 2f + 14, 1, true);
            }
            //render stuff
        }

        protected virtual void DrawBackground()
        {
            float sizeX = background.TextureSize.Width * background.Scale;
            float sizeY = background.TextureSize.Height * background.Scale;

            double countX = Math.Ceiling(SharpCraft.Instance.ClientSize.Width / sizeX);
            double countY = Math.Ceiling(SharpCraft.Instance.ClientSize.Height / sizeY);

            for (int x = 0; x <= countX; x++)
            {
                for (int y = 0; y <= countY; y++)
                {
                    RenderTexture(background, (int)(x * sizeX), (int)(y * sizeY));
                }
            }
        }

        // Draws background to screen, stretching it to the screen size
        protected virtual void DrawBackroundStretch()
        {
            RenderTextureStretchToScreen(background);
        }

        protected virtual void DrawDefaultBackground()
        {
            float sizeX = background_default.TextureSize.Width * background_default.Scale;
            float sizeY = background_default.TextureSize.Height * background_default.Scale;

            double countX = Math.Ceiling(SharpCraft.Instance.ClientSize.Width / sizeX);
            double countY = Math.Ceiling(SharpCraft.Instance.ClientSize.Height / sizeY);

            for (int x = 0; x <= countX; x++)
            {
                for (int y = 0; y <= countY; y++)
                {
                    RenderTexture(background_default, x * sizeX, y * sizeY);
                }
            }
        }

        public virtual void OnMouseClick(int x, int y, MouseButton button)
        {
            for (int i = buttons.Count - 1; i >= 0; i--)
            {
                GuiButton btn = buttons[i];

                if (btn.IsMouseOver(x, y))
                {
                    ButtonClicked(btn, button);
                    break;
                }
            }
        }

        protected virtual void ButtonClicked(GuiButton btn, MouseButton button)
        {
        }

        public virtual void OnClose()
        {
        }
    }
}