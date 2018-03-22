using System;
using OpenTK;

namespace SharpCraft
{
    internal class MatrixHelper
    {
        public static Matrix4 createTransformationMatrixOrtho(Vector3 translation, Vector3 rot, float scale)
        {
            var x = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rot.X));
            var y = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rot.Y));
            var z = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rot.Z));

            var s = Matrix4.CreateScale(scale, scale, 0);
            var t = Matrix4.CreateTranslation(translation);

            return x * z * y * s * t;
        }

        public static Matrix4 createTransformationMatrix(Vector3 translation, Vector3 rot, float scale)
        {
            var x = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rot.X));
            var y = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rot.Y));
            var z = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rot.Z));

            var s = Matrix4.CreateScale(scale);
            var t = Matrix4.CreateTranslation(translation);

            return x * z * y * s * t;
        }

        public static Matrix4 createTransformationMatrix(Vector2 translation, Vector2 scale)
        {
            var s = Matrix4.CreateScale(scale.X, scale.Y, 1);
            var t = Matrix4.CreateTranslation(translation.X, translation.Y, 0);

            return s * t;
        }

        public static Matrix4 createTransformationMatrix(Vector3 translation, Vector3 scale)
        {
            var s = Matrix4.CreateScale(scale.X, scale.Y, scale.Z);
            var t = Matrix4.CreateTranslation(translation.X, translation.Y, translation.Z);

            return s * t;
        }

        public static Matrix4 createTransformationMatrix(Vector3 translation)
        {
            return Matrix4.CreateTranslation(translation);
        }

        public static Matrix4 createViewMatrix(Camera c)
        {
            var x = Matrix4.CreateRotationX(c.pitch);
            var y = Matrix4.CreateRotationY(c.yaw);

            var t = Matrix4.CreateTranslation(-c.pos);

            return t * y * x;
        }
    }
}