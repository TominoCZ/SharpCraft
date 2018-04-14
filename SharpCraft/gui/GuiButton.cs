using OpenTK;
using SharpCraft.texture;

namespace SharpCraft.gui
{
    internal class GuiButton : Gui
    {
        public int ID;

        public float AlphaForRender = 1;

        public float PosX;
        public float PosY;

        public float Scale = 1;

        public string Text;

        public bool CenteredX;
        public bool CenteredY;

        protected bool Hovered;

        protected Vector3 HoverColor = new Vector3(1, 1, 0.65f);

        public GuiButton(int id, float x, float y, string text = "")
        {
            ID = id;

            PosX = x;
            PosY = y;

            Text = text;
        }

        public GuiButton(int id, float x, float y, float scale, string text = "") : this(id, x, y, text)
        {
            Scale = scale;
        }

        public override void Render(int mouseX, int mouseY)
        {
            var v = 0;

            Hovered = IsMouseOver(mouseX, mouseY);

            if (Hovered)
                v += 20;

            var tex = TextureManager.TEXTURE_GUI_WIDGETS;

            if (CenteredX)
                PosX = (int)(SharpCraft.Instance.ClientSize.Width / 2f - 200 * Scale / 2);
            if (CenteredY)
                PosY = (int)(SharpCraft.Instance.ClientSize.Height / 2f - 20 * Scale / 2);

            RenderTexture(tex, PosX, PosY, 0, v, 200, 20, Scale);

            if (Text != "")
                RenderText(Text, PosX + Scale * 200 / 2, PosY + Scale * 20 / 2, Scale * 0.75f, Hovered ? HoverColor : Vector3.One, true, true);
        }

        public virtual bool IsMouseOver(int x, int y)
        {
            return x >= PosX &&
                   y >= PosY &&
                   x <= PosX + 200 * Scale &&
                   y <= PosY + 20 * Scale;
        }
    }
}