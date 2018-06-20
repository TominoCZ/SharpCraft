using SharpCraft.render.shader;

namespace SharpCraft.model
{
    public class ModelBlock : ModelBaked<ModelBlock>
    {
        private readonly TextureMapElement _particleTexture;

        public ModelBlock(TextureMapElement particleTexture, Shader<ModelBlock> shader, ModelBlockRaw rawModel) : base(null, shader)
        {
            RawModel = rawModel;
            _particleTexture = particleTexture;
        }

        public TextureMapElement GetParticleTexture()
        {
            return _particleTexture;
        }
    }
}