using SharpCraft_Client.block;

namespace SharpCraft_Client
{
    internal class SumBlock : Block
    {
        public SumBlock() : base(Material.GetMaterial("stone"))
        {
            SetUnlocalizedName(TestMod.Instance.ModInfo.ID, "sum_block");

            IsFullCube = false;
        }
    }
}