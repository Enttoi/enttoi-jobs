
using JobsCommon;
using JobsCommon.Logger;
using Microsoft.Azure.Documents;
using SensorStateStats.Models;
using System;
using System.Linq;

namespace SensorStateStats.Storage
{
    class StatsCollection : BaseDocumentCollection, IStatsCollection
    {
        private static readonly Uri _statsCollectionUri = new Uri($"dbs/{Configurations.DocumentDbName}/colls/stats-sensor-states", UriKind.Relative);

        public StatsCollection(ILogger logger) : base(logger)
        {

        }

        public StatsSensorState GetLatestStatsRecord(Guid clientId, int sensorId)
        {
            var _query = new SqlQuerySpec()
            {
                QueryText = "SELECT TOP 1 * FROM s WHERE s.clientId = @clientId AND s.sensorId = @sensorId ORDER BY s.timeStampHourResolution DESC",
                Parameters = new SqlParameterCollection {
                    new SqlParameter("@clientId", clientId),
                    new SqlParameter("@sensorId", sensorId)
                }
            };

            return _client.CreateDocumentQuery<StatsSensorState>(_statsCollectionUri, _query)
                .ToList()
                .SingleOrDefault();
        }

        public void StoreHourlyStats(StatsSensorState statsRecord)
        {
            throw new NotImplementedException();
        }
    }
}
