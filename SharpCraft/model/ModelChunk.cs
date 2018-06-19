using SharpCraft.render.shader;
using System.Collections.Generic;

namespace SharpCraft.model
{
    internal class ModelChunk : ModelBaked<ModelBlock>
    {
        public bool IsGenerated { get; private set; }

        public ModelChunk(List<float> vertexes, List<float> normals, List<float> uvs, Shader<ModelBlock> shader) : base(null, shader)
        {
            IsGenerated = vertexes.Count > 0;
            RawModel = ModelManager.LoadModelToVAO(vertexes.ToArray(), normals.ToArray(), uvs.ToArray());
        }

        public void OverrideData(List<float> vertexes, List<float> normals, List<float> uvs)
        {
            IsGenerated = vertexes.Count > 0;
            RawModel = ModelManager.OverrideModel3InVAO(RawModel.VaoID, RawModel.BufferIDs, vertexes, normals, uvs);
        }
    }
}