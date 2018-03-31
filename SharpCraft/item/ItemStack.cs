using System;

namespace SharpCraft.item
{
    [Serializable]
    public class ItemStack
    {
        public Item Item;

        public int Count;
        public int Meta;

        public bool IsEmpty => Count <= 0 || Item == null || Item.item == null;

        public ItemStack(Item item, int count = 1, int meta = 0)
        {
            Item = item;
            Meta = meta;

            Count = count;
        }
    }
}