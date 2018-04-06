using System;
using SharpCraft.block;

namespace SharpCraft.item
{
    [Serializable]
    internal class ItemBlock : Item
    {
        public ItemBlock(EnumBlock block) : base(block.ToString(), block)
        {
        }

        public EnumBlock GetBlock()
        {
            return (EnumBlock)item;
        }
    }
}