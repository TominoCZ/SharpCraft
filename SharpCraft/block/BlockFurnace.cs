namespace SharpCraft.block
{
    public class BlockFurnace : Block
    {
        public BlockFurnace() : base(Material.GetMaterial("stone"), "furnace")
        {
            IsFullCube = false;
            Hardness = 64;
        }
    }
}