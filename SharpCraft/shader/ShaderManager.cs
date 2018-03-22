using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace SharpCraft
{
    static class ShaderManager
    {
        private static List<ShaderProgram> shaders = new List<ShaderProgram>();

        private static Matrix4 projectionMatrix;

        [Obsolete]
        public static void registerShader(ShaderProgram shader)
        {
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

        public static void cleanup()
        {
            for (int i = 0; i < shaders.Count; i++)
            {
                shaders[i].destroy();
            }
        }
    }
}
