namespace SharpCraft.block
{
    public class BlockAir : Block
    {
        public BlockAir() : base(Material.GetMaterial("air"))
        {
            SetUnlocalizedName("sharpcraft", "air");
            
            IsOpaque = false;
        }
    }
}