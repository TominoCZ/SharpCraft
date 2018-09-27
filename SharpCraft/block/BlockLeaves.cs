namespace SharpCraft.block
{
    internal class BlockLeaves : Block
    {
        public BlockLeaves() : base(Material.GetMaterial("grass"))
        {
            SetUnlocalizedName("sharpcraft", "leaves");
            Hardness = 8;
        }
    }
}