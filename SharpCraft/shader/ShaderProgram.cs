using System.IO;
using OpenTK.Graphics.OpenGL;

namespace OpenG_Basics
{
    class ShaderProgram
    {
        private int programID;

        private int vshID;
        private int fshID;

        public ShaderProgram(string shaderName)
        {
            loadShader(shaderName);

            programID = GL.CreateProgram();

            GL.AttachShader(programID, vshID);
            GL.AttachShader(programID, fshID);

            //bindAttributes();

            GL.LinkProgram(programID);
            GL.ValidateProgram(programID);

            //getAllUniformLocations();

            //ShaderManager.registerShader(this);
        }

        private void loadShader(string shaderName)
        {
            string vshCode = File.ReadAllText($"assets/shader/{shaderName}.vsh");
            string fshCode = File.ReadAllText($"assets/shader/{shaderName}.fsh");

            vshID = GL.CreateShader(ShaderType.VertexShader);
            fshID = GL.CreateShader(ShaderType.FragmentShader);

            GL.ShaderSource(vshID, vshCode);
            GL.ShaderSource(fshID, fshCode);

            GL.CompileShader(vshID);
            GL.CompileShader(fshID);
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