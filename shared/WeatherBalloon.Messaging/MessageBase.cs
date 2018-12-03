using System;

namespace WeatherBalloon.Messaging
{

    public class MessageBase
    {
        public virtual string Type { get { return "unknown"; }}
        public DateTime Timestamp { get;set;}

        public MessageBase()
        {
            Timestamp = DateTime.UtcNow;
        }

    }
}