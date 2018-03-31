using OpenTK;
using SharpCraft.shader;
using SharpCraft.texture;

namespace SharpCraft.gui
{
    internal class GuiScreenCrafting : GuiScreen
    {
        private GuiTexture gui;

        public GuiScreenCrafting()
        {
            gui = new GuiTexture(TextureManager.loadTexture("gui/crafting", false), Vector2.One * 0.5f);
            DoesGuiPauseGame = false;
        }

        public override void render(ShaderGui shader, int mouseX, int mouseY)
        {
            renderTexture(shader, gui, 0, 0);
        }

        public override void onClose()
        {
            TextureManager.destroyTexture(gui.textureID);
        }
    }
}