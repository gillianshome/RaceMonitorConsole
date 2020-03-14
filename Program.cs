using System;
using System.Threading;

namespace RaceMonitor
{
    class Program
    {
        /// <summary>
        /// program entry point
        /// </summary>
        /// <param name="args">command line arguments</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Currently unused")]
        static void Main(string[] args)
        {
            try
            {
                RunRaceMonitor();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception caught: " + e.ToString());
            }
            while (true)
            {
                Thread.Sleep(2000);
            }
        }
        
        /// <summary>
        /// start the race monitor
        /// </summary>
        private static void RunRaceMonitor()
        {
            // connect to the telemetry data and run forever to processes it
            // TODO handle the case when there is no MQTT broker started
            RaceMonitor.Instance.Connect();
        }
    }
}
