using CommonServer.CosmosDb.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommonServer.CosmosDb.ReadModelHandler
{
	public class InventoryReadModelHandler
	{
		private InventoryRepository repo = new InventoryRepository();

		public async Task ChangeHandler(IReadOnlyCollection<InventoryEvent> inventoryChanges, CancellationToken cancellationToken)
		{
			Console.WriteLine($"Got {inventoryChanges.Count} changes to process.");
			foreach (var inventoryChange in inventoryChanges)
			{
				Console.WriteLine($"Processing inventory Change {inventoryChange.Id} {inventoryChange.PlayerId}.");
				var inventory = await repo.GetAsync(inventoryChange.PlayerId);
				if (inventory == null)
				{
					inventory = new Inventory()
					{
						Id = inventoryChange.PlayerId,
					};
				}

				if (inventory.LatestEventTimestamp < inventoryChange.CreationDate)
				{
					inventory.LatestEventTimestamp = inventoryChange.CreationDate;
				}

				foreach (var add in inventoryChange.Add)
				{
					if (inventory.Items.ContainsKey(add.Key))
					{
						inventory.Items[add.Key] += add.Value;
					}
					else
					{
						inventory.Items.Add(add.Key, add.Value);
					}
				}

				foreach (var remove in inventoryChange.Remove)
				{
					if (inventory.Items.ContainsKey(remove.Key))
					{
						inventory.Items[remove.Key] -= remove.Value;
					}
					else
					{
						Console.WriteLine("Inventory doesn't have any of those items.");
					}
				}

				await repo.SaveAsync(inventory, inventory.Id);
			}
		}
	}
}
