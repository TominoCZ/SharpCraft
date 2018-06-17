using OpenTK;
using SharpCraft.util;
using System;

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

        public Vector3 pos;

        public float pitchOffset;

        public float pitch
        {
            get => _pitch;

            set => _pitch = MathHelper.Clamp(value, -MathHelper.PiOver2 + MathHelper.DegreesToRadians(0.1f), MathHelper.PiOver2 - MathHelper.DegreesToRadians(0.1f));
        }

        public float yaw
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

        public void SetFOV(float fov)
        {
            PartialFov = fov;

            UpdateProjectionMatrix();
        }

        public void UpdateViewMatrix()
        {
            Matrix4 x = Matrix4.CreateRotationX(pitch + pitchOffset);
            Matrix4 y = Matrix4.CreateRotationY(yaw);

            Matrix4 t = Matrix4.CreateTranslation(-pos);

            View = t * y * x;
        }

        public void UpdateProjectionMatrix()
        {
            //var matrix = Matrix4.Identity;

            float aspectRatio = (float)SharpCraft.Instance.Width / SharpCraft.Instance.Height;
            /*var yScale = (float)(1f / Math.Tan(MathHelper.DegreesToRadians(PartialFov / 2f)));
            var xScale = yScale / aspectRatio;
            var frustumLength = FarPlane - NearPlane;

            matrix.M11 = xScale;
            matrix.M22 = yScale;
            matrix.M33 = -((FarPlane + NearPlane) / frustumLength);
            matrix.M34 = -1;
            matrix.M43 = -(2 * NearPlane * FarPlane / frustumLength);
            matrix.M44 = 0;

            Projection = matrix;*/

            Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(PartialFov), aspectRatio,
                NearPlane, FarPlane);
        }

        public Vector3 GetLookVec()
        {
            return MathUtil.Rotate(Vector3.UnitZ, pitch, _yaw, 0).Normalized();
        }

        public Vector2 left
        {
            get
            {
                float s = (float)Math.Sin(-(_yaw + MathHelper.PiOver2));
                float c = (float)Math.Cos(_yaw + MathHelper.PiOver2);

                return new Vector2(s, c).Normalized();
            }
        }

        public Vector2 forward
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