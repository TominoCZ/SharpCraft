using OpenTK;
using SharpCraft.texture;
using System.Collections.Generic;
using SharpCraft.render.shader;
using SharpCraft.render.shader.shaders;

namespace SharpCraft.gui
{
    internal class GuiScreen : Gui
    {
        private GuiTexture background;

        protected List<GuiButton> buttons = new List<GuiButton>();

        public bool DoesGuiPauseGame { get; protected set; } = true;

        public GuiScreen()
        {
            background = new GuiTexture(TextureManager.loadTexture("gui/bg"), Vector2.Zero, Vector2.One * 4);
        }

        public override void Render(Shader<Gui> shader, int mouseX, int mouseY)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].Render(shader, mouseX, mouseY);
            }
            //render stuff
        }

        protected virtual void drawBackground(Shader<Gui> shader, GuiTexture tex)
        {
            var sizeX = tex.textureSize.Width * tex.scale.X;
            var sizeY = tex.textureSize.Height * tex.scale.Y;

            var countX = SharpCraft.Instance.ClientSize.Width / sizeX;
            var countY = SharpCraft.Instance.ClientSize.Height / sizeY;

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