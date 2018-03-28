using System;
using OpenTK;
using SharpCraft.block;

namespace SharpCraft.util
{
    internal class FacingUtil
    {
        public static EnumFacing[] SIDES;

        static FacingUtil()
        {
            SIDES = (EnumFacing[])Enum.GetValues(typeof(EnumFacing));
        }

        public static Vector3 getFacingVector(EnumFacing dir)
        {
            switch (dir)
            {
                case EnumFacing.NORTH:
                    return -Vector3.UnitZ;

                case EnumFacing.SOUTH:
                    return Vector3.UnitZ;

                case EnumFacing.EAST:
                    return Vector3.UnitX;

                case EnumFacing.WEST:
                    return -Vector3.UnitX;

                case EnumFacing.UP:
                    return Vector3.UnitY;

                case EnumFacing.DOWN:
                    return -Vector3.UnitY;

                default:
                    return Vector3.Zero;
            }
        }
    }
}