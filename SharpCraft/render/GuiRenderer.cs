using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
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
            RawQuad rawQuad = new RawQuad(new float[] {
                -1,  1,
                -1, -1,
                1, -1,
                1, 1 }, 2);

            GuiQuad = ModelManager.LoadModelToVAO(new List<RawQuad> { rawQuad }, 2);
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

            GL.BindVertexArray(GuiQuad.VaoID);

            MouseState state = OpenTK.Input.Mouse.GetCursorState();
            Point mouse = SharpCraft.Instance.PointToClient(new Point(state.X, state.Y));

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