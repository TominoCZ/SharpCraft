namespace SharpCraft.block
{
    internal class BlockGlass : Block
    {
        public BlockGlass() : base(Material.GetMaterial("stone"), "glass")
        {
            HasTransparency = true;
            Hardness = 8;
        }
    }
}