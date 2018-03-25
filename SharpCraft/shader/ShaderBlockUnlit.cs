using OpenTK.Graphics.OpenGL;

namespace SharpCraft
{
    internal class ShaderBlockUnlit : ShaderProgram
    {
        public ShaderBlockUnlit(string shaderName, PrimitiveType renderType) : base(shaderName, renderType)
        {
        }

        protected override void onBindAttributes()
        {
            bindAttribute(0, "position");
            bindAttribute(1, "textureCoords");
        }

        protected override void onRegisterUniforms()
        {
        }
    }
}