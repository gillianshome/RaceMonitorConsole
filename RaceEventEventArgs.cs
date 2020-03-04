namespace RaceMonitor
{
    /// <summary>
    /// Event that is raised when there is an event in the race
    /// </summary>
    public class RaceEventEventArgs
    {
        public long Timestamp { get; }
        public string Message { get; }

        public RaceEventEventArgs(long timestamp, string message)
        {
            Timestamp = timestamp;
            Message = message;
        }
    }
}