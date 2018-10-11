using System;
using System.Collections.Generic;
using System.Linq;
using SharpCraft_Client.block;
using SharpCraft_Client.world.chunk;

namespace SharpCraft_Client.world
{
    public class WorldClient : World
    {
        //TODO - clientside only
        public WorldClient(string saveName, string levelName, string seed) : base(saveName, levelName, seed)
        {
            foreach (var block in BlockRegistry.AllBlocks())
            {
                _worldLut.Put(block.UnlocalizedName);
            }
        }

        /*public void SetBlockState(BlockPos pos, BlockState state, bool rebuild = true)
        {
            Chunk chunk = GetChunk(ChunkPos.FromWorldSpace(pos));
            if (chunk == null || !chunk.HasData)
                return;

            _worldLut.Put(state.Block.UnlocalizedName);

            var localPos = ChunkPos.ToChunkLocal(pos);

            chunk.SetBlockState(localPos, state, rebuild);

            if (state.Block.CreateTileEntity(this, pos) is TileEntity te)
            {
                chunk.AddTileEntity(localPos, te);
            }

            chunk.Save();
        }*/
        /*
        public void SaveAllChunks()
        {
            foreach (Chunk data in Chunks.Values)
            {
                if (data.HasData)
                    data.Save();
            }
        }
        */

        public void DestroyChunkModels()
        {
            foreach (Chunk data in Chunks.Values)
            {
                data.DestroyModel();
            }

            GC.Collect();
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

        public IEnumerable<Chunk> GetNeighbourChunks(ChunkPos pos)
        {
            return FaceSides.YPlane.Select(dir => GetChunk(pos + dir));
        }

        public void SaveAllChunks()
        {
            foreach (var chunk in Chunks.Values)
            {
                chunk.Save();
            }
        }
    }
}