using System;

namespace WeatherBalloon.Messaging
{
    public class GPSMessage : MessageBase
    { 
        public override string Type { get { return "gps";}}

        public GPSLocation Location { get;set;}
    }
}
