using OpenTK;
using OpenTK.Input;
using SharpCraft.texture;
using System;
using System.Collections.Generic;

namespace SharpCraft.gui
{
    internal class GuiScreen : Gui
    {
        private static GuiTexture _backgroundDefault;
        protected static GuiTexture Background;

        protected List<GuiButton> Buttons = new List<GuiButton>();

        public bool DoesGuiPauseGame { get; protected set; } = true;
        //public bool Visible { get; protected set; } = true;

        public GuiScreen()
        {
            _backgroundDefault = new GuiTexture(TextureManager.LoadTexture("gui/bg"), Vector2.Zero, Vector2.One * 16, 8);
            Background = new GuiTexture(TextureManager.LoadTexture("gui/bg_transparent"), Vector2.Zero, Vector2.One * 16, 8);
            //Visible = true;

            DoesGuiPauseGame = true;
        }

        public override void Render(int mouseX, int mouseY)
        {
            //if (visible == false)
              //  return;

            for (int i = 0; i < Buttons.Count; i++)
            {
                //GuiButton btn = buttons[i];

                Buttons[i].Render(mouseX, mouseY);
                //hovered? HoverColor : Vector3.One
                //if (btn is GuiItemSlot slot && !slot.stack?.IsEmpty == true && slot.stack.Count > 1)
                //RenderText(slot.stack.Count.ToString(), slot.PosX + 32 * slot.Scale / 2f, slot.PosY + 32 * slot.Scale / 2f + 14, 1, true);
            }
            //render stuff
        }

        protected virtual void DrawBackground()
        {
            float sizeX = Background.TextureSize.Width * Background.Scale;
            float sizeY = Background.TextureSize.Height * Background.Scale;

            double countX = Math.Ceiling(SharpCraft.Instance.ClientSize.Width / sizeX);
            double countY = Math.Ceiling(SharpCraft.Instance.ClientSize.Height / sizeY);

            for (int x = 0; x <= countX; x++)
            {
                for (int y = 0; y <= countY; y++)
                {
                    RenderTexture(Background, (int)(x * sizeX), (int)(y * sizeY));
                }
            }
        }

        protected virtual void DrawBackground(float countX, float countY, float offsetX = 0, float offsetY = 0)
        {
            float sizeX = Background.TextureSize.Width * Background.Scale;
            float sizeY = Background.TextureSize.Height * Background.Scale;

            for (int x = 0; x <= countX; x++)
            {
                for (int y = 0; y <= countY; y++)
                {
                    RenderTexture(Background, (int)(offsetX + (x * sizeX)), (int)(offsetY + (y - sizeY)));
                }
            }
        }

        // Draws background to screen, stretching it to the screen size
        protected virtual void DrawBackroundStretch()
        {
            RenderTextureStretchToScreen(Background);
        }

        protected virtual void DrawDefaultBackground()
        {
            float sizeX = _backgroundDefault.TextureSize.Width * _backgroundDefault.Scale;
            float sizeY = _backgroundDefault.TextureSize.Height * _backgroundDefault.Scale;

            double countX = Math.Ceiling(SharpCraft.Instance.ClientSize.Width / sizeX);
            double countY = Math.Ceiling(SharpCraft.Instance.ClientSize.Height / sizeY);

            for (int x = 0; x <= countX; x++)
            {
                for (int y = 0; y <= countY; y++)
                {
                    RenderTexture(_backgroundDefault, x * sizeX, y * sizeY);
                }
            }
        }

        public virtual void OnMouseClick(int x, int y, MouseButton button)
        {
            for (int i = Buttons.Count - 1; i >= 0; i--)
            {
                GuiButton btn = Buttons[i];

                if (btn.Enabled && btn.IsMouseOver(x, y))
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