namespace SharpCraft.block
{
    public class BlockGrass : Block
    {
        public BlockGrass() : base("grass")
        {
        }
    }

    public class BlockAir : Block
    {
        public BlockAir() : base("air")
        {
            IsSolid = false;
            IsOpaque = false;
        }
    }
}