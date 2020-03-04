using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceMonitor
{
    /// <summary>
    /// interface representing handler for incoming race data
    /// </summary>
    interface ICarCoordinates
    {
        /// <summary>
        /// handle new race data message 
        /// </summary>
        /// <param name="coords">car coordinate data</param>
        void ProcessRaceData(JCarCoords coords);
    }
}
