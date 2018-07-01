using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.model;
using SharpCraft.render.shader;
using SharpCraft.texture;
using SharpCraft.util;
using Vector3 = OpenTK.Vector3;

namespace SharpCraft.render
{
    internal class SkyboxRenderer
    {
        private static ModelBaked<object> cube;
        private static int texture;

        private long tick;
        private long lastTick;

        public SkyboxRenderer()
        {
            if (cube == null)
            {
                var SIZE = 500f;

                cube = new ModelBaked<object>(ModelManager.LoadModel3ToVao(new[] {
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
                }), new Shader<object>("skybox"));

                texture = TextureManager.LoadCubeMap();
            }
        }

        public void Update()
        {
            lastTick = tick++;
        }

        public void Render(float partialTicks)
        {
            float partialRot = lastTick + (tick - lastTick) * partialTicks;

            Matrix4 mat = MatrixHelper.CreateTransformationMatrix(SharpCraft.Instance.Camera.Pos, Vector3.UnitY * partialRot / 10, 1);

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