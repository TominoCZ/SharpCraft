namespace SharpCraft.block
{
    internal class BlockStone : Block
    {
        public BlockStone() : base(Material.GetMaterial("stone"), "stone")
        {
            Hardness = 64;
        }
    }
}