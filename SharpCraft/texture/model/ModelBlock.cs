using SharpCraft.render.shader;

namespace SharpCraft.model
{
    public class ModelBlock : ModelBaked<ModelBlock>
    {
        public ModelBlock(Shader<ModelBlock> shader, ModelBlockRaw rawModel) : base(null, shader)
        {
            RawModel = rawModel;
        }
    }
}