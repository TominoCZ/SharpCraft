using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.gui;
using SharpCraft.model;
using SharpCraft.shader;
using SharpCraft.texture;
using System.Collections.Generic;

namespace SharpCraft.render
{
    internal class GuiRenderer
    {
        public static ModelRaw GUIquad;

        private ShaderGui shader;

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
            shader = new ShaderGui("gui");

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

            shader.bind();

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Disable(EnableCap.DepthTest);

            GL.BindVertexArray(GUIquad.vaoID);
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);

            var state = OpenTK.Input.Mouse.GetCursorState();
            var mouse = SharpCraft.Instance.PointToClient(new Point(state.X, state.Y));

            gui.render(shader, mouse.X, mouse.Y);

            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.DisableVertexAttribArray(2);
            GL.BindVertexArray(0);

            GL.Enable(EnableCap.DepthTest);

            shader.unbind();
        }

        public void renderCrosshair()
        {
            render(crosshairGui);
        }

        public void renderHUD()
        {
            render(hudGui);
        }

        public void cleanUp()
        {
            shader.destroy();
        }
    }
}