using Common.Protocol.Inventory;

namespace Common.GameDesign.Loot
{
	public class LoottableEntryItemDrop
	{
		public int MinAmount = 1;
		public int MaxAmount = 1;

		public string ItemId = InventoryItemIds.Coins;
	}
}
