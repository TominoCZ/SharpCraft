using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.model;
using SharpCraft.render.shader;
using SharpCraft.texture;
using SharpCraft.util;
using System.Collections.Generic;
using Vector3 = OpenTK.Vector3;

namespace SharpCraft.render
{
    internal class SkyboxRenderer
    {
        private static readonly float SIZE = 500f;

        private static readonly float[] _vertexes = {
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

        private readonly ModelBaked<object> cube;
        private readonly int texture;

        private long tick;
        private long lastTick;

        public SkyboxRenderer()
        {
            cube = new ModelBaked<object>(ModelManager.LoadModel3ToVao(_vertexes), new Shader<object>("skybox"));
            texture = TextureManager.LoadCubeMap();
        }

        public void Update()
        {
            lastTick = tick++;
        }

        public void Render(float partialTicks)
        {
            float partialRot = lastTick + (tick - lastTick) * partialTicks;

            Matrix4 mat = MatrixHelper.CreateTransformationMatrix(SharpCraft.Instance.Camera.pos, Vector3.UnitY * partialRot / 10, 1);

            cube.Bind();
            cube.Shader.UpdateGlobalUniforms();
            cube.Shader.UpdateModelUniforms(cube.RawModel);
            cube.Shader.UpdateInstanceUniforms(mat, null);

            GL.BindTexture(TextureTarget.TextureCubeMap, texture);
            cube.RawModel.Render(PrimitiveType.Triangles);

            cube.Unbind();
        }
    }
}