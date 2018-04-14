using System;

namespace SharpCraft.item
{

    [Serializable]
    public abstract class Item
    {
        public static bool operator ==(Item i1, Item i2)
        {
            return i1?.InnerItem == i2?.InnerItem;
        }

        public static bool operator !=(Item i1, Item i2)
        {
            return !(i1 == i2);
        }

        public dynamic InnerItem { get; }

        private string DisplayName { get; }

        protected Item(string displayName, object innerItem)
        {
            InnerItem = innerItem;
            DisplayName = displayName;
        }

        public virtual int MaxStackSize()
        {
            return 256;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}