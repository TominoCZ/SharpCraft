using OpenTK;
using SharpCraft.render.shader.shaders;

namespace SharpCraft.model
{
    public class ModelCubeOutline : ModelBaked<ModelCubeOutline>
    {
        private Vector4 _color;

        public ModelCubeOutline() : base(ModelManager.LoadModel3ToVao(CubeModelBuilder.CreateCubeVertexes()), new ShaderColor())
        {
        }

        public Vector4 GetColor()
        {
            return _color;
        }

        public void SetColor(Vector4 color)
        {
            _color = color;
        }
    }
}