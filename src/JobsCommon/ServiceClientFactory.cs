using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Client.TransientFaultHandling;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace JobsCommon
{
    public static class ServiceClientFactory
    {
        private const int RETRY_COUNT = 3;
        private static readonly TimeSpan RETRY_INTERVAL = TimeSpan.FromMilliseconds(500);

        public static IReliableReadWriteDocumentClient GetDocumentClient()
        {
            var uri = new Uri(Configurations.DocumentDbEndpoint);
            return new DocumentClient(uri, Configurations.DocumentDbAccessKey)
                .AsReliable(new FixedInterval(RETRY_COUNT, RETRY_INTERVAL));
        }

        public static CloudTableClient GetStorageClient()
        {
            var storageAccount = CloudStorageAccount.Parse(Configurations.StorageConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            tableClient.DefaultRequestOptions = new TableRequestOptions()
            {
                RetryPolicy = new LinearRetry(RETRY_INTERVAL, RETRY_COUNT),
                LocationMode = LocationMode.PrimaryThenSecondary
            };

            return tableClient;
        }

        public static TopicClient GetTopicClient(string topicPath)
        {
            var client = TopicClient.CreateFromConnectionString(Configurations.ServiceBusConnectionString, topicPath);
            client.RetryPolicy = new RetryExponential(RETRY_INTERVAL, RETRY_INTERVAL + new TimeSpan(RETRY_INTERVAL.Ticks / 2), RETRY_COUNT);
            return client;
        }
    }
}
