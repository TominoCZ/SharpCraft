namespace SharpCraft.block
{
    public class BlockGrass : Block
    {
        public BlockGrass() : base(Material.GetMaterial("grass"), "grass")
        {
            Hardness = 16;
        }
    }
}