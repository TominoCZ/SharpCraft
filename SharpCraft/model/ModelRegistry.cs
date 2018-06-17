using SharpCraft.block;
using System.Collections.Generic;

namespace SharpCraft.model
{
    internal class ModelRegistry
    {
        private static Dictionary<EnumBlock, List<BlockNode>> models = new Dictionary<EnumBlock, List<BlockNode>>();

        public static void RegisterBlockModel(ModelBlock model, int meta, int hardness = 10)
        {
            //if already contains state with this meta tag, remove and set a new one
            if (models.TryGetValue(model.block, out List<BlockNode> states))
            {
                for (int i = 0; i < states.Count; i++)
                {
                    BlockNode state = states[i];

                    if (state.meta == meta)
                    {
                        states.Remove(state);
                        break;
                    }
                }
            }
            else
            {
                models.Add(model.block, states = new List<BlockNode>());
            }

            states.Add(new BlockNode(model, meta));
        }

        public static ModelBlock GetModelForBlock(EnumBlock blockType, int meta)
        {
            return GetBlockState(blockType, meta < 0 ? 0 : meta)?.Model;
        }

        public static BlockNode GetBlockState(EnumBlock blockType, int meta)
        {
            meta = meta < 0 ? 0 : meta;

            if (blockType == EnumBlock.AIR || !models.TryGetValue(blockType, out List<BlockNode> states))
                return models[EnumBlock.MISSING][0];

            foreach (BlockNode node in states)
            {
                if (node.meta == meta)
                    return node;
            }

            return null;
        }
    }
}