using System;
using System.Reflection.Metadata.Ecma335;

namespace SharpCraft.item
{
	[Serializable]
	public class ItemStack
	{
		public Item Item;

		private int _count;

		public int Count
		{
			get => _count;
			set
			{
			    if (Item == null)
			    {
			        _count = 0;
			        return;
			    }

				_count = Math.Clamp(value, 0, Item.MaxStackSize());

				if (_count == 0)
					Item = null;
			}
		}

		public int Meta;

		public bool IsEmpty => Count == 0 || Item?.InnerItem == null;

		public ItemStack(Item item, int count = 1, int meta = 0)
		{
			Item = item;
			Meta = meta;

			Count = count;
		}

		public ItemStack Copy()
		{
		    return Copy(Count);
		}

		public ItemStack Copy(int size)
		{
			return new ItemStack(Item, size, Meta);
		}

		public bool ItemSame(ItemStack other)
		{
			if (other == null) return false;
			return Meta == other.Meta &&
			       Item == other.Item;
		}

	    public override string ToString() => Item != null ? Item.ToString() : "";
	}
}