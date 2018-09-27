using OpenTK;
using SharpCraft.entity;
using SharpCraft.item;
using SharpCraft.world;

namespace SharpCraft.block
{
    internal class BlockTNT : Block
    {
        public BlockTNT() : base(Material.GetMaterial("grass"))
        {
            SetUnlocalizedName("sharpcraft", "tnt");
        }

        public override TileEntity CreateTileEntity(World world, BlockPos pos)
        {
            return new TileEntityTNT(pos, world);
        }
    }

    public class TileEntityTNT : TileEntity
    {
        private BlockPos _pos;
        private int _ticksBeforeExplosion = 60;

        public TileEntityTNT(BlockPos pos, World world) : base(world)
        {
            _pos = pos;
        }

        public override void Update()
        {
            if (_ticksBeforeExplosion == 0)
            {
                OnExplode();
            }

            _ticksBeforeExplosion--;
        }

        private void OnExplode()
        {
            var state = World.GetBlockState(_pos);

            state.Block.OnDestroyed(World, _pos, state, null);

            int radius = 3;

            var air = BlockRegistry.GetBlock<BlockAir>().GetState();

            for (int y = -radius; y < radius; y++)
            {
                for (int x = -radius; x < radius; x++)
                {
                    for (int z = -radius; z < radius; z++)
                    {
                        var vec = new Vector3(x, y, z);
                        var distance = vec.LengthFast;

                        if (distance <= radius)
                        {
                            var pos = new BlockPos(_pos.ToVec() + vec + Vector3.One * 0.5f);
                            var oldState = World.GetBlockState(pos);

                            if (oldState.Block is BlockAir)
                                continue;

                            World.SetBlockState(pos, air);

                            SharpCraft.Instance.World.AddEntity(new EntityItem(World, pos.ToVec(),
                                Vector3.UnitY * 0.2f, ItemRegistry.GetItemStack(oldState)));
                        }
                    }
                }
            }

            //var chunkLocalPos = new BlockPos(MathUtil.ToLocal(_pos.X, Chunk.ChunkSize), MathUtil.ToLocal(_pos.Y, Chunk.ChunkSize), MathUtil.ToLocal(_pos.Z, Chunk.ChunkSize));

            //World.GetChunk(_pos.ChunkPos()).NotifyModelChange(chunkLocalPos);
        }
    }
}