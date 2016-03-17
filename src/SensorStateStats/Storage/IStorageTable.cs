namespace SensorStateStats.Storage
{
    interface IStorageTable<T>
    {
        T Get(string partitionKey, string rowKey);
    }
}
