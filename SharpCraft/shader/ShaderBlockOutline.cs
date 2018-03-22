using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SharpCraft
{
    class ShaderBlockOutline : ShaderBlockUnlit
    {
        public ShaderBlockOutline() : base("color", PrimitiveType.Quads)
        {

        }

        protected override void onBindAttributes()
        {
            bindAttribute(0, "position");
        }

        protected override void onRegisterUniforms()
        {
            registerUniforms("colorIn");
        }
    }
}