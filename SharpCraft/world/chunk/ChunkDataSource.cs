using System;

namespace SharpCraft.world.chunk
{
	public interface IChunkDataSource
	{
		short[,,] Get(Chunk chunk);
	}
}