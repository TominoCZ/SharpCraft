using SharpCraft.render.shader;

namespace SharpCraft.model
{
    internal class ModelGuiItem : ModelBaked<object>
    {
        public ModelGuiItem(Shader<object> shader) : base(null, shader)
        {
            float[] vertexes =
            {
                -1, 1,
                -1, -1,
                1, 1,

                1, 1,
                -1, -1,
                1, -1
            };
            float[] uvs =
            {
                0, 0,
                0, 1,
                1, 0,

                1, 0,
                0, 1,
                1, 1
            };

            RawModel = ModelManager.LoadModel2ToVao(vertexes, uvs);
        }
    }
}