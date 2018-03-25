using OpenTK.Graphics.OpenGL;

namespace SharpCraft
{
    internal class ShaderBlockOutline : ShaderBlockUnlit
    {
        public ShaderBlockOutline() : base("color", PrimitiveType.Quads)
        {
        }

        protected override void onBindAttributes()
        {
            bindAttribute(0, "position");
        }

        protected override void onRegisterUniforms()
        {
            registerUniforms("colorIn");
        }
    }
}