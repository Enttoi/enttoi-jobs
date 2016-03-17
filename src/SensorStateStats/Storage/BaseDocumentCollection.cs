
using JobsCommon;
using JobsCommon.Logger;
using Microsoft.Azure.Documents.Client.TransientFaultHandling;

namespace SensorStateStats.Storage
{
    abstract class BaseDocumentCollection
    {
        protected readonly IReliableReadWriteDocumentClient _client;
        protected readonly ILogger _logger;

        public BaseDocumentCollection(ILogger logger)
        {
            _client = ServiceClientFactory.GetDocumentClient();
            _logger = logger;
        }
    }
}
