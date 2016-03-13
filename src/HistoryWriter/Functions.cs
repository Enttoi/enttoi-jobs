using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.ServiceBus.Messaging;
using JobsCommon.Logger;
using System.Text;
using HistoryWriter.Models;
using Newtonsoft.Json;
using JobsCommon;
using Microsoft.WindowsAzure.Storage.Table;

namespace HistoryWriter
{
    public class Functions
    {
        private const string TOPIC_CLIENTS_STATE = "client-state-changed";
        private const string HISTORY_TABLE_CLIENTS_STATE = "HistoryDayliClientsState";

        private const string TOPIC_SENSORS_STATE = "sensor-state-changed";
        private const string HISTORY_TABLE_SENSORS_STATE = "HistoryDayliSensorsState";

        private const string SUBSCRIPTION_PREFIX = "history_";

        private static ILogger _logger = new ConsoleLogger();

        public static void ClientsStateLinstener([ServiceBusTrigger(TOPIC_CLIENTS_STATE, SUBSCRIPTION_PREFIX + TOPIC_CLIENTS_STATE)]
            BrokeredMessage message)
        {
            using (var stream = new StreamReader(message.GetBody<Stream>(), Encoding.UTF8))
            {
                var payload = stream.ReadToEnd();

                var parsedMessage = JsonConvert.DeserializeObject<ClientStateMessage>(payload);
                var historyRecord = new ClientStateHistory(parsedMessage);
                
                ServiceClientFactory.GetStorageClient()
                    .GetTableReference(HISTORY_TABLE_CLIENTS_STATE)
                    .Execute(TableOperation.Insert(historyRecord));
            }
        }

        public static void SensorsStateLinstener([ServiceBusTrigger(TOPIC_SENSORS_STATE, SUBSCRIPTION_PREFIX + TOPIC_SENSORS_STATE)]
            BrokeredMessage message)
        {
            using (var stream = new StreamReader(message.GetBody<Stream>(), Encoding.UTF8))
            {
                var _payload = stream.ReadToEnd();

                var parsedMessage = JsonConvert.DeserializeObject<SensorStateMessage>(_payload);
                var historyRecord = new SensorStateHistory(parsedMessage);

                ServiceClientFactory.GetStorageClient()
                    .GetTableReference(HISTORY_TABLE_SENSORS_STATE)
                    .Execute(TableOperation.Insert(historyRecord));
            }
        }
    }
}
