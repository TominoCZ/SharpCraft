using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SharpCraft
{
    class ShaderGui : ShaderProgram
    {
        public ShaderGui(string shaderName) : base(shaderName, PrimitiveType.TriangleStrip)
        {

        }

        protected override void onBindAttributes()
        {
            bindAttribute(0, "position");
        }

        protected override void onRegisterUniforms()
        {

        }
    }
}
