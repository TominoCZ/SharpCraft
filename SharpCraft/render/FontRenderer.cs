using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.gui;
using SharpCraft.render.shader.shaders;
using SharpCraft.texture;
using SharpCraft.util;

namespace SharpCraft.render
{
    class FontRenderer
    {
        public static ShaderText Shader { get; private set; }

        public FontRenderer()
        {
            Shader = new ShaderText();
        }

        public void RenderText(string text, float x, float y, float scale, Vector3 color, bool centered = false, bool dropShadow = false, int spacing = 4) //#TODO
        {
            scale *= 0.5f;

            var tex = TextureManager.TEXTURE_TEXT;

            Shader.Bind();

            GL.BindVertexArray(GuiRenderer.GuiQuad.vaoID);
            GL.EnableVertexAttribArray(0);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, tex.ID);

            Shader.UpdateGlobalUniforms();
            Shader.UpdateModelUniforms();
            //{ (.*?)\}

            var totalSize = Vector2.Zero;
            var matches = Regex.Matches(text, @"\\{(.*?)\}");
            List<Tuple<FontMapCharacter, Vector3>> present = new List<Tuple<FontMapCharacter, Vector3>>();

            var currentColor = color;

            for (var index = 0; index < text.Length; index++)
            {
                char c = text[index];

                var node = FontManager.GetCharacter(c);
                if (node == null)
                    continue;

                var first = matches.FirstOrDefault(match => match.Index == index);
                if (first != null && first.Length > 0)
                {
                    var clr = ColorTranslator.FromHtml($"#{first.Value.Replace(@"\{", "").Replace("}", "")}");
                    currentColor.X = clr.R / 255f;
                    currentColor.Y = clr.G / 255f;
                    currentColor.Z = clr.B / 255f;

                    index += first.Length - 1;
                    continue;
                }

                totalSize.X += node.Character.W + node.Character.OffsetX;
                totalSize.Y += node.Character.H + node.Character.OffsetY;

                present.Add(new Tuple<FontMapCharacter, Vector3>(node, currentColor));
            }

            totalSize.X += (present.Count - 1) * spacing;

            totalSize *= scale;

            totalSize.Y /= -present.Count;

            var positionX = 0f;

            foreach (var tuple in present)
            {
                float width = tuple.Item1.Character.W * scale;
                float height = tuple.Item1.Character.H * scale;

                var ratio = new Vector2(width / SharpCraft.Instance.ClientSize.Width, height / SharpCraft.Instance.ClientSize.Height);
                var unit = new Vector2(1f / SharpCraft.Instance.ClientSize.Width, 1f / SharpCraft.Instance.ClientSize.Height);
                var pos = new Vector2(x + positionX + width / 2, -(y + height / 2));

                pos.Y -= tuple.Item1.Character.OffsetY * scale;

                if (centered)
                {
                    pos.X -= totalSize.X / 2;
                    pos.Y -= totalSize.Y / 2;
                }

                if (dropShadow)
                {
                    var mat1 = MatrixHelper.CreateTransformationMatrix(
                        (pos + (Vector2.UnitX - Vector2.UnitY) * 4f * scale) * unit * 2 +
                        Vector2.UnitY - Vector2.UnitX,
                        ratio);

                    Shader.SetColor(Vector3.One * 0.1f);

                    Shader.UpdateInstanceUniforms(mat1, tuple.Item1.TextureUv);

                    GL.DrawArrays(PrimitiveType.Quads, 0, 4);
                }

                var mat2 = MatrixHelper.CreateTransformationMatrix(
                    pos * unit * 2 +
                    Vector2.UnitY - Vector2.UnitX,
                    ratio);

                Shader.SetColor(tuple.Item2);

                Shader.UpdateInstanceUniforms(mat2, tuple.Item1.TextureUv);

                GL.DrawArrays(PrimitiveType.Quads, 0, 4);

                positionX += width + (tuple.Item1.Character.OffsetX + spacing) * scale;
            }

            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.DisableVertexAttribArray(0);
            GL.BindVertexArray(0);

            Shader.Unbind();
        }
    }
}
