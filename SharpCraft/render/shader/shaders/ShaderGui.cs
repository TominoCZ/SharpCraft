using OpenTK;
using SharpCraft.gui;
using SharpCraft.model;
using SharpCraft.particle;
using SharpCraft.render.shader.uniform;
using SharpCraft.util;

namespace SharpCraft.render.shader.shaders
{
	public class ShaderGui : Shader<GuiTexture>
	{
		private UniformVec2  UVmin;
		private UniformVec2  UVmax;

        public ShaderGui() : base("gui")
		{
		}

	    protected override void RegisterUniforms()
		{
			base.RegisterUniforms();

			UVmin = GetUniformVec2("UVmin");
			UVmax = GetUniformVec2("UVmax");
        }

		public override void UpdateInstanceUniforms(Matrix4 transform, GuiTexture instance)
		{
			base.UpdateInstanceUniforms(transform, instance);
			UVmin?.Update(instance.UVmin);
			UVmax?.Update(instance.UVmax);
		}
	}
}