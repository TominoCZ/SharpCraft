using SharpCraft.render.shader;

namespace SharpCraft.model
{
    public class ModelCustom : ModelBaked
    {
        public int TextureID { get; }

        public ModelCustom(int textureId, IModelRaw rawModel, Shader shader) : base(rawModel, shader)
        {
            TextureID = textureId;
        }
    }
}