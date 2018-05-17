using OpenTK.Graphics.OpenGL;

namespace SharpCraft.render.shader.uniform
{
    public class UniformFloat : Uniform<float>
    {
        public UniformFloat(int id) : base(id)
        {
        }

        protected override void Upload()
        {
            GL.Uniform1(Id, Data);
        }
    }
}