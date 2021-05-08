namespace CommonServer.Configuration
{
    public static class CosmosDbConfiguration
    {
#if DEBUG
        public static readonly string CosmosDbEndpointUrl = "https://localhost:8081";
#else
        public static readonly string CosmosDbEndpointUrl = "https://host.docker.internal:8081";
#endif
        public static readonly string CosmosDbKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        public static readonly string CosmosDb = "MMORPGDb";
        public static readonly string CosmosDbUserDbCollection = "UserCollection";
        public static readonly string CosmosDbCharacterDbCollection = "CharacterCollection";
    }
}
