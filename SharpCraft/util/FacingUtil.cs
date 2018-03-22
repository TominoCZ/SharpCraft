using System;
using System.Collections.Generic;
using System.Text;

namespace SharpCraft
{
    class FacingUtil
    {
        public static EnumFacing[] SIDES;

        static FacingUtil()
        {
            SIDES = (EnumFacing[])Enum.GetValues(typeof(EnumFacing));
        }
    }
}
