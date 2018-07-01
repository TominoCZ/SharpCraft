namespace SharpCraft.block
{
    internal class BlockLog : Block
    {
        public BlockLog() : base(Material.GetMaterial("wood"))
        {
            SetUnlocalizedName("sharpcraft", "log");
            Hardness = 32;
            IsFullCube = false;
        }
    }
}