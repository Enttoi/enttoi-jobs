using System;
using Microsoft.Azure.WebJobs;
using JobsCommon.Logger;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SensorStateStats.Storage;

namespace SensorStateStats
{
    public class Functions
    {
        private static readonly TimeSpan INTERVAL_CHECK = TimeSpan.FromMinutes(1);
        private static ILogger _logger = new ConsoleLogger();

        [NoAutomaticTrigger]
        public static async Task ProcessSensorsState(CancellationToken token)
        {
            _logger.Log($"Started processing history with interval {INTERVAL_CHECK}");

            while (!token.IsCancellationRequested)
            {
                var watch = new Stopwatch();
                watch.Start();
                var recordsGenerated = false;
                try
                {
                    recordsGenerated = await generateHourlyStatsRecords();
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

        private static async Task<bool> generateHourlyStatsRecords()
        {
            var metaClients = new ClientsCollection(_logger).GetClients();
            var sensorsHistoryTable = new SensorHistoryTable(_logger);
            var clientHistoryTable = new ClientHistoryTable(_logger);
            var statsCollection = new StatsCollection(_logger);

            var recordsGenerated = false;
            var now = DateTime.UtcNow;

            foreach (var metaClient in metaClients)
            {
                foreach (var metaSensor in metaClient.Sensors)
                {
                    var lastStat = statsCollection.GetLatestStatsRecord(metaClient.ClientId, metaSensor.sensorId);
                    var fromHour = lastStat.TimeStampHourResolution.AddHours(1);
                    if (lastStat == null || fromHour < now)
                    {

                    }

                    var sensorsHistory = sensorsHistoryTable.GetHourHistory(metaClient.ClientId, metaSensor.sensorId, fromHour);
                }
            }

            return recordsGenerated;
        }        
    }
}
