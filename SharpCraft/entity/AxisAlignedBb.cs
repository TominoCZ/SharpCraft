using OpenTK;
using SharpCraft.util;
using System;

namespace SharpCraft.entity
{
    public class AxisAlignedBb
    {
        public static AxisAlignedBb BLOCK_FULL { get; } = new AxisAlignedBb(Vector3.Zero, Vector3.One);
        public static AxisAlignedBb NULL { get; } = new AxisAlignedBb(Vector3.Zero, Vector3.Zero);
        public Vector3 min { get; }
        public Vector3 max { get; }

        public Vector3 size { get; }

        public AxisAlignedBb(Vector3 min, Vector3 max)
        {
            this.min = min;
            this.max = max;

            var minX = MathUtil.Min(min.X, max.X);
            var minY = MathUtil.Min(min.Y, max.Y);
            var minZ = MathUtil.Min(min.Z, max.Z);

            var maxX = MathUtil.Max(min.X, max.X);
            var maxY = MathUtil.Max(min.Y, max.Y);
            var maxZ = MathUtil.Max(min.Z, max.Z);

            var v1 = new Vector3(minX, minY, minZ);
            var v2 = new Vector3(maxX, maxY, maxZ);

            size = v2 - v1;
        }

        public AxisAlignedBb(float minX, float minY, float minZ, float maxX, float maxY, float maxZ) : this(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ))
        {
        }

        public AxisAlignedBb offset(Vector3 by)
        {
            return new AxisAlignedBb(min + by, max + by);
        }

        public AxisAlignedBb Grow(Vector3 by)
        {
            return new AxisAlignedBb(min + by / 2, max - by / 2);
        }

        public AxisAlignedBb Union(AxisAlignedBb other)
        {
            var minX = (int)Math.Floor(MathUtil.Min(min.X, max.X, other.min.X, other.max.X));
            var minY = (int)Math.Floor(MathUtil.Min(min.Y, max.Y, other.min.Y, other.max.Y));
            var minZ = (int)Math.Floor(MathUtil.Min(min.Z, max.Z, other.min.Z, other.max.Z));

            var maxX = (int)Math.Ceiling(MathUtil.Max(min.X, max.X, other.min.X, other.max.X));
            var maxY = (int)Math.Ceiling(MathUtil.Max(min.Y, max.Y, other.min.Y, other.max.Y));
            var maxZ = (int)Math.Ceiling(MathUtil.Max(min.Z, max.Z, other.min.Z, other.max.Z));

            return new AxisAlignedBb(minX, minY, minZ, maxX, maxY, maxZ);
        }

        public float CalculateYOffset(AxisAlignedBb other, float offset)
        {
            //Y
            if (other.max.X > min.X && other.min.X < max.X && other.max.Z > min.Z && other.min.Z < max.Z)
            {
                if (offset > 0.0D && other.max.Y <= min.Y)
                {
                    float d1 = min.Y - other.max.Y;

                    if (d1 < offset)
                    {
                        offset = d1;
                    }
                }
                else if (offset < 0.0D && other.min.Y >= max.Y)
                {
                    float d0 = max.Y - other.min.Y;

                    if (d0 > offset)
                    {
                        offset = d0;
                    }
                }
            }

            return offset;
        }

        public float CalculateXOffset(AxisAlignedBb other, float offset)
        {
            //X
            if (other.max.Y > min.Y && other.min.Y < max.Y && other.max.Z > min.Z && other.min.Z < max.Z)
            {
                if (offset > 0.0D && other.max.X <= min.X)
                {
                    float d1 = min.X - other.max.X;

                    if (d1 < offset)
                    {
                        offset = d1;
                    }
                }
                else if (offset < 0.0D && other.min.X >= max.X)
                {
                    float d0 = max.X - other.min.X;

                    if (d0 > offset)
                    {
                        offset = d0;
                    }
                }
            }

            return offset;
        }

        public float CalculateZOffset(AxisAlignedBb other, float offset)
        {
            //Z
            if (other.max.X > min.X && other.min.X < max.X && other.max.Y > min.Y && other.min.Y < max.Y)
            {
                if (offset > 0.0D && other.max.Z <= min.Z)
                {
                    float d1 = min.Z - other.max.Z;

                    if (d1 < offset)
                    {
                        offset = d1;
                    }
                }
                else if (offset < 0.0D && other.min.Z >= max.Z)
                {
                    float d0 = max.Z - other.min.Z;

                    if (d0 > offset)
                    {
                        offset = d0;
                    }
                }
            }

            return offset;
        }

        public Vector3 GetCenter()
        {
            return (min + max) / 2;
        }

        public bool IntersectsWith(AxisAlignedBb other)
        {
            return min.X < other.max.X && max.X > other.min.X &&
                   min.Y < other.max.Y && max.Y > other.min.Y &&
                   min.Z < other.max.Z && max.Z > other.min.Z;
        }
    }
}