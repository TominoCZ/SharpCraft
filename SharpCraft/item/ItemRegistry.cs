using System;
using System.Collections.Generic;
using System.Linq;
using SharpCraft.render.shader;

namespace SharpCraft.item
{
    internal class ItemRegistry //TODO
    {
        private static readonly Dictionary<string, IItem> Registry = new Dictionary<string, IItem>();

        public void Put(IItem i)
        {
            var s = i.GetUnlocalizedName();

            Registry.Add(s, i);
        }

        public void RegisterItemsPost(JsonModelLoader loader)
        {
            //Item.DefaultShader = new Shader<ModelItem>("item"); TODO
        }

        public static List<IItem> AllItems()
        {
            return Registry.Values.ToList();
        }

        public static IItem GetItem(string unlocalizedName)
        {
            return Registry[unlocalizedName];
        }
    }
}