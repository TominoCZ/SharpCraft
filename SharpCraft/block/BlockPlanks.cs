namespace SharpCraft_Client.block
{
    public class BlockPlanks : Block
    {
        public BlockPlanks() : base(Material.GetMaterial("wood"))
        {
            SetUnlocalizedName("sharpcraft", "planks");
            IsFullCube = false;
            Hardness = 32;
        }
    }
}