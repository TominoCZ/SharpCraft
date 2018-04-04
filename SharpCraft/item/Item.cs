using System;

namespace SharpCraft.item
{
	[Serializable]
	public abstract class Item
	{
		public static bool operator ==(Item i1, Item i2)
		{
			return i1?.item == i2?.item;
		}

		public static bool operator !=(Item i1, Item i2)
		{
			return i1?.item != i2?.item;
		}

		public dynamic item { get; }

		private string displayName { get; }

		protected Item(string displayName, object item)
		{
			this.item = item;
			this.displayName = displayName;
		}

		public virtual int MaxStackSize(ItemStack stack)
		{
			return 64;
		}
	}
}