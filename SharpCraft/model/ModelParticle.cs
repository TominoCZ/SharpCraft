using SharpCraft.block;
using SharpCraft.particle;
using SharpCraft.render.shader;
using SharpCraft.render.shader.shaders;
using SharpCraft.util;

namespace SharpCraft.model
{
    class ModelParticle : ModelBaked<Particle>
    {
        public ModelParticle(ShaderParticle shader) : base(null, shader)
        {
            var cube = ModelHelper.createTexturedCubeModel(EnumBlock.RARE);

            rawModel = ModelManager.loadBlockModelToVAO(cube);
        }
    }
}
