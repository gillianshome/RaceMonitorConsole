using Newtonsoft.Json;

namespace RaceMonitor
{
    /// <summary>
    /// Car Status class for data derived from car coordinates.
    /// The format is 
    /// {
    ///     timestamp: number,
    ///     carIndex: number,
    ///     type: string<SPEED|POSITION>,
    ///     value: number
    /// }
    /// an example: 
    /// {
    ///     "timestamp": 1541693114862,
    ///     "carIndex": 2,
    ///     "type": "SPEED",
    ///     "value": 100
    /// }
    /// </summary>
    public class JCarStatus
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
        /// Status type
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }
        /// <summary>
        /// status value
        /// </summary>
        [JsonProperty("value")]
        public int Value { get; set; }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public JCarStatus()
        {

        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="timestamp">timestamp of the status data</param>
        /// <param name="index">car index</param>
        /// <param name="type">status type (SPEED | POSITION)</param>
        /// <param name="value">the data value</param>
        public JCarStatus(long timestamp, int index, string type, int value)
        {
            Timestamp = timestamp;
            CarIndex = index;
            Type = type;
            Value = value;
        }
    }
}