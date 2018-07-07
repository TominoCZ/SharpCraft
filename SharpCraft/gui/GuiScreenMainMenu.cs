using OpenTK.Input;
using SharpCraft.texture;

namespace SharpCraft.gui
{
    internal class GuiScreenMainMenu : GuiScreen
    {
        private static GuiTexture _titleTexture;

        public GuiScreenMainMenu()
        {
            Buttons.Add(new GuiButton(0, 0, -45, 2, @"\{66FF00}SINGLEPLAYER") { CenteredX = true, CenteredY = true });
            Buttons.Add(new GuiButton(1, 0, 0, 2, @"\{DD6600}MULTIPLAYER") { CenteredX = true, CenteredY = true, Enabled = false });
            Buttons.Add(new GuiButton(2, 0, 45, 2, @"\{FF0000}EXIT") { CenteredX = true, CenteredY = true });

            var titleTextgure = TextureManager.LoadTexture("gui/title");

            _titleTexture = new GuiTexture(titleTextgure, 0, 0, titleTextgure.TextureSize.Width, titleTextgure.TextureSize.Height, 0.7f);
        }

        public override void Render(int mouseX, int mouseY)
        {
            DrawDefaultBackground();

            for (int i = 0; i < 16; i++)
            {
                RenderTexture(_titleTexture,
                    SharpCraft.Instance.Width / 2 - (int)(_titleTexture.Size.X * _titleTexture.Scale / 2) - i, 25 - i);
            }

            base.Render(mouseX, mouseY);
        }

        protected override void ButtonClicked(GuiButton btn, MouseButton button)
        {
            switch (btn.ID)
            {
                case 0:
                    SharpCraft.Instance.CloseGuiScreen();
                    SharpCraft.Instance.StartGame();
                    break;

                case 2:
                    SharpCraft.Instance.Close();
                    break;
            }
        }
    }
}