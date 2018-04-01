using OpenTK;
using SharpCraft.render.shader;
using SharpCraft.render.shader.shaders;
using SharpCraft.texture;

namespace SharpCraft.gui
{
    internal class GuiScreenCrafting : GuiScreen
    {
        private GuiTexture gui;

        public GuiScreenCrafting()
        {
            gui = new GuiTexture(TextureManager.loadTexture("gui/crafting"), Vector2.One * 0.5f);
            DoesGuiPauseGame = false;
        }

        public override void render(Shader<Gui> shader, int mouseX, int mouseY)
        {
            renderTexture(shader, gui, 0, 0);
        }

        public override void onClose()
        {
            TextureManager.destroyTexture(gui.textureID);
        }
    }
}