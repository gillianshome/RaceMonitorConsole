using Newtonsoft.Json;

namespace RaceMonitor
{
    /// <summary>
    /// Car Coordinates class for data provided by the GPS Source.
    /// The format is 
    ///     {
    ///         timestamp: number,
    ///         carIndex: number,
    ///         location: {
    ///             lat: float,
    ///             long: float
    ///         }
    ///     }
    /// 
    /// an example: 
    ///     {
    ///         "timestamp": 1541693114862,
    ///         "carIndex": 2,
    ///         "location": {
    ///             "lat": 51.349937311969725,
    ///             "long": -0.544958142167281
    ///         }
    ///     }
    /// </summary>
    public class JCarCoords
    {
        #region data    
        /// <summary>
        /// timestamp for this data
        /// </summary>
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        /// <summary>
        /// Unique car identifier
        /// </summary>
        [JsonProperty("carIndex")]
        public int CarIndex { get; set; }

        /// <summary>
        /// Location of the car at this time
        /// </summary>
        [JsonProperty("location")]
        public JLocation Location { get; set; }
        #endregion

        public override string ToString()
        {
            return $"{Timestamp}: Car {CarIndex} at {Location}";
        }
    }
}