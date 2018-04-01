using OpenTK;
using SharpCraft.entity;
using System;

namespace SharpCraft.util
{
    internal class RayHelper
    {
        public static bool rayIntersectsBB(Vector3 /*ray*/origin, Vector3 /*ray*/direction, AxisAlignedBb bb, out Vector3 hitPosition, out Vector3 hitNormal)
        {
            direction = direction.Normalized();
            hitNormal = Vector3.One.Normalized();
            hitPosition = Vector3.Zero;

            float tmin, tmax, tymin, tymax, tzmin, tzmax;
            var invrd = direction;
            invrd.X = 1.0f / invrd.X;
            invrd.Y = 1.0f / invrd.Y;
            invrd.Z = 1.0f / invrd.Z;

            if (invrd.X >= 0.0f)
            {
                tmin = (bb.min.X - origin.X) * invrd.X;
                tmax = (bb.max.X - origin.X) * invrd.X;
            }
            else
            {
                tmin = (bb.max.X - origin.X) * invrd.X;
                tmax = (bb.min.X - origin.X) * invrd.X;
            }

            if (invrd.Y >= 0.0f)
            {
                tymin = (bb.min.Y - origin.Y) * invrd.Y;
                tymax = (bb.max.Y - origin.Y) * invrd.Y;
            }
            else
            {
                tymin = (bb.max.Y - origin.Y) * invrd.Y;
                tymax = (bb.min.Y - origin.Y) * invrd.Y;
            }

            if (tmin > tymax || tymin > tmax)
            {
                return false;
            }
            if (tymin > tmin) tmin = tymin;
            if (tymax < tmax) tmax = tymax;

            if (invrd.Z >= 0.0f)
            {
                tzmin = (bb.min.Z - origin.Z) * invrd.Z;
                tzmax = (bb.max.Z - origin.Z) * invrd.Z;
            }
            else
            {
                tzmin = (bb.max.Z - origin.Z) * invrd.Z;
                tzmax = (bb.min.Z - origin.Z) * invrd.Z;
            }

            if (tmin > tzmax || tzmin > tmax)
            {
                return false;
            }
            if (tzmin > tmin) tmin = tzmin;
            if (tzmax < tmax) tmax = tzmax;

            if (tmin < 0) tmin = tmax;
            if (tmax < 0)
            {
                return false;
            }

            var t = tmin;
            hitPosition = origin + t * direction;

            var AABBCenter = (bb.min + bb.max) * 0.5f;

            var dir = hitPosition - AABBCenter;

            var width = bb.max - bb.min;
            width.X = Math.Abs(width.X);
            width.Y = Math.Abs(width.Y);
            width.Z = Math.Abs(width.Z);

            var ratio = Vector3.One;
            ratio.X = Math.Abs(dir.X / width.X);
            ratio.Y = Math.Abs(dir.Y / width.Y);
            ratio.Z = Math.Abs(dir.Z / width.Z);

            hitNormal = Vector3.Zero;
            var maxDir = 0; // x
            if (ratio.X >= ratio.Y && ratio.X >= ratio.Z)
            { // x is the greatest
                maxDir = 0;
            }
            else if (ratio.Y >= ratio.X && ratio.Y >= ratio.Z)
            { // y is the greatest
                maxDir = 1;
            }
            else if (ratio.Z >= ratio.X && ratio.Z >= ratio.Y)
            { // z is the greatest
                maxDir = 2;
            }

            if (dir[maxDir] > 0)
                hitNormal[maxDir] = 1.0f;
            else hitNormal[maxDir] = -1.0f;

            return true;
        }
    }
}