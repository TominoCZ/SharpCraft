using OpenTK;
using SharpCraft.render.shader.uniform;
using SharpCraft.util;

namespace SharpCraft.render.shader.shaders
{
    public class ShaderTexturedCube : Shader<object>
    {
        private UniformVec2 UVmin;
        private UniformVec2 UVmax;

        public ShaderTexturedCube() : base("textured_cube")
        {
        }

        protected override void RegisterUniforms()
        {
            base.RegisterUniforms();

            UVmin = GetUniformVec2("UVmin");
            UVmax = GetUniformVec2("UVmax");
        }

        public override void UpdateInstanceUniforms(Matrix4 transform, object o = null)
        {
            base.UpdateInstanceUniforms(transform, o);
        }

        public void UpdateUVs(Texture tex, float startU, float startV, int size)
        {
            var pixel = new Vector2(1f / tex.TextureSize.Width, 1f / tex.TextureSize.Height);

            var min = new Vector2(startU, startV) * pixel;
            var max = min + size * pixel;

            UVmin?.Update(min);
            UVmax?.Update(max);
        }
    }
}