using OpenTK.Graphics.OpenGL;

namespace SharpCraft
{
    internal class ShaderGuiItem : ShaderProgram
    {
        public ShaderGuiItem(string shaderName) : base(shaderName, PrimitiveType.Quads)
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