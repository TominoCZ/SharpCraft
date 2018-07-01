namespace SharpCraft.block
{
    public class BlockPlanks : Block
    {
        public BlockPlanks() : base(Material.GetMaterial("wood"), "planks")
        {
            IsFullCube = false;
            Hardness = 32;
        }
    }
}