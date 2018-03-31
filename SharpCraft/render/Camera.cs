using OpenTK;
using SharpCraft.util;
using System;

namespace SharpCraft.render
{
    public class Camera
    {
        private float _pitch = MathHelper.PiOver6, _yaw = MathHelper.PiOver2 + MathHelper.PiOver4;

        public Vector3 pos;

        //public static Camera INSTANCE;

        public float pitch
        {
            get => _pitch;

            set => _pitch = MathHelper.Clamp(value, -MathHelper.PiOver2, MathHelper.PiOver2);
        }

        public float yaw
        {
            get => _yaw;

            set => _yaw = value;
        }

        public Vector3 getLookVec()
        {
            return MathUtil.Rotate(Vector3.UnitZ, _pitch, _yaw, 0).Normalized();
        }

        public Vector2 left
        {
            get
            {
                var s = (float)Math.Sin(-(_yaw + MathHelper.PiOver2));
                var c = (float)Math.Cos((_yaw + MathHelper.PiOver2));

                return new Vector2(s, c).Normalized();
            }
        }

        public Vector2 forward
        {
            get
            {
                var s = -(float)Math.Sin(-_yaw);
                var c = -(float)Math.Cos(_yaw);

                return new Vector2(s, c).Normalized();
            }
        }

        public Matrix4 View
        {
            get
            {
                var x = Matrix4.CreateRotationX(pitch);
                var y = Matrix4.CreateRotationY(yaw);

                var t = Matrix4.CreateTranslation(-pos);

                return t * y * x;
            }
        }
    }
}