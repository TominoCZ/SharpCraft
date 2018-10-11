using SharpCraft_Client.render.shader;

namespace SharpCraft_Client.model
{
    internal class ModelGuiItem : ModelBaked
    {
        public ModelGuiItem(Shader shader) : base(null, shader)
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