using JobsCommon;
using JobsCommon.Logger;
using Microsoft.Azure.Documents;
using SensorStateStats.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace SensorStateStats.Storage
{
    class ClientsCollection : BaseDocumentCollection, IClientsCollection
    {
        private const string CACHE_KEY_META_CLIENTS = "__meta_clients";
        private static readonly TimeSpan CACHE_TTL_META_CLIENTS = TimeSpan.FromMinutes(10);

        private readonly MemoryCache _cache;

        private static readonly Uri _clientsCollectionUri = new Uri($"dbs/{Configurations.DocumentDbName}/colls/clients", UriKind.Relative);

        public ClientsCollection(ILogger logger) : base(logger)
        {
            _cache = MemoryCache.Default;
        }

        public List<Client> GetClients()
        {
            var metaClients = _cache.Get(CACHE_KEY_META_CLIENTS) as List<Client>;
            if (metaClients == null)
            {
                metaClients = _client
                    .CreateDocumentQuery<Client>(
                        _clientsCollectionUri,
                        new SqlQuerySpec("SELECT * FROM c WHERE c.isDisabled = false"))
                    .ToList();

                _cache.Add(CACHE_KEY_META_CLIENTS, metaClients, DateTimeOffset.UtcNow.Add(CACHE_TTL_META_CLIENTS));
                _logger.Log($"{metaClients.Count} client loaded to cache");
            }
            return metaClients;
        }

    }
}
