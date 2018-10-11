using SharpCraft_Client.json;
using SharpCraft_Client.model;

namespace SharpCraft_Client.block
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