using SharpCraft.block;

namespace SharpCraft
{
    internal class TestMod : ModMain
    {
        public TestMod() : base(new ModInfo("testmod", "The Test Mod", "1.0", "Me"))
        {
        }

        public override void OnItemsAndBlocksRegistry(RegistryEventArgs args)
        {
            args.Register(new BlockGrass());
        }

        public override void OnRecipeRegistry(RecipeRegistryEventArgs args)
        {
        }
    }
}