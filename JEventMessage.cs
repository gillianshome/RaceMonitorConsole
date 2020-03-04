using Newtonsoft.Json;

namespace RaceMonitor
{
    internal class JEventMessage
    {
        #region data
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="timestamp">timestamp of the event</param>
        /// <param name="eventMessage">the event message</param>
        public JEventMessage(long timestamp, string eventMessage)
        {
            Timestamp = timestamp;
            Text = eventMessage;
        }

    }
}