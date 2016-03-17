using Microsoft.Azure.WebJobs;
using JobsCommon;

namespace SensorStateStats
{
    class Program
    {
        static void Main()
        {
            using (var host = new JobHost(new JobHostConfiguration
            {
                DashboardConnectionString = Configurations.StorageConnectionString,
                StorageConnectionString = Configurations.StorageConnectionString
            }))
            {
                var task = host.CallAsync(typeof(Functions).GetMethod("ProcessSensorsState"));
                host.RunAndBlock();
            }
        }
    }
}
