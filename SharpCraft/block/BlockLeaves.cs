namespace SharpCraft.block
{
    internal class BlockLeaves : Block
    {
        public BlockLeaves() : base("leaves")
        {
            Hardness = 8;
            IsFullCube = false;
        }
    }
}