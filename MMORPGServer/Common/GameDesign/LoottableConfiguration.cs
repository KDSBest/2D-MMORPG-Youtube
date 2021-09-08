using Common.GameDesign.Loot;
using Common.Protocol.Inventory;
using System.Collections.Generic;

namespace Common.GameDesign
{
	public static class LoottableConfiguration
	{
		public static Lotttable Daily = new Lotttable
		{
			new LoottableEntry()
			{
				Loots = new List<LoottableEntryItemDrop>()
				{
					new LoottableEntryItemDrop()
					{
						MinAmount = 100,
						MaxAmount = 100,
						ItemId = InventoryItemIds.Coins
					}
				}
			}
		};

		public static Dictionary<PropType, Lotttable> Prop = new Dictionary<PropType, Lotttable>()
		{
			{
				PropType.Flower,
				new Lotttable
				{
					new LoottableEntry()
					{
						Loots = new List<LoottableEntryItemDrop>()
						{
							new LoottableEntryItemDrop()
							{
								MinAmount = 1,
								MaxAmount = 2,
								ItemId = InventoryItemIds.Flowers
							}
						}
					},
					new LoottableEntry()
					{
						Loots = new List<LoottableEntryItemDrop>()
						{
							new LoottableEntryItemDrop()
							{
								MinAmount = 1,
								MaxAmount = 1,
								ItemId = InventoryItemIds.Flowers
							},
							new LoottableEntryItemDrop()
							{
								MinAmount = 1,
								MaxAmount = 1,
								ItemId = InventoryItemIds.Coins
							}
						}
					}
				}
			}
		};
	}
}
