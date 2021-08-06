using CommonServer.CosmosDb;
using CommonServer.CosmosDb.Model;
using CommonServer.CosmosDb.ReadModelHandler;
using System;
using System.Threading.Tasks;

namespace EventProcessorService
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var readModelHandler = new PlayerInventoryReadModelHandler();
			var instanceName = Guid.NewGuid().ToString("N");
			var processor = CosmosClientSinglton.Instance.GetInventoryEventChangeFeedProcessor<InventoryEvent>(instanceName, "EventAggregator", readModelHandler.ChangeHandler);
			await processor.StartAsync();

			Console.WriteLine($"Processor Started '{instanceName}'.");

			while (true)
			{
				await Task.Delay(100);
			}

		}
	}
}
