using System;

namespace WeatherBalloon.Messaging 
{
    public class TrackerMessage : MessageBase, IDeviceMessage
    {
        public override string Type { get { return "tracker";}}


        // Properties common with Balloon Message
        public string FlightId {get;set;}
        public string DeviceName { get;set; }

        public GPSLocation Location {get;set;}
    }


}