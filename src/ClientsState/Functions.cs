using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using System.Diagnostics;
using Microsoft.Azure.Documents.Client.TransientFaultHandling;
using Microsoft.Azure.Documents.Client;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Microsoft.Azure.Documents;
using ClientsState.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceBus;
using Newtonsoft.Json;
using System.Text;
using System.Runtime.Caching;
using System.Collections.Generic;

namespace ClientsState
{
    public class Functions
    {
        private const string CACHE_KEY_META_CLIENTS = "__meta_clients";
        private static readonly TimeSpan CACHE_TTL_META_CLIENTS = TimeSpan.FromMinutes(1);

        private static readonly TimeSpan INTERVAL_CHECK = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan INTERVAL_SWITCH_STATE = TimeSpan.FromMinutes(1);

        private const int RETRY_COUNT = 3;
        private static readonly TimeSpan RETRY_INTERVAL = TimeSpan.FromMilliseconds(500);

        private static readonly Uri _collectionUri = new Uri($"dbs/{(Program.Conf("DOCUMENT_DB_NAME") ?? "development")}/colls/clients", UriKind.Relative);
        private static readonly SqlQuerySpec _query = new SqlQuerySpec("SELECT * FROM c WHERE c.isDisabled = false");

        private const string TABLE_CLIENTS_STATE = "ClientsState";

        private const string TOPIC_CLIENTS_STATE = "client-state-changed";

        [NoAutomaticTrigger]
        public static async Task MonitorClientsState()
        {
            l("Started monitoring of clients state");

            var documentClient = getDocumentClient(Program.Conf("DOCUMENT_DB_ENDPOINT"), Program.Conf("DOCUMENT_DB_ACCESS_KEY"));
            var storageClient = getStorageClient(Program.Conf("STORAGE_CONNECTION_STRING") ?? "UseDevelopmentStorage=true");
            var topicsClient = getTopicClient(Program.Conf("SERVICEBUS_CONNECTION_STRING"));

            l($"Clients initialized => starting loop with interval {INTERVAL_CHECK}");

            while (true)
            {
                var watch = new Stopwatch();
                watch.Start();
                l($"Checking clients state");
                try
                {
                    await checkClientsState(documentClient, storageClient, topicsClient);
                }
                catch (Exception ex)
                {
                    l($"Error occurred in checking clients state: {ex.Message}");
                }
                l($"Finished checking clients state after {watch.Elapsed}");

                await Task.Delay(INTERVAL_CHECK);
            }
        }

        private static async Task checkClientsState(IReliableReadWriteDocumentClient dbClient, CloudTableClient tableClient, TopicClient topicClient)
        {
            var tableRef = tableClient.GetTableReference(TABLE_CLIENTS_STATE);
            var metaClients = MemoryCache.Default.Get(CACHE_KEY_META_CLIENTS) as List<Client>;
            if (metaClients == null)
            {
                metaClients = dbClient.CreateDocumentQuery<Client>(_collectionUri, _query).ToList();
                MemoryCache.Default.Add(CACHE_KEY_META_CLIENTS, metaClients, DateTimeOffset.UtcNow.Add(CACHE_TTL_META_CLIENTS));
            }

            foreach (var metaClient in metaClients)
            {
                var tableRow = await tableRef.ExecuteAsync(
                    TableOperation.Retrieve(metaClient.Id.ToString(), "LastPing"));

                if (tableRow.HttpStatusCode != 200)
                    l($"Error occurred while querying for state of {metaClient.Id}: [Status Code {tableRow.HttpStatusCode}]");
                else
                {
                    var tableRowResult = (DynamicTableEntity)tableRow.Result;
                    var lastPing = tableRowResult["TimeStamp"].DateTime.Value;
                    var now = DateTime.UtcNow;

                    if (metaClient.IsOnline && now.Add(-INTERVAL_SWITCH_STATE) > lastPing)
                        l($"Client {metaClient.Id} was ONLINE, but it's last ping occurred on {lastPing}, thus the state will be changed to OFFLINE.");
                    else if (!metaClient.IsOnline && now.Add(-INTERVAL_SWITCH_STATE) < lastPing)
                        l($"Client {metaClient.Id} was OFFLINE, but it's last ping occurred on {lastPing}, thus the state will be changed to ONLINE.");
                    else
                        continue;

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

                    using (var message = new BrokeredMessage(new MemoryStream(Encoding.UTF8.GetBytes(notification)), true))
                    {
                        await Task.WhenAll(
                            dbClient.ReplaceDocumentAsync(metaClient),
                            topicClient.SendAsync(message));
                    }
                }
            }
        }

        private static IReliableReadWriteDocumentClient getDocumentClient(string dbEndPoint, string dbAccessKey)
        {
            return new DocumentClient(new Uri(dbEndPoint), dbAccessKey)
                .AsReliable(new FixedInterval(RETRY_COUNT, RETRY_INTERVAL));
        }

        private static CloudTableClient getStorageClient(string storageConnection)
        {
            var storageAccount = CloudStorageAccount.Parse(storageConnection);
            var tableClient = storageAccount.CreateCloudTableClient();
            tableClient.DefaultRequestOptions = new TableRequestOptions()
            {
                RetryPolicy = new LinearRetry(RETRY_INTERVAL, RETRY_COUNT),
                LocationMode = LocationMode.PrimaryThenSecondary
            };

            return tableClient;
        }

        private static TopicClient getTopicClient(string sertviceBusConnection)
        {
            var client = TopicClient.CreateFromConnectionString(sertviceBusConnection, TOPIC_CLIENTS_STATE);
            client.RetryPolicy = new RetryExponential(RETRY_INTERVAL, RETRY_INTERVAL + new TimeSpan(RETRY_INTERVAL.Ticks / 2), RETRY_COUNT);
            return client;
        }

        private static void l(string message)
        {
            l(message, null);
        }

        private static void l(string message, params object[] arg)
        {
            Console.WriteLine(message, arg);
        }
    }
}
