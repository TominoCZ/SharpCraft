using System;

namespace SharpCraft
{
    internal class FacingUtil
    {
        public static EnumFacing[] SIDES;

        static FacingUtil()
        {
            SIDES = (EnumFacing[])Enum.GetValues(typeof(EnumFacing));
        }
    }
}