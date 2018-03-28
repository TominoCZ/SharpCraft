using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.util;

namespace SharpCraft.block
{
    [Serializable]
    public struct BlockPos
    {
        public int x { get; }
        public int y { get; }
        public int z { get; }
        
        public Vector3 toVec()
        {
            return new Vector3(x,y,z);
        }

        public static BlockPos operator +(BlockPos p1, BlockPos p2) => new BlockPos(p1.x + p2.x, p1.y + p2.y, p1.z + p2.z);

        public static BlockPos operator -(BlockPos p1, BlockPos p2) => new BlockPos(p1.x - p2.x, p1.y - p2.y, p1.z - p2.z);

        public BlockPos(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public BlockPos(float x, float y, float z) : this((int)Math.Floor(x), (int)Math.Floor(y), (int)Math.Floor(z))
        {
        }

        public BlockPos(Vector3 vec) : this((int)Math.Floor(vec.X), (int)Math.Floor(vec.Y), (int)Math.Floor(vec.Z))
        {
        }

        public BlockPos offset(EnumFacing dir)
        {
            return new BlockPos(new Vector3(x, y, z) + FacingUtil.getFacingVector(dir));
        }

        public BlockPos offset(float x, float y, float z)
        {
            return new BlockPos(this.x + x, this.y + y, this.z + z);
        }

        public BlockPos offsetChunk(EnumFacing dir)
        {
            return new BlockPos(new Vector3(x, y, z) + FacingUtil.getFacingVector(dir) * 16);
        }

        public BlockPos chunkPos()
        {
            return new BlockPos((int)Math.Floor(x / 16f) * 16, 0, (int)Math.Floor(z / 16f) * 16);
        }

        public override string ToString()
        {
            return $"BlockPos[{x},{y},{z}]";
        }
    }
}