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

namespace ClientsState
{
    public class Functions
    {
        private static readonly TimeSpan INTERVAL_CHECK = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan INTERVAL_WHEN_CLIENT_OFFLINE = TimeSpan.FromMinutes(1);

        private const int RETRY_COUNT = 3;
        private static readonly TimeSpan RETRY_INTERVAL = TimeSpan.FromMilliseconds(500);
        
        private static readonly Uri _collectionUri = new Uri($"dbs/{(getConfig("DOCUMENT_DB_NAME") ?? "development")}/colls/clients", UriKind.Relative);
        private static readonly SqlQuerySpec _query = new SqlQuerySpec("SELECT * FROM c WHERE c.isDisabled = false");

        private const string TABLE_CLIENTS_STATE = "ClientsState";

        [NoAutomaticTrigger]
        public static async Task MonitorClientsState(TextWriter log)
        {
            await log.WriteLineAsync("Started monitoring of clients state");

            var documentClient = getDocumentClient(getConfig("DOCUMENT_DB_ENDPOINT"), getConfig("DOCUMENT_DB_ACCESS_KEY"));
            var storageClient = getStorageClient(getConfig("STORAGE_CONNECTION_STRING") ?? "UseDevelopmentStorage=true");
            
            await log.WriteLineAsync($"Clients initialized => starting loop with interval {INTERVAL_CHECK}");

            while (true)
            {
                var watch = new Stopwatch();
                watch.Start();
                await log.WriteAsync($"Checking clients state");
                try
                {
                    await checkClientsState(log, documentClient, storageClient);
                }
                catch (Exception ex)
                {
                    log.WriteLine($"Error occurred in checking clients state: {ex.Message}");
                }
                await log.WriteAsync($"Finished checking clients state after {watch.Elapsed}");

                await Task.Delay(INTERVAL_CHECK);
            }
        }

        private static async Task checkClientsState(TextWriter log, IReliableReadWriteDocumentClient dbClient, CloudTableClient tableClient)
        {
            var tableRef = tableClient.GetTableReference(TABLE_CLIENTS_STATE);
            var metaClients = dbClient.CreateDocumentQuery<Client>(_collectionUri, _query).ToList();

            foreach (var metaClient in metaClients)
            {
                var tableRow = await tableRef.ExecuteAsync(
                    TableOperation.Retrieve(metaClient.Id.ToString(), "LastPing"));

                if (tableRow.HttpStatusCode != 200)
                    log.WriteLine($"Error occurred while querying for state of {metaClient.Id}: [Status Code {tableRow.HttpStatusCode}]");
                else
                {
                    var tableRowResult = (DynamicTableEntity)tableRow.Result;
                    var lastPing = tableRowResult["TimeStamp"].DateTime.Value;
                    if (metaClient.IsOnline && DateTime.UtcNow.Add(-INTERVAL_WHEN_CLIENT_OFFLINE) > lastPing)
                        await log.WriteAsync($"Client {metaClient.Id} was ONLINE, but it's last ping occurred on {lastPing}, thus the state will be changed to OFFLINE.");
                    else if (!metaClient.IsOnline && DateTime.UtcNow.Add(-INTERVAL_WHEN_CLIENT_OFFLINE) < lastPing)
                        await log.WriteAsync($"Client {metaClient.Id} was OFFLINE, but it's last ping occurred on {lastPing}, thus the state will be changed to ONLINE.");
                    else
                        continue;

                    metaClient.IsOnline = !metaClient.IsOnline;
                    metaClient.IsOnlineChanged = DateTime.UtcNow;
                    await dbClient.ReplaceDocumentAsync(metaClient);
                    
                    // TODO: send notification

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

        private static string getConfig(string key) => Environment.GetEnvironmentVariable(key);
    }
}
