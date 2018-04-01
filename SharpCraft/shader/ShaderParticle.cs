using OpenTK.Graphics.OpenGL;

namespace SharpCraft.shader
{
    internal class ShaderParticle : ShaderProgram
    {
        public ShaderParticle() : base("particle", PrimitiveType.Quads)
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
            registerUniforms("lightColor", "UVmin", "UVmax", "alpha");
        }
    }
}