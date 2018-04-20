using System.Collections.Generic;
using SharpCraft.model;

namespace SharpCraft.block
{

    abstract class Block
    {
        private Dictionary<short, BlockState> _otherStates = new Dictionary<short, BlockState>();

        public BlockState DefaultState { get; private set; }

        public string UnlocalizedName { get; protected set; }

        public int Hardness { get; protected set; } = 8;

        public bool IsOpaque { get; protected set; } = true;
        public bool HasTransparency { get; protected set; }
        public bool IsSolid { get; protected set; } = true;

        protected Block(string unlocalizedName)
        {
            UnlocalizedName = unlocalizedName;
        }

        public virtual void OnRegisterStates()
        {

        }

        protected void RegisterState(short meta, BlockState state)
        {
            _otherStates.Add(meta, state);
        }

        public BlockState GetStateFromMeta(short meta)
        {
            if (meta <= 0 || !_otherStates.TryGetValue(meta, out var state))
                return DefaultState;

            return state;
        }
    }

    class BlockState
    {
        public Block Block { get; }
        public ModelBlock Model { get; }
    }

    internal class BlockNode
    {
        public ModelBlock Model { get; }
        public int meta { get; }

        public BlockNode(ModelBlock model, int meta)
        {
            Model = model;

            this.meta = meta;
        }
    }
}
