using System;
using System.Collections.Generic;
using System.Text;

namespace RaceMonitor
{
    public class MqttMessageEventArgs : EventArgs
    {
        /// <summary>
        /// MQTT message object
        /// </summary>
        /// <param name="topic">topic string</param>
        /// <param name="message">the message</param>
        public MqttMessageEventArgs(string topic, object message)
        {
            Topic = topic;
            Message = message;
        }

        public string Topic { get; set; }
        public object Message { get; set; }
    }
}
