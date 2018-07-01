using SharpCraft.block;
using System;

namespace SharpCraft.item
{
    [Serializable]
    public class ItemBlock : Item
    {
        public Block Block { get; }

        public ItemBlock(Block block)
        {
            SetUnlocalizedName(block.UnlocalizedName);
            Block = block;
        }
    }
}