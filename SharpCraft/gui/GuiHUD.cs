using OpenTK;
using SharpCraft.block;
using SharpCraft.entity;
using SharpCraft.item;
using SharpCraft.render.shader;
using SharpCraft.render.shader.shaders;
using SharpCraft.texture;

namespace SharpCraft.gui
{
    internal class GuiHUD : Gui
    {
        //private GuiTexture slot;
        //private GuiTexture slot_selected;

        public GuiHUD()
        {
            /*var slot_texture = TextureManager.LoadTexture("gui/slot");
            var slot_selected_texture = TextureManager.LoadTexture("gui/slot_selected");

            if (slot_texture != null)
                slot = new GuiTexture(slot_texture, Vector2.Zero, Vector2.One * 2);
            if (slot_selected_texture != null)
                slot_selected = new GuiTexture(slot_selected_texture.textureID, slot_selected_texture.textureSize, Vector2.Zero, Vector2.One * 2);*/
        }

        public override void Render(int mouseX, int mouseY)
        {
            var size = SharpCraft.Instance.ClientSize;

            int space = 5;

            int scaledWidth = 32 * 2;
            int scaledHeight = 32 * 2;

            int totalHotbarWidth = 9 * scaledWidth + 8 * space;

            int startPos = size.Width / 2 - totalHotbarWidth / 2;

            for (int i = 0; i < 9; i++)
            {
                var b = i == SharpCraft.Instance.Player.HotbarIndex;

                var x = startPos + i * (scaledWidth + space);
                var y = size.Height - 20 - scaledHeight;

                var u = 224;
                var v = 0;

                if (b)
                    v += 32;

                RenderTexture(TextureManager.TEXTURE_GUI_WIDGETS, x, y, u, v, 32, 32, 2);

                var stack = SharpCraft.Instance.Player.hotbar[i];

                if (!stack?.IsEmpty == true)
                {
                    if (stack.Item is ItemBlock itemBlock)
                    {
                        var block = itemBlock.GetBlock();

                        x += 14;
                        y += 14;

                        RenderBlock(block, x, y, 2.25f);
                    }
                }
            }
        }
    }
}