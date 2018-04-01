using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SharpCraft.render.shader.uniform
{
	public class UniformVec3 : Uniform<Vector3>
	{
		public UniformVec3(int id) : base(id)
		{
		}

		protected override void Upload()
		{
			GL.Uniform3(Id, Data);
		}
	}
}