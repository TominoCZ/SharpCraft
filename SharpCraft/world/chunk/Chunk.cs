using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpCraft
{
    class Chunk
    {
        private short[,,] _chunkBlocks;

        public bool isDirty { get; private set; }

        public BlockPos chunkPos { get; }

        public AxisAlignedBB boundingBox { get; }

        public Chunk(BlockPos chunkPos)
        {
            this.chunkPos = chunkPos;
            boundingBox =
                new AxisAlignedBB(Vector3.Zero, Vector3.One * 16 + Vector3.UnitY * 240).offset(chunkPos.vector);

            _chunkBlocks = new short[16, 256, 16];
        }

        private Chunk(ChunkCache cache)
        {
            chunkPos = cache.chunkPos;
            boundingBox =
                new AxisAlignedBB(Vector3.Zero, Vector3.One * 16 + Vector3.UnitY * 240).offset(chunkPos.vector);

            _chunkBlocks = cache.chunkBlocks;
        }

        public static Chunk CreateFromCache(ChunkCache cache)
        {
            return new Chunk(cache);
        }

        public void setBlock(BlockPos pos, EnumBlock blockType, int meta)
        {
            short id = (short)((short)blockType << 4 | meta);

            _chunkBlocks[pos.x, pos.y, pos.z] = id;
        }

        public EnumBlock getBlock(World w, BlockPos pos)
        {
            if (isPosInChunk(pos))
                return (EnumBlock)(_chunkBlocks[pos.x, pos.y, pos.z] >> 4);

            var block = w.getBlock(pos + chunkPos);

            return block;
        }

        public int getMetadata(World w, BlockPos pos)
        {
            if (isPosInChunk(pos))
                return _chunkBlocks[pos.x, pos.y, pos.z] & 15;

            return w.getMetadata(pos + chunkPos);
        }

        public void setMetadata(World w, BlockPos pos, int meta, bool redraw)
        {
            if (isPosInChunk(pos))
            {
                var id = _chunkBlocks[pos.x, pos.y, pos.z];

                _chunkBlocks[pos.x, pos.y, pos.z] = (short)(id & 4095 | meta);
            }

            w.setMetadata(pos + chunkPos, meta, redraw);
        }

        private bool isPosInChunk(BlockPos pos)
        {
            return
                pos.x >= 0 && pos.x < 16 &&
                pos.y >= 0 && pos.y < 256 &&
                pos.z >= 0 && pos.z < 16;
        }

        public ModelChunk createChunkModel(World w, ModelChunk previousChunkModel, bool updateContainingEntities)
        {
            List<RawQuad> quads;

            var MODEL_RAW = new Dictionary<ShaderProgram, List<RawQuad>>();

            //generate the model / fill MODEL_RAW
            for (int z = 0; z < 16; z++)
            {
                for (int y = 0; y < 256; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        var pos = new BlockPos(x, y, z);

                        var block = getBlock(w, pos);

                        if (block == EnumBlock.AIR)
                            continue;

                        var blockModel = ModelRegistry.getModelForBlock(block, getMetadata(w, pos));

                        if (!MODEL_RAW.TryGetValue(blockModel.shader, out quads))
                            MODEL_RAW.Add(blockModel.shader, quads = new List<RawQuad>());

                        for (int i = 0; i < FacingUtil.SIDES.Length; i++)
                        {
                            var dir = FacingUtil.SIDES[i];
                            var block_o = getBlock(w, pos.offset(dir));

                            if (block_o == EnumBlock.AIR || (block_o == EnumBlock.GLASS && block != EnumBlock.GLASS))
                            {
                                var quad = ((ModelBlockRaw)blockModel.rawModel)?.getQuadForSide(dir)?.offset(pos);

                                if (quad != null)
                                    quads.Add(quad);
                            }
                        }
                    }
                }
            }

            var previousShaders = previousChunkModel.getShadersPresent();

            var finish = new ThreadLock(() =>
            {
                var newShaders = MODEL_RAW.Keys.ToArray();

                for (int i = 0; i < previousShaders.Count; i++)
                {
                    var oldShader = previousShaders[i];

                    if (!newShaders.Contains(oldShader))
                    {
                        previousChunkModel.getFragmentModelWithShader(oldShader).overrideData(new List<RawQuad>());
                    }
                }

                foreach (var value in MODEL_RAW)
                {
                    var newShader = value.Key;
                    var newData = value.Value;

                    if (!previousShaders.Contains(newShader))
                    {
                        var newFragment = new ModelChunkFragment(newShader, newData);
                        previousChunkModel.setFragmentModelWithShader(newShader, newFragment);
                    }
                    else
                    {
                        previousChunkModel.getFragmentModelWithShader(newShader).overrideData(newData);
                    }
                }
            });
            Game.MAIN_THREAD_QUEUE.Add(finish);
            finish.WaitFor();

            if (updateContainingEntities)
            {
                w.markNeighbourChunksDirty(chunkPos);

                for (var index = 0; index < w._entities.Count; index++)
                {
                    var entity = w._entities[index];

                    var pos1 = chunkPos.ChunkPos();
                    var pos2 = new BlockPos(entity.pos).ChunkPos();

                    if (pos1.x == pos2.x && pos1.z == pos2.z)
                    {
                        int height = w.getHeightAtPos(entity.pos.X, entity.pos.Z);

                        if (entity.pos.Y < height)
                            entity.teleportTo(new Vector3(entity.pos.X, entity.lastPos.Y = height, entity.pos.Z));
                    }
                }
            }

            isDirty = false;

            return previousChunkModel;
        }

        public void markDirty()
        {
            isDirty = true;
        }

        public ChunkCache createChunkCache()
        {
            return new ChunkCache(chunkPos, _chunkBlocks);
        }
    }

    [Serializable]
    internal struct ChunkCache
    {
        private readonly BlockPos _chunkPos;
        private readonly short[,,] _chunkBlocks;

        public BlockPos chunkPos => _chunkPos;
        public short[,,] chunkBlocks => _chunkBlocks;

        public ChunkCache(BlockPos chunkPos, short[,,] chunkBlocks)
        {
            _chunkPos = chunkPos;
            _chunkBlocks = chunkBlocks;
        }
    }
}