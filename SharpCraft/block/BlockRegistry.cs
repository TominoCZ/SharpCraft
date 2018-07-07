using SharpCraft.model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpCraft.block
{
    internal class BlockRegistry
    {
        private static readonly Dictionary<string, Block> Registry = new Dictionary<string, Block>();
        private static readonly Dictionary<Type, string> TypeRegistry = new Dictionary<Type, string>();

        public void Put(Block b)
        {
            var s = b.UnlocalizedName;

            Registry.Add(s, b);
            TypeRegistry.Add(b.GetType(), s);
        }

        public void RegisterBlocksPost(JsonModelLoader loader)
        {
            foreach (var block in Registry.Values)
            {
                block.RegisterState(loader, new BlockState(block, JsonModelLoader.GetModelForBlock(block)));
            }

            loader.LoadBlocks();
        }

        public static List<Block> AllBlocks()
        {
            return Registry.Values.ToList();
        }

        public static Block GetBlock<TBlock>() where TBlock : Block
        {
            return Registry[TypeRegistry[typeof(TBlock)]];
        }

        [Obsolete("Please use GetBlock<T:Block>")]
        public static Block GetBlock(string unlocalizedName)
        {
            if (Registry.TryGetValue(unlocalizedName, out var block))
                return block;

            return GetBlock<BlockAir>();
        }
    }
}