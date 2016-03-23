using System;
using Microsoft.Azure.WebJobs;
using JobsCommon.Logger;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SensorStateStats.Storage;
using SensorStateStats.Processors;
using Autofac;

namespace SensorStateStats
{
    public class Functions
    {
        private static readonly TimeSpan INTERVAL_CHECK = TimeSpan.FromMinutes(1);

        [NoAutomaticTrigger]
        public static async Task ProcessSensorsState(CancellationToken token)
        {
            using (var container = createContainer())
            {
                var logger = container.Resolve<ILogger>();
                logger.Log($"Started processing history with interval {INTERVAL_CHECK}");

                while (!token.IsCancellationRequested)
                {
                    var watch = new Stopwatch();
                    watch.Start();

                    var recordsGenerated = 0;
                    using (var scope = container.BeginLifetimeScope())
                    {
                        try
                        {
                            recordsGenerated = await scope.Resolve<SensorStateStatsProcessor>()
                                .GenerateHourlyStats(DateTime.UtcNow);
                        }
                        catch (Exception ex)
                        {
                            logger.Log($"Error occurred in checking clients state: {ex.Message}");
                        }

                        if (recordsGenerated > 0)
                            logger.Log($"Generated {recordsGenerated} stats records within {watch.ElapsedMilliseconds}ms");

                    }

                    if (recordsGenerated == 0)
                        await Task.Delay(INTERVAL_CHECK, token);
                }

                logger.Log($"Stopped processing history");

            }
        }

        private static IContainer createContainer()
        {
            var builder = new ContainerBuilder();

            builder
                .RegisterType<ConsoleLogger>()
                .As<ILogger>()
                .SingleInstance();

            builder
                .RegisterType<ClientsCollection>()
                .As<IClientsCollection>()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<StatsCollection>()
                .As<IStatsCollection>()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<SensorsHistoryTable>()
                .As<ISensorsHistoryTable>()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<ClientsHistoryTable>()
                .As<IClientsHistoryTable>()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<SensorStateStatsProcessor>()
                .As<SensorStateStatsProcessor>()
                .InstancePerLifetimeScope();

            return builder.Build();
        }
    }
}
