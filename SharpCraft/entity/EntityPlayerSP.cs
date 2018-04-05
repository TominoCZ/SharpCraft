using System;
using OpenTK;
using OpenTK.Input;
using SharpCraft.block;
using SharpCraft.gui;
using SharpCraft.model;
using SharpCraft.util;
using SharpCraft.world;
using System.Linq;
using SharpCraft.item;

namespace SharpCraft.entity
{
    public class EntityPlayerSP : Entity
    {
        private float maxMoveSpeed = 0.235f;
        private float moveSpeedMult = 1;

        public float EyeHeight = 1.625f;

        private Vector2 moveSpeed;

        public int HotbarIndex { get; private set; }

        public ItemStack[] hotbar { get; }
        public ItemStack[] inventory { get; }

        public bool HasFullInventory => hotbar.All(stack => stack != null && !stack.IsEmpty) && inventory.All(stack => stack != null && !stack.IsEmpty);

        public EntityPlayerSP(World world, Vector3 pos = new Vector3()) : base(world, pos)
        {
            SharpCraft.Instance.Camera.pos = pos + Vector3.UnitY * 1.625f;

            collisionBoundingBox = new AxisAlignedBB(new Vector3(0.6f, 1.65f, 0.6f));
            boundingBox = collisionBoundingBox.offset(pos - new Vector3(collisionBoundingBox.size.X / 2, 0, collisionBoundingBox.size.Z / 2));

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

            if (state.IsKeyDown(Key.LShift))
                mult = 1.5f;

            if (dirVec != Vector2.Zero)
            {
                moveSpeedMult = MathHelper.Clamp(moveSpeedMult + 0.085f, 1, 1.55f);

                moveSpeed = MathUtil.Clamp(moveSpeed + dirVec.Normalized() * 0.1f * moveSpeedMult, 0, maxMoveSpeed);// * moveSpeed;

                motion.Xz = moveSpeed * mult;
            }
            else
            {
                moveSpeed = Vector2.Zero;
                moveSpeedMult = 1;
            }
        }

        public void SetItemStackInHotbar(int index, ItemStack stack)
        {
            hotbar[index % 9] = stack;
        }

        public void SetItemStackInSelectedSlot(ItemStack stack)
        {
            hotbar[HotbarIndex] = stack;
        }

        public bool OnPickup(ItemStack dropped)
        {
            //TODO - add to inventory if stack of same type is present in it, otherwise put the stack into the hotbar slot if possible
            for (var i = 0; i < hotbar.Length + inventory.Length; i++)
            {
                if (i < hotbar.Length)
                {
                    var stack = hotbar[i];

                    if (stack == null || stack.IsEmpty)
                    {
                        SetItemStackInHotbar(i, dropped.Copy());
                        return true;
                    }

                    if (dropped.Item == stack.Item && stack.Count <= 64)
                    {
                        var toPickUp = Math.Min(64 - stack.Count, stack.Count);

                        stack.Count += toPickUp;
                        dropped.Count -= toPickUp;
                    }
                }
                else
                {
                    var index = i - hotbar.Length;
                    var stack = inventory[index];

                    if (stack == null || stack.IsEmpty)
                    {
                        inventory[index] = dropped.Copy();
                        return true;
                    }

                    if (dropped.Item == stack.Item && stack.Count <= 64)
                    {
                        var toPickUp = Math.Min(64 - stack.Count, stack.Count);

                        stack.Count += toPickUp;
                        dropped.Count -= toPickUp;
                    }
                }
            }

            return dropped.IsEmpty;
        }

        public bool CanPickUpStack(ItemStack dropped)
        {
            return hotbar.Any(stack => stack == null || stack.IsEmpty || stack.ItemSame(dropped) && stack.Count + dropped.Count <= 64) || inventory.Any(stack => stack == null || (stack.IsEmpty || stack.ItemSame(dropped) && stack.Count + dropped.Count <= 64));
        }

        public void OnClick(MouseButton btn)
        {
            var moo = SharpCraft.Instance.MouseOverObject;

            if (moo.hit is EnumBlock)
            {
                if (btn == MouseButton.Right)
                {
                    var block = world.GetBlock(moo.blockPos);
                    var model = ModelRegistry.getModelForBlock(block, world.GetMetadata(moo.blockPos));

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
                    BreakBlock();
            }
        }

        public void BreakBlock()
        {
            var moo = SharpCraft.Instance.MouseOverObject;
            if (!(moo.hit is EnumBlock))
                return;

            var block = world.GetBlock(moo.blockPos);
            var meta = world.GetMetadata(moo.blockPos);

            world.SetBlock(moo.blockPos, EnumBlock.AIR, 0);

            var motion = new Vector3(MathUtil.NextFloat(-0.15f, 0.15f), 0.25f, MathUtil.NextFloat(-0.15f, 0.15f));

            var entityDrop = new EntityItem(world, moo.blockPos.ToVec() + Vector3.One * 0.5f, motion, new ItemStack(new ItemBlock(block), 1, meta));

            SharpCraft.Instance.ParticleRenderer.SpawnDestroyParticles(moo.blockPos, block, meta);

            world.AddEntity(entityDrop);
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

            var heldBlock = itemBlock.getBlock();
            var blockBb = ModelRegistry.getModelForBlock(heldBlock, world.GetMetadata(pos))
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
        }

        public void PickBlock()
        {
            var moo = SharpCraft.Instance.MouseOverObject;

            var clickedBlock = world.GetBlock(moo.blockPos);
            var clickedMeta = world.GetMetadata(moo.blockPos);

            if (clickedBlock != EnumBlock.AIR)
            {
                var inHotbar = false; //this variable is here in case i need code under this if block (so that i dont have to return;)

                for (int i = 0; i < hotbar.Length; i++)
                {
                    var stack = hotbar[i];

                    if (stack?.Item?.item == clickedBlock && stack.Meta == clickedMeta)
                    {
                        SetSelectedSlot(i);
                        inHotbar = true;
                        break;
                    }
                }

                if (!inHotbar)
                    SetItemStackInSelectedSlot(new ItemStack(new ItemBlock(clickedBlock), 1, world.GetMetadata(moo.blockPos)));
            }
        }

        public void DropHeldItem()
        {
            var stack = GetEquippedItemStack();

            if (stack != null && !stack.IsEmpty)
            {
                world?.AddEntity(new EntityItem(world, SharpCraft.Instance.Camera.pos - Vector3.UnitY * 0.35f,
                    SharpCraft.Instance.Camera.GetLookVec() * 0.75f + Vector3.UnitY * 0.1f, stack.CopyUnit()));

                stack.Count--;
            }
        }

        public void DropHeldStack()
        {
            var stack = GetEquippedItemStack();

            if (stack != null && !stack.IsEmpty)
            {
                world?.AddEntity(new EntityItem(world, SharpCraft.Instance.Camera.pos - Vector3.UnitY * 0.35f,
                    SharpCraft.Instance.Camera.GetLookVec() * 0.75f + Vector3.UnitY * 0.1f, stack.Copy()));

                stack.Count = 0;
            }
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