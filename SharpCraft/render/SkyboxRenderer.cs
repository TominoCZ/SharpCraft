using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.model;
using SharpCraft.texture;
using System.Collections.Generic;
using SharpCraft.render.shader;
using SharpCraft.render.shader.shaders;

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

        private ModelBaked<object> cube;

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

            cube = new ModelBaked<object>(ModelManager.loadModelToVAO(quads, 3), new Shader<object>("skybox"));
            texture = TextureManager.loadCubeMap();
        }

        public void Render()
        {
            var mat = SharpCraft.Instance.Camera.View;
            mat.Column3 *= Vector4.UnitW;//viewMatrix.M41 = viewMatrix.M42 = viewMatrix.M43 = 0;

            cube.Bind();
	        cube.Shader.UpdateGlobalUniforms();
	        cube.Shader.UpdateModelUniforms(cube.RawModel);
	        cube.Shader.UpdateInstanceUniforms(mat,null);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, texture);
            cube.RawModel.Render(PrimitiveType.Triangles);

            cube.Unbind();
        }
    }
}