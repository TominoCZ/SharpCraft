using SharpCraft.block;
using System;

namespace SharpCraft.item
{
    [Serializable]
    internal class ItemBlock : Item
    {
        public ItemBlock(Block block) : base(block.ToString(), block)
        {
        }

        public Block GetBlock()
        {
            return (Block)InnerItem;
        }
    }
}