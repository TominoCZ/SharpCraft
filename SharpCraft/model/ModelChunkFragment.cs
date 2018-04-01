using System.Collections.Generic;
using SharpCraft.render.shader;

namespace SharpCraft.model
{
    internal class ModelChunkFragment : ModelBaked<ModelBlock>
    {
        public ModelChunkFragment(Shader<ModelBlock> shader, List<RawQuad> quads) : base(null, shader)
        {
            rawModel = ModelManager.loadModelToVAO(quads, 3);
        }

        public void overrideData(List<RawQuad> quads)
        {
            rawModel = ModelManager.overrideModelInVAO(rawModel.vaoID, rawModel.bufferIDs, quads, 3);
        }
    }
}