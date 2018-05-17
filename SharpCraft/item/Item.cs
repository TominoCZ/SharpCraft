using System;
using System.Collections.Generic;

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

        public override bool Equals(object obj)
        {
            var item = obj as Item;
            return item != null &&
                   EqualityComparer<dynamic>.Default.Equals(InnerItem, item.InnerItem) &&
                   DisplayName == item.DisplayName;
        }

        public override int GetHashCode()
        {
            var hashCode = 1145637622;
            hashCode = hashCode * -1521134295 + EqualityComparer<dynamic>.Default.GetHashCode(InnerItem);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DisplayName);
            return hashCode;
        }
    }
}