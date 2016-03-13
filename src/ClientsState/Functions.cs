using ClientsState.Models;
using JobsCommon;
using JobsCommon.Logger;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client.TransientFaultHandling;
using Microsoft.Azure.WebJobs;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientsState
{
    public class Functions
    {
        private const string CACHE_KEY_META_CLIENTS = "__meta_clients";
        private static readonly TimeSpan CACHE_TTL_META_CLIENTS = TimeSpan.FromMinutes(1);

        private static readonly TimeSpan INTERVAL_CHECK = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan INTERVAL_SWITCH_STATE = TimeSpan.FromMinutes(1);

        private static readonly Uri _collectionUri = new Uri($"dbs/{Configurations.DocumentDbName}/colls/clients", UriKind.Relative);
        private static readonly SqlQuerySpec _queryAllClients = new SqlQuerySpec("SELECT * FROM c WHERE c.isDisabled = false");

        private const string TABLE_CLIENTS_STATE = "ClientsState";
        private const string TOPIC_CLIENTS_STATE = "client-state-changed";

        private static ILogger _logger = new ConsoleLogger();

        [NoAutomaticTrigger]
        public static async Task MonitorClientsState(CancellationToken token)
        {
            _logger.Log("Started monitoring of clients state");

            var documentClient = ServiceClientFactory.GetDocumentClient();
            var storageClient = ServiceClientFactory.GetStorageClient();
            var topicsClient = ServiceClientFactory.GetTopicClient(TOPIC_CLIENTS_STATE);

            _logger.Log($"Clients initialized => starting loop with interval {INTERVAL_CHECK}");
            
            while (!token.IsCancellationRequested)
            {
                var watch = new Stopwatch();
                watch.Start();
                var updatesPerformed = false;
                try
                {
                    updatesPerformed = await checkClientsState(documentClient, storageClient, topicsClient);
                }
                catch (Exception ex)
                {
                    _logger.Log($"Error occurred in checking clients state: {ex.Message}");
                }

                if (updatesPerformed)
                    _logger.Log($"Performed update/s within {watch.ElapsedMilliseconds}ms");

                await Task.Delay(INTERVAL_CHECK, token);
            }

            _logger.Log($"Stopped monitoring of clients state");
        }

        private static async Task<bool> checkClientsState(IReliableReadWriteDocumentClient dbClient, CloudTableClient tableClient, TopicClient topicClient)
        {
            var tableRef = tableClient.GetTableReference(TABLE_CLIENTS_STATE);
            var metaClients = MemoryCache.Default.Get(CACHE_KEY_META_CLIENTS) as List<Client>;

            if (metaClients == null)
            {
                metaClients = dbClient.CreateDocumentQuery<Client>(_collectionUri, _queryAllClients).ToList();
                MemoryCache.Default.Add(CACHE_KEY_META_CLIENTS, metaClients, DateTimeOffset.UtcNow.Add(CACHE_TTL_META_CLIENTS));
                _logger.Log($"{metaClients.Count} client loaded to cache");
            }

            bool updatesPerformed = false; //signals to invalidate cache

            foreach (var metaClient in metaClients)
            {
                var tableRow = await tableRef.ExecuteAsync(
                    TableOperation.Retrieve(metaClient.Id.ToString(), "LastPing"));

                if (tableRow.HttpStatusCode != 200)
                    _logger.Log($"Error occurred while querying for state of {metaClient.Id}: [Status Code {tableRow.HttpStatusCode}]");
                else
                {
                    var tableRowResult = (DynamicTableEntity)tableRow.Result;
                    var lastPing = tableRowResult["TimeStamp"].DateTime.Value;
                    var now = DateTime.UtcNow;

                    if (!isStateChanged(metaClient, lastPing, now))
                        continue;

                    updatesPerformed = true;
                    var previousStateDuration = now - metaClient.IsOnlineChanged;

                    metaClient.IsOnline = !metaClient.IsOnline;
                    metaClient.IsOnlineChanged = now;

                    var notification = JsonConvert.SerializeObject(new
                    {
                        clientId = metaClient.Id,
                        newState = metaClient.IsOnline,
                        previousStateDurationMs = previousStateDuration.TotalMilliseconds,
                        timestamp = now
                    });

                    try
                    {
                        using (var message = new BrokeredMessage(new MemoryStream(Encoding.UTF8.GetBytes(notification)), true))
                        {
                            await Task.WhenAll(
                                dbClient.ReplaceDocumentAsync(metaClient),
                                topicClient.SendAsync(message));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Failed to update state and send notification {ex}");
                    }
                }
            }

            if (updatesPerformed)
                MemoryCache.Default.Remove(CACHE_KEY_META_CLIENTS);

            return updatesPerformed;
        }

        private static bool isStateChanged(Client metaClient, DateTime lastPing, DateTime now)
        {
            if (metaClient.IsOnline && now.Add(-INTERVAL_SWITCH_STATE) > lastPing)
                _logger.Log($"Client {metaClient.Id} was ONLINE, but it's last ping occurred on {lastPing}, thus the state will be changed to OFFLINE.");
            else if (!metaClient.IsOnline && now.Add(-INTERVAL_SWITCH_STATE) < lastPing)
                _logger.Log($"Client {metaClient.Id} was OFFLINE, but it's last ping occurred on {lastPing}, thus the state will be changed to ONLINE.");
            else
                return false;
            return true;
        }
    }
}
