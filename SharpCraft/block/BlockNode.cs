using SharpCraft.model;

namespace SharpCraft.block
{
    internal class BlockNode
    {
        public ModelBlock Model { get; }
        public int meta { get; }

        public BlockNode(ModelBlock model, int meta)
        {
            Model = model;

            this.meta = meta;
        }
    }
}