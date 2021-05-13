﻿using Microsoft.Azure.Cosmos;
using CommonServer.Configuration;
using System;
using System.Net.Http;

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
            UserContainer = Database.CreateContainerIfNotExistsAsync(CosmosDbConfiguration.CosmosDbUserDbCollection, "/id").Result.Container;
            CharacterContainer = Database.CreateContainerIfNotExistsAsync(CosmosDbConfiguration.CosmosDbCharacterDbCollection, "/id").Result.Container;
        }

    }
}