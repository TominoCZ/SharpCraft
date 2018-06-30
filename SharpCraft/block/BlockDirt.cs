namespace SharpCraft.block
{
    internal class BlockDirt : Block
    {
        public BlockDirt() : base(Material.GetMaterial("dirt"), "dirt")
        {
            Hardness = 16;
        }
    }
}