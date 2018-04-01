﻿using System;

namespace SharpCraft.item
{
    [Serializable]
    public class ItemStack
    {
        public Item Item;

        private int _count;

        public int Count
        {
            get => _count;
            set
            {
                _count = Math.Max(value, 0);

                if (_count == 0)
                    Item = null;
            }
        }

        public int Meta;

        public bool IsEmpty => Count == 0 || Item == null || Item.item == null;

        public ItemStack(Item item, int count = 1, int meta = 0)
        {
            Item = item;
            Meta = meta;

            Count = count;
        }
    }
}