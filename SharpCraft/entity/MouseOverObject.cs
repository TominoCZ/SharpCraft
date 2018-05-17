using OpenTK;
using SharpCraft.block;

namespace SharpCraft.entity
{
    internal struct MouseOverObject
    {
        public FaceSides sideHit;

        public Vector3 hitVec;

        public Vector3 normal;

        public BlockPos blockPos;

        public AxisAlignedBB boundingBox;

        public dynamic hit;
    }
}