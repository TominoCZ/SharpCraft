using OpenTK;
using System;
using SharpCraft.util;

#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand

namespace SharpCraft.render
{
    public class Camera
    {
        public const float NearPlane = 0.1f;
        public const float FarPlane = 1000f;

        public float TargetFov { get; }

        public float PartialFov { get; private set; }

        private float _pitch;
        private float _yaw = MathHelper.PiOver2;

        public Vector3 Pos;

        public float PitchOffset;

        public float Pitch
        {
            get => _pitch;

            set => _pitch = MathHelper.Clamp(value, -MathHelper.PiOver2 + MathHelper.DegreesToRadians(0.1f), MathHelper.PiOver2 - MathHelper.DegreesToRadians(0.1f));
        }

        public float Yaw
        {
            get => _yaw;

            set => _yaw = value;
        }

        public Camera()
        {
            PartialFov = TargetFov = 70;

            UpdateViewMatrix();
            UpdateProjectionMatrix();
        }

        public void SetFov(float fov)
        {
            PartialFov = fov;

            UpdateProjectionMatrix();
        }

        public void UpdateViewMatrix()
        {
            Matrix4 x = Matrix4.CreateRotationX(Pitch + PitchOffset);
            Matrix4 y = Matrix4.CreateRotationY(Yaw);

            Matrix4 t = Matrix4.CreateTranslation(-Pos);

            View = t * y * x;
        }

        public void UpdateProjectionMatrix()
        {
            float aspectRatio = (float)SharpCraft.Instance.Width / SharpCraft.Instance.Height;

            Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(PartialFov), aspectRatio, NearPlane, FarPlane);
        }

        public Vector3 GetLookVec()
        {
            return MathUtil.Rotate(Vector3.UnitX, -_pitch, -_yaw + MathHelper.PiOver2, 0);
        }

        public Vector2 Left
        {
            get
            {
                float s = (float)Math.Sin(-(_yaw + MathHelper.PiOver2));
                float c = (float)Math.Cos(_yaw + MathHelper.PiOver2);

                return new Vector2(s, c).Normalized();
            }
        }

        public Vector2 Forward
        {
            get
            {
                float s = -(float)Math.Sin(-_yaw);
                float c = -(float)Math.Cos(_yaw);

                return new Vector2(s, c).Normalized();
            }
        }

        public Matrix4 View { get; private set; }
        public Matrix4 Projection { get; private set; }
    }
}