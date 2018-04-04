using System;
using System.Diagnostics;
using OpenTK;
using SharpCraft.render.shader.uniform;

namespace SharpCraft.render.shader.shaders
{
    public class ShaderBlockOutline : Shader<object>
    {
        private UniformVec4 colorIn;

        private Vector4 _selectionOutlineColor;

        private Stopwatch _updateTimer;
        int _hue;

        public ShaderBlockOutline(string shaderName) : base(shaderName)
        {
            _updateTimer = Stopwatch.StartNew();
        }

        protected override void RegisterUniforms()
        {
            base.RegisterUniforms();
            colorIn = GetUniformVec4("colorIn");
        }

        public override void UpdateInstanceUniforms(Matrix4 transform, object renderable)
        {
            if (_updateTimer.ElapsedMilliseconds >= 50)
            {
                _hue = (_hue + 5) % 365;

                _selectionOutlineColor = GetHue(_hue);

                _updateTimer.Restart();
            }

            base.UpdateInstanceUniforms(transform, renderable);
            colorIn.Update(_selectionOutlineColor);
        }


        private Vector4 GetHue(int hue)
        {
            var rads = MathHelper.DegreesToRadians(hue);

            var r = (float)(Math.Sin(rads) * 0.5 + 0.5);
            var g = (float)(Math.Sin(rads + MathHelper.PiOver3 * 2) * 0.5 + 0.5);
            var b = (float)(Math.Sin(rads + MathHelper.PiOver3 * 4) * 0.5 + 0.5);

            return Vector4.UnitX * r + Vector4.UnitY * g + Vector4.UnitZ * b + Vector4.UnitW;
        }
    }
}