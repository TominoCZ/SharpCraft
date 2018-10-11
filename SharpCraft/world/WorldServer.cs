using System;
using System.Linq;
using OpenTK;
using SharpCraft_Client.block;
using SharpCraft_Client.world.chunk;

namespace SharpCraft_Client.world
{
    public class WorldServer : World
    {
        public WorldServer() : base("world", "world", "0")
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

        public override bool LoadChunk(ChunkPos chunkPos)
        {
            return false;
        }

        public override void Update(Vector3 playerPos, int renderDistance)
        {
            foreach (Chunk chunk in Chunks.Values)
            {
                chunk.Update();

                var inRadius = chunk.Pos.DistanceTo(playerPos.Xz) > renderDistance * Chunk.ChunkSize + 50;

                if (!inRadius)
                    UnloadChunk(chunk.Pos);
            }

            UpdateEntities();
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
    }
}