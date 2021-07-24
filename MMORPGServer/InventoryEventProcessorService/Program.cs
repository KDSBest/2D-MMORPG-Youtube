using CommonServer.CosmosDb;
using CommonServer.CosmosDb.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryEventProcessorService
{
	public class Program
	{
		private static InventoryRepository repo = new InventoryRepository();

		public static async Task Main(string[] args)
		{
			var processor = CosmosClientSinglton.Instance.GetInventoryEventChangeFeedProcessor<InventoryEvent>(Guid.NewGuid().ToString("N"), "InventoryEventAggregator", ChangeHandler);

			await processor.StartAsync();

			while(true)
			{
				Thread.Sleep(100);
			}
		}

		private static async Task ChangeHandler(IReadOnlyCollection<InventoryEvent> inventoryChanges, CancellationToken cancellationToken)
		{
			foreach(var inventoryChange in inventoryChanges)
			{
				Console.WriteLine($"Processing inventory Change {inventoryChange.Id} {inventoryChange.PlayerId}.");
				var inventory = await repo.GetAsync(inventoryChange.PlayerId);
				if (inventory == null)
				{
					inventory = new Inventory()
					{
						Id = inventoryChange.PlayerId
					};
				}

				foreach(var add in inventoryChange.Add)
				{
					if(inventory.Items.ContainsKey(add.Key))
					{
						inventory.Items[add.Key] += add.Value;
					}
					else
					{
						inventory.Items.Add(add.Key, add.Value);
					}
				}

				// TODO: error cases (they shouldn't happen here!)
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
