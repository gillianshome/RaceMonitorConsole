using System;
using System.Collections.Generic;
using System.IO;

namespace RaceMonitor
{
    public class RaceTrack
    {
        /// <summary>
        /// location at the centre of the racetrack
        /// </summary>
        public JLocation Centre { get; }
        private double StartFinishAngle;

        private string Name { get; }

        private RaceTrack(string name, JLocation centre, JLocation startFinish) //SortedList<int, Location> trackPoints)
        {
            Name = name;
            Centre = centre;
            StartFinishAngle = GetAngleToCentre(startFinish);
        }

        // the centre of the track where most parts of the track are moving clockwise 
        // 52°04'33.0"N 1°00'56.0"W
        private static readonly JLocation SilverstoneCentre = new JLocation(52.071520, -1.013419);
        //52.075820, -1.015565);
        private static readonly JLocation SilverstoneStartLine = new JLocation(52.069231, -1.022266);
        //52.078611,-1.016944);
        internal static readonly RaceTrack instance = new RaceTrack(
            "Silverstone", SilverstoneCentre, SilverstoneStartLine);

        public static RaceTrack Instance
        {
            get 
            {
                /*
                double north = instance.TestCompassDirection(0, 1);
                double south = instance.TestCompassDirection(0, -1);
                double east = instance.TestCompassDirection(1, 0);
                double west = instance.TestCompassDirection(-1, 0);
                double northeast = instance.TestCompassDirection(1, 1);
                double southeast = instance.TestCompassDirection(1, -1);
                double southeast = instance.TestCompassDirection(1, -1);
                double northwest = instance.TestCompassDirection(-1, 1);


                */
                return instance; 
            }
        }

        public double TestCompassDirection(double latOffset, double lonOffset)
        {
            // setup a point using the same latitude as the centre
            JLocation point = new JLocation(Centre.Lat + latOffset, Centre.Lon + lonOffset);
            return GetAngleToCentre(point);
        }


        /// <summary>
        /// calculate the angle from the location to the "centre" of the racetrack
        /// </summary>
        /// <param name="location">location on the racetrack</param>
        /// <returns></returns>
        public double GetAngleToCentre(JLocation location)
        {
            if (location is null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            double latDelta = location.Lat - Centre.Lat;
            double lonDelta = location.Lon - Centre.Lon;

            return Math.Atan2(latDelta, lonDelta) * (180 / Math.PI);
        }

        /// <summary>
        /// tests whether the finish line is between the two angles provided
        /// </summary>
        /// <param name="angle1">first angle</param>
        /// <param name="angle2">second angle</param>
        /// <returns>only true when the finish line is between the two angles</returns>
        internal bool HasPassedFinishLine(double angle1, double angle2)
        {
            return (StartFinishAngle > angle1 && StartFinishAngle < angle2);
        }

        //TODO remove log function
        public static void WriteLog(string strLog)
        {
            string logFilePath = @"C:\Logs\Silverstone-" + DateTime.Today.ToString("MM-dd-yyyy") + "." + "txt";
            FileInfo logFileInfo = new FileInfo(logFilePath);
            DirectoryInfo logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
            if (!logDirInfo.Exists) logDirInfo.Create();
            using (FileStream fileStream = new FileStream(logFilePath, FileMode.Append))
            {
                using (StreamWriter log = new StreamWriter(fileStream))
                {
                    log.WriteLine(strLog);
                }
            }
        }
    }

    /*

        public Silverstone
            {   
            {"Start/Finish", 52.078611,-1.016944 },
            { "Farm Curve", }
            {"Club Corner",  52.078611,-1.016944 }
            };
    */
}

#if false

        // segments of a racetrack identified by the @Direction of travel
        static private List<Direction> Segments;
        private static readonly List<Direction> SilverstoneSegments = new List<Direction>()
        {
            Direction.NorthEast,      // start-finish straight
            Direction.NorthWest,
            Direction.NorthEast,
            Direction.NorthWest,
            Direction.SouthWest,
            Direction.NorthWest,      // Luffield Corner
            Direction.NorthEast,      // National Pits Straight
            Direction.SouthEast,      // Copse Corner
            Direction.SouthWest,      // after Maggotts Corner
            Direction.SouthEast,      // Chapel Curve
            Direction.SouthWest,      // Hangar Straight
            Direction.NorthWest,      // into Vale
            Direction.SouthWest,
            Direction.NorthWest,      // Club Corner
        };

// constructor
- caller
- param = List<Direction> segments, 
            Segments = segments;

        internal static void GetSegment(TrackSegment trackSegment)
        {

            if (trackSegment.Segment != -1)
            {
                // the segment is already known
                // TODO - check that direction has changed and matches the next segment
                //                RaceTrack.NextSegment(this);
            }

            List<Direction> history = trackSegment.history;
            if (history.Count >= GetUniqueSegmentCount())
            {
                // there is enough information to determine which segment the car is now in

                Nullable<int> newSegment;
                for (int segment = 0; segment < Segments.Count; segment++)
                {
                    newSegment = null;

                    // look for a matching sequence of segments, wrapping back to the start of the segment list as required
                    for (int nth = 0; nth < history.Count; nth++)
                    {
                        if (history[nth] != Segments[(segment + nth) % Segments.Count])
                        {
                            // this is not the correct starting segment, try the next segment as the starting point
                            break;
                        }
                        newSegment = segment;
                    }

                    if (newSegment != null)
                    {
                        // found a match, set the segment and clear the history
                        trackSegment.Segment = newSegment;
                        history.RemoveRange(1, history.Count - 1);
                        break;
                    }
                }
            }
        }


        internal static void NextSegment(TrackSegment trackSegment)
        {
            // assume that the car has moved onto the next segment
            trackSegment.Segment = (trackSegment.Segment + 1) % Segments.Count;
        }

        internal static int GetUniqueSegmentCount()
        {
            // TODO - this will depend on the race track layout to determine the number of segments 
            // needed to determine the segment that a car is travelling on 
            return 3;
        }


#endif