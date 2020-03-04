using System;

namespace RaceMonitor
{
    /// <summary>
    /// A car object stores the latest location information and if more than one
    /// location has been received can store position and speed information.
    /// </summary>
    class Car
    {
        #region data
        /// <summary>
        /// unique car identifier 
        /// </summary>
        public int Index { private set; get; }
        /// <summary>
        /// last time of coordinates report from the car
        /// </summary>
        public long Timestamp { private set; get; }
        /// <summary>
        /// last reported location of the car
        /// </summary>
        public JLocation Location 
        {
            private set
            {
                // support lazy instantiation of angle value
                angle = null;
                location = value;
            }
            get => location; 
        }
        /// <summary>
        /// calculated position in race relative to tracked cars
        /// generates PositionChangedEventArgs<> events when the value changes
        /// </summary>
        private int position = -1;      // not in the race              
        public int Position
        {
            get => position;
            set
            {
                bool changed = position != value;
                if (changed)
                {
                    // Copy to a temporary variable to be thread-safe.
                    EventHandler<PositionChangedEventArgs> temp = PositionEvent;
                    if (temp != null)
                    {
                        PositionEvent(this, new PositionChangedEventArgs(Timestamp, Index, position, value));
                    }
                }
                position = value;
            }
        }

        /// <summary>
        /// calculated speed of this car, derived from the time and distance between 
        /// the last reported location and the location previously reported 
        /// </summary>
        private double speedMph;
        /// <summary>
        /// local value that supports lazy instantiation of Angle value
        /// </summary>
        private Nullable<double> angle;
        /// <summary>
        /// car's location value
        /// </summary>
        private JLocation location;
        private bool newLap = false;

        public double SpeedMph
        {
            get => speedMph;
            private set
            {
                speedMph = value;
                // Copy to a temporary variable to be thread-safe.
                EventHandler<SpeedChangedEventArgs> temp = SpeedEvent;
                if (temp != null)
                {
                    SpeedEvent(this, new SpeedChangedEventArgs(Timestamp, Index, SpeedMph));
                }
            }
        }
        public double Angle
        {
            get
            {
                // lazy instantiation of angle value
                angle = RaceTrack.Instance.GetAngleToCentre(Location);
                return angle.Value;
            }
            internal set
            {
                if (angle.HasValue)
                {
                    NewLap = RaceTrack.Instance.HasPassedFinishLine(angle.Value, value);
                }
                angle = value;
            }
        }

        public bool NewLap
        {
            get => newLap;
            internal set 
            {
                if (value)
                {
                    // add another lap 
                    Lap++;
                }
                newLap = value; 
            }
        }
        public Int16 Lap { get; internal set; }

        /// <summary>
        /// Declare an event of delegate type EventHandler of SpeedChangedEventArgs.
        /// register a listener by   SpeedEvent += Car_SpeedEvent;
        /// where the signature is   void Car_SpeedEvent(object sender, SpeedChangedEventArgs e) {}
        /// </summary>
        public event EventHandler<SpeedChangedEventArgs> SpeedEvent;
        /// <summary>
        /// Declare an event of delegate type EventHandler of PositionChangedEventArgs.
        /// </summary>
        public event EventHandler<PositionChangedEventArgs> PositionEvent;
        #endregion

        #region methods
        /// <summary>
        /// constructor, store the location of this car
        /// </summary>
        /// <param name="coords"></param>
        /// <param name="speedChangedListener"></param>
        public Car(JCarCoords coords, 
            EventHandler<SpeedChangedEventArgs> speedChangedListener = null, 
            EventHandler<PositionChangedEventArgs> positionChangedListener = null)
        {
            // default values
            Index = coords.CarIndex;
            Timestamp = coords.Timestamp;
            SpeedEvent += speedChangedListener;
            PositionEvent += positionChangedListener;

            // store the current location 
            Location = new JLocation(coords.Location.Lat, coords.Location.Lon);
        }
        
        /// <summary>
        /// update the car location properties and calculate speed from the 
        /// new coordinates provided
        /// </summary>
        /// <param name="coords">new coordinates</param>
        public void UpdateCoordinates(JCarCoords coords)
        {
            // calculate the distance the car has moved
            var dist = Distance(Location.Lat, Location.Lon, coords.Location.Lat, coords.Location.Lon);
            if (dist != 0)
            {
                SpeedMph = CalculateSpeedMph(coords.Timestamp - Timestamp, dist);
            }
            else
            {
                // the car has not moved
                SpeedMph = 0;
            }

            // store the new coordinate information
            Location.Update(coords.Location.Lat, coords.Location.Lon);
            Timestamp = coords.Timestamp;
        }

        /// <summary>
        /// calculate the speed
        /// </summary>
        /// <param name="time">time taken</param>
        /// <param name="dist">distance travelled</param>
        /// <returns>speed in miles per hour</returns>
        private static double CalculateSpeedMph(long time, double dist)
        {
            var time_s = time / 1000.0;
            // meters per second
            double speed_mps = dist / time_s;
            // convert to miles per hour
            return speed_mps * 2236.94;
        }

        /// <summary>
        /// This routine calculates the distance between two points (given the
        /// latitude/longitude of those points). 
        /// Definitions:
        ///     South latitudes are negative, east longitudes are positive           
        /// </summary>
        /// <param name="lat1">Latitude of point 1 (in decimal degrees)</param>
        /// <param name="lon1">Longitude of point 1 (in decimal degrees)</param>
        /// <param name="lat2">Latitude of point 2 (in decimal degrees)</param>
        /// <param name="lon2">Longitude of point 2 (in decimal degrees)</param>
        /// <returns></returns>
        private static double Distance(double lat1, double lon1, double lat2, double lon2)
        {
            if ((lat1 == lat2) && (lon1 == lon2))
            {
                // positions are the same
                return 0;
            }
            else
            {
                double theta = lon1 - lon2;
                double dist = Math.Sin(Deg2rad(lat1)) * Math.Sin(Deg2rad(lat2)) +
                    Math.Cos(Deg2rad(lat1)) * Math.Cos(Deg2rad(lat2)) * Math.Cos(Deg2rad(theta));
                dist = Math.Acos(dist);
                dist = Rad2deg(dist);
                dist = dist * 60 * 1.1515;

                // convert to Kilometers
                dist *= 1.609344;
                return (dist);
            }
        }

        /// <summary>
        /// converts decimal degrees to radians
        /// </summary>
        /// <param name="deg">decimal degrees</param>
        /// <returns>radians</returns>
        private static double Deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        /// <summary>
        /// converts radians to decimal degrees
        /// </summary>
        /// <param name="rad">radians</param>
        /// <returns>decimal degrees</returns>
        private static double Rad2deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }
        #endregion
    }
}

#if false        /// <summary>
        /// calculated direction derived from the current and previous positions
        /// </summary>
        internal Direction Direction { private set; get; }

        /// <summary>
        /// a list of directions that the car has travelled
        /// </summary>
        internal List<Direction> history = new List<Direction>();
        /// <summary>
        /// segment of the track that this car is on
        /// </summary>
        internal TrackSegment Segment { private set; get; }
        /// <summary>
        /// 
        /// </summary>

updatelocation
                // the car is moving, calculate the direction 
                Direction = CalculateDirection(Location.Lat, Location.Lon, coords.Location.Lat, coords.Location.Lon);
                if (Segment == null)
                {
                    Segment = new TrackSegment(Direction);
                }
                else
                {
                    Segment.SetDirection(Direction);
                }
#endif