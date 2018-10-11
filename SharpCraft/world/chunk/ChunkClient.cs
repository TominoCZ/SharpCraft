namespace SharpCraft_Client.world.chunk
{
    public class ChunkClient : Chunk
    {
        public ChunkClient(ChunkPos pos, World world) : base(pos, world)
        {
        }

        public ChunkClient(ChunkPos pos, World world, short[,,] blockData) : base(pos, world, blockData)
        {
        }
    }
}