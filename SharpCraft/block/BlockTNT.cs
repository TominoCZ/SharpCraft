using SharpCraft_Client.world;

namespace SharpCraft_Client.block
{
    public class BlockTNT : Block
    {
        public BlockTNT() : base(Material.GetMaterial("grass"))
        {
            SetUnlocalizedName("sharpcraft", "tnt");
        }

        public override TileEntity CreateTileEntity(World world, BlockPos pos)
        {
            return new TileEntityTNT(pos, world);
        }
    }
}