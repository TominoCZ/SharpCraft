namespace SharpCraft_Client.block
{
    public class BlockStone : Block
    {
        public BlockStone() : base(Material.GetMaterial("stone"))
        {
            SetUnlocalizedName("sharpcraft", "stone");
            Hardness = 64;
        }
    }
}