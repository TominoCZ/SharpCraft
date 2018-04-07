using System.Drawing;

namespace SharpCraft.util
{
    public class Texture
    {
        public int ID { get; }
        public Size textureSize { get; }

        public Texture(int id, Size textureSize)
        {
            this.ID = id;
            this.textureSize = textureSize;
        }
    }
}