using System;

namespace SharpCraft.item
{
    [Serializable]
    public abstract class Item : IItem
    {
        private string _unlocalizedName { get; }

        public Item(string unlocalizedItem)
        {
            _unlocalizedName = unlocalizedItem;
        }

        public int GetMaxStackSize()
        {
            return 256;
        }

        public string GetUnlocalizedName()
        {
            return _unlocalizedName;
        }

        public string GetDisplayName()
        {
            return GetUnlocalizedName();
        }
    }
}