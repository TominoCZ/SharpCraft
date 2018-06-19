namespace SharpCraft.block
{
    public class BlockAir : Block
    {
        public BlockAir() : base("air")
        {
            IsSolid = false;
            IsOpaque = false;
        }
    }
}