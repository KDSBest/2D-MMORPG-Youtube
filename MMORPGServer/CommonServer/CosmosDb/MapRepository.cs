using Common.Protocol.Map;
using CommonServer.CosmosDb.Model;
using System.Threading.Tasks;

namespace CommonServer.CosmosDb
{
	public class MapRepository : CosmosDbRepository<PlayerStateMessage>
    {
        public MapRepository() : base(CosmosClientSinglton.Instance.MapContainer.Value)
        {
		}
	}
}
