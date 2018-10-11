using System;
using System.Collections.Concurrent;
using System.Linq;

namespace SharpCraft_Client.item
{
    public class RecipeRegistry
    {
        private static readonly BlockingCollection<Recipe> _registry = new BlockingCollection<Recipe>();

        private static RecipeRegistry _instance;

        public RecipeRegistry()
        {
            if (_instance != null)
                throw new Exception("There can only be one instance of the RecipeRegitry class!");

            _instance = this;
        }

        public void RegisterRecipe(Item[] rows, ItemStack product, bool shapeless = false)
        {
            if (rows == null || rows.Length != 9 || product == null || product.IsEmpty || rows.All(item => item == null))
                return;

            _registry.Add(new Recipe(product, rows, shapeless));
        }

        public void RegisterRecipe(Item[] rows, Item product, bool shapeless = false)
        {
            RegisterRecipe(rows, new ItemStack(product), shapeless);
        }

        public static ItemStack GetProduct(Item[] table)
        {
            foreach (var recipe in _registry)
            {
                if (recipe.Matches(table))
                    return recipe.Product.Copy();
            }

            return null;
        }

        private struct Recipe
        {
            public readonly ItemStack Product;

            public readonly Item[] Items;

            public readonly bool Shapeless;

            public Recipe(ItemStack product, Item[] recipe, bool shapeless)
            {
                Items = recipe;
                Product = product;
                Shapeless = shapeless;
            }

            public bool Matches(Item[] table)
            {
                //TODO
                /*
                   while(top row is all empty) move all items up 1 spot
                   while(left column is all empty) move all items left 1 spot
                   do the same with recipe registry entries
                   so it kinda serves as a normalise function
                */

                if (Shapeless)
                    return MatchesShapeless(table);

                Item[] arr = new Item[9];

                table.CopyTo(arr, 0);

                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        var found = true;

                        for (var y = 0; y < 3; y++)
                        {
                            for (int x = 0; x < 3; x++)
                            {
                                if (Items[y * 3 + x] != arr[y * 3 + x])
                                    found = false;
                            }
                        }

                        if (found)
                            return true;

                        arr = Rotate(arr);
                    }

                    arr = Flip(arr);
                }

                return false;
            }

            private bool MatchesShapeless(Item[] table)
            {
                var recipeItems = Items.Where(item => item != null);
                var tableItems = table.Where(item => item != null).ToList();

                foreach (var item in recipeItems)
                {
                    if (!tableItems.Remove(item))
                        return false;
                }

                return tableItems.Count == 0;
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

            private static Item[] Flip(Item[] arr)
            {
                Item[] flipped = new Item[9];

                for (int y = 0; y < 3; y++)
                {
                    for (int x = 0; x < 3; x++)
                    {
                        flipped[x + (2 - y) * 3] = arr[x + y * 3];
                    }
                }

                return flipped;
            }
        }
    }
}