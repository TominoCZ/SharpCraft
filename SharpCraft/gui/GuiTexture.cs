using OpenTK;
using SharpCraft.util;

namespace SharpCraft.gui
{
    internal class GuiTexture : Texture
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

        public GuiTexture(Texture tex, Vector2 textureStart, Vector2 size, float scale) : base(tex.ID, tex.TextureSize)
        {
            Size = size;
            Scale = scale;

            var pixel = new Vector2(1f / TextureSize.Width, 1f / TextureSize.Height);

            UVmin = textureStart * pixel;
            UVmax = UVmin + size * pixel;
        }
    }
}