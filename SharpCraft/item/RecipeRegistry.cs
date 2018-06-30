using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;

namespace SharpCraft.item
{
    class RecipeRegistry
    {
        private static Dictionary<Item, Item[]> _registry = new Dictionary<Item, Item[]>();

        private RecipeRegistry _instance;

        public RecipeRegistry()
        {
            if (_instance != null)
                throw new Exception("There can only be one instance of the RecipeRegitry class!");
        }

        public void RegisterRecipe(Item[] rows, Item product)
        {
            _registry.Add(product, rows);
        }

        public static Item[] GetRecipe(Item product)
        {
            _registry.TryGetValue(product, out var recipe);

            return recipe;
        }

        public static Item GetProduct(Item[] table)
        {
            foreach (var pair in _registry)
            {
                var found = true;

                Item[] arr = new Item[9];

                table.CopyTo(arr, 0);

                for (int i = 0; i < 4; i++)
                {
                    found = true;

                    for (var y = 0; y < 3; y++)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            if (pair.Value[y * 3 + x] != arr[y * 3 + x])
                                found = false;
                        }
                    }

                    if (found)
                        return pair.Key;

                    arr = Rotate(arr);
                }
            }

            return null;
        }

        private static Item[] Rotate(Item[] arr)
        {
            Item[] rotated = new Item[9];

            for (int i = 2; i >= 0; --i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    rotated[j * 3 + 2 - i] = arr[i * 3 + j];
                }
            }

            return rotated;
        }
    }
}
