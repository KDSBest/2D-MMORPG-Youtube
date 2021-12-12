using Common.QuestSystem;
using CommonServer.CosmosDb.Model;

namespace CommonServer.CosmosDb
{
	public class QuestTrackingRepository : CosmosDbRepository<QuestTracking>
    {
        public QuestTrackingRepository() : base(CosmosClientSinglton.Instance.QuestTrackingContainer.Value)
        {

        }
    }
}
