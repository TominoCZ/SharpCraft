using SharpCraft_Client.render.shader;
using SharpCraft_Client.texture;

namespace SharpCraft_Client.model
{
    public class ModelItem : ModelBaked
    {
        public TextureMapElement SlotTexture { get; }

        public ModelItem(TextureMapElement texture, Shader shader, ModelItemRaw rawModel) : base(rawModel, shader)
        {
            SlotTexture = texture;
        }
    }
}