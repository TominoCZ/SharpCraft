namespace SharpCraft.block
{
    internal class BlockBedrock : Block
    {
        public BlockBedrock() : base(Material.GetMaterial("stone"))
        {
            SetUnlocalizedName("sharpcraft", "bedrock");
            Hardness = -1;
        }
    }
}