using System.IO;
using OpenTK.Graphics.OpenGL;

namespace OpenG_Basics
{
    abstract class ShaderProgram
    {
        private int programID;

        private int vshID;
        private int fshID;

        protected ShaderProgram(string shaderName)
        {
            loadShader(shaderName);

            //creates and ID for this program
            programID = GL.CreateProgram();

            //attaches shaders to this program
            GL.AttachShader(programID, vshID);
            GL.AttachShader(programID, fshID);

            bindAttributes();

            GL.LinkProgram(programID);
            GL.ValidateProgram(programID);

            //getAllUniformLocations();
            //ShaderManager.registerShader(this);
        }

        protected abstract void bindAttributes();

        protected void bindAttribute(int attrib, string variable)
        {
            //set variable names that the vertex shader will be taking in
            GL.BindAttribLocation(programID, attrib, variable);
        }

        private void loadShader(string shaderName)
        {
            //vertex and fragment shader code
            string vshCode = File.ReadAllText($"assets/shader/{shaderName}.vsh");
            string fshCode = File.ReadAllText($"assets/shader/{shaderName}.fsh");

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