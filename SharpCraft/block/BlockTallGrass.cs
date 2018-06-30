using OpenTK;
using SharpCraft.entity;

namespace SharpCraft.block
{
    class BlockTallGrass : Block
    {
        public BlockTallGrass() : base(Material.GetMaterial("tallgrass"), "tallgrass")
        {
            IsFullCube = false;
            //IsSolid = false;
            IsReplaceable = true;

            BoundingBox = new AxisAlignedBB(0.85f).offset(new Vector3(0.075f, 0, 0.075f));

            Hardness = 0;
        }
    }
}
