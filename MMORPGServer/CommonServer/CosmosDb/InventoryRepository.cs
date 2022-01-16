using CommonServer.CosmosDb.Model;
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
			var inventory = await this.GetAsync(playerId);
			var cInv = new Common.Protocol.Inventory.Inventory();

			if (inventory != null)
				cInv.Items = inventory.Items;

			return cInv;
		}
	}
}
