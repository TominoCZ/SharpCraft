namespace SharpCraft.world.chunk.region
{
    public interface IRegionCord
    {
        int Length { get; }
        int this[int i] { get; }
    }
}