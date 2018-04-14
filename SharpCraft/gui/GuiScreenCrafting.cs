namespace SharpCraft.gui
{
    internal class GuiScreenCrafting : GuiScreen
    {
        public GuiScreenCrafting()
        {
            DoesGuiPauseGame = false;
        }

        public override void Render(int mouseX, int mouseY)
        {
            DrawBackground();
        }
    }
}