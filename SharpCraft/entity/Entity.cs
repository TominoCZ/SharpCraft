using OpenTK;
using SharpCraft.world;
using System;
using System.Collections.Generic;
using System.Linq;
using SharpCraft.block;

namespace SharpCraft.entity
{
    public class Entity
    {
        protected AxisAlignedBB BoundingBox, CollisionBoundingBox;

        public World World;

        public Vector3 Pos;
        public Vector3 LastPos;

        public Vector3 Motion;

        public bool OnGround;

        public bool IsAlive = true;

        public float Gravity = 1.875f;

        protected static readonly float StepHeight = 0.5f;

        protected Entity(World world, Vector3 pos, Vector3 motion = new Vector3())
        {
            World = world;
            Pos = LastPos = pos;
            Motion = motion;

            CollisionBoundingBox = AxisAlignedBB.BLOCK_FULL;
            BoundingBox = CollisionBoundingBox.offset(pos - CollisionBoundingBox.size / 2);
        }

        public virtual void Update()
        {
            LastPos = Pos;

            Motion.Y -= 0.04f * Gravity;

            Vector3 motion = Motion;
            motion.Y = 0;

            if (OnGround && Motion.Xz.Length > 0.0001f)
            {
                AxisAlignedBB bbO = BoundingBox.Union(BoundingBox.offset(motion));

                var list = SharpCraft.Instance.World.GetBlockCollisionBoxes(bbO).OrderBy(box => (box.min - new BlockPos(box.min).ToVec() + box.size).Y);

                foreach (var bb in list)
                {
                    var blockPos = new BlockPos(bb.min);
                    var bbTop = bb.min + bb.size;
                    var b = bbTop - blockPos.ToVec();

                    var step = bbTop.Y - Pos.Y;

                    if (step <= StepHeight && step > 0)
                    {
                        Motion.Y = 0;
                        Pos.Y = blockPos.Y + b.Y;
                        
                        TeleportTo(Pos);
                    }
                }
            }

            Move();

            Motion.Xz *= 0.8664021f;

            if (OnGround)
            {
                Motion.Xz *= 0.6676801f;
            }
        }

        public virtual void Move()
        {
            AxisAlignedBB bbO = BoundingBox.Union(BoundingBox.offset(Motion));

            List<AxisAlignedBB> list = SharpCraft.Instance.World.GetBlockCollisionBoxes(bbO);

            Vector3 mOrig = Motion;

            for (int i = 0; i < list.Count; i++)
            {
                AxisAlignedBB blockBb = list[i];
                Motion.Y = blockBb.CalculateYOffset(BoundingBox, Motion.Y);
            }
            BoundingBox = BoundingBox.offset(Motion * Vector3.UnitY);

            for (int i = 0; i < list.Count; i++)
            {
                AxisAlignedBB blockBb = list[i];
                Motion.X = blockBb.CalculateXOffset(BoundingBox, Motion.X);
            }
            BoundingBox = BoundingBox.offset(Motion * Vector3.UnitX);

            for (int i = 0; i < list.Count; i++)
            {
                AxisAlignedBB blockBb = list[i];
                Motion.Z = blockBb.CalculateZOffset(BoundingBox, Motion.Z);
            }
            BoundingBox = BoundingBox.offset(Motion * Vector3.UnitZ);

            SetPositionToBb();

            bool stoppedX = Math.Abs(mOrig.X - Motion.X) > 0.00001f;
            bool stoppedY = Math.Abs(mOrig.Y - Motion.Y) > 0.00001f;
            bool stoppedZ = Math.Abs(mOrig.Z - Motion.Z) > 0.00001f;

            OnGround = stoppedY && mOrig.Y < 0.0D;

            bool onCeiling = stoppedY && mOrig.Y > 0.0D;

            if (stoppedX)
                Motion.X = 0;

            if (stoppedZ)
                Motion.Z = 0;

            if (onCeiling)
                Motion.Y *= 0.15f;
            else if (OnGround)
                Motion.Y = 0;
        }

        public virtual void Render(float partialTicks)
        {
        }

        public void TeleportTo(Vector3 pos)
        {
            this.Pos = LastPos = pos;

            BoundingBox = CollisionBoundingBox.offset(pos - Vector3.UnitX * CollisionBoundingBox.size.X / 2 - Vector3.UnitZ * CollisionBoundingBox.size.Z / 2);
        }

        public virtual void SetDead()
        {
            IsAlive = false;
        }

        public AxisAlignedBB GetEntityBoundingBox()
        {
            return BoundingBox;
        }

        public AxisAlignedBB GetCollisionBoundingBox()
        {
            return CollisionBoundingBox;
        }

        protected void SetPositionToBb()
        {
            Vector3 center = BoundingBox.GetCenter();

            Pos.X = center.X;
            Pos.Y = BoundingBox.min.Y;
            Pos.Z = center.Z;
        }
    }
}