using OpenTK;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SharpCraft_Client.block;
using SharpCraft_Client.entity;
using SharpCraft_Client.util;
using SharpCraft_Client.world.chunk;
using SharpCraft_Client.world.chunk.region;

namespace SharpCraft_Client.world
{
    public abstract class World
    {
        protected readonly WorldLut _worldLut = new WorldLut();

        private readonly NoiseUtil _noiseUtil;

        private bool _initalLoad = true; //just dirty hack needs to be removed soon

        public ConcurrentDictionary<ChunkPos, Chunk> Chunks { get; } = new ConcurrentDictionary<ChunkPos, Chunk>();

        public readonly ChunkDataManager<RegionStaticImpl<ChunkPos>, ChunkPos> ChunkData;

        public List<Entity> Entities = new List<Entity>();

        public readonly string Seed;
        public readonly string LevelName;

        public readonly int Dimension = 0;
        public readonly string SaveRoot;

        protected World(string saveName, string levelName, string seed)
        {
            _noiseUtil = new NoiseUtil((Seed = seed).GetHashCode());
            _noiseUtil.SetFractalType(NoiseUtil.FractalType.FBM);

            LevelName = levelName;
            SaveRoot = $"./";
            ChunkData = new ChunkDataManager<RegionStaticImpl<ChunkPos>, ChunkPos>(
                $"{SaveRoot}{Dimension}/region",
                new RegionInfo<ChunkPos>(new[] { 12, 12 }, 2 * Chunk.ChunkSize * Chunk.ChunkHeight * Chunk.ChunkSize),
                RegionStaticImpl<ChunkPos>.Ctor,
                ChunkPos.Ctor);
        }

        public ChunkLoadManager LoadManager { get; } = new ChunkLoadManager();

        public virtual void AddEntity(Entity e)
        {
            if (!Entities.Contains(e))
                Entities.Add(e);
        }

        public void UpdateEntities()
        {
            Entities.RemoveAll(e =>
            {
                if (!IsChunkLoaded(ChunkPos.FromWorldSpace(e.Pos))) return false;
                if (e.IsAlive)
                {
                    e.Update();
                    return !e.IsAlive;
                }

                return true;
            });
        }

        public virtual void Update(Vector3 playerPos, int renderDistance)
        {
            //TODO use EntityPlayer instead
            LoadManager.LoadImportantChunks();
            LoadManager.UpdateLoad(this, playerPos, renderDistance, _initalLoad);
            _initalLoad = false;

            foreach (Chunk chunk in Chunks.Values)
            {
                chunk.Update();

                if (chunk.Pos.DistanceTo(playerPos.Xz) > renderDistance * Chunk.ChunkSize + 50) UnloadChunk(chunk.Pos);
            }

            UpdateEntities();
        }

        public int GetHeightAtPos(float x, float z)
        {
            BlockPos pos = new BlockPos(x, 0, z);

            Chunk chunk = GetChunk(pos.ChunkPos());
            if (chunk == null || !chunk.HasData)
                return 0;

            return chunk.GetHeightAtPos(MathUtil.ToLocal(pos.X, Chunk.ChunkSize),
                MathUtil.ToLocal(pos.Z, Chunk.ChunkSize));
        }

        public virtual Chunk GetChunk(ChunkPos pos)
        {
            return Chunks.TryGetValue(pos, out Chunk chunk) ? chunk : null;
        }

        public virtual BlockState GetBlockState(BlockPos pos)
        {
            if (pos.Y < 0 || pos.Y >= Chunk.ChunkHeight)
                return BlockRegistry.GetBlock<BlockAir>().GetState();

            Chunk chunk = GetChunk(ChunkPos.FromWorldSpace(pos));
            if (chunk == null || !chunk.HasData)
                return BlockRegistry.GetBlock<BlockAir>().GetState();

            return chunk.GetBlockState(ChunkPos.ToChunkLocal(pos));
        }

        public virtual void SetBlockState(BlockPos pos, BlockState state)
        {
            Chunk chunk = GetChunk(ChunkPos.FromWorldSpace(pos));
            if (chunk == null || !chunk.HasData)
                return;

            _worldLut.Put(state.Block.UnlocalizedName);

            var localPos = ChunkPos.ToChunkLocal(pos);

            chunk.SetBlockState(localPos, state);

            if (state.Block.CreateTileEntity(this, pos) is TileEntity te)
            {
                chunk.AddTileEntity(localPos, te);
            }

            chunk.Save();
        }

        public string GetLocalBlockName(short localId)
        {
            return _worldLut.Translate(localId);
        }

        public short GetLocalBlockId(string unlocalizedName)
        {
            return _worldLut.Translate(unlocalizedName);
        }

        public virtual void RemoveTileEntity(BlockPos pos)
        {
            GetChunk(pos.ChunkPos())?.RemoveTileEntity(ChunkPos.ToChunkLocal(pos));
        }

        public TileEntity GetTileEntity(BlockPos pos)
        {
            return GetChunk(pos.ChunkPos())?.GetTileEntity(ChunkPos.ToChunkLocal(pos));
        }

        public void SaveTileEntity(BlockPos pos)
        {
            GetChunk(pos.ChunkPos())?.SaveTileEntity(ChunkPos.ToChunkLocal(pos));
        }

        public bool IsAir(BlockPos pos)
        {
            return GetBlockState(pos).Block == BlockRegistry.GetBlock<BlockAir>();
        }

        public bool IsChunkLoaded(ChunkPos pos)
        {
            return GetChunk(pos) is Chunk chunk && chunk.HasData;
        }

        public void UnloadChunk(ChunkPos pos)
        {
            if (Chunks.TryRemove(pos, out Chunk chunk)) // && data.model.isGenerated)
            {
                chunk.DestroyModel();
                chunk.Save();
            }
        }

        public virtual bool LoadChunk(ChunkPos chunkPos)
        {
            byte[] data = ChunkData.GetChunkData(chunkPos);
            if (data == null) return false;

            short[,,] blockData = new short[Chunk.ChunkSize, Chunk.ChunkHeight, Chunk.ChunkSize];
            Buffer.BlockCopy(data, 0, blockData, 0, data.Length);

            Chunk chunk = null;

            lock (Chunks)
            {
                chunk = PutChunk(chunkPos, blockData);
            }

            if (chunk != null)
            {
                var air = BlockRegistry.GetBlock<BlockAir>();

                Enumerable.Range(0, Chunk.ChunkHeight).AsParallel().ForAll(y =>
                {
                    for (int x = 0; x < Chunk.ChunkSize; x++)
                    {
                        for (int z = 0; z < Chunk.ChunkSize; z++)
                        {
                            var localPos = new BlockPos(x, y, z);

                            var state = chunk.GetBlockState(localPos);

                            if (state.Block == air)
                                continue;

                            var worldPos = new BlockPos(chunkPos.ToVec() + localPos.ToVec());

                            if (state.Block.CreateTileEntity(this, worldPos) is TileEntity te)
                            {
                                chunk.AddTileEntity(localPos, te);
                            }
                        }
                    }
                });
            }

            return true;
        }

        //TODO move this to some chunk generator
        public short[,,] GenerateChunk(ChunkPos chunkPos, bool updateContainingEntities)
        {
            Chunk chunk = null;

            lock (Chunks)
            {
                chunk = PutChunk(chunkPos, null);
            }

            if (chunk == null)
                return null;

            var air = BlockRegistry.GetBlock<BlockAir>().GetState();

            var leavesGreen = BlockRegistry.GetBlock<BlockLeaves>().GetState();
            var leavesYellow = BlockRegistry.GetBlock<BlockLeaves>().GetState(1);
            var leavesOrange = BlockRegistry.GetBlock<BlockLeaves>().GetState(2);
            var leavesRed = BlockRegistry.GetBlock<BlockLeaves>().GetState(3);

            var log = BlockRegistry.GetBlock<BlockLog>().GetState();
            var grass = BlockRegistry.GetBlock<BlockGrass>().GetState();
            var dirt = BlockRegistry.GetBlock<BlockDirt>().GetState();
            var stone = BlockRegistry.GetBlock<BlockStone>().GetState();
            var rare = BlockRegistry.GetBlock<BlockRare>().GetState();
            var bedrock = BlockRegistry.GetBlock<BlockBedrock>().GetState();
            var tallgrass = BlockRegistry.GetBlock<BlockTallGrass>().GetState();
            var tulipRed = BlockRegistry.GetBlock<BlockTulipRed>().GetState();
            var tulipOrange = BlockRegistry.GetBlock<BlockTulipOrange>().GetState();

            short airId = GetLocalBlockId(air.Block.UnlocalizedName);

            short[,,] chunkData = new short[Chunk.ChunkSize, Chunk.ChunkHeight, Chunk.ChunkSize];

            void SetBlock(int x, int y, int z, BlockState s)
            {
                short id = GetLocalBlockId(s.Block.UnlocalizedName);
                short meta = s.Block.GetMetaFromState(s);

#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand
                short value = (short)(id << 4 | meta);
#pragma warning restore CS0675 // Bitwise-or operator used on a sign-extended operand
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

                    float grassSeed = _noiseUtil.GetPerlinFractal(-xCh * 32 + 8, -zCh * 32 + x * 8);

                    for (int y = peakY; y >= 0; y--)
                    {
                        if (y == peakY)
                        {
                            SetBlock(x, y, z, grass);

                            //set tallgrass above
                            if (IsAir(x, y + 1, z) && grassSeed >= 0.3f)
                            {
                                var random = MathUtil.NextFloat();

                                if (random >= 0 && random <= 0.6666)
                                    SetBlock(x, y + 1, z, tallgrass);
                                else
                                {
                                    SetBlock(x, y + 1, z, random <= 0.83325D ? tulipRed : tulipOrange);
                                }
                            }
                        }
                        else if (y > 0 && peakY - y > 0 && peakY - y < 3)
                        {
                            SetBlock(x, y, z, dirt);
                        }
                        else if (y == 0)
                        {
                            SetBlock(x, y, z, bedrock);
                        }
                        else
                        {
                            float f = _noiseUtil.GetNoise(xCh * 32 - y * 16, zCh * 32 + x * 16);

                            SetBlock(x, y, z, f >= 0.75f ? rare : stone);
                        }
                    }

                    //trees
                    float treeSeed = Math.Abs(MathHelper.Clamp(_noiseUtil.GetWhiteNoise(xCh, zCh), 0, 1));
                    //var treeSeed2 = Math.Abs(MathHelper.Clamp(0.35f + _noiseUtil.GetPerlinFractal(zCh, xCh), 0, 1));

                    if (treeSeed >= 0.85f && x >= 3 && z >= 3 && x <= 13 && z <= 13 && x % 4 == 0 && z % 4 == 0)
                    {
                        int treeTop = 0;

                        int height = (int)MathHelper.Clamp(peakY * treeSeed / 64f * 7.5f, 3, 4);

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

                                    float leavesSeed = (peakY - 32) / 32f;

                                    BlockState leaves = leavesGreen;

                                    if (leavesSeed > 0.2f && leavesSeed <= 0.4f)
                                        leaves = leavesYellow;
                                    else if (leavesSeed > 0.4f && leavesSeed <= 0.55f)
                                        leaves = leavesOrange;
                                    else if (leavesSeed > 0.65f)
                                        leaves = leavesRed;

                                    if (Vector3.Distance(vec, Vector3.Zero) <= 2.5f)
                                    {
                                        SetBlock(pX, pY, pZ, leaves);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return chunkData;

            chunk.GeneratedData(chunkData);

            if (updateContainingEntities)
            {
                foreach (Entity entity in Entities)
                {
                    BlockPos pos = new BlockPos(entity.Pos);

                    if (chunk.Pos == pos.ChunkPos())
                    {
                        int height = chunk.GetHeightAtPos(MathUtil.ToLocal(pos.X, Chunk.ChunkSize),
                            MathUtil.ToLocal(pos.Z, Chunk.ChunkSize));

                        if (entity.Pos.Y < height)
                            entity.TeleportTo(new Vector3(entity.Pos.X, entity.LastPos.Y = height, entity.Pos.Z));
                    }
                }
            }
        }

        public virtual Chunk PutChunk(ChunkPos pos, short[,,] data)
        {
            Chunk chunk = data == null ? new ChunkClient(pos, this) : new ChunkClient(pos, this, data);
            if (!Chunks.TryAdd(chunk.Pos, chunk))
            {
                Console.Error.WriteLine("Chunk already exists at " + chunk.Pos);
                return null;
            }
            // throw new Exception("Chunk already exists at " + chunk.Pos);
            return chunk;
        }

        public List<AxisAlignedBb> GetIntersectingEntitiesBBs(AxisAlignedBb with)
        {
            return (from entity in Entities
                    where !(entity is EntityItem)
                    select entity.GetEntityBoundingBox()
                into bb
                    where bb.IntersectsWith(with)
                    select bb).ToList();
        }

        public List<AxisAlignedBb> GetBlockCollisionBoxes(AxisAlignedBb box)
        {
            List<AxisAlignedBb> blocks = new List<AxisAlignedBb>();

            AxisAlignedBb bb = box.Union(box);

            var air = BlockRegistry.GetBlock<BlockAir>();

            for (int x = (int)bb.Min.X, maxX = (int)bb.Max.X; x < maxX; x++)
            {
                for (int y = (int)bb.Min.Y, maxY = (int)bb.Max.Y; y < maxY; y++)
                {
                    for (int z = (int)bb.Min.Z, maxZ = (int)bb.Max.Z; z < maxZ; z++)
                    {
                        BlockPos pos = new BlockPos(x, y, z);
                        BlockState state = SharpCraft.Instance.World.GetBlockState(pos);
                        if (state.Block == air || state.Block.Material.CanWalkThrough)
                            continue;

                        blocks.Add(state.Block.BoundingBox.Offset(pos.ToVec()));
                    }
                }
            }

            return blocks;
        }
    }
}