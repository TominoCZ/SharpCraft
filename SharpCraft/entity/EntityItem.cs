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
using SharpCraft.render.shader.shaders;

namespace SharpCraft.entity
{
	internal class EntityItem : Entity
	{
		private static Shader<EntityItem> shader;

		private ItemStack stack;

		private int entityAge;
		private int entityAgeLast;

		static EntityItem()
		{
			shader = new Shader<EntityItem>("entity_item");
		}

		public EntityItem(World world, Vector3 pos, Vector3 motion, ItemStack stack) : base(world, pos, motion)
		{
			this.stack = stack;

			collisionBoundingBox = AxisAlignedBb.BLOCK_FULL.offset(Vector3.One * -0.5f).Grow(Vector3.One * -0.6f);
			boundingBox = collisionBoundingBox.offset(pos + Vector3.UnitY * collisionBoundingBox.size.Y / 2);

			gravity = 1.45f;
			isAlive = stack != null && !stack.IsEmpty;
		}

		public override void Update()
		{
			base.Update();

			entityAgeLast = entityAge;

			if (onGround && ++entityAge >= 20 * 50 * 60 + 10) //stay on ground for a minute, 20 ticks as a pick up delay
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

				var time = onGround ? (float) ((Math.Sin(partialTime / 8) + 1) / 16) : 0;

				shader.bind();

				GL.BindVertexArray(model.rawModel.vaoID);

				GL.EnableVertexAttribArray(0);
				GL.EnableVertexAttribArray(1);
				GL.EnableVertexAttribArray(2);

				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, TextureManager.blockTextureAtlasID);
				shader.UpdateGlobalUniforms();
				shader.UpdateModelUniforms(model.rawModel);
				shader.UpdateInstanceUniforms(MatrixHelper.createTransformationMatrix(partialPos - (Vector3.UnitX * 0.125f + Vector3.UnitZ * 0.125f) + Vector3.UnitY * time, Vector3.UnitY * partialTime * 3, 0.25f), this);
				model.rawModel.Render(PrimitiveType.Quads);

				GL.BindVertexArray(0);

				GL.DisableVertexAttribArray(0);
				GL.DisableVertexAttribArray(1);
				GL.DisableVertexAttribArray(2);

				shader.unbind();
			}
		}
	}
}