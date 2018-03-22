using OpenTK;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

        public List<AxisAlignedBB> getIntersectingEntitiesBBs(AxisAlignedBB with)
        {
            List<AxisAlignedBB> bbs = new List<AxisAlignedBB>();

            for (int i = 0; i < _entities.Count; i++)
            {
                var bb = _entities[i].getBoundingBox();

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

        public void setBlock(BlockPos pos, EnumBlock blockType, int meta, bool redraw)
        {
            var chunk = getChunkFromPos(pos);
            if (chunk == null)
            {
                var chp = pos.ChunkPos();

                ThreadPool.RunTask(true, () =>
                {
                    generateChunk(chp, false);
                    setBlock(pos, blockType, meta, redraw);
                });

                return;
            }

            chunk.setBlock(pos - chunk.chunkPos, blockType, meta);

            if (redraw)
            {
                updateModelForChunk(chunk.chunkPos);
                markNeighbourChunksForUpdate(chunk, pos);
            }
        }

        private void markChunksOnSidesForUpdate(BlockPos pos)
        {
            var sw = Stopwatch.StartNew();

            int chunksUpdated = 0;

            for (var index = 0; index < FacingUtil.SIDES.Length - 2; index++) //TODO - negative 2 with 2D chunks
            {
                EnumFacing side = FacingUtil.SIDES[index];

                var p = pos.offsetChunk(side);
                var ch = getChunkFromPos(p);

                if (ch != null)
                {
                    updateModelForChunk(ch.chunkPos);
                    chunksUpdated++;
                }
            }

            sw.Stop();

            Console.WriteLine($"DEBUG: updated chunks on sides [{sw.Elapsed.TotalMilliseconds:F}ms] ({chunksUpdated} {(chunksUpdated > 1 ? "chunks" : "chunk")})");
        }

        private void markNeighbourChunksForUpdate(Chunk chunk, BlockPos pos)
        {
            var sw = Stopwatch.StartNew();

            int chunksUpdated = 1;

            for (var index = 0; index < FacingUtil.SIDES.Length - 2; index++)
            {
                EnumFacing side = FacingUtil.SIDES[index];

                var p = pos.offset(side);
                var ch = getChunkFromPos(p);

                if (ch != chunk && ch != null)
                {
                    updateModelForChunk(ch.chunkPos);
                    chunksUpdated++;
                }
            }

            sw.Stop();

            Console.WriteLine(
                $"DEBUG: built terrain model [{sw.Elapsed.TotalMilliseconds:F}ms] ({chunksUpdated} {(chunksUpdated > 1 ? "chunks" : "chunk")})");
        }

        public EnumBlock getBlock(BlockPos pos)
        {
            var chunk = getChunkFromPos(pos);
            if (chunk == null)
                return EnumBlock.AIR;

            return chunk.getBlock(this, pos - chunk.chunkPos);
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
                updateModelForChunk(chunk.chunkPos);
                markNeighbourChunksForUpdate(chunk, pos);
            }
        }

        public int getHeightAtPos(int x, int z)
        {
            for (int y = BuildHeight - 1; y >= 0; y--)
            {
                var pos = new BlockPos(x, y, z);

                var chunk = getChunkFromPos(pos);

                if (chunk == null)
                    ThreadPool.RunTask(false, () => generateChunk(pos, false));

                var block = getBlock(pos);

                if (block != EnumBlock.AIR)
                    return y + 1;
            }

            return 0;
        }

        public void generateChunk(BlockPos pos, bool redraw)
        {
            var chunkPos = pos.ChunkPos();

            if (_chunks.ContainsKey(chunkPos))
                return;

            var chunk = new Chunk(chunkPos);
            var data = new ChunkData(chunk, new ModelChunk());

            _chunks.TryAdd(chunkPos, data);

            for (int z = 0; z < 16; z++)
            {
                for (int x = 0; x < 16; x++)
                {
                    var X = (x + chunkPos.x) / 1.25f;
                    var Y = (z + chunkPos.z) / 1.25f;

                    int peakY = 32 + (int)Math.Abs(MathHelper.Clamp(0.35f + _noiseUtil.GetPerlinFractal(X, Y), 0, 1) * 30);

                    for (int y = peakY; y >= 0; y--)
                    {
                        var p = new BlockPos(x, y, z);

                        if (y == peakY)
                            chunk.setBlock(p, EnumBlock.GRASS, 0);
                        else if (y > 0 && peakY - y > 0 && peakY - y < 3) // for 2 blocks
                            chunk.setBlock(p, EnumBlock.DIRT, 0);
                        else if (y == 0)
                            chunk.setBlock(p, EnumBlock.BEDROCK, 0);
                        else
                        {
                            var f = 0.35f + _noiseUtil.GetNoise(X * 32 - y * 16, Y * 32 + x * 16);

                            chunk.setBlock(p, f >= 0.75f ? EnumBlock.RARE : EnumBlock.STONE, 0);
                        }
                    }
                }
            }

            if (redraw)
            {
                updateModelForChunk(chunkPos);

                markChunksOnSidesForUpdate(chunkPos);
            }

            data.chunkGenerated = true;

            /* var sides = (EnumFacing[]) Enum.GetValues(typeof(EnumFacing));

             for (var index = 0; index < sides.Length - 2; index++)
             {
                 var side = sides[index];

                 var vec = new BlockPos().offset(side).vector;
                 var offset = new BlockPos(vec * 16);

                 var c = getChunkFromPos(offset + chunkPos);

                 if (c != null)
                     updateModelForChunk(c.chunkPos);
             }*/
        }

        public void updateModelForChunk(BlockPos pos)
        {
            if (_chunks.TryGetValue(pos.ChunkPos(), out var node))
            {
                if (!node.chunkGenerated || node.modelGenerating)
                    return;

                ThreadPool.RunTask(false, () =>
                {
                    node.modelGenerating = true;
                    var model = node.chunk.generateModel(this, node.model);
                    node.model = model;
                    node.modelGenerating = false;
                });
            }
        }

        public bool doesChunkHaveModel(BlockPos pos)
        {
            var n = _chunks[pos.ChunkPos()];

            return n.chunkGenerated && n.model.isGenerated;
        }
    }
}