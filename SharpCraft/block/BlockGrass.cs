namespace SharpCraft_Client.block
{
    public class BlockGrass : Block
    {
        public BlockGrass() : base(Material.GetMaterial("grass"))
        {
            SetUnlocalizedName("sharpcraft", "grass");
            Hardness = 16;
        }
    }
}