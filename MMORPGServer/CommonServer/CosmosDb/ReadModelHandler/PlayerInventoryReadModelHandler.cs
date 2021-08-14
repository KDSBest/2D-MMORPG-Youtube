using Common.Protocol.PlayerEvent;
using CommonServer.CosmosDb.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommonServer.CosmosDb.ReadModelHandler
{
	public class PlayerInventoryReadModelHandler
	{
		private PlayerEventRepository repo = new PlayerEventRepository();

		public async Task ChangeHandler(IReadOnlyCollection<InventoryEvent> inventoryChanges, CancellationToken cancellationToken)
		{
			foreach (var inventoryChange in inventoryChanges)
			{
				var ev = new PlayerEvent()
				{
					CreationDate = inventoryChange.CreationDate,
					Id = inventoryChange.Id,
					PlayerId = inventoryChange.PlayerId,
					Type = inventoryChange.Type,
					Add = inventoryChange.Add,
					Remove = inventoryChange.Remove
				};
				await repo.SaveAsync(ev, ev.PlayerId);
			}
		}
	}
}
