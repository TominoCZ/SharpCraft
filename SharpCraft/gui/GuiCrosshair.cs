using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpCraft
{
    class GuiCrosshair : Gui
    {
        private GuiTexture crosshairTexture;

        public GuiCrosshair(GuiTexture crosshairTexture)
        {
            this.crosshairTexture = crosshairTexture;
        }

        public override void render(ShaderGui shader, int mouseX, int mouseY)
        {
            renderTexture(shader, crosshairTexture);
        }
    }
}
