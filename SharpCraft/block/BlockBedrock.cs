namespace SharpCraft.block
{
    internal class BlockBedrock : Block
    {
        public BlockBedrock() : base(Material.GetMaterial("stone"), "bedrock")
        {
            Hardness = -1;
        }
    }
}