using JobsCommon.Logger;
using Newtonsoft.Json;
using SensorStateStats.Models;
using SensorStateStats.Storage;
using System;
using System.Collections.Generic;
using System.Linq;

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
                    var history = getHourlyHistory(now, metaClient, metaSensor, oldestClientHistory);
                    if (history == null && oldestClientHistory.IsValueCreated && oldestClientHistory.Value == null)
                        // no point to check other sensors for this client, since
                        // the client never went online
                        break; 
                    else if (history == null)
                        continue;
                    
                    var statsRecord = calculateHourlyStats(history.Item1, history.Item2);
                    _statsCollection.StoreHourlyStats(statsRecord);

                    _logger.Log($"Stored stats:\n {JsonConvert.SerializeObject(statsRecord, Formatting.Indented)}");
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
        private Tuple<List<ClientStateHistory>, List<SensorStateHistory>> getHourlyHistory(DateTime now, Client client, Sensor sensor, Lazy<ClientStateHistory> oldestClientHistory)
        {
            var lastStat = _statsCollection.GetLatestStatsRecord(client.ClientId, sensor.sensorId);
            DateTime fromHour; // the beginning of the hour for which stats generated

            if (lastStat == null)
            {
                // no previously generated statistics for this sensor => get the very first history record
                if (oldestClientHistory.Value == null)
                {
                    _logger.Log($"Client {client.ClientId} doesn't have any history records");
                    return null;
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
                return null;

            var clientsHistoryRecords = _clientsHistory.GetHourHistory(client.ClientId, fromHour);
            var sensorsHistoryRecords = _sensorsHistory.GetHourHistory(client.ClientId, sensor.sensorId, fromHour);

            _logger.Log($"History records found for client {client.ClientId} sensor {sensor.sensorId} from {fromHour} until {fromHour.AddHours(1)} are {clientsHistoryRecords.Count} client and {sensorsHistoryRecords.Count} sensors records");

            if(lastStat == null)
            {
                clientsHistoryRecords.Insert(0, null);
                sensorsHistoryRecords.Insert(0, null);
            }
            else
            {
                clientsHistoryRecords.Insert(0, _clientsHistory.Get(
                    $"{client.ClientId}", lastStat.ClientPreviousHistoryRecordRowKey));
                sensorsHistoryRecords.Insert(0, _sensorsHistory.Get(
                    $"{client.ClientId}-{sensor.sensorId}", lastStat.SensorPreviousHistoryRecordRowKey));
            }

            return Tuple.Create(clientsHistoryRecords, sensorsHistoryRecords);
        }

        private StatsSensorState calculateHourlyStats(List<ClientStateHistory> clientsHistory, List<SensorStateHistory> sensorsHistory)
        {



            var stats = new StatsSensorState {
                ClientPreviousHistoryRecordRowKey = clientsHistory.Last().RowKey,
                SensorPreviousHistoryRecordRowKey = sensorsHistory.Last().RowKey
            };
            throw new NotImplementedException();
        }
    }
}
