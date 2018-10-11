namespace SharpCraft_Client.world.chunk
{
    public class ChunkServer : Chunk
    {
        public ChunkServer(ChunkPos pos, World world) : base(pos, world)
        {
        }

        public ChunkServer(ChunkPos pos, World world, short[,,] blockData) : base(pos, world, blockData)
        {
        }
    }
}