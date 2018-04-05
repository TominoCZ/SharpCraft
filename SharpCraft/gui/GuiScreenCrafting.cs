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
            //gui = new GuiTexture(TextureManager.LoadTexture("gui/crafting"), Vector2.One * 0.5f);
            DoesGuiPauseGame = false;
        }

        public override void Render(int mouseX, int mouseY)
        {
            DrawBackground();
            //RenderTexture(shader, gui, 0, 0);
        }

        public override void OnClose()
        {
            //TextureManager.DestroyTexture(gui.textureID);
        }
    }
}