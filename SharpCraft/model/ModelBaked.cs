using OpenTK.Graphics.OpenGL;
using SharpCraft.render.shader;

namespace SharpCraft.model
{
    public class ModelBaked<T> : IModelBaked<T>
    {
        public IModelRaw RawModel { get; protected set; }
        public Shader<T> Shader { get; }

        public ModelBaked(IModelRaw rawModel, Shader<T> shader)
        {
            RawModel = rawModel;
            Shader = shader;
        }

        public void Bind()
        {
            if (RawModel == null)
                return;

            Shader.Bind();

            GL.BindVertexArray(RawModel.vaoID);

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

            for (int index = 0; index < RawModel.bufferIDs.Length; index++)
            {
                int bufferId = RawModel.bufferIDs[index];

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.DisableVertexAttribArray(index);

                GL.DeleteBuffer(bufferId);
            }

            GL.BindVertexArray(0);
            GL.DeleteVertexArray(RawModel.vaoID);
        }
    }
}