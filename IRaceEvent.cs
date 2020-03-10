namespace RaceMonitor
{
    internal interface IRaceEvent
    {
        /// <summary>
        /// the race event
        /// </summary>
        /// <param name="source">event source</param>
        /// <param name="args">event arguments</param>
        void Event(object source, RaceEventEventArgs args);

        /// <summary>
        /// Update the racing position of a car
        /// </summary>
        /// <param name="carIndex">the car index value</param>
        /// <param name="newPosition">the positon</param>
        public void UpdatePosition(int carIndex, int newPosition);
    }
}