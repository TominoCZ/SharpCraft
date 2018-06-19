using OpenTK;

namespace SharpCraft
{
    internal class TextureMapElement
    {
        public Vector2 UVMin { get; }
        public Vector2 UVMax { get; }

        public TextureMapElement(Vector2 uvMin, Vector2 uvMax)
        {
            UVMin = uvMin;
            UVMax = uvMax;
        }
    }
}