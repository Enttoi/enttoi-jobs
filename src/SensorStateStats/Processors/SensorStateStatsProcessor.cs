using JobsCommon.Logger;
using SensorStateStats.Storage;
using System;

namespace SensorStateStats.Processors
{
    class SensorStateStatsProcessor
    {
        private ILogger _logger;
        private IClientsCollection _clientsCollection;
        private IStatsCollection _statsCollection;
        private ISensorsHistoryTable _sensorsHistory;
        private IClientsHistoryTable _clientsHistory;

        public SensorStateStatsProcessor(
            ILogger logger,
            IClientsCollection clientsCollection,
            IStatsCollection statsCollection,
            ISensorsHistoryTable sensorsHistory,
            IClientsHistoryTable clientsHistory)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (clientsCollection == null) throw new ArgumentNullException(nameof(clientsCollection));
            if (statsCollection == null) throw new ArgumentNullException(nameof(statsCollection));
            if (sensorsHistory == null) throw new ArgumentNullException(nameof(sensorsHistory));
            if (clientsHistory == null) throw new ArgumentNullException(nameof(clientsHistory));

            _clientsCollection = clientsCollection;
            _statsCollection = statsCollection;
            _sensorsHistory = sensorsHistory;
            _clientsHistory = clientsHistory;
        }

        public bool GenerateHourlyStats(DateTime now)
        {
            var recordsGenerated = false;

            foreach (var metaClient in _clientsCollection.GetClients())
            {
                foreach (var metaSensor in metaClient.Sensors)
                {
                    var lastStat = _statsCollection.GetLatestStatsRecord(metaClient.ClientId, metaSensor.sensorId);
                    DateTime fromHour; // the beginning of the hour for which stats generated

                    if (lastStat == null)
                    {
                        // no previously generated statistics for this sensor => get the very first history record
                        var historyRecord = _clientsHistory.GetOldestClientHistory(metaClient.ClientId);
                        if (historyRecord == null)
                        {
                            _logger.Log($"Client {metaClient.ClientId} doesn't have any history records");
                            break;
                        }
                        fromHour = new DateTime(
                            historyRecord.StateChangedTimestamp.Year, 
                            historyRecord.StateChangedTimestamp.Month, 
                            historyRecord.StateChangedTimestamp.Day, 
                            historyRecord.StateChangedTimestamp.Hour, 0, 0, DateTimeKind.Utc);
                    }
                    else
                    {
                        fromHour = lastStat.TimeStampHourResolution.AddHours(1);
                    }

                    var untilHour = fromHour.AddHours(1);
                    if (untilHour > now)
                    {
                        // too soon to generate stats for this hour
                        continue;
                    }

                    _logger.Log($"Going to generate stats for client {metaClient.ClientId} sensor {metaSensor.sensorId} from {fromHour} until {untilHour}");

                    //var sensorsHistory = _sensorsHistory.GetHourHistory(metaClient.ClientId, metaSensor.sensorId, fromHour);
                }
            }

            return recordsGenerated;
        }
    }
}
