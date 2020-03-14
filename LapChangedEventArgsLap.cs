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
        public long LapDurationMs { get; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="timestamp">timestamp for the lap event</param>
        /// <param name="index">car index</param>
        /// <param name="lap">the lap number</param>
        /// <param name="durationMs">duration of the lap just completed</param>
        public LapChangedEventArgs(long timestamp, int index, double lap, long durationMs)
        {
            Timestamp = timestamp;
            Index = index;
            Lap = lap;
            LapDurationMs = durationMs;
        }
    }
}