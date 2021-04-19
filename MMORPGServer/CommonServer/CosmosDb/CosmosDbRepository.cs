using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace CommonServer.CosmosDb
{
    public abstract class CosmosDbRepository<T> where T : class
    {
        protected Container Container;

        protected CosmosDbRepository(Container container)
        {
            this.Container = container;
        }

        public async Task<T> GetAsync(string key)
        {

            try
            {
                var result = await Container.ReadItemAsync<T>(key, new PartitionKey(key));
                return result.Resource;
            }
            catch (CosmosException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
            }

            return null;
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await GetAsync(key) != null;
        }

        public async Task<bool> CreateAsync(T obj, string partitionKey)
        {
            try
            {
                await Container.CreateItemAsync(obj, new PartitionKey(partitionKey));
            }
            catch (CosmosException)
            {
                return false;
            }

            return true;
        }

        public async Task SaveAsync(T obj, string partitionKey)
        {
            await Container.UpsertItemAsync(obj, new PartitionKey(partitionKey));
        }
    }
}
