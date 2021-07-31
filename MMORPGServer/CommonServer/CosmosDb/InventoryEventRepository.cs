using CommonServer.CosmosDb.Model;

namespace CommonServer.CosmosDb
{
	public class InventoryEventRepository : CosmosDbRepository<InventoryEvent>
    {
        public EventSourcingLeaseManagement LeaseManagement { get; set; }

        public InventoryEventRepository() : base(CosmosClientSinglton.Instance.InventoryEventContainer.Value)
        {
            LeaseManagement = new EventSourcingLeaseManagement(CosmosClientSinglton.Instance.InventoryEventESLeaseContainer.Value);
        }
    }
}
