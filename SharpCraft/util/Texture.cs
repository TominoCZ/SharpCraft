using System.Drawing;

namespace SharpCraft
{
    internal class Texture
    {
        public int textureID { get; }
        public Size textureSize { get; }

        public Texture(int textureID, Size textureSize)
        {
            this.textureID = textureID;
            this.textureSize = textureSize;
        }
    }
}