using OpenTK;
using SharpCraft.block;
using SharpCraft.entity;
using SharpCraft.model;
using SharpCraft.util;
using SharpCraft.world.chunk;
using SharpCraft.world.chunk.region;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SharpCraft.world
{
    internal class World
    {
        public ConcurrentDictionary<ChunkPos, Chunk> Chunks { get; }

        public List<Entity> Entities;

        public readonly int Seed;
        public readonly string LevelName;

        private NoiseUtil _noiseUtil;
        private int _dimension = 0;
        public readonly String SaveRoot;

        internal readonly ChunkDataManager<RegionStaticImpl<ChunkPos>, ChunkPos> ChunkData;

        private bool initalLoad = true; //just dirty hack needs to be removed soon

        public ChunkLoadManager LoadManager { get; } = new ChunkLoadManager();

        public World(string saveName, string levelName, int seed)
        {
            Chunks = new ConcurrentDictionary<ChunkPos, Chunk>();
            Entities = new List<Entity>();

            _noiseUtil = new NoiseUtil(seed);
            _noiseUtil.SetFractalType(NoiseUtil.FractalType.FBM);

            Seed = seed;
            LevelName = levelName;
            SaveRoot = $"{SharpCraft.Instance.GameFolderDir}/saves/{saveName}/";
            ChunkData = new ChunkDataManager<RegionStaticImpl<ChunkPos>, ChunkPos>(
                $"{SaveRoot}{_dimension}/region",
                new RegionInfo<ChunkPos>(new[] { 12, 12 }, 2 * Chunk.ChunkSize * Chunk.ChunkHeight * Chunk.ChunkSize),
                RegionStaticImpl<ChunkPos>.Ctor,
                ChunkPos.Ctor);
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
                var e = Entities[i];

                if (GetChunk(new BlockPos(e.pos).ChunkPos()) is Chunk chunk && chunk.HasData)
                {
                    e.Update();

                    if (!e.isAlive)
                        Entities.Remove(e);
                }
            }
        }

        public List<AxisAlignedBB> GetIntersectingEntitiesBBs(AxisAlignedBB with)
        {
            var bbs = new List<AxisAlignedBB>();

            for (var i = 0; i < Entities.Count; i++)
            {
                var entity = Entities[i];

                if (entity is EntityItem)
                    continue;

                var bb = entity.getEntityBoundingBox();

                if (bb.intersectsWith(with))
                    bbs.Add(bb);
            }

            return bbs;
        }

        public List<AxisAlignedBB> GetBlockCollisionBoxes(AxisAlignedBB box)
        {
            var blocks = new List<AxisAlignedBB>();

            var bb = box.union(box);

            for (int x = (int)bb.min.X, maxX = (int)bb.max.X; x < maxX; x++)
            {
                for (int y = (int)bb.min.Y, maxY = (int)bb.max.Y; y < maxY; y++)
                {
                    for (int z = (int)bb.min.Z, maxZ = (int)bb.max.Z; z < maxZ; z++)
                    {
                        var pos = new BlockPos(x, y, z);
                        var block = SharpCraft.Instance.World.GetBlock(pos);
                        if (block == EnumBlock.AIR)
                            continue;

                        blocks.Add(ModelRegistry.getModelForBlock(block, GetMetadata(pos)).boundingBox.offset(pos.ToVec()));
                    }
                }
            }

            return blocks;
        }

        public Chunk GetChunk(ChunkPos pos)
        {
            return Chunks.TryGetValue(pos, out var chunkData) ? chunkData : null;
        }

        public EnumBlock GetBlock(BlockPos pos)
        {
            var chunk = GetChunk(ChunkPos.FromWorldSpace(pos));
            if (chunk == null || !chunk.HasData)
                return EnumBlock.AIR;

            return chunk.GetBlock(ChunkPos.ToChunkLocal(pos));
        }

        public void SetBlock(BlockPos pos, EnumBlock blockType, int meta)
        {
            var chunk = GetChunk(ChunkPos.FromWorldSpace(pos));
            if (chunk == null || !chunk.HasData)
                return;

            chunk.SetBlock(ChunkPos.ToChunkLocal(pos), blockType, meta);
        }

        public void UnloadChunk(ChunkPos pos)
        {
            if (Chunks.TryRemove(pos, out var chunk)) // && data.model.isGenerated)
            {
                chunk.DestroyModel();
                chunk.Save();
            }
        }

        public bool LoadChunk(ChunkPos chunkPos)
        {
            var data = ChunkData.GetChunkData(chunkPos);
            if (data == null) return false;

            var blockData = new short[Chunk.ChunkSize, Chunk.ChunkHeight, Chunk.ChunkSize];
            Buffer.BlockCopy(data, 0, blockData, 0, data.Length);

            CreateChunk(chunkPos, blockData);
            return true;
        }

        public void SaveAllChunks()
        {
            foreach (var data in Chunks.Values)
            {
                data.Save();
            }
        }

        public void DestroyChunkModels()
        {
            foreach (var data in Chunks.Values)
            {
                data.DestroyModel();
            }
        }

        public int GetMetadata(BlockPos pos)
        {
            var chunk = GetChunk(ChunkPos.FromWorldSpace(pos));
            if (chunk == null || !chunk.HasData)
                return -1;

            return chunk.GetMetadata(ChunkPos.ToChunkLocal(pos));
        }

        public void SetMetadata(BlockPos pos, int meta)
        {
            var chunk = GetChunk(ChunkPos.FromWorldSpace(pos));
            if (chunk == null || !chunk.HasData)
                return;

            chunk.SetMetadata(ChunkPos.ToChunkLocal(pos), meta);
        }

        public int GetHeightAtPos(float x, float z)
        {
            var pos = new BlockPos(x, 0, z);

            var chunk = GetChunk(pos.ChunkPos());
            if (chunk == null || !chunk.HasData)
                return 0;

            return chunk.GetHeightAtPos(MathUtil.ToLocal(pos.X, Chunk.ChunkSize), MathUtil.ToLocal(pos.Z, Chunk.ChunkSize));
        }

        public IEnumerable<Chunk> GetNeighbourChunks(ChunkPos pos)
        {
            return FaceSides.YPlane.Select(dir => GetChunk(pos + dir));
        }

        private Chunk CreateChunk(ChunkPos pos, short[,,] data)
        {
            lock (Chunks)
            {
                var chunk = data == null ? new Chunk(pos, this) : new Chunk(pos, this, data);
                if (!Chunks.TryAdd(chunk.Pos, chunk))
                {
                    Console.Error.WriteLine("Chunk already exists at " + chunk.Pos);
                    return null;
                }
                // throw new Exception("Chunk already exists at " + chunk.Pos);
                return chunk;
            }
        }

        public void GenerateChunk(ChunkPos chunkPos, bool updateContainingEntities)
        {
            var chunk = CreateChunk(chunkPos, null);
            if (chunk == null)
                return;

            var chunkData = new short[Chunk.ChunkSize, Chunk.ChunkHeight, Chunk.ChunkSize];

            void SetBlock(int x, int y, int z, EnumBlock b)
            {
                short id = (short)((short)b << 4);
                chunkData[x, y, z] = id;
            }

            bool IsAir(int x, int y, int z)
            {
                return chunkData[x, y, z] >> 4 == (int)EnumBlock.AIR;
            }

            for (var z = 0; z < Chunk.ChunkSize; z++)
            {
                for (var x = 0; x < Chunk.ChunkSize; x++)
                {
                    var wsX = chunk.Pos.WorldSpaceX();
                    var wsZ = chunk.Pos.WorldSpaceZ();

                    var xCh = x + wsX;
                    var zCh = z + wsZ;

                    var peakY = 32 + (int)Math.Abs(MathHelper.Clamp(0.35f + _noiseUtil.GetPerlinFractal(xCh, zCh), 0, 1) * 32);

                    for (var y = peakY; y >= 0; y--)
                    {
                        if (y == peakY) SetBlock(x, y, z, EnumBlock.GRASS);
                        else if (y > 0 && peakY - y > 0 && peakY - y < 3) SetBlock(x, y, z, EnumBlock.DIRT);
                        else if (y == 0) SetBlock(x, y, z, EnumBlock.BEDROCK);
                        else
                        {
                            var f = _noiseUtil.GetNoise(xCh * 32 - y * 16, zCh * 32 + x * 16);

                            SetBlock(x, y, z, f >= 0.75f ? EnumBlock.RARE : EnumBlock.STONE);
                        }
                    }

                    var treeSeed = Math.Abs(MathHelper.Clamp(_noiseUtil.GetWhiteNoise(xCh, zCh), 0, 1));
                    //var treeSeed2 = Math.Abs(MathHelper.Clamp(0.35f + _noiseUtil.GetPerlinFractal(zCh, xCh), 0, 1));

                    if (treeSeed >= 0.85f && x >= 3 && z >= 3 && x <= 13 && z <= 13 && x % 4 == 0 && z % 4 == 0)
                    {
                        var treeTop = 0;

                        for (var treeY = 0; treeY < 5; treeY++)
                        {
                            SetBlock(x, treeTop = peakY + 1 + treeY, z, EnumBlock.LOG);
                        }

                        //leaves
                        for (int i = -3; i <= 3; i++)
                        {
                            for (int j = -3; j <= 3; j++)
                            {
                                for (int k = -3; k <= 3; k++)
                                {
                                    if (i == 0 && k == 0 && j <= 0)
                                        continue;

                                    var pX = x + i;
                                    var pY = treeTop + j;
                                    var pZ = z + k;

                                    if (!IsAir(pX, pY, pZ))
                                        continue;

                                    var vec = new Vector3(i, j, k);

                                    if (MathUtil.Distance(vec, Vector3.Zero) <= 3)
                                        SetBlock(pX, pY, pZ, EnumBlock.LEAVES);
                                }
                            }
                        }
                    }
                }
            }

            chunk.GeneratedData(chunkData);

            if (updateContainingEntities)
            {
                foreach (var entity in Entities)
                {
                    var pos = new BlockPos(entity.pos);

                    if (chunk.Pos == pos.ChunkPos())
                    {
                        var height = chunk.GetHeightAtPos(MathUtil.ToLocal(pos.X, Chunk.ChunkSize), MathUtil.ToLocal(pos.Z, Chunk.ChunkSize));

                        if (entity.pos.Y < height)
                            entity.teleportTo(new Vector3(entity.pos.X, entity.lastPos.Y = height, entity.pos.Z));
                    }
                }
            }
        }

        public bool AreNeighbourChunksGenerated(ChunkPos pos)
        {
            return GetNeighbourChunks(pos).All(chunk => chunk != null && chunk.HasData);
        }

        public void update(EntityPlayerSP player, int renderDistance)
        {
            if (player == null) return;

            LoadManager.LoadImportantChunks();
            LoadManager.UpdateLoad(player, renderDistance, initalLoad);
            initalLoad = false;

            foreach (var chunk in Chunks.Values)
            {
                chunk.Update();

                if (chunk.Pos.DistanceTo(player.pos.Xz) > renderDistance * Chunk.ChunkSize + 50) UnloadChunk(chunk.Pos);
            }
        }
    }
}