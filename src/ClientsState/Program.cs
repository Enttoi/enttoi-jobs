using Microsoft.Azure.WebJobs;
using System;

namespace ClientsState
{
    class Program
    {
        static void Main()
        {
            var host = new JobHost(new JobHostConfiguration
            {
                DashboardConnectionString = Conf("STORAGE_CONNECTION_STRING") ?? "UseDevelopmentStorage=true",
                StorageConnectionString = Conf("STORAGE_CONNECTION_STRING") ?? "UseDevelopmentStorage=true"
            });
            host.CallAsync(typeof(Functions).GetMethod("MonitorClientsState"));
            host.RunAndBlock();
        }

        internal static string Conf(string key) => Environment.GetEnvironmentVariable(key);
    }
}
