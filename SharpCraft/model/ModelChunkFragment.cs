using SharpCraft.render.shader;
using System.Collections.Generic;

namespace SharpCraft.model
{
    internal class ModelChunkFragment : ModelBaked<ModelBlock>
    {
        public ModelChunkFragment(Shader<ModelBlock> shader, List<RawQuad> quads) : base(null, shader)
        {
            RawModel = ModelManager.LoadModelToVAO(quads, 3);
        }

        public void OverrideData(List<RawQuad> quads)
        {
            RawModel = ModelManager.OverrideModelInVAO(RawModel.VaoID, RawModel.BufferIDs, quads, 3);
        }
    }
}