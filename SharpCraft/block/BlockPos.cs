using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.util;
using SharpCraft.world.chunk;

namespace SharpCraft.block
{
	[Serializable]
	public struct BlockPos
	{
		public int X { get; }
		public int Y { get; }
		public int Z { get; }

		public Vector3 ToVec()
		{
			return new Vector3(X, Y, Z);
		}

		public static BlockPos operator +(BlockPos p1, BlockPos p2) => new BlockPos(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z);
		public static BlockPos operator +(BlockPos p1, Vector3  p2) => new BlockPos(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z);

		public static BlockPos operator -(BlockPos p1, BlockPos p2) => new BlockPos(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);

		public BlockPos(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public BlockPos(float x, float y, float z) : this((int) Math.Floor(x), (int) Math.Floor(y), (int) Math.Floor(z))
		{
		}

		public BlockPos(Vector3 vec) : this((int) Math.Floor(vec.X), (int) Math.Floor(vec.Y), (int) Math.Floor(vec.Z))
		{
		}

		public BlockPos Offset(FaceSides dir)
		{
			return this + dir;
		}

		public BlockPos Offset(float x, float y, float z)
		{
			return new BlockPos(X + x, Y + y, Z + z);
		}

		public BlockPos OffsetChunk(FaceSides dir)
		{
			return this + dir * Chunk.ChunkSize;
		}

		public ChunkPos ChunkPos()
		{
			return world.chunk.ChunkPos.FromWorldSpace(this);
		}

		public override string ToString()
		{
			return $"BlockPos[{X},{Y},{Z}]";
		}
	}
}