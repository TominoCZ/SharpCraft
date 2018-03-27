using OpenTK;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpCraft.world;

namespace SharpCraft
{
    internal class World
    {
        public ConcurrentDictionary<BlockPos, ChunkData> Chunks { get; }

        public List<Entity> Entities;

        public int BuildHeight = 256;

        public readonly int seed;
        public readonly string levelName;

        private NoiseUtil _noiseUtil;
        private int dimension=0;
        private ChunkDataManager _chunkManager;
        public readonly String saveRoot;
        
        public World(string saveName, string levelName, int seed)
        {
            Chunks = new ConcurrentDictionary<BlockPos, ChunkData>();
            Entities = new List<Entity>();

            _noiseUtil = new NoiseUtil(seed);
            _noiseUtil.SetFractalType(NoiseUtil.FractalType.FBM);

            this.seed = seed;
            this.levelName = levelName;
            saveRoot = $"SharpCraft_Data/saves/{saveName}/";
            _chunkManager = new ChunkDataManager($"{saveRoot}{dimension}/chunks", 
                                                 new RegionInfo(new[]{8,8}, sizeof(short)*16*256*16));
        }

        public void addEntity(Entity e)
        {
            if (!Entities.Contains(e))
                Entities.Add(e);
        }

        public void updateEntities()
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                Entities[i].Update();
            }
        }

        private ChunkData addChunkPlaceholder(BlockPos pos)
        {
            var data = new ChunkData(new Chunk(pos = pos.chunkPos(), this), new ModelChunk());

            Chunks.TryAdd(pos, data);

            return data;
        }

        public List<AxisAlignedBB> getIntersectingEntitiesBBs(AxisAlignedBB with)
        {
            List<AxisAlignedBB> bbs = new List<AxisAlignedBB>();

            for (int i = 0; i < Entities.Count; i++)
            {
                var bb = Entities[i].getEntityBoundingBox();

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
            if (!Chunks.TryGetValue(pos.chunkPos(), out var chunkData))
                return null;

            return chunkData?.chunk;
        }

        public EnumBlock getBlock(BlockPos pos)
        {
            var chunk = getChunkFromPos(pos);
            if (chunk == null)
                return EnumBlock.AIR;

            return chunk.getBlock( pos - chunk.chunkPos);
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
            if (Chunks.TryRemove(pos, out var data))// && data.model.isGenerated)
            {
                data.model.destroy();

                ThreadPool.QueueUserWorkItem(e => saveChunk(data));
            }
        }

        private void saveChunk(ChunkData chunk)
        {
            byte[] data = new byte[_chunkManager.Info.ChunkByteSize];
            Buffer.BlockCopy(chunk.chunk._chunkBlocks,0,data,0, data.Length);
            _chunkManager.WriteChunkData(new []{chunk.chunk.chunkPos.x,chunk.chunk.chunkPos.z},data);   
        }

        public bool loadChunk(BlockPos pos)
        {
            var chunkPos = pos.chunkPos();
            
            var data = _chunkManager.GetChunkData(new[] {pos.x, pos.z});
            if (data == null) return false;
            
            var blockData = new short[16,256,16];
            Buffer.BlockCopy(data,0,blockData,0, blockData.Length);
            
            var chunk = Chunk.CreateWithData(chunkPos, blockData);

            var chunkData = addChunkPlaceholder(chunkPos);
            chunkData.chunk = chunk;
            chunkData.chunkGenerated = true;
            return true;
        }

        public void saveAllChunks()
        {
            foreach (var data in Chunks.Values)
            {
                saveChunk(data);
            }
        }

        public void destroyChunkModels()
        {
            foreach (var data in Chunks)
            {
                if (!data.Value.modelGenerating && data.Value.model.isGenerated)
                    data.Value.model.destroy();
            }
        }

        private void markNeighbourChunksDirty(BlockPos pos)
        {
            var chunk = getChunkFromPos(pos);

            for (var index = 0; index < FacingUtil.SIDES.Length - 2; index++)
            {
                EnumFacing side = FacingUtil.SIDES[index];

                var p = pos.offset(side);
                var ch = getChunkFromPos(p);

                if (ch != chunk)
                    ch?.markDirty();
            }
        }

        public int getMetadata(BlockPos pos)
        {
            var chunk = getChunkFromPos(pos);
            if (chunk == null)
                return 0;

            return chunk.getMetadata(pos - chunk.chunkPos);
        }

        public void setMetadata(BlockPos pos, int meta, bool redraw)
        {
            var chunk = getChunkFromPos(pos);
            if (chunk == null)
                return;

            chunk.setMetadata(pos - chunk.chunkPos, meta, redraw);

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

        public void beginGenerateChunk(BlockPos pos, bool updateContainingEntities)
        {
            var chunkPos = pos.chunkPos();

            if (Chunks.ContainsKey(chunkPos))
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
                                data.chunk.setBlock(new BlockPos(x, peakY + 1 + treeY, z), EnumBlock.LOG, 0);
                            }
                        }
                    }
                }

                data.chunkGenerated = true;

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
                            int height = getHeightAtPos(entity.pos.X, entity.pos.Z);

                            if (entity.pos.Y < height)
                                entity.teleportTo(new Vector3(entity.pos.X, entity.lastPos.Y = height, entity.pos.Z));
                        }
                    }
                }

                saveChunk(data);
            });
        }

        public void beginUpdateModelForChunk(BlockPos pos)
        {
            pos = pos.chunkPos();

            if (Chunks.TryGetValue(pos, out var node))
            {
                node.beginUpdateModel();
            }
        }

        public bool areNeighbourChunksGenerated(BlockPos pos)
        {
            pos = pos.chunkPos();

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
            var n = Chunks[pos.chunkPos()];

            return n.chunkGenerated && n.model.isGenerated;
        }

        public bool isChunkGenerated(BlockPos pos)
        {
            if (Chunks.TryGetValue(pos.chunkPos(), out var n))
                return n.chunkGenerated;

            return false;
        }
    }
}