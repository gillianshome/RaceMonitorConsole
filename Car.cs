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
                if (angle.HasValue)
                {
                    lastAngle = angle;
                }
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
        
        /// <summary>
        /// the angle from the 'centre point' of the race track to the position of this car.
        /// Angles are relative, in the range [-180..180] and only calculated when needed, the 
        /// value is set to null when the location is changed and recalculated next time it is used
        /// </summary>
        public double Angle
        {
            get
            {
                if (!angle.HasValue)
                {
                    // lazy instantiation of angle value
                    angle = RaceTrack.Instance.GetAngleToCentre(Location);

                    if (lastAngle.HasValue)
                    {
                        // check for interesting events 
                        if (RaceTrack.Instance.HasPassedFinishLine(angle.Value, lastAngle.Value))
                        {
                            Lap++;
                            // Copy to a temporary variable to be thread-safe.
                            EventHandler<LapChangedEventArgs> temp = LapEvent;
                            if (temp != null)
                            {
                                LapEvent(this, new LapChangedEventArgs(Timestamp, Index, Lap));
                            }
                        }
                    }
                }
                return angle.Value;
            }
            private set { }
        }
        /// <summary>
        /// the angle from the 'centre point' of the race track to the position of this car
        /// taking into account the number of laps completed, a larger LapAngle will always 
        /// be ahead in the race
        /// </summary>
        public double LapAngle { get => Lap * 360 + Angle; private set { } }
        /// <summary>
        /// The angle at the previous location, used to determine the movement of the car
        /// </summary>
        private Nullable<double> lastAngle = null;

        /// <summary>
        /// Count the number of times that this car has passed the start / finish line
        /// </summary>
        private Int16 Lap;

        /// <summary>
        /// Declare an event of delegate type EventHandler of SpeedChangedEventArgs.
        /// register a listener by   SpeedEvent += Car_SpeedEvent;
        /// where the signature is   void Car_SpeedEvent(object sender, SpeedChangedEventArgs e) {}
        /// </summary>
        public event EventHandler<SpeedChangedEventArgs> SpeedEvent;
        /// <summary>
        /// Declare an event of delegate type EventHandler of LapChangedEventArgs.
        /// </summary>
        public event EventHandler<LapChangedEventArgs> LapEvent;
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
            EventHandler<PositionChangedEventArgs> positionChangedListener = null,
            EventHandler<LapChangedEventArgs> lapChangedListener = null)
        {
            // default values
            Index = coords.CarIndex;
            Timestamp = coords.Timestamp;
            SpeedEvent += speedChangedListener;
            PositionEvent += positionChangedListener;
            LapEvent += lapChangedListener;

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