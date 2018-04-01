using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.model;
using SharpCraft.render.shader.module;
using SharpCraft.render.shader.uniform;

namespace SharpCraft.render.shader
{
	public class Shader<TRenderable>
	{
		public static List<Shader<TRenderable>> list=new List<Shader<TRenderable>>();

		private List<ShaderModule<TRenderable>> _modules = new List<ShaderModule<TRenderable>>();

		private int program;

		private int vshID;
		private int fshID;

		private string shaderName;

		public Shader(string shaderName)
		{
			this.shaderName = shaderName;

			_modules.Add(new ShaderModule3D<TRenderable>(this));//todo auto detect

			init();
			list.Add(this);
		}

		private void init()
		{
			loadShader(shaderName);

			//creates and ID for this program
			program = GL.CreateProgram();

			//attaches shaders to this program
			GL.AttachShader(program, vshID);
			GL.AttachShader(program, fshID);

			bindAttributes();

			GL.LinkProgram(program);
			GL.ValidateProgram(program);

			registerUniforms();
		}


		protected void bindAttributes()
		{
			bindAttribute(0, "position");
			bindAttribute(1, "textureCoords");
			bindAttribute(2, "normal");
		}

		protected virtual void registerUniforms()
		{
			foreach (var m in _modules)
			{
				m.InitUniforms();
			}
		}

		public virtual void UpdateGlobalUniforms()
		{
			foreach (var m in _modules)
			{
				m.UpdateGlobalUniforms();
			}
		}

		public virtual void UpdateModelUniforms(IModelRaw model)
		{
			foreach (var m in _modules)
			{
				m.UpdateModelUniforms(model);
			}
		}

		public virtual void UpdateInstanceUniforms(Matrix4 transform, TRenderable renderable)
		{
			foreach (var m in _modules)
			{
				m.UpdateInstanceUniforms(transform,  renderable);
			}
		}

		protected void bindAttribute(int attrib, string variable)
		{
			GL.BindAttribLocation(program, attrib, variable);
		}

		private void loadShader(string shaderName)
		{
			//vertex and fragment shader code
			string vshCode = File.ReadAllText($"SharpCraft_Data/assets/shaders/{shaderName}.vsh");
			string fshCode = File.ReadAllText($"SharpCraft_Data/assets/shaders/{shaderName}.fsh");

			//create IDs for shaders
			vshID = GL.CreateShader(ShaderType.VertexShader);
			fshID = GL.CreateShader(ShaderType.FragmentShader);

			//load shader codes into memory
			GL.ShaderSource(vshID, vshCode);
			GL.ShaderSource(fshID, fshCode);

			//compile shaders
			GL.CompileShader(vshID);
			GL.CompileShader(fshID);
		}


		public void reload()
		{
			destroy();

			init();
		}

		public void bind()
		{
			GL.UseProgram(program);
		}

		public void unbind()
		{
			GL.UseProgram(0);
		}

		public void destroy()
		{
			unbind();

			GL.DetachShader(program, vshID);
			GL.DetachShader(program, fshID);

			GL.DeleteShader(vshID);
			GL.DeleteShader(fshID);

			GL.DeleteProgram(program);
		}

		public int GetUniformId(string name)
		{
			int id = GL.GetUniformLocation(program, name);
			if (id == -1) throw new ArgumentException($"Uniform {name} does not exist!");
			return id;
		}
	}
}