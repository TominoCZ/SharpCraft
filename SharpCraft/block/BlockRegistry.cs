using System;
using System.Collections.Generic;
using System.Text;
using SharpCraft.model;

namespace SharpCraft.block
{
    class BlockRegistry
    {
        private static Dictionary<string, Block> _registry = new Dictionary<string, Block>();

        public static void RegisterBlock(string localizedName, Block block)
        {
            if (!_registry.ContainsKey(localizedName))
                _registry.Add(localizedName, block);
        }

        public static Block GetBlockFromName(string localizedName)
        {
            _registry.TryGetValue(localizedName, out var block);

            return block;
        }
    }

    class BlockGrass : Block
    {
        protected BlockGrass()
        {

        }

        public override BlockState GetStateFromMeta(int meta)
        {
            switch (meta)
            {
                case 1:
                    return BlockRegistry.GetBlockFromName("dirt").GetDefaultBlockState();
                default: return defaultState;
            }
        }
    }

    class Block
    {
        protected BlockState defaultState;
        public int hardness = 8;

        protected Block()
        {
            defaultState = new BlockState(null, 0);
        }

        public virtual BlockState GetStateFromMeta(int meta)
        {
            return null;
        }

        public BlockState GetDefaultBlockState()
        {
            return defaultState;
        }
    }
}
