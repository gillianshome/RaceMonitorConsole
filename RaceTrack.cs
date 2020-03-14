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
        /// <summary>
        /// A special section of track where the clockwise rule does not apply, this array 
        /// contains the angles to the start and finish of the bendy bit of the track
        /// </summary>
        public double[] BendyBit { get; }
        /// <summary>
        /// angle from the centre of the track to the start-finish line
        /// </summary>
        private readonly double StartFinishAngle;
        /// <summary>
        /// track name
        /// </summary>
        private string Name { get; }

        private RaceTrack(string name, JLocation centre, JLocation startFinish, JLocation[] bendyBit)
        {
            Name = name;
            Centre = centre;
            // convert all other locations to angles relative to the centre
            BendyBit = new double[bendyBit.Length];
            for (int i = 0; i < bendyBit.Length; i++)
            {
                BendyBit[i] = GetAngleToCentre(bendyBit[i]);
            }
            StartFinishAngle = GetAngleToCentre(startFinish);
        }

        // the centre of the track where most parts of the track are moving clockwise 
        // 52°04'21.3"N 1°00'48.0"W
        private static readonly JLocation SilverstoneCentre = new JLocation(52.072589, -1.013328);
        private static readonly JLocation SilverstoneStartLine = new JLocation(52.069231, -1.022266);
        private static readonly JLocation LuffieldCorner = new JLocation(52.075741, -1.021487);
        private static readonly JLocation BrooklandsCorner = new JLocation(52.077203, -1.019370);
        /// <summary>
        /// constructor for Silverstone race track
        /// </summary>
        internal static readonly RaceTrack instance = new RaceTrack(
            "Silverstone", 
            SilverstoneCentre, 
            SilverstoneStartLine,
            new JLocation[]{LuffieldCorner, BrooklandsCorner});

        /// <summary>
        /// singleton instance
        /// </summary>
        public static RaceTrack Instance
        {
            get 
            {
                return instance; 
            }
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
        /// Calculates the angle to a location taking into account a bendy bit of the track 
        /// where all locations are adjusted to be equal 
        /// </summary>
        /// <param name="location">the location</param>
        /// <returns>the angle to the location, adjusted if necessary for bendy bits of the track</returns>
        internal double GetAngleForPosition(JLocation location)
        {
            double angle = GetAngleToCentre(location);

            if (BendyBit[0] < angle && angle < BendyBit[1])
            {
                // position by angle is unreliable in the bendy bit of the track
                // so use the angle to the start for all cars on the bend(s)
                angle = BendyBit[0];
            }
            return angle;
        }

        /// <summary>
        /// tests whether the finish line is between the two angles provided
        /// </summary>
        /// <param name="angle1">first angle</param>
        /// <param name="angle2">second angle</param>
        /// <returns>only true when the finish line is between the two angles</returns>
        internal bool HasPassedFinishLine(double angle1, double angle2)
        {
            // the assumption made here is that all tracks are raced in a clockwise direction
            return (angle1 < StartFinishAngle && StartFinishAngle < angle2);
        }

        /// <summary>
        /// Helper function used for logging sample data to a file for testing 
        /// </summary>
        /// <param name="strLog">data entry to be written</param>
        public static void WriteLog(string strLog)
        {
            // TODO - to be more useful the configuration would be read from a file
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

        public override string ToString()
        {
            return $"{Name}";
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
