using OpenTK.Graphics.OpenGL;
using SharpCraft.render.shader;

namespace SharpCraft.model
{
    public class ModelBaked : IModelBaked
    {
        public IModelRaw RawModel { get; protected set; }
        public Shader Shader { get; }

        public ModelBaked(IModelRaw rawModel, Shader shader)
        {
            RawModel = rawModel;
            Shader = shader;
        }

        public void Bind()
        {
            if (RawModel == null)
                return;

            Shader.Bind();

            GL.BindVertexArray(RawModel.VaoID);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);
        }

        public void Unbind()
        {
            if (RawModel == null)
                return;

            GL.BindVertexArray(0);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);

            Shader.Unbind();
        }

        public void Destroy()
        {
            Shader.Unbind();

            for (int index = 0; index < RawModel.BufferIDs.Length; index++)
            {
                int bufferId = RawModel.BufferIDs[index];

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.DisableVertexAttribArray(index);

                GL.DeleteBuffer(bufferId);
            }

            GL.BindVertexArray(0);
            GL.DeleteVertexArray(RawModel.VaoID);
        }
    }
}