using SharpCraft.entity;
using SharpCraft.model;
using SharpCraft.render.shader;
using System.Collections.Generic;
using OpenTK;
using SharpCraft.world;

namespace SharpCraft.block
{
    public abstract class Block
    {
        public static Shader<ModelBlock> DefaultShader { get; private set; } = new Shader<ModelBlock>("block");

        private readonly List<BlockState> _states = new List<BlockState>();

        public Material Material { get; }

        public AxisAlignedBB BoundingBox { get; protected set; } = AxisAlignedBB.BLOCK_FULL;

        public string UnlocalizedName { get; protected set; }

        public int StateCount => _states.Count;

        public int Hardness { get; protected set; } = 8;

        public bool CanBeInteractedWith { get; protected set; } = false;
        public bool IsOpaque { get; protected set; } = true;
        public bool HasTransparency { get; protected set; }
        //public bool IsSolid { get; protected set; } = true;
        public bool IsFullCube { get; protected set; } = true;
        public bool IsReplaceable { get; protected set; } = false;

        protected Block(Material material, string unlocalizedName)
        {
            UnlocalizedName = unlocalizedName;
            Material = material;
        }

        public void RegisterState(JsonModelLoader loader, BlockState state)
        {
            _states.Add(state);
        }

        public BlockState GetState(short meta = 0)
        {
            return _states[meta > 0 ? meta : 0];
        }

        public short GetMetaFromState(BlockState state)
        {
            return (short)_states.IndexOf(state);
        }

        public static void SetDefaultShader(Shader<ModelBlock> shader)
        {
            DefaultShader = shader;
        }

        public virtual void OnRightClicked(MouseOverObject moo, EntityPlayerSP clicked)
        {

        }

        public virtual void OnPlaced(World world, BlockPos pos, EntityPlayerSP placer)
        {

        }

        public virtual void OnDestroyed(BlockPos pos, BlockState oldState, EntityPlayerSP player)
        {
            player.World.RemoveTileEntity(pos);
        }

        public override string ToString()
        {
            //TODO - localization
            return UnlocalizedName;
        }
    }
}