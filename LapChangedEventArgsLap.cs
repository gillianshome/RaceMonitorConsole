namespace RaceMonitor
{
    /// <summary>
    /// Event that is raised when there is a change in lap
    /// </summary>
    public class LapChangedEventArgs
    {
        public long Timestamp { get; }
        public int Index { get; }
        public double Lap { get; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="timestamp">timestamp for the lap event</param>
        /// <param name="index">car index</param>
        /// <param name="lap">the lap number</param>
        public LapChangedEventArgs(long timestamp, int index, double lap)
        {
            Timestamp = timestamp;
            Index = index;
            Lap = lap;
        }
    }
}