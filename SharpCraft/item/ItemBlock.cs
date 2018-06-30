using SharpCraft.block;
using System;

namespace SharpCraft.item
{
    [Serializable]
    public class ItemBlock : Item
    {
        public Block Block { get; }

        public ItemBlock(Block block) : base(block.UnlocalizedName)
        {
            Block = block;
        }

        public int GetMaxStackSize()
        {
            return 256;
        }
    }
}