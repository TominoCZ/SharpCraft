using System;
using OpenTK;
using SharpCraft.block;
using SharpCraft.util;

namespace SharpCraft.world.chunk
{
	public struct ChunkPos
	{
		public readonly int x, z;

		public static bool operator ==(ChunkPos    p1, ChunkPos p2) => p1.x == p2.x && p1.z == p2.z;
		public static bool operator !=(ChunkPos    p1, ChunkPos p2) => !(p1 == p2);
		public static ChunkPos operator +(ChunkPos p1, ChunkPos p2) => new ChunkPos(p1.x+p2.x,p1.z+p2.z);

		public static ChunkPos FromWorldSpace(Vector3  pos)                  => FromWorldSpace((int) pos.X, (int) pos.Z);
		public static ChunkPos FromWorldSpace(BlockPos pos)                  => FromWorldSpace(pos.X, pos.Z);
		public static ChunkPos FromWorldSpace(float    xWorld, float zWorld) => FromWorldSpace((int) xWorld, (int) zWorld);

		public static ChunkPos FromWorldSpace(int xWorld, int zWorld)
		{
			MathUtil.ToLocal(xWorld, Chunk.ChunkSize, out var _, out var x);
			MathUtil.ToLocal(zWorld, Chunk.ChunkSize, out var _, out var z);
			return new ChunkPos(x, z);
		}

		public ChunkPos(int x, int z)
		{
			this.x = x;
			this.z = z;
		}

		public Vector3 ToVec()
		{
			return new Vector3(WorldSpaceX(), 0, WorldSpaceZ());
		}

		public double DistanceTo(Vector3 vec) => DistanceTo(vec.X, vec.Z);
		public double DistanceTo(Vector2 vec) => DistanceTo(vec.X, vec.Y);

		public double DistanceTo(double x, double z)
		{
			double xD = WorldSpaceXCenter() - x; // xDDDD
			double zD = WorldSpaceZCenter() - z; // zDDDD
			return Math.Sqrt(xD * xD + zD * zD);
		}

		public float WorldSpaceX() =>x * Chunk.ChunkSize;

		public float WorldSpaceZ() =>z * Chunk.ChunkSize;

		public float WorldSpaceXCenter() =>(x+0.5F) * Chunk.ChunkSize;

		public float WorldSpaceZCenter() =>(z+0.5F) * Chunk.ChunkSize;

		public Chunk GetChunk(World inWorld)
		{
			return inWorld.GetChunk(this);
		}

		public override string ToString()
		{
			return $"ChunkPos[{x}, {z}]";
		}

		public static BlockPos ToChunkLocal(BlockPos worldPos)
		{
			return new BlockPos(MathUtil.ToLocal(worldPos.X, Chunk.ChunkSize), MathUtil.MinMax(worldPos.Y, 0, 255), MathUtil.ToLocal(worldPos.Z, Chunk.ChunkSize));
		}
	}
}