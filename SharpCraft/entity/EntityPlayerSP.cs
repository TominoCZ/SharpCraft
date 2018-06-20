using OpenTK;
using OpenTK.Input;
using SharpCraft.block;
using SharpCraft.gui;
using SharpCraft.item;
using SharpCraft.util;
using SharpCraft.world;
using System;
using System.Linq;

namespace SharpCraft.entity
{
    public class EntityPlayerSP : Entity
    {
        private readonly float maxMoveSpeed = 0.22f;
        private float moveSpeedMult = 1;

        public float EyeHeight = 1.625f;

        private Vector2 moveSpeed;

        // Health Variables
        // 0% is death, 100% is full health
        private float health = 100.0f;

        public float Health
        {
            get { return health; }
            set
            {
                health = value;
                GuiHUD.UpdateLivesUI(health);
            }
        }

        public bool IsRunning { get; private set; }

        public int HotbarIndex { get; private set; }

        public ItemStack[] Hotbar { get; }
        public ItemStack[] Inventory { get; }

        // falling variables
        private float fallDistance;

        private bool isFalling;
        private float fallYPosition;

        public bool HasFullInventory => Hotbar.All(stack => stack != null && !stack.IsEmpty) && Inventory.All(stack => stack != null && !stack.IsEmpty);

        public EntityPlayerSP(World world, Vector3 pos = new Vector3()) : base(world, pos)
        {
            SharpCraft.Instance.Camera.pos = pos + Vector3.UnitY * 1.625f;

            collisionBoundingBox = new AxisAlignedBB(new Vector3(0.6f, 1.65f, 0.6f));
            boundingBox = collisionBoundingBox.offset(pos - (Vector3.UnitX * collisionBoundingBox.size.X / 2 + Vector3.UnitZ * collisionBoundingBox.size.Z / 2));

            Hotbar = new ItemStack[9];
            Inventory = new ItemStack[27];
        }

        public override void Update()
        {
            if (SharpCraft.Instance.Focused)
                UpdateCameraMovement();

            // Dont regen or test for fall damage if paused
            if (SharpCraft.Instance.IsPaused == false)
            {
                FallDamage();
                LifeRegen();
            }

            base.Update();
        }

        public override void Render(float partialTicks)
        {
            Vector3 interpolatedPos = LastPos + (Pos - LastPos) * partialTicks;

            SharpCraft.Instance.Camera.pos = interpolatedPos + Vector3.UnitY * EyeHeight;
        }

        private void FallDamage()
        {
            // 1 block is 1 distance unit

            // Falling
            if (Pos.Y < LastPos.Y)
            {
                if (isFalling == false)
                {
                    // inital conditions
                    isFalling = true;
                    fallYPosition = Pos.Y;
                }
            }
            else
            {
                // hit the ground
                if (isFalling == true)
                {
                    // final condition
                    fallDistance = fallYPosition - Pos.Y;

                    // damage calculation:
                    // do half a heart of damage for every block passed past 3 blocks
                    const float halfHeartPercentage = 5.0f;
                    const int lowestBlockHeight = 3;

                    if (fallDistance > lowestBlockHeight)
                        TakeDamage((fallDistance - lowestBlockHeight) * halfHeartPercentage);
                }

                // Not falling
                isFalling = false;
                fallYPosition = 0.0f;
            }
        }

        private void TakeDamage(float percentage)
        {
            // Health[0, 100]
            if (Health - percentage < 0)
            {
                Health = 0.0f;
                return;
            }

            // reduce health
            Health -= percentage;
        }

        private void LifeRegen()
        {
            // Health[0, 100]
            if (Health > 100.0f)
                Health = 100.0f;

            if (Health == 100.0f)
                return;

            // increase health
            Health += 0.06f; // 0.5 heart in 4 seconds
        }

        private void UpdateCameraMovement()
        {
            if (SharpCraft.Instance.GuiScreen != null)
                return;

            KeyboardState state = SharpCraft.Instance.KeyboardState;

            Vector2 dirVec = Vector2.Zero;

            bool w = state.IsKeyDown(Key.W); //might use this later
            bool s = state.IsKeyDown(Key.S);
            bool a = state.IsKeyDown(Key.A);
            bool d = state.IsKeyDown(Key.D);

            if (w) dirVec += SharpCraft.Instance.Camera.forward;
            if (s) dirVec += -SharpCraft.Instance.Camera.forward;
            if (a) dirVec += SharpCraft.Instance.Camera.left;
            if (d) dirVec += -SharpCraft.Instance.Camera.left;

            float mult = 1;

            if (IsRunning = state.IsKeyDown(Key.LShift))
                mult = 1.5f;

            if (dirVec != Vector2.Zero)
            {
                moveSpeedMult = MathHelper.Clamp(moveSpeedMult + 0.085f, 1, 1.55f);

                moveSpeed = MathUtil.Clamp(moveSpeed + dirVec.Normalized() * 0.1f * moveSpeedMult, 0, maxMoveSpeed);

                Motion.Xz = moveSpeed * mult;
            }
            else
            {
                moveSpeed = Vector2.Zero;
                moveSpeedMult = 1;
            }
        }

        public void FastMoveStack(int index)
        {
            ItemStack stack = GetItemStackInInventory(index);

            // return if there is no item to move
            if (stack == null || stack.Item == null)
                return;

            int maxStackSize = stack.Item.GetMaxStackSize();

            // Hotbar to Inventory
            if (index < Hotbar.Length)
                TryMoveStack(Hotbar, Inventory, SetItemStackInHotbar, index, index, maxStackSize);
            // Inventory to Hotbar
            else
                TryMoveStack(Inventory, Hotbar, SetItemStackInInventory, index, index - Hotbar.Length, maxStackSize);
        }

        private void TryMoveStack(ItemStack[] from, ItemStack[] to, Action<int, ItemStack> setItemFunction, int slotIndex, int localSlotIndex, int maxStackSize)
        {
            // 1. find same object in inventory to stack
            for (int inventoryIdx = 0; inventoryIdx < to.Length; inventoryIdx++)
            {
                if (to[inventoryIdx] == null || to[inventoryIdx].Item == null
                   || from[localSlotIndex] == null || from[localSlotIndex].Item == null
                   // Continue if:
                   || to[inventoryIdx].Item != from[localSlotIndex].Item // different item
                   || to[inventoryIdx].IsEmpty  // empty
                   || to[inventoryIdx].Count >= maxStackSize) // full
                {
                    continue;
                }

                // Combine stacks, storing any remainder
                ItemStack remainingStack = to[inventoryIdx].Combine(from[localSlotIndex]);
                // Assign remainder as new value
                setItemFunction(slotIndex, remainingStack);

                // finished
                if (remainingStack == null || remainingStack.Count <= 0)
                    return;
            }

            // 2. find first free inventory spot
            for (int inventoryIdx = 0; inventoryIdx < to.Length; inventoryIdx++)
            {
                if (to[inventoryIdx] != null && to[inventoryIdx].Item != null
                    || from[localSlotIndex] == null)
                {
                    continue;
                }

                // Initialise inventory slot without an item
                if (to[inventoryIdx] == null)
                    to[inventoryIdx] = new ItemStack(null);

                // Combine stacks, storing any remainder
                ItemStack remainingStack = to[inventoryIdx].Combine(from[localSlotIndex]);
                // Assign remainder as new value
                setItemFunction(slotIndex, remainingStack);
            }
        }

        public void SetItemStackInInventory(int index, ItemStack stack)
        {
            if (index < Hotbar.Length)
                SetItemStackInHotbar(index, stack);
            else
                Inventory[index - Hotbar.Length] = stack;
        }

        private void SetItemStackInHotbar(int index, ItemStack stack)
        {
            Hotbar[index % Hotbar.Length] = stack;
        }

        public ItemStack GetItemStackInInventory(int index)
        {
            if (index < Hotbar.Length)
                return GetItemStackInHotbar(index);

            return Inventory[index - Hotbar.Length];
        }

        private ItemStack GetItemStackInHotbar(int index)
        {
            return Hotbar[index % Hotbar.Length];
        }

        public void SetItemStackInSelectedSlot(ItemStack stack)
        {
            Hotbar[HotbarIndex] = stack;
        }

        public bool CanPickUpStack(ItemStack dropped)
        {
            return Hotbar.Any(stack => stack == null || stack.IsEmpty || stack.ItemSame(dropped) && stack.Count + dropped.Count <= dropped.Item.GetMaxStackSize()) ||
                   Inventory.Any(stack => stack == null || stack.IsEmpty || stack.ItemSame(dropped) && stack.Count + dropped.Count <= dropped.Item.GetMaxStackSize());
        }

        public bool OnPickup(ItemStack dropped)
        {
            int inventorySize = Hotbar.Length + Inventory.Length;

            int lastKnownEmpty = -1;

            // Check Hotbar first
            for (int i = 0; i < Hotbar.Length; i++)
            {
                ItemStack stack = GetItemStackInInventory(i);
                if (stack == null || stack.IsEmpty || !stack.ItemSame(dropped))
                    continue;

                if (dropped.ItemSame(stack) && stack.Count <= stack.Item.GetMaxStackSize())
                {
                    int toPickUp = Math.Min(stack.Item.GetMaxStackSize() - stack.Count, dropped.Count);

                    stack.Count += toPickUp;
                    dropped.Count -= toPickUp;
                }

                // return if fully combined
                if (dropped.IsEmpty)
                    return true;
            }

            for (int i = inventorySize - 1; i >= 0; i--)
            {
                ItemStack stack = GetItemStackInInventory(i);

                if (stack == null || stack.IsEmpty)
                {
                    lastKnownEmpty = i;
                    continue;
                }

                // Continue as already looked at Hotbar
                if (i < Hotbar.Length)
                    continue;

                if (dropped.ItemSame(stack) && stack.Count <= stack.Item.GetMaxStackSize())
                {
                    int toPickUp = Math.Min(stack.Item.GetMaxStackSize() - stack.Count, dropped.Count);

                    stack.Count += toPickUp;
                    dropped.Count -= toPickUp;
                }

                if (dropped.IsEmpty)
                    break;
            }

            if (lastKnownEmpty != -1)
            {
                SetItemStackInInventory(lastKnownEmpty, dropped.Copy());
                dropped.Count = 0;
            }

            return dropped.IsEmpty;
        }

        public void OnClick(MouseButton btn)
        {
            MouseOverObject moo = SharpCraft.Instance.MouseOverObject;

            if (moo.hit == HitType.Block)
            {
                if (btn == MouseButton.Right)
                {
                    BlockState state = World.GetBlockState(moo.blockPos);

                    if (state.Block.CanBeInteractedWith)
                    {
                        if (state.Block == BlockRegistry.GetBlock<BlockCraftingTable>())
                        {
                            SharpCraft.Instance.OpenGuiScreen(new GuiScreenCrafting());
                        }
                    }
                    else
                        PlaceBlock();
                }
                else if (btn == MouseButton.Left)
                {
                    //BreakBlock(); TODO - UVMin breaking EDIT: whhhat? :DDD
                }
            }
        }

        public void BreakBlock()
        {
            MouseOverObject moo = SharpCraft.Instance.MouseOverObject;
            if (moo.hit != HitType.Block)
                return;

            BlockState state = World.GetBlockState(moo.blockPos);

            if (JsonModelLoader.GetModelForBlock(state.Block.UnlocalizedName) != null)
                SharpCraft.Instance.ParticleRenderer.SpawnDestroyParticles(moo.blockPos, state);

            World.SetBlockState(moo.blockPos, BlockRegistry.GetBlock<BlockAir>().GetState());

            Vector3 motion = new Vector3(MathUtil.NextFloat(-0.15f, 0.15f), 0.3f, MathUtil.NextFloat(-0.15f, 0.15f));

            EntityItem entityDrop = new EntityItem(World, moo.blockPos.ToVec() + Vector3.One * 0.5f, motion, new ItemStack(new ItemBlock(state.Block), 1, state.Block.GetMetaFromState(state)));

            World.AddEntity(entityDrop);

            SharpCraft.Instance.GetMouseOverObject();
        }

        public void PlaceBlock()
        {
            MouseOverObject moo = SharpCraft.Instance.MouseOverObject;
            if (moo.hit != HitType.Block)
                return;

            ItemStack stack = GetEquippedItemStack();

            if (!(stack?.Item is ItemBlock itemBlock))
                return;

            Block air = BlockRegistry.GetBlock<BlockAir>();
            Block glass = BlockRegistry.GetBlock<BlockGlass>();
            Block grass = BlockRegistry.GetBlock<BlockGrass>();
            Block dirt = BlockRegistry.GetBlock<BlockDirt>();

            BlockPos pos = moo.blockPos.Offset(moo.sideHit);
            BlockState stateAtPos = World.GetBlockState(pos);

            Block heldBlock = itemBlock.Block;
            AxisAlignedBB blockBb = heldBlock.BoundingBox.offset(pos.ToVec());

            if (stateAtPos.Block != air || World.GetIntersectingEntitiesBBs(blockBb).Count > 0)
                return;

            BlockPos posUnder = pos.Offset(FaceSides.Down);

            BlockState stateUnder = World.GetBlockState(posUnder);
            BlockState stateAbove = World.GetBlockState(pos.Offset(FaceSides.Up));

            if (stateUnder.Block == grass && heldBlock != glass && heldBlock.IsSolid)
                World.SetBlockState(posUnder, dirt.GetState());
            if (stateAbove.Block != air && stateAbove.Block != glass && heldBlock == grass && stateAbove.Block.IsSolid)
                World.SetBlockState(pos, dirt.GetState());
            else
                World.SetBlockState(pos, heldBlock.GetState(stack.Meta));

            stack.Count--;

            SharpCraft.Instance.GetMouseOverObject();
        }

        public void PickBlock()
        {
            MouseOverObject moo = SharpCraft.Instance.MouseOverObject;

            if (moo.hit == HitType.Block)
            {
                var clickedState = World.GetBlockState(moo.blockPos);

                if (clickedState.Block != BlockRegistry.GetBlock<BlockAir>())
                {
                    for (int i = 0; i < Hotbar.Length; i++)
                    {
                        ItemStack stack = Hotbar[i];

                        if (stack?.Item is ItemBlock ib && ib.Block == clickedState.Block && stack.Meta == clickedState.Block.GetMetaFromState(clickedState))
                        {
                            SetSelectedSlot(i);
                            return;
                        }

                        if (stack?.IsEmpty == true)
                        {
                            ItemBlock itemBlock = new ItemBlock(clickedState.Block);
                            ItemStack itemStack = new ItemStack(itemBlock, 1, clickedState.Block.GetMetaFromState(clickedState));

                            SetItemStackInHotbar(i, itemStack);
                            SetSelectedSlot(i);
                            return;
                        }
                    }

                    SetItemStackInSelectedSlot(new ItemStack(new ItemBlock(clickedState.Block), 1, clickedState.Block.GetMetaFromState(clickedState)));
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

            int ammountToThrow = Math.Min(count, stack.Count);

            ItemStack toThrow = stack.Copy(1);
            toThrow.Count = ammountToThrow;

            World?.AddEntity(new EntityItem(World, SharpCraft.Instance.Camera.pos - Vector3.UnitY * 0.35f,
                SharpCraft.Instance.Camera.GetLookVec() * 0.75f + Vector3.UnitY * 0.1f, toThrow));

            stack.Count -= ammountToThrow;
        }

        public ItemStack GetEquippedItemStack()
        {
            return Hotbar[HotbarIndex];
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