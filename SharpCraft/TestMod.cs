using SharpCraft.block;

namespace SharpCraft
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

    internal class SumBlock : Block
    {
        public SumBlock() : base(Material.GetMaterial("stone"))
        {
            SetUnlocalizedName(TestMod.Instance.ModInfo.ID, "sum_block");

            IsFullCube = false;
        }
    }
}