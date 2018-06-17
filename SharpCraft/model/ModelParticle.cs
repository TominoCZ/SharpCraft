using SharpCraft.block;
using SharpCraft.particle;
using SharpCraft.render.shader.shaders;
using SharpCraft.util;
using System.Collections.Generic;

namespace SharpCraft.model
{
    internal class ModelParticle : ModelBaked<Particle>
    {
        public ModelParticle(ShaderParticle shader) : base(null, shader)
        {
            Dictionary<FaceSides, RawQuad> cube = ModelHelper.createTexturedCubeModel(EnumBlock.MISSING);

            RawModel = ModelManager.loadBlockModelToVAO(cube);
        }
    }
}