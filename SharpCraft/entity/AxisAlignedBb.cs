using OpenTK;
using SharpCraft.util;
using System;

namespace SharpCraft.entity
{
    public class AxisAlignedBb
    {
        public static readonly AxisAlignedBb BlockFull = new AxisAlignedBb(Vector3.Zero, Vector3.One);
        public static readonly AxisAlignedBb Null = new AxisAlignedBb(Vector3.Zero, Vector3.Zero);
        public readonly Vector3 Min;
        public readonly Vector3 Max;

        public readonly Vector3 Size;

        public AxisAlignedBb(float size) : this(Vector3.One * size)
        {
        }

        public AxisAlignedBb(Vector3 size) : this(Vector3.Zero, size)
        {
        }

        public AxisAlignedBb(Vector3 min, Vector3 max)
        {
            this.Min = min;
            this.Max = max;

            float minX = MathUtil.Min(min.X, max.X);
            float minY = MathUtil.Min(min.Y, max.Y);
            float minZ = MathUtil.Min(min.Z, max.Z);

            float maxX = MathUtil.Max(min.X, max.X);
            float maxY = MathUtil.Max(min.Y, max.Y);
            float maxZ = MathUtil.Max(min.Z, max.Z);

            Vector3 v1 = new Vector3(minX, minY, minZ);
            Vector3 v2 = new Vector3(maxX, maxY, maxZ);

            Size = v2 - v1;
        }

        public AxisAlignedBb(float minX, float minY, float minZ, float maxX, float maxY, float maxZ) : this(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ))
        {
        }

        public AxisAlignedBb Offset(Vector3 by)
        {
            return new AxisAlignedBb(Min + by, Max + by);
        }

        public AxisAlignedBb Grow(Vector3 by)
        {
            return new AxisAlignedBb(Min + by / 2, Max - by / 2);
        }

        public AxisAlignedBb Union(AxisAlignedBb other)
        {
            int minX = (int)Math.Floor(MathUtil.Min(Min.X, Max.X, other.Min.X, other.Max.X));
            int minY = (int)Math.Floor(MathUtil.Min(Min.Y, Max.Y, other.Min.Y, other.Max.Y));
            int minZ = (int)Math.Floor(MathUtil.Min(Min.Z, Max.Z, other.Min.Z, other.Max.Z));

            int maxX = (int)Math.Ceiling(MathUtil.Max(Min.X, Max.X, other.Min.X, other.Max.X));
            int maxY = (int)Math.Ceiling(MathUtil.Max(Min.Y, Max.Y, other.Min.Y, other.Max.Y));
            int maxZ = (int)Math.Ceiling(MathUtil.Max(Min.Z, Max.Z, other.Min.Z, other.Max.Z));

            return new AxisAlignedBb(minX, minY, minZ, maxX, maxY, maxZ);
        }

        public float CalculateYOffset(AxisAlignedBb other, float offset)
        {
            //Y
            if (other.Max.X > Min.X && other.Min.X < Max.X && other.Max.Z > Min.Z && other.Min.Z < Max.Z)
            {
                if (offset > 0.0D && other.Max.Y <= Min.Y)
                {
                    float d1 = Min.Y - other.Max.Y;

                    if (d1 < offset)
                    {
                        offset = d1;
                    }
                }
                else if (offset < 0.0D && other.Min.Y >= Max.Y)
                {
                    float d0 = Max.Y - other.Min.Y;

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
            //x
            if (other.Max.Y > Min.Y && other.Min.Y < Max.Y && other.Max.Z > Min.Z && other.Min.Z < Max.Z)
            {
                if (offset > 0.0D && other.Max.X <= Min.X)
                {
                    float d1 = Min.X - other.Max.X;

                    if (d1 < offset)
                    {
                        offset = d1;
                    }
                }
                else if (offset < 0.0D && other.Min.X >= Max.X)
                {
                    float d0 = Max.X - other.Min.X;

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
            //z
            if (other.Max.X > Min.X && other.Min.X < Max.X && other.Max.Y > Min.Y && other.Min.Y < Max.Y)
            {
                if (offset > 0.0D && other.Max.Z <= Min.Z)
                {
                    float d1 = Min.Z - other.Max.Z;

                    if (d1 < offset)
                    {
                        offset = d1;
                    }
                }
                else if (offset < 0.0D && other.Min.Z >= Max.Z)
                {
                    float d0 = Max.Z - other.Min.Z;

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
            return (Min + Max) / 2;
        }

        public bool IntersectsWith(AxisAlignedBb other)
        {
            return Min.X < other.Max.X && Max.X > other.Min.X &&
                   Min.Y < other.Max.Y && Max.Y > other.Min.Y &&
                   Min.Z < other.Max.Z && Max.Z > other.Min.Z;
        }
    }
}