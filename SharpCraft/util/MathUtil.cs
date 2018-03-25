using OpenTK;
using System;

namespace SharpCraft
{
    internal class MathUtil
    {
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