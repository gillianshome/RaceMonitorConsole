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
        private readonly double StartFinishAngle;

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
        private static readonly JLocation LuffieldCornerStart = new JLocation(52.075741, -1.021487);
        private static readonly JLocation LuffieldCornerEnd = new JLocation(52.075929, -1.017760);

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
