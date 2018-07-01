using SharpCraft.block;

namespace SharpCraft.item
{
    public class ItemPickaxe : Item
    {
        private readonly string _pickaxeMaterial;

        public ItemPickaxe(string type)
        {
            SetUnlocalizedName("sharpcraft", "pick_" + type);
            _pickaxeMaterial = type;
        }

        public override float GetMiningSpeed(Material mat)
        {
            float mult = 1f;

            switch (_pickaxeMaterial)
            {
                case "rare":
                    mult = 8f;
                    break;

                case "stone":
                    mult = 2.5f;
                    break;

                case "wood":
                    mult = 1.5f;
                    break;
            }

            switch (mat.Name)
            {
                case "stone":
                    return mult;
            }

            return 1;
        }

        public override int GetMaxStackSize()
        {
            return 1;
        }
    }
}