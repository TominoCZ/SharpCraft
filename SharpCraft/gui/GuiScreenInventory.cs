using System;
using System.Linq;
using System.Security;
using OpenTK;
using OpenTK.Input;
using SharpCraft.block;
using SharpCraft.entity;
using SharpCraft.item;
using SharpCraft.texture;

namespace SharpCraft.gui
{
    internal class GuiScreenInventory : GuiScreen
    {
        private static float guiScale = 1.75f;

        private static float space = 5 * guiScale / 2;

        private static float scaledWidth = 32 * guiScale;
        private static float scaledHeight = 32 * guiScale;

        private static float totalInventoryWidth = 9 * scaledWidth + 8 * space;
        private static float totalInventoryHeight = 3 * scaledHeight + 2 * space + scaledHeight + space * 5;

        private static GuiTexture inventoryBackground;

        private ItemStack draggedStack;

        private float startPosX;
        private float startPosY;

        public GuiScreenInventory()
        {
            DoesGuiPauseGame = false;

            for (int i = 0; i < 36; i++)
            {
                buttons.Add(new GuiItemSlot(i, 0, 0, guiScale, null));
            }

            inventoryBackground = new GuiTexture(TextureManager.LoadTexture("gui/inventory_bg"), 0, 0, 318, 163, guiScale);
        }

        public override void Render(int mouseX, int mouseY)
        {
            DrawBackground();

            guiScale = Math.Clamp((SharpCraft.Instance.ClientSize.Width / 640f + SharpCraft.Instance.ClientSize.Width / 480f) / 2, 1.75f, 2);

            space = 5 * guiScale / 2;

            scaledWidth = 32 * guiScale;
            scaledHeight = 32 * guiScale;

            totalInventoryWidth = 9 * scaledWidth + 8 * space;
            totalInventoryHeight = 3 * scaledHeight + 2 * space + scaledHeight + space * 5;

            inventoryBackground.Scale = guiScale;
            RenderTexture(inventoryBackground, startPosX - 10 * guiScale / 2, startPosY - 10 * guiScale / 2);

            var size = SharpCraft.Instance.ClientSize;

            startPosX = size.Width / 2f - totalInventoryWidth / 2f;
            startPosY = size.Height / 2f - totalInventoryHeight / 2f;

            var hotbarY = scaledHeight + startPosY + 2 * (scaledHeight + space * 5);

            for (int i = 0; i < 9; i++)
            {
                var x = startPosX + i * (scaledWidth + space);

                GuiItemSlot targetBtn = (GuiItemSlot)buttons.Find(btn => btn.ID == i && btn is GuiItemSlot);

                if (targetBtn != null)
                {
                    var alpha = 1f;

                    if (targetBtn.stack != null && !targetBtn.stack.ItemSame(draggedStack) && draggedStack != null && !draggedStack.IsEmpty)
                        alpha = 0.85f;

                    targetBtn.alphaForRender = alpha;

                    targetBtn.posX = x;
                    targetBtn.posY = hotbarY;
                    targetBtn.stack = SharpCraft.Instance.Player.GetItemStackInInventory(i);
                    targetBtn.scale = guiScale;
                }
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    var x = startPosX + j * (scaledWidth + space);
                    var y = startPosY + i * (scaledHeight + space);

                    var index = 9 + i * 9 + j;

                    GuiItemSlot targetBtn = (GuiItemSlot)buttons.Find(btn => btn.ID == index && btn is GuiItemSlot);

                    if (targetBtn != null)
                    {
                        targetBtn.posX = x;
                        targetBtn.posY = y;
                        targetBtn.stack = SharpCraft.Instance.Player.GetItemStackInInventory(index);
                        targetBtn.scale = guiScale;
                    }
                }
            }

            base.Render(mouseX, mouseY);

            if (draggedStack != null && !draggedStack.IsEmpty && draggedStack.Item?.InnerItem is EnumBlock block)
            {
                RenderBlock(block, mouseX - 16 * guiScale / 2, mouseY - 16 * guiScale / 2, guiScale); //TODO - make this render Items too once they're implemented
            }
        }

        public override void OnMouseClick(int x, int y)
        {
            base.OnMouseClick(x, y);

            if (x <= startPosX - 10 * guiScale / 2 || x > startPosX + totalInventoryWidth + 10 * guiScale /2||
                y <= startPosY - 10 * guiScale / 2 || y > startPosY + totalInventoryHeight + 10 * guiScale)
            {
                SharpCraft.Instance.Player.ThrowStack(draggedStack);
            }
        }

        protected override void ButtonClicked(GuiButton btn)
        {
            if (btn is GuiItemSlot slot)
            {
                if (SharpCraft.Instance.KeyboardState.IsKeyDown(Key.LShift))
                {
                    SharpCraft.Instance.Player.FastMoveStack(slot.ID);
                }
                else if ((draggedStack == null || draggedStack.IsEmpty) && slot.stack != null && !slot.stack.IsEmpty)
                {
                    draggedStack = slot.stack.Copy();
                    slot.stack.Count = 0;
                }
                else
                {
                    if (slot.stack == null || slot.stack.IsEmpty)
                    {
                        SharpCraft.Instance.Player.SetItemStackInInventory(btn.ID, draggedStack);

                        draggedStack = null;
                    }
                    else if (draggedStack != null && slot.stack.ItemSame(draggedStack))
                    {
                        var ammountToMove = Math.Min(slot.stack.Item.MaxStackSize() - slot.stack.Count, draggedStack.Count);

                        slot.stack.Count += ammountToMove;
                        draggedStack.Count -= ammountToMove;
                    }
                    else if (draggedStack != null && !draggedStack.IsEmpty)
                    {
                        var stackInSlot = SharpCraft.Instance.Player.GetItemStackInInventory(btn.ID);
                        SharpCraft.Instance.Player.SetItemStackInInventory(btn.ID, draggedStack.Copy());

                        draggedStack = stackInSlot.Copy();
                    }
                }
            }
        }

        public override void OnClose()
        {
            SharpCraft.Instance.Player.ThrowStack(draggedStack);
        }
    }
}