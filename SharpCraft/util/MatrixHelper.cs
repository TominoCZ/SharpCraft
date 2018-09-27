using OpenTK;
using SharpCraft.world.chunk;
using System;

namespace SharpCraft.util
{
    internal class MatrixHelper
    {
        public static Matrix4 CreateTransformationMatrix(Vector3 translation, Vector3 rot, float scale)
        {
            Matrix4 x = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rot.X));
            Matrix4 y = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rot.Y));
            Matrix4 z = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rot.Z));

            Matrix4 s = Matrix4.CreateScale(scale);
            Matrix4 t = Matrix4.CreateTranslation(translation);

            return x * z * y * s * t;
        }

        public static Matrix4 CreateTransformationMatrix(Vector2 translation, Vector2 scale)
        {
            Matrix4 s = Matrix4.CreateScale(scale.X, scale.Y, 1);
            Matrix4 t = Matrix4.CreateTranslation(translation.X, translation.Y, 0);

            return s * t;
        }

        public static Matrix4 CreateTransformationMatrix(Vector3 translation, Vector3 scale)
        {
            Matrix4 s = Matrix4.CreateScale(scale.X, scale.Y, scale.Z);
            Matrix4 t = Matrix4.CreateTranslation(translation.X, translation.Y, translation.Z);

            return s * t;
        }

        public static Matrix4 CreateTransformationMatrix(ChunkPos translation, Vector3 scale)
        {
            Matrix4 s = Matrix4.CreateScale(scale.X, scale.Y, scale.Z);
            Matrix4 t = Matrix4.CreateTranslation(translation.WorldSpaceX(), 0, translation.WorldSpaceZ());

            return s * t;
        }

        public static Matrix4 CreateTransformationMatrix(ChunkPos translation)
        {
            return Matrix4.CreateTranslation(translation.x * Chunk.ChunkSize, 0, translation.z * Chunk.ChunkSize);
        }

        public static Matrix4 CreatePerspectiveFieldOfView(float fovy, float aspect, float zNear, float zFar)
        {
            float top = zNear * (float)Math.Tan(0.5 * (double)fovy);
            float bottom = -top;
            Matrix4.CreatePerspectiveOffCenter(bottom * aspect, top * aspect, bottom, top, zNear, zFar, out var result);

            return result;
        }
    }
}