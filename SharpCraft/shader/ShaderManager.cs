using OpenTK;
using System.Collections.Generic;

namespace SharpCraft
{
    internal static class ShaderManager
    {
        private static List<ShaderProgram> shaders = new List<ShaderProgram>();

        private static Matrix4 projectionMatrix;

        public static void registerShader(ShaderProgram shader)
        {
            if (shaders.Contains(shader))
                return;

            shader.bind();
            shader.loadProjectionMatrix(projectionMatrix);
            shader.unbind();

            shaders.Add(shader);
        }

        public static void updateProjectionMatrix()
        {
            projectionMatrix = Game.INSTANCE.createProjectionMatrix();

            for (int i = 0; i < shaders.Count; i++)
            {
                var shader = shaders[i];

                shader.bind();
                shader.loadProjectionMatrix(projectionMatrix);
                shader.unbind();
            }
        }

        public static void reload()
        {
            for (int i = 0; i < shaders.Count; i++)
            {
                var shader = shaders[i];

                shader.reload();
            }

            updateProjectionMatrix();
        }

        public static void cleanup()
        {
            for (int i = 0; i < shaders.Count; i++)
            {
                shaders[i].destroy();
            }
        }
    }
}