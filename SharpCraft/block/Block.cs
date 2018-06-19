using SharpCraft.model;
using SharpCraft.render.shader;
using System.Collections.Generic;
using SharpCraft.entity;

namespace SharpCraft.block
{
    public abstract class Block
    {
        private static readonly Shader<ModelBlock> DefaultShader = new Shader<ModelBlock>("block");
        public Shader<ModelBlock> Shader { get; protected set; }

        private List<BlockState> _states = new List<BlockState>();

        public AxisAlignedBB BoundingBox { get; protected set; } = AxisAlignedBB.BLOCK_FULL;

        public string UnlocalizedName { get; protected set; }

        public int StateCount => _states.Count;

        public int Hardness { get; protected set; } = 8;

        public bool CanBeInteractedWith { get; protected set; } = false;
        public bool IsOpaque { get; protected set; } = true;
        public bool HasTransparency { get; protected set; }
        public bool IsSolid { get; protected set; } = true;

        protected Block(string unlocalizedName)
        {
            UnlocalizedName = unlocalizedName;
            Shader = DefaultShader;
        }

        public BlockState GetState(short meta)
        {
            return _states[meta > 0 ? meta : 0];
        }

        public short GetMetaFromState(BlockState state)
        {
            return (short)_states.IndexOf(state);
        }
    }

    public class BlockState
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