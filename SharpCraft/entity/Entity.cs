using OpenTK;
using SharpCraft.world;
using System;
using System.Collections.Generic;

namespace SharpCraft.entity
{
    public class Entity
    {
        protected AxisAlignedBB boundingBox, collisionBoundingBox;

        public World World;

        public Vector3 Pos;
        public Vector3 LastPos;

        public Vector3 Motion;

        public bool onGround;

        public bool isAlive = true;

        public float gravity = 1.875f;

        protected Entity(World world, Vector3 pos, Vector3 motion = new Vector3())
        {
            World = world;
            Pos = LastPos = pos;
            Motion = motion;

            collisionBoundingBox = AxisAlignedBB.BLOCK_FULL;
            boundingBox = collisionBoundingBox.offset(pos - collisionBoundingBox.size / 2);
        }

        public virtual void Update()
        {
            LastPos = Pos;

            Motion.Y -= 0.04f * gravity;

            Move();

            Motion.Xz *= 0.8664021f;

            if (onGround)
            {
                Motion.Xz *= 0.6676801f;
            }
        }

        public virtual void Move()
        {
            AxisAlignedBB bbO = boundingBox.Union(boundingBox.offset(Motion));

            List<AxisAlignedBB> list = SharpCraft.Instance.World.GetBlockCollisionBoxes(bbO);

            Vector3 mOrig = Motion;

            for (int i = 0; i < list.Count; i++)
            {
                AxisAlignedBB blockBb = list[i];
                Motion.Y = blockBb.CalculateYOffset(boundingBox, Motion.Y);
            }
            boundingBox = boundingBox.offset(Motion * Vector3.UnitY);

            for (int i = 0; i < list.Count; i++)
            {
                AxisAlignedBB blockBb = list[i];
                Motion.X = blockBb.CalculateXOffset(boundingBox, Motion.X);
            }
            boundingBox = boundingBox.offset(Motion * Vector3.UnitX);

            for (int i = 0; i < list.Count; i++)
            {
                AxisAlignedBB blockBb = list[i];
                Motion.Z = blockBb.CalculateZOffset(boundingBox, Motion.Z);
            }
            boundingBox = boundingBox.offset(Motion * Vector3.UnitZ);

            SetPositionToBB();

            bool stoppedX = Math.Abs(mOrig.X - Motion.X) > 0.00001f;
            bool stoppedY = Math.Abs(mOrig.Y - Motion.Y) > 0.00001f;
            bool stoppedZ = Math.Abs(mOrig.Z - Motion.Z) > 0.00001f;

            onGround = stoppedY && mOrig.Y < 0.0D;

            bool onCeiling = stoppedY && mOrig.Y > 0.0D;

            if (stoppedX)
                Motion.X = 0;

            if (stoppedZ)
                Motion.Z = 0;

            if (onCeiling)
                Motion.Y *= 0.15f;
            else if (onGround)
                Motion.Y = 0;
        }

        public virtual void Render(float partialTicks)
        {
        }

        public void TeleportTo(Vector3 pos)
        {
            this.Pos = LastPos = pos;

            boundingBox = collisionBoundingBox.offset(pos - Vector3.UnitX * collisionBoundingBox.size.X / 2 - Vector3.UnitZ * collisionBoundingBox.size.Z / 2);
        }

        public virtual void SetDead()
        {
            isAlive = false;
        }

        public AxisAlignedBB GetEntityBoundingBox()
        {
            return boundingBox;
        }

        public AxisAlignedBB GetCollisionBoundingBox()
        {
            return collisionBoundingBox;
        }

        protected void SetPositionToBB()
        {
            Vector3 center = boundingBox.GetCenter();

            Pos.X = center.X;
            Pos.Y = boundingBox.min.Y;
            Pos.Z = center.Z;
        }
    }
}