using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;

namespace SharpCraft_Client.render.shader
{
    public sealed class Shader
    {
        private static readonly List<Shader> Shaders = new List<Shader>();

        private int _vsh;
        private int _fsh;

        private int _program;
        private bool _registered;
        public readonly string ShaderName;

        private readonly Dictionary<string, int> _uniforms = new Dictionary<string, int>();

        public Shader(string shaderName, params string[] uniforms)
        {
            ShaderName = shaderName;

            Init();

            RegisterUniformsSilent("transformationMatrix", "projectionMatrix", "viewMatrix");
            RegisterUniforms(uniforms);

            Shaders.Add(this);
        }

        private void Init()
        {
            LoadShader(ShaderName);

            //creates and ID for this program
            _program = GL.CreateProgram();

            //attaches shaders to this program
            GL.AttachShader(_program, _vsh);
            GL.AttachShader(_program, _fsh);

            BindAttributes();

            GL.LinkProgram(_program);
            GL.ValidateProgram(_program);
        }

        private void BindAttributes()
        {
            BindAttribute(0, "position");
            BindAttribute(1, "textureCoords");
            BindAttribute(2, "normal");
        }

        private int GetUniformLocation(string uniform)
        {
            if (_uniforms.TryGetValue(uniform, out var loc))
                return loc;

            return -1;
        }

        /*
        protected void BindAttributes()
        {
        }

        protected void BindAttribute(int attrib, string variable)
        {
            GL.BindAttribLocation(_program, attrib, variable);
        }*/

        private void RegisterUniforms(params string[] uniforms)
        {
            if (_registered)
                throw new Exception("Can't register uniforms twice, they need to be registered only once.");

            _registered = true;

            Bind();
            foreach (var uniform in uniforms)
            {
                if (_uniforms.ContainsKey(uniform))
                {
                    Console.WriteLine($"Attemted to register uniform '{uniform}' in shader '{ShaderName}' twice");
                    continue;
                }

                var loc = GL.GetUniformLocation(_program, uniform);

                if (loc == -1)
                {
                    Console.WriteLine($"Could not find uniform '{uniform}' in shader '{ShaderName}'");
                    continue;
                }

                _uniforms.Add(uniform, loc);
            }
            Unbind();
        }

        private void RegisterUniformsSilent(params string[] uniforms)
        {
            Bind();
            foreach (var uniform in uniforms)
            {
                if (_uniforms.ContainsKey(uniform))
                    continue;

                var loc = GL.GetUniformLocation(_program, uniform);

                if (loc == -1)
                    continue;

                _uniforms.Add(uniform, loc);
            }
            Unbind();
        }

        public void SetFloat(string uniform, float f)
        {
            var loc = GetUniformLocation(uniform);

            if (loc != -1)
                GL.Uniform1(loc, f);
        }

        public void SetVector2(string uniform, Vector2 vec)
        {
            var loc = GetUniformLocation(uniform);

            if (loc != -1)
                GL.Uniform2(loc, vec);
        }

        public void SetVector3(string uniform, Vector3 vec)
        {
            var loc = GetUniformLocation(uniform);

            if (loc != -1)
                GL.Uniform3(loc, vec);
        }

        public void SetVector4(string uniform, Vector4 vec)
        {
            var loc = GetUniformLocation(uniform);

            if (loc != -1)
                GL.Uniform4(loc, vec);
        }

        public void SetMatrix4(string uniform, Matrix4 mat)
        {
            var loc = GetUniformLocation(uniform);

            if (loc != -1)
                GL.UniformMatrix4(loc, false, ref mat);
        }

        public static void SetProjectionMatrix(Matrix4 mat)
        {
            for (var index = 0; index < Shaders.Count; index++)
            {
                var shader = Shaders[index];
                shader.Bind();
                shader.SetMatrix4("projectionMatrix", mat);
                shader.Unbind();
            }
        }

        public static void SetViewMatrix(Matrix4 mat)
        {
            for (var index = 0; index < Shaders.Count; index++)
            {
                var shader = Shaders[index];
                shader.Bind();
                shader.SetMatrix4("viewMatrix", mat);
                shader.Unbind();
            }
        }

        private void BindAttribute(int attrib, string variable)
        {
            GL.BindAttribLocation(_program, attrib, variable);
        }

        private void LoadShader(string shaderName)
        {
            //vertex and fragment shader code
            string vshCode = File.ReadAllText($"{SharpCraft.Instance.GameFolderDir}/assets/sharpcraft/shaders/{shaderName}.vsh");
            string fshCode = File.ReadAllText($"{SharpCraft.Instance.GameFolderDir}/assets/sharpcraft/shaders/{shaderName}.fsh");

            //create IDs for shaders
            _vsh = GL.CreateShader(ShaderType.VertexShader);
            _fsh = GL.CreateShader(ShaderType.FragmentShader);

            //load shader codes into memory
            GL.ShaderSource(_vsh, vshCode);
            GL.ShaderSource(_fsh, fshCode);

            //compile shaders
            GL.CompileShader(_vsh);
            GL.CompileShader(_fsh);
        }

        public void Bind()
        {
            GL.UseProgram(_program);
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

            GL.DetachShader(_program, _vsh);
            GL.DetachShader(_program, _fsh);

            GL.DeleteShader(_vsh);
            GL.DeleteShader(_fsh);

            GL.DeleteProgram(_program);
        }

        public static void ReloadAll()
        {
            for (int index = 0; index < Shaders.Count; index++)
            {
                var shader = Shaders[index];

                shader.Reload(); //WARNING: keep this name of the function the same
            }
        }

        public static void DestroyAll()
        {
            for (int index = 0; index < Shaders.Count; index++)
            {
                var shader = Shaders[index];

                shader.Destroy(); //keep this name of the function the same
            }
        }

        public Shader Reloaded()
        {
            Reload();
            return this;
        }
    }
}