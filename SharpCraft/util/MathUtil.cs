using OpenTK;
using System;
using System.Security.Cryptography;

namespace SharpCraft.util
{
    internal static class MathUtil
    {
        private static RandomNumberGenerator _rand = RandomNumberGenerator.Create();

        public static float GetNext()
        {
            var bytes = new byte[8];
            _rand.GetBytes(bytes);

            var ul = BitConverter.ToUInt64(bytes, 0) / (1 << 11);
            double d = ul / (double)(1UL << 53);

            return (float)d;
        }

        public static Vector2 Ceiling(this Vector2 vec)
        {
            vec.X = (float)Math.Ceiling(vec.X);
            vec.Y = (float)Math.Ceiling(vec.Y);

            return vec;
        }

        public static Vector3 Rotate(this Vector3 vec, float angleX, float angleY, float angleZ)
        {
            var sinX = (float)Math.Sin(angleX);
            var sinY = (float)Math.Sin(angleY);
            var sinZ = (float)Math.Sin(angleZ);

            var cosX = (float)Math.Cos(angleX);
            var cosY = (float)Math.Cos(angleY);
            var cosZ = (float)Math.Cos(angleZ);

            var vecX = vec.X;
            var vecY = vec.Y * cosX - vec.Z * sinX;
            var vecZ = vec.Y * sinX + vec.Z * cosX;

            vec.X = vecX;
            vec.Y = vecY;
            vec.Z = vecZ;

            vecX = vec.X * cosY + vec.Z * sinY;
            vecZ = vec.X * sinY - vec.Z * cosY;

            vec.X = vecX;
            vec.Z = vecZ;

            vecX = vec.X * cosZ - vec.Y * sinZ;
            vecY = vec.X * sinZ + vec.Y * cosZ;

            vec.X = vecX;
            vec.Y = vecY;

            return vec;
        }

        public static float NextFloat(float min = 0, float max = 1)
        {
            var f = GetNext();

            return min + f * (max - min);
        }

        public static float Min(params float[] values)
        {
            var min = float.MaxValue;

            foreach (var f in values)
                min = Math.Min(min, f);

            return min;
        }

        public static float Max(params float[] values)
        {
            var max = float.MinValue;

            foreach (var f in values)
                max = Math.Max(max, f);

            return max;
        }

        public static float Distance(Vector2 v1, Vector2 v2)
        {
            return (v1 - v2).LengthFast;
        }

        public static float Distance(Vector3 v1, Vector3 v2)
        {
            return (v1 - v2).LengthFast;
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
        // -9 -8-7-6-5-4-3-2-1  0 1 2 3 4 5 6 7  8 9 global pos
        // -2        -1               0           1  region pos
        //  7  0 1 2 3 4 5 6 7  0 1 2 3 4 5 6 7  0 1 part pos

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

            var partPos = (pos + 1) / partSize - 1;
            return pos - partPos * partSize;
        }

        public static Vector4 Hue(int angle)
        {
            var rad = MathHelper.DegreesToRadians(angle);

            var r = (float)(Math.Sin(rad) * 0.5 + 0.5);
            var g = (float)(Math.Sin(rad + MathHelper.PiOver3 * 2) * 0.5 + 0.5);
            var b = (float)(Math.Sin(rad + MathHelper.PiOver3 * 4) * 0.5 + 0.5);

            return new Vector4(r, g, b, 1);
        }
    }
}