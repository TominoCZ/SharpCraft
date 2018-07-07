using SharpCraft.model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpCraft.json;
using SharpCraft.util;

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
            loader.LoadBlockModels();

            foreach (var block in Registry.Values)
            {
                var count = JsonModelLoader.GetModelCount(block);

                for (var index = 0; index < count; index++)
                {
                    var state = new BlockState(block, (short)index);
                    block.RegisterState(loader, state);
                }
            }
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