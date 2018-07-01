using SharpCraft.render.shader;

namespace SharpCraft.model
{
    public class ModelItem : ModelBaked<ModelItem>
    {
        public TextureMapElement SlotTexture { get; }

        public ModelItem(TextureMapElement texture, Shader<ModelItem> shader, ModelItemRaw rawModel) : base(rawModel, shader)
        {
            SlotTexture = texture;
        }
    }
}