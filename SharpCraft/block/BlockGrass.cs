using System;
using System.Collections.Generic;
using System.Text;
using SharpCraft.model;

namespace SharpCraft.block
{
    class BlockGrass : Block
    {
        public BlockGrass() : base("grass")
        {
        }

        public override void OnRegisterStates()
        {
            RegisterState("grass");
        }
    }
}
