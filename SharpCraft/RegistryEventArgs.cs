using SharpCraft.block;
using SharpCraft.item;
using System;

namespace SharpCraft
{
    internal class RegistryEventArgs : EventArgs
    {
        private readonly Action<Item> _funcRegisterItem;
        private readonly Action<Block> _funcRegisterBlock;

        public RegistryEventArgs(BlockRegistry blockRegistry, ItemRegistry itemRegistry)
        {
            _funcRegisterBlock = blockRegistry.Put;
            _funcRegisterItem = itemRegistry.Put;
        }

        public void Register(Block block)
        {
            _funcRegisterBlock(block);
        }

        public void Register(Item item)
        {
            _funcRegisterItem(item);
        }
    }
}