using System.Runtime.InteropServices.ComTypes;
using SharpCraft.entity;
using SharpCraft.item;
using SharpCraft.world;

namespace SharpCraft.block
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
            if (moo.sideHit == FaceSides.Up && clicked.World.GetTileEntity(moo.blockPos) is TileEntityCraftingGrid tecg)
            {
                var wasEmpty = tecg.IsEmpty();

                if (wasEmpty && clicked.IsSneaking)
                    return false;

                if (!wasEmpty && clicked.IsSneaking && !tecg.HasResult)
                    return true;

                tecg.OnRightClicked(clicked.World, moo.hitVec, clicked.GetEquippedItemStack(), clicked);
                
                if (tecg.IsEmpty() || !wasEmpty)
                    return true;
            }

            return false;
        }

        public override TileEntity CreateTileEntity(World world, BlockPos pos)
        {
            return new TileEntityCraftingGrid(world, pos);
        }
    }
}