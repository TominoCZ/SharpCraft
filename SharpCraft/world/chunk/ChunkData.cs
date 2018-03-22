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
    }
}