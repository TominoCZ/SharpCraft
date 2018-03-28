using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using OpenTK;
using SharpCraft.block;
using SharpCraft.entity;
using SharpCraft.model;
using SharpCraft.util;
using SharpCraft.world.chunk;

namespace SharpCraft.world
{
    internal class World
    {
        public ConcurrentDictionary<BlockPos, ChunkData> Chunks { get; }

        public List<Entity> Entities;

        public int BuildHeight = 256;

        public readonly int Seed;
        public readonly string LevelName;

        private NoiseUtil _noiseUtil;
        private int _dimension = 0;
        private ChunkDataManager _chunkManager;
        public readonly String SaveRoot;

        public World(string saveName, string levelName, int seed)
        {
            Chunks = new ConcurrentDictionary<BlockPos, ChunkData>();
            Entities = new List<Entity>();

            _noiseUtil = new NoiseUtil(seed);
            _noiseUtil.SetFractalType(NoiseUtil.FractalType.FBM);

            Seed = seed;
            LevelName = levelName;
            SaveRoot = $"SharpCraft_Data/saves/{saveName}/";
            _chunkManager = new ChunkDataManager($"{SaveRoot}{_dimension}/chunks",
                new RegionInfo(new[] {12,12}, 2 * 16 * 256 * 16));
        }

        public void AddEntity(Entity e)
        {
            if (!Entities.Contains(e))
                Entities.Add(e);
        }

        public void UpdateEntities()
        {
            for (var i = 0; i < Entities.Count; i++)
            {
                Entities[i].Update();
            }
        }

        private ChunkData AddChunkPlaceholder(BlockPos pos)
        {
            var data = new ChunkData(new Chunk(pos = pos.chunkPos(), this), new ModelChunk());

            Chunks.TryAdd(pos, data);

            return data;
        }

        public List<AxisAlignedBB> GetIntersectingEntitiesBBs(AxisAlignedBB with)
        {
            var bbs = new List<AxisAlignedBB>();

            for (var i = 0; i < Entities.Count; i++)
            {
                var bb = Entities[i].getEntityBoundingBox();

                if (bb.intersectsWith(with))
                    bbs.Add(bb);
            }

            return bbs;
        }

        public List<AxisAlignedBB> GetBlockCollisionBoxes(AxisAlignedBB box)
        {
            var blocks = new List<AxisAlignedBB>();

            var bb = box.union(box);

            for (int x = (int) bb.min.X, maxX = (int) bb.max.X; x < maxX; x++)
            {
                for (int y = (int) bb.min.Y, maxY = (int) bb.max.Y; y < maxY; y++)
                {
                    for (int z = (int) bb.min.Z, maxZ = (int) bb.max.Z; z < maxZ; z++)
                    {
                        var pos = new BlockPos(x, y, z);
                        var block = Game.Instance.World.GetBlock(pos);
                        if (block == EnumBlock.AIR)
                            continue;

                        blocks.Add(
                            ModelRegistry.getModelForBlock(block, GetMetadata(pos)).boundingBox.offset(pos.toVec()));
                    }
                }
            }

            return blocks;
        }

        public Chunk GetChunkFromPos(BlockPos pos)
        {
            if (!Chunks.TryGetValue(pos.chunkPos(), out var chunkData))
                return null;

            return chunkData?.Chunk;
        }

        public EnumBlock GetBlock(BlockPos pos)
        {
            var chunk = GetChunkFromPos(pos);
            if (chunk == null)
                return EnumBlock.AIR;

            return chunk.GetBlock(pos - chunk.ChunkPos);
        }

        public void SetBlock(BlockPos pos, EnumBlock blockType, int meta, bool markDirty)
        {
            var chunk = GetChunkFromPos(pos);
            if (chunk == null)
                return;

            chunk.SetBlock(pos - chunk.ChunkPos, blockType, meta);

            if (markDirty)
            {
                BeginUpdateModelForChunk(pos);

                MarkNeighbourChunksDirty(pos);
            }
        }

        public void UnloadChunk(BlockPos pos)
        {
            if (Chunks.TryRemove(pos, out var data)) // && data.model.isGenerated)
            {
                data.Model.destroy();

                SaveChunk(data);
            }
        }

        private void SaveChunk(ChunkData chunk)
        {
            if (!chunk.Chunk.NeedsSave) return;
            chunk.Chunk.NeedsSave = false;
            
            //Console.WriteLine($"Saving chunk @ {chunk.Chunk.ChunkPos.x / 16} x {chunk.Chunk.ChunkPos.z / 16}");
            var data = new byte[_chunkManager.Info.ChunkByteSize];
            Buffer.BlockCopy(chunk.Chunk.ChunkBlocks, 0, data, 0, data.Length);
            _chunkManager.WriteChunkData(new[] {chunk.Chunk.ChunkPos.x / 16, chunk.Chunk.ChunkPos.z / 16}, data);
        }

        public bool LoadChunk(BlockPos pos)
        {
            var chunkPos = pos.chunkPos();

            var data = _chunkManager.GetChunkData(new[] {chunkPos.x / 16, chunkPos.z / 16});
            if (data == null) return false;
            
            var chunkData = AddChunkPlaceholder(chunkPos);

            var blockData = new short[16, 256, 16];
            Buffer.BlockCopy(data, 0, blockData, 0, data.Length);

            var chunk = Chunk.CreateWithData(chunkPos, this, blockData);

            chunkData.Chunk = chunk;
            chunkData.ChunkGenerated = true;

            return true;
        }

        public void SaveAllChunks()
        {
            foreach (var data in Chunks.Values)
            {
                SaveChunk(data);
            }
        }

        public void DestroyChunkModels()
        {
            foreach (var data in Chunks)
            {
                if (!data.Value.ModelGenerating && data.Value.Model.isGenerated)
                    data.Value.Model.destroy();
            }
        }

        private void MarkNeighbourChunksDirty(BlockPos pos)
        {
            var chunk = GetChunkFromPos(pos);

            for (var index = 0; index < FacingUtil.SIDES.Length - 2; index++)
            {
                var side = FacingUtil.SIDES[index];

                var p = pos.offset(side);
                var ch = GetChunkFromPos(p);

                if (ch != chunk)
                    ch?.MarkDirty();
            }
        }

        public int GetMetadata(BlockPos pos)
        {
            var chunk = GetChunkFromPos(pos);
            if (chunk == null)
                return 0;

            return chunk.GetMetadata(pos - chunk.ChunkPos);
        }

        public void SetMetadata(BlockPos pos, int meta, bool redraw)
        {
            var chunk = GetChunkFromPos(pos);
            if (chunk == null)
                return;

            chunk.SetMetadata(pos - chunk.ChunkPos, meta, redraw);

            if (redraw)
            {
                chunk.MarkDirty();
                MarkNeighbourChunksDirty(pos);
            }
        }

        public int GetHeightAtPos(float x, float z)
        {
            //TODO this code only for 2D

            var pos = new BlockPos(x, 256, z);

            var chunk = GetChunkFromPos(new BlockPos(pos.x, 0, pos.z));

            if (chunk == null)
                return 0; //ThreadPool.ScheduleTask(false, () => generateChunk(pos));

            var lastPos = pos;

            for (var y = BuildHeight - 1; y >= 0; y--)
            {
                var block = GetBlock(lastPos = lastPos.offset(EnumFacing.DOWN));

                if (block != EnumBlock.AIR)
                    return y + 1;
            }

            return 0;
        }

        public void BeginGenerateChunk(BlockPos pos, bool updateContainingEntities)
        {
            var chunkPos = pos.chunkPos();

            if (Chunks.ContainsKey(chunkPos))
                return;

            var data = AddChunkPlaceholder(chunkPos);
            ThreadPool.QueueUserWorkItem(e =>
            {
                for (var z = 0; z < 16; z++)
                {
                    for (var x = 0; x < 16; x++)
                    {
                        var xCh= (x + chunkPos.x) / 1.25f;
                        var yCh = (z + chunkPos.z) / 1.25f;

                        var peakY = 32 + (int) Math.Abs(
                                        MathHelper.Clamp(0.35f + _noiseUtil.GetPerlinFractal(xCh, yCh), 0, 1) * 30);

                        for (var y = peakY; y >= 0; y--)
                        {
                            var p = new BlockPos(x, y, z);

                            if (y == peakY)
                                data.Chunk.SetBlock(p, EnumBlock.GRASS, 0);
                            else if (y > 0 && peakY - y > 0 && peakY - y < 3) // for 2 blocks
                                data.Chunk.SetBlock(p, EnumBlock.DIRT, 0);
                            else if (y == 0)
                                data.Chunk.SetBlock(p, EnumBlock.BEDROCK, 0);
                            else
                            {
                                var f = _noiseUtil.GetNoise(xCh * 32 - y * 16, yCh * 32 + x * 16);

                                data.Chunk.SetBlock(p, f >= 0.75f ? EnumBlock.RARE : EnumBlock.STONE, 0);
                            }
                        }

                        var treeSeed = Math.Abs(MathHelper.Clamp(_noiseUtil.GetWhiteNoise(xCh, yCh), 0, 1));
                        var treeSeed2 = Math.Abs(MathHelper.Clamp(0.35f + _noiseUtil.GetPerlinFractal(yCh, xCh), 0, 1));

                        if (treeSeed >= 0.995f && treeSeed2 >= 0.233f)
                        {
                            for (var treeY = 0; treeY < 5; treeY++)
                            {
                                data.Chunk.SetBlock(new BlockPos(x, peakY + 1 + treeY, z), EnumBlock.LOG, 0);
                            }
                        }
                    }
                }

                data.ChunkGenerated = true;

                if (updateContainingEntities)
                {
                    // w.markNeighbourChunksDirty(chunkPos);

                    for (var index = 0; index < Entities.Count; index++)
                    {
                        var entity = Entities[index];

                        var pos1 = chunkPos.chunkPos();
                        var pos2 = new BlockPos(entity.pos).chunkPos();

                        if (pos1.x == pos2.x && pos1.z == pos2.z)
                        {
                            var height = GetHeightAtPos(entity.pos.X, entity.pos.Z);

                            if (entity.pos.Y < height)
                                entity.teleportTo(new Vector3(entity.pos.X, entity.lastPos.Y = height, entity.pos.Z));
                        }
                    }
                }

                data.Chunk.NeedsSave = true;
                SaveChunk(data);
            });
        }

        public void BeginUpdateModelForChunk(BlockPos pos)
        {
            pos = pos.chunkPos();

            if (Chunks.TryGetValue(pos, out var node))
            {
                node.BeginUpdateModel();
            }
        }

        public bool AreNeighbourChunksGenerated(BlockPos pos)
        {
            pos = pos.chunkPos();

            for (var index = 0; index < FacingUtil.SIDES.Length - 2; index++)
            {
                var face = FacingUtil.SIDES[index];

                if (!IsChunkGenerated(pos.offsetChunk(face)))
                    return false;
            }

            return true;
        }

        public bool DoesChunkHaveModel(BlockPos pos)
        {
            var n = Chunks[pos.chunkPos()];

            return n.ChunkGenerated && n.Model.isGenerated;
        }

        public bool IsChunkGenerated(BlockPos pos)
        {
            if (Chunks.TryGetValue(pos.chunkPos(), out var n))
                return n.ChunkGenerated;

            return false;
        }
    }
}