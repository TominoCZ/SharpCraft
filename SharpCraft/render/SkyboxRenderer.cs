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
        private static ModelBaked<object> _cube;
        private static int _texture;

        private long _tick;
        private long _lastTick;

        public SkyboxRenderer()
        {
            if (_cube == null)
            {
                var size = 500f;

                _cube = new ModelBaked<object>(ModelManager.LoadModel3ToVao(new[] {
                    -size,  size, -size,
                    -size, -size, -size,
                    size, -size, -size,
                    size, -size, -size,
                    size,  size, -size,
                    -size,  size, -size,

                    -size, -size,  size,
                    -size, -size, -size,
                    -size,  size, -size,
                    -size,  size, -size,
                    -size,  size,  size,
                    -size, -size,  size,

                    size, -size, -size,
                    size, -size,  size,
                    size,  size,  size,
                    size,  size,  size,
                    size,  size, -size,
                    size, -size, -size,

                    -size, -size,  size,
                    -size,  size,  size,
                    size,  size,  size,
                    size,  size,  size,
                    size, -size,  size,
                    -size, -size,  size,

                    -size,  size, -size,
                    size,  size, -size,
                    size,  size,  size,
                    size,  size,  size,
                    -size,  size,  size,
                    -size,  size, -size,

                    -size, -size, -size,
                    -size, -size,  size,
                    size, -size, -size,
                    size, -size, -size,
                    -size, -size,  size,
                    size, -size,  size
                }), new Shader<object>("skybox"));

                _texture = TextureManager.LoadCubeMap();
            }
        }

        public void Update()
        {
            _lastTick = _tick++;
        }

        public void Render(float partialTicks)
        {
            GL.DepthMask(false);
            float partialRot = _lastTick + (_tick - _lastTick) * partialTicks;

            Matrix4 mat = MatrixHelper.CreateTransformationMatrix(SharpCraft.Instance.Camera.Pos, Vector3.UnitY * partialRot / 10, 1);

            _cube.Bind();
            _cube.Shader.UpdateGlobalUniforms();
            _cube.Shader.UpdateModelUniforms(_cube.RawModel);
            _cube.Shader.UpdateInstanceUniforms(mat, null);

            GL.BindTexture(TextureTarget.TextureCubeMap, _texture);
            _cube.RawModel.Render();

            _cube.Unbind();
            GL.DepthMask(true);
        }
    }
}