using SharpCraft_Client.entity;
using SharpCraft_Client.world;

namespace SharpCraft_Client.block
{
    public class BlockCraftingTable : Block
    {
        public BlockCraftingTable() : base(Material.GetMaterial("wood"))
        {
            SetUnlocalizedName("sharpcraft", "crafting_table");

            IsFullCube = false;

            Hardness = 32;
        }

        public override bool OnActivated(MouseOverObject moo, EntityPlayerSp clicked)
        {
            if (moo.sideHit != FaceSides.Up ||
                !(clicked.World.GetTileEntity(moo.blockPos) is TileEntityCraftingGrid tecg))
                return false;

            //var wasEmpty = tecg.IsEmpty();

            tecg.OnRightClicked(clicked.World, moo.hitVec, clicked.GetEquippedItemStack(), clicked);

            //if (wasEmpty)
            //return true;

            return false;
        }

        public override bool CanBlockBePlacedAtSide(World world, BlockPos blockPos, FaceSides sideHit, EntityPlayerSp placer)
        {
            if (sideHit != FaceSides.Up ||
                !(world.GetTileEntity(blockPos) is TileEntityCraftingGrid tecg))
                return true;

            return tecg.IsEmpty() && placer.IsSneaking;
        }

        public override TileEntity CreateTileEntity(World world, BlockPos pos)
        {
            return new TileEntityCraftingGrid(world, pos);
        }
    }
}