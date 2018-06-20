using SharpCraft.render.shader;

namespace SharpCraft.model
{
    internal class ModelChunk : ModelBaked<ModelBlock>
    {
        public bool IsGenerated { get; private set; }

        public ModelChunk(float[] vertexes, float[] normals, float[] uvs, Shader<ModelBlock> shader) : base(null, shader)
        {
            IsGenerated = vertexes.Length > 0;
            RawModel = ModelManager.LoadModel3ToVao(vertexes, normals, uvs);
        }

        public void OverrideData(float[] vertexes, float[] normals, float[] uvs)
        {
            IsGenerated = vertexes.Length > 0;
            RawModel = ModelManager.OverrideModel3InVao(RawModel.VaoID, RawModel.BufferIDs, vertexes, normals, uvs);
        }
    }
}