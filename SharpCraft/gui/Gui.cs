﻿using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.block;
using SharpCraft.model;
using SharpCraft.render;
using SharpCraft.render.shader;
using SharpCraft.render.shader.shaders;
using SharpCraft.texture;
using SharpCraft.util;

namespace SharpCraft.gui
{
	internal class Gui
	{
		private static ModelGuiItem _item;

		static Gui()
		{
			_item = new ModelGuiItem(new Shader<object>("gui_item"));
		}

		protected virtual void renderTexture(Shader<Gui> shader, GuiTexture tex)
		{
			var ratio = new Vector2((float) tex.textureSize.Width / SharpCraft.Instance.ClientSize.Width, (float) tex.textureSize.Height / SharpCraft.Instance.ClientSize.Height);

			var mat = MatrixHelper.createTransformationMatrix(tex.pos * 2, tex.scale * ratio);
			shader.UpdateGlobalUniforms();
			shader.UpdateModelUniforms(null);
			shader.UpdateInstanceUniforms(mat, this);

			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, tex.textureID);
			GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
		}

		protected virtual void renderTexture(Shader<Gui> shader, GuiTexture tex, int x, int y)
		{
			renderTexture(shader, tex, tex.scale, x, y);
		}

		protected virtual void renderTexture(Shader<Gui> shader, GuiTexture tex, Vector2 scale, int x, int y)
		{
			shader.bind();
			GL.BindVertexArray(GuiRenderer.GUIquad.vaoID);

			var unit = new Vector2(1f / SharpCraft.Instance.ClientSize.Width, 1f / SharpCraft.Instance.ClientSize.Height);

			float width = tex.textureSize.Width;
			float height = tex.textureSize.Height;

			float scaledWidth = width * scale.X;
			float scaledHeight = height * scale.Y;

			float posX = x + scaledWidth / 2;
			float posY = -y - scaledHeight / 2;

			var pos = new Vector2(posX, posY) * unit;

			var mat = MatrixHelper.createTransformationMatrix(pos * 2 - Vector2.UnitX + Vector2.UnitY, scale * new Vector2(width, height) * unit);
			shader.UpdateGlobalUniforms();
			shader.UpdateModelUniforms(null);
			shader.UpdateInstanceUniforms(mat,this);

			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, tex.textureID);

			GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

			GL.BindVertexArray(0);

			shader.unbind();
		}

		protected virtual void renderBlock(EnumBlock block, float scale, int x, int y)
		{
			var UVs = TextureManager.getUVsFromBlock(block);
			ModelManager.overrideModelUVsInVAO(_item.rawModel.bufferIDs[1], UVs.getUVForSide(FaceSides.South).ToArray());

			var unit = new Vector2(1f / SharpCraft.Instance.ClientSize.Width, 1f / SharpCraft.Instance.ClientSize.Height);

			float width = 16;
			float height = 16;

			float scaledWidth = 16 * scale;
			float scaledHeight = 16 * scale;

			float posX = x + scaledWidth / 2;
			float posY = -y - scaledHeight / 2;

			var pos = new Vector2(posX, posY) * unit;

			var mat = MatrixHelper.createTransformationMatrix(pos * 2 - Vector2.UnitX + Vector2.UnitY, scale * new Vector2(width, height) * unit);

			_item.bind();
			_item.shader.UpdateGlobalUniforms();
			_item.shader.UpdateModelUniforms(null);
			_item.shader.UpdateInstanceUniforms(mat,null);

			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, TextureManager.blockTextureAtlasID);
			GL.DrawArrays(PrimitiveType.Quads, 0, 4);

			_item.unbind();
		}

		public virtual void Render(Shader<Gui> shader, int mouseX, int mouseY)
		{

		}
	}
}