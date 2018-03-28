using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.model;
using SharpCraft.shader;
using SharpCraft.texture;

namespace SharpCraft.render
{
    internal class SkyboxRenderer
    {
        private static float SIZE = 500f;

        private static float[] VERTICES = {
            -SIZE,  SIZE, -SIZE,
            -SIZE, -SIZE, -SIZE,
            SIZE, -SIZE, -SIZE,
            SIZE, -SIZE, -SIZE,
            SIZE,  SIZE, -SIZE,
            -SIZE,  SIZE, -SIZE,

            -SIZE, -SIZE,  SIZE,
            -SIZE, -SIZE, -SIZE,
            -SIZE,  SIZE, -SIZE,
            -SIZE,  SIZE, -SIZE,
            -SIZE,  SIZE,  SIZE,
            -SIZE, -SIZE,  SIZE,

            SIZE, -SIZE, -SIZE,
            SIZE, -SIZE,  SIZE,
            SIZE,  SIZE,  SIZE,
            SIZE,  SIZE,  SIZE,
            SIZE,  SIZE, -SIZE,
            SIZE, -SIZE, -SIZE,

            -SIZE, -SIZE,  SIZE,
            -SIZE,  SIZE,  SIZE,
            SIZE,  SIZE,  SIZE,
            SIZE,  SIZE,  SIZE,
            SIZE, -SIZE,  SIZE,
            -SIZE, -SIZE,  SIZE,

            -SIZE,  SIZE, -SIZE,
            SIZE,  SIZE, -SIZE,
            SIZE,  SIZE,  SIZE,
            SIZE,  SIZE,  SIZE,
            -SIZE,  SIZE,  SIZE,
            -SIZE,  SIZE, -SIZE,

            -SIZE, -SIZE, -SIZE,
            -SIZE, -SIZE,  SIZE,
            SIZE, -SIZE, -SIZE,
            SIZE, -SIZE, -SIZE,
            -SIZE, -SIZE,  SIZE,
            SIZE, -SIZE,  SIZE
        };

        private ModelBaked cube;

        private int texture;

        public SkyboxRenderer()
        {
            List<RawQuad> quads = new List<RawQuad>();

            for (int i = 0; i < VERTICES.Length; i += 18)
            {
                float[] vertices = new float[18];

                for (int j = 0; j < 18; j++)
                {
                    vertices[j] = VERTICES[i + j];
                }

                quads.Add(new RawQuad(vertices, 3));
            }

            var shader = new SkyboxShader();

            cube = new ModelBaked(ModelManager.loadModelToVAO(quads, 3), shader);
            texture = TextureManager.loadCubeMap();
        }

        public void render(Matrix4 viewMatrix)
        {
            cube.bind();
            cube.shader.loadViewMatrix(viewMatrix);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, texture);
	        cube.rawModel.Render(cube.shader.renderType);

            cube.unbind();
        }
    }
}