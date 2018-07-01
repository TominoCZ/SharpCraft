namespace SharpCraft.block
{
    public class BlockRare : Block
    {
        public BlockRare() : base(Material.GetMaterial("stone"), "rare")
        {
            Hardness = 128;
        }
    }
}