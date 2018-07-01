namespace SharpCraft.block
{
    public class BlockCobbleStone : Block
    {
        public BlockCobbleStone() : base(Material.GetMaterial("stone"), "cobblestone")
        {
            Hardness = 64;
        }
    }
}