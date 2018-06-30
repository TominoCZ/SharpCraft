namespace SharpCraft.block
{
    internal class BlockLog : Block
    {
        public BlockLog() : base(Material.GetMaterial("wood"), "log")
        {
            Hardness = 32;
            IsFullCube = false;
        }
    }
}