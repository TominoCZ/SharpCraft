namespace SharpCraft.block
{
    internal class BlockGrass : Block
    {
        public BlockGrass() : base("grass")
        {
        }

        public override void OnRegisterStates()
        {
            RegisterState("grass");
        }
    }
}