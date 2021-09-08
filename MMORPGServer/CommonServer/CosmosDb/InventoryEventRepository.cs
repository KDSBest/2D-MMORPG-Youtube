using Common.GameDesign.Loot;
using Common.Protocol.PlayerEvent;
using CommonServer.CosmosDb.Model;
using System;
using System.Threading.Tasks;

namespace CommonServer.CosmosDb
{
	public class InventoryEventRepository : CosmosDbRepository<InventoryEvent>
    {
        public EventSourcingLeaseManagement LeaseManagement { get; set; }

        public InventoryEventRepository() : base(CosmosClientSinglton.Instance.InventoryEventContainer.Value)
        {
            LeaseManagement = new EventSourcingLeaseManagement(CosmosClientSinglton.Instance.InventoryEventESLeaseContainer.Value);
        }

		public async Task GiveLoot(string playerId, Lotttable loottable, PlayerEventType type)
		{
			await SaveAsync(new InventoryEvent()
			{
				Id = Guid.NewGuid(),
				PlayerId = playerId,
				Type = type,
				Add = loottable.GetLoot()
			}, playerId);
		}
	}
}
