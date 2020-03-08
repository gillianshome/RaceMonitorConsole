using System;
using System.Threading;

namespace RaceMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            bool restart = false;
            int i = 0;
            while (i < 20)
            {

                try
                {
                    RunRaceMonitor(restart);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception caught: " + e.ToString());
                    restart = true;
                }               
                Thread.Sleep(2000);

                // run the worker thread - this blocks to process tasks
                RaceMonitor.Instance.RunPublisherThread();

                Console.WriteLine(GetLetter());
                i++;
            }
        }
        
        /// <summary>
        /// start the race monitor
        /// </summary>
        /// <param name="restart">restart flag</param>
        private static void RunRaceMonitor(bool restart)
        {
            // connect to the telemetry data and run forever to processes it
            // TODO handle the case when there is no MQTT broker started
            RaceMonitor.Instance.Connect();
        }

        // 
        private static char GetLetter()
        {
            const string chars = "1234567890qwertyuiopasdfghjklzxcvbnm!\"£$%^&*()";
            var rand = new Random();
            var num = rand.Next(0, chars.Length - 1);
            return chars[num];
        }
    }
}
