using System;
using OpenTK;
using OpenTK.Input;
using SharpCraft.block;
using SharpCraft.gui;
using SharpCraft.model;
using SharpCraft.util;
using SharpCraft.world;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using SharpCraft.item;

namespace SharpCraft.entity
{
    public class EntityPlayerSP : Entity
    {
        private float maxMoveSpeed = 0.235f;
        private float moveSpeedMult = 1;

        public float EyeHeight = 1.625f;

        private Vector2 moveSpeed;

        public bool isRunning { get; private set; }

        public int HotbarIndex { get; private set; }

        public ItemStack[] hotbar { get; }
        public ItemStack[] inventory { get; }

        public bool HasFullInventory => hotbar.All(stack => stack != null && !stack.IsEmpty) && inventory.All(stack => stack != null && !stack.IsEmpty);

        public EntityPlayerSP(World world, Vector3 pos = new Vector3()) : base(world, pos)
        {
            SharpCraft.Instance.Camera.pos = pos + Vector3.UnitY * 1.625f;

            collisionBoundingBox = new AxisAlignedBB(new Vector3(0.6f, 1.65f, 0.6f));
            boundingBox = collisionBoundingBox.offset(pos - (Vector3.UnitX * collisionBoundingBox.size.X / 2 + Vector3.UnitZ * collisionBoundingBox.size.Z / 2));

            hotbar = new ItemStack[9];
            inventory = new ItemStack[27];
        }

        public override void Update()
        {
            if (SharpCraft.Instance.Focused)
                UpdateCameraMovement();

            base.Update();
        }

        public override void Render(float particalTicks)
        {
            var interpolatedPos = lastPos + (pos - lastPos) * particalTicks;

            SharpCraft.Instance.Camera.pos = interpolatedPos + Vector3.UnitY * EyeHeight;
        }

        private void UpdateCameraMovement()
        {
            if (SharpCraft.Instance.GuiScreen != null)
                return;

            var state = SharpCraft.Instance.KeyboardState;

            Vector2 dirVec = Vector2.Zero;

            var w = state.IsKeyDown(Key.W); //might use this later
            var s = state.IsKeyDown(Key.S);
            var a = state.IsKeyDown(Key.A);
            var d = state.IsKeyDown(Key.D);

            if (w) dirVec += SharpCraft.Instance.Camera.forward;
            if (s) dirVec += -SharpCraft.Instance.Camera.forward;
            if (a) dirVec += SharpCraft.Instance.Camera.left;
            if (d) dirVec += -SharpCraft.Instance.Camera.left;

            float mult = 1;

            if (isRunning = state.IsKeyDown(Key.LShift))
                mult = 1.5f;

            if (dirVec != Vector2.Zero)
            {
                moveSpeedMult = MathHelper.Clamp(moveSpeedMult + 0.085f, 1, 1.55f);

                moveSpeed = MathUtil.Clamp(moveSpeed + dirVec.Normalized() * 0.1f * moveSpeedMult, 0, maxMoveSpeed);

                motion.Xz = moveSpeed * mult;
            }
            else
            {
                moveSpeed = Vector2.Zero;
                moveSpeedMult = 1;
            }
        }

        public void FastMoveStack(int index)
        {
            //TODO - finish :D dont forget, that there is a possibility that the item in the stacks are the same, so they merge
            int i = 0;

            for (; i < inventory.Length; i++)
            {
                if (inventory[i] == null || inventory[i].IsEmpty)
                    break;
            }

            if (i >= index)
                return;
        }

        public void SetItemStackInInventory(int index, ItemStack stack)
        {
            if (index < hotbar.Length)
                SetItemStackInHotbar(index, stack);
            else
                inventory[index % inventory.Length] = stack;
        }

        private void SetItemStackInHotbar(int index, ItemStack stack)
        {
            hotbar[index % hotbar.Length] = stack;
        }

        public ItemStack GetItemStackInInventory(int index)
        {
            if (index < hotbar.Length)
                return GetItemStackInHotbar(index);

            return inventory[index % inventory.Length];
        }

        private ItemStack GetItemStackInHotbar(int index)
        {
            return hotbar[index % hotbar.Length];
        }

        public void SetItemStackInSelectedSlot(ItemStack stack)
        {
            hotbar[HotbarIndex] = stack;
        }

        public bool CanPickUpStack(ItemStack dropped)
        {
            return hotbar.Any(stack => stack == null || stack.IsEmpty || stack.ItemSame(dropped) && stack.Count + dropped.Count <= dropped.Item.MaxStackSize()) ||
                   inventory.Any(stack => stack == null || stack.IsEmpty || stack.ItemSame(dropped) && stack.Count + dropped.Count <= dropped.Item.MaxStackSize());
        }

        public bool OnPickup(ItemStack dropped)
        {
            for (var i = 0; i < hotbar.Length + inventory.Length; i++)
            {
                var stack = GetItemStackInInventory(i);

                if (stack == null || stack.IsEmpty)
                {
                    SetItemStackInInventory(i, dropped.Copy());
                    dropped.Count = 0;
                    continue;
                }

                if (dropped.Item == stack.Item && stack.Count <= stack.Item.MaxStackSize())
                {
                    var toPickUp = Math.Min(stack.Item.MaxStackSize() - stack.Count, dropped.Count);

                    stack.Count += toPickUp;
                    dropped.Count -= toPickUp;
                }

                if (dropped.IsEmpty)
                    break;
            }

            return dropped.IsEmpty;
        }

        public void OnClick(MouseButton btn)
        {
            var moo = SharpCraft.Instance.MouseOverObject;

            if (moo.hit is EnumBlock)
            {
                if (btn == MouseButton.Right)
                {
                    var block = world.GetBlock(moo.blockPos);
                    var model = ModelRegistry.GetModelForBlock(block, world.GetMetadata(moo.blockPos));

                    if (model != null && model.canBeInteractedWith)
                    {
                        switch (block)
                        {
                            case EnumBlock.FURNACE:
                            case EnumBlock.CRAFTING_TABLE:
                                SharpCraft.Instance.OpenGuiScreen(new GuiScreenCrafting());
                                break;
                        }
                    }
                    else
                        PlaceBlock();
                }
                else if (btn == MouseButton.Left)
                {
                    //BreakBlock(); TODO - start breaking
                }
            }
        }

        public void BreakBlock()
        {
            var moo = SharpCraft.Instance.MouseOverObject;
            if (!(moo.hit is EnumBlock))
                return;

            var block = world.GetBlock(moo.blockPos);

            if (block == EnumBlock.AIR)
                return;

            var meta = world.GetMetadata(moo.blockPos);

            SharpCraft.Instance.ParticleRenderer.SpawnDestroyParticles(moo.blockPos, block, meta);

            world.SetBlock(moo.blockPos, EnumBlock.AIR, 0);

            var motion = new Vector3(MathUtil.NextFloat(-0.15f, 0.15f), 0.25f, MathUtil.NextFloat(-0.15f, 0.15f));

            var entityDrop = new EntityItem(world, moo.blockPos.ToVec() + Vector3.One * 0.5f, motion, new ItemStack(new ItemBlock(block), 1, meta));

            world.AddEntity(entityDrop);

            SharpCraft.Instance.GetMouseOverObject();
        }

        public void PlaceBlock()
        {
            var moo = SharpCraft.Instance.MouseOverObject;
            if (!(moo.hit is EnumBlock))
                return;

            var stack = GetEquippedItemStack();

            if (!(stack?.Item is ItemBlock itemBlock))
                return;

            var pos = moo.blockPos.Offset(moo.sideHit);
            var blockAtPos = world.GetBlock(pos);

            var heldBlock = itemBlock.GetBlock();
            var blockBb = ModelRegistry.GetModelForBlock(heldBlock, world.GetMetadata(pos))
                .boundingBox.offset(pos.ToVec());

            if (blockAtPos != EnumBlock.AIR || world.GetIntersectingEntitiesBBs(blockBb).Count > 0)
                return;

            var posUnder = pos.Offset(FaceSides.Down);

            var blockUnder = world.GetBlock(posUnder);
            var blockAbove = world.GetBlock(pos.Offset(FaceSides.Up));

            if (blockUnder == EnumBlock.GRASS && heldBlock != EnumBlock.GLASS)
                world.SetBlock(posUnder, EnumBlock.DIRT, 0);
            if (blockAbove != EnumBlock.AIR && blockAbove != EnumBlock.GLASS &&
                heldBlock == EnumBlock.GRASS)
                world.SetBlock(pos, EnumBlock.DIRT, 0);
            else
                world.SetBlock(pos, heldBlock, stack.Meta);

            stack.Count--;

            SharpCraft.Instance.GetMouseOverObject();
        }

        public void PickBlock()
        {
            var moo = SharpCraft.Instance.MouseOverObject;

            if (moo.hit is EnumBlock clickedBlock)
            {
                var clickedMeta = world.GetMetadata(moo.blockPos);

                if (clickedBlock != EnumBlock.AIR)
                {
                    for (int i = 0; i < hotbar.Length; i++)
                    {
                        var stack = hotbar[i];

                        if (stack?.Item?.InnerItem == clickedBlock && stack.Meta == clickedMeta)
                        {
                            SetSelectedSlot(i);
                            return;
                        }
                    }

                    SetItemStackInSelectedSlot(new ItemStack(new ItemBlock(clickedBlock), 1,
                        world.GetMetadata(moo.blockPos)));
                }
            }
        }

        public void DropHeldItem()
        {
            ThrowStack(GetEquippedItemStack(), 1);
        }

        public void DropHeldStack()
        {
            ThrowStack(GetEquippedItemStack());
        }

        public void ThrowStack(ItemStack stack)
        {
            if (stack == null)
                return;

            ThrowStack(stack, stack.Count);
        }

        public void ThrowStack(ItemStack stack, int count)
        {
            if (stack == null || stack.IsEmpty)
                return;

            var ammountToThrow = Math.Min(count, stack.Count);

            var toThrow = stack.Copy(1);
            toThrow.Count = ammountToThrow;

            world?.AddEntity(new EntityItem(world, SharpCraft.Instance.Camera.pos - Vector3.UnitY * 0.35f,
                SharpCraft.Instance.Camera.GetLookVec() * 0.75f + Vector3.UnitY * 0.1f, toThrow));

            stack.Count -= ammountToThrow;
        }

        public ItemStack GetEquippedItemStack()
        {
            return hotbar[HotbarIndex];
        }

        public void SetSelectedSlot(int index)
        {
            HotbarIndex = index % 9;
        }

        public void SelectNextItem()
        {
            HotbarIndex = (HotbarIndex + 1) % 9;
        }

        public void SelectPreviousItem()
        {
            if (HotbarIndex <= 0)
                HotbarIndex = 8;
            else
                HotbarIndex = HotbarIndex - 1;
        }
    }
}