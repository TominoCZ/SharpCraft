using SharpCraft.model;

namespace SharpCraft.block
{
    public struct BlockState
    {
        public Block Block { get; }
        public ModelBlock Model { get; }

        public BlockState(Block block, ModelBlock model)
        {
            Block = block;
            Model = model;
        }
    }
}