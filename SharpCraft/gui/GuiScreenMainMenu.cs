using OpenTK;
using SharpCraft.shader;
using SharpCraft.texture;

namespace SharpCraft.gui
{
    internal class GuiScreenMainMenu : GuiScreen
    {
        private GuiTexture background;

        public GuiScreenMainMenu()
        {
            buttons.Add(new GuiButton(0, 0, 200, Vector2.One * 2) { centered = true });
            background = new GuiTexture(TextureManager.loadTexture("gui/bg", false), Vector2.Zero, Vector2.One * 8);
        }

        public override void render(ShaderGui shader, int mouseX, int mouseY)
        {
            drawBackground(shader, background);

            base.render(shader, mouseX, mouseY);
        }

        protected override void buttonClicked(GuiButton btn)
        {
            switch (btn.ID)
            {
                case 0:
                    Game.Instance.CloseGuiScreen();
                    Game.Instance.StartGame();
                    break;
            }
        }

        public override void onClose()
        {
            TextureManager.destroyTexture(background.textureID);
        }
    }
}