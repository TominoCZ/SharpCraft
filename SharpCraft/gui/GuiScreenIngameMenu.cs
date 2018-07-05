using System.Drawing;
using OpenTK;
using OpenTK.Input;

namespace SharpCraft.gui
{
    internal class GuiScreenIngameMenu : GuiScreen
    {
        public GuiScreenIngameMenu()
        {
            buttons.Add(new GuiButton(0, 0, -45, 2, @"\{66FF00}RESUME") { CenteredX = true, CenteredY = true });
            buttons.Add(new GuiButton(1, 0, 0, 2, @"\{66FF00}RETURN  TO  MENU") { CenteredX = true, CenteredY = true });
            buttons.Add(new GuiButton(2, 0, 45, 2, @"\{FF0000}EXIT") { CenteredX = true, CenteredY = true });
        }

        public override void Render(int mouseX, int mouseY)
        {
            DrawBackroundStretch();

            Size size = SharpCraft.Instance.ClientSize;

            RenderText(@"\{FFDD000}PAUSED", size.Width / 2f, size.Height / 2f - 150, 5, true, true);

            base.Render(mouseX, mouseY);
        }

        protected override void ButtonClicked(GuiButton btn, MouseButton button)
        {
            switch (btn.ID)
            {
                case 0:
                    SharpCraft.Instance.CloseGuiScreen();
                    break;

                case 1:
                    SharpCraft.Instance.Disconnect();

                    SharpCraft.Instance.OpenGuiScreen(new GuiScreenMainMenu());
                    break;

                case 2:
                    SharpCraft.Instance.Close();
                    break;
            }
        }
    }
}