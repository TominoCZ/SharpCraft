using SharpCraft.block;
using System.Collections.Generic;

namespace SharpCraft.model
{
    internal class ModelRegistry
    {
        private static Dictionary<EnumBlock, List<BlockState>> models = new Dictionary<EnumBlock, List<BlockState>>();

        public static void RegisterBlockModel(ModelBlock model, int meta, int hardness = 10)
        {
            List<BlockState> states;

            //if already contains state with this meta tag, remove and set a new one
            if (models.TryGetValue(model.block, out states))
            {
                for (int i = 0; i < states.Count; i++)
                {
                    var state = states[i];

                    if (state.meta == meta)
                    {
                        states.Remove(state);
                        break;
                    }
                }
            }
            else
            {
                models.Add(model.block, states = new List<BlockState>());
            }

            states.Add(new BlockState(model, meta));
        }

        public static ModelBlock GetModelForBlock(EnumBlock blockType, int meta)
        {
            return GetBlockState(blockType, meta < 0 ? 0 : meta)?.Model;
        }

        public static BlockState GetBlockState(EnumBlock blockType, int meta)
        {
            meta = meta < 0 ? 0 : meta;

            if (blockType == EnumBlock.AIR || !models.TryGetValue(blockType, out var states))
                return models[EnumBlock.MISSING][0];

            for (int i = 0; i < states.Count; i++)
            {
                var state = states[i];

                if (state.meta == meta)
                    return state;
            }

            return null;
        }
    }

    internal class BlockState
    {
        public ModelBlock Model { get; }
        public int meta { get; }

        public BlockState(ModelBlock model, int meta)
        {
            Model = model;

            this.meta = meta;
        }
    }
}