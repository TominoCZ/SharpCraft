using OpenTK.Graphics.OpenGL;

namespace SharpCraft
{
    internal class ShaderBlock : ShaderProgram
    {
        public ShaderBlock(string shaderName, PrimitiveType renderType) : base(shaderName, renderType)
        {
        }

        protected override void onBindAttributes()
        {
            bindAttribute(0, "position");
            bindAttribute(1, "textureCoords");
            bindAttribute(2, "normal");
        }

        protected override void onRegisterUniforms()
        {
            registerUniforms("lightColor");
        }
    }
}