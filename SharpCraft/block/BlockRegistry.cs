using System;
using System.Collections.Generic;

namespace SharpCraft.block
{
    internal class BlockRegistry
    {
        private static Dictionary<string, Block> _registry = new Dictionary<string, Block>();
        private static Dictionary<Type, Block> _typeRegistry = new Dictionary<Type, Block>();

        public void Put(Block b)
        {
            _registry.Add(b.UnlocalizedName, b);
            _typeRegistry.Add(b.GetType(), b);
        }

        public void RegisterBlocksPost()
        {
            foreach (var value in _registry.Values)
            {
                value.OnRegisterStates();
            }
        }

        public static Block GetBlock<TBlock>() where TBlock : Block
        {
            return _typeRegistry[typeof(TBlock)];
        }

        public static Block GetBlock(string unlocalizedName)
        {
            return _registry[unlocalizedName];
        }
    }
}