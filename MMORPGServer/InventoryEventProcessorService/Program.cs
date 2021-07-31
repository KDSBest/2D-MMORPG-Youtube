using CommonServer.CosmosDb;
using CommonServer.CosmosDb.Model;
using CommonServer.CosmosDb.ReadModelHandler;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryEventProcessorService
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var readModelHandler = new InventoryReadModelHandler();
			var instanceName = Guid.NewGuid().ToString("N");
			var processor = CosmosClientSinglton.Instance.GetInventoryEventChangeFeedProcessor<InventoryEvent>(instanceName, "InventoryEventAggregator", readModelHandler.ChangeHandler);
			await processor.StartAsync();

			Console.WriteLine($"Processor Started '{instanceName}'.");

			while(true)
			{
				Thread.Sleep(100);
			}
		}
	}
}
