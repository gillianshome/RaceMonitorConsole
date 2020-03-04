using System;
using System.Text;
using Newtonsoft.Json;
using Nmqtt;
//using uPLibrary.Networking.M2Mqtt;

namespace RaceMonitor
{
    /// <summary>
    /// wrapper for a generic MQTT client to access the McLaren MAT Coding Challenge
    /// </summary>
    class MqttRaceClient
    {
        #region data
        /// <summary>
        /// the client that connects to the MQTT broker
        /// </summary>
        private readonly MqttClient Client;
        /// <summary>
        /// address of the MQTT broker
        /// </summary>
        private readonly string BrokerAddress = "localhost";
        /// <summary>
        /// client identifier
        /// </summary>
        private readonly string MqttClientId = "Race Client";

        /// <summary>
        /// MQTT topic names
        /// </summary>
        readonly string CarStatusTopic = "carStatus";
        /// <summary>
        /// MQTT topic names
        /// </summary>
        readonly string EventTopic = "events";
        /// <summary>
        /// MQTT topic names
        /// </summary>
        readonly string CarCoordinatesTopic = "carCoordinates";

        public static ICarCoordinates RaceDataHandler;
        #endregion

        /// <summary>
        /// constructor 
        /// </summary>
        public MqttRaceClient()
        {
            // connect to the MQTT broker using the default port
            Client = new MqttClient(BrokerAddress,
                                    /*port, // default port */
                                    MqttClientId);
        }

        /// <summary>
        /// establish a connection and start listening for incoming data
        /// </summary>
        public void Connect()
        {
            // create a connection and subscribe to car coordinate data
            try
            {
                ConnectionState state = Client.Connect();
                if (state == ConnectionState.Connected)
                {
                    ListenForCarCoordinates();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{Client} cannot connect {e}");
                throw;
            }            
        }

        /// <summary>
        /// subscribe to car coordinate messages and register a method to process them
        /// </summary>
        public void ListenForCarCoordinates()
        {
            if (Client == null)
            {
                throw new InvalidOperationException("You must connect before you can subscribe to a topic.");
            }

            string topic = CarCoordinatesTopic;
            MqttQos qos = MqttQos.AtMostOnce;
            var res = Client.Subscribe(topic, qos);
            Client.MessageAvailable += Client_MessageAvailable;
        }

        /// <summary>
        /// handles a new MQTT Car Coordinates message 
        /// </summary>
        /// <param name="sender">event source</param>
        /// <param name="e">event arguments</param>
        private void Client_MessageAvailable(object sender, MqttMessageEventArgs e)
        {
            // only process messages of interest
            if (CarCoordinatesTopic == e.Topic)
            {
                string ReceivedMessage = Encoding.UTF8.GetString((byte[])e.Message);

                // convert the message into a JCarCoords message
                JCarCoords coords = JsonConvert.DeserializeObject<JCarCoords>(ReceivedMessage);
                RaceDataHandler.ProcessRaceData(coords);
            }
        }

        /// <summary>
        /// send a position status event
        /// </summary>
        /// <param name="timestamp">message timestamp</param>
        /// <param name="index">the index for the car</param>
        /// <param name="position">the position in the race</param>
        /// <returns>The message identifier assigned to the message.</returns>
        internal short SendPosition(long timestamp, int index, int position)
        {
            return SendCarStatus(timestamp, index, "POSITION", position);
        }

        /// <summary>
        /// send a speed status event
        /// </summary>
        /// <param name="timestamp">message timestamp</param>
        /// <param name="index">the index for the car</param>
        /// <param name="mph">the car's speed</param>
        /// <returns>The message identifier assigned to the message.</returns>
        internal short SendSpeed(long timestamp, int index, int mph)
        {
            return SendCarStatus(timestamp, index, "SPEED", mph);
        }

        /// <summary>
        /// pubilsh a car status message to the MQTT broker
        /// </summary>
        /// <param name="timestamp">message timestamp</param>
        /// <param name="index">the index for the car</param>
        /// <param name="type">the status type</param>
        /// <param name="value">the status value</param>
        /// <returns></returns>
        private short SendCarStatus(long timestamp, int index, string type, int value)
        {
            JCarStatus status = new JCarStatus(timestamp, index, type, value);
            string message = JsonConvert.SerializeObject(status);
            /*
            string message = "{ \"timestamp\": " + timestamp +
                ", \"carIndex\": " + index +
                ", \"type\": \"" + type +
                "\", \"value\": " + value + "}";
                */
            short msgId = PublishMessage(CarStatusTopic, message);
            return msgId;
        }

        /// <summary>
        /// publish an event message to the MQTT broker
        /// </summary>
        /// <param name="timestamp">message timestamp</param>
        /// <param name="eventMessage">the event description</param>
        /// <returns></returns>
        public short SendRaceEvent(long timestamp, string eventMessage)
        {
            JEventMessage status = new JEventMessage(timestamp, eventMessage);
            string message = JsonConvert.SerializeObject(status);
            // example message shows the desired format
            //  { 
            //      "timestamp": 1541693114862, 
            //      "text": "Car 2 races ahead of Car 4 in a dramatic overtake."
            //  }
            /*
            string message = "{ \"timestamp\": " + timestamp + ", \"text\": \"" + eventMessage + "\" }";
            */
            return PublishMessage(EventTopic, message);
        }

        /// <summary>
        /// Publishes a message to the MQTT message broker
        /// </summary>
        /// <param name="topic">The topic to publish the message to.</param>
        /// <param name="message">The message to publish.</param>
        /// <returns>The message identifier assigned to the message.</returns>
        private short PublishMessage(string topic, string message)
        {
            byte[] messageData = Encoding.ASCII.GetBytes(message);
            return Client.PublishMessage(topic, messageData);
        }

        public override string ToString()
        {
            return MqttClientId;
        }
    }
}
