using OpenTK;
using System;

namespace SharpCraft
{
    [Serializable]
    public struct BlockPos
    {
        public int x { get; }
        public int y { get; }
        public int z { get; }

        public Vector3 vector { get; }

        public static BlockPos operator +(BlockPos p1, BlockPos p2) => new BlockPos(p1.x + p2.x, p1.y + p2.y, p1.z + p2.z);

        public static BlockPos operator -(BlockPos p1, BlockPos p2) => new BlockPos(p1.x - p2.x, p1.y - p2.y, p1.z - p2.z);

        public BlockPos(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;

            vector = new Vector3(this.x, this.y, this.z);
        }

        public BlockPos(float x, float y, float z) : this((int)Math.Floor(x), (int)Math.Floor(y), (int)Math.Floor(z))
        {
        }

        public BlockPos(Vector3 vec) : this((int)Math.Floor(vec.X), (int)Math.Floor(vec.Y), (int)Math.Floor(vec.Z))
        {
        }

        public BlockPos offset(EnumFacing dir)
        {
            return new BlockPos(new Vector3(x, y, z) + ModelHelper.getFacingVector(dir));
        }

        public BlockPos offset(float x, float y, float z)
        {
            return new BlockPos(this.x + x, this.y + y, this.z + z);
        }

        public BlockPos offsetChunk(EnumFacing dir)
        {
            return new BlockPos(new Vector3(x, y, z) + ModelHelper.getFacingVector(dir) * 16);
        }

        public BlockPos ChunkPos()
        {
            var X = (int)Math.Floor(x / 16f) * 16;
            var Y = (int)Math.Floor(y / 256f) * 256f;
            var Z = (int)Math.Floor(z / 16f) * 16;

            return new BlockPos(X, Y, Z);
        }

        public override string ToString()
        {
            return $"BlockPos[{x},{y},{z}]";
        }
    }
}