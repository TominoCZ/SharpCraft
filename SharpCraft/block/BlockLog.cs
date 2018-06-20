namespace SharpCraft.block
{
    internal class BlockLog : Block
    {
        public BlockLog() : base("log")
        {
            Hardness = 32;
            IsFullCube = false;
        }
    }
}