using OpenTK;

namespace SharpCraft
{
    public class ModelLight
    {
        public Vector3 pos;

        public Vector3 color;

        public ModelLight(Vector3 pos, Vector3 color)
        {
            this.pos = pos;
            this.color = color;
        }
    }
}