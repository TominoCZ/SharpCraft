using OpenTK;
using SharpCraft.render.shader.shaders;
using SharpCraft.util;

namespace SharpCraft.model
{
    public class ModelCubeOutline : ModelBaked<ModelCubeOutline>
    {
        private Vector4 _color;

        public ModelCubeOutline() : base(ModelManager.LoadModelToVAO(ModelHelper.CreateCubeModel(), 3), new ShaderColor())
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