using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.GameDesign.Loot
{
	public class Lotttable : List<LoottableEntry>
	{
		private LoottableEntry PickEntry(Random random)
		{
			int maxPickTable = this.Sum(x => x.Weight);
			int pickTable = new Random().Next(0, maxPickTable);

			for (int i = 0; i < this.Count; i++)
			{
				if (pickTable < this[i].Weight)
					return this[i];

				pickTable -= this[i].Weight;
			}

			return this.LastOrDefault();
		}

		public Dictionary<string, int> GetLoot()
		{
			Dictionary<string, int> result = new Dictionary<string, int>();
			Random random = new Random();
			var table = PickEntry(random);
			if (table == null)
				return result;

			for (int i = 0; i < table.Loots.Count; i++)
			{
				var itemDrop = table.Loots[i];

				int amount = GetAmount(random, itemDrop);

				AddToDrop(result, itemDrop, amount);
			}

			return result;
		}

		private static void AddToDrop(Dictionary<string, int> result, LoottableEntryItemDrop itemDrop, int amount)
		{
			if (amount > 0)
			{
				if (result.ContainsKey(itemDrop.ItemId))
				{
					result[itemDrop.ItemId] += amount;
				}
				else
				{
					result.Add(itemDrop.ItemId, amount);
				}
			}
		}

		private static int GetAmount(Random random, LoottableEntryItemDrop itemDrop)
		{
			if (itemDrop.MinAmount == itemDrop.MaxAmount)
			{
				return itemDrop.MinAmount;
			}

			// max+1 because of exclusive max
			return random.Next(itemDrop.MinAmount, itemDrop.MaxAmount + 1);
		}
	}
}
