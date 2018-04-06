using System.Security.Cryptography.X509Certificates;
using OpenTK;
using SharpCraft.render.shader;
using SharpCraft.render.shader.shaders;
using SharpCraft.texture;

namespace SharpCraft.gui
{
    internal class GuiButton : Gui
    {
        public int ID;

        public float alphaForRender = 1;

        public float posX;
        public float posY;

        public float scale = 1;

        public bool centered;

        public GuiButton(int ID, float x, float y)
        {
            this.ID = ID;

            posX = x;
            posY = y;
        }

        public GuiButton(int ID, float x, float y, float scale) : this(ID, x, y)
        {
            this.scale = scale;
        }

        public override void Render(int mouseX, int mouseY)
        {
            var v = 0;

            if (IsMouseOver(mouseX, mouseY))
                v += 20;

            var tex = TextureManager.TEXTURE_GUI_WIDGETS;

            if (centered)
                posX = (int)(SharpCraft.Instance.ClientSize.Width / 2f - 200 * scale / 2);
            
            RenderTexture(tex, posX, posY, 0, v, 200, 20, scale);
        }

        public virtual bool IsMouseOver(int x, int y)
        {
            return x >= posX &&
                   y >= posY &&
                   x <= posX + 200 * scale &&
                   y <= posY + 20 * scale;
        }
    }
}