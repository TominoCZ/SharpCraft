using OpenTK.Graphics.OpenGL;

namespace SharpCraft.shader
{
    internal class ShaderEntityItem : ShaderProgram
    {
        public ShaderEntityItem() : base("entity_item", PrimitiveType.Quads)
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
