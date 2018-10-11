using OpenTK;
using System;
using SharpCraft_Client.world.chunk;

namespace SharpCraft_Client.block
{
    [Serializable]
    public struct BlockPos
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        public Vector3 ToVec()
        {
            return new Vector3(X, Y, Z);
        }

        public static bool operator ==(BlockPos p1, BlockPos p2) => p1.X == p2.X && p1.Y == p2.Y && p1.Z == p2.Z;

        public static bool operator !=(BlockPos p1, BlockPos p2) => !(p1 == p2);

        public static BlockPos operator +(BlockPos p1, BlockPos p2) => new BlockPos(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z);

        public static BlockPos operator +(BlockPos p1, Vector3 p2) => new BlockPos(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z);

        public static BlockPos operator -(BlockPos p1, BlockPos p2) => new BlockPos(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);

        public BlockPos(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public BlockPos(float x, float y, float z) : this((int)Math.Floor(x), (int)Math.Floor(y), (int)Math.Floor(z))
        {
        }

        public BlockPos(Vector3 vec) : this((int)Math.Floor(vec.X), (int)Math.Floor(vec.Y), (int)Math.Floor(vec.Z))
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

        public override bool Equals(object obj)
        {
            if (!(obj is BlockPos))
            {
                return false;
            }

            BlockPos pos = (BlockPos)obj;
            return X == pos.X &&
                   Y == pos.Y &&
                   Z == pos.Z;
        }

        public override int GetHashCode()
        {
            int hashCode = -307843816;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + Z.GetHashCode();
            return hashCode;
        }
    }
}