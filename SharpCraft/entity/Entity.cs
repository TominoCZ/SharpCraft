using System;
using System.Collections.Generic;
using OpenTK;

namespace SharpCraft.entity
{
    public class Entity
    {
        protected AxisAlignedBB boundingBox, collisionBoundingBox;

        public Vector3 pos;
        public Vector3 lastPos;

        public Vector3 motion;

        public bool onGround;

        protected Entity(Vector3 pos)
        {
            this.pos = pos;

            collisionBoundingBox = AxisAlignedBB.BLOCK_FULL.offset(Vector3.One * -0.5f);
            boundingBox = collisionBoundingBox.offset(pos);
        }

        public virtual void Update()
        {
            lastPos = pos;

            motion.Y -= 0.0775f;

            Move();

            motion.Xz *= 0.8664021f;

            if (onGround)
            {
                motion.Xz *= 0.6676801f;
            }
        }

        public virtual void Move()
        {
            var bb_o = boundingBox.union(boundingBox.offset(motion));

            List<AxisAlignedBB> list = Game.Instance.World.GetBlockCollisionBoxes(bb_o);

            var m_orig = motion;

            for (int i = 0; i < list.Count; i++)
            {
                var blockBB = list[i];
                motion.Y = blockBB.calculateYOffset(boundingBox, motion.Y);
            }
            boundingBox = boundingBox.offset(motion * Vector3.UnitY);

            for (int i = 0; i < list.Count; i++)
            {
                var blockBB = list[i];
                motion.X = blockBB.calculateXOffset(boundingBox, motion.X);
            }
            boundingBox = boundingBox.offset(motion * Vector3.UnitX);

            for (int i = 0; i < list.Count; i++)
            {
                var blockBB = list[i];
                motion.Z = blockBB.calculateZOffset(boundingBox, motion.Z);
            }
            boundingBox = boundingBox.offset(motion * Vector3.UnitZ);

            setPositionToBB();

            onGround = Math.Abs(m_orig.Y - motion.Y) > 0.00001f && m_orig.Y < 0.0D;

            if (Math.Abs(m_orig.X - motion.X) > 0.00001f)
                motion.X = 0;

            if (Math.Abs(m_orig.Z - motion.Z) > 0.00001f)
                motion.Z = 0;

            if (onGround && motion.Y < 0)
                motion.Y = 0;
        }

        public virtual void Render(float particalTicks)
        {
        }

        public void teleportTo(Vector3 pos)
        {
            this.pos = lastPos = pos;

            boundingBox = collisionBoundingBox.offset(pos);
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