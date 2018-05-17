using SharpCraft.block;
using SharpCraft.particle;
using SharpCraft.render.shader.shaders;
using SharpCraft.util;

namespace SharpCraft.model
{
    internal class ModelParticle : ModelBaked<Particle>
    {
        public ModelParticle(ShaderParticle shader) : base(null, shader)
        {
            var cube = ModelHelper.createTexturedCubeModel(EnumBlock.MISSING);

            RawModel = ModelManager.loadBlockModelToVAO(cube);
        }
    }
}