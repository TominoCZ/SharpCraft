using OpenTK;

namespace SharpCraft_Client.texture
{
    public class TextureUvNode
    {
        public readonly Vector2 Start;
        public readonly Vector2 End;

        public TextureUvNode(Vector2 start, Vector2 end)
        {
            Start = start;
            End = end;
        }
    }
}