using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpCraft.entity;
using SharpCraft.world;

namespace SharpCraft.block
{
    public class BlockCraftingTable : Block
    {
        public BlockCraftingTable() : base(Material.GetMaterial("wood"), "crafting_table")
        {
            IsFullCube = false;

            CanBeInteractedWith = true;

            Hardness = 32;
        }

        public override void OnRightClicked(MouseOverObject moo, EntityPlayerSp clicked)
        {
            if (moo.sideHit != FaceSides.Up)
                return;

            if (clicked.World.GetTileEntity(moo.blockPos) is TileEntityCraftingGrid tecg)
            {
                tecg.OnRightClicked(clicked.World, moo.hitVec, clicked.GetEquippedItemStack(), clicked);
            }
        }

        public override TileEntity CreateTileEntity(World world, BlockPos pos)
        {
            return new TileEntityCraftingGrid(world, pos);
        }
    }
}
