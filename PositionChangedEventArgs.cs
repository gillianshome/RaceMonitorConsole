namespace RaceMonitor
{
    /// <summary>
    /// Event that is raised when there is a change in race position
    /// </summary>
    public class PositionChangedEventArgs
    {
        public long Timestamp { get; }
        public int Index { get; }
        public int OldPosition { get; }
        public int NewPosition { get; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="timestamp">timestamp for the speed event</param>
        /// <param name="index">car index</param>
        /// <param name="oldPosition">the previous race position of this car</param>
        /// <param name="newPosition">the new race position of this car</param>
        public PositionChangedEventArgs(long timestamp, int index, int oldPosition, int newPosition)
        {
            Timestamp = timestamp;
            Index = index;
            OldPosition = oldPosition;
            NewPosition = newPosition;
        }
    }
}