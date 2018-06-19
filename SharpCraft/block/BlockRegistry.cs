using System;
using System.Collections.Generic;
using System.Linq;
using SharpCraft.model;
using SharpCraft.render.shader;

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
            Block.SetDefaultShader(new Shader<ModelBlock>("block"));

            foreach (var pair in _registry)
            {
                pair.Value.RegisterState(loader, new BlockState(pair.Value, JsonModelLoader.GetModelForBlock(pair.Key)));
            }
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
            if (_registry.TryGetValue(unlocalizedName, out var block))
                return block;

            return GetBlock<BlockAir>();
        }
    }
}