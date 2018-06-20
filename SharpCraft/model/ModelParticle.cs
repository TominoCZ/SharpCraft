using SharpCraft.particle;
using SharpCraft.render.shader.shaders;

namespace SharpCraft.model
{
    internal class ModelParticle : ModelBaked<Particle>
    {
        public ModelParticle(ShaderParticle shader) : base(null, shader)
        {
            var vertexes = CubeModelBuilder.CreateCubeVertexes();
            var normals = CubeModelBuilder.CreateCubeNormals();
            var uvs = CubeModelBuilder.CreateCubeUvs();

            RawModel = ModelManager.LoadModel3ToVao(vertexes, normals, uvs);
        }
    }
}