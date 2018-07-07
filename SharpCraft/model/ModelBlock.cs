using SharpCraft.render.shader;

namespace SharpCraft.model
{
    public class ModelBlock : ModelBaked
    {
        public TextureMapElement SlotTexture { get; }
        public TextureMapElement ParticleTexture { get; }

        public ModelBlock(TextureMapElement slotTexture, TextureMapElement particleTexture, Shader shader, ModelBlockRaw rawModel) : base(rawModel, shader)
        {
            SlotTexture = slotTexture;
            ParticleTexture = particleTexture;
        }
    }
}