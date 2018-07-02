using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenTK;
using SharpCraft.render.shader.shaders;

namespace SharpCraft.model
{
    public class ModelCubeOutline : ModelBaked<ModelCubeOutline>
    {
        private Vector4 _color;

        public ModelCubeOutline() : base(null, new ShaderColor())
        {
            float[] vertices =
            {
                0, 1, 1,
                0, 0, 1,
                0, 1, 1,
                1, 1, 1,
                0, 0, 1,
                1, 0, 1,
                1, 0, 1,
                1, 1, 1,
                0, 1, 0,
                0, 1, 1,
                1, 1, 0,
                1, 1, 1,
                0, 0, 0,
                0, 0, 1,
                1, 0, 0,
                1, 0, 1,
                0, 1, 0,
                0, 0, 0,
                0, 1, 0,
                1, 1, 0,
                0, 0, 0,
                1, 0, 0,
                1, 0, 0,
                1, 1, 0
            };

            RawModel = ModelManager.LoadModel3ToVao(vertices);
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