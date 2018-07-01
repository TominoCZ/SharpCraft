namespace SharpCraft.block
{
    internal class BlockStone : Block
    {
        public BlockStone() : base(Material.GetMaterial("stone"))
        {
            SetUnlocalizedName("sharpcraft", "stone");
            Hardness = 64;
        }
    }
}