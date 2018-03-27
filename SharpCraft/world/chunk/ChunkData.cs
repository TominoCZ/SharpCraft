using System.Threading;

namespace SharpCraft
{
    internal class ChunkData
    {
        public Chunk chunk;
        public ModelChunk model;

        public bool modelGenerating { get; set; }
    
        public bool chunkGenerated;

        public ChunkData(Chunk chunk, ModelChunk model)
        {
            this.chunk = chunk;
            this.model = model;
        }

        public void beginUpdateModel()
        {
            if (!chunkGenerated || modelGenerating)
                return;

            modelGenerating = true;

            ThreadPool.QueueUserWorkItem(e =>
            {
                chunk.buildChunkModel(model);
                modelGenerating = false;
            });
        }
    }
}