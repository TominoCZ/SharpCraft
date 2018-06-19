using System;
using System.Collections.Generic;
using System.Text;

namespace SharpCraft.block
{
    class BlockGlass:Block
    {
        public BlockGlass() : base("glass")
        {
            HasTransparency = true;
        }
    }
}
