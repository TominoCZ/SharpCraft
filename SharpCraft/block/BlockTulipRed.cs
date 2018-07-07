using OpenTK;
using SharpCraft.entity;

namespace SharpCraft.block
{
    public class BlockTulipRed: Block
    {
        public BlockTulipRed() : base(Material.GetMaterial("tallgrass"))
        {
            SetUnlocalizedName("sharpcraft", "tulip_red");
            
            IsFullCube = false;
            IsReplaceable = true;

            BoundingBox = new AxisAlignedBb(0.85f).Offset(new Vector3(0.075f, 0, 0.075f));

            Hardness = 0;
        }
    }
}