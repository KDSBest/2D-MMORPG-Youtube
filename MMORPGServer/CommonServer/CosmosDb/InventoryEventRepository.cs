using Common.GameDesign.Loot;
using Common.Protocol.PlayerEvent;
using Common.QuestSystem;
using CommonServer.CosmosDb.Model;
using System;
using System.Collections.Generic;
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

		public async Task ApplyInventoryQuestTaskAsync(string playerId, InventoryQuestTask questTask)
		{
			await SaveAsync(new InventoryEvent()
			{
				Id = Guid.NewGuid(),
				PlayerId = playerId,
				Type = PlayerEventType.Quest,
				Remove = new Dictionary<string, int>()
				{
					{ questTask.ItemId, questTask.Amount }
				}
			}, playerId);
		}

		public async Task ApplyInventoryQuestRewardAsync(string playerId, List<QuestReward> rewards)
		{
			var toAdd = new Dictionary<string, int>();
			foreach(var reward in rewards)
			{
				toAdd.Add(reward.ItemId, reward.Amount);
			}

			await SaveAsync(new InventoryEvent()
			{
				Id = Guid.NewGuid(),
				PlayerId = playerId,
				Type = PlayerEventType.Quest,
				Add = toAdd
			}, playerId);
		}
	}
}
