using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SharpCraft
{
    class SkyboxShader : ShaderProgram
    {
        public SkyboxShader() : base("skybox", PrimitiveType.Triangles)
        {

        }

        protected override void onBindAttributes()
        {
            bindAttribute(0, "position");
        }

        protected override void onRegisterUniforms()
        {

        }

        public override void loadViewMatrix(Matrix4 mat)
        {
            var m = mat;

            m.M41 = 0;
            m.M42 = 0;
            m.M43 = 0;

            base.loadViewMatrix(m);
        }
    }
}
