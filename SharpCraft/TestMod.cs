namespace SharpCraft_Client
{
    internal class TestMod : ModMain
    {
        public static ModMain Instance;

        public SumBlock MyBlock;

        public TestMod() : base(new ModInfo("testmod", "The Test Mod", "1.0", "Me"))
        {
            Instance = this;
        }

        public override void OnItemsAndBlocksRegistry(RegistryEventArgs args)
        {
            MyBlock = new SumBlock();
            args.Register(MyBlock);
        }

        public override void OnRecipeRegistry(RecipeRegistryEventArgs args)
        {
        }
    }
}