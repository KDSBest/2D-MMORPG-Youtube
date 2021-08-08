using Microsoft.Azure.Cosmos;
using CommonServer.Configuration;
using System;
using System.Net.Http;

namespace CommonServer.CosmosDb
{
    public class CosmosClientSinglton
    {
        private static Lazy<CosmosClientSinglton> instance = new Lazy<CosmosClientSinglton>(() => new CosmosClientSinglton());

        public static CosmosClientSinglton Instance { get { return instance.Value; } }
        public static void RemoveSingleton()
		{
            CosmosClientSinglton.instance = new Lazy<CosmosClientSinglton>(() => new CosmosClientSinglton());
        }

        public Database Database { get; private set; }
        public Lazy<Container> UserContainer { get; private set; }
        public Lazy<Container> UserLastLoginContainer { get; private set; }
        public Lazy<Container> CharacterContainer { get; private set; }

        public Lazy<Container> InventoryContainer { get; private set; }
        public Lazy<Container> InventoryEventContainer { get; private set; }
        public Lazy<Container> InventoryEventLeaseContainer { get; private set; }

        public Lazy<Container> InventoryEventESLeaseContainer { get; private set; }
        public Lazy<Container> PlayerEventContainer { get; private set; }

        private CosmosClientSinglton()
        {
            // TODO: remove and clean up cert in cosmos emulator to accept host: https://host.docker.internal:8081
            CosmosClientOptions cosmosClientOptions = new CosmosClientOptions()
            {
                HttpClientFactory = () =>
                {
                    HttpMessageHandler httpMessageHandler = new HttpClientHandler()
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
                    return new HttpClient(httpMessageHandler);
                },
                ConnectionMode = ConnectionMode.Gateway
            };

            var cosmosDb = new CosmosClient(CosmosDbConfiguration.CosmosDbEndpointUrl, CosmosDbConfiguration.CosmosDbKey, cosmosClientOptions);
            Database = cosmosDb.CreateDatabaseIfNotExistsAsync(CosmosDbConfiguration.CosmosDb).Result.Database;
            UserContainer = new Lazy<Container>(() => Database.CreateContainerIfNotExistsAsync(CosmosDbConfiguration.CosmosDbUserDbCollection, "/id").Result.Container);
            UserLastLoginContainer = new Lazy<Container>(() => Database.CreateContainerIfNotExistsAsync(CosmosDbConfiguration.CosmosDbUserLastLoginDbCollection, "/id").Result.Container);
            CharacterContainer = new Lazy<Container>(() => Database.CreateContainerIfNotExistsAsync(CosmosDbConfiguration.CosmosDbCharacterDbCollection, "/id").Result.Container);
            InventoryContainer = new Lazy<Container>(() => Database.CreateContainerIfNotExistsAsync(CosmosDbConfiguration.CosmosDbInventoryDbCollection, "/id").Result.Container);
            InventoryEventContainer = new Lazy<Container>(() => Database.CreateContainerIfNotExistsAsync(CosmosDbConfiguration.CosmosDbInventoryEventDbCollection, "/playerId").Result.Container);
            InventoryEventLeaseContainer = new Lazy<Container>(() => Database.CreateContainerIfNotExistsAsync(CosmosDbConfiguration.CosmosDbInventoryEventLeaseDbCollection, "/id").Result.Container);
            InventoryEventESLeaseContainer = new Lazy<Container>(() => Database.CreateContainerIfNotExistsAsync(CosmosDbConfiguration.CosmosDbInventoryEventESLeaseDbCollection, "/id").Result.Container);
            PlayerEventContainer = new Lazy<Container>(() => Database.CreateContainerIfNotExistsAsync(new ContainerProperties() {
                Id = CosmosDbConfiguration.PlayerEventDbCollection, 
                PartitionKeyPath = "/playerId",
                DefaultTimeToLive = CosmosDbConfiguration.EventDefaultTimeToLive
            }).Result.Container);
        }

        public ChangeFeedProcessor GetInventoryEventChangeFeedProcessor<T>(string instanceName, string processorName, Container.ChangesHandler<T> action)
		{
            return InventoryEventContainer.Value.GetChangeFeedProcessorBuilder(processorName, action)
                .WithInstanceName(instanceName)
                .WithLeaseContainer(InventoryEventLeaseContainer.Value)
                .WithStartTime(DateTime.MinValue.ToUniversalTime())
                .Build();
        }

    }
}
