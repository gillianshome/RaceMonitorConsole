using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RaceMonitor
{
    /// <summary>
    /// this class monitors car locations, calculating speed and position
    /// of cars in a race around a track. Data input and output is through 
    /// a MQTT broker.
    /// It establishes connections to the for receiving locations
    /// and publishing car speed and position value and 
    /// </summary>
    class RaceMonitor : ICarCoordinates
    {
        #region data
        /// <summary>
        /// the singleton instance
        /// </summary>
        private static readonly RaceMonitor instance = new RaceMonitor();
        /// <summary>
        /// handler for the MQTT connection
        /// </summary>
        private readonly MqttRaceClient client;
        /// <summary>
        /// cars ordered by car index
        /// </summary>
        private static ConcurrentDictionary<int, Car> Cars;

        internal void RunPublisherThread()
        {
            client.RunMessagePublisher();
        }

        /// <summary>
        /// temporary storage for incoming race coordinate data
        /// </summary>
        //private readonly ConcurrentDictionary<TimestampData, List<JCarCoords>> RaceData =
        //    new ConcurrentDictionary<TimestampData, List<JCarCoords>>();

        /// <summary>
        /// Declare an event of delegate type EventHandler of RaceEventEventArgs.
        /// register a listener by   RaceEvent += RaceStatus_RaceEvent;
        /// where the signature is   void RaceStatus_RaceEvent(object sender, RaceEventEventArgs e) {}
        /// </summary>
        public event EventHandler<RaceEventEventArgs> RaceEvent;
        #endregion

        /// <summary>
        /// The instance of the RaceMonitor that manages to the Mqtt message processing.
        /// </summary>
        public static RaceMonitor Instance
        {
            get { return instance; }
        }


        /// <summary>
        /// private for singleton implementation
        /// </summary>
        private RaceMonitor()
        {
            // set up to receive race data from the MQTT connection
            client = new MqttRaceClient(this);

            // prepare storage for the current car data
            Cars = new ConcurrentDictionary<int, Car>();
            RaceEvent += RaceMonitor_RaceEvent;
        }

        internal void UpdatePosition(int index, int newPosition)
        {
            Car car = Cars[index];
            car.Position = newPosition;
        }

        /// <summary>
        /// send a race event message
        /// </summary>
        /// <param name="sender">event source</param>
        /// <param name="e">event arguments</param>
        private void RaceMonitor_RaceEvent(object sender, RaceEventEventArgs e)
        {
            client.SendRaceEvent(e.Timestamp, e.Message);
        }

        /// <summary>
        /// send a position changed event
        /// </summary>
        /// <param name="sender">event source</param>
        /// <param name="e">event arguments</param>
        private void RaceMonitor_PositionEvent(object sender, PositionChangedEventArgs e)
        {
            client.SendPosition(e.Timestamp, e.Index, e.NewPosition);

            // report changes in postition, except for the first reported position
            if (e.OldPosition != -1)
            {
                NewRaceEvent(e.Timestamp, $"car {e.Index} moved to {e.NewPosition} from {e.OldPosition}");
            }
        }

        internal void NewRaceEvent(long timestamp, string message)
        {
            // Copy to a temporary variable to be thread-safe.
            EventHandler<RaceEventEventArgs> temp = RaceEvent;
            if (temp != null)
            {
                RaceEvent(this, new RaceEventEventArgs(timestamp, message));
            }
        }

        /// <summary>
        /// establish a connection
        /// </summary>
        internal void Connect()
        {
            try
            {
                client.Connect();
                NewRaceEvent(1234, $"Connecting {client} to data source");
            }
            catch (SystemException)
            {
                // could not connect
                Console.WriteLine("Failed to establish connection, is the data source for {client} available?");
            }
        }

        class TimestampData
        {
            public long Timestamp { get; private set; }

            public TimestampData(long timestamp)
            {
                Timestamp = timestamp;
            }
        }

        /// <summary>
        /// object containing data relating to a single timestamp
        /// </summary>
        TimestampData currentTimeData = null;
        /// <summary>
        /// implementation of the ICarCoordinates interface
        /// </summary>
        /// <param name="coords">car coordinate data</param>
        public void ProcessRaceData(JCarCoords coords)
        {
            if (currentTimeData == null)
            {
                // set up the first timestamp object
                currentTimeData = new TimestampData(coords.Timestamp);
            }
            else if (currentTimeData.Timestamp != coords.Timestamp)
            {
                // data has arrived with a new timestamp, work out the positions 
                // of the cars for the previous timestamp
                RecalculatePositions(currentTimeData);
            }

            HandleCarDataEvent(coords);
        }

        /// <summary>
        /// recalculate the race positions of the cars at a single time
        /// </summary>
        /// <param name="timestampData">the data for a timestamp</param>
        private void RecalculatePositions(TimestampData timestampData)
        {
            // collect the latest angles of all cars
            Car car;
            double[] angles = new double[Cars.Count];
            int[] carIndexes = new int[Cars.Count];
            int index = 0;
            foreach (var key in Cars.Keys)
            {
                car = Cars[key];
                angles[index] = car.Angle;
                carIndexes[index] = car.Index;
                index++;
            }

            // ensure that all entries are in race order
            Array.Sort(angles, carIndexes);

            GenerateEnhancedRaceEvents(timestampData, carIndexes);
        }

        /// <summary>
        /// the lap number of the car leading the race
        /// </summary>
        private double leadingLap = 1;

        /// <summary>
        /// look for and report enhanced race events, for example overtaking moves
        /// </summary>
        static private int[] oldPositions = null;

        private void GenerateEnhancedRaceEvents(TimestampData timestampData, int[] newPositions)
        {
            if (oldPositions != null)
            {
                // find all the events that have changed
                var diff_check = newPositions.Zip(oldPositions, (x, y) => !x.Equals(y));
                bool changes = false;
                int index = 0;
                string summary = "";
                foreach (var item in diff_check)
                {
                    char ch = item ? '<' : '=';
                    summary += $"{newPositions[index]}{ch}{oldPositions[index]} ";
                    if (item && newPositions[index] < oldPositions[index])
                    {
                        // position has changed and item refers to a car that overtook another 
                        NewRaceEvent(timestampData.Timestamp, $"Car {newPositions[index]} has overtaken {oldPositions[index]} into position {index}");
                        changes = true;
                    }

                    // update the position of this car
                    try
                    {
                        Car car = Cars[index];
                        car.Position = newPositions[index];

                        index++;

                    }
                    catch (KeyNotFoundException)
                    {
                        // this car was not present in the old positions, wait until it has moved before
                        // reporting a change in position
                        throw;
                    }                
                }
                if (changes)
                {
                    NewRaceEvent(timestampData.Timestamp, $"{summary}");
                }
            }

            // store the new positions for use next time around
            oldPositions = newPositions;
        }

        /// <summary>
        /// process a new coordinates value for a car, work out the speed and race positions
        /// </summary>
        /// <param name="coords">new car coordinates</param>
        void HandleCarDataEvent(JCarCoords coords)
        {
            // TODO check whether it is computationally expensive to provide the new value 
            // as this will only be needed each time data about a new car is received
            Car car = Cars.GetOrAdd(coords.CarIndex,
                new Car(coords, RaceMonitor_SpeedEvent, RaceMonitor_PositionEvent, RaceMonitor_LapEvent));

            // store the new coordinates
            Cars[coords.CarIndex].UpdateCoordinates(coords);

        }

        private void RaceMonitor_LapEvent(object sender, LapChangedEventArgs e)
        {
            if (leadingLap < e.Lap)
            {
                NewRaceEvent(e.Timestamp,
                    $"Car {e.Index} starts a new lap in the lead");
                leadingLap = e.Lap;
            }
        }

        /// <summary>
        /// send a speed update
        /// </summary>
        /// <param name="sender">event source</param>
        /// <param name="e">event arguments</param>
        private void RaceMonitor_SpeedEvent(object sender, SpeedChangedEventArgs e)
        {
            client.SendSpeed(e.Timestamp, e.Index, (int)e.SpeedMph);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public void PerformTask(JCarCoords coords)
        {
            ProcessRaceData(coords);
        }
    }
}
