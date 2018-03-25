using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.IO;

namespace SharpCraft
{
    internal abstract class ShaderProgram
    {
        public PrimitiveType renderType;

        private Dictionary<string, int> uniforms;

        private int programID;

        private int vshID;
        private int fshID;

        private string shaderName;

        protected ShaderProgram(string shaderName, PrimitiveType renderType)
        {
            uniforms = new Dictionary<string, int>();

            this.renderType = renderType;
            this.shaderName = shaderName;

            init();

            registerUniforms("transformationMatrix", "projectionMatrix", "viewMatrix");

            onRegisterUniforms();

            ShaderManager.registerShader(this);
        }

        private void init()
        {
            loadShader(shaderName);

            //creates and ID for this program
            programID = GL.CreateProgram();

            //attaches shaders to this program
            GL.AttachShader(programID, vshID);
            GL.AttachShader(programID, fshID);

            onBindAttributes();

            GL.LinkProgram(programID);
            GL.ValidateProgram(programID);
        }

        protected abstract void onRegisterUniforms();

        protected abstract void onBindAttributes();

        protected void registerUniforms(params string[] variables)
        {
            foreach (var variable in variables)
            {
                var loc = GL.GetUniformLocation(programID, variable);

                if (loc > -1 && !uniforms.ContainsKey(variable))
                    uniforms.Add(variable, loc);
            }
        }

        protected void bindAttribute(int attrib, string variable)
        {
            //set variable names that the vertex shader will be taking in
            GL.BindAttribLocation(programID, attrib, variable);
        }

        public void loadVec3(Vector3 vec, string variable)
        {
            if (uniforms.TryGetValue(variable, out var pos))
                GL.Uniform3(pos, vec);
        }

        public void loadVec4(Vector4 vec, string variable)
        {
            if (uniforms.TryGetValue(variable, out var pos))
                GL.Uniform4(pos, vec);
        }

        public void loadMatrix4(Matrix4 mat, string variable)
        {
            if (uniforms.TryGetValue(variable, out var pos))
                GL.UniformMatrix4(pos, false, ref mat);
        }

        public void loadProjectionMatrix(Matrix4 mat)
        {
            loadMatrix4(mat, "projectionMatrix");
        }

        public void loadTransformationMatrix(Matrix4 mat)
        {
            loadMatrix4(mat, "transformationMatrix");
        }

        public virtual void loadViewMatrix(Matrix4 mat)
        {
            loadMatrix4(mat, "viewMatrix");
        }

        public void loadLight(ModelLight light)
        {
            loadVec3(light.pos, "lightPosition");
            loadVec3(light.color, "lightColor");
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
            GL.UseProgram(programID);
        }

        public void unbind()
        {
            GL.UseProgram(0);
        }

        public void destroy()
        {
            unbind();

            GL.DetachShader(programID, vshID);
            GL.DetachShader(programID, fshID);

            GL.DeleteShader(vshID);
            GL.DeleteShader(fshID);

            GL.DeleteProgram(programID);
        }
    }
}