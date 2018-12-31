using System;
using Newtonsoft.Json;
using System.Text;

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

        public Byte[] ToRawBytes()
        {
            var messageString = JsonConvert.SerializeObject(this);
            var bytes = Encoding.UTF8.GetBytes(messageString);

            return bytes;
        }
        
    }
}