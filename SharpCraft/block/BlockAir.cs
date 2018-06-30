namespace SharpCraft.block
{
    public class BlockAir : Block
    {
        public BlockAir() : base(Material.GetMaterial("air"), "air")
        {
            //IsSolid = false;
            IsOpaque = false;
        }
    }
}