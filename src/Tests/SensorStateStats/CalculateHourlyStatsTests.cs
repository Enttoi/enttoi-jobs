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

        /// <summary>
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | Previous client history | Previous sensor history | List of clients history |  List of sensors history  | Expected result |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | null                    | null                    | Online at **:10:00.000  | State 1 from **:10:00.000 | {               |
        /// |                         |                         |                         |                           | "-1": 600000    |
        /// |                         |                         |                         |                           | "0": 0          |
        /// |                         |                         |                         |                           | "1": 3000000    |
        /// |                         |                         |                         |                           | }               |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// </summary>
        [TestMethod]
        public void FirstStatsRecord_1_Test()
        {
            // ### Arrange ####################
            var clientsHistory = new List<ClientStateHistory>() {
                generateClient(isOnline: true, minutesPortion: 10)
            };
            var sensorsHistory = new List<SensorStateHistory>() {
                generateSensor(state: 1, minutesPortion: 10)
            };

            // ### Act ########################
            var result = invokeCalculateHourlyStats(clientsHistory, sensorsHistory, null, null);

            // ### Assert #####################
            Assert.AreEqual(3, result.Count, "The number of returned state types is incorrect");
            Assert.AreEqual(TimeSpan.FromMinutes(10).TotalMilliseconds, result[-1], "The 'offline' state duration is wrong");
            Assert.AreEqual(0, result[0], "The 'free' state duration is wrong");
            Assert.AreEqual(TimeSpan.FromMinutes(50).TotalMilliseconds, result[1], "The 'occupied' state duration is wrong");
        }

        /// <summary>
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | Previous client history | Previous sensor history | List of clients history |  List of sensors history  | Expected result |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | null                    | null                    | Online at **:10:00.000  | State 1 from **:10:00.000 | {               |
        /// |                         |                         | Offline at **:30:00.000 |                           | "-1": 1200000   |
        /// |                         |                         | Online at **:40:00.000  |                           | "0": 0          |
        /// |                         |                         |                         |                           | "1": 2400000    |
        /// |                         |                         |                         |                           | }               |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// </summary>
        [TestMethod]
        public void FirstStatsRecord_2_Test()
        {
            // ### Arrange ####################
            var clientsHistory = new List<ClientStateHistory>() {
                generateClient(isOnline: true, minutesPortion: 10),
                generateClient(isOnline: false, minutesPortion: 30),
                generateClient(isOnline: true, minutesPortion: 40)
            };
            var sensorsHistory = new List<SensorStateHistory>() {
                generateSensor(state: 1, minutesPortion: 10)
            };

            // ### Act ########################
            var result = invokeCalculateHourlyStats(clientsHistory, sensorsHistory, null, null);

            // ### Assert #####################
            Assert.AreEqual(3, result.Count, "The number of returned state types is incorrect");
            Assert.AreEqual(TimeSpan.FromMinutes(20).TotalMilliseconds, result[-1], "The 'offline' state duration is wrong");
            Assert.AreEqual(0, result[0], "The 'free' state duration is wrong");
            Assert.AreEqual(TimeSpan.FromMinutes(40).TotalMilliseconds, result[1], "The 'occupied' state duration is wrong");
        }

        /// <summary>
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | Previous client history | Previous sensor history | List of clients history |  List of sensors history  | Expected result |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | null                    | null                    | Online at **:10:00.000  | State 1 from **:10:00.000 | {               |
        /// |                         |                         |                         | State 0 from **:20:00.000 | "-1": 600000    |
        /// |                         |                         |                         | State 1 from **:30:00.000 | "0": 1800000    |
        /// |                         |                         |                         | State 0 from **:40:00.000 | "1": 1200000    |
        /// |                         |                         |                         |                           | }               |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// </summary>
        [TestMethod]
        public void FirstStatsRecord_3_Test()
        {
            // ### Arrange ####################
            var clientsHistory = new List<ClientStateHistory>() {
                generateClient(isOnline: true, minutesPortion: 10)
            };
            var sensorsHistory = new List<SensorStateHistory>() {
                generateSensor(state: 1, minutesPortion: 10),
                generateSensor(state: 0, minutesPortion: 20),
                generateSensor(state: 1, minutesPortion: 30),
                generateSensor(state: 0, minutesPortion: 40)
            };

            // ### Act ########################
            var result = invokeCalculateHourlyStats(clientsHistory, sensorsHistory, null, null);

            // ### Assert #####################
            Assert.AreEqual(3, result.Count, "The number of returned state types is incorrect");
            Assert.AreEqual(TimeSpan.FromMinutes(10).TotalMilliseconds, result[-1], "The 'offline' state duration is wrong");
            Assert.AreEqual(TimeSpan.FromMinutes(30).TotalMilliseconds, result[0], "The 'free' state duration is wrong");
            Assert.AreEqual(TimeSpan.FromMinutes(20).TotalMilliseconds, result[1], "The 'occupied' state duration is wrong");
        }

        /// <summary>
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | Previous client history | Previous sensor history | List of clients history |  List of sensors history  | Expected result |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | null                    | null                    | Online at **:10:00.000  | State 1 from **:10:00.000 | {               |
        /// |                         |                         | Offline at **:30:00.000 | State 0 from **:20:00.000 | "-1": 2400000   |
        /// |                         |                         |                         |                           | "0": 600000     |
        /// |                         |                         |                         |                           | "1": 600000     |
        /// |                         |                         |                         |                           | }               |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// </summary>
        [TestMethod]
        public void FirstStatsRecord_4_Test()
        {
            // ### Arrange ####################
            var clientsHistory = new List<ClientStateHistory>() {
                generateClient(isOnline: true, minutesPortion: 10),
                generateClient(isOnline: false, minutesPortion: 30)
            };
            var sensorsHistory = new List<SensorStateHistory>() {
                generateSensor(state: 1, minutesPortion: 10),
                generateSensor(state: 0, minutesPortion: 20)
            };

            // ### Act ########################
            var result = invokeCalculateHourlyStats(clientsHistory, sensorsHistory, null, null);

            // ### Assert #####################
            Assert.AreEqual(3, result.Count, "The number of returned state types is incorrect");
            Assert.AreEqual(TimeSpan.FromMinutes(40).TotalMilliseconds, result[-1], "The 'offline' state duration is wrong");
            Assert.AreEqual(600000, result[0], "The 'free' state duration is wrong");
            Assert.AreEqual(600000, result[1], "The 'occupied' state duration is wrong");
        }

        /// <summary>
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | Previous client history | Previous sensor history | List of clients history |  List of sensors history  | Expected result |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | Online                  | State 1                 | Empty                   | Empty                     | {               |
        /// |                         |                         |                         |                           |   "-1": 0       |
        /// |                         |                         |                         |                           |   "0": 0        |
        /// |                         |                         |                         |                           |   "1": 3600000  |
        /// |                         |                         |                         |                           | }               |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// </summary>
        [TestMethod]
        public void NonFirstStatsRecord_1_Test()
        {
            // ### Arrange ####################
            var previousClientHistory = generateClient(true);
            var previousSensorHistory = generateSensor(1);
            var clientsHistory = new List<ClientStateHistory>();
            var sensorsHistory = new List<SensorStateHistory>();

            // ### Act ########################
            var result = invokeCalculateHourlyStats(clientsHistory, sensorsHistory, previousClientHistory, previousSensorHistory);

            // ### Assert #####################
            Assert.AreEqual(3, result.Count, "The number of returned state types is incorrect");
            Assert.AreEqual(0, result[-1], "The 'offline' state duration is wrong");
            Assert.AreEqual(0, result[0], "The 'free' state duration is wrong");
            Assert.AreEqual(3600000, result[1], "The 'occupied' state duration is wrong");
        }

        /// <summary>
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | Previous client history | Previous sensor history | List of clients history |  List of sensors history  | Expected result |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | Offline                 | State 1                 | Empty                   | Empty                     | {               |
        /// |                         |                         |                         |                           |   "-1": 3600000 |
        /// |                         |                         |                         |                           |   "0": 0        |
        /// |                         |                         |                         |                           |   "1": 0        |
        /// |                         |                         |                         |                           | }               |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// </summary>
        [TestMethod]
        public void NonFirstStatsRecord_2_Test()
        {
            // ### Arrange ####################
            var previousClientHistory = generateClient(false);
            var previousSensorHistory = generateSensor(1);
            var clientsHistory = new List<ClientStateHistory>();
            var sensorsHistory = new List<SensorStateHistory>();

            // ### Act ########################
            var result = invokeCalculateHourlyStats(clientsHistory, sensorsHistory, previousClientHistory, previousSensorHistory);

            // ### Assert #####################
            Assert.AreEqual(3, result.Count, "The number of returned state types is incorrect");
            Assert.AreEqual(TimeSpan.FromMinutes(60).TotalMilliseconds, result[-1], "The 'offline' state duration is wrong");
            Assert.AreEqual(0, result[0], "The 'free' state duration is wrong");
            Assert.AreEqual(0, result[1], "The 'occupied' state duration is wrong");
        }

        /// <summary>
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | Previous client history | Previous sensor history | List of clients history |  List of sensors history  | Expected result |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | Online                  | State 0                 | Empty                   | Empty                     | {               |
        /// |                         |                         |                         |                           |   "-1": 0       |
        /// |                         |                         |                         |                           |   "0": 3600000  |
        /// |                         |                         |                         |                           |   "1": 0        |
        /// |                         |                         |                         |                           | }               |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// </summary>
        [TestMethod]
        public void NonFirstStatsRecord_3_Test()
        {
            // ### Arrange ####################
            var previousClientHistory = generateClient(true);
            var previousSensorHistory = generateSensor(0);
            var clientsHistory = new List<ClientStateHistory>();
            var sensorsHistory = new List<SensorStateHistory>();

            // ### Act ########################
            var result = invokeCalculateHourlyStats(clientsHistory, sensorsHistory, previousClientHistory, previousSensorHistory);

            // ### Assert #####################
            Assert.AreEqual(3, result.Count, "The number of returned state types is incorrect");
            Assert.AreEqual(0, result[-1], "The 'offline' state duration is wrong");
            Assert.AreEqual(3600000, result[0], "The 'free' state duration is wrong");
            Assert.AreEqual(0, result[1], "The 'occupied' state duration is wrong");
        }

        /// <summary>
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | Previous client history | Previous sensor history | List of clients history |  List of sensors history  | Expected result |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | Online                  | State 1                 | Empty                   | State 0 from **:10:00.000 | {               |
        /// |                         |                         |                         |                           |   "-1": 0       |
        /// |                         |                         |                         |                           |   "0": 3000000  |
        /// |                         |                         |                         |                           |   "1": 600000   |
        /// |                         |                         |                         |                           | }               |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// </summary>
        [TestMethod]
        public void NonFirstStatsRecord_5_Test()
        {
            // ### Arrange ####################
            var previousClientHistory = generateClient(true);
            var previousSensorHistory = generateSensor(1);
            var clientsHistory = new List<ClientStateHistory>();
            var sensorsHistory = new List<SensorStateHistory>()
            {
                generateSensor(0, 10)
            };

            // ### Act ########################
            var result = invokeCalculateHourlyStats(clientsHistory, sensorsHistory, previousClientHistory, previousSensorHistory);

            // ### Assert #####################
            Assert.AreEqual(3, result.Count, "The number of returned state types is incorrect");
            Assert.AreEqual(0, result[-1], "The 'offline' state duration is wrong");
            Assert.AreEqual(3000000, result[0], "The 'free' state duration is wrong");
            Assert.AreEqual(600000, result[1], "The 'occupied' state duration is wrong");
        }

        /// <summary>
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | Previous client history | Previous sensor history | List of clients history |  List of sensors history  | Expected result |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | Online                  | State 1                 | Empty                   | State 0 from **:10:00.000 | {               |
        /// |                         |                         |                         |                           |   "-1": 0       |
        /// |                         |                         |                         |                           |   "0": 3000000  |
        /// |                         |                         |                         |                           |   "1": 600000   |
        /// |                         |                         |                         |                           | }               |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// </summary>
        [TestMethod]
        public void NonFirstStatsRecord_6_Test()
        {
            // ### Arrange ####################
            var previousClientHistory = generateClient(true);
            var previousSensorHistory = generateSensor(1);
            var clientsHistory = new List<ClientStateHistory>();
            var sensorsHistory = new List<SensorStateHistory>()
            {
                generateSensor(0, 10)
            };

            // ### Act ########################
            var result = invokeCalculateHourlyStats(clientsHistory, sensorsHistory, previousClientHistory, previousSensorHistory);

            // ### Assert #####################
            Assert.AreEqual(3, result.Count, "The number of returned state types is incorrect");
            Assert.AreEqual(0, result[-1], "The 'offline' state duration is wrong");
            Assert.AreEqual(3000000, result[0], "The 'free' state duration is wrong");
            Assert.AreEqual(600000, result[1], "The 'occupied' state duration is wrong");
        }

        /// <summary>
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | Previous client history | Previous sensor history | List of clients history |  List of sensors history  | Expected result |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | Online                  | State 0                 | Empty                   | State 1 from **:30:00.000 | {               |
        /// |                         |                         |                         |                           |   "-1": 0       |
        /// |                         |                         |                         |                           |   "0": 1800000  |
        /// |                         |                         |                         |                           |   "1": 1800000  |
        /// |                         |                         |                         |                           | }               |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// </summary>
        [TestMethod]
        public void NonFirstStatsRecord_7_Test()
        {
            // ### Arrange ####################
            var previousClientHistory = generateClient(true);
            var previousSensorHistory = generateSensor(0);
            var clientsHistory = new List<ClientStateHistory>();
            var sensorsHistory = new List<SensorStateHistory>()
            {
                generateSensor(1, 30)
            };

            // ### Act ########################
            var result = invokeCalculateHourlyStats(clientsHistory, sensorsHistory, previousClientHistory, previousSensorHistory);

            // ### Assert #####################
            Assert.AreEqual(3, result.Count, "The number of returned state types is incorrect");
            Assert.AreEqual(0, result[-1], "The 'offline' state duration is wrong");
            Assert.AreEqual(1800000, result[0], "The 'free' state duration is wrong");
            Assert.AreEqual(1800000, result[1], "The 'occupied' state duration is wrong");
        }

        /// <summary>
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | Previous client history | Previous sensor history | List of clients history |  List of sensors history  | Expected result |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | Online                  | State 0                 | Empty                   | State 1 from **:10:00.000 | {               |
        /// |                         |                         |                         | State 0 from **:20:00.000 |   "-1": 0       |
        /// |                         |                         |                         | State 1 from **:30:00.000 |   "0": 2400000  |
        /// |                         |                         |                         | State 0 from **:40:00.000 |   "1": 1200000  |
        /// |                         |                         |                         |                           | }               |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// </summary>
        [TestMethod]
        public void NonFirstStatsRecord_8_Test()
        {
            // ### Arrange ####################
            var previousClientHistory = generateClient(true);
            var previousSensorHistory = generateSensor(0);
            var clientsHistory = new List<ClientStateHistory>();
            var sensorsHistory = new List<SensorStateHistory>()
            {
                generateSensor(1, 10),
                generateSensor(0, 20),
                generateSensor(1, 30),
                generateSensor(0, 40)
            };

            // ### Act ########################
            var result = invokeCalculateHourlyStats(clientsHistory, sensorsHistory, previousClientHistory, previousSensorHistory);

            // ### Assert #####################
            Assert.AreEqual(3, result.Count, "The number of returned state types is incorrect");
            Assert.AreEqual(0, result[-1], "The 'offline' state duration is wrong");
            Assert.AreEqual(2400000, result[0], "The 'free' state duration is wrong");
            Assert.AreEqual(1200000, result[1], "The 'occupied' state duration is wrong");
        }

        /// <summary>
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | Previous client history | Previous sensor history | List of clients history |  List of sensors history  | Expected result |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | Offline                 | State 0                 | Online at **:10:00.000  | Empty                     | {               |
        /// |                         |                         |                         |                           |   "-1": 600000  |
        /// |                         |                         |                         |                           |   "0": 3000000  |
        /// |                         |                         |                         |                           |   "1": 0        |
        /// |                         |                         |                         |                           | }               |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// </summary>
        [TestMethod]
        public void NonFirstStatsRecord_9_Test()
        {
            // ### Arrange ####################
            var previousClientHistory = generateClient(false);
            var previousSensorHistory = generateSensor(0);
            var clientsHistory = new List<ClientStateHistory>()
            {
                generateClient(true, 10)
            };
            var sensorsHistory = new List<SensorStateHistory>();

            // ### Act ########################
            var result = invokeCalculateHourlyStats(clientsHistory, sensorsHistory, previousClientHistory, previousSensorHistory);

            // ### Assert #####################
            Assert.AreEqual(3, result.Count, "The number of returned state types is incorrect");
            Assert.AreEqual(600000, result[-1], "The 'offline' state duration is wrong");
            Assert.AreEqual(3000000, result[0], "The 'free' state duration is wrong");
            Assert.AreEqual(0, result[1], "The 'occupied' state duration is wrong");
        }

        /// <summary>
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | Previous client history | Previous sensor history | List of clients history |  List of sensors history  | Expected result |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | Offline                 | State 1                 | Online at **:10:00.000  | Empty                     | {               |
        /// |                         |                         | Offline at **:30:00.000 |                           |   "-1": 1200000 |
        /// |                         |                         | Online at **:40:00.000  |                           |   "0": 0        |
        /// |                         |                         |                         |                           |   "1": 2400000  |
        /// |                         |                         |                         |                           | }               |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// </summary>
        [TestMethod]
        public void NonFirstStatsRecord_10_Test()
        {
            // ### Arrange ####################
            var previousClientHistory = generateClient(false);
            var previousSensorHistory = generateSensor(1);
            var clientsHistory = new List<ClientStateHistory>()
            {
                generateClient(true, 10),
                generateClient(false, 30),
                generateClient(true, 40)
            };
            var sensorsHistory = new List<SensorStateHistory>();

            // ### Act ########################
            var result = invokeCalculateHourlyStats(clientsHistory, sensorsHistory, previousClientHistory, previousSensorHistory);

            // ### Assert #####################
            Assert.AreEqual(3, result.Count, "The number of returned state types is incorrect");
            Assert.AreEqual(1200000, result[-1], "The 'offline' state duration is wrong");
            Assert.AreEqual(0, result[0], "The 'free' state duration is wrong");
            Assert.AreEqual(2400000, result[1], "The 'occupied' state duration is wrong");
        }

        /// <summary>
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | Previous client history | Previous sensor history | List of clients history |  List of sensors history  | Expected result |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | Online                  | State 1                 | Offline at **:30:00.000 | Empty                     | {               |
        /// |                         |                         | Online at **:40:00.000  |                           |   "-1": 600000  |
        /// |                         |                         |                         |                           |   "0": 0        |
        /// |                         |                         |                         |                           |   "1": 3000000  |
        /// |                         |                         |                         |                           | }               |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// </summary>
        [TestMethod]
        public void NonFirstStatsRecord_11_Test()
        {
            // ### Arrange ####################
            var previousClientHistory = generateClient(true);
            var previousSensorHistory = generateSensor(1);
            var clientsHistory = new List<ClientStateHistory>()
            {
                generateClient(false, 30),
                generateClient(true, 40)
            };
            var sensorsHistory = new List<SensorStateHistory>();

            // ### Act ########################
            var result = invokeCalculateHourlyStats(clientsHistory, sensorsHistory, previousClientHistory, previousSensorHistory);

            // ### Assert #####################
            Assert.AreEqual(3, result.Count, "The number of returned state types is incorrect");
            Assert.AreEqual(600000, result[-1], "The 'offline' state duration is wrong");
            Assert.AreEqual(0, result[0], "The 'free' state duration is wrong");
            Assert.AreEqual(3000000, result[1], "The 'occupied' state duration is wrong");
        }

        /// <summary>
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | Previous client history | Previous sensor history | List of clients history |  List of sensors history  | Expected result |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | Online                  | State 1                 | Offline at **:30:00.000 | State 0 from **:20:00.000 | {               |
        /// |                         |                         |                         |                           |   "-1": 1800000 |
        /// |                         |                         |                         |                           |   "0": 600000   |
        /// |                         |                         |                         |                           |   "1": 1200000  |
        /// |                         |                         |                         |                           | }               |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// </summary>
        [TestMethod]
        public void NonFirstStatsRecord_12_Test()
        {
            // ### Arrange ####################
            var previousClientHistory = generateClient(true);
            var previousSensorHistory = generateSensor(1);
            var clientsHistory = new List<ClientStateHistory>()
            {
                generateClient(false, 30)
            };
            var sensorsHistory = new List<SensorStateHistory>()
            {
                generateSensor(0, 20)
            };

            // ### Act ########################
            var result = invokeCalculateHourlyStats(clientsHistory, sensorsHistory, previousClientHistory, previousSensorHistory);

            // ### Assert #####################
            Assert.AreEqual(3, result.Count, "The number of returned state types is incorrect");
            Assert.AreEqual(1800000, result[-1], "The 'offline' state duration is wrong");
            Assert.AreEqual(600000, result[0], "The 'free' state duration is wrong");
            Assert.AreEqual(1200000, result[1], "The 'occupied' state duration is wrong");
        }

        /// <summary>
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | Previous client history | Previous sensor history | List of clients history |  List of sensors history  | Expected result |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | Online                  | State 1                 | Offline at **:30:00.000 | State 0 from **:10:00.000 | {               |
        /// |                         |                         | Online at **:40:00.000  | State 1 from **:50:00.000 |   "-1": 600000  |
        /// |                         |                         |                         |                           |   "0": 1800000  |
        /// |                         |                         |                         |                           |   "1": 1200000  |
        /// |                         |                         |                         |                           | }               |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// </summary>
        [TestMethod]
        public void NonFirstStatsRecord_13_Test()
        {
            // ### Arrange ####################
            var previousClientHistory = generateClient(true);
            var previousSensorHistory = generateSensor(1);
            var clientsHistory = new List<ClientStateHistory>()
            {
                generateClient(false, 30),
                generateClient(true, 40)
            };
            var sensorsHistory = new List<SensorStateHistory>()
            {
                generateSensor(0, 10),
                generateSensor(1, 50)
            };

            // ### Act ########################
            var result = invokeCalculateHourlyStats(clientsHistory, sensorsHistory, previousClientHistory, previousSensorHistory);

            // ### Assert #####################
            Assert.AreEqual(3, result.Count, "The number of returned state types is incorrect");
            Assert.AreEqual(TimeSpan.FromMinutes(10).TotalMilliseconds, result[-1], "The 'offline' state duration is wrong");
            Assert.AreEqual(TimeSpan.FromMinutes(30).TotalMilliseconds, result[0], "The 'free' state duration is wrong");
            Assert.AreEqual(TimeSpan.FromMinutes(20).TotalMilliseconds, result[1], "The 'occupied' state duration is wrong");
        }

        /// <summary>
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | Previous client history | Previous sensor history | List of clients history |  List of sensors history  | Expected result |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// | Offline                 | State 1                 | Online at **:10:00.000  | State 0 from **:30:00.000 | {               |
        /// |                         |                         | Offline at **:20:00.000 | State 1 from **:35:00.000 |   "-1": 900000  |
        /// |                         |                         | Online at **:25:00.000  | State 0 from **:40:00.000 |   "0": 900000   |
        /// |                         |                         |                         | State 1 from **:50:00.000 |   "1": 1800000  |
        /// |                         |                         |                         |                           | }               |
        /// +-------------------------+-------------------------+-------------------------+---------------------------+-----------------+
        /// </summary>
        [TestMethod]
        public void NonFirstStatsRecord_14_Test()
        {
            // ### Arrange ####################
            var previousClientHistory = generateClient(false);
            var previousSensorHistory = generateSensor(1);
            var clientsHistory = new List<ClientStateHistory>()
            {
                generateClient(true, 10),
                generateClient(false, 20),
                generateClient(true, 25)
            };
            var sensorsHistory = new List<SensorStateHistory>()
            {
                generateSensor(0, 30),
                generateSensor(1, 35),
                generateSensor(0, 40),
                generateSensor(1, 50)
            };

            // ### Act ########################
            var result = invokeCalculateHourlyStats(clientsHistory, sensorsHistory, previousClientHistory, previousSensorHistory);

            // ### Assert #####################
            Assert.AreEqual(3, result.Count, "The number of returned state types is incorrect");
            Assert.AreEqual(900000, result[-1], "The 'offline' state duration is wrong");
            Assert.AreEqual(900000, result[0], "The 'free' state duration is wrong");
            Assert.AreEqual(1800000, result[1], "The 'occupied' state duration is wrong");
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


        private ClientStateHistory generateClient(bool isOnline, int minutesPortion = -1)
        {
            return new ClientStateHistory
            {
                IsOnline = isOnline,
                StateChangedTimestamp = getDateTimeFromMinutes(minutesPortion)
            };
        }

        private SensorStateHistory generateSensor(int state, int minutesPortion = -1)
        {
            return new SensorStateHistory
            {
                State = state,
                StateChangedTimestamp = getDateTimeFromMinutes(minutesPortion)
            };
        }

        private DateTime getDateTimeFromMinutes(int minutesPortion)
        {
            if (minutesPortion > 0)
                // current hour history record
                return new DateTime(1979, 5, 30, 20, minutesPortion, 0, DateTimeKind.Utc);
            else
                // previous history records
                return new DateTime(1979, 5, 30, 19, 30, 0, DateTimeKind.Utc);
        }
    }
}
