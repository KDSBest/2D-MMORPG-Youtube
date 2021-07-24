using CommonServer.CosmosDb.Model;

namespace CommonServer.CosmosDb
{
	public class InventoryRepository : CosmosDbRepository<Inventory>
    {
        public InventoryRepository() : base(CosmosClientSinglton.Instance.InventoryContainer.Value)
        {

        }
    }
}
