using OpenTK;
using SharpCraft.render.shader;

namespace SharpCraft.model
{
    public class ModelCubeOutline : ModelBaked
    {
        public ModelCubeOutline() : base(null, new Shader("color", "colorIn"))
        {
            float[] vertices =
            {
                0, 1, 1,
                0, 0, 1,
                0, 1, 1,
                1, 1, 1,
                0, 0, 1,
                1, 0, 1,
                1, 0, 1,
                1, 1, 1,
                0, 1, 0,
                0, 1, 1,
                1, 1, 0,
                1, 1, 1,
                0, 0, 0,
                0, 0, 1,
                1, 0, 0,
                1, 0, 1,
                0, 1, 0,
                0, 0, 0,
                0, 1, 0,
                1, 1, 0,
                0, 0, 0,
                1, 0, 0,
                1, 0, 0,
                1, 1, 0
            };

            RawModel = ModelManager.LoadModel3ToVao(vertices);
        }

        public void SetColor(Vector4 color)
        {
            Shader.SetVector4("colorIn", color);
        }
    }
}