using OpenTK;
using OpenTK.Input;
using SharpCraft.item;
using SharpCraft.texture;
using System;

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
            DrawBackroundStretch();

            guiScale = Math.Clamp((SharpCraft.Instance.ClientSize.Width / 640f + SharpCraft.Instance.ClientSize.Width / 480f) / 2, 1.75f, 2);

            space = 5 * guiScale / 2;

            scaledWidth = 32 * guiScale;
            scaledHeight = 32 * guiScale;

            totalInventoryWidth = 9 * scaledWidth + 8 * space;
            totalInventoryHeight = 3 * scaledHeight + 2 * space + scaledHeight + space * 5;

            inventoryBackground.Scale = guiScale;

            Size size = SharpCraft.Instance.ClientSize;

            startPosX = size.Width / 2f - totalInventoryWidth / 2f;
            startPosY = size.Height / 2f - totalInventoryHeight / 2f;

            RenderTexture(inventoryBackground, startPosX - 10 * guiScale / 2, startPosY - 10 * guiScale / 2);

            // Hotbar item slots
            float hotbarY = scaledHeight + startPosY + 2 * (scaledHeight + space * 5);
            for (int i = 0; i < 9; i++)
            {
                float x = startPosX + i * (scaledWidth + space);

                GuiItemSlot targetBtn = (GuiItemSlot)buttons.Find(btn => btn.ID == i && btn is GuiItemSlot);

                if (targetBtn != null)
                {
                    float alpha = 1f;

                    if (targetBtn.Stack != null && !targetBtn.Stack.ItemSame(draggedStack) && draggedStack != null && !draggedStack.IsEmpty)
                        alpha = 0.85f;

                    targetBtn.AlphaForRender = alpha;

                    targetBtn.PosX = x;
                    targetBtn.PosY = hotbarY;
                    targetBtn.Stack = SharpCraft.Instance.Player.GetItemStackInInventory(i);
                    targetBtn.Scale = guiScale;
                }
            }

            // Inventory item slots
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    float x = startPosX + j * (scaledWidth + space);
                    float y = startPosY + i * (scaledHeight + space);

                    int index = 9 + i * 9 + j;

                    GuiItemSlot targetBtn = (GuiItemSlot)buttons.Find(btn => btn.ID == index && btn is GuiItemSlot);

                    if (targetBtn != null)
                    {
                        targetBtn.PosX = x;
                        targetBtn.PosY = y;
                        targetBtn.Stack = SharpCraft.Instance.Player.GetItemStackInInventory(index);
                        targetBtn.Scale = guiScale;
                    }
                }
            }

            // Draw slots
            base.Render(mouseX, mouseY);

            if (draggedStack != null && !draggedStack.IsEmpty)
            {
                float x = mouseX - 16 * guiScale / 2;
                float y = mouseY - 16 * guiScale / 2;

                RenderStack(draggedStack, x, y, guiScale);

                if (draggedStack.Count > 1)
                    RenderText(draggedStack.Count.ToString(), x + 16 * guiScale / 2f, y + 16 * guiScale / 2f + 14, 1,
                        Vector3.One, true, true);
            }
        }

        public override void OnMouseClick(int x, int y, MouseButton button)
        {
            base.OnMouseClick(x, y, button);

            if (x <= startPosX - 10 * guiScale / 2 || x > startPosX + totalInventoryWidth + 10 * guiScale / 2 ||
                y <= startPosY - 10 * guiScale / 2 || y > startPosY + totalInventoryHeight + 10 * guiScale)
            {
                SharpCraft.Instance.Player.ThrowStack(draggedStack);
            }
        }

        protected override void ButtonClicked(GuiButton btn, MouseButton button)
        {
            if (btn is GuiItemSlot slot)
            {
                if (SharpCraft.Instance.KeyboardState.IsKeyDown(Key.LShift))
                {
                    SharpCraft.Instance.Player.FastMoveStack(slot.ID);
                }
                else if ((draggedStack == null || draggedStack.IsEmpty) && slot.Stack != null && !slot.Stack.IsEmpty) //when not holding anything and clicked a non-empty stack in the inventory
                {
                    int toTake = slot.Stack.Count;

                    if (button == MouseButton.Right)
                    {
                        toTake /= 2;
                        toTake = toTake == 0 ? 1 : toTake;
                    }

                    draggedStack = slot.Stack.Copy(toTake);

                    slot.Stack.Count -= toTake;
                }
                else
                {
                    if (slot.Stack == null || slot.Stack.IsEmpty) // when holding a non-empty stack and clicking an empty slot
                    {
                        if (button == MouseButton.Right && draggedStack != null)
                        {
                            SharpCraft.Instance.Player.SetItemStackInInventory(btn.ID, draggedStack.Copy(1));

                            draggedStack.Count--;
                        }
                        else
                        {
                            SharpCraft.Instance.Player.SetItemStackInInventory(btn.ID, draggedStack);

                            draggedStack = null;
                        }
                    }
                    else if (draggedStack != null && slot.Stack.ItemSame(draggedStack))
                    {
                        int ammountToMove = button == MouseButton.Right ? 1 : Math.Min(slot.Stack.Item.GetMaxStackSize() - slot.Stack.Count, draggedStack.Count);

                        slot.Stack.Count += ammountToMove;
                        draggedStack.Count -= ammountToMove;
                    }
                    else if (draggedStack != null && !draggedStack.IsEmpty)
                    {
                        ItemStack stackInSlot = SharpCraft.Instance.Player.GetItemStackInInventory(btn.ID);
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