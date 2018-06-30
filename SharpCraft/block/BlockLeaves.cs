namespace SharpCraft.block
{
    internal class BlockLeaves : Block
    {
        public BlockLeaves() : base(Material.GetMaterial("grass"), "leaves")
        {
            Hardness = 8;
        }
    }
}