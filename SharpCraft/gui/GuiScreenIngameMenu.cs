using OpenTK;
using SharpCraft.render.shader;
using SharpCraft.render.shader.shaders;
using SharpCraft.texture;

namespace SharpCraft.gui
{
    internal class GuiScreenIngameMenu : GuiScreen
    {
        public GuiScreenIngameMenu()
        {

        }

        public override void Render(int mouseX, int mouseY)
        {
            DrawBackground();

            base.Render(mouseX, mouseY);
        }
    }
}