using OpenTK;
using SharpCraft.model;
using SharpCraft.particle;
using SharpCraft.render.shader.uniform;

namespace SharpCraft.render.shader.shaders
{
	public class ShaderColor : Shader<ModelCubeOutline>
	{
		private UniformVec4 color;

		public ShaderColor() : base("color")
		{
		}

		protected override void RegisterUniforms()
		{
			base.RegisterUniforms();

		    color = GetUniformVec4("colorIn");
		}

		public override void UpdateInstanceUniforms(Matrix4 transform, ModelCubeOutline instance)
		{
			base.UpdateInstanceUniforms(transform, instance);

		    color?.Update(instance.GetColor());
		}
	}
}