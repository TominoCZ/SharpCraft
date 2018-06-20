using OpenTK;
using SharpCraft.block;
using SharpCraft.entity;
using SharpCraft.util;
using SharpCraft.world.chunk;
using SharpCraft.world.chunk.region;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SharpCraft.world
{
#pragma warning disable CS0618 // Type or member is obsolete
    public class World
    {
        public ConcurrentDictionary<ChunkPos, Chunk> Chunks { get; } = new ConcurrentDictionary<ChunkPos, Chunk>();

        public List<Entity> Entities = new List<Entity>();

        public readonly int Seed;
        public readonly string LevelName;

        private readonly NoiseUtil _noiseUtil;
        private readonly int _dimension = 0;
        public readonly String SaveRoot;

        public readonly ChunkDataManager<RegionStaticImpl<ChunkPos>, ChunkPos> ChunkData;

        private bool _initalLoad = true; //just dirty hack needs to be removed soon

        public ChunkLoadManager LoadManager { get; } = new ChunkLoadManager();

        //TODO - clientside only
        private readonly Dictionary<BlockPos, Waypoint> _waypoints = new Dictionary<BlockPos, Waypoint>();

        private WorldLut _worldLut;

        public World(string saveName, string levelName, int seed)
        {
            _noiseUtil = new NoiseUtil(Seed = seed);
            _noiseUtil.SetFractalType(NoiseUtil.FractalType.FBM);

            LevelName = levelName;
            SaveRoot = $"{SharpCraft.Instance.GameFolderDir}/saves/{saveName}/";
            ChunkData = new ChunkDataManager<RegionStaticImpl<ChunkPos>, ChunkPos>(
                $"{SaveRoot}{_dimension}/region",
                new RegionInfo<ChunkPos>(new[] { 12, 12 }, 2 * Chunk.ChunkSize * Chunk.ChunkHeight * Chunk.ChunkSize),
                RegionStaticImpl<ChunkPos>.Ctor,
                ChunkPos.Ctor);

            _worldLut = new WorldLut();

            foreach (var block in BlockRegistry.AllBlocks())
            {
                _worldLut.Put(block.UnlocalizedName);
            }
        }

        public short GetLocalBlockId(string unlocalizedName)
        {
            return _worldLut.Translate(unlocalizedName);
        }

        public string GetLocalBlockName(short localId)
        {
            return _worldLut.Translate(localId);
        }

        public void AddEntity(Entity e)
        {
            if (!Entities.Contains(e))
                Entities.Add(e);
        }

        private void UpdateEntities()
        {
            Entities.RemoveAll(e =>
            {
                if (!IsChunkLoaded(ChunkPos.FromWorldSpace(e.Pos))) return false;
                if (e.isAlive)
                {
                    e.Update();
                    return !e.isAlive;
                }
                return true;
            });
        }

        public bool IsChunkLoaded(ChunkPos pos)
        {
            return GetChunk(pos) is Chunk chunk && chunk.HasData;
        }

        public List<AxisAlignedBB> GetIntersectingEntitiesBBs(AxisAlignedBB with)
        {
            return (from entity in Entities where !(entity is EntityItem) select entity.GetEntityBoundingBox() into bb where bb.IntersectsWith(with) select bb).ToList();
        }

        public List<AxisAlignedBB> GetBlockCollisionBoxes(AxisAlignedBB box)
        {
            List<AxisAlignedBB> blocks = new List<AxisAlignedBB>();

            AxisAlignedBB bb = box.Union(box);

            var air = BlockRegistry.GetBlock<BlockAir>();

            for (int x = (int)bb.min.X, maxX = (int)bb.max.X; x < maxX; x++)
            {
                for (int y = (int)bb.min.Y, maxY = (int)bb.max.Y; y < maxY; y++)
                {
                    for (int z = (int)bb.min.Z, maxZ = (int)bb.max.Z; z < maxZ; z++)
                    {
                        BlockPos pos = new BlockPos(x, y, z);
                        BlockState state = SharpCraft.Instance.World.GetBlockState(pos);
                        if (state.Block == air)
                            continue;

                        blocks.Add(state.Block.BoundingBox.offset(pos.ToVec()));
                    }
                }
            }

            return blocks;
        }

        public Chunk GetChunk(ChunkPos pos)
        {
            return Chunks.TryGetValue(pos, out Chunk chunkData) ? chunkData : null;
        }

        public bool IsAir(BlockPos pos)
        {
            return GetBlockState(pos).Block == BlockRegistry.GetBlock<BlockAir>();
        }

        public BlockState GetBlockState(BlockPos pos)
        {
            if (pos.Y < 0 || pos.Y >= Chunk.ChunkHeight)
                return BlockRegistry.GetBlock<BlockAir>().GetState();

            Chunk chunk = GetChunk(ChunkPos.FromWorldSpace(pos));
            if (chunk == null || !chunk.HasData)
                return BlockRegistry.GetBlock<BlockAir>().GetState();

            return chunk.GetBlockState(ChunkPos.ToChunkLocal(pos));
        }

        public void SetBlockState(BlockPos pos, BlockState state)
        {
            Chunk chunk = GetChunk(ChunkPos.FromWorldSpace(pos));
            if (chunk == null || !chunk.HasData)
                return;

            _worldLut.Put(state.Block.UnlocalizedName);

            chunk.SetBlockState(ChunkPos.ToChunkLocal(pos), state);
        }

        public void UnloadChunk(ChunkPos pos)
        {
            if (Chunks.TryRemove(pos, out Chunk chunk)) // && data.model.isGenerated)
            {
                chunk.DestroyModel();
                chunk.Save();
            }
        }

        public bool LoadChunk(ChunkPos chunkPos)
        {
            byte[] data = ChunkData.GetChunkData(chunkPos);
            if (data == null) return false;

            short[,,] blockData = new short[Chunk.ChunkSize, Chunk.ChunkHeight, Chunk.ChunkSize];
            Buffer.BlockCopy(data, 0, blockData, 0, data.Length);

            CreateChunk(chunkPos, blockData);
            return true;
        }

        public void SaveAllChunks()
        {
            foreach (Chunk data in Chunks.Values)
            {
                if (data.HasData)
                    data.Save();
            }
        }

        public void DestroyChunkModels()
        {
            foreach (Chunk data in Chunks.Values)
            {
                data.DestroyModel();
            }
        }

        /*
        public int GetMetadata(BlockPos pos)
        {
            Chunk chunk = GetChunk(ChunkPos.FromWorldSpace(pos));
            if (chunk == null || !chunk.HasData)
                return -1;

            return chunk.GetMetadata(ChunkPos.ToChunkLocal(pos));
        }*/

        /*
        public void SetMetadata(BlockPos pos, int meta)
        {
            Chunk chunk = GetChunk(ChunkPos.FromWorldSpace(pos));
            if (chunk == null || !chunk.HasData)
                return;

            chunk.SetMetadata(ChunkPos.ToChunkLocal(pos), meta);
        }*/

        public int GetHeightAtPos(float x, float z)
        {
            BlockPos pos = new BlockPos(x, 0, z);

            Chunk chunk = GetChunk(pos.ChunkPos());
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
                Chunk chunk = data == null ? new Chunk(pos, this) : new Chunk(pos, this, data);
                if (!Chunks.TryAdd(chunk.Pos, chunk))
                {
                    Console.Error.WriteLine("Chunk already exists at " + chunk.Pos);
                    return null;
                }
                // throw new Exception("Chunk already exists at " + chunk.Pos);
                return chunk;
            }
        }

        public void GenerateChunk(ChunkPos chunkPos, bool updateContainingEntities) //TODO - something is causing this to be extremely slow and ran many times simultaneously for one chunk
        {
            Chunk chunk = CreateChunk(chunkPos, null);
            if (chunk == null)
                return;

            var air = BlockRegistry.GetBlock<BlockAir>().GetState();

            var leaves = BlockRegistry.GetBlock("leaves").GetState();

            var log = BlockRegistry.GetBlock("log").GetState();
            var grass = BlockRegistry.GetBlock<BlockGrass>().GetState();
            var dirt = BlockRegistry.GetBlock("dirt").GetState();
            var stone = BlockRegistry.GetBlock("stone").GetState();
            var rare = BlockRegistry.GetBlock("rare").GetState();
            var bedrock = BlockRegistry.GetBlock("bedrock").GetState();

            short airId = GetLocalBlockId(air.Block.UnlocalizedName);

            short[,,] chunkData = new short[Chunk.ChunkSize, Chunk.ChunkHeight, Chunk.ChunkSize];

            void SetBlock(int x, int y, int z, BlockState s)
            {
                short id = GetLocalBlockId(s.Block.UnlocalizedName);
                short meta = s.Block.GetMetaFromState(s);

                short value = (short)(id << 4 | meta);
                chunkData[x, y, z] = value;
            }

            bool IsAir(int x, int y, int z)
            {
                return chunkData[x, y, z] >> 4 == airId;
            }

            for (int z = 0; z < Chunk.ChunkSize; z++)
            {
                for (int x = 0; x < Chunk.ChunkSize; x++)
                {
                    float wsX = chunk.Pos.WorldSpaceX();
                    float wsZ = chunk.Pos.WorldSpaceZ();

                    float xCh = x + wsX;
                    float zCh = z + wsZ;

                    int peakY = 32 + (int)Math.Abs(
                                    MathHelper.Clamp(0.35f + _noiseUtil.GetPerlinFractal(xCh, zCh), 0, 1) * 32);

                    for (int y = peakY; y >= 0; y--)
                    {
                        if (y == peakY) SetBlock(x, y, z, grass);
                        else if (y > 0 && peakY - y > 0 && peakY - y < 3) SetBlock(x, y, z, dirt);
                        else if (y == 0)
                            SetBlock(x, y, z, bedrock);
                        else
                        {
                            float f = _noiseUtil.GetNoise(xCh * 32 - y * 16, zCh * 32 + x * 16);

                            SetBlock(x, y, z, f >= 0.75f ? rare : stone);
                        }
                    }

                    float treeSeed = Math.Abs(MathHelper.Clamp(_noiseUtil.GetWhiteNoise(xCh, zCh), 0, 1));
                    //var treeSeed2 = Math.Abs(MathHelper.Clamp(0.35f + _noiseUtil.GetPerlinFractal(zCh, xCh), 0, 1));

                    if (treeSeed >= 0.85f && x >= 3 && z >= 3 && x <= 13 && z <= 13 && x % 4 == 0 && z % 4 == 0)
                    {
                        int treeTop = 0;

                        int height = (int)Math.Clamp(peakY * treeSeed / 64f * 7.5f, 3, 4);

                        for (int treeY = 0; treeY < height; treeY++)
                        {
                            treeTop = peakY + 1 + treeY;
                            SetBlock(x, treeTop, z, log);
                        }

                        //leaves
                        for (int i = -3; i <= 3; i++)
                        {
                            for (int j = 0; j <= 3; j++)
                            {
                                for (int k = -3; k <= 3; k++)
                                {
                                    if (i == 0 && k == 0 && j <= 0)
                                        continue;

                                    int pX = x + i;
                                    int pY = treeTop + j - 1;
                                    int pZ = z + k;

                                    if (!IsAir(pX, pY, pZ))
                                        continue;

                                    Vector3 vec = new Vector3(i, j, k);

                                    if (MathUtil.Distance(vec, Vector3.Zero) <= 2.5f)
                                        SetBlock(pX, pY, pZ, leaves);
                                }
                            }
                        }
                    }
                }
            }

            chunk.GeneratedData(chunkData);

            if (updateContainingEntities)
            {
                foreach (Entity entity in Entities)
                {
                    BlockPos pos = new BlockPos(entity.Pos);

                    if (chunk.Pos == pos.ChunkPos())
                    {
                        int height = chunk.GetHeightAtPos(MathUtil.ToLocal(pos.X, Chunk.ChunkSize), MathUtil.ToLocal(pos.Z, Chunk.ChunkSize));

                        if (entity.Pos.Y < height)
                            entity.TeleportTo(new Vector3(entity.Pos.X, entity.LastPos.Y = height, entity.Pos.Z));
                    }
                }
            }
        }

        public bool AreNeighbourChunksGenerated(ChunkPos pos)
        {
            return GetNeighbourChunks(pos).All(chunk => chunk != null && chunk.HasData);
        }

        public void Update(EntityPlayerSP player, int renderDistance)
        {
            if (player == null) return;

            LoadManager.LoadImportantChunks();
            LoadManager.UpdateLoad(player, renderDistance, _initalLoad);
            _initalLoad = false;

            foreach (Chunk chunk in Chunks.Values)
            {
                chunk.Update();

                if (chunk.Pos.DistanceTo(player.Pos.Xz) > renderDistance * Chunk.ChunkSize + 50) UnloadChunk(chunk.Pos);
            }

            UpdateEntities();
        }

        public void AddWaypoint(BlockPos pos, Color color, string name)
        {
            _waypoints.TryAdd(pos, new Waypoint(pos, color, name));
        }

        public void RemoveWaypoint(BlockPos pos)
        {
            _waypoints.Remove(pos);
        }

        public void EditWaypoint(BlockPos pos, Color color, string name)
        {
            if (_waypoints.TryGetValue(pos, out Waypoint wp))
            {
                wp.Color = color;
                wp.Name = name;
            }
        }

        public Dictionary<BlockPos, Waypoint>.ValueCollection GetWaypoints()
        {
            return _waypoints.Values;
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}