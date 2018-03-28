using System.Collections.Generic;
using OpenTK;
using SharpCraft.shader;
using SharpCraft.texture;

namespace SharpCraft.gui
{
    internal class GuiScreen : Gui
    {
        private GuiTexture background;

        protected List<GuiButton> buttons = new List<GuiButton>();

        public GuiScreen()
        {
            background = new GuiTexture(TextureManager.loadTexture("gui/bg", false), Vector2.Zero, Vector2.One * 4);
        }

        public override void render(ShaderGui shader, int mouseX, int mouseY)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].render(shader, mouseX, mouseY);
            }
            //render stuff
        }

        protected virtual void drawBackground(ShaderGui shader, GuiTexture tex)
        {
            var sizeX = tex.textureSize.Width * tex.scale.X;
            var sizeY = tex.textureSize.Height * tex.scale.Y;

            var countX = Game.Instance.ClientSize.Width / sizeX;
            var countY = Game.Instance.ClientSize.Height / sizeY;

            for (int x = 0; x <= countX; x++)
            {
                for (int y = 0; y <= countY; y++)
                {
                    renderTexture(shader, tex, (int)(x * sizeX), (int)(y * sizeY));
                }
            }
        }

        public virtual void onMouseClick(int x, int y)
        {
            for (int i = buttons.Count - 1; i >= 0; i--)
            {
                var btn = buttons[i];

                if (btn.isMouseOver(x, y))
                {
                    buttonClicked(btn);
                    break;
                }
            }
        }

        protected virtual void buttonClicked(GuiButton btn)
        {
        }

        public virtual void onClose()
        {
            TextureManager.destroyTexture(background.textureID);
        }
    }
}