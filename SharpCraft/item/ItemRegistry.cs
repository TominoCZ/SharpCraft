using System;
using SharpCraft.model;
using System.Collections.Generic;
using System.Linq;
using SharpCraft.block;
#pragma warning disable 618

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

        [Obsolete("Use GetItem(modid, name)")]
        public static Item GetItem(string unlocalizedName)
        {
            Registry.TryGetValue(unlocalizedName, out Item item);

            return item;
        }

        public static Item GetItem(string modid, string name)
        {
            return GetItem(modid + ".item." + name);
        }
        
        public static ItemBlock GetItem(Block block)
        {
            return GetItem(block.UnlocalizedName) is ItemBlock ib ? ib : null;
        }
    }
}