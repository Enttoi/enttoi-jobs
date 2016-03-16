using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace SensorStateStats
{
    public class StorageRangeQuery<T> where T : TableEntity, new()
    {
        protected TableQuery<T> Query;

        public StorageRangeQuery(string partitionKey, string from, string to)
        {
            if (String.IsNullOrEmpty(partitionKey)) throw new ArgumentNullException(nameof(partitionKey));
            if (String.IsNullOrEmpty(from)) throw new ArgumentNullException(nameof(from));
            if (String.IsNullOrEmpty(to)) throw new ArgumentNullException(nameof(to));

            var partition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
            var low = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, from);
            var high = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, to);

            this.Query = new TableQuery<T>().Where(
                TableQuery.CombineFilters(
                    partition, 
                    TableOperators.And,
                    TableQuery.CombineFilters(low, TableOperators.And, high)));
        }

        public virtual IEnumerable<T> ExecuteOn(CloudTable table)
        {
            var token = new TableContinuationToken();
            var segment = table.ExecuteQuerySegmented(Query, token);
            while (token != null)
            {
                foreach (var result in segment)
                {
                    yield return result;
                }
                token = segment.ContinuationToken;
                segment = table.ExecuteQuerySegmented(Query, token);
            }
        }
    }
}
