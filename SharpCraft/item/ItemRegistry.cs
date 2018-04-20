using System;
using System.Collections.Generic;

namespace SharpCraft.item
{
    class ItemRegistry //TODO
    {
        private static Dictionary<string, Item> _registry = new Dictionary<string, Item>();
        private static Dictionary<Type, Item> _typeRegistry = new Dictionary<Type, Item>();

        public void Put(Item i)
        {
            _registry.Add("item_" + _typeRegistry.Count/*i.UnlocalizedName*/, i);
            _typeRegistry.Add(i.GetType(), i);
        }

        public void RegisterItems()
        {
            foreach (var value in _registry.Values)
            {
                //value.RegisterSomeShitInItemsLaterOnBoiFunc();
            }
        }

        public static Item GetItem<TItem>() where TItem : Item
        {
            return _typeRegistry[typeof(TItem)];
        }

        public static Item GetItem(string unlocalizedName)
        {
            return _registry[unlocalizedName];
        }
    }
}
