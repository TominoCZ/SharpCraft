namespace SharpCraft_Client.block
{
    public class BlockCobbleStone : Block
    {
        public BlockCobbleStone() : base(Material.GetMaterial("stone"))
        {
            SetUnlocalizedName("sharpcraft", "cobblestone");
            Hardness = 64;
        }
    }
}