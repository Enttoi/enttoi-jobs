using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SensorStateStats.Processors;
using JobsCommon.Logger;
using Tests.SensorStateStats.Stabs;
using System.Collections.Generic;
using System.Reflection;
using SensorStateStats.Models;

namespace Tests.SensorStateStats
{
    [TestClass]
    public class CalculateHourlyStatsTests
    {
        SensorStateStatsProcessor _processor;
        MethodInfo _testedMethod;

        [TestInitialize]
        public void Init()
        {
            _processor = new SensorStateStatsProcessor(
                new ConsoleLogger(),
                new TestClientsCollection(),
                new TestStatsCollection(),
                new TestSensorsHistoryTable(),
                new TestClientsHistoryTable()
                );
            _testedMethod = typeof(SensorStateStatsProcessor).GetMethod("calculateHourlyStats", BindingFlags.NonPublic | BindingFlags.Instance);
        }


        [TestMethod]
        public void AmountOfStatesTest()
        {
            var result = invokeCalculateHourlyStats(new List<ClientStateHistory>(), new List<SensorStateHistory>(), null, null);
            Assert.AreEqual(3, result.Count, "The amount of states should be always 3");
        }

        private Dictionary<int, long> invokeCalculateHourlyStats(
                    List<ClientStateHistory> clientsHistory,
                    List<SensorStateHistory> sensorsHistory,
                    ClientStateHistory previousClientHistory,
                    SensorStateHistory previousSensorHistory)
        {
            var result = _testedMethod.Invoke(_processor, parameters: new object[] {
                clientsHistory, sensorsHistory, previousClientHistory, previousSensorHistory });
            return result as Dictionary<int, long>;
        }
    }
}
