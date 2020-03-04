namespace RaceMonitor
{
    /// <summary>
    /// Event that is raised when there is a change in speed
    /// </summary>
    public class SpeedChangedEventArgs
    {
        public long Timestamp { get; }
        public int Index { get; }
        public double SpeedMph { get; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="timestamp">timestamp for the speed event</param>
        /// <param name="index">car index</param>
        /// <param name="speedMph">the speed in mph</param>
        public SpeedChangedEventArgs(long timestamp, int index, double speedMph)
        {
            Timestamp = timestamp;
            Index = index;
            SpeedMph = speedMph;
        }
    }
}