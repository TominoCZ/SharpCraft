using OpenTK;
using SharpCraft.render.shader;
using SharpCraft.render.shader.shaders;
using SharpCraft.texture;

namespace SharpCraft.gui
{
    internal class GuiScreenIngameMenu : GuiScreen
    {
        private GuiTexture background;

        public GuiScreenIngameMenu()
        {
            background = new GuiTexture(TextureManager.loadTexture("gui/bg_transparent"), Vector2.Zero, Vector2.One * 4);
        }

        public override void Render(Shader<Gui> shader, int mouseX, int mouseY)
        {
            drawBackground(shader, background);

            base.Render(shader, mouseX, mouseY);
        }

        public override void onClose()
        {
            TextureManager.destroyTexture(background.textureID);
        }
    }
}