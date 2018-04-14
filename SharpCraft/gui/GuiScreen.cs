using System;
using OpenTK;
using SharpCraft.texture;
using System.Collections.Generic;
using OpenTK.Input;

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
                var btn = buttons[i];

                btn.Render(mouseX, mouseY);
                //hovered? HoverColor : Vector3.One
                //if (btn is GuiItemSlot slot && !slot.stack?.IsEmpty == true && slot.stack.Count > 1)
                    //RenderText(slot.stack.Count.ToString(), slot.PosX + 32 * slot.Scale / 2f, slot.PosY + 32 * slot.Scale / 2f + 14, 1, true);
            }
            //render stuff
        }

        protected virtual void DrawBackground()
        {
            var sizeX = background.textureSize.Width * background.Scale;
            var sizeY = background.textureSize.Height * background.Scale;

            var countX = Math.Ceiling(SharpCraft.Instance.ClientSize.Width / sizeX);
            var countY = Math.Ceiling(SharpCraft.Instance.ClientSize.Height / sizeY);

            for (int x = 0; x <= countX; x++)
            {
                for (int y = 0; y <= countY; y++)
                {
                    RenderTexture(background, (int)(x * sizeX), (int)(y * sizeY));
                }
            }
        }

        protected virtual void DrawDefaultBackground()
        {
            var sizeX = background_default.textureSize.Width * background_default.Scale;
            var sizeY = background_default.textureSize.Height * background_default.Scale;

            var countX = Math.Ceiling(SharpCraft.Instance.ClientSize.Width / sizeX);
            var countY = Math.Ceiling(SharpCraft.Instance.ClientSize.Height / sizeY);

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
                var btn = buttons[i];

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