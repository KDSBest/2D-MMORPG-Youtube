using CommonServer.CosmosDb.Model;

namespace CommonServer.CosmosDb
{
	public class InventoryEventRepository : CosmosDbRepository<InventoryEvent>
    {
        public InventoryEventRepository() : base(CosmosClientSinglton.Instance.InventoryEventContainer.Value)
        {

        }
    }
}
