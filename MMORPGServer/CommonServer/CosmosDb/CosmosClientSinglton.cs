using Microsoft.Azure.Cosmos;
using CommonServer.Configuration;
using System;

namespace CommonServer.CosmosDb
{
    public class CosmosClientSinglton
    {
        private static readonly Lazy<CosmosClientSinglton> instance = new Lazy<CosmosClientSinglton>(() => new CosmosClientSinglton());

        public static CosmosClientSinglton Instance { get { return instance.Value; } }

        public Database Database { get; private set; }
        public Container UserContainer { get; private set; }
        public Container CharacterContainer { get; private set; }

        private CosmosClientSinglton()
        {
            var cosmosDb = new CosmosClient(CosmosDbConfiguration.CosmosDbEndpointUrl, CosmosDbConfiguration.CosmosDbKey);
            Database = cosmosDb.CreateDatabaseIfNotExistsAsync(CosmosDbConfiguration.CosmosDb).Result.Database;
            UserContainer = Database.CreateContainerIfNotExistsAsync(CosmosDbConfiguration.CosmosDbUserDbCollection, "/id").Result.Container;
            CharacterContainer = Database.CreateContainerIfNotExistsAsync(CosmosDbConfiguration.CosmosDbCharacterDbCollection, "/id").Result.Container;
        }

    }
}
