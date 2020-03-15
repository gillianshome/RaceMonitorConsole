using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceMonitor;

namespace RaceUnitTest
{
    [TestClass]
    public class RaceTrackUnitTest
    {
        [TestMethod]
        public void TestCompassDirections()
        {
            double north = TestCompassDirection(0, 1);
            Assert.AreEqual(0, north);

            double south = TestCompassDirection(0, -1);
            Assert.AreEqual(180, south);

            double east = TestCompassDirection(1, 0);
            Assert.AreEqual(90, east);

            double west = TestCompassDirection(-1, 0);
            Assert.AreEqual(-90, west);

            double northeast = TestCompassDirection(1, 1);
            Assert.AreEqual(45, northeast);

            double southeast = TestCompassDirection(1, -1);
            Assert.AreEqual(135, southeast);

            double southwest = TestCompassDirection(-1, -1);
            Assert.AreEqual(-135, southwest);

            double northwest = TestCompassDirection(-1, 1);
            Assert.AreEqual(-45, northwest);
        }

        /// <summary>
        /// method to calculate a compass direction
        /// </summary>
        /// <param name="latOffset">latitude offset</param>
        /// <param name="lonOffset">longitude offset</param>
        /// <returns></returns>
        public double TestCompassDirection(double latOffset, double lonOffset)
        {
            // setup a point using the same latitude as the centre
            JLocation point = new JLocation(RaceTrack.Instance.Centre.Lat + latOffset, RaceTrack.Instance.Centre.Lon + lonOffset);
            return RaceTrack.Instance.GetAngleToCentre(point);
        }

        [TestMethod]
        public void TestNewLap()
        {
            // Arrange
            JLocation[] locations = new JLocation[]{
                new JLocation(52.0689492713318, -1.02262936850151),
                new JLocation(52.0690921288708, -1.02245453206889),
                /* crossed start - finish line */
                new JLocation(52.069237340058, -1.02227681364904)
            };

            long timestamp = 10000;
            Car car = new Car(new JCarCoords() { Timestamp = timestamp, CarIndex = 1, Location = locations[0] });
            _ = car.LapAngle;
            car.UpdateCoordinates(new JCarCoords() { Timestamp = ++timestamp, CarIndex = 1, Location = locations[1] });
            _ = car.LapAngle;

            // Arrange
            car.UpdateCoordinates(new JCarCoords() { Timestamp = ++timestamp, CarIndex = 1, Location = locations[2] });
            _ = car.LapAngle;

            // Assert
            Assert.AreEqual(1, car.Lap);
        }

        //[TestMethod]
        //public void TestChangePosisitons()
        //{
        //    // Arrange
        //    JLocation beforeFinish = new JLocation(START_LAT - 0.01, START_LON - 0.01);
        //    JLocation afterStart = new JLocation(START_LAT - 0.01, START_LON + 0.01);
        //    long timestamp = 10000;

        //    RaceMonitor.RaceMonitor monitor = RaceMonitor.RaceMonitor.Instance;

        //    //            loc = new JLocation(loc.Lat, loc.Lon);

        //    monitor.HandleCarDataEvent(new JCarCoords() { Timestamp = timestamp, CarIndex = 1, Location = beforeFinish });
        //    monitor.HandleCarDataEvent(new JCarCoords() { Timestamp = ++timestamp, CarIndex = 1, Location = afterStart });

        //    // Act
        //    monitor.HandleCarDataEvent(new JCarCoords() { Timestamp = ++timestamp, CarIndex = 1, Location = beforeFinish });
        //    monitor.HandleCarDataEvent(new JCarCoords() { Timestamp = ++timestamp, CarIndex = 1, Location = afterStart });


        //    // Assert
        //    //            monitor.Cars[1].;



        //    //          Car car = new Car(beforeFinish);
        //}

        /// <summary>
        /// helper class to provide insight into the events reported 
        /// </summary>
        class RaceEventHelper : IRaceEvent
        {
            public RaceEventHelper()
            {
                Message = null;
                CarIndex = -1;
                Position = -1;
            }

            public string Message { get;  set; }
            public int CarIndex { get;  set; }
            public int Position { get;  set; }

            public void Event(object source, RaceEventEventArgs args)
            {
                Message = args.Message;
            }

            public void UpdatePosition(int carIndex, int newPosition)
            {
                CarIndex = carIndex;
                Position = newPosition;
            }
        }
        /// <summary>
        /// event helper instance
        /// </summary>
        private RaceEventHelper eventHelper;

        [TestMethod]
        public void TestNoOvertake()
        {
            // Arrange - set up the initial positions
            long timestamp = 10000;

            eventHelper = new RaceEventHelper();
            TimestampData timestampData = new TimestampData(timestamp, eventHelper);
            timestampData.GenerateEnhancedRaceEvents(new int[] { 1, 2, 3 });
            // Act, unchanged positions
            timestampData.GenerateEnhancedRaceEvents(new int[] { 1, 2, 3 });

            // Assert, no overtake report is expected
            Assert.IsNull(eventHelper.Message);
        }
        

        [TestMethod]
        public void TestOvertake()
        {
            // Arrange - add two cars to the race
            long timestamp = 10000;
            eventHelper = new RaceEventHelper();
            TimestampData timestampData = new TimestampData(timestamp, eventHelper);
            timestampData.GenerateEnhancedRaceEvents(new int[] { 1, 2, 3 });
            // Act, unchanged positions
            timestampData.GenerateEnhancedRaceEvents(new int[] { 1, 3, 2 });

            // Assert, overtake report is expected
            Assert.AreEqual("Car 3 has overtaken 2 into position 1", eventHelper.Message);
        }

        [TestMethod]
        public void TestSpeed()
        {
            // Arrange
            JLocation[] locations = new JLocation[]{
                new JLocation(0, 0),
                /* move latitudinally one unit (approx 111km = 69 miles) */
                new JLocation(1, 0),
                /* move longitudinally one unit (approx 111km = 69 miles) */
                new JLocation(1, 1),
            };

            long timestamp = 0;
            Car car = new Car(new JCarCoords() { Timestamp = timestamp, CarIndex = 1, Location = locations[0] });

            // Act
            // adjust time by 1 hour
            timestamp = 3600000;
            car.UpdateCoordinates(new JCarCoords() { Timestamp = timestamp, CarIndex = 1, Location = locations[1] });

            // Assert
            Assert.IsTrue(69 < car.SpeedMph && car.SpeedMph < 70, $"Wrong speed {car.SpeedMph}");
        }
    }
}
