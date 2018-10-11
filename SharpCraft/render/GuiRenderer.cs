using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Drawing;
using SharpCraft_Client.gui;
using SharpCraft_Client.model;

namespace SharpCraft_Client.render
{
    internal class GuiRenderer
    {
        public static ModelRaw GuiQuad;

        private readonly GuiCrosshair crosshairGui;
        private readonly GuiHud hudGui;

        static GuiRenderer()
        {
            float[] vertexes =
            {
                -1, 1,
                -1, -1,
                1, 1,

                1, 1,
                -1, -1,
                1, -1
            };

            GuiQuad = ModelManager.LoadModel2ToVao(vertexes);
        }

        public GuiRenderer()
        {
            crosshairGui = new GuiCrosshair();
            hudGui = new GuiHud();
        }

        public void Render(Gui gui)
        {
            if (gui == null)
                return;

            GL.Disable(EnableCap.DepthTest);

            GL.BindVertexArray(GuiQuad.VaoID);

            MouseState state = Mouse.GetCursorState();
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