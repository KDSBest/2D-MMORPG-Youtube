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
        public Container LoginContainer { get; private set; }

        private CosmosClientSinglton()
        {
            var docDb = new CosmosClient(CosmosDbConfiguration.DocumentDbEndpointUrl, CosmosDbConfiguration.DocumentDbKey);
            Database = docDb.CreateDatabaseIfNotExistsAsync(CosmosDbConfiguration.DocumentDb).Result.Database;
            LoginContainer = Database.CreateContainerIfNotExistsAsync(CosmosDbConfiguration.DocumentDbLoginDbCollection, "/id").Result.Container;
        }

    }
}
