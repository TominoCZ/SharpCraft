using SharpCraft_Client.render.shader;

namespace SharpCraft_Client.model
{
    internal class ModelParticle : ModelBaked
    {
        public ModelParticle(Shader shader) : base(null, shader)
        {
            var vertexes = CubeModelBuilder.CreateCubeVertexes(true);
            var normals = CubeModelBuilder.CreateCubeNormals();
            var uvs = CubeModelBuilder.CreateCubeUvs();

            RawModel = ModelManager.LoadModel3ToVao(vertexes, normals, uvs);
        }
    }
}