using System;
using System.Collections.Generic;
using System.ComponentModel;
using SharpCraft.block;

namespace SharpCraft.item
{
    public abstract class Item
    {
        public string UnlocalizedName { get; }

        private Dictionary<Material, float> _materialTable = new Dictionary<Material, float>();

        protected Item(string unlocalizedItem)
        {
            UnlocalizedName = unlocalizedItem;
        }

        public virtual int GetMaxStackSize()
        {
            return 256;
        }

        public virtual float GetMiningSpeed(Material mat)
        {
            if (_materialTable.TryGetValue(mat, out float speed))
                return speed;

            return 1;
        }
    }
}