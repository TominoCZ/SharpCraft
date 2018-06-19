using OpenTK;
using SharpCraft.render.shader.uniform;
using System;
using System.Diagnostics;

namespace SharpCraft.render.shader.shaders
{
    public class ShaderBlockOutline : Shader<object>
    {
        private UniformVec4 colorIn;

        private Vector4 _selectionOutlineColor;

        private readonly Stopwatch _updateTimer;

        private int _hue;

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
            colorIn?.Update(_selectionOutlineColor);
        }

        private Vector4 GetHue(int hue)
        {
            float rads = MathHelper.DegreesToRadians(hue);

            float r = (float)(Math.Sin(rads) * 0.5 + 0.5);
            float g = (float)(Math.Sin(rads + MathHelper.PiOver3 * 2) * 0.5 + 0.5);
            float b = (float)(Math.Sin(rads + MathHelper.PiOver3 * 4) * 0.5 + 0.5);

            return Vector4.UnitX * r + Vector4.UnitY * g + Vector4.UnitZ * b + Vector4.UnitW;
        }
    }
}