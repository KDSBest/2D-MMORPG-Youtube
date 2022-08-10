using CommonServer.CosmosDb.Model;
using System;
using System.Threading.Tasks;

namespace CommonServer.CosmosDb
{
	public class InventoryRepository : CosmosDbRepository<Inventory>
	{
		public InventoryRepository() : base(CosmosClientSinglton.Instance.InventoryContainer.Value)
		{
		}

		public async Task<Common.Protocol.Inventory.Inventory> GetClientInventoryAsync(string playerId)
		{
			return await GetClientInventoryAsync(playerId, DateTime.MinValue);
		}

		public async Task<Common.Protocol.Inventory.Inventory> GetClientInventoryAsync(string playerId, DateTime leaseTime)
		{
			var inventory = await this.GetAsync(playerId);

			if (leaseTime != DateTime.MinValue && inventory.LatestEventTimestamp < leaseTime)
				throw new InventoryNotUpdatedException();

			var cInv = new Common.Protocol.Inventory.Inventory();

			if (inventory != null)
				cInv.Items = inventory.Items;

			return cInv;
		}
	}
}
