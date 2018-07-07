using SharpCraft.json;
using SharpCraft.model;

namespace SharpCraft.block
{
    public struct BlockState
    {
        public short Meta { get; }
        public Block Block { get; }
        public ModelBlock Model => JsonModelLoader.GetModelForBlock(Block, Meta);

        public BlockState(Block block, short meta)
        {
            Block = block;
            Meta = meta;
        }
    }
}