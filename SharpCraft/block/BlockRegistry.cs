using System;
using System.Collections.Generic;
using System.Linq;
using SharpCraft.model;
using SharpCraft.render.shader;

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
            Shader<ModelBlock> shader = new Shader<ModelBlock>("block");
            JsonModelLoader modelLoader = new JsonModelLoader(shader);
        }

        public static List<Block> AllBlocks()
        {
            return _registry.Values.ToList();
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