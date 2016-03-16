using System;
using Microsoft.Azure.WebJobs;
using JobsCommon.Logger;
using Microsoft.Azure.Documents;
using JobsCommon;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client.TransientFaultHandling;
using Microsoft.WindowsAzure.Storage.Table;
using SensorStateStats.Models;
using System.Linq;
using System.Collections.Generic;

namespace SensorStateStats
{
    public class Functions
    {
        private static readonly TimeSpan INTERVAL_CHECK = TimeSpan.FromMinutes(1);

        private static readonly Uri _clientsCollectionUri = new Uri($"dbs/{Configurations.DocumentDbName}/colls/clients", UriKind.Relative);
        private static readonly Uri _statsCollectionUri = new Uri($"dbs/{Configurations.DocumentDbName}/colls/stats-sensor-states", UriKind.Relative);

        private static ILogger _logger = new ConsoleLogger();

        [NoAutomaticTrigger]
        public static async Task ProcessSensorsState(CancellationToken token)
        {
            _logger.Log("Started processing history");

            var documentClient = ServiceClientFactory.GetDocumentClient();
            var storageClient = ServiceClientFactory.GetStorageClient();

            _logger.Log($"Clients initialized => starting loop with interval {INTERVAL_CHECK}");

            while (!token.IsCancellationRequested)
            {
                var watch = new Stopwatch();
                watch.Start();
                var recordsGenerated = false;
                try
                {
                    recordsGenerated = await generateHourlyStatsRecords(documentClient, storageClient);
                }
                catch (Exception ex)
                {
                    _logger.Log($"Error occurred in checking clients state: {ex.Message}");
                }

                if (recordsGenerated)
                    _logger.Log($"Generated stats records within {watch.ElapsedMilliseconds}ms");

                await Task.Delay(INTERVAL_CHECK, token);
            }

            _logger.Log($"Stopped processing history");
        }

        private static async Task<bool> generateHourlyStatsRecords(IReliableReadWriteDocumentClient dbClient, CloudTableClient tableClient)
        {
            var metaClients = getMetaClients(dbClient);
            var sensorsTableRef = tableClient.GetTableReference(Configurations.HISTORY_TABLE_SENSORS_STATE);
            var clientsTableRef = tableClient.GetTableReference(Configurations.HISTORY_TABLE_CLIENTS_STATE);

            var recordsGenerated = false;

            foreach (var metaClient in metaClients)
            {
                foreach (var metaSensor in metaClient.Sensors)
                {
                    var lastStat = getLastStat(dbClient, metaClient.ClientId, metaSensor.sensorId);
                    


                }
            }

            return recordsGenerated;
        }

        private static List<Client> getMetaClients(IReliableReadWriteDocumentClient dbClient)
        {
            return dbClient
                .CreateDocumentQuery<Client>(
                    _clientsCollectionUri,
                    new SqlQuerySpec("SELECT * FROM c WHERE c.isDisabled = false"))
                .ToList();
        }

        private static StatsSensorState getLastStat(IReliableReadWriteDocumentClient dbClient, Guid clientId, int sensorId)
        {
            var _query = new SqlQuerySpec()
            {
                QueryText = "SELECT TOP 1 * FROM s WHERE s.clientId = @clientId AND s.sensorId = @sensorId ORDER BY s.timeStampHourResolution DESC",
                Parameters = new SqlParameterCollection {
                    new SqlParameter("@clientId", clientId),
                    new SqlParameter("@sensorId", sensorId)
                }
            };

            return dbClient.CreateDocumentQuery<StatsSensorState>(_statsCollectionUri, _query)
                .ToList()
                .SingleOrDefault();
        }

        private static void foo(CloudTable clientsTableRef, CloudTable sensorsTableRef, Guid clientId, int sensorId, DateTime from, DateTime to)
        {
            var ticksFrom = from.Ticks.ToString("d19");
            var ticksTo = to.Ticks.ToString("d19");

            var clientsQuery = new StorageRangeQuery<ClientStateHistory>(clientId.ToString(), ticksFrom, ticksTo);
            var clientHistory = clientsQuery.ExecuteOn(clientsTableRef);

            var sensorsQuery = new StorageRangeQuery<SensorStateHistory>($"{clientId}-{sensorId}", ticksFrom, ticksTo);
            var sensorsHistory = clientsQuery.ExecuteOn(sensorsTableRef);
        }
    }
}
