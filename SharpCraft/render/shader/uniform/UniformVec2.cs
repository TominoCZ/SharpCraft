using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SharpCraft.render.shader.uniform
{
	public class UniformVec2 : Uniform<Vector2>
	{
		public UniformVec2(int id) : base(id)
		{
		}

		protected override void Upload()
		{
			GL.Uniform2(Id, Data);
		}
	}
}