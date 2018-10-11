using OpenTK;
using OpenTK.Input;
using System;
using System.Drawing;
using SharpCraft_Client.item;
using SharpCraft_Client.texture;

namespace SharpCraft_Client.gui
{
    internal class GuiScreenInventory : GuiScreen
    {
        private static float _guiScale = 1.75f;

        private static float _space = 5 * _guiScale / 2;

        private static float _scaledWidth = 32 * _guiScale;
        private static float _scaledHeight = 32 * _guiScale;

        private static float _totalInventoryWidth = 9 * _scaledWidth + 8 * _space;
        private static float _totalInventoryHeight = 3 * _scaledHeight + 2 * _space + _scaledHeight + _space * 5;

        private static GuiTexture _inventoryBackground;

        private ItemStack _draggedStack;

        private float _startPosX;
        private float _startPosY;

        public GuiScreenInventory()
        {
            DoesGuiPauseGame = false;

            for (int i = 0; i < 36; i++)
            {
                Buttons.Add(new GuiItemSlot(i, 0, 0, _guiScale, null));
            }

            _inventoryBackground = new GuiTexture(TextureManager.LoadTexture("gui/inventory_bg"), 0, 0, 318, 163, _guiScale);
        }

        public override void Render(int mouseX, int mouseY)
        {
            DrawBackroundStretch();

            _guiScale = MathHelper.Clamp((SharpCraft.Instance.ClientSize.Width / 640f + SharpCraft.Instance.ClientSize.Width / 480f) / 2, 1.75f, 2);

            _space = 5 * _guiScale / 2;

            _scaledWidth = 32 * _guiScale;
            _scaledHeight = 32 * _guiScale;

            _totalInventoryWidth = 9 * _scaledWidth + 8 * _space;
            _totalInventoryHeight = 3 * _scaledHeight + 2 * _space + _scaledHeight + _space * 5;

            _inventoryBackground.Scale = _guiScale;

            Size size = SharpCraft.Instance.ClientSize;

            _startPosX = size.Width / 2f - _totalInventoryWidth / 2f;
            _startPosY = size.Height / 2f - _totalInventoryHeight / 2f;

            RenderTexture(_inventoryBackground, _startPosX - 10 * _guiScale / 2, _startPosY - 10 * _guiScale / 2);

            // Hotbar item slots
            float hotbarY = _scaledHeight + _startPosY + 2 * (_scaledHeight + _space * 5);
            for (int i = 0; i < 9; i++)
            {
                float x = _startPosX + i * (_scaledWidth + _space);

                GuiItemSlot targetBtn = (GuiItemSlot)Buttons.Find(btn => btn.ID == i && btn is GuiItemSlot);

                if (targetBtn != null)
                {
                    float alpha = 1f;

                    if (targetBtn.Stack != null && !targetBtn.Stack.ItemSame(_draggedStack) && _draggedStack != null && !_draggedStack.IsEmpty)
                        alpha = 0.85f;

                    targetBtn.AlphaForRender = alpha;

                    targetBtn.PosX = x;
                    targetBtn.PosY = hotbarY;
                    targetBtn.Stack = SharpCraft.Instance.Player.GetItemStackInInventory(i);
                    targetBtn.Scale = _guiScale;
                }
            }

            // Inventory item slots
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    float x = _startPosX + j * (_scaledWidth + _space);
                    float y = _startPosY + i * (_scaledHeight + _space);

                    int index = 9 + i * 9 + j;

                    GuiItemSlot targetBtn = (GuiItemSlot)Buttons.Find(btn => btn.ID == index && btn is GuiItemSlot);

                    if (targetBtn != null)
                    {
                        targetBtn.PosX = x;
                        targetBtn.PosY = y;
                        targetBtn.Stack = SharpCraft.Instance.Player.GetItemStackInInventory(index);
                        targetBtn.Scale = _guiScale;
                    }
                }
            }

            // Draw slots
            base.Render(mouseX, mouseY);

            if (_draggedStack != null && !_draggedStack.IsEmpty)
            {
                float x = mouseX - 16 * _guiScale / 2;
                float y = mouseY - 16 * _guiScale / 2;

                RenderStack(_draggedStack, x, y, _guiScale);

                if (_draggedStack.Count > 1)
                    RenderText(_draggedStack.Count.ToString(), x + 16 * _guiScale / 2f, y + 16 * _guiScale / 2f + 14, 1,
                        Vector3.One, true, true);
            }
        }

        public override void OnMouseClick(int x, int y, MouseButton button)
        {
            base.OnMouseClick(x, y, button);

            if (x <= _startPosX - 10 * _guiScale / 2 || x > _startPosX + _totalInventoryWidth + 10 * _guiScale / 2 ||
                y <= _startPosY - 10 * _guiScale / 2 || y > _startPosY + _totalInventoryHeight + 10 * _guiScale)
            {
                SharpCraft.Instance.Player.ThrowStack(_draggedStack);
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
                else if ((_draggedStack == null || _draggedStack.IsEmpty) && slot.Stack != null && !slot.Stack.IsEmpty) //when not holding anything and clicked a non-empty stack in the inventory
                {
                    int toTake = slot.Stack.Count;

                    if (button == MouseButton.Right)
                    {
                        toTake /= 2;
                        toTake = toTake == 0 ? 1 : toTake;
                    }

                    _draggedStack = slot.Stack.Copy(toTake);

                    slot.Stack.Count -= toTake;
                }
                else
                {
                    if (slot.Stack == null || slot.Stack.IsEmpty) // when holding a non-empty stack and clicking an empty slot
                    {
                        if (button == MouseButton.Right && _draggedStack != null)
                        {
                            SharpCraft.Instance.Player.SetItemStackInInventory(btn.ID, _draggedStack.Copy(1));

                            _draggedStack.Count--;
                        }
                        else
                        {
                            SharpCraft.Instance.Player.SetItemStackInInventory(btn.ID, _draggedStack);

                            _draggedStack = null;
                        }
                    }
                    else if (_draggedStack != null && slot.Stack.ItemSame(_draggedStack))
                    {
                        int ammountToMove = button == MouseButton.Right ? 1 : Math.Min(slot.Stack.Item.GetMaxStackSize() - slot.Stack.Count, _draggedStack.Count);

                        slot.Stack.Count += ammountToMove;
                        _draggedStack.Count -= ammountToMove;
                    }
                    else if (_draggedStack != null && !_draggedStack.IsEmpty)
                    {
                        ItemStack stackInSlot = SharpCraft.Instance.Player.GetItemStackInInventory(btn.ID);
                        SharpCraft.Instance.Player.SetItemStackInInventory(btn.ID, _draggedStack.Copy());

                        _draggedStack = stackInSlot.Copy();
                    }
                }
            }
        }

        public override void OnClose()
        {
            SharpCraft.Instance.Player.ThrowStack(_draggedStack);
        }
    }
}