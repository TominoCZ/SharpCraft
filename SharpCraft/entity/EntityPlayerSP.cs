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

        private Vector2 moveSpeed;

        public int HotbarIndex { get; private set; }

        public ItemStack[] hotbar { get; }

        public bool HasFullInventory => hotbar.All(stack => stack != null && !stack.IsEmpty);

        public EntityPlayerSP(World world, Vector3 pos = new Vector3()) : base(world, pos)
        {
            SharpCraft.Instance.Camera.pos = pos + Vector3.UnitY * 1.625f;

            collisionBoundingBox = new AxisAlignedBB(new Vector3(0.3f, 1.65f, 0.3f));
            boundingBox = collisionBoundingBox.offset(pos - new Vector3(collisionBoundingBox.size.X / 2, 0, collisionBoundingBox.size.Z / 2));

            hotbar = new ItemStack[9];
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

            SharpCraft.Instance.Camera.pos = interpolatedPos + Vector3.UnitY * 1.625f;
        }

        private void UpdateCameraMovement()
        {
            if (SharpCraft.Instance.GuiScreen != null)
                return;

            var state = SharpCraft.Instance.KeysDown;

            Vector2 dirVec = Vector2.Zero;

            var w = state.Contains(Key.W); //might use this later
            var s = state.Contains(Key.S);
            var a = state.Contains(Key.A);
            var d = state.Contains(Key.D);

            if (w) dirVec += SharpCraft.Instance.Camera.forward;
            if (s) dirVec += -SharpCraft.Instance.Camera.forward;
            if (a) dirVec += SharpCraft.Instance.Camera.left;
            if (d) dirVec += -SharpCraft.Instance.Camera.left;

            float mult = 1;

            if (state.Contains(Key.LShift))
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

        public void setItemStackInHotbar(int index, ItemStack stack)
        {
            hotbar[index % 9] = stack;
        }

        public void setItemStackInSelectedSlot(ItemStack stack)
        {
            hotbar[HotbarIndex] = stack;
        }

        public bool OnPickup(ItemStack stack)
        {
            for (var i = 0; i < hotbar.Length; i++)
            {
                var itemStack = hotbar[i];

                if (itemStack == null || itemStack.IsEmpty)
                {
                    setItemStackInHotbar(i, stack);
                    return true;
                }

                if (stack.Item == itemStack.Item && stack.Count < 63)
                {
                    itemStack.Count++;
                    return true;
                }
            }

            return false;
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

            var stack = getEquippedItemStack();

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
                        setSelectedSlot(i);
                        inHotbar = true;
                        break;
                    }
                }

                if (!inHotbar)
                    setItemStackInSelectedSlot(new ItemStack(new ItemBlock(clickedBlock), 1, world.GetMetadata(moo.blockPos)));
            }
        }

        public void DropHeldItem()
        {
            var stack = getEquippedItemStack();

            if (stack != null && !stack.IsEmpty)
            {
                world?.AddEntity(new EntityItem(world, SharpCraft.Instance.Camera.pos - Vector3.UnitY * 0.35f,
                    SharpCraft.Instance.Camera.getLookVec() * 0.75f + Vector3.UnitY * 0.2f, stack.CopyUnit()));

                stack.Count--;
            }
        }

        public ItemStack getEquippedItemStack()
        {
            return hotbar[HotbarIndex];
        }

        public void setSelectedSlot(int index)
        {
            HotbarIndex = index % 9;
        }

        public void selectNextItem()
        {
            HotbarIndex = (HotbarIndex + 1) % 9;
        }

        public void selectPreviousItem()
        {
            if (HotbarIndex <= 0)
                HotbarIndex = 8;
            else
                HotbarIndex = HotbarIndex - 1;
        }
    }
}