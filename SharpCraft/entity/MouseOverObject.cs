using OpenTK;
using SharpCraft_Client.block;
using SharpCraft_Client.util;

namespace SharpCraft_Client.entity
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