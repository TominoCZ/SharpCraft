using SharpCraft.render.shader;

namespace SharpCraft.model
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