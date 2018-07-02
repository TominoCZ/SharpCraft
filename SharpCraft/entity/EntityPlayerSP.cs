using OpenTK;
using OpenTK.Input;
using SharpCraft.block;
using SharpCraft.gui;
using SharpCraft.item;
using SharpCraft.model;
using SharpCraft.util;
using SharpCraft.world;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpCraft.entity
{
    public class EntityPlayerSp : Entity
    {
        private readonly float _maxMoveSpeed = 0.22f;
        private float _moveSpeedMult = 1;

        public float EyeHeight = 1.625f;

        private Vector2 _moveSpeed;

        // Health Variables
        // 0% is death, 100% is full health
        private float _health = 100.0f;

        public float Health
        {
            get { return _health; }
            set
            {
                _health = value;
                GuiHud.UpdateLivesUi(_health);
            }
        }

        public bool IsRunning { get; private set; }
        public bool IsSneaking { get; private set; }

        public int HotbarIndex { get; private set; }

        public ItemStack[] Hotbar { get; }
        public ItemStack[] Inventory { get; }

        // falling variables
        private float _fallDistance;

        private bool _wasSpaceDown;
        private bool _wasWalkDown;
        private bool _wasSneaking;
        private bool _isFalling;
        private float _fallYPosition;
        private int _runTimer;

        private BlockPos _sneakPos;

        public bool HasFullInventory => Hotbar.All(stack => stack != null && !stack.IsEmpty) && Inventory.All(stack => stack != null && !stack.IsEmpty);

        public EntityPlayerSp(World world, Vector3 pos = new Vector3()) : base(world, pos)
        {
            SharpCraft.Instance.Camera.Pos = pos + Vector3.UnitY * 1.625f;

            CollisionBoundingBox = new AxisAlignedBb(new Vector3(0.6f, 1.65f, 0.6f));
            BoundingBox = CollisionBoundingBox.Offset(pos - (Vector3.UnitX * CollisionBoundingBox.Size.X / 2 + Vector3.UnitZ * CollisionBoundingBox.Size.Z / 2));

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

            if (_runTimer > 0)
                _runTimer--;

            base.Update();
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

            if (w) dirVec += SharpCraft.Instance.Camera.Forward;
            if (s) dirVec += -SharpCraft.Instance.Camera.Forward;
            if (a) dirVec += SharpCraft.Instance.Camera.Left;
            if (d) dirVec += -SharpCraft.Instance.Camera.Left;

            float mult = 1;

            if (IsRunning && !IsSneaking)
                mult = 1.5f;
            else if (IsSneaking)
            {
                IsRunning = false;
                mult = 0.625f;
            }

            if (dirVec != Vector2.Zero)
            {
                _moveSpeedMult = MathHelper.Clamp(_moveSpeedMult + 0.085f, 1, 1.55f);

                _moveSpeed = MathUtil.Clamp(_moveSpeed + dirVec.Normalized() * 0.1f * _moveSpeedMult, 0, _maxMoveSpeed);

                Motion.Xz = _moveSpeed * mult;
            }
            else
            {
                _moveSpeed = Vector2.Zero;
                _moveSpeedMult = 1;
            }
        }

        public override void Move()
        {
            AxisAlignedBb bbO = BoundingBox.Union(BoundingBox.Offset(Motion));

            List<AxisAlignedBb> list = SharpCraft.Instance.World.GetBlockCollisionBoxes(bbO);

            if (IsSneaking)
            {
                if (OnGround)
                {
                    var pos = new BlockPos(Pos - Vector3.UnitY * 0.1f);
                    var air = BlockRegistry.GetBlock<BlockAir>();
                    var block = World.GetBlockState(pos).Block;

                    if ((!_wasSneaking || _sneakPos != pos && !block.Material.CanWalkThrough) && block != air)
                    {
                        _wasSneaking = true;

                        _sneakPos = pos;
                    }

                    foreach (var side in FaceSides.YPlane)
                    {
                        var posO = _sneakPos.Offset(side);
                        var blockO = World.GetBlockState(posO).Block;

                        if (blockO != air || !blockO.Material.CanWalkThrough)
                            continue;

                        var offset = side.ToVec() * CollisionBoundingBox.Size * 0.975f;

                        var box = AxisAlignedBb.BlockFull.Offset(posO.Offset(FaceSides.Up).ToVec() + offset);

                        list.Add(box);
                    }
                }
            }
            else
            {
                _wasSneaking = false;
            }

            Vector3 mOrig = Motion;

            for (int i = 0; i < list.Count; i++)
            {
                AxisAlignedBb blockBb = list[i];
                Motion.Y = blockBb.CalculateYOffset(BoundingBox, Motion.Y);
            }
            BoundingBox = BoundingBox.Offset(Motion * Vector3.UnitY);

            for (int i = 0; i < list.Count; i++)
            {
                AxisAlignedBb blockBb = list[i];
                Motion.X = blockBb.CalculateXOffset(BoundingBox, Motion.X);
            }
            BoundingBox = BoundingBox.Offset(Motion * Vector3.UnitX);

            for (int i = 0; i < list.Count; i++)
            {
                AxisAlignedBb blockBb = list[i];
                Motion.Z = blockBb.CalculateZOffset(BoundingBox, Motion.Z);
            }
            BoundingBox = BoundingBox.Offset(Motion * Vector3.UnitZ);

            SetPositionToBb();

            bool stoppedX = Math.Abs(mOrig.X - Motion.X) > 0.00001f;
            bool stoppedY = Math.Abs(mOrig.Y - Motion.Y) > 0.00001f;
            bool stoppedZ = Math.Abs(mOrig.Z - Motion.Z) > 0.00001f;

            OnGround = stoppedY && mOrig.Y < 0.0D;

            bool onCeiling = stoppedY && mOrig.Y > 0.0D;

            if (stoppedX)
                Motion.X = 0;

            if (stoppedZ)
                Motion.Z = 0;

            if (onCeiling)
                Motion.Y *= 0.15f;
            else if (OnGround)
                Motion.Y = 0;

            if (stoppedX || stoppedZ)
                IsRunning = false;
        }

        public override void Render(float partialTicks)
        {
            Vector3 interpolatedPos = LastPos + (Pos - LastPos) * partialTicks;

            KeyboardState state = SharpCraft.Instance.KeyboardState;

            Vector3 offset = Vector3.Zero;

            if (IsSneaking = state.IsKeyDown(Key.LShift))
                offset = Vector3.UnitY * -0.2f;

            if (state.IsKeyDown(Key.Space) && !_wasSpaceDown && OnGround)
            {
                _wasSpaceDown = true;
                Motion.Y = 0.55F;
            }
            else if ((!state.IsKeyDown(Key.Space) || OnGround) && _wasSpaceDown)
                _wasSpaceDown = false;

            SharpCraft.Instance.Camera.Pos = interpolatedPos + Vector3.UnitY * EyeHeight + offset;

            if (state.IsKeyDown(Key.W))
            {
                if (!_wasWalkDown)
                {
                    _wasWalkDown = true;

                    if (_runTimer > 0)
                        IsRunning = true;
                    else
                    {
                        IsRunning = false;
                        _runTimer = 6;
                    }

                    Console.WriteLine(_runTimer);
                }
            }
            else
            {
                _wasWalkDown = false;
            }
        }

        private void FallDamage()
        {
            // 1 block is 1 distance unit

            // Falling
            if (Pos.Y < LastPos.Y)
            {
                if (_isFalling == false)
                {
                    // inital conditions
                    _isFalling = true;
                    _fallYPosition = Pos.Y;
                }
            }
            else
            {
                // hit the ground
                if (_isFalling == true)
                {
                    // final condition
                    _fallDistance = _fallYPosition - Pos.Y;

                    // damage calculation:
                    // do half a heart of damage for every block passed past 3 blocks
                    const float halfHeartPercentage = 5.0f;
                    const int lowestBlockHeight = 3;

                    if (_fallDistance > lowestBlockHeight)
                        TakeDamage((_fallDistance - lowestBlockHeight) * halfHeartPercentage);
                }

                // Not falling
                _isFalling = false;
                _fallYPosition = 0.0f;
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

                    var canPlace = state.Block.CanBlockBePlacedAtSide(World, moo.blockPos, moo.sideHit, this);
                    var activated = state.Block.OnActivated(SharpCraft.Instance.MouseOverObject, this);

                    if ((IsSneaking || !activated) && canPlace)
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

            if (JsonModelLoader.GetModelForBlock(state.Block) != null)
                SharpCraft.Instance.ParticleRenderer.SpawnDestroyParticles(moo.blockPos, state);

            World.SetBlockState(moo.blockPos, BlockRegistry.GetBlock<BlockAir>().GetState());

            state.Block.OnDestroyed(moo.blockPos, state, this);

            if (state.Block == BlockRegistry.GetBlock<BlockStone>())
                state = BlockRegistry.GetBlock<BlockCobbleStone>().GetState();

            Vector3 motion = new Vector3(MathUtil.NextFloat(-0.15f, 0.15f), 0.3f, MathUtil.NextFloat(-0.15f, 0.15f));

            EntityItem entityDrop = new EntityItem(World, moo.blockPos.ToVec() + Vector3.One * 0.5f, motion, new ItemStack(ItemRegistry.GetItem(state.Block), 1, state.Block.GetMetaFromState(state)));

            World.AddEntity(entityDrop);

            SharpCraft.Instance.GetMouseOverObject();
        }

        private void PlaceBlock()
        {
            MouseOverObject moo = SharpCraft.Instance.MouseOverObject;
            if (moo.hit != HitType.Block)
                return;

            ItemStack stack = GetEquippedItemStack();

            BlockState clickedState = World.GetBlockState(moo.blockPos);

            if (!(stack?.Item is ItemBlock itemBlock))
                return;

            Block air = BlockRegistry.GetBlock<BlockAir>();
            Block glass = BlockRegistry.GetBlock<BlockGlass>();
            Block grass = BlockRegistry.GetBlock<BlockGrass>();
            Block dirt = BlockRegistry.GetBlock<BlockDirt>();

            bool replacing;

            BlockPos pos = (replacing = clickedState.Block.IsReplaceable && itemBlock.Block != clickedState.Block) ? moo.blockPos : moo.blockPos.Offset(moo.sideHit);

            Block heldBlock = itemBlock.Block;
            AxisAlignedBb blockBb = heldBlock.BoundingBox.Offset(pos.ToVec());

            if (!replacing && World.GetBlockState(pos).Block != air || World.GetIntersectingEntitiesBBs(blockBb).Count > 0 && !heldBlock.Material.CanWalkThrough)
                return;

            BlockPos posUnder = pos.Offset(FaceSides.Down);

            BlockState stateUnder = World.GetBlockState(posUnder);
            BlockState stateAbove = World.GetBlockState(pos.Offset(FaceSides.Up));

            BlockState toPlace = heldBlock.GetState(stack.Meta);
            BlockPos placePos = pos;

            if (stateUnder.Block == grass && heldBlock != glass && !heldBlock.Material.CanWalkThrough)
                World.SetBlockState(posUnder, dirt.GetState());

            if (stateAbove.Block != air && stateAbove.Block != glass && heldBlock == grass &&
                !heldBlock.Material.CanWalkThrough)
            {
                placePos = pos;
                toPlace = dirt.GetState();
            }

            World.SetBlockState(placePos, toPlace);

            toPlace.Block.OnPlaced(World, placePos, this);

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
                            ItemBlock itemBlock = ItemRegistry.GetItem(clickedState.Block);
                            ItemStack itemStack = new ItemStack(itemBlock, 1, clickedState.Block.GetMetaFromState(clickedState));

                            SetItemStackInHotbar(i, itemStack);
                            SetSelectedSlot(i);
                            return;
                        }
                    }

                    SetItemStackInSelectedSlot(new ItemStack(ItemRegistry.GetItem(clickedState.Block), 1, clickedState.Block.GetMetaFromState(clickedState)));
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

            World?.AddEntity(new EntityItem(World, SharpCraft.Instance.Camera.Pos - Vector3.UnitY * 0.35f,
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