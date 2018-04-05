using OpenTK;
using SharpCraft.render.shader;
using SharpCraft.render.shader.shaders;
using SharpCraft.texture;

namespace SharpCraft.gui
{
    internal class GuiButton : Gui
    {
        public int ID;

        public int posX;
        public int posY;

        public float scale = 1;

        public bool centered;

        public GuiButton(int ID, int x, int y)
        {
            this.ID = ID;

            posX = x;
            posY = y;
        }

        public GuiButton(int ID, int x, int y, float scale) : this(ID, x, y)
        {
            this.scale = scale;
        }

        public override void Render(int mouseX, int mouseY)
        {
            var u = 0;
            var v = 0;

            if (isMouseOver(mouseX, mouseY))
                v += 20;

            var tex = TextureManager.TEXTURE_GUI_WIDGETS;

            if (centered)
                posX = (int)(SharpCraft.Instance.ClientSize.Width / 2f - 200 * scale / 2);

            RenderTexture(tex, posX, posY, u, v, 200, 20, scale);
        }

        public virtual void Dispose()
        {
        }

        internal bool isMouseOver(int x, int y)
        {
            return x >= posX &&
                   y >= posY &&
                   x <= posX + 200 * scale &&
                   y <= posY + 20 * scale;
        }
    }
}