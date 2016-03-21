using JobsCommon.Logger;
using Microsoft.WindowsAzure.Storage.Table;
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
                foreach (var metaSensor in metaClient.Sensors)
                {
                    // get previous stats written for this sensor (if any)
                    var previousStats = _statsCollection.GetLatestStatsRecord(metaClient.ClientId, metaSensor.sensorId);

                    // get a starting time of the hour for which stats will be generated
                    var oldestSensorHistory = new Lazy<SensorStateHistory>(() =>
                        _sensorsHistory.GetOldestHistoryRecord(metaClient.ClientId, metaSensor.sensorId));
                    var startingHour = getStartingHour(previousStats, oldestSensorHistory);

                    if (startingHour == null)
                    {
                        _logger.Log($"Sensor {metaSensor.sensorId} in client {metaClient.ClientId} doesn't have any history records");
                        continue;
                    }

                    if (startingHour.Value.AddHours(1) > now)
                        // too soon to generate stats for this hour
                        continue;

                    // get the actual history records for the given hour 
                    // note, there could be no records - sensor was offline the entire hour
                    var clientsHistoryRecords = _clientsHistory.GetHourHistory(metaClient.ClientId, startingHour.Value);
                    var sensorsHistoryRecords = _sensorsHistory.GetHourHistory(metaClient.ClientId, metaSensor.sensorId, startingHour.Value);

                    // calculate statistics for the new stats record
                    var newStats = new StatsSensorState
                    {
                        ClientId = metaClient.ClientId,
                        SensorId = metaSensor.sensorId,
                        TimeStampHourResolution = startingHour.Value,
                        States = new Dictionary<int, long>() { { -1, 0 }, { 0, 0 }, { 1, 0 } }
                    };
                    calculateHourlyStats(clientsHistoryRecords, sensorsHistoryRecords, previousStats, ref newStats);

                    // preserve the reference to the last history record used for calculating this stats record
                    newStats.ClientPreviousHistoryRecordRowKey = clientsHistoryRecords.LastOrDefault()?.RowKey ?? previousStats?.ClientPreviousHistoryRecordRowKey;
                    newStats.SensorPreviousHistoryRecordRowKey = sensorsHistoryRecords.LastOrDefault()?.RowKey ?? previousStats?.SensorPreviousHistoryRecordRowKey;


                    _statsCollection.StoreHourlyStats(newStats);
                    _logger.Log($"Stored stats:\n {JsonConvert.SerializeObject(newStats, Formatting.Indented)}");

                    recordsGenerated++;
                }
            }

            return recordsGenerated;
        }

        /// <summary>
        /// Gets the starting hour for which the statistics will be calculated.
        /// </summary>
        /// <param name="latestStatsRecord">The latest (most recent) stats record.</param>
        /// <param name="oldestSensorHistory">The oldest sensor history.</param>
        private DateTime? getStartingHour(StatsSensorState latestStatsRecord, Lazy<SensorStateHistory> oldestSensorHistory)
        {
            if (latestStatsRecord == null)
            {
                // no previously generated statistics for this sensor => get the very first history record
                if (oldestSensorHistory.Value == null)
                    return null;

                return new DateTime(
                    oldestSensorHistory.Value.StateChangedTimestamp.Year,
                    oldestSensorHistory.Value.StateChangedTimestamp.Month,
                    oldestSensorHistory.Value.StateChangedTimestamp.Day,
                    oldestSensorHistory.Value.StateChangedTimestamp.Hour, 0, 0, DateTimeKind.Utc);
            }
            else
            {
                return latestStatsRecord.TimeStampHourResolution.AddHours(1);
            }
        }

        private void calculateHourlyStats(List<ClientStateHistory> clientsHistory, List<SensorStateHistory> sensorsHistory, StatsSensorState previousStats, ref StatsSensorState stats)
        {
            throw new NotImplementedException();
            //if (previousStats != null)
            //{
            //    memorizedClientPortion = _clientsHistory.Get($"{stats.ClientId}", previousStats.ClientPreviousHistoryRecordRowKey);
            //    memorizedPortion = _sensorsHistory.Get($"{stats.ClientId}-{stats.SensorId}", previousStats.ClientPreviousHistoryRecordRowKey);



            //}
            //else
            //{
            //    // first ever record => there is at least one history record for sensor
            //    if (sensorsHistory.Count > 1) 
            //    {
            //        SensorStateHistory memorizedSensorPortion = sensorsHistory[0];
            //        for (int i = 1; i < sensorsHistory.Count; i++)
            //        {
            //            var clientStates = clientsHistory
            //                .Where(h => h.StateChangedTimestamp > memorizedSensorPortion.StateChangedTimestamp && h.StateChangedTimestamp < sensorsHistory[i].StateChangedTimestamp);

                        

            //            stats.States[memorizedSensorPortion.State] += (long)(sensorsHistory[i].StateChangedTimestamp - memorizedSensorPortion.StateChangedTimestamp).TotalMilliseconds;
            //            memorizedSensorPortion = sensorsHistory[i];
            //        }
            //    }
            //    stats.States[sensorsHistory.Last().State] += sensorsHistory.Last().StateChangedTimestamp.MsTillTheEndOfHour();
            //}
        }
    }
}
