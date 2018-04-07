using OpenTK;
using SharpCraft.world;
using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;

namespace SharpCraft.entity
{
    public class Entity
    {
        protected AxisAlignedBB boundingBox, collisionBoundingBox;

        public World world;

        public Vector3 pos;
        public Vector3 lastPos;

        public Vector3 motion;

        public bool onGround;

        public bool isAlive = true;

        public float gravity = 1.875f;

        protected Entity(World world, Vector3 pos, Vector3 motion = new Vector3())
        {
            this.world = world;
            this.pos = lastPos = pos;
            this.motion = motion;

            collisionBoundingBox = AxisAlignedBB.BLOCK_FULL;
            boundingBox = collisionBoundingBox.offset(pos - (Vector3.UnitX * collisionBoundingBox.size.X / 2 + Vector3.UnitZ * collisionBoundingBox.size.Z / 2));
        }

        public virtual void Update()
        {
            lastPos = pos;

            motion.Y -= 0.04f * gravity;

            Move();

            motion.Xz *= 0.8664021f;

            var bbs = SharpCraft.Instance.World.GetBlockCollisionBoxes(boundingBox);

            if (bbs.Count > 0)
            {
                var dir = Vector3.Zero;

                foreach (var bb in bbs) //TODO - fix this with half sized blocks
                {
                    //dir += pos - bb.GetCenter();
                }

                //dir = dir.Normalized() * 0.25f;
                //dir.Y = 0;

                //motion += dir;
            }

            if (onGround)
            {
                motion.Xz *= 0.6676801f;
            }
        }

        public virtual void Move()
        {
            var bb_o = boundingBox.Union(boundingBox.offset(motion));

            List<AxisAlignedBB> list = SharpCraft.Instance.World.GetBlockCollisionBoxes(bb_o);

            var m_orig = motion;

            for (int i = 0; i < list.Count; i++)
            {
                var blockBB = list[i];
                motion.Y = blockBB.CalculateYOffset(boundingBox, motion.Y);
            }
            boundingBox = boundingBox.offset(motion * Vector3.UnitY);

            for (int i = 0; i < list.Count; i++)
            {
                var blockBB = list[i];
                motion.X = blockBB.CalculateXOffset(boundingBox, motion.X);
            }
            boundingBox = boundingBox.offset(motion * Vector3.UnitX);

            for (int i = 0; i < list.Count; i++)
            {
                var blockBB = list[i];
                motion.Z = blockBB.CalculateZOffset(boundingBox, motion.Z);
            }
            boundingBox = boundingBox.offset(motion * Vector3.UnitZ);

            setPositionToBB();

            var stoppedX = Math.Abs(m_orig.X - motion.X) > 0.00001f;
            var stoppedY = Math.Abs(m_orig.Y - motion.Y) > 0.00001f;
            var stoppedZ = Math.Abs(m_orig.Z - motion.Z) > 0.00001f;

            onGround = stoppedY && m_orig.Y < 0.0D;

            var onCeiling = stoppedY && m_orig.Y > 0.0D;

            if (stoppedX)
                motion.X = 0;

            if (stoppedZ)
                motion.Z = 0;

            if (onCeiling)
                motion.Y *= 0.15f;
            else if (onGround)
                motion.Y = 0;
        }

        public virtual void Render(float particalTicks)
        {
        }

        public void TeleportTo(Vector3 pos)
        {
            this.pos = lastPos = pos;

            boundingBox = collisionBoundingBox.offset(pos - Vector3.UnitX * collisionBoundingBox.size.X / 2 - Vector3.UnitZ * collisionBoundingBox.size.Z / 2);
        }

        public virtual void SetDead()
        {
            isAlive = false;
        }

        public AxisAlignedBB getEntityBoundingBox()
        {
            return boundingBox;
        }

        public AxisAlignedBB getCollisionBoundingBox()
        {
            return collisionBoundingBox;
        }

        protected void setPositionToBB()
        {
            pos.X = (boundingBox.min.X + boundingBox.max.X) / 2.0f;
            pos.Y = boundingBox.min.Y;
            pos.Z = (boundingBox.min.Z + boundingBox.max.Z) / 2.0f;
        }
    }
}