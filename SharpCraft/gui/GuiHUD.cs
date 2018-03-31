using OpenTK;
using SharpCraft.entity;
using SharpCraft.item;
using SharpCraft.shader;
using SharpCraft.texture;

namespace SharpCraft.gui
{
    internal class GuiHUD : Gui
    {
        private GuiTexture slot;
        private GuiTexture slot_selected;

        public GuiHUD()
        {
            var slot_texture = TextureManager.loadTexture("gui/slot", false);
            var slot_selected_texture = TextureManager.loadTexture("gui/slot_selected", false);

            if (slot_texture != null)
                slot = new GuiTexture(slot_texture.textureID, slot_texture.textureSize, Vector2.Zero, Vector2.One * 2);
            if (slot_selected_texture != null)
                slot_selected = new GuiTexture(slot_selected_texture.textureID, slot_selected_texture.textureSize, Vector2.Zero, Vector2.One * 2);
        }

        public override void render(ShaderGui shader, int mouseX, int mouseY)
        {
            var size = Game.Instance.ClientSize;

            int space = 5;

            int scaledWidth = (int)(slot.textureSize.Width * slot.scale.X);
            int scaledHeight = (int)(slot.textureSize.Height * slot.scale.Y);

            int totalHotbarWidth = 9 * scaledWidth + 8 * space;

            int startPos = size.Width / 2 - totalHotbarWidth / 2;

            for (int i = 0; i < 9; i++)
            {
                var b = i == Game.Instance.Player.HotbarIndex;

                var x = startPos + i * (scaledWidth + space);
                var y = size.Height - 20 - scaledHeight;

                renderTexture(shader, b ? slot_selected : slot, x, y);

                var stack = Game.Instance.Player.hotbar[i];

                if (!stack?.IsEmpty == true)
                {
                    if (stack.Item is ItemBlock itemBlock)
                    {
                        var block = itemBlock.getBlock();

                        x += 14;
                        y += 14;

                        renderBlock(block, 2.25f, x, y);
                    }
                }
            }
        }
    }
}