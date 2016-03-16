
using System;

namespace JobsCommon
{
    public static class Configurations
    {
        public const string HISTORY_TABLE_CLIENTS_STATE = "HistoryDayliClientsState";
        public const string HISTORY_TABLE_SENSORS_STATE = "HistoryDayliSensorsState";
        public const string TABLE_CLIENTS_STATE = "ClientsState";

        public const string TOPIC_CLIENTS_STATE = "client-state-changed";
        public const string TOPIC_SENSORS_STATE = "sensor-state-changed";

        public static string StorageConnectionString => conf("STORAGE_CONNECTION_STRING") ?? "UseDevelopmentStorage=true";

        public static string ServiceBusConnectionString => conf("SERVICEBUS_CONNECTION_STRING");

        public static string DocumentDbName => conf("DOCUMENT_DB_NAME") ?? "development";

        public static string DocumentDbEndpoint => conf("DOCUMENT_DB_ENDPOINT");

        public static string DocumentDbAccessKey => conf("DOCUMENT_DB_ACCESS_KEY");

        /// <summary>
        /// Gets configuration for specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private static string conf(string key) => Environment.GetEnvironmentVariable(key);
    }
}
