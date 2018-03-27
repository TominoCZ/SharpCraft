using OpenTK;
using System;

namespace SharpCraft
{
    internal class MathUtil
    {
        public static Vector3 Rotate(Vector3 vec, float AngleX, float AngleY, float AngleZ)
        {
            Vector3 sin = new Vector3((float)Math.Sin(AngleX), (float)Math.Sin(AngleY), (float)Math.Sin(AngleZ));
            Vector3 cos = new Vector3((float)Math.Cos(AngleX), (float)Math.Cos(AngleY), (float)Math.Cos(AngleZ));

            vec = new Vector3(vec.X, vec.Y * cos.X - vec.Z * sin.X, vec.Y * sin.X + vec.Z * cos.X);
            vec = new Vector3(vec.X * cos.Y + vec.Z * sin.Y, vec.Y, vec.X * sin.Y - vec.Z * cos.Y);
            vec = new Vector3(vec.X * cos.Z - vec.Y * sin.Z, vec.X * sin.Z + vec.Y * cos.Z, vec.Z);

            return vec;
        }

        public static float Min(params float[] values)
        {
            var min = float.MaxValue;

            for (int i = 0; i < values.Length; i++)
            {
                min = Math.Min(min, values[i]);
            }

            return min;
        }

        public static float Max(params float[] values)
        {
            var max = float.MinValue;

            for (int i = 0; i < values.Length; i++)
            {
                max = Math.Max(max, values[i]);
            }

            return max;
        }

        public static float distance(Vector2 v1, Vector2 v2)
        {
            return (v1 - v2).LengthFast;
        }
    }
}