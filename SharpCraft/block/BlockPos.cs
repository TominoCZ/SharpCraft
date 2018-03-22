using OpenTK;
using System;

namespace SharpCraft
{
    [Serializable]
    public struct BlockPos
    {
        private readonly int _x;
        private readonly int _y;
        private readonly int _z;

        public int x => _x;
        public int y => _y;
        public int z => _z;

        public Vector3 vector => new Vector3(x, y, z);

        public static BlockPos operator -(BlockPos p1, BlockPos p2)
        {
            return new BlockPos(p1.x - p2.x, p1.y - p2.y, p1.z - p2.z);
        }

        public static BlockPos operator +(BlockPos p1, BlockPos p2)
        {
            return new BlockPos(p1.x + p2.x, p1.y + p2.y, p1.z + p2.z);
        }

        public static bool operator ==(BlockPos p1, BlockPos p2)
        {
            return p1.x == p2.x && p1.y == p2.y && p1.z == p2.z;
        }

        public static bool operator !=(BlockPos p1, BlockPos p2)
        {
            return p1.x != p2.x || p1.y != p2.y || p1.z != p2.z;
        }

        public BlockPos(int x, int y, int z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        public BlockPos(float x, float y, float z)
        {
            _x = (int)Math.Floor(x);
            _y = (int)Math.Floor(y);
            _z = (int)Math.Floor(z);
        }

        public BlockPos(Vector3 vec)
        {
            _x = (int)Math.Floor(vec.X);
            _y = (int)Math.Floor(vec.Y);
            _z = (int)Math.Floor(vec.Z);
        }

        public BlockPos offset(EnumFacing dir)
        {
            return new BlockPos(new Vector3(_x, _y, _z) + ModelHelper.getFacingVector(dir));
        }

        public BlockPos offsetChunk(EnumFacing dir)
        {
            return new BlockPos(new Vector3(_x, _y, _z) + ModelHelper.getFacingVector(dir) * 16);
        }

        public BlockPos ChunkPos()
        {
            var X = (int)Math.Floor(_x / 16f) * 16;
            var Y = (int)Math.Floor(_y / 256f) * 256f;
            var Z = (int)Math.Floor(_z / 16f) * 16;

            return new BlockPos(X, Y, Z);
        }
    }
}