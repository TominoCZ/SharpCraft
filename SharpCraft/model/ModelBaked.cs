using OpenTK.Graphics.OpenGL;
using SharpCraft_Client.render.shader;

namespace SharpCraft_Client.model
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

            for (int i = 0; i < RawModel.BufferIDs.Length; i++)
            {
                GL.EnableVertexAttribArray(i);
            }
        }

        public void Unbind()
        {
            if (RawModel == null)
                return;

            GL.BindVertexArray(0);

            for (int i = 0; i < RawModel.BufferIDs.Length; i++)
            {
                GL.DisableVertexAttribArray(i);
            }

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