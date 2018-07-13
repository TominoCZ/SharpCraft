using SharpCraft.entity;
using SharpCraft.model;
using SharpCraft.render.shader;
using SharpCraft.world;
using System.Collections.Generic;
using SharpCraft.json;

namespace SharpCraft.block
{
    public abstract class Block
    {
        public static Shader DefaultShader { get; private set; }

        private readonly List<BlockState> _states = new List<BlockState>();

        public Material Material { get; }

        public AxisAlignedBb BoundingBox { get; protected set; } = AxisAlignedBb.BlockFull;

        public string UnlocalizedName { get; private set; }

        public int StateCount => _states.Count;

        public int Hardness { get; protected set; } = 8;

        public bool IsOpaque { get; protected set; } = true;
        public bool HasTransparency { get; protected set; }

        //public bool IsSolid { get; protected set; } = true;
        public bool IsFullCube { get; protected set; } = true;

        public bool IsReplaceable { get; protected set; } = false;

        protected Block(Material material)
        {
            Material = material;
        }

        protected void SetUnlocalizedName(string modid, string unlocalizedName)
        {
            UnlocalizedName = modid + ".block." + unlocalizedName;
        }

        public void RegisterState(JsonModelLoader loader, BlockState state)
        {
            _states.Add(state);
        }

        public BlockState GetState(short meta = 0)
        {
            return _states[meta >= _states.Count ? _states.Count - 1 : (meta < 0 ? 0 : meta)];
        }

        public short GetMetaFromState(BlockState state)
        {
            return (short)_states.IndexOf(state);
        }

        public static void SetDefaultShader(Shader shader)
        {
            DefaultShader = shader;
        }

        /// <summary>
        /// If the returned value is true, the held block is not going to be placed.
        /// </summary>
        /// <param name="moo"></param>
        /// <param name="clicked"></param>
        /// <returns></returns>
        public virtual bool OnActivated(MouseOverObject moo, EntityPlayerSp clicked)
        {
            return false;
        }

        public virtual bool CanBlockBePlacedAtSide(World world, BlockPos blockPos, FaceSides sideHit, EntityPlayerSp placer)
        {
            return true;
        }

        public virtual void OnPlaced(World world, BlockPos pos, EntityPlayerSp placer)
        {
        }

        public virtual void OnDestroyed(World world, BlockPos pos, BlockState oldState, EntityPlayerSp destroyer)
        {
            world.RemoveTileEntity(pos);
        }

        public virtual TileEntity CreateTileEntity(World world, BlockPos pos)
        {
            return null;
        }
    }
}