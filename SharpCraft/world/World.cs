using OpenTK;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SharpCraft
{
    internal class World
    {
        private ConcurrentDictionary<BlockPos, ChunkData> _chunks;

        public List<Entity> _entities;

        public int BuildHeight = 256;

        public readonly int seed;

        private NoiseUtil _noiseUtil;

        public World(int seed)
        {
            _chunks = new ConcurrentDictionary<BlockPos, ChunkData>();
            _entities = new List<Entity>();

            _noiseUtil = new NoiseUtil(seed);
            _noiseUtil.SetFractalType(NoiseUtil.FractalType.FBM);

            this.seed = seed;
        }

        private World(int seed, List<ChunkCache> caches) : this(seed)
        {
            foreach (var cache in caches)
            {
                var pos = cache.chunkPos;

                var chunk = Chunk.CreateFromCache(cache);
                var model = new ModelChunk();

                var data = new ChunkData(chunk, model);
                data.chunkGenerated = true;

                _chunks.TryAdd(pos, data);
            }
        }

        public static World Create(int seed, List<ChunkCache> caches)
        {
            return new World(seed, caches);
        }

        public void addEntity(Entity e)
        {
            if (!_entities.Contains(e))
                _entities.Add(e);
        }

        public void updateEntities()
        {
            for (int i = 0; i < _entities.Count; i++)
            {
                _entities[i].Update();
            }
        }

        private ChunkData addChunkPlaceholder(BlockPos pos)
        {
            var data = new ChunkData(new Chunk(pos = pos.ChunkPos()), new ModelChunk());

            _chunks.TryAdd(pos, data);

            return data;
        }

        public List<AxisAlignedBB> getIntersectingEntitiesBBs(AxisAlignedBB with)
        {
            List<AxisAlignedBB> bbs = new List<AxisAlignedBB>();

            for (int i = 0; i < _entities.Count; i++)
            {
                var bb = _entities[i].getEntityBoundingBox();

                if (bb.intersectsWith(with))
                    bbs.Add(bb);
            }

            return bbs;
        }

        public List<AxisAlignedBB> getBlockCollisionBoxes(AxisAlignedBB box)
        {
            List<AxisAlignedBB> blocks = new List<AxisAlignedBB>();

            var bb = box.union(box);

            for (int x = (int)bb.min.X, maxX = (int)bb.max.X; x < maxX; x++)
            {
                for (int y = (int)bb.min.Y, maxY = (int)bb.max.Y; y < maxY; y++)
                {
                    for (int z = (int)bb.min.Z, maxZ = (int)bb.max.Z; z < maxZ; z++)
                    {
                        var pos = new BlockPos(x, y, z);
                        var block = Game.INSTANCE.world.getBlock(pos);
                        if (block == EnumBlock.AIR)
                            continue;

                        blocks.Add(
                            ModelRegistry.getModelForBlock(block, getMetadata(pos)).boundingBox.offset(pos.vector));
                    }
                }
            }

            return blocks;
        }

        public Chunk getChunkFromPos(BlockPos pos)
        {
            if (!_chunks.TryGetValue(pos.ChunkPos(), out var chunkData))
                return null;

            return chunkData?.chunk;
        }

        public ChunkData[] getChunkDataNodes()
        {
            return _chunks.Values.ToArray();
        }

        public EnumBlock getBlock(BlockPos pos)
        {
            var chunk = getChunkFromPos(pos);
            if (chunk == null)
                return EnumBlock.AIR;

            return chunk.getBlock(this, pos - chunk.chunkPos);
        }

        public void setBlock(BlockPos pos, EnumBlock blockType, int meta, bool markDirty)
        {
            var chunk = getChunkFromPos(pos);
            if (chunk == null)
                return;

            chunk.setBlock(pos - chunk.chunkPos, blockType, meta);

            if (markDirty)
            {
                beginUpdateModelForChunk(pos);
                markNeighbourChunksDirty(pos);
            }
        }

        public void unloadChunk(BlockPos pos)
        {
            var chunkPos = pos.ChunkPos();

            if (_chunks.TryGetValue(chunkPos, out var data))
                data.model.destroy();
        }

        public void destroyChunkModels()
        {
            var data = getChunkDataNodes();

            for (var index = 0; index < data.Length; index++)
            {
                var chunkData = data[index];
                chunkData.model.destroy();
            }
        }

        private void markNeighbourChunksDirty(BlockPos pos, bool chunks = false)
        {
            for (var index = 0; index < FacingUtil.SIDES.Length - 2; index++)
            {
                EnumFacing side = FacingUtil.SIDES[index];

                var p = chunks ? pos.offsetChunk(side) : pos.offset(side);
                var ch = getChunkFromPos(p);

                ch?.markDirty();
            }
        }

        public int getMetadata(BlockPos pos)
        {
            var chunk = getChunkFromPos(pos);
            if (chunk == null)
                return 0;

            return chunk.getMetadata(this, pos - chunk.chunkPos);
        }

        public void setMetadata(BlockPos pos, int meta, bool redraw)
        {
            var chunk = getChunkFromPos(pos);
            if (chunk == null)
                return;

            chunk.setMetadata(this, pos - chunk.chunkPos, meta, redraw);

            if (redraw)
            {
                chunk.markDirty();
                markNeighbourChunksDirty(pos);
            }
        }

        public int getHeightAtPos(float x, float z)
        {
            //TODO this code only for 2D

            var pos = new BlockPos(x, 256, z);

            var chunk = getChunkFromPos(new BlockPos(pos.x, 0, pos.z));

            if (chunk == null)
                return 0;//ThreadPool.ScheduleTask(false, () => generateChunk(pos));

            var lastPos = pos;

            for (int y = BuildHeight - 1; y >= 0; y--)
            {
                var block = getBlock(lastPos = lastPos.offset(EnumFacing.DOWN));

                if (block != EnumBlock.AIR)
                    return y + 1;
            }

            return 0;
        }

        public void beginGenerateChunk(BlockPos pos)
        {
            var chunkPos = pos.ChunkPos();

            if (_chunks.ContainsKey(chunkPos))
                return;

            var data = addChunkPlaceholder(chunkPos);

            ThreadPool.QueueUserWorkItem(e =>
            {
                for (int z = 0; z < 16; z++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        var X = (x + chunkPos.x) / 1.25f;
                        var Y = (z + chunkPos.z) / 1.25f;

                        int peakY = 32 + (int)Math.Abs(
                                        MathHelper.Clamp(0.35f + _noiseUtil.GetPerlinFractal(X, Y), 0, 1) * 30);

                        for (int y = peakY; y >= 0; y--)
                        {
                            var p = new BlockPos(x, y, z);

                            if (y == peakY)
                                data.chunk.setBlock(p, EnumBlock.GRASS, 0);
                            else if (y > 0 && peakY - y > 0 && peakY - y < 3) // for 2 blocks
                                data.chunk.setBlock(p, EnumBlock.DIRT, 0);
                            else if (y == 0)
                                data.chunk.setBlock(p, EnumBlock.BEDROCK, 0);
                            else
                            {
                                var f = _noiseUtil.GetNoise(X * 32 - y * 16, Y * 32 + x * 16);

                                data.chunk.setBlock(p, f >= 0.75f ? EnumBlock.RARE : EnumBlock.STONE, 0);
                            }
                        }

                        float treeSeed = Math.Abs(MathHelper.Clamp(_noiseUtil.GetWhiteNoise(X, Y), 0, 1));
                        float treeSeed2 = Math.Abs(MathHelper.Clamp(0.35f + _noiseUtil.GetPerlinFractal(Y, X), 0, 1));

                        if (treeSeed >= 0.995f && treeSeed2 >= 0.233f)
                        {
                            for (int treeY = 0; treeY < 5; treeY++)
                            {
                                data.chunk.setBlock(new BlockPos(x, peakY + 1 + treeY, z), EnumBlock.RARE, 1);
                            }
                        }
                    }
                }

                data.chunkGenerated = true;
            });
        }

        public void beginUpdateModelForChunk(BlockPos pos, bool updateContainingEntities = false)
        {
            pos = pos.ChunkPos();

            if (_chunks.TryGetValue(pos, out var node))
            {
                node.beginUpdateModel(this, updateContainingEntities);
            }
        }

        public bool areNeighbourChunksGenerated(BlockPos pos)
        {
            pos = pos.ChunkPos();

            for (var index = 0; index < FacingUtil.SIDES.Length - 2; index++)
            {
                var face = FacingUtil.SIDES[index];

                if (!isChunkGenerated(pos.offsetChunk(face)))
                    return false;
            }

            return true;
        }

        public bool doesChunkHaveModel(BlockPos pos)
        {
            var n = _chunks[pos.ChunkPos()];

            return n.chunkGenerated && n.model.isGenerated;
        }

        public bool isChunkGenerated(BlockPos pos)
        {
            if (_chunks.TryGetValue(pos.ChunkPos(), out var n))
                return n.chunkGenerated;

            return false;
        }
    }
}