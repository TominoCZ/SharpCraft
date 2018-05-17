using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.model;
using SharpCraft.render.shader.module;
using SharpCraft.render.shader.uniform;
using System.Collections.Generic;
using System.IO;

namespace SharpCraft.render.shader
{
    public class Shader
    {
        private static List<dynamic> _shaders = new List<dynamic>();

        public static void Register(dynamic shader)
        {
            _shaders.Add(shader);
        }

        public static void ReloadAll()
        {
            for (var index = 0; index < _shaders.Count; index++)
            {
                var shader = _shaders[index];

                shader.Reload(); //TODO WARNING: keep this name of the function the same
            }
        }

        public static void DestroyAll()
        {
            for (var index = 0; index < _shaders.Count; index++)
            {
                var shader = _shaders[index];

                shader.Destroy(); //TODO WARNING: keep this name of the function the same
            }
        }
    }

    public class Shader<TRenderable>
    {
        private List<ShaderModule<TRenderable>> _modules = new List<ShaderModule<TRenderable>>();

        private int program;

        private int vshID;
        private int fshID;

        private readonly string shaderName;

        public Shader(string shaderName)
        {
            this.shaderName = shaderName;

            _modules.Add(new ShaderModule3D<TRenderable>(this));//todo auto detect

            Init();
            Shader.Register(this);
        }

        private void Init()
        {
            LoadShader(shaderName);

            //creates and ID for this program
            program = GL.CreateProgram();

            //attaches shaders to this program
            GL.AttachShader(program, vshID);
            GL.AttachShader(program, fshID);

            BindAttributes();

            GL.LinkProgram(program);
            GL.ValidateProgram(program);

            RegisterUniforms();
        }

        protected void BindAttributes()
        {
            BindAttribute(0, "position");
            BindAttribute(1, "textureCoords");
            BindAttribute(2, "normal");
        }

        protected virtual void RegisterUniforms()
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

        public virtual void UpdateModelUniforms(IModelRaw model = null)
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
                m.UpdateInstanceUniforms(transform, renderable);
            }
        }

        protected void BindAttribute(int attrib, string variable)
        {
            GL.BindAttribLocation(program, attrib, variable);
        }

        private void LoadShader(string shaderName)
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

        public void Bind()
        {
            GL.UseProgram(program);
        }

        public void Unbind()
        {
            GL.UseProgram(0);
        }

        public void Reload()
        {
            Destroy();

            Init();
        }

        public void Destroy()
        {
            Unbind();

            GL.DetachShader(program, vshID);
            GL.DetachShader(program, fshID);

            GL.DeleteShader(vshID);
            GL.DeleteShader(fshID);

            GL.DeleteProgram(program);
        }

        private int GetUniformId(string name)
        {
            return GL.GetUniformLocation(program, name);
        }

        public UniformMat4 GetUniformMat4(string name)
        {
            var id = GetUniformId(name);

            return id != -1 ? new UniformMat4(id) : null;
        }

        public UniformVec4 GetUniformVec4(string name)
        {
            var id = GetUniformId(name);

            return id != -1 ? new UniformVec4(id) : null;
        }

        public UniformVec3 GetUniformVec3(string name)
        {
            var id = GetUniformId(name);

            return id != -1 ? new UniformVec3(id) : null;
        }

        public UniformVec2 GetUniformVec2(string name)
        {
            var id = GetUniformId(name);

            return id != -1 ? new UniformVec2(id) : null;
        }

        public UniformFloat GetUniformFloat(string name)
        {
            var id = GetUniformId(name);

            return id != -1 ? new UniformFloat(id) : null;
        }
    }
}