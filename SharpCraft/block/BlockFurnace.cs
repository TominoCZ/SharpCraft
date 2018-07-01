namespace SharpCraft.block
{
    public class BlockFurnace : Block
    {
        public BlockFurnace() : base(Material.GetMaterial("stone"))
        {
            SetUnlocalizedName("sharpcraft", "furnace");

            IsFullCube = false;
            Hardness = 64;
        }
    }
}