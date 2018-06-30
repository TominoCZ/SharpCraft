using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.block;
using SharpCraft.item;
using SharpCraft.model;
using SharpCraft.render.shader;
using SharpCraft.util;
using SharpCraft.world;
using System;
using System.Collections.Generic;
using System.Linq;
using SharpCraft.texture;

namespace SharpCraft.entity
{
    public class EntityItem : Entity
    {
        private static readonly Shader<EntityItem> Shader;

        private readonly ItemStack _stack;

        private int _entityAge;

        private int _tick;
        private int _tickLast;

        static EntityItem()
        {
            Shader = new Shader<EntityItem>("entity_item");
        }

        public EntityItem(World world, Vector3 pos, Vector3 motion, ItemStack stack, bool noDelay = false) : base(world, pos, motion)
        {
            if (noDelay)
                _entityAge = 14;

            _stack = stack;

            CollisionBoundingBox = new AxisAlignedBB(0.25f);
            BoundingBox = CollisionBoundingBox.offset(pos - Vector3.One * CollisionBoundingBox.size / 2);

            Gravity = 1.25f;

            IsAlive = stack != null && !stack.IsEmpty;
        }

        public override void Update()
        {
            _tickLast = _tick;

            if (OnGround)
                _tick = (_tick + 1) % 360;

            LastPos = Pos;

            Motion.Y -= 0.04f * Gravity;

            Move();

            Motion.Xz *= 0.8664021f;

            List<AxisAlignedBB> bbs = SharpCraft.Instance.World.GetBlockCollisionBoxes(BoundingBox);

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

            if (OnGround)
            {
                Motion.Xz *= 0.6676801f;
            }

            if (++_entityAge >= 20 * 50 * 60 + 10) //stay on ground for a minute, 20 ticks as a pick up delay
            {
                SetDead();
                return;
            }

            if (_entityAge < 5)
                return;

            List<EntityItem> inAttractionArea = World.Entities.OfType<EntityItem>().Where(e => e != this && e.IsAlive && e._stack.ItemSame(_stack)).OrderByDescending(e => e._stack.Count).ToList();
            float attractionRange = 1.8F;
            float mergeRange = 0.15F;

            foreach (EntityItem entity in inAttractionArea)
            {
                if (_stack.IsEmpty || entity._stack.IsEmpty || entity._stack.Count == entity._stack.Item.GetMaxStackSize())
                    continue;

                Vector3 distanceVector = entity.Pos - Pos;
                float distance = distanceVector.Length;
                if (distance >= attractionRange) continue;

                int ammountToTake = Math.Min(_stack.Item.GetMaxStackSize() - _stack.Count, entity._stack.Count);
                if (ammountToTake == 0) continue;

                if (distance <= mergeRange)
                {
                    //Motion -= entity.Motion * MathUtil.Remap(entity.stack.Count / (float)stack.Count, 1, 64, 1, 3);
                    //entity.Motion -= Motion * MathUtil.Remap(stack.Count / (float)entity.stack.Count, 1, 64, 1, 3);

                    entity._stack.Count -= ammountToTake;
                    if (entity._stack.IsEmpty) entity.SetDead();
                    _stack.Count += ammountToTake;

                    _entityAge = 3;
                    entity._entityAge = 1;
                    continue;
                }

                distanceVector.Normalize();

                float distanceMul = (float)Math.Sqrt(1 - distance / attractionRange);
                if (distanceMul > 0.8) distanceMul = ((1 - distanceMul) / 0.2F) * 0.6F + 0.2F;
                Vector3 baseForce = distanceVector * 0.02f * distanceMul * MathUtil.Remap(_stack.Count / (float)entity._stack.Count, 1, entity._stack.Item.GetMaxStackSize(), 2, 5);

                Motion += baseForce * entity._stack.Count / Math.Max(entity._stack.Count, _stack.Count);
                entity.Motion -= baseForce * _stack.Count / Math.Max(entity._stack.Count, _stack.Count);
            }

            if (_entityAge < 15 || !IsAlive)
                return;

            //TODO change this for multiplayer
            IEnumerable<EntityPlayerSP> players = World.Entities.OfType<EntityPlayerSP>()
                               .OrderBy(entity => MathUtil.Distance(entity.Pos, Pos))
                               .Where(e => MathUtil.Distance(e.Pos, Pos) <= attractionRange);

            foreach (EntityPlayerSP player in players)
            {
                if (!player.CanPickUpStack(_stack))
                    continue;

                Vector3 attrTarget = player.Pos;
                attrTarget.Y += player.GetCollisionBoundingBox().size.Y / 2;

                Vector3 distanceVector = attrTarget - Pos;

                if (distanceVector.Length <= 0.35f)
                {
                    if (player.OnPickup(_stack))
                        SetDead();

                    Motion *= -1f;
                }

                Motion = distanceVector.Normalized() * 0.45f;
            }
            if (_stack.IsEmpty) SetDead();
        }

        public override void Render(float partialTicks)
        {
            Vector3 partialPos = LastPos + (Pos - LastPos) * partialTicks;
            float partialTime = _tickLast + (_tick - _tickLast) * partialTicks;

            if (_stack == null || _stack.IsEmpty)
                return;

            if (_stack.Item is ItemBlock itemBlock)
            {
                ModelBlock model = JsonModelLoader.GetModelForBlock(itemBlock.Block.UnlocalizedName);

                if (model?.RawModel == null)
                    return;

                float time = OnGround ? (float)((Math.Sin(partialTime / 8) + 1) / 16) : 0;

                Shader.Bind();

                GL.BindVertexArray(model.RawModel.VaoID);

                GL.EnableVertexAttribArray(0);
                GL.EnableVertexAttribArray(1);
                GL.EnableVertexAttribArray(2);

                GL.BindTexture(TextureTarget.Texture2D, JsonModelLoader.TEXTURE_BLOCKS);

                int itemsToRender = 1;

                if (_stack.Count > 1)
                    itemsToRender = 2;
                if (_stack.Count >= 32 * 4)
                    itemsToRender = 3;
                if (_stack.Count == 64 * 4)
                    itemsToRender = 4;

                for (int i = 0; i < itemsToRender; i++)
                {
                    Vector3 rot = Vector3.UnitY * partialTime * 4;
                    Vector3 pos = partialPos - (Vector3.UnitX * 0.125f + Vector3.UnitZ * 0.125f) + Vector3.UnitY * time;
                    Vector3 posO = Vector3.One * (i / 8f);

                    Matrix4 x = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rot.X));
                    Matrix4 y = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rot.Y));
                    Matrix4 z = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rot.Z));

                    Vector3 vec = Vector3.One * 0.5f;

                    Matrix4 s = Matrix4.CreateScale(0.25f);
                    Matrix4 t = Matrix4.CreateTranslation(pos + vec * 0.25f);
                    Matrix4 t2 = Matrix4.CreateTranslation(-vec);
                    Matrix4 t3 = Matrix4.CreateTranslation(posO);

                    Matrix4 mat = t3 * t2 * (z * y * x * s) * t;

                    Shader.UpdateGlobalUniforms();
                    Shader.UpdateModelUniforms(model.RawModel);
                    Shader.UpdateInstanceUniforms(mat, this);
                    model.RawModel.Render(PrimitiveType.Quads);
                }

                GL.BindVertexArray(0);

                GL.DisableVertexAttribArray(0);
                GL.DisableVertexAttribArray(1);
                GL.DisableVertexAttribArray(2);

                Shader.Unbind();
            }
            else
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                GL.Disable(EnableCap.CullFace);
                ModelItem model = JsonModelLoader.GetModelForItem(_stack.Item.UnlocalizedName);

                if (model?.RawModel == null)
                    return;

                float time = OnGround ? (float)((Math.Sin(partialTime / 8) + 1) / 16) : 0;

                Shader.Bind();

                GL.BindVertexArray(model.RawModel.VaoID);

                GL.EnableVertexAttribArray(0);
                GL.EnableVertexAttribArray(1);
                GL.EnableVertexAttribArray(2);

                GL.BindTexture(TextureTarget.Texture2D, JsonModelLoader.TEXTURE_ITEMS);

                int itemsToRender = 1;

                if (_stack.Count > 1)
                    itemsToRender = 2;
                if (_stack.Count >= 32 * 4)
                    itemsToRender = 3;
                if (_stack.Count == 64 * 4)
                    itemsToRender = 4;

                for (int i = 0; i < itemsToRender; i++)
                {
                    Vector3 rot = Vector3.UnitY * partialTime * 4;
                    Vector3 pos = partialPos - (Vector3.UnitX * 0.125f + Vector3.UnitZ * 0.125f) + Vector3.UnitY * time;
                    Vector3 posO = Vector3.One * (i / 8f);

                    Matrix4 x = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rot.X));
                    Matrix4 y = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rot.Y));
                    Matrix4 z = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rot.Z));

                    Vector3 vec = Vector3.One * 0.5f;

                    Matrix4 s = Matrix4.CreateScale(0.35f);
                    Matrix4 t = Matrix4.CreateTranslation(pos + vec * 0.35f);
                    Matrix4 t2 = Matrix4.CreateTranslation(-vec);
                    Matrix4 t3 = Matrix4.CreateTranslation(posO);

                    Matrix4 mat = t3 * t2 * (z * y * x * s) * t;

                    Shader.UpdateGlobalUniforms();
                    Shader.UpdateModelUniforms(model.RawModel);
                    Shader.UpdateInstanceUniforms(mat, this);
                    model.RawModel.Render(PrimitiveType.Quads);
                }

                GL.BindVertexArray(0);

                GL.DisableVertexAttribArray(0);
                GL.DisableVertexAttribArray(1);
                GL.DisableVertexAttribArray(2);

                Shader.Unbind();
                GL.Enable(EnableCap.CullFace);
                GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
            }
        }
    }
}