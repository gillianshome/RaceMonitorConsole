using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RaceMonitor
{
    /// <summary>
    /// Location described in Latitude and Longitude values
    /// </summary>
    public class JLocation
    {
        #region data
        [JsonProperty("lat")]
        public double Lat { get; set; }

        [JsonProperty("long")]
        public double Lon { get; set; }
        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="lat">latitude value</param>
        /// <param name="lon">longitude value</param>
        [JsonConstructor()]
        public JLocation(double lat, double lon)
        {
            Lat = lat;
            Lon = lon;
        }

        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="location">location value</param>
        public JLocation(JLocation location)
            : this(location.Lat, location.Lon)
        {
        }

        /// <summary>
        /// change the position
        /// </summary>
        /// <param name="lat">latitude value</param>
        /// <param name="lon">longitude value</param>
        public void Update(double lat, double lon)
        {
            Lat = lat;
            Lon = lon;
        }

        public override string ToString()
        {
            return $"Lat: {Lat}, Long:{Lon}";
        }
    }
}