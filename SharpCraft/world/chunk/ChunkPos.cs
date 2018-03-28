using System;
using OpenTK;
using SharpCraft.block;
using SharpCraft.util;

namespace SharpCraft.world.chunk
{
	public struct ChunkPos
	{
		public readonly int x,z;

		public ChunkPos(BlockPos pos) : this(pos.X / 16, pos.Z / 16){}

		public ChunkPos(int x,int z)
		{
			this.x = x;
			this.z = z;
		}

		public Vector3 CenterVec()
		{
			return new Vector3(WorldSpaceX()+Chunk.ChunkSize/2F,0,WorldSpaceZ()+Chunk.ChunkSize/2F);
		}

		public Vector3 ToVec()
		{
			return new Vector3(WorldSpaceX(),0,WorldSpaceZ());
		}

		public double DistanceTo(Vector3 vec) => DistanceTo(vec.X,vec.Z);
		public double DistanceTo(Vector2 vec) => DistanceTo(vec.X,vec.Y);
		public double DistanceTo(double x, double z)
		{
			double xD = WorldSpaceX()-x;// xDDDD
			double zD = WorldSpaceZ()-z;
			return Math.Sqrt(xD*xD+zD*zD);
		}

		public float WorldSpaceX()
		{
			return x*Chunk.ChunkSize;
		}
		public float WorldSpaceZ()
		{
			return z*Chunk.ChunkSize;
		}

		public Chunk GetChunk(World inWorld)
		{
			return inWorld.GetChunk(this);
		}

		public static BlockPos ToChunkLocal(BlockPos worldPos)
		{
			return new BlockPos(MathUtil.ToLocal(worldPos.X, Chunk.ChunkSize),MathUtil.MinMax(worldPos.Y,0,255),MathUtil.ToLocal(worldPos.Z,Chunk.ChunkSize));
		}
	}
}