using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft_Client.model;
using SharpCraft_Client.render.shader;
using SharpCraft_Client.texture;
using SharpCraft_Client.util;
using Vector3 = OpenTK.Vector3;

namespace SharpCraft_Client.render
{
    internal class SkyboxRenderer
    {
        private static ModelBaked _cube;
        private static int _texture;

        private long _tick;
        private long _lastTick;

        public SkyboxRenderer()
        {
            if (_cube == null)
            {
                var size = 500f;

                _cube = new ModelBaked(ModelManager.LoadModel3ToVao(new[] {
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
                }), new Shader("skybox"));

                _texture = TextureManager.LoadCubeMap();
            }
        }

        public void Update()
        {
            _lastTick = _tick++;
        }

        public void Render(float partialTicks)
        {
            float partialRot = _lastTick + (_tick - _lastTick) * partialTicks;

            Matrix4 mat = MatrixHelper.CreateTransformationMatrix(SharpCraft.Instance.Camera.Pos, Vector3.UnitY * partialRot / 10, 1);

            _cube.Bind();
            _cube.Shader.SetMatrix4("transformationMatrix", mat);

            GL.BindTexture(TextureTarget.TextureCubeMap, _texture);

            _cube.RawModel.Render();

            _cube.Unbind();
        }
    }
}