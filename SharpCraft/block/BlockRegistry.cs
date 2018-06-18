using System;
using System.Collections.Generic;

namespace SharpCraft.block
{
    internal class BlockRegistry
    {
        private static Dictionary<string, Block> _registry = new Dictionary<string, Block>();
        private static Dictionary<Type, string> _typeRegistry = new Dictionary<Type, string>();

        public void Put(Block b)
        {
            _registry.Add(b.UnlocalizedName, b);
            _typeRegistry.Add(b.GetType(), b.UnlocalizedName);
        }

        public void RegisterBlocksPost()
        {
            foreach (Block value in _registry.Values)
            {
                value.OnRegisterStates();
            }
        }

        public static Block GetBlock<TBlock>() where TBlock : Block
        {
            return _registry[_typeRegistry[typeof(TBlock)]];
        }

        public static Block GetBlock(string unlocalizedName)
        {
            return _registry[unlocalizedName];
        }
    }
}