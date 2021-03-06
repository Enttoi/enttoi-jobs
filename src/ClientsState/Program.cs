﻿using JobsCommon;
using Microsoft.Azure.WebJobs;
using System;

namespace ClientsState
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

                var task = host.CallAsync(typeof(Functions).GetMethod("MonitorClientsState"));
                host.RunAndBlock();
            }
        }
    }
}
