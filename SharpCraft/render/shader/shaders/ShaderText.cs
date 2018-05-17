using OpenTK;
using SharpCraft.render.shader.uniform;

namespace SharpCraft.render.shader.shaders
{
    internal class ShaderText : ShaderGui
    {
        private UniformVec3 color;

        public ShaderText() : base("gui_text")
        {
        }

        protected override void RegisterUniforms()
        {
            base.RegisterUniforms();

            color = GetUniformVec3("colorIn");
        }

        public void SetColor(Vector3 color)
        {
            this.color?.Update(color);
        }
    }
}