using System;
using OpenTK;
using OpenTK.Input;
using SharpCraft.block;
using SharpCraft.util;

namespace SharpCraft.entity
{
    public class EntityPlayerSP : Entity
    {
        private float maxMoveSpeed = 0.245f;
        private float moveSpeedMult = 1;

        private Vector2 moveSpeed;
        
        public int equippedItemHotbarIndex { get; private set; }

        public ItemStack[] hotbar { get; }

        public EntityPlayerSP(Vector3 pos) : base(pos)
        {
            Game.Instance.Camera.pos = (pos += Vector3.UnitY);

            collisionBoundingBox = new AxisAlignedBB(new Vector3(-0.3f, 0, -0.3f), new Vector3(0.3f, 1.65f, 0.3f));
            boundingBox = collisionBoundingBox.offset(lastPos = pos);

            hotbar = new ItemStack[9];
        }

        public EntityPlayerSP() : this(Vector3.Zero)
        {
        }

        public override void Update()
        {
            if (Game.Instance.Focused)
                UpdateCameraMovement();

            base.Update();
        }

        public override void Render(float particalTicks)
        {
            var interpolatedPos = lastPos + (pos - lastPos) * particalTicks;

            Game.Instance.Camera.pos = interpolatedPos + Vector3.UnitY * 1.625f;
        }

        private void UpdateCameraMovement()
        {
            if (Game.Instance.GuiScreen != null)
                return;

            var state = Game.Instance.KeysDown;

            Vector2 dirVec = Vector2.Zero;

            var w = state.Contains(Key.W); //might use this later
            var s = state.Contains(Key.S);
            var a = state.Contains(Key.A);
            var d = state.Contains(Key.D);

            if (w) dirVec += Game.Instance.Camera.forward;
            if (s) dirVec += -Game.Instance.Camera.forward;
            if (a) dirVec += Game.Instance.Camera.left;
            if (d) dirVec += -Game.Instance.Camera.left;

            float mult = 1;

            if (state.Contains(Key.LShift))
                mult = 1.5f;

            if (dirVec != Vector2.Zero)
            {
                moveSpeedMult = MathHelper.Clamp(moveSpeedMult + 0.085f, 1, 1.75f);

                moveSpeed = MathUtil.Clamp(moveSpeed + dirVec.Normalized() * 0.1f * moveSpeedMult, 0, maxMoveSpeed);// * moveSpeed;

                Console.WriteLine(moveSpeed.LengthFast.ToString("F"));
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
            hotbar[equippedItemHotbarIndex] = stack;
        }

        public ItemStack getEquippedItemStack()
        {
            return hotbar[equippedItemHotbarIndex];
        }

        public void setSelectedSlot(int index)
        {
            equippedItemHotbarIndex = index % 9;
        }

        public void selectNextItem()
        {
            equippedItemHotbarIndex = (equippedItemHotbarIndex + 1) % 9;
        }

        public void selectPreviousItem()
        {
            if (equippedItemHotbarIndex <= 0)
                equippedItemHotbarIndex = 8;
            else
                equippedItemHotbarIndex = equippedItemHotbarIndex - 1;
        }
    }

    [Serializable]
    public abstract class Item
    {
        public static bool operator ==(Item i1, Item i2)
        {
            return i1?.item == i2?.item;
        }

        public static bool operator !=(Item i1, Item i2)
        {
            return i1?.item != i2?.item;
        }

        public object item { get; }

        private string displayName { get; }

        protected Item(string displayName, object item)
        {
            this.item = item;
            this.displayName = displayName;
        }
    }

    [Serializable]
    internal class ItemBlock : Item
    {
        public ItemBlock(EnumBlock block) : base(block.ToString(), block)
        {
        }

        public EnumBlock getBlock()
        {
            return (EnumBlock)item;
        }
    }

    [Serializable]
    public class ItemStack
    {
        public Item Item;

        public int Count = 1;
        public int Meta;

        public ItemStack(Item item)
        {
            Item = item;
        }

        public ItemStack(Item item, int count) : this(item)
        {
            Count = count;
        }

        public ItemStack(Item item, int count, int meta) : this(item, count)
        {
            Meta = meta;
        }
    }
}