using System;

namespace WeatherBalloon.Messaging 
{
    public class TrackerMessage : MessageBase
    {
        public override string Type { get { return "tracker";}}

        public GPSLocation BalloonLocation {get;set;}
        public GPSLocation TrackerLocation {get;set;}
    }


}