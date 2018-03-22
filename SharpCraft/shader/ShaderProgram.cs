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

<<<<<<< HEAD
            programID = GL.CreateProgram();

=======
            //creates and ID for this program
            programID = GL.CreateProgram();

            //attaches shaders to this program
>>>>>>> a8d82a55a6dfe057e93c4777ec3a884304d4c636
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
<<<<<<< HEAD
            string vshCode = File.ReadAllText($"assets/shader/{shaderName}.vsh");
            string fshCode = File.ReadAllText($"assets/shader/{shaderName}.fsh");

            vshID = GL.CreateShader(ShaderType.VertexShader);
            fshID = GL.CreateShader(ShaderType.FragmentShader);

            GL.ShaderSource(vshID, vshCode);
            GL.ShaderSource(fshID, fshCode);

=======
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
>>>>>>> a8d82a55a6dfe057e93c4777ec3a884304d4c636
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
<<<<<<< HEAD

=======
            
>>>>>>> a8d82a55a6dfe057e93c4777ec3a884304d4c636
            GL.DeleteShader(vshID);
            GL.DeleteShader(fshID);

            GL.DeleteProgram(programID);
        }
    }
}