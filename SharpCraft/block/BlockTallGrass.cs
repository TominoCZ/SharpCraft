using OpenTK;
using SharpCraft_Client.entity;

namespace SharpCraft_Client.block
{
    public class BlockTallGrass : Block
    {
        public BlockTallGrass() : base(Material.GetMaterial("tallgrass"))
        {
            SetUnlocalizedName("sharpcraft", "tallgrass");

            IsFullCube = false;
            IsReplaceable = true;

            BoundingBox = new AxisAlignedBb(0.85f).Offset(new Vector3(0.075f, 0, 0.075f));

            Hardness = 0;
        }
    }
}