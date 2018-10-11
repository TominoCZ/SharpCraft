using System;
using System.Collections.Concurrent;
using System.Linq;
using OpenTK;
using SharpCraft_Client.block;
using SharpCraft_Client.entity;
using SharpCraft_Client.world.chunk;

namespace SharpCraft_Client.world
{
    public class WorldClientServer : World
    {
        public ConcurrentDictionary<ChunkPos, Chunk> Chunks { get; } = new ConcurrentDictionary<ChunkPos, Chunk>();

        private bool _initalLoad = true; //just dirty hack needs to be removed soon

        public WorldClientServer() : base("", "", "")
        {
            foreach (var block in BlockRegistry.AllBlocks())
            {
                _worldLut.Put(block.UnlocalizedName);
            }
        }

        public override void RemoveTileEntity(BlockPos pos)
        {
            GetChunk(pos.ChunkPos())?.RemoveTileEntity(ChunkPos.ToChunkLocal(pos));
        }

        public void DestroyChunkModels()
        {
            foreach (Chunk data in Chunks.Values)
            {
                data.DestroyModel();
            }

            GC.Collect();
        }

        public override bool LoadChunk(ChunkPos chunkPos)
        {
            SharpCraft.Instance.ServerHandler.RequestChunk(chunkPos);
            return true;
        }

        public override Chunk PutChunk(ChunkPos pos, short[,,] data)
        {
            Chunk chunk = data == null ? new ChunkClientServer(pos, this) : new ChunkClientServer(pos, this, data);
            if (!Chunks.TryAdd(chunk.Pos, chunk))
            {
                Console.Error.WriteLine("Chunk already exists at " + chunk.Pos);
                return null;
            }
            // throw new Exception("Chunk already exists at " + chunk.Pos);
            return chunk;
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

        public override void Update(Vector3 playerPos, int renderDistance)
        {
            _initalLoad = false;

            ChunkPos playerChunkPos = ChunkPos.FromWorldSpace(playerPos);

            for (int z = -renderDistance; z <= renderDistance; z++)
            {
                for (int x = -renderDistance; x <= renderDistance; x++)
                {
                    ChunkPos pos = new ChunkPos(playerChunkPos.x + x, playerChunkPos.z + z);

                    if (pos.DistanceTo(playerPos.Xz) < renderDistance * Chunk.ChunkSize)
                    {
                        if (GetChunk(pos) == null)
                        {
                            SharpCraft.Instance.ServerHandler.RequestChunk(pos);
                        }
                    }
                }
            }

            foreach (Chunk chunk in Chunks.Values)
            {
                chunk.Update();

                if (chunk.Pos.DistanceTo(playerPos.Xz) > renderDistance * Chunk.ChunkSize + 50) UnloadChunk(chunk.Pos);
            }

            UpdateEntities();
        }
    }
}