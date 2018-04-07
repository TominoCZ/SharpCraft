using OpenTK;
using SharpCraft.util;
using Size = System.Drawing.Size;

namespace SharpCraft.gui
{
    public class GuiTexture : Texture
    {
        public float Scale;
        public Vector2 Size;

        public Vector2 UVmin;
        public Vector2 UVmax;

        public GuiTexture(Texture tex, int startX, int startY, int sizeX, int sizeY) : this(tex, new Vector2(startX, startY), new Vector2(sizeX, sizeY), 1)
        {

        }

        public GuiTexture(Texture tex, int startX, int startY, int sizeX, int sizeY, float scale) : this(tex, new Vector2(startX, startY), new Vector2(sizeX, sizeY), scale)
        {

        }

        public GuiTexture(Texture tex, Vector2 textureStart, Vector2 size) : this(tex, textureStart, size, 1)
        {

        }

        public GuiTexture(Texture tex, Vector2 textureStart, Vector2 size, float scale) : base(tex.ID, tex.textureSize)
        {
            Size = size;
            Scale = scale;

            var pixel = new Vector2(1f / textureSize.Width, 1f / textureSize.Height);

            UVmin = textureStart * pixel;
            UVmax = UVmin + size * pixel;
        }

        /*
        public GuiTexture(int textureID, Size textureSize, Vector2 pos, Vector2 scale) : base(textureID, textureSize)
        {
            this.pos = pos;
            this.scale = scale;
        }

        public GuiTexture(int textureId, Size textureSize, Vector2 pos) : this(textureId, textureSize, pos, Vector2.One)
        {
        }

        public GuiTexture(int textureId, Size textureSize) : this(textureId, textureSize, Vector2.Zero, Vector2.One)
        {
        }

        public GuiTexture(Texture tex, Vector2 pos, Vector2 scale) : base(tex.textureID, tex.textureSize)
        {
            this.pos = pos;
            this.scale = scale;
        }

        public GuiTexture(Texture tex, Vector2 pos) : this(tex.textureID, tex.textureSize, pos, Vector2.One)
        {
        }

        public GuiTexture(Texture tex) : this(tex.textureID, tex.textureSize, Vector2.Zero, Vector2.One)
        {
        }*/
    }
}