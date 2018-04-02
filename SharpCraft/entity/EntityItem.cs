using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.block;
using SharpCraft.model;
using SharpCraft.texture;
using SharpCraft.util;
using SharpCraft.world;
using System;
using System.Linq;
using SharpCraft.item;
using SharpCraft.render.shader;

namespace SharpCraft.entity
{
    public class EntityItem : Entity
    {
        private static Shader<EntityItem> _shader;

        private ItemStack stack;

        private int entityAge;
        private int entityAgeLast;

        static EntityItem()
        {
            _shader = new Shader<EntityItem>("entity_item");
        }

        public EntityItem(World world, Vector3 pos, Vector3 motion, ItemStack stack) : base(world, pos, motion)
        {
            this.stack = stack;

            collisionBoundingBox = new AxisAlignedBB(0.25f);
            boundingBox = collisionBoundingBox.offset(pos - Vector3.One * collisionBoundingBox.size / 2);

            gravity = 1.45f;
            isAlive = stack != null && !stack.IsEmpty;
        }

        public override void Update()
        {
            base.Update();

            entityAgeLast = entityAge;

            if (onGround && ++entityAge >= 20 * 50 * 60 + 10) //stay on ground for a minute, 20 ticks as a pick up delay
            {
                SetDead();
                return;
            }

            if (entityAge >= 10)
            {
                //TODO change this for multiplayer
                var players = world.Entities.OfType<EntityPlayerSP>()
                                   .AsParallel()
                                   .OrderBy(entity => MathUtil.Distance(entity.pos, pos))
                                   .Where(e => MathUtil.Distance(e.pos, pos) <= 2);

                foreach (var player in players)
                {
                    if (player.OnPickup(stack))
                        SetDead();
                }

                var otherDrops = world.Entities.OfType<EntityItem>()
                    .AsParallel()
                    .OrderBy(entity => MathUtil.Distance(entity.pos, pos))
                    .Where(
                        e => MathUtil.Distance(e.pos, pos) <= 1 &&
                        e != this &&
                        e.isAlive &&
                        e.stack.Item.item == stack.Item.item &&
                        e.stack.Meta == stack.Meta &&
                        e.stack.Count + stack.Count <= 64).ToList();

                if (otherDrops.Count > 0)
                {
                    var closest = otherDrops.First();

                    if (closest.isAlive)
                    {
                        closest.stack.Count += stack.Count;

                        SetDead();

                        closest.TeleportTo((closest.pos + pos) / 2);
                    }
                }
            }
        }

        public override void Render(float particalTicks)
        {
            var partialPos = lastPos + (pos - lastPos) * particalTicks;
            var partialTime = entityAgeLast + (entityAge - entityAgeLast) * particalTicks;

            if (stack?.Item?.item is EnumBlock block)
            {
                var model = ModelRegistry.getModelForBlock(block, stack.Meta);

                if (model.RawModel == null)
                    return;

                var time = onGround ? (float)((Math.Sin(partialTime / 8) + 1) / 16) : 0;

                _shader.Bind();

                GL.BindVertexArray(model.RawModel.vaoID);

                GL.EnableVertexAttribArray(0);
                GL.EnableVertexAttribArray(1);
                GL.EnableVertexAttribArray(2);

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, TextureManager.blockTextureAtlasID);

                for (int i = 0; i == 0 || i < stack.Count / 16; i++)
                {
                    var rot = Vector3.UnitY * partialTime * 3;
                    var pos = partialPos - (Vector3.UnitX * 0.125f + Vector3.UnitZ * 0.125f) + Vector3.UnitY * time;
                    var posO = Vector3.One * (i / 16f);

                    var x = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rot.X));
                    var y = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rot.Y));
                    var z = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rot.Z));

                    var vec = Vector3.One * 0.5f;

                    var s = Matrix4.CreateScale(0.25f);
                    var t = Matrix4.CreateTranslation(pos + vec * 0.25f);
                    var t2 = Matrix4.CreateTranslation(-vec);
                    var t3 = Matrix4.CreateTranslation(posO);

                    var mat = t3 * t2 * (x * z * y * s) * t;

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