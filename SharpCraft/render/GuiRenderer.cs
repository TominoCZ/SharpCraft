using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.gui;
using SharpCraft.model;
using SharpCraft.texture;
using System.Collections.Generic;
using SharpCraft.render.shader;
using SharpCraft.render.shader.shaders;

namespace SharpCraft.render
{
    internal class GuiRenderer
    {
        public static ModelRaw GUIquad;

        private Shader<Gui> _shader;

        private GuiCrosshair crosshairGui;
        private GuiHUD hudGui;

        static GuiRenderer()
        {
            var rawQuad = new RawQuad(new float[] {
                -1,  1,
                -1, -1,
                1, 1,
                1, -1 }, 2);

            GUIquad = ModelManager.loadModelToVAO(new List<RawQuad> { rawQuad }, 2);
        }

        public GuiRenderer()
        {
            _shader = new Shader<Gui>("gui");

            var texture = TextureManager.loadTexture("gui/cross", true);

            if (texture != null)
            {
                var tex = new GuiTexture(texture.textureID, texture.textureSize, Vector2.Zero, Vector2.One * 1.4f);
                crosshairGui = new GuiCrosshair(tex);
            }

            hudGui = new GuiHUD();
        }

        public void render(Gui gui)
        {
            if (gui == null)
                return;

            _shader.Bind();

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Disable(EnableCap.DepthTest);

            GL.BindVertexArray(GUIquad.vaoID);
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);

            var state = OpenTK.Input.Mouse.GetCursorState();
            var mouse = SharpCraft.Instance.PointToClient(new Point(state.X, state.Y));
            
            gui.Render(_shader, mouse.X, mouse.Y);

            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.DisableVertexAttribArray(2);
            GL.BindVertexArray(0);

            GL.Enable(EnableCap.DepthTest);

            _shader.Unbind();
        }

        public void renderCrosshair()
        {
            render(crosshairGui);
        }

        public void renderHUD()
        {
            render(hudGui);
        }
    }
}