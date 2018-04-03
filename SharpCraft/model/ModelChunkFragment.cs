using System.Collections.Concurrent;
using System.Collections.Generic;
using SharpCraft.render.shader;

namespace SharpCraft.model
{
    internal class ModelChunkFragment : ModelBaked<ModelBlock>
    {
        public ModelChunkFragment(Shader<ModelBlock> shader, List<RawQuad> quads) : base(null, shader)
        {
            RawModel = ModelManager.loadModelToVAO(quads, 3);
        }

        public void overrideData(List<RawQuad> quads)
        {
            RawModel = ModelManager.overrideModelInVAO(RawModel.vaoID, RawModel.bufferIDs, quads, 3);
        }
    }
}