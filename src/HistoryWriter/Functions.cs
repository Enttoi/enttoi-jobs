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
        private const string SUBSCRIPTION_PREFIX = "history_";

        private static ILogger _logger = new ConsoleLogger();

        public static void ClientsStateLinstener([ServiceBusTrigger(Configurations.TOPIC_CLIENTS_STATE, SUBSCRIPTION_PREFIX + Configurations.TOPIC_CLIENTS_STATE)]
            BrokeredMessage message)
        {
            using (var stream = new StreamReader(message.GetBody<Stream>(), Encoding.UTF8))
            {
                var payload = stream.ReadToEnd();

                var parsedMessage = JsonConvert.DeserializeObject<ClientStateMessage>(payload);
                var historyRecord = new ClientStateHistory(parsedMessage);
                
                ServiceClientFactory.GetStorageClient()
                    .GetTableReference(Configurations.HISTORY_TABLE_CLIENTS_STATE)
                    .Execute(TableOperation.Insert(historyRecord));
            }
        }

        public static void SensorsStateLinstener([ServiceBusTrigger(Configurations.TOPIC_SENSORS_STATE, SUBSCRIPTION_PREFIX + Configurations.TOPIC_SENSORS_STATE)]
            BrokeredMessage message)
        {
            using (var stream = new StreamReader(message.GetBody<Stream>(), Encoding.UTF8))
            {
                var _payload = stream.ReadToEnd();

                var parsedMessage = JsonConvert.DeserializeObject<SensorStateMessage>(_payload);
                var historyRecord = new SensorStateHistory(parsedMessage);

                ServiceClientFactory.GetStorageClient()
                    .GetTableReference(Configurations.HISTORY_TABLE_SENSORS_STATE)
                    .Execute(TableOperation.Insert(historyRecord));
            }
        }
    }
}
