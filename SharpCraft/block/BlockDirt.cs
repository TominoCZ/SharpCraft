namespace SharpCraft_Client.block
{
    public class BlockDirt : Block
    {
        public BlockDirt() : base(Material.GetMaterial("dirt"))
        {
            SetUnlocalizedName("sharpcraft", "dirt");
            Hardness = 16;
        }
    }
}