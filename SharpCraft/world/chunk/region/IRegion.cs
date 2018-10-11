namespace SharpCraft_Client.world.chunk.region
{
    public interface IRegion
    {
        void WriteChunkData(int id, byte[] data);

        byte[] ReadChunkData(int id);

        void Optimize();
    }
}