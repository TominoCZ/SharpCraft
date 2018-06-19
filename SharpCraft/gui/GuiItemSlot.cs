using OpenTK;
using SharpCraft.block;
using SharpCraft.item;
using SharpCraft.texture;

namespace SharpCraft.gui
{
    internal class GuiItemSlot : GuiButton
    {
        public ItemStack Stack;

        public GuiItemSlot(int ID, float x, float y, float scale, ItemStack stack) : base(ID, x, y, scale)
        {
            Stack = stack;
        }

        public override void Render(int mouseX, int mouseY)
        {
            int v = 0;

            Hovered = IsMouseOver(mouseX, mouseY);

            if (Hovered)
                v += 32;

            float x = PosX;
            float y = PosY;

            if (CenteredX)
                x = (int)(SharpCraft.Instance.ClientSize.Width / 2f - 32 * Scale / 2);

            RenderTexture(TextureManager.TEXTURE_GUI_WIDGETS, x, y, 224, v, 32, 32, Scale);

            if (Stack == null || Stack.IsEmpty)
                return;

            if (Stack.Item is ItemBlock itemBlock)
            {
                Block block = itemBlock.Block;

                x += 14 * Scale / 2;
                y += 14 * Scale / 2;

                RenderBlock(block, x, y, 2.25f * Scale / 2);
            }

            if (Stack.Count > 1)
                RenderText(Stack.Count.ToString(), PosX + 32 * Scale / 2f, PosY + 32 * Scale / 2f + 14, 1, Hovered ? HoverColor : Vector3.One, true, true);
        }

        public override bool IsMouseOver(int x, int y)
        {
            return x >= PosX &&
                   y >= PosY &&
                   x <= PosX + 32 * Scale &&
                   y <= PosY + 32 * Scale;
        }
    }
}