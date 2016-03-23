namespace SensorStateStats.Storage
{
    public interface IStorageTable<T>
    {
        T Get(string partitionKey, string rowKey);
    }
}
