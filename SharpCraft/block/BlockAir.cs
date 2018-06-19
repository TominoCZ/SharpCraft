using System;
using System.Collections.Generic;
using System.Text;

namespace SharpCraft.block
{
    public class BlockAir : Block
    {
        public BlockAir() : base("air")
        {
            IsSolid = false;
            IsOpaque = false;
        }
    }
}
