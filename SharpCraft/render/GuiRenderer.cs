using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.gui;
using SharpCraft.model;
using System.Collections.Generic;

namespace SharpCraft.render
{
    internal class GuiRenderer
    {
        public static ModelRaw GuiQuad;

        private readonly GuiCrosshair crosshairGui;
        private readonly GuiHUD hudGui;

        static GuiRenderer()
        {
            var rawQuad = new RawQuad(new float[] {
                -1,  1,
                -1, -1,
                1, -1,
                1, 1 }, 2);

            GuiQuad = ModelManager.loadModelToVAO(new List<RawQuad> { rawQuad }, 2);
        }

        public GuiRenderer()
        {
            crosshairGui = new GuiCrosshair();
            hudGui = new GuiHUD();
        }

        public void Render(Gui gui)
        {
            if (gui == null)
                return;

            GL.Disable(EnableCap.DepthTest);

            GL.BindVertexArray(GuiQuad.vaoID);

            var state = OpenTK.Input.Mouse.GetCursorState();
            var mouse = SharpCraft.Instance.PointToClient(new Point(state.X, state.Y));

            gui.Render(mouse.X, mouse.Y);

            GL.BindVertexArray(0);

            GL.Enable(EnableCap.DepthTest);
        }

        public void RenderCrosshair()
        {
            Render(crosshairGui);
        }

        public void RenderHUD()
        {
            Render(hudGui);
        }
    }
}