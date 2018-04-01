using System;
using OpenTK.Graphics.OpenGL;
using SharpCraft.block;
using SharpCraft.particle;
using SharpCraft.render.shader;

namespace SharpCraft.model
{
	public class ModelBaked<T> : IModelBaked<T>
	{
		public IModelRaw rawModel { get; protected set; }
		public Shader<T> shader   { get; }

		public ModelBaked(IModelRaw rawModel, Shader<T> shader)
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

			GL.EnableVertexAttribArray(0);
			GL.EnableVertexAttribArray(1);
			GL.EnableVertexAttribArray(2);
		}

		public void unbind()
		{
			if (rawModel == null)
				return;

			GL.BindVertexArray(0);

			GL.EnableVertexAttribArray(0);
			GL.EnableVertexAttribArray(1);
			GL.EnableVertexAttribArray(2);

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