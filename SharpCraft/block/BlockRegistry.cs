using SharpCraft.model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpCraft.block
{
    internal class BlockRegistry
    {
        private static readonly Dictionary<string, Block> _registry = new Dictionary<string, Block>();
        private static readonly Dictionary<Type, string> _typeRegistry = new Dictionary<Type, string>();

        public void Put(Block b)
        {
            var s = b.UnlocalizedName;

            _registry.Add(s, b);
            _typeRegistry.Add(b.GetType(), s);
        }

        public void RegisterBlocksPost(JsonModelLoader loader)
        {
            foreach (var block in _registry.Values)
            {
                block.RegisterState(loader, new BlockState(block, JsonModelLoader.GetModelForBlock(block)));
            }

            loader.LoadBlocks();
        }

        public static List<Block> AllBlocks()
        {
            return _registry.Values.ToList();
        }

        public static Block GetBlock<TBlock>() where TBlock : Block
        {
            return _registry[_typeRegistry[typeof(TBlock)]];
        }

        [Obsolete("Please use GetBlock<T:Block>")]
        public static Block GetBlock(string unlocalizedName)
        {
            if (_registry.TryGetValue(unlocalizedName, out var block))
                return block;

            return GetBlock<BlockAir>();
        }
    }
}