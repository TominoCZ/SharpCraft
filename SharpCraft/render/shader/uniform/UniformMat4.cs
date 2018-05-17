using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SharpCraft.render.shader.uniform
{
    public class UniformMat4 : Uniform<Matrix4>
    {
        public UniformMat4(int id) : base(id)
        {
        }

        protected override void Upload()
        {
            GL.UniformMatrix4(Id, false, ref Data);
        }
    }
}