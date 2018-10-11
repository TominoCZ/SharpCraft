using System;
using SharpCraft_Client.item;

namespace SharpCraft_Client
{
    internal class RecipeRegistryEventArgs : EventArgs
    {
        private readonly Action<Item[], ItemStack, bool> _funcRegisterRecipe;

        public RecipeRegistryEventArgs(RecipeRegistry recipeRegistry)
        {
            _funcRegisterRecipe = recipeRegistry.RegisterRecipe;
        }

        public void Register(Item[] items, ItemStack product, bool shapeless)
        {
            _funcRegisterRecipe(items, product, shapeless);
        }

        public void Register(Item[] items, Item product, bool shapeless)
        {
            _funcRegisterRecipe(items, new ItemStack(product), shapeless);
        }
    }
}