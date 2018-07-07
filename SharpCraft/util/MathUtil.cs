using OpenTK;
using System;

namespace SharpCraft.util
{
    internal static class MathUtil
    {
        private static readonly Random _rand = new Random();

        public static Vector2 Ceiling(this Vector2 vec)
        {
            vec.X = (float)Math.Ceiling(vec.X);
            vec.Y = (float)Math.Ceiling(vec.Y);

            return vec;
        }

        public static float NextFloat(float min = 0, float max = 1)
        {
            double d = _rand.NextDouble();

            return (float)(min + d * (max - min));
        }

        public static float Min(params float[] values)
        {
            float min = float.MaxValue;

            foreach (float f in values)
                min = Math.Min(min, f);

            return min;
        }

        public static float Max(params float[] values)
        {
            float max = float.MinValue;

            foreach (float f in values)
                max = Math.Max(max, f);

            return max;
        }

        public static float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public static Vector2 Clamp(Vector2 val, float minLength, float maxLength)
        {
            if (val.LengthFast > maxLength) return val.Normalized() * maxLength;
            return val.LengthFast < minLength ? val.Normalized() * minLength : val;
        }

        public static int Clamp(int val, int min, int max)
        {
            if (val > max) return max;
            return val < min ? min : val;
        }

        //  .| . . . . . . . .| . . . . . . . .| . .
        // -9 -8-7-6-5-4-3-2-1  0 1 2 3 4 5 6 7  8 9 global Pos
        // -2        -1               0           1  region Pos
        //  7  0 1 2 3 4 5 6 7  0 1 2 3 4 5 6 7  0 1 part Pos

        public static void ToLocal(int pos, int partSize, out int localPos, out int partPos)
        {
            if (pos >= 0)
            {
                partPos = pos / partSize;
                localPos = pos % partSize;
                return;
            }

            partPos = (pos + 1) / partSize - 1;
            localPos = pos - partPos * partSize;
        }

        public static int ToLocal(int pos, int partSize)
        {
            if (pos >= 0) return pos % partSize;

            int partPos = (pos + 1) / partSize - 1;
            return pos - partPos * partSize;
        }

        public static Vector4 Hue(int angle)
        {
            float rad = MathHelper.DegreesToRadians(angle);

            float r = (float)(Math.Sin(rad) * 0.5 + 0.5);
            float g = (float)(Math.Sin(rad + MathHelper.PiOver3 * 2) * 0.5 + 0.5);
            float b = (float)(Math.Sin(rad + MathHelper.PiOver3 * 4) * 0.5 + 0.5);

            return new Vector4(r, g, b, 1);
        }
    }
}