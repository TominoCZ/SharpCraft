using SharpCraft.render.shader;
using System.Collections.Generic;

namespace SharpCraft.model
{
    internal class ModelChunkFragment : ModelBaked<ModelBlock>
    {
        public ModelChunkFragment(Shader<ModelBlock> shader, List<RawQuad> quads) : base(null, shader)
        {
            RawModel = ModelManager.loadModelToVAO(quads, 3);
        }

        public void OverrideData(List<RawQuad> quads)
        {
            RawModel = ModelManager.overrideModelInVAO(RawModel.vaoID, RawModel.bufferIDs, quads, 3);
        }
    }
}