namespace SharpCraft_Client.block
{
    public class BlockRare : Block
    {
        public BlockRare() : base(Material.GetMaterial("stone"))
        {
            SetUnlocalizedName("sharpcraft", "rare");
            Hardness = 128;
        }
    }
}