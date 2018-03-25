using OpenTK;

namespace SharpCraft
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
                    Game.INSTANCE.closeGuiScreen();
                    Game.INSTANCE.startGame();
                    break;
            }
        }

        public override void onClose()
        {
            TextureManager.destroyTexture(background.textureID);
        }
    }
}