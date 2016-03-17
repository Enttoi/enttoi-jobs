using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SensorStateStats.Storage
{
    public class StorageRangeQuery<T> where T : TableEntity, new()
    {
        private TableQuery<T> _query;

        public StorageRangeQuery(string partitionKey, string from, string to)
        {
            if (String.IsNullOrEmpty(partitionKey)) throw new ArgumentNullException(nameof(partitionKey));
            if (String.IsNullOrEmpty(from)) throw new ArgumentNullException(nameof(from));
            if (String.IsNullOrEmpty(to)) throw new ArgumentNullException(nameof(to));

            var partition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
            var low = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, from);
            var high = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, to);

            this._query = new TableQuery<T>().Where(
                TableQuery.CombineFilters(
                    partition,
                    TableOperators.And,
                    TableQuery.CombineFilters(low, TableOperators.And, high)));
        }

        public IEnumerable<T> GetFullResult(CloudTable table)
        {
            var token = new TableContinuationToken();
            var segment = table.ExecuteQuerySegmented(_query, token);
            while (token != null)
            {
                foreach (var result in segment)
                {
                    yield return result;
                }
                token = segment.ContinuationToken;
                segment = table.ExecuteQuerySegmented(_query, token);
            }
        }

        public T GetTopOne(CloudTable table)
        {
            return table
                .ExecuteQuery(this._query.Take(1))
                .ToList()
                .FirstOrDefault() as T;
        }
    }
}
