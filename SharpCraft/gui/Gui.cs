using System.Collections.Generic;
using OpenTK;
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
        private static readonly ModelGuiItem _item;

        public static ShaderGui Shader { get; }

        static Gui()
        {
            _item = new ModelGuiItem(new Shader<object>("gui_item"));

            Shader = new ShaderGui();
        }

        protected void RenderText(string text, float x, float y, float scale, bool centered = false, bool dropShadow = false)
        {
            RenderText(text, x, y, scale, Vector3.One, centered, dropShadow);
        }

        protected void RenderText(string text, float x, float y, float scale, Vector3 color, bool centered = false, bool dropShadow = false)
        {
            SharpCraft.Instance.FontRenderer.RenderText(text, x, y, scale, color, centered, dropShadow);
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

            GL.BindVertexArray(GuiRenderer.GuiQuad.VaoID);

            Shader.UpdateGlobalUniforms();
            Shader.UpdateModelUniforms();
            Shader.UpdateInstanceUniforms(mat, tex);

            GL.EnableVertexAttribArray(0);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, tex.ID);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            GL.DisableVertexAttribArray(0);

            Shader.Unbind();
        }

        protected virtual void RenderTextureStretchToScreen(GuiTexture tex)
        {
            if (tex == null)
                return;

            Shader.Bind();

            GL.BindVertexArray(GuiRenderer.GuiQuad.VaoID);

            Shader.UpdateGlobalUniforms();
            Shader.UpdateModelUniforms();
            Shader.UpdateInstanceUniforms(Matrix4.Identity, tex);

            GL.EnableVertexAttribArray(0);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, tex.ID);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            GL.DisableVertexAttribArray(0);

            Shader.Unbind();
        }

        protected virtual void RenderBlock(Block block, float x, float y, float scale)
        {
            var model = JsonModelLoader.GetModelForBlock(block.UnlocalizedName);

            if (model == null)
                return;

            ModelBlockRaw mbr = (ModelBlockRaw)model.RawModel;
            List<float> uvs = new List<float>(8);
            mbr.AppendUvsForSide(FaceSides.South, uvs);

            Vector2 UVmin = new Vector2(uvs[0], uvs[1]);
            Vector2 UVmax = new Vector2(uvs[4], uvs[5]);

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
            Shader.UpdateGlobalUniforms();
            Shader.UpdateModelUniforms();
            Shader.UpdateInstanceUniforms(mat, UVmin, UVmax);

            GL.BindTexture(TextureTarget.Texture2D, JsonModelLoader.TEXTURE_BLOCKS);
            _item.RawModel.Render(PrimitiveType.Quads);

            _item.Unbind();
        }

        public virtual void Render(int mouseX, int mouseY)
        {
        }
    }
}