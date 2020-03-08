﻿using System;
using System.Text;
using Newtonsoft.Json;
//using Nmqtt;
//using uPLibrary.Networking.M2Mqtt;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using MQTTnet.Server;
using MQTTnet;
using System.Threading.Tasks;

namespace RaceMonitor
{
    /// <summary>
    /// wrapper for a generic MQTT client to access the McLaren MAT Coding Challenge
    /// </summary>
    class MqttRaceClient
    {
        private IManagedMqttClient managedMqttClientPublisher;

        private IManagedMqttClient managedMqttClientSubscriber;

//        private IMqttServer mqttServer;




        #region data
        /// <summary>
        /// the client that connects to the MQTT broker
        /// </summary>
//        private readonly  MqttClient Client;
        /// <summary>
        /// address of the MQTT broker
        /// </summary>
        private readonly string BrokerAddress = "localhost";
        //private readonly string BrokerAddress = "172.18.0.2"; // TODO may need to fix this for working in a container
        private readonly int BrokerPort = 1883;
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

        private readonly ProducerConsumerQueue<JCarCoords> InQ;
        private readonly ProducerConsumerQueue<Tuple<string, string>> OutQ;
        #endregion

        /// <summary>
        /// constructor 
        /// </summary>
        /// <param name="HandleCarCoords">object to process received coordinates</param>
        public MqttRaceClient(IPerformTask<JCarCoords> HandleCarCoords)
        {
            //// connect to the MQTT broker using the default port
            //Client = new MqttClient(BrokerAddress,
            //                        /*port, // default port */
            //                        MqttClientId);

            InQ = new ProducerConsumerQueue<JCarCoords>(HandleCarCoords);

            // need to call worker thread from this thread
            OutQ = new ProducerConsumerQueue<Tuple<string, string>>(new MessagePublisher(this), false);
        }

        internal void RunMessagePublisher()
        {
            OutQ.Work();
        }

        private class MessagePublisher : IPerformTask<Tuple<string, string>>
        {
            readonly MqttRaceClient Client;

            public MessagePublisher(MqttRaceClient client)
            {
                Client = client;                
            }

            /// <summary>
            /// Publishes a message to the MQTT broker
            /// </summary>
            /// <param name="messageData">expected to be an array of strings 
            /// [0] is the MQTT topic 
            /// [1] is the message</param>
            public void PerformTask(Tuple<string, string> s)
            {
                if (s is null)
                {
                    throw new ArgumentNullException(nameof(s));
                }
                Client.PublishMessage(s.Item1, s.Item2);
            }

        }

        /// <summary>
        /// establish a connection and start listening for incoming data
        /// </summary>
        public void Connect()
        {
            // create a connection and subscribe to car coordinate data
            try
            {
                // connect to publish messages
                ButtonPublisherStartClick(this, null);
                // connect to listen for messages
                ListenForCarCoordinates();

                //ConnectionState state = Client.Connect();
                //if (state == ConnectionState.Connected)
                //{
                //    ListenForCarCoordinates();
                //}
            }
            catch (Exception)
            {
                //Console.WriteLine($"{Client} cannot connect {e}");
                throw;
            }
        }

        /// <summary>
        /// subscribe to car coordinate messages and register a method to process them
        /// </summary>
        public void ListenForCarCoordinates()
        {
            //if (client == null)
            //{
            //    throw new invalidoperationexception("you must connect before you can subscribe to a topic.");
            //}

            //string topic = carcoordinatestopic;
            //mqttqos qos = mqttqos.atmostonce;
            //var res = client.subscribe(topic, qos);
            //client.messageavailable += client_messageavailable;

            var mqttFactory = new MqttFactory();

            var tlsOptions = new MqttClientTlsOptions
            {
                UseTls = false,
                IgnoreCertificateChainErrors = true,
                IgnoreCertificateRevocationErrors = true,
                AllowUntrustedCertificates = true
            };

            var options = new MqttClientOptions
            {
                ClientId = "ClientSubscriber",
                ProtocolVersion = MqttProtocolVersion.V311,
                ChannelOptions = new MqttClientTcpOptions
                {
                    Server = "localhost",
                    Port = BrokerPort,
                    TlsOptions = tlsOptions
                }
            };

            if (options.ChannelOptions == null)
            {
                throw new InvalidOperationException();
            }

            options.CleanSession = true;
            options.KeepAlivePeriod = TimeSpan.FromSeconds(5);

            this.managedMqttClientSubscriber = mqttFactory.CreateManagedMqttClient();
            this.managedMqttClientSubscriber.ConnectedHandler = new MqttClientConnectedHandlerDelegate(OnSubscriberConnected);
            this.managedMqttClientSubscriber.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(OnSubscriberDisconnected);
            this.managedMqttClientSubscriber.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(this.OnSubscriberMessageReceived);

            this.managedMqttClientSubscriber.StartAsync(
                new ManagedMqttClientOptions
                {
                    ClientOptions = options
                });


            this.managedMqttClientSubscriber.SubscribeAsync(new TopicFilterBuilder().WithTopic(CarCoordinatesTopic).Build());

        }

        private void OnSubscriberConnected(MqttClientConnectedEventArgs obj)
        {
            Console.WriteLine("Subscriber: Connected ");
        }

        private void OnSubscriberDisconnected(MqttClientDisconnectedEventArgs obj)
        {
            if (obj.ClientWasConnected)
            {
                Console.WriteLine("Subscriber: Disconnected ");
            }
            else
            {
                Console.WriteLine("Subscriber: Still NOT Connected ");
            }
        }

        private void OnSubscriberMessageReceived(MqttApplicationMessageReceivedEventArgs arg)
        {
            if (CarCoordinatesTopic.Equals(arg.ApplicationMessage.Topic))
            {
                // topic of interest
                string topic = arg.ApplicationMessage.Topic;
                object payload = arg.ApplicationMessage.Payload;
                Client_MessageAvailable(this, new Nmqtt.MqttMessageEventArgs(topic, payload));
            }
        }

        /// <summary>
        /// handles a new MQTT Car Coordinates message 
        /// </summary>
        /// <param name="sender">event source</param>
        /// <param name="e">event arguments</param>
        private void Client_MessageAvailable(object sender, Nmqtt.MqttMessageEventArgs e)
        {
            // only process messages of interest
            if (CarCoordinatesTopic == e.Topic)
            {
                string ReceivedMessage = Encoding.UTF8.GetString((byte[])e.Message);

                // convert the message into a JCarCoords message
                JCarCoords coords = JsonConvert.DeserializeObject<JCarCoords>(ReceivedMessage);
                InQ.EnqueueTask(coords);
            }
        }

        /// <summary>
        /// send a position status event
        /// </summary>
        /// <param name="timestamp">message timestamp</param>
        /// <param name="index">the index for the car</param>
        /// <param name="position">the position in the race</param>
        internal void SendPosition(long timestamp, int index, int position)
        {
            SendCarStatus(timestamp, index, "POSITION", position);
        }

        /// <summary>
        /// send a speed status event
        /// </summary>
        /// <param name="timestamp">message timestamp</param>
        /// <param name="index">the index for the car</param>
        /// <param name="mph">the car's speed</param>
        internal void SendSpeed(long timestamp, int index, int mph)
        {
            SendCarStatus(timestamp, index, "SPEED", mph);
        }

        /// <summary>
        /// pubilsh a car status message to the MQTT broker
        /// </summary>
        /// <param name="timestamp">message timestamp</param>
        /// <param name="index">the index for the car</param>
        /// <param name="type">the status type</param>
        /// <param name="value">the status value</param>
        private void SendCarStatus(long timestamp, int index, string type, int value)
        {
            JCarStatus status = new JCarStatus(timestamp, index, type, value);
            string message = JsonConvert.SerializeObject(status);
            /*
            string message = "{ \"timestamp\": " + timestamp +
                ", \"carIndex\": " + index +
                ", \"type\": \"" + type +
                "\", \"value\": " + value + "}";
                */
            EnqueueMessage(CarStatusTopic, message);
        }

        /// <summary>
        /// publish an event message to the MQTT broker
        /// </summary>
        /// <param name="timestamp">message timestamp</param>
        /// <param name="eventMessage">the event description</param>
        public void SendRaceEvent(long timestamp, string eventMessage)
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
            EnqueueMessage(EventTopic, message);
        }

        private void EnqueueMessage(string eventTopic, string message)
        {
            OutQ.EnqueueTask(new Tuple<string, string>(eventTopic, message));
        }

        /// <summary>
        /// Publishes a message to the MQTT message broker
        /// </summary>
        /// <param name="topic">The topic to publish the message to.</param>
        /// <param name="message">The message to publish.</param>
        /// <returns>The message identifier assigned to the message.</returns>
        private short PublishMessage(string topic, string messageText)
        {
            try
            {
                var payload = Encoding.UTF8.GetBytes(messageText);
                var message = new MqttApplicationMessageBuilder().WithTopic(topic).WithPayload(payload).WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag().Build();

                if (this.managedMqttClientPublisher != null)
                {
                    this.managedMqttClientPublisher.PublishAsync(message);
                }
            }
            catch (Exception)
            {
                //MessageBox.Show(ex.Message, "Error Occurs", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }



            return 0;

//            byte[] messageData = Encoding.ASCII.GetBytes(message);
//            return Client.PublishMessage(topic, messageData);
        }

        // TODO rename
        private async void ButtonPublisherStartClick(object sender, EventArgs e)
        {
            var mqttFactory = new MqttFactory();

            var tlsOptions = new MqttClientTlsOptions
            {
                UseTls = false,
                IgnoreCertificateChainErrors = true,
                IgnoreCertificateRevocationErrors = true,
                AllowUntrustedCertificates = true
            };

            var options = new MqttClientOptions
            {
                ClientId = "ClientPublisher-" + MqttClientId,
                ProtocolVersion = MqttProtocolVersion.V311,
                ChannelOptions = new MqttClientTcpOptions
                {
                    Server = BrokerAddress,
                    Port = BrokerPort,
                    TlsOptions = tlsOptions
                }
            };

            if (options.ChannelOptions == null)
            {
                throw new InvalidOperationException();
            }

            //options.Credentials = new MqttClientCredentials
            //{
            //    Username = "username",
            //    Password = Encoding.UTF8.GetBytes("password")
            //};

            options.CleanSession = true;
            options.KeepAlivePeriod = TimeSpan.FromSeconds(5);
            this.managedMqttClientPublisher = mqttFactory.CreateManagedMqttClient();
            this.managedMqttClientPublisher.UseApplicationMessageReceivedHandler(this.HandleReceivedApplicationMessage);
            this.managedMqttClientPublisher.ConnectedHandler = new MqttClientConnectedHandlerDelegate(OnPublisherConnected);
            this.managedMqttClientPublisher.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(OnPublisherDisconnected);

            await this.managedMqttClientPublisher.StartAsync(
                new ManagedMqttClientOptions
                {
                    ClientOptions = options
                });
        }

        private void OnPublisherConnected(MqttClientConnectedEventArgs obj)
        {
            Console.WriteLine("Publisher: connected");
        }

        private void OnPublisherDisconnected(MqttClientDisconnectedEventArgs obj)
        {
            Console.WriteLine("Publisher: disconnected");
        }

        private Task HandleReceivedApplicationMessage(MqttApplicationMessageReceivedEventArgs arg)
        {
            string topic = arg.ApplicationMessage.Topic;
            object payload = arg.ApplicationMessage.Payload;
            Client_MessageAvailable(this, new Nmqtt.MqttMessageEventArgs(topic, payload));
            Action a = null;
            return new Task(a);
        }

        public override string ToString()
        {
            return MqttClientId;
        }
    }
}
