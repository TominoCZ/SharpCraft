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
using SharpCraft.item;

namespace SharpCraft.entity
{
    internal class EntityItem : Entity
    {
        private static ShaderProgram shader;

        private ItemStack stack;

        private int entityAge;
        private int entityAgeLast;

        static EntityItem()
        {
            shader = new ShaderEntityItem();
        }

        public EntityItem(World world, Vector3 pos, Vector3 motion, ItemStack stack) : base(world, pos, motion)
        {
            this.stack = stack;

            collisionBoundingBox = AxisAlignedBB.BLOCK_FULL.offset(Vector3.One * -0.5f).shrink(Vector3.One * 0.6f);
            boundingBox = collisionBoundingBox.offset(pos + Vector3.UnitY * collisionBoundingBox.size.Y / 2);

            gravity = 1.45f;
            isAlive = stack != null && !stack.IsEmpty;
        }

        public override void Update()
        {
            base.Update();

            entityAgeLast = entityAge;

            if (onGround && ++entityAge >= (20 * 50 * 60) + 10) //stay on ground for a minute, 20 ticks as a pick up delay
                SetDead();

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

                var time = onGround ? (float)((Math.Sin(partialTime / 8) + 1) / 16) : 0;

                shader.bind();

                GL.BindVertexArray(model.rawModel.vaoID);

                GL.EnableVertexAttribArray(0);
                GL.EnableVertexAttribArray(1);
                GL.EnableVertexAttribArray(2);

                shader.loadVec3(Vector3.One, "lightColor");
                shader.loadViewMatrix(viewMatrix);
                shader.loadTransformationMatrix(MatrixHelper.createTransformationMatrix(partialPos - (Vector3.UnitX * 0.125f + Vector3.UnitZ * 0.125f) + Vector3.UnitY * time, Vector3.UnitY * partialTime * 3, 0.25f));

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