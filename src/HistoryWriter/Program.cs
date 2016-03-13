using JobsCommon;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;

namespace HistoryWriter
{
    class Program
    {
        static void Main()
        {
            var config = new JobHostConfiguration
            {
                DashboardConnectionString = Configurations.StorageConnectionString,
                StorageConnectionString = Configurations.StorageConnectionString
            };
            config.UseServiceBus(new ServiceBusConfiguration {
                ConnectionString = Configurations.ServiceBusConnectionString
            });

            using (var host = new JobHost(config))
            {
                host.RunAndBlock();
            }
        }

    }
}
