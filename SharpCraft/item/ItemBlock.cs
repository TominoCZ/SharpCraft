using System;
using SharpCraft.block;

namespace SharpCraft.item
{
    [Serializable]
    internal class ItemBlock : Item
    {
        public ItemBlock(EnumBlock block) : base(block.ToString(), (object) block)
        {
        }

        public EnumBlock getBlock()
        {
            return (EnumBlock)item;
        }
    }
}