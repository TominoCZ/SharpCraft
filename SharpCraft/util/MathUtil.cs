using System;
using OpenTK;

namespace SharpCraft.util
{
    internal class MathUtil
    {
        public static Vector3 Rotate(Vector3 vec, float angleX, float angleY, float angleZ)
        {
            var sin = new Vector3((float)Math.Sin(angleX), (float)Math.Sin(angleY), (float)Math.Sin(angleZ));
            var cos = new Vector3((float)Math.Cos(angleX), (float)Math.Cos(angleY), (float)Math.Cos(angleZ));

            vec = new Vector3(vec.X, vec.Y * cos.X - vec.Z * sin.X, vec.Y * sin.X + vec.Z * cos.X);
            vec = new Vector3(vec.X * cos.Y + vec.Z * sin.Y, vec.Y, vec.X * sin.Y - vec.Z * cos.Y);
            vec = new Vector3(vec.X * cos.Z - vec.Y * sin.Z, vec.X * sin.Z + vec.Y * cos.Z, vec.Z);

            return vec;
        }

        public static float Min(params float[] values)
        {
            var min = float.MaxValue;

            for (var i = 0; i < values.Length; i++)
            {
                min = Math.Min(min, values[i]);
            }

            return min;
        }

        public static float Max(params float[] values)
        {
            var max = float.MinValue;

            for (var i = 0; i < values.Length; i++)
            {
                max = Math.Max(max, values[i]);
            }

            return max;
        }

        public static float Distance(Vector2 v1, Vector2 v2)
        {
            return (v1 - v2).Length;
        }
    }
}