using JobsCommon.Logger;
using Newtonsoft.Json;
using SensorStateStats.Models;
using SensorStateStats.Storage;
using System;
using System.Collections.Generic;

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

            _logger = logger;
            _clientsCollection = clientsCollection;
            _statsCollection = statsCollection;
            _sensorsHistory = sensorsHistory;
            _clientsHistory = clientsHistory;
        }

        /// <summary>
        /// Generates the hourly stats for sensors.
        /// </summary>
        /// <param name="now">Current time.</param>
        /// <returns>Amount os sensors for whom stats were produced</returns>
        public int GenerateHourlyStats(DateTime now)
        {
            var recordsGenerated = 0;

            foreach (var metaClient in _clientsCollection.GetClients())
            {
                var oldestClientHistory = new Lazy<ClientStateHistory>(() =>
                    _clientsHistory.GetOldestClientHistory(metaClient.ClientId));

                foreach (var metaSensor in metaClient.Sensors)
                {
                    var result = processSingleSensor(now, metaClient, metaSensor, oldestClientHistory);
                    if (result == -1)
                        break;
                    if (result > 0)
                        recordsGenerated ++;
                }
            }

            return recordsGenerated;
        }

        /// <summary>
        /// Processes stats for single sensor.
        /// </summary>
        /// <param name="now">The current time.</param>
        /// <param name="client">The client.</param>
        /// <param name="sensor">The sensor.</param>
        /// <param name="oldestClientHistory">The oldest client history.</param>
        /// <returns>
        /// -1 should skip the entire client
        /// 0 no statistics generated
        /// >0 - generated some stats
        /// </returns>
        private int processSingleSensor(DateTime now, Client client, Sensor sensor, Lazy<ClientStateHistory> oldestClientHistory)
        {
            var lastStat = _statsCollection.GetLatestStatsRecord(client.ClientId, sensor.sensorId);
            DateTime fromHour; // the beginning of the hour for which stats generated

            if (lastStat == null)
            {
                // no previously generated statistics for this sensor => get the very first history record
                if (oldestClientHistory.Value == null)
                {
                    _logger.Log($"Client {client.ClientId} doesn't have any history records");
                    return -1;
                }
                fromHour = new DateTime(
                    oldestClientHistory.Value.StateChangedTimestamp.Year,
                    oldestClientHistory.Value.StateChangedTimestamp.Month,
                    oldestClientHistory.Value.StateChangedTimestamp.Day,
                    oldestClientHistory.Value.StateChangedTimestamp.Hour, 0, 0, DateTimeKind.Utc);
            }
            else
            {
                fromHour = lastStat.TimeStampHourResolution.AddHours(1);
            }

            if (fromHour.AddHours(1) > now)
                // too soon to generate stats for this hour
                return 0;

            _logger.Log($"Going to generate stats for client {client.ClientId} sensor {sensor.sensorId} from {fromHour} until {fromHour.AddHours(1)}");

            var sensorsHistory = _sensorsHistory.GetHourHistory(client.ClientId, sensor.sensorId, fromHour);
            var clientsHistory = _clientsHistory.GetHourHistory(client.ClientId, fromHour);

            var statsRecord = calculateStats(sensorsHistory, clientsHistory);
            _statsCollection.StoreHourlyStats(statsRecord);

            _logger.Log($"Stored stats:\n {JsonConvert.SerializeObject(statsRecord, Formatting.Indented)}");

            return 1;
        }

        private StatsSensorState calculateStats(List<SensorStateHistory> sensorsHistory, List<ClientStateHistory> clientsHistory)
        {
            throw new NotImplementedException();
        }
    }
}
