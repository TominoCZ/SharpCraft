using SharpCraft.render.shader;

namespace SharpCraft.model
{
    internal class ModelChunk : ModelBaked<ModelBlock>
    {
        public bool IsGenerated { get; private set; }

        public ModelChunk(float[] vertexes, float[] normals, float[] uvs, Shader<ModelBlock> shader) : base(null, shader)
        {
            IsGenerated = vertexes.Length > 0;
            RawModel = ModelManager.LoadModelToVAO(vertexes, normals, uvs);
        }

        public void OverrideData(float[] vertexes, float[] normals, float[] uvs)
        {
            IsGenerated = vertexes.Length > 0;
            RawModel = ModelManager.OverrideModel3InVAO(RawModel.VaoID, RawModel.BufferIDs, vertexes, normals, uvs);
        }
    }
}