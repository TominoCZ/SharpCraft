using OpenTK;

namespace SharpCraft.gui
{
    internal class GuiScreenIngameMenu : GuiScreen
    {
        public override void Render(int mouseX, int mouseY)
        {
            DrawBackroundStretch();

            Size size = SharpCraft.Instance.ClientSize;

            RenderText(@"\{FFDD000}PAUSED", size.Width / 2f, size.Height / 2f, 5, true, true);

            base.Render(mouseX, mouseY);
        }
    }
}