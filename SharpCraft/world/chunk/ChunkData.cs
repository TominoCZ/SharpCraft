using System.Threading;
using SharpCraft.model;

namespace SharpCraft.world.chunk
{
    internal class ChunkData
    {
        public Chunk Chunk;
        public ModelChunk Model;

        public bool ModelGenerating { get; set; }
    
        public bool ChunkGenerated;

        public ChunkData(Chunk chunk, ModelChunk model)
        {
            Chunk = chunk;
            Model = model;
        }

        public void BeginUpdateModel()
        {
            if (!ChunkGenerated || ModelGenerating)
                return;

            ModelGenerating = true;

            ThreadPool.QueueUserWorkItem(e =>
            {
                Chunk.BuildChunkModel(Model);
                ModelGenerating = false;
            });
        }
    }
}