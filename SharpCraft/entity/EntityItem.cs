using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.block;
using SharpCraft.model;
using SharpCraft.shader;
using SharpCraft.texture;
using SharpCraft.util;
using SharpCraft.world;
using System;
using System.Linq;

namespace SharpCraft.entity
{
    internal class EntityItem : Entity
    {
        private static ShaderProgram shader;

        private ItemStack stack;

        private int entityAge;
        private int entityAgeLast;

        public bool canBePickedUp;

        static EntityItem()
        {
            shader = new ShaderEntityItem();
        }

        public EntityItem(World world, Vector3 pos, Vector3 motion, ItemStack stack) : base(world, pos, motion)
        {
            this.stack = stack;

            collisionBoundingBox = AxisAlignedBB.BLOCK_FULL.offset(Vector3.One * -0.5f).shrink(Vector3.One * 0.6f);
            boundingBox = collisionBoundingBox.offset(pos);

            isAlive = stack != null && !stack.IsEmpty;
        }

        public override void Update()
        {
            base.Update();

            entityAgeLast = entityAge;

            if (onGround && ++entityAge >= (20 * 50 * 60) + 40) //stay on ground for a minute, 20 ticks as a pick up delay
                SetDead();

            if (entityAge >= 40)
            {
                EntityPlayerSP closestPlayer = null;
                float smallestDistance = float.MaxValue;

                //TODO change this for multiplayer
                world.Entities.OfType<EntityPlayerSP>().AsParallel().ForAll(player =>
                {
                    var dist = MathUtil.Distance(player.pos, pos);

                    if (dist < smallestDistance && dist <= 2 && !player.HasFullInventory)
                    {
                        smallestDistance = dist;
                        closestPlayer = player;
                    }
                });

                if (closestPlayer != null)
                {
                    closestPlayer.OnPickup(stack);
                    SetDead();
                }
            }
        }

        public override void Render(Matrix4 viewMatrix, float particalTicks)
        {
            var partialPos = lastPos + (pos - lastPos) * particalTicks;
            var partialTime = entityAgeLast + (entityAge - entityAgeLast) * particalTicks;

            if (stack?.Item?.item is EnumBlock block)
            {
                var model = ModelRegistry.getModelForBlock(block, stack.Meta);

                if (model.rawModel == null)
                    return;

                var time = (float)((Math.Sin(partialTime / 12) + 1) / 16);

                shader.bind();

                GL.BindVertexArray(model.rawModel.vaoID);

                GL.EnableVertexAttribArray(0);
                GL.EnableVertexAttribArray(1);
                GL.EnableVertexAttribArray(2);

                shader.loadVec3(Vector3.One, "lightColor");
                shader.loadViewMatrix(viewMatrix);
                shader.loadTransformationMatrix(MatrixHelper.createTransformationMatrix(partialPos - Vector3.One * 0.2f + Vector3.UnitY * 0.4f + Vector3.UnitY * time, Vector3.UnitY * partialTime * 2, 0.4f));

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, TextureManager.blockTextureAtlasID);
                model.rawModel.Render(shader.renderType);

                GL.BindVertexArray(0);

                GL.DisableVertexAttribArray(0);
                GL.DisableVertexAttribArray(1);
                GL.DisableVertexAttribArray(2);

                shader.unbind();
            }
        }
    }
}