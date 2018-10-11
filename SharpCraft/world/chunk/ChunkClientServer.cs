namespace SharpCraft_Client.world.chunk
{
    public class ChunkClientServer : Chunk
    {
        public ChunkClientServer(ChunkPos pos, World world) : base(pos, world)
        {
        }

        public ChunkClientServer(ChunkPos pos, World world, short[,,] blockData) : base(pos, world, blockData)
        {
        }
    }
}