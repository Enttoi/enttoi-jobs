using JobsCommon.Logger;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using SensorStateStats.Models;
using SensorStateStats.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SensorStateStats.Processors
{
    public class SensorStateStatsProcessor
    {
        private enum StatState
        {
            Offline = -1,
            Available = 0,
            Occupied = 1

        }

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
        public async Task<int> GenerateHourlyStats(DateTime now)
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

                    // get last history records used when calculating previous hour
                    ClientStateHistory previousClientHistory = null;
                    if (previousStats != null && previousStats.ClientPreviousHistoryRecordRowKey != null)
                        previousClientHistory = _clientsHistory.Get($"{metaClient.ClientId}", previousStats.ClientPreviousHistoryRecordRowKey);
                    SensorStateHistory previousSensorHistory = null;
                    if (previousStats != null && previousStats.SensorPreviousHistoryRecordRowKey != null)
                        previousSensorHistory = _sensorsHistory.Get($"{metaClient.ClientId}-{metaSensor.sensorId}", previousStats.ClientPreviousHistoryRecordRowKey);

                    // calculate statistics for the new stats record
                    var newStats = new StatsSensorState
                    {
                        ClientId = metaClient.ClientId,
                        SensorId = metaSensor.sensorId,
                        TimeStampHourResolution = startingHour.Value,
                        States = calculateHourlyStats(clientsHistoryRecords, sensorsHistoryRecords, previousClientHistory, previousSensorHistory)
                    };

                    // preserve the reference to the last history record used for calculating this stats record
                    newStats.ClientPreviousHistoryRecordRowKey = clientsHistoryRecords.LastOrDefault()?.RowKey ?? previousStats?.ClientPreviousHistoryRecordRowKey;
                    newStats.SensorPreviousHistoryRecordRowKey = sensorsHistoryRecords.LastOrDefault()?.RowKey ?? previousStats?.SensorPreviousHistoryRecordRowKey;

                    if (await _statsCollection.StoreHourlyStatsAsync(newStats))
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

        /// <summary>
        /// Calculates for how long the sensor stayed in each state during the hour.
        /// </summary>
        /// <param name="clientsHistory">The clients states changes history for the given hour (can be empty).</param>
        /// <param name="sensorsHistory">The sensors states changes history for the give hour (can be empty).</param>
        /// <param name="previousClientHistory">The most recent client state change prior to the hour we calculating for (can be null).</param>
        /// <param name="previousSensorHistory">The most recent sensor state change prior to the hour we calculating for (can be null).</param>
        /// <returns>Dictionary where is the key is a state and the value is for how long in ms the sensor stayed in this state</returns>
        private Dictionary<int, long> calculateHourlyStats(
            List<ClientStateHistory> clientsHistory,
            List<SensorStateHistory> sensorsHistory,
            ClientStateHistory previousClientHistory,
            SensorStateHistory previousSensorHistory)
        {
            IDictionary<ClientStateHistory, List<SensorStateHistory>> sensorStatesByClient = new SortedDictionary<ClientStateHistory, List<SensorStateHistory>>();

            var result = new Dictionary<int, long>() { { -1, 0 }, { 0, 0 }, { 1, 0 } };

            //No Data on current hour, take previous stats and set for full hour 
            if (clientsHistory.Count == 0 && sensorsHistory.Count == 0)
            {
                if (previousClientHistory == null || previousClientHistory.IsOnline == false)
                {
                    result[-1] = ((long)TimeSpan.FromHours(1).TotalMilliseconds);
                }

                if (previousClientHistory != null && previousClientHistory.IsOnline && previousSensorHistory != null)
                {
                    result[previousSensorHistory.State] = ((long)TimeSpan.FromHours(1).TotalMilliseconds);
                }
                return result;
            }

            //Set previous and set to beginging of current hour
            if (previousClientHistory != null)
            {
                previousClientHistory.StateChangedTimestamp = getStartingHour(clientsHistory, sensorsHistory);
                sensorStatesByClient.Add(previousClientHistory, new List<SensorStateHistory>());

                if (previousSensorHistory != null)
                {
                    previousSensorHistory.StateChangedTimestamp = previousClientHistory.StateChangedTimestamp;
                    sensorStatesByClient[previousClientHistory].Add(previousSensorHistory);
                }
            }

            //clients list to dictionary keys
            foreach (var clientHistoryRecord in clientsHistory)
            {
                sensorStatesByClient.Add(clientHistoryRecord, new List<SensorStateHistory>());
            }

            //triage sensors to relevant client records
            foreach (var sensorHistRecord in sensorsHistory)
            {
                //going backwards.
                var key = sensorStatesByClient.Keys.OrderByDescending(c => c.StateChangedTimestamp).First(c => c.StateChangedTimestamp <= sensorHistRecord.StateChangedTimestamp);
                sensorStatesByClient[key].Add(sensorHistRecord);
            }

            //for each client set a dummy first sensor record (with time equal to  begining of the range..)
            foreach (var client in sensorStatesByClient)
            {
                if (client.Key.IsOnline)
                {
                    var closest = FindClosestSensorHistory(client.Key.StateChangedTimestamp, sensorsHistory) ?? previousSensorHistory;
                    if (!client.Value.Any() || client.Value.First().StateChangedTimestamp != client.Key.StateChangedTimestamp)
                    {
                        client.Value.Insert(0, new SensorStateHistory() { StateChangedTimestamp = client.Key.StateChangedTimestamp, State = closest.State });
                    }
                }
            }

            //calc offline
            var orderedKeys = sensorStatesByClient.Keys.OrderBy(c => c.StateChangedTimestamp);
            DateTime from = orderedKeys.First().StateChangedTimestamp.ToRoundHourBottom();

            foreach (var key in orderedKeys)
            {
                if (key.IsOnline == true)
                {
                    result[-1] += ((long)(key.StateChangedTimestamp - from).TotalMilliseconds);
                }
                from = key.StateChangedTimestamp;
            }
            //add the rest
            if (!orderedKeys.Last().IsOnline)
            {
                result[-1] += orderedKeys.Last().StateChangedTimestamp.MsTillTheEndOfHour();
            }


            //calc available/occupied for each online range
            DateTime to = orderedKeys.First().StateChangedTimestamp.ToRoundHourUpper();

            foreach (var key in orderedKeys.Reverse())
            {
                if (key.IsOnline == true)
                {
                    for (int i = sensorStatesByClient[key].Count - 1; i >= 0; i--)
                    {
                        var sensorState = sensorStatesByClient[key][i];
                        result[sensorState.State] += ((long)(to - sensorStatesByClient[key][i].StateChangedTimestamp).TotalMilliseconds);
                        to = sensorState.StateChangedTimestamp;
                    }
                }
                to = key.StateChangedTimestamp;
            }

            return result;


        }

        private DateTime getStartingHour(List<ClientStateHistory> clientsHistory, List<SensorStateHistory> sensorsHistory)
        {
            if (clientsHistory.Count > 0)
            {
                return clientsHistory.First().StateChangedTimestamp.ToRoundHourBottom();
            }
            if (sensorsHistory.Count > 0)
            {
                return sensorsHistory.First().StateChangedTimestamp.ToRoundHourBottom();
            }
            throw new Exception("!!");
        }


        /// <summary>
        /// Finding closestSensorStateHistory record in list below a reference date
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="sensorsHistory"></param>
        /// <returns></returns>
        private SensorStateHistory FindClosestSensorHistory(DateTime reference, List<SensorStateHistory> sensorsHistory)
        {
            for (int i = sensorsHistory.Count - 1; i >= 0; i--)
            {
                if (sensorsHistory[i].StateChangedTimestamp <= reference)
                {
                    return sensorsHistory[i];
                }
            }
            return null;
        }
    }

}
