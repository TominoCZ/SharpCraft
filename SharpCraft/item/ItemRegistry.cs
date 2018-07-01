using SharpCraft.model;
using System.Collections.Generic;
using System.Linq;

namespace SharpCraft.item
{
    internal class ItemRegistry //TODO
    {
        private static readonly Dictionary<string, Item> Registry = new Dictionary<string, Item>();

        public void Put(Item i)
        {
            Registry.Add(i.UnlocalizedName, i);
        }

        public void RegisterItemsPost(JsonModelLoader loader)
        {
            loader.LoadItems();
        }

        public static List<Item> AllItems()
        {
            return Registry.Values.ToList();
        }

        public static Item GetItem(string unlocalizedName)
        {
            Registry.TryGetValue(unlocalizedName, out Item item);

            return item;
        }
    }
}