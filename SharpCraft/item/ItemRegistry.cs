using System;
using System.Collections.Generic;
using System.Linq;
using SharpCraft_Client.block;
using SharpCraft_Client.json;

#pragma warning disable 618

namespace SharpCraft_Client.item
{
    public class ItemRegistry
    {
        private static readonly Dictionary<string, Item> Registry = new Dictionary<string, Item>();

        public void Put(Item i)
        {
            Registry.Add(i.UnlocalizedName, i);
        }

        public void RegisterItemsPost(JsonModelLoader loader)
        {
            loader.LoadItemModels();
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

        public static ItemStack GetItemStack(BlockState state)
        {
            return GetItem(state.Block.UnlocalizedName) is ItemBlock ib ? new ItemStack(ib, 1, state.Block.GetMetaFromState(state)) : null;
        }
    }
}