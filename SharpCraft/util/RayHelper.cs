using OpenTK;
using System;
using SharpCraft_Client.entity;

namespace SharpCraft_Client.util
{
    internal class RayHelper
    {
        public static bool RayIntersectsBB(Vector3 /*ray*/origin, Vector3 /*ray*/direction, AxisAlignedBb bb, out Vector3 hitPosition, out Vector3 hitNormal)
        {
            direction = direction.Normalized();
            hitNormal = Vector3.One.Normalized();
            hitPosition = Vector3.Zero;

            float tmin, tmax, tymin, tymax, tzmin, tzmax;
            Vector3 invrd = direction;
            invrd.X = 1.0f / invrd.X;
            invrd.Y = 1.0f / invrd.Y;
            invrd.Z = 1.0f / invrd.Z;

            if (invrd.X >= 0.0f)
            {
                tmin = (bb.Min.X - origin.X) * invrd.X;
                tmax = (bb.Max.X - origin.X) * invrd.X;
            }
            else
            {
                tmin = (bb.Max.X - origin.X) * invrd.X;
                tmax = (bb.Min.X - origin.X) * invrd.X;
            }

            if (invrd.Y >= 0.0f)
            {
                tymin = (bb.Min.Y - origin.Y) * invrd.Y;
                tymax = (bb.Max.Y - origin.Y) * invrd.Y;
            }
            else
            {
                tymin = (bb.Max.Y - origin.Y) * invrd.Y;
                tymax = (bb.Min.Y - origin.Y) * invrd.Y;
            }

            if (tmin > tymax || tymin > tmax)
            {
                return false;
            }
            if (tymin > tmin) tmin = tymin;
            if (tymax < tmax) tmax = tymax;

            if (invrd.Z >= 0.0f)
            {
                tzmin = (bb.Min.Z - origin.Z) * invrd.Z;
                tzmax = (bb.Max.Z - origin.Z) * invrd.Z;
            }
            else
            {
                tzmin = (bb.Max.Z - origin.Z) * invrd.Z;
                tzmax = (bb.Min.Z - origin.Z) * invrd.Z;
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

            float t = tmin;
            hitPosition = origin + t * direction;

            Vector3 AABBCenter = (bb.Min + bb.Max) * 0.5f;

            Vector3 dir = hitPosition - AABBCenter;

            Vector3 width = bb.Max - bb.Min;
            width.X = Math.Abs(width.X);
            width.Y = Math.Abs(width.Y);
            width.Z = Math.Abs(width.Z);

            Vector3 ratio = Vector3.One;
            ratio.X = Math.Abs(dir.X / width.X);
            ratio.Y = Math.Abs(dir.Y / width.Y);
            ratio.Z = Math.Abs(dir.Z / width.Z);

            hitNormal = Vector3.Zero;
            int maxDir = 0; // x
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