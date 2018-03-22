using OpenTK;

namespace SharpCraft
{
    class TextureUVNode
    {
        public Vector2 start;
        public Vector2 end;

        public TextureUVNode(Vector2 start, Vector2 end)
        {
            this.start = start;
            this.end = end;
        }

        public float[] ToArray()
        {
            return new[]
            {
                start.X, start.Y,
                start.X, end.Y,
                end.X, end.Y,
                end.X, start.Y
            };
        }
    }
}
