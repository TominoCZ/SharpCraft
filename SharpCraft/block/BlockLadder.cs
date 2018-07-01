using SharpCraft.entity;

namespace SharpCraft.block
{
    internal class BlockLadder : Block
    {
        public BlockLadder() : base(Material.GetMaterial("wood"), "ladder")
        {
            IsFullCube = false;

            BoundingBox = new AxisAlignedBb(0, 0, 0.8f, 1, 1, 1);

            Hardness = 32;
        }
    }
}