using OpenTK;
using SharpCraft.util;
using Size = System.Drawing.Size;

namespace SharpCraft.gui
{
    internal class GuiTexture : Texture
    {
        public Vector2 pos;
        public Vector2 scale = Vector2.One;

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
        }
    }
}