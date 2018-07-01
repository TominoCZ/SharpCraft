namespace SharpCraft.block
{
    internal class BlockDirt : Block
    {
        public BlockDirt() : base(Material.GetMaterial("dirt"))
        {
            SetUnlocalizedName("sharpcraft", "dirt");
            Hardness = 16;
        }
    }
}