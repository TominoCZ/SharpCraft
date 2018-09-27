using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.block;
using SharpCraft.item;
using SharpCraft.json;
using SharpCraft.model;
using SharpCraft.render;
using SharpCraft.render.shader;
using SharpCraft.util;
using System;

#pragma warning disable 618

namespace SharpCraft.gui
{
    internal class Gui
    {
        private static readonly ModelGuiItem _item;

        public static Shader Shader { get; }

        static Gui()
        {
            _item = new ModelGuiItem(new Shader("gui_item"));

            Shader = new Shader("gui", "UVmin", "UVmax");
        }

        protected void RenderText(string text, float x, float y, float scale, bool centered = false, bool dropShadow = false)
        {
            RenderText(text, x, y, scale, Vector3.One, centered, dropShadow);
        }

        protected void RenderText(string text, float x, float y, float scale, Vector3 color, bool centered = false, bool dropShadow = false)
        {
            RenderText(text, x, y, scale, color, 1, centered, dropShadow);
        }

        protected void RenderText(string text, float x, float y, float scale, Vector3 color, float brightness, bool centered = false, bool dropShadow = false)
        {
            SharpCraft.Instance.FontRenderer.RenderText(text, x, y, scale, color, brightness, centered, dropShadow);
        }

        protected virtual void RenderTexture(Texture tex, float x, float y, int textureX, int textureY, int sizeX, int sizeY, float scale = 1, bool centered = false)
        {
            if (tex == null)
                return;

            RenderTexture(new GuiTexture(tex, new Vector2(textureX, textureY), new Vector2(sizeX, sizeY), scale), x, y, centered);
        }

        protected virtual void RenderTexture(GuiTexture tex, float x, float y, bool cenetered = false)
        {
            if (tex == null)
                return;

            float width = tex.Size.X * tex.Scale;
            float height = tex.Size.Y * tex.Scale;

            Vector2 ratio = new Vector2(width / SharpCraft.Instance.ClientSize.Width, height / SharpCraft.Instance.ClientSize.Height);

            Vector2 unit = new Vector2(1f / SharpCraft.Instance.ClientSize.Width, 1f / SharpCraft.Instance.ClientSize.Height);

            Vector2 pos = new Vector2(x, -y);

            if (!cenetered)
            {
                pos.X += width / 2;
                pos.Y -= height / 2;
            }

            Matrix4 mat = MatrixHelper.CreateTransformationMatrix(pos.Ceiling() * unit * 2 + Vector2.UnitY - Vector2.UnitX, ratio);

            Shader.Bind();
            Shader.SetMatrix4("transformationMatrix", mat);
            Shader.SetVector2("UVmin", tex.UVmin);
            Shader.SetVector2("UVmax", tex.UVmax);

            GL.BindVertexArray(GuiRenderer.GuiQuad.VaoID);

            GL.EnableVertexAttribArray(0);

            GL.BindTexture(TextureTarget.Texture2D, tex.ID);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            GL.DisableVertexAttribArray(0);

            Shader.Unbind();
        }

        protected virtual void RenderTextureStretchToScreen(GuiTexture tex)
        {
            if (tex == null)
                return;

            Shader.Bind();

            GL.BindVertexArray(GuiRenderer.GuiQuad.VaoID);
            Shader.SetMatrix4("transformationMatrix", Matrix4.Identity);
            Shader.SetVector2("UVmin", tex.UVmin);
            Shader.SetVector2("UVmax", tex.UVmax);

            GL.EnableVertexAttribArray(0);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, tex.ID);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            GL.DisableVertexAttribArray(0);

            Shader.Unbind();
        }

        [Obsolete("Use RenderStack() instead")]
        protected virtual void RenderBlock(Block block, short meta, float x, float y, float scale)
        {
            var model = block.GetState(meta).Model;

            if (model == null)
                return;

            Vector2 unit = new Vector2(1f / SharpCraft.Instance.ClientSize.Width, 1f / SharpCraft.Instance.ClientSize.Height);

            float width = 16;
            float height = 16;

            float scaledWidth = 16 * scale;
            float scaledHeight = 16 * scale;

            float posX = x + scaledWidth / 2;
            float posY = -y - scaledHeight / 2;

            Vector2 pos = new Vector2(posX, posY).Ceiling() * unit;

            Matrix4 mat = MatrixHelper.CreateTransformationMatrix(pos * 2 - Vector2.UnitX + Vector2.UnitY, scale * new Vector2(width, height) * unit);

            _item.Bind();

            Shader.Bind();
            Shader.SetMatrix4("transformationMatrix", mat);
            Shader.SetVector2("UVmin", model.SlotTexture.UVMin);
            Shader.SetVector2("UVmax", model.SlotTexture.UVMax);

            GL.BindTexture(TextureTarget.Texture2D, JsonModelLoader.TextureBlocks);
            _item.RawModel.Render();

            _item.Unbind();
        }

        [Obsolete("Use RenderStack() instead")]
        protected virtual void RenderItem(Item item, float x, float y, float scale)
        {
            var model = JsonModelLoader.GetModelForItem(item.UnlocalizedName);

            if (model?.SlotTexture == null)
                return;

            Vector2 unit = new Vector2(1f / SharpCraft.Instance.ClientSize.Width, 1f / SharpCraft.Instance.ClientSize.Height);

            float width = 16;
            float height = 16;

            float scaledWidth = 16 * scale;
            float scaledHeight = 16 * scale;

            float posX = x + scaledWidth / 2;
            float posY = -y - scaledHeight / 2;

            Vector2 pos = new Vector2(posX, posY).Ceiling() * unit;

            Matrix4 mat = MatrixHelper.CreateTransformationMatrix(pos * 2 - Vector2.UnitX + Vector2.UnitY, scale * new Vector2(width, height) * unit);

            _item.Bind();

            Shader.Bind();
            Shader.SetMatrix4("transformationMatrix", mat);
            Shader.SetVector2("UVmin", model.SlotTexture.UVMin);
            Shader.SetVector2("UVmax", model.SlotTexture.UVMax);

            GL.BindTexture(TextureTarget.Texture2D, JsonModelLoader.TextureItems);
            _item.RawModel.Render();

            _item.Unbind();
        }

        protected void RenderStack(ItemStack stack, float x, float y, float scale)
        {
            if (stack.Item is ItemBlock itemBlock)
                RenderBlock(itemBlock.Block, stack.Meta, x, y, scale);
            else
                RenderItem(stack.Item, x, y, scale);
        }

        public virtual void Render(int mouseX, int mouseY)
        {
        }
    }
}