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

		public static Dictionary<EnemyType, Lotttable> Prop = new Dictionary<EnemyType, Lotttable>()
		{
			{
				EnemyType.Flower,
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
			}, 
			{
				EnemyType.Boss1,
				new Lotttable
				{
					new LoottableEntry()
					{
						Loots = new List<LoottableEntryItemDrop>()
						{
							new LoottableEntryItemDrop()
							{
								MinAmount = 2,
								MaxAmount = 4,
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
								MinAmount = 10,
								MaxAmount = 100,
								ItemId = InventoryItemIds.Coins
							}
						}
					}
				}
			}
		};
	}
}
