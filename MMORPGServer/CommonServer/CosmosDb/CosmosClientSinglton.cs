using Microsoft.Azure.Cosmos;
using CommonServer.Configuration;

namespace CommonServer.CosmosDb
{
    public class CosmosClientSinglton
    {
        private static CosmosClientSinglton instance;

        private static readonly object lockObject = new object();
        public Database Database { get; private set; }
        public Container LoginContainer { get; private set; }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static CosmosClientSinglton Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            instance = new CosmosClientSinglton();
                        }
                    }
                }

                return instance;
            }
        }

        private CosmosClientSinglton()
        {
            var docDb = new CosmosClient(CosmosDbConfiguration.DocumentDbEndpointUrl, CosmosDbConfiguration.DocumentDbKey);
            Database = docDb.CreateDatabaseIfNotExistsAsync(CosmosDbConfiguration.DocumentDb).Result.Database;
            LoginContainer = Database.CreateContainerIfNotExistsAsync(CosmosDbConfiguration.DocumentDbLoginDbCollection, "/id").Result.Container;
        }

    }
}
