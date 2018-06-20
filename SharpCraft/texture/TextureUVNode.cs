using OpenTK;

namespace SharpCraft.texture
{
    internal class TextureUVNode
    {
        public Vector2 start;
        public Vector2 end;

        public TextureUVNode(Vector2 start, Vector2 end)
        {
            this.start = start;
            this.end = end;
        }
    }
}