using OpenTK;
using SharpCraft_Client.texture;
using SharpCraft_Client.util;

namespace SharpCraft_Client.gui
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

        public bool Enabled = true;

        protected bool Hovered;

        protected Vector3 HoverColor = new Vector3(1, 1, 0.65f);

        private readonly float _startX;
        private readonly float _startY;

        public GuiButton(int id, float x, float y, string text = "")
        {
            ID = id;

            _startX = x;
            _startY = y;

            Text = text;
        }

        public GuiButton(int id, float x, float y, float scale, string text = "") : this(id, x, y, text)
        {
            Scale = scale;
        }

        public override void Render(int mouseX, int mouseY)
        {
            int v = 0;

            Hovered = IsMouseOver(mouseX, mouseY);

            if (Hovered && Enabled)
                v += 20;

            Texture tex = TextureManager.TEXTURE_GUI_WIDGETS;

            if (CenteredX)
                PosX = _startX + (int)(SharpCraft.Instance.ClientSize.Width / 2f - 200 * Scale / 2);
            if (CenteredY)
                PosY = _startY + (int)(SharpCraft.Instance.ClientSize.Height / 2f - 20 * Scale / 2);

            RenderTexture(tex, PosX, PosY, 0, v, 200, 20, Scale);

            if (Text != "")
            {
                Vector3 color = Enabled ? (Hovered ? HoverColor : Vector3.One) : Vector3.One;
                float brightness = Enabled ? 1 : 0.65f;

                RenderText(Text, PosX + Scale * 200 / 2, PosY + Scale * 20 / 2, Scale * 0.75f, color, brightness, true, true);
            }
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