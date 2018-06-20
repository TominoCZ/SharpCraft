using OpenTK;
using SharpCraft.entity;

namespace SharpCraft.block
{
    class BlockLadder : Block
    {
        public BlockLadder() : base("ladder")
        {
            IsFullCube = false;
            
            BoundingBox = new AxisAlignedBB(0, 0, 0.8f, 1, 1, 1);

            Hardness = 32;
        }
    }
}
