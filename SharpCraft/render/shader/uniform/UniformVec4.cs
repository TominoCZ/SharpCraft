using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SharpCraft.render.shader.uniform
{
	public class UniformVec4 : Uniform<Vector4>
	{
		public UniformVec4(int id) : base(id)
		{
		}

		protected override void Upload()
		{
			GL.Uniform4(Id, Data);
		}
	}
}