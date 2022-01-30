using System;

namespace CommonServer.Configuration
{
    public static class CosmosDbConfiguration
    {
#if DEBUG
        public static readonly string CosmosDbEndpointUrl = Environment.GetEnvironmentVariable("CosmosUrl") ?? "https://localhost:8081";
#else
        public static readonly string CosmosDbEndpointUrl = Environment.GetEnvironmentVariable("CosmosUrl") ?? "https://host.docker.internal:8081";
#endif
        public static readonly string CosmosDbKey = Environment.GetEnvironmentVariable("CosmosKey") ?? "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        public static readonly string CosmosDb = "MMORPGDb";
        public static readonly string CosmosDbUserDbCollection = "UserCollection";
        public static readonly string CosmosDbUserLastLoginDbCollection = "UserLastLoginCollection";
        public static readonly string CosmosDbCharacterDbCollection = "CharacterCollection";
        public static readonly string CosmosDbInventoryDbCollection = "InventoryCollection";
        public static readonly string CosmosDbMapDbCollection = "MapCollection";
        public static readonly string CosmosDbQuestTrackingDbCollection = "QuestTrackingCollection";
        public static readonly string CosmosDbInventoryEventDbCollection = "InventoryEventCollection";
        public static readonly string CosmosDbInventoryEventLeaseDbCollection = "InventoryEventLeaseCollection";
        public static readonly string CosmosDbInventoryEventESLeaseDbCollection = "InventoryEventESLeaseCollection";
        public static readonly string PlayerEventDbCollection = "PlayerEventCollection";

        public static readonly int EventDefaultTimeToLive = 60 * 60;
    }
}
