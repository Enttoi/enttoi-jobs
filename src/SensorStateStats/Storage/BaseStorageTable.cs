using System;
using JobsCommon;
using JobsCommon.Logger;
using Microsoft.WindowsAzure.Storage.Table;

namespace SensorStateStats.Storage
{
    abstract class BaseStorageTable<T> : IStorageTable<T> where T: class, ITableEntity
    {
        protected readonly CloudTableClient _client;
        protected CloudTable _table;
        protected readonly ILogger _logger;

        public BaseStorageTable(ILogger logger)
        {
            _client = ServiceClientFactory.GetStorageClient();
            _logger = logger;
        }

        public T Get(string partitionKey, string rowKey)
        {
            var retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            return _table.Execute(retrieveOperation).Result as T;
        }
    }
}
