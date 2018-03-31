using OpenTK;
using SharpCraft.shader;
using SharpCraft.texture;

namespace SharpCraft.gui
{
    internal class GuiScreenIngameMenu : GuiScreen
    {
        private GuiTexture background;

        public GuiScreenIngameMenu()
        {
            background = new GuiTexture(TextureManager.loadTexture("gui/bg_transparent", false), Vector2.Zero, Vector2.One * 4);

            DoesGuiPauseGame = true;
        }

        public override void render(ShaderGui shader, int mouseX, int mouseY)
        {
            drawBackground(shader, background);

            base.render(shader, mouseX, mouseY);
        }

        public override void onClose()
        {
            TextureManager.destroyTexture(background.textureID);
        }
    }
}