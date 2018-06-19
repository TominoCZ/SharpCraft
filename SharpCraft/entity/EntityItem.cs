using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.block;
using SharpCraft.item;
using SharpCraft.model;
using SharpCraft.render.shader;
using SharpCraft.texture;
using SharpCraft.util;
using SharpCraft.world;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpCraft.entity
{
    public class EntityItem : Entity
    {
        private static readonly Shader<EntityItem> _shader;

        private readonly ItemStack stack;

        private int entityAge;

        private long tick;

        static EntityItem()
        {
            _shader = new Shader<EntityItem>("entity_item");
        }

        public EntityItem(World world, Vector3 pos, Vector3 motion, ItemStack stack) : base(world, pos, motion)
        {
            this.stack = stack;

            collisionBoundingBox = new AxisAlignedBB(0.25f);
            boundingBox = collisionBoundingBox.offset(pos - Vector3.One * collisionBoundingBox.size / 2);

            gravity = 1.25f;

            isAlive = stack != null && !stack.IsEmpty;
        }

        public override void Update()
        {
            tick++;

            LastPos = Pos;

            Motion.Y -= 0.04f * gravity;

            Move();

            Motion.Xz *= 0.8664021f;

            List<AxisAlignedBB> bbs = SharpCraft.Instance.World.GetBlockCollisionBoxes(boundingBox);

            if (bbs.Count > 0)
            {
                BlockPos bp = new BlockPos(Pos);

                FaceSides lastFace = FaceSides.Up;
                bool blocksAround = FaceSides.YPlane.All(face => World.IsAir(bp.Offset(lastFace = face)))
                                   && World.IsAir(bp.Offset(lastFace = FaceSides.Up))
                                   && World.IsAir(bp.Offset(lastFace = FaceSides.Down)); //has to be in this order

                if (!blocksAround)
                {
                    Motion += lastFace.ToVec() * 0.1f;
                }
            }

            if (onGround)
            {
                Motion.Xz *= 0.6676801f;
            }

            if (++entityAge >= 20 * 50 * 60 + 10) //stay on ground for a minute, 20 ticks as a pick up delay
            {
                SetDead();
                return;
            }

            if (entityAge < 5)
                return;

            List<EntityItem> inAttractionArea = World.Entities.OfType<EntityItem>().Where(e => e != this && e.isAlive && e.stack.ItemSame(stack)).OrderByDescending(e => e.stack.Count).ToList();
            float attractionRange = 1.8F;
            float mergeRange = 0.15F;

            foreach (EntityItem entity in inAttractionArea)
            {
                if (stack.IsEmpty || entity.stack.IsEmpty || entity.stack.Count == entity.stack.Item.GetMaxStackSize())
                    continue;

                Vector3 distanceVector = entity.Pos - Pos;
                float distance = distanceVector.Length;
                if (distance >= attractionRange) continue;

                int ammountToTake = Math.Min(stack.Item.GetMaxStackSize() - stack.Count, entity.stack.Count);
                if (ammountToTake == 0) continue;

                if (distance <= mergeRange)
                {
                    //Motion -= entity.Motion * MathUtil.Remap(entity.stack.Count / (float)stack.Count, 1, 64, 1, 3);
                    //entity.Motion -= Motion * MathUtil.Remap(stack.Count / (float)entity.stack.Count, 1, 64, 1, 3);

                    entity.stack.Count -= ammountToTake;
                    if (entity.stack.IsEmpty) entity.SetDead();
                    stack.Count += ammountToTake;

                    entityAge = 3;
                    entity.entityAge = 1;
                    continue;
                }

                distanceVector.Normalize();

                float distanceMul = (float)Math.Sqrt(1 - distance / attractionRange);
                if (distanceMul > 0.8) distanceMul = ((1 - distanceMul) / 0.2F) * 0.6F + 0.2F;
                Vector3 baseForce = distanceVector * 0.02f * distanceMul * MathUtil.Remap(stack.Count / (float)entity.stack.Count, 1, entity.stack.Item.GetMaxStackSize(), 2, 5);

                Motion += baseForce * entity.stack.Count / Math.Max(entity.stack.Count, stack.Count);
                entity.Motion -= baseForce * stack.Count / Math.Max(entity.stack.Count, stack.Count);
            }

            if (entityAge < 15 || !isAlive)
                return;

            //TODO change this for multiplayer
            IEnumerable<EntityPlayerSP> players = World.Entities.OfType<EntityPlayerSP>()
                               .OrderBy(entity => MathUtil.Distance(entity.Pos, Pos))
                               .Where(e => MathUtil.Distance(e.Pos, Pos) <= attractionRange);

            foreach (EntityPlayerSP player in players)
            {
                if (!player.CanPickUpStack(stack))
                    continue;

                Vector3 attrTarget = player.Pos;
                attrTarget.Y += player.GetCollisionBoundingBox().size.Y / 2;

                Vector3 distanceVector = attrTarget - Pos;

                if (distanceVector.Length <= 0.35f)
                {
                    if (player.OnPickup(stack))
                        SetDead();

                    Motion *= -1f;
                }

                Motion = distanceVector.Normalized() * 0.45f;
            }
            if (stack.IsEmpty) SetDead();
        }

        public override void Render(float partialTicks)
        {
            Vector3 partialPos = LastPos + (Pos - LastPos) * partialTicks;
            float partialTime = tick + partialTicks;

            if (stack?.Item is ItemBlock itemBlock)
            {
                ModelBlock model = JsonModelLoader.GetModelForBlock(itemBlock.Block.UnlocalizedName);

                if (model == null || model.RawModel == null)
                    return;

                float time = onGround ? (float)((Math.Sin(partialTime / 8) + 1) / 16) : 0;

                _shader.Bind();

                GL.BindVertexArray(model.RawModel.VaoID);

                GL.EnableVertexAttribArray(0);
                GL.EnableVertexAttribArray(1);
                GL.EnableVertexAttribArray(2);
                
                GL.BindTexture(TextureTarget.Texture2D, JsonModelLoader.TEXTURE_BLOCKS);

                int itemsToRender = 1;

                if (stack.Count > 1)
                    itemsToRender = 2;
                if (stack.Count >= 32 * 4)
                    itemsToRender = 3;
                if (stack.Count == 64 * 4)
                    itemsToRender = 4;

                for (int i = 0; i < itemsToRender; i++)
                {
                    Vector3 rot = Vector3.UnitY * partialTime * 3;
                    Vector3 pos = partialPos - (Vector3.UnitX * 0.125f + Vector3.UnitZ * 0.125f) + Vector3.UnitY * time;
                    Vector3 pos_o = Vector3.One * (i / 8f);

                    Matrix4 x = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rot.X));
                    Matrix4 y = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rot.Y));
                    Matrix4 z = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rot.Z));

                    Vector3 vec = Vector3.One * 0.5f;

                    Matrix4 s = Matrix4.CreateScale(0.25f);
                    Matrix4 t = Matrix4.CreateTranslation(pos + vec * 0.25f);
                    Matrix4 t2 = Matrix4.CreateTranslation(-vec);
                    Matrix4 t3 = Matrix4.CreateTranslation(pos_o);

                    Matrix4 mat = t3 * t2 * (z * y * x * s) * t;

                    _shader.UpdateGlobalUniforms();
                    _shader.UpdateModelUniforms(model.RawModel);
                    _shader.UpdateInstanceUniforms(mat, this);
                    model.RawModel.Render(PrimitiveType.Quads);
                }

                GL.BindVertexArray(0);

                GL.DisableVertexAttribArray(0);
                GL.DisableVertexAttribArray(1);
                GL.DisableVertexAttribArray(2);

                _shader.Unbind();
            }
        }
    }
}