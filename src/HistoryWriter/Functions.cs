using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.ServiceBus.Messaging;
using System;

namespace HistoryWriter
{
    public class Functions
    {
        private const string TOPIC_CLIENTS_STATE = "client-state-changed";
        private const string TOPIC_SENSORS_STATE = "sensor-state-changed";

        private const string SUBSCRIPTION_PREFIX = "history_";

        public static void ClientsStateLinstener([ServiceBusTrigger(TOPIC_CLIENTS_STATE, SUBSCRIPTION_PREFIX + TOPIC_CLIENTS_STATE)]
            BrokeredMessage message)
        {
            using (Stream stream = message.GetBody<Stream>())            
            using (TextReader reader = new StreamReader(stream))
            {
                l("1111111111111 " + reader.ReadToEnd());
            }
        }

        public static void SensorsStateLinstener([ServiceBusTrigger(TOPIC_SENSORS_STATE, SUBSCRIPTION_PREFIX + TOPIC_SENSORS_STATE)]
            BrokeredMessage message)
        {
            using (Stream stream = message.GetBody<Stream>())
            using (TextReader reader = new StreamReader(stream))
            {
                l("2222222222222 " + reader.ReadToEnd());
            }
        }

        private static void l(string message)
        {
            Console.Out.WriteLine(message);
        }

        private static void e(string message)
        {
            Console.Error.WriteLine(message);
        }
    }
}
