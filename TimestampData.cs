using System;
using System.Linq;

namespace RaceMonitor
{
    /// <summary>
    /// helper class for passing around data related to a timestamp for generating 
    /// enhanced race events
    /// </summary>
    class TimestampData
    {
        /// <summary>
        /// the current timestamp
        /// </summary>
        public long Timestamp { get; set; }
        /// <summary>
        /// look for and report enhanced race events, for example overtaking moves
        /// </summary>
        static private int[] oldPositions = null;
        /// <summary>
        /// interface for race events and position updates
        /// </summary>
        private readonly IRaceEvent RaceEvent;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="timestamp">timestamp at which the data occurred</param>
        /// <param name="raceEvent">the interface for reporting race events</param>
        public TimestampData(long timestamp, IRaceEvent raceEvent)
        {
            Timestamp = timestamp;
            this.RaceEvent = raceEvent;
        }

        /// <summary>
        /// generate additional race events resulting from an update to the car positions, 
        /// including overtaking 
        /// </summary>
        /// <param name="timestamp">time of position update </param>
        /// <param name="newPositions">the new positions</param>
        public void GenerateEnhancedRaceEvents(int[] newPositions)
        {
            // looking for changes of position
            if (oldPositions != null)
            {
                // flag to turn off reporting when data about a new car is added to the system
                bool report = newPositions.Length == oldPositions.Length;

                // find all the events that have changed
                var diff_check = newPositions.Zip(oldPositions, (x, y) => !x.Equals(y));
                bool changes = false;
                int index = 0;
                string summary = "";
                foreach (var item in diff_check)
                {
                    char ch = item ? '<' : '=';
                    summary += $"{newPositions[index]}{ch}{oldPositions[index]} ";
                    if (item && newPositions[index] > oldPositions[index])
                    {
                        // position has changed and item refers to a car that overtook another 
                        if (report)
                        {
                            NewRaceEvent($"Car {newPositions[index]} has overtaken {oldPositions[index]} into position {index}");
                        }
                        changes = true;
                    }

                    // update the position of this car
                    RaceEvent.UpdatePosition(index, newPositions[index]);

                    index++;
                }
                if (changes && report && false)
                {
                    NewRaceEvent($"{summary}");
                }
            }

            // store the new positions for use next time around
            oldPositions = newPositions;
        }
        
        private void NewRaceEvent(string message)
        {
            RaceEvent.Event(this, new RaceEventEventArgs(Timestamp, message));
        }
    }
}