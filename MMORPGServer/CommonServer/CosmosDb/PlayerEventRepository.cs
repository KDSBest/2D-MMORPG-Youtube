using CommonServer.CosmosDb.Model;
using Microsoft.Azure.Cosmos.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommonServer.CosmosDb
{
	public class PlayerEventRepository : CosmosDbRepository<PlayerEvent>
    {
        public PlayerEventRepository() : base(CosmosClientSinglton.Instance.PlayerEventContainer.Value)
        {

        }

        public List<PlayerEvent> GetEvents(string playerId)
		{
            return this.Container.GetItemLinqQueryable<PlayerEvent>(true).Where(x => x.PlayerId == playerId).AsEnumerable().ToList();
        }

        public async Task RemoveEventAsync(PlayerEvent ev)
		{
            await this.DeleteAsync(ev.Id.ToString(), ev.PlayerId);
		}
    }
}
