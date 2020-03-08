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
        internal int[] newPositions;

        public long Timestamp { get; private set; }
        public int Size 
        {
            get => newPositions.Length; 
            internal set
            {
                newPositions = new int[value];
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="timestamp">timestamp at which the data occurred</param>
        public TimestampData(long timestamp)
        {
            Timestamp = timestamp;
        }

        internal void AddCar(int index, int carIndex)
        {
            newPositions[index] = carIndex;
        }

        internal static void GenerateEnhancedRaceEvents(object obj)
        {
            TimestampData data = (TimestampData)obj;
            data.GenerateEnhancedRaceEvents();
        }

        /// <summary>
        /// look for and report enhanced race events, for example overtaking moves
        /// </summary>
        static private int[] oldPositions = null;

         private void GenerateEnhancedRaceEvents(//TimestampData timestampData, int[] newPositions
             )
        {
            if (oldPositions != null)
            {
                // find all the events that have changed
                var diff_check = newPositions.Zip(oldPositions, (x, y) => !x.Equals(y));
                bool changes = false;
                int index = 0;
                string summary = "";
                foreach (var item in diff_check)
                {
                    char ch = item ? '<' : '=';
                    summary += $"{newPositions[index]}{ch}{oldPositions[index]} ";
                    if (item && newPositions[index] < oldPositions[index])
                    {
                        // position has changed and item refers to a car that overtook another 
                        NewRaceEvent($"Car {newPositions[index]} has overtaken {oldPositions[index]} into position {index}");
                        changes = true;
                    }

                    // update the position of this car
                    RaceMonitor.Instance.UpdatePosition(index, newPositions[index]);

                    index++;
                }
                if (changes)
                {
                    NewRaceEvent($"{summary}");
                }
            }

            // store the new positions for use next time around
            oldPositions = newPositions;
        }
        
        private void NewRaceEvent(string message)
        {
            RaceMonitor.Instance.NewRaceEvent(Timestamp, message);
        }
    }
}