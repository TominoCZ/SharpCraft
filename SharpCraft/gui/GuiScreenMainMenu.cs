using OpenTK;
using SharpCraft.render.shader;
using SharpCraft.render.shader.shaders;
using SharpCraft.texture;

namespace SharpCraft.gui
{
    internal class GuiScreenMainMenu : GuiScreen
    {
        public GuiScreenMainMenu()
        {
            buttons.Add(new GuiButton(0, 0, 200, 2) { centered = true });
        }

        public override void Render(int mouseX, int mouseY)
        {
            DrawDefaultBackground();

            base.Render(mouseX, mouseY);
        }

        protected override void ButtonClicked(GuiButton btn)
        {
            switch (btn.ID)
            {
                case 0:
                    SharpCraft.Instance.CloseGuiScreen();
                    SharpCraft.Instance.StartGame();
                    break;
            }
        }
    }
}