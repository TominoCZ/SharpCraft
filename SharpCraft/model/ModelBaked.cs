using OpenTK.Graphics.OpenGL;

namespace SharpCraft
{
    internal class ModelBaked : IModelBaked
    {
        public IModelRaw rawModel { get; protected set; }
        public ShaderProgram shader { get; }

        public ModelBaked(IModelRaw rawModel, ShaderProgram shader)
        {
            this.rawModel = rawModel;
            this.shader = shader;
        }

        public void bind()
        {
            if (rawModel == null)
                return;

            shader.bind();

            GL.BindVertexArray(rawModel.vaoID);

            for (int i = 0; i < rawModel.bufferIDs.Length; i++)
            {
                GL.EnableVertexAttribArray(i);
            }
        }

        public void unbind()
        {
            if (rawModel == null)
                return;

            for (int i = 0; i < rawModel.bufferIDs.Length; i++)
            {
                GL.DisableVertexAttribArray(i);
            }

            GL.BindVertexArray(0);

            shader.unbind();
        }

        public void destroy()
        {
            shader.unbind();

            for (var index = 0; index < rawModel.bufferIDs.Length; index++)
            {
                var bufferId = rawModel.bufferIDs[index];

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.DisableVertexAttribArray(index);

                GL.DeleteBuffer(bufferId);
            }

            GL.BindVertexArray(0);
            GL.DeleteVertexArray(rawModel.vaoID);
        }
    }
}