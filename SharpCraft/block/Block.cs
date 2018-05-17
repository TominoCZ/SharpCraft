using SharpCraft.model;
using SharpCraft.render.shader;
using System.Collections.Generic;

namespace SharpCraft.block
{
    internal abstract class Block
    {
        private static readonly Shader<ModelBlock> DefaultShader = new Shader<ModelBlock>("block");
        public Shader<ModelBlock> Shader { get; protected set; }

        private List<BlockState> _states = new List<BlockState>();

        public string UnlocalizedName { get; protected set; }

        public int StateCount => _states.Count;

        public int Hardness { get; protected set; } = 8;

        public bool IsOpaque { get; protected set; } = true;
        public bool HasTransparency { get; protected set; }
        public bool IsSolid { get; protected set; } = true;

        protected Block(string unlocalizedName)
        {
            UnlocalizedName = unlocalizedName;
            Shader = DefaultShader;
        }

        /// <summary>
        /// Used to register the states of this block. Including the default one. Is called after all blocks are registered
        /// </summary>
        public abstract void OnRegisterStates();

        protected void RegisterState(string modelJson)
        {
            var state = new BlockState(this, new ModelBlock(EnumBlock.MISSING, Shader));

            //TODO LOAD MODEL AND TEXTURE INFO FROM JSON

            _states.Add(state);
        }

        public BlockState GetState(short meta)
        {
            return _states[meta > 0 ? meta : 0];
        }
    }

    internal class BlockState
    {
        public Block Block { get; }
        public ModelBlock Model { get; }

        public BlockState(Block block, ModelBlock model)
        {
            Block = block;
            Model = model;
        }
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