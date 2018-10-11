using SharpCraft_Client.entity;

namespace SharpCraft_Client.block
{
    public class BlockLadder : Block
    {
        public BlockLadder() : base(Material.GetMaterial("wood"))
        {
            SetUnlocalizedName("sharpcraft", "ladder");
            IsFullCube = false;

            BoundingBox = new AxisAlignedBb(0, 0, 0.8f, 1, 1, 1);

            Hardness = 32;
        }
    }
}