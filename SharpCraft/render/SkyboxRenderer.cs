using System;
using System.Collections.Concurrent;
using OpenTK.Graphics.OpenGL;
using SharpCraft.model;
using SharpCraft.texture;
using System.Collections.Generic;
using System.Numerics;
using SharpCraft.render.shader;
using SharpCraft.render.shader.shaders;
using SharpCraft.util;
using Vector3 = OpenTK.Vector3;

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

        private long tick;
        private long lastTick;

        public SkyboxRenderer()
        {
            var quads = new List<RawQuad>();

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

        public void Update()
        {
            lastTick = tick++;
        }

        public void Render(float partialTicks)
        {
            var partialRot = lastTick + (tick - lastTick) * partialTicks;

            var mat = MatrixHelper.CreateTransformationMatrix(SharpCraft.Instance.Camera.pos, Vector3.UnitY * partialRot / 10, 1);

            cube.Bind();
            cube.Shader.UpdateGlobalUniforms();
            cube.Shader.UpdateModelUniforms(cube.RawModel);
            cube.Shader.UpdateInstanceUniforms(mat, null);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, texture);
            cube.RawModel.Render(PrimitiveType.Triangles);

            cube.Unbind();
        }
    }
}