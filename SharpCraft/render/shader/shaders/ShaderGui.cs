using OpenTK;
using SharpCraft.gui;
using SharpCraft.render.shader.uniform;
using SharpCraft.texture;

namespace SharpCraft.render.shader.shaders
{
	class ShaderGui : Shader<GuiTexture>
	{
		private UniformVec2  UVmin;
		private UniformVec2  UVmax;

	    public ShaderGui() : base("gui")
		{
		}

        protected ShaderGui(string shaderName) : base(shaderName)
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
			UpdateInstanceUniforms(transform, instance.UVmin, instance.UVmax);
		}

	    public void UpdateInstanceUniforms(Matrix4 transform, TextureUVNode node)
	    {
	        UpdateInstanceUniforms(transform, node.start, node.end);
	    }

	    public void UpdateInstanceUniforms(Matrix4 transform, Vector2 UVmin, Vector2 UVmax)
	    {
	        base.UpdateInstanceUniforms(transform, null);

	        this.UVmin?.Update(UVmin);
	        this.UVmax?.Update(UVmax);
	    }
    }
}