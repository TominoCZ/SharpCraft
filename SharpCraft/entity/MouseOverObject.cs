using OpenTK;
using SharpCraft.block;
using SharpCraft.util;

namespace SharpCraft.entity
{
    public struct MouseOverObject
    {
        public FaceSides sideHit;

        public Vector3 hitVec;

        public Vector3 normal;

        public BlockPos blockPos;

        public AxisAlignedBb boundingBox;

        public HitType hit;
    }
}