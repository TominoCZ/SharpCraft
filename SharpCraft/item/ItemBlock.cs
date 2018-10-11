using System;
using SharpCraft_Client.block;

namespace SharpCraft_Client.item
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