using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SharpCraft
{
    class ShaderBlockUnlit : ShaderProgram
    {
        public ShaderBlockUnlit(string shaderName, PrimitiveType renderType) : base(shaderName, renderType)
        {

        }

        protected override void onBindAttributes()
        {
            bindAttribute(0, "position");
            bindAttribute(1, "textureCoords");
        }

        protected override void onRegisterUniforms()
        {

        }
    }
}
