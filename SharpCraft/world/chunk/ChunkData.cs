namespace SharpCraft
{
    internal class ChunkData
    {
        public Chunk chunk;
        public ModelChunk model;

        public bool modelGenerating;
        public bool chunkGenerated;

        public ChunkData(Chunk chunk, ModelChunk model)
        {
            this.chunk = chunk;
            this.model = model;
        }

        public void beginUpdateModel(World w, bool updateContainingEntities)
        {
            if (!chunkGenerated || modelGenerating)
                return;

            modelGenerating = true;

            ThreadPool.ScheduleTask(false, () =>
            {
                chunk.createChunkModel(w, model, updateContainingEntities);
                modelGenerating = false;
            });
        }
    }
}