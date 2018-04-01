using SharpCraft.render.shader;
using SharpCraft.render.shader.shaders;

namespace SharpCraft.gui
{
    internal class GuiCrosshair : Gui
    {
        private GuiTexture crosshairTexture;

        public GuiCrosshair(GuiTexture crosshairTexture)
        {
            this.crosshairTexture = crosshairTexture;
        }

        public override void Render(Shader<Gui> shader, int mouseX, int mouseY)
        {
            renderTexture(shader, crosshairTexture);
        }
    }
}