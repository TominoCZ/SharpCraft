using OpenTK;
using System;

namespace SharpCraft.util
{
    internal class MathUtil
    {
        private static Random rand;

        static MathUtil()
        {
            rand = new Random();
        }

        public static Vector3 Rotate(Vector3 vec, float angleX, float angleY, float angleZ)
        {
            var sin = new Vector3((float)Math.Sin(angleX), (float)Math.Sin(angleY), (float)Math.Sin(angleZ));
            var cos = new Vector3((float)Math.Cos(angleX), (float)Math.Cos(angleY), (float)Math.Cos(angleZ));

            vec = new Vector3(vec.X, vec.Y * cos.X - vec.Z * sin.X, vec.Y * sin.X + vec.Z * cos.X);
            vec = new Vector3(vec.X * cos.Y + vec.Z * sin.Y, vec.Y, vec.X * sin.Y - vec.Z * cos.Y);
            vec = new Vector3(vec.X * cos.Z - vec.Y * sin.Z, vec.X * sin.Z + vec.Y * cos.Z, vec.Z);

            return vec;
        }

        public static float NextFloat(float min = 0, float max = 1)
        {
            return min + (float)rand.NextDouble() * (max - min);
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
    }
}