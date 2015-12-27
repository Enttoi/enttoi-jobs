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

        private const string COLLECTION_CLIENTS = "clients";
        private const int RETRY_COUNT = 3;
        private static readonly TimeSpan RETRY_INTERVAL = TimeSpan.FromMilliseconds(500);
        private static readonly SqlQuerySpec _query = new SqlQuerySpec("SELECT * FROM c WHERE c.isDisabled = false");

        private const string TABLE_CLIENTS_STATE = "ClientsState";

        [NoAutomaticTrigger]
        public static async Task MonitorClientsState(TextWriter log)
        {
            await log.WriteAsync("Started monitoring of clients state");

            while (true)
            {
                var watch = new Stopwatch();
                watch.Start();
                await log.WriteAsync($"Checking clients state");
                try
                {
                    await checkClientsState(
                        log,
                        Environment.GetEnvironmentVariable("DOCUMENT_DB_ENDPOINT"),
                        Environment.GetEnvironmentVariable("DOCUMENT_DB_ACCESS_KEY"),
                        Environment.GetEnvironmentVariable("DOCUMENT_DB_NAME") ?? "development",
                        Environment.GetEnvironmentVariable("STORAGE_CONNECTION_STRING") ?? "UseDevelopmentStorage=true");
                }
                catch (Exception ex)
                {
                    log.WriteLine($"Error occurred in checking clients state: {ex.Message}");
                }
                await log.WriteAsync($"Finished checking clients state after {watch.Elapsed}");

                await Task.Delay(INTERVAL_CHECK);
            }
        }

        private static async Task checkClientsState(TextWriter log, string dbEndPoint, string dbAccessKey, string dbName, string storageConnection)
        {
            var dbClient = new DocumentClient(new Uri(dbEndPoint), dbAccessKey)
                .AsReliable(new FixedInterval(RETRY_COUNT, RETRY_INTERVAL));
            var clientCollectionLink = new Uri($"dbs/{dbName}/colls/{COLLECTION_CLIENTS}", UriKind.Relative);

            var storageAccount = CloudStorageAccount.Parse(storageConnection);
            var tableClient = storageAccount.CreateCloudTableClient();
            tableClient.DefaultRequestOptions = new TableRequestOptions()
            {
                RetryPolicy = new LinearRetry(RETRY_INTERVAL, RETRY_COUNT),
                LocationMode = LocationMode.PrimaryThenSecondary
            };
            var tableRef = tableClient.GetTableReference(TABLE_CLIENTS_STATE);
            var metaClients = dbClient.CreateDocumentQuery<Client>(clientCollectionLink, _query).ToList();

            foreach (var metaClient in metaClients)
            {
                var tableRow = await tableRef.ExecuteAsync(
                    TableOperation.Retrieve(metaClient.ClientId.ToString(), "LastPing"));

                if (tableRow.HttpStatusCode != 200)
                    log.WriteLine($"Error occurred while querying for state of {metaClient.ClientId}: [Status Code {tableRow.HttpStatusCode}]");
                else
                {
                    var tableRowResult = (DynamicTableEntity)tableRow.Result;
                    if(DateTime.UtcNow.Add(-INTERVAL_WHEN_CLIENT_OFFLINE) < tableRowResult["TimeStamp"].DateTime.Value)
                    {
                        // TODO: mark client is offline (only if it was online before)
                    }
                    else
                    {
                        // TODO: make the opposite of above
                    }
                }
            }
        }
    }
}
