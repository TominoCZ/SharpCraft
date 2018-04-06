using OpenTK;
using SharpCraft.item;
using SharpCraft.render.shader;
using SharpCraft.render.shader.shaders;
using SharpCraft.texture;

namespace SharpCraft.gui
{
    internal class GuiItemSlot : GuiButton
    {
        public ItemStack stack;

        public GuiItemSlot(int ID, float x, float y, ItemStack stack) : this(ID, x, y, 1, stack)
        {
            
        }

        public GuiItemSlot(int ID, float x, float y, float scale, ItemStack stack) : base(ID, x, y, scale)
        {
            this.stack = stack;
        }

        public override void Render(int mouseX, int mouseY)
        {
            var v = 0;

            if (IsMouseOver(mouseX, mouseY))
                v += 32;

            var x = posX;
            var y = posY;

            if (centered)
                x = (int)(SharpCraft.Instance.ClientSize.Width / 2f - 32 * scale / 2);

            RenderTexture(TextureManager.TEXTURE_GUI_WIDGETS, x, y, 224, v, 32, 32, scale);

            if (!stack?.IsEmpty == true)
            {
                if (stack.Item is ItemBlock itemBlock)
                {
                    var block = itemBlock.GetBlock();

                    x += 14* scale / 2;
                    y += 14* scale / 2;

                    RenderBlock(block, x, y, 2.25f * scale / 2);
                }
            }
        }

        public override bool IsMouseOver(int x, int y)
        {
            return x >= posX &&
                   y >= posY &&
                   x <= posX + 32 * scale &&
                   y <= posY + 32 * scale;
        }
    }
}