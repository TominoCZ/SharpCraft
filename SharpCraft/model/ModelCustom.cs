using SharpCraft_Client.render.shader;

namespace SharpCraft_Client.model
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