namespace SharpCraft.item
{
    public interface IItem
    {
        int GetMaxStackSize();
        string GetUnlocalizedName();
        string GetDisplayName();
    }
}