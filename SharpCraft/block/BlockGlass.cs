namespace SharpCraft_Client.block
{
    public class BlockGlass : Block
    {
        public BlockGlass() : base(Material.GetMaterial("stone"))
        {
            SetUnlocalizedName("sharpcraft", "glass");
            HasTransparency = true;
            Hardness = 8;
        }
    }
}