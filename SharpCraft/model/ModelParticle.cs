using SharpCraft.block;
using SharpCraft.shader;
using SharpCraft.util;

namespace SharpCraft.model
{
    class ModelParticle : ModelBaked
    {
        public ModelParticle(ShaderProgram shader) : base(null, shader)
        {
            var cube = ModelHelper.createTexturedCubeModel(EnumBlock.RARE);

            rawModel = ModelManager.loadBlockModelToVAO(cube);
        }
    }
}
