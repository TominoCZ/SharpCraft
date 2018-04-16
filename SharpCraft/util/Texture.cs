using System.Drawing;

namespace SharpCraft.util
{
    public class Texture
    {
        public int ID { get; }
        public Size TextureSize { get; }

        public Texture(int id, Size textureSize)
        {
            ID = id;
            TextureSize = textureSize;
        }
    }
}