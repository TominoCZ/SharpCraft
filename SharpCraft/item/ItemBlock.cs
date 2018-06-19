using SharpCraft.block;
using System;

namespace SharpCraft.item
{
    [Serializable]
    internal class ItemBlock : IItem
    {
        public Block Block { get; }

        public ItemBlock(Block block)
        {
            Block = block;
        }

        public int GetMaxStackSize()
        {
            return 256;
        }

        public string GetUnlocalizedName()
        {
            return Block.UnlocalizedName;
        }

        public string GetDisplayName()
        {
            return GetUnlocalizedName();
        }
    }
}